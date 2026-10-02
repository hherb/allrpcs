#!/usr/bin/env python3
"""Validate a bounded external shrine-entry clip and independently projected capture changes."""

import argparse
import json

from PIL import ImageChops

from build_woodland_shrine_path_correction import (
    MARKER_HALF_DEPTH, OPENING_CENTER_Y, OPENING_HALF_HEIGHT, SHRINE_RIGHT, compose,
)
from build_woodland_shrine_path_feasibility import ORIGIN, SIZE
from check_shrine_southern_road_path_feasibility import digest
from check_village_edge_square_connector_feasibility import ROOT, REVIEW, require
from check_woodland_shrine_path_feasibility import TARGETS, rgba, validate as validate_original, validate_images

EVIDENCE_HASH = '3a0ce3f2909b6a6b3c897c33914f1000a33d078db0e86adfe98414aeec5ca277'


def projected_bounds(target, zoom, height):
    """Project only the clip region using ordinary 3200x1800 camera limits plus 2px filtering."""
    half_x, half_y = 640 / zoom, height / (2 * zoom)
    cx = min(max(target[0], half_x), 3200 - half_x)
    cy = min(max(target[1], half_y), 1800 - half_y)
    return ((ORIGIN[0] - cx) * zoom + 638, (ORIGIN[1] - cy) * zoom + height / 2 - 2,
            (SHRINE_RIGHT + MARKER_HALF_DEPTH - cx) * zoom + 642,
            (ORIGIN[1] + SIZE[1] - cy) * zoom + height / 2 + 2)


def validate(check_live_baseline=False):
    """Preserve failed evidence, prove exact pixel removal and independently bounded screen changes."""
    validate_original(check_live_baseline)
    record_path = REVIEW / 'woodland-shrine-path-correction-evidence.json'
    require(digest(record_path) == EVIDENCE_HASH, 'Woodland–Shrine correction evidence changed')
    evidence = json.loads(record_path.read_text())
    for relative, sha in evidence['inputs'].items():
        require(digest(ROOT / relative) == sha, f'correction input changed: {relative}')
    require(evidence['clipping'] == {
        'shrine_right': SHRINE_RIGHT, 'opening_center_y': OPENING_CENTER_Y,
        'opening_half_height': OPENING_HALF_HEIGHT, 'marker_half_depth': MARKER_HALF_DEPTH,
    }, 'correction clipping geometry changed')
    validate_images(evidence, 'correction')
    original = rgba(REVIEW / 'woodland-shrine-path-feasibility-overlay.png')
    corrected = rgba(REVIEW / 'woodland-shrine-path-correction-overlay.png')
    require(compose().tobytes() == corrected.tobytes(), 'correction is not reproducible')
    removed = 0
    for y in range(SIZE[1]):
        for x in range(SIZE[0]):
            wx, wy = ORIGIN[0] + x + .5, ORIGIN[1] + y + .5
            clipped = (wx < SHRINE_RIGHT or (wx < SHRINE_RIGHT + MARKER_HALF_DEPTH
                       and abs(wy - OPENING_CENTER_Y) > OPENING_HALF_HEIGHT))
            before, after = original.getpixel((x, y)), corrected.getpixel((x, y))
            require(after == ((0, 0, 0, 0) if clipped else before), 'correction changed unclipped pixels')
            removed += bool(clipped and before[3])
    require(removed > 0, 'correction removes no visible pixels')
    changed_views = 0
    for view, target in TARGETS.items():
        for zoom in (.8, 1., 2.):
            for frame, height in (('wide', 720), ('tall', 827)):
                suffix = f'{view}-{zoom:.1f}x-{frame}.png'
                before = rgba(REVIEW / f'woodland-shrine-path-feasibility-{suffix}')
                after = rgba(REVIEW / f'woodland-shrine-path-correction-{suffix}')
                bounds = ImageChops.difference(before, after).convert('RGB').getbbox()
                if bounds:
                    changed_views += 1
                    left, top, right, bottom = projected_bounds(target, zoom, height)
                    require(bounds[0] >= left and bounds[1] >= top and bounds[2] <= right
                            and bounds[3] <= bottom, f'capture changed outside clip region: {suffix}')
                require(bounds is not None or view != 'shrine-entry', f'shrine clip not visible: {suffix}')
    print(f'Woodland–Shrine correction valid: {removed} removed pixels; {changed_views}/18 views changed inside clip.')


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--check-live-baseline', action='store_true')
    validate(parser.parse_args().check_live_baseline)
