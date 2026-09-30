#!/usr/bin/env python3
"""Validate the bounded manifest-owned runtime herb-bed promotion."""

from __future__ import annotations

import hashlib
import json
import struct
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
PROOF_ROOT = ROOT / "art-direction" / "production-proof" / "corrected"
MANIFEST = PROOF_ROOT / "asset-manifest.json"
RUNTIME_ROOT = ROOT / "game" / "art" / "runtime" / "plants"
MAIN_SCENE = ROOT / "game" / "Main.tscn"
ASSET_ID = "plants.herb_bed_empty"
RUNTIME_TEXTURE = RUNTIME_ROOT / "herb-bed-empty.png"
EXPECTED_DIMENSIONS = (96, 80)
EXPECTED_PIVOT = (48, 77)


def require(condition: bool, message: str) -> None:
    """Raise one concise validation error when a runtime-herb-bed invariant fails."""

    if not condition:
        raise AssertionError(message)


def sha256(path: Path) -> str:
    """Return one file's lowercase SHA-256 digest."""

    digest = hashlib.sha256()
    with path.open("rb") as stream:
        for chunk in iter(lambda: stream.read(1024 * 1024), b""):
            digest.update(chunk)
    return digest.hexdigest()


def png_dimensions(path: Path) -> tuple[int, int]:
    """Read one PNG's IHDR dimensions without adding an image-library dependency."""

    with path.open("rb") as stream:
        signature = stream.read(8)
        length = struct.unpack(">I", stream.read(4))[0]
        chunk_type = stream.read(4)
        require(signature == b"\x89PNG\r\n\x1a\n", f"not a PNG: {path}")
        require(chunk_type == b"IHDR" and length >= 8, f"missing PNG IHDR: {path}")
        return struct.unpack(">II", stream.read(8))


def resource_path(path: Path) -> str:
    """Return one game file's canonical Godot resource path."""

    return f"res://{path.relative_to(ROOT / 'game').as_posix()}"


def imported_cache_path(path: Path) -> str:
    """Return Godot's deterministic imported texture cache path."""

    source = resource_path(path)
    source_hash = hashlib.md5(source.encode("utf-8"), usedforsecurity=False).hexdigest()
    return f"res://.godot/imported/{path.name}-{source_hash}.ctex"


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
    """Check exact provenance, geometry, import settings, and runtime isolation."""

    manifest = json.loads(MANIFEST.read_text(encoding="utf-8"))
    manifest_assets = {asset["id"]: asset for asset in manifest["assets"]}
    asset = manifest_assets.get(ASSET_ID)
    require(asset is not None, f"expected one {ASSET_ID} manifest entry")
    require(
        tuple(asset["export_rect_pixels"][key] for key in ("width", "height"))
        == EXPECTED_DIMENSIONS,
        f"{ASSET_ID} manifest geometry drifted",
    )
    require(
        tuple(asset["pivot"][key] for key in ("x", "y")) == EXPECTED_PIVOT,
        f"{ASSET_ID} manifest pivot drifted",
    )
    require(RUNTIME_TEXTURE.is_file(), f"missing runtime texture for {ASSET_ID}")
    require(not RUNTIME_TEXTURE.is_symlink(), "runtime herb bed must be owned, not linked")

    source = PROOF_ROOT / asset["export"]
    require(
        sha256(source) == asset["sha256"] == sha256(RUNTIME_TEXTURE),
        f"runtime texture for {ASSET_ID} lost exact manifest/source parity",
    )
    require(
        png_dimensions(RUNTIME_TEXTURE) == EXPECTED_DIMENSIONS,
        f"runtime texture for {ASSET_ID} has unexpected dimensions",
    )

    sidecar = RUNTIME_TEXTURE.with_suffix(".png.import")
    require(sidecar.is_file(), f"missing runtime import sidecar for {ASSET_ID}")
    options = read_import_options(sidecar)
    expected_source = resource_path(RUNTIME_TEXTURE)
    expected_cache = imported_cache_path(RUNTIME_TEXTURE)
    require(
        options.get(("deps", "source_file")) == f'"{expected_source}"',
        "runtime herb-bed import source path drifted",
    )
    require(
        options.get(("remap", "path")) == f'"{expected_cache}"'
        and options.get(("deps", "dest_files")) == f'["{expected_cache}"]',
        "runtime herb-bed import cache path drifted",
    )
    require(
        options.get(("remap", "importer")) == '"texture"'
        and options.get(("params", "compress/mode")) == "0"
        and options.get(("params", "mipmaps/generate")) == "false",
        "runtime herb bed is not lossless and mipmap-free",
    )
    require(
        options.get(("params", "process/fix_alpha_border")) == "true"
        and options.get(("params", "process/premult_alpha")) == "false",
        "runtime herb-bed alpha processing drifted",
    )

    game_art_root = ROOT / "game" / "art"
    other_sidecars = {
        candidate for candidate in game_art_root.rglob("*.png.import") if candidate != sidecar
    }
    other_uids = {
        read_import_options(candidate).get(("remap", "uid")) for candidate in other_sidecars
    }
    runtime_uid = options.get(("remap", "uid"))
    require(
        runtime_uid is not None
        and runtime_uid.startswith('"uid://')
        and runtime_uid.endswith('"')
        and runtime_uid not in other_uids,
        "runtime herb bed needs its own unique resource UID",
    )

    main_scene = MAIN_SCENE.read_text(encoding="utf-8")
    require(
        main_scene.count(expected_source) == 1,
        "Main.tscn must reference the runtime herb-bed resource exactly once",
    )
    require(
        "direction-a-import-proof" not in main_scene
        and "DirectionAImportProof" not in main_scene,
        "ordinary Main.tscn references the proof-only directory or scene",
    )

    print("Runtime herb-bed validation passed")
    print(f"asset: {ASSET_ID}, exact SHA-256 {asset['sha256']}")
    print("geometry: two 96x80 beds with (48, 77) pivots; lossless import")
    print("isolation: Main.tscn references one runtime-owned copy and no proof resource")


if __name__ == "__main__":
    main()
