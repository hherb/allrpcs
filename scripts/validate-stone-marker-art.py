#!/usr/bin/env python3
"""Validate the stone marker runtime art against its owner-approved objects-batch export.

Checks only the bytes this slice reuses, never the whole tree, so later unrelated changes do not
trip it (see issue #471).
"""
import hashlib
import json
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
OBJECTS = ROOT / 'art-direction/objects-batch'
EXPORT = 'exports/inscriptions-01.png'
RUNTIME = 'game/art/runtime/objects/inscriptions-01.png'
# Import settings that change what renders: lossless, full size, no mipmaps, straight alpha with border fix.
IMPORT_SETTINGS = ('compress/mode=0', 'mipmaps/generate=false', 'process/fix_alpha_border=true',
                   'process/premult_alpha=false', 'process/size_limit=0')


def require(condition, message):
    """Fail with one actionable error."""
    if not condition:
        raise ValueError(message)


def validate():
    """Check the acceptance binding, runtime byte identity and the runtime import settings."""
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
    print('PASS: stone marker art is byte-identical to the owner-approved inscriptions-01 export with a lossless import')


if __name__ == '__main__':
    validate()
