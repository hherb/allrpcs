#!/usr/bin/env python3
"""Build an external review strip; never write production textures."""
import hashlib
from pathlib import Path
from PIL import Image

ROOT = Path(__file__).resolve().parents[4]
OUTPUT = ROOT / 'art-direction/production-proof/corrected/review/village-edge-square-connector-corrected-strip.png'


def compose() -> Image.Image:
    terrain = ROOT / 'game/art/runtime/terrain'
    expected = {
        'straight-path.png': ((96, 256), '3d12ffdfb32fbbda2e9d0c2029803691a4eb4f8484a4ee5245a729f2fc8278bd'),
        'village-square-junction.png': ((512, 512), '0ba0b2cfa16466936710bb801409ac4c790058449e7d609b0df2cc96d772e2e3'),
    }
    for name, (size, digest) in expected.items():
        path = terrain / name
        if hashlib.sha256(path.read_bytes()).hexdigest() != digest:
            raise ValueError(f'Approved source changed: {name}')
        with Image.open(path) as source:
            if source.size != size:
                raise ValueError(f'Approved source dimensions changed: {name}')
    with Image.open(terrain / 'straight-path.png') as source:
        strip = source.convert('RGBA').crop((0, 0, 96, 124)).transpose(Image.Transpose.ROTATE_270)
    with Image.open(terrain / 'village-square-junction.png') as source:
        junction = source.convert('RGBA')
    # Godot's opaque greybox colour, rounded to 8-bit channels.
    west = (114, 100, 77, 255)
    for y in range(96):
        east = junction.getpixel((0, y + 208))
        for x in range(124):
            if x < 32:
                target, weight = west, 31 - x
            elif x >= 92:
                target, weight = east, x - 92
            else:
                continue
            original = strip.getpixel((x, y))
            strip.putpixel((x, y), tuple((a * (31 - weight) + b * weight + 15) // 31
                                        for a, b in zip(original, target)))
    return strip


if __name__ == '__main__':
    compose().save(OUTPUT)
    print(OUTPUT)
