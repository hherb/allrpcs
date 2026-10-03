#!/usr/bin/env python3
"""Build a source-bound external western outlet; never author runtime or physical state."""

import argparse
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
PREFIX = 'art-direction/western-outlet-proof/'
BASE_COMMIT = 'f25c173c0a6c3868441e233964222508c6490099'
ORIGIN = (0, 504)
SIZE = (320, 192)
SCALE = 2
TOP, BOTTOM, BANK_DEPTH = 552, 648, 16
OFF_MAP_WIDTH = 64
ORDER = ('ground', 'water', 'north_bank', 'south_bank')
SOURCES = {
    'ground': 'art-direction/production-proof/corrected/exports/terrain/quiet-moss.png',
    'water': 'art-direction/production-proof/corrected/exports/terrain/stream-water.png',
    'bank': 'art-direction/production-proof/corrected/exports/terrain/stream-bank.png',
    'proposal': 'art-direction/western-watercourse-proposal/proposal.json',
    'helpers': 'art-direction/b2-bridge-proof/tools/proof.py',
    'builder': 'art-direction/production-proof/corrected/tools/build.py',
    'geometry_helpers': 'art-direction/western-watercourse-proposal/tools/proposal.py',
    'layout': 'art-direction/world-layout-proposal/layout-proposal.json',
    'b2_view': 'art-direction/b2-bridge-proof/review/joined-1.0x.png',
    'b2_water': 'art-direction/b2-bridge-proof/exports/water.png',
    'b2_bank': 'art-direction/b2-bridge-proof/exports/bank.png',
    'spring_acceptance': 'art-direction/spring-curved-bank-proof/visual-acceptance.json',
}


def load_helpers():
    """Reuse frozen repository image, geometry and deterministic OpenRaster helpers."""
    spec = importlib.util.spec_from_file_location('outlet_helpers', ROOT / SOURCES['helpers'])
    module = importlib.util.module_from_spec(spec)
    sys.modules[spec.name] = module
    spec.loader.exec_module(module)
    return module


helpers = load_helpers()
require, sha, equal, merge = helpers.require, helpers.sha, helpers.equal, helpers.merge


def protected_inventory():
    """Protect every tracked predecessor except the explicitly editable handover."""
    names = subprocess.check_output(['git', 'ls-files', '-z'], cwd=ROOT).decode().split('\0')
    return sorted(n for n in names if n and n != 'HANDOVER.md' and not n.startswith(PREFIX)
                  and Path(n).name != '.DS_Store')


def check_scope(build):
    """Freeze the named starting commit once, and verify all predecessor bytes and paths."""
    path = HERE / 'baseline.json'
    if not path.exists():
        require(build, 'baseline missing')
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


def check_inputs():
    """Bind the frozen outlet envelope and the accepted spring image without granting promotion."""
    config = json.loads((ROOT / SOURCES['proposal']).read_text())
    require(config['outlet'] == {'kind': 'water continues beyond west world boundary', 'point': [0, 600]},
            'outlet proposal changed')
    require(config['western_water_polygon'] == [[0, 552], [1400, 552], [1600, 452],
                                               [1600, 548], [1400, 648], [0, 648]]
            and config['planning_bank_allowance'] == BANK_DEPTH, 'channel envelope changed')
    record = json.loads((ROOT / SOURCES['spring_acceptance']).read_text())
    require(record['layout_runtime_physical_promotion_authorized'] is False,
            'spring acceptance grants no promotion')
    require(sha(ROOT / record['review_path']) == record['review_sha256'], 'accepted spring image changed')


def render(origin=ORIGIN, size=SIZE):
    """Continue B2's absolute-world UV recipe, including mirrored 16px source-derived banks."""
    textures = {key: Image.open(ROOT / SOURCES[key]).convert('RGBA') for key in ('ground', 'water', 'bank')}
    require(textures['water'].size == (128, 96) and textures['bank'].size == (128, 32),
            'source dimensions changed')
    dimensions = tuple(n * SCALE for n in size)
    result = {name: Image.new('RGBA', dimensions) for name in ORDER}
    buffers = {name: [] for name in ORDER}
    for y in range(dimensions[1]):
        wy = origin[1] + (y + .5) / SCALE
        for x in range(dimensions[0]):
            wx = origin[0] + (x + .5) / SCALE
            ground, water, bank = (textures[key] for key in ('ground', 'water', 'bank'))
            buffers['ground'].append(ground.getpixel((math.floor(wx) % ground.width,
                                                       math.floor(wy) % ground.height)))
            buffers['water'].append(water.getpixel((math.floor(wx) % water.width,
                                                     math.floor(wy) % water.height))
                                    if TOP <= wy < BOTTOM else (0, 0, 0, 0))
            for name, wet_edge in (('north_bank', TOP), ('south_bank', BOTTOM)):
                depth = wy - (wet_edge - BANK_DEPTH) if name == 'north_bank' else wet_edge + BANK_DEPTH - wy
                inside = TOP - BANK_DEPTH <= wy < TOP if name == 'north_bank' else BOTTOM <= wy < BOTTOM + BANK_DEPTH
                buffers[name].append(bank.getpixel((math.floor(wx) % bank.width,
                                                       min(bank.height - 1, math.floor(depth * bank.height / BANK_DEPTH))))
                                     if inside else (0, 0, 0, 0))
    for name, image in result.items():
        image.putdata(buffers[name])
    return result


def exports(masters, size=SIZE):
    """Box-filter independent 2x layers, clear hidden RGB, then compose in declared order."""
    result = {}
    for name, image in masters.items():
        image = image.resize(size, Image.Resampling.BOX)
        image.putdata([p if p[3] else (0, 0, 0, 0) for p in image.get_flattened_data()])
        result[name] = image
    result['joined'] = merge([result[name] for name in ORDER])
    return result


def validate_images(images, origin=ORIGIN, size=SIZE):
    """Independently check every water/bank texel, full masks, alpha and both open export ends."""
    require(set(images) == set(ORDER) | {'joined'}, 'export layer set changed')
    for name, image in images.items():
        require(image.mode == 'RGBA' and image.size == size, f'{name}: export dimensions/mode changed')
        require(all(p[3] or p == (0, 0, 0, 0) for p in image.get_flattened_data()),
                f'{name}: hidden RGB')
    equal(images['joined'], merge([images[name] for name in ORDER]), 'joined draw order')
    water = Image.open(ROOT / SOURCES['water']).convert('RGBA')
    # Resize the entire source independently: this includes premultiplied-alpha BOX filtering.
    bank = Image.open(ROOT / SOURCES['bank']).convert('RGBA').resize((128, BANK_DEPTH), Image.Resampling.BOX)
    bank.putdata([p if p[3] else (0, 0, 0, 0) for p in bank.get_flattened_data()])
    ground = Image.open(ROOT / SOURCES['ground']).convert('RGBA')
    for y in range(size[1]):
        wy = origin[1] + y
        for x in range(size[0]):
            wx = origin[0] + x
            expected_water = water.getpixel((wx % 128, wy % 96)) if TOP <= wy < BOTTOM else (0, 0, 0, 0)
            require(images['water'].getpixel((x, y)) == expected_water, 'water mask/UV continuity')
            for name, row in (('north_bank', wy - TOP + BANK_DEPTH),
                              ('south_bank', BOTTOM + BANK_DEPTH - 1 - wy)):
                expected = bank.getpixel((wx % 128, row)) if 0 <= row < BANK_DEPTH else (0, 0, 0, 0)
                require(images[name].getpixel((x, y)) == expected, f'{name} mask/UV continuity')
            require(images['ground'].getpixel((x, y)) == ground.getpixel((wx % ground.width, wy % ground.height)),
                    'ground phase changed')
    # Repeated columns are equal at a full tile period, not incorrectly at adjacent edges.
    for name in ('water', 'north_bank', 'south_bank'):
        equal(images[name].crop((0, 0, size[0] - 128, size[1])),
              images[name].crop((128, 0, size[0], size[1])),
              f'{name} repeat phase')


def check_b2_phase(images):
    """Compare exact export texels to the frozen B2 water and unobstructed western banks."""
    with Image.open(ROOT / SOURCES['b2_water']) as water:
        equal(images['water'].crop((48, TOP - ORIGIN[1], 176, BOTTOM - ORIGIN[1])),
              water.crop((0, TOP - 384, 128, BOTTOM - 384)), 'B2 water phase')
    with Image.open(ROOT / SOURCES['b2_bank']) as bank:
        for name, top in (('north_bank', TOP - BANK_DEPTH), ('south_bank', BOTTOM)):
            equal(images[name].crop((48, top - ORIGIN[1], 64, top + BANK_DEPTH - ORIGIN[1])),
                  bank.crop((0, top - 384, 16, top + BANK_DEPTH - 384)), f'B2 {name} phase')


def measurements(count):
    """Report route clearance without assigning collision, traversal or scenario authority."""
    _, routes = helpers.proposal.load_geometry()
    rectangle = ((0, TOP - BANK_DEPTH), (SIZE[0], TOP - BANK_DEPTH),
                 (SIZE[0], BOTTOM + BANK_DEPTH), (0, BOTTOM + BANK_DEPTH))
    # Distance to sampled occupied squares subtracts a half-diagonal for a conservative bound.
    clearance = min(helpers.proposal.distance((x + .5, y + .5), a, b)
                    for x in range(SIZE[0]) for y in range(TOP - BANK_DEPTH, BOTTOM + BANK_DEPTH)
                    for a, b in routes.values()) - 48 - math.sqrt(.5)
    require(clearance > 0, 'outlet halo contacts retained route corridor')
    return {'status': 'external western outlet; visual assessment pending; no runtime/layout/physical approval',
            'baseline_commit': BASE_COMMIT, 'protected_file_count': count,
            'world_origin': ORIGIN, 'export_size': SIZE, 'water_bounds': [0, TOP, 320, BOTTOM],
            'bank_halo_corners': rectangle, 'water_uv_origin': [0, 0], 'bank_uv_origin_x': 0,
            'tiling_recipe': 'exact B2 absolute-world UVs; 128px horizontal period; 32-to-16px BOX bank depth',
            'off_map_reference_width': OFF_MAP_WIDTH, 'draw_order_back_to_front': ORDER,
            'raster_halo_to_route_corridor_clearance_lower_bound': clearance,
            'physical_limit': 'water-only off-map reference; world boundary remains closed to actors; no physical changes',
            'source_sha256': {name: sha(ROOT / path) for name, path in SOURCES.items()}}


def review(images):
    """Show local scales and a separate off-world continuity reference with an engineering boundary."""
    wide_size = (SIZE[0] + OFF_MAP_WIDTH, SIZE[1])
    wide = exports(render((-OFF_MAP_WIDTH, ORIGIN[1]), wide_size), wide_size)
    validate_images(wide, (-OFF_MAP_WIDTH, ORIGIN[1]), wide_size)
    for name in images:
        equal(wide[name].crop((OFF_MAP_WIDTH, 0, wide_size[0], SIZE[1])), images[name],
              f'{name} off-map continuity')
    overlay = wide['joined'].copy()
    draw = ImageDraw.Draw(overlay)
    draw.line((OFF_MAP_WIDTH, 0, OFF_MAP_WIDTH, SIZE[1] - 1), fill='#ffd66e', width=1)
    draw.text((OFF_MAP_WIDTH + 8, 8), 'x=0: closed world edge / external water only',
              fill='white', font=ImageFont.load_default(size=11))
    draw.text((OFF_MAP_WIDTH + 8, SIZE[1] - 20), '96px water + 16px banks / no end caps',
              fill='white', font=ImageFont.load_default(size=11))
    return {'joined-0.8x': images['joined'].resize(tuple(round(n * .8) for n in SIZE), Image.Resampling.LANCZOS),
            'joined-1.0x': images['joined'],
            'joined-2.0x': images['joined'].resize(tuple(n * 2 for n in SIZE), Image.Resampling.NEAREST),
            'off-map-reference': wide['joined'], 'engineering': overlay}


def run(build):
    """Build or verify exact source-bound PNG, editable master, manifest and evidence bytes."""
    count = check_scope(build)
    check_inputs()
    masters = render()
    images = exports(masters)
    validate_images(images)
    check_b2_phase(images)
    evidence = measurements(count)
    with tempfile.TemporaryDirectory() as directory:
        staging = Path(directory)
        ora = staging / 'outlet.ora'
        helpers.builder.create_ora(ora, tuple(n * SCALE for n in SIZE),
                                   [(name, layer, 0, 0, True) for name, layer in masters.items()])
        with zipfile.ZipFile(ora) as archive:
            stack = ET.fromstring(archive.read('stack.xml'))
            require((int(stack.attrib['w']), int(stack.attrib['h'])) == (640, 384), 'master dimensions')
            require([node.attrib['name'] for node in stack.findall('./stack/layer')] == list(reversed(ORDER)),
                    'master draw order')
            with archive.open('mergedimage.png') as stream:
                equal(Image.open(stream), merge(list(masters.values())), 'master merged image')
        artifacts = {f'exports/{name}.png': image for name, image in images.items()}
        artifacts.update({f'review/{name}.png': image for name, image in review(images).items()})
        manifest = {'schema_version': 1, 'status': evidence['status'], 'master_scale': SCALE,
                    'world_origin': ORIGIN, 'export_size': SIZE, 'source_paths': SOURCES,
                    'source_sha256': evidence['source_sha256'], 'draw_order_back_to_front': ORDER,
                    'master': {'path': 'masters/outlet.ora', 'sha256': sha(ora)}, 'exports': []}
        for name, image in artifacts.items():
            target = staging / name
            target.parent.mkdir(parents=True, exist_ok=True)
            image.save(target)
            manifest['exports'].append({'path': name, 'sha256': sha(target), 'size': image.size})
        payloads = {name: (staging / name).read_bytes() for name in artifacts}
        payloads['masters/outlet.ora'] = ora.read_bytes()
        for name, value in (('manifest.json', manifest), ('evidence.json', evidence)):
            payloads[name] = (json.dumps(value, indent=2) + '\n').encode()
        for name, content in payloads.items():
            target = HERE / name
            if build:
                target.parent.mkdir(parents=True, exist_ok=True)
                target.write_bytes(content)
            else:
                require(target.read_bytes() == content, f'non-reproducible artifact: {name}')
    print(f'PASS: western outlet; {count} protected files; exact full water/bank masks and UVs; off-map continuity')


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--build', action='store_true')
    run(parser.parse_args().build)
