#!/usr/bin/env python3
"""Extend the external strip 24px beneath the unchanged Clearing end cap."""

import math

from PIL import Image

from build import render_path_leg
from build_woodland_shrine_path_correction import compose as compose_clipped
from build_woodland_shrine_path_feasibility import END, ORIGIN, SIZE, START
from check_village_edge_square_connector_feasibility import ROOT, REVIEW

EXTENSION = 24
BOUNDS = (994, 300, 1028, 402)


def compose():
    """Preserve the shrine clip and every pixel outside the local junction rectangle."""
    dx, dy = END[0] - START[0], END[1] - START[1]
    length = math.hypot(dx, dy)
    start = (START[0] - dx / length * EXTENSION, START[1] - dy / length * EXTENSION)
    with Image.open(ROOT / 'game/art/runtime/terrain/straight-path.png') as image:
        extension = render_path_leg(image.convert('RGBA'), SIZE,
                                    (start[0] - ORIGIN[0], start[1] - ORIGIN[1]),
                                    math.degrees(math.atan2(dy, dx)), EXTENSION)
    original = compose_clipped()
    joined = Image.alpha_composite(extension, original)
    result = original.copy()
    box = (BOUNDS[0] - ORIGIN[0], BOUNDS[1] - ORIGIN[1],
           BOUNDS[2] - ORIGIN[0], BOUNDS[3] - ORIGIN[1])
    result.paste(joined.crop(box), box[:2])
    for y in range(SIZE[1]):
        for x in range(SIZE[0]):
            if result.getpixel((x, y))[3] == 0:
                result.putpixel((x, y), (0, 0, 0, 0))
    return result


if __name__ == '__main__':
    compose().save(REVIEW / 'woodland-shrine-path-junction-overlay.png')
