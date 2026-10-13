"""Run a bounded, local-only Qwen2.5-Omni speech audition with stage measurements."""

from __future__ import annotations

import argparse
import hashlib
import json
import os
import platform
import resource
import threading
import time
import traceback
from datetime import datetime, timezone
from pathlib import Path

import numpy as np
import psutil
import soundfile as sf
import torch
import transformers
from transformers import Qwen2_5OmniForConditionalGeneration, Qwen2_5OmniProcessor

MODEL_ID = "Qwen/Qwen2.5-Omni-7B"
MODEL_REVISION = "ae9e1690543ffd5c0221dc27f79834d0294cba00"
SYSTEM_PROMPT = (
    "You are Qwen, a virtual human developed by the Qwen Team, Alibaba Group, "
    "capable of perceiving auditory and visual inputs, as well as generating text and speech."
)
SAMPLE_RATE = 24000
SEED = 20261013


def synchronize() -> None:
    """Include completed Metal execution in each timing interval."""
    torch.mps.synchronize()


class MemorySampler:
    """Sample process and Metal memory separately; they must not be added together."""

    def __init__(self) -> None:
        self.stop_event = threading.Event()
        self.peaks = {"rss_bytes": 0, "mps_allocated_bytes": 0, "mps_driver_bytes": 0}
        self.thread = threading.Thread(target=self.sample, daemon=True)

    def sample(self) -> None:
        """Retain interval peaks, with a final sample when stopped."""
        process = psutil.Process()
        while True:
            values = {
                "rss_bytes": process.memory_info().rss,
                "mps_allocated_bytes": torch.mps.current_allocated_memory(),
                "mps_driver_bytes": torch.mps.driver_allocated_memory(),
            }
            for key, value in values.items():
                self.peaks[key] = max(self.peaks[key], value)
            if self.stop_event.wait(0.1):
                return

    def __enter__(self) -> MemorySampler:
        self.thread.start()
        return self

    def __exit__(self, *_: object) -> None:
        self.stop_event.set()
        self.thread.join()


def measure_method(owner: object, method_name: str, label: str, timings: dict) -> None:
    """Wrap a pipeline stage for timing without changing its inputs or outputs."""
    original = getattr(owner, method_name)

    def measured(*args: object, **kwargs: object) -> object:
        synchronize()
        started = time.perf_counter()
        result = original(*args, **kwargs)
        synchronize()
        elapsed = time.perf_counter() - started
        timings[label] = elapsed
        print(f"STAGE {label}: {elapsed:.3f}s", flush=True)
        return result

    setattr(owner, method_name, measured)


def main() -> None:
    """Load once, generate four bounded clips, and retain incremental results."""
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--model-dir", type=Path, required=True)
    parser.add_argument("--output-dir", type=Path, required=True)
    parser.add_argument("--cases", type=Path, default=Path(__file__).with_name("cases.json"))
    args = parser.parse_args()
    args.output_dir.mkdir(parents=True, exist_ok=True)
    if not torch.backends.mps.is_available():
        raise RuntimeError("The requested Metal benchmark requires host MPS access.")
    torch.set_num_threads(8)
    torch.manual_seed(SEED)
    cases = json.loads(args.cases.read_text())
    report = {
        "model": MODEL_ID,
        "revision": MODEL_REVISION,
        "platform": platform.platform(),
        "recorded_at_utc": datetime.now(timezone.utc).isoformat(),
        "total_memory_bytes": psutil.virtual_memory().total,
        "logical_cpu_count": psutil.cpu_count(),
        "machine": "Apple M3 Max, 16 CPU cores, 128 GB unified memory",
        "torch": torch.__version__,
        "transformers": transformers.__version__,
        "device": "mps",
        "thinker_talker_dtype": "bfloat16",
        "token2wav_dtype": "float32",
        "attention": "sdpa",
        "seed": SEED,
        "system_prompt": SYSTEM_PROMPT,
        "local_files_only": True,
        "mps_fallback_enabled": os.getenv("PYTORCH_ENABLE_MPS_FALLBACK") == "1",
        "measurement": "Buffered full pipeline; first clip cold, subsequent clips warm. No streaming TTFA.",
        "system_available_memory_before_bytes": psutil.virtual_memory().available,
        "system_load_average_before": os.getloadavg(),
        "cases": [],
    }
    report_path = args.output_dir / "results.json"

    def save() -> None:
        report_path.write_text(json.dumps(report, ensure_ascii=False, indent=2) + "\n")

    with MemorySampler() as loading_memory:
        started = time.perf_counter()
        print("Loading pinned local model on MPS...", flush=True)
        model = Qwen2_5OmniForConditionalGeneration.from_pretrained(
            str(args.model_dir), torch_dtype=torch.bfloat16, device_map="mps",
            attn_implementation="sdpa", local_files_only=True,
        ).eval()
        model.token2wav.float()
        processor = Qwen2_5OmniProcessor.from_pretrained(str(args.model_dir), local_files_only=True)
        synchronize()
        report["load_seconds"] = time.perf_counter() - started
    report["load_memory_peaks"] = loading_memory.peaks
    report["parameter_bytes"] = sum(p.numel() * p.element_size() for p in model.parameters())
    print(f"Model loaded in {report['load_seconds']:.3f}s", flush=True)
    save()
    stage_times: dict[str, float] = {}
    measure_method(model.thinker, "generate", "thinker", stage_times)
    measure_method(model.talker, "generate", "talker", stage_times)
    measure_method(model.token2wav, "forward", "token2wav", stage_times)

    for index, case in enumerate(cases):
        print(f"CASE {case['id']}", flush=True)
        instruction = (
            f"Perform this short game dialogue in {case['language']}. {case['delivery']} "
            "Reply with exactly the following words, without introductions, quotation marks, "
            f"stage directions or extra words: {case['text']}"
        )
        conversation = [
            {"role": "system", "content": [{"type": "text", "text": SYSTEM_PROMPT}]},
            {"role": "user", "content": [{"type": "text", "text": instruction}]},
        ]
        chat = processor.apply_chat_template(conversation, add_generation_prompt=True, tokenize=False)
        inputs = processor(text=chat, return_tensors="pt", padding=True).to("mps")
        torch.manual_seed(SEED)
        stage_times.clear()
        record = dict(case, index=index, warm=index > 0, prompt=instruction)
        try:
            with MemorySampler() as memory:
                synchronize()
                started = time.perf_counter()
                with torch.inference_mode():
                    text_ids, audio = model.generate(
                        **inputs, speaker=case["speaker"], return_audio=True,
                        thinker_max_new_tokens=64, thinker_do_sample=False,
                        talker_max_new_tokens=512,
                    )
                synchronize()
                elapsed = time.perf_counter() - started
            generated_text = processor.batch_decode(
                text_ids[:, inputs.input_ids.shape[1]:], skip_special_tokens=True,
                clean_up_tokenization_spaces=False,
            )[0].strip()
            waveform = audio.reshape(-1).detach().cpu().numpy()
            if not np.isfinite(waveform).all() or len(waveform) == 0:
                raise RuntimeError("Generated waveform is empty or non-finite.")
            output_path = args.output_dir / (case["id"] + ".wav")
            sf.write(output_path, waveform, SAMPLE_RATE, subtype="PCM_16")
            duration = len(waveform) / SAMPLE_RATE
            record.update(
                status="generated", generated_text=generated_text,
                exact_text_match=generated_text == case["text"], seconds=elapsed,
                stage_seconds=dict(stage_times), audio_seconds=duration,
                real_time_factor=elapsed / duration, peak_sample=float(np.max(np.abs(waveform))),
                clipped_sample_count=int(np.sum(np.abs(waveform) >= 1)), memory_peaks=memory.peaks,
                output=output_path.name, sha256=hashlib.sha256(output_path.read_bytes()).hexdigest(),
            )
            print(json.dumps(record, ensure_ascii=False), flush=True)
            del text_ids, audio, waveform
        except Exception as error:
            record.update(status="failed", error=repr(error), traceback=traceback.format_exc())
            print(record["traceback"], flush=True)
        report["cases"].append(record)
        report["process_max_rss_bytes"] = resource.getrusage(resource.RUSAGE_SELF).ru_maxrss
        report["system_available_memory_after_bytes"] = psutil.virtual_memory().available
        report["system_load_average_after"] = os.getloadavg()
        save()
        if record["status"] == "failed":
            break
    print(f"Saved report to {report_path}", flush=True)


if __name__ == "__main__":
    main()
