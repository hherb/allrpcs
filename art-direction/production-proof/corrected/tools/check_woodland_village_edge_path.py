#!/usr/bin/env python3
"""Validate both external Woodland–Village Edge stages and independently bounded clipping."""

import argparse
import json
import math

from PIL import ImageChops

from build_woodland_village_edge_path_feasibility import END, ORIGIN, SIZE, START, Z_INDEX, compose
from build_woodland_village_edge_path_correction import (
    EDGE_LEFT, MARKER_HALF_DEPTH, OPENING_CENTER_Y, OPENING_HALF_HEIGHT, compose as compose_corrected,
)
from check_shrine_southern_road_path_correction import renderer_digest
from check_shrine_southern_road_path_feasibility import digest
from check_village_edge_square_connector_feasibility import ROOT, REVIEW, require
from check_woodland_shrine_path_feasibility import rgba

FEASIBILITY_HASH = 'a0165397bf3bb453f396ca8036beba5b2f6b8d5b4926602c4fa790facac4c770'
CORRECTION_HASH = '5e70a65b30f386737636dad410c3e87784e4e15df8bf89336c9165a8e7d74a68'
AUTHORITY_HASH = '751256ec133c8703c699bbbec5bc402643044101f0e0464172e7c603cc6eda94'
TARGETS = {'midpoint': (1350, 625), 'clearing-junction': END, 'village-entry': (1580, 805.714286)}


def image_names(kind):
    """Require the exact 18-view matrix and one overlay for each immutable stage."""
    names = {f'woodland-village-edge-path-{kind}-{view}-{zoom}x-{frame}.png'
             for view in TARGETS for zoom in ('0.8', '1.0', '2.0') for frame in ('wide', 'tall')}
    names.add(f'woodland-village-edge-path-{kind}-overlay.png')
    return names


def validate_record(kind, sha):
    """Pin record, input and image bytes independently of a future promotion baseline."""
    path = REVIEW / f'woodland-village-edge-path-{kind}-evidence.json'
    require(digest(path) == sha, f'{kind} evidence changed')
    record = json.loads(path.read_text())
    for relative, expected in record['inputs'].items():
        require(digest(ROOT / relative) == expected, f'{kind} input changed: {relative}')
    require(set(record['images']) == image_names(kind), f'{kind} image set changed')
    for name, expected in record['images'].items():
        path = REVIEW / name
        require(digest(path) == expected, f'archived image changed: {name}')
        image = rgba(path)
        if name.endswith('overlay.png'):
            require(image.size == SIZE, 'overlay canvas changed')
        else:
            require(image.size == (1280, 827 if name.endswith('tall.png') else 720)
                    and image.getchannel('A').getextrema() == (255, 255), f'invalid capture: {name}')
    return record


def projected_bounds(target, zoom, height):
    """Project only the clipped world region through ordinary camera limits plus 2px filtering."""
    half_x, half_y = 640 / zoom, height / (2 * zoom)
    cx = min(max(target[0], half_x), 3200 - half_x)
    cy = min(max(target[1], half_y), 1800 - half_y)
    return ((EDGE_LEFT - MARKER_HALF_DEPTH - cx) * zoom + 638,
            (ORIGIN[1] - cy) * zoom + height / 2 - 2,
            (ORIGIN[0] + SIZE[0] - cx) * zoom + 642,
            (ORIGIN[1] + SIZE[1] - cy) * zoom + height / 2 + 2)


def validate(check_live_baseline=False):
    """Reproduce unchanged-source sampling, measure port fringe, and prove pixel-only clipping."""
    first = validate_record('feasibility', FEASIBILITY_HASH)
    second = validate_record('correction', CORRECTION_HASH)
    authority_path = REVIEW / 'woodland-village-edge-path-authority-evidence.json'
    require(digest(authority_path) == AUTHORITY_HASH, 'supplemental port authority record changed')
    authority = json.loads(authority_path.read_text())['retained_port_owners']
    require(set(authority) == {
        'game/OpeningRegionVillageTransitionGeometry.cs',
        'game/OpeningRegionVillageTransitionSceneSource.cs',
        'src/Application/Navigation/OpeningRegionVillageTransitionLayout.cs',
    }, 'supplemental port owner set changed')
    require(renderer_digest() == first['renderer_sha256'], 'shared renderer changed')
    require(first['geometry'] == {
        'route_start': list(START), 'route_end': list(END), 'origin': list(ORIGIN), 'size': list(SIZE),
        'z_index': Z_INDEX, 'width': 96, 'clearing_opening': [1128, 450],
        'village_opening': [1580, OPENING_CENTER_Y], 'opening_width': 96, 'marker_half_depth': 8,
    }, 'route/opening geometry changed')
    require(second['clipping'] == {
        'edge_left': EDGE_LEFT, 'opening_center_y': OPENING_CENTER_Y,
        'opening_half_height': OPENING_HALF_HEIGHT, 'marker_half_depth': MARKER_HALF_DEPTH,
    }, 'clip geometry changed')
    crossing = START[1] + (EDGE_LEFT - START[0]) * (END[1] - START[1]) / (END[0] - START[0])
    require(math.isclose(crossing, OPENING_CENTER_Y, abs_tol=1e-10), 'Village opening alignment changed')
    original = rgba(REVIEW / 'woodland-village-edge-path-feasibility-overlay.png')
    corrected = rgba(REVIEW / 'woodland-village-edge-path-correction-overlay.png')
    require(compose().tobytes() == original.tobytes() and compose_corrected().tobytes() == corrected.tobytes(),
            'external overlays are not reproducible')
    samples = dict(village_marker_band=0, clearing_marker_band=0, village_interior=0)
    removed = 0
    for y in range(SIZE[1]):
        for x in range(SIZE[0]):
            before, after = original.getpixel((x, y)), corrected.getpixel((x, y))
            wx, wy = ORIGIN[0] + x + .5, ORIGIN[1] + y + .5
            clipped = (wx >= EDGE_LEFT or (wx >= EDGE_LEFT - MARKER_HALF_DEPTH
                       and abs(wy - OPENING_CENTER_Y) > OPENING_HALF_HEIGHT))
            require(after == ((0, 0, 0, 0) if clipped else before), 'unclipped sampled pixel changed')
            require(before[3] != 0 or before == (0, 0, 0, 0), 'hidden RGB introduced')
            if x in (0, SIZE[0] - 1) or y in (0, SIZE[1] - 1):
                require(before[3] == 0 and after[3] == 0, 'overlay gutter clips visible pixels')
            removed += bool(clipped and before[3])
            if before[3]:
                samples['village_marker_band'] += 1572 <= wx < 1588 and abs(wy - OPENING_CENTER_Y) > 48
                samples['clearing_marker_band'] += 442 <= wy < 458 and abs(wx - 1128) > 48
                samples['village_interior'] += wx >= EDGE_LEFT
    require(samples == first['visible_samples'] == {
        'village_marker_band': 205, 'clearing_marker_band': 651, 'village_interior': 12859,
    }, 'opening fringe measurements changed')
    require(removed == second['removed_visible_pixels'], 'clip removal count changed')
    changed_views = 0
    for view, target in TARGETS.items():
        for zoom in (.8, 1., 2.):
            for frame, height in (('wide', 720), ('tall', 827)):
                suffix = f'{view}-{zoom:.1f}x-{frame}.png'
                before = rgba(REVIEW / f'woodland-village-edge-path-feasibility-{suffix}')
                after = rgba(REVIEW / f'woodland-village-edge-path-correction-{suffix}')
                bounds = ImageChops.difference(before, after).convert('RGB').getbbox()
                if bounds:
                    changed_views += 1
                    left, top, right, bottom = projected_bounds(target, zoom, height)
                    require(bounds[0] >= left and bounds[1] >= top and bounds[2] <= right
                            and bounds[3] <= bottom, f'capture escaped clip bounds: {suffix}')
                if view == 'village-entry':
                    require(bounds is not None, f'Village clip not visible: {suffix}')
                if view == 'clearing-junction' and zoom == 2.:
                    require(bounds is None, f'Clearing close view changed: {suffix}')
    if check_live_baseline:
        require(len(first['baseline']) == 438, 'frozen post-promotion file set changed')
        for relative, sha in first['baseline'].items():
            require(digest(ROOT / relative) == sha, f'live authority changed: {relative}')
        for relative, sha in authority.items():
            require(digest(ROOT / relative) == sha, f'Village port authority changed: {relative}')
        print('Frozen 438-file baseline and three supplemental Village port owners unchanged.')
    print(f'Woodland–Village Edge proof valid: 205 marker fringe samples; {removed} pixels clipped; '
          f'{changed_views}/18 independently bounded views changed.')


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--check-live-baseline', action='store_true')
    validate(parser.parse_args().check_live_baseline)
