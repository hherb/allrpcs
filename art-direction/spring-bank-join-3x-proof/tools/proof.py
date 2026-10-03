#!/usr/bin/env python3
"""Build a three-times-diameter external spring, preserving the accepted Stream."""

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
PREFIX = 'art-direction/spring-bank-join-3x-proof/'
BASE_COMMIT = '2cfce8af460da05a95da4a1e69bfa81ccb4ea9c5'
ORIGIN = (1900, 212)
SIZE = (512, 576)
SCALE = 2
CENTER = (2032, 500)
RADIUS = 192
OUTER_RADIUS = 208
JOIN_X = 2000
WATER_UV_ORIGIN = (1600, 452)
BANK_UV_ORIGIN_X = 1932
SOURCES = {
    'ground': 'art-direction/production-proof/corrected/exports/terrain/quiet-moss.png',
    'water': 'game/art/runtime/terrain/stream-water.png',
    'bank': 'game/art/runtime/terrain/stream-bank.png',
    'cap': 'game/art/runtime/terrain/stream-bank-opening-cap.png',
    'proposal': 'art-direction/western-watercourse-proposal/proposal.json',
    'scene': 'game/Main.tscn',
    'geometry': 'game/OpeningRegionStreamLaundryGeometry.cs',
    'helpers': 'art-direction/b2-bridge-proof/tools/proof.py',
    'original_view': 'art-direction/spring-bank-join-proof/review/joined-1.0x.png',
}
ORDER = ('ground', 'retained_water', 'retained_banks', 'new_water', 'new_banks', 'spring_detail')


def load_helpers():
    """Reuse immutable image/geometry/OpenRaster helpers without running their CLI."""
    spec = importlib.util.spec_from_file_location('spring_proof_helpers', ROOT / SOURCES['helpers'])
    result = importlib.util.module_from_spec(spec)
    sys.modules[spec.name] = result
    spec.loader.exec_module(result)
    return result


helpers = load_helpers()
require, sha, equal, merge = helpers.require, helpers.sha, helpers.equal, helpers.merge


def protected_inventory():
    """Inventory all pre-existing tracked authorship, excluding only the authorized handover."""
    names = subprocess.check_output(['git', 'ls-files', '-z'], cwd=ROOT).decode().split('\0')
    return sorted(n for n in names if n and n != 'HANDOVER.md' and not n.startswith(PREFIX)
                  and Path(n).name != '.DS_Store')


def check_scope(build):
    """Freeze the named starting head once and preserve its entire authored inventory."""
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
    """Bind the accepted source envelope and B2 shown-view acceptance to exact inputs."""
    config = json.loads((ROOT / SOURCES['proposal']).read_text())
    require(config['source']['center'] == list(CENTER) and config['source']['radius'] * 3 == RADIUS,
            'source envelope changed')
    request = json.loads((HERE / 'enlargement-request.json').read_text())
    require(request['owner_statement'] ==
            'looks good - perhaps make it three times as large, would look even better'
            and request['review_path'] == SOURCES['original_view'], 'owner request differs')
    require(request['linear_scale'] == 3 and request['review_sha256'] ==
            sha(ROOT / request['review_path']), 'enlargement request/source differs')
    require(request['layout_runtime_physical_promotion_authorized'] is False, 'promotion unauthorized')
    require(config['retained_water'] == [1600, 452, 2000, 548], 'retained Stream bounds changed')
    record = json.loads((ROOT / 'art-direction/spring-bank-join-proof/b2-visual-acceptance.json').read_text())
    expected = 'art-direction/b2-bridge-proof/review/joined-1.0x.png'
    require(record['owner_statement'] == 'Approved' and record['review_path'] == expected,
            'B2 acceptance differs from the shown owner response')
    require(record['review_sha256'] == sha(ROOT / expected), 'accepted B2 image changed')
    require(record['layout_runtime_physical_promotion_authorized'] is False,
            'B2 visual acceptance grants no promotion authority')


def profile(x):
    """Return inner/outer half-heights for a 32px inlet joining the circular eastern basin."""
    if x < JOIN_X or x >= CENTER[0] + OUTER_RADIUS:
        return 0, 0
    if x < CENTER[0]:
        phase = max(0, x - JOIN_X - 4) / (CENTER[0] - JOIN_X - 4)
        smooth = phase * phase * (3 - 2 * phase)
        return 16 + (RADIUS - 16) * smooth, 48 + (OUTER_RADIUS - 48) * smooth
    dx = x - CENTER[0]
    inner = math.sqrt(max(0, RADIUS * RADIUS - dx * dx)) if dx < RADIUS else 0
    outer = math.sqrt(max(0, OUTER_RADIUS * OUTER_RADIUS - dx * dx))
    return inner, outer


def water_pixel(tile, point):
    """Continue the accepted Polygon2D UV phase, rather than restarting a local tile."""
    return tile.getpixel((math.floor(point[0] - WATER_UV_ORIGIN[0]) % tile.width,
                          math.floor(point[1] - WATER_UV_ORIGIN[1]) % tile.height))


def retained_bank_pixel(bank, cap, point):
    """Reproduce the actual north/south eastern caps and bank regions inside the water band."""
    x, y = point
    if not 1900 <= x < JOIN_X:
        return (0, 0, 0, 0)
    if 452 <= y < 484:
        row = math.floor(y - 452)
    elif 516 <= y < 548:
        row = bank.height - 1 - math.floor(y - 516)
    else:
        return (0, 0, 0, 0)
    if x < BANK_UV_ORIGIN_X:
        return cap.getpixel((cap.width - 1 - math.floor(x - 1900), row))
    return bank.getpixel((math.floor(x - BANK_UV_ORIGIN_X), row))


def bank_pixel(bank, point, inner, outer):
    """Map the neck's bank depth into the existing tile, then use radial depth around the basin."""
    x, y = point
    if x < CENTER[0]:
        phase = (outer - abs(y - CENTER[1])) / (outer - inner)
    else:
        phase = (OUTER_RADIUS - math.dist(point, CENTER)) / (OUTER_RADIUS - RADIUS)
    row = max(0, min(bank.height - 1, math.floor(phase * bank.height)))
    return bank.getpixel((math.floor(x - BANK_UV_ORIGIN_X) % bank.width, row))


def bank_composite(raw, water):
    """Keep the retained bank's translucent/transparent rows over the continuing water phase."""
    alpha = raw[3]
    return tuple((raw[channel] * alpha + water[channel] * (255 - alpha) + 127) // 255
                 for channel in range(3)) + (255,)


def detail_pixel(point):
    """Scale the static emergence highlight with the basin without scenario authority."""
    radius = math.dist(point, CENTER) / 3
    alpha = round(50 * math.exp(-radius * radius / 16) + 36 * math.exp(-((radius - 9) ** 2) / 4))
    return (111, 181, 180, alpha) if alpha else (0, 0, 0, 0)


def layers():
    """Render independent 2x layers and retain the rejected plain-circle overlay separately."""
    textures = {key: Image.open(ROOT / value).convert('RGBA')
                for key, value in SOURCES.items() if value.endswith('.png')}
    require(textures['water'].size == (128, 96) and textures['bank'].size == (128, 32)
            and textures['cap'].size == (32, 32), 'retained texture dimensions changed')
    size = tuple(n * SCALE for n in SIZE)
    result = {name: Image.new('RGBA', size) for name in ORDER}
    rejected = Image.new('RGBA', size)
    buffers = {name: [] for name in ORDER}
    rejected_pixels = []
    for y in range(size[1]):
        wy = ORIGIN[1] + (y + .5) / SCALE
        for x in range(size[0]):
            wx = ORIGIN[0] + (x + .5) / SCALE
            point = (wx, wy)
            inner, outer = profile(wx)
            height = abs(wy - CENTER[1])
            wet = wx >= JOIN_X and wx < CENTER[0] + RADIUS and height < inner
            bank = wx >= JOIN_X and inner <= height < outer
            ground = textures['ground']
            buffers['ground'].append(ground.getpixel((math.floor(wx) % ground.width,
                                                       math.floor(wy) % ground.height)))
            water = water_pixel(textures['water'], point)
            buffers['retained_water'].append(water if wx < JOIN_X and 452 <= wy < 548 else (0, 0, 0, 0))
            buffers['retained_banks'].append(retained_bank_pixel(textures['bank'], textures['cap'], point))
            buffers['new_water'].append(water if wet else (0, 0, 0, 0))
            buffers['new_banks'].append(bank_composite(bank_pixel(textures['bank'], point, inner, outer), water)
                                        if bank else (0, 0, 0, 0))
            buffers['spring_detail'].append(detail_pixel(point) if wet else (0, 0, 0, 0))
            rejected_pixels.append(water if math.dist(point, CENTER) < RADIUS else (0, 0, 0, 0))
    for name, image in result.items():
        image.putdata(buffers[name])
    rejected.putdata(rejected_pixels)
    return result, rejected


def downsample(image):
    """Export one 2x layer with clean alpha and zero RGB beneath full transparency."""
    image = image.resize(SIZE, Image.Resampling.BOX)
    image.putdata([p if p[3] else (0, 0, 0, 0) for p in image.get_flattened_data()])
    return image


def exports(masters):
    """Make 1x layers and compose them in the exact declared draw order."""
    result = {name: downsample(image) for name, image in masters.items()}
    result['joined'] = merge([result[name] for name in ORDER])
    return result


def retained_reference(images):
    """Keep a separate comparison composition of the accepted Stream and banks."""
    return merge([images[name] for name in ORDER[:3]])


def validate_images(images):
    """Check complete preserved territory, both exact connectors, masks and the closed basin bank."""
    equal(images['joined'], merge([images[name] for name in ORDER]), 'joined draw order')
    reference = retained_reference(images)
    seam_x = JOIN_X - ORIGIN[0]
    equal(images['joined'].crop((0, 0, seam_x, SIZE[1])), reference.crop((0, 0, seam_x, SIZE[1])),
          'retained Stream pixels')
    bank = Image.open(ROOT / SOURCES['bank']).convert('RGBA')
    water = Image.open(ROOT / SOURCES['water']).convert('RGBA')
    for y in range(SIZE[1]):
        for x in range(SIZE[0]):
            point = (ORIGIN[0] + x + .5, ORIGIN[1] + y + .5)
            inner, outer = profile(point[0])
            height = abs(point[1] - CENTER[1])
            wa = images['new_water'].getpixel((x, y))[3]
            ba = images['new_banks'].getpixel((x, y))[3]
            if point[0] < JOIN_X:
                require(wa == 0 and ba == 0 and images['spring_detail'].getpixel((x, y))[3] == 0,
                        'new layer overwrites retained territory')
            else:
                samples = [(ORIGIN[0] + x + dx, ORIGIN[1] + y + dy)
                           for dx in (.25, .75) for dy in (.25, .75)]
                water_interior = all(p[0] < CENTER[0] + RADIUS and
                                     abs(p[1] - CENTER[1]) < profile(p[0])[0] for p in samples)
                bank_interior = all(profile(p[0])[0] <= abs(p[1] - CENTER[1]) < profile(p[0])[1]
                                    for p in samples)
                if water_interior:
                    require(wa == 255, 'gap in connected inlet/basin water')
                    require(ba == 0, 'bank intrudes into basin water')
                if bank_interior:
                    require(ba == 255, 'gap in closed basin bank')
                if wa:
                    require(math.dist(point, CENTER) < RADIUS + 1, 'water leaves proposed source envelope')
                if ba:
                    require(math.dist(point, CENTER) < OUTER_RADIUS + 1, 'bank leaves bounded halo')
    # These are exact source-pixel continuations, not a merely similar-colour seam.
    for row in range(32):
        for top, flip in ((452, False), (516, True)):
            raw = bank.getpixel((68, 31 - row if flip else row))
            point = (JOIN_X + .5, top + row + .5)
            expected_image = Image.new('RGBA', (1, 1), water_pixel(water, point))
            expected_image.alpha_composite(Image.new('RGBA', (1, 1), raw))
            expected = expected_image.getpixel((0, 0))
            require(images['new_banks'].getpixel((seam_x, top - ORIGIN[1] + row)) == expected,
                    'bank connector differs from retained texture continuation')
    for wy in range(484, 516):
        expected = water_pixel(water, (JOIN_X + .5, wy + .5))
        require(images['new_water'].getpixel((seam_x, wy - ORIGIN[1])) == expected,
                'water connector restarts accepted UV phase')


def measurements(images, rejected, count):
    """Measure the circular overlay conflict and conservative route/physical limits."""
    reference = retained_reference(images)
    naive = merge([reference, rejected])
    bank_conflicts = 0
    changed = 0
    for y in range(SIZE[1]):
        for x in range(JOIN_X - ORIGIN[0]):
            point = (ORIGIN[0] + x + .5, ORIGIN[1] + y + .5)
            if math.dist(point, CENTER) < RADIUS and images['retained_banks'].getpixel((x, y))[3]:
                bank_conflicts += 1
            changed += reference.getpixel((x, y)) != naive.getpixel((x, y))
    require(bank_conflicts > 0 and changed > 0, 'naive circular overlay no longer demonstrates bank conflict')
    layout, routes = helpers.proposal.load_geometry()
    require(OUTER_RADIUS < CENTER[0] < layout['world']['width'] - OUTER_RADIUS
            and OUTER_RADIUS < CENTER[1] < layout['world']['height'] - OUTER_RADIUS,
            'source/bank halo reaches the world boundary')
    distances = {name: helpers.proposal.distance(CENTER, a, b) for name, (a, b) in routes.items()}
    # Every nonzero exported halo pixel represents its whole 1px square. Subtract
    # the square half-diagonal to bound continuous clearance, including steep necks.
    covered = [(ORIGIN[0] + x + .5, ORIGIN[1] + y + .5)
               for y in range(SIZE[1]) for x in range(SIZE[0])
               if images['new_water'].getpixel((x, y))[3] or images['new_banks'].getpixel((x, y))[3]]
    clearance = min(helpers.proposal.distance(p, a, b) for p in covered
                    for a, b in routes.values()) - 48 - math.sqrt(.5)
    require(clearance > 0, 'new source/bank envelope contacts retained 96px route corridor')
    route_cells = 0
    for wy in range(4, 1800, 8):
        for wx in range(4, 3200, 8):
            if wx >= JOIN_X and abs(wy - CENTER[1]) <= profile(wx)[1] and profile(wx)[1] > 0:
                route_cells += any(helpers.proposal.distance((wx, wy), a, b) <= 48 for a, b in routes.values())
    require(route_cells == 0, 'new source halo contains a retained route-cell centre')
    return {'status': 'external spring join; visual assessment pending; no runtime/layout/physical approval',
            'baseline_commit': BASE_COMMIT, 'protected_file_count': count,
            'source_center': CENTER, 'water_radius_envelope': RADIUS, 'bank_radius_envelope': OUTER_RADIUS,
            'inlet': {'x': JOIN_X, 'visible_water_top': 484, 'visible_water_bottom': 516, 'width': 32,
                      'bank_depth_at_join': 32, 'bank_depth_at_basin_center': 16},
            'retained_territory': 'all x < 2000 within review, including accepted bank/cap pixels',
            'retained_composite_pixels_changed_by_candidate': 0,
            'naive_circle_retained_bank_pixels_covered': bank_conflicts,
            'naive_circle_retained_composite_pixels_changed': changed,
            'minimum_center_to_route_distance': min(distances.values()),
            'raster_halo_to_route_corridor_clearance_lower_bound': clearance,
            'full_unclipped_circle_clearance': min(distances.values()) - OUTER_RADIUS - 48,
            'linear_scale_relative_to_original': 3,
            'new_halo_retained_route_cell_centres': route_cells,
            'draw_order_back_to_front': ORDER,
            'water_uv_origin': WATER_UV_ORIGIN, 'bank_continuation_source_column': 68,
            'new_bank_recipe': 'existing bank RGBA composited over continuing water UV phase',
            'spring_detail': 'static proposed emergence highlight; no scenario fact or interaction',
            'physical_limit': 'existing Stream collision remains; new basin/banks have no '
                              'collision or navigation authority',
            'source_sha256': {key: sha(ROOT / value) for key, value in SOURCES.items()}}


def review(images, rejected):
    """Show the candidate, preserved reference and rejected circle separately."""
    candidate = images['joined']
    reference = retained_reference(images)
    naive = merge([reference, rejected])
    original = reference.copy()
    with Image.open(ROOT / SOURCES['original_view']) as image:
        original.paste(image, (0, 340 - ORIGIN[1]))
    overlay = candidate.copy()
    draw = ImageDraw.Draw(overlay)
    sx, sy = CENTER[0] - ORIGIN[0], CENTER[1] - ORIGIN[1]
    draw.ellipse((sx - RADIUS, sy - RADIUS, sx + RADIUS, sy + RADIUS), outline='#ffd66e', width=1)
    draw.line((JOIN_X - ORIGIN[0], 452 - ORIGIN[1], JOIN_X - ORIGIN[0], 548 - ORIGIN[1]), fill='#f26880', width=1)
    draw.text((8, 8), 'External spring join - 32px inlet', fill='white', font=ImageFont.load_default(size=14))
    draw.text((8, 28), 'Pink: retained edge / gold: 192px envelope', fill='white', font=ImageFont.load_default(size=12))
    return {'joined-0.8x': candidate.resize(tuple(round(n * .8) for n in SIZE), Image.Resampling.LANCZOS),
            'joined-1.0x': candidate, 'joined-2.0x': candidate.resize(tuple(n * 2 for n in SIZE), Image.Resampling.NEAREST),
            'engineering': overlay, 'retained-stream': reference, 'rejected-circle-overlay': naive,
            'original-common-scale': original}


def run(build):
    """Build external files or verify exact PNG/master/manifest/evidence reproduction."""
    count = check_scope(build)
    check_inputs()
    masters, rejected_master = layers()
    images = exports(masters)
    rejected = downsample(rejected_master)
    validate_images(images)
    evidence = measurements(images, rejected, count)
    with tempfile.TemporaryDirectory() as directory:
        staging = Path(directory)
        ora = staging / 'spring-join.ora'
        helpers.builder.create_ora(ora, tuple(n * SCALE for n in SIZE),
                                   [(name, layer, 0, 0, True) for name, layer in masters.items()])
        artifacts = {f'exports/{name}.png': image for name, image in images.items()}
        artifacts.update({f'review/{name}.png': image for name, image in review(images, rejected).items()})
        manifest = {'schema_version': 1, 'status': evidence['status'], 'master_scale': SCALE,
                    'world_origin': ORIGIN, 'export_size': SIZE, 'source_sha256': evidence['source_sha256'],
                    'b2_visual_acceptance_sha256': sha(ROOT / 'art-direction/spring-bank-join-proof/b2-visual-acceptance.json'),
                    'enlargement_request_sha256': sha(HERE / 'enlargement-request.json'),
                    'master': {'path': 'masters/spring-join.ora', 'sha256': sha(ora)}, 'exports': []}
        for name, image in artifacts.items():
            target = staging / name
            target.parent.mkdir(parents=True, exist_ok=True)
            image.save(target)
            manifest['exports'].append({'path': name, 'sha256': sha(target), 'size': image.size})
            if build:
                (HERE / name).write_bytes(target.read_bytes())
            else:
                with Image.open(HERE / name) as actual:
                    equal(actual, image, name)
                require(sha(HERE / name) == sha(target), f'export bytes changed: {name}')
        with zipfile.ZipFile(ora) as archive:
            stack = ET.fromstring(archive.read('stack.xml'))
            require((int(stack.attrib['w']), int(stack.attrib['h'])) == tuple(n * SCALE for n in SIZE), 'master size changed')
            require([node.attrib['name'] for node in stack.findall('./stack/layer')] == list(reversed(ORDER)),
                    'master layer order changed')
            with archive.open('mergedimage.png') as stream:
                equal(Image.open(stream), merge(list(masters.values())), 'master merged image')
        if build:
            (HERE / 'masters/spring-join.ora').write_bytes(ora.read_bytes())
        else:
            require((HERE / 'masters/spring-join.ora').read_bytes() == ora.read_bytes(), 'master bytes changed')
        for name, payload in (('manifest.json', manifest), ('evidence.json', evidence)):
            content = json.dumps(payload, indent=2) + '\n'
            if build:
                (HERE / name).write_text(content)
            else:
                require((HERE / name).read_text() == content, f'non-reproducible {name}')
    print(f'PASS: external spring join; {count} protected files; exact water/bank connectors; zero route cells')


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--build', action='store_true')
    run(parser.parse_args().build)
