#!/usr/bin/env python3
"""Validate the approved material successor without rewriting frozen predecessors."""

import argparse
import hashlib
import json
from pathlib import Path
import re
import subprocess

ROOT = Path(__file__).resolve().parents[1]
RESOURCES = '''[ext_resource type="Script" path="res://OpeningRegionMaterialAudioController.cs" id="material_audio"]
[ext_resource type="PackedScene" path="res://MaterialSoundCaptionPanel.tscn" id="material_captions"]
[ext_resource type="Script" path="res://MaterialAudioRuntimeSmokeCheck.cs" id="material_check"]

'''
NODES = '''
[node name="MaterialSoundCaptionPanel" parent="." instance=ExtResource("material_captions")]

[node name="OpeningRegionMaterialAudioController" type="Node" parent="."]
script = ExtResource("material_audio")

[node name="MaterialAudioRuntimeSmokeCheck" type="Node" parent="."]
script = ExtResource("material_check")
'''


def require(value, message):
    """Reject an altered boundary independently of Python optimization."""
    if not value:
        raise ValueError(message)


def digest(path):
    """Hash exact file bytes, including accepted audio and authority records."""
    return hashlib.sha256((ROOT / path).read_bytes()).hexdigest()


def binding(row):
    """Verify one hash-bound predecessor or newly reviewed input."""
    require(digest(row['path']) == row['sha256'], 'Changed binding: ' + row['path'])


def prior(commit, name):
    """Read protected predecessor bytes directly from the recorded commit."""
    return subprocess.check_output(['git', 'show', commit + ':' + name], cwd=ROOT).decode()


def validate_main(live, baseline):
    """Allow exactly the reviewed declarations and three material nodes."""
    offset = baseline.index('[sub_resource')
    expected = baseline[:offset] + RESOURCES + baseline[offset:]
    expected = re.sub(r'load_steps=(\d+)', lambda match: 'load_steps=' + str(int(match[1]) + 3), expected, count=1)
    offset = expected.index('[node name="WholeChannelPresentation"')
    expected = expected[:offset] + NODES + '\n' + expected[offset:]
    require(live == expected, 'Material Main changed a retained world, physical or visual owner')


def wave_bytes(actual, expected):
    """Require exact accepted waveform bytes, without trusting import sidecars."""
    require(hashlib.sha256(actual).hexdigest() == expected, 'Accepted waveform changed')


def main():
    """Check authority, exact runtime inventory and three independent rejection probes."""
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--reviewed', action='store_true', help='Also verify completed source/evidence hashes')
    args = parser.parse_args()
    scope = json.loads((ROOT / 'audio-direction/material-runtime-scope.json').read_text())
    authority = json.loads((ROOT / 'audio-direction/material-runtime-authority.json').read_text())
    require(authority['owner_statement'] == 'approved' and authority['status'] == 'runtime_scope_authorized',
            'Missing actual six-sound implementation approval')
    for row in authority['proposal_bindings']:
        binding(row)
    expected_ids = {'B01', 'B02', 'B03', 'B04', 'A04', 'A05'}
    require(set(authority['selected_ids']) == expected_ids, 'Authority candidate mismatch')
    acceptance = json.loads((ROOT / 'audio-direction/material-audition/owner-acceptance.json').read_text())
    for row in acceptance['accepted_material_bindings']:
        binding(row)
    mutable = {row['path'] for row in scope['implementation_files']['bounded_changes']}
    for row in scope['inspected_inputs']:
        if row['path'] not in mutable:
            binding(row)
    binding(scope['preserved_local_editor_rewrite'])
    validate_main((ROOT / 'game/Main.tscn').read_text(), prior(scope['baseline_commit'], 'game/Main.tscn'))
    original_exports = prior(scope['baseline_commit'], 'game/export_presets.cfg')
    expected_exports = original_exports.replace('include_filter=""',
        'include_filter="assets/audio/material/*.txt,assets/audio/material/*.json,assets/audio/material/*.html"')
    require((ROOT / 'game/export_presets.cfg').read_text() == expected_exports, 'Unbounded export change')
    credits = (ROOT / 'game/assets/audio/material/CREDITS.txt').read_text()
    require(all(word in credits for word in ['kurt', 'Alexander', 'CC-BY-3.0', 'CC-BY-4.0', 'Bashar']),
            'Missing runtime attribution')
    manifest = json.loads((ROOT / 'game/assets/audio/material/provenance.json').read_text())
    require({row['id'] for row in manifest['clips']} == expected_ids, 'Incorrect shipping clip manifest')
    for row in manifest['clips']:
        accepted = next(asset for asset in scope['assets'] if asset['id'] == row['id'])
        require(row['sha256'] == accepted['audition']['sha256'], 'Manifest drift')
        wave_bytes((ROOT / 'game/assets/audio/material' / row['path']).read_bytes(), row['sha256'])
        sidecar = ROOT / ('game/assets/audio/material/' + row['path'] + '.import')
        require(sidecar.exists(), 'Missing imported resource metadata')
        settings = sidecar.read_text()
        require(all(setting in settings for setting in ['compress/mode=0', 'edit/normalize=false',
                'edit/trim=false', 'force/8_bit=false', 'force/mono=false', 'force/max_rate=false']),
                'Lossy or transforming WAV import settings')
    allowed = mutable | set(scope['implementation_files']['new_files']) | {
        'CONTINUATION.json', 'HANDOVER.md', 'ROADMAP.md', 'game/project.godot',
        'audio-direction/material-runtime-scope.md', 'audio-direction/material-runtime-scope.json'}
    changed = set(subprocess.check_output(['git', 'diff', '--name-only', scope['baseline_commit']], cwd=ROOT, text=True).splitlines())
    added = set(subprocess.check_output(['git', 'ls-files', '--others', '--exclude-standard'], cwd=ROOT, text=True).splitlines())
    unexpected = {name for name in changed | added if name not in allowed
                  and not name.startswith('audio-direction/material-runtime-review/')
                  and not name.startswith('artifacts/unsigned-development/')}
    require(not unexpected, 'Unapproved files: ' + ', '.join(sorted(unexpected)))
    live = (ROOT / 'game/Main.tscn').read_text()
    probes = [lambda: wave_bytes(b'replaced', manifest['clips'][0]['sha256']),
              lambda: validate_main(live + NODES, prior(scope['baseline_commit'], 'game/Main.tscn')),
              lambda: validate_main(live.replace('position = Vector2(2300, 1300)',
                                                'position = Vector2(2301, 1300)', 1),
                                    prior(scope['baseline_commit'], 'game/Main.tscn'))]
    for probe in probes:
        try:
            probe()
        except ValueError:
            pass
        else:
            raise ValueError('Successor boundary admitted a forbidden mutation')
    if args.reviewed:
        review = json.loads((ROOT / 'audio-direction/material-runtime-verification.json').read_text())
        for row in review['reviewed_inputs'] + review['evidence_bindings']:
            binding(row)
    subprocess.run(['python3', 'scripts/check-continuation.py'], cwd=ROOT, check=True)
    print('PASS: approved six byte-identical assets; exact additive Main and export boundary; frozen inputs; editor rewrite; 3 rejection probes')


if __name__ == '__main__':
    main()
