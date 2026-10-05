#!/usr/bin/env python3
"""Build the selected external Square ground proof without changing product inputs."""

import argparse
import hashlib
from io import BytesIO
import json
from pathlib import Path
import re
import zipfile
from xml.etree.ElementTree import Element, SubElement, tostring

import numpy as np
from PIL import Image, ImageDraw, ImageFilter, ImageFont

ROOT = Path(__file__).resolve().parent
REPO = ROOT.parents[1]
ORIGIN = (1920, 620)
SIZE = (560, 560)
VARIANTS = ('baseline', 'hard-support', 'feathered')
FONT = Path('/System/Library/Fonts/Supplemental/Arial.ttf')
CROPS = {'west-port': (0, 180, 180, 400), 'southwest-port': (0, 380, 180, 560),
         'southeast-port': (360, 380, 560, 560), 'junction-edge': (160, 160, 360, 360)}


def sha(path):
    """Hash source or review bytes."""
    return hashlib.sha256(path.read_bytes()).hexdigest()


def require(condition, message):
    """Keep drift rejection enabled in optimized Python."""
    if not condition:
        raise ValueError(message)


def raster(path):
    """Load source pixels without changing their native size."""
    with Image.open(path) as image:
        return image.convert('RGBA')


def png(image):
    """Serialize deterministic proof bytes."""
    output = BytesIO()
    image.save(output, format='PNG')
    return output.getvalue()


def text(draw, xy, value, size=17):
    """Keep bound-font labels outside artwork frames."""
    draw.text(xy, value, fill='#25312e', font=ImageFont.truetype(str(FONT), size))


def repeat(path, period):
    """Sample immutable source RGB at absolute-world phase zero."""
    paint = np.asarray(raster(path))
    require(paint.shape == (period, period, 4), 'Paint period changed')
    x = (ORIGIN[0] + np.arange(SIZE[0])) % period
    y = (ORIGIN[1] + np.arange(SIZE[1])) % period
    return Image.fromarray(paint[y[:, None], x[None, :]])


def clip(image, origin):
    """Clip a retained route to the external review window only."""
    layer = Image.new('RGBA', SIZE)
    layer.alpha_composite(image, (origin[0] - ORIGIN[0], origin[1] - ORIGIN[1]))
    return layer


def merge(layers):
    """Composite visible native layers bottom first."""
    result = Image.new('RGBA', SIZE)
    for _, layer in layers:
        result.alpha_composite(layer)
    return result


def mask_field(field, coverage):
    """Keep original RGB at every covered texel; canonicalize only transparent pixels."""
    pixels = np.asarray(field).copy()
    pixels[:, :, 3] = np.asarray(coverage)
    pixels[pixels[:, :, 3] == 0, :3] = 0
    return Image.fromarray(pixels)


def masks(junction):
    """Square-dilate exact alpha support and compute a zero-padded inward feather."""
    support = Image.new('L', SIZE)
    alpha = junction.getchannel('A').point(lambda value: 255 if value else 0)
    support.paste(alpha, (24, 24))
    support = support.filter(ImageFilter.MaxFilter(49))
    # Sixteen zero-padded one-pixel square erosions give Chebyshev distance.
    # The first covered boundary row has pixel-centre distance 0.5, not a full pixel.
    padded = Image.new('L', (SIZE[0] + 34, SIZE[1] + 34))
    padded.paste(support, (17, 17))
    distance = np.zeros(SIZE, dtype=np.float64)
    for _ in range(16):
        distance += np.asarray(padded.crop((17, 17, 577, 577))) > 0
        padded = padded.filter(ImageFilter.MinFilter(3))
    fraction = np.clip((distance - 0.5) / 16.0, 0, 1)
    # A seventeenth surviving row is at least 16.5px inward and must be opaque.
    fraction[np.asarray(padded.crop((17, 17, 577, 577))) > 0] = 1
    feather = Image.fromarray(np.rint(255 * fraction * fraction * (3 - 2 * fraction)).astype('uint8'))
    require(not np.any((np.asarray(feather) > 0) & (np.asarray(support) == 0)), 'Feather escapes support')
    return support, feather


def master(title, layers, field, coverage):
    """Save exact editable 2x visible layers with hidden source paint and mask references."""
    size = (1120, 1120)
    xml = Element('image', w='1120', h='1120', name=title, version='0.0.3')
    stack = SubElement(xml, 'stack')
    mask_reference = Image.merge('RGBA', (coverage, coverage, coverage, Image.new('L', SIZE, 255)))
    entries = [('Corrections', Image.new('RGBA', SIZE), 'visible'),
               *[(name, image, 'visible') for name, image in reversed(layers)],
               ('Coverage reference', mask_reference, 'hidden'), ('Unmasked world paint', field, 'hidden')]
    members = [('mimetype', b'image/openraster')]
    for index, (name, image, visible) in enumerate(entries):
        path = f'data/layer-{index}.png'
        SubElement(stack, 'layer', name=name, src=path, x='0', y='0', opacity='1.0',
                   visibility=visible, **{'composite-op': 'svg:src-over'})
        members.append((path, png(image.resize(size, Image.Resampling.NEAREST))))
    members.extend([('stack.xml', tostring(xml)),
                    ('mergedimage.png', png(merge(layers).resize(size, Image.Resampling.NEAREST)))])
    output = BytesIO()
    with zipfile.ZipFile(output, 'w') as archive:
        for name, data in members:
            archive.writestr(zipfile.ZipInfo(name, date_time=(2026, 10, 5, 0, 0, 0)), data)
    return output.getvalue()


def routes():
    """Derive five live source/transform recipes, preserving actual equal-depth order."""
    scene = (REPO / 'game/Main.tscn').read_text()
    code = (REPO / 'game/GreyboxNavigation.cs').read_text()
    resources = {key: path for path, key in re.findall(
        r'\[ext_resource type="Texture2D" path="([^"]+)" id="([^"]+)"\]', scene)}
    # Order follows the live connection loop, then the junction appended last.
    names = ('SquareSickhousePath', 'SquareGardenPath', 'SquareSouthernRoadPath',
             'VillageEdgeSquareConnector', 'VillageSquareJunction')
    result = []
    for name in names:
        assignment = re.search(rf'^{name}Texture = ExtResource\("([^"]+)"\)', scene, re.MULTILINE)
        require(assignment is not None, f'Missing route assignment: {name}')
        path = 'game/' + resources[assignment.group(1)].removeprefix('res://')
        method = re.search(rf'private Sprite2D Create{name}Presentation\(\)(.*?)(?=\n    private )',
                           code, re.DOTALL)
        require(method is not None, f'Missing route recipe: {name}')
        body = method.group(1)
        require(not re.search(r'\b(?:Scale|Rotation|Offset|Centered|RegionEnabled|Modulate)\s*=', body),
                f'Unsupported route transform: {name}')
        if name == 'VillageSquareJunction':
            require('Position = OpeningRegionLayoutGeometry.Anchors[VillageSquareJunctionAnchorId]' in body,
                    'Junction positioning changed')
            centre = (2200, 900)
        else:
            match = re.search(r'Position = new Vector2\((\d+)\.0f, (\d+)\.0f\)', body)
            require(match is not None, f'Unsupported route placement: {name}')
            centre = tuple(int(value) for value in match.groups())
        z = int(re.search(r'ZIndex = (-?\d+)', body).group(1))
        image = raster(REPO / path)
        result.append({'name': name, 'source': path, 'z': z,
                       'origin': [centre[i] - image.size[i] // 2 for i in range(2)]})
    require([route['z'] for route in result] == [-71, -71, -71, -70, -70], 'Route depths changed')
    return result


def render():
    """Produce the entire CPU series and bind the exact selected recipe and predecessor."""
    proposal = json.loads((REPO / 'art-direction/ground-placement-proposal/proposal.json').read_text())
    selection = json.loads((ROOT / 'owner-selection.json').read_text())
    for name, expected in {**proposal['sources'], **selection['selected_proposal']}.items():
        require(sha(Path(name) if name.startswith('/') else REPO / name) == expected, f'Selected source drift: {name}')
    junction = raster(REPO / 'game/art/runtime/terrain/village-square-junction.png')
    support, feather = masks(junction)
    field = repeat(REPO / 'art-direction/residual-ground-tiling-proof/exports/packed-earth.png', 128)
    moss = repeat(REPO / 'game/art/runtime/terrain/quiet-moss.png', 256)
    route_records = routes()
    route_layers = [(route['name'], clip(raster(REPO / route['source']), route['origin']))
                    for route in route_records]
    layers = {'baseline': [('Quiet moss', moss), *route_layers],
              'hard-support': [('Quiet moss', moss), ('Packed earth hard support', mask_field(field, support)), *route_layers],
              'feathered': [('Quiet moss', moss), ('Packed earth inward feather', mask_field(field, feather)), *route_layers]}
    views = {name: merge(value) for name, value in layers.items()}
    outputs = {}
    for name, image in views.items():
        outputs[f'review/{name}-1x.png'] = png(image)
        if name != 'baseline':
            mask = support if name == 'hard-support' else feather
            outputs[f'masters/{name}.ora'] = master(name, layers[name], field, mask)
            outputs[f'proof-inputs/{name}-layer.png'] = png(mask_field(field, mask))
            outputs[f'proof-inputs/{name}-coverage.png'] = png(mask)
    outputs['proof-inputs/world-paint.png'] = png(field)
    board = Image.new('RGB', (1736, 666), '#eee9df')
    draw = ImageDraw.Draw(board)
    text(draw, (20, 15), 'Village Square / selected packed-earth external feasibility series', 24)
    text(draw, (20, 49), 'Native CPU route-only reconstruction; objects/labels and GPU filtering appear in the disposable previews.')
    for index, name in enumerate(VARIANTS):
        x = 16 + index * 576
        board.paste(views[name].convert('RGB'), (x, 80))
        text(draw, (x, 643), name)
    outputs['review/comparison-1x.png'] = png(board)
    for title, box in CROPS.items():
        width, height = (box[2] - box[0]) * 2, (box[3] - box[1]) * 2
        board = Image.new('RGB', ((width + 16) * 3 + 16, height + 100), '#eee9df')
        draw = ImageDraw.Draw(board)
        text(draw, (16, 12), f'{title} / exact nearest 2x / baseline, hard support, feathered', 18)
        for index, name in enumerate(VARIANTS):
            x = 16 + index * (width + 16)
            board.paste(views[name].crop(box).resize((width, height), Image.Resampling.NEAREST).convert('RGB'), (x, 48))
            text(draw, (x, height + 65), name)
        outputs[f'review/{title}-2x.png'] = png(board)
    sources = dict(proposal['sources'])
    for path in [ROOT / 'owner-selection.json', ROOT / 'baseline.json', REPO / 'art-direction/ground-placement-proposal/proposal.json',
                 REPO / 'game/art/runtime/terrain/quiet-moss.png', *[REPO / item['source'] for item in route_records]]:
        sources[str(path.relative_to(REPO))] = sha(path)
    sources[str(FONT)] = sha(FONT)
    measurements = {}
    baseline = np.asarray(views['baseline'])
    for name, mask in [('hard-support', support), ('feathered', feather)]:
        changed = np.any(np.asarray(views[name]) != baseline, axis=2)
        require(not np.any(changed & (np.asarray(mask) == 0)), 'Composition escapes mask')
        measurements[name] = {'coverage_pixels': int(np.count_nonzero(np.asarray(mask))),
                              'visible_changed_pixels': int(changed.sum())}
    record = {'date': '2026-10-05', 'timezone': 'Australia/Sydney', 'status': 'external feasibility; visual review pending',
              'origin': list(ORIGIN), 'size': list(SIZE), 'world_phase': [0, 0], 'new_period': 128, 'legacy_period': 256,
              'mask_recipe': 'alpha > 0; square dilation radius 24; zero-padded Chebyshev pixel-centre distance; inward 16px smoothstep',
              'routes': route_records, 'sources': dict(sorted(sources.items())), 'measurements': measurements,
              'outputs': {name: hashlib.sha256(data).hexdigest() for name, data in outputs.items()},
              'limits': 'CPU route-only crop; disposable GPU views retain actual scene objects/labels; no runtime promotion.'}
    outputs['inventory.json'] = (json.dumps(record, indent=2) + '\n').encode()
    return outputs


def main():
    """Write only this unapproved external packet or verify byte reproduction read-only."""
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--check', action='store_true')
    args = parser.parse_args()
    outputs = render()
    if (ROOT / 'inventory.json').exists():
        saved = json.loads((ROOT / 'inventory.json').read_text())
        require(saved['sources'] == json.loads(outputs['inventory.json'])['sources'], 'Frozen input drift')
    require(args.check or not (ROOT / 'owner-acceptance.json').exists(), 'Accepted packet cannot be overwritten')
    for name, data in outputs.items():
        path = ROOT / name
        if args.check:
            require(path.exists() and path.read_bytes() == data, f'Output drift: {name}')
        else:
            path.parent.mkdir(parents=True, exist_ok=True)
            path.write_bytes(data)
    print('PASS: complete baseline/hard/feathered CPU series and two editable 2x masters')


if __name__ == '__main__':
    main()
