#!/usr/bin/env python3
"""Render a separate alternating cloak gait from retained editable OpenRaster paint."""

from io import BytesIO
import hashlib
import importlib.util
import json
import math
from pathlib import Path
import shutil
from xml.etree import ElementTree as ET
import zipfile

from PIL import Image, ImageDraw, ImageEnhance

REPO = Path(__file__).resolve().parents[2]
OUT = Path(__file__).resolve().parent
SOURCE = REPO / 'art-direction/production-proof/corrected'
GAME = REPO / 'game/art/seeker-cloak-gait-correction'
DIRECTIONS = ['south', 'west', 'east', 'north']
HEMS = {'south': 216, 'west': 216, 'east': 216, 'north': 236}
BOOT_TOP = 236
STEPS = 8
BASELINE = 312
SHEET = SOURCE / 'masters/seeker-four-direction-idle-breathe.ora'


def require(value, message):
    """Reject source or gait drift before retaining candidate output."""
    if not value:
        raise ValueError(message)


def sha(path):
    """Hash exact file bytes."""
    return hashlib.sha256(path.read_bytes()).hexdigest()


def rgba_bytes(image):
    """Encode one lossless PNG for an editable layer."""
    buffer = BytesIO()
    image.save(buffer, format='PNG')
    return buffer.getvalue()


def bounds(image):
    """Measure substantial paint without modifying the stored alpha."""
    return image.getchannel('A').point(lambda alpha: 255 if alpha > 96 else 0).getbbox()


def source_paint(direction, walking=False):
    """Read the original native 2x neutral paint layer, without export upscaling."""
    path = SOURCE / 'masters/seeker-four-direction-walk.ora' if walking else SHEET
    with zipfile.ZipFile(path) as archive:
        root = ET.fromstring(archive.read('stack.xml'))
        name = f'{direction} walk 01' if walking else f'{direction} idle breathe 01'
        layer = next(row for row in root.iter('layer') if row.get('name') == name)
        return Image.open(BytesIO(archive.read(layer.get('src')))).convert('RGBA')


def opaque_patch(paint):
    """Borrow one solid pants-paint patch for the native joint connector."""
    alpha = paint.getchannel('A')
    candidates = []
    for y in range(paint.height - 6):
        for x in range(paint.width - 6):
            box = (x, y, x + 6, y + 6)
            if alpha.crop(box).getextrema()[0] > 220:
                candidates.append((abs(x + 3 - paint.width / 2) + y * 0.1, box))
    require(candidates, 'No opaque native pants patch')
    return paint.crop(min(candidates)[1])


def prepare(direction):
    """Retain torso and cloak; split native boots with fixed anatomical ownership."""
    paint = source_paint(direction)
    hem = HEMS[direction]
    torso = paint.copy()
    alpha = torso.getchannel('A')
    ImageDraw.Draw(alpha).rectangle((0, hem, paint.width, paint.height), fill=0)
    # Cloak tails and the small hand fringe occlude the legs; they remain on the torso.
    for y in range(hem, paint.height):
        for x in range(paint.width):
            red, green, blue, original_alpha = paint.getpixel((x, y))
            cape = blue > red + 12 and green > red + 8
            hand = y < BOOT_TOP and red > 150 and green > 100 and blue > 75
            if cape or hand:
                alpha.putpixel((x, y), original_alpha)
    # Retain paint attached to the torso, excluding isolated lower-body mask specks.
    connected = alpha.point(lambda value: 255 if value else 0)
    row = hem - 20
    seed_x = min((x for x in range(paint.width) if alpha.getpixel((x, row)) > 96),
                 key=lambda x: abs(x - paint.width / 2))
    ImageDraw.floodfill(connected, (seed_x, row), 128)
    for y in range(paint.height):
        for x in range(paint.width):
            if connected.getpixel((x, y)) != 128:
                alpha.putpixel((x, y), 0)
    torso.putalpha(alpha)
    center = paint.width / 2
    side = direction in ('west', 'east')
    if side:
        seed = source_paint(direction, walking=True)
        box = (0, BOOT_TOP, 140, seed.height) if direction == 'west' else (144, BOOT_TOP, seed.width, seed.height)
        boot = seed.crop(box)
        boot = boot.crop(bounds(boot))
        dark = ImageEnhance.Brightness(boot).enhance(0.88)
        feet = [boot, dark] if direction == 'west' else [dark, boot]
        sockets = [center - 4, center + 4]
        patch = opaque_patch(paint.crop((int(center - 30), hem, int(center + 30), BOOT_TOP)))
        patches = [patch, ImageEnhance.Brightness(patch).enhance(0.88)]
        if direction == 'east':
            patches.reverse()
    else:
        feet = []
        for left, right in [(0, int(center)), (int(center), paint.width)]:
            boot = paint.crop((left, 248 if direction == 'north' else BOOT_TOP, right, paint.height))
            feet.append(boot.crop(bounds(boot)))
        sockets = [center - 22, center + 22]
        patches = [opaque_patch(paint.crop((int(socket - 14), hem, int(socket + 14), max(BOOT_TOP, hem + 20))))
                   for socket in sockets]
        if direction == 'south':
            feet.reverse()
            sockets.reverse()
            patches.reverse()
    return torso, feet, patches, sockets, hem


def limb(size, boot, patch, socket, hem, advance, lift, bend):
    """Join each rigid boot shaft to its unchanged hidden hip using native paint."""
    result = Image.new('RGBA', size)
    substantive = bounds(boot)
    shaft = boot.getchannel('A').crop((0, substantive[1], boot.width, substantive[1] + 8))
    shaft_bounds = shaft.point(lambda alpha: 255 if alpha > 96 else 0).getbbox()
    shaft_x = (shaft_bounds[0] + shaft_bounds[2]) / 2
    width = min(36, max(20, shaft_bounds[2] - shaft_bounds[0]))
    boot_x = round(socket - shaft_x + advance)
    boot_y = round(BASELINE - 1 - lift + 1 - boot.height)
    hip = (socket, hem - 14)
    ankle = (boot_x + shaft_x, boot_y + 8)
    knee = ((hip[0] + ankle[0]) / 2 + bend, (hip[1] + ankle[1]) / 2)
    mask = Image.new('L', size)
    draw = ImageDraw.Draw(mask)
    draw.line([hip, knee, ankle], fill=255, width=round(width), joint='curve')
    radius = width / 2
    for x, y in [hip, knee, ankle]:
        draw.ellipse((x - radius, y - radius, x + radius, y + radius), fill=255)
    texture = Image.new('RGBA', size)
    left, top, right, bottom = mask.getbbox()
    texture.paste(patch.resize((right - left, bottom - top), Image.Resampling.BICUBIC), (left, top))
    texture.putalpha(mask)
    result.alpha_composite(texture)
    require(boot_x >= 0 and boot_x + boot.width <= size[0]
            and boot_y >= 0 and boot_y + boot.height <= size[1], 'Boot clips candidate canvas')
    result.alpha_composite(boot, (boot_x, boot_y))
    return result, {'boot_box_2x': [boot_x, boot_y, boot_x + boot.width, boot_y + boot.height],
                    'shaft_2x': [boot_x + shaft_x, boot_y + 8], 'hip_2x': list(hip)}


def render(direction, diagnostic=False):
    """Render both contacts and both passes without changing anatomical ownership."""
    torso, feet, patches, sockets, hem = prepare(direction)
    side = direction in ('west', 'east')
    sign = 1 if direction == 'east' else -1
    frames, records, editable = [], [], []
    for step in range(STEPS + 1):
        phase = math.tau * step / STEPS
        layers, legs = [], []
        for index in range(2):
            theta = phase + index * math.pi
            travel = math.cos(theta)
            swing = max(0, -math.sin(theta))
            advance = sign * 40 * travel if side else 0
            depth = 0 if side else 8 * travel * (1 if direction == 'south' else -1)
            lift = swing * (12 if side else 8) - depth
            layer, measured = limb(torso.size, feet[index], patches[index], sockets[index], hem,
                                   advance, lift, -sign * swing * 8 if side else 0)
            if diagnostic:
                color = Image.new('RGBA', layer.size, '#d13d38' if index == 0 else '#2879cd')
                color.putalpha(layer.getchannel('A'))
                layer = color
            layers.append(layer)
            legs.append({'leg': 'A' if index == 0 else 'B',
                         'anatomical_side': 'left' if index == 0 else 'right',
                         'travel': round(travel, 6), 'advance_2x': round(advance, 6),
                         'swing_lift_2x': round(swing * (12 if side else 8), 6),
                         'depth_2x': round(depth, 6), 'stance': math.sin(theta) >= -1e-9,
                         **measured})
        order = ([0, 1] if direction == 'east' else [1, 0]) if side else sorted(range(2), key=lambda i: legs[i]['depth_2x'])
        bob = -round(2 * abs(math.sin(phase)))
        body = Image.new('RGBA', torso.size)
        body.alpha_composite(torso, (0, bob))
        merged = Image.new('RGBA', torso.size)
        for index in order:
            merged.alpha_composite(layers[index])
        merged.alpha_composite(body)
        frames.append(merged)
        editable.append([('torso and cloak', body), ('anatomical left A', layers[0]), ('anatomical right B', layers[1])])
        records.append({'index': step + 1, 'phase_degrees': step * 45, 'body_bob_2x': bob,
                        'layer_order': order, 'legs': legs})
    require(frames[0].tobytes() == frames[-1].tobytes(), 'Loop does not close')
    return frames[:-1], records[:-1], editable[:-1]


def master(path, merged, layers, order):
    """Retain semantic torso/left/right layers, plus a blank editable correction layer."""
    root = ET.Element('image', w=str(merged.width), h=str(merged.height), name=path.stem, version='0.0.3')
    stack = ET.SubElement(root, 'stack')
    indexed = [('corrections', Image.new('RGBA', merged.size)), layers[0], layers[1 + order[1]], layers[1 + order[0]]]
    with zipfile.ZipFile(path, 'w', compression=zipfile.ZIP_DEFLATED) as archive:
        def store(name, data, compressed=True):
            """Retain byte-reproducible ZIP metadata for editable masters."""
            info = zipfile.ZipInfo(name, date_time=(1980, 1, 1, 0, 0, 0))
            info.compress_type = zipfile.ZIP_DEFLATED if compressed else zipfile.ZIP_STORED
            info.external_attr = 0o100644 << 16
            archive.writestr(info, data)

        store('mimetype', b'image/openraster', False)
        for index, (name, image) in enumerate(indexed):
            src = f'data/layer{index:03}.png'
            ET.SubElement(stack, 'layer', name=name, src=src, x='0', y='0', visibility='visible', opacity='1.0',
                          **{'composite_op': 'svg:src-over'})
            store(src, rgba_bytes(image))
        store('stack.xml', ET.tostring(root))
        store('mergedimage.png', rgba_bytes(merged))


def main():
    """Write all four directions together, retaining original paint and proof bytes."""
    require(not (OUT / 'candidate.json').exists(), 'Candidate snapshot already retained')
    for folder in [OUT / 'exports', OUT / 'masters', GAME / 'frames']:
        folder.mkdir(parents=True, exist_ok=True)
    spec = importlib.util.spec_from_file_location('import_recipe', REPO / 'art-direction/character-animation-import-proof/build.py')
    recipe = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(recipe)
    assets, rigs, import_frames, animations = [], [], [], []
    for direction in DIRECTIONS:
        frames, records, layers = render(direction)
        rigs.append({'direction': direction, 'source_layer': f'{direction} idle breathe 01', 'phases': records})
        identities = []
        for index, (image, record, layers_for_frame) in enumerate(zip(frames, records, layers), 1):
            identity = f'seeker-cloak-{direction}-walk-{index:02d}'
            exported = OUT / 'exports' / (identity + '.png')
            native = image.resize((image.width // 2, image.height // 2), Image.Resampling.LANCZOS)
            native.save(exported)
            ora = OUT / 'masters' / (identity + '.ora')
            master(ora, image, layers_for_frame, record['layer_order'])
            copied = GAME / 'frames' / exported.name
            shutil.copyfile(exported, copied)
            resource = 'res://' + str(copied.relative_to(REPO / 'game'))
            copied.with_suffix('.png.import').write_text(recipe.sidecar(resource))
            asset = {'id': identity, 'direction': direction, 'index': index,
                     'source_master': str(SHEET.relative_to(REPO)), 'source_master_sha256': sha(SHEET),
                     'side_boot_source': 'Original walk 01 single leading boot' if direction in ('west', 'east') else 'Original idle 01 separated boots',
                     'export': str(exported.relative_to(REPO)), 'sha256': sha(exported),
                     'master': str(ora.relative_to(REPO)), 'master_sha256': sha(ora),
                     'texture': str(copied.relative_to(REPO)), 'size': list(native.size),
                     'pivot': [native.width // 2, BASELINE // 2]}
            assets.append(asset)
            import_frames.append(asset)
            identities.append(identity)
        animations.append({'name': 'walk_' + direction, 'frames': identities, 'frame_ms': 90, 'loop': True})
    (GAME / 'walk.tres').write_text(recipe.sprite_frames(import_frames, animations))
    data = {'date': '2026-10-06', 'status': 'new gait candidate; owner selection not inferred',
            'directions': DIRECTIONS, 'frames_per_direction': 8, 'cycle_ms': 720,
            'source_policy': 'Original native 2x editable paint; rigid boot cutouts and local textured hip/knee/ankle connectors',
            'original_walk_defect': 'Six inherited poses retain one leading leg; counts/import parity cannot prove alternating gait',
            'anatomical_ownership': 'A always left, B always right; no identity swaps at passing; fixed side occlusion',
            'source_masters': {str(path.relative_to(REPO)): sha(path) for path in
                               [SHEET, SOURCE / 'masters/seeker-four-direction-walk.ora']},
            'limitations': ['Simple cutout gait; near/far side boot reuses visible paint with restrained value difference',
                            'Rigid upper body, accessories and neutral arms; no Main installation or footstep/physical semantics'],
            'assets': assets, 'rigs': rigs}
    (OUT / 'rig.json').write_text(json.dumps(data, indent=2) + '\n')
    (GAME / 'rig.json').write_text(json.dumps(data, indent=2) + '\n')
    print('PASS: 32 separate cloak gait frames and semantic editable 2x masters')


if __name__ == '__main__':
    main()
