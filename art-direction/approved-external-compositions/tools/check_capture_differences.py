#!/usr/bin/env python3
"""Independently confine forest capture differences to the actual clipped strip footprint."""

from PIL import Image, ImageChops

import common as c
import forest

FILTER_MARGIN = 3
CLIP_BOUNDS = (584, 192, 1072, 416)


def run():
    """Validate capture provenance, then project the clip through the retained limited camera."""
    c.scope()
    forest.captures()
    for target, point in forest.TARGETS.items():
        for zoom in (.8, 1., 2.):
            for frame, height in (('wide', 720), ('tall', 827)):
                suffix = f'{target}-{zoom:.1f}x-{frame}.png'
                with Image.open(c.HERE / 'forest/captures' / ('raw-' + suffix)) as source:
                    before = source.convert('RGB')
                with Image.open(c.HERE / 'forest/captures' / ('bounded-' + suffix)) as source:
                    after = source.convert('RGB')
                bounds = ImageChops.difference(before, after).getbbox()
                cx = min(max(point[0], 640 / zoom), 3200 - 640 / zoom)
                cy = min(max(point[1], height / (2 * zoom)), 1800 - height / (2 * zoom))
                left, top, right, bottom = CLIP_BOUNDS
                expected = ((left - cx) * zoom + 640 - FILTER_MARGIN,
                            (top - cy) * zoom + height / 2 - FILTER_MARGIN,
                            (right - cx) * zoom + 640 + FILTER_MARGIN,
                            (bottom - cy) * zoom + height / 2 + FILTER_MARGIN)
                c.require(bounds is not None and bounds[0] >= expected[0] and bounds[1] >= expected[1]
                          and bounds[2] <= expected[2] and bounds[3] <= expected[3],
                          f'capture differences escaped clip: {suffix}')
    print('PASS: 18 independently bounded raw/corrected capture differences')


if __name__ == '__main__':
    run()
