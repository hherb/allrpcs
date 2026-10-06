#!/usr/bin/env python3
"""Arrange the entire external GPU comparison, with exact native detail crops."""

import argparse
import hashlib
import json

from PIL import Image, ImageDraw, ImageFont

from build import FONT, ORIGIN, ROOT, SIZE, VARIANTS, png, raster, require, sha, write_or_check


def label(draw, xy, text, size=17):
    draw.text(xy, text, font=ImageFont.truetype(str(FONT), size), fill='#25312e')


def crop(image, transform, scale):
    a, b, c, d, tx, ty = transform
    require(a == d == scale and b == c == 0, 'Non-native crop axes')
    bounds = (ORIGIN[0] * a + tx, ORIGIN[1] * d + ty,
              (ORIGIN[0] + SIZE[0]) * a + tx, (ORIGIN[1] + SIZE[1]) * d + ty)
    require(all(v == round(v) for v in bounds), 'Fractional native crop bounds')
    require(0 <= bounds[0] < bounds[2] <= image.width
            and 0 <= bounds[1] < bounds[3] <= image.height, 'Detail crop outside viewport')
    return image.crop(tuple(map(int, bounds)))


def generate():
    evidence = json.loads((ROOT / 'capture-evidence.json').read_text())
    expected = {f'captures/{name}-{zoom:.1f}x-{frame}.png'
                for name in VARIANTS for zoom in (0.8, 1, 2) for frame in ('wide', 'tall')}
    require(set(evidence['captures']) == set(evidence['frames']) == expected,
            'Incomplete 24-view GPU matrix')
    sources = {'boards.py': sha(ROOT / 'boards.py'), 'capture-evidence.json': sha(ROOT / 'capture-evidence.json'),
               str(FONT): sha(FONT)}
    images = {}
    for name in sorted(expected):
        require(sha(ROOT / name) == evidence['captures'][name], 'Capture drift: ' + name)
        sources[name] = sha(ROOT / name)
        images[name] = raster(ROOT / name)
        require(list(images[name].size) == evidence['frames'][name]['viewport'], 'Viewport drift')
    outputs = {}
    for frame, height in (('wide', 720), ('tall', 827)):
        sample_height = round(height * 0.4)
        board = Image.new('RGB', (2128, 85 + 3 * (sample_height + 58)), '#eee9df')
        draw = ImageDraw.Draw(board)
        label(draw, (16, 12), f'Clearing opening / complete {frame} comparison / external only', 24)
        label(draw, (16, 47), 'Columns: retained baseline, hard support, feather 16, feather 24. Thumbnails 0.4x.')
        for row, zoom in enumerate((0.8, 1, 2)):
            for col, variant in enumerate(VARIANTS):
                name = f'captures/{variant}-{zoom:.1f}x-{frame}.png'
                x, y = 16 + col * 528, 85 + row * (sample_height + 58)
                board.paste(images[name].resize((512, sample_height), Image.Resampling.LANCZOS).convert('RGB'), (x, y))
                label(draw, (x, y + sample_height + 8), f'{variant} / camera {zoom:.1f}x / {frame}')
                center = evidence['frames'][name]['screen_center']
                label(draw, (x, y + sample_height + 29), f'actual world center {center}', 14)
        outputs[f'review/gpu-series-{frame}.png'] = png(board)
    for scale in (1, 2):
        width, height = SIZE[0] * scale, SIZE[1] * scale
        board = Image.new('RGB', (16 + (width + 16) * 4, height + 106), '#eee9df')
        draw = ImageDraw.Draw(board)
        label(draw, (16, 12), f'Opening and shoulder / exact GPU pixels at camera {scale}x', 24)
        label(draw, (16, 47), 'Entire authoring envelope; route, Clearing transition and channel retained. No resampling.')
        for index, variant in enumerate(VARIANTS):
            name = f'captures/{variant}-{scale:.1f}x-wide.png'
            detail = crop(images[name], evidence['frames'][name]['world_to_canvas'], scale)
            outputs[f'review/{variant}-detail-{scale}x.png'] = png(detail)
            x = 16 + index * (width + 16)
            board.paste(detail.convert('RGB'), (x, 76))
            label(draw, (x, height + 82), variant)
        outputs[f'review/complete-details-{scale}x.png'] = png(board)
    record = {'sources': sources,
              'outputs': {name: hashlib.sha256(data).hexdigest() for name, data in outputs.items()}}
    outputs['review-board-manifest.json'] = (json.dumps(record, indent=2) + '\n').encode()
    return outputs


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--check', action='store_true')
    args = parser.parse_args()
    write_or_check(generate(), args.check)
    print('PASS: all 24 views; complete exact 1x/2x detail comparisons')


if __name__ == '__main__':
    main()
