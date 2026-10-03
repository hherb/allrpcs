#!/usr/bin/env python3
"""Package generated character cells without repainting; all alignment is provisional."""

from io import BytesIO
from collections import deque
from pathlib import Path
import hashlib
import json
import zipfile
from xml.etree.ElementTree import Element, SubElement, tostring

from PIL import Image, ImageDraw, ImageFont

ROOT = Path(__file__).resolve().parent
REPO = ROOT.parents[1]
CAST = ['seeker', 'healer', 'elian', 'sera', 'orin', 'vale', 'rowan', 'mira']
ANIMATED = CAST[1:] + ['seeker-plain', 'seeker-apron']
DIRECTIONS = ['south', 'west', 'east', 'north']
# The generated Rowan sheet reverses its two side-view rows; relabel, never mirror.
SOURCE_DIRECTIONS = {'rowan': ['south', 'east', 'west', 'north']}
# Sleeping poses have unequal horizontal spacing; these measured gutters contain whole figures.
DORMANT_X = [0, 420, 824, 1220, 1536]
PORTRAIT_BOUNDS = [(10, 35, 401, 515), (402, 55, 765, 515),
                   (766, 45, 1163, 515), (1164, 60, 1536, 515),
                   (10, 518, 401, 992), (402, 515, 766, 992),
                   (767, 514, 1179, 992), (1180, 512, 1536, 992)]
COMPONENT_ALPHA_THRESHOLD = 16
MIN_FIGURE_PIXELS = 1000
FIGURE_PADDING = 3
BASE_FRAMES = ['idle-01', 'idle-02', 'walk-01', 'walk-02', 'walk-03', 'walk-04', 'inspect-01', 'inspect-02']
WALK_FRAMES = [f'walk-{index:02}' for index in range(1, 5)]
FRAMES = ['idle-01', 'idle-02', *WALK_FRAMES, 'inspect-01', 'inspect-02']
FONT_PATH = '/System/Library/Fonts/Supplemental/Arial.ttf'
FONT = ImageFont.truetype(FONT_PATH, 20)
SMALL = ImageFont.truetype(FONT_PATH, 15)
BACKGROUND = '#e8e3d5'


def digest(path):
    """Return a byte digest for a repository artifact."""
    return hashlib.sha256(path.read_bytes()).hexdigest()


def png_bytes(image):
    """Encode deterministic PNG bytes for an OpenRaster member."""
    buffer = BytesIO()
    image.save(buffer, format='PNG')
    return buffer.getvalue()


def write_master(image, path, name):
    """Keep source paint plus a blank correction layer in a reproducible 2x master."""
    xml = Element('image', w=str(image.width), h=str(image.height), name=name, version='0.0.3')
    stack = SubElement(xml, 'stack')
    for title, source in [('Corrections', 'data/corrections.png'), ('Source paint', 'data/source.png')]:
        SubElement(stack, 'layer', name=title, src=source, x='0', y='0', opacity='1.0',
                   visibility='visible', **{'composite-op': 'svg:src-over'})
    with zipfile.ZipFile(path, 'w') as archive:
        for name, data in [('mimetype', b'image/openraster'), ('stack.xml', tostring(xml)),
                           ('data/source.png', png_bytes(image)),
                           ('data/corrections.png', png_bytes(Image.new('RGBA', image.size))),
                           ('mergedimage.png', png_bytes(image))]:
            info = zipfile.ZipInfo(name, date_time=(2026, 10, 4, 0, 0, 0))
            archive.writestr(info, data, compress_type=zipfile.ZIP_STORED)


def cell_bounds(sheet, columns, rows, index):
    """Partition the full source sheet without dropping fractional boundary pixels."""
    row, column = divmod(index, columns)
    return (column * sheet.width // columns, row * sheet.height // rows,
            (column + 1) * sheet.width // columns, (row + 1) * sheet.height // rows)


def figure_bounds(sheet, columns=8, rows=4, threshold=COMPONENT_ALPHA_THRESHOLD):
    """Locate the full source series; threshold affects detection only, not exported alpha."""
    width, height = sheet.size
    mask = bytearray(alpha > threshold for alpha in sheet.getchannel('A').tobytes())
    figures = []
    for start in range(width * height):
        if not mask[start]:
            continue
        mask[start] = 0
        queue = deque([start])
        count, left, top, right, bottom = 0, width, height, 0, 0
        while queue:
            pixel = queue.popleft()
            y, x = divmod(pixel, width)
            count += 1
            left, top, right, bottom = min(left, x), min(top, y), max(right, x), max(bottom, y)
            neighbors = ([pixel - 1] if x else []) + ([pixel + 1] if x < width - 1 else [])
            neighbors += ([pixel - width] if y else []) + ([pixel + width] if y < height - 1 else [])
            for neighbor in neighbors:
                if mask[neighbor]:
                    mask[neighbor] = 0
                    queue.append(neighbor)
        if count >= MIN_FIGURE_PIXELS:
            figures.append((max(0, left - FIGURE_PADDING), max(0, top - FIGURE_PADDING),
                            min(width, right + 1 + FIGURE_PADDING), min(height, bottom + 1 + FIGURE_PADDING)))
    if len(figures) != columns * rows:
        # Some generated edits bridge figures with faint alpha. Detect at a higher threshold,
        # while keeping the original alpha unchanged in the padded export crop.
        if threshold < 128:
            return figure_bounds(sheet, columns, rows, min(128, threshold * 2))
        raise ValueError(f'Expected {columns * rows} isolated figures, found {len(figures)}; inspect the source')
    by_row = sorted(figures, key=lambda bounds: bounds[1] + bounds[3])
    return ([bounds for row in range(rows)
             for bounds in sorted(by_row[row * columns:(row + 1) * columns],
                                  key=lambda bounds: bounds[0] + bounds[2])], threshold)


def package_cell(sheet, bounds, asset_id, size, trim, assets, paint_scale=None, **metadata):
    """Extract source paint without repainting; proposed registration remains provisional."""
    paint = sheet.crop(bounds)
    alpha_bounds = paint.getchannel('A').getbbox()
    if alpha_bounds is None:
        raise ValueError(f'Empty artwork: {asset_id}')
    if trim:
        paint = paint.crop(alpha_bounds)
    scale = paint_scale if paint_scale is not None else min(size[0] * 2 / paint.width, size[1] * 2 / paint.height)
    resized = paint.resize((max(1, round(paint.width * scale)), max(1, round(paint.height * scale))),
                           Image.Resampling.LANCZOS)
    master = Image.new('RGBA', (size[0] * 2, size[1] * 2))
    master.alpha_composite(resized, ((master.width - resized.width) // 2, master.height - resized.height))
    export = master.resize(size, Image.Resampling.LANCZOS)
    master_path = ROOT / 'masters' / f'{asset_id}.ora'
    export_path = ROOT / 'exports' / f'{asset_id}.png'
    write_master(master, master_path, asset_id)
    export.save(export_path)
    assets.append({'id': asset_id, 'source_cell': list(bounds), 'source_alpha_bounds_in_cell': list(alpha_bounds),
                   'master': f'masters/{asset_id}.ora', 'export': f'exports/{asset_id}.png',
                   'size': list(size), 'proposed_pivot': [size[0] // 2, size[1] - 1],
                   'alignment': 'provisional; shared family scale and bottom-center registration for animation',
                   'sha256': digest(export_path), 'master_sha256': digest(master_path), **metadata})
    return export


def board(title, sprites, labels, columns, cell_size):
    """Label a family board on a neutral backdrop for alpha-edge review."""
    width, height = cell_size
    rows = (len(sprites) + columns - 1) // columns
    result = Image.new('RGB', (columns * width, rows * height + 64), BACKGROUND)
    draw = ImageDraw.Draw(result)
    draw.text((16, 12), title, fill='#1a201d', font=FONT)
    draw.text((16, 38), 'Candidate artwork / scale, timing and pivots provisional', fill='#454a40', font=SMALL)
    for index, (sprite, label) in enumerate(zip(sprites, labels, strict=True)):
        row, column = divmod(index, columns)
        preview = sprite.copy()
        preview.thumbnail((width - 16, height - 36), Image.Resampling.LANCZOS)
        x, y = column * width, 64 + row * height
        result.paste(preview, (x + (width - preview.width) // 2, y + height - 32 - preview.height), preview)
        draw.text((x + 8, y + height - 26), label, fill='#1a201d', font=SMALL)
    return result


def main():
    if (ROOT / 'owner-acceptance.json').exists():
        raise ValueError('This package is owner-approved; build revisions in a separate candidate package')
    for folder in ['masters', 'exports', 'review']:
        (ROOT / folder).mkdir(exist_ok=True)
    assets, designs, portraits, animations = [], [], [], {}
    for family, size, target in [('cast', (160, 240), designs), ('portraits', (256, 320), portraits)]:
        with Image.open(ROOT / 'sources' / f'{family}.png') as source:
            sheet = source.convert('RGBA')
        for index, identity in enumerate(CAST):
            bounds = PORTRAIT_BOUNDS[index] if family == 'portraits' else cell_bounds(sheet, 4, 2, index)
            if family == 'cast' and identity in ['sera', 'mira']:
                # Sera's foot fringe reaches the nominal divider; Mira's head starts below this gutter.
                bounds = (bounds[0], 0 if identity == 'sera' else 518,
                          bounds[2], 518 if identity == 'sera' else sheet.height)
            target.append(package_cell(sheet, bounds, f'{identity}-{family}',
                                       size, True, assets, source=f'sources/{family}.png', kind=family, role=identity))
        board(f'CHARACTERS / {family}', target, CAST, 4, (320, 360)).save(ROOT / 'review' / f'{family}.png')
    for identity in ANIMATED:
        with Image.open(ROOT / 'sources' / f'{identity}.png') as source:
            sheet = source.convert('RGBA')
        bounds, detection_threshold = figure_bounds(sheet)
        # One family scale preserves relative pose sizes, with a maximum 144-pixel figure height.
        scale = min(288 / max(b[3] - b[1] for b in bounds), 248 / max(b[2] - b[0] for b in bounds))
        sprites, labels = [], []
        for row, direction in enumerate(DIRECTIONS):
            source_row = SOURCE_DIRECTIONS.get(identity, DIRECTIONS).index(direction)
            for column, frame in enumerate(FRAMES):
                asset_id = f'{identity}-{direction}-{frame}'
                source_bounds = bounds[source_row * 8 + BASE_FRAMES.index(frame)]
                sprites.append(package_cell(sheet, source_bounds, asset_id,
                                            (128, 192), False, assets, source=f'sources/{identity}.png',
                                            paint_scale=scale, kind='animation', role=identity,
                                            direction=direction, frame=frame,
                                            source_detection_threshold=detection_threshold))
                labels.append(f'{direction} {frame}')
        animations[identity] = sprites
        board(f'{identity.upper()} / idle, incomplete stride study, inspect', sprites, labels,
              len(FRAMES), (192, 248)).save(ROOT / 'review' / f'{identity}.png')
        for action, indices in [('idle', [0, 1]), ('walk', list(range(2, 6))), ('inspect', [6, 7, 6, 0])]:
            title = f'{identity.upper()} / INCOMPLETE stride study' if action == 'walk' else f'{identity.upper()} / {action} cycle candidate'
            pictures = [board(title,
                              [sprites[row * len(FRAMES) + index] for row in range(4)], DIRECTIONS, 4, (192, 248))
                        for index in indices]
            pictures[0].save(ROOT / 'review' / f'{identity}-{action}.gif', save_all=True,
                             append_images=pictures[1:], duration=240 if action != 'walk' else 160, loop=0)
    with Image.open(ROOT / 'sources' / 'dormant.png') as source:
        sheet = source.convert('RGBA')
    sleeping, sleep_labels = [], []
    for index in range(8):
        identity = 'rowan' if index < 4 else 'mira'
        frame = index % 4 + 1
        asset_id = f'{identity}-dormant-{frame:02}'
        row, column = divmod(index, 4)
        bounds = (DORMANT_X[column], row * sheet.height // 2,
                  DORMANT_X[column + 1], (row + 1) * sheet.height // 2)
        if sheet.width != DORMANT_X[-1]:
            raise ValueError('Dormant source width changed; inspect crop gutters again')
        sleeping.append(package_cell(sheet, bounds, asset_id, (256, 160), True,
                                     assets, source='sources/dormant.png', kind='dormant', role=identity))
        sleep_labels.append(f'{identity} pose {frame}')
    board('ROWAN AND MIRA / sleeping, breathing, waking poses', sleeping, sleep_labels, 4,
          (320, 220)).save(ROOT / 'review' / 'dormant.png')
    outfits = [designs[0]] + [animations[identity][0] for identity in ['seeker-plain', 'seeker-apron']]
    board('SEEKER / three authored appearance choices', outfits,
          ['Travelling cloak (design)', 'Plain tunic', 'Work apron'], 3, (320, 300)).save(
              ROOT / 'review' / 'seeker-appearances.png')
    composition = Image.new('RGBA', (1280, 720), '#8b9568')
    for index, identity in enumerate(CAST):
        sprite = designs[index] if identity == 'seeker' else animations[identity][0]
        x, y = 96 + (index % 4) * 304, 330 + (index // 4) * 330
        if identity == 'seeker':
            sprite = sprite.resize((128, 192), Image.Resampling.LANCZOS)
        composition.alpha_composite(sprite, (x, y - sprite.height))
        ImageDraw.Draw(composition).text((x, y + 8), identity, fill='#1a201d', font=FONT)
    ImageDraw.Draw(composition).text((20, 18), 'Cast / proposed export scale / external composition',
                                    fill='#1a201d', font=FONT)
    composition.convert('RGB').save(ROOT / 'review' / 'cast-scale.png')
    inherited = []
    for path in sorted((REPO / 'art-direction/production-proof/corrected/exports/characters/seeker').rglob('*.png')):
        inherited.append({'path': str(path.relative_to(REPO)), 'sha256': digest(path)})
    if len(inherited) != 48:
        raise ValueError('Expected the 48 existing approved Seeker frames')
    inventory = {'status': 'revised character artwork; full walking cycles unresolved', 'generator': 'built-in image_gen',
                 'asset_count': len(assets), 'assets': assets, 'inherited_seeker_frames': inherited,
                 'notes': 'No runtime changes. Walk frames repeat a leading foot and are incomplete stride studies; do not promote. Other timing, continuity and pivots require review.'}
    (ROOT / 'inventory.json').write_text(json.dumps(inventory, indent=2) + '\n')
    print(f'Packaged {len(assets)} new exports/2x masters and retained {len(inherited)} approved Seeker references.')


if __name__ == '__main__':
    main()
