#!/usr/bin/env python3
"""Clip only the Village shell/opening in the external Village Edge–Stream proof."""

from build_village_edge_stream_path_feasibility import ORIGIN, SIZE, compose as compose_strip
from check_village_edge_square_connector_feasibility import REVIEW

VILLAGE_TOP = 700
OPENING_CENTER_X = 1750
OPENING_HALF_WIDTH = 48
MARKER_HALF_DEPTH = 8


def compose():
    """Retain every surviving sampled pixel, including the existing bridge underlay."""
    overlay = compose_strip()
    pixels = overlay.load()
    for y in range(SIZE[1]):
        for x in range(SIZE[0]):
            wx, wy = ORIGIN[0] + x + .5, ORIGIN[1] + y + .5
            if wy >= VILLAGE_TOP or (wy >= VILLAGE_TOP - MARKER_HALF_DEPTH
                                    and abs(wx - OPENING_CENTER_X) > OPENING_HALF_WIDTH):
                pixels[x, y] = (0, 0, 0, 0)
    return overlay


if __name__ == '__main__':
    compose().save(REVIEW / 'village-edge-stream-path-correction-overlay.png')
