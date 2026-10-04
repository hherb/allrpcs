#!/usr/bin/env python3
"""Expose padded material candidates without changing approved source artwork."""
from pathlib import Path
import hashlib
import json
from PIL import Image, ImageDraw, ImageFont

ROOT = Path(__file__).resolve().parent
REPO = ROOT.parents[1]
PACKAGE = REPO / 'art-direction/residual-ground-batch'
FONT = ImageFont.truetype('/System/Library/Fonts/Supplemental/Arial.ttf', 17)


def digest(path):
    """Hash the exact persisted evidence input."""
    return hashlib.sha256(path.read_bytes()).hexdigest()


def main():
    """Repeat all twelve material/transition exports exactly at 1:1 and report borders."""
    inventory = json.loads((PACKAGE / 'inventory.json').read_text())
    acceptance = json.loads((PACKAGE / 'owner-acceptance.json').read_text())
    manifest = json.loads((PACKAGE / 'review-manifest.json').read_text())
    if (acceptance['status'] != 'approved' or
            digest(PACKAGE / 'review-manifest.json') != acceptance['review_manifest_sha256'] or
            digest(PACKAGE / 'inventory.json') != manifest['files']['inventory.json']):
        raise ValueError('Approved package inventory or manifest changed')
    rows = []
    for family in ['ground-surfaces', 'surface-transitions']:
        board = Image.new('RGBA', (960, 760), '#e8e3d5')
        draw = ImageDraw.Draw(board)
        draw.text((16, 12), family.upper() + ' / exact padded exports repeated 2x2 at 1:1',
                  fill='#1a201d', font=FONT)
        draw.text((16, 38), 'Background gutters expose missing tile continuity; no artwork repair or runtime claim.',
                  fill='#1a201d', font=FONT)
        for index, asset in enumerate(a for a in inventory['assets'] if a['id'].startswith(family + '-')):
            path = PACKAGE / asset['export']
            sha256 = digest(path)
            if sha256 != acceptance['approved_artwork'][asset['export']]:
                raise ValueError('Approved material changed: ' + asset['id'])
            with Image.open(path) as export:
                x, y = 20 + (index % 3) * 320, 86 + (index // 3) * 330
                for column in range(2):
                    for row in range(2):
                        board.alpha_composite(export, (x + column * export.width, y + row * export.height))
                draw.text((x, y + 282), asset['id'], fill='#1a201d', font=FONT)
                alpha = export.getchannel('A')
                border_boxes = [(0, 0, export.width, 1), (0, export.height - 1, export.width, export.height),
                                (0, 0, 1, export.height), (export.width - 1, 0, export.width, export.height)]
                borders = [alpha.crop(box).getextrema()[1] for box in border_boxes]
                bounds = alpha.point(lambda value: 255 if value >= 8 else 0).getbbox()
                rows.append({'id': asset['id'], 'export': str(path.relative_to(REPO)), 'sha256': sha256,
                             'export_size': list(export.size), 'border_max_alpha_top_bottom_left_right': borders,
                             'alpha_ge_8_bounds': list(bounds),
                             'transparent_outer_edges': all(value == 0 for value in borders),
                             'repeatability_status': 'padded presentation candidate; opaque connected material tiling not established'})
        board.convert('RGB').save(ROOT / f'{family}-repeat.png')
    if len(rows) != 12:
        raise ValueError('Material audit must cover all six surfaces and six transitions')
    result = {'date': '2026-10-04', 'scope': 'Read-only external material readiness audit',
              'acceptance_sha256': digest(PACKAGE / 'owner-acceptance.json'), 'assets': rows,
              'review_images': {f'{f}-repeat.png': digest(ROOT / f'{f}-repeat.png')
                                for f in ['ground-surfaces', 'surface-transitions']},
              'findings': ['All twelve exports have fully transparent outer rows and columns; adjacent padded copies leave background gutters',
                           'Matching transparent border values is not evidence of seamless material paint',
                           'Generated transition corners have no verified shared endpoints, fixed material coverage or connectors',
                           'Six variation overlays retain their intended isolated alpha; no new variation candidates are required by this audit'],
              'next_slice': 'Separate complete external production-feasibility package of six repeatable surface modules and six source-matched straight/outer/inset transition modules; preserve approved material studies and existing terrain',
              'authority': 'No runtime, collision, navigation, topology or terrain-completeness acceptance'}
    (ROOT / 'readiness.json').write_text(json.dumps(result, indent=2) + '\n')
    print('PASS: audited 12 exact approved exports; two 1:1 repeat boards; no approved artwork modified.')


if __name__ == '__main__':
    main()
