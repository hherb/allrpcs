#!/usr/bin/env python3
"""Render an external unchanged-source Village Edge–Stream route test."""

import math

from PIL import Image

from build import render_path_leg
from check_village_edge_square_connector_feasibility import ROOT, REVIEW

START = (1700, 900)
END = (1800, 500)
ORIGIN = (1640, 440)
SIZE = (224, 528)
Z_INDEX = -75


def compose():
    """Retain finite source sampling; canonicalize only invisible RGB."""
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
    compose().save(REVIEW / 'village-edge-stream-path-feasibility-overlay.png')
