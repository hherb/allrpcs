#!/usr/bin/env python3
"""Validate the exact approved positional successor without weakening frozen predecessors."""
import argparse
import hashlib
import json
from pathlib import Path
import re
import subprocess
import wave

ROOT = Path(__file__).resolve().parents[1]
RESOURCES = '''[ext_resource type="Script" path="res://OpeningRegionPositionalAudioController.cs" id="positional_audio"]
[ext_resource type="Script" path="res://PositionalAudioRuntimeSmokeCheck.cs" id="positional_check"]

'''
NODES = '''[node name="OpeningRegionPositionalAudioController" type="Node" parent="."]
script = ExtResource("positional_audio")

[node name="PositionalAudioRuntimeSmokeCheck" type="Node" parent="."]
script = ExtResource("positional_check")

'''


def require(value, message):
    """Reject changed protected ownership or missing exact authority."""
    if not value:
        raise ValueError(message)


def binding(row):
    """Verify hash-bound repository bytes."""
    require(hashlib.sha256((ROOT / row['path']).read_bytes()).hexdigest() == row['sha256'], 'Changed binding: ' + row['path'])


def validate_pcm(pcm, expected):
    """Reject changed samples or an incorrect interval independently of provenance."""
    require(hashlib.sha256(pcm).hexdigest() == expected, 'Wrong approved PCM range')


def validate_main(actual, baseline):
    """Only two additive references/nodes are permitted; physical and camera owners remain exact."""
    offset = baseline.index('[sub_resource')
    expected = baseline[:offset] + RESOURCES + baseline[offset:]
    expected = re.sub(r'load_steps=(\d+)', lambda m: 'load_steps=' + str(int(m[1]) + 2), expected, count=1)
    offset = expected.index('[node name="WholeChannelPresentation"')
    expected = expected[:offset] + NODES + expected[offset:]
    require(actual == expected, 'Main changed beyond exact additive audio owners')


def main():
    """Verify grant, protected product inputs, exact PCM, routing, packaging and negative probes."""
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--reviewed', action='store_true')
    args = parser.parse_args()
    scope = json.loads((ROOT / 'audio-direction/positional-cue-runtime-scope.json').read_text())
    authority = json.loads((ROOT / 'audio-direction/positional-cue-runtime-authority.json').read_text())
    require(authority['owner_statement'] == 'approved'
            and authority['status'] == 'runtime_and_exact_pcm_extraction_authorized'
            and authority['resource_count'] == 1, 'Missing exact new owner grant')
    for row in authority['proposal_bindings']:
        binding(row)
    mutable = set(scope['implementation_files']['bounded_changes'])
    for row in scope['unchanged_inspected_inputs']:
        if row['path'] not in mutable:
            binding(row)
    for row in json.loads((ROOT / scope['retained_acceptance']['record']['path']).read_text())['accepted_material_bindings']:
        binding(row)
    binding(scope['protected_product_snapshot']['local_editor_exception'])
    baseline = subprocess.check_output(['git', 'show', scope['inspected_commit'] + ':game/Main.tscn'], cwd=ROOT, text=True)
    main_scene = (ROOT / 'game/Main.tscn').read_text()
    validate_main(main_scene, baseline)
    altered = set(subprocess.check_output(['git', 'diff', '--name-only', scope['inspected_commit']], cwd=ROOT, text=True).splitlines())
    new = set(subprocess.check_output(['git', 'ls-files', '--others', '--exclude-standard'], cwd=ROOT, text=True).splitlines())
    allowed = mutable | set(scope['implementation_files']['new_files']) | set(scope['implementation_files']['documentation_maintenance']) | {
        'game/project.godot', 'audio-direction/positional-cue-runtime-scope.md', 'audio-direction/positional-cue-runtime-scope.json'}
    unexpected = {p for p in altered | new if p not in allowed and not p.startswith(scope['implementation_files']['review_prefix'])}
    require(not unexpected, 'Unapproved mutation: ' + ', '.join(sorted(unexpected)))
    e = scope['extraction']
    binding(e['accepted_parent'])
    with wave.open(str(ROOT / e['accepted_parent']['path']), 'rb') as source:
        source.setpos(e['begin_frame'])
        expected = source.readframes(e['frame_count'])
    validate_pcm(expected, e['expected_pcm_sha256'])
    with wave.open(str(ROOT / e['proposed_path']), 'rb') as derivative:
        require((derivative.getnchannels(), derivative.getsampwidth(), derivative.getframerate(), derivative.getnframes())
                == (2, 3, 48000, 24000), 'Changed derivative format')
        require(derivative.readframes(24000) == expected, 'Derivative samples differ')
    require(len(list((ROOT / 'game/assets/audio/positional').glob('*.wav'))) == 1, 'Extra shipping audio')
    imports = (ROOT / (e['proposed_path'] + '.import')).read_text()
    require(all(value in imports for value in ['compress/mode=0', 'edit/normalize=false', 'edit/trim=false',
            'force/mono=false', 'force/8_bit=false', 'force/max_rate=false', 'edit/loop_mode=0']), 'Transforming/lossy import')
    binding(scope['waterway']['clip'])
    material = (ROOT / 'game/OpeningRegionMaterialAudioController.cs').read_text()
    require('BedIds.Where(id => id != "B02")' in material and 'positional.PresentWaterway' in material, 'Duplicate/unrouted B02 owner')
    recorder = (ROOT / 'game/TemporaryCharacterTurnResponseSpeechRecorder.cs').read_text()
    require(recorder.index('lastRecordedDelivery = delivery;') < recorder.index('ReplyRecorded?.GetInvocationList()'), 'Notification precedes commit/guard')
    require('private const float DialogueHearingRangePixels = 192.0f;' in recorder
            and 'private const float WhisperHearingRangePixels = 48.0f;' in recorder, 'Changed hearing ranges')
    require('catch (Exception exception)' in recorder and 'TryAppendSpokenStatement(delivery)' in recorder, 'Missing commit/failure isolation')
    exports = (ROOT / 'game/export_presets.cfg').read_text()
    previous_exports = subprocess.check_output(['git', 'show', scope['inspected_commit'] + ':game/export_presets.cfg'], cwd=ROOT, text=True)
    require(exports == previous_exports.replace('assets/audio/footsteps/*.html"',
            'assets/audio/footsteps/*.html,assets/audio/positional/*.txt,assets/audio/positional/*.json,assets/audio/positional/*.html"'), 'Unbounded export change')
    for file in ['CREDITS.txt', 'provenance.json', 'CC0-1.0-deed.html']:
        require((ROOT / 'game/assets/audio/positional' / file).is_file(), 'Missing shipping notice')
    require('Kenney' in (ROOT / 'game/assets/audio/positional/CREDITS.txt').read_text(), 'Missing contributor')
    binding({'path':'game/assets/audio/positional/CC0-1.0-deed.html','sha256':scope['rights']['licence_notice']['sha256']})
    probes = [lambda: validate_pcm(b'changed', e['expected_pcm_sha256']),
              lambda: validate_pcm(expected[6:] + b'\0' * 6, e['expected_pcm_sha256']),
              lambda: validate_main(main_scene + NODES, baseline),
              lambda: validate_main(main_scene.replace('position = Vector2(2300, 1300)', 'position = Vector2(2301, 1300)', 1), baseline)]
    for probe in probes:
        try:
            probe()
        except ValueError:
            pass
        else:
            raise ValueError('Admitted forbidden mutation')
    if args.reviewed:
        verification = json.loads((ROOT / 'audio-direction/positional-cue-runtime-verification.json').read_text())
        for row in verification['reviewed_inputs'] + verification['evidence_bindings']:
            binding(row)
    subprocess.run(['python3', 'scripts/check-continuation.py'], cwd=ROOT, check=True)
    print('PASS: exact grant/PCM/import; protected inputs/art/geometry/footsteps/editor; single B02 routing; hearing/commit isolation; notices; 4 rejection probes')


if __name__ == '__main__':
    main()
