#!/usr/bin/env python3
"""Validate the consolidated Square external proof after visual revisions are agreed."""

from io import BytesIO
import json
import math
from pathlib import Path
import subprocess
import zipfile
from xml.etree.ElementTree import fromstring

import numpy as np
from PIL import Image

from build import ROOT, REPO, SIZE, ORIGIN, raster, require, sha, merge


def check_preservation():
    """Verify every predecessor byte except the explicitly maintained continuation docs."""
    baseline = json.loads((ROOT / 'baseline.json').read_text())
    exceptions = {'HANDOVER.md', 'ROADMAP.md', 'README.md'}
    for name, expected in baseline['files'].items():
        if name not in exceptions:
            require((REPO / name).is_file() and sha(REPO / name) == expected, f'Predecessor drift: {name}')
    changed = subprocess.check_output(['git', 'diff', baseline['head'], '--name-only'], cwd=REPO, text=True).splitlines()
    # Existing artwork remains byte-protected above. Independent later external
    # packets must not invalidate this read-only product-preservation check.
    require(all(name in exceptions or name.startswith('art-direction/') for name in changed),
            'Product changes outside the external proof scope')
    return len(baseline['files']) - len(exceptions)


def expected_masks():
    """Use array dilation and two-pass chessboard distance independently of Pillow erosions."""
    source = np.asarray(raster(REPO / 'game/art/runtime/terrain/village-square-junction.png'))[:, :, 3] > 0
    support = np.pad(source, 24)
    for _ in range(24):
        padded = np.pad(support, 1)
        support = np.logical_or.reduce([padded[y:y + 560, x:x + 560] for y in range(3) for x in range(3)])
    # Zero boundary outside the finite proof window is part of the selected feather recipe.
    distance = np.pad(np.where(support, 999, 0).astype(np.int32), 1)
    for y in range(1, 561):
        for x in range(1, 561):
            if distance[y, x]:
                distance[y, x] = min(distance[y, x], 1 + min(distance[y, x - 1], *distance[y - 1, x - 1:x + 2]))
    for y in range(560, 0, -1):
        for x in range(560, 0, -1):
            if distance[y, x]:
                distance[y, x] = min(distance[y, x], 1 + min(distance[y, x + 1], *distance[y + 1, x - 1:x + 2]))
    fraction = np.clip((distance[1:-1, 1:-1] - 0.5) / 16, 0, 1)
    feather = np.rint(255 * fraction * fraction * (3 - 2 * fraction)).astype(np.uint8)
    return support, feather


def check_paint_masks_masters():
    """Check exact world phase, source RGB, mask recipe and all editable master layers."""
    hard, feather = expected_masks()
    tile = np.asarray(raster(REPO / 'art-direction/residual-ground-tiling-proof/exports/packed-earth.png'))
    xx, yy = (ORIGIN[0] + np.arange(560)) % 128, (ORIGIN[1] + np.arange(560)) % 128
    field = tile[yy[:, None], xx[None, :]]
    require(np.array_equal(np.asarray(raster(ROOT / 'proof-inputs/world-paint.png')), field), 'World paint phase drift')
    layers_checked = 0
    for name, expected in [('hard-support', hard.astype(np.uint8) * 255), ('feathered', feather)]:
        with Image.open(ROOT / f'proof-inputs/{name}-coverage.png') as mask:
            require(np.array_equal(np.asarray(mask), expected), f'Mask recipe drift: {name}')
        layer = np.asarray(raster(ROOT / f'proof-inputs/{name}-layer.png'))
        require(np.array_equal(layer[:, :, 3], expected), 'Mask/layer alpha mismatch')
        require(np.array_equal(layer[expected > 0, :3], field[expected > 0, :3]), 'Covered source RGB changed')
        require(not np.any(layer[expected == 0]), 'Transparent canonical pixels changed')
        with zipfile.ZipFile(ROOT / f'masters/{name}.ora') as archive:
            require(archive.read('mimetype') == b'image/openraster', 'Invalid master type')
            xml = fromstring(archive.read('stack.xml'))
            require((xml.attrib['w'], xml.attrib['h']) == ('1120', '1120'), 'Master geometry drift')
            visible = []
            hidden = {}
            for node in xml.find('stack'):
                with Image.open(BytesIO(archive.read(node.attrib['src']))) as image:
                    paint = image.convert('RGBA')
                require(paint.size == (1120, 1120), 'Master layer geometry drift')
                # Every master texel is an exact 2x native block, without new painting.
                native = paint.resize(SIZE, Image.Resampling.NEAREST)
                require(np.array_equal(np.asarray(paint), np.asarray(native.resize((1120, 1120), Image.Resampling.NEAREST))),
                        'Non-exact 2x master layer')
                if node.attrib['visibility'] == 'visible':
                    visible.append((node.attrib['name'], native))
                else:
                    hidden[node.attrib['name']] = np.asarray(native)
                layers_checked += 1
            require(set(hidden) == {'Coverage reference', 'Unmasked world paint'}, 'Missing editable references')
            require(np.array_equal(hidden['Unmasked world paint'], field), 'Editable paint drift')
            require(np.array_equal(hidden['Coverage reference'][:, :, 0], expected), 'Editable coverage drift')
            merged = merge(list(reversed(visible)))
            with Image.open(BytesIO(archive.read('mergedimage.png'))) as image:
                require(np.array_equal(np.asarray(image.convert('RGBA')),
                                       np.asarray(merged.resize((1120, 1120), Image.Resampling.NEAREST))), 'Master merge drift')
            require(np.array_equal(np.asarray(merged), np.asarray(raster(ROOT / f'review/{name}-1x.png'))),
                    'Master/native view mismatch')
    return hard, layers_checked


def check_separation(support):
    """Preserve channel and Garden pixels including a padded native two-pixel guard."""
    guard = np.pad(support, 2)
    for _ in range(2):
        pad = np.pad(guard, 1)
        guard = np.logical_or.reduce([pad[y:y + 564, x:x + 564] for y in range(3) for x in range(3)])
    for name in ('water', 'banks', 'decks', 'detail', 'fringe'):
        channel = raster(REPO / f'game/art/runtime/terrain/channel-{name}.png').getchannel('A')
        canvas = Image.new('L', (564, 564))
        canvas.paste(channel, (-1982, -406))
        require(not np.any(guard & (np.asarray(canvas) > 0)), 'Channel overlap')
    require(ORIGIN[1] + SIZE[1] + 2 < 1243, 'Garden apron guard overlap')


def check_captures(support):
    """Check paired transforms and every changed GPU pixel against the projected mask."""
    evidence = json.loads((ROOT / 'capture-evidence.json').read_text())
    require(len(evidence['captures']) == 18 and len(evidence['frames']) == 18, 'Incomplete captures')
    for path, expected in evidence['inputs'].items():
        source = REPO / 'game' / path.removeprefix('res://') if path.startswith('res://') else Path(path)
        require(source.is_file() and sha(source) == expected, f'Capture input drift: {path}')
    for name, expected in evidence['captures'].items():
        require(sha(ROOT / name) == expected, f'Capture pixel drift: {name}')
    measurements = []
    for zoom in (0.8, 1.0, 2.0):
        for frame, height in [('wide', 720), ('tall', 827)]:
            base_name = f'captures/baseline-{zoom:.1f}x-{frame}.png'
            transform = evidence['frames'][base_name]
            require(transform['viewport'] == [1280, height], 'Capture viewport mismatch')
            a, b, c, d, tx, ty = transform['world_to_canvas']
            require(abs(a - zoom) < 1e-6 and abs(d - zoom) < 1e-6 and b == c == 0, 'Unsupported canvas transform')
            radius = math.ceil(2 / zoom) + 1
            pad = np.pad(support, radius)
            for _ in range(radius):
                p = np.pad(pad, 1)
                size = 560 + 2 * radius
                pad = np.logical_or.reduce([p[y:y + size, x:x + size] for y in range(3) for x in range(3)])
            x = np.floor((np.arange(1280) + 0.5 - tx) / zoom - ORIGIN[0]).astype(int) + radius
            y = np.floor((np.arange(height) + 0.5 - ty) / zoom - ORIGIN[1]).astype(int) + radius
            valid = (y[:, None] >= 0) & (y[:, None] < pad.shape[0]) & (x[None, :] >= 0) & (x[None, :] < pad.shape[1])
            allowed = pad[np.clip(y, 0, pad.shape[0] - 1)[:, None], np.clip(x, 0, pad.shape[1] - 1)[None, :]] & valid
            baseline = np.asarray(raster(ROOT / base_name))
            require(baseline.shape == (height, 1280, 4), 'Capture size drift')
            for variant in ('hard-support', 'feathered'):
                name = f'captures/{variant}-{zoom:.1f}x-{frame}.png'
                require(evidence['frames'][name] == transform, 'Unpaired camera transforms')
                candidate = np.asarray(raster(ROOT / name))
                require(candidate.shape == baseline.shape, 'Candidate viewport drift')
                changed = np.any(candidate != baseline, axis=2)
                require(changed.any() and not np.any(changed & ~allowed), f'GPU change outside support: {name}')
                measurements.append({'variant': variant, 'zoom': zoom, 'frame': frame,
                                     'changed_pixels': int(changed.sum()), 'outside_support_with_filter_guard': 0})
    return measurements


def main():
    """Report consolidated validation without rewriting capture or acceptance records."""
    inventory = json.loads((ROOT / 'inventory.json').read_text())
    for name, expected in inventory['sources'].items():
        source = Path(name) if name.startswith('/') else REPO / name
        require(sha(source) == expected, f'Source drift: {name}')
    for name, expected in inventory['outputs'].items():
        require(sha(ROOT / name) == expected, f'Artwork drift: {name}')
    acceptance = ROOT / 'owner-acceptance.json'
    if acceptance.exists():
        record = json.loads(acceptance.read_text())
        require(record['status'] == 'feathered external composition approved', 'Unexpected acceptance status')
        for name, expected in record['selected_files'].items():
            require(sha(ROOT / name) == expected, f'Selected composition drift: {name}')
        for name, expected in record['evidence_sha256'].items():
            require(sha(ROOT / name) == expected, f'Accepted evidence drift: {name}')
    preserved = check_preservation()
    support, layers = check_paint_masks_masters()
    check_separation(support)
    captures = check_captures(support)
    print(json.dumps({'status': 'pass', 'preserved_predecessor_files': preserved,
                      'editable_master_layers': layers, 'capture_measurements': captures}, indent=2))


if __name__ == '__main__':
    main()
