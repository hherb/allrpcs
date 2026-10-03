#!/usr/bin/env python3
"""Verify ending correction captures, bounded changes and in-memory negative cases."""

import argparse
import json

from PIL import Image, ImageChops

import proof as p

TARGETS = {'gate': (744, 200), 'midpoint': (825, 250), 'closed-end': (650, 150)}


def check(record=False):
    """Require fresh capture inputs and confine every changed view to the external ending area."""
    p.run(False)
    context = json.loads((p.HERE / 'result/capture-context.json').read_text())
    p.c.require(context['manifest_sha256'] == p.c.sha(p.HERE / 'result/manifest.json')
                and context['overlay_sha256'] == {'corrected.png': p.c.sha(p.HERE / 'result/exports/corrected.png')},
                'stale capture context')
    names = [f'corrected-{target}-{zoom:.1f}x-{frame}.png'
             for target in TARGETS for zoom in (.8, 1., 2.) for frame in ('wide', 'tall')]
    evidence = {'manifest_sha256': context['manifest_sha256'],
                'images': {name: p.c.sha(p.HERE / 'result/captures' / name) for name in names}}
    path = p.HERE / 'result/capture-evidence.json'
    if record:
        path.write_text(json.dumps(evidence, indent=2) + '\n')
    else:
        p.c.require(json.loads(path.read_text()) == evidence, 'capture evidence changed')
    for target, point in TARGETS.items():
        for zoom in (.8, 1., 2.):
            for frame, height in (('wide', 720), ('tall', 827)):
                suffix = f'{target}-{zoom:.1f}x-{frame}.png'
                with Image.open(p.OLD / 'forest/captures' / ('bounded-' + suffix)) as image:
                    before = image.convert('RGBA')
                with Image.open(p.HERE / 'result/captures' / ('corrected-' + suffix)) as image:
                    after = image.convert('RGBA')
                p.c.require(after.size == (1280, height) and after.getchannel('A').getextrema() == (255, 255),
                            f'capture dimensions/opacity: {suffix}')
                bounds = ImageChops.difference(before, after).convert('RGB').getbbox()
                cx = min(max(point[0], 640 / zoom), 3200 - 640 / zoom)
                cy = min(max(point[1], height / (2 * zoom)), 1800 - height / (2 * zoom))
                expected = ((520 - cx) * zoom + 637, (32 - cy) * zoom + height / 2 - 3,
                            (1072 - cx) * zoom + 643, (196 - cy) * zoom + height / 2 + 3)
                p.c.require(bounds is not None and bounds[0] >= expected[0] and bounds[1] >= expected[1]
                            and bounds[2] <= expected[2] and bounds[3] <= expected[3],
                            f'ending capture changes escaped external territory: {suffix}')
    rounded, corrected = p.render()
    for label, world, pixel in (
        ('gate intrusion', (744, 200), (1, 2, 3, 255)),
        ('opaque fade cutoff', (710, 185), (1, 2, 3, 255)),
        ('rounded source changed', (636, 142), (1, 2, 3, 255)),
        ('rounded terminus removed', (636, 142), (0, 0, 0, 0)),
        ('hidden RGB', (520, 32), (1, 2, 3, 0)),
    ):
        candidate = corrected.copy()
        candidate.putpixel((world[0] - p.ORIGIN[0], world[1] - p.ORIGIN[1]), pixel)
        try:
            p.validate(rounded, candidate)
        except ValueError:
            continue
        raise ValueError(f'mutation accepted: {label}')
    print('PASS: 18 fresh captures; independently bounded image changes; five mutation rejections')


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--record', action='store_true')
    check(parser.parse_args().record)
