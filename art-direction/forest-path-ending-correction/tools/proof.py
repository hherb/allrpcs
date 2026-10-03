#!/usr/bin/env python3
"""Round the external forest terminus and soften the rejected gate cutoff without runtime changes."""

import argparse
import importlib.util
import json
import math
from pathlib import Path
import sys

from PIL import Image

ROOT = Path(__file__).resolve().parents[3]
HERE = Path(__file__).resolve().parents[1]
OLD = ROOT / 'art-direction/approved-external-compositions'
spec = importlib.util.spec_from_file_location('ending_common', OLD / 'tools/common.py')
c = importlib.util.module_from_spec(spec)
sys.modules[spec.name] = c
spec.loader.exec_module(c)
c.HERE = HERE
c.PREFIX = HERE.relative_to(ROOT).as_posix() + '/'
c.BASE_COMMIT = 'cfba0ce63e98f6c4a4f738ddebb0f17ddcafaf1a'

START, END = (1000, 350), (650, 150)
ORIGIN, SIZE = (520, 32), (552, 384)
GATE_TOP = 196
FADE_START = 168
CAP_BLEND_END_BEFORE_END = 72
CAP_BLEND = 16
TEXTURE = 'game/art/runtime/terrain/straight-path.png'
CAP = 'game/art/runtime/terrain/path-end-cap.png'
REFERENCE = 'art-direction/approved-external-compositions/forest/captures/bounded-gate-2.0x-wide.png'
SOURCES = [TEXTURE, CAP, REFERENCE, 'game/Main.tscn',
           'game/OpeningRegionClosedForestPathGeometry.cs',
           'game/OpeningRegionWoodlandClearingGeometry.cs',
           'art-direction/production-proof/corrected/tools/build.py',
           'art-direction/approved-external-compositions/tools/common.py',
           c.PREFIX + 'owner-request.json', c.PREFIX + 'tools/proof.py', c.PREFIX + 'tools/capture.gd']


def mix(first, second, amount):
    """Blend premultiplied texels without doubling alpha or introducing dark fringes."""
    weight = first[3] * (1 - amount) + second[3] * amount
    alpha = round(weight)
    if not alpha:
        return (0, 0, 0, 0)
    return tuple(round((first[i] * first[3] * (1 - amount) + second[i] * second[3] * amount) / weight)
                 for i in range(3)) + (alpha,)


def render():
    """Keep source-defined route sampling, attach its rounded cap and ease the gate-facing alpha."""
    builder = c.load('ending_builder', 'art-direction/production-proof/corrected/tools/build.py')
    length = math.dist(START, END)
    unit = ((END[0] - START[0]) / length, (END[1] - START[1]) / length)
    right = (unit[1], -unit[0])
    local = (START[0] - ORIGIN[0], START[1] - ORIGIN[1])
    strip = builder.render_path_leg(c.rgba(TEXTURE), SIZE, local,
                                    math.degrees(math.atan2(unit[1], unit[0])), length)
    cap_entry = length - CAP_BLEND_END_BEFORE_END
    cap = c.rgba(CAP).transform(SIZE, Image.Transform.AFFINE,
        (right[0], right[1], 128 - right[0] * local[0] - right[1] * local[1],
         unit[0], unit[1], -unit[0] * local[0] - unit[1] * local[1] - cap_entry + CAP_BLEND),
        resample=Image.Resampling.BICUBIC)
    rounded, corrected = Image.new('RGBA', SIZE), Image.new('RGBA', SIZE)
    for y in range(SIZE[1]):
        for x in range(SIZE[0]):
            wx, wy = ORIGIN[0] + x + .5, ORIGIN[1] + y + .5
            along = (wx - START[0]) * unit[0] + (wy - START[1]) * unit[1]
            amount = max(0, min(1, (along - cap_entry + CAP_BLEND) / CAP_BLEND))
            pixel = mix(strip.getpixel((x, y)), cap.getpixel((x, y)), amount)
            rounded.putpixel((x, y), pixel)
            # The easing has zero slope at both ends. Alpha reaches zero before
            # the gate band, so no solid horizontal cutoff survives at y=196.
            t = max(0, min(1, (wy - FADE_START) / (GATE_TOP - FADE_START)))
            alpha = round(pixel[3] * (1 - t * t * (3 - 2 * t)))
            corrected.putpixel((x, y), pixel[:3] + (alpha,) if alpha else (0, 0, 0, 0))
    return rounded, corrected


def validate(rounded, corrected):
    """Require exact source colours, transparent gate territory and a gradual source-alpha envelope."""
    c.require(rounded.mode == corrected.mode == 'RGBA' and rounded.size == corrected.size == SIZE,
              'ending dimensions/mode changed')
    faded = 0
    for y in range(SIZE[1]):
        for x in range(SIZE[0]):
            wy = ORIGIN[1] + y + .5
            before, after = rounded.getpixel((x, y)), corrected.getpixel((x, y))
            if wy >= 196:
                c.require(after == (0, 0, 0, 0), 'gate or Clearing covered')
            elif wy <= 168:
                c.require(after == before, 'rounded source outside fade changed')
            else:
                t = (wy - 168) / 28
                expected = round(before[3] * (1 - 3 * t * t + 2 * t * t * t))
                c.require(after == (before[:3] + (expected,) if expected else (0, 0, 0, 0)),
                          'gate easing changed source colours or alpha')
                faded += bool(expected and expected < before[3])
            c.require(after[3] or after == (0, 0, 0, 0), 'hidden RGB')
            if x in (0, SIZE[0] - 1) or y in (0, SIZE[1] - 1):
                c.require(after[3] == 0, 'ending clips canvas gutter')
    c.require(faded > 0, 'no gradual gate transition')
    # The old finite terminus was flat. This cap continues beyond it with a
    # rounded alpha footprint, not a new rectangular source crop.
    c.require(corrected.getpixel((636 - ORIGIN[0], 142 - ORIGIN[1]))[3] > 0,
              'rounded terminus missing')
    return faded


def predecessors():
    """Reproduce both prior compositions under the verified superset without editing history."""
    baseline = c.scope()
    original = c.load('common', 'art-direction/approved-external-compositions/tools/common.py')
    frozen = json.loads((OLD / 'baseline.json').read_text())
    c.require(all(baseline.get(name) == digest for name, digest in frozen.items()), 'old baseline changed')
    original.scope = lambda build=False: frozen
    original.predecessors()
    channel = c.load('ending_prior_channel', 'art-direction/approved-external-compositions/tools/channel.py')
    channel.run(False)
    forest = c.load('ending_prior_forest', 'art-direction/approved-external-compositions/tools/forest.py')
    forest.run(False, True)


def run(build=False):
    """Bind the rejected owner view, deterministic correction, 2x master and preservation inventory."""
    baseline = c.scope(build)
    request = json.loads((HERE / 'owner-request.json').read_text())
    c.require(request['review_path'] == REFERENCE and request['review_sha256'] == c.sha(ROOT / REFERENCE),
              'owner correction reference changed')
    rounded, corrected = render()
    faded = validate(rounded, corrected)
    images = {'exports/rounded-reference.png': rounded, 'exports/corrected.png': corrected,
              'review/overlay-2x.png': corrected.resize((SIZE[0] * 2, SIZE[1] * 2), Image.Resampling.NEAREST)}
    evidence = {'protected_file_count': len(baseline), 'runtime_files_changed': 0,
                'retained_gate_top': 196, 'gate_fade_start': 168, 'faded_visible_pixels': faded,
                'cap_source_entry_before_old_endpoint': CAP_BLEND_END_BEFORE_END + CAP_BLEND,
                'cap_blend_end_before_old_endpoint': CAP_BLEND_END_BEFORE_END, 'cap_blend': CAP_BLEND,
                'status': 'external forest ending correction; owner visual assessment pending',
                'limit': 'source-derived rounded cap and gate alpha easing only; no physical/layout promotion'}
    c.write_artifacts('result', ORIGIN, SIZE, [('rounded_and_eased_path', corrected, (0, 0))],
                      images, evidence, SOURCES, build)
    print(f'PASS: rounded forest end; {faded} eased pixels; {len(baseline)} unchanged predecessors')


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--build', action='store_true')
    parser.add_argument('--check-predecessors', action='store_true')
    args = parser.parse_args()
    run(args.build)
    if args.check_predecessors:
        predecessors()
