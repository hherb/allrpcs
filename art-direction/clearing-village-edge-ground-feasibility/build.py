#!/usr/bin/env python3
"""Author one complete external shoulder comparison from retained leaf-litter paint."""

import argparse
import hashlib
from io import BytesIO
import json
from pathlib import Path
import zipfile
from xml.etree.ElementTree import Element, SubElement, tostring

import numpy as np
from PIL import Image, ImageFilter

ROOT = Path(__file__).resolve().parent
REPO = ROOT.parents[1]
ORIGIN = (1008, 414)
SIZE = (304, 164)
CENTER = (1160, 510)
SEGMENT = (1056, 450, 1240, 490)
VARIANTS = ('baseline', 'hard-support', 'feather-16', 'feather-24')
SOURCE = 'game/art/runtime/terrain/woodland-leaf-litter.png'
ROUTE = 'game/art/runtime/terrain/woodland-village-edge-path.png'
OWNERS = 'art-direction/closed-forest-ground-coverage/capture-evidence.json'
FONT = Path('/System/Library/Fonts/Supplemental/Arial.ttf')


def require(condition, message):
    if not condition:
        raise ValueError(message)


def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def raster(path):
    with Image.open(path) as image:
        return image.convert('RGBA')


def png(image):
    output = BytesIO()
    image.save(output, format='PNG')
    return output.getvalue()


def owner_alpha(owner):
    """Place the recorded native sprite in the bounded world-space envelope."""
    require(owner['world_transform'][:4] == [1, 0, 0, 1] and owner['offset'] == [0, 0],
            'Non-native owner transform')
    image = raster(REPO / 'game' / owner['texture'].removeprefix('res://'))
    require(list(image.size) == owner['size'], 'Owner texture size changed')
    x, y = owner['world_transform'][4:]
    if owner['centered']:
        x -= image.width / 2
        y -= image.height / 2
    require(x == int(x) and y == int(y), 'Fractional native owner origin')
    canvas = Image.new('L', SIZE)
    canvas.paste(image.getchannel('A'), (int(x) - ORIGIN[0], int(y) - ORIGIN[1]))
    return canvas


def masks():
    """Compare one native route segment; exclude the entire guarded channel union."""
    owners = json.loads((REPO / OWNERS).read_text())['owners']
    route = owners['GreyboxNavigation/RoutePresentation/WoodlandVillageEdgePath']
    require(route['texture'] == 'res://art/runtime/terrain/woodland-village-edge-path.png'
            and route['world_transform'] == [1, 0, 0, 1, 1352, 632]
            and route['z_index'] == -75, 'Accepted route owner changed')
    alpha = owner_alpha(route)
    local = tuple(SEGMENT[i] - ORIGIN[i % 2] for i in range(4))
    selected = Image.new('L', SIZE)
    selected.paste(alpha.crop(local).point(lambda a: 255 if a else 0), local[:2])
    support = selected.filter(ImageFilter.MaxFilter(49))
    forbidden = Image.new('L', SIZE)
    channel_names = ('water', 'banks', 'decks', 'detail', 'fringe')
    for name in channel_names:
        channel = owner_alpha(owners['WholeChannelPresentation/' + name])
        forbidden = Image.fromarray(np.maximum(np.asarray(forbidden), np.asarray(channel)))
    # Four native pixels protect filtered channel edges, including bridge decking.
    forbidden = forbidden.point(lambda a: 255 if a else 0).filter(ImageFilter.MaxFilter(9))
    pixels = np.asarray(support).copy()
    excluded = int(np.count_nonzero((pixels > 0) & (np.asarray(forbidden) > 0)))
    pixels[np.asarray(forbidden) > 0] = 0
    support = Image.fromarray(pixels)
    box = support.getbbox()
    require(box is not None and 0 < box[0] < box[2] < SIZE[0]
            and 0 < box[1] < box[3] < SIZE[1], 'Support clipped by authoring envelope')
    result = {'hard-support': support}
    for width in (16, 24):
        padding = width + 1
        padded = Image.new('L', (SIZE[0] + padding * 2, SIZE[1] + padding * 2))
        padded.paste(support, (padding, padding))
        box = (padding, padding, padding + SIZE[0], padding + SIZE[1])
        distance = np.zeros((SIZE[1], SIZE[0]), dtype=np.float64)
        for _ in range(width):
            distance += np.asarray(padded.crop(box)) > 0
            padded = padded.filter(ImageFilter.MinFilter(3))
        fraction = np.clip((distance - 0.5) / width, 0, 1)
        fraction[np.asarray(padded.crop(box)) > 0] = 1
        result[f'feather-{width}'] = Image.fromarray(
            np.rint(255 * fraction * fraction * (3 - 2 * fraction)).astype('uint8'))
    return result, forbidden, excluded


def master(name, field, coverage, paint):
    """Keep the source field and coverage independently editable at exact 2x."""
    size = (SIZE[0] * 2, SIZE[1] * 2)
    xml = Element('image', w=str(size[0]), h=str(size[1]), name=name, version='0.0.3')
    stack = SubElement(xml, 'stack')
    reference = Image.merge('RGBA', (coverage, coverage, coverage, Image.new('L', SIZE, 255)))
    entries = [('Corrections', Image.new('RGBA', SIZE), 'visible'),
               ('Masked leaf litter', paint, 'visible'),
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
            archive.writestr(zipfile.ZipInfo(path, date_time=(2026, 10, 6, 0, 0, 0)), data)
    return output.getvalue()


def generate():
    baseline = json.loads((ROOT / 'baseline.json').read_text())
    paths = {name for name in baseline['files'] if name.startswith('game/')}
    paths.update({OWNERS, 'art-direction/closed-forest-ground-coverage/audit.json',
                  'art-direction/village-edge-ground-runtime-promotion/owner-runtime-acceptance.json',
                  'art-direction/village-edge-ground-runtime-promotion/verification.json',
                  'art-direction/production-proof/corrected/asset-manifest.json'})
    sources = {}
    for name in sorted(paths):
        sources[name] = sha(REPO / name)
        require(sources[name] == baseline['files'][name], 'Predecessor input drift: ' + name)
    for name in ('build.py', 'baseline.json', 'continuation.json'):
        sources[str((ROOT / name).relative_to(REPO))] = sha(ROOT / name)
    source = np.asarray(raster(REPO / SOURCE))
    require(source.shape == (256, 256, 4) and np.all(source[:, :, 3] == 255),
            'Retained material is not opaque 256px paint')
    xx = (ORIGIN[0] + np.arange(SIZE[0])) % 256
    yy = (ORIGIN[1] + np.arange(SIZE[1])) % 256
    field = Image.fromarray(source[yy[:, None], xx[None, :]])
    coverage, forbidden, excluded = masks()
    outputs = {'proof-inputs/world-paint.png': png(field),
               'proof-inputs/channel-exclusion.png': png(forbidden)}
    measurements = {}
    for name, mask in coverage.items():
        pixels = np.asarray(field).copy()
        pixels[:, :, 3] = np.asarray(mask)
        pixels[pixels[:, :, 3] == 0] = 0
        paint = Image.fromarray(pixels)
        outputs[f'proof-inputs/{name}-coverage.png'] = png(mask)
        outputs[f'proof-inputs/{name}-layer.png'] = png(paint)
        outputs[f'masters/{name}.ora'] = master(name, field, mask, paint)
        measurements[name] = {'nonzero_texels': int(np.count_nonzero(pixels[:, :, 3])),
                              'opaque_texels': int(np.count_nonzero(pixels[:, :, 3] == 255)),
                              'alpha_bounds': list(mask.getbbox())}
    record = {'status': 'complete external comparison; baseline remains an option; no runtime selection',
              'date': '2026-10-06', 'timezone': 'Australia/Sydney',
              'baseline_head': baseline['head'], 'origin': list(ORIGIN), 'size': list(SIZE),
              'center': list(CENTER), 'variants': list(VARIANTS), 'source': SOURCE,
              'world_phase': [0, 0], 'period': 256, 'z_index': -78,
              'recipe': {'native_route_segment': list(SEGMENT), 'dilation_radius': 24,
                         'channel_exclusion_guard': 4, 'channel_excluded_texels': excluded,
                         'feather_widths': [16, 24], 'feather': 'zero-padded inward smoothstep'},
              'measurements': measurements, 'sources': sources,
              'outputs': {name: hashlib.sha256(data).hexdigest() for name, data in outputs.items()}}
    outputs['inventory.json'] = (json.dumps(record, indent=2) + '\n').encode()
    return outputs


def write_or_check(outputs, check):
    """Validate every destination before writing any file."""
    for name, data in outputs.items():
        path = ROOT / name
        require(path.is_file() and path.read_bytes() == data if check else not path.exists(),
                ('Output drift: ' if check else 'Refusing overwrite: ') + name)
    if not check:
        for name, data in outputs.items():
            (ROOT / name).write_bytes(data)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--check', action='store_true')
    args = parser.parse_args()
    write_or_check(generate(), args.check)
    print('PASS: complete three-underlay comparison; source paint and editable 2x masters')


if __name__ == '__main__':
    main()
