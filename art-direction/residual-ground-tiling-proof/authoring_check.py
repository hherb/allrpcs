#!/usr/bin/env python3
"""Lightweight candidate geometry, editable-layer and review-placement checks."""

from io import BytesIO
import json
import zipfile
from xml.etree import ElementTree

import numpy as np
from PIL import Image
from build import ROOT, REPO, MATERIALS, PAIRS, KINDS, digest


def require(condition, message):
    """Keep authoring guards active under optimized Python."""
    if not condition:
        raise ValueError(message)


def raster(path):
    """Load a detached image and close its source handle."""
    with Image.open(path) as image:
        return image.copy()


def equal(a, b):
    """Compare pixels as well as format and geometry."""
    return a.mode == b.mode and a.size == b.size and a.tobytes() == b.tobytes()


def edges(image):
    """Return top, right, bottom and left in increasing world coordinates."""
    return [image.crop((0, 0, image.width, 1)),
            image.crop((image.width - 1, 0, image.width, image.height)),
            image.crop((0, image.height - 1, image.width, image.height)),
            image.crop((0, 0, 1, image.height))]


def check_master(asset, export):
    """Composite stored ORA layers independently before checking downsample parity."""
    with zipfile.ZipFile(ROOT / asset['master']) as archive:
        require(archive.testzip() is None, f"Corrupt master: {asset['id']}")
        require(archive.read('mimetype') == b'image/openraster', 'Master MIME')
        xml = ElementTree.fromstring(archive.read('stack.xml'))
        require((int(xml.attrib['w']), int(xml.attrib['h'])) == (256, 256), '2x master size')
        layers = list(xml.find('stack'))
        require(len(layers) == 3 and layers[0].attrib['name'] == 'Corrections', 'Editable layer coverage')
        merged = Image.new('RGBA', (256, 256))
        for layer in reversed(layers):
            require(layer.attrib['x'] == layer.attrib['y'] == '0' and
                    layer.attrib['opacity'] == '1.0' and layer.attrib['visibility'] == 'visible' and
                    layer.attrib['composite-op'] == 'svg:src-over', 'Unsupported layer composition')
            with Image.open(BytesIO(archive.read(layer.attrib['src']))) as paint:
                require(paint.mode == 'RGBA' and paint.size == (256, 256), 'Layer size/format')
                merged.alpha_composite(paint)
        with Image.open(BytesIO(archive.read('mergedimage.png'))) as stored:
            require(equal(merged, stored), f"Editable layers drift: {asset['id']}")
        require(equal(export, merged.resize((128, 128), Image.Resampling.BOX)),
                f"Master/export parity: {asset['id']}")


def check_boards(inventory, exports):
    """Check all tile placements and every connected adjacency against exact source pixels."""
    joins = 0
    for path, placements in inventory['review_placements'].items():
        board = raster(ROOT / path)
        grid = {}
        for item in placements:
            if 'image' in item:
                tile = raster(ROOT / item['image'])
            elif item['scale'] == 1:
                tile = exports[item['id']]
            else:
                with zipfile.ZipFile(ROOT / f"masters/{item['id']}.ora") as archive:
                    tile = raster(BytesIO(archive.read('mergedimage.png')))
            x, y = item['position']
            require(list(tile.size) == item['size'], 'Placement size metadata')
            require(0 <= x <= board.width - tile.width and 0 <= y <= board.height - tile.height,
                    f'Clipped tile: {path}')
            actual = board.crop((x, y, x + tile.width, y + tile.height))
            require(equal(tile.convert('RGB'), actual), f"Board paint mismatch: {path}/{item['id']}")
            if 'grid' in item:
                key = (item['panel'], *item['grid'])
                require(key not in grid, 'Duplicate connected placement')
                grid[key] = tile
        if path.endswith('connected-1x.png'):
            require(set(grid) == {(panel, x, y) for panel in range(2) for x in range(5) for y in range(5)},
                    'Connected panel coverage')
            for (panel, x, y), tile in grid.items():
                border = edges(tile)
                for neighbour, own, other in [((panel, x + 1, y), 1, 3), ((panel, x, y + 1), 2, 0)]:
                    if neighbour in grid:
                        require(equal(border[own], edges(grid[neighbour])[other]),
                                f'Connected seam mismatch: {path}/{panel}/{x}/{y}/{neighbour}')
                        joins += 1
    require(joins == 160, 'Expected all 160 connected panel joins')
    require({item['id'] for path, items in inventory['review_placements'].items()
             if path.endswith('-detail.png') for item in items} == set(exports), 'Full detail coverage')
    repeats = inventory['review_placements']['review/surfaces-repeat-1x.png']
    require(len(repeats) == 54 and all(sum(item['id'] == name for item in repeats) == 9 for name in MATERIALS),
            'Six complete 3x3 repeated surfaces')
    return joins


def check_orientation_paint():
    """Verify rotated coverage samples retain exact world-aligned material-master pixels."""
    fields = {}
    for name in MATERIALS:
        with zipfile.ZipFile(ROOT / f'masters/{name}.ora') as archive:
            fields[name] = raster(BytesIO(archive.read('mergedimage.png')))
    for pair, (a, b) in PAIRS.items():
        for kind in KINDS:
            mask = raster(ROOT / f'sources/{kind}-mask.png')
            require(mask.mode == 'L' and mask.size == (256, 256), 'Coverage mask format')
            for turns in range(4):
                layer = fields[a].copy()
                layer.putalpha(mask.rotate(90 * turns))
                expected = fields[b].copy()
                expected.alpha_composite(layer)
                expected = expected.resize((128, 128), Image.Resampling.BOX)
                path = (f'review/{pair}-{kind}-orientation-{turns}.png' if turns
                        else f'exports/{pair}-{kind}.png')
                require(equal(expected, raster(ROOT / path)), f'World-aligned paint changed: {path}')


def main():
    """Report authoring evidence only; owner acceptance and runtime gates remain separate."""
    inventory = json.loads((ROOT / 'inventory.json').read_text())
    manifest = json.loads((ROOT / 'review-manifest.json').read_text())
    expected = set(MATERIALS) | {f'{pair}-{kind}' for pair in PAIRS for kind in KINDS}
    require(inventory['count'] == len(inventory['assets']) == 12 and
            {asset['id'] for asset in inventory['assets']} == expected, 'Twelve unique modules')
    for folder, suffix in [('exports', 'png'), ('masters', 'ora')]:
        require({path.stem for path in (ROOT / folder).glob(f'*.{suffix}')} == expected, 'Module file coverage')
    files = {str(path.relative_to(ROOT)) for folder in ('sources', 'exports', 'masters', 'review', 'evidence')
             for path in (ROOT / folder).glob('*') if path.is_file()}
    files |= {'build.py', 'boards.py', 'authoring_check.py', 'inspect_package.py', 'README.md', 'inventory.json'}
    require(set(manifest['files']) == files, 'Review manifest exact coverage')
    for path, sha256 in manifest['files'].items():
        require(digest(ROOT / path) == sha256, f'Changed review input: {path}')
    for path, sha256 in inventory['predecessors'].items():
        require(digest(REPO / path) == sha256, f'Changed approved predecessor: {path}')
    terrain = [path for path in inventory['predecessors'] if '/exports/terrain/' in path]
    require(len(terrain) == 35, 'Existing 35 manifest terrain exports')
    exports = {}
    stats = {}
    for asset in inventory['assets']:
        name = asset['id']
        require(digest(ROOT / asset['master']) == asset['master_sha256'] and
                digest(ROOT / asset['export']) == asset['export_sha256'], 'Asset hashes')
        export = raster(ROOT / asset['export'])
        require(export.mode == 'RGBA' and export.size == (128, 128) and
                export.getchannel('A').getextrema() == (255, 255), f'Filled 128px surface: {name}')
        check_master(asset, export)
        exports[name] = export
        if name in MATERIALS:
            top, right, bottom, left = edges(export)
            require(equal(top, bottom) and equal(left, right), f'Opposite painted edges: {name}')
            pixels = np.asarray(export, dtype=float)[:, :, :3]
            interior = np.concatenate([np.abs(np.diff(pixels, axis=0)).ravel(),
                                       np.abs(np.diff(pixels, axis=1)).ravel()])
            stats[name] = {'interior_rgb_step_p95': round(float(np.percentile(interior, 95)), 2),
                           'rgb_stddev': round(float(pixels.std(axis=(0, 1)).mean()), 2),
                           'opposite_edge_max_delta': 0}
    for pair, (a, b) in PAIRS.items():
        straight = edges(exports[f'{pair}-straight'])
        outer = edges(exports[f'{pair}-outer'])
        inset = edges(exports[f'{pair}-inset'])
        require(equal(straight[0], straight[2]), f'Repeatable straight: {pair}')
        require(equal(straight[3], edges(exports[a])[3]) and equal(straight[1], edges(exports[b])[1]),
                f'Pure material ports: {pair}')
        require(equal(outer[0], straight[0]) and equal(inset[2], straight[2]), f'Curved horizontal ports: {pair}')
        require(not equal(exports[f'{pair}-outer'], exports[f'{pair}-inset']), 'Distinct corner polarity')
    check_orientation_paint()
    joins = check_boards(inventory, exports)
    print(json.dumps({'status': 'lightweight authoring checks pass',
                      'modules': 12, 'editable_2x_masters': 12, 'connected_exact_joins': joins,
                      'predecessor_files_preserved': len(inventory['predecessors']),
                      'surface_texture_diagnostics': stats}, indent=2))


if __name__ == '__main__':
    main()
