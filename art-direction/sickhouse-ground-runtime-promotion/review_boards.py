#!/usr/bin/env python3
"""Arrange the six actual runtime views and six paired baselines without altering captures."""

import argparse
import hashlib
from io import BytesIO
import json
from pathlib import Path

from PIL import Image, ImageDraw, ImageFont

ROOT = Path(__file__).resolve().parent
FONT = Path('/System/Library/Fonts/Supplemental/Arial.ttf')


def sha(path):
    """Bind exact pixels and labels."""
    return hashlib.sha256(path.read_bytes()).hexdigest()


def require(condition, message):
    """Reject incomplete or drifted capture evidence."""
    if not condition:
        raise ValueError(message)


def render():
    """Keep every native capture; use labelled half-scale thumbnails for collective review."""
    evidence = json.loads((ROOT / 'capture-evidence.json').read_text())
    expected = {f'captures/{variant}-{zoom:.1f}x-{frame}.png' for variant in ('baseline', 'runtime')
                for zoom in (0.8, 1.0, 2.0) for frame in ('wide', 'tall')}
    require(set(evidence['captures']) == expected, 'Incomplete runtime capture series')
    outputs = {}
    for variant in ('baseline', 'runtime'):
        board = Image.new('RGB', (1328, 1505), '#eee9df')
        draw = ImageDraw.Draw(board)
        draw.text((16, 12), f'Sickhouse entrance / all six actual {variant} views', fill='#25312e',
                  font=ImageFont.truetype(str(FONT), 25))
        draw.text((16, 48), 'Actual imported runtime texture. Columns: wide / tall. Thumbnails 0.5x; native captures retained.',
                  fill='#25312e', font=ImageFont.truetype(str(FONT), 17))
        for row, zoom in enumerate((0.8, 1.0, 2.0)):
            y = 84 + row * 470
            for column, frame in enumerate(('wide', 'tall')):
                name = f'captures/{variant}-{zoom:.1f}x-{frame}.png'
                require(sha(ROOT / name) == evidence['captures'][name], 'Capture byte drift: ' + name)
                with Image.open(ROOT / name) as image:
                    height = 720 if frame == 'wide' else 827
                    require(image.size == (1280, height), 'Capture geometry drift')
                    thumbnail = image.convert('RGB').resize((640, round(height / 2)), Image.Resampling.LANCZOS)
                x = 16 + column * 656
                board.paste(thumbnail, (x, y))
                draw.text((x, y + 425), f'{variant} / {zoom:.1f}x / {frame}', fill='#25312e',
                          font=ImageFont.truetype(str(FONT), 18))
        stream = BytesIO()
        board.save(stream, format='PNG')
        outputs[f'{variant}-series.png'] = stream.getvalue()
    record = {'capture_evidence_sha256': sha(ROOT / 'capture-evidence.json'), 'font_sha256': sha(FONT),
              'outputs': {name: hashlib.sha256(data).hexdigest() for name, data in outputs.items()}}
    outputs['series-bindings.json'] = (json.dumps(record, indent=2) + '\n').encode()
    return outputs


def main():
    """Write pending review boards or check identical bytes without writing."""
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--check', action='store_true')
    args = parser.parse_args()
    require(args.check or not (ROOT / 'owner-runtime-acceptance.json').exists(), 'Accepted boards are immutable')
    for name, data in render().items():
        path = ROOT / name
        if args.check:
            require(path.is_file() and path.read_bytes() == data, 'Review board drift: ' + name)
        else:
            path.write_bytes(data)
    print('PASS: complete actual-runtime and paired-baseline review series')


if __name__ == '__main__':
    main()
