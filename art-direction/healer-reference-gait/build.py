#!/usr/bin/env python3
"""Re-pose the Healer's approved side-walk paint to the reference gait model.

The reference gait model (art-direction/reference-gait-model.md) was accepted
for the Seeker on 2026-10-07. This builder applies the same rules to the
Healer's side views (west/east) using only her owner-approved characters-batch
paint: the neutral idle body with the legs erased below the skirt hem, and the
boot cut from her first walk study. Nothing is generated or repainted.

How one pose is built (all lengths in the 2x master canvas, 256 x 384):

1. Each leg has a phase in [0, 1). Leg B runs half a cycle after leg A.
   Phases up to STANCE_END are stance (foot on the ground), the rest is swing.
2. foot_path() gives the foot's forward offset, lift and pitch for that phase:
   heel contact with the toe raised, a flat planted sole sliding back under the
   body, a trailing toe-off, then a lifted swing that eases forward.
3. The boot is rotated about its ankle by the pitch and lowered until its
   lowest solid paint touches the floor row (or the floor minus the lift).
4. The body (hip) height follows the supporting legs only: a straight support
   leg cannot reach further than THIGH + SHIN, so the body dips slightly when
   the feet are spread (double support) and is at its idle height mid-stance.
5. solve_leg() places the knee with a two-bone hinge that only flexes
   backward. Thigh and knee sit under the skirt; only the shin and boot show.
6. Legs are drawn far-to-near, then the body on top, so the skirt hides the
   upper legs exactly as in the approved idle paint.

The approved front/back walks (8 x 90 ms) are carried forward unchanged.
Run this file to write exports/, masters/, review/ and candidate.json, then
run check_gait.py. It refuses to overwrite a candidate the owner has accepted.
"""

import hashlib
from io import BytesIO
import json
import math
from pathlib import Path
import sys

from PIL import Image, ImageChops, ImageDraw, ImageFilter

HERE = Path(__file__).resolve().parent
REPO = HERE.parents[1]
BATCH = REPO / 'art-direction/characters-batch'
sys.path.insert(0, str(BATCH))
from build_review import board, write_master  # noqa: E402  (needs the path above)
from polish_walk import ankle as boot_shaft, prepare_polished  # noqa: E402

SIDES = ('west', 'east')
SIDE_FRAMES = 16          # sixteen 45 ms poses = the model's 720 ms cycle
FRAME_MS = 45
STANCE_END = 0.6          # 60/40 stance/swing split, as in the accepted Seeker gait
PELVIS_X = 128.0          # both thighs pivot at one hidden pelvis point (2x canvas)
THIGH = 41.0              # fixed bone lengths (2x); thigh + shin is the leg reach
SHIN = 41.0
CONTACT_FORWARD = 22.0    # foot ahead of the pelvis at heel contact (2x)
TOE_OFF_FORWARD = -26.0   # foot behind the pelvis at toe-off (2x)
SWING_LIFT = 12.0         # peak swing clearance above the floor (2x)
BONE_WIDTH = 26           # connector width for the shin under the hem (2x)
# First row (2x boot-cut coordinates) of the boot's own shaft. The approved cut
# also holds a strip of skirt hem and stocking above it, which would poke out
# below the skirt once the boot rotates; those rows are cleared, not repainted.
BOOT_TOP = {'west': 3, 'east': 12}
SOLID_ALPHA = 96          # paint stronger than this counts as the sole (ignores fringe)
EXPORT_SIZE = (128, 192)
RUNTIME = REPO / 'game/art/runtime/characters/healer'

# Toe-up pitch in degrees through one leg cycle: heel contact, flat, toe-off,
# toes trailing in early swing, level in mid swing, toe raised for contact.
PITCH_KNOTS = ((0.0, 12.0), (0.1, 0.0), (0.4, 0.0), (STANCE_END, -12.0),
               (0.72, -8.0), (0.86, 0.0), (1.0, 12.0))


def interpolate(knots, phase):
    """Piecewise-linear lookup in (phase, value) knots sorted by phase."""
    for (start, value), (end, following) in zip(knots, knots[1:]):
        if phase <= end:
            weight = (phase - start) / (end - start)
            return value * (1 - weight) + following * weight
    return knots[-1][1]


def foot_pitch(phase):
    """Toe-up foot angle in degrees (positive = toe raised, heel down)."""
    return interpolate(PITCH_KNOTS, phase % 1.0)


def foot_path(phase):
    """Forward offset, lift and support state of one foot at a leg phase.

    Stance slides the planted foot back at constant speed (the body moves over
    it). Swing eases the foot forward with a smoothstep, so it starts slowly
    while the knee folds and advances first, then extends into the next contact.
    """
    phase %= 1.0
    if phase <= STANCE_END:
        forward = CONTACT_FORWARD + (TOE_OFF_FORWARD - CONTACT_FORWARD) * phase / STANCE_END
        return {'phase': phase, 'forward': forward, 'lift': 0.0, 'supporting': True}
    swing = (phase - STANCE_END) / (1 - STANCE_END)
    progress = swing * swing * (3 - 2 * swing)
    lift = SWING_LIFT * math.sin(math.pi * swing) ** 0.65
    if swing > 0.65:
        # Lower the foot over the last third so it meets the ground at contact.
        lift *= (1 - swing) / 0.35
    forward = TOE_OFF_FORWARD + (CONTACT_FORWARD - TOE_OFF_FORWARD) * progress
    return {'phase': phase, 'forward': forward, 'lift': lift, 'supporting': False}


def solve_leg(direction, hip, ankle):
    """Place the knee for a fixed-length two-bone leg that bends only backward.

    "Backward" means the shin folds behind the thigh, so the knee points in the
    walking direction. Returns the knee point and the flexion (0 = straight).
    """
    sign = 1 if direction == 'east' else -1
    dx, dy = sign * (ankle[0] - hip[0]), ankle[1] - hip[1]
    reach = math.hypot(dx, dy)
    if not abs(THIGH - SHIN) <= reach <= THIGH + SHIN + 1e-6:
        raise ValueError(f'Ankle {ankle} outside the fixed leg reach from {hip}')
    cosine = (reach * reach - THIGH ** 2 - SHIN ** 2) / (2 * THIGH * SHIN)
    flexion = math.acos(max(-1.0, min(1.0, cosine)))
    thigh_angle = math.atan2(dx, dy) + math.atan2(SHIN * math.sin(flexion),
                                                  THIGH + SHIN * math.cos(flexion))
    knee = (hip[0] + sign * THIGH * math.sin(thigh_angle), hip[1] + THIGH * math.cos(thigh_angle))
    # Re-derive the ankle from the bones so both lengths hold exactly.
    shin_angle = thigh_angle - flexion
    exact_ankle = (knee[0] + sign * SHIN * math.sin(shin_angle), knee[1] + SHIN * math.cos(shin_angle))
    return {'knee': knee, 'ankle': exact_ankle, 'knee_flexion_degrees': math.degrees(flexion),
            'thigh_angle_degrees': math.degrees(thigh_angle)}


def solid_bounds(image):
    """Bounding box of solid paint, ignoring the faint generated fringe."""
    return image.getchannel('A').point(lambda alpha: 255 if alpha > SOLID_ALPHA else 0).getbbox()


def intrinsic_pitch(boot, direction):
    """Toe-up angle at which the boot was painted.

    The approved boot cut is the leading foot of a stride study, so it is drawn
    tipped onto its heel. A flat floor would touch the boot at its lowest point
    and along the shallowest line from there towards the toe (the tangent under
    the sole). That line's angle is the painted pitch; the rig removes it so the
    sole is exactly flat at mid-stance.
    """
    alpha = boot.getchannel('A').load()
    left, _top, right, _bottom = solid_bounds(boot)
    profile = {}
    for x in range(left, right):
        rows = [y for y in range(boot.height) if alpha[x, y] > SOLID_ALPHA]
        if rows:
            profile[x] = max(rows)
    low_x = max(profile, key=lambda x: (profile[x], -x if direction == 'east' else x))
    toe = 1 if direction == 'east' else -1
    # Ignore points right next to the lowest one: their tiny run exaggerates the slope.
    reach = (right - left) / 4
    slopes = [(profile[low_x] - y) / (toe * (x - low_x)) for x, y in profile.items()
              if toe * (x - low_x) >= reach]
    return math.degrees(math.atan(min(slopes)))


def place_boot(boot, pivot, ankle, rotation, size):
    """Draw the boot with its ankle pivot at `ankle`, rotated counter-clockwise.

    The rotation happens on a padded local canvas centred on the pivot, so no
    paint is clipped by the frame edge before it is rotated.
    """
    radius = math.ceil(math.hypot(*boot.size)) + 2
    local = Image.new('RGBA', (2 * radius, 2 * radius))
    local.alpha_composite(boot, (round(radius - pivot[0]), round(radius - pivot[1])))
    local = local.rotate(rotation, Image.Resampling.BICUBIC, center=(radius, radius))
    layer = Image.new('RGBA', size)
    # The layer is empty, so a plain paste (which clips at the edges) is exact.
    layer.paste(local, (round(ankle[0]) - radius, round(ankle[1]) - radius))
    return layer


def bone_layer(size, points, texture):
    """Shin/thigh connector: the approved leather patch stretched once over rounded bones."""
    mask = Image.new('L', size)
    draw = ImageDraw.Draw(mask)
    draw.line(points, fill=255, width=BONE_WIDTH, joint='curve')
    radius = BONE_WIDTH / 2
    for x, y in points:
        draw.ellipse((x - radius, y - radius, x + radius, y + radius), fill=255)
    left, top, right, bottom = mask.getbbox()
    layer = Image.new('RGBA', size)
    layer.paste(texture.resize((right - left, bottom - top), Image.Resampling.BICUBIC), (left, top))
    layer.putalpha(mask)
    return layer


def clean_boot(boot, top_row):
    """Boot paint only: clear the rows above its shaft and the faint haze around it.

    Generated paint carries a faint rectangular haze. Unrotated it is invisible,
    but rotated it shows as a pale parallelogram, so alpha is kept only within
    two pixels of solid paint (enough for the anti-aliased outline).
    """
    alpha = boot.getchannel('A')
    ImageDraw.Draw(alpha).rectangle((0, 0, boot.width, top_row - 1), fill=0)
    near_solid = alpha.point(lambda value: 255 if value > SOLID_ALPHA else 0).filter(ImageFilter.MaxFilter(5))
    result = boot.copy()
    result.putalpha(ImageChops.multiply(alpha, near_solid))
    return result


def render(direction, diagnostic=False):
    """Render the sixteen side poses; returns frames, rig records and named layers."""
    body, boots, textures, _sockets, hem, floor, _sources = prepare_polished('healer', direction)
    boots = [clean_boot(boot, BOOT_TOP[direction]) for boot in boots]
    size = body.size
    sign = 1 if direction == 'east' else -1
    # Ankle pivot: middle of the boot shaft, a little below its top edge.
    pivots = []
    for boot in boots:
        shaft_x, _width = boot_shaft(boot)
        pivots.append((shaft_x, round(boot.height * 0.45)))
    painted = [intrinsic_pitch(boot, direction) for boot in boots]

    def boot_at(index, pitch, ankle):
        return place_boot(boots[index], pivots[index], ankle, sign * (pitch - painted[index]), size)

    def sole_depth(index, pitch):
        """Distance from the ankle pivot down to the lowest solid boot row at a pitch."""
        probe = (size[0] / 2, size[1] / 2)
        return solid_bounds(boot_at(index, pitch, probe))[3] - 1 - probe[1]

    reach = THIGH + SHIN
    # Rest hip: straight leg above a flat foot directly under the pelvis (the idle pose).
    rest_hip_y = min(floor - sole_depth(index, 0.0) for index in (0, 1)) - reach
    frames, records, editable = [], [], []
    for step in range(SIDE_FRAMES + 1):
        phase = step / SIDE_FRAMES
        feet = []
        for index in (0, 1):
            path = foot_path(phase + 0.5 * index)
            pitch = foot_pitch(path['phase'])
            ankle_x = PELVIS_X + sign * path['forward']
            ankle_y = floor - sole_depth(index, pitch) - path['lift']
            feet.append({**path, 'pitch': pitch, 'ankle': [ankle_x, ankle_y]})
        # Support legs set the body height; ceil keeps every support foot within reach.
        hip_y = max(foot['ankle'][1] - math.sqrt(reach ** 2 - (foot['ankle'][0] - PELVIS_X) ** 2)
                    for foot in feet if foot['supporting'])
        bob = max(0, math.ceil(hip_y - rest_hip_y - 1e-9))
        hip = (PELVIS_X, rest_hip_y + bob)
        layers, legs = [], []
        for index, foot in enumerate(feet):
            if not foot['supporting']:
                # A swing foot the leg cannot reach is lifted higher, never stretched.
                limit = hip[1] + math.sqrt((reach - 0.5) ** 2 - (foot['ankle'][0] - PELVIS_X) ** 2)
                extra = max(0.0, foot['ankle'][1] - limit)
                foot['ankle'][1] -= extra
                foot['lift'] += extra
            leg = solve_leg(direction, hip, tuple(foot['ankle']))
            layer = bone_layer(size, [hip, leg['knee'], leg['ankle']], textures[index])
            boot_paint = boot_at(index, foot['pitch'], leg['ankle'])
            layer.alpha_composite(boot_paint)
            if diagnostic:
                tint = Image.new('RGBA', size, '#d13d38' if index == 0 else '#2879cd')
                tint.putalpha(layer.getchannel('A'))
                layer = tint
            layers.append(layer)
            legs.append({'leg': 'AB'[index], 'anatomical_side': ('left', 'right')[index],
                         'leg_phase': foot['phase'], 'supporting': foot['supporting'],
                         'forward_2x': foot['forward'], 'lift_2x': foot['lift'],
                         'foot_pitch_degrees': foot['pitch'], 'hip_2x': list(hip),
                         'knee_2x': list(leg['knee']), 'ankle_2x': list(leg['ankle']),
                         'knee_flexion_degrees': leg['knee_flexion_degrees'],
                         'sole_row_2x': solid_bounds(boot_paint)[3] - 1})
        # Far leg first (east: A is far; west: B is far), then the body hides the hips.
        order = [0, 1] if direction == 'east' else [1, 0]
        frame = Image.new('RGBA', size)
        for index in order:
            frame.alpha_composite(layers[index])
        lowered = Image.new('RGBA', size)
        lowered.alpha_composite(body, (0, bob))
        frame.alpha_composite(lowered)
        frames.append(frame)
        editable.append(lowered)
        records.append({'index': step + 1, 'phase_degrees': step * 360 / SIDE_FRAMES,
                        'body_bob_2x': bob, 'floor_2x': floor, 'hem_2x': hem,
                        'layer_order': order, 'legs': legs})
    if frames[0].tobytes() != frames[-1].tobytes():
        raise ValueError(f'Loop endpoint differs: {direction}')
    return frames[:-1], records[:-1], editable[:-1]


def export_image(frame):
    """Native-size export, downsampled from the 2x master as the approved batch was."""
    return frame.resize(EXPORT_SIZE, Image.Resampling.LANCZOS)


def export_bytes(frame):
    buffer = BytesIO()
    export_image(frame).save(buffer, format='PNG')
    return buffer.getvalue()


def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def review(sides):
    """Old-vs-new GIF at normal speed and half-speed leg-identity diagnostic."""
    scale = 2
    old = {d: [Image.open(RUNTIME / d / f'walk-{i:02d}.png').convert('RGBA') for i in range(1, 9)]
           for d in SIDES}
    diagnostic = {d: render(d, diagnostic=True)[0] for d in SIDES}
    pictures, overlays = [], []
    for index in range(SIDE_FRAMES):
        sprites = []
        for direction in SIDES:
            # The approved runtime walk holds each of its 8 poses for 90 ms.
            sprites += [old[direction][index // 2], export_image(sides[direction][index])]
        sprites = [s.resize((s.width * scale, s.height * scale), Image.Resampling.NEAREST) for s in sprites]
        pictures.append(board('HEALER / side walk: approved (left) vs reference gait (right)', sprites,
                              ['west now', 'west new', 'east now', 'east new'], 4, (290, 440)))
        tinted = [export_image(diagnostic[d][index]) for d in SIDES]
        tinted = [t.resize((t.width * scale, t.height * scale), Image.Resampling.NEAREST) for t in tinted]
        overlays.append(board('HEALER / leg identity: A (left) red, B (right) blue', tinted,
                              ['west', 'east'], 2, (290, 440)))
    pictures[0].save(HERE / 'review/healer-sides-old-vs-new.gif', save_all=True,
                     append_images=pictures[1:], duration=FRAME_MS, loop=0, disposal=2)
    overlays[0].save(HERE / 'review/healer-leg-identity-half-speed.gif', save_all=True,
                     append_images=overlays[1:], duration=FRAME_MS * 2, loop=0, disposal=2)
    poses = [export_image(f) for d in SIDES for f in sides[d]]
    labels = [f'{d} {i * 360 // SIDE_FRAMES}' for d in SIDES for i in range(SIDE_FRAMES)]
    board('HEALER / all 32 side poses', poses, labels, SIDE_FRAMES // 2, (150, 220)).save(
        HERE / 'review/all-side-poses.png')


def main():
    """Write the candidate package; never overwrite an owner-accepted one."""
    if (HERE / 'owner-acceptance.json').exists():
        raise SystemExit('Accepted candidate: author a separate revision instead of overwriting it')
    for folder in ('exports', 'masters', 'review'):
        (HERE / folder).mkdir(exist_ok=True)
    sides, rigs, exports = {}, {}, {}
    for direction in SIDES:
        frames, records, _layers = render(direction)
        sides[direction], rigs[direction] = frames, records
        for index, frame in enumerate(frames, 1):
            name = f'healer-{direction}-walk-{index:02d}'
            (HERE / 'exports' / f'{name}.png').write_bytes(export_bytes(frame))
            write_master(frame, HERE / 'masters' / f'{name}.ora', name)
            exports[f'{name}.png'] = sha(HERE / 'exports' / f'{name}.png')
    carried = {str(path.relative_to(REPO)): sha(path)
               for direction in ('south', 'north') for path in sorted((RUNTIME / direction).glob('walk-*.png'))}
    review(sides)
    candidate = {
        'date': '2026-11-06', 'status': 'candidate for owner visual review',
        'model': 'art-direction/reference-gait-model.md', 'method': 'code rig of approved characters-batch paint',
        'frames_per_direction': {'south': 8, 'west': SIDE_FRAMES, 'east': SIDE_FRAMES, 'north': 8},
        'frame_ms': {'south': 90, 'west': FRAME_MS, 'east': FRAME_MS, 'north': 90}, 'cycle_ms': 720,
        'rig_2x': {'pelvis_x': PELVIS_X, 'thigh': THIGH, 'shin': SHIN, 'contact_forward': CONTACT_FORWARD,
                   'toe_off_forward': TOE_OFF_FORWARD, 'swing_lift': SWING_LIFT, 'stance_end': STANCE_END,
                   'pitch_knots': PITCH_KNOTS},
        'exports': exports, 'carried_forward': carried, 'rigs': rigs,
        'limitations': ['Cutout paint: rigid upper body, arms and skirt; only the body height moves',
                        'Thighs and knees are hidden by the skirt; the shin is the approved leather patch'],
    }
    (HERE / 'candidate.json').write_text(json.dumps(candidate, indent=2) + '\n')
    print(f'Wrote {len(exports)} side exports and masters; front/back carried forward ({len(carried)} frames)')


if __name__ == '__main__':
    main()
