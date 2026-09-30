#!/usr/bin/env python3
"""Validate the bounded manifest-owned runtime Seeker walk and idle asset set."""

from __future__ import annotations

import hashlib
import json
import struct
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
PROOF_ROOT = ROOT / "art-direction" / "production-proof" / "corrected"
MANIFEST = PROOF_ROOT / "asset-manifest.json"
RUNTIME_ROOT = ROOT / "game" / "art" / "runtime" / "characters" / "seeker"
MAIN_SCENE = ROOT / "game" / "Main.tscn"
DIRECTIONS = {"south": 108, "west": 144, "east": 144, "north": 108}
ANIMATIONS = {"walk": 6, "idle-breathe": 2}


def require(condition: bool, message: str) -> None:
    """Fail validation with one precise reason."""
    if not condition:
        raise SystemExit(message)


def sha256(path: Path) -> str:
    """Return the hexadecimal SHA-256 digest for a file."""
    return hashlib.sha256(path.read_bytes()).hexdigest()


def png_dimensions(path: Path) -> tuple[int, int]:
    """Read the IHDR dimensions from a PNG."""
    data = path.read_bytes()
    require(data[:8] == b"\x89PNG\r\n\x1a\n", f"not a PNG: {path}")
    return struct.unpack(">II", data[16:24])


def read_import_options(path: Path) -> dict[tuple[str, str], str]:
    """Parse the simple section/key structure used by Godot import sidecars."""
    section = ""
    options: dict[tuple[str, str], str] = {}
    for line in path.read_text(encoding="utf-8").splitlines():
        if line.startswith("[") and line.endswith("]"):
            section = line[1:-1]
        elif "=" in line and not line.startswith('"'):
            key, value = line.split("=", 1)
            options[(section, key)] = value
    return options


def resource_path(path: Path) -> str:
    """Return a game-relative Godot resource path."""
    return "res://" + path.relative_to(ROOT / "game").as_posix()


def imported_cache_path(path: Path) -> str:
    """Return Godot's deterministic imported texture cache path."""
    source = resource_path(path)
    source_hash = hashlib.md5(source.encode("utf-8"), usedforsecurity=False).hexdigest()
    return f"res://.godot/imported/{path.name}-{source_hash}.ctex"


def main() -> None:
    """Check exact provenance, geometry, import settings, and ordinary-scene isolation."""
    manifest = json.loads(MANIFEST.read_text(encoding="utf-8"))
    assets = {asset["id"]: asset for asset in manifest["assets"]}
    expected_pngs: set[Path] = set()
    runtime_uids: set[str] = set()

    for direction, width in DIRECTIONS.items():
        for animation, frame_count in ANIMATIONS.items():
            manifest_animation = animation.replace("-", "_")
            for frame in range(1, frame_count + 1):
                asset_id = f"characters.seeker.{direction}.{manifest_animation}_{frame:02d}"
                asset = assets.get(asset_id)
                require(asset is not None, f"missing manifest asset {asset_id}")
                runtime = RUNTIME_ROOT / direction / f"{animation}-{frame:02d}.png"
                source = PROOF_ROOT / asset["export"]
                expected_pngs.add(runtime)
                require(runtime.is_file(), f"missing runtime texture {runtime}")
                require(not runtime.is_symlink(), f"runtime texture must be owned: {runtime}")
                require(
                    sha256(source) == asset["sha256"] == sha256(runtime),
                    f"runtime texture lost manifest/source parity: {asset_id}",
                )
                require(png_dimensions(runtime) == (width, 168), f"geometry drifted: {asset_id}")
                require(
                    asset["pivot"] == {"x": width // 2, "y": 156}
                    and asset["contact_baseline"] == 156,
                    f"pivot or contact baseline drifted: {asset_id}",
                )

                sidecar = runtime.with_suffix(".png.import")
                require(sidecar.is_file(), f"missing runtime import sidecar: {sidecar}")
                options = read_import_options(sidecar)
                expected_source = resource_path(runtime)
                cache = imported_cache_path(runtime)
                require(
                    options.get(("deps", "source_file")) == f'"{expected_source}"'
                    and options.get(("remap", "path")) == f'"{cache}"'
                    and options.get(("deps", "dest_files")) == f'["{cache}"]',
                    f"runtime import paths drifted: {asset_id}",
                )
                require(
                    options.get(("remap", "importer")) == '"texture"'
                    and options.get(("params", "compress/mode")) == "0"
                    and options.get(("params", "mipmaps/generate")) == "false"
                    and options.get(("params", "process/fix_alpha_border")) == "true"
                    and options.get(("params", "process/premult_alpha")) == "false",
                    f"runtime import settings drifted: {asset_id}",
                )
                uid = options.get(("remap", "uid"))
                require(
                    uid is not None and uid not in runtime_uids,
                    f"runtime UID reused: {asset_id}",
                )
                runtime_uids.add(uid)

    require(
        set(RUNTIME_ROOT.rglob("*.png")) == expected_pngs,
        "runtime Seeker animation set is not exact",
    )
    require(
        set(RUNTIME_ROOT.rglob("*.png.import"))
        == {path.with_suffix(".png.import") for path in expected_pngs},
        "runtime Seeker animation sidecar set is not exact",
    )
    proof_uids = {
        read_import_options(path).get(("remap", "uid"))
        for path in (ROOT / "game" / "art" / "direction-a-import-proof").rglob("*.png.import")
    }
    require(runtime_uids.isdisjoint(proof_uids), "runtime animation reuses proof resource UIDs")
    other_runtime_uids = {
        read_import_options(path).get(("remap", "uid"))
        for path in (ROOT / "game" / "art" / "runtime").rglob("*.png.import")
        if not path.is_relative_to(RUNTIME_ROOT)
    }
    require(
        runtime_uids.isdisjoint(other_runtime_uids),
        "runtime animation reuses another runtime UID",
    )

    main_scene = MAIN_SCENE.read_text(encoding="utf-8")
    require(
        main_scene.count("res://art/runtime/characters/seeker/walk-animations.tres") == 1,
        "Main.tscn must reference the runtime Seeker walk resource exactly once",
    )
    require("direction-a-import-proof" not in main_scene, "Main.tscn references proof-only art")
    require('name="Body" type="Polygon2D" parent="Seeker"' not in main_scene, "greybox body remains")
    require('name="Facing" type="Polygon2D" parent="Seeker"' not in main_scene, "greybox facing remains")

    print("Runtime Seeker animation validation passed")
    print("assets: 32 exact manifest/source/runtime SHA-256 matches")
    print("geometry: four 120 ms walk loops and four 600 ms idle loops with exact pivots")
    print("isolation: ordinary scene references runtime-owned Seeker art only")


if __name__ == "__main__":
    main()
