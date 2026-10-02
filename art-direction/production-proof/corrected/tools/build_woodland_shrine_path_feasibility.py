#!/usr/bin/env python3
"""Render only the approved external Woodland–Shrine unchanged-source strip test."""

import math

from PIL import Image

from build import render_path_leg
from check_village_edge_square_connector_feasibility import ROOT, REVIEW

START = (1000, 350)
END = (380, 300)
ORIGIN = (352, 224)
SIZE = (704, 208)


def compose():
    """Keep finite route sampling and visible pixels; canonicalize only zero-alpha RGB."""
    with Image.open(ROOT / 'game/art/runtime/terrain/straight-path.png') as image:
        source = image.convert('RGBA')
    dx, dy = END[0] - START[0], END[1] - START[1]
    overlay = render_path_leg(source, SIZE, (START[0] - ORIGIN[0], START[1] - ORIGIN[1]),
                              math.degrees(math.atan2(dy, dx)), math.hypot(dx, dy))
    pixels = overlay.load()
    for y in range(SIZE[1]):
        for x in range(SIZE[0]):
            if pixels[x, y][3] == 0:
                pixels[x, y] = (0, 0, 0, 0)
    return overlay


if __name__ == '__main__':
    compose().save(REVIEW / 'woodland-shrine-path-feasibility-overlay.png')
