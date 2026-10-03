#!/usr/bin/env python3
"""Assemble the complete external channel from frozen accepted local compositions."""

import argparse
import math

from PIL import Image, ImageDraw, ImageFont

import common as c

ORIGIN = (-64, 212)
SIZE = (2604, 620)
GROUND = 'art-direction/production-proof/corrected/exports/terrain/quiet-moss.png'
WATER = 'game/art/runtime/terrain/stream-water.png'
BANK = 'game/art/runtime/terrain/stream-bank.png'
CAP = 'game/art/runtime/terrain/stream-bank-opening-cap.png'
PATCHES = (
    ('outlet', 'art-direction/western-outlet-proof/exports/joined.png', (0, 504)),
    ('B2', 'art-direction/b2-bridge-proof/exports/joined.png', (432, 384)),
    ('B1', 'art-direction/b1-bridge-proof/exports/joined.png', (1088, 384)),
    ('curved_spring', 'art-direction/spring-curved-bank-proof/exports/joined.png', (1900, 212)),
    ('accepted_fringe_join', 'art-direction/western-watercourse-join-audit/fringe-correction/exports/joined.png',
     (1536, 384)),
)
SOURCES = [GROUND, WATER, BANK, CAP, 'game/Main.tscn',
           'art-direction/western-watercourse-proposal/proposal.json',
           'art-direction/spring-curved-bank-proof/visual-acceptance.json',
           'art-direction/spring-bank-join-proof/b2-visual-acceptance.json',
           'art-direction/b2-bridge-proof/b1-visual-acceptance.json',
           'art-direction/western-watercourse-join-audit/outlet-visual-acceptance.json',
           'art-direction/western-watercourse-join-audit/fringe-correction/visual-acceptance.json',
           c.PREFIX + 'scope-authorization.json', c.PREFIX + 'tools/channel.py', c.PREFIX + 'tools/common.py']
SOURCES += [path for _, path, _ in PATCHES]


def position(world):
    """Translate an exact world position to the external assembly canvas."""
    return world[0] - ORIGIN[0], world[1] - ORIGIN[1]


def western():
    """Fill uncovered western runs with the exact B1/B2 supersampled bank/UV recipe."""
    size = (1600, 448)
    origin = (-64, 384)
    ground, water, bank = (c.rgba(path) for path in (GROUND, WATER, BANK))
    ground_layer = c.tile(ground, origin, size)
    layers = [Image.new('RGBA', (size[0] * 2, size[1] * 2)) for _ in range(2)]
    pixels = [image.load() for image in layers]
    for y in range(size[1] * 2):
        wy = origin[1] + (y + .5) / 2
        for x in range(size[0] * 2):
            wx = origin[0] + (x + .5) / 2
            top = 552 - max(0, wx - 1400) / 2
            bottom = top + 96
            if top <= wy < bottom:
                pixels[0][x, y] = water.getpixel((math.floor(wx) % 128, math.floor(wy) % 96))
            row = None
            if top - 16 <= wy < top:
                row = math.floor((wy - top + 16) * 2)
            elif bottom <= wy < bottom + 16:
                row = math.floor((bottom + 16 - wy) * 2)
            if row is not None:
                pixels[1][x, y] = bank.getpixel((math.floor(wx) % 128, min(31, row)))
    for image in layers:
        exported = image.resize(size, Image.Resampling.BOX)
        exported.putdata([p if p[3] else (0, 0, 0, 0) for p in exported.get_flattened_data()])
        ground_layer.alpha_composite(exported)
    return ground_layer


def retained():
    """Reconstruct authored Stream UVs, caps, banks and its retained central crossing."""
    origin, size = (1600, 384), (400, 256)
    ground, water, bank, cap = (c.rgba(path) for path in (GROUND, WATER, BANK, CAP))
    result = c.tile(ground, origin, size)
    wet = c.tile(water, (0, 0), (400, 96))
    result.paste(wet, (0, 68))
    # Authored Polygon2D Color(0.62, 0.49, 0.31, 0.82); reference only.
    result.alpha_composite(Image.new('RGBA', (128, 96), (158, 125, 79, 209)), (136, 68))
    for x, image in ((0, bank.crop((60, 0, 128, 32))), (68, cap),
                     (300, cap.transpose(Image.Transpose.FLIP_LEFT_RIGHT)),
                     (332, bank.crop((0, 0, 68, 32)))):
        result.alpha_composite(image, (x, 68))
        result.alpha_composite(image.transpose(Image.Transpose.FLIP_TOP_BOTTOM), (x, 132))
    return result


def render():
    """Keep local sources independent and paste the accepted fringe successor last."""
    layers = [('ground', c.tile(c.rgba(GROUND), ORIGIN, SIZE), (0, 0)),
              ('western_continuation', western(), position((-64, 384))),
              ('retained_Stream_reference', retained(), position((1600, 384)))]
    layers += [(name, c.rgba(path), position(world)) for name, path, world in PATCHES]
    return layers, c.compose(layers, SIZE)


def crop(image, world, size):
    """Extract a world-aligned rectangle without resampling."""
    x, y = position(world)
    return image.crop((x, y, x + size[0], y + size[1]))


def validate(image):
    """Require exact accepted local views, allowing only the already accepted B1 patch."""
    c.require(image.size == SIZE and image.getchannel('A').getextrema() == (255, 255),
              'channel dimensions/opacity')
    for name, path, world in PATCHES:
        source = c.rgba(path)
        actual = crop(image, world, source.size)
        if name == 'B1':
            # The accepted join supersedes only this exact local rectangle.
            actual.paste(source.crop((448, 0, 512, 256)), (448, 0))
        c.equal(actual, source, f'accepted {name} view changed')
    # Independently verify all flat uncovered water/bank/ground texels. Adjacent
    # columns need not match; absolute world phase must match the source tile.
    ground, water = c.rgba(GROUND), c.rgba(WATER)
    bank = c.rgba(BANK).resize((128, 16), Image.Resampling.BOX)
    for left, right in ((-64, 0), (320, 432), (944, 1088)):
        for wy in range(384, 832):
            for wx in range(left, right):
                expected = ground.getpixel((wx % ground.width, wy % ground.height))
                if 552 <= wy < 648:
                    expected = water.getpixel((wx % 128, wy % 96))
                row = wy - 536 if 536 <= wy < 552 else 663 - wy if 648 <= wy < 664 else None
                if row is not None:
                    pixel = Image.new('RGBA', (1, 1), expected)
                    pixel.alpha_composite(Image.new('RGBA', (1, 1), bank.getpixel((wx % 128, row))))
                    expected = pixel.getpixel((0, 0))
                c.require(image.getpixel(position((wx, wy))) == expected,
                          f'uncovered channel mask/UV error at {(wx, wy)}')
    c.equal(crop(image, (1664, 384), (236, 256)), retained().crop((64, 0, 300, 256)),
            'retained caps/crossing changed')


def run(build=False):
    """Build or reproduce the assembly, full-resolution views, detail views and master."""
    baseline = c.scope(build)
    layers, image = render()
    validate(image)
    views = {'exports/joined.png': image, 'review/full-1x.png': image,
             'review/overview.png': image.resize((1302, 310), Image.Resampling.LANCZOS)}
    for name, world, size in (('B2', (432, 384), (512, 448)),
                              ('B1', (1088, 384), (576, 448)),
                              ('retained-crossing', (1600, 400), (400, 192)),
                              ('spring', (1900, 212), (640, 576))):
        detail = crop(image, world, size)
        views[f'review/{name}-1x.png'] = detail
        views[f'review/{name}-2x.png'] = detail.resize((size[0] * 2, size[1] * 2), Image.Resampling.NEAREST)
    engineering = views['review/overview.png'].copy()
    draw = ImageDraw.Draw(engineering)
    for x, label in ((0, 'outlet'), (432, 'B2'), (1088, 'B1'), (1536, 'join'),
                     (1736, 'retained crossing'), (2000, 'spring inlet')):
        px = (x - ORIGIN[0]) // 2
        draw.line((px, 0, px, engineering.height - 1), fill='#ffd66e')
        draw.text((px + 3, 8), label, fill='white', font=ImageFont.load_default(size=11))
    views['review/engineering.png'] = engineering
    evidence = {
        'protected_file_count': len(baseline), 'accepted_source_files_changed': 0,
        'runtime_files_changed': 0, 'origin': ORIGIN, 'size': SIZE,
        'accepted_local_view_overrides': 'only the already accepted 128x256 fringe join at (1536,384)',
        'spring': {'center': [2192, 500], 'basin_diameter': 384, 'inlet_width': 32},
        'limitations': [
            'external source composition, not a Godot capture or physical promotion',
            'retained crossing colour is an authored CPU reconstruction, not GPU capture parity',
            'accepted local fringe correction ends at x=1664; original retained cap/bank recipe resumes',
            'complete composition still requires its own visual assessment',
        ],
    }
    c.write_artifacts('channel', ORIGIN, SIZE, layers, views, evidence, SOURCES, build)
    print(f'PASS: whole channel; {len(baseline)} predecessors; exact local views, caps and crossing; editable master')


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--build', action='store_true')
    run(parser.parse_args().build)
