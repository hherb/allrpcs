#!/usr/bin/env python3
"""Validate immutable Woodland–Shrine strip evidence without granting promotion authority."""

import argparse
import json
import math

from PIL import Image

from build_woodland_shrine_path_feasibility import END, ORIGIN, SIZE, START, compose
from check_shrine_southern_road_path_correction import renderer_digest
from check_shrine_southern_road_path_feasibility import digest
from check_village_edge_square_connector_feasibility import ROOT, REVIEW, require

EVIDENCE_HASH = 'e7bf9b06198a74b490e0b9c06e6d3da82c2791423de3034e326eeb5496a1a6d0'
TARGETS = {'midpoint': (690, 325), 'clearing-junction': START, 'shrine-entry': (620, 319.354839)}


def rgba(path):
    """Load exact RGBA pixels without implicit conversion."""
    with Image.open(path) as image:
        require(image.mode == 'RGBA', f'not RGBA: {path}')
        return image.copy()


def image_names(kind):
    """Name the exact 18-view capture matrix and one overlay."""
    names = {f'woodland-shrine-path-{kind}-{view}-{zoom}x-{frame}.png'
             for view in TARGETS for zoom in ('0.8', '1.0', '2.0') for frame in ('wide', 'tall')}
    names.add(f'woodland-shrine-path-{kind}-overlay.png')
    return names


def validate_images(evidence, kind):
    """Pin exact file set/hashes and require opaque full-resolution viewport captures."""
    require(set(evidence['images']) == image_names(kind), 'expected 18 views and one overlay')
    for name, sha in evidence['images'].items():
        path = REVIEW / name
        require(digest(path) == sha, f'archived image changed: {name}')
        image = rgba(path)
        if name.endswith('-overlay.png'):
            require(image.size == SIZE, 'overlay canvas changed')
        else:
            require(image.size == (1280, 827 if name.endswith('tall.png') else 720)
                    and image.getchannel('A').getextrema() == (255, 255), f'invalid view: {name}')


def validate(check_live_baseline=False):
    """Reproduce the failed strip and its exact opening mismatch; optionally pin live authority."""
    path = REVIEW / 'woodland-shrine-path-feasibility-evidence.json'
    require(digest(path) == EVIDENCE_HASH, 'Woodland–Shrine feasibility evidence changed')
    evidence = json.loads(path.read_text())
    for relative, sha in evidence['inputs'].items():
        require(digest(ROOT / relative) == sha, f'proof input changed: {relative}')
    require(renderer_digest() == evidence['renderer_sha256'], 'shared path renderer changed')
    require(evidence['geometry'] == {
        'route_start': list(START), 'route_end': list(END), 'origin': list(ORIGIN), 'size': list(SIZE),
        'route_width': 96, 'woodland_opening': [600, 317.741935483871], 'shrine_opening': [620, 300],
        'opening_height': 96, 'marker_half_depth': 8,
    }, 'route/opening geometry changed')
    validate_images(evidence, 'feasibility')
    overlay = rgba(REVIEW / 'woodland-shrine-path-feasibility-overlay.png')
    require(compose().tobytes() == overlay.tobytes(), 'strip is not reproducible')
    outside = 0
    for y in range(SIZE[1]):
        for x in range(SIZE[0]):
            pixel = overlay.getpixel((x, y))
            require(pixel[3] != 0 or pixel == (0, 0, 0, 0), 'hidden RGB introduced')
            if x in (0, SIZE[0] - 1) or y in (0, SIZE[1] - 1):
                require(pixel[3] == 0, 'overlay gutter clips visible pixels')
            wx, wy = ORIGIN[0] + x + .5, ORIGIN[1] + y + .5
            if pixel[3] and 612 <= wx <= 628 and not 252 <= wy <= 348:
                outside += 1
    crossing = START[1] + (620 - START[0]) * (END[1] - START[1]) / (END[0] - START[0])
    require(math.isclose(crossing - 300, 19.354838709677438, abs_tol=1e-10), 'opening offset changed')
    require(outside == evidence['visible_samples_outside_shrine_opening'] == 229,
            'recorded opening fringe changed')
    if check_live_baseline:
        require(len(evidence['baseline']) == 432, 'frozen baseline file set changed')
        for relative, sha in evidence['baseline'].items():
            require(digest(ROOT / relative) == sha, f'live baseline changed: {relative}')
        print('Frozen 432-file runtime/production/import/physical baseline unchanged.')
    print('Woodland–Shrine failed strip valid: 18 views; 19.355px offset, 229 samples outside opening.')


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--check-live-baseline', action='store_true')
    validate(parser.parse_args().check_live_baseline)
