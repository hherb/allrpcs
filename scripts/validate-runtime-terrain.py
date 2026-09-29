#!/usr/bin/env python3
"""Validate the bounded quiet-moss runtime terrain promotion."""

from __future__ import annotations

import hashlib
import json
import struct
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
PROOF_ROOT = ROOT / "art-direction" / "production-proof" / "corrected"
MANIFEST = PROOF_ROOT / "asset-manifest.json"
RUNTIME_ROOT = ROOT / "game" / "art" / "runtime" / "terrain"
RUNTIME_TEXTURE = RUNTIME_ROOT / "quiet-moss.png"
MAIN_SCENE = ROOT / "game" / "Main.tscn"
ASSET_ID = "terrain.quiet_moss"
EXPECTED_DIMENSIONS = (256, 256)


def require(condition: bool, message: str) -> None:
    """Raise one concise validation error when a runtime-terrain invariant fails."""

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
    assets = [asset for asset in manifest["assets"] if asset["id"] == ASSET_ID]
    require(len(assets) == 1, f"expected one {ASSET_ID} manifest entry")
    asset = assets[0]
    source = PROOF_ROOT / asset["export"]
    require(
        tuple(asset["export_rect_pixels"][key] for key in ("width", "height"))
        == EXPECTED_DIMENSIONS,
        f"{ASSET_ID} manifest geometry drifted",
    )
    require(RUNTIME_TEXTURE.is_file(), "missing runtime quiet-moss texture")
    require(not RUNTIME_TEXTURE.is_symlink(), "runtime quiet-moss texture must be owned, not linked")
    require(
        set(RUNTIME_ROOT.glob("*.png")) == {RUNTIME_TEXTURE},
        "runtime terrain contains an asset outside the bounded quiet-moss promotion",
    )
    require(
        sha256(source) == asset["sha256"] == sha256(RUNTIME_TEXTURE),
        "runtime quiet-moss texture lost exact manifest/source parity",
    )
    require(
        png_dimensions(RUNTIME_TEXTURE) == EXPECTED_DIMENSIONS,
        "runtime quiet-moss texture is not 256x256",
    )

    sidecar = RUNTIME_TEXTURE.with_suffix(".png.import")
    require(sidecar.is_file(), "missing runtime quiet-moss import sidecar")
    options = read_import_options(sidecar)
    expected_source = resource_path(RUNTIME_TEXTURE)
    expected_cache = imported_cache_path(RUNTIME_TEXTURE)
    require(
        options.get(("deps", "source_file")) == f'"{expected_source}"',
        "runtime quiet-moss import source path drifted",
    )
    require(
        options.get(("remap", "path")) == f'"{expected_cache}"'
        and options.get(("deps", "dest_files")) == f'["{expected_cache}"]',
        "runtime quiet-moss import cache path drifted",
    )
    require(
        options.get(("remap", "importer")) == '"texture"'
        and options.get(("params", "compress/mode")) == "0"
        and options.get(("params", "mipmaps/generate")) == "false",
        "runtime quiet-moss import is not lossless and mipmap-free",
    )

    proof_sidecar = ROOT / "game" / "art" / "direction-a-import-proof" / "terrain" / "quiet-moss.png.import"
    proof_options = read_import_options(proof_sidecar)
    runtime_uid = options.get(("remap", "uid"))
    require(
        runtime_uid is not None
        and runtime_uid.startswith('"uid://')
        and runtime_uid.endswith('"')
        and runtime_uid != proof_options.get(("remap", "uid")),
        "runtime quiet-moss import needs its own resource UID",
    )

    main_scene = MAIN_SCENE.read_text(encoding="utf-8")
    require(expected_source in main_scene, "Main.tscn does not reference runtime quiet moss")
    require(
        "direction-a-import-proof" not in main_scene
        and "DirectionAImportProof" not in main_scene,
        "ordinary Main.tscn references the proof-only directory or scene",
    )

    print("Runtime terrain validation passed")
    print(f"asset: {ASSET_ID}, exact SHA-256 {asset['sha256']}")
    print("geometry: 256x256; import: lossless, mipmap-free, runtime-owned UID")
    print("isolation: Main.tscn references runtime terrain and no proof-only resource")


if __name__ == "__main__":
    main()
