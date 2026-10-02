#!/usr/bin/env python3
"""Validate external Village Edge–Stream evidence without granting runtime authority."""

import argparse
import json
import math

from PIL import Image

from build_village_edge_stream_path_feasibility import START, END, ORIGIN, SIZE, Z_INDEX, compose
from build_village_edge_stream_path_correction import (
    VILLAGE_TOP, OPENING_CENTER_X, OPENING_HALF_WIDTH, MARKER_HALF_DEPTH,
    compose as compose_corrected,
)
from check_shrine_southern_road_path_correction import renderer_digest
from check_shrine_southern_road_path_feasibility import digest
from check_village_edge_square_connector_feasibility import ROOT, REVIEW, require

FEASIBILITY_HASH = '322d1fed25e2f4af7b9159c62c8fd835a3f43f95dbe68ef0786ba37ac8aa2e5e'
CORRECTION_HASH = 'fa50f3bac4a80dcbdabb3fc4f2965530565e834200d26ea8c90e4145002ac027'
TARGETS = ('midpoint', 'stream-entry', 'village-entry')


def rgba(path):
    """Decode one immutable image, releasing its file handle before returning."""
    with Image.open(path) as image:
        require(image.mode == 'RGBA', f'non-RGBA evidence: {path}')
        return image.copy()


def validate_record(kind, sha):
    """Pin recipe, harness and the exact 18-view plus overlay archive."""
    record_path = REVIEW / f'village-edge-stream-path-{kind}-evidence.json'
    require(digest(record_path) == sha, f'{kind} record changed')
    record = json.loads(record_path.read_text())
    expected_inputs = {
        f'art-direction/production-proof/corrected/tools/build_village_edge_stream_path_{kind}.py',
        f'scripts/capture-village-edge-stream-path-{kind}.gd',
    }
    if kind == 'feasibility':
        expected_inputs.add('game/art/runtime/terrain/straight-path.png')
    require(set(record['inputs']) == expected_inputs, f'{kind} input set changed')
    for relative, expected in record['inputs'].items():
        require(digest(ROOT / relative) == expected, f'{kind} input changed: {relative}')
    names = {f'village-edge-stream-path-{kind}-{view}-{zoom}x-{frame}.png'
             for view in TARGETS for zoom in ('0.8', '1.0', '2.0') for frame in ('wide', 'tall')}
    names.add(f'village-edge-stream-path-{kind}-overlay.png')
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
    """Reproduce finite sampling, measure hidden shell fringe and verify the exact local clip."""
    first = validate_record('feasibility', FEASIBILITY_HASH)
    second = validate_record('correction', CORRECTION_HASH)
    require(renderer_digest() == first['renderer_sha256'], 'shared renderer changed')
    require(first['geometry'] == {
        'route_start': list(START), 'route_end': list(END), 'origin': list(ORIGIN), 'size': list(SIZE),
        'z_index': Z_INDEX, 'width': 96, 'village_opening': [1750, 700], 'opening_width': 96,
        'marker_half_depth': 8, 'stream_bounds': [1736, 452, 1864, 548],
    }, 'route, bridge or Village opening geometry changed')
    crossing = START[0] + (VILLAGE_TOP - START[1]) * (END[0] - START[0]) / (END[1] - START[1])
    require(crossing == OPENING_CENTER_X, 'Village opening alignment changed')
    # The 96px normal-width strip has a wider horizontal cross-section; retain this diagnostic.
    cross_section = 96 * math.hypot(100, 400) / 400
    require(math.isclose(cross_section, 98.95453501482385, abs_tol=1e-10), 'heading-width diagnostic changed')
    require(second['clipping'] == {
        'village_top': VILLAGE_TOP, 'opening_center_x': OPENING_CENTER_X,
        'opening_half_width': OPENING_HALF_WIDTH, 'marker_half_depth': MARKER_HALF_DEPTH,
    }, 'clip geometry changed')
    before = rgba(REVIEW / 'village-edge-stream-path-feasibility-overlay.png')
    after = rgba(REVIEW / 'village-edge-stream-path-correction-overlay.png')
    require(before.tobytes() == compose().tobytes() and after.tobytes() == compose_corrected().tobytes(),
            'overlays are not reproducible')
    samples = dict(village_marker_band=0, village_interior=0, stream_overlap=0)
    removed = 0
    for y in range(SIZE[1]):
        for x in range(SIZE[0]):
            a, b = before.getpixel((x, y)), after.getpixel((x, y))
            wx, wy = ORIGIN[0] + x + .5, ORIGIN[1] + y + .5
            clipped = wy >= VILLAGE_TOP or (wy >= VILLAGE_TOP - MARKER_HALF_DEPTH
                      and abs(wx - OPENING_CENTER_X) > OPENING_HALF_WIDTH)
            require(b == ((0, 0, 0, 0) if clipped else a), 'surviving sampled pixel changed')
            require(a[3] != 0 or a == (0, 0, 0, 0), 'hidden RGB introduced')
            if x in (0, SIZE[0] - 1) or y in (0, SIZE[1] - 1):
                require(a[3] == b[3] == 0, 'canvas gutter clips visible pixels')
            removed += bool(clipped and a[3])
            if a[3]:
                samples['village_marker_band'] += 692 <= wy < 708 and abs(wx - 1750) > 48
                samples['village_interior'] += wy >= 700
                samples['stream_overlap'] += wy < 548
                if wy < 548:
                    require(a == b, 'bridge underlay changed')
    require(samples == first['visible_samples'] == {
        'village_marker_band': 1, 'village_interior': 17406, 'stream_overlap': 4477,
    }, 'source-sample measurements changed')
    require(removed == second['removed_visible_pixels'] == 17406, 'clip removal count changed')
    # The single fringe sample is inside the opaque floor. The clip changes source support,
    # but intentionally changes none of the 18 rendered views at the tested camera frames.
    require(second['changed_views'] == 0, 'masked-clip expectation changed')
    for view in TARGETS:
        for zoom in ('0.8', '1.0', '2.0'):
            for frame in ('wide', 'tall'):
                suffix = f'{view}-{zoom}x-{frame}.png'
                require(digest(REVIEW / f'village-edge-stream-path-feasibility-{suffix}')
                        == digest(REVIEW / f'village-edge-stream-path-correction-{suffix}'),
                        f'opaque-floor clip changed rendered view: {suffix}')
    if check_live_baseline:
        require(len(first['baseline']) == 451, 'frozen baseline file set changed')
        for relative, sha in first['baseline'].items():
            require(digest(ROOT / relative) == sha, f'live authority changed: {relative}')
        print('Frozen 451-file runtime/production/physical baseline unchanged.')
    print('Village Edge–Stream proof valid: 17406 shell samples clipped; bridge underlay retained; 18 views exact.')


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--check-live-baseline', action='store_true')
    validate(parser.parse_args().check_live_baseline)
