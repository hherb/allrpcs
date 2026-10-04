#!/usr/bin/env python3
"""Compare finite ground repeats with sparse accepted overlays at native scale."""

import argparse
import json

import numpy as np
from PIL import Image, ImageFont

from audit import (ROOT, REPO, TILING, BATCH, FONT_PATH, FIELD_SIZE,
                   TILE_SIZE, canvas, digest, inputs, place, raster,
                   read_json, repeated_field, require)

PATCH_SIZE = FIELD_SIZE * 2
CENTRES = [(x, y) for y in (156, 356) for x in (86, 256, 426)]


def render():
    """Place all six unchanged overlays once on each of six 4x4 tile fields."""
    fields, overlays, _, bound = inputs()
    require(read_json(ROOT / 'audit.json')['inputs'] == bound, 'Compatibility inputs changed')
    boards, samples = {}, []
    font = ImageFont.truetype(str(FONT_PATH), 16)
    for field in fields:
        plain = Image.new('RGBA', (PATCH_SIZE, PATCH_SIZE))
        quadrant = repeated_field(field)
        for y in (0, FIELD_SIZE):
            for x in (0, FIELD_SIZE):
                plain.alpha_composite(quadrant, (x, y))
        dressed = plain.copy()
        placements = []
        boxes = []
        for overlay, centre in zip(overlays, CENTRES, strict=True):
            image = raster(BATCH / overlay['export'], tuple(overlay['size']))
            xy = (centre[0] - image.width // 2, centre[1] - image.height // 2)
            require(xy[0] >= 0 and xy[1] >= 0 and xy[0] + image.width <= PATCH_SIZE and
                    xy[1] + image.height <= PATCH_SIZE, 'Dressing clips an overlay')
            box = (*xy, xy[0] + image.width, xy[1] + image.height)
            require(all(box[2] <= other[0] or other[2] <= box[0] or
                        box[3] <= other[1] or other[3] <= box[1] for other in boxes),
                    'Dressing overlays overlap')
            boxes.append(box)
            dressed.alpha_composite(image, xy)
            placements.append({'overlay': overlay['id'],
                               'source': str((BATCH / overlay['export']).relative_to(REPO)),
                               'patch_origin': list(xy), 'size': list(image.size),
                               'tile_phase': [xy[0] % TILE_SIZE, xy[1] % TILE_SIZE],
                               'scale': 1, 'rotation_degrees': 0})
        board, draw = canvas((1100, 715), f'{field["id"]} / plain and sparsely dressed 512px fields',
                             'Exact 1:1 / sixteen repeats per field / each accepted overlay once / diagnostic placements only')
        plain_xy, dressed_xy = (20, 90), (560, 90)
        place(board, plain, plain_xy)
        place(board, dressed, dressed_xy)
        draw.text((20, 614), 'Plain: 4x4 unchanged 128px tiles', font=font, fill='#202820')
        draw.text((560, 614), 'Dressed: same phase and tiles, six native overlays', font=font, fill='#202820')
        for row in range(2):
            text = ' / '.join(a['name'] for a in overlays[row * 3:row * 3 + 3])
            draw.text((560, 642 + row * 24), text, font=font, fill='#202820')
        filename = f'review/dressing-{field["id"]}-1x.png'
        boards[filename] = board.convert('RGB')
        changed = np.any(np.asarray(plain) != np.asarray(dressed), axis=2)
        samples.append({'field': field['id'], 'board': filename,
                        'field_tile': str((TILING / field['export']).relative_to(REPO)),
                        'patch_size': [PATCH_SIZE, PATCH_SIZE], 'tile_repeat': [4, 4],
                        'plain_origin': list(plain_xy), 'dressed_origin': list(dressed_xy),
                        'overlay_order': placements, 'changed_pixel_count': int(changed.sum()),
                        'unchanged_pixel_fraction': round(float((~changed).mean()), 4)})
    report = {'schema_version': 1, 'date': '2026-10-04', 'timezone': 'Australia/Sydney',
              'scope': 'Read-only external large-area repeat/dressing diagnostic; no new artwork',
              'compatibility_audit_sha256': digest(ROOT / 'audit.json'),
              'samples': samples, 'counts': {'boards': 6, 'overlay_placements': 36,
                                           'surface_tiles_per_board': 32},
              'limitations': 'One sparse layout per field. Repeated base motifs remain; no world geography, terrain completion, preferred placement density or runtime acceptance established.'}
    return boards, report


def main():
    """Write only diagnostic files, or recheck exact persisted pixels and metadata."""
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--check', action='store_true')
    args = parser.parse_args()
    boards, report = render()
    if args.check:
        saved = read_json(ROOT / 'dressing.json')
        for filename, expected in boards.items():
            actual = raster(ROOT / filename)
            require(actual.size == expected.size and
                    np.array_equal(np.asarray(actual.convert('RGB')), np.asarray(expected)),
                    f'Dressing pixels changed: {filename}')
            require(digest(ROOT / filename) == saved['boards'][filename], 'Dressing board bytes changed')
        report['boards'] = saved['boards']
        require(set(report['boards']) == set(boards) and report == saved, 'Dressing metadata changed')
    else:
        (ROOT / 'review').mkdir(parents=True, exist_ok=True)
        for filename, board in boards.items():
            board.save(ROOT / filename)
        report['boards'] = {name: digest(ROOT / name) for name in boards}
        (ROOT / 'dressing.json').write_text(json.dumps(report, indent=2) + '\n')
    print('PASS: six native 512px plain/dressed comparisons; 36 unclipped, non-overlapping overlays; frozen inputs intact.')


if __name__ == '__main__':
    main()
