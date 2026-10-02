#!/usr/bin/env python3
"""Validate external correction evidence without granting runtime promotion authority."""

import argparse
import ast
import hashlib
import json
import math

from PIL import Image

from build_shrine_southern_road_path_correction import (
    END, OPENING_CENTER_X, OPENING_HALF_WIDTH, OPENING_MARKER_HALF_DEPTH,
    ORIGIN, SHRINE_BOTTOM, SIZE, START, compose,
)
from check_shrine_southern_road_path_feasibility import digest, validate as validate_failed_strip
from check_village_edge_square_connector_feasibility import ROOT, REVIEW, require

EVIDENCE_HASH = "574c49f626023075188949db5a5628cc935b29fc92bd3dc9a5d7eb6cc082dffb"
VIEWS = ("midpoint", "shrine-exit", "junction")


def renderer_digest():
    """Pin only the shared renderer, allowing unrelated future production additions."""
    source = (ROOT / "art-direction/production-proof/corrected/tools/build.py").read_text()
    renderers = [node for node in ast.parse(source).body
                 if isinstance(node, ast.FunctionDef) and node.name == "render_path_leg"]
    require(len(renderers) == 1, "expected one shared path renderer")
    return hashlib.sha256(ast.get_source_segment(source, renderers[0]).encode()).hexdigest()


def validate(check_live_baseline=False):
    """Check 18 endpoint/midpoint views, entry geometry, reproducibility and frozen authority."""
    validate_failed_strip(check_live_baseline)
    record = REVIEW / "shrine-southern-road-path-correction-evidence.json"
    require(digest(record) == EVIDENCE_HASH, "Shrine–Road correction evidence changed")
    evidence = json.loads(record.read_text())
    for relative, expected in evidence["inputs"].items():
        require(digest(ROOT / relative) == expected, f"correction input changed: {relative}")
    require(renderer_digest() == evidence["renderer_sha256"], "shared path renderer changed")
    require(evidence["geometry"] == {
        "origin": list(ORIGIN), "size": list(SIZE), "route_start": list(START),
        "route_end": list(END), "shrine_bottom": SHRINE_BOTTOM,
        "opening_half_width": OPENING_HALF_WIDTH, "opening_marker_half_depth": OPENING_MARKER_HALF_DEPTH,
        "view_targets": {"midpoint": [940, 850], "shrine-exit": [543, 460], "junction": [1500, 1400]},
        "z_index": -72,
    }, "correction geometry changed")
    require(math.isclose(OPENING_CENTER_X, 542.9091, abs_tol=0.0001), "route misses retained shrine opening")
    names = {f"shrine-southern-road-path-correction-{view}-{zoom}x-{frame}.png"
             for view in VIEWS for zoom in ("0.8", "1.0", "2.0") for frame in ("wide", "tall")}
    names.add("shrine-southern-road-path-correction-overlay.png")
    require(set(evidence["images"]) == names, "expected 18 corrected views and one overlay")
    for name, expected in evidence["images"].items():
        require(digest(REVIEW / name) == expected, f"correction image changed: {name}")
        with Image.open(REVIEW / name) as image:
            size = SIZE if name.endswith("overlay.png") else (
                1280, 827 if name.endswith("tall.png") else 720)
            require(image.mode == "RGBA" and image.size == size, f"invalid view: {name}")
    with Image.open(REVIEW / "shrine-southern-road-path-correction-overlay.png") as image:
        overlay = image.copy()
    require(compose().tobytes() == overlay.tobytes(), "correction overlay is not reproducible")
    pixels = overlay.load()
    for y in range(SIZE[1]):
        for x in range(SIZE[0]):
            pixel = pixels[x, y]
            if pixel[3] == 0:
                require(pixel == (0, 0, 0, 0), "hidden RGB introduced")
            if x in (0, SIZE[0] - 1) or y in (0, SIZE[1] - 1):
                require(pixel[3] == 0, "canvas clips visible texture")
            world_x, world_y = ORIGIN[0] + x + 0.5, ORIGIN[1] + y + 0.5
            if world_y < SHRINE_BOTTOM or (
                    world_y < SHRINE_BOTTOM + OPENING_MARKER_HALF_DEPTH
                    and abs(world_x - OPENING_CENTER_X) > OPENING_HALF_WIDTH):
                require(pixel[3] == 0, "texture crosses shrine shell outside the opening")
    if check_live_baseline:
        for relative, expected in evidence["baseline"].items():
            require(digest(ROOT / relative) == expected, f"frozen authority changed: {relative}")
        print("Runtime, imports, production and physical baseline unchanged; external correction only.")
    print("Shrine–Road correction valid: 18 views; junction seam awaits owner review.")


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--check-live-baseline", action="store_true")
    validate(parser.parse_args().check_live_baseline)
