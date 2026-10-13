"""Audition two local TTS backends with separate processes and streaming measurements."""

from __future__ import annotations

import argparse
import hashlib
import importlib.metadata
import json
import os
import platform
import resource
import threading
import time
from pathlib import Path

import numpy as np
import psutil
import soundfile as sf

PACKAGE = Path(__file__).resolve().parent
MODEL_ROOT = PACKAGE.parents[1] / "artifacts/speech-models"
SEED = 20261013
ENGLISH = "Stay close. There is still hope for the village."
GERMAN = "Bleib bei mir. Es gibt noch Hoffnung für das Dorf."
GAME_EN = "Do not take the moonwort yet. Bring two clean bandages to the healer."
GAME_DE = "Nimm den Mondrautenfarn noch nicht. Bring zwei saubere Verbände zur Heilerin."
STYLES = {
    "calm": "Speak gently and reassuringly, as a healer comforting a frightened companion.",
    "urgent": "Speak urgently with fear and determination, warning a companion of immediate danger.",
    "whisper": "Whisper quietly and cautiously, trying not to alert someone nearby. Keep the words intelligible.",
}


class MemorySampler:
    """Sample RSS without adding it to overlapping Apple unified-memory GPU metrics."""

    def __init__(self):
        self.peak = 0
        self.stop = threading.Event()
        self.process = psutil.Process()
        self.thread = threading.Thread(target=self.run, daemon=True)

    def run(self):
        while not self.stop.is_set():
            self.peak = max(self.peak, self.process.memory_info().rss)
            self.stop.wait(0.05)


def cases(backend):
    """Keep exact words for Qwen style trials; Pocket trials vary punctuation only."""
    result = []
    for language, line, diagnostic in [("English", ENGLISH, GAME_EN), ("German", GERMAN, GAME_DE)]:
        code = "en" if language == "English" else "de"
        for style in (["calm", "urgent", "whisper"] if backend == "qwen3" else ["normal", "punctuation"]):
            text = line if style != "punctuation" else line.replace(".", "!")
            result.append({"id": f"{code}-{style}", "language": language,
                           "text": text, "style": style, "instruction": STYLES.get(style)})
        result.append({"id": f"{code}-game", "language": language,
                       "text": diagnostic, "style": "normal", "instruction": None})
    return result


def main():
    """Write raw clips and incremental reports; fail if any generated stream is invalid."""
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("backend", choices=["pocket", "qwen3"])
    parser.add_argument("--output-dir", type=Path)
    args = parser.parse_args()
    output = args.output_dir or PACKAGE / "output" / args.backend
    output.mkdir(parents=True, exist_ok=True)
    sampler = MemorySampler()
    sampler.thread.start()
    versions = ["numpy", "soundfile", "psutil"]
    report = {
        "backend": args.backend, "machine": platform.machine(), "platform": platform.platform(),
        "physical_ram_bytes": psutil.virtual_memory().total, "load_average_before": os.getloadavg(),
        "seed": SEED, "sampled_rss_peak_bytes": 0, "cases": [], "loads": [],
        "protocol": "One process/backend, one trial/case, local assets, no playback device. First case is cold.",
        "limits": "Background load uncontrolled; no M1/game coexistence/non-macOS test; first PCM chunk is not speaker playback.",
    }
    mx = None
    if args.backend == "pocket":
        import torch
        from pocket_tts import TTSModel
        versions += ["pocket-tts", "torch", "tokenizers", "huggingface-hub"]
        torch.set_num_threads(1)
        torch.set_num_interop_threads(1)
        report["precision"] = "CPU float32, no quantization; one torch intra-op thread"
    else:
        import mlx.core as mx
        from mlx_audio.tts.utils import load_model
        versions += ["mlx", "mlx-audio", "transformers", "huggingface-hub"]
        report["precision"] = "MLX 4-bit converted Talker checkpoint, bundled speech tokenizer"
        report["generation_settings"] = {"max_tokens": 256, "temperature": 0.9,
                                         "stream": True, "streaming_interval": 0.32}
        start = time.perf_counter()
        model = load_model(str(MODEL_ROOT / "Qwen3-TTS-1.7B-CustomVoice-4bit-f35faf19b0cc"))
        mx.eval(model.parameters())
        assert model.tokenizer is not None and model.speech_tokenizer is not None
        report["loads"].append({"language": "shared", "model_seconds": time.perf_counter() - start})
    report["versions"] = {name: importlib.metadata.version(name) for name in versions}
    current_language = None
    try:
        for case in cases(args.backend):
            if args.backend == "pocket":
                if current_language != case["language"]:
                    if current_language is not None:
                        del state, model
                        import gc
                        gc.collect()
                    language = case["language"].lower()
                    directory = MODEL_ROOT / "pocket-tts-3.3.0" / language
                    start = time.perf_counter()
                    model = TTSModel.load_model(config=directory / "config.yaml")
                    load_seconds = time.perf_counter() - start
                    start = time.perf_counter()
                    state = model.get_state_for_audio_prompt(directory / "voice.safetensors")
                    report["loads"].append({"language": language, "model_seconds": load_seconds,
                                            "voice_seconds": time.perf_counter() - start})
                    current_language = case["language"]
                torch.manual_seed(SEED)
                case["voice"] = "alba" if case["language"] == "English" else "juergen"
                stream = model.generate_audio_stream(state, case["text"])
            else:
                mx.random.seed(SEED)
                case["voice"] = "Ryan"
                stream = model.generate_custom_voice(
                    text=case["text"], speaker="Ryan", language=case["language"],
                    instruct=case["instruction"], stream=True, streaming_interval=0.32, max_tokens=256,
                )
            chunks, arrivals, tokens = [], [], 0
            start = time.perf_counter()
            for chunk in stream:
                if mx is not None:
                    mx.eval(chunk.audio)
                    array = np.asarray(chunk.audio, dtype=np.float32).reshape(-1)
                    tokens += chunk.token_count
                else:
                    array = chunk.detach().cpu().numpy().astype(np.float32).reshape(-1)
                if array.size:
                    arrivals.append({"seconds": time.perf_counter() - start, "samples": int(array.size)})
                    chunks.append(array)
            elapsed = time.perf_counter() - start
            audio = np.concatenate(chunks)
            assert audio.size > 0 and np.isfinite(audio).all()
            filename = case["id"] + ".wav"
            sf.write(output / filename, audio, model.sample_rate, subtype="PCM_16")
            duration = audio.size / model.sample_rate
            case.update({"status": "generated", "output": filename, "sample_rate": model.sample_rate,
                         "audio_seconds": duration, "generation_seconds": elapsed,
                         "first_pcm_chunk_seconds": arrivals[0]["seconds"], "chunks": arrivals,
                         "real_time_factor": elapsed / duration, "raw_peak": float(np.abs(audio).max()),
                         "clipped_samples": int(np.count_nonzero(np.abs(audio) >= 1)),
                         "sha256": hashlib.sha256((output / filename).read_bytes()).hexdigest()})
            if mx is not None:
                case["speech_tokens"] = tokens
                case["token_budget_reached"] = tokens >= 256
                report["mlx_peak_memory_bytes"] = mx.get_peak_memory()
            report["cases"].append(case)
            report["sampled_rss_peak_bytes"] = sampler.peak
            (output / "results.json").write_text(json.dumps(report, indent=2) + "\n")
            print(json.dumps({k: v for k, v in case.items() if k != "chunks"}), flush=True)
    finally:
        sampler.stop.set()
        sampler.thread.join()
    report["sampled_rss_peak_bytes"] = sampler.peak
    report["process_lifetime_maxrss_bytes"] = resource.getrusage(resource.RUSAGE_SELF).ru_maxrss
    report["load_average_after"] = os.getloadavg()
    (output / "results.json").write_text(json.dumps(report, indent=2) + "\n")


if __name__ == "__main__":
    main()
