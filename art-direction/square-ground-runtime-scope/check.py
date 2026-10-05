#!/usr/bin/env python3
"""Check the proposal's exact source bytes and selected external acceptance read-only."""

import hashlib
import json
from pathlib import Path

ROOT = Path(__file__).resolve().parent
REPO = ROOT.parents[1]


def require(condition, message):
    """Reject any unreviewed source or proposed ownership drift."""
    if not condition:
        raise ValueError(message)


def sha(path):
    """Bind exact files, including masks and historical diagnostics."""
    return hashlib.sha256(path.read_bytes()).hexdigest()


def main():
    """Verify source hashes and proposal geometry without creating product files."""
    spec = json.loads((ROOT / 'source-bindings.json').read_text())
    require(spec['status'] == 'proposal only; named runtime scope approval absent', 'Unexpected proposal status')
    for name, expected in spec['source_sha256'].items():
        require((REPO / name).is_file() and sha(REPO / name) == expected, 'Scope input drift: ' + name)
    owner, asset = spec['owner'], spec['asset']
    require(owner == {'name': 'SquareGroundPresentation', 'type': 'Node2D', 'parent': '.',
                     'position': [1920, 620], 'z_index': 0, 'z_as_relative': True,
                     'scale': [1, 1], 'rotation': 0, 'script': None}, 'Owner scope drift')
    require(asset['asset_id'] == 'terrain.village_square_ground' and asset['node_name'] == 'PackedEarth'
            and asset['dimensions'] == [560, 560] and asset['master_dimensions'] == [1120, 1120]
            and asset['z_index'] == -90 and asset['local_position'] == asset['offset'] == [0, 0]
            and asset['centered'] is False and asset['scale'] == [1, 1] and asset['rotation'] == 0
            and asset['region_enabled'] is False and asset['texture_filter'] == 'linear'
            and asset['texture_repeat'] == 'disabled' and asset['modulate'] == [1, 1, 1, 1], 'Asset scope drift')
    require(spec['phase'] == [0, 0] and spec['period'] == 128
            and spec['maximum_envelope'] == [1920, 620, 2480, 1180], 'Phase/extent scope drift')
    require(spec['manifest_counts'] == {'before_assets': 91, 'before_masters': 42,
                                       'after_assets': 92, 'after_masters': 43}, 'Manifest scope drift')
    require(spec['import_parameters'] == {'compress/mode': '0', 'mipmaps/generate': 'false',
                                         'process/fix_alpha_border': 'false', 'process/premult_alpha': 'false',
                                         'process/channel_remap/red': '0', 'process/channel_remap/green': '1',
                                         'process/channel_remap/blue': '2', 'process/channel_remap/alpha': '3',
                                         'process/size_limit': '0'}, 'Import scope drift')
    proof = REPO / 'art-direction/square-ground-feasibility'
    acceptance = json.loads((proof / 'owner-acceptance.json').read_text())
    require(acceptance['status'] == 'feathered external composition approved', 'External acceptance missing')
    source = REPO / asset['source']
    require(sha(source) == asset['source_sha256'] == acceptance['selected_files']['proof-inputs/feathered-layer.png'],
            'Selected runtime source drift')
    require(asset['source_sha256'] in (ROOT / 'README.md').read_text(), 'README digest mismatch')
    for field in ('production_export', 'production_master', 'proof_texture', 'runtime_texture'):
        require(not (REPO / asset[field]).exists(), 'Unexpected installation before named runtime approval: ' + field)
    review = ROOT / 'session-review.json'
    if review.exists():
        saved = json.loads(review.read_text())
        for name, expected in saved['proposal_sha256'].items():
            require(sha(ROOT / name) == expected, 'Reviewed proposal drift: ' + name)
    print('PASS:', len(spec['source_sha256']), 'bound inputs; exact one-asset proposal; no runtime installation')


if __name__ == '__main__':
    main()
