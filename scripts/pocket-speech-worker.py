"""Run a bounded local-only Pocket TTS worker; stdout contains protocol messages only."""

import argparse
import base64
import json
import os
import re
import sys
import threading
from pathlib import Path

MAX_TEXT = 2000
MAX_LINE = 16000
LANGUAGES = {"en": "english", "de": "german"}


class Worker:
    """Keep two language models and bounded voice states warm, with one active utterance."""

    def __init__(self, assets, profiles):
        os.environ["HF_HUB_OFFLINE"] = "1"
        os.environ["TRANSFORMERS_OFFLINE"] = "1"
        import yaml
        import torch
        from pocket_tts import TTSModel

        torch.set_num_threads(1)
        torch.set_num_interop_threads(1)
        self.torch = torch
        self.lock = threading.RLock()
        self.active = None
        self.thread = None
        self.models = {}
        self.states = {}
        profiles = json.loads(profiles.read_text())
        if not isinstance(profiles, dict) or not 1 <= len(profiles) <= 16:
            raise ValueError("Invalid profile count")
        for language, name in LANGUAGES.items():
            config_path = assets / name / "config.yaml"
            config = yaml.safe_load(config_path.read_text())
            if config.get("weights_path_without_voice_cloning") is not None:
                raise ValueError("Remote fallback is disallowed")
            for location in [config["weights_path"], config["flow_lm"]["lookup_table"]["tokenizer_path"]]:
                local = Path(location).resolve()
                if not local.is_relative_to(assets) or not local.is_file():
                    raise ValueError("A local model or tokenizer is missing")
            model = TTSModel.load_model(config=config_path)
            self.models[language] = model
            for profile, mapping in profiles.items():
                if not re.fullmatch(r"[A-Za-z0-9_-]{1,64}", profile):
                    raise ValueError("Invalid voice identifier")
                relative = Path(mapping[language])
                path = (assets / relative).resolve()
                if relative.is_absolute() or not path.is_relative_to(assets) or path.suffix != ".safetensors":
                    raise ValueError("Invalid local voice state")
                self.states[(language, profile)] = model.get_state_for_audio_prompt(path)
        self.profiles = list(profiles)

    def emit(self, message):
        """Serialize output so cancellation and audio cannot interleave protocol bytes."""
        with self.lock:
            sys.stdout.write(json.dumps(message, separators=(",", ":")) + "\n")
            sys.stdout.flush()

    def synthesize(self, request, stop):
        """Generate exact submitted text and retire buffered audio when cancellation arrives."""
        import numpy as np

        identifier = request["id"]
        try:
            language = request["language"]
            model = self.models[language]
            state = self.states[(language, request["voiceProfile"])]
            sequence = 0
            for audio in model.generate_audio_stream(state, request["text"], stop=stop):
                samples = audio.detach().cpu().numpy().astype("<f4")
                if samples.size > 48000 or not np.isfinite(samples).all() or np.abs(samples).max() > 1:
                    raise ValueError("Malformed generated PCM")
                with self.lock:
                    if stop.is_set():
                        # Exhaust the cancelled generator so its decoder joins before model reuse.
                        continue
                    self.emit({"type": "audio", "id": identifier, "sequence": sequence,
                               "sampleRate": model.sample_rate, "samples": int(samples.size),
                               "pcm": base64.b64encode(samples.tobytes()).decode("ascii")})
                sequence += 1
            with self.lock:
                self.active = None
                self.emit({"type": "finished", "id": identifier,
                           "status": "cancelled" if stop.is_set() else "completed"})
        except Exception:
            with self.lock:
                self.active = None
                self.emit({"type": "error", "id": identifier, "code": "synthesis_failed"})
        finally:
            with self.lock:
                if self.active and self.active[0] == identifier:
                    self.active = None

    def handle(self, request):
        """Accept one generation or cancellation; never queue obsolete dialogue."""
        if not isinstance(request, dict):
            raise ValueError("A request must be an object")
        kind = request.get("type")
        if kind == "shutdown":
            with self.lock:
                if self.active:
                    self.active[1].set()
            if self.thread:
                self.thread.join(timeout=2)
            return False
        identifier = request.get("id")
        if not isinstance(identifier, str) or not re.fullmatch(r"[A-Za-z0-9_-]{1,64}", identifier):
            raise ValueError("Invalid request identity")
        with self.lock:
            if kind == "cancel":
                if self.active and self.active[0] == identifier:
                    self.active[1].set()
                return True
            if kind != "synthesize":
                raise ValueError("Unknown request")
            text, language, profile = request.get("text"), request.get("language"), request.get("voiceProfile")
            if (not isinstance(text, str) or not text.strip() or len(text) > MAX_TEXT or "\0" in text
                    or language not in self.models or (language, profile) not in self.states):
                self.emit({"type": "error", "id": identifier, "code": "invalid_request"})
                return True
            if self.active:
                self.emit({"type": "error", "id": identifier, "code": "busy"})
                return True
            stop = threading.Event()
            self.active = (identifier, stop)
            self.thread = threading.Thread(target=self.synthesize, args=(request, stop), daemon=True)
            self.thread.start()
        return True


def main():
    """Initialize local assets, then read bounded NDJSON without echoing failed requests."""
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--assets", type=Path, required=True)
    parser.add_argument("--profiles", type=Path, required=True)
    args = parser.parse_args()
    try:
        worker = Worker(args.assets.resolve(), args.profiles.resolve())
    except Exception:
        print(json.dumps({"type": "unavailable", "protocol": 1, "code": "runtime_or_assets_unavailable"}), flush=True)
        return 1
    worker.emit({"type": "ready", "protocol": 1, "processId": os.getpid(),
                 "languages": list(LANGUAGES), "profiles": worker.profiles})
    try:
        while True:
            line = sys.stdin.readline(MAX_LINE + 1)
            if not line:
                break
            if len(line) > MAX_LINE or not line.endswith("\n"):
                return 1
            if not worker.handle(json.loads(line)):
                return 0
    except (ValueError, TypeError, KeyError):
        return 1
    finally:
        worker.handle({"type": "shutdown"})
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
