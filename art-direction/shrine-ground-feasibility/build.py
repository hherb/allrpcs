#!/usr/bin/env python3
"""Compose a bounded external Shrine apron from unchanged approved source paint."""

import argparse
import hashlib
import importlib.util
from io import BytesIO
import json
from pathlib import Path
import zipfile
from xml.etree.ElementTree import Element, SubElement, tostring

import numpy as np
from PIL import Image, ImageDraw, ImageFilter

ROOT = Path(__file__).resolve().parent
REPO = ROOT.parents[1]
ORIGIN = (596, 250)
SIZE = (92, 143)
CONTEXT_ORIGIN = (460, 220)
CONTEXT_SIZE = (420, 290)
VARIANTS = ('baseline', 'hard-support', 'feather-16', 'feather-24')
SOURCE = 'art-direction/residual-ground-tiling-proof/exports/pale-gravel.png'
ROUTE = 'game/art/runtime/terrain/woodland-shrine-path.png'
FONT = Path('/System/Library/Fonts/Supplemental/Arial.ttf')
CROPS = {'north-edge': (115, 10, 255, 105), 'entrance': (115, 90, 255, 210),
         'south-edge': (115, 150, 255, 265)}
HELPER = REPO / 'art-direction/square-ground-feasibility/build.py'
spec = importlib.util.spec_from_file_location('square_proof_helpers', HELPER)
helpers = importlib.util.module_from_spec(spec)
spec.loader.exec_module(helpers)
png, raster, sha, require, text = helpers.png, helpers.raster, helpers.sha, helpers.require, helpers.text


def repeat(path, period, origin, size):
    """Sample source paint at absolute-world phase zero without rotating or scaling."""
    source = np.asarray(raster(path))
    require(source.shape == (period, period, 4), 'Source period changed')
    xx = (origin[0] + np.arange(size[0])) % period
    yy = (origin[1] + np.arange(size[1])) % period
    return Image.fromarray(source[yy[:, None], xx[None, :]])


def support_and_feathers():
    """Dilate the exact exterior route-alpha segment, then feather strictly inward."""
    route = raster(REPO / ROUTE)
    require(route.size == (704, 208), 'Shrine route geometry changed')
    alpha = route.getchannel('A').point(lambda value: 255 if value else 0)
    # Retained route origin is (352,224); selected world columns are [620,664).
    segment = alpha.crop((268, 0, 312, 208))
    canvas = Image.new('L', SIZE)
    canvas.paste(segment, (620 - ORIGIN[0], 224 - ORIGIN[1]))
    support = canvas.filter(ImageFilter.MaxFilter(49))
    masks = {'hard-support': support}
    for width in (16, 24):
        padding = width + 1
        padded = Image.new('L', (SIZE[0] + 2 * padding, SIZE[1] + 2 * padding))
        padded.paste(support, (padding, padding))
        box = (padding, padding, padding + SIZE[0], padding + SIZE[1])
        distance = np.zeros((SIZE[1], SIZE[0]), dtype=np.float64)
        for _ in range(width):
            distance += np.asarray(padded.crop(box)) > 0
            padded = padded.filter(ImageFilter.MinFilter(3))
        fraction = np.clip((distance - 0.5) / width, 0, 1)
        fraction[np.asarray(padded.crop(box)) > 0] = 1
        masks[f'feather-{width}'] = Image.fromarray(
            np.rint(255 * fraction * fraction * (3 - 2 * fraction)).astype('uint8'))
    return masks


def mask_field(field, coverage):
    """Keep exact source RGB at every nonzero coverage texel."""
    pixels = np.asarray(field).copy()
    pixels[:, :, 3] = np.asarray(coverage)
    pixels[pixels[:, :, 3] == 0] = 0
    return Image.fromarray(pixels)


def master(name, paint, field, coverage):
    """Preserve a 2x editable underlay, source field, coverage plane and Corrections layer."""
    size = (SIZE[0] * 2, SIZE[1] * 2)
    xml = Element('image', w=str(size[0]), h=str(size[1]), name=name, version='0.0.3')
    stack = SubElement(xml, 'stack')
    reference = Image.merge('RGBA', (coverage, coverage, coverage, Image.new('L', SIZE, 255)))
    entries = [('Corrections', Image.new('RGBA', SIZE), 'visible'),
               ('Masked pale gravel', paint, 'visible'),
               ('Coverage reference', reference, 'hidden'), ('Unmasked world paint', field, 'hidden')]
    members = [('mimetype', b'image/openraster')]
    for index, (title, layer, visibility) in enumerate(entries):
        path = f'data/layer-{index}.png'
        SubElement(stack, 'layer', name=title, src=path, x='0', y='0', opacity='1.0',
                   visibility=visibility, **{'composite-op': 'svg:src-over'})
        members.append((path, png(layer.resize(size, Image.Resampling.NEAREST))))
    members.extend([('stack.xml', tostring(xml)),
                    ('mergedimage.png', png(paint.resize(size, Image.Resampling.NEAREST)))])
    output = BytesIO()
    with zipfile.ZipFile(output, 'w') as archive:
        for path, data in members:
            archive.writestr(zipfile.ZipInfo(path, date_time=(2026, 10, 5, 0, 0, 0)), data)
    return output.getvalue()


def compose(layer=None):
    """Show local moss/route paint only; actual shells/channel/woodland owners are GPU-only."""
    context = repeat(REPO / 'game/art/runtime/terrain/quiet-moss.png', 256, CONTEXT_ORIGIN, CONTEXT_SIZE)
    if layer is not None:
        context.alpha_composite(layer, tuple(ORIGIN[i] - CONTEXT_ORIGIN[i] for i in range(2)))
    for path, origin in [(ROUTE, (352, 224)),
                         ('game/art/runtime/terrain/shrine-southern-road-path.png', (432, 400))]:
        context.alpha_composite(raster(REPO / path), tuple(origin[i] - CONTEXT_ORIGIN[i] for i in range(2)))
    return context


def source_bindings():
    """Bind predecessor paint, actual owners, proposal and immutable helper implementation."""
    baseline = json.loads((ROOT / 'baseline.json').read_text())
    paths = [SOURCE, ROUTE, 'game/Main.tscn', 'game/GreyboxNavigation.cs',
             'game/OpeningRegionOldShrineSceneSource.cs', 'game/OpeningRegionOldShrineGeometry.cs', 'game/OpeningRegionLayoutGeometry.cs',
             'game/OpeningRegionWoodlandClearingSceneSource.cs', 'game/OpeningRegionWoodlandClearingGeometry.cs',
             'game/WholeChannelPresentation.cs', str(HELPER.relative_to(REPO)),
             'art-direction/production-proof/corrected/asset-manifest.json',
             'art-direction/residual-ground-tiling-proof/owner-acceptance.json',
             'art-direction/garden-ground-runtime-promotion/owner-runtime-acceptance.json',
             'art-direction/square-ground-runtime-promotion/owner-runtime-acceptance.json',
             'art-direction/sickhouse-ground-runtime-promotion/owner-runtime-acceptance.json',
             'art-direction/wagon-ground-runtime-promotion/owner-runtime-acceptance.json',
             'game/art/runtime/terrain/shrine-southern-road-path.png']
    paths.extend(record['source'] for record in helpers.routes())
    paths.extend(f'game/art/runtime/terrain/{name}.png' for name in
                 ('wagon-entrance-ground', 'sickhouse-entrance-ground', 'quiet-moss', 'garden-ground-meadow', 'garden-ground-soil', 'village-square-ground',
                  'channel-water', 'channel-banks', 'channel-decks', 'channel-detail', 'channel-fringe'))
    sources = {}
    for name in sorted(set(paths)):
        require(sha(REPO / name) == baseline['files'][name], f'Selected predecessor drift: {name}')
        sources[name] = sha(REPO / name)
    approval = json.loads((REPO / 'art-direction/residual-ground-tiling-proof/owner-acceptance.json').read_text())
    require(approval['status'] == 'approved' and sources[SOURCE] == approval['approved_artwork']['exports/pale-gravel.png'],
            'Material approval mismatch')
    for path in (ROOT / 'selection.json', ROOT / 'baseline.json', FONT):
        sources[str(path.relative_to(REPO)) if path.is_relative_to(REPO) else str(path)] = sha(path)
    return dict(sorted(sources.items()))


def render():
    """Generate the complete comparison and editable underlay series."""
    sources = source_bindings()
    selection = json.loads((ROOT / 'selection.json').read_text())
    require(selection['origin'] == list(ORIGIN) and selection['size'] == list(SIZE)
            and selection['route_alpha_segment_world_x'] == [620, 664]
            and selection['dilation_radius'] == 24 and selection['feather_widths'] == [16, 24]
            and selection['world_phase'] == [0, 0] and selection['period'] == 128 and selection['z'] == -78
            and selection['source_material'] == SOURCE, 'Selected recipe drift')
    field = repeat(REPO / SOURCE, 128, ORIGIN, SIZE)
    masks = support_and_feathers()
    outputs = {'proof-inputs/world-paint.png': png(field)}
    views = {'baseline': compose()}
    measurements = {}
    for name, mask in masks.items():
        layer = mask_field(field, mask)
        views[name] = compose(layer)
        outputs[f'proof-inputs/{name}-layer.png'] = png(layer)
        outputs[f'proof-inputs/{name}-coverage.png'] = png(mask)
        outputs[f'masters/{name}.ora'] = master(name, layer, field, mask)
        measurements[name] = {'nonzero_coverage_pixels': int(np.count_nonzero(np.asarray(mask))),
                              'opaque_pixels': int(np.count_nonzero(np.asarray(mask) == 255))}
    for name, image in views.items():
        outputs[f'review/{name}-1x.png'] = png(image)
    board = Image.new('RGB', (888, 724), '#eee9df')
    draw = ImageDraw.Draw(board)
    text(draw, (16, 12), 'Shrine entrance / complete pale-gravel comparison', 24)
    text(draw, (16, 48), 'Moss/route substrate study only. Woodland, shell and actual filtering appear in GPU views.')
    for index, name in enumerate(VARIANTS):
        x, y = 16 + index % 2 * 436, 84 + index // 2 * 320
        board.paste(views[name].convert('RGB'), (x, y))
        text(draw, (x, y + 294), name)
    outputs['review/comparison-1x.png'] = png(board)
    for title, box in CROPS.items():
        width, height = (box[2] - box[0]) * 2, (box[3] - box[1]) * 2
        board = Image.new('RGB', ((width + 16) * 4 + 16, height + 94), '#eee9df')
        draw = ImageDraw.Draw(board)
        text(draw, (16, 12), f'{title} / exact nearest 2x / all four variants', 20)
        for index, name in enumerate(VARIANTS):
            x = 16 + index * (width + 16)
            board.paste(views[name].crop(box).resize((width, height), Image.Resampling.NEAREST).convert('RGB'), (x, 48))
            text(draw, (x, height + 60), name)
        outputs[f'review/{title}-2x.png'] = png(board)
    record = {'date': '2026-10-05', 'timezone': 'Australia/Sydney',
              'status': 'external comparison; owner visual review pending',
              'origin': list(ORIGIN), 'size': list(SIZE), 'context_origin': list(CONTEXT_ORIGIN),
              'context_size': list(CONTEXT_SIZE), 'variants': list(VARIANTS), 'sources': sources,
              'mask_recipe': 'route alpha > 0 in world x [620,664); square dilation radius 24; zero-padded inward smoothstep 16/24px',
              'world_phase': [0, 0], 'period': 128, 'z': -78, 'measurements': measurements,
              'outputs': {name: hashlib.sha256(data).hexdigest() for name, data in outputs.items()}}
    outputs['inventory.json'] = (json.dumps(record, indent=2) + '\n').encode()
    return outputs


def main():
    """Write an unaccepted packet or verify deterministic bytes without writing."""
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--check', action='store_true')
    args = parser.parse_args()
    outputs = render()
    if (ROOT / 'inventory.json').exists():
        saved = json.loads((ROOT / 'inventory.json').read_text())
        require(saved['sources'] == json.loads(outputs['inventory.json'])['sources'], 'Frozen input drift')
    require(args.check or not (ROOT / 'inventory.json').exists(), 'Existing packet is immutable; use a separate revision')
    for name, data in outputs.items():
        path = ROOT / name
        if args.check:
            require(path.is_file() and path.read_bytes() == data, f'Output drift: {name}')
        else:
            path.parent.mkdir(parents=True, exist_ok=True)
            path.write_bytes(data)
    print('PASS: complete four-variant source series and three editable 2x masters')


if __name__ == '__main__':
    main()
