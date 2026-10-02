#!/usr/bin/env python3
"""Check the external Clearing join, archived evidence and independently bounded screen changes."""

import argparse
import json

from PIL import ImageChops

from build_woodland_shrine_path_feasibility import ORIGIN, SIZE
from build_woodland_shrine_path_junction import BOUNDS, EXTENSION, compose
from check_shrine_southern_road_path_feasibility import digest
from check_village_edge_square_connector_feasibility import ROOT, REVIEW, require
from check_woodland_shrine_path_correction import validate as validate_clipped
from check_woodland_shrine_path_feasibility import TARGETS, rgba, validate_images

EVIDENCE_HASH = 'e937ffe10acf299eac8336320f6b845a7bd14bee55aa3e1ad5a04b04100730b5'


def projected_bounds(target, zoom, height):
    """Project the local rectangle through ordinary camera clamps, allowing 2px filtering."""
    half_x, half_y = 640 / zoom, height / (2 * zoom)
    cx = min(max(target[0], half_x), 3200 - half_x)
    cy = min(max(target[1], half_y), 1800 - half_y)
    return ((BOUNDS[0] - cx) * zoom + 638, (BOUNDS[1] - cy) * zoom + height / 2 - 2,
            (BOUNDS[2] - cx) * zoom + 642, (BOUNDS[3] - cy) * zoom + height / 2 + 2)


def validate(check_live_baseline=False):
    """Prove local additions, retained opaque pixels, and coverage of the reported wedge."""
    validate_clipped(check_live_baseline)
    record_path = REVIEW / 'woodland-shrine-path-junction-evidence.json'
    require(digest(record_path) == EVIDENCE_HASH, 'junction evidence changed')
    evidence = json.loads(record_path.read_text())
    for relative, sha in evidence['inputs'].items():
        require(digest(ROOT / relative) == sha, f'junction input changed: {relative}')
    require(evidence['extension'] == EXTENSION and evidence['bounds'] == list(BOUNDS),
            'junction geometry changed')
    validate_images(evidence, 'junction')
    before = rgba(REVIEW / 'woodland-shrine-path-correction-overlay.png')
    after = rgba(REVIEW / 'woodland-shrine-path-junction-overlay.png')
    require(compose().tobytes() == after.tobytes(), 'junction is not reproducible')
    changed = 0
    for y in range(SIZE[1]):
        for x in range(SIZE[0]):
            old, new = before.getpixel((x, y)), after.getpixel((x, y))
            require(new[3] >= old[3], 'junction removed coverage')
            require(old[3] != 255 or old == new, 'junction changed an opaque original pixel')
            require(new[3] != 0 or new == (0, 0, 0, 0), 'junction introduced hidden RGB')
            if old != new:
                changed += 1
                wx, wy = ORIGIN[0] + x, ORIGIN[1] + y
                require(BOUNDS[0] <= wx < BOUNDS[2] and BOUNDS[1] <= wy < BOUNDS[3],
                        'junction changed outside local rectangle')
    require(changed == evidence['changed_overlay_pixels'] == 2241, 'junction pixel count changed')
    changed_views = 0
    for view, target in TARGETS.items():
        for zoom in (.8, 1., 2.):
            for frame, height in (('wide', 720), ('tall', 827)):
                suffix = f'{view}-{zoom:.1f}x-{frame}.png'
                old = rgba(REVIEW / f'woodland-shrine-path-correction-{suffix}')
                new = rgba(REVIEW / f'woodland-shrine-path-junction-{suffix}')
                bounds = ImageChops.difference(old, new).convert('RGB').getbbox()
                if bounds:
                    changed_views += 1
                    left, top, right, bottom = projected_bounds(target, zoom, height)
                    require(bounds[0] >= left and bounds[1] >= top and bounds[2] <= right
                            and bounds[3] <= bottom, f'capture escaped junction rectangle: {suffix}')
                if view == 'clearing-junction':
                    require(bounds is not None, f'junction absent in view: {suffix}')
    # These samples lay inside the visible greybox wedge in the reported close view.
    old = rgba(REVIEW / 'woodland-shrine-path-correction-clearing-junction-2.0x-wide.png')
    new = rgba(REVIEW / 'woodland-shrine-path-junction-clearing-junction-2.0x-wide.png')
    for point in ((640, 390), (645, 410), (650, 430)):
        require(old.getpixel(point) == (114, 100, 77, 255), 'reported gap sample moved')
        red, green, blue, alpha = new.getpixel(point)
        require(red > 180 and green > 130 and blue < 120 and alpha == 255,
                'reported gap still exposes greybox road')
    print(f'Woodland–Shrine junction valid: {changed} local pixels; {changed_views}/18 bounded views; gap covered.')


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--check-live-baseline', action='store_true')
    validate(parser.parse_args().check_live_baseline)
