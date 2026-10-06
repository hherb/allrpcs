#!/usr/bin/env python3
"""Independently verify scope, capture bindings and inherited product-quality evidence."""

import hashlib
import json
from pathlib import Path
import subprocess

ROOT = Path(__file__).resolve().parent
REPO = ROOT.parents[1]
PREFIX = str(ROOT.relative_to(REPO)) + '/'
DOCUMENTS = {'README.md', 'ROADMAP.md', 'HANDOVER.md'}


def require(condition, message):
    """Reject evidence drift even with optimized Python."""
    if not condition:
        raise ValueError(message)


def sha(path):
    """Hash bytes, without modifying source evidence."""
    return hashlib.sha256(path.read_bytes()).hexdigest()


def resolve_capture_input(name):
    """Resolve actual capture keys; reject paths outside the repository."""
    path = REPO / 'game' / name.removeprefix('res://') if name.startswith('res://') else Path(name)
    require(path.resolve().is_relative_to(REPO), 'Capture input outside repository')
    return path


def main():
    """Check every predecessor, all six captures, and terminal Village Edge acceptance."""
    baseline = json.loads((ROOT / 'baseline.json').read_text())
    # Verify the baseline inventory against Git itself, not only its working-tree copy.
    tracked = subprocess.check_output(['git', 'ls-tree', '-r', '--name-only', baseline['head']],
                                      cwd=REPO, text=True).splitlines()
    require(set(tracked) == set(baseline['files']), 'Incomplete predecessor inventory')
    for name, digest in baseline['files'].items():
        if name not in DOCUMENTS:
            require(sha(REPO / name) == digest, 'Predecessor drift: ' + name)
    changed = subprocess.check_output(['git', 'diff', baseline['head'], '--name-only'],
                                      cwd=REPO, text=True).splitlines()
    untracked = subprocess.check_output(['git', 'ls-files', '--others', '--exclude-standard'],
                                        cwd=REPO, text=True).splitlines()
    require(all(name in DOCUMENTS or name.startswith(PREFIX) for name in changed + untracked),
            'Change outside external audit scope')
    require(not any(name.endswith('.DS_Store') for name in changed + untracked), 'Unwanted Finder metadata')
    inputs = json.loads((ROOT / 'inputs.json').read_text())['sources']
    require({name: digest for name, digest in baseline['files'].items() if name.startswith('game/')} ==
            {name: digest for name, digest in inputs.items() if name.startswith('game/')},
            'Capture inputs omit a tracked game source')
    captures = json.loads((ROOT / 'capture-evidence.json').read_text())
    for name, digest in captures['inputs'].items():
        require(sha(resolve_capture_input(name)) == digest, 'Capture input drift: ' + name)
    for name, digest in inputs.items():
        require(captures['inputs'].get(str(REPO / name)) == digest, 'Unbound capture source: ' + name)
    expected = {f'captures/baseline-{zoom:.1f}x-{frame}.png'
                for zoom in (0.8, 1.0, 2.0) for frame in ('wide', 'tall')}
    require(set(captures['captures']) == set(captures['frames']) == expected, 'Incomplete capture matrix')
    for name, digest in captures['captures'].items():
        require(sha(ROOT / name) == digest, 'Changed capture: ' + name)
    audit = json.loads((ROOT / 'audit.json').read_text())
    for name, digest in audit['outputs'].items():
        require(sha(ROOT / name) == digest, 'Changed review output: ' + name)
    for name, digest in audit['sources'].items():
        path = Path(name) if Path(name).is_absolute() else ROOT / name
        require(sha(path) == digest, 'Changed board source: ' + name)
    require(audit['baseline_head'] == baseline['head'], 'Audit head mismatch')
    predecessor = REPO / 'art-direction/village-edge-ground-runtime-promotion'
    acceptance = json.loads((predecessor / 'owner-runtime-acceptance.json').read_text())
    require(acceptance['status'].startswith('accepted exact nine actual runtime views'),
            'Terminal Village Edge acceptance missing')
    require(sha(predecessor / 'runtime-review-request.json') == acceptance['runtime_review_request_sha256'], 'Acceptance request drift')
    for group in ('bindings_sha256', 'actual_views', 'displayed_series'):
        for name, digest in acceptance[group].items():
            require(sha(predecessor / name) == digest, 'Acceptance evidence drift: ' + name)
    for name, digest in acceptance['implementation_sha256'].items():
        require(sha(REPO / name) == digest, 'Accepted implementation drift: ' + name)
    quality = json.loads((predecessor / 'verification.json').read_text())
    require(quality['test_total'] == sum(quality['test_suites'].values()) == 5033
            and quality['build_warnings'] == quality['build_errors'] == 0
            and quality['quality']['host_retry_exit'] == 0,
            'Inherited verification incomplete')
    for name, digest in quality['evidence_sha256'].items():
        require(sha(predecessor / name) == digest, 'Inherited evidence drift: ' + name)
    require(audit['ground_alpha_extrema'] == [255, 255], 'Coverage changed')
    print(f'PASS: {len(baseline["files"]) - len(DOCUMENTS)} unchanged predecessor files; '
          'complete six-view capture; accepted implementation and inherited 5033-test evidence')


if __name__ == '__main__':
    main()
