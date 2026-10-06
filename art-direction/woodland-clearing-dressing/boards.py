#!/usr/bin/env python3
"""Show the whole actual-scene series and every placement at exact native/2x scale."""

import argparse
import hashlib
import json

from PIL import Image, ImageDraw

from build import FONT, ORIGIN, ROOT, SIZE, VARIANTS, label, png, raster, read, require, sha, write_or_check

CLUSTERS = {'north-belt': (832, 64, 1250, 274), 'southwest-details': (600, 376, 850, 452)}


def expected_frames():
    return {f'captures/{variant}-{zoom:.1f}x-{frame}.png'
            for variant in VARIANTS for zoom in (0.8, 1, 2) for frame in ('wide', 'tall')} \
        | {f'captures/{variant}-{zoom:.1f}x-placement.png' for variant in VARIANTS for zoom in (1, 2)}


def crop(image, transform, bounds, scale):
    """Extract world bounds through the recorded camera transform without resampling."""
    a, b, c, d, tx, ty = transform
    require(a == d == scale and b == c == 0, 'Non-native crop axes')
    left, top, right, bottom = bounds
    mapped = (left * a + tx, top * d + ty, right * a + tx, bottom * d + ty)
    require(all(value == round(value) for value in mapped), 'Fractional native crop bounds')
    require(0 <= mapped[0] < mapped[2] <= image.width and 0 <= mapped[1] < mapped[3] <= image.height,
            'Placement crop outside complete viewport')
    return image.crop(tuple(map(int, mapped)))


def detail_board(images, scale, bounds, title):
    width = (bounds[2] - bounds[0]) * scale
    height = (bounds[3] - bounds[1]) * scale
    board = Image.new('RGB', (16 + (width + 16) * 3, height + 108), '#eee9df')
    draw = ImageDraw.Draw(board)
    label(draw, (16, 12), f'{title} / exact camera {scale}x GPU pixels', 23)
    label(draw, (16, 47), 'Columns: retained baseline, sparse (8 placements), grouped (16 placements). No resampling.')
    for index, variant in enumerate(VARIANTS):
        detail = images[variant]
        require(detail.size == (width, height), 'Detail dimensions changed')
        x = 16 + index * (width + 16)
        board.paste(detail.convert('RGB'), (x, 76))
        label(draw, (x, height + 82), variant)
    return board


def generate():
    evidence = read(ROOT / 'capture-evidence.json')
    require(set(evidence['captures']) == set(evidence['frames']) == expected_frames(), 'Incomplete capture matrix')
    sources = {'boards.py': sha(ROOT / 'boards.py'), 'build.py': sha(ROOT / 'build.py'),
               'capture-evidence.json': sha(ROOT / 'capture-evidence.json'), str(FONT): sha(FONT)}
    images = {}
    for name in sorted(expected_frames()):
        require(sha(ROOT / name) == evidence['captures'][name], 'Capture drift: ' + name)
        sources[name] = sha(ROOT / name)
        images[name] = raster(ROOT / name)
        require(list(images[name].size) == evidence['frames'][name]['viewport'], 'Viewport drift')
    outputs = {}
    for frame, height in (('wide', 720), ('tall', 827)):
        sample_height = round(height * .4)
        board = Image.new('RGB', (1600, 85 + 3 * (sample_height + 58)), '#eee9df')
        draw = ImageDraw.Draw(board)
        label(draw, (16, 12), f'Woodland Clearing / complete {frame} dressing comparison / external only', 24)
        label(draw, (16, 47), 'Columns: retained baseline, sparse, grouped. Overview thumbnails 0.4x; native details separate.')
        for row, zoom in enumerate((0.8, 1, 2)):
            for col, variant in enumerate(VARIANTS):
                name = f'captures/{variant}-{zoom:.1f}x-{frame}.png'
                x, y = 16 + col * 528, 85 + row * (sample_height + 58)
                board.paste(images[name].resize((512, sample_height), Image.Resampling.LANCZOS).convert('RGB'), (x, y))
                label(draw, (x, y + sample_height + 8), f'{variant} / camera {zoom:.1f}x / {frame}')
                label(draw, (x, y + sample_height + 29),
                      f'actual world center {evidence["frames"][name]["screen_center"]}', 14)
        outputs[f'review/gpu-series-{frame}.png'] = png(board)
    for scale in (1, 2):
        bounds = (ORIGIN[0], 0, ORIGIN[0] + SIZE[0], SIZE[1])
        details = {}
        for variant in VARIANTS:
            name = f'captures/{variant}-{scale:.1f}x-placement.png'
            details[variant] = crop(images[name], evidence['frames'][name]['world_to_canvas'], bounds, scale)
            outputs[f'review/{variant}-detail-{scale}x.png'] = png(details[variant])
        outputs[f'review/complete-details-{scale}x.png'] = png(
            detail_board(details, scale, bounds, 'Entire placement envelope'))
        for cluster, bounds in CLUSTERS.items():
            details = {}
            for variant in VARIANTS:
                name = f'captures/{variant}-{scale:.1f}x-placement.png'
                details[variant] = crop(images[name], evidence['frames'][name]['world_to_canvas'], bounds, scale)
                outputs[f'review/{variant}-{cluster}-{scale}x.png'] = png(details[variant])
            outputs[f'review/complete-{cluster}-{scale}x.png'] = png(
                detail_board(details, scale, bounds, cluster.replace('-', ' ').title()))
    record = {'sources': sources,
              'outputs': {name: hashlib.sha256(data).hexdigest() for name, data in outputs.items()}}
    outputs['review-board-manifest.json'] = (json.dumps(record, indent=2) + '\n').encode()
    return outputs


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--check', action='store_true')
    args = parser.parse_args()
    write_or_check(generate(), args.check)
    print('PASS: complete 24-view series; complete native/2x envelope and both placement clusters')


if __name__ == '__main__':
    main()
