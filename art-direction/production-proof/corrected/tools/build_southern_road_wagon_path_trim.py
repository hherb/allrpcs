#!/usr/bin/env python3
"""Shorten the external Square junction tip without changing the reviewed road legs."""

import math

from PIL import Image

from build import render_path_leg
from build_southern_road_wagon_path_correction import (
    ORIGIN, SIZE, START, BEND, END, JOIN_EXTENSION, road_leg, merge,
)
from check_village_edge_square_connector_feasibility import ROOT, REVIEW

INCOMING_EXTENSION = 16


def compose():
    """Retain road sampling and reduce only the incoming extension from 48 to 16 pixels."""
    with Image.open(ROOT / "game/art/runtime/terrain/straight-path.png") as image:
        source = image.convert("RGBA")
    road = merge(road_leg(source, START, BEND, JOIN_EXTENSION),
                 road_leg(source, BEND, END, JOIN_EXTENSION))
    length = math.hypot(-700, 500)
    incoming = render_path_leg(source, SIZE, (2200 - ORIGIN[0], 900 - ORIGIN[1]),
                               144.462322, length + INCOMING_EXTENSION)
    for y in range(SIZE[1]):
        for x in range(SIZE[0]):
            dx, dy = ORIGIN[0] + x + 0.5 - 2200, ORIGIN[1] + y + 0.5 - 900
            if (dx * -700 + dy * 500) / length <= length:
                incoming.putpixel((x, y), (0, 0, 0, 0))
    return merge(incoming, road)


if __name__ == "__main__":
    compose().save(REVIEW / "southern-road-wagon-path-trim-overlay.png")
