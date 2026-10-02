#!/usr/bin/env python3
"""Validate reproducible external correction evidence and optional frozen runtime inputs."""

import argparse
import json

from PIL import Image

from build_southern_road_wagon_path_correction import SIZE, compose
from check_southern_road_wagon_path_feasibility import digest, validate as validate_strip
from check_village_edge_square_connector_feasibility import ROOT, REVIEW, require

EVIDENCE_HASH = "c829b0f56af8be4a3629cbbbd84195424b3f6f5be3487e6a0cedef43ed5f4a0a"


def validate(check_live_baseline=False):
    validate_strip(check_live_baseline)
    record = REVIEW / "southern-road-wagon-path-correction-evidence.json"
    require(digest(record) == EVIDENCE_HASH, "correction evidence record changed")
    evidence = json.loads(record.read_text())
    for relative, expected in evidence["inputs"].items():
        require(digest(ROOT / relative) == expected, f"correction input changed: {relative}")
    names = {f"southern-road-wagon-path-correction-{zoom}x-{frame}.png"
             for zoom in ("0.8", "1.0", "2.0") for frame in ("wide", "tall")}
    names.add("southern-road-wagon-path-correction-overlay.png")
    require(set(evidence["images"]) == names, "expected six corrected views and overlay")
    for name, expected in evidence["images"].items():
        require(digest(REVIEW / name) == expected, f"correction image changed: {name}")
        with Image.open(REVIEW / name) as image:
            size = SIZE if name.endswith("overlay.png") else (
                1280, 827 if name.endswith("tall.png") else 720)
            require(image.mode == "RGBA" and image.size == size, f"invalid image: {name}")
    with Image.open(REVIEW / "southern-road-wagon-path-correction-overlay.png") as image:
        overlay = image.copy()
    require(compose().tobytes() == overlay.tobytes(), "correction overlay is not reproducible")
    for y in range(SIZE[1]):
        for x in range(SIZE[0]):
            pixel = overlay.getpixel((x, y))
            if pixel[3] == 0:
                require(pixel == (0, 0, 0, 0), "hidden RGB introduced")
            if x in (0, SIZE[0] - 1) or y in (0, SIZE[1] - 1):
                require(pixel[3] == 0, "external canvas clips visible texture")
    print("Road–Wagon correction valid: six views, reproducible overlay, retained failed strip.")


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--check-live-baseline", action="store_true")
    validate(parser.parse_args().check_live_baseline)
