"""Independent compositing checks, safe rejection probes and byte reproduction."""
import contextlib
import io
import json
import shutil
import sys
import tempfile
from pathlib import Path

import numpy as np
from PIL import Image

root = Path(__file__).resolve().parent
sys.path.insert(0, str(root))
import audit
import dressing
import interfaces


def rgb(path):
    """Load source RGB values for independent integer compositing."""
    with Image.open(path) as image:
        return np.asarray(image.convert('RGB'), dtype=np.int64)


def blend(background, source):
    """Calculate source-over pixels over a filled field without Pillow composition."""
    with Image.open(source) as image:
        foreground = np.asarray(image.convert('RGBA'), dtype=np.int64)
    alpha = foreground[:, :, 3:4]
    return (foreground[:, :, :3] * alpha + background * (255 - alpha) + 127) // 255


packet = json.loads((root / 'audit.json').read_text())
checks = 0
for row in packet['overlay_combinations']:
    x, y, width, height = row['field_rect']
    expected = np.tile(rgb(audit.REPO / row['field_tile']), (2, 2, 1))
    ox, oy = row['overlay_origin'][0] - x, row['overlay_origin'][1] - y
    ow, oh = row['overlay_size']
    expected[oy:oy+oh, ox:ox+ow] = blend(expected[oy:oy+oh, ox:ox+ow], audit.REPO / row['overlay_source'])
    actual = rgb(root / row['board'])[y:y+height, x:x+width]
    audit.require(np.array_equal(actual, expected), f'Overlay pixels changed: {row}')
    checks += 1
for row in packet['direct_joins']:
    board = rgb(root / row['board'])
    for key, expected in [('legacy', rgb(audit.REPO / row['legacy_source'])),
                          ('field', np.tile(rgb(audit.REPO / row['field_tile']), (2, 2, 1)))]:
        x, y = row[key + '_origin']
        audit.require(np.array_equal(board[y:y+256, x:x+256], expected), f'Join pixels changed: {row}')
        checks += 1
large = json.loads((root / 'dressing.json').read_text())
for row in large['samples']:
    board = rgb(root / row['board'])
    expected = np.tile(rgb(audit.REPO / row['field_tile']), (4, 4, 1))
    x, y = row['plain_origin']
    audit.require(np.array_equal(board[y:y+512, x:x+512], expected), 'Large-field pixels changed')
    checks += 1
    for overlay in row['overlay_order']:
        ox, oy = overlay['patch_origin']
        ow, oh = overlay['size']
        expected[oy:oy+oh, ox:ox+ow] = blend(expected[oy:oy+oh, ox:ox+ow], audit.REPO / overlay['source'])
    x, y = row['dressed_origin']
    audit.require(np.array_equal(board[y:y+512, x:x+512], expected), 'Large-field pixels changed')
    checks += 1

with tempfile.TemporaryDirectory(prefix='allrpcs-ground-review-') as folder:
    temporary = Path(folder)
    for module in [audit, dressing, interfaces]:
        module.ROOT = temporary
    sys.argv = ['review']
    with contextlib.redirect_stdout(io.StringIO()):
        for module in [audit, dressing, interfaces]:
            module.main()
    outputs = sorted([*packet['boards'], *large['boards'], 'audit.json', 'dressing.json', 'interfaces.json'])
    for name in outputs:
        audit.require((temporary / name).read_bytes() == (root / name).read_bytes(), f'Byte reproduction failed: {name}')
    sys.argv = ['review', '--check']
    probes = []
    # Mutate only temporary diagnostic metadata and output bytes.
    altered = json.loads((temporary / 'audit.json').read_text())
    altered['overlay_combinations'][0]['overlay_origin'][0] += 1
    (temporary / 'audit.json').write_text(json.dumps(altered))
    try:
        audit.main()
    except ValueError:
        probes.append('Changed placement metadata rejected')
    else:
        raise AssertionError('Placement tamper accepted')
    shutil.copyfile(root / 'audit.json', temporary / 'audit.json')
    name = next(iter(packet['boards']))
    path = temporary / name
    path.write_bytes(path.read_bytes() + b'tamper')
    try:
        audit.main()
    except ValueError:
        probes.append('Changed PNG bytes rejected even when decoded pixels match')
    else:
        raise AssertionError('Board tamper accepted')
    shutil.copyfile(root / name, path)
    frozen = audit.TILING / 'exports/packed-earth.png'
    original_digest = audit.digest
    audit.digest = lambda path: '0' * 64 if path == frozen else original_digest(path)
    try:
        audit.main()
    except ValueError:
        probes.append('Frozen accepted-artwork digest mismatch rejected before output writes')
    else:
        raise AssertionError('Frozen input mismatch accepted')
    audit.digest = original_digest
    with contextlib.redirect_stdout(io.StringIO()):
        for module in [audit, dressing, interfaces]:
            module.main()

for module in [audit, dressing, interfaces]:
    module.ROOT = root
# Verification preserves the historical human visual-review record instead of recertifying it.
review = json.loads((root / 'session-review.json').read_text())
audit.require(review['independent_exact_pixel_checks'] == checks, 'Review pixel-check coverage changed')
audit.require(review['byte_reproduction_files'] == len(outputs), 'Review output coverage changed')
audit.require(review['rejection_probes'] == probes, 'Review rejection coverage changed')
for name, expected_digest in review['reviewed_files'].items():
    audit.require(audit.digest(root / name) == expected_digest, f'Reviewed source changed: {name}')
for name, expected_digest in review['outputs'].items():
    audit.require(audit.digest(root / name) == expected_digest, f'Reviewed output changed: {name}')
print(json.dumps({'pixel_checks': checks, 'byte_reproduced_files': len(outputs), 'rejection_probes': probes}, indent=2))
