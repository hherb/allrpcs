#!/usr/bin/env python3
"""Validate exact source parity and explicit Godot import settings for Direction A proof assets."""

from __future__ import annotations

import hashlib
import json
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
SOURCE_ROOT = ROOT / "art-direction" / "production-proof" / "corrected"
GAME_ROOT = ROOT / "game" / "art" / "direction-a-import-proof"
MANIFEST = SOURCE_ROOT / "asset-manifest.json"
MAIN_SCENE = ROOT / "game" / "Main.tscn"


def require(condition: bool, message: str) -> None:
    """Raise one concise validation error when an import-proof invariant fails."""

    if not condition:
        raise AssertionError(message)


def sha256(path: Path) -> str:
    """Return one file's lowercase SHA-256 digest."""

    digest = hashlib.sha256()
    with path.open("rb") as stream:
        for chunk in iter(lambda: stream.read(1024 * 1024), b""):
            digest.update(chunk)
    return digest.hexdigest()


def game_path(export_path: str) -> Path:
    """Map one corrected export path to its isolated Godot import-proof copy."""

    prefix = "exports/"
    require(export_path.startswith(prefix), f"unexpected export path: {export_path}")
    return GAME_ROOT / export_path[len(prefix) :]


def read_import_options(path: Path) -> dict[tuple[str, str], str]:
    """Read scalar values from one Godot .import sidecar."""

    section = ""
    values: dict[tuple[str, str], str] = {}
    for raw_line in path.read_text(encoding="utf-8").splitlines():
        line = raw_line.strip()
        if line.startswith("[") and line.endswith("]"):
            section = line[1:-1]
        elif "=" in line and not line.endswith("{"):
            key, value = line.split("=", 1)
            values[(section, key)] = value
    return values


def main() -> None:
    """Check the bounded import set, byte parity, and painterly texture import options."""

    manifest = json.loads(MANIFEST.read_text(encoding="utf-8"))
    assets = manifest["assets"]
    expected_pngs = {game_path(asset["export"]) for asset in assets}
    actual_pngs = set(GAME_ROOT.rglob("*.png"))
    require(
        actual_pngs == expected_pngs,
        "Godot import-proof PNG set diverged from the manifest",
    )
    main_scene = MAIN_SCENE.read_text(encoding="utf-8")
    require(
        "direction-a-import-proof" not in main_scene
        and "DirectionAImportProof" not in main_scene,
        "ordinary Main.tscn references the isolated import proof",
    )

    for asset in assets:
        source = SOURCE_ROOT / asset["export"]
        imported = game_path(asset["export"])
        require(
            sha256(source) == asset["sha256"], f"source hash drifted: {asset['id']}"
        )
        require(
            sha256(imported) == asset["sha256"], f"Godot copy drifted: {asset['id']}"
        )

        sidecar = imported.with_suffix(imported.suffix + ".import")
        require(sidecar.exists(), f"missing Godot import sidecar: {sidecar}")
        options = read_import_options(sidecar)
        require(
            options.get(("remap", "importer")) == '"texture"',
            f"{sidecar}: not a texture import",
        )
        require(
            options.get(("params", "compress/mode")) == "0",
            f"{sidecar}: texture compression is not lossless",
        )
        require(
            options.get(("params", "mipmaps/generate")) == "false",
            f"{sidecar}: mipmaps unexpectedly enabled",
        )
        require(
            options.get(("params", "process/fix_alpha_border")) == "true",
            f"{sidecar}: alpha-border fix disabled",
        )
        require(
            options.get(("params", "process/premult_alpha")) == "false",
            f"{sidecar}: premultiplied alpha enabled",
        )

    print("Direction A Godot import-proof validation passed")
    print(f"assets: {len(assets)} exact source-hash matches")
    print("imports: lossless, no mipmaps, alpha-border fix, no premultiplied alpha")


if __name__ == "__main__":
    main()
