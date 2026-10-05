#!/usr/bin/env python3
"""Enforce the exact proposal, artwork selection and unchanged runtime predecessors."""

import argparse
from copy import deepcopy
import hashlib
import json
from pathlib import Path
import subprocess

ROOT = Path(__file__).resolve().parent
REPO = ROOT.parents[1]
PROOF = REPO / 'art-direction/village-edge-ground-feasibility'
PREFIX = str(ROOT.relative_to(REPO)) + '/'
SELECTION = str((PROOF / 'owner-selection.json').relative_to(REPO))
DOCS = {'HANDOVER.md', 'README.md', 'ROADMAP.md'}


def require(condition, message):
    """Reject drift even with Python optimization enabled."""
    if not condition:
        raise ValueError(message)


def sha(path):
    """Hash exact persisted bytes."""
    return hashlib.sha256(path.read_bytes()).hexdigest()


def metadata(record):
    """Accept one script-free independent owner and the selected three-entrance layer."""
    require(record['status'] == 'proposal only; named runtime approval absent', 'Scope status changed')
    require(record['owner'] == {'name': 'VillageEdgeGroundPresentation', 'type': 'Node2D', 'parent': '.',
                               'position': [1480, 620], 'z_index': 0, 'z_as_relative': True,
                               'scale': [1, 1], 'rotation': 0, 'script': None}, 'Owner drift')
    require(record['asset'] == {'asset_id': 'terrain.village_edge_entrance_ground', 'node_name': 'PackedEarth',
                               'variant': 'feather-16', 'source': 'proof-inputs/feather-16-layer.png',
                               'coverage': 'proof-inputs/feather-16-coverage.png', 'master': 'masters/feather-16.ora',
                               'dimensions': [500, 390], 'master_dimensions': [1000, 780],
                               'local_position': [0, 0], 'offset': [0, 0], 'centered': False,
                               'scale': [1, 1], 'rotation': 0, 'z_index': -78, 'z_as_relative': True,
                               'region_enabled': False, 'modulate': [1, 1, 1, 1],
                               'texture_filter': 'linear', 'texture_repeat': 'disabled',
                               'world_phase': [0, 0], 'period': 128}, 'Asset recipe drift')
    require(record['counts'] == {'before': [96, 47], 'after': [97, 48]}, 'Inventory count drift')
    require(record['root_order'] == 'after accepted Laundry roots; before final WholeChannelPresentation',
            'Sibling order drift')
    require(record['runtime_confirmation'] == 'nine actual views and nine paired baselines; separate owner confirmation',
            'Runtime confirmation gate drift')


def inspect():
    """Bind proposal hashes and every published predecessor outside continuation docs."""
    record = json.loads((ROOT / 'source-bindings.json').read_text())
    metadata(record)
    require(record['scope_readme_sha256'] == sha(ROOT / 'README.md'), 'Proposal prose drift')
    require(record['checker_sha256'] == sha(ROOT / 'check.py'), 'Checker drift')
    baseline = json.loads((ROOT / 'baseline.json').read_text())
    require(record['prepared_against_commit'] == baseline['head'], 'Baseline head drift')
    tracked = subprocess.check_output(['git', 'ls-tree', '-r', '--name-only', baseline['head']],
                                      cwd=REPO, text=True).splitlines()
    require(set(tracked) == set(baseline['files']), 'Incomplete predecessor inventory')
    for name, digest in baseline['files'].items():
        if name not in DOCS:
            require(sha(REPO / name) == digest, 'Published predecessor drift: ' + name)
    changes = subprocess.check_output(['git', 'diff', baseline['head'], '--name-only'],
                                      cwd=REPO, text=True).splitlines()
    untracked = subprocess.check_output(['git', 'ls-files', '--others', '--exclude-standard'],
                                        cwd=REPO, text=True).splitlines()
    require(all(name in DOCS or name == SELECTION or name.startswith(PREFIX) for name in changes + untracked),
            'Change outside approval/proposal scope')
    approval = json.loads((PROOF / 'owner-selection.json').read_text())
    require(approval['owner_statement'] == 'feather-16 approved all around'
            and approval['selected_variant'] == 'feather-16'
            and approval['entrances'] == ['woodland', 'stream', 'square'], 'Selection mismatch')
    for section in ('selected_files', 'displayed_series', 'evidence_sha256'):
        for name, digest in approval[section].items():
            require(sha(PROOF / name) == digest, 'Selected evidence drift: ' + name)
    for name, digest in record['bound_files'].items():
        require(sha(REPO / name) == digest, 'Scope binding drift: ' + name)
    require(record['bound_files'].get(SELECTION) == sha(PROOF / 'owner-selection.json'), 'Unbound approval')
    print(f'PASS: exact feather-16 scope; {len(baseline["files"]) - len(DOCS)} unchanged published predecessors')
    return record


def main():
    """Check the proposal; optionally reject seven disposable metadata mutations."""
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--check-rejections', action='store_true')
    args = parser.parse_args()
    record = inspect()
    if args.check_rejections:
        mutations = [('asset', 'variant', 'feather-24'), ('asset', 'world_phase', [1, 0]),
                     ('asset', 'z_index', -90), ('asset', 'centered', True),
                     ('asset', 'texture_filter', 'nearest'), ('owner', 'position', [1481, 620]),
                     ('owner', 'script', 'runtime.gd')]
        for section, key, value in mutations:
            candidate = deepcopy(record)
            candidate[section][key] = value
            try:
                metadata(candidate)
            except ValueError:
                continue
            raise ValueError('Mutation accepted: ' + key)
        print('PASS: seven wrong-variant/phase/depth/transform/sampling/owner probes rejected')


if __name__ == '__main__':
    main()
