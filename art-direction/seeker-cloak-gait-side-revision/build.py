#!/usr/bin/env python3
"""Articulate the side-view cloak legs while copying accepted front/back paint exactly."""

import importlib.util
import json
import math
from pathlib import Path
import shutil

from PIL import Image, ImageChops, ImageDraw

REPO = Path(__file__).resolve().parents[2]
OUT = Path(__file__).resolve().parent
GAME = REPO / 'game/art/seeker-cloak-gait-side-revision'
PREVIOUS = REPO / 'art-direction/seeker-cloak-gait-correction'


def module(name, path):
    """Load an immutable predecessor authoring recipe without changing its outputs."""
    spec = importlib.util.spec_from_file_location(name, path)
    result = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(result)
    return result


old = module('previous_cloak', PREVIOUS / 'build.py')
recipe = module('import_recipe', REPO / 'art-direction/character-animation-import-proof/build.py')
DIRECTIONS = old.DIRECTIONS
require, sha, bounds = old.require, old.sha, old.bounds
# Anchors measured on the original native 2x boot cutouts, in their local coordinates.
# The cuff and ankle must rotate together; copying the old shaft angle caused the defect.
BOOT_ANCHORS = {'west': ((60.0, 9.0), (46.0, 51.0)),
                'east': ((38.0, 9.0), (52.0, 51.0))}
THIGH_LENGTH = 51.0
STRIDE = 34.0
SWING_LIFT = 18.0
PELVIS_Y = 204.0


def solve(hip, ankle, length, sign):
    """Solve the forward-facing knee using fixed thigh and measured shin lengths."""
    dx, dy = ankle[0] - hip[0], ankle[1] - hip[1]
    distance = math.hypot(dx, dy)
    require(abs(THIGH_LENGTH - length) < distance < THIGH_LENGTH + length,
            'Unreachable articulated ankle target')
    along = (THIGH_LENGTH ** 2 - length ** 2 + distance ** 2) / (2 * distance)
    across = math.sqrt(max(0.0, THIGH_LENGTH ** 2 - along ** 2))
    knee = (hip[0] + along * dx / distance + sign * across * dy / distance,
            hip[1] + along * dy / distance - sign * across * dx / distance)
    flexion = math.degrees(math.acos((distance ** 2 - THIGH_LENGTH ** 2 - length ** 2)
                                    / (2 * THIGH_LENGTH * length)))
    return knee, flexion


def rotate_boot(size, boot, source_knee, source_ankle, knee, ankle):
    """Rigidly rotate native boot paint so its actual shin follows the knee–ankle bone."""
    source_angle = math.atan2(source_ankle[1] - source_knee[1], source_ankle[0] - source_knee[0])
    target_angle = math.atan2(ankle[1] - knee[1], ankle[0] - knee[0])
    angle = target_angle - source_angle
    cosine, sine = math.cos(angle), math.sin(angle)
    # Pillow maps destination coordinates back into the unchanged source paint.
    inverse = (cosine, sine, source_knee[0] - cosine * knee[0] - sine * knee[1],
               -sine, cosine, source_knee[1] + sine * knee[0] - cosine * knee[1])
    image = boot.transform(size, Image.Transform.AFFINE, inverse, Image.Resampling.BICUBIC)
    return image, math.degrees(angle)


def articulate(size, boot, patch, direction, hip, advance, lift, travel, swing):
    """Articulate knee and ankle independently, with heel/toe stance contact."""
    source_knee, source_ankle = BOOT_ANCHORS[direction]
    length = math.dist(source_knee, source_ankle)
    sign = 1 if direction == 'east' else -1
    # The original foot tilts up toward its toe. Correct that neutral tilt, then
    # permit heel strike / toe-off independently of knee and shin orientation.
    neutral = math.radians(25 if direction == 'east' else -22)
    foot_angle = neutral + math.radians(sign * (-8 * travel + 8 * swing))
    boot = boot.copy()
    connected = boot.getchannel('A').point(lambda alpha: 255 if alpha > 8 else 0)
    ImageDraw.floodfill(connected, (round(source_knee[0]), round(source_knee[1])), 128)
    keep = connected.point(lambda alpha: 255 if alpha == 128 else 0)
    boot.putalpha(ImageChops.multiply(boot.getchannel('A'), keep))
    foot = boot.copy()
    foot_mask = foot.getchannel('A')
    ImageDraw.Draw(foot_mask).rectangle((0, 0, boot.width, 40), fill=0)
    foot.putalpha(foot_mask)
    # A narrow calf mask avoids rotating the toe together with the shin.
    shin = boot.copy()
    calf_mask = Image.new('L', boot.size)
    calf_draw = ImageDraw.Draw(calf_mask)
    calf_draw.rectangle((0, 0, boot.width, 24), fill=255)
    calf_draw.line([source_knee, source_ankle], fill=255, width=29)
    calf_draw.rectangle((0, 45, boot.width, boot.height), fill=0)
    shin.putalpha(ImageChops.multiply(shin.getchannel('A'), calf_mask))
    anchor = (size[0] / 2, 200.0)
    toe_vector = (anchor[0] + math.cos(foot_angle), anchor[1] + math.sin(foot_angle))
    foot_at_anchor, _ = rotate_boot(size, foot, source_ankle,
                                    (source_ankle[0] + 1, source_ankle[1]), anchor, toe_vector)
    sole_depth = bounds(foot_at_anchor)[3] - anchor[1]
    ankle = (hip[0] + advance, old.BASELINE - lift - sole_depth)
    knee, flexion = solve(hip, ankle, length, sign)
    painted, rotation = rotate_boot(size, shin, source_knee, source_ankle, knee, ankle)
    toe_vector = (ankle[0] + math.cos(foot_angle), ankle[1] + math.sin(foot_angle))
    foot_paint, _ = rotate_boot(size, foot, source_ankle,
                              (source_ankle[0] + 1, source_ankle[1]), ankle, toe_vector)
    mask = Image.new('L', size)
    draw = ImageDraw.Draw(mask)
    width = 26
    draw.line([hip, knee], fill=255, width=width)
    for x, y in (hip, knee):
        draw.ellipse((x - width / 2, y - width / 2, x + width / 2, y + width / 2), fill=255)
    thigh = Image.new('RGBA', size)
    left, top, right, bottom = mask.getbbox()
    thigh.paste(patch.resize((right - left, bottom - top), Image.Resampling.BICUBIC), (left, top))
    thigh.putalpha(mask)
    thigh.alpha_composite(painted)
    # Paint the ankle under the overlapping calf/foot cutouts to avoid a split seam.
    joint_mask = Image.new('L', size)
    ImageDraw.Draw(joint_mask).ellipse((ankle[0] - 10, ankle[1] - 9, ankle[0] + 10, ankle[1] + 9), fill=255)
    joint = Image.new('RGBA', size)
    left, top, right, bottom = joint_mask.getbbox()
    boot_patch = old.opaque_patch(boot.crop((int(source_ankle[0] - 12), int(source_ankle[1] - 18),
                                          int(source_ankle[0] + 12), int(source_ankle[1] - 4))))
    joint.paste(boot_patch.resize((right - left, bottom - top), Image.Resampling.BICUBIC), (left, top))
    joint.putalpha(joint_mask)
    joint.alpha_composite(painted)
    joint.alpha_composite(foot_paint)
    thigh.alpha_composite(joint)
    return thigh, {'hip_2x': list(hip), 'knee_2x': list(knee), 'ankle_2x': list(ankle),
                   'shaft_2x': list(knee), 'boot_box_2x': list(bounds(foot_paint)),
                   'knee_flexion_degrees': flexion, 'boot_rotation_degrees': rotation,
                   'foot_rotation_degrees': math.degrees(foot_angle),
                   'thigh_length_2x': THIGH_LENGTH, 'shin_length_2x': length,
                   'source_knee_2x': list(source_knee), 'source_ankle_2x': list(source_ankle)}


def render(direction, diagnostic=False):
    """Render the revised side cycle and reuse the accepted front/back renderer."""
    if direction not in ('west', 'east'):
        return old.render(direction, diagnostic)
    torso, feet, patches, _sockets, _hem = old.prepare(direction)
    sign = 1 if direction == 'east' else -1
    frames, records, editable = [], [], []
    for step in range(9):
        phase = math.tau * step / 8
        bob = -round(2 * abs(math.sin(phase)))
        hip = (torso.width / 2, PELVIS_Y + bob)
        layers, legs = [], []
        for index in range(2):
            theta = phase + index * math.pi
            travel = math.cos(theta)
            swing = max(0.0, -math.sin(theta))
            advance = sign * STRIDE * travel
            lift = SWING_LIFT * swing
            layer, geometry = articulate(torso.size, feet[index], patches[index], direction, hip,
                                         advance, lift, travel, swing)
            if diagnostic:
                color = Image.new('RGBA', layer.size, '#d13d38' if index == 0 else '#2879cd')
                color.putalpha(layer.getchannel('A'))
                layer = color
            layers.append(layer)
            legs.append({'leg': 'A' if index == 0 else 'B',
                         'anatomical_side': 'left' if index == 0 else 'right',
                         'travel': round(travel, 6), 'advance_2x': round(advance, 6),
                         'swing_lift_2x': round(lift, 6), 'depth_2x': 0,
                         'stance': math.sin(theta) >= -1e-9, **geometry})
        order = [0, 1] if direction == 'east' else [1, 0]
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
    require(frames[0].tobytes() == frames[-1].tobytes(), 'Articulated loop does not close')
    return frames[:-1], records[:-1], editable[:-1]


def main():
    """Retain a separate full candidate with exact accepted south/north files."""
    require(not (OUT / 'candidate.json').exists(), 'Revision snapshot already retained')
    for folder in (OUT / 'exports', OUT / 'masters', GAME / 'frames'):
        folder.mkdir(parents=True, exist_ok=True)
    previous = json.loads((PREVIOUS / 'rig.json').read_text())
    assets, rigs, animations = [], [], []
    accepted = {}
    for direction in DIRECTIONS:
        frames, records, layers = render(direction)
        rigs.append({'direction': direction, 'phases': records})
        identities = []
        for index, (image, record, paint) in enumerate(zip(frames, records, layers), 1):
            identity = f'seeker-cloak-{direction}-walk-{index:02d}'
            predecessor = next(row for row in previous['assets'] if row['id'] == identity)
            exported = OUT / 'exports' / (identity + '.png')
            master = OUT / 'masters' / (identity + '.ora')
            if direction in ('south', 'north'):
                for source, target in ((REPO / predecessor['export'], exported), (REPO / predecessor['master'], master)):
                    shutil.copyfile(source, target)
                    accepted[str(source.relative_to(REPO))] = sha(source)
            else:
                image.resize((image.width // 2, image.height // 2), Image.Resampling.LANCZOS).save(exported)
                old.master(master, image, paint, record['layer_order'])
            copied = GAME / 'frames' / exported.name
            shutil.copyfile(exported, copied)
            resource = 'res://' + str(copied.relative_to(REPO / 'game'))
            copied.with_suffix('.png.import').write_text(recipe.sidecar(resource))
            asset = {**predecessor, 'export': str(exported.relative_to(REPO)), 'sha256': sha(exported),
                     'master': str(master.relative_to(REPO)), 'master_sha256': sha(master),
                     'texture': str(copied.relative_to(REPO)), 'retained_accepted_view': direction in ('south', 'north')}
            assets.append(asset)
            identities.append(identity)
        animations.append({'name': 'walk_' + direction, 'frames': identities, 'frame_ms': 90, 'loop': True})
    (GAME / 'walk.tres').write_text(recipe.sprite_frames(assets, animations))
    data = {'date': '2026-10-07', 'status': 'accepted front/back carried forward; revised sides await visual selection',
            'directions': DIRECTIONS, 'frames_per_direction': 8, 'cycle_ms': 720,
            'source_policy': 'Original cutout paint on a pelvis–knee–ankle chain; measured shin rotation; fixed bone lengths',
            'accepted_front_back_bindings': accepted,
            'predecessor_candidate_sha256': sha(PREVIOUS / 'candidate.json'),
            'source_masters': previous['source_masters'],
            'limitations': ['Cutout articulation; torso/arms remain largely rigid',
                            'No terrain contact, movement-speed matching, Main installation or footsteps'],
            'assets': assets, 'rigs': rigs}
    for path in (OUT / 'rig.json', GAME / 'rig.json'):
        path.write_text(json.dumps(data, indent=2) + '\n')
    print('PASS: sixteen revised articulated side poses; sixteen accepted front/back exports and masters copied exactly')


if __name__ == '__main__':
    main()
