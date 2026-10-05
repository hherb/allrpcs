#!/usr/bin/env python3
"""Arrange all twenty-four disposable GPU views and all native detail comparisons."""

import argparse
import hashlib
import json
from pathlib import Path

from PIL import Image, ImageDraw

from build import CONTEXT_ORIGIN, ROOT, VARIANTS, CROPS, FONT, png, raster, require, sha, text


def generate():
    """Make complete labelled series boards; retain full-size captures separately."""
    evidence = json.loads((ROOT / 'capture-evidence.json').read_text())
    require(len(evidence['captures']) == 24, 'Incomplete GPU series')
    sources = {'capture-evidence.json': sha(ROOT / 'capture-evidence.json'), 'boards.py': sha(ROOT / 'boards.py'), str(FONT): sha(FONT)}
    outputs = {}
    for frame, height in [('wide', 720), ('tall', 827)]:
        sample_height = round(height * 0.4)
        board = Image.new('RGB', (2128, 85 + 3 * (sample_height + 49)), '#eee9df')
        draw = ImageDraw.Draw(board)
        text(draw, (16, 12), f'Village Edge / three entrances / all {frame} GPU comparisons / external proof', 24)
        text(draw, (16, 47), 'Columns: baseline, hard support, feather 16, feather 24. Thumbnails 0.4x; full captures retained.')
        for row, zoom in enumerate([0.8, 1.0, 2.0]):
            y = 85 + row * (sample_height + 49)
            for col, name in enumerate(VARIANTS):
                path = f'captures/{name}-{zoom:.1f}x-{frame}.png'
                require(sha(ROOT / path) == evidence['captures'][path], f'Capture drift: {path}')
                sources[path] = evidence['captures'][path]
                image = raster(ROOT / path)
                require(image.size == (1280, height), 'Unexpected viewport')
                x = 16 + col * 528
                board.paste(image.resize((512, sample_height), Image.Resampling.LANCZOS).convert('RGB'), (x, y))
                text(draw, (x, y + sample_height + 10), f'{name} / camera {zoom:.1f}x / {frame}')
        outputs[f'review/gpu-series-{frame}.png'] = png(board)
    crops = []
    for title, box in CROPS.items():
        width, height = (box[2] - box[0]) * 2, (box[3] - box[1]) * 2
        board = Image.new('RGB', ((width + 16) * 4 + 16, height + 94), '#eee9df')
        draw = ImageDraw.Draw(board)
        text(draw, (16, 12), f'{title} / actual GPU pixels at camera 2x / all variants', 20)
        for index, name in enumerate(VARIANTS):
            path = f'captures/{name}-2.0x-wide.png'
            transform = evidence['frames'][path]['world_to_canvas']
            a, b, c, d, tx, ty = transform
            require(a == d == 2 and b == c == 0, 'GPU crop requires exact 2x axes')
            x0, y0 = CONTEXT_ORIGIN[0] + box[0], CONTEXT_ORIGIN[1] + box[1]
            px, py = x0 * a + tx, y0 * d + ty
            require(px == round(px) and py == round(py), 'GPU crop fractional origin')
            image = raster(ROOT / path)
            bounds = (int(px), int(py), int(px) + width, int(py) + height)
            require(bounds[0] >= 0 and bounds[1] >= 0 and bounds[2] <= image.width
                    and bounds[3] <= image.height, 'GPU crop outside viewport')
            x = 16 + index * (width + 16)
            board.paste(image.crop(bounds).convert('RGB'), (x, 48))
            text(draw, (x, height + 60), name)
        outputs[f'review/gpu-{title}-2x.png'] = png(board)
        crops.append(board)
    board = Image.new('RGB', (max(im.width for im in crops) + 32,
                              sum(im.height for im in crops) + 100), '#eee9df')
    draw = ImageDraw.Draw(board)
    text(draw, (16, 12), 'Complete actual-scene edge comparisons / exact GPU 2x crops', 24)
    text(draw, (16, 48), 'Each row: baseline, hard support, feather 16, feather 24. Woodland and shell retained.')
    y = 84
    for im in crops:
        board.paste(im, (16, y))
        y += im.height
    outputs['review/complete-edge-series-2x.png'] = png(board)
    record = {'sources': sources, 'outputs': {name: hashlib.sha256(data).hexdigest() for name, data in outputs.items()}}
    outputs['review-board-manifest.json'] = (json.dumps(record, indent=2) + '\n').encode()
    return outputs


def main():
    """Write review boards or verify identical bytes without writing."""
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--check', action='store_true')
    args = parser.parse_args()
    require(args.check or not (ROOT / 'review-board-manifest.json').exists(),
            'Existing review boards are immutable; use a separate revision')
    for name, data in generate().items():
        path = ROOT / name
        if args.check:
            require(path.exists() and path.read_bytes() == data, f'Review output drift: {name}')
        else:
            path.write_bytes(data)
    print('PASS: all twenty-four GPU views and three edge groups arranged as a complete labelled series')


if __name__ == '__main__':
    main()
