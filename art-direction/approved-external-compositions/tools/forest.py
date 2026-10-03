#!/usr/bin/env python3
"""Source-derived external closed-forest strip with an unchanged Clearing/gate boundary."""

import argparse
import json
import math

from PIL import Image

import common as c

START, END = (1000, 350), (650, 150)
ORIGIN, SIZE = (584, 80), (488, 336)
GATE = (744, 200)
GATE_HALF_WIDTH = 48
GATE_HALF_DEPTH = 4
TEXTURE = 'game/art/runtime/terrain/straight-path.png'
TARGETS = {'gate': (744, 200), 'midpoint': (825, 250), 'closed-end': END}
SOURCES = [TEXTURE, 'game/Main.tscn', 'game/OpeningRegionWoodlandClearingGeometry.cs',
           'game/OpeningRegionClosedForestPathGeometry.cs',
           'game/OpeningRegionClosedForestPathSceneSource.cs',
           'src/Application/Navigation/OpeningRegionClosedForestPathLayout.cs',
           'art-direction/world-layout-proposal/layout-proposal.json',
           'art-direction/production-proof/corrected/tools/build.py',
           c.PREFIX + 'scope-authorization.json', c.PREFIX + 'tools/forest.py',
           c.PREFIX + 'tools/common.py', c.PREFIX + 'tools/capture_forest.gd']


def check_geometry():
    """Bind both live anchor positions and the independent physical gate owner."""
    scene = (c.ROOT / 'game/Main.tscn').read_text()
    for name, point in (('woodland-clearing', START), ('closed-deeper-forest-path', END)):
        blocks = [b for b in scene.split('\n\n') if b.startswith(f'[node name="{name}" ')]
        c.require(len(blocks) == 1 and f'position = Vector2({point[0]}, {point[1]})' in blocks[0],
                  'retained route anchor changed')
    geometry = (c.ROOT / 'game/OpeningRegionWoodlandClearingGeometry.cs').read_text()
    c.require('ClearingTop = 200.0f' in geometry and 'ClosedForestPathOpeningX = 744.0f' in geometry
              and 'OpeningWidthPixels = 96.0f' in geometry, 'retained gate/clearing dimensions changed')
    owner = (c.ROOT / 'game/OpeningRegionClosedForestPathGeometry.cs').read_text()
    c.require('BoundaryThicknessPixels = 8.0f' in owner and 'ApproachLengthPixels = 84.0f' in owner,
              'physical gate/approach changed')


def raw_strip():
    """Use the frozen finite-strip sampler and canonicalize only invisible RGB."""
    builder = c.load('forest_builder', 'art-direction/production-proof/corrected/tools/build.py')
    dx, dy = END[0] - START[0], END[1] - START[1]
    result = builder.render_path_leg(c.rgba(TEXTURE), SIZE,
                                     (START[0] - ORIGIN[0], START[1] - ORIGIN[1]),
                                     math.degrees(math.atan2(dy, dx)), math.hypot(dx, dy))
    result.putdata([p if p[3] else (0, 0, 0, 0) for p in result.get_flattened_data()])
    return result


def allowed(wx, wy):
    """Keep the entire Clearing and 8px gate band clear; restrict the adjacent gate port."""
    return wy < GATE[1] - GATE_HALF_DEPTH and (
        wy < GATE[1] - 2 * GATE_HALF_DEPTH or abs(wx - GATE[0]) < GATE_HALF_WIDTH)


def bounded(raw):
    """Clip only presentation pixels, retaining all allowed source texels exactly."""
    result = raw.copy()
    pixels = result.load()
    for y in range(SIZE[1]):
        for x in range(SIZE[0]):
            if not allowed(ORIGIN[0] + x + .5, ORIGIN[1] + y + .5):
                pixels[x, y] = (0, 0, 0, 0)
    return result


def validate(candidate, raw):
    """Independently require clearing/gate transparency and exact texels outside the clip."""
    c.require(candidate.mode == raw.mode == 'RGBA' and candidate.size == raw.size == SIZE,
              'forest overlay geometry changed')
    removed = 0
    for y in range(SIZE[1]):
        for x in range(SIZE[0]):
            wx, wy = ORIGIN[0] + x + .5, ORIGIN[1] + y + .5
            before, after = raw.getpixel((x, y)), candidate.getpixel((x, y))
            prohibited = wy >= 196 or (wy >= 192 and not 696 <= wx < 792)
            c.require(after == ((0, 0, 0, 0) if prohibited else before),
                      'forest clip changed source pixels or escaped protected region')
            c.require(after[3] or after == (0, 0, 0, 0), 'forest hidden RGB')
            if x in (0, SIZE[0] - 1) or y in (0, SIZE[1] - 1):
                c.require(before[3] == after[3] == 0, 'forest canvas clips source strip')
            removed += bool(prohibited and before[3])
    c.require(removed > 0 and candidate.getchannel('A').getbbox() is not None, 'empty forest clip')
    return removed


def captures():
    """Bind the full 54-view matrix to its current renderer/source digests without fabricating captures."""
    record_path = c.HERE / 'forest/capture-evidence.json'
    c.require(record_path.exists(), 'capture evidence missing; run the explicit capture script first')
    record = json.loads(record_path.read_text())
    context = json.loads((c.HERE / 'forest/capture-context.json').read_text())
    c.require(context['manifest_sha256'] == c.sha(c.HERE / 'forest/manifest.json'),
              'captures predate current manifest/renderer')
    c.require(context['overlay_sha256'] == {
        name: c.sha(c.HERE / 'forest/exports' / name) for name in ('raw-strip.png', 'bounded-strip.png')},
        'captures predate current overlays')
    expected = {f'{kind}-{target}-{zoom:.1f}x-{frame}.png'
                for kind in ('retained', 'raw', 'bounded') for target in TARGETS
                for zoom in (.8, 1., 2.) for frame in ('wide', 'tall')}
    c.require(set(record['images']) == expected, 'capture matrix changed')
    c.require(record['inputs'] == {name: c.sha(c.ROOT / name) for name in SOURCES},
              'capture inputs changed')
    for name, digest in record['images'].items():
        path = c.HERE / 'forest/captures' / name
        c.require(c.sha(path) == digest, f'capture changed: {name}')
        with Image.open(path) as image:
            c.require(image.size == (1280, 827 if name.endswith('tall.png') else 720)
                      and image.convert('RGBA').getchannel('A').getextrema() == (255, 255),
                      f'capture geometry/opacity: {name}')
    print('PASS: 54 explicit disposable-scene captures; source and renderer digests bound')


def run(build=False, check_captures=False, record_captures=False):
    """Reproduce raw/bounded exports, rejection comparison and editable 2x strip master."""
    baseline = c.scope(build)
    check_geometry()
    raw = raw_strip()
    candidate = bounded(raw)
    removed = validate(candidate, raw)
    layers = [('bounded_source_strip', candidate, (0, 0))]
    evidence = {
        'protected_file_count': len(baseline), 'route_start': START, 'route_end': END,
        'route_width': 96, 'physical_gate_center': GATE, 'gate_boundary_width': 8,
        'route_crossing_at_clearing_top': [737.5, 200], 'centerline_gate_offset_pixels': 6.5,
        'removed_visible_pixels': removed, 'retained_clearing_pixels_changed_by_bounded_overlay': 0,
        'physical_owners_changed': 0, 'status': 'external presentation proof; visual assessment pending',
        'limitations': 'closed-side strip only; disposable views hide the route line; physical approach/gate remain intact',
    }
    images = {'exports/raw-strip.png': raw, 'exports/bounded-strip.png': candidate,
              'review/raw-2x.png': raw.resize((SIZE[0] * 2, SIZE[1] * 2), Image.Resampling.NEAREST),
              'review/bounded-2x.png': candidate.resize((SIZE[0] * 2, SIZE[1] * 2), Image.Resampling.NEAREST)}
    c.write_artifacts('forest', ORIGIN, SIZE, layers, images, evidence, SOURCES, build)
    if record_captures:
        context = json.loads((c.HERE / 'forest/capture-context.json').read_text())
        c.require(context['manifest_sha256'] == c.sha(c.HERE / 'forest/manifest.json'),
                  'cannot bind stale captures to new inputs; recapture first')
        names = [f'{kind}-{target}-{zoom:.1f}x-{frame}.png'
                 for kind in ('retained', 'raw', 'bounded') for target in TARGETS
                 for zoom in (.8, 1., 2.) for frame in ('wide', 'tall')]
        record = {'inputs': {name: c.sha(c.ROOT / name) for name in SOURCES},
                  'images': {name: c.sha(c.HERE / 'forest/captures' / name) for name in names}}
        (c.HERE / 'forest/capture-evidence.json').write_text(json.dumps(record, indent=2) + '\n')
    if check_captures or record_captures:
        captures()
    print(f'PASS: closed-side forest proof; {removed} raw strip pixels clipped; physical gate/clearing retained')


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--build', action='store_true')
    parser.add_argument('--check-captures', action='store_true')
    parser.add_argument('--record-captures', action='store_true')
    args = parser.parse_args()
    run(args.build, args.check_captures, args.record_captures)
