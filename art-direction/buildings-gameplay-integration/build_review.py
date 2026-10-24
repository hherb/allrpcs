#!/usr/bin/env python3
"""Assemble labelled review boards from unmodified ordinary-scene captures."""
from pathlib import Path
from PIL import Image, ImageDraw, ImageFont

ROOT = Path(__file__).resolve().parent
FONT = ImageFont.load_default(size=20)
SMALL = ImageFont.load_default(size=16)


def board(name, title, entries, columns):
    """Keep labels outside the captured game frames and retain complete frames."""
    width, height, gap, heading = 640, 360, 20, 64
    rows = (len(entries) + columns - 1) // columns
    output = Image.new('RGB', (gap + columns * (width + gap), heading + rows * (height + 42)), '#e9e3d5')
    draw = ImageDraw.Draw(output)
    draw.text((gap, 16), title, font=FONT, fill='#26322a')
    for index, (capture, label) in enumerate(entries):
        x = gap + (index % columns) * (width + gap)
        y = heading + (index // columns) * (height + 42)
        image = Image.open(ROOT / 'captures' / (capture + '.png')).convert('RGB')
        image = image.resize((width, height), Image.Resampling.LANCZOS)
        output.paste(image, (x, y))
        draw.text((x, y + height + 7), label, font=SMALL, fill='#26322a')
    output.save(ROOT / name)


if __name__ == '__main__':
    board('building-review.png', 'Map structures : complete exterior series (candidate placements)', [
        ('sickhouse-exterior', 'Sickhouse : closed south wall; existing entrance remains north'),
        ('laundry-exterior', 'Laundry shelter : trough and drying rack'),
        ('shrine-exterior', 'Old shrine : approved open shelter'),
        ('wagon-exterior', 'Abandoned wagon : approved broken open wagon'),
        ('village-exterior', 'Village meeting house : backdrop north of the existing square'),
        ('map-overview', 'Whole-map context : existing anchors and paths'),
    ], 2)
    board('cutaway-review.png', 'Map structures : interior and entrance visibility', [
        ('sickhouse-cutaway', 'Sickhouse : north entrance and sleeping patient remain accessible'),
        ('laundry-cutaway', 'Laundry : both existing side openings remain clear'),
        ('shrine-cutaway', 'Shrine : concealed inscription remains concealed'),
    ], 1)
