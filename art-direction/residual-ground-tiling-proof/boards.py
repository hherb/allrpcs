#!/usr/bin/env python3
"""Compose labelled native-master and exact 1:1 tile/connector review boards."""

from PIL import Image, ImageDraw, ImageFont
from build import ROOT, MATERIALS, PAIRS, KINDS, composite, export_image

FONT_PATH = '/System/Library/Fonts/Supplemental/Arial.ttf'
FONT = ImageFont.truetype(FONT_PATH, 23)
SMALL = ImageFont.truetype(FONT_PATH, 17)
INK = '#202820'
BACKGROUND = '#e8e3d5'


def canvas(size, title, subtitle):
    """Keep headings and labels outside every tile placement."""
    image = Image.new('RGB', size, BACKGROUND)
    draw = ImageDraw.Draw(image)
    draw.text((24, 14), title, fill=INK, font=FONT)
    draw.text((24, 48), subtitle, fill=INK, font=SMALL)
    return image, draw


def place(board, paint, position, records, asset_id, **extra):
    """Record exact displayed pixels, including supplemental oriented-mask samples."""
    x, y = position
    if x < 0 or y < 0 or x + paint.width > board.width or y + paint.height > board.height:
        raise ValueError(f'Board clips {asset_id}')
    board.paste(paint.convert('RGB'), position)
    records.append({'id': asset_id, 'position': [x, y], 'size': list(paint.size), **extra})


def build_boards(fields, masks, drafts):
    """Show the entire series together, repeated fields, convex loops and concave joins."""
    records = {}
    exports = {name: export_image(field) for name, field in fields.items()}
    for family, (a, b) in PAIRS.items():
        for kind in KINDS:
            with Image.open(ROOT / f'exports/{family}-{kind}.png') as image:
                exports[f'{family}-{kind}'] = image.copy()
    for family, names in [('surfaces', MATERIALS),
                          ('transitions', [f'{family}-{kind}' for family in PAIRS for kind in KINDS])]:
        board, draw = canvas((1140, 740), f'{family.title()} / all six candidates',
                             '2x authored master detail / complete external feasibility series / owner review pending')
        placements = []
        for index, name in enumerate(names):
            x, y = 36 + index % 3 * 370, 94 + index // 3 * 320
            if family == 'surfaces':
                detail = fields[name]
            else:
                pair, kind = name.rsplit('-', 1)
                a, b = PAIRS[pair]
                detail = composite(fields[a], fields[b], masks[kind])[0]
            place(board, detail, (x, y), placements, name, scale=2)
            draw.text((x, y + 269), name, fill=INK, font=SMALL)
        filename = f'{family}-detail.png'
        board.save(ROOT / 'review' / filename)
        records[f'review/{filename}'] = placements
    board, draw = canvas((1280, 970), 'Six surfaces / 3x3 repeats at 1:1',
                         'Exact 128x128 exports abut with no padding / finite repeat and brush continuity require visual review')
    placements = []
    evidence, evidence_draw = canvas(board.size, 'Unwrapped interior / retained failed repeat baseline',
                                    'Raw painted crops: opaque but incompatible opposite edges / never proposed for runtime')
    for index, name in enumerate(MATERIALS):
        x, y = 24 + index % 3 * 420, 90 + index // 3 * 440
        for row in range(3):
            for column in range(3):
                position = (x + column * 128, y + row * 128)
                place(board, exports[name], position, placements, name, scale=1)
                evidence.paste(drafts[name].convert('RGB'), position)
        draw.text((x, y + 396), name, fill=INK, font=SMALL)
        evidence_draw.text((x, y + 396), name, fill=INK, font=SMALL)
    board.save(ROOT / 'review' / 'surfaces-repeat-1x.png')
    evidence.save(ROOT / 'evidence' / 'unwrapped-repeat-1x.png')
    records['review/surfaces-repeat-1x.png'] = placements
    for family, (a, b) in PAIRS.items():
        # Orient only coverage. Rotating painted exports would change texture phase at their neighbours.
        tiles = {a: exports[a], b: exports[b]}
        for kind in KINDS:
            for turns in range(4):
                mask = masks[kind].rotate(90 * turns)
                name = f'{kind}-{turns}'
                tiles[name] = export_image(composite(fields[a], fields[b], mask)[0])
                if turns:
                    tiles[name].save(ROOT / 'review' / f'{family}-{kind}-orientation-{turns}.png')
        board, draw = canvas((1360, 810), f'{family} / connected convex and inset compositions at 1:1',
                             'Coverage turns in place; material paint stays world-aligned / no runtime placement or geography')
        convex = [[b] * 5,
                  [b, 'outer-2', 'straight-1', 'outer-1', b],
                  [b, 'straight-2', a, 'straight-0', b],
                  [b, 'outer-3', 'straight-3', 'outer-0', b],
                  [b] * 5]
        inset = [[a] * 5, [a] * 5,
                 [a, a, 'inset-0', 'straight-3', 'straight-3'],
                 [a, a, 'straight-0', b, b],
                 [a, a, 'straight-0', b, b]]
        placements = []
        for panel, (title, grid) in enumerate([('Convex / closed boundary', convex), ('Inset / concave boundary', inset)]):
            x, y = 24 + panel * 680, 106
            draw.text((x, 78), title, fill=INK, font=SMALL)
            for row, line in enumerate(grid):
                for column, name in enumerate(line):
                    if name in (a, b):
                        asset_id, orientation = name, 0
                        image_path = f'exports/{name}.png'
                    else:
                        kind, turns = name.split('-')
                        orientation = int(turns)
                        asset_id = f'{family}-{kind}'
                        image_path = (f'review/{family}-{kind}-orientation-{turns}.png' if orientation
                                      else f'exports/{asset_id}.png')
                    place(board, tiles[name], (x + column * 128, y + row * 128), placements,
                          asset_id, scale=1, orientation=orientation, image=image_path, panel=panel,
                          grid=[column, row])
        draw.text((24, 764), 'Each square is 128px. Rotated coverage samples are derived proof inputs, not additional approved modules.',
                  fill=INK, font=SMALL)
        filename = f'{family}-connected-1x.png'
        board.save(ROOT / 'review' / filename)
        records[f'review/{filename}'] = placements
    return records
