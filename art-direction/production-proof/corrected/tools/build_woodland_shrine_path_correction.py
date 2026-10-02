#!/usr/bin/env python3
"""Clip the external Woodland–Shrine strip at the unchanged shrine shell/opening only."""

from build_woodland_shrine_path_feasibility import ORIGIN, SIZE, compose as compose_original
from check_village_edge_square_connector_feasibility import REVIEW

SHRINE_RIGHT = 620
OPENING_CENTER_Y = 300
OPENING_HALF_HEIGHT = 48
MARKER_HALF_DEPTH = 8


def compose():
    """Keep source phase/visible pixels outside the shrine; clip the marker band to its opening."""
    overlay = compose_original()
    pixels = overlay.load()
    for y in range(SIZE[1]):
        for x in range(SIZE[0]):
            world_x, world_y = ORIGIN[0] + x + 0.5, ORIGIN[1] + y + 0.5
            outside_opening = (world_x < SHRINE_RIGHT + MARKER_HALF_DEPTH
                               and abs(world_y - OPENING_CENTER_Y) > OPENING_HALF_HEIGHT)
            if world_x < SHRINE_RIGHT or outside_opening:
                pixels[x, y] = (0, 0, 0, 0)
    return overlay


if __name__ == '__main__':
    compose().save(REVIEW / 'woodland-shrine-path-correction-overlay.png')
