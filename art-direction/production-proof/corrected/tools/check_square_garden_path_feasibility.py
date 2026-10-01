#!/usr/bin/env python3
"""Validate archived external Square–Garden proofs without granting runtime authority."""

import argparse
import hashlib
import json
import math

from PIL import Image

from build_square_garden_path_correction import ORIGIN, SIZE, compose
from check_village_edge_square_connector_feasibility import ROOT, REVIEW, require

EVIDENCE_HASH = "ea59807bcab4317b8d7e8a779852974b5fd203f7cae2beb1d6ddd645e2bc42ab"


def digest(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def validate(check_live_baseline=False):
    evidence_path = REVIEW / "square-garden-path-evidence.json"
    require(digest(evidence_path) == EVIDENCE_HASH, "Square–Garden evidence record changed")
    evidence = json.loads(evidence_path.read_text())
    for relative, expected in evidence["baseline"].items():
        # Ordinary scene/code hashes are optional frozen-slice checks. Recipes and
        # texture hashes always bind the archived captures and deterministic overlay.
        if check_live_baseline or not relative.endswith((".cs", ".tscn")):
            require(digest(ROOT / relative) == expected, f"proof input changed: {relative}")
    require(len(evidence["images"]) == 13, "expected twelve views and one overlay")
    for name, expected in evidence["images"].items():
        path = REVIEW / name
        require(digest(path) == expected, f"archived image changed: {name}")
        with Image.open(path) as image:
            size = SIZE if name.endswith("overlay.png") else (
                1280, 827 if name.endswith("tall.png") else 720)
            require(image.mode == "RGBA" and image.size == size, f"invalid image: {name}")
    with Image.open(REVIEW / "square-garden-path-correction-overlay.png") as image:
        overlay = image.copy()
    require(compose().tobytes() == overlay.tobytes(), "overlay is not reproducible")
    with Image.open(ROOT / "game/art/runtime/terrain/village-square-junction.png") as image:
        junction = image.copy()
    visible = 0
    length = math.hypot(100, 400)
    for y in range(SIZE[1]):
        for x in range(SIZE[0]):
            pixel = overlay.getpixel((x, y))
            if pixel[3] == 0:
                require(pixel == (0, 0, 0, 0), "hidden RGB introduced")
                continue
            along = ((ORIGIN[0] + x + 0.5 - 2200) * 100 / length
                     + (ORIGIN[1] + y + 0.5 - 900) * 400 / length)
            require(along <= length, "overlay exceeds the Garden endpoint")
            if y < 16:
                visible += 1
                require(pixel == junction.getpixel((x + 256, y + 496)),
                        "junction overlap loses exact source phase/alpha")
    require(visible == 1400, "junction overlap coverage changed")
    print("Square–Garden external evidence valid: twelve views, reproducible overlay, "
          "1,400 exact overlap pixels, retained endpoint.")
    if check_live_baseline:
        print("Frozen ordinary scene/code baseline unchanged; no runtime promotion.")


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--check-live-baseline", action="store_true")
    validate(parser.parse_args().check_live_baseline)
