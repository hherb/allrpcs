#!/usr/bin/env python3
"""Validate the sickhouse bed/patient runtime art against its approved sources.

Checks only the bytes this slice owns or reuses, never the whole tree, so later unrelated
changes do not trip it (see issue #471).
"""
import hashlib
import json
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
PACKAGE = ROOT / 'art-direction/sickhouse-patient-bed'
CHARACTERS = ROOT / 'art-direction/characters-batch'
# Runtime file -> (approved export it must equal, acceptance record that binds that export).
RUNTIME = {
    'game/art/runtime/objects/sickbed-01.png': ('exports/sickbed-01.png', PACKAGE),
    'game/art/runtime/characters/mira/dormant-01.png': ('exports/mira-dormant-01.png', CHARACTERS),
    'game/art/runtime/characters/mira/dormant-02.png': ('exports/mira-dormant-02.png', CHARACTERS),
}


def require(condition, message):
    """Fail with one actionable error."""
    if not condition:
        raise ValueError(message)


def sha(path):
    """Hash exact persisted bytes."""
    return hashlib.sha256(path.read_bytes()).hexdigest()


def approved_hashes():
    """Return {package-relative export path: sha256} from both owner acceptance records."""
    bed = json.loads((PACKAGE / 'owner-acceptance.json').read_text())
    require(bed['approved'] == 'sickbed-01', 'Owner approved a different bed')
    for name, binding in bed['bindings'].items():
        require(sha(PACKAGE / binding['path']) == binding['sha256'], 'Approved bed file changed: ' + name)
    characters = json.loads((CHARACTERS / 'owner-acceptance.json').read_text())['active_assets']
    hashes = {bed['bindings']['export']['path']: bed['bindings']['export']['sha256']}
    for key in ('mira-dormant-01', 'mira-dormant-02'):
        hashes[characters[key]['export']] = characters[key]['sha256']
    return hashes


def validate():
    """Check acceptance bindings (source, master, export, review) and runtime byte identity."""
    hashes = approved_hashes()
    for runtime, (export, package) in RUNTIME.items():
        require(sha(package / export) == hashes[export], 'Approved export changed: ' + export)
        require((ROOT / runtime).read_bytes() == (package / export).read_bytes(),
                'Runtime art differs from its approved export: ' + runtime)
    print('PASS: sickhouse bed and Mira breathing poses are byte-identical to their owner-approved exports')


if __name__ == '__main__':
    validate()
