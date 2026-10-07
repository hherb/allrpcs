#!/usr/bin/env python3
"""Author the supplied gait sequence using grounded stance and backward-only knee hinges."""

import importlib.util
import json
import math
from pathlib import Path
import shutil

from PIL import Image, ImageChops, ImageDraw

REPO = Path(__file__).resolve().parents[2]
OUT = Path(__file__).resolve().parent
PREVIOUS = REPO / 'art-direction/seeker-cloak-lead-exchange-revision'
GAME = REPO / 'game/art/seeker-cloak-reference-gait-revision'
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
# Art timing chosen from the supplied sequence: 60% stance, 40% swing.
# Sixteen side poses at 45ms retain the accepted front/back 720ms cycle.
SIDE_FRAMES = 16
STANCE_END = 0.6


def interpolate(knots, phase):
    """Interpolate authored cyclic foot-roll knots without angle overshoot."""
    for (start, value), (end, following) in zip(knots, knots[1:]):
        if phase <= end:
            weight = (phase - start) / (end - start)
            return value * (1 - weight) + following * weight
    return knots[-1][1]


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


def foot_target(direction, phase, size, foot):
    """Ground the stance sole; lift and advance the trailing foot during swing."""
    phase %= 1.0
    sign = 1 if direction == 'east' else -1
    pitch = interpolate(((0, -12), (.1, 0), (.4, 0), (.6, 12),
                         (.72, 8), (.86, 0), (1, -12)), phase)
    angle = math.radians((25 if sign == 1 else -22) + sign * pitch)
    anchor = (size[0] / 2, 200.0)
    source_ankle = side.BOOT_ANCHORS[direction][1]
    sample, _ = side.rotate_boot(size, foot, source_ankle,
                                (source_ankle[0] + 1, source_ankle[1]), anchor,
                                (anchor[0] + math.cos(angle), anchor[1] + math.sin(angle)))
    sole_depth = bounds(sample)[3] - anchor[1]
    support = phase <= STANCE_END
    if support:
        forward = 42 - 96 * phase / STANCE_END
        lift = 0.0
    else:
        swing = (phase - STANCE_END) / (1 - STANCE_END)
        progress = swing * swing * (3 - 2 * swing)
        forward = -54 + 96 * progress
        # Knee folds first while the heel trails, then extends for the next contact.
        lift = 17 * math.sin(math.pi * swing) ** .65
        if swing > .65:
            lift *= (1 - swing) / .35
    return {'phase': phase, 'forward': forward, 'lift': lift, 'supporting': support,
            'foot_angle': angle, 'sole_depth': sole_depth,
            'ankle_x': PELVIS[direction][0] + sign * forward,
            'ankle_y': old.BASELINE + TOP_PAD - sole_depth - lift}


def solve_leg(direction, hip, target):
    """Use the backward-knee branch of a fixed-length two-bone hinge."""
    sign = 1 if direction == 'east' else -1
    shin = math.dist(*side.BOOT_ANCHORS[direction])
    dx, dy = sign * (target['ankle_x'] - hip[0]), target['ankle_y'] - hip[1]
    reach = math.hypot(dx, dy)
    require(abs(THIGH_LENGTH - shin) < reach <= THIGH_LENGTH + shin + 1e-6,
            'Foot target outside fixed-length leg reach')
    beta = math.acos(max(-1.0, min(1.0,
        (reach * reach - THIGH_LENGTH ** 2 - shin ** 2) / (2 * THIGH_LENGTH * shin))))
    alpha = math.atan2(dx, dy) + math.atan2(shin * math.sin(beta), THIGH_LENGTH + shin * math.cos(beta))
    knee = (sign * THIGH_LENGTH * math.sin(alpha), THIGH_LENGTH * math.cos(alpha))
    ankle = (sign * dx, dy)
    return {**target, 'thigh_angle_degrees': math.degrees(alpha),
            'knee_flexion_degrees': math.degrees(beta), 'relative_knee': knee,
            'relative_ankle': ankle, 'shin_length_2x': shin,
            'travel': round(target['forward'] / 54, 6), 'swing': round(target['lift'] / 17, 6)}


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
    for step in range(SIDE_FRAMES + 1):
        phase = (step / SIDE_FRAMES) % 1.0
        targets = [foot_target(direction, phase + index * .5, size, parts[index][1]) for index in range(2)]
        supports = [index for index, target in enumerate(targets) if target['supporting']]
        reach = THIGH_LENGTH + math.dist(*side.BOOT_ANCHORS[direction])
        # Common pelvis height follows the grounded support legs. Loading softens
        # the knee slightly; the opposite swing leg never determines body height.
        hip_y = max(targets[index]['ankle_y'] - math.sqrt(reach ** 2 - targets[index]['forward'] ** 2)
                    for index in supports)
        loading = min(targets[index]['phase'] for index in supports)
        compression = 1.2 * math.sin(math.pi * min(loading / .2, 1)) if loading < .2 else 0
        bob = hip_y + compression - PELVIS[direction][1] - TOP_PAD
        hip = (PELVIS[direction][0], PELVIS[direction][1] + TOP_PAD + bob)
        for target in targets:
            if not target['supporting']:
                attainable = hip[1] + math.sqrt(reach ** 2 - target['forward'] ** 2) - .5
                extra_lift = max(0.0, target['ankle_y'] - attainable)
                target['ankle_y'] -= extra_lift
                target['lift'] += extra_lift
        geometry = [solve_leg(direction, hip, target) for target in targets]
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
                         'boot_box_2x': list(bounds(foot_paint)), 'supporting': geometry_for_leg['supporting'],
                         'leg_phase': geometry_for_leg['phase'], 'swing_lift_2x': geometry_for_leg['lift'],
                         'foot_centroid_2x': paint_centroid(foot_paint),
                         'travel': geometry_for_leg['travel'], 'swing': geometry_for_leg['swing']})
        order = [0, 1] if direction == 'east' else [1, 0]
        body = Image.new('RGBA', size)
        body = body_paint.transform(size, Image.Transform.AFFINE,
                                    (1, 0, 0, 0, 1, -TOP_PAD - bob), Image.Resampling.BICUBIC)
        merged = Image.new('RGBA', size)
        for index in order:
            merged.alpha_composite(layers[index])
        merged.alpha_composite(body)
        frames.append(merged)
        editable.append([('torso and cloak', body), ('anatomical left A', layers[0]), ('anatomical right B', layers[1])])
        records.append({'index': step + 1, 'phase_degrees': step * 360 / SIDE_FRAMES, 'body_bob_2x': bob,
                        'layer_order': order, 'legs': legs, 'supporting_legs': supports})
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
            predecessor = next(row for row in previous['assets'] if row['direction'] == direction and row['index'] == min(index, 8))
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
            assets.append({**predecessor, 'id': identity, 'index': index, 'export': str(export.relative_to(REPO)), 'sha256': sha(export),
                           'master': str(master.relative_to(REPO)), 'master_sha256': sha(master),
                           'texture': str(copied.relative_to(REPO)), 'size': [image.width // 2, image.height // 2],
                           'pivot': [image.width // 4, (old.BASELINE + (TOP_PAD if direction in PELVIS else 0)) // 2]})
            identities.append(identity)
        animations.append({'name': 'walk_' + direction, 'frames': identities, 'frame_ms': 45 if direction in PELVIS else 90, 'loop': True})
    (GAME / 'walk.tres').write_text(recipe.sprite_frames(assets, animations))
    data = {'date': '2026-10-07', 'status': 'reference-based natural side gait ready for visual selection',
            'directions': DIRECTIONS, 'frames_per_direction': {'south': 8, 'west': 16, 'east': 16, 'north': 8}, 'cycle_ms': 720,
            'pelvis_source_2x': PELVIS, 'pelvis_contour_2x': PELVIC_CONTOUR,
            'side_top_padding_2x': TOP_PAD, 'knee_policy': 'straight at zero; backward flexion only; no hyperextension',
            'side_timing': {'stance_fraction': STANCE_END, 'swing_fraction': 1-STANCE_END, 'opposite_leg_offset': .5, 'authoring_choice': 'Timing from reference sequence, not measured clinical data'},
            'animations': animations,
            'source_policy': 'retained native paint, fixed-length hip/knee hinges, grounded stance and lifted swing targets',
            'accepted_front_back_bindings': accepted, 'source_masters': previous['source_masters'],
            'predecessor_candidate_sha256': sha(PREVIOUS / 'candidate.json'),
            'limitations': ['Cutout paint; rigid upper body/arms', 'Isolated candidate; no Main, speed, terrain or footsteps'],
            'assets': assets, 'rigs': rigs}
    for path in (OUT / 'rig.json', GAME / 'rig.json'):
        path.write_text(json.dumps(data, indent=2) + '\n')
    print('PASS: sixteen-pose natural side gait series; accepted front/back copied byte for byte')


if __name__ == '__main__':
    main()
