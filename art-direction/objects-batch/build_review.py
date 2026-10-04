#!/usr/bin/env python3
"""Package external object artwork without repainting or changing runtime inputs."""

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
FAMILIES = {
    'herbs': ['Broad-leaf sprig', 'Purple flowering sprig', 'White flowering sprig',
              'Drying bundle', 'Root bundle', 'Dried-leaf tray'],
    'remedies': ['Corked bottle', 'Salve pot and lid', 'Poultice bundle',
                 'Infusion cup', 'Ingredient packet', 'Crushed-paste bowl'],
    'tools': ['Mortar and pestle', 'Herb knife', 'Balance',
              'Stirring spoon', 'Straining cloth', 'Preparation board'],
    'scrolls': ['Tied scroll', 'Half-open scroll', 'Open parchment',
                'Field notebook', 'Wrapped scroll pair', 'Sealed letter'],
    'inscriptions': ['Standing tablet', 'Stone plaque', 'Hanging sign',
                     'Clay tablet', 'Stone fragment', 'Brass plaque'],
    'containers': ['Wicker basket', 'Open satchel', 'Timber chest',
                   'Storage jar', 'Open crate', 'Supply sack'],
    'clue-objects': ['Pottery shard', 'Iron key', 'Frayed cloth',
                     'Wax fragment', 'Broken strap', 'Wooden token'],
}
# Provisional world-scale canvas heights; blank fields carry no authored lore.
HEIGHTS = {
    'herbs': [38, 52, 46, 54, 38, 28], 'remedies': [32, 22, 24, 20, 22, 20],
    'tools': [30, 26, 48, 26, 22, 22], 'scrolls': [28, 34, 38, 34, 34, 28],
    'inscriptions': [72, 38, 48, 34, 30, 34], 'containers': [42, 48, 54, 58, 48, 54],
    'clue-objects': [18, 22, 24, 14, 24, 18],
}
# Optional measured source-cell overrides, added only after visual gutter review.
CELLS = {
    'herbs': [(0, 0, 512, 500), (512, 0, 1024, 500), (1024, 0, 1536, 500),
              (0, 500, 512, 1024), (512, 500, 1024, 1024), (1024, 500, 1536, 1024)],
    'scrolls': [(0, 0, 512, 512), (512, 0, 1000, 512), (1000, 0, 1536, 512),
                (0, 512, 512, 1024), (512, 512, 1000, 1024), (1000, 512, 1536, 1024)],
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
    """Retain every alpha component in a reviewed cell, including detached parts."""
    crop = sheet.crop(cell)
    bounds = crop.getchannel('A').getbbox()
    if bounds is None:
        raise ValueError(f'Empty object: {asset_id}')
    edges = [crop.crop((0, 0, crop.width, 1)), crop.crop((0, crop.height - 1, crop.width, crop.height)),
             crop.crop((0, 0, 1, crop.height)), crop.crop((crop.width - 1, 0, crop.width, crop.height))]
    if any(edge.getchannel('A').getextrema()[1] > 64 for edge in edges):
        raise ValueError(f'Substantial paint crosses source cell: {asset_id}; inspect gutters')
    return crop.crop(bounds), list(bounds)


def make_compositions(sprites):
    """Show actual export scale on light and dark grounds beside an approved character."""
    reference_path = REPO / 'art-direction/characters-batch/exports/seeker-plain-south-idle-01.png'
    reference = Image.open(reference_path).convert('RGBA')
    bounds = reference.getchannel('A').getbbox()
    reference = reference.crop(bounds)
    reference = reference.resize((round(reference.width * 144 / reference.height), 144), Image.Resampling.LANCZOS)
    for title, families, ground in [
        ('apothecary-scale', ['herbs', 'remedies', 'tools', 'containers'], '#8b9568'),
        ('investigation-scale', ['scrolls', 'inscriptions', 'clue-objects'], '#424a40'),
    ]:
        canvas = Image.new('RGBA', (1280, 720), ground)
        draw = ImageDraw.Draw(canvas)
        draw.rectangle((0, 0, 1280, 70), fill=BACKGROUND)
        draw.text((24, 12), f'{title} / actual proposed export scale', fill='#1a201d', font=FONT)
        draw.text((24, 42), 'External artwork arrangement; sizes and pivots provisional.', fill='#454a40', font=SMALL)
        canvas.alpha_composite(reference, (55, 200))
        draw.line([(45, 200), (45, 344)], fill=BACKGROUND, width=3)
        draw.text((22, 365), '144 px\ncharacter', fill=BACKGROUND, font=SMALL)
        for row, family in enumerate(families):
            baseline = 190 + row * 145
            draw.text((220, baseline - 104), family.upper(), fill=BACKGROUND, font=SMALL)
            for index in range(6):
                sprite = sprites[f'{family}-{index + 1:02}']
                center = 280 + index * 170
                canvas.alpha_composite(sprite, (center - sprite.width // 2, baseline - sprite.height))
                draw.text((center - 35, baseline + 12), f'{index + 1:02}', fill=BACKGROUND, font=SMALL)
        canvas.convert('RGB').save(ROOT / 'review' / f'{title}.png')
    return {'path': str(reference_path.relative_to(REPO)), 'sha256': digest(reference_path),
            'paint_height_in_compositions': 144}


def main():
    """Build the complete seven-family candidate package and hash its review inputs."""
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
        draw.text((24, 14), f'{family.upper()} / object artwork candidates', fill='#1a201d', font=FONT)
        draw.text((24, 45), 'Enlarged detail; actual proposed scale shown separately.', fill='#454a40', font=SMALL)
        for index, name in enumerate(names):
            asset_id = f'{family}-{index + 1:02}'
            cell = CELLS.get(family, cells(sheet))[index]
            paint, bounds = paint_from_cell(sheet, cell, asset_id)
            height = HEIGHTS[family][index]
            width = max(1, round(paint.width * height / paint.height))
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
                           'source_cell': list(cell), 'source_alpha_bounds_in_cell': bounds,
                           'master': f'masters/{asset_id}.ora', 'export': f'exports/{asset_id}.png',
                           'size': list(export.size), 'master_size': list(master.size),
                           'proposed_pivot': [export.width // 2, export.height - PADDING - 1],
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
                 'notes': 'Blank inscription fields; no scenario facts or runtime authority.'}
    (ROOT / 'inventory.json').write_text(json.dumps(inventory, indent=2) + '\n')
    files = sorted(path for folder in ['sources', 'exports', 'masters', 'review']
                   for path in (ROOT / folder).iterdir() if path.is_file())
    files += [ROOT / 'inventory.json', ROOT / 'prompts.json', Path(__file__),
              ROOT / 'inspect_package.py', ROOT / 'README.md']
    (ROOT / 'review-manifest.json').write_text(json.dumps({
        'status': inventory['status'], 'files': {str(path.relative_to(ROOT)): digest(path) for path in files},
    }, indent=2) + '\n')


if __name__ == '__main__':
    main()
