#!/usr/bin/env python3
"""Validate the approved cask-cover successor and retained predecessor boundaries."""
import argparse
import gzip
import hashlib
import io
import json
from pathlib import Path
import re
import subprocess
import wave
from zipfile import ZipFile

ROOT = Path(__file__).resolve().parents[1]
BASE = 'a23a14ad1165b27a30ca2409310cd44ca3627de1'
REVIEW = ROOT / 'audio-direction/cask-cover-runtime-review'
PROPOSAL_SHA = 'e8deef9efab44006a5bd6c1306a4eb81195126f40cfa47cfd560ee8d50cf28ce'
MODIFIED = {'game/Main.tscn', 'game/MaterialSoundCaptionPanel.cs', 'game/MaterialAudioCreditsPanel.cs',
            'quality.py', 'game/export_presets.cfg', 'CONTINUATION.json', 'HANDOVER.md', 'game/project.godot'}


def require(value, message):
    """Reject drift and incomplete evidence even with Python optimization enabled."""
    if not value:
        raise ValueError(message)


def sha(data):
    """Hash exact input and evidence bytes."""
    return hashlib.sha256(data).hexdigest()


def load(path):
    """Read a repository-bound JSON record."""
    return json.loads((ROOT / path).read_text())


def bindings(rows):
    """Require each named current or frozen input to match its recorded hash."""
    for row in rows:
        require(sha((ROOT / row['path']).read_bytes()) == row['sha256'], 'Changed binding: ' + row['path'])


def main_bytes(data, baseline):
    """Remove only four new resources/nodes; preserve all prior scene bytes and final-child order."""
    text = data.decode()
    require(text.count('id="cask_') == 4, 'Changed cask resource membership')
    text = re.sub(r'^\[ext_resource[^\n]*id="cask_[0-3]"\]\n', '', text, flags=re.M)
    for name in ('OpeningRegionCaskCoverHandlingCoordinator', 'CaskCoverHandlingPanel',
                 'OpeningRegionCaskCoverAudioController', 'CaskCoverHandlingRuntimeSmokeCheck'):
        pattern = r'\[node name="' + name + r'"[^\n]*\]\n(?:(?!\[node)[\s\S])*?(?=\[node|\Z)'
        text, count = re.subn(pattern, '', text)
        require(count == 1, 'Missing/extra cask Main node: ' + name)
    text = re.sub(r'load_steps=(\d+)', lambda m: 'load_steps=' + str(int(m[1]) - 4), text, count=1)
    require([line for line in text.splitlines() if line.strip()] ==
            [line for line in baseline.decode().splitlines() if line.strip()], 'Changed prior Main/geometry/order')


def proposal_bytes(data):
    """Bind the single owner-approved published proposal, not mutable later prose."""
    require(sha(data) == PROPOSAL_SHA, 'Changed approved proposal')


def pcm_bytes(data):
    """Preserve the complete approved wooden-contact and cotton-handling passage."""
    require(len(data) == 1080000 and sha(data) ==
            '5dc4e93225eaad07ff94861d531aaed74e697d46bdcb7e85dea1db894c5bc041', 'Changed approved PCM')


def measured(record, names):
    """Require twenty measured cases with exact live admission and once-only consumption."""
    require(record['native_exact'] and record['cases'] == 20 and set(record['metrics']) == set(names), 'Missing output case')
    rejected = {'out-of-reach', 'unsupported-segment', 'stale-target', 'overlay-rejection', 'concealed-or-substituted-target'}
    for name, row in record['metrics'].items():
        expected = 0 if name in rejected else 1
        require(row['commits'] == row['presentations'] == expected, 'Forged/repeated commit feedback: ' + name)
        require(row['admitted'] == (name not in rejected | {'duplicate-request'}), 'Changed action admission: ' + name)


def truth_checks(record):
    """Check serialized world/inventory/discovery/sabotage evidence around the exact handling commits."""
    require({row['name'] for row in record['truthChecks']} == {'moved-cask', 'hidden-contamination'}, 'Lost truth checks')
    for row in record['truthChecks']:
        require(row['before'] == row['after'], 'Mutated protected truth: ' + row['name'])
        value = json.loads(row['before'])
        require(set(value) == {'objects', 'inventory', 'perception', 'placements', 'moveEvidence', 'concealment',
                              'substitutions', 'contamination', 'contaminationEvidence'}, 'Lost protected state membership')


def rejection_probes(proposal, pcm, scene, baseline, output, names, parity):
    """Exercise real validators on in-memory mutations without touching frozen evidence."""
    def rejected(callback):
        try:
            callback()
        except (ValueError, KeyError, FileNotFoundError):
            return
        raise ValueError('Mutation incorrectly passed')
    rejected(lambda: proposal_bytes(proposal + b' '))
    rejected(lambda: pcm_bytes(bytes([pcm[0] ^ 1]) + pcm[1:]))
    forged = json.loads(json.dumps(output)); forged['metrics']['duplicate-request']['presentations'] = 2
    rejected(lambda: measured(forged, names))
    rejected(lambda: main_bytes(scene.replace(b'position = Vector2(2200, 900)', b'position = Vector2(2199, 900)', 1), baseline))
    missing = json.loads(json.dumps(output)); del missing['metrics']['default']
    rejected(lambda: measured(missing, names))
    mutated = json.loads(json.dumps(parity)); mutated['truthChecks'][0]['after'] += ' '
    rejected(lambda: truth_checks(mutated))
    return ['altered proposal', 'altered PCM', 'forged/repeated presentation', 'changed retained geometry',
            'missing measured case', 'mutated protected truth']


def main():
    """Check scope/PCM/import/product boundary, then actual reviewed output, parity, native and packaging evidence."""
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--reviewed', action='store_true')
    args = parser.parse_args()
    proposal_data = (ROOT / 'audio-direction/container-cloth-owner-scope.json').read_bytes(); proposal_bytes(proposal_data)
    proposal = json.loads(proposal_data)
    authority = load('audio-direction/cask-cover-runtime-authority.json')
    require(authority['owner_statement'] == 'approved' and authority['status'] == 'exact_proposal_scope_authorized', 'Missing owner grant')
    bindings(authority['proposal_bindings'] + [authority['publication_authority']])
    allowed = MODIFIED | set(proposal['implementation_files']['new']) | {
        'audio-direction/cask-cover-runtime-authority.json', 'audio-direction/cask-cover-runtime-verification.json'}
    changed = subprocess.check_output(['git', 'diff', '--name-only', BASE], cwd=ROOT, text=True).splitlines()
    extra = subprocess.check_output(['git', 'ls-files', '--others', '--exclude-standard'], cwd=ROOT, text=True).splitlines()
    outside = [name for name in changed + extra if name not in allowed and not name.startswith('audio-direction/cask-cover-runtime-review/')]
    require(not outside, 'Outside approved boundary: ' + str(outside))
    bindings([proposal['protected_repository_snapshot']['local_exception']])
    bindings([row for row in proposal['unchanged_inspected_inputs'] if row['path'] not in MODIFIED])
    bindings(proposal['rights']['records'] + proposal['retained_evidence'])
    accepted = load('audio-direction/material-audition/owner-acceptance.json')['accepted_material_bindings']
    require(len(accepted) == 82, 'Lost accepted material membership'); bindings(accepted)
    # Protect every baseline tracked input, including uninspected inventory/save/perception and historical evidence.
    old = subprocess.check_output(['git', 'ls-tree', '-r', BASE], cwd=ROOT, text=True).splitlines()
    for entry in old:
        metadata, name = entry.split('\t', 1)
        if name in MODIFIED:
            continue
        content = (ROOT / name).read_bytes()
        blob = hashlib.sha1(b'blob ' + str(len(content)).encode() + b'\0' + content).hexdigest()
        require(blob == metadata.split()[2], 'Changed protected input: ' + name)
    with wave.open(str(ROOT / 'game/assets/audio/cask-cover/A02-C01.wav'), 'rb') as stream:
        require((stream.getnchannels(), stream.getsampwidth(), stream.getframerate(), stream.getnframes()) == (2, 3, 48000, 180000), 'Wrong source format')
        pcm = stream.readframes(stream.getnframes()); pcm_bytes(pcm)
    with wave.open(str(ROOT / proposal['proposed_resources'][0]['source']['path']), 'rb') as stream:
        stream.setpos(48000); require(stream.readframes(180000) == pcm, 'Not exact approved accepted-source frame range')
    imported = (ROOT / 'game/assets/audio/cask-cover/A02-C01.wav.import').read_text()
    require(all(value in imported for value in ('compress/mode=0', 'edit/loop_mode=0', 'edit/normalize=false',
                'edit/trim=false', 'force/mono=false', 'force/max_rate=false', 'force/8_bit=false')), 'Changed import/gain/loop')
    scene = (ROOT / 'game/Main.tscn').read_bytes()
    baseline = subprocess.check_output(['git', 'show', BASE + ':game/Main.tscn'], cwd=ROOT); main_bytes(scene, baseline)
    presets = (ROOT / 'game/export_presets.cfg').read_text().replace(
        ',assets/audio/cask-cover/*.txt,assets/audio/cask-cover/*.json,assets/audio/cask-cover/*.html', '')
    require(presets.encode() == subprocess.check_output(['git', 'show', BASE + ':game/export_presets.cfg'], cwd=ROOT), 'Changed other export options')
    if not args.reviewed:
        print('PASS: approved cask-cover grant, exact A02 extraction/import and retained product boundary'); return
    verification = load('audio-direction/cask-cover-runtime-verification.json')
    require(verification['status'] in ('runtime_evidence_reviewed_full_quality_pending', 'approved_scope_implemented_and_verified'), 'Unreviewed successor')
    bindings(verification['runtime_bindings'] + verification['evidence_bindings'])
    names = proposal['verification_required']['master_output_cases']
    output = load('audio-direction/cask-cover-runtime-review/output/measurements.json'); measured(output, names)
    master = gzip.decompress((REVIEW / 'output/master.wav.gz').read_bytes())
    require(sha(master) == output['wave_sha256'], 'Changed actual Master bytes')
    require(sha((REVIEW / 'output/timeline.json').read_bytes()) == output['timeline_sha256'], 'Changed live timeline')
    native = gzip.decompress((REVIEW / 'native/source.pcm16.gz').read_bytes())
    require(sha(native) == output['native_pcm16_sha256'], 'Changed native decode')
    with wave.open(io.BytesIO(master), 'rb') as stream:
        require(stream.getnframes() >= 120 * 48000, 'Truncated actual Master')
    modes = proposal['verification_required']['parity_modes']
    saves = [(REVIEW / 'parity' / (mode + '.json.save.json')).read_bytes() for mode in modes]
    rows = [json.loads((REVIEW / 'parity' / (mode + '.json')).read_text()) for mode in modes]
    require(all(data == saves[0] for data in saves) and all(row['transcript'] == rows[0]['transcript'] for row in rows), 'Changed complete save/transcript parity')
    require(all(row['commits'] == row['presentations'] == 7 for row in rows[:-1])
            and rows[-1]['commits'] == rows[-1]['presentations'] == 0, 'Changed exact schedule/no-action control')
    for row in rows[:-1]:
        truth_checks(row)
    views = load('audio-direction/cask-cover-runtime-review/native/views.json')
    require(any('actual native focus loss' in line for line in views['checks']), 'Missing native focus/keyboard evidence')
    for size in (1280, 1920):
        for name in ('contact', 'captions-off', 'credits', 'history'):
            require((REVIEW / 'views' / f'{name}-{size}.png').is_file(), 'Missing viewport')
    package = load('audio-direction/cask-cover-runtime-review/packaging/inventory.json')
    require(package['native_streams'] == 35 and package['notices'] == 20 and package['resource_parity'], 'Missing streams/notices')
    subset = REVIEW / 'packaging/audio-subset.zip'
    require(sha(subset.read_bytes()) == package['retained_subset_sha256'], 'Changed package bytes')
    with ZipFile(subset) as archive:
        require(len(archive.namelist()) == 90, 'Lost packaged entries')
        for row in package['notice_bindings']:
            require(sha(archive.read(row['path'])) == row['sha256'], 'Changed exported notice')
    probes = rejection_probes(proposal_data, pcm, scene, baseline, output, names, rows[0])
    require(probes == verification['rejection_probes'], 'Lost mutation rejection membership')
    if verification['status'] == 'approved_scope_implemented_and_verified':
        full = load('audio-direction/cask-cover-runtime-review/quality/summary.json')
        require(full['status'] == 'passed' and len(full['stages']) == 55 and len(full['cask_modes']) == 6, 'Missing full current quality')
        require(sha((REVIEW / 'quality/executed-runner.py').read_bytes()) == full['inputs']['quality.py'], 'Changed executed quality runner')
        for stage in full['stages']:
            require(stage['attempts'][-1]['exit_code'] == 0 and stage['attempts'][-1]['completion_seen'], 'Incomplete quality stage')
            for attempt in stage['attempts']:
                require(sha(gzip.decompress((REVIEW / 'quality' / (attempt['log'] + '.gz')).read_bytes())) == attempt['sha256'], 'Changed full quality log')
    print('PASS: reviewed approved cask successor; exact A02, retained boundary, twenty Master cases, six-mode full parity/truth, native UI/package and six rejection probes')


if __name__ == '__main__':
    main()
