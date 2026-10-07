#!/usr/bin/env python3
"""Retain all four cloak directions, source phases and anatomy diagnostics together."""

import json
from pathlib import Path

from PIL import Image, ImageDraw, ImageSequence

from build import DIRECTIONS, OUT, REPO, render, require, sha

SAMPLES = [0, 90, 120, 180, 240, 270, 360, 450, 480, 540, 600, 630]


def lossless(target, frames, durations):
    """Write and verify every animation frame without palette quantization."""
    frames[0].save(target, save_all=True, append_images=frames[1:], duration=durations,
                   loop=0, lossless=True, quality=100, method=4)
    with Image.open(target) as animation:
        decoded = [frame.convert('RGBA').copy() for frame in ImageSequence.Iterator(animation)]
    require(len(frames) == len(decoded) and all(a.tobytes() == b.tobytes() for a, b in zip(frames, decoded)),
            'Lossless comparison changed a rendered frame')


def main():
    """Package actual imported comparison captures and the complete native contact sheet."""
    require(not (OUT / 'candidate.json').exists(), 'Candidate comparison snapshot is retained')
    review = OUT / 'review'
    review.mkdir(exist_ok=True)
    durations = [end - start for start, end in zip(SAMPLES, SAMPLES[1:] + [720])]
    for scale in [1, 2]:
        frames = [Image.open(OUT / f'captures/comparison-{scale}x-{sample:03d}.png').convert('RGBA') for sample in SAMPLES]
        lossless(review / f'old-vs-new-{scale}x.webp', frames, durations)
    contacts = Image.new('RGBA', (1280, 792), '#e9e5d9')
    draw = ImageDraw.Draw(contacts)
    for row, direction in enumerate(DIRECTIONS):
        for index in range(1, 9):
            frame = Image.open(OUT / f'exports/seeker-cloak-{direction}-walk-{index:02d}.png').convert('RGBA')
            contacts.alpha_composite(frame, (160 * (index - 1), 198 * row + 20))
            label = f'{direction} {index}: {"A contact" if index == 1 else "B contact" if index == 5 else "pass" if index in (3, 7) else "stride"}'
            draw.text((160 * (index - 1) + 2, 198 * row + 2), label, fill='black')
    contacts.save(review / 'all-32-phases.png')
    diagnostic = [render(direction, diagnostic=True)[0] for direction in DIRECTIONS]
    frames = []
    for index in range(8):
        board = Image.new('RGBA', (1280, 406), '#e9e5d9')
        draw = ImageDraw.Draw(board)
        draw.text((8, 5), 'Anatomical ownership diagnostic: RED = left A; BLUE = right B; colors are not production paint', fill='black')
        for column, direction in enumerate(DIRECTIONS):
            board.alpha_composite(diagnostic[column][index], (320 * column, 44))
            draw.text((320 * column + 8, 26), f'{direction}: phase {index * 45} deg', fill='black')
        frames.append(board)
    lossless(review / 'leg-ownership-2x.webp', frames, [90] * 8)
    report = {'status': 'passed', 'source': '24 actual imported Godot comparison captures',
              'comparison_cycle_ms': 720, 'sample_times_ms': SAMPLES, 'lossless_decoded_rgba': 'Exact source capture matches',
              'files': {str(path.relative_to(REPO)): sha(path) for path in sorted(review.iterdir())}}
    (review / 'manifest.json').write_text(json.dumps(report, indent=2) + '\n')
    print('PASS: complete native/2x old/new comparisons, all 32 poses and fixed-anatomy diagnostic')


if __name__ == '__main__':
    main()
