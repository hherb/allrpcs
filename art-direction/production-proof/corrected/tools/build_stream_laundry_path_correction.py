#!/usr/bin/env python3
"""Clip only Laundry floor/opening support in the external Stream–Laundry strip."""

from build_stream_laundry_path_feasibility import ORIGIN, SIZE, compose as compose_strip
from check_village_edge_square_connector_feasibility import REVIEW

LAUNDRY_RIGHT = 1580
OPENING_CENTER_Y = 294.6666666666667
OPENING_HALF_HEIGHT = 48
MARKER_HALF_DEPTH = 8


def compose():
    """Retain every surviving sampled pixel and the complete Stream underlay."""
    overlay = compose_strip()
    pixels = overlay.load()
    for y in range(SIZE[1]):
        for x in range(SIZE[0]):
            wx, wy = ORIGIN[0] + x + .5, ORIGIN[1] + y + .5
            if wx <= LAUNDRY_RIGHT or (wx <= LAUNDRY_RIGHT + MARKER_HALF_DEPTH
                                      and abs(wy - OPENING_CENTER_Y) > OPENING_HALF_HEIGHT):
                pixels[x, y] = (0, 0, 0, 0)
    return overlay


if __name__ == '__main__':
    compose().save(REVIEW / 'stream-laundry-path-correction-overlay.png')
