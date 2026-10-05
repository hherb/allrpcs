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
    require(owner == {'name': 'WagonGroundPresentation', 'type': 'Node2D', 'parent': '.',
                     'position': [1633, 1512], 'z_index': 0, 'z_as_relative': True,
                     'scale': [1, 1], 'rotation': 0, 'script': None}, 'Owner scope drift')
    require(asset['asset_id'] == 'terrain.wagon_entrance_ground' and asset['node_name'] == 'PackedEarth'
            and asset['dimensions'] == [285, 92] and asset['master_dimensions'] == [570, 184]
            and asset['z_index'] == -90 and asset['local_position'] == asset['offset'] == [0, 0]
            and asset['centered'] is False and asset['scale'] == [1, 1] and asset['rotation'] == 0
            and asset['region_enabled'] is False and asset['texture_filter'] == 'linear'
            and asset['texture_repeat'] == 'disabled' and asset['modulate'] == [1, 1, 1, 1], 'Asset scope drift')
    require(spec['phase'] == [0, 0] and spec['period'] == 128
            and spec['maximum_envelope'] == [1633, 1512, 1918, 1604], 'Phase/extent scope drift')
    require(spec['manifest_counts'] == {'before_assets': 93, 'before_masters': 44,
                                       'after_assets': 94, 'after_masters': 45}, 'Manifest scope drift')
    require(spec['import_parameters'] == {'compress/mode': '0', 'mipmaps/generate': 'false',
                                         'process/fix_alpha_border': 'false', 'process/premult_alpha': 'false',
                                         'process/channel_remap/red': '0', 'process/channel_remap/green': '1',
                                         'process/channel_remap/blue': '2', 'process/channel_remap/alpha': '3',
                                         'process/size_limit': '0'}, 'Import scope drift')
    proof = REPO / 'art-direction/wagon-ground-feasibility'
    acceptance = json.loads((proof / 'owner-selection.json').read_text())
    require(acceptance['status'] == 'owner selected feather-24 external composition', 'External acceptance missing')
    source = REPO / asset['source']
    require(asset['source'] == 'art-direction/wagon-ground-feasibility/proof-inputs/feather-24-layer.png'
            and asset['coverage'] == 'art-direction/wagon-ground-feasibility/proof-inputs/feather-24-coverage.png'
            and asset['world_paint'] == 'art-direction/wagon-ground-feasibility/proof-inputs/world-paint.png',
            'External source path drift')
    require(asset['production_export'] == 'art-direction/production-proof/corrected/exports/terrain/wagon-entrance-ground.png'
            and asset['production_master'] == 'art-direction/production-proof/corrected/masters/wagon-entrance-ground.ora'
            and asset['proof_texture'] == 'game/art/direction-a-import-proof/terrain/wagon-entrance-ground.png'
            and asset['runtime_texture'] == 'game/art/runtime/terrain/wagon-entrance-ground.png', 'Installation path drift')
    require(sha(source) == asset['source_sha256'] == acceptance['selected_files']['proof-inputs/feather-24-layer.png'],
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
