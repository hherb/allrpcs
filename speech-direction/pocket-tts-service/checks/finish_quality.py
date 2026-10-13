"""Finish unchanged late quality checks after the retained historical Godot import crash."""

import hashlib
import importlib.util
import json
from pathlib import Path
import subprocess
import sys


def digest(path):
    """Bind exact retained source and output bytes."""
    return hashlib.sha256(path.read_bytes()).hexdigest()


def main():
    """Verify the failed-run prefix, then execute every remaining original check."""
    root = Path(__file__).resolve().parents[3]
    prior_directory = root / "artifacts/quality-current"
    prior_path = prior_directory / "summary.json"
    prior = json.loads(prior_path.read_text())
    assert prior["status"] == "failed" and prior["failed_stage"] == "heating-historical-import"
    assert len(prior["stages"]) == 118
    assert prior["stages"][-1]["attempts"][-1]["exit_code"] == -11
    assert prior["stages"][-1]["arguments"] == [
        "godot", "--headless", "--editor", "--path", "game", "--import"]
    assert prior["stages"][-1]["revision"] == "b7912dd46a91e4b101c44cf56701d00f45b9ffd8"
    assert subprocess.check_output(["git", "rev-parse", "HEAD"], cwd=root, text=True).strip() == prior["product_revision"]
    for name, expected in prior["inputs"].items():
        assert digest(root / name) == expected
    for row in prior["stages"]:
        for attempt in row["attempts"]:
            assert Path(attempt["log"]).name == attempt["log"]
            assert digest(prior_directory / attempt["log"]) == attempt["sha256"]
        if row is not prior["stages"][-1]:
            assert row["attempts"][-1]["exit_code"] == 0
            assert row["attempts"][-1]["completion_seen"]
    tracked = subprocess.check_output(["git", "ls-files", "-z"], cwd=root).split(b"\0")
    sources = {name.decode(): digest(root / name.decode()) for name in tracked
               if name and (name.startswith(b"src/") or name.startswith(b"tests/") or name.startswith(b"game/"))}
    sources["scripts/pocket-speech-worker.py"] = digest(root / "scripts/pocket-speech-worker.py")
    spec = importlib.util.spec_from_file_location("retained_quality", root / "quality.py")
    quality = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(quality)
    run = quality.Run(root / "artifacts/pocket-service-quality-recovery")
    run.report.update(
        original_runner_sha256=digest(root / "quality.py"),
        executed_finish_runner_sha256=digest(Path(__file__)),
        python=sys.version,
        source_bindings=sources,
        verified_prefix={"report_sha256": digest(prior_path), "completed_stages": 117,
                         "original_status": "failed", "original_failed_stage": "heating-historical-import",
                         "reason": "Retained SIGSEGV in Godot ResourceImporterWAV on WorkerThread 0; repeat the exact historical checks after verifying unchanged source and completed prefix logs."})
    run.save()
    environment = {"ALLRPCS_CHARACTER_MODEL_COMPOSITION_DISABLED": "1",
                   "ALLRPCS_ORDINARY_PARTICIPATION_SMOKE_MODEL": "1"}
    try:
        for check in (quality.heating_runtime, quality.steeping_runtime,
                      quality.cooling_runtime, quality.herb_runtime):
            check(run, environment)
        for name, expected in sources.items():
            assert digest(root / name) == expected
        run.report.update(status="passed", finished=quality.now())
        run.save()
        print("PASS: unchanged remaining quality checks; retained root run stays marked failed", flush=True)
    except Exception:
        run.report.update(status="failed", finished=quality.now())
        run.save()
        raise


if __name__ == "__main__":
    main()
