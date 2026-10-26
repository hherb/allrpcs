#!/usr/bin/env python3
"""Validate the authorised screenshot successor while retaining the complete building proof."""
import gzip
import hashlib
import json
from pathlib import Path
import subprocess

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
    """Preserve the predecessor and permit only the two presentation owners and moved review entry in Main."""
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
    # The evidence review entry, list and close button move together from x=880 to beside Save/Load.
    start = restored.index('[node name="TemporaryCharacterDialogueMisquotationEvidenceReviewPanel"')
    end = restored.index('[node name="CharacterDialogueMisquotationEvidencePresentationSelectionController"')
    panel = restored[start:end]
    require(panel.count('offset_left = 960.0\n') == panel.count('offset_right = 1152.0\n') == 3,
            'Evidence review controls are not moved together')
    panel = panel.replace('offset_left = 960.0\n', 'offset_left = 880.0\n').replace(
        'offset_right = 1152.0\n', 'offset_right = 1072.0\n')
    restored = restored[:start] + panel + restored[end:]
    require(restored.encode() == original('game/Main.tscn'), 'Unscoped scene, route or physical change')
    names = subprocess.check_output(['git', 'diff', BASE, '--name-only'], cwd=ROOT, text=True).splitlines()
    require(not any(name.startswith('src/') for name in names), 'Gameplay/domain source changed')
    require(not any(name.startswith('art-direction/') and not name.startswith('art-direction/gameplay-view-repairs/')
                    for name in names), 'Predecessor art/evidence changed')
    require(not any(name.startswith('game/art/') for name in names), 'Existing runtime artwork changed')
    # git diff ignores untracked files, so also reject stray files in fenced trees.
    untracked = subprocess.check_output(['git', 'ls-files', '--others', '--exclude-standard', '--', 'src', 'game/art',
                                         'art-direction'], cwd=ROOT, text=True).splitlines()
    require(all(name.startswith('art-direction/gameplay-view-repairs/') for name in untracked),
            'Untracked file in a fenced source/art tree')
    allowed_game = {'game/Main.tscn', 'game/GameplaySettingsLayout.cs', 'game/RetainedStreamCrossingPresentation.cs'}
    require(all(not name.startswith('game/') or name in allowed_game for name in names),
            'Existing gameplay, geometry or character source changed')
    # Deck opacity under the live UVs is asserted by scripts/check-gameplay-view-repairs.gd.
    captures = json.loads((PACKAGE / 'capture-results.json').read_text())
    require(captures['status'] == 'passed' and len(captures['captures']) == 8 and captures['inputs'],
            'Incomplete UI-visible capture series')
    for name, expected in captures['captures'].items():
        require(sha(PACKAGE / name) == expected, 'UI-visible capture changed: ' + name)
    for name, expected in captures['inputs'].items():
        path = (ROOT / 'game' / name.removeprefix('res://')).resolve()
        require(path.is_relative_to(ROOT) and sha(path) == expected, 'Capture input changed: ' + name)
    print('PASS: screenshot successor preserves verified building/art evidence, source pixels, physics and scene ownership; eight bound captures')


if __name__ == '__main__':
    validate()
