#!/usr/bin/env python3
"""Validate reproducible external correction evidence and optional frozen runtime inputs."""

import argparse
import json
import math

from PIL import Image

from build_southern_road_wagon_path_trim import ORIGIN, SIZE, INCOMING_EXTENSION, compose
from check_southern_road_wagon_path_correction import validate as validate_previous
from check_southern_road_wagon_path_feasibility import digest
from check_village_edge_square_connector_feasibility import ROOT, REVIEW, require

EVIDENCE_HASH = "40b3901e34c1f6617bf78c6b817e584d5465917ed2ed860b671ed2b93bade4d3"


def validate(check_live_baseline=False):
    validate_previous(check_live_baseline)
    record = REVIEW / "southern-road-wagon-path-trim-evidence.json"
    require(digest(record) == EVIDENCE_HASH, "correction evidence record changed")
    evidence = json.loads(record.read_text())
    for relative, expected in evidence["inputs"].items():
        require(digest(ROOT / relative) == expected, f"correction input changed: {relative}")
    names = {f"southern-road-wagon-path-trim-{zoom}x-{frame}.png"
             for zoom in ("0.8", "1.0", "2.0") for frame in ("wide", "tall")}
    names.add("southern-road-wagon-path-trim-overlay.png")
    require(set(evidence["images"]) == names, "expected six corrected views and overlay")
    for name, expected in evidence["images"].items():
        require(digest(REVIEW / name) == expected, f"correction image changed: {name}")
        with Image.open(REVIEW / name) as image:
            size = SIZE if name.endswith("overlay.png") else (
                1280, 827 if name.endswith("tall.png") else 720)
            require(image.mode == "RGBA" and image.size == size, f"invalid image: {name}")
    with Image.open(REVIEW / "southern-road-wagon-path-trim-overlay.png") as image:
        overlay = image.copy()
    require(compose().tobytes() == overlay.tobytes(), "correction overlay is not reproducible")
    for y in range(SIZE[1]):
        for x in range(SIZE[0]):
            pixel = overlay.getpixel((x, y))
            if pixel[3] == 0:
                require(pixel == (0, 0, 0, 0), "hidden RGB introduced")
            if x in (0, SIZE[0] - 1) or y in (0, SIZE[1] - 1):
                require(pixel[3] == 0, "external canvas clips visible texture")
    with Image.open(REVIEW / "southern-road-wagon-path-correction-overlay.png") as image:
        previous = image.copy()
    length = math.hypot(-700, 500)
    changed = 0
    for y in range(SIZE[1]):
        for x in range(SIZE[0]):
            before, after = previous.getpixel((x, y)), overlay.getpixel((x, y))
            if before != after:
                changed += 1
                dx, dy = ORIGIN[0] + x + 0.5 - 2200, ORIGIN[1] + y + 0.5 - 900
                along = (dx * -700 + dy * 500) / length
                require(length + INCOMING_EXTENSION - 1 < along < length + 51,
                        "trim altered pixels outside the incoming tip")
                require(after[3] <= before[3], "trim added visible coverage")
    require(changed > 0, "junction tip was not shortened")
    print(f"Road–Wagon trim valid: six views; {changed} pixels confined to the incoming tip.")


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--check-live-baseline", action="store_true")
    validate(parser.parse_args().check_live_baseline)
