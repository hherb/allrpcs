#!/usr/bin/env python3
"""Blend only the external Shrine/Road–Wagon texture crossing; preserve both originals."""

import math

from PIL import Image

from build_shrine_southern_road_path_correction import ORIGIN as ROAD_ORIGIN, compose as compose_road
from check_village_edge_square_connector_feasibility import ROOT, REVIEW

ORIGIN = (1400, 1280)
SIZE = (160, 160)
END = (1500, 1400)
ROAD_DIRECTION = (320 / math.hypot(320, 180), 180 / math.hypot(320, 180))
SQUARE_ORIGIN = (1440, 1024)
LEADING_EXTENSION = 48
BLEND_HALF_LENGTH = 24
OPAQUE_HALF_LENGTH = 6


def smoothstep(value):
    """Give the local patch continuous color and opacity at either end."""
    value = min(1.0, max(0.0, value))
    return value * value * (3 - 2 * value)


def compose():
    """Bridge the leading edge with existing texture colors in a 48px band only."""
    incoming = compose_road()
    local_road = incoming.crop((ORIGIN[0] - ROAD_ORIGIN[0], ORIGIN[1] - ROAD_ORIGIN[1],
                                ORIGIN[0] - ROAD_ORIGIN[0] + SIZE[0],
                                ORIGIN[1] - ROAD_ORIGIN[1] + SIZE[1]))
    with Image.open(ROOT / "game/art/runtime/terrain/square-southern-road-path.png") as image:
        square = image.convert("RGBA").crop((ORIGIN[0] - SQUARE_ORIGIN[0],
                                             ORIGIN[1] - SQUARE_ORIGIN[1],
                                             ORIGIN[0] - SQUARE_ORIGIN[0] + SIZE[0],
                                             ORIGIN[1] - SQUARE_ORIGIN[1] + SIZE[1]))
    underlay = Image.alpha_composite(local_road, square)
    with Image.open(ROOT / "game/art/runtime/terrain/southern-road-wagon-path.png") as image:
        accepted = image.convert("RGBA")
    patch = Image.new("RGBA", SIZE)
    for y in range(SIZE[1]):
        for x in range(SIZE[0]):
            world_x, world_y = ORIGIN[0] + x + 0.5, ORIGIN[1] + y + 0.5
            along = ((world_x - END[0]) * ROAD_DIRECTION[0]
                     + (world_y - END[1]) * ROAD_DIRECTION[1] + LEADING_EXTENSION)
            if abs(along) >= BLEND_HALF_LENGTH:
                continue
            base = underlay.getpixel((x, y))
            # Extend only color from the first interior row across the finite edge.
            shift = max(0.0, OPAQUE_HALF_LENGTH - along)
            sample_x = round(x + shift * ROAD_DIRECTION[0])
            sample_y = round(y + shift * ROAD_DIRECTION[1])
            if not (0 <= sample_x < accepted.width and 0 <= sample_y < accepted.height):
                continue
            road = accepted.getpixel((sample_x, sample_y))
            if min(base[3], road[3]) <= 32:
                continue
            color_weight = smoothstep((along + BLEND_HALF_LENGTH) / (2 * BLEND_HALF_LENGTH))
            opacity = smoothstep((BLEND_HALF_LENGTH - abs(along))
                                 / (BLEND_HALF_LENGTH - OPAQUE_HALF_LENGTH))
            alpha = round(opacity * min(base[3], road[3]))
            if alpha:
                color = tuple(round(base[i] * (1 - color_weight) + road[i] * color_weight)
                              for i in range(3))
                patch.putpixel((x, y), color + (alpha,))
    return patch


if __name__ == "__main__":
    compose().save(REVIEW / "shrine-southern-road-path-blend-overlay.png")
