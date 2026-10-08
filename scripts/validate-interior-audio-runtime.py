#!/usr/bin/env python3
"""Validate the bounded interior successor without editing frozen predecessor validators."""
import argparse
import copy
import hashlib
import json
import re
import subprocess
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]


def require(value, message):
    """Reject unauthorized inputs, ownership changes or missing output evidence."""
    if not value:
        raise ValueError(message)


def digest(data):
    """Hash exact bytes without rewriting the inspected input."""
    return hashlib.sha256(data).hexdigest()


def binding(row):
    """Check one path/hash pair from retained authority or evidence."""
    require(digest((ROOT / row['path']).read_bytes()) == row['sha256'], 'Changed binding: ' + row['path'])


def validate_main(scene, baseline):
    """Permit only the two nonphysical interior nodes and their exact script references."""
    for name, identifier in [('OpeningRegionInteriorAudioSource', 'interior_source'), ('InteriorAudioRuntimeSmokeCheck', 'interior_check')]:
        reference = f'[ext_resource type="Script" path="res://{name}.cs" id="{identifier}"]\n'
        require(scene.count(reference) == 1, 'Duplicate/missing interior reference')
        scene = scene.replace(reference, '', 1)
    trailer = '\n[node name="OpeningRegionInteriorAudioSource" type="Node" parent="."]\nprocess_priority = -50\nscript = ExtResource("interior_source")\nAutomatic = true\n\n[node name="InteriorAudioRuntimeSmokeCheck" type="Node" parent="."]\nscript = ExtResource("interior_check")\n'
    block = trailer[1:] + '\n'
    require(scene.count(block) == 1 and block + '[node name="WholeChannelPresentation"' in scene, 'Changed/duplicate interior node layout or channel final-child order')
    scene = scene.replace(block, '', 1)
    scene = re.sub(r'load_steps=(\d+)', lambda m: 'load_steps=' + str(int(m[1]) - 2), scene, count=1)
    require(scene == baseline, 'Unapproved physical/art/scene change')


def validate_ownership(material, positional):
    """Retain exactly one B02 player and exclude it from material player creation."""
    require(material.count('Where(id => id != "B02")') == 1, 'Material duplicated B02')
    require(positional.count('Player("B02",') == 1, 'Duplicate/missing positional B02')
    require('water.VolumeLinear = waterWeight * InteriorGain();' in positional, 'Missing single B02 multiplier')


def validate_output(metrics, timeline):
    """Require all finite cases and native stream evidence, not synthetic gain assertions."""
    require(metrics['cases'] == len(timeline) == len(metrics['metrics']) == 39, 'Missing measured-output case')
    require({r['name'] for r in timeline} == set(metrics['metrics']), 'Timeline/output mismatch')
    require(len(metrics['native_parity']) == 16 and all(r['PCM24_to_PCM16_exact'] for r in metrics['native_parity']), 'Missing native PCM evidence')
    for row in timeline:
        require(metrics['metrics'][row['name']]['peak_dbfs'] < 0, 'Clipped/missing peak measurement')


def rejected(check, *args):
    """Require the same production helper to reject a deliberate mutation."""
    try:
        check(*args)
    except ValueError:
        return
    raise ValueError('Mutation probe admitted')


def main():
    """Check authority, untouched resources/geometry/product, native output and review bindings."""
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--reviewed', action='store_true')
    args = parser.parse_args()
    scope = json.loads((ROOT / 'audio-direction/interior-runtime-scope.json').read_text())
    authority = json.loads((ROOT / 'audio-direction/interior-runtime-authority.json').read_text())
    require(authority['status'] == 'implementation_authorized' and authority['owner_statement'] == 'approved', 'Missing exact scope grant')
    for row in authority['proposal_bindings']:
        binding(row)
    for clip in scope['reused_resources']:
        binding(clip); binding(clip['import'])
    for row in scope['retained_rights'] + scope['retained_evidence']:
        binding(row)
    allowed = set(scope['implementation_files']['existing_files_to_change'] + scope['implementation_files']['new_files'])
    for row in scope['unchanged_inspected_inputs']:
        if row['path'] not in allowed:
            binding(row)
    binding(scope['protected_product_snapshot']['local_editor_exception'])
    baseline = subprocess.check_output(['git', 'show', scope['inspected_commit'] + ':game/Main.tscn'], cwd=ROOT, text=True)
    scene = (ROOT / 'game/Main.tscn').read_text()
    validate_main(scene, baseline)
    roots = scope['protected_product_snapshot']['roots']
    changed = subprocess.check_output(['git', 'diff', '--name-only', scope['inspected_commit'], '--', *roots], cwd=ROOT, text=True).splitlines()
    require(set(changed) <= allowed | {'game/project.godot'}, 'Unapproved protected product changes: ' + str(set(changed) - allowed - {'game/project.godot'}))
    untracked = subprocess.check_output(['git', 'ls-files', '--others', '--exclude-standard', '--', *roots], cwd=ROOT, text=True).splitlines()
    require(set(untracked) <= allowed, 'Unapproved untracked product input')
    material = (ROOT / 'game/OpeningRegionMaterialAudioController.cs').read_text()
    positional = (ROOT / 'game/OpeningRegionPositionalAudioController.cs').read_text()
    validate_ownership(material, positional)
    output_path = ROOT / 'audio-direction/interior-runtime-review/output/measurements.json'
    metrics = json.loads(output_path.read_text())
    timeline_path = ROOT / 'audio-direction/interior-runtime-review/output/timeline.json'
    timeline = json.loads(timeline_path.read_text())
    validate_output(metrics, timeline)
    require(digest(timeline_path.read_bytes()) == metrics['timeline_sha256'], 'Changed output timeline')
    modes = ['normal', 'muted', 'zero', 'mono-reduced']
    saves = [(ROOT / f'audio-direction/interior-runtime-review/main-{mode}.json.save.json').read_bytes() for mode in modes]
    transcripts = [json.loads((ROOT / f'audio-direction/interior-runtime-review/main-{mode}.json').read_text())['transcript'] for mode in modes]
    require(all(s == saves[0] for s in saves), 'Audio changed full saves')
    require(all(t == transcripts[0] for t in transcripts), 'Audio changed transcripts')
    require(all(len(json.loads((ROOT / f'audio-direction/interior-runtime-review/main-{mode}.json').read_text())['checks']) >= 4 for mode in modes), 'Missing ordinary Main checks')
    package = json.loads((ROOT / 'audio-direction/interior-runtime-review/output/packaging.json').read_text())
    require(package['native_streams'] == 33 and package['notices'] == 14 and package['resource_parity'], 'Missing actual resource packaging')
    views = json.loads((ROOT / 'audio-direction/interior-runtime-review/views/checks.json').read_text())
    require(any('1280x720 and 1920x1080' in row for row in views), 'Missing caption/keyboard views')
    rejected(lambda data: require(digest(data) == scope['proposal_document']['sha256'], 'Changed proposal'), (ROOT / scope['proposal_document']['path']).read_bytes() + b'changed')
    rejected(validate_main, scene.replace('position = Vector2(2640, 1390)', 'position = Vector2(2641, 1390)', 1), baseline)
    rejected(validate_ownership, material, positional + '\nPlayer("B02",')
    bad = copy.deepcopy(metrics); bad['cases'] = 38
    rejected(validate_output, bad, timeline)
    if args.reviewed:
        verification = json.loads((ROOT / 'audio-direction/interior-runtime-verification.json').read_text())
        require(verification['status'] == 'approved_scope_implemented_and_verified', 'Incomplete implementation record')
        for row in verification['reviewed_inputs'] + verification['evidence_bindings']:
            binding(row)
    print('PASS: bound authority/resources, protected scene/product, single B02, 39 measured cases, 16 native streams, four-mode full-save/transcript parity, actual packaging/views; four mutation rejection probes')


if __name__ == '__main__':
    main()
