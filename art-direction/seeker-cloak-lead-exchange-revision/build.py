#!/usr/bin/env python3
"""Scissor the thighs from the painted pelvis and allow backward-only knee hinges."""

import importlib.util
import json
import math
from pathlib import Path
import shutil

from PIL import Image, ImageChops, ImageDraw

REPO = Path(__file__).resolve().parents[2]
OUT = Path(__file__).resolve().parent
PREVIOUS = REPO / 'art-direction/seeker-cloak-pelvis-hinge-revision'
GAME = REPO / 'game/art/seeker-cloak-lead-exchange-revision'
spec = importlib.util.spec_from_file_location('side_recipe', REPO / 'art-direction/seeker-cloak-gait-side-revision/build.py')
side = importlib.util.module_from_spec(spec)
spec.loader.exec_module(side)
old, recipe = side.old, side.recipe
DIRECTIONS = old.DIRECTIONS
require, sha, bounds = old.require, old.sha, old.bounds
# These are centres of the painted pants below the tunic, not canvas centres.
PELVIS = {'west': (123.0, 210.0), 'east': (165.0, 210.0)}
PELVIC_CONTOUR = {'west': (93, 189, 153, 238), 'east': (135, 189, 195, 238)}
TOP_PAD = 24
THIGH_LENGTH = 44.0
# Contact, loading, passing, terminal stance, toe-off, early swing,
# advancing swing, extended swing. The opposite leg uses the half-cycle offset.
THIGH_POSES = (32, 12, -8, -26, -32, -8, 26, 34)
KNEE_POSES = (0, 5, 0, 8, 18, 48, 28, 8)
FOOT_POSES = (-10, 0, 0, 8, 15, 5, -6, -10)


def pose_angle(values, theta):
    """Interpolate periodic positive hinge poses without overshoot or identity swaps."""
    phase = (theta / math.tau * 8) % 8
    index = math.floor(phase)
    weight = phase - index
    return values[index] * (1 - weight) + values[(index + 1) % 8] * weight


def paint_centroid(image):
    """Measure the actual substantial foot alpha, independent of the ankle coordinate."""
    left, top, right, bottom = bounds(image)
    values = list(image.getchannel('A').crop((left, top, right, bottom)).tobytes())
    width = right - left
    values = [value if value > 96 else 0 for value in values]
    mass = sum(values)
    require(mass > 0, 'Foot paint vanished')
    return [left + sum((i % width) * value for i, value in enumerate(values)) / mass,
            top + sum((i // width) * value for i, value in enumerate(values)) / mass]


def paint_parts(direction):
    """Retain original pelvic paint with a rounded lower contour over the leg roots."""
    torso, boots, patches, _sockets, _hem = old.prepare(direction)
    original = old.source_paint(direction)
    contour = Image.new('L', torso.size)
    ImageDraw.Draw(contour).ellipse(PELVIC_CONTOUR[direction], fill=255)
    # Only the missing lower pelvic paint is added; tunic, cloak and hand stay above it.
    ImageDraw.Draw(contour).rectangle((0, 0, torso.width, 206), fill=0)
    pelvis = original.copy()
    pelvis.putalpha(ImageChops.multiply(original.getchannel('A'), contour))
    pelvis.alpha_composite(torso)
    return pelvis, boots, patches


def boot_parts(boot, direction):
    """Separate native calf and foot paint at the existing ankle with an overlap."""
    source_knee, source_ankle = side.BOOT_ANCHORS[direction]
    boot = boot.copy()
    connected = boot.getchannel('A').point(lambda alpha: 255 if alpha > 8 else 0)
    ImageDraw.floodfill(connected, (round(source_knee[0]), round(source_knee[1])), 128)
    boot.putalpha(ImageChops.multiply(boot.getchannel('A'), connected.point(lambda alpha: 255 if alpha == 128 else 0)))
    foot = boot.copy()
    mask = foot.getchannel('A')
    ImageDraw.Draw(mask).rectangle((0, 0, boot.width, 40), fill=0)
    foot.putalpha(mask)
    shin = boot.copy()
    mask = Image.new('L', boot.size)
    draw = ImageDraw.Draw(mask)
    draw.rectangle((0, 0, boot.width, 24), fill=255)
    draw.line([source_knee, source_ankle], fill=255, width=29)
    draw.rectangle((0, 45, boot.width, boot.height), fill=0)
    shin.putalpha(ImageChops.multiply(shin.getchannel('A'), mask))
    patch = old.opaque_patch(boot.crop((int(source_ankle[0] - 12), int(source_ankle[1] - 18),
                                      int(source_ankle[0] + 12), int(source_ankle[1] - 4))))
    return shin, foot, patch


def joints(direction, theta, size, foot):
    """Forward kinematics: hip angle controls thigh; knee bends backward from that line."""
    sign = 1 if direction == 'east' else -1
    travel, swing = math.cos(theta), max(0.0, -math.sin(theta))
    thigh_degrees = pose_angle(THIGH_POSES, theta)
    knee_degrees = pose_angle(KNEE_POSES, theta)
    alpha, beta = math.radians(thigh_degrees), math.radians(knee_degrees)
    length = math.dist(*side.BOOT_ANCHORS[direction])
    knee = (sign * THIGH_LENGTH * math.sin(alpha), THIGH_LENGTH * math.cos(alpha))
    ankle = (knee[0] + sign * length * math.sin(alpha - beta),
             knee[1] + length * math.cos(alpha - beta))
    neutral = math.radians(25 if direction == 'east' else -22)
    foot_angle = neutral + math.radians(sign * pose_angle(FOOT_POSES, theta))
    anchor = (size[0] / 2, 200.0)
    toe = (anchor[0] + math.cos(foot_angle), anchor[1] + math.sin(foot_angle))
    source_ankle = side.BOOT_ANCHORS[direction][1]
    at_anchor, _ = side.rotate_boot(size, foot, source_ankle,
                                   (source_ankle[0] + 1, source_ankle[1]), anchor, toe)
    sole_depth = bounds(at_anchor)[3] - anchor[1]
    return {'thigh_angle_degrees': thigh_degrees, 'knee_flexion_degrees': knee_degrees,
            'relative_knee': knee, 'relative_ankle': ankle, 'shin_length_2x': length,
            'travel': round(travel, 6), 'swing': round(swing, 6),
            'foot_angle': foot_angle, 'sole_offset': ankle[1] + sole_depth}


def textured_mask(size, points, radius, patch):
    """Join native paint around a bone or joint without a rectangular cut edge."""
    mask = Image.new('L', size)
    draw = ImageDraw.Draw(mask)
    if len(points) > 1:
        draw.line(points, width=round(2 * radius), fill=255)
    for x, y in points:
        draw.ellipse((x - radius, y - radius, x + radius, y + radius), fill=255)
    result = Image.new('RGBA', size)
    left, top, right, bottom = mask.getbbox()
    result.paste(patch.resize((right - left, bottom - top), Image.Resampling.BICUBIC), (left, top))
    result.putalpha(mask)
    return result


def render(direction, diagnostic=False):
    """Render complete one-way hinges, preserving accepted south/north exactly."""
    if direction not in PELVIS:
        return old.render(direction, diagnostic)
    body_paint, boots, patches = paint_parts(direction)
    size = (body_paint.width, body_paint.height + TOP_PAD)
    parts = [boot_parts(boot, direction) for boot in boots]
    frames, records, editable = [], [], []
    for step in range(9):
        phase = math.tau * step / 8
        geometry = [joints(direction, phase + index * math.pi, size, parts[index][1]) for index in range(2)]
        supporting = 0 if math.sin(phase) >= -1e-9 else 1
        bob = round(old.BASELINE - PELVIS[direction][1] - geometry[supporting]['sole_offset'])
        hip = (PELVIS[direction][0], PELVIS[direction][1] + TOP_PAD + bob)
        layers, legs = [], []
        for index, geometry_for_leg in enumerate(geometry):
            calf, foot, ankle_patch = parts[index]
            knee = tuple(hip[i] + geometry_for_leg['relative_knee'][i] for i in (0, 1))
            ankle = tuple(hip[i] + geometry_for_leg['relative_ankle'][i] for i in (0, 1))
            layer = textured_mask(size, [hip, knee], 15, patches[index])
            source_knee, source_ankle = side.BOOT_ANCHORS[direction]
            calf_paint, calf_angle = side.rotate_boot(size, calf, source_knee, source_ankle, knee, ankle)
            ankle_paint = textured_mask(size, [ankle], 9, ankle_patch)
            angle = geometry_for_leg['foot_angle']
            toe = (ankle[0] + math.cos(angle), ankle[1] + math.sin(angle))
            foot_paint, _ = side.rotate_boot(size, foot, source_ankle,
                                            (source_ankle[0] + 1, source_ankle[1]), ankle, toe)
            layer.alpha_composite(ankle_paint)
            layer.alpha_composite(calf_paint)
            layer.alpha_composite(foot_paint)
            if diagnostic:
                color = Image.new('RGBA', size, '#d13d38' if index == 0 else '#2879cd')
                color.putalpha(layer.getchannel('A'))
                layer = color
            layers.append(layer)
            legs.append({'leg': 'A' if index == 0 else 'B', 'anatomical_side': 'left' if index == 0 else 'right',
                         'hip_2x': list(hip), 'knee_2x': list(knee), 'ankle_2x': list(ankle),
                         'thigh_angle_degrees': geometry_for_leg['thigh_angle_degrees'],
                         'knee_flexion_degrees': geometry_for_leg['knee_flexion_degrees'],
                         'boot_rotation_degrees': calf_angle, 'foot_rotation_degrees': math.degrees(angle),
                         'thigh_length_2x': THIGH_LENGTH, 'shin_length_2x': geometry_for_leg['shin_length_2x'],
                         'boot_box_2x': list(bounds(foot_paint)), 'supporting': index == supporting,
                         'foot_centroid_2x': paint_centroid(foot_paint),
                         'travel': geometry_for_leg['travel'], 'swing': geometry_for_leg['swing']})
        order = [0, 1] if direction == 'east' else [1, 0]
        body = Image.new('RGBA', size)
        body.alpha_composite(body_paint, (0, TOP_PAD + bob))
        merged = Image.new('RGBA', size)
        for index in order:
            merged.alpha_composite(layers[index])
        merged.alpha_composite(body)
        frames.append(merged)
        editable.append([('torso and cloak', body), ('anatomical left A', layers[0]), ('anatomical right B', layers[1])])
        records.append({'index': step + 1, 'phase_degrees': step * 45, 'body_bob_2x': bob,
                        'layer_order': order, 'legs': legs, 'supporting_leg': supporting})
    require(frames[0].tobytes() == frames[-1].tobytes(), 'Loop endpoint differs')
    return frames[:-1], records[:-1], editable[:-1]


def main():
    """Retain independent revision exports, semantic native masters and exact accepted views."""
    require(not (OUT / 'candidate.json').exists(), 'Retained candidate must not be overwritten')
    for folder in (OUT / 'exports', OUT / 'masters', GAME / 'frames'):
        folder.mkdir(parents=True, exist_ok=True)
    previous = json.loads((PREVIOUS / 'rig.json').read_text())
    assets, rigs, animations, accepted = [], [], [], {}
    for direction in DIRECTIONS:
        frames, records, layers = render(direction)
        rigs.append({'direction': direction, 'phases': records})
        identities = []
        for index, (image, record, paint) in enumerate(zip(frames, records, layers), 1):
            identity = f'seeker-cloak-{direction}-walk-{index:02d}'
            predecessor = next(row for row in previous['assets'] if row['id'] == identity)
            export, master = OUT / 'exports' / (identity + '.png'), OUT / 'masters' / (identity + '.ora')
            if direction not in PELVIS:
                for source, target in ((REPO / predecessor['export'], export), (REPO / predecessor['master'], master)):
                    shutil.copyfile(source, target)
                    accepted[str(source.relative_to(REPO))] = sha(source)
            else:
                image.resize((image.width // 2, image.height // 2), Image.Resampling.LANCZOS).save(export)
                old.master(master, image, paint, record['layer_order'])
            copied = GAME / 'frames' / export.name
            shutil.copyfile(export, copied)
            copied.with_suffix('.png.import').write_text(recipe.sidecar('res://' + str(copied.relative_to(REPO / 'game'))))
            assets.append({**predecessor, 'export': str(export.relative_to(REPO)), 'sha256': sha(export),
                           'master': str(master.relative_to(REPO)), 'master_sha256': sha(master),
                           'texture': str(copied.relative_to(REPO)), 'size': [image.width // 2, image.height // 2],
                           'pivot': [image.width // 4, (old.BASELINE + (TOP_PAD if direction in PELVIS else 0)) // 2]})
            identities.append(identity)
        animations.append({'name': 'walk_' + direction, 'frames': identities, 'frame_ms': 90, 'loop': True})
    (GAME / 'walk.tres').write_text(recipe.sprite_frames(assets, animations))
    data = {'date': '2026-10-07', 'status': 'knee/foot transfer and lead exchange revision awaiting visual selection',
            'directions': DIRECTIONS, 'frames_per_direction': 8, 'cycle_ms': 720,
            'pelvis_source_2x': PELVIS, 'pelvis_contour_2x': PELVIC_CONTOUR,
            'side_top_padding_2x': TOP_PAD, 'knee_policy': 'straight at zero; backward flexion only; no hyperextension',
            'side_pose_angles': {'thigh': THIGH_POSES, 'knee': KNEE_POSES, 'foot': FOOT_POSES},
            'source_policy': 'retained native paint with explicit hip and knee forward kinematics',
            'accepted_front_back_bindings': accepted, 'source_masters': previous['source_masters'],
            'predecessor_candidate_sha256': sha(PREVIOUS / 'candidate.json'),
            'limitations': ['Cutout paint; rigid upper body/arms', 'Isolated candidate; no Main, speed, terrain or footsteps'],
            'assets': assets, 'rigs': rigs}
    for path in (OUT / 'rig.json', GAME / 'rig.json'):
        path.write_text(json.dumps(data, indent=2) + '\n')
    print('PASS: complete side pelvis/hinge series; accepted front/back copied byte for byte')


if __name__ == '__main__':
    main()
