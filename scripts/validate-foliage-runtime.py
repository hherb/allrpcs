#!/usr/bin/env python3
"""Validate the bounded granted foliage successor and exact retained owners without modifying them."""
import argparse
import copy
import gzip
import hashlib
import importlib.util
import json
from pathlib import Path
import re
import subprocess
import tempfile
from zipfile import ZipFile

ROOT = Path(__file__).resolve().parents[1]
REVIEW = ROOT / 'audio-direction/foliage-runtime-review'
BASE = '55c59c0b'
PROPOSAL_SHA = 'e5cb4a8db63024d4859a5f978be6f98e4a713d035eb06e9258000f8f21e17386'
MODES = ['default', 'quiet', 'master-muted', 'ambience-muted', 'missing-stream', 'captions-off', 'foliage-disabled']


def require(value, message):
    """Reject missing evidence or drift, including optimized interpreter runs."""
    if not value:
        raise ValueError(message)


def sha(data):
    """Bind exact bytes, not path timestamps."""
    return hashlib.sha256(data).hexdigest()


def load(path):
    """Read JSON relative to the current product root."""
    return json.loads((ROOT / path).read_text())


def old(path):
    """Read the exact pre-grant retained blob."""
    return subprocess.check_output(['git', 'show', BASE + ':' + path], cwd=ROOT)


def normalized(text):
    """Allow formatter whitespace while protecting every statement and prior node order."""
    return re.sub(r'\s+', '', text)


def retained_scene(text):
    """Reverse precisely three new siblings/resources before checking the old complete scene."""
    require(text.count('id="foliage_') == 3, 'Wrong foliage resource inventory')
    text = re.sub(r'^\[ext_resource[^\n]*id="foliage_[^"]+"\]\n', '', text, flags=re.M)
    for name in ('OpeningRegionFoliageAudioSource', 'OpeningRegionFoliageAudioController', 'FoliageAudioRuntimeSmokeCheck'):
        text, count = re.subn(r'\[node name="' + name + r'"[^\n]*\]\n(?:(?!\[node)[\s\S])*?(?=\[node|\Z)', '', text)
        require(count == 1, 'Missing/extra foliage node')
    text = re.sub(r'load_steps=(\d+)', lambda m: 'load_steps=' + str(int(m[1]) - 3), text, count=1)
    require(normalized(text) == normalized(old('game/Main.tscn').decode()), 'Changed retained Main/geometry/order')


def retained_ui(caption, history, credits, exports):
    """Reverse only tenth-line storage, foliage section/notices and own caption bound."""
    for line in ('    private Label foliageLabel = null!;\n', '    private string foliage = string.Empty;\n',
                 '        foliageLabel = new() { Name = "Foliage", MouseFilter = Control.MouseFilterEnum.Ignore };\n',
                 '        lines.AddChild(foliageLabel);\n', '        foliageLabel.Text = enabled ? foliage : string.Empty;\n'):
        caption = caption.replace(line, '')
    caption = caption.replace('panel.OffsetTop = -350', 'panel.OffsetTop = -310').replace(', foliageLabel', '').replace(' || foliage.Length > 0', '')
    caption = re.sub(r'    /// <summary>Shows only the independently owned anonymous garden passage[\s\S]*?(?=    /// <inheritdoc />)', '', caption)
    require(normalized(caption) == normalized(old('game/MaterialSoundCaptionPanel.cs').decode()), 'Changed prior caption owner/timer')
    history = history.replace('    private OpeningRegionFoliageAudioController? foliage;\n', '')
    history = history.replace('        foliage = GetNodeOrNull<OpeningRegionFoliageAudioController>("../../OpeningRegionFoliageAudioController");\n', '')
    history = history.replace('Reply, footstep and foliage cues', 'Reply and nearby footstep cues')
    history = re.sub(r'        IReadOnlyList<string> leaves =[^\n]*\n        string rustling =[^\n]*\n', '', history)
    history = history.replace(' + "\\n\\nNearby foliage cues\\n" + rustling', '')
    require(normalized(history) == normalized(old('game/PositionalSoundCueHistoryPanel.cs').decode()), 'Changed prior history/focus')
    credits = credits.replace('        text += "\\n\\n" + Godot.FileAccess.GetFileAsString("res://assets/audio/foliage/CREDITS.txt");\n', '')
    require(normalized(credits) == normalized(old('game/MaterialAudioCreditsPanel.cs').decode()), 'Changed prior credits')
    exports = exports.replace(',assets/audio/foliage/*.txt,assets/audio/foliage/*.json,assets/audio/foliage/*.html', '')
    require(normalized(exports) == normalized(old('game/export_presets.cfg').decode()), 'Changed export presets')


def contracts(spatial, schedule, controller):
    """Protect the exclusive range, deterministic clock, player, listener and bus authority."""
    require('RangePixels = 192;' in spatial and '< RangePixels * RangePixels' in spatial, 'Range drift')
    require('IntervalSeconds = 12;' in schedule and 'PassageSeconds = 3;' in schedule and 'MaximumDeltaSeconds = 0.1;' in schedule, 'Schedule drift')
    require(all(s in controller for s in ['Bus = "Ambience"', 'Attenuation = 1', 'PanningStrength = 0.6f', 'PitchScale = 1',
                                         'MaxPolyphony = 1', 'AreaMask = 0', 'VolumeLinear = 1', 'ProcessPhysicsPriority = 110']), 'Player/bus drift')
    require('MakeCurrent(' not in controller and 'Ducking.Begin' not in controller and 'DistanceGain(' not in controller, 'New hearing/listener/distance/ducking authority')


def parity(rows):
    """Require complete serialized protected truth, save, transcript and actual motion across seven modes."""
    keys = {'objects', 'inventory', 'healerInventory', 'perception', 'knowledge', 'beliefs', 'placements', 'moveEvidence',
            'concealment', 'substitutions', 'contamination', 'contaminationEvidence', 'transcript', 'events', 'time'}
    for row in rows:
        require(row['truthBefore'] == row['truthAfter'] and set(json.loads(row['truthBefore'])) == keys, 'Protected truth drift')
        require(row['truthBefore'] == rows[0]['truthBefore'] and row['motion'] == rows[0]['motion'] and row['transcript'] == rows[0]['transcript'], 'Motion/transcript/truth mode drift')
        require(len(row['motion']) >= 2200 and any(m['start'] != m['end'] for m in row['motion']), 'Actual motion omitted')
        expected = 0 if row['mode'] == 'foliage-disabled' else 3
        require(row['starts'] == expected and row['presentations'] == expected, 'Consumed/admitted schedule mismatch')
    saves = [(REVIEW / f'live/{mode}.json.save.json').read_bytes() for mode in MODES]
    require(all(s == saves[0] for s in saves), 'Complete durable save mismatch')


def matching_output(measured, recorded):
    """Reject incomplete or forged native measurements using the same production guard."""
    require(measured == recorded and recorded.get('cases') == 46 and recorded.get('metrics'), 'Native evidence or measurement drift')


def output_only():
    """Reanalyse actual native Master bytes instead of trusting a list of case names."""
    spec = importlib.util.spec_from_file_location('foliage_output', ROOT / 'scripts/check-foliage-audio-output.py')
    checker = importlib.util.module_from_spec(spec); spec.loader.exec_module(checker)
    with tempfile.TemporaryDirectory(prefix='foliage-measure-') as directory:
        path = Path(directory) / 'master.wav'; path.write_bytes(gzip.decompress((REVIEW / 'output/master.wav.gz').read_bytes()))
        measured = checker.measure(path, REVIEW / 'output/timeline.json')
        matching_output(measured, load('audio-direction/foliage-runtime-review/output/measurements.json'))


def source_bindings(provenance):
    """Require the selected single original, creator/deed captures, listening and exact derivative."""
    require(provenance['license'] == 'CC0-1.0' and provenance['originals_acquired'] == 1, 'Unapproved source/license')
    require(provenance['listening']['status'] == 'passed' and provenance['listening']['start_frame'] == 384000, 'Selected listening window missing')
    for row in [provenance['original'], provenance['derivative'], *provenance['rights']]:
        require(sha((ROOT / row['path']).read_bytes()) == row['sha256'], 'Source/rights/derivative hash drift')
    require(provenance['derivative']['frames'] == 144000 and provenance['derivative']['sample_peak_dbfs'] <= -18, 'Derivative format/ceiling drift')
    require((ROOT / 'game/assets/audio/foliage/provenance.json').read_bytes() == (ROOT / 'audio-direction/foliage-source/provenance.json').read_bytes(), 'Packaged provenance mismatch')


def rejection_probes(scene, ui, spatial, schedule, controller, provenance, rows):
    """Execute seven independent mutation rejection groups without changing repository inputs."""
    def rejects(action):
        try:
            action()
        except (ValueError, KeyError, FileNotFoundError):
            return
        raise ValueError('Mutation passed')
    rejects(lambda: retained_scene(scene.replace('position = Vector2(-68, 28)', 'position = Vector2(-67, 28)')))
    changed = copy.deepcopy(provenance); changed['original']['sha256'] = '0' * 64; rejects(lambda: source_bindings(changed))
    for a, b, c in [(spatial.replace('RangePixels = 192;', 'RangePixels = 193;'), schedule, controller),
                    (spatial, schedule.replace('IntervalSeconds = 12;', 'IntervalSeconds = 11;'), controller),
                    (spatial, schedule, controller.replace('Bus = "Ambience"', 'Bus = "Master"'))]:
        rejects(lambda: contracts(a, b, c))
    changed = copy.deepcopy(rows); changed[0]['truthAfter'] += ' '; rejects(lambda: parity(changed))
    rejects(lambda: retained_ui(ui[0].replace('remaining = Math.Max(0, remaining - delta);', 'remaining = 0;'), *ui[1:]))
    rejects(lambda: require(sha(b'forged proposal') == PROPOSAL_SHA, 'Proposal hash drift'))
    measured = load('audio-direction/foliage-runtime-review/output/measurements.json')
    rejects(lambda: matching_output(measured, {'status': 'passed'}))
    rejects(lambda: retained_ui(ui[0], ui[1].replace('IReadOnlyList<string> cues =', 'IReadOnlyList<string> forged ='), *ui[2:]))
    return 7


def main():
    """Check current boundary, review evidence and all retained product bytes before closure/publication."""
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--reviewed', action='store_true'); parser.add_argument('--output-only', action='store_true')
    args = parser.parse_args()
    if args.output_only:
        output_only(); print('PASS: foliage native output reanalysis'); return
    proposal = load('audio-direction/foliage-owner-scope.json')
    authority = load('audio-direction/foliage-runtime-authority.json')
    for row in authority['proposal']:
        require(sha((ROOT / row['path']).read_bytes()) == row['sha256'], 'Granted proposal changed')
    require(sha((ROOT / 'audio-direction/foliage-owner-scope.md').read_bytes()) == PROPOSAL_SHA, 'Proposal changed')
    allowed = set(proposal['implementation_files']['new_exact_paths']) | set(proposal['implementation_files']['narrow_existing_edits'])
    allowed.add('game/project.godot')
    tracked = subprocess.check_output(['git', 'ls-tree', '-rz', BASE], cwd=ROOT).split(b'\0')
    count = 0
    for item in tracked:
        if not item:
            continue
        header, raw_name = item.split(b'\t', 1); name = raw_name.decode()
        if name in allowed:
            continue
        data = (ROOT / name).read_bytes()
        blob = hashlib.sha1(b'blob ' + str(len(data)).encode() + b'\0' + data).hexdigest()
        require(blob == header.split()[2].decode(), 'Protected tracked input changed: ' + name); count += 1
    extra = subprocess.check_output(['git', 'ls-files', '--others', '--exclude-standard'], cwd=ROOT, text=True).splitlines()
    require(all(p in allowed or p.startswith('audio-direction/foliage-runtime-review/') for p in extra), 'Unapproved extra path')
    scene = (ROOT / 'game/Main.tscn').read_text(); retained_scene(scene)
    ui = [(ROOT / n).read_text() for n in ['game/MaterialSoundCaptionPanel.cs', 'game/PositionalSoundCueHistoryPanel.cs', 'game/MaterialAudioCreditsPanel.cs', 'game/export_presets.cfg']]
    retained_ui(*ui)
    spatial = (ROOT / 'src/Application/Audio/FoliageSpatialProjection.cs').read_text()
    schedule = (ROOT / 'src/Application/Audio/FoliagePresentationSchedule.cs').read_text()
    controller = (ROOT / 'game/OpeningRegionFoliageAudioController.cs').read_text(); contracts(spatial, schedule, controller)
    provenance = load('audio-direction/foliage-source/provenance.json'); source_bindings(provenance)
    if args.reviewed:
        rows = [load(f'audio-direction/foliage-runtime-review/live/{m}.json') for m in MODES]; parity(rows)
        output_only()
        require(load('audio-direction/foliage-runtime-review/views/native.json')['status'] == 'passed', 'Missing native UI')
        package = load('audio-direction/foliage-runtime-review/packaging/inventory.json')
        require(package['native_streams'] == 37 and package['individual_notices'] == 26 and package['notice_groups'] == 18
                and package['retained_entries_byte_identical'] == 95, 'Wrong package inventory/parity')
        require(load('audio-direction/foliage-runtime-review/packaging/native-decode.json')['exact'], 'Mounted native conversion failed')
        with ZipFile(REVIEW / 'packaging/audio-subset.zip') as package_zip:
            for row in package['entries']:
                require(sha(package_zip.read(row['path'])) == row['sha256'], 'Packaged entry drift')
            with ZipFile(ROOT / 'audio-direction/healer-footstep-runtime-review/packaging/audio-subset.zip') as prior:
                require(all(package_zip.read(name) == prior.read(name) for name in prior.namelist()), 'Prior packaged audio changed')
            for name in ['CREDITS.txt', 'provenance.json', 'deed.html']:
                require(package_zip.read('assets/audio/foliage/' + name) == (ROOT / 'game/assets/audio/foliage' / name).read_bytes(), 'New packaged notice mismatch')
        verification = load('audio-direction/foliage-runtime-verification.json')
        for row in verification['evidence_bindings']:
            require(sha((ROOT / row['path']).read_bytes()) == row['sha256'], 'Changed review/installed binding')
        groups = rejection_probes(scene, ui, spatial, schedule, controller, provenance, rows)
        require(groups == verification['rejection_groups'], 'Lost rejection probes')
    print(f'PASS: foliage boundary: source/proposal/retained owners; {count} protected tracked inputs' + ('; seven live modes/native/packaging/rejection groups' if args.reviewed else ''))


if __name__ == '__main__':
    main()
