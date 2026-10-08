#!/usr/bin/env python3
"""Validate the authorized footstep successor while preserving frozen predecessor evidence."""

import argparse
import hashlib
import json
from pathlib import Path
import re
import subprocess
import wave

ROOT = Path(__file__).resolve().parents[1]
RESOURCES = '''[ext_resource type="Script" path="res://OpeningRegionFootstepMaterialSource.cs" id="footstep_material"]
[ext_resource type="Script" path="res://OpeningRegionFootstepAudioController.cs" id="footstep_audio"]
[ext_resource type="Script" path="res://FootstepAudioRuntimeSmokeCheck.cs" id="footstep_check"]

'''
NODES = '''[node name="OpeningRegionFootstepMaterialSource" type="Node" parent="."]
script = ExtResource("footstep_material")

[node name="OpeningRegionFootstepAudioController" type="Node" parent="."]
script = ExtResource("footstep_audio")

[node name="FootstepAudioRuntimeSmokeCheck" type="Node" parent="."]
script = ExtResource("footstep_check")

'''


def require(value, message):
    """Fail closed on missing authority, changed PCM or unbounded source mutations."""
    if not value:
        raise ValueError(message)


def digest(path):
    """Hash a repository-relative artifact's exact bytes."""
    return hashlib.sha256((ROOT / path).read_bytes()).hexdigest()


def binding(row):
    """Verify a retained proposal, input or accepted evidence binding."""
    require(digest(row['path']) == row['sha256'], 'Changed binding: ' + row['path'])


def previous(commit, path):
    """Read protected product bytes from the proposal's actual inspected commit."""
    return subprocess.check_output(['git', 'show', commit + ':' + path], cwd=ROOT).decode()


def validate_main(actual, baseline):
    """Allow only three additive nodes before the retained final channel owner."""
    offset = baseline.index('[sub_resource')
    expected = baseline[:offset] + RESOURCES + baseline[offset:]
    expected = re.sub(r'load_steps=(\d+)', lambda m: 'load_steps=' + str(int(m[1]) + 3), expected, count=1)
    offset = expected.index('[node name="WholeChannelPresentation"')
    expected = expected[:offset] + NODES + expected[offset:]
    require(actual == expected, 'Footstep Main changed protected world/physical/art composition')


def validate_pcm(data, expected):
    """Reject a wrong interval or changed sample without trusting the derivative manifest."""
    require(hashlib.sha256(data).hexdigest() == expected, 'Contact PCM differs from approved parent range')


def normalized(text):
    """Ignore formatting and documentation while comparing retained movement statements."""
    return re.sub(r'\s+', '', re.sub(r'///[^\n]*', '', text))


def validate_movement(live, baseline):
    """Strip only the authorized observer addition; require all original movement statements."""
    live = live.replace('using Allrpcs.Application.Audio;', '')
    live = live.replace('private long motionTick;', '')
    live = live.replace('public event Action<FootstepMotionSample>? MotionCompleted;', '')
    live = live.replace('Vector2 start = GlobalPosition;', '')
    live = re.sub(r'FootstepMotionSample sample =.*?\n    }', '}', live, flags=re.S)
    require(normalized(live) == normalized(baseline.replace('_ = delta;', '')), 'Seeker movement changed beyond observer')


def main():
    """Verify proposal authority, 24 PCM contacts, preserved inputs and meaningful rejection probes."""
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--reviewed', action='store_true')
    args = parser.parse_args()
    scope = json.loads((ROOT / 'audio-direction/footstep-runtime-scope.json').read_text())
    authority = json.loads((ROOT / 'audio-direction/footstep-runtime-authority.json').read_text())
    require(authority['owner_statement'] == 'approved' and authority['seeker_only']
            and authority['resource_count'] == 24
            and authority['status'] == 'runtime_and_exact_pcm_extraction_authorized', 'Missing exact implementation grant')
    for row in authority['proposal_bindings']:
        binding(row)
    mutable = set(scope['implementation_files']['bounded_changes'])
    for row in scope['unchanged_inputs']:
        if row['path'] not in mutable:
            binding(row)
    acceptance = json.loads((ROOT / 'audio-direction/material-audition/owner-acceptance.json').read_text())
    for row in acceptance['accepted_material_bindings']:
        binding(row)
    baseline = previous(scope['inspected_commit'], 'game/Main.tscn')
    live = (ROOT / 'game/Main.tscn').read_text()
    validate_main(live, baseline)
    validate_movement((ROOT / 'game/Seeker.cs').read_text(), previous(scope['inspected_commit'], 'game/Seeker.cs'))
    old_exports = previous(scope['inspected_commit'], 'game/export_presets.cfg')
    expected = old_exports.replace('assets/audio/material/*.html"',
        'assets/audio/material/*.html,assets/audio/footsteps/*.txt,assets/audio/footsteps/*.json,assets/audio/footsteps/*.html"')
    require((ROOT / 'game/export_presets.cfg').read_text() == expected, 'Unbounded export preset change')
    manifest = json.loads((ROOT / 'game/assets/audio/footsteps/provenance.json').read_text())
    approved = {v['proposed_path']: v for c in scope['extraction']['contacts'] for v in c['variants']}
    require({v['proposed_path'] for v in manifest['clips']} == set(approved), 'Wrong contact inventory')
    for c in scope['extraction']['contacts']:
        with wave.open(str(ROOT / c['accepted_parent']['path']), 'rb') as parent:
            pcm = parent.readframes(parent.getnframes())
        for v in c['variants']:
            a, b = v['parent_pcm_begin_frame'], v['parent_pcm_end_frame_exclusive']
            expected_pcm = pcm[a * 6:b * 6]
            validate_pcm(expected_pcm, v['expected_pcm_sha256'])
            with wave.open(str(ROOT / v['proposed_path']), 'rb') as derived:
                require((derived.getnchannels(), derived.getsampwidth(), derived.getframerate(), derived.getnframes())
                        == (2, 3, 48000, 24000), 'Changed derivative format')
                actual = derived.readframes(derived.getnframes())
            require(actual == expected_pcm, 'Derived samples changed')
            row = next(r for r in manifest['clips'] if r['proposed_path'] == v['proposed_path'])
            require(row['derived_wav_sha256'] == digest(v['proposed_path']), 'Derivative header/manifest changed')
            settings = (ROOT / (v['proposed_path'] + '.import')).read_text()
            require(all(s in settings for s in ['compress/mode=0', 'edit/trim=false', 'edit/normalize=false',
                'force/mono=false', 'force/8_bit=false', 'force/max_rate=false', 'edit/loop_mode=0']), 'Transforming contact import')
    require(len(list((ROOT / 'game/assets/audio/footsteps').glob('*.wav'))) == 24, 'Extra shipping WAVs')
    credits = (ROOT / 'game/assets/audio/footsteps/CREDITS.txt').read_text()
    require(all(author in credits for author in ['simonjeffery13', 'BenDrain', 'Dominik_W', 'florianreichelt']), 'Missing contributor')
    allowed = mutable | set(scope['implementation_files']['new_files']) | set(scope['implementation_files']['documentation_maintenance']) | {
        'game/project.godot', 'audio-direction/footstep-runtime-scope.md', 'audio-direction/footstep-runtime-scope.json'}
    changed = set(subprocess.check_output(['git', 'diff', '--name-only', scope['inspected_commit']], cwd=ROOT, text=True).splitlines())
    added = set(subprocess.check_output(['git', 'ls-files', '--others', '--exclude-standard'], cwd=ROOT, text=True).splitlines())
    unexpected = {p for p in changed | added if p not in allowed and not p.startswith('audio-direction/footstep-runtime-review/')}
    require(not unexpected, 'Unapproved files: ' + ', '.join(sorted(unexpected)))
    probes = [lambda: validate_pcm(b'changed', next(iter(approved.values()))['expected_pcm_sha256']),
              lambda: validate_main(live + NODES, baseline),
              lambda: validate_main(live.replace('position = Vector2(2300, 1300)', 'position = Vector2(2301, 1300)', 1), baseline),
              lambda: validate_movement((ROOT / 'game/Seeker.cs').read_text().replace('240.0f', '241.0f', 1), previous(scope['inspected_commit'], 'game/Seeker.cs'))]
    for probe in probes:
        try:
            probe()
        except ValueError:
            pass
        else:
            raise ValueError('Boundary admitted a forbidden mutation')
    if args.reviewed:
        verification = json.loads((ROOT / 'audio-direction/footstep-runtime-verification.json').read_text())
        for row in verification['reviewed_inputs'] + verification['evidence_bindings']:
            binding(row)
    subprocess.run(['python3', 'scripts/check-continuation.py'], cwd=ROOT, check=True)
    print('PASS: exact authority; 24 accepted PCM slices; additive Main/export; retained movement/physical/art/audio/editor inputs; 4 rejection probes')


if __name__ == '__main__':
    main()
