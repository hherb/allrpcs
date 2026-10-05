#!/usr/bin/env python3
"""Arrange all eighteen disposable GPU views and all native detail comparisons."""

import argparse
import hashlib
import json
from pathlib import Path

from PIL import Image, ImageDraw

from build import ROOT, VARIANTS, CROPS, png, raster, require, sha, text


def generate():
    """Make complete labelled series boards; retain full-size captures separately."""
    evidence = json.loads((ROOT / 'capture-evidence.json').read_text())
    require(len(evidence['captures']) == 18, 'Incomplete GPU series')
    sources = {'capture-evidence.json': sha(ROOT / 'capture-evidence.json')}
    outputs = {}
    for frame, height in [('wide', 720), ('tall', 827)]:
        sample_height = round(height / 2)
        board = Image.new('RGB', (1984, 85 + 3 * (sample_height + 49)), '#eee9df')
        draw = ImageDraw.Draw(board)
        text(draw, (16, 12), f'Village Square / all {frame} GPU comparisons / external proof', 24)
        text(draw, (16, 47), 'Columns: baseline, hard-support diagnostic, feathered candidate. Thumbnails 0.5x; full captures retained.')
        for row, zoom in enumerate([0.8, 1.0, 2.0]):
            y = 85 + row * (sample_height + 49)
            for col, name in enumerate(VARIANTS):
                path = f'captures/{name}-{zoom:.1f}x-{frame}.png'
                require(sha(ROOT / path) == evidence['captures'][path], f'Capture drift: {path}')
                sources[path] = evidence['captures'][path]
                image = raster(ROOT / path)
                require(image.size == (1280, height), 'Unexpected viewport')
                x = 16 + col * 656
                board.paste(image.resize((640, sample_height), Image.Resampling.LANCZOS).convert('RGB'), (x, y))
                text(draw, (x, y + sample_height + 10), f'{name} / camera {zoom:.1f}x / {frame}')
        outputs[f'review/gpu-series-{frame}.png'] = png(board)
    crops = [raster(ROOT / f'review/{name}-2x.png') for name in CROPS]
    board = Image.new('RGB', (max(image.width for image in crops) + 32,
                              sum(image.height for image in crops) + 100), '#eee9df')
    draw = ImageDraw.Draw(board)
    text(draw, (16, 12), 'Complete native source edge/port comparisons / exact 2x crops', 24)
    text(draw, (16, 48), 'Each row: baseline, hard-support diagnostic, feathered candidate. CPU route-only reconstruction.')
    y = 84
    for name, image in zip(CROPS, crops):
        sources[f'review/{name}-2x.png'] = sha(ROOT / f'review/{name}-2x.png')
        board.paste(image.convert('RGB'), (16, y))
        y += image.height
    outputs['review/complete-edge-series-2x.png'] = png(board)
    record = {'sources': sources, 'outputs': {name: hashlib.sha256(data).hexdigest() for name, data in outputs.items()}}
    outputs['review-board-manifest.json'] = (json.dumps(record, indent=2) + '\n').encode()
    return outputs


def main():
    """Write review boards or verify identical bytes without writing."""
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--check', action='store_true')
    args = parser.parse_args()
    require(args.check or not (ROOT / 'owner-acceptance.json').exists(), 'Accepted review cannot be overwritten')
    for name, data in generate().items():
        path = ROOT / name
        if args.check:
            require(path.exists() and path.read_bytes() == data, f'Review output drift: {name}')
        else:
            path.write_bytes(data)
    print('PASS: all eighteen GPU views and four edge groups arranged as a complete labelled series')


if __name__ == '__main__':
    main()
