#!/usr/bin/env python3
"""Validate external Stream–Laundry evidence without granting runtime authority."""

import argparse
import json
import math

from PIL import Image, ImageChops

from build_stream_laundry_path_feasibility import START, END, ORIGIN, SIZE, Z_INDEX, compose
from build_stream_laundry_path_correction import (
    LAUNDRY_RIGHT, OPENING_CENTER_Y, OPENING_HALF_HEIGHT, MARKER_HALF_DEPTH,
    compose as compose_corrected,
)
from check_shrine_southern_road_path_correction import renderer_digest
from check_shrine_southern_road_path_feasibility import digest
from check_village_edge_square_connector_feasibility import ROOT, REVIEW, require

FEASIBILITY_HASH = '590151aec86dad9f02028003b53258f7cf3df01906e39774ab68150ec643a529'
CORRECTION_HASH = '2eb2863428a4e5409fd03b752d60e7fedef8de104fe039d27c3a272db00bbb9f'
TARGETS = ('midpoint', 'stream-entry', 'laundry-entry')


def rgba(path):
    """Decode one immutable image, releasing its file handle before returning."""
    with Image.open(path) as image:
        require(image.mode == 'RGBA', f'non-RGBA evidence: {path}')
        return image.copy()


def validate_record(kind, sha):
    """Pin recipe, harness and the exact 18-view plus overlay archive."""
    record_path = REVIEW / f'stream-laundry-path-{kind}-evidence.json'
    require(digest(record_path) == sha, f'{kind} record changed')
    record = json.loads(record_path.read_text())
    expected_inputs = {
        f'art-direction/production-proof/corrected/tools/build_stream_laundry_path_{kind}.py',
        f'scripts/capture-stream-laundry-path-{kind}.gd',
    }
    if kind == 'feasibility':
        expected_inputs.add('game/art/runtime/terrain/straight-path.png')
    require(set(record['inputs']) == expected_inputs, f'{kind} input set changed')
    for relative, expected in record['inputs'].items():
        require(digest(ROOT / relative) == expected, f'{kind} input changed: {relative}')
    names = {f'stream-laundry-path-{kind}-{view}-{zoom}x-{frame}.png'
             for view in TARGETS for zoom in ('0.8', '1.0', '2.0') for frame in ('wide', 'tall')}
    names.add(f'stream-laundry-path-{kind}-overlay.png')
    require(set(record['images']) == names, f'{kind} image matrix changed')
    for name, expected in record['images'].items():
        path = REVIEW / name
        require(digest(path) == expected, f'archived image changed: {name}')
        image = rgba(path)
        if name.endswith('overlay.png'):
            require(image.size == SIZE, 'overlay dimensions changed')
        else:
            require(image.size == (1280, 827 if name.endswith('tall.png') else 720)
                    and image.getchannel('A').getextrema() == (255, 255), f'invalid capture: {name}')
    return record


def validate(check_live_baseline=False):
    """Reproduce finite sampling, measure Laundry fringe and verify the exact local clip."""
    first = validate_record('feasibility', FEASIBILITY_HASH)
    second = validate_record('correction', CORRECTION_HASH)
    require(renderer_digest() == first['renderer_sha256'], 'shared renderer changed')
    require(first['geometry'] == {
        'route_start': list(START), 'route_end': list(END), 'origin': list(ORIGIN), 'size': list(SIZE),
        'z_index': Z_INDEX, 'width': 96, 'laundry_opening': [1580, 294.6666666666667],
        'opening_width': 96, 'marker_half_depth': 8, 'stream_bounds': [1736, 452, 1864, 548],
        'bridge_north_approach': [1748.571429, 452],
    }, 'route, bridge or Laundry opening geometry changed')
    crossing = START[1] + (LAUNDRY_RIGHT - START[0]) * (END[1] - START[1]) / (END[0] - START[0])
    require(math.isclose(crossing, OPENING_CENTER_Y, abs_tol=1e-10), 'Laundry port alignment changed')
    require(second['clipping'] == {
        'laundry_right': LAUNDRY_RIGHT, 'opening_center_y': OPENING_CENTER_Y,
        'opening_half_height': OPENING_HALF_HEIGHT, 'marker_half_depth': MARKER_HALF_DEPTH,
    }, 'clip geometry changed')
    before = rgba(REVIEW / 'stream-laundry-path-feasibility-overlay.png')
    after = rgba(REVIEW / 'stream-laundry-path-correction-overlay.png')
    require(before.tobytes() == compose().tobytes() and after.tobytes() == compose_corrected().tobytes(),
            'overlays are not reproducible')
    samples = dict(laundry_marker_band=0, laundry_interior=0, stream_overlap=0)
    removed = 0
    bridge_inside = bridge_outside = 0
    for y in range(SIZE[1]):
        for x in range(SIZE[0]):
            a, b = before.getpixel((x, y)), after.getpixel((x, y))
            wx, wy = ORIGIN[0] + x + .5, ORIGIN[1] + y + .5
            clipped = wx <= LAUNDRY_RIGHT or (wx <= LAUNDRY_RIGHT + MARKER_HALF_DEPTH
                      and abs(wy - OPENING_CENTER_Y) > OPENING_HALF_HEIGHT)
            require(b == ((0, 0, 0, 0) if clipped else a), 'surviving sampled pixel changed')
            require(a[3] != 0 or a == (0, 0, 0, 0), 'hidden RGB introduced')
            if x in (0, SIZE[0] - 1) or y in (0, SIZE[1] - 1):
                require(a[3] == b[3] == 0, 'canvas gutter clips visible pixels')
            removed += bool(clipped and a[3])
            if a[3]:
                samples['laundry_marker_band'] += 1572 <= wx < 1588 and abs(wy - OPENING_CENTER_Y) > 48
                samples['laundry_interior'] += wx <= 1580
                samples['stream_overlap'] += wy >= 452
                if wy >= 452:
                    bridge_inside += 1736 <= wx < 1864
                    bridge_outside += not 1736 <= wx < 1864
                    require(a == b, 'Stream overlap changed')
    require(samples == first['visible_samples'] == {
        'laundry_marker_band': 296, 'laundry_interior': 9538, 'stream_overlap': 6029,
    }, 'source-sample measurements changed')
    require((bridge_inside, bridge_outside) == (4843, 1186), 'bridge overlap split changed')
    require(removed == second['removed_visible_pixels'] == 9680, 'clip removal count changed')
    changed = 0
    targets = {'midpoint': (1650, 360), 'stream-entry': (1748.571429, 452),
               'laundry-entry': (1580, 294.666667)}
    for view, target in targets.items():
        for zoom in (.8, 1., 2.):
            for frame, height in (('wide', 720), ('tall', 827)):
                suffix = f'{view}-{zoom:.1f}x-{frame}.png'
                before_view = rgba(REVIEW / f'stream-laundry-path-feasibility-{suffix}')
                after_view = rgba(REVIEW / f'stream-laundry-path-correction-{suffix}')
                bounds = ImageChops.difference(before_view, after_view).convert('RGB').getbbox()
                if bounds:
                    changed += 1
                    half_x, half_y = 640 / zoom, height / (2 * zoom)
                    cx = min(max(target[0], half_x), 3200 - half_x)
                    cy = min(max(target[1], half_y), 1800 - half_y)
                    expected = ((ORIGIN[0] - cx) * zoom + 638,
                                (ORIGIN[1] - cy) * zoom + height / 2 - 2,
                                (LAUNDRY_RIGHT + MARKER_HALF_DEPTH - cx) * zoom + 642,
                                (ORIGIN[1] + SIZE[1] - cy) * zoom + height / 2 + 2)
                    require(bounds[0] >= expected[0] and bounds[1] >= expected[1]
                            and bounds[2] <= expected[2] and bounds[3] <= expected[3],
                            f'capture escaped projected Laundry clip: {suffix}')
    require(changed == second['changed_views'] == 18, 'changed view count differs')
    if check_live_baseline:
        require(len(first['baseline']) == 450, 'frozen baseline file set changed')
        for relative, sha in first['baseline'].items():
            require(digest(ROOT / relative) == sha, f'live authority changed: {relative}')
        print('Frozen 450-file runtime/production/physical baseline unchanged.')
    print('Stream–Laundry proof valid: 9680 Laundry samples clipped; bridge underlay retained; 18 bounded views changed.')


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--check-live-baseline', action='store_true')
    validate(parser.parse_args().check_live_baseline)
