#!/usr/bin/env python3
"""Independently check native phase, source-over composition, masks and editable masters."""

from io import BytesIO
import json
import zipfile
from xml.etree import ElementTree

import numpy as np
from PIL import Image

import build

ROOT, REPO = build.ROOT, build.REPO


def array(path):
    """Load detached source pixels for independent integer calculations."""
    with Image.open(path) as image:
        return np.asarray(image.convert('RGBA'), dtype=np.int64)


def field(path, origin, size):
    """Sample a periodic field independently using absolute-world coordinates."""
    paint = array(path)
    yy, xx = np.indices((size[1], size[0]))
    return paint[(yy + origin[1]) % paint.shape[0], (xx + origin[0]) % paint.shape[1]]


def over_opaque(background, paint):
    """Apply integer source-over onto opaque RGB without Pillow's compositor."""
    alpha = paint[:, :, 3:4]
    result = background.copy()
    result[:, :, :3] = (paint[:, :, :3] * alpha + background[:, :, :3] * (255 - alpha) + 127) // 255
    result[:, :, 3] = 255
    return result


def placed(source, world, origin, size):
    """Project a recorded source rectangle independently into the review window."""
    result = np.zeros((size[1], size[0], 4), dtype=np.int64)
    paint = array(source)
    left, top = world[0] - origin[0], world[1] - origin[1]
    x0, y0 = max(0, left), max(0, top)
    x1, y1 = min(size[0], left + paint.shape[1]), min(size[1], top + paint.shape[0])
    if x0 < x1 and y0 < y1:
        result[y0:y1, x0:x1] = paint[y0-top:y1-top, x0-left:x1-left]
    return result


def main():
    """Reject drift and verify all three views against independently sampled source texels."""
    saved = json.loads((ROOT / 'inventory.json').read_text())
    outputs, expected_report = build.render()
    for name, expected in outputs.items():
        build.require((ROOT / name).read_bytes() == expected, f'Byte reproduction drift: {name}')
    expected_report['outputs'] = {name: build.digest(ROOT / name) for name in outputs}
    build.require(saved == expected_report, 'Input or metadata drift')
    selection = json.loads((ROOT / 'owner-selection.json').read_text())
    build.require(selection['status'] == 'meadow-apron variant selected', 'Missing variant selection')
    build.require(set(selection['selected_files']) == build.SELECTED_NAMES, 'Incomplete variant binding')
    for name, expected in selection['selected_files'].items():
        build.require(build.digest(ROOT / name) == expected, f'Selected proof drift: {name}')
    origin, size = saved['world_origin'], saved['native_size']
    moss = field(REPO / 'game/art/runtime/terrain/quiet-moss.png', origin, size)
    soil = field(build.TILING / 'exports/garden-soil.png', origin, size)
    meadow = field(build.TILING / 'exports/mixed-meadow.png', origin, size)
    masks = {}
    for name in ('soil', 'apron'):
        with Image.open(ROOT / f'proof-inputs/{name}-coverage.png') as image:
            build.require(image.mode == 'L' and image.size == tuple(size), 'Coverage format')
            masks[name] = np.asarray(image, dtype=np.int64)
    top_layers = [placed(REPO / row['source'], row['world_origin'], origin, size)
                  for row in saved['route_layers'] + saved['garden_sprites']]
    bed_alpha = np.zeros((size[1], size[0]), dtype=np.int64)
    for row in saved['garden_sprites']:
        if row['name'].startswith('HerbBed'):
            alpha = placed(REPO / row['source'], row['world_origin'], origin, size)[:, :, 3]
            bed_alpha = (alpha * 255 + bed_alpha * (255 - alpha) + 127) // 255
    build.require(np.array_equal(bed_alpha, masks['soil']), 'Soil coverage exceeds exact retained bed alpha')
    soil[:, :, 3], meadow[:, :, 3] = masks['soil'], masks['apron']
    views = {}
    for name, underlays in [('baseline', []), ('soil-only', [soil]), ('meadow-apron', [meadow, soil])]:
        expected = moss.copy()
        for plane in underlays + top_layers:
            expected = over_opaque(expected, plane)
        actual = array(ROOT / f'review/{name}-1x.png')
        build.require(np.array_equal(actual, expected), f'Independent native compositing mismatch: {name}')
        views[name] = actual
    opaque_top = np.zeros(masks['soil'].shape, dtype=bool)
    for plane in top_layers:
        opaque_top |= plane[:, :, 3] == 255
    for name, mask in [('soil-only', masks['soil']), ('meadow-apron', masks['apron'])]:
        changed = np.any(views[name] != views['baseline'], axis=2)
        build.require(not (changed & (mask == 0)).any(), f'Changes outside {name} mask')
        build.require(not (changed & opaque_top).any(), 'An opaque retained route/object pixel changed')
    master_layers = 0
    for name in ('soil-only', 'meadow-apron'):
        with zipfile.ZipFile(ROOT / f'masters/{name}.ora') as archive:
            build.require(archive.testzip() is None and archive.read('mimetype') == b'image/openraster', 'ORA container')
            xml = ElementTree.fromstring(archive.read('stack.xml'))
            build.require((int(xml.attrib['w']), int(xml.attrib['h'])) == (size[0] * 2, size[1] * 2), '2x geometry')
            layers = list(xml.find('stack'))
            build.require(layers[0].attrib['name'] == 'Corrections', 'Editable corrections layer')
            composite = Image.new('RGBA', (size[0] * 2, size[1] * 2))
            for layer in reversed(layers):
                build.require(layer.attrib['x'] == layer.attrib['y'] == '0' and
                              layer.attrib['opacity'] == '1.0' and layer.attrib['visibility'] == 'visible' and
                              layer.attrib['composite-op'] == 'svg:src-over', 'Unsupported master transform')
                with Image.open(BytesIO(archive.read(layer.attrib['src']))) as image:
                    build.require(image.mode == 'RGBA' and image.size == composite.size, 'Layer geometry')
                    composite.alpha_composite(image)
                master_layers += 1
            with Image.open(BytesIO(archive.read('mergedimage.png'))) as stored:
                build.require(stored.tobytes() == composite.tobytes(), 'Stored master merge drift')
            expected = Image.fromarray(views[name].astype('uint8')).resize(composite.size, Image.Resampling.NEAREST)
            build.require(composite.tobytes() == expected.tobytes(), 'Master/native-view parity')
    result = {'status': 'scoped Garden proof checks pass', 'native_views_independently_verified': 3,
              'source_layers_per_view': len(top_layers), 'editable_2x_masters': 2,
              'master_layers_verified': master_layers, 'byte_reproduced_outputs': len(outputs),
              'source_bound_inputs': len(saved['inputs']), 'measurements': saved['measurements'],
              'scope': 'CPU composition and source preservation only; no owner visual or runtime acceptance'}
    captures = json.loads((ROOT / 'capture-evidence.json').read_text())
    for key, path in [('inventory_sha256', ROOT / 'inventory.json'), ('scene_sha256', build.SCENE),
                      ('script_sha256', ROOT / 'capture.gd'),
                      ('soil_layer_sha256', ROOT / 'proof-inputs/soil-layer.png'),
                      ('meadow_layer_sha256', ROOT / 'proof-inputs/meadow-layer.png')]:
        build.require(captures[key] == build.digest(path), f'Capture source drift: {key}')
    build.require(captures['viewport'] == [1280, 720] and captures['camera_center'] == [2300, 1300],
                  'Capture framing changed')
    expected_capture_names = {f'review/godot-{name}-{zoom:.1f}x.png'
                              for name in ('baseline', 'soil-only', 'meadow-apron') for zoom in (1.0, 2.0)}
    build.require(set(captures['captures']) == expected_capture_names, 'Incomplete GPU capture matrix')
    gpu_rows = []
    for zoom in (1.0, 2.0):
        baseline = array(ROOT / f'review/godot-baseline-{zoom:.1f}x.png')
        for name, bounds in [('soil-only', saved['soil_envelope']), ('meadow-apron', saved['meadow_envelope'])]:
            filename = f'review/godot-{name}-{zoom:.1f}x.png'
            actual = array(ROOT / filename)
            build.require(build.digest(ROOT / filename) == captures['captures'][filename], 'GPU PNG bytes drift')
            build.require(actual.shape == baseline.shape == (720, 1280, 4), 'GPU PNG dimensions')
            changed = np.any(actual != baseline, axis=2)
            # Linear filtering may influence neighbouring screen pixels at a support edge.
            x0 = int(640 + (bounds[0] - 2300) * zoom) - 2
            y0 = int(360 + (bounds[1] - 1300) * zoom) - 2
            x1 = int(640 + (bounds[2] - 2300) * zoom) + 2
            y1 = int(360 + (bounds[3] - 1300) * zoom) + 2
            permitted = np.zeros(changed.shape, dtype=bool)
            permitted[max(0, y0):min(720, y1), max(0, x0):min(1280, x1)] = True
            build.require(changed.any() and not (changed & ~permitted).any(), 'GPU changes outside bounded proof')
            gpu_rows.append({'variant': name, 'zoom': zoom, 'changed_pixels': int(changed.sum())})
        baseline_name = f'review/godot-baseline-{zoom:.1f}x.png'
        build.require(build.digest(ROOT / baseline_name) == captures['captures'][baseline_name], 'Baseline capture drift')
    result.update({'godot_preview_captures': 6, 'gpu_change_scope': gpu_rows,
                   'scope': 'CPU composition, immutable source paint and bounded disposable GPU previews; no named runtime promotion'})
    print(json.dumps(result, indent=2))


if __name__ == '__main__':
    main()
