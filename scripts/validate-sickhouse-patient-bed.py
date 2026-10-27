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
# Runtime file -> (approved export it must equal, package whose owner-acceptance.json binds it).
RUNTIME = {
    'game/art/runtime/objects/sickbed-01.png': ('exports/sickbed-01.png', PACKAGE),
    'game/art/runtime/characters/mira/dormant-01.png': ('exports/mira-dormant-01.png', CHARACTERS),
    'game/art/runtime/characters/mira/dormant-02.png': ('exports/mira-dormant-02.png', CHARACTERS),
}
# Import settings that change what renders: lossless, no mipmaps, straight alpha with border fix.
IMPORT_SETTINGS = ('compress/mode=0', 'mipmaps/generate=false', 'process/fix_alpha_border=true',
                   'process/premult_alpha=false')


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
    require(bed.get('approved') == 'sickbed-01', 'Owner approved a different bed')
    bindings = bed.get('bindings', {})
    require('export' in bindings, 'Bed acceptance no longer binds an export')
    for name, binding in bindings.items():
        require(sha(PACKAGE / binding['path']) == binding['sha256'], 'Approved bed file changed: ' + name)
    characters = json.loads((CHARACTERS / 'owner-acceptance.json').read_text()).get('active_assets', {})
    hashes = {bindings['export']['path']: bindings['export']['sha256']}
    for key in ('mira-dormant-01', 'mira-dormant-02'):
        require(key in characters, 'Mira dormant pose is no longer an active approved asset: ' + key)
        hashes[characters[key]['export']] = characters[key]['sha256']
    return hashes


def validate():
    """Check acceptance bindings, runtime byte identity and the runtime import settings."""
    hashes = approved_hashes()
    for runtime, (export, package) in RUNTIME.items():
        require(export in hashes, 'Approved export is not bound by its acceptance record: ' + export)
        require(sha(package / export) == hashes[export], 'Approved export changed: ' + export)
        require((ROOT / runtime).read_bytes() == (package / export).read_bytes(),
                'Runtime art differs from its approved export: ' + runtime)
        lines = (ROOT / (runtime + '.import')).read_text().splitlines()
        missing = [setting for setting in IMPORT_SETTINGS if setting not in lines]
        require(not missing, f'Runtime import settings changed for {runtime}: {missing}')
    print('PASS: sickhouse bed and Mira breathing poses are byte-identical to their owner-approved exports with lossless imports')


if __name__ == '__main__':
    validate()
