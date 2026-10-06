#!/usr/bin/env python3
"""Independently check frozen inputs, editable paint, and bounded GPU changes."""

import hashlib
from io import BytesIO
import json
from pathlib import Path
import subprocess
import zipfile
from xml.etree.ElementTree import fromstring

import numpy as np
from PIL import Image

ROOT = Path(__file__).resolve().parent
REPO = ROOT.parents[1]
PREFIX = str(ROOT.relative_to(REPO)) + '/'
DOCUMENTS = {'README.md', 'ROADMAP.md', 'HANDOVER.md'}
VARIANTS = ('baseline', 'hard-support', 'feather-16', 'feather-24')
ORIGIN = (1008, 414)
SIZE = (304, 164)


def require(condition, message):
    if not condition:
        raise ValueError(message)


def digest(data):
    return hashlib.sha256(data).hexdigest()


def pixels(path):
    with Image.open(path) as image:
        return np.array(image.convert('RGBA'))


def plane(path):
    with Image.open(path) as image:
        require(image.mode == 'L' and image.size == SIZE, 'Invalid coverage plane')
        return np.array(image)


def dilate(mask, radius):
    """Independent Boolean neighbourhood union, without Pillow morphology."""
    height, width = mask.shape
    padded = np.pad(mask, radius)
    horizontal = np.logical_or.reduce([padded[:, dx:dx + width] for dx in range(2 * radius + 1)])
    return np.logical_or.reduce([horizontal[dy:dy + height] for dy in range(2 * radius + 1)])


def source_alpha(path, origin):
    alpha = pixels(path)[:, :, 3]
    result = np.zeros((SIZE[1], SIZE[0]), dtype=bool)
    for row in range(SIZE[1]):
        y = ORIGIN[1] + row - origin[1]
        if not 0 <= y < alpha.shape[0]:
            continue
        x = ORIGIN[0] + np.arange(SIZE[0]) - origin[0]
        valid = (x >= 0) & (x < alpha.shape[1])
        result[row, valid] = alpha[y, x[valid]] > 0
    return result


def check_preservation(baseline):
    tree = subprocess.check_output(['git', 'ls-tree', '-r', baseline['head']], cwd=REPO, text=True)
    blobs = {}
    for row in tree.splitlines():
        metadata, name = row.split('\t', 1)
        mode, kind, blob = metadata.split()
        require(kind == 'blob' and mode in ('100644', '100755'), 'Unsupported baseline entry')
        blobs[name] = blob
    require(set(blobs) == set(baseline['files']), 'Incomplete predecessor inventory')
    for name, expected in baseline['files'].items():
        data = subprocess.check_output(['git', 'show', baseline['head'] + ':' + name], cwd=REPO) \
            if name in DOCUMENTS else (REPO / name).read_bytes()
        require(digest(data) == expected, 'Predecessor SHA-256 drift: ' + name)
        blob = hashlib.sha1(b'blob ' + str(len(data)).encode() + b'\0' + data).hexdigest()
        require(blob == blobs[name], 'Baseline differs from committed Git blob: ' + name)
    changed = subprocess.check_output(['git', 'diff', baseline['head'], '--name-only'], cwd=REPO, text=True).splitlines()
    untracked = subprocess.check_output(['git', 'ls-files', '--others', '--exclude-standard'], cwd=REPO, text=True).splitlines()
    require(all(name in DOCUMENTS or name.startswith(PREFIX) for name in changed + untracked),
            'Change outside external comparison scope')
    require(not any(name.endswith('.DS_Store') for name in changed + untracked), 'Finder metadata in scope')


def check_art(inventory):
    source = pixels(REPO / 'game/art/runtime/terrain/woodland-leaf-litter.png')
    require(source.shape == (256, 256, 4) and np.all(source[:, :, 3] == 255), 'Material changed')
    xx = (ORIGIN[0] + np.arange(SIZE[0])) % 256
    yy = (ORIGIN[1] + np.arange(SIZE[1])) % 256
    field = source[yy[:, None], xx[None, :]]
    require(np.array_equal(field, pixels(ROOT / 'proof-inputs/world-paint.png')), 'World paint phase drift')
    route = source_alpha(REPO / 'game/art/runtime/terrain/woodland-village-edge-path.png', (944, 288))
    rows, cols = np.indices(route.shape)
    segment = route & (cols + ORIGIN[0] >= 1056) & (cols + ORIGIN[0] < 1240) \
        & (rows + ORIGIN[1] >= 450) & (rows + ORIGIN[1] < 490)
    channel = np.zeros(route.shape, dtype=bool)
    for name in ('water', 'banks', 'decks', 'detail', 'fringe'):
        channel |= source_alpha(REPO / f'game/art/runtime/terrain/channel-{name}.png', (-64, 212))
    forbidden = dilate(channel, 4)
    require(np.array_equal(plane(ROOT / 'proof-inputs/channel-exclusion.png') > 0, forbidden), 'Channel exclusion drift')
    expanded = dilate(segment, 24)
    support = expanded & ~forbidden
    require(inventory['recipe']['channel_excluded_texels'] == int(np.count_nonzero(expanded & forbidden)),
            'Incorrect channel exclusion measurement')
    require(np.array_equal(plane(ROOT / 'proof-inputs/hard-support-coverage.png'), support.astype('uint8') * 255),
            'Native segment/dilation support drift')
    masks = {}
    for name in VARIANTS[1:]:
        mask = plane(ROOT / f'proof-inputs/{name}-coverage.png')
        require(not np.any((mask > 0) & ~support), 'Feather expands beyond support')
        if name != 'hard-support':
            width = int(name.split('-')[1])
            distance = np.full(support.shape, width + 1, dtype=np.float64)
            # Distance to the nearest absent support texel in the Chebyshev metric.
            for radius in range(width, -1, -1):
                distance[dilate(~support, radius)] = radius
            fraction = np.clip((distance - 0.5) / width, 0, 1)
            expected = np.rint(255 * fraction * fraction * (3 - 2 * fraction)).astype('uint8')
            expected[distance > width] = 255
            require(np.array_equal(mask, expected), 'Incorrect inward smoothstep: ' + name)
        paint = pixels(ROOT / f'proof-inputs/{name}-layer.png')
        require(paint.shape == field.shape and np.array_equal(paint[:, :, 3], mask), 'Paint coverage mismatch')
        require(np.array_equal(paint[mask > 0, :3], field[mask > 0, :3]), 'Retained source RGB changed')
        require(np.all(paint[mask == 0] == 0), 'Hidden RGB under zero alpha')
        require(not np.any((mask > 0) & forbidden), 'Paint overlaps guarded channel')
        with zipfile.ZipFile(ROOT / f'masters/{name}.ora') as archive:
            require(archive.namelist()[0] == 'mimetype' and archive.read('mimetype') == b'image/openraster',
                    'Invalid OpenRaster format')
            xml = fromstring(archive.read('stack.xml'))
            require((int(xml.attrib['w']), int(xml.attrib['h'])) == (SIZE[0] * 2, SIZE[1] * 2), 'Master is not 2x')
            layers = xml.find('stack').findall('layer')
            require([layer.attrib['name'] for layer in layers] ==
                    ['Corrections', 'Masked leaf litter', 'Coverage reference', 'Unmasked world paint'],
                    'Editable master layers changed')
            require([layer.attrib['visibility'] for layer in layers] == ['visible', 'visible', 'hidden', 'hidden'],
                    'Editable master visibility changed')
            expected_layers = [np.zeros_like(paint), paint,
                               np.stack((mask, mask, mask, np.full_like(mask, 255)), axis=2), field]
            for layer, expected in zip(layers, expected_layers):
                actual = pixels(BytesIO(archive.read(layer.attrib['src'])))
                require(np.array_equal(actual, expected.repeat(2, axis=0).repeat(2, axis=1)), 'Master layer mismatch')
            merged = pixels(BytesIO(archive.read('mergedimage.png')))
            require(np.array_equal(merged, paint.repeat(2, axis=0).repeat(2, axis=1)), 'Master merged pixels drift')
        masks[name] = mask
    return masks


def resolve_capture_input(name):
    path = REPO / 'game' / name.removeprefix('res://') if name.startswith('res://') else Path(name)
    path = path.resolve()
    require(path.is_relative_to(REPO), 'Capture input outside repository')
    return path


def check_difference(actual, baseline, mask, transform):
    """Reject any visible mutation beyond the filtered candidate support."""
    a, b, c, d, tx, ty = transform
    require(a > 0 and d > 0 and b == c == 0, 'Invalid difference transform')
    require(actual.shape == baseline.shape and actual.ndim == 3 and actual.shape[2] == 4,
            'Difference viewport mismatch')
    height, width = actual.shape[:2]
    changed = np.any(actual != baseline, axis=2)
    protected = dilate(mask > 0, 2).astype('uint8') * 255
    allowed = Image.fromarray(protected).transform((width, height), Image.Transform.AFFINE,
        (1 / a, 0, -tx / a - ORIGIN[0], 0, 1 / d, -ty / d - ORIGIN[1]),
        resample=Image.Resampling.NEAREST)
    require(not np.any(changed & (np.asarray(allowed) == 0)), 'GPU changes outside proposed support')
    count = int(np.count_nonzero(changed))
    require(count > 0, 'Invisible or missing candidate')
    return count


def check_capture(inventory, masks):
    evidence = json.loads((ROOT / 'capture-evidence.json').read_text())
    expected = {f'captures/{name}-{zoom:.1f}x-{frame}.png' for name in VARIANTS
                for zoom in (0.8, 1, 2) for frame in ('wide', 'tall')}
    require(set(evidence['captures']) == set(evidence['frames']) == expected, 'Incomplete GPU matrix')
    for name, expected_hash in evidence['inputs'].items():
        require(digest(resolve_capture_input(name).read_bytes()) == expected_hash, 'Capture input drift: ' + name)
    for name, expected_hash in inventory['sources'].items():
        require(evidence['inputs'].get(str(REPO / name)) == expected_hash, 'Unbound capture input: ' + name)
    for name in ('inventory.json', 'baseline.json'):
        require(evidence['inputs'].get(str(ROOT / name)) == digest((ROOT / name).read_bytes()), 'Capture metadata drift')
    require(any(resolve_capture_input(name) == ROOT / 'capture.gd' for name in evidence['inputs']), 'Capture harness unbound')
    prior = json.loads((REPO / 'art-direction/closed-forest-ground-coverage/capture-evidence.json').read_text())
    require(evidence['owners'] == prior['owners'], 'Accepted runtime owner changed')
    require(evidence['target_center'] == [1160, 510], 'Capture target changed')
    changes = {}
    for zoom in (0.8, 1, 2):
        for frame, height in (('wide', 720), ('tall', 827)):
            name = f'captures/baseline-{zoom:.1f}x-{frame}.png'
            baseline = pixels(ROOT / name)
            transform = evidence['frames'][name]['world_to_canvas']
            a, b, c, d, tx, ty = transform
            require(abs(a - zoom) < 1e-6 and abs(d - zoom) < 1e-6 and b == c == 0, 'Capture zoom drift')
            for variant in VARIANTS:
                candidate_name = f'captures/{variant}-{zoom:.1f}x-{frame}.png'
                require(digest((ROOT / candidate_name).read_bytes()) == evidence['captures'][candidate_name], 'GPU capture hash drift')
                actual = pixels(ROOT / candidate_name)
                record = evidence['frames'][candidate_name]
                require(actual.shape == (height, 1280, 4) and record['viewport'] == [1280, height]
                        and np.all(actual[:, :, 3] == 255), 'Invalid opaque GPU viewport')
                require(record['world_to_canvas'] == transform and record['screen_center'] == evidence['frames'][name]['screen_center'],
                        'Comparison camera changed')
                if variant == 'baseline':
                    continue
                changes[candidate_name] = check_difference(actual, baseline, masks[variant], transform)
    log = (ROOT / 'capture-host.log').read_text()
    require('PASS: complete 24-view Clearing opening external comparison' in log
            and log.count('Clearing opening external preview: ') == 24
            and not any(term in log for term in ('SCRIPT ERROR', 'Parse Error', 'ERROR:')),
            'Incomplete or erroneous capture log')
    return changes


def check_hash_manifest(path):
    record = json.loads(path.read_text())
    for group in ('sources', 'outputs'):
        for name, expected in record[group].items():
            file = Path(name) if Path(name).is_absolute() else ROOT / name
            require(digest(file.read_bytes()) == expected, 'Review manifest drift: ' + name)


def main():
    baseline = json.loads((ROOT / 'baseline.json').read_text())
    check_preservation(baseline)
    inventory = json.loads((ROOT / 'inventory.json').read_text())
    require(inventory['baseline_head'] == baseline['head'] and inventory['origin'] == list(ORIGIN)
            and inventory['size'] == list(SIZE) and inventory['z_index'] == -78
            and inventory['world_phase'] == [0, 0] and inventory['period'] == 256
            and inventory['variants'] == list(VARIANTS), 'Comparison recipe drift')
    require(inventory['source'] == 'game/art/runtime/terrain/woodland-leaf-litter.png'
            and inventory['center'] == [1160, 510]
            and inventory['recipe']['native_route_segment'] == [1056, 450, 1240, 490]
            and inventory['recipe']['dilation_radius'] == 24
            and inventory['recipe']['channel_exclusion_guard'] == 4
            and inventory['recipe']['feather_widths'] == [16, 24], 'Mask recipe drift')
    game = {name: value for name, value in baseline['files'].items() if name.startswith('game/')}
    require(game == {name: value for name, value in inventory['sources'].items() if name.startswith('game/')},
            'Missing tracked game input')
    for name, expected in inventory['sources'].items():
        require(digest((REPO / name).read_bytes()) == expected, 'Authoring source drift: ' + name)
    for name, expected in inventory['outputs'].items():
        require(digest((ROOT / name).read_bytes()) == expected, 'Authoring output drift: ' + name)
    masks = check_art(inventory)
    for name, mask in masks.items():
        measurement = inventory['measurements'][name]
        yy, xx = np.nonzero(mask)
        require(measurement == {'nonzero_texels': int(np.count_nonzero(mask)),
                                'opaque_texels': int(np.count_nonzero(mask == 255)),
                                'alpha_bounds': [int(xx.min()), int(yy.min()), int(xx.max()) + 1, int(yy.max()) + 1]},
                'Authoring measurement drift: ' + name)
    changes = check_capture(inventory, masks)
    check_hash_manifest(ROOT / 'review-board-manifest.json')
    check_hash_manifest(ROOT / 'readability-manifest.json')
    predecessor = REPO / 'art-direction/village-edge-ground-runtime-promotion'
    acceptance = json.loads((predecessor / 'owner-runtime-acceptance.json').read_text())
    require(acceptance['status'].startswith('accepted exact nine actual runtime views'), 'Terminal acceptance missing')
    for name, expected in acceptance['implementation_sha256'].items():
        require(digest((REPO / name).read_bytes()) == expected, 'Accepted implementation drift')
    quality = json.loads((predecessor / 'verification.json').read_text())
    require(quality['test_total'] == sum(quality['test_suites'].values()) == 5033
            and quality['build_warnings'] == quality['build_errors'] == 0
            and quality['quality']['host_retry_exit'] == 0, 'Inherited quality incomplete')
    for name, expected in quality['evidence_sha256'].items():
        require(digest((predecessor / name).read_bytes()) == expected, 'Inherited evidence drift')
    print(f'PASS: {len(baseline["files"]) - 3} preserved predecessors; independent masks, RGB, 2x masters; '
          '24 bounded GPU views; terminal acceptance and inherited 5033-test quality')
    print(json.dumps({'changed_screen_pixels': changes}, sort_keys=True))


if __name__ == '__main__':
    main()
