#!/usr/bin/env python3
"""Render a separate seam-polish candidate, preserving the first mechanical snapshot."""

import math

from PIL import Image, ImageDraw, ImageEnhance

from build_review import ROOT
from rig_walk import STEPS, prepare, render_package, substantive_bounds

OUT = ROOT / 'walk-rig-polish'


def side_boot(paint, role, direction):
    """Mask the measured exposed staff strip embedded in Elian's west boot crop."""
    result = paint.copy()
    if role == 'elian' and direction == 'west':
        # The staff touches the toe, so connected-component extraction cannot
        # separate it. Keep the toe contour while masking the strip above it.
        alpha = result.getchannel('A')
        ImageDraw.Draw(alpha).polygon([(0, 0), (27, 0), (27, 29), (24, 33), (0, 33)], fill=0)
        result.putalpha(alpha)
    return result


def ankle(paint):
    """Find the boot shaft, rather than joining a shin to the toe-centered crop."""
    bounds = substantive_bounds(paint)
    band = paint.getchannel('A').crop((0, bounds[1], paint.width, min(paint.height, bounds[1] + 8)))
    shaft = band.point(lambda value: 255 if value > 96 else 0).getbbox()
    return (shaft[0] + shaft[2]) / 2, max(12, min(28, shaft[2] - shaft[0]))


def connected_limb(paint, texture, socket, hem, floor, advance, lift, bend):
    """Join a rigid boot shaft to the hidden hip with two rounded textured segments."""
    result = Image.new('RGBA', (256, 384))
    shaft_x, shaft_width = ankle(paint)
    boot_x = round(socket - paint.width / 2 + advance)
    boot_y = round(floor - lift + 1 - paint.height)
    hip = (socket, hem - 12)
    shin = (boot_x + shaft_x, boot_y + 10)
    knee = ((hip[0] + shin[0]) / 2 + bend, (hip[1] + shin[1]) / 2)
    mask = Image.new('L', result.size)
    draw = ImageDraw.Draw(mask)
    draw.line([hip, knee, shin], fill=255, width=round(shaft_width), joint='curve')
    radius = shaft_width / 2
    for x, y in [hip, knee, shin]:
        draw.ellipse((x - radius, y - radius, x + radius, y + radius), fill=255)
    # Stretch once over the local connector bounds. Repeating a small swatch makes
    # a conspicuous checker pattern that conflicts with the original gouache paint.
    textured = Image.new('RGBA', result.size)
    left, top, right, bottom = mask.getbbox()
    textured.paste(texture.resize((right - left, bottom - top), Image.Resampling.BICUBIC), (left, top))
    textured.putalpha(mask)
    result.alpha_composite(textured)
    result.alpha_composite(paint, (boot_x, boot_y))
    return result


def prepare_polished(role, direction):
    """Keep identical near/far boot geometry while masking the exposed staff strip."""
    body, feet, textures, sockets, hem, floor, sources = prepare(role, direction)
    if direction in ('east', 'west'):
        # Clean the bright limb once so the near/far layers share identical geometry.
        bright = feet[1] if direction == 'east' else feet[0]
        bright = side_boot(bright, role, direction)
        dark = ImageEnhance.Brightness(bright).enhance(0.88)
        feet = [dark, bright] if direction == 'east' else [bright, dark]
    return body, feet, textures, sockets, hem, floor, sources


def render(role, direction, diagnostic=False):
    """Keep anatomical ownership and rigid upper-body/accessory paint through a full loop."""
    body, feet, textures, sockets, hem, floor, sources = prepare_polished(role, direction)
    side = direction in ('east', 'west')
    sign = 1 if direction == 'east' else -1
    stride = min(22, max(12, (floor - hem) * 0.75))
    frames, metadata = [], []
    for step in range(STEPS + 1):
        phase = math.tau * step / STEPS
        layers, positions = [], []
        for leg in range(2):
            theta = phase + math.pi * leg
            travel = math.cos(theta)
            swing = max(0, -math.sin(theta))
            lift = swing * (8 if side else 5)
            advance = sign * stride * travel if side else 0
            depth = 0 if side else 5 * travel * (1 if direction == 'south' else -1)
            bend = -sign * 4 * swing if side else 0
            layer = connected_limb(feet[leg], textures[leg], sockets[leg], hem, floor,
                                   advance, lift - depth, bend)
            if diagnostic:
                tint = Image.new('RGBA', layer.size, '#d13d38' if leg == 0 else '#2879cd')
                tint.putalpha(layer.getchannel('A'))
                layer = tint
            layers.append(layer)
            positions.append({'leg': 'A' if leg == 0 else 'B',
                              'anatomical_side': 'left' if leg == 0 else 'right',
                              'travel': round(travel, 6), 'lift': round(lift, 6),
                              'advance': round(advance, 6), 'depth': round(depth, 6),
                              'knee_bend_2x': round(bend, 6),
                              'paint_bounds_2x': substantive_bounds(layer),
                              'stance': math.sin(theta) >= -1e-9})
        order = ([0, 1] if direction == 'east' else [1, 0]) if side else sorted(
            range(2), key=lambda leg: positions[leg]['depth'])
        canvas = Image.new('RGBA', body.size)
        for leg in order:
            canvas.alpha_composite(layers[leg])
        bob = -round(2 * abs(math.sin(phase)))
        canvas.alpha_composite(body, (0, bob))
        frames.append(canvas)
        metadata.append({'frame': step + 1, 'phase_degrees': step * 45,
                         'body_bob_2x': bob, 'legs': positions, 'layer_order': order})
    if frames[0].tobytes() != frames[-1].tobytes():
        raise ValueError(f'Loop endpoint differs: {role} {direction}')
    return frames[:-1], metadata[:-1], sources


if __name__ == '__main__':
    render_package(OUT, render, 'seam polish', [
        'Rigid boots with shaft-aligned textured connectors and restrained knee flex',
        'Neutral arms and rigid torso/accessories; gait, registration and timing need owner review',
        'Occluded side limb reuses visible-side paint; no runtime promotion',
    ], preview_label='seam polish')
