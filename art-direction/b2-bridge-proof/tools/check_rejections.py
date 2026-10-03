#!/usr/bin/env python3
"""Exercise proof rejection boundaries using in-memory mutations only."""

import proof


def expect_rejection(images, config, crossing, routes, unit, message):
    """Require each mutation to fail the relevant geometric or composition check."""
    try:
        proof.measurements(config, crossing, routes, unit, images, 3938)
    except ValueError as error:
        proof.require(message in str(error), f'unexpected rejection: {error}')
    else:
        raise ValueError(f'mutation accepted: {message}')


def main():
    """Check uncovered roads, missing banks, bank intrusion and incorrect draw order."""
    config, crossing, routes, unit = proof.geometry()
    images = proof.exports(proof.layers(config, crossing, unit))
    proof.measurements(config, crossing, routes, unit, images, 3938)
    uncovered = {key: value.copy() for key, value in images.items()}
    # Remove a visible accepted road pixel that should be beneath the water/deck.
    selected = None
    for y in range(proof.SIZE[1]):
        for x in range(proof.SIZE[0]):
            wx, wy = proof.ORIGIN[0] + x + .5, proof.ORIGIN[1] + y + .5
            top, bottom = proof.water_edges(wx)
            if top < wy < bottom and images['road'].getpixel((x, y))[3]:
                selected = (x, y)
                break
        if selected:
            break
    proof.require(selected is not None, 'missing mutation target')
    uncovered['deck'].putpixel(selected, (0, 0, 0, 0))
    uncovered['joined'] = proof.merge([uncovered[key] for key in ('ground', 'road', 'water', 'bank', 'deck')])
    expect_rejection(uncovered, config, crossing, routes, unit, 'accepted road pixel exposed')

    gap = {key: value.copy() for key, value in images.items()}
    gap['bank'].putpixel((0, 552 - 5 - proof.ORIGIN[1]), (0, 0, 0, 0))
    gap['joined'] = proof.merge([gap[key] for key in ('ground', 'road', 'water', 'bank', 'deck')])
    expect_rejection(gap, config, crossing, routes, unit, 'gap in joined bank')

    intrusion = {key: value.copy() for key, value in images.items()}
    intrusion['bank'].putpixel((253, 164), (120, 90, 60, 255))
    intrusion['joined'] = proof.merge([intrusion[key] for key in ('ground', 'road', 'water', 'bank', 'deck')])
    expect_rejection(intrusion, config, crossing, routes, unit, 'bank intrudes into deck interior')

    order = {key: value.copy() for key, value in images.items()}
    order['joined'] = proof.merge([order[key] for key in ('ground', 'road', 'deck', 'water', 'bank')])
    expect_rejection(order, config, crossing, routes, unit, 'joined draw order')
    print('PASS: four in-memory rejection checks; no input or runtime writes')


if __name__ == '__main__':
    main()
