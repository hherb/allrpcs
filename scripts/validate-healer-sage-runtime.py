#!/usr/bin/env python3
"""Validate the Healer and Sage (Orin) runtime art against the owner-approved characters batch.

Each runtime frame must be byte-identical to the export that the characters-batch acceptance
record binds (idle/inspect from the initial batch, walks from the polished 8-phase rig), with
lossless imports, and the runtime folders must hold no other PNGs. Checks only these files, so
unrelated changes elsewhere cannot trip it (see issue #471).
"""
import hashlib
import json
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
CHARACTERS = ROOT / 'art-direction/characters-batch'
RUNTIME = ROOT / 'game/art/runtime/characters'
ROLES = ('healer', 'orin')
DIRECTIONS = ('south', 'west', 'east', 'north')
FRAME_COUNTS = {'idle': 2, 'walk': 8, 'inspect': 2}
# Import settings that change what renders: lossless, full size, no mipmaps, straight alpha with border fix.
IMPORT_SETTINGS = ('compress/mode=0', 'mipmaps/generate=false', 'process/fix_alpha_border=true',
                   'process/premult_alpha=false', 'process/size_limit=0')


def require(condition, message):
    """Fail with one actionable error."""
    if not condition:
        raise ValueError(message)


def sha(path):
    """Hash exact persisted bytes."""
    return hashlib.sha256(path.read_bytes()).hexdigest()


def expected_frames():
    """Return {acceptance key: runtime path relative to RUNTIME} for every frame this slice owns."""
    return {f'{role}-{direction}-{kind}-{index:02}': f'{role}/{direction}/{kind}-{index:02}.png'
            for role in ROLES for direction in DIRECTIONS
            for kind, count in FRAME_COUNTS.items() for index in range(1, count + 1)}


def validate():
    """Check acceptance bindings, runtime byte identity, import settings and no stray files."""
    acceptance = json.loads((CHARACTERS / 'owner-acceptance.json').read_text())
    require('active_assets' in acceptance, 'Characters acceptance record has no active_assets')
    assets = acceptance['active_assets']
    frames = expected_frames()
    for key, runtime in frames.items():
        require(key in assets, 'Frame is no longer an active approved asset: ' + key)
        export = CHARACTERS / assets[key]['export']
        require(sha(export) == assets[key]['sha256'], 'Approved export changed: ' + key)
        require((RUNTIME / runtime).read_bytes() == export.read_bytes(),
                'Runtime art differs from its approved export: ' + runtime)
        lines = (RUNTIME / (runtime + '.import')).read_text().splitlines()
        missing = [setting for setting in IMPORT_SETTINGS if setting not in lines]
        require(not missing, f'Runtime import settings changed for {runtime}: {missing}')
    pngs = {path.relative_to(RUNTIME).as_posix() for role in ROLES for path in (RUNTIME / role).rglob('*.png')}
    require(pngs == set(frames.values()), f'Unexpected runtime frames: {sorted(pngs - set(frames.values()))}')
    print(f'PASS: {len(frames)} Healer and Orin frames are byte-identical to their owner-approved exports with lossless imports')


if __name__ == '__main__':
    validate()
