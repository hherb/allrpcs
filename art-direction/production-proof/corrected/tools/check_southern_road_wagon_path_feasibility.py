#!/usr/bin/env python3
"""Validate archived external Road–Wagon strip views and optional live baseline."""

import argparse
import hashlib
import json

from PIL import Image

from check_village_edge_square_connector_feasibility import ROOT, REVIEW, require

EVIDENCE_HASH = "41542680b616d6b2b8ed9ec83df7724ff9174a0554103ed6dd7853ce886d84b7"


def digest(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def validate(check_live_baseline=False):
    evidence_path = REVIEW / "southern-road-wagon-path-evidence.json"
    require(digest(evidence_path) == EVIDENCE_HASH, "Road–Wagon evidence record changed")
    evidence = json.loads(evidence_path.read_text())
    for relative, expected in evidence["inputs"].items():
        require(digest(ROOT / relative) == expected, f"proof input changed: {relative}")
    expected_names = {
        f"southern-road-wagon-path-proof-{zoom}x-{frame}.png"
        for zoom in ["0.8", "1.0", "2.0"] for frame in ["wide", "tall"]
    }
    require(set(evidence["images"]) == expected_names, "expected six external strip views")
    for name, expected in evidence["images"].items():
        path = REVIEW / name
        require(digest(path) == expected, f"archived image changed: {name}")
        with Image.open(path) as image:
            require(image.mode == "RGBA" and image.size == (
                1280, 827 if name.endswith("tall.png") else 720), f"invalid view: {name}")
    if check_live_baseline:
        for relative, expected in evidence["baseline"].items():
            require(digest(ROOT / relative) == expected, f"runtime baseline changed: {relative}")
        print("Frozen runtime/manifest/geometry baseline unchanged; no promotion.")
    print("Road–Wagon external evidence valid: six retained failed-strip views and exact inputs.")


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--check-live-baseline", action="store_true")
    validate(parser.parse_args().check_live_baseline)
