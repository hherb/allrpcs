#!/usr/bin/env python3
"""Validate external Southern Road evidence without granting runtime authority."""

import argparse
import hashlib
import json
import math

from PIL import Image

from build_square_southern_road_path_correction import ORIGIN, SIZE, compose
from check_village_edge_square_connector_feasibility import ROOT, REVIEW, require

EVIDENCE_HASH = "4e24aa43341245c6c582e9ec1cd1de0e1c8708a11b2f2aa48daf102732ede365"


def digest(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def validate(check_live_baseline=False):
    evidence_path = REVIEW / "square-southern-road-path-evidence.json"
    require(digest(evidence_path) == EVIDENCE_HASH, "Southern Road evidence record changed")
    evidence = json.loads(evidence_path.read_text())
    for relative, expected in evidence["baseline"].items():
        if check_live_baseline or not relative.endswith((".cs", ".tscn")):
            require(digest(ROOT / relative) == expected, f"proof input changed: {relative}")
    expected_names = {
        f"square-southern-road-path-{mode}-{zoom:.1f}x-{frame}.png"
        for mode in ("proof", "correction")
        for zoom in (0.8, 1.0, 2.0)
        for frame in ("wide", "tall")
    } | {"square-southern-road-path-correction-overlay.png"}
    require(set(evidence["images"]) == expected_names, "expected twelve views and overlay")
    for name, expected in evidence["images"].items():
        path = REVIEW / name
        require(digest(path) == expected, f"archived image changed: {name}")
        with Image.open(path) as image:
            size = SIZE if name.endswith("overlay.png") else (
                1280, 827 if name.endswith("tall.png") else 720)
            require(image.mode == "RGBA" and image.size == size, f"invalid image: {name}")
    with Image.open(REVIEW / "square-southern-road-path-correction-overlay.png") as image:
        overlay = image.copy()
    require(compose().tobytes() == overlay.tobytes(), "overlay is not reproducible")
    with Image.open(ROOT / "game/art/runtime/terrain/village-square-junction.png") as image:
        junction = image.copy()
    length = math.hypot(-700, 500)
    visible = 0
    for y in range(SIZE[1]):
        for x in range(SIZE[0]):
            pixel = overlay.getpixel((x, y))
            if pixel[3] == 0:
                require(pixel == (0, 0, 0, 0), "hidden RGB introduced")
            else:
                along = ((ORIGIN[0] + x + 0.5 - 2200) * -700 / length
                         + (ORIGIN[1] + y + 0.5 - 900) * 500 / length)
                require(along <= length, "overlay exceeds the Southern Road endpoint")
                require(x != 0 and y != SIZE[1] - 1
                        and (y != 0 or x >= 504)
                        and (x != SIZE[0] - 1 or y < 132),
                        "external canvas clips the path side or endpoint")
            # Compare both alpha-zero and visible pixels in the entire intersection.
            if y < 132 and x >= 504:
                require(pixel == junction.getpixel((x - 504, y + 380)),
                        "junction overlap loses exact source phase/alpha")
                visible += pixel[3] > 0
    require(visible == 1662, "junction overlap coverage changed")
    print("Square–Southern Road external evidence valid: twelve views, reproducible overlay, "
          "1,662 exact overlap pixels, retained endpoint.")
    if check_live_baseline:
        print("Frozen ordinary scene/code baseline unchanged; no runtime promotion.")


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--check-live-baseline", action="store_true")
    validate(parser.parse_args().check_live_baseline)
