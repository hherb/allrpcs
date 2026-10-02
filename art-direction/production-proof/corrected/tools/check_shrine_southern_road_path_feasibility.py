#!/usr/bin/env python3
"""Validate archived external Shrine–Southern Road strip views and retained authority."""

import argparse
import hashlib
import json
import math

from PIL import Image

from check_village_edge_square_connector_feasibility import ROOT, REVIEW, require

EVIDENCE_HASH = "4450bce720c00703d3e6157d0ef95677a80f6a8738e1c1fef98b6761a1f71707"


def digest(path):
    """Return one exact file digest without executing its contents."""
    return hashlib.sha256(path.read_bytes()).hexdigest()


def validate(check_live_baseline=False):
    """Verify six saved views, recipe/source identity, and optional frozen runtime inputs."""
    record = REVIEW / "shrine-southern-road-path-evidence.json"
    require(digest(record) == EVIDENCE_HASH, "Shrine–Road evidence record changed")
    evidence = json.loads(record.read_text())
    for relative, expected in evidence["inputs"].items():
        require(digest(ROOT / relative) == expected, f"proof input changed: {relative}")
    names = {f"shrine-southern-road-path-proof-{zoom}x-{frame}.png"
             for zoom in ("0.8", "1.0", "2.0") for frame in ("wide", "tall")}
    require(set(evidence["images"]) == names, "expected exactly six Shrine–Road strip views")
    for name, expected in evidence["images"].items():
        path = REVIEW / name
        require(digest(path) == expected, f"archived image changed: {name}")
        with Image.open(path) as image:
            require(image.mode == "RGBA" and image.size == (
                1280, 827 if name.endswith("tall.png") else 720), f"invalid view: {name}")
    with Image.open(ROOT / "game/art/runtime/terrain/straight-path.png") as image:
        require(image.mode == "RGBA" and image.size == (96, 256), "path source geometry changed")
    require(evidence["geometry"] == {
        "route_start": [380, 300], "route_end": [1500, 1400], "route_width": 96,
        "upstream_fill_start": [572, 460], "upstream_fill_end": [1500, 1400],
    }, "retained route/fill geometry changed")
    # Independently quantify the authored centerline mismatch near the shrine exit.
    length = math.hypot(1120, 1100)
    offset = abs(1120 * 160 - 1100 * 192) / length
    require(20 < offset < 21, "recorded shrine-exit centerline mismatch changed")
    if check_live_baseline:
        for relative, expected in evidence["baseline"].items():
            require(digest(ROOT / relative) == expected, f"runtime baseline changed: {relative}")
        print("Frozen runtime/manifest/physical baseline unchanged; no promotion.")
    print(f"Shrine–Road evidence valid: six failed strip views; {offset:.3f}px exit centerline offset.")


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--check-live-baseline", action="store_true")
    validate(parser.parse_args().check_live_baseline)
