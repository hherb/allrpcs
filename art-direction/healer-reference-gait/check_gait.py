#!/usr/bin/env python3
"""Check the Healer side-gait candidate against the reference gait model.

The reference gait model (art-direction/reference-gait-model.md) asks for:
fixed leg identity, backward-only knees with fixed bone lengths, heel contact
then a planted sole then a trailing toe-off, a lifted swing in which the knee
advances before the foot, opposite legs half a cycle apart and brief double
support. This script re-renders the rig with build.py and checks each rule on
the actual geometry and paint, then checks that the published exports and
candidate.json still match what the builder produces.

Run: python3 art-direction/healer-reference-gait/check_gait.py
"""

import hashlib
import json
import math
from pathlib import Path
import sys

sys.path.insert(0, str(Path(__file__).resolve().parent))
import build  # noqa: E402  (needs the path above)

HERE = Path(__file__).resolve().parent
REPO = HERE.parents[1]


def require(condition, message):
    """Fail loudly: a silent pass would hide a gait regression."""
    if not condition:
        raise SystemExit('FAIL: ' + message)


def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def check_pure_functions():
    """Unit checks for the small helpers, independent of any paint."""
    knots = ((0, 10.0), (0.5, 0.0), (1, 10.0))
    require(build.interpolate(knots, 0.25) == 5.0, 'interpolate midpoint')
    require(build.interpolate(knots, 1.0) == 10.0, 'interpolate endpoint')
    # Heel strike points the toe up, mid-stance is flat, toe-off points it down.
    require(build.foot_pitch(0.0) > 0, 'contact pitch must lift the toe')
    require(build.foot_pitch(0.25) == 0, 'mid-stance sole must be flat')
    require(build.foot_pitch(build.STANCE_END) < 0, 'toe-off must lift the heel')
    # A straight-down target at full reach gives a straight leg (zero flexion).
    hip = (100.0, 100.0)
    straight = build.solve_leg('east', hip, (100.0, 100.0 + build.THIGH + build.SHIN))
    require(abs(straight['knee_flexion_degrees']) < 1e-4, 'full reach must be a straight knee')
    # A closer target bends the knee forward of the hip-ankle line (backward shin flexion).
    for direction, sign in (('east', 1), ('west', -1)):
        bent = build.solve_leg(direction, hip, (100.0, 100.0 + build.THIGH))
        require(bent['knee_flexion_degrees'] > 0, 'closer target must flex the knee')
        require(sign * (bent['knee'][0] - hip[0]) > 0, f'{direction} knee must point forward')


def check_direction(direction):
    """Check every reference-model rule on the rendered sixteen-pose cycle."""
    frames, records, _layers = build.render(direction)
    require(len(frames) == build.SIDE_FRAMES == 16, f'{direction}: sixteen poses')
    sign = 1 if direction == 'east' else -1
    double_support = 0
    for record in records:
        legs = record['legs']
        require([leg['leg'] for leg in legs] == ['A', 'B'], 'leg identity must stay A then B')
        # Opposite legs are half a cycle apart.
        offset = (legs[1]['leg_phase'] - legs[0]['leg_phase']) % 1.0
        require(abs(offset - 0.5) < 1e-9, f'{direction}: legs must be half a cycle apart')
        supports = [leg for leg in legs if leg['supporting']]
        require(supports, f'{direction} frame {record["index"]}: no supporting foot')
        double_support += len(supports) == 2
        for leg in legs:
            hip, knee, ankle = leg['hip_2x'], leg['knee_2x'], leg['ankle_2x']
            require(abs(math.dist(hip, knee) - build.THIGH) < 1e-6, 'thigh length must stay fixed')
            require(abs(math.dist(knee, ankle) - build.SHIN) < 1e-6, 'shin length must stay fixed')
            require(leg['knee_flexion_degrees'] >= -1e-9, 'knee must never hyperextend')
            if leg['supporting']:
                # A grounded sole: the lowest substantial boot paint sits on the floor row.
                require(abs(leg['sole_row_2x'] - record['floor_2x']) <= 1,
                        f'{direction} frame {record["index"]} leg {leg["leg"]}: stance sole off the ground')
            else:
                require(leg['sole_row_2x'] < record['floor_2x'] - 1,
                        f'{direction} frame {record["index"]} leg {leg["leg"]}: swing foot must lift')
        # Only support legs set the body height: the bob is never above the rest pose.
        require(record['body_bob_2x'] >= 0, f'{direction}: body rose above the idle pose')
    require(double_support >= 2, f'{direction}: needs brief double support at both handovers')
    require(double_support <= 4, f'{direction}: double support must stay brief')
    # Knee before foot: in early swing the knee has advanced further than the foot.
    for leg_index in (0, 1):
        swing = [r['legs'][leg_index] for r in records if not r['legs'][leg_index]['supporting']]
        early = swing[:len(swing) // 2]
        leads = [sign * (leg['knee_2x'][0] - leg['ankle_2x'][0]) for leg in early]
        require(all(lead > 0 for lead in leads), f'{direction}: knee must advance before the foot')
        # Heel-to-toe: the first stance pose lands on the heel, the last rolls onto the toe.
        stance = [r['legs'][leg_index] for r in sorted(records, key=lambda r: r['legs'][leg_index]['leg_phase'])
                  if r['legs'][leg_index]['supporting']]
        require(stance[0]['foot_pitch_degrees'] > 0, f'{direction}: contact must land on the heel')
        require(stance[-1]['foot_pitch_degrees'] < 0, f'{direction}: stance must end on the toe')
    return frames, records


def check_published():
    """The committed exports must be exactly what the builder renders now."""
    candidate = json.loads((HERE / 'candidate.json').read_text())
    for direction in build.SIDES:
        frames, _records = check_direction(direction)
        for index, frame in enumerate(frames, 1):
            export = HERE / 'exports' / f'healer-{direction}-walk-{index:02d}.png'
            require(export.exists(), f'missing export {export.name}')
            require(build.export_bytes(frame) == export.read_bytes(), f'{export.name} differs from the builder')
            require(candidate['exports'][export.name] == sha(export), f'{export.name} hash differs from candidate.json')
    # Front/back walks carry forward unchanged from the approved runtime frames.
    for path, digest in candidate['carried_forward'].items():
        require(sha(REPO / path) == digest, f'carried-forward frame changed: {path}')


def main():
    check_pure_functions()
    if (HERE / 'candidate.json').exists():
        check_published()
    else:
        for direction in build.SIDES:
            check_direction(direction)
    print('PASS: Healer side gait follows the reference gait model (16 x 45 ms per side)')


if __name__ == '__main__':
    main()
