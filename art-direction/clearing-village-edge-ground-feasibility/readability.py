#!/usr/bin/env python3
"""Create static colour-vision/luminance comparisons from unchanged GPU detail pixels."""

import argparse
import hashlib
import json

import numpy as np
from PIL import Image, ImageDraw

from boards import label
from build import ROOT, SIZE, VARIANTS, FONT, png, raster, require, sha, write_or_check

CONDITIONS = ('normal', 'protanomaly', 'deuteranomaly', 'tritanomaly', 'luminance')


def simulate(image, condition, model):
    pixels = np.asarray(image)
    require(pixels.shape[2] == 4 and np.all(pixels[:, :, 3] == 255), 'Diagnostic requires opaque GPU pixels')
    if condition == 'normal':
        return image.copy()
    srgb = pixels[:, :, :3].astype(np.float64) / 255
    linear = np.where(srgb <= 0.04045, srgb / 12.92, ((srgb + 0.055) / 1.055) ** 2.4)
    if condition == 'luminance':
        luminance = linear @ np.asarray(model['luminance_weights'])
        transformed = np.repeat(luminance[:, :, None], 3, axis=2)
    else:
        transformed = linear @ np.asarray(model['matrices'][condition]).T
    transformed = np.clip(transformed, 0, 1)
    encoded = np.where(transformed <= 0.0031308, transformed * 12.92,
                       1.055 * transformed ** (1 / 2.4) - 0.055)
    result = pixels.copy()
    result[:, :, :3] = np.rint(np.clip(encoded, 0, 1) * 255).astype('uint8')
    return Image.fromarray(result)


def generate():
    model = json.loads((ROOT / 'simulation-model.json').read_text())
    require(model['severity'] == 100 and set(model['matrices']) == set(CONDITIONS[1:4]), 'Model scope drift')
    require(model['luminance_weights'] == [0.2126, 0.7152, 0.0722], 'Luminance weights drift')
    manifest = json.loads((ROOT / 'review-board-manifest.json').read_text())
    sources = {'readability.py': sha(ROOT / 'readability.py'), 'boards.py': sha(ROOT / 'boards.py'),
               'build.py': sha(ROOT / 'build.py'), 'simulation-model.json': sha(ROOT / 'simulation-model.json'),
               'review-board-manifest.json': sha(ROOT / 'review-board-manifest.json'), str(FONT): sha(FONT)}
    outputs = {}
    for scale in (1, 2):
        width, height = SIZE[0] * scale, SIZE[1] * scale
        board = Image.new('RGB', (16 + (width + 16) * 4, 86 + len(CONDITIONS) * (height + 54)), '#eee9df')
        draw = ImageDraw.Draw(board)
        label(draw, (16, 12), f'Clearing shoulder / colour-vision and luminance comparisons / actual camera {scale}x', 23)
        label(draw, (16, 47), 'Rows: normal, protan, deutan, tritan (severity 100), luminance. Static diagnostics; no accessibility verdict.')
        for col, variant in enumerate(VARIANTS):
            name = f'review/{variant}-detail-{scale}x.png'
            sources[name] = sha(ROOT / name)
            require(sources[name] == manifest['outputs'][name], 'GPU detail source drift')
            image = raster(ROOT / name)
            require(image.size == (width, height), 'Diagnostic input scale changed')
            for row, condition in enumerate(CONDITIONS):
                preview = simulate(image, condition, model)
                outputs[f'readability/{variant}-{condition}-{scale}x.png'] = png(preview)
                x, y = 16 + col * (width + 16), 86 + row * (height + 54)
                board.paste(preview.convert('RGB'), (x, y))
                label(draw, (x, y + height + 8), f'{variant} / {condition}', 16)
        outputs[f'readability/complete-series-{scale}x.png'] = png(board)
    record = {'status': 'static diagnostic series complete; human accessibility remains open',
              'date': '2026-10-06', 'conditions': list(CONDITIONS), 'scales': [1, 2],
              'model': model, 'sources': sources,
              'outputs': {name: hashlib.sha256(data).hexdigest() for name, data in outputs.items()}}
    outputs['readability-manifest.json'] = (json.dumps(record, indent=2) + '\n').encode()
    return outputs


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--check', action='store_true')
    args = parser.parse_args()
    write_or_check(generate(), args.check)
    print('PASS: 40 source-bound colour-vision/luminance images; two complete native-scale boards')


if __name__ == '__main__':
    main()
