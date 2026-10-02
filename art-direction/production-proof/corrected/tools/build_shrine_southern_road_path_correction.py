#!/usr/bin/env python3
"""Compose an external road texture on the retained shrine-to-road centerline."""

import math

from PIL import Image

from build import render_path_leg
from check_village_edge_square_connector_feasibility import ROOT, REVIEW

ORIGIN = (432, 400)
SIZE = (1152, 1080)
START = (380, 300)
END = (1500, 1400)
SHRINE_BOTTOM = 460
OPENING_CENTER_X = START[0] + (SHRINE_BOTTOM - START[1]) * (END[0] - START[0]) / (END[1] - START[1])
OPENING_HALF_WIDTH = 48
OPENING_MARKER_HALF_DEPTH = 8


def compose():
    """Keep the route sampling phase, clipping its entry at the unchanged shrine opening."""
    with Image.open(ROOT / "game/art/runtime/terrain/straight-path.png") as image:
        source = image.convert("RGBA")
    dx, dy = END[0] - START[0], END[1] - START[1]
    overlay = render_path_leg(source, SIZE, (START[0] - ORIGIN[0], START[1] - ORIGIN[1]),
                              math.degrees(math.atan2(dy, dx)), math.hypot(dx, dy))
    pixels = overlay.load()
    for y in range(SIZE[1]):
        for x in range(SIZE[0]):
            world_x, world_y = ORIGIN[0] + x + 0.5, ORIGIN[1] + y + 0.5
            # Leave the shell and its 96px gold opening entirely under their existing owners.
            outside_entry = (world_y < SHRINE_BOTTOM + OPENING_MARKER_HALF_DEPTH
                             and abs(world_x - OPENING_CENTER_X) > OPENING_HALF_WIDTH)
            if world_y < SHRINE_BOTTOM or outside_entry or pixels[x, y][3] == 0:
                pixels[x, y] = (0, 0, 0, 0)
    return overlay


if __name__ == "__main__":
    compose().save(REVIEW / "shrine-southern-road-path-correction-overlay.png")
