#!/usr/bin/env python3
"""Build and validate the external B2 deck and joined-bank proof; never write runtime."""

import argparse
import hashlib
import importlib.util
import json
import math
from pathlib import Path
import subprocess
import sys
import tempfile
import zipfile
import xml.etree.ElementTree as ET

from PIL import Image, ImageDraw, ImageFont

ROOT = Path(__file__).resolve().parents[3]
HERE = Path(__file__).resolve().parents[1]
PREFIX = 'art-direction/b2-bridge-proof/'
BASE_COMMIT = '4ddce3e428402769d5954cfbdf14f88ff85d49f7'
ORIGIN = (432, 384)
SIZE = (512, 448)
SCALE = 2
BANK_DEPTH = 16
DECK_HALF_WIDTH = 48
DECK_HALF_LENGTH = 144
SOURCES = {
    'road': 'game/art/runtime/terrain/shrine-southern-road-path.png',
    'ground': 'art-direction/production-proof/corrected/exports/terrain/quiet-moss.png',
    'water': 'art-direction/production-proof/corrected/exports/terrain/stream-water.png',
    'bank': 'art-direction/production-proof/corrected/exports/terrain/stream-bank.png',
    'proposal': 'art-direction/western-watercourse-proposal/proposal.json',
    'layout': 'art-direction/world-layout-proposal/layout-proposal.json',
}


def require(condition, message):
    """Keep validation active even when Python runs with optimization."""
    if not condition:
        raise ValueError(message)


def sha(path):
    """Hash one artifact without modifying it."""
    return hashlib.sha256(path.read_bytes()).hexdigest()


def module(name, path):
    """Load existing repository-owned geometry and OpenRaster helpers without invoking their CLI."""
    spec = importlib.util.spec_from_file_location(name, ROOT / path)
    result = importlib.util.module_from_spec(spec)
    sys.modules[name] = result
    spec.loader.exec_module(result)
    return result


proposal = module('watercourse_proposal', 'art-direction/western-watercourse-proposal/tools/proposal.py')
builder = module('corrected_builder', 'art-direction/production-proof/corrected/tools/build.py')


def protected_inventory():
    """Preserve every tracked file except this new proof and the requested handover edit."""
    names = subprocess.check_output(['git', 'ls-files', '-z'], cwd=ROOT).decode().split('\0')
    return sorted(n for n in names if n and n != 'HANDOVER.md' and not n.startswith(PREFIX)
                  and Path(n).name != '.DS_Store')


def check_scope(build):
    """Freeze the live baseline once, then require unchanged bytes and inventory."""
    path = HERE / 'baseline.json'
    if not path.exists():
        require(build, 'baseline missing; build once at the named clean baseline')
        head = subprocess.check_output(['git', 'rev-parse', 'HEAD'], cwd=ROOT).decode().strip()
        require(head == BASE_COMMIT, 'unexpected baseline head')
        path.write_text(json.dumps({n: sha(ROOT / n) for n in protected_inventory()}, indent=2) + '\n')
    baseline = json.loads(path.read_text())
    require(set(baseline) == set(protected_inventory()), 'protected inventory changed')
    for name, digest in baseline.items():
        require(sha(ROOT / name) == digest, f'protected file changed: {name}')
    names = subprocess.check_output(['git', 'ls-files', '--others', '--exclude-standard', '-z'], cwd=ROOT)
    for name in names.decode().split('\0'):
        require(not name or name.startswith(PREFIX) or Path(name).name == '.DS_Store',
                f'unscoped addition: {name}')
    return len(baseline)


def geometry():
    """Retain the exact accepted B2 road centreline and proposed deck geometry."""
    config = json.loads((ROOT / SOURCES['proposal']).read_text())
    require(config['western_water_polygon'] == [[0, 552], [1400, 552], [1600, 452],
                                               [1600, 548], [1400, 648], [0, 648]],
            'water geometry differs from the bounded sampler')
    require(config['planning_bank_allowance'] == BANK_DEPTH, 'bank envelope changed')
    _, routes = proposal.load_geometry()
    crossing = next(m for m in proposal.measure(config, routes) if m['id'] == 'B2')
    require(crossing['deck_width'] == 96 and crossing['deck_length'] == 288, 'planning deck changed')
    a, b = routes[crossing['route']]
    length = math.dist(a, b)
    unit = ((b[0] - a[0]) / length, (b[1] - a[1]) / length)
    return config, crossing, routes, unit


def local(point, center, unit):
    """Project a world point onto deck length and transverse axes."""
    dx, dy = point[0] - center[0], point[1] - center[1]
    return dx * unit[0] + dy * unit[1], -dx * unit[1] + dy * unit[0]


def in_deck(point, crossing, unit):
    """Use the complete rail-free 96px footprint, including both abutments."""
    along, across = local(point, crossing['center'], unit)
    return abs(along) < DECK_HALF_LENGTH and abs(across) < DECK_HALF_WIDTH


def water_edges(x):
    """Evaluate the exact accepted western polygon in this proof's x range."""
    shift = max(0, x - 1400) / 2
    return 552 - shift, 648 - shift


def plank_color(along, across):
    """Draw original deterministic transverse timber planks, flush ties and grain."""
    plank = math.floor((along + DECK_HALF_LENGTH) / 16)
    phase = (along + DECK_HALF_LENGTH) % 16
    grain = 5 * math.sin(across / 4 + plank * 1.7) + 2 * math.sin(across * .9 + along / 3)
    tone = (plank * 17 % 13) - 6 + grain
    # Flush edge boards stay within the deck; there are no rails, posts or side shadows.
    edge = abs(across) > 42
    seam = phase < 1.0
    base = (144, 106, 65) if edge else (173, 136, 88)
    if seam:
        base = (91, 70, 47)
    if abs(abs(across) - 38) < .8 and 5 < phase < 6.5:
        base = (83, 74, 55)
    return tuple(max(0, min(255, round(c + tone))) for c in base) + (255,)


def layers(config, crossing, unit):
    """Render independent editable 2x layers at absolute world coordinates."""
    textures = {key: Image.open(ROOT / path).convert('RGBA')
                for key, path in SOURCES.items() if path.endswith('.png')}
    size = tuple(n * SCALE for n in SIZE)
    result = {key: Image.new('RGBA', size) for key in ('ground', 'road', 'water', 'bank', 'deck')}
    buffers = {key: [] for key in result}
    road_origin = crossing['origin']
    for y in range(size[1]):
        wy = ORIGIN[1] + (y + .5) / SCALE
        for x in range(size[0]):
            wx = ORIGIN[0] + (x + .5) / SCALE
            point = (wx, wy)
            top, bottom = water_edges(wx)
            deck = in_deck(point, crossing, unit)
            ground = textures['ground']
            buffers['ground'].append(ground.getpixel((int(wx) % ground.width, int(wy) % ground.height)))
            rx, ry = int(wx - road_origin[0]), int(wy - road_origin[1])
            road = textures['road']
            buffers['road'].append(road.getpixel((rx, ry)) if 0 <= rx < road.width and 0 <= ry < road.height
                                   else (0, 0, 0, 0))
            water = textures['water']
            buffers['water'].append(water.getpixel((int(wx) % water.width, int(wy) % water.height))
                                    if top <= wy < bottom else (0, 0, 0, 0))
            bank = textures['bank']
            bank_y = None
            if top - BANK_DEPTH <= wy < top:
                bank_y = (wy - top + BANK_DEPTH) / BANK_DEPTH
            elif bottom <= wy < bottom + BANK_DEPTH:
                bank_y = (bottom + BANK_DEPTH - wy) / BANK_DEPTH
            buffers['bank'].append(bank.getpixel((int(wx) % bank.width,
                                                  min(bank.height - 1, int(bank_y * bank.height))))
                                   if bank_y is not None and not deck else (0, 0, 0, 0))
            buffers['deck'].append(plank_color(*local(point, crossing['center'], unit))
                                   if deck else (0, 0, 0, 0))
    for key in result:
        result[key].putdata(buffers[key])
    return result


def merge(images):
    """Composite in declared back-to-front order."""
    result = Image.new('RGBA', images[0].size)
    for layer in images:
        result.alpha_composite(layer)
    return result


def exports(masters):
    """Box-filter 2x layers and clear invisible RGB in all proof exports."""
    result = {}
    for key, master in masters.items():
        image = master.resize(SIZE, Image.Resampling.BOX)
        image.putdata([p if p[3] else (0, 0, 0, 0) for p in image.get_flattened_data()])
        result[key] = image
    result['joined'] = merge(list(result.values()))
    return result


def measurements(config, crossing, routes, unit, images, count):
    """Check every road pixel and affected physical cell plus full bank and deck masks."""
    equal(images['joined'], merge([images[name] for name in
                                  ('ground', 'road', 'water', 'bank', 'deck')]), 'joined draw order')
    a, b = routes[crossing['route']]
    cells = 0
    bank_cells = 0
    road_pixels = 0
    fringe_cells = 0
    maximum_cell_corner_protrusion = 0
    for y in range(SIZE[1]):
        for x in range(SIZE[0]):
            point = (ORIGIN[0] + x + .5, ORIGIN[1] + y + .5)
            top, bottom = water_edges(point[0])
            along, across = local(point, crossing['center'], unit)
            bank_alpha = images['bank'].getpixel((x, y))[3]
            deck_alpha = images['deck'].getpixel((x, y))[3]
            if top - BANK_DEPTH <= point[1] < bottom + BANK_DEPTH and images['road'].getpixel((x, y))[3]:
                require(deck_alpha == 255, 'accepted road pixel exposed under water/bank')
                road_pixels += 1
            bank_band = top - BANK_DEPTH < point[1] < top or bottom < point[1] < bottom + BANK_DEPTH
            # Check complete joins, not just the two deck endpoints. Stay half a pixel
            # from analytic edges where the 2x box filter intentionally retains partial alpha.
            clear_of_deck = abs(along) > DECK_HALF_LENGTH + .5 or abs(across) > 48.5
            shore_interior = (top - 15.5 < point[1] < top - .5
                              or bottom + .5 < point[1] < bottom + 15.5)
            if clear_of_deck and shore_interior:
                require(bank_alpha > 0, 'gap in joined bank outside bridge opening')
            if not bank_band and min(abs(point[1] - edge) for edge in
                                     (top - 16, top, bottom, bottom + 16)) > .5:
                require(bank_alpha == 0, 'bank outside proposed shoreline')
            if bank_alpha:
                require(not (abs(along) < DECK_HALF_LENGTH - 1 and abs(across) < 47),
                        'bank intrudes into deck interior')
            if deck_alpha:
                require(abs(along) <= DECK_HALF_LENGTH + 1 and abs(across) <= 49,
                        'deck alpha outside antialias boundary')
            # Actual navigation uses an 8px grid with centers 4 mod 8.
            if (ORIGIN[0] + x) % 8 == 4 and (ORIGIN[1] + y) % 8 == 4:
                cell = (ORIGIN[0] + x, ORIGIN[1] + y)
                ct, cb = water_edges(cell[0])
                if proposal.distance(cell, a, b) <= 48 and ct - BANK_DEPTH <= cell[1] < cb + BANK_DEPTH:
                    require(proposal.inside(cell, crossing['deck_polygon']),
                            'deck omits affected retained navigation cell')
                    cell_along, cell_across = local(cell, crossing['center'], unit)
                    if abs(cell_along) < DECK_HALF_LENGTH - 1 and abs(cell_across) < 47:
                        require(bank_alpha == 0 and deck_alpha == 255,
                                'rendered bank/deck obstructs a retained interior route cell')
                    corners = [(cell[0] + dx, cell[1] + dy) for dx in (-4, 4) for dy in (-4, 4)]
                    protrusion = max(abs(local(corner, crossing['center'], unit)[1]) - 48 for corner in corners)
                    fringe_cells += protrusion > 0
                    maximum_cell_corner_protrusion = max(maximum_cell_corner_protrusion, protrusion)
                    bank_cells += 1
                    cells += ct <= cell[1] < cb
    require(cells == crossing['exposed_route_cells'], 'water cell count differs from accepted proposal')
    require(road_pixels == crossing['bank_envelope_alpha_pixels'], 'road footprint count differs from proposal')
    # Preserve the accepted conservative 8px Clearing-wall planning envelope y=446..454.
    deck_top = min(p[1] for p in crossing['deck_polygon'])
    require(deck_top > 454, 'deck reaches Clearing south wall')
    gaps = []
    for across in (-48, 48):
        # Entire deck end reaches beyond the bank envelope on BOTH sides, not just its centreline.
        points = [(crossing['center'][0] + sign * DECK_HALF_LENGTH * unit[0] - across * unit[1],
                   crossing['center'][1] + sign * DECK_HALF_LENGTH * unit[1] + across * unit[0]) for sign in (-1, 1)]
        for wx, wy in points:
            top, bottom = water_edges(wx)
            gaps.append(max(top - BANK_DEPTH - wy, wy - bottom - BANK_DEPTH))
    require(min(gaps) > 0, 'deck end does not reach dry abutment along its full width')
    return {'status': 'external B2 proof; visual assessment pending; no physical or runtime approval',
            'baseline_commit': BASE_COMMIT, 'protected_file_count': count,
            'center': crossing['center'], 'deck_width': 96, 'deck_length': 288,
            'deck_polygon': crossing['deck_polygon'], 'bank_depth': BANK_DEPTH,
            'affected_water_route_cells': cells, 'affected_water_and_bank_route_cells': bank_cells,
            'covered_accepted_road_bank_envelope_pixels': road_pixels,
            'affected_route_cells_with_corners_outside_deck': fringe_cells,
            'maximum_route_cell_corner_transverse_protrusion': maximum_cell_corner_protrusion,
            'minimum_full_width_dry_abutment_y_margin': min(gaps),
            'deck_to_clearing_south_wall_y_gap': deck_top - 454,
            'horizontal_north_bank_to_clearing_wall_y_gap': 552 - BANK_DEPTH - 454,
            'draw_order_back_to_front': ['ground', 'road', 'water', 'bank', 'deck'],
            'rails_posts_side_shadows': False, 'new_collision_or_navigation_authored': False,
            'physical_limit': 'cell-centre feasibility only; fringe cell corners extend outside deck; '
                              'actor clearance and live bank exclusions require named approval',
            'source_sha256': {key: sha(ROOT / value) for key, value in SOURCES.items()}}


def review(images, crossing, unit):
    """Provide unannotated art views and a separate labelled engineering overlay."""
    joined = images['joined']
    overlay = joined.copy()
    draw = ImageDraw.Draw(overlay)
    points = [(p[0] - ORIGIN[0], p[1] - ORIGIN[1]) for p in crossing['deck_polygon']]
    draw.line(points + points[:1], fill='#ffe079', width=2)
    draw.line((max(0, 600 - ORIGIN[0]), 454 - ORIGIN[1],
               min(SIZE[0] - 1, 1248 - ORIGIN[0]), 454 - ORIGIN[1]), fill='#f26880', width=3)
    draw.text((8, 8), 'B2 external proof - gold: 96x288 deck', fill='white', font=ImageFont.load_default(size=15))
    draw.text((8, 28), 'Pink: Clearing wall planning edge y=454', fill='white', font=ImageFont.load_default(size=13))
    return {'joined-0.8x': joined.resize((410, 358), Image.Resampling.LANCZOS),
            'joined-1.0x': joined, 'joined-2.0x': joined.resize((1024, 896), Image.Resampling.NEAREST),
            'engineering': overlay,
            'retained-road': merge([images['ground'], images['road']])}


def equal(actual, expected, name):
    """Compare all channels, including RGB under transparent pixels."""
    require(actual.mode == expected.mode and actual.size == expected.size
            and actual.tobytes() == expected.tobytes(), f'non-reproducible image: {name}')


def check_b1_acceptance():
    """Bind the owner's shown-view acceptance to the unchanged B1 image bytes."""
    record = json.loads((HERE / 'b1-visual-acceptance.json').read_text())
    expected = 'art-direction/b1-bridge-proof/review/joined-1.0x.png'
    require(record['owner_statement'] == 'Perfect!' and record['review_path'] == expected,
            'B1 acceptance scope differs from the owner response')
    require(record['review_sha256'] == sha(ROOT / expected), 'accepted B1 view changed')
    require(record['layout_runtime_physical_promotion_authorized'] is False,
            'B1 visual acceptance grants no promotion authority')


def run(build):
    """Generate into a temporary directory, then publish or verify the bounded proof outputs."""
    count = check_scope(build)
    check_b1_acceptance()
    config, crossing, routes, unit = geometry()
    masters = layers(config, crossing, unit)
    images = exports(masters)
    evidence = measurements(config, crossing, routes, unit, images, count)
    with tempfile.TemporaryDirectory() as directory:
        staged = Path(directory)
        ora = staged / 'b2-joined.ora'
        builder.create_ora(ora, tuple(n * SCALE for n in SIZE),
                           [(name, layer, 0, 0, True) for name, layer in masters.items()])
        artifact_images = {f'exports/{key}.png': value for key, value in images.items()}
        artifact_images.update({f'review/{key}.png': value for key, value in review(images, crossing, unit).items()})
        manifest = {'schema_version': 1, 'status': evidence['status'], 'master_scale': SCALE,
                    'world_origin': ORIGIN, 'export_size': SIZE, 'source_sha256': evidence['source_sha256'],
                    'b1_visual_acceptance_sha256': sha(HERE / 'b1-visual-acceptance.json'),
                    'master': {'path': 'masters/b2-joined.ora', 'sha256': sha(ora)}, 'exports': []}
        for name, image in artifact_images.items():
            target = staged / name
            target.parent.mkdir(parents=True, exist_ok=True)
            image.save(target)
            manifest['exports'].append({'path': name, 'sha256': sha(target), 'size': image.size})
            if build:
                (HERE / name).write_bytes(target.read_bytes())
            else:
                with Image.open(HERE / name) as actual:
                    equal(actual, image, name)
                require(sha(HERE / name) == sha(target), f'export bytes changed: {name}')
        # Check editable layer stack, dimensions and merged image, independently of archive digest.
        with zipfile.ZipFile(ora) as archive:
            stack = ET.fromstring(archive.read('stack.xml'))
            require((int(stack.attrib['w']), int(stack.attrib['h'])) == (1024, 896), 'master size changed')
            names = [layer.attrib['name'] for layer in stack.findall('./stack/layer')]
            require(names == list(reversed(masters)), 'master draw order changed')
            with archive.open('mergedimage.png') as stream:
                equal(Image.open(stream), merge(list(masters.values())), 'master merged image')
        if build:
            (HERE / 'masters/b2-joined.ora').write_bytes(ora.read_bytes())
        else:
            require((HERE / 'masters/b2-joined.ora').read_bytes() == ora.read_bytes(), 'master bytes changed')
        for name, payload in [('manifest.json', manifest), ('evidence.json', evidence)]:
            content = json.dumps(payload, indent=2) + '\n'
            if build:
                (HERE / name).write_text(content)
            else:
                require((HERE / name).read_text() == content, f'non-reproducible {name}')
    print(f'PASS: external B2 proof; {count} protected files; {cells_summary(evidence)}')


def cells_summary(evidence):
    """Return a concise validation summary."""
    return f"{evidence['affected_water_route_cells']} water cells; 96x288 rail-free deck"


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--build', action='store_true')
    run(parser.parse_args().build)
