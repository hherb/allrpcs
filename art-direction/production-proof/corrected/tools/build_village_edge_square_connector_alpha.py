#!/usr/bin/env python3
"""Build the separate eight-column, source-alpha west-handoff comparison."""
from PIL import Image
from build_village_edge_square_connector_correction import ROOT, compose as corrected

OUTPUT = ROOT / 'art-direction/production-proof/corrected/review/village-edge-square-connector-alpha-strip.png'


def compose() -> Image.Image:
    strip = corrected()  # Checks approved sources; retains the exact east blend.
    with Image.open(ROOT / 'game/art/runtime/terrain/straight-path.png') as image:
        source = image.convert('RGBA').crop((0, 0, 96, 124)).transpose(Image.Transpose.ROTATE_270)
    for y in range(96):
        for x in range(32):
            pixel = source.getpixel((x, y))
            if x < 8:
                weight = 7 - x
                rgb = tuple((a * (7 - weight) + b * weight + 3) // 7
                            for a, b in zip(pixel[:3], (114, 100, 77)))
                pixel = (*rgb, pixel[3])
            strip.putpixel((x, y), pixel)
    return strip


if __name__ == '__main__':
    compose().save(OUTPUT)
    print(OUTPUT)
