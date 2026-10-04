#!/usr/bin/env python3
"""Package external UI artwork without repainting or changing runtime inputs."""

from io import BytesIO
from pathlib import Path
import hashlib
import json
import zipfile
from xml.etree.ElementTree import Element, SubElement, tostring

from PIL import Image, ImageDraw, ImageFont

ROOT = Path(__file__).resolve().parent
REPO = ROOT.parents[1]
BACKGROUND = '#e8e3d5'
FONT_PATH = '/System/Library/Fonts/Supplemental/Arial.ttf'
FONT = ImageFont.truetype(FONT_PATH, 22)
SMALL = ImageFont.truetype(FONT_PATH, 17)
PADDING = 4
CROP_ALPHA_THRESHOLD = 8
CROP_MARGIN = 8
FAMILIES = {
    'journal': ['Open journal', 'Entry card', 'Moss bookmark', 'Ochre bookmark', 'Previous page', 'Next page'],
    'map': ['Blank map panel', 'Legend card', 'Compass', 'Location pin', 'Zoom in', 'Zoom out'],
    'inventory': ['Inventory panel', 'Empty slot', 'Selected slot', 'Description card', 'Category tab', 'Close'],
    'dialogue': ['Dialogue panel', 'Entry field', 'Portrait surround', 'Response', 'Focused response', 'Waiting dots'],
    'settings': ['Settings panel', 'Neutral button', 'Focused button', 'Slider', 'Unchecked', 'Checked'],
    'provider-state': ['Disconnected', 'Connecting', 'Ready', 'Waiting', 'Failed', 'Locked'],
}
# Maximum painted bounds in the proposed 1280x720 review; never runtime dimensions.
SIZES = {
    'journal': [(380, 270), (120, 160), (40, 64), (40, 64), (44, 44), (44, 44)],
    'map': [(380, 270), (120, 160), (64, 64), (44, 64), (44, 44), (44, 44)],
    'inventory': [(380, 270), (60, 60), (60, 60), (350, 90), (140, 40), (44, 44)],
    'dialogue': [(380, 160), (380, 44), (88, 88), (150, 44), (150, 44), (48, 32)],
    'settings': [(380, 270), (150, 44), (150, 44), (280, 28), (36, 36), (36, 36)],
    'provider-state': [(350, 64)] * 6,
}
# Measured transparent gutters; generated panels do not follow equal-width cells.
CELLS = {
    'journal': [(0, 0, 720, 520), (720, 0, 1160, 520), (1160, 0, 1536, 520),
                (0, 520, 512, 1024), (512, 520, 1024, 1024), (1024, 520, 1536, 1024)],
    'map': [(0, 0, 740, 560), (740, 0, 1110, 560), (1110, 0, 1536, 560),
            (0, 560, 512, 1024), (512, 560, 1024, 1024), (1024, 560, 1536, 1024)],
    'inventory': [(0, 0, 715, 550), (715, 0, 1120, 550), (1120, 0, 1536, 550),
                  (0, 550, 640, 1024), (640, 550, 1200, 1024), (1200, 550, 1536, 1024)],
    'dialogue': [(0, 0, 570, 580), (570, 0, 1140, 580), (1140, 0, 1536, 580),
                 (0, 580, 580, 1024), (580, 580, 1170, 1024), (1170, 580, 1536, 1024)],
}


def digest(path):
    """Hash the exact persisted bytes."""
    return hashlib.sha256(path.read_bytes()).hexdigest()


def png_bytes(image):
    """Encode a raster layer for OpenRaster."""
    buffer = BytesIO()
    image.save(buffer, format='PNG')
    return buffer.getvalue()


def write_master(image, path, name):
    """Store paint and an empty correction layer with deterministic ZIP timestamps."""
    xml = Element('image', w=str(image.width), h=str(image.height), name=name, version='0.0.3')
    stack = SubElement(xml, 'stack')
    for title, source in [('Corrections', 'data/corrections.png'), ('Source paint', 'data/source.png')]:
        SubElement(stack, 'layer', name=title, src=source, x='0', y='0', opacity='1.0',
                   visibility='visible', **{'composite-op': 'svg:src-over'})
    with zipfile.ZipFile(path, 'w') as archive:
        for member, data in [('mimetype', b'image/openraster'), ('stack.xml', tostring(xml)),
                             ('data/source.png', png_bytes(image)),
                             ('data/corrections.png', png_bytes(Image.new('RGBA', image.size))),
                             ('mergedimage.png', png_bytes(image))]:
            info = zipfile.ZipInfo(member, date_time=(2026, 10, 4, 0, 0, 0))
            archive.writestr(info, data, compress_type=zipfile.ZIP_STORED)


def cells(sheet):
    """Partition the entire sheet, including fractional grid remainders."""
    return [(column * sheet.width // 3, row * sheet.height // 2,
             (column + 1) * sheet.width // 3, (row + 1) * sheet.height // 2)
            for row in range(2) for column in range(3)]


def paint_from_cell(sheet, cell, asset_id):
    """Crop distant faint speckles while retaining a margin around all alpha >= 8."""
    crop = sheet.crop(cell)
    alpha = crop.getchannel('A')
    bounds = alpha.point(lambda value: 255 if value >= CROP_ALPHA_THRESHOLD else 0).getbbox()
    if bounds is None:
        raise ValueError(f'Empty UI component: {asset_id}')
    edges = [crop.crop((0, 0, crop.width, 1)), crop.crop((0, crop.height - 1, crop.width, crop.height)),
             crop.crop((0, 0, 1, crop.height)), crop.crop((crop.width - 1, 0, crop.width, crop.height))]
    if any(edge.getchannel('A').getextrema()[1] >= CROP_ALPHA_THRESHOLD for edge in edges):
        raise ValueError(f'Visible paint crosses source cell: {asset_id}; inspect gutters')
    left, top, right, bottom = bounds
    bounds = (max(0, left - CROP_MARGIN), max(0, top - CROP_MARGIN),
              min(crop.width, right + CROP_MARGIN), min(crop.height, bottom + CROP_MARGIN))
    outside = alpha.copy()
    outside.paste(0, bounds)
    excluded = {'pixel_count': sum(outside.histogram()[1:]), 'max_alpha': outside.getextrema()[1]}
    if excluded['max_alpha'] >= CROP_ALPHA_THRESHOLD:
        raise ValueError(f'Crop excludes visible paint: {asset_id}')
    return crop.crop(bounds), list(bounds), excluded


def make_compositions(sprites):
    """Show every actual export once, on contrasting grounds at proposed screen scale."""
    layouts = {
        'journal': [(16, 100), (56, 395), (80, 570), (150, 570), (100, 650), (220, 650)],
        'map': [(16, 100), (56, 395), (70, 570), (190, 570), (100, 650), (220, 650)],
        'inventory': [(16, 100), (80, 412), (190, 412), (31, 508), (60, 630), (310, 630)],
        'dialogue': [(16, 120), (16, 330), (150, 428), (36, 568), (220, 568), (175, 646)],
        'settings': [(16, 100), (36, 420), (220, 420), (66, 540), (100, 630), (250, 630)],
        'provider-state': [(30, 104 + index * 96) for index in range(6)],
    }
    for title, families, ground in [
        ('research-scale', ['journal', 'map', 'inventory'], '#e8e3d5'),
        ('conversation-scale', ['dialogue', 'settings', 'provider-state'], '#424a40'),
    ]:
        canvas = Image.new('RGBA', (1280, 720), ground)
        draw = ImageDraw.Draw(canvas)
        ink = '#1a201d' if title == 'research-scale' else '#f0ecdf'
        draw.text((24, 10), f'{title} / actual proposed export scale / 1280 x 720', fill=ink, font=FONT)
        draw.text((24, 40), 'External artwork composition. Blank fields; no functional UI or scenario content.',
                  fill=ink, font=SMALL)
        rectangles = []
        for column, family in enumerate(families):
            offset = 16 + column * 420
            draw.text((offset + 16, 76), family.upper(), fill=ink, font=SMALL)
            for index, (x, y) in enumerate(layouts[family]):
                sprite = sprites[f'{family}-{index + 1:02}']
                if offset + x < 0 or y < 0 or offset + x + sprite.width > 1280 or y + sprite.height > 720:
                    raise ValueError(f'Composition clips {family}-{index + 1:02}')
                rectangle = (offset + x, y, offset + x + sprite.width, y + sprite.height)
                for previous in rectangles:
                    if (rectangle[0] < previous[2] and previous[0] < rectangle[2] and
                            rectangle[1] < previous[3] and previous[1] < rectangle[3]):
                        raise ValueError(f'Composition overlaps {family}-{index + 1:02}')
                rectangles.append(rectangle)
                canvas.alpha_composite(sprite, (offset + x, y))
        canvas.convert('RGB').save(ROOT / 'review' / f'{title}.png')
    return {'canvas': [1280, 720], 'units': 'pixels at 1:1 export scale',
            'status': 'proposed only; responsive layout and accessibility untested'}


def main():
    """Build the complete six-family candidate package and hash its review inputs."""
    if (ROOT / 'owner-acceptance.json').exists():
        raise ValueError('Approved package is frozen; preserve it and author a separate revision')
    for folder in ['masters', 'exports', 'review']:
        (ROOT / folder).mkdir(exist_ok=True)
    assets, sprites = [], {}
    for family, names in FAMILIES.items():
        sheet = Image.open(ROOT / 'sources' / f'{family}.png')
        if sheet.mode != 'RGBA' or sheet.getchannel('A').getextrema()[0] != 0:
            raise ValueError(f'Expected transparent RGBA source: {family}')
        board = Image.new('RGB', (1536, 1160), BACKGROUND)
        draw = ImageDraw.Draw(board)
        draw.text((24, 14), f'{family.upper()} / UI artwork candidates', fill='#1a201d', font=FONT)
        draw.text((24, 45), 'Enlarged detail; actual proposed scale shown separately.', fill='#454a40', font=SMALL)
        for index, name in enumerate(names):
            asset_id = f'{family}-{index + 1:02}'
            cell = CELLS.get(family, cells(sheet))[index]
            paint, bounds, excluded = paint_from_cell(sheet, cell, asset_id)
            limit_width, limit_height = SIZES[family][index]
            ratio = min(limit_width / paint.width, limit_height / paint.height)
            width, height = max(1, round(paint.width * ratio)), max(1, round(paint.height * ratio))
            resized = paint.resize((width * 2, height * 2), Image.Resampling.LANCZOS)
            master = Image.new('RGBA', (width * 2 + PADDING * 4, height * 2 + PADDING * 4))
            master.alpha_composite(resized, (PADDING * 2, PADDING * 2))
            export = master.resize((width + PADDING * 2, height + PADDING * 2), Image.Resampling.LANCZOS)
            master_path = ROOT / 'masters' / f'{asset_id}.ora'
            export_path = ROOT / 'exports' / f'{asset_id}.png'
            write_master(master, master_path, name)
            export.save(export_path)
            sprites[asset_id] = export
            assets.append({'id': asset_id, 'name': name, 'source': f'sources/{family}.png',
                           'source_cell': list(cell), 'source_crop_in_cell': bounds, 'excluded_faint_pixels': excluded,
                           'master': f'masters/{asset_id}.ora', 'export': f'exports/{asset_id}.png',
                           'size': list(export.size), 'master_size': list(master.size),
                           'proposed_pivot': [0, 0],
                           'pivot_status': 'provisional', 'sha256': digest(export_path),
                           'master_sha256': digest(master_path)})
            preview = paint.copy()
            preview.thumbnail((450, 425), Image.Resampling.LANCZOS)
            row, column = divmod(index, 3)
            x, y = column * 512, 90 + row * 530
            board.paste(preview, (x + (512 - preview.width) // 2, y + (425 - preview.height) // 2), preview)
            draw.text((x + 16, y + 470), f'{asset_id}: {name}', fill='#1a201d', font=SMALL)
        board.save(ROOT / 'review' / f'{family}.png')
    reference = make_compositions(sprites)
    inventory = {'status': 'awaiting collective owner artwork review', 'generator': 'built-in image_gen',
                 'asset_count': len(assets), 'assets': assets, 'scale_reference': reference,
                 'crop_policy': {'alpha_threshold': CROP_ALPHA_THRESHOLD, 'margin_pixels': CROP_MARGIN,
                                 'original_sources_preserved': True},
                 'notes': 'Blank content fields; no functional UI, scenario facts or runtime authority.'}
    (ROOT / 'inventory.json').write_text(json.dumps(inventory, indent=2) + '\n')
    files = sorted(path for folder in ['sources', 'exports', 'masters', 'review', 'evidence']
                   for path in (ROOT / folder).iterdir() if path.is_file())
    files += [ROOT / 'inventory.json', ROOT / 'prompts.json', Path(__file__),
              ROOT / 'inspect_package.py', ROOT / 'README.md']
    (ROOT / 'review-manifest.json').write_text(json.dumps({
        'status': inventory['status'], 'files': {str(path.relative_to(ROOT)): digest(path) for path in files},
    }, indent=2) + '\n')


if __name__ == '__main__':
    main()
