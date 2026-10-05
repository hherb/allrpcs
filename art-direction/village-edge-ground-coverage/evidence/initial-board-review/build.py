#!/usr/bin/env python3
"""Measure retained entrance coverage and arrange immutable GPU baseline evidence."""

import argparse
import hashlib
from io import BytesIO
import json
from pathlib import Path

import numpy as np
from PIL import Image, ImageDraw, ImageFilter, ImageFont

ROOT = Path(__file__).resolve().parent
REPO = ROOT.parents[1]
FONT = Path('/System/Library/Fonts/Supplemental/Arial.ttf')
ORIGIN = (1480, 620)
SIZE = (500, 390)
CROPS = {'woodland': (1490, 730, 1630, 880), 'stream': (1670, 660, 1830, 770),
         'square': (1760, 815, 1948, 985)}
SEGMENTS = {
    'woodland': ('WoodlandVillageEdgePath', (1532, 730, 1580, 880)),
    'stream': ('VillageEdgeStreamPath', (1678, 652, 1822, 700)),
    'square': ('VillageEdgeSquareConnector', (1820, 828, 1868, 972)),
}
PAPER = '#eee9df'


def require(condition, message):
    """Reject incomplete or changed evidence under ordinary and optimized Python."""
    if not condition:
        raise ValueError(message)


def sha(path):
    """Hash persisted bytes."""
    return hashlib.sha256(path.read_bytes()).hexdigest()


def png(image):
    """Serialize deterministic review pixels."""
    out = BytesIO()
    image.save(out, format='PNG')
    return out.getvalue()


def raster(path):
    """Read detached RGBA pixels."""
    with Image.open(path) as image:
        return image.convert('RGBA')


def text(draw, xy, value, size=17):
    """Draw labels outside the unchanged captured image pixels."""
    draw.text(xy, value, font=ImageFont.truetype(str(FONT), size), fill='#25312e')


def world_alpha(owner):
    """Place an unscaled native sprite alpha plane in the local audit envelope."""
    require(owner['world_transform'][:4] == [1, 0, 0, 1] and owner['offset'] == [0, 0],
            'Unexpected sprite transform')
    image = raster(REPO / 'game' / owner['texture'].removeprefix('res://'))
    require(list(image.size) == owner['size'], 'Texture geometry drift')
    x, y = owner['world_transform'][4:]
    if owner['centered']:
        x -= image.width / 2
        y -= image.height / 2
    require(x == int(x) and y == int(y), 'Fractional native sprite origin')
    canvas = Image.new('L', SIZE)
    canvas.paste(image.getchannel('A'), (int(x) - ORIGIN[0], int(y) - ORIGIN[1]))
    return canvas


def crop(evidence, name, scale, box):
    """Extract exact GPU pixels using the recorded canvas transform, without resampling."""
    path = f'captures/baseline-{scale:.1f}x-wide.png'
    a, b, c, d, tx, ty = evidence['frames'][path]['world_to_canvas']
    require(a == d == scale and b == c == 0, 'Crop axes are not exact')
    bounds = (box[0] * a + tx, box[1] * d + ty, box[2] * a + tx, box[3] * d + ty)
    require(all(value == round(value) for value in bounds), 'Fractional crop bounds')
    image = raster(ROOT / path)
    require(0 <= bounds[0] < bounds[2] <= image.width and 0 <= bounds[1] < bounds[3] <= image.height,
            'Crop outside captured viewport: ' + name)
    return image.crop(tuple(map(int, bounds)))


def generate():
    """Generate the entire labelled series and one measured external comparison recipe."""
    evidence = json.loads((ROOT / 'capture-evidence.json').read_text())
    inputs = json.loads((ROOT / 'inputs.json').read_text())['sources']
    for name, digest in inputs.items():
        require(sha(REPO / name) == digest, 'Bound input drift: ' + name)
    expected = {f'captures/baseline-{zoom:.1f}x-{frame}.png'
                for zoom in (0.8, 1.0, 2.0) for frame in ('wide', 'tall')}
    require(set(evidence['captures']) == expected and set(evidence['frames']) == expected,
            'Incomplete baseline series')
    sources = {'capture-evidence.json': sha(ROOT / 'capture-evidence.json'), str(FONT): sha(FONT),
               'build.py': sha(ROOT / 'build.py'), 'inputs.json': sha(ROOT / 'inputs.json')}
    for name in sorted(expected):
        require(sha(ROOT / name) == evidence['captures'][name], 'Capture drift: ' + name)
        sources[name] = sha(ROOT / name)
        frame = evidence['frames'][name]
        require(frame['screen_center'] == [1700, 840], 'Unexpected camera center')
        require(list(raster(ROOT / name).size) == frame['viewport'], 'Viewport drift')
    outputs = {}
    board = Image.new('RGB', (1328, 1394), PAPER)
    draw = ImageDraw.Draw(board)
    text(draw, (16, 12), 'Village Edge / complete retained baseline series / external audit', 25)
    text(draw, (16, 48), 'Wide and tall at camera 0.8x, 1x, 2x. Thumbnails 0.5x; full captures retained.')
    y = 86
    for zoom in (0.8, 1.0, 2.0):
        for col, frame in enumerate(('wide', 'tall')):
            image = raster(ROOT / f'captures/baseline-{zoom:.1f}x-{frame}.png')
            image = image.resize((640, round(image.height / 2)), Image.Resampling.LANCZOS)
            x = 16 + col * 656
            board.paste(image.convert('RGB'), (x, y))
            text(draw, (x, y + image.height + 7), f'baseline / camera {zoom:.1f}x / {frame}')
        y += 436
    outputs['review/baseline-series.png'] = png(board)
    for scale in (1, 2):
        images = {name: crop(evidence, name, scale, box) for name, box in CROPS.items()}
        width = sum(image.width + 16 for image in images.values()) + 16
        height = max(image.height for image in images.values()) + 105
        board = Image.new('RGB', (max(width, 760), height), PAPER)
        draw = ImageDraw.Draw(board)
        text(draw, (16, 12), f'All three entrances / exact GPU pixels / camera {scale}x', 23)
        text(draw, (16, 44), 'Shell/openings/routes retained; no paint injected; no crop resampling.', 16)
        x = 16
        for name, image in images.items():
            board.paste(image.convert('RGB'), (x, 76))
            text(draw, (x, 80 + image.height), name)
            outputs[f'review/{name}-{scale}x.png'] = png(image)
            x += image.width + 16
        outputs[f'review/entrances-{scale}x.png'] = png(board)

    owners = evidence['owners']
    ground = owners['Ground']
    require(ground['texture'] == 'res://art/runtime/terrain/quiet-moss.png'
            and ground['world_transform'] == [1, 0, 0, 1, 0, 0] and ground['z_index'] == -100,
            'Ground owner drift')
    moss = raster(REPO / 'game/art/runtime/terrain/quiet-moss.png')
    require(moss.getchannel('A').getextrema() == (255, 255), 'Ground substrate has holes')
    require('region_rect = Rect2(0, 0, 3200, 1800)' in (REPO / 'game/Main.tscn').read_text(),
            'Ground region changed')
    shell = owners['OpeningRegionVillageTransitionSceneSource/VillageEdgeArea']
    require(shell['polygon'] == [[1580, 700], [1820, 700], [1820, 1020], [1580, 1020]]
            and shell['visible'] and shell['z_index'] == 0, 'Retained shell changed')
    approval = json.loads((REPO / 'art-direction/residual-ground-tiling-proof/owner-acceptance.json').read_text())
    material = 'art-direction/residual-ground-tiling-proof/exports/packed-earth.png'
    require(approval['status'] == 'approved' and sha(REPO / material) ==
            approval['approved_artwork']['exports/packed-earth.png'], 'Approved material drift')
    support = Image.new('L', SIZE)
    measurements = {}
    all_routes = Image.new('L', SIZE)
    for name, (node, box) in SEGMENTS.items():
        owner = owners['GreyboxNavigation/RoutePresentation/' + node]
        alpha = world_alpha(owner)
        all_routes = Image.fromarray(np.maximum(np.asarray(all_routes), np.asarray(alpha)))
        selected = Image.new('L', SIZE)
        local = tuple(box[i] - ORIGIN[i % 2] for i in range(4))
        selected.paste(alpha.crop(local).point(lambda a: 255 if a else 0), local[:2])
        pixels = np.asarray(alpha.crop(local))
        require(np.count_nonzero(pixels) > 0, 'Empty entrance segment')
        expanded = selected.filter(ImageFilter.MaxFilter(49))
        support = Image.fromarray(np.maximum(np.asarray(support), np.asarray(expanded)))
        bounds = expanded.getbbox()
        measurements[name] = {'route_node': node, 'segment_world_bounds': list(box),
                              'route_nonzero_texels': int(np.count_nonzero(pixels)),
                              'route_translucent_texels': int(np.count_nonzero((pixels > 0) & (pixels < 255))),
                              'support_world_bounds': [bounds[i] + ORIGIN[i % 2] for i in range(4)],
                              'route_owner': owner}
    require(support.getbbox()[0] > 2 and support.getbbox()[1] > 2
            and support.getbbox()[2] < SIZE[0] - 2 and support.getbbox()[3] < SIZE[1] - 2,
            'Support/filter guard clipped by analysis envelope')
    guard = support.filter(ImageFilter.MaxFilter(5))
    exclusions = {}
    names = [name for name in owners if name.startswith('WholeChannelPresentation/')
             or ('GroundPresentation/' in name)]
    for name in names:
        if owners[name]['class'] != 'Sprite2D':
            continue
        alpha = world_alpha(owners[name])
        overlap = int(np.count_nonzero((np.asarray(guard) > 0) & (np.asarray(alpha) > 0)))
        require(overlap == 0, 'Candidate apron touches accepted owner: ' + name)
        exclusions[name] = overlap
    overlay = Image.new('RGBA', SIZE, '#d8dfd1')
    tint = Image.new('RGBA', SIZE, '#ddaa70')
    overlay.paste(tint, (0, 0), support)
    overlay.paste('#9e7040', (0, 0), all_routes)
    d = ImageDraw.Draw(overlay)
    d.rectangle((100, 80, 340, 389), outline='#25312e', width=2)
    diagram = Image.new('RGB', (900, 520), PAPER)
    d = ImageDraw.Draw(diagram)
    text(d, (16, 12), 'Candidate support diagram / geometry only / no new artwork', 24)
    diagram.paste(overlay.convert('RGB'), (16, 70))
    for row, line in enumerate(['Envelope origin (1480,620)', 'Orange: 24px route dilation',
                               'Brown: retained route alpha', 'Outline: retained shell',
                               'World phase (0,0), period 128', 'Future inward feathers 16 / 24',
                               'Candidate z=-78; routes above', '2px guard clear of accepted',
                               'channel and six ground owners']):
        text(d, (536, 86 + row * 34), line, 17)
    text(d, (16, 482), 'A comparison recipe is not an artwork selection or runtime scope approval.', 17)
    outputs['review/support-diagram.png'] = png(diagram)
    record = {'status': 'external coverage audit complete; optional apron comparison recipe only',
              'baseline_head': json.loads((ROOT / 'baseline.json').read_text())['head'],
              'date': '2026-10-05', 'timezone': 'Australia/Sydney',
              'finding': 'No unpainted alpha holes. All three exterior route shoulders use opaque quiet moss; no independent packed-earth apron.',
              'scope_limit': 'Coverage is not terrain-art completion. Retained opaque greybox shell occludes interior paint. An apron may soften route shoulders; necessity and preference require the external comparison.',
              'ground_alpha_extrema': [255, 255], 'entrances': measurements,
              'candidate_recipe': {'material': material, 'origin': list(ORIGIN), 'analysis_size': list(SIZE),
                                   'support': 'Union of nonzero native route alpha inside the three exterior segments; square dilation radius 24',
                                   'inward_feathers': [16, 24], 'feather_rule': 'Zero-pad by width+1, erode with 3x3 min filter; smoothstep coverage strictly inside support',
                                   'world_phase': [0, 0], 'period': 128, 'z_index': -78,
                                   'filter': 'linear', 'repeat': 'disabled', 'scale': [1, 1],
                                   'variants': ['baseline', 'hard-support', 'feather-16', 'feather-24'],
                                   'exclusions': 'Retain shell/openings, routes, channel, accepted ground and every physical owner; no route repaint, masking or runtime change'},
              'guard_overlap_texels': exclusions,
              'verification_policy': 'Lightweight external checks; retain predecessor 5033-test quality evidence because every tracked product and CI input is unchanged',
              'sources': sources,
              'outputs': {name: hashlib.sha256(data).hexdigest() for name, data in outputs.items()}}
    outputs['audit.json'] = (json.dumps(record, indent=2) + '\n').encode()
    return outputs


def main():
    """Write once, or reproduce all evidence bytes without modifying them."""
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--check', action='store_true')
    args = parser.parse_args()
    require(args.check or not (ROOT / 'audit.json').exists(), 'Existing audit is immutable')
    for name, data in generate().items():
        path = ROOT / name
        if args.check:
            require(path.read_bytes() == data, 'Audit output drift: ' + name)
        else:
            require(not path.exists(), 'Refusing to overwrite: ' + name)
            path.write_bytes(data)
    print('PASS: six retained GPU views, three native/2x entrance crops and guarded apron recipe')


if __name__ == '__main__':
    main()
