#!/usr/bin/env python3
"""Validate the authorized additive audio composition while preserving accepted product/art bytes."""

import hashlib
import json
from pathlib import Path
import re
import subprocess

ROOT = Path(__file__).resolve().parents[1]

RESOURCES = '''[ext_resource type="Script" path="res://AudioSettingsStorageController.cs" id="audio_storage"]
[ext_resource type="Script" path="res://AudioSettingsController.cs" id="audio_settings"]
[ext_resource type="PackedScene" path="res://AudioSettingsPanel.tscn" id="audio_panel"]

[ext_resource type="Script" path="res://AudioSettingsMainSmokeCheck.cs" id="audio_main_check"]

'''
NODES = '''[node name="AudioSettingsStorageController" type="Node" parent="."]
script = ExtResource("audio_storage")

[node name="AudioSettingsController" type="Node" parent="."]
script = ExtResource("audio_settings")

[node name="AudioSettingsPanel" parent="." instance=ExtResource("audio_panel")]

[node name="AudioSettingsMainSmokeCheck" type="Node" parent="."]
script = ExtResource("audio_main_check")

'''
ENTRY = '''[node name="AudioEntry" type="Button" parent="SettingsOverlay"]
offset_left = 12.0
offset_top = 190.0
offset_right = 160.0
offset_bottom = 230.0
text = "Audio settings"

'''


def require(condition, message):
    """Fail closed without changing any frozen evidence or validator."""
    if not condition:
        raise ValueError(message)


def original(revision, name):
    """Read exact predecessor bytes directly from Git."""
    return subprocess.check_output(['git', 'show', revision + ':' + name], cwd=ROOT)


def validate_main(live, baseline):
    """Permit only the four declarations, four composition nodes and one Audio entry."""
    expected = baseline.decode()
    position = expected.index('[sub_resource')
    expected = expected[:position] + RESOURCES + expected[position:]
    position = expected.index('[node name="OptionalOverlayVisibilityController"')
    expected = expected[:position] + NODES + expected[position:]
    position = expected.index('[node name="Panel" type="ColorRect" parent="SettingsOverlay"]')
    expected = expected[:position] + ENTRY + expected[position:]
    expected = re.sub(r'load_steps=(\d+)', lambda match: 'load_steps=' + str(int(match[1]) + 4), expected, count=1)
    require(live == expected, 'Audio Main additions changed an existing visual, physical or world owner')


def main():
    """Bind current authority and check the finite scene delta and protected predecessor membership."""
    baseline = json.loads((ROOT / 'audio-direction/buses-controls-baseline.json').read_text())
    authority = json.loads((ROOT / 'audio-direction/buses-controls-authority.json').read_text())
    require(authority['owner_statement'] == 'scope approved', 'Missing actual runtime authorization')
    binding = authority['scope_binding']
    require(hashlib.sha256((ROOT / binding['path']).read_bytes()).hexdigest() == binding['sha256'], 'Approved scope changed')
    revision = baseline['commit']
    prior = original(revision, 'game/Main.tscn')
    require(hashlib.sha256(prior).hexdigest() == baseline['main_sha256'], 'Predecessor Main binding changed')
    require(hashlib.sha256(original(revision, 'game/OptionalOverlayVisibilityController.cs')).hexdigest()
            == baseline['optional_overlays_sha256'], 'Predecessor overlay binding changed')
    live = (ROOT / 'game/Main.tscn').read_text()
    validate_main(live, prior)
    allowed = {'CONTINUATION.json', 'HANDOVER.md', 'ROADMAP.md', 'game/Main.tscn',
               'game/OptionalOverlayVisibilityController.cs', 'game/project.godot'}
    changed = subprocess.check_output(['git', 'diff', '--name-only', revision], cwd=ROOT, text=True).splitlines()
    require(set(changed) <= allowed, 'Unapproved predecessor changes: ' + ', '.join(sorted(set(changed) - allowed)))
    require(hashlib.sha256((ROOT / 'game/project.godot').read_bytes()).hexdigest()
            == baseline['preserved_editor_project_sha256'], 'Unrelated editor rewrite changed')
    # Prove the comparator actually rejects a moved world owner and duplicate audio route.
    for label, mutation in (
        ('physical placement', live.replace('position = Vector2(2300, 1300)', 'position = Vector2(2301, 1300)', 1)),
        ('duplicate mixer', live + '\n[node name="DuplicateAudioSettingsController" type="Node" parent="."]\n'),
        ('renamed bus composition', live.replace('id="audio_settings"', 'id="renamed_audio"', 1)),
    ):
        require(mutation != live, 'Ineffective boundary probe: ' + label)
        try:
            validate_main(mutation, prior)
        except ValueError:
            pass
        else:
            raise ValueError('Boundary accepted mutation: ' + label)
    subprocess.run(['python3', 'scripts/check-continuation.py'], cwd=ROOT, check=True)
    print('PASS: approved additive Main audio scope; predecessor art/world/provider files and editor rewrite retained; 3 rejection probes')


if __name__ == '__main__':
    main()
