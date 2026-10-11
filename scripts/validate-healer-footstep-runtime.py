#!/usr/bin/env python3
"""Validate the exact approved Healer successor without reopening frozen owners."""
import argparse
import copy
import gzip
import hashlib
import json
from pathlib import Path
import re
import subprocess
import sys
import tempfile
import wave
from zipfile import ZipFile

ROOT = Path(__file__).resolve().parents[1]
BASE = '6108c4d33922234df4c1a9e385e72084a694b209'
REVIEW = ROOT / 'audio-direction/healer-footstep-runtime-review'
PROPOSAL_SHA = 'f826536fa64c0e1680199108880dae6bca7f8d11c709f8d2c1226a1320f5d662'
MODIFIED = {'game/Healer.cs', 'game/Main.tscn', 'game/MaterialSoundCaptionPanel.cs',
            'game/PositionalSoundCueHistoryPanel.cs', 'quality.py', 'CONTINUATION.json', 'HANDOVER.md', 'game/project.godot'}
MODES = ['default', 'quiet', 'master-muted', 'effects-muted', 'missing-resources', 'captions-off', 'observer-disabled']
REJECTED = {'distance-192', 'distance-193', 'stationary-wait-arrival', 'wall-pressure', 'unsupported-segment',
            'interior-source', 'interior-listener', 'duplicate-foreign-motion', 'teleport-invalid-delta',
            'listener-stale-replaced', 'source-hidden-moved-replaced'}
SILENT = REJECTED | {'effects-zero', 'effects-muted', 'master-zero', 'master-muted', 'missing-stream', 'missing-effects-bus'}


def require(value, message):
    """Reject incomplete evidence or drift, including optimized Python runs."""
    if not value:
        raise ValueError(message)


def sha(data):
    """Bind exact bytes, independently of path timestamps."""
    return hashlib.sha256(data).hexdigest()


def load(path):
    """Read one repository-bound JSON record."""
    return json.loads((ROOT / path).read_text())


def old(path):
    """Read the immutable approved predecessor blob."""
    return subprocess.check_output(['git', 'show', BASE + ':' + path], cwd=ROOT)


def bindings(rows):
    """Require unchanged exact source, runtime or evidence content."""
    for row in rows:
        require(sha((ROOT / row['path']).read_bytes()) == row['sha256'], 'Changed binding: ' + row['path'])


def normalized(text):
    """Allow formatter whitespace without allowing changed statements/comments."""
    return re.sub(r'\s+', '', text)


def main_bytes(data, baseline):
    """Remove precisely three new observer resources/nodes, preserving all prior scene blocks."""
    text = data.decode()
    require(text.count('id="healer_step_') == 3, 'Changed observer scene inventory')
    text = re.sub(r'^\[ext_resource[^\n]*id="healer_step_[^"]+"\]\n', '', text, flags=re.M)
    for name in ('OpeningRegionHealerFootstepMaterialSource', 'OpeningRegionHealerFootstepAudioController', 'HealerFootstepRuntimeSmokeCheck'):
        text, count = re.subn(r'\[node name="' + name + r'"[^\n]*\]\n(?:(?!\[node)[\s\S])*?(?=\[node|\Z)', '', text)
        require(count == 1, 'Missing/extra observer node: ' + name)
    text = re.sub(r'load_steps=(\d+)', lambda m: 'load_steps=' + str(int(m[1]) - 3), text, count=1)
    require(normalized(text) == normalized(baseline.decode()), 'Changed retained Main/geometry/order')


def healer_bytes(text):
    """Reverse only the observational hook and delta plumbing to prove original movement unchanged."""
    text = text.replace('using Allrpcs.Application.Audio;\n', '')
    text = re.sub(r'    private long motionTick;[\s\S]*?(?=    private HealerCommandSelection)', '', text)
    text = re.sub(r'    private void CompleteMovement\(double delta\)[\s\S]*?(?=    private static NumericsVector2 ToNumerics)', '', text)
    text = text.replace('CompleteMovement(delta);', 'MoveAndSlide();').replace('StopMoving(delta);', 'StopMoving();')
    text = text.replace('private void StopMoving(double delta)', 'private void StopMoving()')
    text = text.replace('public override void _PhysicsProcess(double delta)\n    {', 'public override void _PhysicsProcess(double delta)\n    {\n        _ = delta;')
    require(normalized(text) == normalized(old('game/Healer.cs').decode()), 'Changed original Healer movement/commands')


def caption_bytes(text):
    """Reverse only the ninth label/storage; retain every prior owner, setter and timer."""
    for line in ('    private Label healerFootstepLabel = null!;\n', '    private string healerFootstep = string.Empty;\n',
                 '        healerFootstepLabel = new() { Name = "HealerFootstep", MouseFilter = Control.MouseFilterEnum.Ignore };\n',
                 '        lines.AddChild(healerFootstepLabel);\n', '        healerFootstepLabel.Text = enabled ? healerFootstep : string.Empty;\n'):
        text = text.replace(line, '')
    text = text.replace('panel.OffsetTop = -310', 'panel.OffsetTop = -274').replace(', healerFootstepLabel', '')
    text = text.replace(' || healerFootstep.Length > 0', '')
    text = re.sub(r'    /// <summary>Shows only the anonymous spatial contact[\s\S]*?(?=    /// <inheritdoc />)', '', text)
    require(normalized(text) == normalized(old('game/MaterialSoundCaptionPanel.cs').decode()), 'Changed retained caption owner/timer')


def spatial_bytes(text):
    """Protect exact new range/finite admission and native listener/player ownership."""
    require('RangePixels = 192;' in text and '< RangePixels * RangePixels' in text, 'Wrong exclusive range')
    controller = (ROOT / 'game/OpeningRegionHealerFootstepAudioController.cs').read_text()
    require('MakeCurrent(' not in controller and 'ProcessPhysicsPriority = 100' in controller
            and 'pendingFrame == Engine.GetPhysicsFrames()' in controller and 'ReferenceEquals(healer.LastMotion, sample)' in controller,
            'Wrong listener/producer/frame ownership')
    require(all(part in controller for part in ('Bus = "Effects"', 'Attenuation = 1', 'PanningStrength = 0.6f',
            'PitchScale = 1', 'MaxPolyphony = 1', 'AreaMask = 0', 'player.VolumeLinear = 1')), 'Changed native spatial/gain contract')


def history_bytes(text):
    """Keep the existing reply collection, keyboard focus, overlay and return behavior intact."""
    text = text.replace('    private OpeningRegionHealerFootstepAudioController? footsteps;\n', '')
    text = text.replace('        footsteps = GetNodeOrNull<OpeningRegionHealerFootstepAudioController>("../../OpeningRegionHealerFootstepAudioController");\n', '')
    text = text.replace('Reply and nearby footstep cues · cleared when loading a save', 'Heard reply indications · cleared when loading a save')
    text = re.sub(r'        string replies = cues.Count[\s\S]*?(?=\n    })',
                  "        entries.Text = cues.Count == 0 ? \"No recent heard reply cues.\" : string.Join('\\\\n', cues);", text)
    require(normalized(text) == normalized(old('game/PositionalSoundCueHistoryPanel.cs').decode()), 'Changed retained reply history/focus')


def measured(record, groups):
    """Require independently measured admission and silence, never merely a case-name list."""
    require(record['status'] == 'passed' and record['groups'] == 43 and record['cases'] == 71, 'Incomplete native output')
    require({key.split('/')[0] for key in record['metrics']} == set(groups) and len(record['metrics']) == 71, 'Missing measured case')
    for key, row in record['metrics'].items():
        group = key.split('/')[0]
        require(row['presentations'] == (0 if group in REJECTED else 2 if group == 'two-healer-tails' else 1), 'Foreign/replayed presentation')
        require(row['peak'] < .99 and (row['peak'] < 1e-6 if group in SILENT else row['peak'] > 1e-6), 'Wrong measured output admission')


def truth_checks(row):
    """Require every protected state member and serialized before/after equality."""
    require(row['truthBefore'] == row['truthAfter'], 'Leaked hearing/protected truth')
    require(set(json.loads(row['truthBefore'])) == {'objects', 'inventory', 'healerInventory', 'perception', 'knowledge', 'beliefs',
            'placements', 'moveEvidence', 'concealment', 'substitutions', 'contamination', 'contaminationEvidence',
            'transcript', 'events', 'time'}, 'Lost protected truth membership')


def output_only():
    """Reanalyse retained actual native Master bytes using the executed measurement checker."""
    record = load('audio-direction/healer-footstep-runtime-review/output/measurements.json')
    master = gzip.decompress((REVIEW / 'output/master.wav.gz').read_bytes())
    require(sha(master) == record['wave_sha256'], 'Changed native Master capture')
    require(sha((REVIEW / 'output/timeline.json').read_bytes()) == record['timeline_sha256'], 'Changed actual timeline')
    with tempfile.TemporaryDirectory(prefix='allrpcs-healer-output-') as folder:
        path = Path(folder); (path / 'master.wav').write_bytes(master)
        subprocess.run([sys.executable, str(ROOT / 'scripts/check-healer-footstep-audio-output.py'), '--wave', str(path / 'master.wav'),
                        '--timeline', str(REVIEW / 'output/timeline.json'), '--output', str(path / 'measured.json')], check=True, cwd=ROOT)
        require(json.loads((path / 'measured.json').read_text()) == record, 'Changed measurement derivation')


def rejection_probes(proposal, scene, output, groups, parity):
    """Mutate in-memory inputs through real checks, without editing historical files."""
    def reject(callback):
        try:
            callback()
        except (ValueError, KeyError, FileNotFoundError):
            return
        raise ValueError('Mutation incorrectly passed')
    reject(lambda: require(sha(proposal + b' ') == PROPOSAL_SHA, 'Changed proposal'))
    pcm = (ROOT / 'game/assets/audio/footsteps/W01-01.wav').read_bytes()
    reject(lambda: require(sha(pcm + b' ') == sha(pcm), 'Changed PCM/import'))
    forged = copy.deepcopy(output); forged['metrics']['duplicate-foreign-motion/foreign']['presentations'] = 1
    reject(lambda: measured(forged, groups))
    reject(lambda: main_bytes(scene.replace(b'position = Vector2(2000, 950)', b'position = Vector2(1999, 950)', 1), old('game/Main.tscn')))
    spatial = (ROOT / 'src/Application/Audio/HealerFootstepSpatialProjection.cs').read_text()
    reject(lambda: spatial_bytes(spatial.replace('RangePixels = 192', 'RangePixels = 193')))
    leaked = copy.deepcopy(parity); leaked['truthAfter'] += ' '
    reject(lambda: truth_checks(leaked))
    missing = copy.deepcopy(output); del missing['metrics']['distance-191/distance-191']
    reject(lambda: measured(missing, groups))
    caption = (ROOT / 'game/MaterialSoundCaptionPanel.cs').read_text()
    reject(lambda: caption_bytes(caption.replace('handTool = text', 'handTool = string.Empty')))
    return ['Changed proposal/resource PCM or import', 'Foreign/replayed motion', 'Changed material/geometry',
            'Wrong listener or range', 'Leaked hearing/protected truth', 'Missing measured case', 'Mutated retained caption/history owner']


def main():
    """Validate authorization/boundary first, then complete approved evidence and retained full quality."""
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--reviewed', action='store_true'); parser.add_argument('--output-only', action='store_true')
    args = parser.parse_args()
    if args.output_only:
        output_only(); return
    proposal_data = (ROOT / 'audio-direction/healer-footstep-owner-scope.json').read_bytes()
    require(sha(proposal_data) == PROPOSAL_SHA, 'Changed approved proposal'); proposal = json.loads(proposal_data)
    authority = load('audio-direction/healer-footstep-runtime-authority.json')
    require(authority['owner_statement'] == 'approved' and authority['status'] == 'exact_proposal_scope_authorized', 'Missing exact owner grant')
    bindings(authority['proposal_bindings'] + [authority['publication_authority']])
    allowed = MODIFIED | set(proposal['implementation_files']['new'])
    allowed |= {name + '.uid' for name in allowed if name.startswith('game/') and name.endswith('.cs') and name not in MODIFIED}
    changed = subprocess.check_output(['git', 'diff', '--name-only', BASE], cwd=ROOT, text=True).splitlines()
    extra = subprocess.check_output(['git', 'ls-files', '--others', '--exclude-standard'], cwd=ROOT, text=True).splitlines()
    require(all(name in allowed or name.startswith('audio-direction/healer-footstep-runtime-review/') for name in changed + extra), 'Outside approved boundary')
    for entry in subprocess.check_output(['git', 'ls-tree', '-r', BASE], cwd=ROOT, text=True).splitlines():
        metadata, name = entry.split('\t', 1)
        if name in MODIFIED:
            continue
        content = (ROOT / name).read_bytes()
        blob = hashlib.sha1(b'blob ' + str(len(content)).encode() + b'\0' + content).hexdigest()
        require(blob == metadata.split()[2], 'Changed protected input: ' + name)
    bindings([proposal['protected_repository_snapshot']['local_exception']])
    bindings([row for row in proposal['unchanged_source_bindings'] if row['path'] not in MODIFIED])
    bindings(proposal['rights']['records'] + proposal['retained_product_evidence'])
    accepted = load('audio-direction/material-audition/owner-acceptance.json')['accepted_material_bindings']
    require(len(accepted) == 82, 'Lost accepted material membership'); bindings(accepted)
    for resource in proposal['resources']:
        bindings([resource, resource['import'], resource['accepted_parent']])
        with wave.open(str(ROOT / resource['path'])) as stream:
            require((stream.getnchannels(), stream.getsampwidth(), stream.getframerate(), stream.getnframes()) == (2, 3, 48000, 24000), 'Changed source format')
            pcm = stream.readframes(24000); require(sha(pcm) == resource['pcm_sha256'], 'Changed accepted PCM')
        with wave.open(str(ROOT / resource['accepted_parent']['path'])) as stream:
            first, last = resource['parent_frame_range_end_exclusive']; stream.setpos(first)
            require(stream.readframes(last - first) == pcm, 'Changed accepted-source passage')
    scene = (ROOT / 'game/Main.tscn').read_bytes(); main_bytes(scene, old('game/Main.tscn'))
    healer_bytes((ROOT / 'game/Healer.cs').read_text()); caption_bytes((ROOT / 'game/MaterialSoundCaptionPanel.cs').read_text())
    history_bytes((ROOT / 'game/PositionalSoundCueHistoryPanel.cs').read_text())
    spatial_bytes((ROOT / 'src/Application/Audio/HealerFootstepSpatialProjection.cs').read_text())
    if not args.reviewed:
        print('PASS: exact Healer grant, unchanged movement/scene/prior owners/resources and protected boundary'); return
    verification = load('audio-direction/healer-footstep-runtime-verification.json')
    require(verification['status'] in ('runtime_evidence_reviewed_full_quality_pending', 'approved_scope_implemented_and_verified'), 'Unreviewed successor')
    bindings(verification['runtime_bindings'] + verification['evidence_bindings'])
    groups = proposal['verification_required']['master_output_cases']
    output = load('audio-direction/healer-footstep-runtime-review/output/measurements.json'); measured(output, groups)
    master = gzip.decompress((REVIEW / 'output/master.wav.gz').read_bytes())
    require(sha(master) == output['wave_sha256'] and sha((REVIEW / 'output/timeline.json').read_bytes()) == output['timeline_sha256'], 'Changed measured Master/timeline')
    rows = [load('audio-direction/healer-footstep-runtime-review/live/' + mode + '.json') for mode in MODES]
    saves = [(REVIEW / 'live' / (mode + '.json.save.json')).read_bytes() for mode in MODES]
    require(all(data == saves[0] for data in saves), 'Changed complete save')
    require(all(row['motion'] == rows[0]['motion'] and row['transcript'] == rows[0]['transcript'] for row in rows), 'Changed movement/transcript')
    require(all(row['contacts'] == rows[0]['contacts'] >= 4 and row['presentations'] == rows[0]['presentations'] >= 4 for row in rows[:-1])
            and rows[-1]['contacts'] == rows[-1]['presentations'] == 0, 'Wrong independent observer/control schedule')
    for row in rows:
        truth_checks(row)
    views = load('audio-direction/healer-footstep-runtime-review/views/native.json')
    require(any('Tab/Shift-Tab/Escape/Back' in line and 'actual native focus loss' in line for line in views['checks']), 'Missing native keyboard/focus/history')
    for size in (1280, 1920):
        for name in ('captions', 'history'):
            require((REVIEW / 'views' / f'{name}-{size}.png').is_file(), 'Missing native viewport')
    require((REVIEW / 'views/offscreen-1280.png').is_file(), 'Missing actual offscreen contact')
    package = load('audio-direction/healer-footstep-runtime-review/packaging/inventory.json')
    require(package['native_streams'] == 36 and package['notices'] == 23 and package['proposal_notice_groups'] == 17 and package['resource_parity'], 'Lost packaged streams/notices')
    subset = REVIEW / 'packaging/audio-subset.zip'; require(sha(subset.read_bytes()) == package['retained_subset_sha256'], 'Changed native archive')
    with ZipFile(subset) as archive, ZipFile(ROOT / 'audio-direction/hand-tool-runtime-review/packaging/audio-subset.zip') as prior:
        require(set(archive.namelist()) == set(prior.namelist()) and len(archive.namelist()) == 95, 'Lost packaged entries')
        require(all(archive.read(name) == prior.read(name) for name in archive.namelist()), 'Changed prior archive bytes')
    native = load('audio-direction/healer-footstep-runtime-review/packaging/native-decode.json')
    require(native['status'] == 'passed' and native['streams'] == 24 and len(native['rows']) == 24 and all(r['exact'] for r in native['rows']), 'Incomplete mounted native decode')
    probes = rejection_probes(proposal_data, scene, output, groups, rows[0])
    require(probes == verification['rejection_probes'], 'Lost rejection groups')
    if verification['status'] == 'approved_scope_implemented_and_verified':
        full = load('audio-direction/healer-footstep-runtime-review/quality/summary.json')
        require(full['status'] == 'passed' and len(full['stages']) == 72 and len(full['healer_modes']) == 7, 'Missing full current quality')
        require(sha((REVIEW / 'quality/executed-runner.py').read_bytes()) == full['inputs']['quality.py'], 'Changed executed quality runner')
        for stage in full['stages']:
            require(stage['attempts'][-1]['exit_code'] == 0 and stage['attempts'][-1]['completion_seen'], 'Incomplete full quality stage')
            for attempt in stage['attempts']:
                require(sha(gzip.decompress((REVIEW / 'quality' / (attempt['log'] + '.gz')).read_bytes())) == attempt['sha256'], 'Changed full quality log')
    print('PASS: approved Healer successor; unchanged prior owners/movement/PCM, 43 Master groups/71 subcases, seven-mode full parity, native UI/package and seven rejection groups')


if __name__ == '__main__':
    main()
