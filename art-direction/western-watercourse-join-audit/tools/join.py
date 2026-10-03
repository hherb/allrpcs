#!/usr/bin/env python3
"""Render one external 64px shoreline/UV transition; preserve all accepted source files."""

import argparse
import importlib.util
import io
import json
import math
import sys
import tempfile
import zipfile
import xml.etree.ElementTree as ET

from PIL import Image

import audit

ORIGIN = (1536, 384)
SIZE = (64, 256)
ORDER = ('ground', 'water_underlay', 'banks')


def helpers():
    """Load frozen B1 geometry and editable-master helpers without its CLI."""
    spec = importlib.util.spec_from_file_location('join_b1', audit.ROOT / audit.SOURCES[1])
    module = importlib.util.module_from_spec(spec)
    sys.modules[spec.name] = module
    spec.loader.exec_module(module)
    return module


def profile(x):
    """Keep 4px endpoint recipes exact and ease the 56px successor between them."""
    if x < 1540:
        top = 552 - (x - 1400) / 2
        return top, top + 96, 16, 0
    if x >= 1596:
        return 484, 516, 32, 1
    t = (x - 1540) / 56
    smooth = t * t * (3 - 2 * t)
    tangent = 56 * -.5 * (t ** 3 - 2 * t * t + t)
    return 482 + 2 * smooth + tangent, 578 - 62 * smooth + tangent, 16 + 16 * smooth, smooth


def mix(a, b, amount):
    """Interpolate only opaque source water/ground samples or declared bank underlays."""
    return tuple(round(first * (1 - amount) + second * amount) for first, second in zip(a, b))


def render():
    """Build editable 2x layers from existing source texels, without new painted pixels."""
    textures = {}
    for name, path in (('ground', audit.SOURCES[9]), ('water', audit.SOURCES[6]), ('bank', audit.SOURCES[7])):
        with Image.open(audit.ROOT / path) as source:
            textures[name] = source.convert('RGBA')
    result = {name: Image.new('RGBA', (128, 512)) for name in ORDER}
    for y in range(512):
        wy = ORIGIN[1] + (y + .5) / 2
        for x in range(128):
            wx = ORIGIN[0] + (x + .5) / 2
            top, bottom, depth, amount = profile(wx)
            gx, gy = math.floor(wx), math.floor(wy)
            ground = textures['ground'].getpixel((gx % textures['ground'].width, gy % textures['ground'].height))
            west_water = textures['water'].getpixel((gx % 128, gy % 96))
            east_water = textures['water'].getpixel(((gx - 1600) % 128, (gy - 452) % 96))
            water = mix(west_water, east_water, amount)
            result['ground'].putpixel((x, y), ground)
            row = None
            if top - depth <= wy < top:
                row = math.floor((wy - top + depth) * 32 / depth)
            elif bottom <= wy < bottom + depth:
                row = math.floor((bottom + depth - wy) * 32 / depth)
            if top <= wy < bottom:
                result['water_underlay'].putpixel((x, y), water)
            elif row is not None:
                result['water_underlay'].putpixel((x, y), mix(ground, water, amount))
                row = min(31, max(0, row))
                west_bank = textures['bank'].getpixel((gx % 128, row))
                east_bank = textures['bank'].getpixel(((gx - 1540) % 128, row))
                # Interpolate premultiplied RGBA so transparent source rows cannot
                # introduce hidden RGB or a dark fringe during the UV transition.
                weight = west_bank[3] * (1 - amount) + east_bank[3] * amount
                alpha = round(weight)
                pixel = tuple(round((west_bank[c] * west_bank[3] * (1 - amount) +
                                     east_bank[c] * east_bank[3] * amount) / weight)
                              for c in range(3)) + (alpha,) if alpha else (0, 0, 0, 0)
                if amount == 1:
                    # Retained sprites are sampled at 1x. BOX resizing even a
                    # constant translucent 2x texel can round its RGB differently;
                    # bake the exact retained bank-over-water pixel at this endpoint.
                    underlay = Image.new('RGBA', (1, 1), east_water)
                    underlay.alpha_composite(Image.new('RGBA', (1, 1), east_bank))
                    pixel = underlay.getpixel((0, 0))
                result['banks'].putpixel((x, y), pixel)
    # Copy the west endpoint's exact independent 2x layers. Replacing a partly
    # covered water layer with an opaque ground/water blend would alter BOX-filtered
    # edge pixels even when the geometry and source sampling are otherwise identical.
    with zipfile.ZipFile(audit.ROOT / audit.SOURCES[10]) as archive:
        stack = ET.fromstring(archive.read('stack.xml'))
        layers = {node.attrib['name']: node.attrib['src'] for node in stack.findall('./stack/layer')}
        for name in ('road', 'deck'):
            with archive.open(layers[name]) as stream:
                layer = Image.open(stream).convert('RGBA')
                audit.require(layer.crop((896, 0, 1024, 512)).getchannel('A').getbbox() is None,
                              'correction envelope covers accepted road/deck pixels')
        for old, new in (('ground', 'ground'), ('water', 'water_underlay'), ('bank', 'banks')):
            with archive.open(layers[old]) as stream:
                layer = Image.open(stream).convert('RGBA')
                result[new].paste(layer.crop((896, 0, 904, 512)), (0, 0))
    return result


def compose(images):
    """Apply the fixed back-to-front layer order."""
    result = Image.new('RGBA', images[0].size)
    for layer in images:
        result.alpha_composite(layer)
    return result


def validate(candidate, reference):
    """Reject changes outside the patch and verify both exact endpoint recipe bands."""
    audit.require(candidate.mode == 'RGBA' and candidate.size == (128, 256), 'candidate dimensions')
    audit.require(candidate.crop((64, 0, 128, 256)).tobytes() == reference.crop((64, 0, 128, 256)).tobytes(),
                  'retained Stream pixels changed')
    audit.require(candidate.crop((0, 0, 4, 256)).tobytes() == reference.crop((0, 0, 4, 256)).tobytes(),
                  'B1 endpoint pixels changed')
    # The last four patch columns use the retained recipe evaluated at x<1600.
    # Compare against independent source sampling, including the mirrored bank.
    with Image.open(audit.ROOT / audit.SOURCES[6]) as source:
        water = source.convert('RGBA')
    with Image.open(audit.ROOT / audit.SOURCES[7]) as source:
        bank = source.convert('RGBA')
    with Image.open(audit.ROOT / audit.SOURCES[9]) as source:
        ground = source.convert('RGBA')
    for y in range(256):
        wy = ORIGIN[1] + y
        for x in range(60, 64):
            wx = ORIGIN[0] + x
            expected = ground.getpixel((wx % ground.width, wy % ground.height))
            if 452 <= wy < 548:
                expected = water.getpixel(((wx - 1600) % 128, (wy - 452) % 96))
                row = wy - 452 if wy < 484 else 547 - wy if wy >= 516 else None
                if row is not None:
                    expected = compose([Image.new('RGBA', (1, 1), expected),
                                        Image.new('RGBA', (1, 1), bank.getpixel(((wx - 1540) % 128, row)))])
                    expected = expected.getpixel((0, 0))
            audit.require(candidate.getpixel((x, y)) == expected,
                          f'retained endpoint recipe changed at {(x, y)}: {candidate.getpixel((x, y))} != {expected}')
    for x in range(64):
        top, bottom, depth, _ = profile(ORIGIN[0] + x + .5)
        audit.require(bottom > top and 16 <= depth <= 32, 'shoreline mask crossed')
        old_top = 552 - (ORIGIN[0] + x + .5 - 1400) / 2
        for y in range(256):
            wy = ORIGIN[1] + y + .5
            in_old = old_top - 17 <= wy <= old_top + 113
            in_new = top - depth - 1 <= wy <= bottom + depth + 1
            if not in_old and not in_new:
                audit.require(candidate.getpixel((x, y)) == reference.getpixel((x, y)),
                              'pixels outside shoreline halo changed')


def validate_layers(exports):
    """Check full water/bank containment and both continuous banks, including the interior."""
    audit.require(set(exports) == set(ORDER), 'layer set changed')
    for name, image in exports.items():
        audit.require(image.mode == 'RGBA' and image.size == SIZE, f'{name} dimensions/mode')
        audit.require(all(pixel[3] or pixel == (0, 0, 0, 0) for pixel in image.get_flattened_data()),
                      f'{name} hidden RGB')
    for x in range(64):
        top, bottom, depth, _ = profile(ORIGIN[0] + x + .5)
        for y in range(256):
            wy = ORIGIN[1] + y + .5
            water_alpha = exports['water_underlay'].getpixel((x, y))[3]
            bank_alpha = exports['banks'].getpixel((x, y))[3]
            if top + 1 < wy < bottom - 1:
                audit.require(water_alpha == 255, 'water interior gap')
                audit.require(bank_alpha == 0, 'bank intrudes into water')
            if wy < top - depth - 1 or wy > bottom + depth + 1:
                audit.require(water_alpha == bank_alpha == 0, 'halo mask escaped')
        for start, end in ((top - depth, top), (bottom, bottom + depth)):
            rows = range(max(0, math.ceil(start - ORIGIN[1])), min(256, math.floor(end - ORIGIN[1])))
            audit.require(any(exports['banks'].getpixel((x, y))[3] for y in rows), 'bank continuity gap')


def run(build):
    """Build or reproduce one source-bound external join and run meaningful endpoint mutations."""
    baseline = audit.scope(False)
    audit.inputs()
    helper = helpers()
    _, crossing, _, _ = helper.geometry()
    deck_right = max(point[0] for point in crossing['deck_polygon'])
    audit.require(ORIGIN[0] - deck_right > 56, 'patch contacts B1 deck')
    masters = render()
    exports = {name: image.resize(SIZE, Image.Resampling.BOX) for name, image in masters.items()}
    for image in exports.values():
        image.putdata([pixel if pixel[3] else (0, 0, 0, 0) for pixel in image.get_flattened_data()])
    validate_layers(exports)
    for layer, point, pixel, message in (
            ('water_underlay', (32, 124), (0, 0, 0, 0), 'water interior gap'),
            ('banks', (32, 124), (100, 90, 60, 255), 'bank intrudes'),
            ('banks', (32, 0), (100, 90, 60, 255), 'halo mask escaped'),
            ('banks', (32, 0), (1, 0, 0, 0), 'hidden RGB')):
        mutation = {name: image.copy() for name, image in exports.items()}
        mutation[layer].putpixel(point, pixel)
        try:
            validate_layers(mutation)
        except ValueError as error:
            audit.require(message in str(error), f'unexpected layer rejection: {error}')
        else:
            raise ValueError(f'mutation accepted: {message}')
    patch = compose([exports[name] for name in ORDER])
    reference = audit.diagnostic(ORIGIN, (128, 256))['review/interface-1x.png']
    candidate = reference.copy()
    candidate.paste(patch, (0, 0))
    validate(candidate, reference)
    for point, message in (((64, 120), 'retained Stream'), ((0, 120), 'B1 endpoint'),
                           ((63, 100), 'retained endpoint')):
        mutation = candidate.copy()
        mutation.putpixel(point, (0, 0, 0, 0))
        try:
            validate(mutation, reference)
        except ValueError as error:
            audit.require(message in str(error), f'unexpected rejection: {error}')
        else:
            raise ValueError(f'mutation accepted: {message}')
    images = {f'correction/exports/{name}.png': image for name, image in exports.items()}
    images['correction/exports/joined.png'] = patch
    images['correction/review/joined-1x.png'] = candidate
    images['correction/review/joined-2x.png'] = candidate.resize((256, 512), Image.Resampling.NEAREST)
    images['correction/review/before-1x.png'] = reference
    payloads = {}
    for name, image in images.items():
        buffer = io.BytesIO()
        image.save(buffer, format='PNG')
        payloads[name] = buffer.getvalue()
    with tempfile.TemporaryDirectory() as directory:
        path = audit.Path(directory) / 'join.ora'
        helper.builder.create_ora(path, (128, 512), [(name, image, 0, 0, True) for name, image in masters.items()])
        payloads['correction/masters/join.ora'] = path.read_bytes()
    manifest = {
        'status': 'external successor join candidate; owner visual assessment pending',
        'origin': ORIGIN, 'size': SIZE, 'master_scale': 2, 'draw_order': ORDER,
        'protected_file_count': len(baseline), 'B1_deck_horizontal_clearance': ORIGIN[0] - deck_right,
        'endpoint_exact_recipe_width': 4, 'retained_pixels_changed': 0,
        'recipe': 'Hermite wet edges; smooth source UV and bank-underlay interpolation; 16-to-32px banks',
        'source_sha256': {name: audit.sha(audit.ROOT / name) for name in audit.SOURCES},
        'artifacts_sha256': {name: audit.hashlib.sha256(content).hexdigest() for name, content in payloads.items()},
        'layout_runtime_physical_promotion_authorized': False,
    }
    payloads['correction/manifest.json'] = (json.dumps(manifest, indent=2) + '\n').encode()
    request_path = audit.HERE / 'correction/review-request.json'
    if request_path.exists():
        request = json.loads(request_path.read_text())
        audit.require(request['review_sha256'] ==
                      audit.hashlib.sha256(payloads['correction/review/joined-2x.png']).hexdigest(),
                      'owner-reviewed candidate changed')
        audit.require(request['layout_runtime_physical_promotion_authorized'] is False,
                      'review request grants no promotion')
    for name, content in payloads.items():
        path = audit.HERE / name
        if build:
            path.parent.mkdir(parents=True, exist_ok=True)
            path.write_bytes(content)
        else:
            audit.require(path.read_bytes() == content, f'non-reproducible artifact: {name}')
    print('PASS: bounded join; full masks/banks; exact endpoints; unchanged retained pixels; seven rejection checks')


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--build', action='store_true')
    run(parser.parse_args().build)
