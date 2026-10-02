#!/usr/bin/env python3
"""Verify a reproducible external seam patch and capture changes confined to its projection."""

import argparse
import json

from PIL import Image, ImageChops

from build_shrine_southern_road_path_blend import (
    BLEND_HALF_LENGTH, END, LEADING_EXTENSION, ORIGIN, ROAD_DIRECTION, SIZE, compose,
)
from check_shrine_southern_road_path_correction import validate as validate_original
from check_shrine_southern_road_path_feasibility import digest
from check_village_edge_square_connector_feasibility import ROOT, REVIEW, require

EVIDENCE_HASH = "7fcd4049139cbdab558da6b7a7b7204a2babe8cc2ac7eb1654f7448558fa1d42"
TARGETS = {"midpoint": (940, 850), "shrine-exit": (543, 460), "junction": (1500, 1400)}


def projected_bounds(target, zoom, height):
    """Project the patch using the unchanged 3200x1800 ordinary camera limits."""
    half_x, half_y = 640 / zoom, height / (2 * zoom)
    center_x = min(max(target[0], half_x), 3200 - half_x)
    center_y = min(max(target[1], half_y), 1800 - half_y)
    # Allow two screen pixels for ordinary linear texture filtering.
    return ((ORIGIN[0] - center_x) * zoom + 640 - 2,
            (ORIGIN[1] - center_y) * zoom + height / 2 - 2,
            (ORIGIN[0] + SIZE[0] - center_x) * zoom + 640 + 2,
            (ORIGIN[1] + SIZE[1] - center_y) * zoom + height / 2 + 2)


def validate(check_live_baseline=False):
    """Keep original evidence intact; prove local patch/capture scope and unchanged authority."""
    validate_original(check_live_baseline)
    record = REVIEW / "shrine-southern-road-path-blend-evidence.json"
    require(digest(record) == EVIDENCE_HASH, "endpoint blend evidence changed")
    evidence = json.loads(record.read_text())
    for relative, expected in evidence["inputs"].items():
        require(digest(ROOT / relative) == expected, f"endpoint blend input changed: {relative}")
    names = {f"shrine-southern-road-path-blend-{view}-{zoom}x-{frame}.png"
             for view in TARGETS for zoom in ("0.8", "1.0", "2.0") for frame in ("wide", "tall")}
    names.add("shrine-southern-road-path-blend-overlay.png")
    require(set(evidence["images"]) == names, "expected 18 blend views and one patch")
    for name, expected in evidence["images"].items():
        require(digest(REVIEW / name) == expected, f"endpoint blend image changed: {name}")
    with Image.open(REVIEW / "shrine-southern-road-path-blend-overlay.png") as image:
        patch = image.copy()
    require(patch.mode == "RGBA" and patch.size == SIZE, "patch geometry changed")
    require(compose().tobytes() == patch.tobytes(), "endpoint blend is not reproducible")
    visible = 0
    for y in range(SIZE[1]):
        for x in range(SIZE[0]):
            pixel = patch.getpixel((x, y))
            if not pixel[3]:
                require(pixel == (0, 0, 0, 0), "hidden RGB introduced")
            if x in (0, SIZE[0] - 1) or y in (0, SIZE[1] - 1):
                require(pixel[3] == 0, "patch canvas clips visible pixels")
            if pixel[3]:
                visible += 1
                along = ((ORIGIN[0] + x + 0.5 - END[0]) * ROAD_DIRECTION[0]
                         + (ORIGIN[1] + y + 0.5 - END[1]) * ROAD_DIRECTION[1] + LEADING_EXTENSION)
                require(abs(along) < BLEND_HALF_LENGTH, "patch escapes the 48px endpoint band")
    require(0 < visible < 6000, "patch exceeds bounded local scope")
    changed_views = 0
    for view, target in TARGETS.items():
        for zoom in (0.8, 1.0, 2.0):
            for frame, height in (("wide", 720), ("tall", 827)):
                suffix = f"{view}-{zoom:.1f}x-{frame}.png"
                with Image.open(REVIEW / f"shrine-southern-road-path-blend-{suffix}") as image:
                    capture = image.copy()
                require(capture.mode == "RGBA" and capture.size == (1280, height), "invalid blend view")
                with Image.open(REVIEW / f"shrine-southern-road-path-correction-{suffix}") as image:
                    before = image.copy()
                require(capture.getchannel("A").getextrema() == (255, 255)
                        and before.getchannel("A").getextrema() == (255, 255), "viewport alpha changed")
                bounds = ImageChops.difference(capture, before).convert("RGB").getbbox()
                if bounds:
                    changed_views += 1
                    left, top, right, bottom = projected_bounds(target, zoom, height)
                    require(bounds[0] >= left and bounds[1] >= top and bounds[2] <= right
                            and bounds[3] <= bottom, f"capture changed outside endpoint patch: {suffix}")
                require(bounds is not None or view != "junction", f"junction patch not visible: {suffix}")
    require(evidence["visible_patch_pixels"] == visible, "patch coverage changed")
    print(f"Shrine–Road endpoint blend valid: {visible} local pixels, {changed_views}/18 views changed only within patch.")


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--check-live-baseline", action="store_true")
    validate(parser.parse_args().check_live_baseline)
