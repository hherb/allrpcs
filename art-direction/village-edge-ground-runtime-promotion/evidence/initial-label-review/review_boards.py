#!/usr/bin/env python3
"""Arrange all nine actual runtime views without altering captured evidence."""
import hashlib
from io import BytesIO
import json
from pathlib import Path
from PIL import Image, ImageDraw, ImageFont

ROOT = Path(__file__).resolve().parent
FONT = Path('/System/Library/Fonts/Supplemental/Arial.ttf')


def require(value, message):
    """Fail closed on capture drift."""
    if not value:
        raise ValueError(message)


def sha(path):
    """Hash persisted bytes."""
    return hashlib.sha256(path.read_bytes()).hexdigest()


def label(draw, xy, text, size=18):
    """Keep labels outside capture pixels."""
    draw.text(xy, text, font=ImageFont.truetype(str(FONT), size), fill='#25312e')


def main():
    """Freeze the complete nine-view boards and explicit native detail crops."""
    require(not (ROOT / 'series-bindings.json').exists(), 'Review boards are immutable')
    evidence = json.loads((ROOT / 'capture-evidence.json').read_text())
    require(len(evidence['captures']) == 18, 'Incomplete paired capture matrix')
    sources = {'capture-evidence.json': sha(ROOT / 'capture-evidence.json'),
               'review_boards.py': sha(Path(__file__)), str(FONT): sha(FONT)}
    for name, digest in evidence['captures'].items():
        require(sha(ROOT / name) == digest, 'Capture drift')
        sources[name] = digest
    boards = {}
    board = Image.new('RGB', (1328, 1420), '#eee9df');d = ImageDraw.Draw(board)
    label(d, (16, 12), 'Village Edge feather-16 / actual runtime / all six overview views', 23)
    label(d, (16, 48), 'Actual imported texture; wide/tall at 0.8x, 1x, 2x. Thumbnails 0.5x; captures retained.')
    for row, zoom in enumerate((0.8, 1.0, 2.0)):
        for col, frame in enumerate(('wide', 'tall')):
            image = Image.open(ROOT / f'captures/runtime-{zoom:.1f}x-{frame}.png').convert('RGB')
            image = image.resize((640, round(image.height / 2)), Image.Resampling.LANCZOS)
            x, y = 16 + col * 656, 86 + row * 436;board.paste(image, (x, y))
            label(d, (x, y + image.height + 7), f'actual runtime / {zoom:.1f}x / {frame}')
    boards['runtime-series.png'] = board
    board = Image.new('RGB', (1984, 494), '#eee9df');d = ImageDraw.Draw(board)
    label(d, (16, 12), 'All three dedicated actual runtime views / camera 2x / thumbnails 0.5x', 24)
    for index, frame in enumerate(('woodland', 'stream', 'square')):
        image = Image.open(ROOT / f'captures/runtime-2.0x-{frame}.png').convert('RGB')
        x = 16 + index * 656;board.paste(image.resize((640, 360), Image.Resampling.LANCZOS), (x, 76))
        label(d, (x, 448), frame)
    boards['runtime-details.png'] = board
    boxes = {'woodland': (1490, 690, 1630, 905), 'stream': (1660, 616, 1850, 750),
             'square': (1780, 808, 1920, 985)}
    board = Image.new('RGB', (1004, 550), '#eee9df');d = ImageDraw.Draw(board)
    label(d, (16, 12), 'Actual runtime entrance edges / exact GPU 2x pixels / no resampling', 22)
    x = 16
    for frame, box in boxes.items():
        name = f'captures/runtime-2.0x-{frame}.png'
        a, b, c, e, tx, ty = evidence['frames'][name]['world_to_canvas']
        require(a == e == 2 and b == c == 0, 'Unexpected detail axes')
        bounds = (box[0]*a+tx, box[1]*e+ty, box[2]*a+tx, box[3]*e+ty)
        require(all(value == round(value) for value in bounds), 'Fractional crop')
        image = Image.open(ROOT / name).convert('RGB')
        require(0 <= bounds[0] < bounds[2] <= image.width and 0 <= bounds[1] < bounds[3] <= image.height,
                'Crop outside actual capture')
        crop = image.crop(tuple(map(int, bounds)));board.paste(crop, (x, 76))
        label(d, (x, 88 + crop.height), frame);x += crop.width + 16
    boards['runtime-edges-2x.png'] = board
    outputs = {}
    for name, board in boards.items():
        stream = BytesIO();board.save(stream, format='PNG');(ROOT / name).write_bytes(stream.getvalue())
        outputs[name] = sha(ROOT / name)
    (ROOT / 'series-bindings.json').write_text(json.dumps({'sources': sources, 'outputs': outputs}, indent=2)+'\n')
    print('PASS: all nine actual runtime views and exact entrance edge series arranged')


if __name__ == '__main__':
    main()
