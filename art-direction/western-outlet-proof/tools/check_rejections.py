#!/usr/bin/env python3
"""Exercise outlet mask, phase, edge and layer rejection boundaries entirely in memory."""

import proof


def expect_rejection(images, message, origin=proof.ORIGIN, size=proof.SIZE):
    """Require the independent verifier to reject a named mutation for its relevant reason."""
    try:
        proof.validate_images(images, origin, size)
    except ValueError as error:
        proof.require(message in str(error), f'unexpected rejection: {error}')
    else:
        raise ValueError(f'mutation accepted: {message}')


def mutate(images, layer, point, pixel):
    """Keep draw order internally consistent while changing one source-derived pixel."""
    result = {name: image.copy() for name, image in images.items()}
    result[layer].putpixel(point, pixel)
    result['joined'] = proof.merge([result[name] for name in proof.ORDER])
    return result


def main():
    """Reject capped ends, lost banks, mask escapes, UV shifts, hidden RGB and wrong compositing."""
    images = proof.exports(proof.render())
    proof.validate_images(images)
    cases = [
        ('water', (0, 96), (0, 0, 0, 0), 'water mask/UV'),
        ('water', (319, 96), (0, 0, 0, 0), 'water mask/UV'),
        ('water', (50, 47), (30, 100, 120, 255), 'water mask/UV'),
        ('water', (100, 96), (30, 100, 120, 255), 'water mask/UV'),
        ('north_bank', (0, 40), (0, 0, 0, 0), 'north_bank mask/UV'),
        ('south_bank', (319, 151), (0, 0, 0, 0), 'south_bank mask/UV'),
        ('north_bank', (100, 60), (100, 90, 60, 255), 'north_bank mask/UV'),
        ('south_bank', (100, 180), (100, 90, 60, 255), 'south_bank mask/UV'),
        ('water', (100, 0), (1, 0, 0, 0), 'hidden RGB'),
    ]
    for layer, point, pixel, message in cases:
        expect_rejection(mutate(images, layer, point, pixel), message)
    wrong = {name: image.copy() for name, image in images.items()}
    wrong['joined'] = proof.merge([images[name] for name in reversed(proof.ORDER)])
    expect_rejection(wrong, 'joined draw order')
    size = (proof.SIZE[0] + proof.OFF_MAP_WIDTH, proof.SIZE[1])
    origin = (-proof.OFF_MAP_WIDTH, proof.ORIGIN[1])
    wide = proof.exports(proof.render(origin, size), size)
    expect_rejection(mutate(wide, 'water', (0, 96), (0, 0, 0, 0)), 'water mask/UV', origin, size)
    print('PASS: eleven in-memory outlet rejection checks; no file writes')


if __name__ == '__main__':
    main()
