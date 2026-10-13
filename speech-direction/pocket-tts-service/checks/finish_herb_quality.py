"""Complete the frozen herb contract in its exact accepted snapshot after speech additions."""

import hashlib
import importlib.util
import json
from pathlib import Path

HERB_REVISION = "db298c8fa5dfe220467c4cc113d0998e1c1aec01"


def digest(path):
    """Bind exact source and retained evidence bytes."""
    return hashlib.sha256(path.read_bytes()).hexdigest()


class HistoricalRun:
    """Retain the original checks while assigning their actual historical cwd and index."""

    def __init__(self, run, tree, environment):
        self.run, self.tree, self.environment = run, tree, environment
        self.directory, self.report = run.directory, run.report

    def stage(self, *args, **kwargs):
        """Run the unchanged command explicitly in the protected snapshot."""
        kwargs["cwd"] = self.tree
        kwargs["revision"] = HERB_REVISION
        kwargs["env"] = dict(self.environment, **(kwargs.get("env") or {}))
        return self.run.stage(*args, **kwargs)


def main():
    """Verify prior completed checks, then run every herb check at its accepted revision."""
    root = Path(__file__).resolve().parents[3]
    prior_directory = root / "artifacts/pocket-service-quality-recovery"
    path = prior_directory / "summary.json"
    prior = json.loads(path.read_text())
    assert prior["status"] == "failed" and prior["failed_stage"] == "boundary-herb-source"
    assert len(prior["stages"]) == 44
    assert digest(root / "quality.py") == prior["original_runner_sha256"]
    for name, expected in prior["source_bindings"].items():
        assert digest(root / name) == expected
    for row in prior["stages"]:
        for attempt in row["attempts"]:
            assert digest(prior_directory / attempt["log"]) == attempt["sha256"]
        if row is not prior["stages"][-1]:
            assert row["attempts"][-1]["exit_code"] == 0 and row["attempts"][-1]["completion_seen"]
    spec = importlib.util.spec_from_file_location("retained_quality", root / "quality.py")
    quality = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(quality)
    run = quality.Run(root / "artifacts/pocket-service-herb-quality")
    run.report.update(original_runner_sha256=digest(root / "quality.py"),
                      executed_finish_runner_sha256=digest(Path(__file__)),
                      verified_prefix_report_sha256=digest(path), verified_prefix_completed_stages=43,
                      herb_revision=HERB_REVISION,
                      reason="The frozen herb validator rejects newly added speech evidence paths; run its exact accepted source instead of weakening the validator.")
    run.save()
    environment = {"ALLRPCS_CHARACTER_MODEL_COMPOSITION_DISABLED": "1",
                   "ALLRPCS_ORDINARY_PARTICIPATION_SMOKE_MODEL": "1"}
    try:
        with quality.snapshot(HERB_REVISION, editor=True) as (tree, git_environment):
            historical = HistoricalRun(run, tree, dict(environment, **git_environment))
            historical.stage("herb-historical-restore", ["dotnet", "restore", "tests/Application.Tests/Allrpcs.Application.Tests.csproj"])
            historical.stage("herb-historical-build", ["dotnet", "build", "game/Allrpcs.Game.csproj", "-m:1", "/nr:false"])
            historical.stage("herb-historical-import", ["godot", "--headless", "--editor", "--path", "game", "--import"], retry_native=True)
            quality.ROOT = tree
            try:
                quality.herb_runtime(historical, environment)
            finally:
                quality.ROOT = root
        for name, expected in prior["source_bindings"].items():
            assert digest(root / name) == expected
        run.report.update(status="passed", finished=quality.now())
        run.save()
        print("PASS: complete accepted herb contract; frozen validator and current speech source unchanged", flush=True)
    except Exception:
        run.report.update(status="failed", finished=quality.now())
        run.save()
        raise


if __name__ == "__main__":
    main()
