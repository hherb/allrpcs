#!/usr/bin/env python3
"""Compose an isolated Garden ground proof from exact accepted native-scale paint."""

import argparse
import hashlib
from io import BytesIO
import json
from pathlib import Path
import re
import sys
import zipfile
from xml.etree.ElementTree import Element, SubElement, tostring

import numpy as np
from PIL import Image, ImageDraw, ImageFont

ROOT = Path(__file__).resolve().parent
REPO = ROOT.parents[1]
sys.path.insert(0, str(REPO / 'art-direction/residual-ground-compatibility'))
from audit import digest, inputs, raster, require

PRODUCTION = REPO / 'art-direction/production-proof/corrected'
TILING = REPO / 'art-direction/residual-ground-tiling-proof'
SCENE = REPO / 'game/Main.tscn'
ORIGIN = (2048, 1120)
SIZE = (512, 384)
DETAIL_BOX = (96, 80, 416, 304)
APRON_WIDTH = 32
APRON_FEATHER = 16
SOIL_RADIUS = 12
FONT_PATH = Path('/System/Library/Fonts/Supplemental/Arial.ttf')
LABEL_FONT = ImageFont.truetype(str(FONT_PATH), 18)
SMALL_FONT = ImageFont.truetype(str(FONT_PATH), 15)
VARIANTS = ('baseline', 'soil-only', 'meadow-apron')
SELECTED_NAMES = {'review/meadow-apron-1x.png', 'review/meadow-apron-detail-2x.png',
                  'masters/meadow-apron.ora', 'proof-inputs/soil-layer.png', 'proof-inputs/meadow-layer.png',
                  'proof-inputs/soil-coverage.png', 'proof-inputs/apron-coverage.png'}
INPUT_CODE = ('game/Main.tscn', 'game/GreyboxNavigation.cs', 'game/OpeningRegionLayoutGeometry.cs',
              'game/OpeningRegionRemedyPreparationSceneSource.cs',
              'game/RuntimeHerbBedSmokeCheck.cs', 'game/RuntimeGardenFoliageSmokeCheck.cs',
              'game/RuntimePreparationTableSmokeCheck.cs')


def read_json(path):
    """Read a source inventory without executing predecessor builders."""
    return json.loads(path.read_text())


def vector(text):
    """Require integer, axis-aligned scene coordinates for this native-pixel proof."""
    match = re.fullmatch(r'Vector2\((-?\d+(?:\.\d+)?), (-?\d+(?:\.\d+)?)\)', text)
    require(match is not None, f'Unsupported vector: {text}')
    values = tuple(float(value) for value in match.groups())
    require(all(value.is_integer() for value in values), 'Subpixel placement requires a separate recipe')
    return tuple(int(value) for value in values)


def scene_sprites():
    """Read the actual Garden anchor, offsets and sibling order from the live scene."""
    scene = SCENE.read_text()
    resources = {key: value for value, key in re.findall(
        r'\[ext_resource type="Texture2D" path="([^"]+)" id="([^"]+)"\]', scene)}
    blocks = re.findall(r'\[node ([^\n]+)\]\n([^\[]*)', scene)
    garden_name = 'OpeningRegionRemedyPreparationSceneSource'
    anchor = None
    sprites = []
    for header, body in blocks:
        properties = dict(re.findall(r'^(\w+) = (.+)$', body, re.MULTILINE))
        name = re.search(r'name="([^"]+)"', header).group(1)
        if name == garden_name:
            require(set(properties) == {'position', 'script'}, 'Unsupported Garden parent transform')
            anchor = vector(properties['position'])
        if 'type="Sprite2D"' not in header or f'parent="{garden_name}"' not in header:
            continue
        require(anchor is not None, 'Garden parent must precede its children')
        require(properties.get('centered') == 'false', 'Unexpected Garden sprite anchor')
        require(set(properties) == {'position', 'texture', 'centered', 'offset'},
                'Unsupported Garden sprite transform, appearance or depth')
        texture = re.fullmatch(r'ExtResource\("([^"]+)"\)', properties['texture']).group(1)
        local, offset = vector(properties['position']), vector(properties['offset'])
        world = tuple(anchor[i] + local[i] + offset[i] for i in range(2))
        sprites.append({'name': name, 'source': 'game/' + resources[texture].removeprefix('res://'),
                        'local_position': list(local), 'offset': list(offset),
                        'world_origin': list(world), 'sibling_order': len(sprites)})
    require(anchor == (2300, 1300) and len(sprites) == 9, 'Expected the retained nine-sprite Garden')
    beds = [sprite for sprite in sprites if sprite['name'].startswith('HerbBed')]
    require(len(beds) == 2, 'Expected exactly two retained bed placements')
    return anchor, sprites, beds


def repeated(path, period):
    """Sample accepted pixels at absolute world phase, with no paint rotation/resizing."""
    paint = np.asarray(raster(path, (period, period)))
    x = (ORIGIN[0] + np.arange(SIZE[0])) % period
    y = (ORIGIN[1] + np.arange(SIZE[1])) % period
    return Image.fromarray(paint[y[:, None], x[None, :]])


def scene_routes():
    """Bind the retained route resources and derive their live world placement."""
    scene = SCENE.read_text()
    resources = {key: value for value, key in re.findall(
        r'\[ext_resource type="Texture2D" path="([^"]+)" id="([^"]+)"\]', scene)}
    code = (REPO / 'game/GreyboxNavigation.cs').read_text()
    layout = (REPO / 'game/OpeningRegionLayoutGeometry.cs').read_text()
    square = re.search(r'\["village-square"\] = new\((\d+)\.0f, (\d+)\.0f\)', layout)
    require(square is not None, 'Unsupported Village Square anchor recipe')
    routes = []
    for name in ('SquareGardenPath', 'SquareSickhousePath', 'VillageSquareJunction'):
        property_name = name + 'Texture'
        resource_id = re.search(rf'^{property_name} = ExtResource\("([^"]+)"\)', scene, re.MULTILINE)
        require(resource_id is not None, f'Missing retained route: {name}')
        source = 'game/' + resources[resource_id.group(1)].removeprefix('res://')
        expected_name = {'SquareGardenPath': 'square-garden-path.png',
                         'SquareSickhousePath': 'square-sickhouse-path.png',
                         'VillageSquareJunction': 'village-square-junction.png'}[name]
        require(Path(source).name == expected_name, f'Unexpected retained source: {name}')
        method = re.search(rf'private Sprite2D Create{name}Presentation\(\)(.*?)(?=\n    private )',
                           code, re.DOTALL)
        require(method is not None, f'Unsupported retained placement recipe: {name}')
        require(not re.search(r'\b(?:Scale|Rotation|Offset|Centered|RegionEnabled|RegionRect|Modulate)\s*=',
                              method.group(1)), 'Unsupported retained route transform or appearance')
        if name == 'VillageSquareJunction':
            require('Position = OpeningRegionLayoutGeometry.Anchors[VillageSquareJunctionAnchorId]' in method.group(1),
                    'Junction anchor recipe changed')
            centre = tuple(int(value) for value in square.groups())
        else:
            position = re.search(r'Position = new Vector2\((\d+)\.0f, (\d+)\.0f\)', method.group(1))
            require(position is not None, f'Unsupported retained centre: {name}')
            centre = tuple(int(value) for value in position.groups())
        z_index = re.search(r'ZIndex = (-?\d+)', method.group(1))
        require(z_index is not None, f'Missing route depth: {name}')
        size = raster(REPO / source).size
        require(all(value % 2 == 0 for value in size), 'Unsupported fractional centered sprite pivot')
        routes.append({'name': name, 'source': source, 'world_origin':
                       [centre[i] - size[i] // 2 for i in range(2)], 'z_index': int(z_index.group(1))})
    require([route['z_index'] for route in routes] == [-71, -71, -70], 'Retained route depth changed')
    return routes


def clipped_layer(image, world):
    """Place an immutable source sprite, clipping only to the declared review window."""
    layer = Image.new('RGBA', SIZE)
    left, top = world[0] - ORIGIN[0], world[1] - ORIGIN[1]
    source_box = (max(0, -left), max(0, -top), min(image.width, SIZE[0] - left),
                  min(image.height, SIZE[1] - top))
    if source_box[0] < source_box[2] and source_box[1] < source_box[3]:
        layer.alpha_composite(image.crop(source_box),
                              (max(0, left), max(0, top)))
    return layer


def coverage(bounds, radius, feather):
    """Make an inward-only rounded coverage mask; this is provisional proof geometry."""
    left, top, right, bottom = bounds
    xx, yy = np.meshgrid(ORIGIN[0] + np.arange(SIZE[0]) + 0.5,
                         ORIGIN[1] + np.arange(SIZE[1]) + 0.5)
    qx = np.abs(xx - (left + right) / 2) - ((right - left) / 2 - radius)
    qy = np.abs(yy - (top + bottom) / 2) - ((bottom - top) / 2 - radius)
    distance = np.hypot(np.maximum(qx, 0), np.maximum(qy, 0)) + np.minimum(np.maximum(qx, qy), 0) - radius
    fraction = np.clip(-distance / feather, 0, 1) if feather else (distance < 0).astype(float)
    return Image.fromarray(np.rint(255 * fraction * fraction * (3 - 2 * fraction)).astype('uint8'))


def masked(field, mask):
    """Retain every source RGB texel beneath a separately editable coverage plane."""
    result = field.copy()
    result.putalpha(mask)
    pixels = np.asarray(result).copy()
    pixels[pixels[:, :, 3] == 0, :3] = 0
    return Image.fromarray(pixels)


def merged(layers):
    """Composite bottom-first native proof layers without source-paint edits."""
    result = Image.new('RGBA', SIZE)
    for _, layer in layers:
        result.alpha_composite(layer)
    return result


def png_bytes(image):
    """Encode deterministic PNG bytes for a review image or editable composition layer."""
    buffer = BytesIO()
    image.save(buffer, format='PNG')
    return buffer.getvalue()


def ora_bytes(title, layers):
    """Store an exact nearest-2x composition master with separate paint/coverage layers."""
    master_size = tuple(value * 2 for value in SIZE)
    xml = Element('image', w=str(master_size[0]), h=str(master_size[1]), name=title, version='0.0.3')
    stack = SubElement(xml, 'stack')
    members = [('mimetype', b'image/openraster')]
    top_first = [('Corrections', Image.new('RGBA', SIZE)), *reversed(layers)]
    for index, (name, layer) in enumerate(top_first):
        filename = f'data/layer-{index}.png'
        SubElement(stack, 'layer', name=name, src=filename, x='0', y='0', opacity='1.0',
                   visibility='visible', **{'composite-op': 'svg:src-over'})
        members.append((filename, png_bytes(layer.resize(master_size, Image.Resampling.NEAREST))))
    members += [('stack.xml', tostring(xml)),
                ('mergedimage.png', png_bytes(merged(layers).resize(master_size, Image.Resampling.NEAREST)))]
    buffer = BytesIO()
    with zipfile.ZipFile(buffer, 'w') as archive:
        for name, data in members:
            item = zipfile.ZipInfo(name, date_time=(2026, 10, 4, 0, 0, 0))
            archive.writestr(item, data, compress_type=zipfile.ZIP_STORED)
    return buffer.getvalue()


def board(images):
    """Show baseline and both proposals at native scale with labels outside the artwork."""
    result = Image.new('RGB', (1600, 492), '#e8e3d5')
    draw = ImageDraw.Draw(result)
    draw.text((20, 12), 'Healer Garden / bounded external ground comparison', fill='#202820', font=LABEL_FONT)
    draw.text((20, 40), 'Native 1:1 source pixels / CPU recipe reconstruction / existing paths and all nine Garden sprites retained',
              fill='#202820', font=SMALL_FONT)
    for index, name in enumerate(VARIANTS):
        x, y = 16 + index * 528, 76
        result.paste(images[name].convert('RGB'), (x, y))
        draw.text((x, y + SIZE[1] + 12), name, fill='#202820', font=LABEL_FONT)
    return result


def render():
    """Build native comparison views, retained sharp-mask diagnostics and editable masters."""
    _, _, _, bound = inputs()
    bound.update({path: digest(REPO / path) for path in INPUT_CODE})
    bound[str(FONT_PATH)] = digest(FONT_PATH)
    anchor, sprites, beds = scene_sprites()
    manifest = read_json(PRODUCTION / 'asset-manifest.json')
    assets = {asset['export'].split('/')[-1]: asset for asset in manifest['assets']}
    runtime_names = {Path(sprite['source']).name for sprite in sprites}
    runtime_names.update(('quiet-moss.png', 'square-garden-path.png', 'square-sickhouse-path.png',
                          'village-square-junction.png'))
    for name in sorted(runtime_names):
        asset = assets[name]
        source = REPO / 'game/art/runtime' / asset['export'].removeprefix('exports/')
        require(digest(source) == asset['sha256'], f'Runtime source/manifest mismatch: {name}')
        bound[str(source.relative_to(REPO))] = digest(source)
    for sprite in sprites:
        sprite['size'] = list(raster(REPO / sprite['source']).size)
    boxes = [(*bed['world_origin'], bed['world_origin'][0] + bed['size'][0],
              bed['world_origin'][1] + bed['size'][1]) for bed in beds]
    footprint = (min(box[0] for box in boxes), min(box[1] for box in boxes),
                 max(box[2] for box in boxes), max(box[3] for box in boxes))
    require(footprint == (2196, 1275, 2384, 1355), 'Bed source envelope changed')
    apron = tuple(value + (-APRON_WIDTH if index < 2 else APRON_WIDTH)
                  for index, value in enumerate(footprint))
    # Use the exact combined bed alpha rather than their crop-frame rectangle.
    # The bounding frame includes transparent corners that otherwise become a dark mat.
    bed_plane = Image.new('RGBA', SIZE)
    for bed in beds:
        bed_plane.alpha_composite(clipped_layer(raster(REPO / bed['source']), bed['world_origin']))
    soil_mask = bed_plane.getchannel('A')
    apron_mask = coverage(apron, SOIL_RADIUS + APRON_WIDTH, APRON_FEATHER)
    moss = repeated(REPO / 'game/art/runtime/terrain/quiet-moss.png', 256)
    soil = repeated(TILING / 'exports/garden-soil.png', 128)
    meadow = repeated(TILING / 'exports/mixed-meadow.png', 128)
    routes = scene_routes()
    route_layers = [(route['name'], clipped_layer(raster(REPO / route['source']), route['world_origin']))
                    for route in routes]
    garden_layers = [(sprite['name'], clipped_layer(raster(REPO / sprite['source']), sprite['world_origin']))
                     for sprite in sprites]
    top_layers = route_layers + garden_layers
    variants = {'baseline': [('Quiet moss', moss), *top_layers],
                'soil-only': [('Quiet moss', moss), ('Soil coverage', masked(soil, soil_mask)), *top_layers],
                'meadow-apron': [('Quiet moss', moss), ('Meadow apron', masked(meadow, apron_mask)),
                                 ('Soil coverage', masked(soil, soil_mask)), *top_layers]}
    images = {name: merged(layers) for name, layers in variants.items()}
    outputs = {}
    for name, image in images.items():
        outputs[f'review/{name}-1x.png'] = png_bytes(image)
        outputs[f'review/{name}-detail-2x.png'] = png_bytes(
            image.crop(DETAIL_BOX).resize((640, 448), Image.Resampling.NEAREST))
        if name != 'baseline':
            outputs[f'masters/{name}.ora'] = ora_bytes(name, variants[name])
    outputs['review/comparison-1x.png'] = png_bytes(board(images))
    for name, mask in (('soil', soil_mask), ('apron', apron_mask)):
        outputs[f'proof-inputs/{name}-coverage.png'] = png_bytes(mask)
    outputs['proof-inputs/soil-layer.png'] = png_bytes(masked(soil, soil_mask))
    outputs['proof-inputs/meadow-layer.png'] = png_bytes(masked(meadow, apron_mask))
    sharp_soil = [('Quiet moss', moss), ('Sharp soil envelope', masked(soil, coverage(footprint, 0, 0))), *top_layers]
    sharp_apron = [('Quiet moss', moss), ('Sharp meadow envelope', masked(meadow, coverage(apron, 0, 0))),
                   ('Sharp soil envelope', masked(soil, coverage(footprint, 0, 0))), *top_layers]
    outputs['evidence/sharp-soil-1x.png'] = png_bytes(merged(sharp_soil))
    outputs['evidence/sharp-apron-1x.png'] = png_bytes(merged(sharp_apron))
    rounded_soil = [('Quiet moss', moss), ('Rounded crop-frame soil', masked(soil, coverage(footprint, SOIL_RADIUS, 6))),
                    *top_layers]
    rounded_apron = [('Quiet moss', moss), ('Meadow apron', masked(meadow, apron_mask)),
                     ('Rounded crop-frame soil', masked(soil, coverage(footprint, SOIL_RADIUS, 6))), *top_layers]
    outputs['evidence/rounded-envelope-soil-1x.png'] = png_bytes(merged(rounded_soil))
    outputs['evidence/rounded-envelope-apron-1x.png'] = png_bytes(merged(rounded_apron))
    baseline = np.asarray(images['baseline'])
    measurements = {}
    for name in ('soil-only', 'meadow-apron'):
        changed = np.any(np.asarray(images[name]) != baseline, axis=2)
        raw_mask = np.asarray(soil_mask if name == 'soil-only' else apron_mask) > 0
        require(not np.any(changed & ~raw_mask), f'{name} changed pixels beyond scope')
        measurements[name] = {'visible_changed_pixels': int(changed.sum()),
                              'coverage_pixels': int(raw_mask.sum()),
                              'unchanged_fraction_inside_coverage': round(float((~changed)[raw_mask].mean()), 4)}
    report = {'schema_version': 1, 'date': '2026-10-04', 'timezone': 'Australia/Sydney',
              'scope': 'External Garden soil underlay within retained bed source envelope, plus optional 32px meadow apron',
              'owner_selection': {'statement': 'OK - please do it',
                                  'scope': 'The suggested first Garden external comparison, not artwork acceptance or runtime promotion'},
              'inputs': dict(sorted(bound.items())), 'world_origin': list(ORIGIN), 'native_size': list(SIZE),
              'garden_anchor': list(anchor), 'garden_sprites': sprites,
              'route_layers': routes,
              'soil_envelope': list(footprint), 'meadow_envelope': list(apron),
              'geometry_limit': 'The soil mask is the exact combined alpha of the retained bed sprites. Their source rectangles bound the proof but do not define new gameplay cultivation geometry. Soil already painted inside the raised beds stays untouched.',
              'coverage': {'soil_mask': 'Exact source-over union of both retained bed alpha channels',
                           'apron_corner_radius': SOIL_RADIUS + APRON_WIDTH,
                           'apron_width': APRON_WIDTH, 'apron_inward_feather': APRON_FEATHER,
                           'status': 'Provisional composition masks, not approved or import-ready transition modules'},
              'paint_phase': {'quiet_moss_period': 256, 'new_field_period': 128, 'origin': [0, 0],
                              'rotation_degrees': 0, 'scale': 1},
              'measurements': measurements,
              'view_limit': 'CPU native-source reconstruction; no Godot GPU filtering, labels, actors, collision, navigation or interaction claim. Route and Garden source pixels retain their declared order.',
              'master_limit': 'Separate editable layers at exact nearest 2x; enlargement preserves source texels and adds no painted detail.'}
    return outputs, report


def main():
    """Write only this isolated package or recompute exact bytes without mutation."""
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--check', action='store_true')
    args = parser.parse_args()
    outputs, report = render()
    selection_path = ROOT / 'owner-selection.json'
    if selection_path.exists():
        selection = read_json(selection_path)
        require(selection['status'] == 'meadow-apron variant selected', 'Unexpected Garden selection status')
        require(set(selection['selected_files']) == SELECTED_NAMES, 'Incomplete selected proof binding')
        for name, expected in selection['selected_files'].items():
            require(name in outputs and hashlib.sha256(outputs[name]).hexdigest() == expected,
                    f'Selected Garden proof changed; use a separate revision: {name}')
    if args.check:
        saved = read_json(ROOT / 'inventory.json')
        for name, expected in outputs.items():
            require((ROOT / name).read_bytes() == expected, f'Proof output changed: {name}')
        report['outputs'] = {name: digest(ROOT / name) for name in outputs}
        require(report == saved, 'Proof metadata or frozen input changed')
    else:
        require(not (ROOT / 'owner-acceptance.json').exists(), 'Accepted Garden proof is frozen')
        for name, data in outputs.items():
            path = ROOT / name
            path.parent.mkdir(parents=True, exist_ok=True)
            path.write_bytes(data)
        report['outputs'] = {name: digest(ROOT / name) for name in outputs}
        (ROOT / 'inventory.json').write_text(json.dumps(report, indent=2) + '\n')
    print('PASS: exact baseline, soil-only and meadow-apron views; nine retained Garden sprites; isolated editable 2x masters.')


if __name__ == '__main__':
    main()
