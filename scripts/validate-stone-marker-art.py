#!/usr/bin/env python3
"""Validate the stone marker runtime art against its owner-approved objects-batch export.

Checks only the bytes this slice reuses, never the whole tree, so later unrelated changes do not
trip it (see issue #471).
"""
import hashlib
import json
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
OBJECTS = ROOT / 'art-direction/objects-batch'
EXPORT = 'exports/inscriptions-01.png'
RUNTIME = 'game/art/runtime/objects/inscriptions-01.png'
SLICE = ROOT / 'art-direction/stone-marker-art'
# Main.tscn is left out: it keeps changing after this slice and is pinned in verification.json.
CAPTURE_INPUTS = {'res://art/runtime/objects/inscriptions-01.png': ROOT / RUNTIME,
                  'res://../scripts/capture-stone-marker-art.gd': ROOT / 'scripts/capture-stone-marker-art.gd'}
# Import settings that change what renders: lossless, full size, no mipmaps, straight alpha with border fix.
IMPORT_SETTINGS = ('compress/mode=0', 'mipmaps/generate=false', 'process/fix_alpha_border=true',
                   'process/premult_alpha=false', 'process/size_limit=0')


def require(condition, message):
    """Fail with one actionable error."""
    if not condition:
        raise ValueError(message)


def sha256(path):
    """Hex SHA-256 of a file's bytes."""
    return hashlib.sha256(path.read_bytes()).hexdigest()


def validate():
    """Check the acceptance binding, runtime byte identity, the runtime import settings and the captures."""
    acceptance = json.loads((OBJECTS / 'owner-acceptance.json').read_text())
    require(acceptance.get('status') == 'approved', 'The objects batch is no longer approved')
    approved = acceptance.get('approved_files', {}).get(EXPORT)
    require(approved is not None, 'The objects-batch acceptance no longer binds ' + EXPORT)
    export = (OBJECTS / EXPORT).read_bytes()
    require(hashlib.sha256(export).hexdigest() == approved, 'Approved export changed: ' + EXPORT)
    require((ROOT / RUNTIME).read_bytes() == export, 'Runtime art differs from its approved export: ' + RUNTIME)
    lines = (ROOT / (RUNTIME + '.import')).read_text().splitlines()
    missing = [setting for setting in IMPORT_SETTINGS if setting not in lines]
    require(not missing, f'Runtime import settings changed for {RUNTIME}: {missing}')
    evidence = json.loads((SLICE / 'capture-evidence.json').read_text())
    for name, path in CAPTURE_INPUTS.items():
        require(evidence['inputs'].get(name) == sha256(path), 'Capture input drift: ' + name)
    require(set(evidence['captures']) == {'captures/stone-marker-seeker-south.png',
                                          'captures/stone-marker-seeker-north.png'}, 'Capture set drift')
    for name, expected in evidence['captures'].items():
        require(sha256(SLICE / name) == expected, 'Capture drift: ' + name)
    print('PASS: stone marker art is byte-identical to the owner-approved inscriptions-01 export with a lossless import;'
          ' its captures match their evidence')


if __name__ == '__main__':
    try:
        validate()
    except (ValueError, KeyError, OSError) as error:
        sys.exit(f'FAIL: {error}')
