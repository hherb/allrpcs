#!/usr/bin/env python3
"""Render external cutout gait candidates with persistent leg ownership, authorized by the owner."""

from io import BytesIO
import json
import math
import zipfile

from PIL import Image, ImageChops, ImageDraw, ImageEnhance

from build_review import ANIMATED, DIRECTIONS, ROOT, board, digest, write_master

OUT = ROOT / 'walk-rig'
STEPS = 8
# Measured neutral clothing/leg boundary in the existing 128x192 authoring canvas.
# Order south, west, east, north. Long clothing naturally hides most of each leg.
HEMS = {
    'healer': [171, 170, 170, 172], 'elian': [155, 157, 157, 158],
    'sera': [168, 170, 170, 173], 'orin': [150, 153, 153, 150],
    'vale': [153, 155, 155, 154], 'rowan': [153, 155, 155, 153],
    'mira': [172, 174, 174, 174], 'seeker-plain': [150, 153, 153, 151],
    'seeker-apron': [152, 154, 154, 152],
}
# Complete side-view leading-foot paint from the original stride studies. The far
# limb reuses this occluded-side paint rather than inventing missing generated pixels.
# x center, upper crop edge, width; values are in export pixels, applied at 2x.
SIDE_PAINT = {
    'healer': [(43, 168, 28), (85, 165, 32)],
    'elian': [(43, 160, 30), (85, 153, 34)],
    'sera': [(38, 171, 28), (88, 170, 28)],
    'orin': [(41, 161, 31), (87, 160, 32)],
    'vale': [(43, 167, 31), (88, 162, 34)],
    'rowan': [(39, 172, 34), (91, 164, 34)],
    'mira': [(47, 178, 29), (82, 176, 29)],
    'seeker-plain': [(47, 166, 32), (85, 154, 34)],
    'seeker-apron': [(41, 169, 32), (84, 167, 34)],
}


def master_paint(asset_id):
    """Load native 2x paint, avoiding an export-upscale round trip."""
    with zipfile.ZipFile(ROOT / 'masters' / f'{asset_id}.ora') as archive:
        with Image.open(BytesIO(archive.read('mergedimage.png'))) as paint:
            return paint.convert('RGBA')


def substantive_bounds(paint):
    """Use painted alpha to measure foot placement, ignoring generated faint fringe."""
    return paint.getchannel('A').point(lambda value: 255 if value > 96 else 0).getbbox()


def prepare(role, direction):
    """Extract a neutral torso and two persistent limb layers from recorded paint."""
    neutral_id = f'{role}-{direction}-idle-01'
    neutral = master_paint(neutral_id)
    hem = HEMS[role][DIRECTIONS.index(direction)] * 2
    box = substantive_bounds(neutral)
    floor = box[3] - 1
    body = neutral.copy()
    # Elian's staff is a separate persistent occluder, never a moving leg.
    leg_region = (0, hem, 256, 384)
    if role == 'elian':
        leg_region = (86, hem, 180, 384)
    erase = Image.new('L', body.size)
    ImageDraw.Draw(erase).rectangle(leg_region, fill=255)
    body.putalpha(ImageChops.subtract(body.getchannel('A'), erase))
    feet = []
    sources = [neutral_id]
    if direction in ('east', 'west'):
        seed_id = f'{role}-{direction}-walk-01'
        seed = master_paint(seed_id)
        center, top, width = SIDE_PAINT[role][direction == 'east']
        cut = seed.crop((int((center - width / 2) * 2), top * 2,
                         int((center + width / 2) * 2), 384))
        bounds = substantive_bounds(cut)
        if bounds is None:
            raise ValueError(f'Empty side limb: {seed_id}')
        cut = cut.crop(bounds)
        # Two separately named layers keep ownership; a subtle value difference
        # communicates the occluded limb, with no left/right swaps or mirroring.
        feet = [cut, ImageEnhance.Brightness(cut).enhance(0.88)]
        if direction == 'east':
            # With the camera south of the actor, its anatomical right side is near.
            feet.reverse()
        sources.append(seed_id)
        sockets = [128 - 5, 128 + 5]
    else:
        divider = (box[0] + box[2]) // 2
        for left, right in [(leg_region[0], divider), (divider, leg_region[2])]:
            paint_top = hem + (6 if role in ('healer', 'sera', 'mira') else 0)
            cut = neutral.crop((left, paint_top, right, 384))
            bounds = substantive_bounds(cut)
            if bounds is None:
                raise ValueError(f'Empty front/rear limb: {neutral_id}')
            feet.append(cut.crop(bounds))
        sockets = [divider - 14, divider + 14]
        if direction == 'south':
            # Facing the viewer reverses anatomical left/right in screen space.
            feet.reverse()
            sockets.reverse()
    if direction in ('east', 'west'):
        sample = neutral.crop((94, hem, 164, min(384, hem + 24)))
        texture = opaque_texture(sample)
        textures = [texture, ImageEnhance.Brightness(texture).enhance(0.88)]
        if direction == 'east':
            textures.reverse()
    else:
        textures = [opaque_texture(foot) for foot in feet]
    return body, feet, textures, sockets, hem, floor, sources


def opaque_texture(paint):
    """Borrow a solid interior paint patch; transparent toe edges cannot form a shin."""
    alpha = paint.getchannel('A')
    for size in (8, 4, 2):
        candidates = []
        for y in range(0, paint.height - size + 1, 2):
            for x in range(0, paint.width - size + 1, 2):
                box = (x, y, x + size, y + size)
                if alpha.crop(box).getextrema()[0] > 200:
                    candidates.append((abs(x + size / 2 - paint.width / 2) + y * 0.2, box))
        if candidates:
            return paint.crop(min(candidates)[1])
    raise ValueError('No solid painted interior for a limb connector')


def deform_limb(paint, texture, socket, hem, floor, advance, lift):
    """Move a rigid painted boot; a textured shin joins it to the hidden upper socket."""
    overlap = 8
    top = hem - overlap
    height = paint.height
    width = paint.width
    limb = paint
    sole = round(floor - lift)
    boot_top = sole - height
    # Only the short connector is sheared. The authored boot retains its rigid
    # outline, details and toe direction throughout the full gait.
    result = Image.new('RGBA', (256, 384))
    if boot_top + 8 <= top:
        result.alpha_composite(limb, (round(socket - width / 2 + advance), boot_top))
        return result
    connector_height = boot_top + 8 - top
    connector = texture.resize((28, connector_height), Image.Resampling.BICUBIC)
    padding = 40
    panel = Image.new('RGBA', (28 + padding * 2, connector_height))
    panel.alpha_composite(connector, (padding, 0))
    panel = panel.transform(panel.size, Image.Transform.AFFINE,
                            (1, -advance / connector_height, 0, 0, 1, 0), Image.Resampling.BICUBIC)
    result.alpha_composite(panel, (round(socket - 14 - padding), top))
    result.alpha_composite(limb, (round(socket - width / 2 + advance), boot_top))
    return result


def render(role, direction, diagnostic=False):
    """Eight poses span both contacts, both passing poses, and the loop closure."""
    body, feet, textures, sockets, hem, floor, sources = prepare(role, direction)
    frames, metadata = [], []
    side = direction in ('east', 'west')
    sign = 1 if direction == 'east' else -1
    stride = min(22, max(12, (floor - hem) * 0.75))
    for step in range(STEPS + 1):
        phase = math.tau * step / STEPS
        layers, positions = [], []
        for leg in range(2):
            theta = phase + math.pi * leg
            travel = math.cos(theta)
            lift = max(0, -math.sin(theta)) * (8 if side else 5)
            advance = sign * stride * travel if side else 0
            depth = 0 if side else (5 * travel if direction == 'south' else -5 * travel)
            layer = deform_limb(feet[leg], textures[leg], sockets[leg], hem, floor,
                                advance, lift - depth)
            if diagnostic:
                tinted = Image.new('RGBA', layer.size, '#d13d38' if leg == 0 else '#2879cd')
                tinted.putalpha(layer.getchannel('A'))
                layer = tinted
            layers.append(layer)
            positions.append({'leg': 'A' if leg == 0 else 'B',
                              'anatomical_side': 'left' if leg == 0 else 'right',
                              'travel': round(travel, 6), 'lift': round(lift, 6),
                              'advance': round(advance, 6), 'depth': round(depth, 6),
                              'paint_bounds_2x': substantive_bounds(layer),
                              'stance': math.sin(theta) >= -1e-9})
        canvas = Image.new('RGBA', body.size)
        # Side limbs retain near/far depth. Front/rear draw the closer foot last.
        order = ([0, 1] if direction == 'east' else [1, 0]) if side else sorted(
            range(2), key=lambda leg: positions[leg]['depth'])
        for leg in order:
            canvas.alpha_composite(layers[leg])
        bob = -round(2 * abs(math.sin(phase)))
        canvas.alpha_composite(body, (0, bob))
        frames.append(canvas)
        metadata.append({'frame': step + 1, 'phase_degrees': step * 45,
                         'body_bob_2x': bob, 'legs': positions, 'layer_order': order})
    if frames[0].tobytes() != frames[-1].tobytes():
        raise ValueError(f'Loop endpoint differs: {role} {direction}')
    return frames[:-1], metadata[:-1], sources


def main():
    """Keep rig candidates separate from the immutable initial review checkpoint."""
    if (OUT / 'owner-acceptance.json').exists():
        raise ValueError('Approved rig package; author a separate revision')
    for folder in ['exports', 'masters', 'review']:
        (OUT / folder).mkdir(parents=True, exist_ok=True)
    assets, rigs = [], []
    for role in ANIMATED:
        by_direction = []
        for direction in DIRECTIONS:
            frames, phases, sources = render(role, direction)
            source_hashes = {asset_id: digest(ROOT / 'masters' / f'{asset_id}.ora') for asset_id in sources}
            rigs.append({'role': role, 'direction': direction, 'sources': source_hashes,
                         'hem_export_y': HEMS[role][DIRECTIONS.index(direction)],
                         'loop_endpoint_pixels_equal': True, 'phases': phases})
            exports = []
            for index, frame in enumerate(frames):
                asset_id = f'{role}-{direction}-walk-{index + 1:02}'
                master_path = OUT / 'masters' / f'{asset_id}.ora'
                export_path = OUT / 'exports' / f'{asset_id}.png'
                write_master(frame, master_path, asset_id)
                export = frame.resize((128, 192), Image.Resampling.LANCZOS)
                export.save(export_path)
                assets.append({'id': asset_id, 'master': str(master_path.relative_to(OUT)),
                               'export': str(export_path.relative_to(OUT)),
                               'master_sha256': digest(master_path), 'sha256': digest(export_path)})
                exports.append(export)
            by_direction.append(exports)
        all_frames = [frame for row in by_direction for frame in row]
        labels = [f'{direction} {index * 45} deg' for direction in DIRECTIONS for index in range(STEPS)]
        board(f'{role.upper()} / cutout full gait candidate', all_frames, labels,
              STEPS, (160, 228)).save(OUT / 'review' / f'{role}.png')
        previews = [board(f'{role.upper()} / mechanical full gait candidate',
                          [row[index] for row in by_direction], DIRECTIONS, 4, (192, 248))
                    for index in range(STEPS)]
        previews[0].save(OUT / 'review' / f'{role}-walk.gif', save_all=True,
                         append_images=previews[1:], duration=100, loop=0, disposal=2)
    diagnostic = [render('healer', direction, diagnostic=True)[0] for direction in DIRECTIONS]
    pictures = [board('HEALER / ownership diagnostic: A red, B blue',
                      [row[index].resize((128, 192), Image.Resampling.LANCZOS) for row in diagnostic],
                      DIRECTIONS, 4, (192, 248)) for index in range(STEPS)]
    pictures[0].save(OUT / 'review' / 'healer-leg-ownership.gif', save_all=True,
                     append_images=pictures[1:], duration=180, loop=0, disposal=2)
    result = {'status': 'mechanically alternating cutout candidates; owner visual review pending',
              'permission': 'Owner explicitly authorized mechanical rigging and rendering on 2026-10-04',
              'frames_per_direction': STEPS, 'asset_count': len(assets), 'assets': assets, 'rigs': rigs,
              'limitations': ['Lower-limb cutout rig, with modest torso bob; arms retain the neutral pose',
                              'Occluded side limb reuses visible-side paint with a darker value',
                              'No runtime promotion; gait quality, pivots and timing need owner review']}
    (OUT / 'inventory.json').write_text(json.dumps(result, indent=2) + '\n')
    print(f'Rendered {len(assets)} cutout gait candidates, {len(rigs)} directional rigs.')


if __name__ == '__main__':
    main()
