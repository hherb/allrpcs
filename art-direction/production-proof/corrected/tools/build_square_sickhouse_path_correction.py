#!/usr/bin/env python3
"""Build an external-only world-aligned continuation of the accepted junction leg."""

import math

from PIL import Image

from build import render_path_leg
from check_village_edge_square_connector_feasibility import ROOT, REVIEW

ORIGIN = (2320, 1140)
SIZE = (380, 360)
HEADING = 51.340192  # Retain the exact accepted junction sampling angle.


def compose():
    """Sample from the Square pivot, clip at the Sickhouse, and preserve source alpha."""
    with Image.open(ROOT / "game/art/runtime/terrain/straight-path.png") as source:
        path = source.convert("RGBA")
    length = math.hypot(440, 550)
    start = (2200 - ORIGIN[0], 900 - ORIGIN[1])
    result = render_path_leg(path, SIZE, start, HEADING, length)
    direction = (440 / length, 550 / length)
    for y in range(SIZE[1]):
        for x in range(SIZE[0]):
            along = ((ORIGIN[0] + x + 0.5 - 2200) * direction[0]
                     + (ORIGIN[1] + y + 0.5 - 900) * direction[1])
            if along > length or result.getpixel((x, y))[3] == 0:
                result.putpixel((x, y), (0, 0, 0, 0))
    return result


if __name__ == "__main__":
    compose().save(REVIEW / "square-sickhouse-path-correction-overlay.png")
