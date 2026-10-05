#!/usr/bin/env python3
"""Check external authoring integrity without claiming owner approval or product CI."""

from io import BytesIO
import json
from pathlib import Path
import zipfile
from xml.etree.ElementTree import fromstring

import numpy as np
from PIL import Image, ImageFilter

from build import ROOT, REPO, AUDIT, SIZE, ORIGIN, SOURCE, VARIANTS, raster, require, sha


def bindings():
    """Verify frozen sources, capture matrices and board output bindings."""
    inventory = json.loads((ROOT / 'inventory.json').read_text())
    for name, digest in inventory['sources'].items():
        require(sha(REPO / name) == digest, 'Source drift: ' + name)
    for name, digest in inventory['outputs'].items():
        require(sha(ROOT / name) == digest, 'External export drift: ' + name)
    for packet, count, centers in [(ROOT, 24, [[1700, 840]]),
                                    (ROOT / 'details', 12, [[1556, 800], [1750, 680], [1844, 900]])]:
        evidence = json.loads((packet / 'capture-evidence.json').read_text())
        expected = {f'captures/{variant}-{zoom:.1f}x-{frame}.png' for variant in VARIANTS
                    for zoom, frame in ([(z, f) for z in (0.8, 1.0, 2.0) for f in ('wide', 'tall')]
                                        if count == 24 else [(2.0, f) for f in ('woodland', 'stream', 'square')])}
        require(set(evidence['captures']) == set(evidence['frames']) == expected,
                'Incomplete capture matrix')
        for name, digest in evidence['inputs'].items():
            path = REPO / 'game' / name.removeprefix('res://') if name.startswith('res://') else Path(name)
            require(path.resolve().is_relative_to(REPO) and sha(path) == digest, 'Capture input drift')
        for name, digest in inventory['sources'].items():
            require(evidence['inputs'].get(str(REPO / name)) == digest, 'Capture omitted inventory source')
        for name, digest in evidence['captures'].items():
            require(sha(packet / name) == digest, 'Capture drift: ' + name)
            require(evidence['frames'][name]['screen_center'] in centers, 'Unexpected camera center')
    boards = json.loads((ROOT / 'review-board-manifest.json').read_text())
    for name, digest in boards['sources'].items():
        path = Path(name) if Path(name).is_absolute() else ROOT / name
        require(sha(path) == digest, 'Review input drift')
    for name, digest in boards['outputs'].items():
        require(sha(ROOT / name) == digest, 'Review output drift')


def masters_and_paint():
    """Inspect editable layers and native world-phase RGB independently of their builder."""
    source = np.asarray(raster(REPO / SOURCE))
    xx = (ORIGIN[0] + np.arange(SIZE[0])) % 128
    yy = (ORIGIN[1] + np.arange(SIZE[1])) % 128
    field = source[yy[:, None], xx[None, :]]
    support = np.asarray(Image.open(ROOT / 'proof-inputs/hard-support-coverage.png')) > 0
    for name in VARIANTS[1:]:
        layer = np.asarray(raster(ROOT / f'proof-inputs/{name}-layer.png'))
        coverage = np.asarray(Image.open(ROOT / f'proof-inputs/{name}-coverage.png'))
        visible = layer[:, :, 3] > 0
        require(layer.shape == (390, 500, 4) and np.array_equal(layer[:, :, 3], coverage),
                'Coverage/export mismatch')
        require(np.array_equal(layer[visible, :3], field[visible, :3])
                and np.all(layer[~visible] == 0), 'Paint phase or hidden RGB changed')
        require(not np.any(visible & ~support), 'Feather extends beyond hard support')
        with zipfile.ZipFile(ROOT / f'masters/{name}.ora') as archive:
            require(archive.read('mimetype') == b'image/openraster', 'Invalid master type')
            xml = fromstring(archive.read('stack.xml'))
            require((xml.attrib['w'], xml.attrib['h']) == ('1000', '780'), 'Master scale changed')
            layers = xml.find('stack').findall('layer')
            require([item.attrib['name'] for item in layers] ==
                    ['Corrections', 'Masked packed earth', 'Coverage reference', 'Unmasked world paint'],
                    'Editable layer contract changed')
            require([item.attrib['visibility'] for item in layers] == ['visible', 'visible', 'hidden', 'hidden'],
                    'Master visibility changed')
            for item, expected in zip(layers, [np.zeros_like(layer), layer,
                                               np.dstack((coverage, coverage, coverage, np.full_like(coverage, 255))), field]):
                image = np.asarray(Image.open(BytesIO(archive.read(item.attrib['src']))).convert('RGBA'))
                require(np.array_equal(image, np.repeat(np.repeat(expected, 2, axis=0), 2, axis=1)),
                        'Editable master layer differs from native export')
            merged = np.asarray(Image.open(BytesIO(archive.read('mergedimage.png'))).convert('RGBA'))
            require(np.array_equal(merged, np.repeat(np.repeat(layer, 2, axis=0), 2, axis=1)),
                    'Master composite/export mismatch')


def rendered_scope():
    """Require exact audited baselines and restrict every changed GPU texel to guarded support."""
    evidence = json.loads((ROOT / 'capture-evidence.json').read_text())
    support = Image.open(ROOT / 'proof-inputs/hard-support-coverage.png').convert('L')
    guard = np.asarray(support.filter(ImageFilter.MaxFilter(5))) > 0
    for zoom in (0.8, 1.0, 2.0):
        for frame in ('wide', 'tall'):
            path = f'captures/baseline-{zoom:.1f}x-{frame}.png'
            baseline = np.asarray(raster(ROOT / path))
            require(np.array_equal(baseline, np.asarray(raster(AUDIT / path))), 'Audited baseline changed')
            a, b, c, d, tx, ty = evidence['frames'][path]['world_to_canvas']
            require(b == c == 0 and abs(a - zoom) < 1e-6 and abs(d - zoom) < 1e-6, 'Unexpected render axes')
            xx = np.floor((np.arange(baseline.shape[1]) + 0.5 - tx) / a - ORIGIN[0]).astype(int)
            yy = np.floor((np.arange(baseline.shape[0]) + 0.5 - ty) / d - ORIGIN[1]).astype(int)
            in_x, in_y = (xx >= 0) & (xx < SIZE[0]), (yy >= 0) & (yy < SIZE[1])
            allowed = guard[np.clip(yy, 0, SIZE[1] - 1)[:, None], np.clip(xx, 0, SIZE[0] - 1)[None, :]]
            allowed &= in_y[:, None] & in_x[None, :]
            for variant in VARIANTS[1:]:
                image = np.asarray(raster(ROOT / f'captures/{variant}-{zoom:.1f}x-{frame}.png'))
                changed = np.any(image != baseline, axis=2)
                require(np.count_nonzero(changed) > 0 and not np.any(changed & ~allowed),
                        'Rendered changes escape guarded external apron: ' + variant)
    print('PASS: six exact audit baselines; all eighteen comparison deltas inside guarded support')


def main():
    """Perform only lightweight package-integrity and authoring checks."""
    bindings()
    masters_and_paint()
    rendered_scope()
    print('PASS: three editable 2x masters; exact paint/alpha; 36 full/detail views and bound review boards')


if __name__ == '__main__':
    main()
