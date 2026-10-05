#!/usr/bin/env python3
"""Validate the external successor without modifying any frozen predecessor checker."""

import importlib.util
import json
import subprocess
import sys

from check import REPO, ROOT, require, sha


def check():
    """Prove the entire published packet unchanged, then rerun its independent checks."""
    baseline = json.loads((ROOT / 'baseline.json').read_text())
    documents = {'HANDOVER.md', 'README.md', 'ROADMAP.md'}
    for name, digest in baseline['files'].items():
        if name not in documents:
            require((REPO / name).is_file() and sha(REPO / name) == digest, 'Predecessor drift: ' + name)
    names = subprocess.check_output(['git', 'diff', baseline['head'], '--name-only'], cwd=REPO, text=True).splitlines()
    names += subprocess.check_output(['git', 'ls-files', '--others', '--exclude-standard'],
                                     cwd=REPO, text=True).splitlines()
    additions = {'art-direction/laundry-ground-feasibility/owner-selection.json',
                 'art-direction/laundry-ground-feasibility/evidence/validation.json'}
    require(all(name in documents or name in additions
                or name.startswith('art-direction/laundry-ground-runtime-scope/') for name in names),
            'Change outside external selection/scope proposal')
    path = REPO / 'art-direction/laundry-ground-feasibility/inspect_package.py'
    spec = importlib.util.spec_from_file_location('frozen_laundry_inspector', path)
    inspector = importlib.util.module_from_spec(spec)
    sys.path.insert(0, str(path.parent))
    try:
        spec.loader.exec_module(inspector)
    finally:
        sys.path.pop(0)
    require(inspector.ROOT == path.parent, 'Frozen inspector imported the wrong build module')
    inspector.recipe()
    support, masks = inspector.expected_masks()
    layers = inspector.artwork(masks)
    planes = inspector.separation(support)
    comparisons = inspector.captures(support)
    return {'status': 'passed', 'preserved_predecessor_files': len(baseline['files']) - len(documents),
            'master_layers_checked': layers, 'protected_alpha_planes': planes,
            'gpu_comparisons': comparisons,
            'membership_policy': 'Exact live baseline plus external selection/evidence/scope additions only; frozen checker unchanged'}


if __name__ == '__main__':
    print(json.dumps(check(), indent=2))
