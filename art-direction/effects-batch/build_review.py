#!/usr/bin/env python3
"""Package external effects artwork without repainting or changing runtime inputs."""

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
    'weather': ['Light rain', 'Dense rain', 'Puddle ripple', 'Low mist', 'Leaf gust', 'Snow flurry'],
    'lighting': ['Sunlight shaft', 'Moonlight shaft', 'Amber light pool', 'Dappled light', 'Dappled shade', 'Warm bloom'],
    'discovery': ['Single glint', 'Glint cluster', 'Inspection ring', 'Dust motes', 'Ripple ring', 'Attention arc'],
    'treatment': ['Vapor wisp', 'Leaf motes', 'Warm glow', 'Cream ripple', 'Rising motes', 'Particle arc'],
    'disease-state': ['Grey haze', 'Ochre pulse', 'Mauve motes', 'Chill wisp', 'Heat shimmer', 'Falling particles'],
}
# Provisional static bounds; not runtime dimensions or semantic mappings.
SIZES = {family: [(144, 144)] * 6 for family in FAMILIES}
# Measured gutters, preserving all pixels with alpha >= 8.
CELLS = {
    'weather': [(0, 0, 514, 518), (514, 0, 1004, 518), (1004, 0, 1536, 518),
                (0, 518, 528, 1024), (528, 518, 1038, 1024), (1038, 518, 1536, 1024)],
    'lighting': [(0, 0, 554, 533), (554, 0, 1043, 533), (1043, 0, 1536, 533),
                 (0, 533, 531, 1024), (531, 533, 1061, 1024), (1061, 533, 1536, 1024)],
    'treatment': [(0, 0, 512, 509), (512, 0, 1024, 509), (1024, 0, 1536, 509),
                  (0, 509, 512, 1024), (512, 509, 1024, 1024), (1024, 509, 1536, 1024)],
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
        raise ValueError(f'Empty effect component: {asset_id}')
    edges = [crop.crop((0, 0, crop.width, 1)), crop.crop((0, crop.height - 1, crop.width, crop.height)),
             crop.crop((0, 0, 1, crop.height)), crop.crop((crop.width - 1, 0, crop.width, crop.height))]
    if any(edge.getchannel('A').getextrema()[1] >= CROP_ALPHA_THRESHOLD for edge in edges):
        raise ValueError(f'Visible paint crosses source cell: {asset_id}; inspect gutters')
    left, top, right, bottom = bounds
    if (left < CROP_MARGIN or top < CROP_MARGIN or
            right + CROP_MARGIN > crop.width or bottom + CROP_MARGIN > crop.height):
        raise ValueError(f'Insufficient full crop margin: {asset_id}; inspect gutters')
    bounds = (left - CROP_MARGIN, top - CROP_MARGIN, right + CROP_MARGIN, bottom + CROP_MARGIN)
    outside = alpha.copy()
    outside.paste(0, bounds)
    excluded = {'pixel_count': sum(outside.histogram()[1:]), 'max_alpha': outside.getextrema()[1]}
    if excluded['max_alpha'] >= CROP_ALPHA_THRESHOLD:
        raise ValueError(f'Crop excludes visible paint: {asset_id}')
    return crop.crop(bounds), list(bounds), excluded


def make_compositions(sprites):
    """Show all exports at 1:1 on both grounds and representative groups."""
    placements = {}
    for title, ground, ink in [('light-scale', '#e8e3d5', '#1a201d'),
                               ('dark-scale', '#424a40', '#f0ecdf')]:
        canvas = Image.new('RGBA', (1280, 1000), ground)
        draw = ImageDraw.Draw(canvas)
        draw.text((24, 10), 'Effects / all candidates at actual proposed export scale', fill=ink, font=FONT)
        draw.text((24, 42), 'Static artwork only; timing, anchors and semantics remain provisional.', fill=ink, font=SMALL)
        for row, family in enumerate(FAMILIES):
            draw.text((20, 92 + row * 180), family.upper(), fill=ink, font=SMALL)
            for index in range(6):
                asset_id = f'{family}-{index + 1:02}'
                sprite = sprites[asset_id]
                x, y = 185 + index * 180, 78 + row * 180
                if x + sprite.width > canvas.width or y + sprite.height > canvas.height:
                    raise ValueError(f'Composition clips {asset_id}')
                canvas.alpha_composite(sprite, (x, y))
                draw.text((x, y + 158), asset_id, fill=ink, font=SMALL)
                placements[asset_id] = [x, y]
        canvas.convert('RGB').save(ROOT / 'review' / f'{title}.png')
    canvas = Image.new('RGBA', (1280, 720), '#424a40')
    draw = ImageDraw.Draw(canvas)
    draw.text((24, 14), 'Representative effect groups / actual export scale', fill='#f0ecdf', font=FONT)
    draw.text((24, 48), 'Abstract light/dark grounds; no runtime scene or efficacy assertion.', fill='#f0ecdf', font=SMALL)
    groups = [('Rain and low light', ['weather-01', 'weather-03', 'lighting-02']),
              ('Mist and emphasis', ['weather-04', 'discovery-01', 'discovery-03']),
              ('Vapor and motes', ['treatment-01', 'treatment-05', 'lighting-03']),
              ('Haze and cool light', ['disease-state-01', 'disease-state-04', 'lighting-05'])]
    grouped = []
    for index, (label, ids) in enumerate(groups):
        column, row = index % 2, index // 2
        x, y = 24 + column * 630, 100 + row * 300
        draw.rectangle((x, y, x + 605, y + 265), fill='#e8e3d5' if row == 0 else '#303831')
        draw.text((x + 12, y + 12), label, fill='#1a201d' if row == 0 else '#f0ecdf', font=FONT)
        for offset, asset_id in enumerate(ids):
            position = (x + 22 + offset * 185, y + 70)
            canvas.alpha_composite(sprites[asset_id], position)
            draw.text((position[0], y + 230), asset_id, fill='#1a201d' if row == 0 else '#f0ecdf', font=SMALL)
            grouped.append({'id': asset_id, 'position': list(position)})
    canvas.convert('RGB').save(ROOT / 'review' / 'grouped-scale.png')
    return {'canvases': {'light-scale': [1280, 1000], 'dark-scale': [1280, 1000], 'grouped-scale': [1280, 720]},
            'placements': placements, 'grouped_placements': grouped, 'units': 'pixels at 1:1 export scale',
            'status': 'proposed only; static effects, timing and anchors untested'}


def main():
    """Build the complete five-family candidate package and hash its review inputs."""
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
        draw.text((24, 14), f'{family.upper()} / effects artwork candidates', fill='#1a201d', font=FONT)
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
    inventory = {'status': 'owner-approved source sheets; derivative review packaging', 'generator': 'built-in image_gen',
                 'asset_count': len(assets), 'assets': assets, 'scale_reference': reference,
                 'crop_policy': {'alpha_threshold': CROP_ALPHA_THRESHOLD, 'margin_pixels': CROP_MARGIN,
                                 'original_sources_preserved': True},
                 'notes': 'No state semantics, animation, scenario facts or runtime authority.'}
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
