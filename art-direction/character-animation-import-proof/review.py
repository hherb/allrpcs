#!/usr/bin/env python3
"""Retain lossless collective animation previews from actual Godot capture pixels."""

import json
from pathlib import Path

from PIL import Image, ImageSequence

from build import PACKAGE, REPO, require, sha, write_json

SAMPLES = {'idle': ([0, 600], 1200), 'inspect': ([0, 160, 320, 480], 640),
           'walk': ([0, 90, 120, 180, 240, 270, 360, 450, 480, 540, 600, 630], 720)}


def main():
    """Encode complete comparisons without changing any captured frame pixels."""
    package = REPO / PACKAGE
    require(not (package / 'verification.json').exists(), 'Verified proof is frozen')
    report = json.loads((package / 'evidence/final/run.json').read_text())
    require(report['status'] == 'passed', 'Final Godot capture check did not pass')
    review = package / 'review'
    require(not review.exists(), 'Retained comparison already exists')
    review.mkdir()
    results = []
    for scale in [1, 2]:
        for action, (samples, period) in SAMPLES.items():
            paths = [package / f'evidence/final/captures/{action}-{scale}x-{sample:03d}.png' for sample in samples]
            frames = [Image.open(path).convert('RGBA') for path in paths]
            durations = [end - start for start, end in zip(samples, samples[1:] + [period])]
            target = review / f'{action}-{scale}x.webp'
            frames[0].save(target, save_all=True, append_images=frames[1:], duration=durations,
                           loop=0, lossless=True, quality=100, method=4)
            with Image.open(target) as animation:
                decoded = [image.convert('RGBA').copy() for image in ImageSequence.Iterator(animation)]
            require(len(decoded) == len(frames), 'Lossless preview omitted a comparison phase')
            require(all(a.tobytes() == b.tobytes() for a, b in zip(decoded, frames)), 'Preview changed captured RGBA')
            results.append({'path': str(target.relative_to(REPO)), 'sha256': sha(target),
                            'duration_ms': durations, 'cycle_ms': period,
                            'capture_paths': [str(path.relative_to(REPO)) for path in paths],
                            'decoded_rgba': 'exact capture matches'})
    write_json(review / 'manifest.json', {'status': 'passed', 'comparisons': results})
    print('PASS: six lossless previews; complete family/direction coverage at native and 2x scale')


if __name__ == '__main__':
    main()
