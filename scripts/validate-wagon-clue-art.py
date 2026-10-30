#!/usr/bin/env python3
"""Validate the wagon clue runtime art against its owner-approved objects-batch exports.

Checks only the bytes this slice reuses or produces, never the whole tree, so later unrelated changes do not
trip it (see issue #471).
"""
import hashlib
import json
import sys
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
OBJECTS = ROOT / 'art-direction/objects-batch'
# The owner chose the broken strap (clue-objects-05) for the wagon clue.
ASSETS = ('clue-objects-05',)
SLICE = ROOT / 'art-direction/wagon-clue-art'
CAPTURE_SCRIPT = 'scripts/capture-wagon-clue-art.gd'
CAPTURES = {'captures/wagon-clue-seeker-south.png', 'captures/wagon-clue-seeker-north.png'}
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
    # Main.tscn is left out: it keeps changing after this slice and is pinned in verification.json.
    capture_inputs = {'res://../' + CAPTURE_SCRIPT: ROOT / CAPTURE_SCRIPT}
    for asset in ASSETS:
        export = f'exports/{asset}.png'
        runtime = f'game/art/runtime/objects/{asset}.png'
        approved = acceptance.get('approved_files', {}).get(export)
        require(approved is not None, 'The objects-batch acceptance no longer binds ' + export)
        export_bytes = (OBJECTS / export).read_bytes()
        require(hashlib.sha256(export_bytes).hexdigest() == approved, 'Approved export changed: ' + export)
        require((ROOT / runtime).read_bytes() == export_bytes, 'Runtime art differs from its approved export: ' + runtime)
        lines = (ROOT / (runtime + '.import')).read_text().splitlines()
        missing = [setting for setting in IMPORT_SETTINGS if setting not in lines]
        require(not missing, f'Runtime import settings changed for {runtime}: {missing}')
        capture_inputs[f'res://art/runtime/objects/{asset}.png'] = ROOT / runtime
    evidence = json.loads((SLICE / 'capture-evidence.json').read_text())
    for name, path in capture_inputs.items():
        require(evidence['inputs'].get(name) == sha256(path), 'Capture input drift: ' + name)
    require(set(evidence['captures']) == CAPTURES, 'Capture set drift')
    for name, expected in evidence['captures'].items():
        require(sha256(SLICE / name) == expected, 'Capture drift: ' + name)
    print('PASS: wagon clue art is byte-identical to the owner-approved clue-objects-05 export with a lossless import;'
          ' its captures match their evidence')


if __name__ == '__main__':
    try:
        validate()
    except (ValueError, KeyError, OSError) as error:
        sys.exit(f'FAIL: {error}')
