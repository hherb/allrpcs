#!/usr/bin/env python3
"""Compose a disposable bent-road overlay from the existing straight-path source."""

import math

from PIL import Image

from build import render_path_leg
from check_village_edge_square_connector_feasibility import ROOT, REVIEW

ORIGIN = (1400, 1280)
SIZE = (560, 432)
START = (1500, 1400)
BEND = (1820, 1580)
END = (1900, 1640)
JOIN_EXTENSION = 48


def road_leg(source, start, end, extend_start):
    """Overlap adjoining legs by half a width without changing the displayed centerline."""
    dx, dy = end[0] - start[0], end[1] - start[1]
    length = math.hypot(dx, dy)
    pivot = (start[0] - ORIGIN[0] - extend_start * dx / length,
             start[1] - ORIGIN[1] - extend_start * dy / length)
    return render_path_leg(source, SIZE, pivot, math.degrees(math.atan2(dy, dx)),
                           length + extend_start)


def merge(first, second):
    """Blend texture where legs overlap, retaining maximum rather than doubled alpha."""
    result = Image.new("RGBA", SIZE)
    for y in range(SIZE[1]):
        for x in range(SIZE[0]):
            a, b = first.getpixel((x, y)), second.getpixel((x, y))
            weight = a[3] + b[3]
            if weight:
                result.putpixel((x, y), tuple(round((a[i] * a[3] + b[i] * b[3]) / weight)
                                             for i in range(3)) + (max(a[3], b[3]),))
    return result


def compose():
    """Follow the retained road bend and overlap the accepted Square endpoint locally."""
    with Image.open(ROOT / "game/art/runtime/terrain/straight-path.png") as image:
        source = image.convert("RGBA")
    road = merge(road_leg(source, START, BEND, JOIN_EXTENSION),
                 road_leg(source, BEND, END, JOIN_EXTENSION))
    # Continue the exact Square sampling phase for 48 pixels beyond its flat end.
    length = math.hypot(-700, 500)
    incoming = render_path_leg(source, SIZE, (2200 - ORIGIN[0], 900 - ORIGIN[1]),
                               144.462322, length + JOIN_EXTENSION)
    # The accepted runtime sprite already owns the incoming side of the endpoint.
    # Do not draw it twice or clip a second copy at this external canvas boundary.
    for y in range(SIZE[1]):
        for x in range(SIZE[0]):
            dx, dy = ORIGIN[0] + x + 0.5 - 2200, ORIGIN[1] + y + 0.5 - 900
            along = (dx * -700 + dy * 500) / length
            if along <= length:
                incoming.putpixel((x, y), (0, 0, 0, 0))
    return merge(incoming, road)


if __name__ == "__main__":
    compose().save(REVIEW / "southern-road-wagon-path-correction-overlay.png")
