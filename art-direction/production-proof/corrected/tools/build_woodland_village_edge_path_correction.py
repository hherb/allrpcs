#!/usr/bin/env python3
"""Clip only the external Village Edge shell and its unchanged opening band."""

from build_woodland_village_edge_path_feasibility import ORIGIN, SIZE, compose as compose_original
from check_village_edge_square_connector_feasibility import REVIEW

EDGE_LEFT = 1580
OPENING_CENTER_Y = 805.7142857142857
OPENING_HALF_HEIGHT = 48
MARKER_HALF_DEPTH = 8


def compose():
    """Preserve every surviving sampled pixel and the Clearing join exactly."""
    overlay = compose_original()
    pixels = overlay.load()
    for y in range(SIZE[1]):
        for x in range(SIZE[0]):
            wx, wy = ORIGIN[0] + x + .5, ORIGIN[1] + y + .5
            if wx >= EDGE_LEFT or (wx >= EDGE_LEFT - MARKER_HALF_DEPTH
                                  and abs(wy - OPENING_CENTER_Y) > OPENING_HALF_HEIGHT):
                pixels[x, y] = (0, 0, 0, 0)
    return overlay


if __name__ == '__main__':
    compose().save(REVIEW / 'woodland-village-edge-path-correction-overlay.png')
