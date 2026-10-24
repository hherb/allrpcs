#!/usr/bin/env python3
"""Validate the authorised screenshot successor while retaining the complete building proof."""
import gzip
import hashlib
import json
from pathlib import Path
import subprocess

from PIL import Image

ROOT = Path(__file__).resolve().parents[1]
PACKAGE = ROOT / 'art-direction/gameplay-view-repairs'
BASE = json.loads((PACKAGE / 'authority.json').read_text())['base_commit']


def require(value, message):
    """Reject drift with a concrete successor-boundary error."""
    if not value:
        raise ValueError(message)


def sha(path):
    """Hash persisted source bytes."""
    return hashlib.sha256(path.read_bytes()).hexdigest()


def original(name):
    """Read the verified building checkpoint independently of mutable files."""
    return subprocess.check_output(['git', 'show', f'{BASE}:{name}'], cwd=ROOT)


def validate():
    """Preserve the predecessor and permit only two new presentation owners in Main."""
    previous = ROOT / 'art-direction/buildings-gameplay-integration'
    record = json.loads((previous / 'verification.json').read_text())
    summary_path = previous / record['full_quality']['summary']
    require(sha(summary_path) == record['full_quality']['summary_sha256'], 'Prior quality evidence changed')
    summary = json.loads(gzip.decompress(summary_path.read_bytes()))
    require(summary['status'] == 'passed' and len(summary['stages']) == 218, 'Missing complete predecessor gate')
    allowed = {'game/Main.tscn', 'scripts/smoke-godot.sh'}
    for name, expected in record['inputs'].items():
        require(hashlib.sha256(original(name)).hexdigest() == expected, 'Predecessor source binding changed')
        if name not in allowed:
            require(sha(ROOT / name) == expected, 'Verified building source changed: ' + name)
    current = (ROOT / 'game/Main.tscn').read_text()
    added = ('[ext_resource type="Script" path="res://GameplaySettingsLayout.cs" id="gameplay_settings_layout"]\n'
             '[ext_resource type="Script" path="res://RetainedStreamCrossingPresentation.cs" id="retained_stream_presentation"]\n\n')
    node = ('[node name="RetainedStreamCrossingPresentation" type="Node" parent="."]\n'
            'script = ExtResource("retained_stream_presentation")\n\n')
    script = ('[node name="SettingsOverlay" type="CanvasLayer" parent="."]\nvisible = false\nlayer = 6\n'
              'script = ExtResource("gameplay_settings_layout")')
    require(current.count(added) == current.count(node) == current.count(script) == 1,
            'Missing or duplicate settings/crossing owners')
    restored = current.replace(added, '', 1).replace(node, '', 1).replace(
        script, script.removesuffix('\nscript = ExtResource("gameplay_settings_layout")'), 1)
    restored = restored.replace('load_steps=246', 'load_steps=244', 1)
    require(restored.encode() == original('game/Main.tscn'), 'Unscoped scene, route or physical change')
    names = subprocess.check_output(['git', 'diff', BASE, '--name-only'], cwd=ROOT, text=True).splitlines()
    require(not any(name.startswith('src/') for name in names), 'Gameplay/domain source changed')
    require(not any(name.startswith('art-direction/') and not name.startswith('art-direction/gameplay-view-repairs/')
                    for name in names), 'Predecessor art/evidence changed')
    require(not any(name.startswith('game/art/') for name in names), 'Existing runtime artwork changed')
    allowed_game = {'game/Main.tscn', 'game/GameplaySettingsLayout.cs', 'game/RetainedStreamCrossingPresentation.cs'}
    require(all(not name.startswith('game/') or name in allowed_game for name in names),
            'Existing gameplay, geometry or character source changed')
    # Every sampled UV is inside fully opaque approved timber paint, so the retired fill cannot leak through.
    import math
    paint = Image.open(ROOT / 'game/art/runtime/terrain/channel-decks.png').convert('RGBA')
    length = math.hypot(700, 550)
    along, across = (700 / length, 550 / length), (-550 / length, 700 / length)
    for y in range(129):
        for x in range(129):
            a, b = (x / 128 * 2 - 1) * 44, (y / 128 * 2 - 1) * 64
            point = (round(1382.1818 + across[0] * a + along[0] * b),
                     round(388 + across[1] * a + along[1] * b))
            require(paint.getpixel(point)[3] == 255, 'Crossing UV enters transparent source pixels')
    report = PACKAGE / 'capture-results.json'
    if report.exists():
        captures = json.loads(report.read_text())
        for name, expected in captures['captures'].items():
            require(sha(PACKAGE / name) == expected, 'UI-visible capture changed: ' + name)
        for name, expected in captures.get('inputs', {}).items():
            path = (ROOT / 'game' / name.removeprefix('res://')).resolve()
            require(path.is_relative_to(ROOT) and sha(path) == expected, 'Capture input changed: ' + name)
    print('PASS: screenshot successor preserves verified building/art evidence, source pixels, physics and scene ownership; approved crossing timber is opaque')


if __name__ == '__main__':
    validate()
