#!/usr/bin/env python3
"""Package generated artwork as editable modules and assemble batch review images.

Mechanical cropping, export scaling and review layout only; retain source RGBA.
No alpha cleanup, repainting, runtime import or product validation occurs here.
"""
from io import BytesIO
from collections import deque
from pathlib import Path
import hashlib
import json
import zipfile
from xml.etree.ElementTree import Element, SubElement, tostring

from PIL import Image, ImageDraw, ImageFont

ROOT = Path(__file__).resolve().parent
FONT_PATH = '/System/Library/Fonts/Supplemental/Arial.ttf'
FONT = ImageFont.truetype(FONT_PATH, 22)
SMALL = ImageFont.truetype(FONT_PATH, 17)
FAMILIES = {
    'trees': ['Mature oak', 'Birch', 'Upright beech', 'Young oak', 'Weathered tree', 'Sapling'],
    'shrubs': ['Rounded shrub', 'Hazel bush', 'Spreading bramble', 'Dark shrub', 'Twiggy shrub',
               'Leaning bush', 'Young shoots', 'Shade bush', 'Sparse scrub'],
    'fallen-wood': ['Broken log', 'End-grain log', 'Hollow log', 'Broad stump', 'Jagged stump',
                    'Root snag', 'Forked branch', 'Curved branch', 'Stick cluster'],
    'rocks': ['Mossy boulder', 'Angular boulder', 'Flat stone', 'Split rock', 'Smooth stone',
              'Rock pair', 'Stone cluster', 'Pebble scatter', 'Small crag'],
    'environmental-details': ['Small fern', 'Broad fern', 'Dry grass', 'Fresh grass',
                              'Leaf cluster', 'Leaf scatter', 'Moss cushion', 'Mushrooms', 'Groundcover'],
}
# Proposed visible heights at 1280x720; review choices, not approved runtime geometry.
HEIGHTS = {
    'trees': [420, 400, 440, 280, 310, 260],
    'shrubs': [88, 140, 80, 90, 110, 130, 72, 80, 56],
    'fallen-wood': [92, 100, 72, 90, 150, 90, 64, 64, 40],
    'rocks': [140, 160, 72, 120, 100, 90, 72, 40, 64],
    'environmental-details': [56, 72, 48, 44, 22, 24, 32, 36, 28],
}
CELLS = {
    'trees': [(20,0,650,584),(640,0,1030,584),(1056,0,1536,584),
              (80,584,550,1024),(580,584,1056,1024),(1100,584,1536,1024)],
    'shrubs': [(0,0,475,365),(475,0,950,367),(940,0,1536,365),
               (0,365,475,690),(475,367,950,690),(960,365,1536,700),
               (0,690,475,1024),(475,695,950,1024),(960,705,1536,1024)],
    'fallen-wood': [(0,0,555,370),(570,0,1068,360),(1090,0,1536,370),
                    (0,380,535,700),(550,330,1030,704),(1040,380,1536,704),
                    (0,710,520,1024),(530,710,1060,1024),(1080,710,1536,1024)],
    'rocks': [(0,0,512,370),(512,0,1024,370),(1024,0,1536,370),
              (0,375,512,715),(512,375,1024,715),(1024,375,1536,715),
              (0,730,512,1024),(512,730,1024,1024),(1024,730,1536,1024)],
    'environmental-details': [(0,0,512,375),(520,0,1050,375),(1090,0,1536,375),
                              (0,400,512,700),(520,400,1024,700),(1024,400,1536,700),
                              (0,695,512,1024),(512,710,1024,1024),(1024,710,1536,1024)],
}


def isolate_connected_module(crop):
    """Separate a tree/bush/wood module from neighboring sheet-cell intrusions."""
    w, h = crop.size
    mask = bytearray(1 if a else 0 for a in crop.getchannel('A').tobytes())
    largest = []
    for k in range(w*h):
        if not mask[k]:
            continue
        mask[k] = 0
        queue, component = deque([k]), []
        while queue:
            v = queue.popleft()
            component.append(v)
            y, x = divmod(v, w)
            adjacent = ([v-1] if x else []) + ([v+1] if x<w-1 else [])
            adjacent += ([v-w] if y else []) + ([v+w] if y<h-1 else [])
            for n in adjacent:
                if mask[n]:
                    mask[n] = 0
                    queue.append(n)
        if len(component) > len(largest):
            largest = component
    selector = bytearray(w*h)
    for v in largest:
        selector[v] = 255
    return Image.composite(crop, Image.new('RGBA', crop.size), Image.frombytes('L', crop.size, bytes(selector)))


def png_bytes(im):
    buf = BytesIO()
    im.save(buf, format='PNG')
    return buf.getvalue()


def write_master(im, path, name):
    """OpenRaster with preserved source paint and a separate blank correction layer."""
    xml = Element('image', w=str(im.width), h=str(im.height), name=name, version='0.0.3')
    stack = SubElement(xml, 'stack')
    for title, src in [('Corrections', 'data/corrections.png'), ('Source paint', 'data/source.png')]:
        SubElement(stack, 'layer', name=title, src=src, x='0', y='0', opacity='1.0',
                   visibility='visible', **{'composite-op': 'svg:src-over'})
    with zipfile.ZipFile(path, 'w') as out:
        out.writestr('mimetype', 'image/openraster', compress_type=zipfile.ZIP_STORED)
        out.writestr('stack.xml', tostring(xml))
        out.writestr('data/source.png', png_bytes(im))
        out.writestr('data/corrections.png', png_bytes(Image.new('RGBA', im.size)))
        out.writestr('mergedimage.png', png_bytes(im))


def main():
    for folder in ['masters', 'exports', 'review']:
        (ROOT / folder).mkdir(exist_ok=True)
    assets = []
    sprites = {}
    for family, names in FAMILIES.items():
        source = ROOT / 'sources' / f'{family}.png'
        sheet = Image.open(source).convert('RGBA')
        board = Image.new('RGB', (1536, 1160), '#e8e3d5')
        draw = ImageDraw.Draw(board)
        draw.text((24, 14), f'{family.upper()} — batch 01 / artwork series', fill='#1a201d', font=FONT)
        draw.text((24, 45), 'Enlarged artwork for detail review. Proposed game scale appears in the compositions.',
                  fill='#454a40', font=SMALL)
        for i, name in enumerate(names):
            row, col = divmod(i, 3)
            crop = sheet.crop(CELLS[family][i])
            if family in ('trees', 'shrubs', 'fallen-wood'):
                crop = isolate_connected_module(crop)
            bounds = crop.getchannel('A').getbbox()
            paint = crop.crop(bounds)
            wanted = HEIGHTS[family][i]
            width = max(2, round(paint.width / paint.height * wanted))
            # Exact 2x master dimensions; the generated source remains untouched.
            master = paint.resize((width * 2, wanted * 2), Image.Resampling.LANCZOS)
            export = master.resize((width, wanted), Image.Resampling.LANCZOS)
            asset_id = f'{family}-{i+1:02d}'
            master_path = ROOT / 'masters' / f'{asset_id}.ora'
            export_path = ROOT / 'exports' / f'{asset_id}.png'
            write_master(master, master_path, name)
            export.save(export_path)
            sprites[asset_id] = export
            assets.append({'id': asset_id, 'name': name, 'source': str(source.relative_to(ROOT)),
                           'source_cell': CELLS[family][i],
                           'source_alpha_bounds_in_cell': bounds, 'master': str(master_path.relative_to(ROOT)),
                           'export': str(export_path.relative_to(ROOT)), 'size': list(export.size),
                           'proposed_pivot': [width//2, wanted-1], 'pivot_status': 'provisional',
                           'sha256': hashlib.sha256(export_path.read_bytes()).hexdigest()})
            cell_h = 530 if family == 'trees' else 350
            x, y = col * 512, 88 + row * cell_h
            preview = paint.copy()
            preview.thumbnail((464, cell_h-65), Image.Resampling.LANCZOS)
            board.paste(preview, (x + (512-preview.width)//2, y), preview)
            draw.text((x+18, y+cell_h-48), f'{asset_id}: {name}', fill='#1a201d', font=SMALL)
        board.save(ROOT / 'review' / f'{family}.png')

    for composition, placements in {
        'woodland-edge': [('trees-01',280,570), ('trees-02',1040,520), ('trees-04',1130,690),
                          ('shrubs-02',160,650), ('shrubs-03',430,580), ('rocks-01',920,660),
                          ('fallen-wood-01',580,680), ('environmental-details-01',850,650),
                          ('environmental-details-04',420,660), ('environmental-details-06',670,560)],
        'quiet-understory': [('trees-05',250,470), ('trees-06',1110,460), ('shrubs-05',460,460),
                             ('shrubs-08',980,570), ('fallen-wood-03',650,470), ('fallen-wood-05',340,630),
                             ('rocks-03',780,610), ('rocks-07',1130,660),
                             ('environmental-details-02',180,650), ('environmental-details-07',620,620),
                             ('environmental-details-08',880,530), ('environmental-details-09',460,650)],
    }.items():
        canvas = Image.new('RGBA', (1280,720), '#8b9568')
        for asset_id, x, y in sorted(placements, key=lambda item: item[2]):
            sprite = sprites[asset_id]
            canvas.alpha_composite(sprite, (x-sprite.width//2, y-sprite.height))
        draw = ImageDraw.Draw(canvas)
        draw.text((24,18), f'{composition} / proposed game-scale arrangement', fill='#1a201d', font=FONT)
        draw.rectangle((550,68,704,240), fill='#e8e3d5')
        draw.line([(564,82),(564,226)], fill='#454a40', width=4)
        draw.text((576,105), '144 px\ncharacter\nheight', fill='#1a201d', font=SMALL)
        canvas.convert('RGB').save(ROOT / 'review' / f'{composition}.png')
    approved = (ROOT/'owner-acceptance.json').exists()
    (ROOT/'inventory.json').write_text(json.dumps({'status':'owner-approved artwork' if approved else 'awaiting collective artwork review',
        'generator':'built-in image_gen', 'asset_count':len(assets),
        'validation':'See validation.json for consolidated checks; runtime imports remain out of scope.' if approved else 'deferred until owner feedback',
        'notes':'Source paint retained; ORA correction layers blank; scale and pivots provisional.',
        'assets':assets}, indent=2)+'\n')
    print(f'Packaged {len(assets)} modules, 42 OpenRaster masters, five family boards and two compositions.')


if __name__ == '__main__':
    main()
