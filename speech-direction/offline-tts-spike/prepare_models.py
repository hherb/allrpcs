"""Download pinned public audition checkpoints and verify their published hashes."""

import hashlib
import json
from concurrent.futures import ThreadPoolExecutor
from pathlib import Path

import yaml
from huggingface_hub import HfApi, hf_hub_download, snapshot_download

PACKAGE = Path(__file__).resolve().parent
ROOT = PACKAGE.parents[1]
CACHE = ROOT / "artifacts/speech-models"
POCKET = CACHE / "pocket-tts-3.3.0"
QWEN = CACHE / "Qwen3-TTS-1.7B-CustomVoice-4bit-f35faf19b0cc"
POCKET_REPO = "kyutai/pocket-tts-without-voice-cloning"
QWEN_REPO = "mlx-community/Qwen3-TTS-12Hz-1.7B-CustomVoice-4bit"
QWEN_REVISION = "f35faf19b0cc2160865af64ecf0f22f83d335135"


def pocket():
    """Use the same weights/tokenizer/voice pins shipped in pocket-tts 3.3.0."""
    import pocket_tts

    shipped = Path(pocket_tts.__file__).parent / "config"
    records = []
    for language, voice in [("english", "alba"), ("german", "juergen")]:
        config = yaml.safe_load((shipped / f"{language}.yaml").read_text())
        assets = {
            "model.safetensors": config["weights_path_without_voice_cloning"],
            "tokenizer.json": config["flow_lm"]["lookup_table"]["tokenizer_path"],
            "voice.safetensors": (
                f"hf://{POCKET_REPO}/languages/{language}/embeddings/{voice}.safetensors"
                "@4e1e0a3e611c51c0b4ed8174fc10f32a54644303"
            ),
        }
        for local_name, source in assets.items():
            address, revision = source.removeprefix("hf://").rsplit("@", 1)
            owner, repo, filename = address.split("/", 2)
            repository = f"{owner}/{repo}"
            info = HfApi().model_info(repository, revision=revision, files_metadata=True)
            entry = next(x for x in info.siblings if x.rfilename == filename)
            target = POCKET / language / local_name
            target.parent.mkdir(parents=True, exist_ok=True)
            if not target.exists():
                downloaded = hf_hub_download(
                    repository, filename, revision=revision, cache_dir=POCKET / ".download-cache"
                )
                import shutil
                shutil.copyfile(downloaded, target)
            digest = hashlib.sha256(target.read_bytes()).hexdigest()
            expected = entry.lfs.sha256 if entry.lfs else None
            assert target.stat().st_size == entry.size
            assert expected is None or digest == expected
            records.append({"path": str(target), "repository": repository,
                            "revision": revision, "file": filename, "size": entry.size,
                            "sha256": digest, "published_sha256": expected})
            print(f"Verified Pocket {language}/{local_name}", flush=True)
        config["weights_path"] = str(POCKET / language / "model.safetensors")
        config["weights_path_without_voice_cloning"] = None
        config["flow_lm"]["lookup_table"]["tokenizer_path"] = str(POCKET / language / "tokenizer.json")
        (POCKET / language / "config.yaml").write_text(yaml.safe_dump(config, sort_keys=False))
    return records


def qwen():
    """Retain the complete converted checkpoint, including its local speech tokenizer."""
    info = HfApi().model_info(QWEN_REPO, revision=QWEN_REVISION, files_metadata=True)
    snapshot_download(QWEN_REPO, revision=QWEN_REVISION, local_dir=QWEN, max_workers=2)
    records = []
    for entry in info.siblings:
        target = QWEN / entry.rfilename
        digest = hashlib.sha256(target.read_bytes()).hexdigest()
        expected = entry.lfs.sha256 if entry.lfs else None
        assert target.stat().st_size == entry.size
        assert expected is None or digest == expected
        records.append({"path": str(target), "repository": QWEN_REPO,
                        "revision": QWEN_REVISION, "file": entry.rfilename,
                        "size": entry.size, "sha256": digest, "published_sha256": expected})
    print("Verified Qwen3 checkpoint and speech tokenizer", flush=True)
    return records


if __name__ == "__main__":
    with ThreadPoolExecutor(max_workers=2) as pool:
        pending = [pool.submit(pocket), pool.submit(qwen)]
        records = [record for future in pending for record in future.result()]
    (PACKAGE / "model-verification.json").write_text(json.dumps(records, indent=2) + "\n")
