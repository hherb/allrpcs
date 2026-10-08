#!/usr/bin/env python3
"""Independently validate the approved music successor without changing frozen predecessor gates."""
import argparse
import hashlib
import json
import re
import subprocess
import wave
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]

def require(value, message):
    """Reject drift in authority, audio, physical scene, existing product or evidence."""
    if not value:
        raise ValueError(message)

def digest(data):
    """Hash exact binary inputs, including unchanged PCM."""
    return hashlib.sha256(data).hexdigest()

def binding(row):
    """Check one retained exact path/hash binding."""
    require(digest((ROOT / row['path']).read_bytes()) == row['sha256'], 'Changed binding: ' + row['path'])

def validate_copy(actual, accepted):
    """Reject any changed byte in the approved whole WAV."""
    require(actual == accepted, 'Changed accepted music WAV')

def validate_import(imports, contract):
    """Reject compressed, edited or looping importer output."""
    require(all(f'{key}={str(value).lower()}' in imports for key, value in contract.items()), 'Changed/lossy/looping import')

def validate_credits(text):
    """Require both current contributors and exclude the rejected predecessor."""
    require('Yoiyami' in text and 'gonzooka' in text and 'Indieteur' not in text, 'Wrong current music contributors')

def rejected(check, *values):
    """Require a mutation probe to fail the same production validation helper."""
    try:
        check(*values)
    except ValueError:
        return
    raise ValueError('Mutation probe was admitted')

def validate_main(scene, baseline):
    """Permit just two new music script references and their nonphysical nodes."""
    normalized = scene
    for name, identifier in [('OpeningRegionMusicAudioController', 'music_controller'), ('MusicAudioRuntimeSmokeCheck', 'music_check')]:
        reference = f'[ext_resource type="Script" path="res://{name}.cs" id="{identifier}"]\n'
        node = f'[node name="{name}" type="Node" parent="."]\nscript = ExtResource("{identifier}")\n\n'
        require(normalized.count(reference) == normalized.count(node) == 1, 'Duplicate/missing music owner')
        normalized = normalized.replace(reference, '', 1).replace(node, '', 1)
    normalized = normalized.replace('\n\n[ext_resource type="Script" path="res://AudioSettingsStorageController.cs"',
                                    '\n[ext_resource type="Script" path="res://AudioSettingsStorageController.cs"', 1)
    normalized = re.sub(r'load_steps=(\d+)', lambda m: 'load_steps=' + str(int(m[1]) - 2), normalized, count=1)
    require(normalized == baseline, 'Unapproved Main scene/physical/art/input change')

def native_parity(parent, data):
    """Match the retained engine PCM24-to-PCM16 low-byte discard convention."""
    import numpy as np
    samples = np.frombuffer(parent, dtype=np.uint8).reshape(-1, 3).astype(np.int32)
    signed = samples[:, 0] | samples[:, 1] << 8 | samples[:, 2] << 16
    signed = np.where(signed & 0x800000, signed - 0x1000000, signed)
    expected = (signed >> 8).astype('<i2').tobytes()
    require(data == expected, 'Wrong native music PCM conversion')

def main():
    """Validate exact copy/import, bounded Main changes, native/packaged evidence and rejection probes."""
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--reviewed', action='store_true')
    args = parser.parse_args()
    scope = json.loads((ROOT / 'audio-direction/music-runtime-scope.json').read_text())
    authority = json.loads((ROOT / 'audio-direction/music-runtime-authority.json').read_text())
    require(authority['status'] == 'exact_scope_approved' and authority['grant']['exact_runtime_implementation'], 'Missing new runtime authority')
    for row in authority['bindings']:
        binding(row)
    mutable = set(scope['implementation_files']['existing_files_to_change'])
    for row in scope['unchanged_inspected_inputs']:
        if row['path'] not in mutable:
            binding(row)
    for row in json.loads((ROOT / scope['retained_acceptance']['record']['path']).read_text())['accepted_material_bindings']:
        binding(row)
    binding(scope['protected_product_snapshot']['local_editor_exception'])
    baseline = scope['inspected_commit']
    paths = sorted(subprocess.check_output(['git', 'ls-tree', '-r', '--name-only', baseline, '--', 'game', 'src', 'tests', 'scripts'], cwd=ROOT, text=True).splitlines())
    batch = subprocess.Popen(['git', 'cat-file', '--batch'], cwd=ROOT, stdin=subprocess.PIPE, stdout=subprocess.PIPE)
    rows = []
    for path in paths:
        batch.stdin.write((baseline + ':' + path + '\n').encode()); batch.stdin.flush()
        header = batch.stdout.readline().split(); size = int(header[-1]); content = batch.stdout.read(size); batch.stdout.read(1)
        rows.append({'path': path, 'sha256': digest(content)})
        if path not in mutable and path != 'game/project.godot':
            require((ROOT / path).read_bytes() == content, 'Protected product changed: ' + path)
    batch.stdin.close(); batch.wait()
    require(digest(json.dumps(rows, sort_keys=True, separators=(',', ':')).encode()) == scope['protected_product_snapshot']['manifest_sha256'], 'Wrong baseline product manifest')
    previous = lambda path: subprocess.check_output(['git', 'show', baseline + ':' + path], cwd=ROOT, text=True)
    scene = (ROOT / 'game/Main.tscn').read_text(); old_scene = previous('game/Main.tscn'); validate_main(scene, old_scene)
    credits = (ROOT / 'game/MaterialAudioCreditsPanel.cs').read_text()
    require(credits == previous('game/MaterialAudioCreditsPanel.cs').replace(
        '        text += "\\n\\n" + Godot.FileAccess.GetFileAsString("res://assets/audio/positional/CREDITS.txt");',
        '        text += "\\n\\n" + Godot.FileAccess.GetFileAsString("res://assets/audio/positional/CREDITS.txt");\n'
        '        text += "\\n\\n" + Godot.FileAccess.GetFileAsString("res://assets/audio/music/CREDITS.txt");'), 'Unbounded credits change')
    require((ROOT / 'game/export_presets.cfg').read_text() == previous('game/export_presets.cfg').replace('assets/audio/positional/*.html"',
        'assets/audio/positional/*.html,assets/audio/music/*.txt,assets/audio/music/*.json,assets/audio/music/*.html"'), 'Unbounded export change')
    panel = (ROOT / 'game/AudioSettingsPanel.cs').read_text()
    panel = panel.replace('\n    /// <summary>Reading includes child views that temporarily hide their Audio parent.</summary>\n'
                          '    internal bool IsReading => Visible || creditsPanel.Visible || historyPanel.Visible;\n', '', 1)
    explanation = '        content.AddChild(new Label\n        {\n            Name = "MusicExplanation",\n'
    explanation += '            Text = "Music plays occasionally in quiet moments. Mute Music for a music-free game.",\n'
    explanation += '            AutowrapMode = TextServer.AutowrapMode.WordSmart\n        });\n'
    panel = panel.replace(explanation, '', 1)
    require(panel == previous('game/AudioSettingsPanel.cs'), 'Unbounded music control/settings change')
    shipping = ROOT / 'game/assets/audio/music'
    require({p.name for p in shipping.iterdir()} == {'M01.wav','M01.wav.import','M02.wav','M02.wav.import','CREDITS.txt','provenance.json','CC0-1.0-deed.html'}, 'Unexpected music package')
    for clip in scope['clips']:
        binding(clip['accepted'])
        runtime = ROOT / clip['proposed_runtime_path']
        accepted = (ROOT / clip['accepted']['path']).read_bytes()
        validate_copy(runtime.read_bytes(), accepted)
        rejected(validate_copy, accepted[:-1] + bytes([accepted[-1] ^ 1]), accepted)
        with wave.open(str(runtime), 'rb') as wav:
            require((wav.getnchannels(), wav.getsampwidth(), wav.getframerate(), wav.getnframes()) == (2,3,48000,1200000), 'Wrong music header')
            pcm = wav.readframes(wav.getnframes())
        require(digest(pcm) == clip['PCM_sha256'], 'Wrong music PCM')
        imports = Path(str(runtime) + '.import').read_text()
        validate_import(imports, clip['import_contract'])
        for key in ['compress/mode', 'edit/loop_mode', 'edit/normalize']:
            value = clip['import_contract'][key]
            replacement = 'true' if value is False else str(int(value) + 1)
            rejected(validate_import, imports.replace(f'{key}={str(value).lower()}', f'{key}={replacement}'), clip['import_contract'])
        decoded = ROOT / ('audio-direction/music-runtime-review/output/' + clip['id'] + '.pcm16')
        if args.reviewed:
            native_parity(pcm, decoded.read_bytes())
    require(digest((shipping / 'CC0-1.0-deed.html').read_bytes()) == scope['rights']['license_notice']['sha256'], 'Changed packaged CC0 notice')
    text = (shipping / 'CREDITS.txt').read_text()
    validate_credits(text)
    rejected(validate_credits, text.replace('gonzooka', 'Indieteur'))
    changed = set(subprocess.check_output(['git','diff','--name-only',baseline],cwd=ROOT,text=True).splitlines())
    new = set(subprocess.check_output(['git','ls-files','--others','--exclude-standard'],cwd=ROOT,text=True).splitlines())
    allowed = mutable | set(scope['implementation_files']['new_files']) | set(scope['implementation_files']['documentation_maintenance']) | {'game/project.godot', 'audio-direction/music-runtime-scope.md', 'audio-direction/music-runtime-scope.json'}
    require(all(p in allowed or p.startswith(scope['implementation_files']['review_prefix']) for p in changed | new), 'Unapproved successor mutation: ' + ', '.join(sorted(p for p in changed | new if p not in allowed and not p.startswith(scope['implementation_files']['review_prefix']))))
    for altered in [scene + '\n[node name="MusicAudioRuntimeSmokeCheck" type="Node" parent="."]\nscript = ExtResource("music_check")\n\n',
                    scene.replace('position = Vector2(2300, 1300)', 'position = Vector2(2301, 1300)', 1)]:
        try:
            validate_main(altered, old_scene)
        except ValueError:
            pass
        else:
            raise ValueError('Admitted duplicate music/changed physical scene')
    if args.reviewed:
        verification = json.loads((ROOT / 'audio-direction/music-runtime-verification.json').read_text())
        for row in verification['reviewed_inputs'] + verification['evidence_bindings']:
            binding(row)
    subprocess.run(['python3','scripts/check-continuation.py'],cwd=ROOT,check=True)
    print('PASS: exact music grant, 82 accepted bindings, protected product/editor, two WAV/PCM/import/current credits, bounded Main/export, mutation probes' + (' and native/review parity' if args.reviewed else ''))

if __name__ == '__main__':
    main()
