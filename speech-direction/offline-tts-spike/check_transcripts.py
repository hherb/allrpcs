"""Check generated WAV words with cached multilingual Whisper, separately from timing."""

from __future__ import annotations

import argparse
import hashlib
import importlib.metadata
import json
import math
import re
from pathlib import Path

import numpy as np
import soundfile as sf
from scipy.signal import resample_poly
from mlx_audio.stt.models.whisper.whisper import Model


def words(text: str) -> list[str]:
    """Ignore punctuation and case for an auxiliary transcript comparison."""
    return re.findall(r"\w+", text.casefold())


def main() -> None:
    """Transcribe local clips without the requested words as an ASR prompt."""
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--model-dir", type=Path, required=True)
    parser.add_argument("--output-dir", type=Path, required=True)
    args = parser.parse_args()
    if not args.model_dir.is_dir():
        raise FileNotFoundError("A cached local Whisper model directory is required.")
    benchmark = json.loads((args.output_dir / "results.json").read_text())
    model = Model.from_pretrained(str(args.model_dir))
    report = {
        "model": "mlx-community/whisper-base-mlx",
        "snapshot": args.model_dir.name,
        "mlx_audio_version": importlib.metadata.version("mlx-audio"),
        "mlx_version": importlib.metadata.version("mlx"),
        "protocol": "Local WAVs; temperature 0; specified language; no initial prompt or prior text.",
        "limits": "Auxiliary ASR can make errors. It does not establish pronunciation or acting quality.",
        "cases": [],
    }
    for case in benchmark["cases"]:
        if case["status"] != "generated":
            continue
        path = args.output_dir / case["output"]
        digest = hashlib.sha256(path.read_bytes()).hexdigest()
        if digest != case["sha256"]:
            raise RuntimeError(f"Benchmark WAV hash changed: {path.name}")
        audio, rate = sf.read(path, dtype="float32")
        divisor = math.gcd(rate, 16000)
        audio = resample_poly(audio, 16000 // divisor, rate // divisor).astype(np.float32)
        result = model.generate(
            audio, language="de" if case["language"] == "German" else "en",
            temperature=0.0, condition_on_previous_text=False, initial_prompt=None, verbose=None,
        )
        record = {
            "id": case["id"], "wav_sha256": digest, "requested_text": case["text"],
            "transcript": result.text.strip(), "language": result.language,
            "word_match_ignoring_case_punctuation": words(result.text) == words(case["text"]),
        }
        report["cases"].append(record)
        print(json.dumps(record, ensure_ascii=False), flush=True)
    (args.output_dir / "transcript-check.json").write_text(
        json.dumps(report, ensure_ascii=False, indent=2) + "\n"
    )


if __name__ == "__main__":
    main()
