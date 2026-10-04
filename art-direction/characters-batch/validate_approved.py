#!/usr/bin/env python3
"""Validate the exact approved active cast, excluding superseded walking studies."""

import json

from build_review import REPO, ROOT, digest
from inspect_package import main as inspect_initial, require
from inspect_walk_polish import main as inspect_polished


def main():
    """Check existing package contracts and the combined owner-approved inventory."""
    inspect_initial()
    inspect_polished()
    approval = json.loads((ROOT / 'owner-acceptance.json').read_text())
    initial = json.loads((ROOT / 'inventory.json').read_text())
    polished = json.loads((ROOT / 'walk-rig-polish/inventory.json').read_text())
    active = {}
    for prefix, assets in [('', initial['assets']), ('walk-rig-polish/', polished['assets'])]:
        for asset in assets:
            if not prefix and '-walk-' in asset['id']:
                continue
            require(asset['id'] not in active, 'Duplicate active character asset')
            active[asset['id']] = {
                'export': prefix + asset['export'], 'sha256': asset['sha256'],
                'master': prefix + asset['master'], 'master_sha256': asset['master_sha256'],
            }
    require(len(active) == 456 and active == approval['active_assets'], 'Changed approved active cast')
    require(approval['runtime_promotion'] is False, 'Artwork acceptance cannot grant runtime authority')
    for asset in active.values():
        require(digest(ROOT / asset['export']) == asset['sha256'], 'Changed approved export')
        require(digest(ROOT / asset['master']) == asset['master_sha256'], 'Changed approved master')
    require(approval['inherited_seeker_frames'] == initial['inherited_seeker_frames'],
            'Changed inherited Seeker inventory')
    for frame in approval['inherited_seeker_frames']:
        require(digest(REPO / frame['path']) == frame['sha256'], 'Changed inherited Seeker frame')
    expected_review_sources = {
        str(path.relative_to(ROOT))
        for folder in ['sources', 'review', 'walk-rig-polish/review']
        for path in (ROOT / folder).iterdir()
        if path.is_file() and not (folder == 'review' and path.name.endswith('-walk.gif'))
    }
    require(set(approval['review_and_source_files']) == expected_review_sources,
            'Incomplete approved review/source inventory')
    for path, recorded_hash in approval['review_and_source_files'].items():
        require(digest(ROOT / path) == recorded_hash, f'Changed approved review/source: {path}')
    require(digest(ROOT / 'walk-rig-polish/owner-acceptance.json') == approval['walking_approval_sha256'],
            'Changed walking approval checkpoint')
    print('PASS: full approved character batch: 456 active export/master pairs and 48 inherited frames.')
    print('Artwork only; runtime imports, registration, camera, timing and physical authority remain separate.')


if __name__ == '__main__':
    main()
