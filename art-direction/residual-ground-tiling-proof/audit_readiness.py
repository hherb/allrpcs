#!/usr/bin/env python3
"""Read-only compatibility measurements; never repair or rebuild accepted artwork."""

import json
from pathlib import Path

import numpy as np
from PIL import Image

ROOT = Path(__file__).resolve().parent
REPO = ROOT.parents[1]


def sha256(path):
    """Bind exact inputs without importing mutable authoring code."""
    import hashlib
    return hashlib.sha256(path.read_bytes()).hexdigest()


def main():
    """Measure direct edge compatibility between new fields and retained legacy grounds."""
    acceptance = json.loads((ROOT / 'owner-acceptance.json').read_text())
    if acceptance['status'] != 'approved' or sha256(ROOT / 'review-manifest.json') != acceptance['review_manifest_sha256']:
        raise ValueError('Accepted tiling packet changed')
    review = json.loads((ROOT / 'review-manifest.json').read_text())
    if sha256(ROOT / 'inventory.json') != review['files']['inventory.json']:
        raise ValueError('Accepted tiling inventory changed')
    production_root = REPO / 'art-direction/production-proof/corrected'
    manifest = json.loads((production_root / 'asset-manifest.json').read_text())
    inventory = json.loads((ROOT / 'inventory.json').read_text())
    manifest_path = 'art-direction/production-proof/corrected/asset-manifest.json'
    if sha256(production_root / 'asset-manifest.json') != inventory['predecessors'][manifest_path]:
        raise ValueError('Retained production manifest changed')
    for path, expected in acceptance['approved_artwork'].items():
        if sha256(ROOT / path) != expected:
            raise ValueError(f'Accepted artwork changed: {path}')
    legacy = [asset for asset in manifest['assets'] if asset['id'] in
              ('terrain.quiet_moss', 'terrain.woodland_leaf_litter')]
    if len(legacy) != 2:
        raise ValueError('Expected both retained legacy ground materials')
    rows = []
    for old in legacy:
        path = production_root / old['export']
        if sha256(path) != old['sha256']:
            raise ValueError('Retained manifest terrain changed')
        with Image.open(path) as image:
            if image.size != (256, 256):
                raise ValueError('Legacy repeat period changed')
            pixels = np.asarray(image.convert('RGB'), dtype=int)
        for new in (asset for asset in inventory['assets'] if asset['family'] == 'surfaces'):
            with Image.open(ROOT / new['export']) as image:
                tiled = np.tile(np.asarray(image.convert('RGB'), dtype=int), (2, 2, 1))
            vertical = np.abs(pixels[:, -1] - tiled[:, 0])
            horizontal = np.abs(pixels[-1] - tiled[0])
            rows.append({'retained': old['id'], 'candidate': new['id'],
                         'retained_sha256': old['sha256'], 'candidate_sha256': new['export_sha256'],
                         'retained_export_size': [256, 256], 'candidate_export_size': [128, 128],
                         'vertical_join_rgb_max_delta': int(vertical.max()),
                         'vertical_join_rgb_mean_delta': round(float(vertical.mean()), 2),
                         'horizontal_join_rgb_max_delta': int(horizontal.max()),
                         'horizontal_join_rgb_mean_delta': round(float(horizontal.mean()), 2),
                         'exact_shared_paint_port': bool(not vertical.any() and not horizontal.any())})
    report = {'date': '2026-10-04', 'timezone': 'Australia/Brisbane',
              'scope': 'Read-only terrain-readiness measurements after accepted tiling proof',
              'owner_acceptance_sha256': sha256(ROOT / 'owner-acceptance.json'),
              'production_manifest_sha256': sha256(production_root / 'asset-manifest.json'),
              'comparisons': rows,
              'interpretation': 'Different materials are expected to differ. These direct joins are not source-matched transitions; measurements neither require every pairing nor establish visible failure.',
              'remaining': ['Accepted variation overlays have not been composited against all new repeatable fields at 1:1',
                            'New 128px fields and legacy 256px grounds do not share exact direct edge paint',
                            'Required mixed-material connectors need external composition evidence before a new artwork scope',
                            'Runtime import settings, placement and visual acceptance remain separate named gates'],
              'next_slice': 'Separate read-only external compatibility boards: all six accepted variation overlays on all six new fields, plus new/legacy material adjacency at 1:1; preserve all accepted artwork and do not invent transition requirements or runtime placement.'}
    (ROOT / 'readiness.json').write_text(json.dumps(report, indent=2) + '\n')
    print(f'PASS: {len(rows)} source-bound direct-edge comparisons; no accepted artwork modified.')


if __name__ == '__main__':
    main()
