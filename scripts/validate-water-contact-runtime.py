#!/usr/bin/env python3
"""Validate the authorized water successor without changing frozen predecessor evidence."""
import argparse
import gzip
import hashlib
import json
from pathlib import Path
import re
import subprocess
import wave
import io

ROOT = Path(__file__).resolve().parents[1]
BASE = '7b8dd659d409974126790b8a417533bded9c2e5e'
REVIEW = ROOT / 'audio-direction/water-contact-runtime-review'
PROPOSAL_SHA = '9196f5e82afb3011bf44d553ce99b8f57d347216d86c36b4834b898143f4f451'
PCM_SHA = '553260f239cce9bd4c5fcb24009512274838b6a3a8faeaffd7fe72f563bf12ea'
MODIFIED = {'game/Main.tscn', 'game/MaterialSoundCaptionPanel.cs', 'game/MaterialAudioCreditsPanel.cs',
            'quality.py', 'game/export_presets.cfg', 'CONTINUATION.json', 'HANDOVER.md', 'game/project.godot'}


def require(value, message):
    """Fail closed on changed authority, product boundaries or incomplete measured evidence."""
    if not value:
        raise ValueError(message)


def sha(data):
    """Bind exact bytes rather than timestamps or file sizes."""
    return hashlib.sha256(data).hexdigest()


def load(path):
    """Read a repository JSON record."""
    return json.loads((ROOT / path).read_text())


def bindings(rows):
    """Require every named retained or current evidence input to remain byte-identical."""
    for row in rows:
        require(sha((ROOT / row['path']).read_bytes()) == row['sha256'], 'Changed binding: ' + row['path'])


def proposal_bytes(data):
    """An owner grant covers precisely the published proposal."""
    require(sha(data) == PROPOSAL_SHA, 'Changed approved proposal')


def pcm_bytes(data):
    """Preserve the approved slice including silence, fades and gain."""
    require(len(data) == 432000 and sha(data) == PCM_SHA, 'Changed approved PCM slice')


def main_bytes(data, baseline):
    """Remove only the four named new nodes/resources; require all prior Main bytes modulo blanks."""
    text = data.decode()
    require(text.count('id="water_') == 4, 'Unexpected water resource membership')
    text = re.sub(r'^\[ext_resource[^\n]*id="water_[0-3]"\]\n', '', text, flags=re.M)
    for name in ('OpeningRegionStreamWaterContactCoordinator', 'StreamWaterContactPanel',
                 'OpeningRegionWaterContactAudioController', 'WaterContactRuntimeSmokeCheck'):
        pattern = r'\[node name="' + name + r'"[^\n]*\]\n(?:(?!\[node)[\s\S])*?(?=\[node|\Z)'
        text, count = re.subn(pattern, '', text)
        require(count == 1, 'Missing/extra approved Main node: ' + name)
    text = re.sub(r'load_steps=(\d+)', lambda m: 'load_steps=' + str(int(m[1]) - 4), text, count=1)
    require([line for line in text.splitlines() if line.strip()] ==
            [line for line in baseline.decode().splitlines() if line.strip()], 'Changed retained Main/geometry/order')


def measured(record):
    """Require all sixteen actual cases with exact admission/presentation ownership."""
    names = load('audio-direction/water-tool-owner-scope.json')['verification_required']['master_output_cases']
    require(record['native_exact'] and record['cases'] == 16 and set(record['metrics']) == set(names), 'Missing measured output')
    for name, row in record['metrics'].items():
        expected = 0 if name in ('unsupported-request', 'overlay-rejection') else 1
        require(row['commits'] == expected and row['presentations'] == expected, 'Forged/extra commit presentation: ' + name)


def rejection_probes(proposal, pcm, main, baseline, output):
    """Exercise the same real validators against changed inputs, never change repository evidence."""
    def rejected(callback):
        try:
            callback()
        except (ValueError, KeyError, FileNotFoundError):
            return
        raise ValueError('Rejection probe incorrectly passed')
    rejected(lambda: proposal_bytes(proposal + b' '))
    rejected(lambda: pcm_bytes(bytes([pcm[0] ^ 1]) + pcm[1:]))
    forged = json.loads(json.dumps(output)); forged['metrics']['duplicate-request']['presentations'] = 2
    rejected(lambda: measured(forged))
    rejected(lambda: main_bytes(main.replace(b'1736', b'1735', 1), baseline))
    missing = json.loads(json.dumps(output)); del missing['metrics']['default']
    rejected(lambda: measured(missing))
    return ['altered proposal', 'altered PCM', 'forged/repeated presentation', 'changed retained geometry', 'missing measured case']


def main():
    """Check scope, live evidence, complete parity, native views, export bytes and rejection probes."""
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--reviewed', action='store_true')
    args = parser.parse_args()
    proposal_data = (ROOT / 'audio-direction/water-tool-owner-scope.json').read_bytes(); proposal_bytes(proposal_data)
    proposal = json.loads(proposal_data)
    authority = load('audio-direction/water-contact-runtime-authority.json')
    require(authority['owner_statement'] == 'scope approved' and authority['status'] == 'exact_proposal_scope_authorized', 'Missing owner scope grant')
    bindings(authority['proposal_bindings'] + [authority['publication_authority']])
    allowed = MODIFIED | set(proposal['implementation_files']['new']) | {
        'audio-direction/water-tool-owner-scope.md', 'audio-direction/water-tool-owner-scope.json',
        'audio-direction/water-contact-runtime-authority.json', 'audio-direction/water-contact-runtime-verification.json'}
    changed = subprocess.check_output(['git', 'diff', '--name-only', BASE], cwd=ROOT, text=True).splitlines()
    extra = subprocess.check_output(['git', 'ls-files', '--others', '--exclude-standard'], cwd=ROOT, text=True).splitlines()
    require(all(name in allowed or name.startswith('audio-direction/water-contact-runtime-review/') for name in changed + extra),
            'Outside approved successor boundary: ' + str([name for name in changed + extra if name not in allowed and not name.startswith('audio-direction/water-contact-runtime-review/')]))
    require(sha((ROOT / 'game/project.godot').read_bytes()) == proposal['protected_repository_snapshot']['local_exception']['sha256'], 'Changed local editor exception')
    bindings([row for row in proposal['unchanged_inspected_inputs'] if row['path'] not in MODIFIED])
    bindings(proposal['rights']['records'] + proposal['retained_evidence'])
    accepted = load('audio-direction/material-audition/owner-acceptance.json')['accepted_material_bindings']
    require(len(accepted) == 82, 'Lost accepted material bindings')
    bindings(accepted)
    retained = load('audio-direction/material-runtime-verification.json')['evidence_bindings']
    excluded = load('audio-direction/material-runtime-review/publication/authority.json')['excluded_evidence_bindings']
    bindings([row for row in retained if row not in excluded])
    with wave.open(str(ROOT / 'game/assets/audio/water-contact/A01-W01.wav'), 'rb') as stream:
        require((stream.getnchannels(), stream.getsampwidth(), stream.getframerate(), stream.getnframes()) == (2, 3, 48000, 72000), 'Wrong source format')
        pcm = stream.readframes(stream.getnframes()); pcm_bytes(pcm)
    with wave.open(str(ROOT / proposal['proposed_resources'][0]['source']['path']), 'rb') as stream:
        stream.setpos(48000); require(stream.readframes(72000) == pcm, 'Not the authorized accepted raw-frame interval')
    imported = (ROOT / 'game/assets/audio/water-contact/A01-W01.wav.import').read_text()
    require(all(value in imported for value in ('compress/mode=0', 'edit/loop_mode=0', 'edit/normalize=false',
                'edit/trim=false', 'force/mono=false', 'force/max_rate=false', 'force/8_bit=false')), 'Changed import bytes/loop/gain')
    scene = (ROOT / 'game/Main.tscn').read_bytes()
    baseline = subprocess.check_output(['git', 'show', BASE + ':game/Main.tscn'], cwd=ROOT)
    main_bytes(scene, baseline)
    if args.reviewed:
        verification = load('audio-direction/water-contact-runtime-verification.json')
        require(verification['status'] in ('runtime_evidence_reviewed_full_quality_pending', 'approved_scope_implemented_and_verified'), 'Unreviewed successor')
        bindings(verification['runtime_bindings'] + verification['evidence_bindings'])
        output = load('audio-direction/water-contact-runtime-review/output/measurements.json'); measured(output)
        master = gzip.decompress((REVIEW / 'output/master.wav.gz').read_bytes())
        require(sha(master) == output['wave_sha256'], 'Changed captured Master bytes')
        timeline = (REVIEW / 'output/timeline.json').read_bytes()
        require(sha(timeline) == output['timeline_sha256'], 'Changed actual capture timeline')
        native = gzip.decompress((REVIEW / 'native/source.pcm16.gz').read_bytes())
        require(sha(native) == output['native_pcm16_sha256'], 'Changed native decode')
        with wave.open(io.BytesIO(master), 'rb') as stream:
            require(stream.getnframes() >= 64 * 48000, 'Truncated actual Master')
        modes = ('default', 'quiet', 'master-muted', 'effects-muted', 'missing-stream', 'no-action')
        saves = [(REVIEW / 'parity' / (mode + '.json.save.json')).read_bytes() for mode in modes]
        rows = [json.loads((REVIEW / 'parity' / (mode + '.json')).read_text()) for mode in modes]
        require(all(data == saves[0] for data in saves) and all(row['transcript'] == rows[0]['transcript'] for row in rows), 'Changed complete saves/transcripts')
        require(all(row['commits'] == row['presentations'] == 5 for row in rows[:-1]) and rows[-1]['commits'] == rows[-1]['presentations'] == 0, 'Changed explicit schedule/no-action')
        views = load('audio-direction/water-contact-runtime-review/native/views.json')
        require(any('actual native focus loss' in line for line in views['checks']), 'Missing native focus/keyboard/views')
        for size in (1280, 1920):
            for name in ('contact', 'captions-off', 'credits', 'history'):
                require((REVIEW / 'views' / f'{name}-{size}.png').is_file(), 'Missing native viewport')
        packaging = load('audio-direction/water-contact-runtime-review/packaging/inventory.json')
        require(packaging['native_streams'] == 34 and packaging['notices'] == 17 and packaging['resource_parity']
                and packaging['retained_subset_entries'] == 85, 'Missing resource/notice packaging')
        probes = rejection_probes(proposal_data, pcm, scene, baseline, output)
        require(probes == verification['rejection_probes'], 'Changed rejection membership')
        if verification['status'] == 'approved_scope_implemented_and_verified':
            full = load('audio-direction/water-contact-runtime-review/quality/summary.json')
            require(full['status'] == 'passed' and len(full['water_modes']) == 6, 'Missing full current quality')
            require(len(full['stages']) == 48, 'Incomplete full quality membership')
            executed = (REVIEW / 'quality/executed-runner.py').read_bytes()
            require(sha(executed) == full['inputs']['quality.py'], 'Changed executed runner source')
            resume = load('audio-direction/water-contact-runtime-review/quality/resume-verification.json')
            require(resume['status'] == 'passed' and resume['ordinary_pipeline_ast_unchanged']
                    and resume['changed_command_rejected'] and resume['real_completed_stages'] == 38
                    and resume['current_runner_sha256'] == sha((ROOT / 'quality.py').read_bytes()), 'Unverified resume-only repair')
            for stage in full['stages']:
                require(stage['attempts'][-1]['exit_code'] == 0 and stage['attempts'][-1]['completion_seen'], 'Incomplete quality stage')
                for attempt in stage['attempts']:
                    log = REVIEW / 'quality' / (attempt['log'] + '.gz')
                    require(sha(gzip.decompress(log.read_bytes())) == attempt['sha256'], 'Changed complete quality log')
        print('PASS: reviewed approved water successor; exact PCM, retained boundary, sixteen Master cases, six-mode complete parity, native UI/export and five rejection probes')
    else:
        print('PASS: approved water authority, exact extraction/import and retained repository/Main boundary')


if __name__ == '__main__':
    main()
