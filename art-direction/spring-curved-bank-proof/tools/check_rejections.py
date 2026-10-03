#!/usr/bin/env python3
"""Check spring-proof rejection boundaries with in-memory mutations only."""

import proof


def expect_rejection(images, message):
    """Require one deliberately damaged candidate to fail its relevant invariant."""
    try:
        proof.validate_images(images)
    except ValueError as error:
        proof.require(message in str(error), f'unexpected rejection: {error}')
    else:
        raise ValueError(f'mutation accepted: {message}')


def mutate(images, layer, point, pixel):
    """Change one in-memory layer and keep its composition internally consistent."""
    result = {key: value.copy() for key, value in images.items()}
    result[layer].putpixel(point, pixel)
    result['joined'] = proof.merge([result[name] for name in proof.ORDER])
    return result


def main():
    """Reject preservation, water continuity, bank closure, UV, mask and order regressions."""
    masters, _ = proof.layers()
    images = proof.exports(masters)
    proof.validate_images(images)
    expect_rejection(mutate(images, 'new_water', (50, 288), (255, 0, 0, 255)),
                     'retained Stream pixels')
    expect_rejection(mutate(images, 'new_water', (102, 288), (0, 0, 0, 0)),
                     'gap in connected inlet/basin water')
    expect_rejection(mutate(images, 'new_banks', (304, 90), (0, 0, 0, 0)),
                     'gap in closed basin bank')
    expect_rejection(mutate(images, 'new_banks', (292, 288), (100, 90, 60, 255)),
                     'bank intrudes into basin water')
    old = images['new_water'].getpixel((100, 272))
    expect_rejection(mutate(images, 'new_water', (100, 272), (old[0] ^ 1, *old[1:])),
                     'water connector restarts accepted UV phase')
    old = images['new_banks'].getpixel((100, 248))
    expect_rejection(mutate(images, 'new_banks', (100, 248), (old[0] ^ 1, *old[1:])),
                     'bank connector differs from retained texture continuation')
    expect_rejection(mutate(images, 'new_water', (550, 288), (50, 120, 140, 255)),
                     'water leaves proposed source envelope')
    order = {key: value.copy() for key, value in images.items()}
    order['joined'] = proof.merge([images[name] for name in
                                  ('ground', 'retained_banks', 'retained_water',
                                   'new_banks', 'new_water', 'spring_detail')])
    expect_rejection(order, 'joined draw order')
    old_profile = proof.profile
    try:
        proof.profile = lambda x: (16, 48) if x <= proof.JOIN_X + 1 else (192, 208)
        try:
            proof.check_shape()
        except ValueError as error:
            proof.require('too steep' in str(error), f'unexpected shape rejection: {error}')
        else:
            raise ValueError('steep western wall accepted')
    finally:
        proof.profile = old_profile
    proof.check_shape()
    print('PASS: nine in-memory rejection checks; no source/runtime writes')


if __name__ == '__main__':
    main()
