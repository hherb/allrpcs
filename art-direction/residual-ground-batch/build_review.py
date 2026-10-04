#!/usr/bin/env python3
"""Package external residual ground artwork without repainting or changing runtime inputs."""

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
    'ground-surfaces': ['Packed earth', 'Pale gravel', 'Mixed meadow', 'Sparse dry grass', 'Dark garden soil', 'Damp mud'],
    'ground-variation': ['Pebble scatter', 'Gravel scatter', 'Damp soil patch', 'Sparse grass patch', 'Worn earth patch', 'Moss flecks'],
    'surface-transitions': ['Earth/gravel straight', 'Earth/gravel outer corner', 'Earth/gravel inset corner',
                            'Grass/soil straight', 'Grass/soil outer corner', 'Grass/soil inset corner'],
}
SIZES = {family: [(128, 128)] * 6 for family in FAMILIES}
CELLS = {}


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
        canvas = Image.new('RGBA', (1500, 680), ground)
        draw = ImageDraw.Draw(canvas)
        draw.text((24, 10), 'Residual ground / all candidates at actual proposed export scale', fill=ink, font=FONT)
        draw.text((24, 42), 'External candidates; seams, tileability, anchors and placement remain provisional.', fill=ink, font=SMALL)
        for row, family in enumerate(FAMILIES):
            draw.text((20, 92 + row * 190), family.upper(), fill=ink, font=SMALL)
            for index in range(6):
                asset_id = f'{family}-{index + 1:02}'
                sprite = sprites[asset_id]
                x, y = 300 + index * 190, 78 + row * 190
                if x + sprite.width > canvas.width or y + sprite.height > canvas.height:
                    raise ValueError(f'Composition clips {asset_id}')
                canvas.alpha_composite(sprite, (x, y))
                draw.text((x, y + 142), asset_id, fill=ink, font=SMALL)
                placements[asset_id] = [x, y]
        canvas.convert('RGB').save(ROOT / 'review' / f'{title}.png')
    canvas = Image.new('RGBA', (1280, 720), '#424a40')
    draw = ImageDraw.Draw(canvas)
    draw.text((24, 14), 'Representative ground groups / actual export scale', fill='#f0ecdf', font=FONT)
    draw.text((24, 48), 'Abstract light/dark grounds; no runtime scene or placement approval.', fill='#f0ecdf', font=SMALL)
    groups = [('Earth, gravel and scatter', ['ground-surfaces-01', 'ground-surfaces-02', 'ground-variation-01']),
              ('Meadow, soil and grass', ['ground-surfaces-03', 'ground-surfaces-05', 'ground-variation-04']),
              ('Earth and gravel transitions', ['surface-transitions-01', 'surface-transitions-02', 'surface-transitions-03']),
              ('Grass and soil transitions', ['surface-transitions-04', 'surface-transitions-05', 'surface-transitions-06'])]
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
    return {'canvases': {'light-scale': [1500, 680], 'dark-scale': [1500, 680], 'grouped-scale': [1280, 720]},
            'placements': placements, 'grouped_placements': grouped, 'units': 'pixels at 1:1 export scale',
            'status': 'proposed only; ground candidate seams, tileability and anchors untested'}


def main():
    """Build the complete three-family candidate package and hash its review inputs."""
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
        draw.text((24, 14), f'{family.upper()} / residual ground artwork candidates', fill='#1a201d', font=FONT)
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
    inventory = {'status': 'external candidate package; collective owner review pending', 'generator': 'built-in image_gen',
                 'asset_count': len(assets), 'assets': assets, 'scale_reference': reference,
                 'crop_policy': {'alpha_threshold': CROP_ALPHA_THRESHOLD, 'margin_pixels': CROP_MARGIN,
                                 'original_sources_preserved': True},
                 'notes': 'Generic ground material candidates only; seams, tileability and runtime suitability unverified.'}
    (ROOT / 'inventory.json').write_text(json.dumps(inventory, indent=2) + '\n')
    files = sorted(path for folder in ['sources', 'exports', 'masters', 'review', 'evidence']
                   for path in (ROOT / folder).glob('*') if path.is_file())
    files += [ROOT / 'inventory.json', ROOT / 'prompts.json', Path(__file__),
              ROOT / 'inspect_package.py', ROOT / 'README.md']
    (ROOT / 'review-manifest.json').write_text(json.dumps({
        'status': inventory['status'], 'files': {str(path.relative_to(ROOT)): digest(path) for path in files},
    }, indent=2) + '\n')


if __name__ == '__main__':
    main()
