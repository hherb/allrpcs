#!/usr/bin/env python3
"""Derive disjoint transparent presentation layers from frozen channel recipes."""

import importlib.util
import math
from pathlib import Path
import sys

from PIL import Image, ImageChops

ROOT = Path(__file__).resolve().parents[3]
ORIGIN = (-64, 212)
SIZE = (2604, 620)
NAMES = ('water', 'banks', 'decks', 'detail', 'fringe')


def load(name, path):
    """Load retained helpers without invoking their preservation/build commands."""
    path = ROOT / path
    sys.path.insert(0, str(path.parent))
    try:
        spec = importlib.util.spec_from_file_location(name, path)
        module = importlib.util.module_from_spec(spec)
        sys.modules[name] = module
        spec.loader.exec_module(module)
        return module
    finally:
        sys.path.pop(0)


c = load('preview_common', 'art-direction/approved-external-compositions/tools/common.py')
channel = load('preview_channel', 'art-direction/approved-external-compositions/tools/channel.py')
join = load('preview_join', 'art-direction/western-watercourse-join-audit/tools/join.py')


def image(path):
    """Load detached source RGBA bytes."""
    with Image.open(ROOT / path) as source:
        return source.convert('RGBA')


def clean(layer):
    """Normalize invisible RGB only, retaining every visible source texel."""
    layer.putdata([p if p[3] else (0, 0, 0, 0) for p in layer.get_flattened_data()])
    return layer


def western():
    """Reproduce the accepted absolute-world supersampled water/bank recipe."""
    size, origin = (1600, 448), (-64, 384)
    water, bank = image(channel.WATER), image(channel.BANK)
    high = [Image.new('RGBA', (size[0] * 2, size[1] * 2)) for _ in range(2)]
    pixels = [layer.load() for layer in high]
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
    return {'water': clean(high[0].resize(size, Image.Resampling.BOX)),
            'banks': clean(high[1].resize(size, Image.Resampling.BOX))}


def sources():
    """Describe exact local layers; roads and crossing are CPU comparison inputs only."""
    parts = [('western', (-64, 384), western())]
    wet = c.tile(image(channel.WATER), (0, 0), (400, 96))
    retained = {name: Image.new('RGBA', (400, 256)) for name in ('water', 'banks', 'crossing')}
    retained['water'].paste(wet, (0, 68))
    retained['crossing'].paste(Image.new('RGBA', (128, 96), (158, 125, 79, 209)), (136, 68))
    bank, cap = image(channel.BANK), image(channel.CAP)
    for x, local in ((0, bank.crop((60, 0, 128, 32))), (68, cap),
                     (300, cap.transpose(Image.Transpose.FLIP_LEFT_RIGHT)),
                     (332, bank.crop((0, 0, 68, 32)))):
        retained['banks'].alpha_composite(local, (x, 68))
        retained['banks'].alpha_composite(local.transpose(Image.Transpose.FLIP_TOP_BOTTOM), (x, 132))
    parts.append(('retained', (1600, 384), retained))
    for name, _, position in channel.PATCHES[:-1]:
        folder = {'outlet': 'western-outlet-proof', 'B2': 'b2-bridge-proof',
                  'B1': 'b1-bridge-proof', 'curved_spring': 'spring-curved-bank-proof'}[name]
        keys = {'outlet': [('water', 'water'), ('banks', 'north_bank'), ('banks2', 'south_bank')],
                'B1': [('road', 'road'), ('water', 'water'), ('banks', 'bank'), ('decks', 'deck')],
                'B2': [('road', 'road'), ('water', 'water'), ('banks', 'bank'), ('decks', 'deck')],
                'curved_spring': [('water', 'retained_water'), ('banks', 'retained_banks'),
                                  ('water2', 'new_water'), ('banks2', 'new_banks'), ('detail', 'spring_detail')]}[name]
        parts.append((name, position, {key: image(f'art-direction/{folder}/exports/{file}.png')
                                      for key, file in keys}))
    raw = join.render()
    # The accepted right half is the actual retained recipe before its bounded fringe patch.
    joining = {key: Image.new('RGBA', (128, 256)) for key in ('water', 'banks')} | {
        'fringe': image('art-direction/western-watercourse-join-audit/fringe-correction/exports/ground-under-bank-fringe.png')}
    for key, old in (('water', 'water_underlay'), ('banks', 'banks')):
        joining[key].paste(clean(raw[old].resize((64, 256), Image.Resampling.BOX)), (0, 0))
        joining[key].paste(retained[key].crop((0, 0, 64, 256)), (64, 0))
    parts.append(('accepted_join', (1536, 384), joining))
    return parts


def placed(local, position):
    """Place one image on the channel canvas without resampling."""
    canvas = Image.new('RGBA', SIZE)
    canvas.paste(local, (position[0] - ORIGIN[0], position[1] - ORIGIN[1]))
    return canvas


def derive():
    """Apply assembly ownership rectangles before stacking independent source layers."""
    parts = sources()
    masks = []
    for _, position, layers in parts:
        size = next(iter(layers.values())).size
        mask = Image.new('L', SIZE)
        mask.paste(255, (position[0] - ORIGIN[0], position[1] - ORIGIN[1],
                         position[0] - ORIGIN[0] + size[0], position[1] - ORIGIN[1] + size[1]))
        masks.append(mask)
    atlases = {name: Image.new('RGBA', SIZE) for name in (*NAMES, 'road', 'crossing')}
    runtime = {name: Image.new('RGBA', SIZE) for name in NAMES}
    for index, (label, position, layers) in enumerate(parts):
        ownership = masks[index].copy()
        for later in masks[index + 1:]:
            ownership = ImageChops.subtract(ownership, later)
        for key, local in layers.items():
            key = key.rstrip('2')
            canvas = placed(local, position)
            canvas.putalpha(ImageChops.multiply(canvas.getchannel('A'), ownership))
            clean(canvas)
            atlases[key].alpha_composite(canvas)
            if key in runtime:
                # Keep original retained water/caps live, including spring's x<2000 reference.
                if label == 'retained':
                    continue
                if label == 'curved_spring':
                    canvas.paste((0, 0, 0, 0), (0, 0, 2000 - ORIGIN[0], SIZE[1]))
                runtime[key].alpha_composite(canvas)
    ground = c.tile(image(channel.GROUND), ORIGIN, SIZE)
    reference = ground.copy()
    for name in ('road', 'water', 'crossing', 'banks', 'decks', 'detail', 'fringe'):
        reference.alpha_composite(atlases[name])
    accepted = image('art-direction/approved-external-compositions/channel/exports/joined.png')
    c.equal(reference, accepted, 'transparent source extraction cannot reproduce accepted RGBA')
    channel.validate(reference)
    return runtime, atlases, reference
