#!/usr/bin/env python3
"""Reject altered accepted views, channel masks, gate coverage and out-of-scope texels."""

import channel
import common as c
import forest


def rejected(label, check):
    """Require a validation failure without writing modified artifacts."""
    try:
        check()
    except ValueError:
        return
    raise ValueError(f'mutation accepted: {label}')


def run():
    """Exercise independent source-parity and protected-region checks in memory."""
    c.scope()
    _, reference = channel.render()
    ground = c.rgba(channel.GROUND)
    exposed_ground = lambda point: ground.getpixel((point[0] % ground.width, point[1] % ground.height))
    for name, point, value in (
        ('water gap', (340, 600), exposed_ground((340, 600))),
        ('bank gap', (340, 540), exposed_ground((340, 540))),
        ('UV restart', (340, 600), (1, 2, 3, 255)),
        ('B2 deck', (688, 600), (1, 2, 3, 255)),
        ('B1 deck', (1344, 600), (1, 2, 3, 255)),
        ('accepted join', (1600, 466), (1, 2, 3, 255)),
        ('retained crossing', (1800, 500), (1, 2, 3, 255)),
        ('spring inlet', (2000, 500), (1, 2, 3, 255)),
    ):
        mutated = reference.copy()
        mutated.putpixel(channel.position(point), value)
        rejected(name, lambda: channel.validate(mutated))
    raw = forest.raw_strip()
    reference = forest.bounded(raw)
    visible = next((x, y) for y in range(forest.SIZE[1]) for x in range(forest.SIZE[0])
                   if reference.getpixel((x, y))[3])
    for name, point, value in (
        ('Clearing interior', (200, 200), (1, 2, 3, 255)),
        ('physical gate band', (160, 120), (1, 2, 3, 255)),
        ('hidden RGB', (0, 0), (1, 2, 3, 0)),
        ('allowed source changed', visible, (1, 2, 3, 255)),
    ):
        mutated = reference.copy()
        mutated.putpixel(point, value)
        rejected(name, lambda: forest.validate(mutated, raw))
    rejected('raw strip offered as bounded', lambda: forest.validate(raw, raw))
    print('PASS: 13 in-memory rejection checks; no artifacts modified')


if __name__ == '__main__':
    run()
