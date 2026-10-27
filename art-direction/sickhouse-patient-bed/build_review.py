#!/usr/bin/env python3
"""Separate the generated sickbed candidates, package 2x masters and build the review board.

The generated source sheet stays unchanged. Separation and the OpenRaster master format
reuse the accepted structures-batch helpers so every artwork module is packaged the same way.
No runtime integration happens here.
"""
from importlib.util import module_from_spec, spec_from_file_location
from pathlib import Path
import hashlib
import json

from PIL import Image, ImageDraw

ROOT = Path(__file__).resolve().parent
REPO = ROOT.parents[1]


def load_structures_helpers():
    """Import the frozen structures-batch build module by path (its folder name has a hyphen)."""
    spec = spec_from_file_location('structures_build', REPO / 'art-direction/structures-batch/build_review.py')
    module = module_from_spec(spec)
    spec.loader.exec_module(module)
    return module


HELPERS = load_structures_helpers()
FONT, SMALL = HELPERS.FONT, HELPERS.SMALL
NAMES = ['Linen pallet on timber frame', 'Rope-strung cot', 'Kerbed straw pallet with herb sachet']
# Each candidate occupies one column of the 1945x808 sheet; the gaps are fully transparent.
CELLS = [(0, 0, 655, 808), (655, 0, 1287, 808), (1287, 0, 1945, 808)]
# Runtime width (approved with sickbed-01): Mira's sleeping body (about 131x107 px at scale 1) fits on the
# mattress with a margin, and the bed stays inside the 320x192 px sickhouse room.
EXPORT_WIDTH = 200
MIRA = [REPO / f'art-direction/characters-batch/exports/mira-dormant-0{n}.png' for n in (1, 2)]
# Pose 2 is painted 4 px left of and 3 px below pose 1; drawing it at this offset registers
# the two poses (least alpha difference over a +/-20 px search) so breathing does not jitter.
POSE2_OFFSET = (4, -3)
# Mira's canvas centre sits this far from the bed centre so her head rests on the pillow.
PATIENT_ON_BED = (20, -18)
FLOOR = REPO / 'game/art/runtime/structures/cutaway-wood.png'


def sha(path):
    """Hash exact persisted bytes."""
    return hashlib.sha256(path.read_bytes()).hexdigest()


def module_exports():
    """Return (asset id, name, cell, alpha bounds, export, 2x master) per candidate."""
    sheet = Image.open(ROOT / 'sources/bed-sheet.png').convert('RGBA')
    rows = []
    for index, (name, cell) in enumerate(zip(NAMES, CELLS), 1):
        crop = HELPERS.separate(sheet.crop(cell))
        bounds = crop.getchannel('A').getbbox()
        if bounds is None:
            raise ValueError(f'Empty module: sickbed-{index:02}')
        paint = crop.crop(bounds)
        height = max(1, round(paint.height * EXPORT_WIDTH / paint.width))
        master = paint.resize((EXPORT_WIDTH * 2, height * 2), Image.Resampling.LANCZOS)
        export = master.resize((EXPORT_WIDTH, height), Image.Resampling.LANCZOS)
        rows.append((f'sickbed-{index:02}', name, cell, bounds, export, master))
    return rows


def floor_tile(size):
    """Tile the existing cutaway wood floor over a review background of the given size."""
    tile = Image.open(FLOOR).convert('RGBA')
    canvas = Image.new('RGBA', size)
    for y in range(0, size[1], tile.height):
        for x in range(0, size[0], tile.width):
            canvas.alpha_composite(tile, (x, y))
    return canvas


def scene(bed, pose):
    """Compose one bed with Mira placed on it at PATIENT_ON_BED, both at runtime scale, on the floor."""
    room = floor_tile((320, 192))
    room.alpha_composite(bed, ((320 - bed.width) // 2, (192 - bed.height) // 2))
    mira = Image.open(MIRA[pose]).convert('RGBA')
    dx, dy = POSE2_OFFSET if pose else (0, 0)
    room.alpha_composite(mira, ((320 - 256) // 2 + PATIENT_ON_BED[0] + dx,
                                (192 - 160) // 2 + PATIENT_ON_BED[1] + dy))
    return room


def main():
    for folder in ('masters', 'exports', 'review'):
        (ROOT / folder).mkdir(exist_ok=True)
    rows = module_exports()
    board = Image.new('RGB', (1536, 1060), '#e8e3d5')
    draw = ImageDraw.Draw(board)
    draw.text((24, 14), 'SICKHOUSE BED — three candidates (pick one)', fill='#1a201d', font=FONT)
    draw.text((24, 45), 'Top: enlarged detail. Below: game scale (2x zoom) with Mira pose 1 and pose 2 on the cutaway floor.',
              fill='#454a40', font=SMALL)
    assets = []
    for column, (asset_id, name, cell, bounds, export, master) in enumerate(rows):
        master_path, export_path = ROOT / 'masters' / f'{asset_id}.ora', ROOT / 'exports' / f'{asset_id}.png'
        HELPERS.write_master(master, master_path, name)
        export.save(export_path)
        x = column * 512
        preview = master.copy()
        preview.thumbnail((464, 340), Image.Resampling.LANCZOS)
        board.paste(preview, (x + (512 - preview.width) // 2, 90 + (340 - preview.height) // 2), preview)
        draw.text((x + 18, 440), f'{asset_id}: {name}', fill='#1a201d', font=SMALL)
        for pose in (0, 1):
            view = scene(export, pose).resize((320 * 3 // 2, 192 * 3 // 2), Image.Resampling.NEAREST)
            board.paste(view, (x + (512 - view.width) // 2, 480 + pose * 295), view)
        assets.append({'id': asset_id, 'name': name, 'source': 'sources/bed-sheet.png', 'source_cell': list(cell),
                       'source_alpha_bounds_in_cell': list(bounds), 'master': f'masters/{asset_id}.ora',
                       'export': f'exports/{asset_id}.png', 'size': list(export.size),
                       'proposed_pivot': [export.width // 2, export.height // 2], 'pivot_status': 'provisional',
                       'sha256': sha(export_path)})
    board.save(ROOT / 'review/sickbed-candidates.png')
    (ROOT / 'inventory.json').write_text(json.dumps({
        'status': 'owner approved sickbed-01' if (ROOT / 'owner-acceptance.json').exists() else 'awaiting owner selection', 'generator': 'Codex CLI 0.162.0-alpha.17.2 built-in image_gen',
        'prompt': 'prompt.txt', 'source_sha256': sha(ROOT / 'sources/bed-sheet.png'),
        'references': ['art-direction/representative-scene/previews/selected-direction-a-refinement.png',
                       'art-direction/woodland-environment-batch/review/trees.png',
                       'art-direction/characters-batch/exports/mira-dormant-01.png'],
        'mira_pose2_registration_offset': list(POSE2_OFFSET), 'patient_on_bed_offset': list(PATIENT_ON_BED),
        'notes': 'Original source paint retained; raster ORA correction layers blank; scale and pivot provisional.',
        'assets': assets}, indent=2) + '\n')
    print(f'Packaged {len(assets)} sickbed candidates, editable 2x masters and one review board.')


if __name__ == '__main__':
    main()
