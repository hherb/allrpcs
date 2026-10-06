#!/usr/bin/env python3
"""Independently verify frozen product bytes, placement clearance and actual GPU evidence."""

import argparse
import copy
import hashlib
import json
import math
from pathlib import Path
import subprocess

import numpy as np
from PIL import Image

import boards
import build
import readability
from build import ART, PREVIOUS, REPO, ROOT, VARIANTS, raster, read, require, sha

DOCUMENTS = {'HANDOVER.md', 'README.md', 'ROADMAP.md'}
PREFIX = 'art-direction/woodland-clearing-dressing/'


def check_baseline():
    """Compare current bytes to the published Git objects, allowing only maintained docs."""
    baseline = read(ROOT / 'baseline.json')
    tree = subprocess.check_output(['git', 'ls-tree', '-r', baseline['head']], cwd=REPO, text=True)
    blobs = {}
    for row in tree.splitlines():
        metadata, name = row.split('\t', 1)
        mode, kind, blob = metadata.split()
        require(kind == 'blob' and mode in ('100644', '100755'), 'Unsupported predecessor entry')
        blobs[name] = blob
    require(set(blobs) == set(baseline['files']), 'Incomplete published inventory')
    for name, digest in baseline['files'].items():
        data = subprocess.check_output(['git', 'show', baseline['head'] + ':' + name], cwd=REPO) \
            if name in DOCUMENTS else (REPO / name).read_bytes()
        require(hashlib.sha256(data).hexdigest() == digest, 'Predecessor SHA-256 drift: ' + name)
        blob = hashlib.sha1(b'blob ' + str(len(data)).encode() + b'\0' + data).hexdigest()
        require(blob == blobs[name], 'Predecessor differs from committed Git blob: ' + name)
    changed = subprocess.check_output(['git', 'diff', baseline['head'], '--name-only'], cwd=REPO, text=True).splitlines()
    untracked = subprocess.check_output(['git', 'ls-files', '--others', '--exclude-standard'], cwd=REPO, text=True).splitlines()
    require(all(name in DOCUMENTS or name.startswith(PREFIX) for name in changed + untracked),
            'Change outside bounded external scope')
    require(not any(name.endswith('.DS_Store') for name in changed + untracked), 'Finder metadata in scope')
    return len(blobs) - len(DOCUMENTS)


def point_segment(point, a, b):
    delta = b - a
    t = np.clip(np.dot(point - a, delta) / np.dot(delta, delta), 0, 1)
    return float(np.linalg.norm(point - a - t * delta))


def point_rectangle(point, rect):
    return float(np.linalg.norm(point - np.clip(point, rect[:2], rect[2:])))


def segment_rectangle(a, b, rect):
    """Continuous capsule-to-rectangle clearance, independent of the sampled author mask."""
    a, b, rect = np.array(a, dtype=float), np.array(b, dtype=float), np.array(rect, dtype=float)
    # Slab intersection tests the segment interior, including axis-aligned segments.
    low, high = 0., 1.
    for axis in (0, 1):
        delta = b[axis] - a[axis]
        if delta == 0:
            if not rect[axis] <= a[axis] <= rect[axis + 2]:
                low, high = 1., 0.
                break
        else:
            limits = sorted(((rect[axis] - a[axis]) / delta, (rect[axis + 2] - a[axis]) / delta))
            low, high = max(low, limits[0]), min(high, limits[1])
    if low <= high:
        return 0.
    corners = [(rect[x], rect[y]) for x in (0, 2) for y in (1, 3)]
    return min(point_rectangle(a, rect), point_rectangle(b, rect),
               *(point_segment(np.array(corner), a, b) for corner in corners))


def overlap(a, b):
    return max(a[0], b[0]) < min(a[2], b[2]) and max(a[1], b[1]) < min(a[3], b[3])


def check_geometry(inventory, evidence):
    layout = read(REPO / 'art-direction/world-layout-proposal/layout-proposal.json')
    anchors = {a['id']: (a['x'], a['y']) for a in layout['anchors']}
    minimum = math.inf
    for variant in VARIANTS[1:]:
        for row in inventory['variants'][variant]:
            x, y = row['origin']
            w, h = row['size']
            rect = (x - 2, y - 2, x + w + 2, y + h + 2)
            for connection in layout['connections']:
                distance = segment_rectangle(anchors[connection['first']], anchors[connection['second']], rect)
                minimum = min(minimum, distance)
                require(distance > 48, 'Native rectangle enters 96px route: ' + row['instance'])
            for name, owner in evidence['owners'].items():
                if name.startswith('OpeningRegionWoodlandClearingSceneSource/') and name.endswith('Opening'):
                    points = np.array(owner['polygon'])
                    bounds = (*points.min(0), *points.max(0))
                    require(not overlap(rect, bounds), 'Artwork enters retained entrance')
                if owner['class'] == 'Label' and owner['visible'] and owner.get('text'):
                    left, top, width, height = owner['rect']
                    require(not overlap(rect, (left, top, left + width, top + height)),
                            'Artwork obscures retained label: ' + name)
            require(point_rectangle(np.array([1050, 350]), np.array(rect)) > 48, 'Specimen footprint obscured')
    return minimum - 48


def check_frames(evidence):
    require(set(evidence['captures']) == set(evidence['frames']) == boards.expected_frames(), 'Incomplete matrix')
    for name in evidence['captures']:
        require(sha(ROOT / name) == evidence['captures'][name], 'Capture hash drift')
        frame = evidence['frames'][name]
        zoom = float(name.split('-')[-2].removesuffix('x'))
        kind = name.split('-')[-1].removesuffix('.png')
        viewport = [int(800 * zoom), int(480 * zoom)] if kind == 'placement' else [1280, 720 if kind == 'wide' else 827]
        require(frame['zoom'] == zoom and frame['viewport'] == viewport, 'Camera/viewport drift')
        require(list(raster(ROOT / name).size) == viewport, 'PNG dimension drift')
        target = [924, 240] if kind == 'placement' else [924, 380]
        center = [target[0], max(target[1], viewport[1] / (zoom * 2))]
        transform = [zoom, 0, 0, zoom, viewport[0] / 2 - center[0] * zoom, viewport[1] / 2 - center[1] * zoom]
        require(frame['target_center'] == target and frame['screen_center'] == center
                and np.allclose(frame['world_to_canvas'], transform, atol=1e-4, rtol=0), 'Recorded camera transform drift')


def check_difference(baseline, candidate, transform, rows):
    """Permit GPU changes only inside transformed approved placement rectangles."""
    require(baseline.shape == candidate.shape, 'GPU difference size drift')
    allowed = np.zeros(baseline.shape[:2], dtype=bool)
    a, b, c, d, tx, ty = transform
    require(a == d and a > 0 and b == c == 0, 'Invalid difference transform')
    for row in rows:
        x, y = row['origin']
        width, height = row['size']
        left, top = math.floor((x - 2) * a + tx), math.floor((y - 2) * d + ty)
        right, bottom = math.ceil((x + width + 2) * a + tx), math.ceil((y + height + 2) * d + ty)
        left, right = np.clip([left, right], 0, allowed.shape[1])
        top, bottom = np.clip([top, bottom], 0, allowed.shape[0])
        allowed[top:bottom, left:right] = True
    difference = np.any(baseline != candidate, axis=2)
    require(not (difference & ~allowed).any(), 'GPU changes outside approved placement footprint')
    require(difference.sum() > 100, 'Candidate missing from actual scene')
    return int(difference.sum())


def check_capture(inventory, evidence):
    require(evidence['published_head'] == inventory['published_head'] == read(ROOT / 'baseline.json')['head'],
            'Capture not bound to published head')
    for path, digest in evidence['inputs'].items():
        require(sha(Path(path)) == digest, 'Capture input drift: ' + path)
    expected_inputs = {str(REPO / path): digest for path, digest in inventory['sources'].items()}
    expected_inputs[str(ROOT / 'inventory.json')] = sha(ROOT / 'inventory.json')
    require(evidence['inputs'] == expected_inputs, 'Incomplete capture input bindings')
    previous = read(PREVIOUS / 'capture-evidence.json')['owners']
    for name, data in previous.items():
        require(name in evidence['owners'] and all(evidence['owners'][name].get(key) == value
                                                 for key, value in data.items()), 'Retained owner drift: ' + name)
    specimen = evidence['owners']['OpeningRegionWoodlandClearingSceneSource/ConcealedRareSpecimen']
    require(not specimen['visible'] and specimen['world_transform'] == [1, 0, 0, 1, 1050, 350], 'Concealed state changed')
    require(set(evidence['placements']) == set(VARIANTS), 'Placement evidence variant drift')
    for variant in VARIANTS:
        rows = inventory['variants'][variant]
        recorded = evidence['placements'][variant]
        require(len(rows) == len(recorded), 'Missing placement')
        for row, actual in zip(rows, recorded):
            expected = {k: row[k] for k in ('instance', 'origin', 'size', 'world_pivot', 'proposed_pivot', 'sha256')}
            expected.update(z_index=-60, centered=False, offset=[0, 0], scale=[1, 1], rotation=0,
                            filter=2, repeat=1, script_free=True, child_count=0)
            require(actual == expected, 'Non-native or authority-bearing candidate: ' + row['instance'])
    check_frames(evidence)
    differences = {}
    for zoom in (.8, 1, 2):
        kinds = ('wide', 'tall', 'placement') if zoom in (1, 2) else ('wide', 'tall')
        for kind in kinds:
            base = f'captures/baseline-{zoom:.1f}x-{kind}.png'
            baseline = np.asarray(raster(ROOT / base))
            for variant in VARIANTS[1:]:
                name = f'captures/{variant}-{zoom:.1f}x-{kind}.png'
                require(evidence['frames'][name] == evidence['frames'][base], 'Variant camera mismatch')
                differences[name] = check_difference(baseline, np.asarray(raster(ROOT / name)),
                    evidence['frames'][name]['world_to_canvas'], inventory['variants'][variant])
    log = (ROOT / 'capture-host.log').read_text()
    require('PASS: complete 24-view Woodland Clearing external dressing comparison' in log
            and log.count('Woodland dressing external preview: captures/') == 24
            and not any(marker in log for marker in ('SCRIPT ERROR', 'ERROR:', 'Parse Error')), 'Capture log is not a pass')
    return differences


def check_crops(evidence):
    for scale in (1, 2):
        for variant in VARIANTS:
            name = f'captures/{variant}-{scale:.1f}x-placement.png'
            image = raster(ROOT / name)
            require(raster(ROOT / f'review/{variant}-detail-{scale}x.png').tobytes() == image.tobytes(),
                    'Complete native envelope pixels drift')
            for cluster, bounds in boards.CLUSTERS.items():
                # Dedicated views map world (524,0) exactly to pixel (0,0).
                expected = image.crop(((bounds[0] - 524) * scale, bounds[1] * scale,
                                       (bounds[2] - 524) * scale, bounds[3] * scale))
                require(expected.tobytes() == raster(ROOT / f'review/{variant}-{cluster}-{scale}x.png').tobytes(),
                        'Native cluster pixels drift')
    for variant in VARIANTS[1:]:
        for row in read(ROOT / 'inventory.json')['variants'][variant]:
            x, y = row['origin']
            w, h = row['size']
            require(any(bounds[0] <= x and bounds[1] <= y and x + w <= bounds[2] and y + h <= bounds[3]
                        for bounds in boards.CLUSTERS.values()), 'Placement omitted from native cluster review')


def check_inherited_quality():
    root = REPO / 'art-direction/village-edge-ground-runtime-promotion'
    quality = read(root / 'verification.json')
    require(quality['test_total'] == sum(quality['test_suites'].values()) == 5033
            and quality['build_errors'] == quality['build_warnings'] == 0
            and quality['quality']['host_retry_exit'] == 0, 'Inherited quality evidence changed')
    for relative, digest in quality['evidence_sha256'].items():
        require(sha(root / relative) == digest, 'Inherited quality log drift')
    acceptance = read(root / 'owner-runtime-acceptance.json')
    require(acceptance['status'] == 'accepted exact nine actual runtime views; reviewed direct-main publication authorized',
            'Terminal acceptance changed')


def probes(inventory, evidence):
    passed = []

    def reject(name, action):
        try:
            action()
        except ValueError:
            passed.append(name)
        else:
            raise ValueError('Review probe failed to reject: ' + name)

    bad = copy.deepcopy(evidence)
    del bad['captures']['captures/grouped-0.8x-wide.png']
    reject('missing GPU frame', lambda: check_frames(bad))
    bad = copy.deepcopy(evidence)
    bad['frames']['captures/grouped-1.0x-wide.png']['viewport'][1] += 1
    reject('altered viewport', lambda: check_frames(bad))
    bad = copy.deepcopy(evidence)
    bad['frames']['captures/grouped-1.0x-wide.png']['world_to_canvas'][4] += .5
    reject('fractional camera transform', lambda: check_frames(bad))
    spec = read(ROOT / 'placements.json')
    spec['variants']['sparse'][0]['world_pivot'][0] += .5
    reject('fractional proposed pivot', lambda: build.placement_rows(spec))
    bad = copy.deepcopy(inventory['variants'])
    bad['sparse'][0]['origin'] = [950, 320]
    reject('route obstruction', lambda: build.check_placements(bad, build.clearance()))
    name = 'captures/sparse-1.0x-placement.png'
    baseline = np.asarray(raster(ROOT / 'captures/baseline-1.0x-placement.png'))
    candidate = np.asarray(raster(ROOT / name)).copy()
    candidate[0, 0, :3] = 255 - candidate[0, 0, :3]
    transform = evidence['frames'][name]['world_to_canvas']
    reject('GPU change outside footprint', lambda: check_difference(baseline, candidate, transform, inventory['variants']['sparse']))
    reject('missing visible dressing', lambda: check_difference(baseline, baseline, transform, inventory['variants']['sparse']))
    model = read(PREVIOUS / 'simulation-model.json')
    probe = Image.fromarray(np.array([[[0, 0, 0, 255], [255, 255, 255, 255], [255, 0, 0, 255]]], dtype='uint8'))
    require(np.array_equal(np.asarray(readability.simulate(probe, 'normal', model)), np.asarray(probe)), 'Normal model drift')
    expected = np.array([[[0, 0, 0, 255], [255, 255, 255, 255], [127, 127, 127, 255]]], dtype='uint8')
    require(np.array_equal(np.asarray(readability.simulate(probe, 'luminance', model)), expected), 'Luminance vector drift')
    require(segment_rectangle((0, 0), (10, 0), (2, -1, 8, 1)) == 0
            and segment_rectangle((0, 0), (10, 0), (2, 3, 8, 6)) == 3, 'Continuous geometry vector drift')
    passed.append('independent luminance/geometry vectors')
    return passed


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--record', action='store_true', help='Create immutable verification.json after all checks')
    args = parser.parse_args()
    predecessor_count = check_baseline()
    inventory, evidence = read(ROOT / 'inventory.json'), read(ROOT / 'capture-evidence.json')
    require({variant: len(rows) for variant, rows in inventory['variants'].items()}
            == {'baseline': 0, 'sparse': 8, 'grouped': 16}, 'Incomplete dressing comparison')
    approved = read(ART / 'owner-acceptance.json')
    for name, digest in approved['approved_files'].items():
        require(sha(ART / name) == digest, 'Approved artwork drift')
    build.write_or_check(build.generate(), True)
    differences = check_capture(inventory, evidence)
    margin = check_geometry(inventory, evidence)
    boards.write_or_check(boards.generate(), True)
    check_crops(evidence)
    build.write_or_check(readability.generate(), True)
    check_inherited_quality()
    review_probes = probes(inventory, evidence)
    result = {'status': 'passed; external placements unselected; no runtime installation',
              'date': '2026-10-06', 'timezone': 'Australia/Sydney', 'published_head': inventory['published_head'],
              'preserved_predecessor_files': predecessor_count, 'capture_count': 24, 'diagnostic_images': 30,
              'placements': {'baseline': 0, 'sparse': 8, 'grouped': 16},
              'minimum_continuous_clearance_beyond_96px_route': margin, 'gpu_difference_pixels': differences,
              'review_probes': review_probes, 'native_crops': 'complete envelope plus both clusters; exact GPU pixels',
              'quality': 'Verified inherited zero-warning/5033-test logs and successful Godot continuation; no fresh full suite or hosted run',
              'verification_sources': {name: sha(ROOT / name) for name in
                  ('verify.py', 'build.py', 'boards.py', 'readability.py', 'inventory.json', 'placements.json',
                   'capture.gd', 'capture-evidence.json', 'capture-host.log', 'capture-status.json',
                   'review-board-manifest.json', 'readability-manifest.json', 'baseline.json')}}
    if args.record:
        with (ROOT / 'verification.json').open('x') as output:
            output.write(json.dumps(result, indent=2) + '\n')
    print('PASS: frozen published product/art bytes; 24 captures; exact crops; 30 diagnostics; 8 review probes')
    print(f'Preserved {predecessor_count} predecessor files; continuous extra route clearance {margin:.2f}px')


if __name__ == '__main__':
    main()
