#!/usr/bin/env python3
"""Independently inspect the external Laundry apron packet."""

from io import BytesIO
import json
import math
from pathlib import Path
import subprocess
import zipfile
from xml.etree.ElementTree import fromstring

import numpy as np
from PIL import Image

from build import ROOT, REPO, SIZE, ORIGIN, SOURCE, VARIANTS, raster, require, sha


def preservation():
    """Keep every predecessor byte except the three maintained continuation documents."""
    baseline = json.loads((ROOT / 'baseline.json').read_text())
    exceptions = {'HANDOVER.md', 'README.md', 'ROADMAP.md'}
    for name, digest in baseline['files'].items():
        if name not in exceptions:
            require((REPO / name).is_file() and sha(REPO / name) == digest, f'Predecessor drift: {name}')
    changed = subprocess.check_output(['git', 'diff', baseline['head'], '--name-only'], cwd=REPO, text=True).splitlines()
    untracked = subprocess.check_output(['git', 'ls-files', '--others', '--exclude-standard'], cwd=REPO, text=True).splitlines()
    require(all(name in exceptions or name.startswith(('art-direction/laundry-ground-feasibility/',))
                for name in changed + untracked), 'Change outside the selected external scope')
    return len(baseline['files']) - len(exceptions)


def recipe():
    """Independently enforce the selected external recipe and frozen input/output hashes."""
    selection = json.loads((ROOT / 'selection.json').read_text())
    inventory = json.loads((ROOT / 'inventory.json').read_text())
    require(selection['origin'] == [1564, 219] and selection['size'] == [92, 210]
            and selection['route_alpha_segment_world_x'] == [1588, 1632]
            and selection['dilation_radius'] == 24 and selection['feather_widths'] == [16, 24]
            and selection['world_phase'] == [0, 0] and selection['period'] == 128
            and selection['z'] == -78 and selection['source_material'] == SOURCE, 'Selected recipe drift')
    require(inventory['origin'] == [1564, 219] and inventory['size'] == [92, 210]
            and inventory['world_phase'] == [0, 0] and inventory['period'] == 128
            and inventory['z'] == -78 and inventory['variants'] == list(VARIANTS), 'Inventory recipe drift')
    for name, digest in inventory['sources'].items():
        # Packet-local bindings resolve to the disposable fixture during rejection probes.
        prefix = 'art-direction/laundry-ground-feasibility/'
        path = ROOT / name.removeprefix(prefix) if name.startswith(prefix) else REPO / name
        require(path.is_file() and sha(path) == digest, 'Inventory source drift: ' + name)
    for name, digest in inventory['outputs'].items():
        require(sha(ROOT / name) == digest, 'Inventory output drift: ' + name)
    script = (ROOT / 'capture.gd').read_text()
    for fragment in ('const CENTER := Vector2(1620, 320)', 'const PAINT_ORIGIN := Vector2(1564, 219)',
                     'Vector2i(92, 210)', 'sprite.z_index = -78', 'sprite.centered = false',
                     'CanvasItem.TEXTURE_FILTER_LINEAR', 'CanvasItem.TEXTURE_REPEAT_DISABLED'):
        require(fragment in script, 'Capture recipe drift: ' + fragment)


def expected_masks():
    """Derive support from world coordinates and compute independent chessboard distances."""
    route = np.asarray(raster(REPO / 'game/art/runtime/terrain/stream-laundry-path.png'))[:, :, 3]
    xx, yy = ORIGIN[0] + np.arange(SIZE[0]), ORIGIN[1] + np.arange(SIZE[1])
    source_x, source_y = xx - 1424, yy - 152
    valid_x, valid_y = (source_x >= 0) & (source_x < 432), (source_y >= 0) & (source_y < 416)
    sampled = route[np.clip(source_y, 0, 415)[:, None], np.clip(source_x, 0, 431)[None, :]]
    support = (sampled > 0) & valid_y[:, None] & valid_x[None, :] & ((xx >= 1588) & (xx < 1632))[None, :]
    for _ in range(24):
        padded = np.pad(support, 1)
        support = np.logical_or.reduce([padded[y:y + SIZE[1], x:x + SIZE[0]]
                                        for y in range(3) for x in range(3)])
    distance = np.pad(np.where(support, 999, 0).astype(np.int32), 1)
    for y in range(1, SIZE[1] + 1):
        for x in range(1, SIZE[0] + 1):
            if distance[y, x]:
                distance[y, x] = min(distance[y, x], 1 + min(distance[y, x - 1], *distance[y - 1, x - 1:x + 2]))
    for y in range(SIZE[1], 0, -1):
        for x in range(SIZE[0], 0, -1):
            if distance[y, x]:
                distance[y, x] = min(distance[y, x], 1 + min(distance[y, x + 1], *distance[y + 1, x - 1:x + 2]))
    masks = {'hard-support': support.astype(np.uint8) * 255}
    for width in (16, 24):
        fraction = np.clip((distance[1:-1, 1:-1] - 0.5) / width, 0, 1)
        masks[f'feather-{width}'] = np.rint(255 * fraction * fraction * (3 - 2 * fraction)).astype(np.uint8)
    return support, masks


def artwork(masks):
    """Inspect RGB/phase/coverage and every master layer without calling the builder's mask code."""
    tile = np.asarray(raster(REPO / SOURCE))
    xx, yy = (ORIGIN[0] + np.arange(SIZE[0])) % 128, (ORIGIN[1] + np.arange(SIZE[1])) % 128
    field = tile[yy[:, None], xx[None, :]]
    require(np.array_equal(np.asarray(raster(ROOT / 'proof-inputs/world-paint.png')), field), 'World paint phase drift')
    layers = 0
    for name, expected in masks.items():
        with Image.open(ROOT / f'proof-inputs/{name}-coverage.png') as coverage:
            require(coverage.mode == 'L' and coverage.size == SIZE and np.array_equal(np.asarray(coverage), expected),
                    f'Coverage recipe drift: {name}')
        layer = np.asarray(raster(ROOT / f'proof-inputs/{name}-layer.png'))
        require(layer.shape == field.shape and np.array_equal(layer[:, :, 3], expected), 'Layer geometry/alpha drift')
        require(np.array_equal(layer[expected > 0, :3], field[expected > 0, :3]), 'Covered source RGB drift')
        require(not np.any(layer[expected == 0]), 'Hidden RGB at zero alpha')
        master_size = (SIZE[0] * 2, SIZE[1] * 2)
        with zipfile.ZipFile(ROOT / f'masters/{name}.ora') as archive:
            require(archive.read('mimetype') == b'image/openraster', 'Master mimetype drift')
            xml = fromstring(archive.read('stack.xml'))
            require((int(xml.attrib['w']), int(xml.attrib['h'])) == master_size, 'Master size drift')
            nodes = list(xml.find('stack'))
            require([(node.attrib['name'], node.attrib['visibility']) for node in nodes] ==
                    [('Corrections', 'visible'), ('Masked packed earth', 'visible'),
                     ('Coverage reference', 'hidden'), ('Unmasked world paint', 'hidden')], 'Master layer structure drift')
            references = [np.zeros_like(layer), layer,
                          np.dstack((expected, expected, expected, np.full_like(expected, 255))), field]
            for node, reference in zip(nodes, references):
                require(node.attrib['opacity'] == '1.0' and node.attrib['x'] == node.attrib['y'] == '0'
                        and node.attrib['composite-op'] == 'svg:src-over', 'Master transform drift')
                with Image.open(BytesIO(archive.read(node.attrib['src']))) as image:
                    require(image.size == master_size and np.array_equal(np.asarray(image.convert('RGBA')),
                            np.repeat(np.repeat(reference, 2, axis=0), 2, axis=1)), 'Master layer pixel drift')
                layers += 1
            with Image.open(BytesIO(archive.read('mergedimage.png'))) as image:
                require(np.array_equal(np.asarray(image.convert('RGBA')), np.repeat(np.repeat(layer, 2, axis=0), 2, axis=1)),
                        'Master merge drift')
    return layers


def separation(support):
    """Check all accepted Garden, Square and channel alpha at their actual world origins."""
    guard = np.pad(support, 2)
    for _ in range(2):
        padded = np.pad(guard, 1)
        guard = np.logical_or.reduce([padded[y:y + SIZE[1] + 4, x:x + SIZE[0] + 4]
                                      for y in range(3) for x in range(3)])
    sources = [('garden-ground-meadow', (2048, 1120)), ('garden-ground-soil', (2048, 1120)),
               ('village-square-ground', (1920, 620)), ('sickhouse-entrance-ground', (2408,1248)), ('wagon-entrance-ground', (1633,1512)), ('shrine-entrance-ground', (596,250))]
    sources.extend((f'channel-{name}', (-64, 212)) for name in ('water', 'banks', 'decks', 'detail', 'fringe'))
    for name, origin in sources:
        alpha = raster(REPO / f'game/art/runtime/terrain/{name}.png').getchannel('A')
        canvas = Image.new('L', (SIZE[0] + 4, SIZE[1] + 4))
        canvas.paste(alpha, tuple(origin[i] - (ORIGIN[i] - 2) for i in range(2)))
        require(not np.any(guard & (np.asarray(canvas) > 0)), f'Protected paint overlap: {name}')
    return len(sources)


def captures(support):
    """Verify all paired transforms and confine every changed GPU pixel to guarded support."""
    evidence = json.loads((ROOT / 'capture-evidence.json').read_text())
    expected_names = {f'captures/{name}-{zoom:.1f}x-{frame}.png' for name in VARIANTS
                      for zoom in (0.8, 1.0, 2.0) for frame in ('wide', 'tall')}
    require(set(evidence['captures']) == set(evidence['frames']) == expected_names, 'Incomplete GPU series')
    for name, digest in evidence['inputs'].items():
        path = REPO / 'game' / name.removeprefix('res://') if name.startswith('res://') else Path(name)
        require(path.is_file() and sha(path) == digest, f'Capture input drift: {name}')
    for name, digest in evidence['captures'].items():
        require(sha(ROOT / name) == digest, f'Capture byte drift: {name}')
    guard = np.pad(support, 2)
    for _ in range(2):
        pad = np.pad(guard, 1)
        guard = np.logical_or.reduce([pad[y:y + SIZE[1] + 4, x:x + SIZE[0] + 4] for y in range(3) for x in range(3)])
    results = []
    for zoom in (0.8, 1.0, 2.0):
        for frame, height in [('wide', 720), ('tall', 827)]:
            key = f'captures/baseline-{zoom:.1f}x-{frame}.png'
            transform = evidence['frames'][key]
            require(transform['viewport'] == [1280, height] and transform['zoom'] == zoom, 'Viewport/zoom drift')
            a, b, c, d, tx, ty = transform['world_to_canvas']
            require(math.isclose(a, zoom, abs_tol=1e-6) and math.isclose(d, zoom, abs_tol=1e-6)
                    and b == c == 0, 'Unexpected canvas transform')
            # Retain ordinary camera limits; northern/western clamps apply at smaller zooms.
            expected_center = [max(1620, 640 / zoom), max(320, height / (2 * zoom))]
            require(np.allclose(transform['screen_center'], expected_center, atol=1e-3), 'Camera centre drift')
            require(abs(tx - (640 - expected_center[0] * a)) < 1e-3
                    and abs(ty - (height / 2 - expected_center[1] * d)) < 1e-3, 'Canvas/camera mismatch')
            base = np.asarray(raster(ROOT / key))
            require(base.shape == (height, 1280, 4), 'Baseline size drift')
            xx = np.floor(((np.arange(1280) + 0.5 - tx) / a) - ORIGIN[0] + 2).astype(int)
            yy = np.floor(((np.arange(height) + 0.5 - ty) / d) - ORIGIN[1] + 2).astype(int)
            valid = (yy[:, None] >= 0) & (yy[:, None] < SIZE[1] + 4) & (xx[None, :] >= 0) & (xx[None, :] < SIZE[0] + 4)
            allowed = valid & guard[np.clip(yy, 0, SIZE[1] + 3)[:, None], np.clip(xx, 0, SIZE[0] + 3)[None, :]]
            for name in VARIANTS[1:]:
                key = f'captures/{name}-{zoom:.1f}x-{frame}.png'
                require(evidence['frames'][key] == transform, 'Paired camera drift')
                image = np.asarray(raster(ROOT / key))
                require(image.shape == base.shape, 'Candidate size drift')
                changed = np.any(image != base, axis=2)
                require(np.any(changed) and not np.any(changed & ~allowed), f'Unbounded GPU change: {key}')
                results.append({'capture': key, 'changed_pixels': int(changed.sum())})
    return results


def check():
    """Return a complete validation result; caller decides where to retain evidence."""
    recipe()
    support, masks = expected_masks()
    return {'status': 'passed', 'preserved_predecessor_files': preservation(),
            'master_layers_checked': artwork(masks), 'protected_alpha_planes': separation(support),
            'gpu_comparisons': captures(support)}


if __name__ == '__main__':
    print(json.dumps(check(), indent=2))
