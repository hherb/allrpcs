#!/usr/bin/env python3
"""Draw a pose diagram for generated art; this never edits or supplies game sprite pixels."""

from pathlib import Path
from PIL import Image, ImageDraw, ImageFont

ROOT = Path(__file__).resolve().parent
LEFT, RIGHT, INK = '#b52f43', '#285ba6', '#454a40'
FONT = ImageFont.truetype('/System/Library/Fonts/Supplemental/Arial.ttf', 17)
POSES = [
    ((95, 162), (64, 219), (151, 167), (185, 205)),
    ((125, 167), (125, 219), (96, 158), (149, 184)),
    ((153, 162), (156, 216), (102, 153), (79, 199)),
    ((151, 167), (185, 205), (95, 162), (64, 219)),
    ((96, 158), (149, 184), (125, 167), (125, 219)),
    ((102, 153), (79, 199), (153, 162), (156, 216)),
]


def main():
    image = Image.new('RGB', (1536, 1024), '#f4f0e5')
    draw = ImageDraw.Draw(image)
    for row, direction in enumerate(['SOUTH', 'WEST', 'EAST', 'NORTH']):
        for column, pose in enumerate(POSES):
            ox, oy = column * 256, row * 256
            left_knee, left_foot, right_knee, right_foot = pose
            if direction in ['SOUTH', 'NORTH']:
                # Screen sides retain anatomical ownership; front/back depth swaps at half-cycle.
                lx, rx = (148, 108) if direction == 'SOUTH' else (108, 148)
                depths = [(225, 182), (218, 191), (193, 221), (182, 225), (191, 218), (221, 193)]
                ly, ry = depths[column]
                if direction == 'NORTH':
                    ly, ry = 407 - ly, 407 - ry
                left_foot, right_foot = (lx, ly), (rx, ry)
                left_knee, right_knee = (lx, (124 + ly) // 2), (rx, (124 + ry) // 2)
            elif direction == 'EAST':
                left_knee, left_foot, right_knee, right_foot = [(256 - x, y) for x, y in pose]
            draw.text((ox + 10, oy + 4), f'{direction} / phase {column + 1}', fill=INK, font=FONT)
            draw.text((ox + 10, oy + 27), 'RED left / BLUE right', fill=INK, font=FONT)
            draw.ellipse((ox + 111, oy + 58, ox + 145, oy + 92), outline=INK, width=4)
            if direction in ['WEST', 'EAST']:
                side = -1 if direction == 'WEST' else 1
                draw.line([(ox + 128 + side * 17, oy + 76), (ox + 128 + side * 27, oy + 80)], fill=INK, width=4)
            draw.line([(ox + 128, oy + 92), (ox + 128, oy + 124)], fill=INK, width=7)
            limbs = [(LEFT, left_knee, left_foot), (RIGHT, right_knee, right_foot)]
            if direction != 'EAST':
                limbs.reverse()
            for color, knee, foot in limbs:
                draw.line([(ox + 128, oy + 124), (ox + knee[0], oy + knee[1]),
                           (ox + foot[0], oy + foot[1])], fill=color, width=8)
                draw.ellipse((ox + foot[0] - 9, oy + foot[1] - 4,
                              ox + foot[0] + 9, oy + foot[1] + 5), fill=color)
    image.save(ROOT / 'walk-pose-guide.png')


if __name__ == '__main__':
    main()
