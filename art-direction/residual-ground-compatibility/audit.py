#!/usr/bin/env python3
"""Render or independently recheck read-only, native-scale ground comparisons."""

import argparse
import hashlib
import json
from pathlib import Path

import numpy as np
from PIL import Image, ImageDraw, ImageFont

ROOT = Path(__file__).resolve().parent
REPO = ROOT.parents[1]
TILING = REPO / 'art-direction/residual-ground-tiling-proof'
BATCH = REPO / 'art-direction/residual-ground-batch'
PRODUCTION = REPO / 'art-direction/production-proof/corrected'
FONT_PATH = Path('/System/Library/Fonts/Supplemental/Arial.ttf')
FIELD_SIZE = 256
TILE_SIZE = 128
BACKGROUND = '#e8e3d5'
INK = '#202820'


def digest(path):
    """Bind persisted bytes, including every frozen predecessor export."""
    return hashlib.sha256(path.read_bytes()).hexdigest()


def read_json(path):
    """Read metadata without importing predecessor builders."""
    return json.loads(path.read_text())


def require(condition, message):
    """Keep invariant checks active under optimized Python."""
    if not condition:
        raise ValueError(message)


def inputs():
    """Verify both complete accepted packets and all 35 production terrain exports."""
    bound = {str(FONT_PATH): digest(FONT_PATH)}

    def bind(path, expected=None):
        actual = digest(path)
        require(expected is None or actual == expected, f'Frozen input changed: {path}')
        bound[str(path.relative_to(REPO))] = actual

    inventories = []
    for package, count in ((TILING, 57), (BATCH, 45)):
        acceptance = read_json(package / 'owner-acceptance.json')
        manifest = read_json(package / 'review-manifest.json')
        require(acceptance['status'] == 'approved', 'Expected owner-approved packet')
        require(len(acceptance['approved_artwork']) == count, 'Incomplete artwork acceptance')
        bind(package / 'owner-acceptance.json')
        bind(package / 'review-manifest.json', acceptance['review_manifest_sha256'])
        # Validate all packet inputs, not just the subset drawn on these boards.
        for path, expected in manifest['files'].items():
            bind(package / path, expected)
        for path, expected in acceptance['approved_artwork'].items():
            bind(package / path, expected)
        inventories.append(read_json(package / 'inventory.json'))
    tiling, batch = inventories
    production_manifest = PRODUCTION / 'asset-manifest.json'
    relative = str(production_manifest.relative_to(REPO))
    bind(production_manifest, tiling['predecessors'][relative])
    terrain = [a for a in read_json(production_manifest)['assets'] if a['id'].startswith('terrain.')]
    require(len(terrain) == 35, 'Expected all 35 retained terrain exports')
    for asset in terrain:
        bind(PRODUCTION / asset['export'], asset['sha256'])
    fields = [a for a in tiling['assets'] if a['family'] == 'surfaces']
    overlays = [a for a in batch['assets'] if a['id'].startswith('ground-variation-')]
    legacy = [a for a in terrain if a['id'] in ('terrain.quiet_moss', 'terrain.woodland_leaf_litter')]
    require((len(fields), len(overlays), len(legacy)) == (6, 6, 2), 'Incomplete comparison matrix')
    require(len({a['id'] for a in fields + overlays + legacy}) == 14, 'Duplicate input identifiers')
    return fields, overlays, legacy, dict(sorted(bound.items()))


def raster(path, size=None):
    """Load exact RGBA pixels without resampling or alpha removal."""
    with Image.open(path) as source:
        image = source.convert('RGBA')
    require(size is None or image.size == size, f'Unexpected native size: {path}')
    return image


def repeated_field(asset):
    """Repeat a filled 128px export at native phase on a 256px field."""
    tile = raster(TILING / asset['export'], (TILE_SIZE, TILE_SIZE))
    require(tile.getchannel('A').getextrema() == (255, 255), 'Field must be fully filled')
    result = Image.new('RGBA', (FIELD_SIZE, FIELD_SIZE))
    for y in range(0, FIELD_SIZE, TILE_SIZE):
        for x in range(0, FIELD_SIZE, TILE_SIZE):
            result.alpha_composite(tile, (x, y))
    return result


def canvas(size, title, subtitle):
    """Keep all labels outside sampled artwork rectangles."""
    result = Image.new('RGBA', size, BACKGROUND)
    draw = ImageDraw.Draw(result)
    draw.text((20, 12), title, font=ImageFont.truetype(str(FONT_PATH), 22), fill=INK)
    draw.text((20, 44), subtitle, font=ImageFont.truetype(str(FONT_PATH), 16), fill=INK)
    return result, draw


def place(board, image, xy):
    """Reject clipping and preserve each native composited pixel."""
    x, y = xy
    require(x >= 0 and y >= 0 and x + image.width <= board.width and
            y + image.height <= board.height, 'Board placement clips artwork')
    board.alpha_composite(image, xy)


def edge_metrics(old, new):
    """Describe direct RGBA joins without declaring a visual defect from colour deltas."""
    a, b = np.asarray(old, dtype=int), np.asarray(new, dtype=int)
    result = {}
    for name, left, right in (('vertical', a[:, -1], b[:, 0]),
                              ('horizontal', a[-1], b[0])):
        delta = np.abs(left - right)
        result[name] = {'rgb_max_delta': int(delta[:, :3].max()),
                        'rgb_mean_delta': round(float(delta[:, :3].mean()), 2),
                        'exact_rgba_port': bool(not delta.any())}
    return result


def render():
    """Build the complete 36-overlay and 12-pair, two-direction evidence matrix."""
    fields, overlays, legacy, bound = inputs()
    boards, overlay_rows, joins = {}, [], []
    font = ImageFont.truetype(str(FONT_PATH), 16)
    for field in fields:
        paint = repeated_field(field)
        board, draw = canvas((900, 710), f'{field["id"]} / six accepted overlays',
                             'Exact 1:1 pixels / 2x2 field repeats / overlays centred without rotation or resizing')
        filename = f'review/overlays-{field["id"]}-1x.png'
        for index, overlay in enumerate(overlays):
            x, y = 20 + index % 3 * 294, 90 + index // 3 * 305
            image = raster(BATCH / overlay['export'], tuple(overlay['size']))
            offset = ((FIELD_SIZE - image.width) // 2, (FIELD_SIZE - image.height) // 2)
            require(min(offset) >= 0, 'Overlay exceeds comparison field')
            sample = paint.copy()
            sample.alpha_composite(image, offset)
            place(board, sample, (x, y))
            draw.text((x, y + FIELD_SIZE + 8), overlay['name'], font=font, fill=INK)
            draw.text((x, y + FIELD_SIZE + 28), overlay['id'], font=font, fill=INK)
            alpha = np.asarray(image.getchannel('A'))
            overlay_rows.append({'field': field['id'], 'overlay': overlay['id'], 'board': filename,
                                 'field_rect': [x, y, FIELD_SIZE, FIELD_SIZE],
                                 'field_tile': str((TILING / field['export']).relative_to(REPO)),
                                 'field_tile_origins': [[x, y], [x + TILE_SIZE, y],
                                                        [x, y + TILE_SIZE], [x + TILE_SIZE, y + TILE_SIZE]],
                                 'overlay_source': str((BATCH / overlay['export']).relative_to(REPO)),
                                 'overlay_origin': [x + offset[0], y + offset[1]],
                                 'overlay_size': list(image.size), 'scale': 1,
                                 'rotation_degrees': 0, 'compositing': 'Pillow RGBA alpha_composite',
                                 'partial_alpha_pixels': int(((alpha > 0) & (alpha < 255)).sum())})
        boards[filename] = board.convert('RGB')
    for old in legacy:
        old_paint = raster(PRODUCTION / old['export'], (FIELD_SIZE, FIELD_SIZE))
        require(old_paint.getchannel('A').getextrema() == (255, 255), 'Legacy field must be filled')
        for direction in ('vertical', 'horizontal'):
            horizontal = direction == 'horizontal'
            size = (900, 1240) if horizontal else (1100, 1040)
            board, draw = canvas(size, f'{old["id"]} / six direct {direction} joins',
                                 'Exact 1:1 / legacy first, new field second / no connector, feathering or correction')
            filename = f'review/{old["id"].split(".")[1]}-{direction}-1x.png'
            for index, field in enumerate(fields):
                x = 20 + index % (3 if horizontal else 2) * (294 if horizontal else 540)
                y = 90 + index // (3 if horizontal else 2) * (580 if horizontal else 310)
                new_paint = repeated_field(field)
                new_xy = (x, y + FIELD_SIZE) if horizontal else (x + FIELD_SIZE, y)
                place(board, old_paint, (x, y))
                place(board, new_paint, new_xy)
                metrics = edge_metrics(old_paint, new_paint)[direction]
                label_y = y + (FIELD_SIZE * 2 if horizontal else FIELD_SIZE) + 8
                draw.text((x, label_y), field['id'], font=font, fill=INK)
                draw.text((x, label_y + 22), f'RGB mean {metrics["rgb_mean_delta"]}; max {metrics["rgb_max_delta"]}',
                          font=font, fill=INK)
                joins.append({'legacy': old['id'], 'field': field['id'], 'direction': direction,
                              'board': filename, 'legacy_origin': [x, y], 'field_origin': list(new_xy),
                              'legacy_source': str((PRODUCTION / old['export']).relative_to(REPO)),
                              'field_tile': str((TILING / field['export']).relative_to(REPO)),
                              'legacy_size': [FIELD_SIZE, FIELD_SIZE], 'field_size': [FIELD_SIZE, FIELD_SIZE],
                              'field_repeat': [2, 2], 'scale': 1, 'rotation_degrees': 0, **metrics})
            boards[filename] = board.convert('RGB')
    report = {'schema_version': 1, 'date': '2026-10-04', 'timezone': 'Australia/Sydney',
              'scope': 'Read-only external ground compatibility; no artwork or runtime changes',
              'inputs': bound, 'overlay_combinations': overlay_rows, 'direct_joins': joins,
              'counts': {'overlay_combinations': 36, 'legacy_pairs': 12, 'join_directions': 24,
                         'boards': 10, 'preserved_accepted_artwork': 102, 'preserved_terrain_exports': 35},
              'interpretation': 'Direct edge inequality is retained evidence, not automatic visual failure or a requirement for every material pairing. Centred overlays sample one phase only.'}
    return boards, report


def main():
    """Write only this audit, or verify it without mutating any persisted output."""
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--check', action='store_true', help='Recompute and compare saved evidence without writing')
    args = parser.parse_args()
    boards, report = render()
    if args.check:
        saved = read_json(ROOT / 'audit.json')
        for filename, expected in boards.items():
            path = ROOT / filename
            actual = raster(path)
            require(actual.size == expected.size and
                    np.array_equal(np.asarray(actual.convert('RGB')), np.asarray(expected)),
                    f'Board pixels changed: {filename}')
            require(digest(path) == saved['boards'][filename], f'Board bytes changed: {filename}')
        report['boards'] = saved['boards']
        require(set(report['boards']) == set(boards), 'Board inventory changed')
        require(report == saved, 'Audit metadata or input bindings changed')
    else:
        (ROOT / 'review').mkdir(parents=True, exist_ok=True)
        for filename, board in boards.items():
            board.save(ROOT / filename)
        report['boards'] = {filename: digest(ROOT / filename) for filename in boards}
        (ROOT / 'audit.json').write_text(json.dumps(report, indent=2) + '\n')
    print('PASS: 36 overlay combinations, 12 legacy pairs in both directions, 10 exact 1:1 boards; frozen inputs intact.')


if __name__ == '__main__':
    main()
