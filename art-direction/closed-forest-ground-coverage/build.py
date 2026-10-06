#!/usr/bin/env python3
"""Reproduce the complete external gate audit without changing product sources."""
import argparse
import hashlib
from io import BytesIO
import json
from pathlib import Path
from PIL import Image, ImageDraw, ImageFont

ROOT = Path(__file__).resolve().parent
REPO = ROOT.parents[1]
FONT = Path('/System/Library/Fonts/Supplemental/Arial.ttf')
BOX = (650, 170, 900, 350)


def require(value, message):
    if not value:
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


def label(draw, xy, text, size=17):
    draw.text(xy, text, font=ImageFont.truetype(str(FONT), size), fill='#25312e')


def generate():
    evidence = json.loads((ROOT / 'capture-evidence.json').read_text())
    sources = {'build.py': sha(ROOT / 'build.py'), 'capture-evidence.json': sha(ROOT / 'capture-evidence.json'), str(FONT): sha(FONT)}
    expected = {f'captures/baseline-{zoom:.1f}x-{frame}.png' for zoom in (0.8, 1, 2) for frame in ('wide', 'tall')}
    require(set(evidence['captures']) == set(evidence['frames']) == expected, 'Incomplete baseline series')
    board = Image.new('RGB', (1328, 1480), '#eee9df')
    draw = ImageDraw.Draw(board)
    label(draw, (16, 12), 'Closed forest approach / complete retained baseline series', 25)
    label(draw, (16, 48), 'Camera 0.8x, 1x, 2x; ordinary world limits retained. Thumbnails 0.5x.')
    outputs = {}
    for row, zoom in enumerate((0.8, 1, 2)):
        for col, frame in enumerate(('wide', 'tall')):
            name = f'captures/baseline-{zoom:.1f}x-{frame}.png'
            require(sha(ROOT / name) == evidence['captures'][name], 'Capture drift: ' + name)
            sources[name] = sha(ROOT / name)
            image = raster(ROOT / name)
            record = evidence['frames'][name]
            require(list(image.size) == record['viewport'], 'Viewport mismatch')
            require(abs(record['world_to_canvas'][0] - zoom) < 1e-6, 'Zoom mismatch')
            thumb = image.resize((640, round(image.height / 2)), Image.Resampling.LANCZOS)
            x, y = 16 + col * 656, 86 + row * 460
            board.paste(thumb.convert('RGB'), (x, y))
            label(draw, (x, y + thumb.height + 7), f'{zoom:.1f}x / {frame} / actual center {record["screen_center"]}')
    outputs['review/baseline-series.png'] = png(board)
    details = Image.new('RGB', (800, 500), '#eee9df')
    draw = ImageDraw.Draw(details)
    label(draw, (16, 12), 'Gate and approach / exact GPU pixels', 25)
    label(draw, (16, 48), 'No resampling, injected paint or physical changes.')
    for scale, x in ((1, 16), (2, 282)):
        name = f'captures/baseline-{scale:.1f}x-wide.png'
        a, b, c, d, tx, ty = evidence['frames'][name]['world_to_canvas']
        require(a == d == scale and b == c == 0, 'Non-native crop transform')
        bounds = (BOX[0]*a+tx, BOX[1]*d+ty, BOX[2]*a+tx, BOX[3]*d+ty)
        require(all(v == round(v) for v in bounds), 'Fractional crop bounds')
        image = raster(ROOT / name)
        require(0 <= bounds[0] < bounds[2] <= image.width and 0 <= bounds[1] < bounds[3] <= image.height, 'Crop outside viewport')
        crop = image.crop(tuple(map(int, bounds)))
        outputs[f'review/gate-{scale}x.png'] = png(crop)
        details.paste(crop.convert('RGB'), (x, 86))
        label(draw, (x, 454), f'camera {scale}x / native pixels')
    outputs['review/gate-details.png'] = png(details)
    owners = evidence['owners']
    ground = owners['Ground']
    require(ground['texture'] == 'res://art/runtime/terrain/quiet-moss.png' and ground['z_index'] == -100, 'Ground changed')
    moss = raster(REPO / 'game/art/runtime/terrain/quiet-moss.png')
    require(moss.getchannel('A').getextrema() == (255, 255), 'Ground substrate holes')
    route = owners['GreyboxNavigation/RoutePresentation/ClosedForestPath']
    require(route['texture'] == 'res://art/runtime/terrain/clearing-closed-forest-path.png' and route['size'] == [552,384] and route['world_transform'] == [1,0,0,1,796,224] and route['z_index'] == -75, 'Accepted route changed')
    gate = owners['OpeningRegionClosedForestPathSceneSource/ClosedGate/CollisionShape2D']
    require(gate['segment'] == [[696,200],[792,200]] and not gate['disabled'], 'Gate changed')
    alpha = raster(REPO / 'game' / route['texture'].removeprefix('res://')).getchannel('A')
    record = {
        'status': 'external coverage audit complete; retain accepted approach unchanged',
        'date': '2026-10-06', 'baseline_head': json.loads((ROOT / 'baseline.json').read_text())['head'],
        'finding': 'Opaque quiet moss prevents unpainted alpha holes. The accepted closed-forest route already carries a broad leaf-litter transition around the gate approach. No additional apron is proposed by this audit.',
        'scope_limit': 'Coverage does not establish terrain completeness or replacement of visible greybox boundaries. The native gate and diagonal approach barriers remain unchanged.',
        'ground_alpha_extrema': [255,255], 'route_owner': route,
        'route_alpha_bounds_local': list(alpha.getbbox()), 'gate': gate,
        'detail_world_bounds': list(BOX),
        'camera_policy': 'Ordinary world limits retained: wide/tall overview centers differ near world top. Crop coordinates use actual recorded transforms.',
        'sources': sources, 'outputs': {k: hashlib.sha256(v).hexdigest() for k,v in outputs.items()}}
    outputs['audit.json'] = (json.dumps(record, indent=2) + '\n').encode()
    return outputs


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--check', action='store_true')
    args = parser.parse_args()
    require(args.check or not (ROOT / 'audit.json').exists(), 'Existing audit is immutable')
    for name, data in generate().items():
        path = ROOT / name
        if args.check:
            require(path.read_bytes() == data, 'Output drift: ' + name)
        else:
            require(not path.exists(), 'Refusing overwrite: ' + name)
            path.write_bytes(data)
    print('PASS: six baseline views and exact 1x/2x gate crops; accepted route and native gate preserved')


if __name__ == '__main__':
    main()
