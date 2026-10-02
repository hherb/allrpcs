#!/usr/bin/env python3
"""Validate the bounded manifest-owned runtime terrain promotions."""

from __future__ import annotations

import hashlib
import json
import struct
from pathlib import Path


ROOT = Path(__file__).resolve().parents[1]
PROOF_ROOT = ROOT / "art-direction" / "production-proof" / "corrected"
MANIFEST = PROOF_ROOT / "asset-manifest.json"
RUNTIME_ROOT = ROOT / "game" / "art" / "runtime" / "terrain"
MAIN_SCENE = ROOT / "game" / "Main.tscn"
RUNTIME_ASSETS = (
    ("terrain.quiet_moss", RUNTIME_ROOT / "quiet-moss.png", (256, 256)),
    (
        "terrain.woodland_leaf_litter",
        RUNTIME_ROOT / "woodland-leaf-litter.png",
        (256, 256),
    ),
    (
        "terrain.woodland_leaf_litter_transition",
        RUNTIME_ROOT / "woodland-leaf-litter-transition.png",
        (256, 256),
    ),
    (
        "terrain.woodland_leaf_litter_corner_transition",
        RUNTIME_ROOT / "woodland-leaf-litter-corner-transition.png",
        (256, 256),
    ),
    (
        "terrain.woodland_leaf_litter_corner_transition_left",
        RUNTIME_ROOT / "woodland-leaf-litter-corner-transition-left.png",
        (256, 256),
    ),
    (
        "terrain.woodland_village_edge_transition",
        RUNTIME_ROOT / "woodland-village-edge-transition.png",
        (1024, 512),
    ),
    ("terrain.straight_path", RUNTIME_ROOT / "straight-path.png", (96, 256)),
    ("terrain.path_end_cap", RUNTIME_ROOT / "path-end-cap.png", (256, 256)),
    (
        "terrain.opening_junction",
        RUNTIME_ROOT / "opening-junction.png",
        (512, 512),
    ),
    (
        "terrain.village_square_junction",
        RUNTIME_ROOT / "village-square-junction.png",
        (512, 512),
    ),
    ("terrain.stream_water", RUNTIME_ROOT / "stream-water.png", (128, 96)),
    ("terrain.square_garden_path", RUNTIME_ROOT / "square-garden-path.png", (200, 220)),
    ("terrain.square_sickhouse_path", RUNTIME_ROOT / "square-sickhouse-path.png", (380, 360)),
    ("terrain.square_southern_road_path", RUNTIME_ROOT / "square-southern-road-path.png", (520, 416)),
    ("terrain.southern_road_wagon_path", RUNTIME_ROOT / "southern-road-wagon-path.png", (560, 432)),
    ("terrain.shrine_southern_road_path", RUNTIME_ROOT / "shrine-southern-road-path.png", (1152, 1080)),
    ("terrain.shrine_southern_road_endpoint_blend", RUNTIME_ROOT / "shrine-southern-road-endpoint-blend.png", (160, 160)),
    ("terrain.woodland_shrine_path", RUNTIME_ROOT / "woodland-shrine-path.png", (704, 208)),
    ("terrain.woodland_village_edge_path", RUNTIME_ROOT / "woodland-village-edge-path.png", (816, 688)),
    ("terrain.village_edge_stream_path", RUNTIME_ROOT / "village-edge-stream-path.png", (224, 528)),
    ("terrain.stream_laundry_path", RUNTIME_ROOT / "stream-laundry-path.png", (432, 416)),
    (
        "terrain.village_edge_square_connector",
        RUNTIME_ROOT / "village-edge-square-connector.png",
        (124, 96),
    ),
    ("terrain.stream_bank", RUNTIME_ROOT / "stream-bank.png", (128, 32)),
    (
        "terrain.stream_bank_opening_cap",
        RUNTIME_ROOT / "stream-bank-opening-cap.png",
        (32, 32),
    ),
)


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
    manifest_assets = {asset["id"]: asset for asset in manifest["assets"]}
    proof_import_root = ROOT / "game" / "art" / "direction-a-import-proof" / "terrain"
    proof_uids = {
        read_import_options(sidecar).get(("remap", "uid"))
        for sidecar in proof_import_root.glob("*.png.import")
    }
    require(
        set(RUNTIME_ROOT.glob("*.png")) == {path for _, path, _ in RUNTIME_ASSETS},
        "runtime terrain differs from the bounded manifest-owned asset set",
    )

    main_scene = MAIN_SCENE.read_text(encoding="utf-8")
    runtime_uids: set[str] = set()
    for asset_id, runtime_texture, expected_dimensions in RUNTIME_ASSETS:
        asset = manifest_assets.get(asset_id)
        require(asset is not None, f"expected one {asset_id} manifest entry")
        source = PROOF_ROOT / asset["export"]
        require(
            tuple(asset["export_rect_pixels"][key] for key in ("width", "height"))
            == expected_dimensions,
            f"{asset_id} manifest geometry drifted",
        )
        require(runtime_texture.is_file(), f"missing runtime texture for {asset_id}")
        require(
            not runtime_texture.is_symlink(),
            f"runtime texture for {asset_id} must be owned, not linked",
        )
        require(
            sha256(source) == asset["sha256"] == sha256(runtime_texture),
            f"runtime texture for {asset_id} lost exact manifest/source parity",
        )
        require(
            png_dimensions(runtime_texture) == expected_dimensions,
            f"runtime texture for {asset_id} has unexpected dimensions",
        )

        sidecar = runtime_texture.with_suffix(".png.import")
        require(sidecar.is_file(), f"missing runtime import sidecar for {asset_id}")
        options = read_import_options(sidecar)
        expected_source = resource_path(runtime_texture)
        expected_cache = imported_cache_path(runtime_texture)
        require(
            options.get(("deps", "source_file")) == f'"{expected_source}"',
            f"runtime import source path drifted for {asset_id}",
        )
        require(
            options.get(("remap", "path")) == f'"{expected_cache}"'
            and options.get(("deps", "dest_files")) == f'["{expected_cache}"]',
            f"runtime import cache path drifted for {asset_id}",
        )
        require(
            options.get(("remap", "importer")) == '"texture"'
            and options.get(("params", "compress/mode")) == "0"
            and options.get(("params", "mipmaps/generate")) == "false",
            f"runtime import is not lossless and mipmap-free for {asset_id}",
        )

        runtime_uid = options.get(("remap", "uid"))
        require(
            runtime_uid is not None
            and runtime_uid.startswith('"uid://')
            and runtime_uid.endswith('"')
            and runtime_uid not in proof_uids
            and runtime_uid not in runtime_uids,
            f"runtime import for {asset_id} needs its own unique resource UID",
        )
        runtime_uids.add(runtime_uid)
        require(
            expected_source in main_scene,
            f"Main.tscn does not reference runtime texture for {asset_id}",
        )

    require(
        "direction-a-import-proof" not in main_scene
        and "DirectionAImportProof" not in main_scene,
        "ordinary Main.tscn references the proof-only directory or scene",
    )

    print("Runtime terrain validation passed")
    for asset_id, _, _ in RUNTIME_ASSETS:
        print(f"asset: {asset_id}, exact SHA-256 {manifest_assets[asset_id]['sha256']}")
    print(
        "geometry: six 256x256 assets, one 96x256 path, two 512x512 junctions, one "
        "1024x512 feather overlay, one 124x96 connector, one 200x220 Garden path and one 380x360 Sickhouse path, one 520x416 Southern Road path, one 560x432 Road–Wagon path, two Shrine layers (1152x1080 and 160x160), and three Stream modules; "
        "imports: lossless, mipmap-free, runtime-owned UIDs"
    )
    print("isolation: Main.tscn references runtime terrain and no proof-only resource")


if __name__ == "__main__":
    main()
