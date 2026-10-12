#!/usr/bin/env python3
"""Validate the bounded granted rain successor and exact retained owners without modifying them."""
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
REVIEW = ROOT / 'audio-direction/rain-runtime-review'
BASE = 'da2826ea8fb49a0464cf177c395de12b19db7998'
PROPOSAL_SHA = '2f95ff884df36caea2ccdd15404e522f24b878fd32377782cfb73dca6b22536e'
MODES = ['default', 'quiet', 'master-muted', 'ambience-muted', 'missing-stream', 'captions-off', 'rain-disabled']


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
    require(text.count('id="rain_') == 3, 'Wrong rain resource inventory')
    text = re.sub(r'^\[ext_resource[^\n]*id="rain_[^"]+"\]\n', '', text, flags=re.M)
    for name in ('OpeningRegionRainAudioSource', 'OpeningRegionRainAudioController', 'RainAudioRuntimeSmokeCheck'):
        text, count = re.subn(r'\[node name="' + name + r'"[^\n]*\]\n(?:(?!\[node)[\s\S])*?(?=\[node|\Z)', '', text)
        require(count == 1, 'Missing/extra rain node')
    text = re.sub(r'load_steps=(\d+)', lambda m: 'load_steps=' + str(int(m[1]) - 3), text, count=1)
    require(normalized(text) == normalized(old('game/Main.tscn').decode()), 'Changed retained Main/geometry/order')


def retained_ui(caption, history, credits, exports):
    """Reverse only twelfth-line storage, rain section/notices and own caption bound."""
    for line in ('    private Label rainLabel = null!;\n', '    private string rain = string.Empty;\n',
                 '        rainLabel = new() { Name = "Rain", MouseFilter = Control.MouseFilterEnum.Ignore };\n',
                 '        lines.AddChild(rainLabel);\n', '        rainLabel.Text = enabled ? rain : string.Empty;\n'):
        caption = caption.replace(line, '')
    caption = caption.replace('panel.OffsetTop = -430', 'panel.OffsetTop = -390').replace(', rainLabel', '').replace(' || rain.Length > 0', '')
    caption = re.sub(r'    /// <summary>Shows only the independent generic woodland rain passage[\s\S]*?(?=    /// <inheritdoc />)', '', caption)
    require(normalized(caption) == normalized(old('game/MaterialSoundCaptionPanel.cs').decode()), 'Changed prior caption owner/timer')
    history = history.replace('    private OpeningRegionRainAudioController? rain;\n', '')
    history = history.replace('        rain = GetNodeOrNull<OpeningRegionRainAudioController>("../../OpeningRegionRainAudioController");\n', '')
    history = history.replace('Reply, footstep, foliage, wind and rain cues', 'Reply, footstep, foliage and wind cues')
    history = re.sub(r'        IReadOnlyList<string> drops =[^\n]*\n        string falling =[^\n]*\n', '', history)
    history = history.replace(' + "\\n\\nNearby rain cues\\n" + falling', '')
    require(normalized(history) == normalized(old('game/PositionalSoundCueHistoryPanel.cs').decode()), 'Changed prior history/focus')
    credits = credits.replace('        text += "\\n\\n" + Godot.FileAccess.GetFileAsString("res://assets/audio/rain/CREDITS.txt");\n', '')
    require(normalized(credits) == normalized(old('game/MaterialAudioCreditsPanel.cs').decode()), 'Changed prior credits')
    exports = exports.replace(',assets/audio/rain/*.txt,assets/audio/rain/*.json,assets/audio/rain/*.html', '')
    require(normalized(exports) == normalized(old('game/export_presets.cfg').decode()), 'Changed export presets')


def contracts(spatial, schedule, controller):
    """Protect the exclusive range, deterministic clock, player, listener and bus authority."""
    require('RangePixels = 256;' in spatial and '< RangePixels * RangePixels' in spatial, 'Range drift')
    require('IntervalSeconds = 20;' in schedule and 'PassageSeconds = 4;' in schedule and 'MaximumDeltaSeconds = 0.1;' in schedule, 'Schedule drift')
    require(all(s in controller for s in ['Bus = "Ambience"', 'Attenuation = 1', 'PanningStrength = 0.6f', 'PitchScale = 1',
                                         'MaxPolyphony = 1', 'AreaMask = 0', 'VolumeLinear = 1', 'ProcessPhysicsPriority = 130']), 'Player/bus drift')
    require('new(1000, 350)' in spatial and 'MaxDistance = 256' in controller and 'player.GlobalPosition = new(1000, 350)' in controller, 'Emitter drift')
    require('MakeCurrent(' not in controller and 'Ducking.Begin' not in controller and 'DistanceGain(' not in controller, 'New hearing/listener/distance/ducking authority')


def parity(rows):
    """Require complete serialized protected truth, save, transcript and actual motion across seven modes."""
    keys = {'objects', 'inventory', 'healerInventory', 'perception', 'knowledge', 'beliefs', 'placements', 'moveEvidence',
            'concealment', 'substitutions', 'contamination', 'contaminationEvidence', 'transcript', 'events', 'time'}
    for row in rows:
        require(row['truthBefore'] == row['truthAfter'] and set(json.loads(row['truthBefore'])) == keys, 'Protected truth drift')
        require(row['truthBefore'] == rows[0]['truthBefore'] and row['motion'] == rows[0]['motion'] and row['transcript'] == rows[0]['transcript'], 'Motion/transcript/truth mode drift')
        require(len(row['motion']) >= 3300 and any(m['start'] != m['end'] for m in row['motion']), 'Actual motion omitted')
        expected = 0 if row['mode'] == 'rain-disabled' else 3
        require(row['starts'] == expected and row['presentations'] == expected, 'Consumed/admitted schedule mismatch')
    saves = [(REVIEW / f'live/{mode}.json.save.json').read_bytes() for mode in MODES]
    require(all(s == saves[0] for s in saves), 'Complete durable save mismatch')


def matching_output(measured, recorded):
    """Reject incomplete or forged native measurements using the same production guard."""
    require(measured == recorded and recorded.get('cases') == 58 and recorded.get('metrics'), 'Native evidence or measurement drift')


def output_only():
    """Reanalyse actual native Master bytes instead of trusting a list of case names."""
    spec = importlib.util.spec_from_file_location('rain_output', ROOT / 'scripts/check-rain-audio-output.py')
    checker = importlib.util.module_from_spec(spec); spec.loader.exec_module(checker)
    with tempfile.TemporaryDirectory(prefix='rain-measure-') as directory:
        path = Path(directory) / 'master.wav'; path.write_bytes(gzip.decompress((REVIEW / 'output/master.wav.gz').read_bytes()))
        measured = checker.measure(path, REVIEW / 'output/timeline.json')
        matching_output(measured, load('audio-direction/rain-runtime-review/output/measurements.json'))


def source_bindings(provenance):
    """Require the owner-qualified retained original and exact deterministic PCM/import/rights."""
    require(provenance['license'] == 'CC0-1.0' and provenance['originals_acquired'] == 0
            and provenance['retained_original_only'], 'Unapproved source/license')
    spec = importlib.util.spec_from_file_location('rain_preparation', ROOT / 'scripts/prepare-rain-passage.py')
    preparation = importlib.util.module_from_spec(spec); spec.loader.exec_module(preparation)
    candidate = REVIEW / 'source/R01-candidate.wav'
    preparation.qualification(provenance, sha(candidate.read_bytes()))
    for row in [provenance['original'], provenance['derivative'], *provenance['rights']]:
        require(sha((ROOT / row['path']).read_bytes()) == row['sha256'], 'Source/rights/derivative hash drift')
    derivative = provenance['derivative']
    require(derivative['frames'] == 192000 and derivative['sample_peak_dbfs'] <= -24
            and derivative['decoded_pcm24_sha256'] == '5041251691a8e912f33abaad27a9ce2747b571f997e0c8e8bdf7bad9c1e49774', 'Derivative format/ceiling drift')
    require((ROOT / derivative['path']).read_bytes() == candidate.read_bytes(), 'Owner-qualified candidate changed')
    import wave
    with wave.open(str(ROOT / derivative['path'])) as reader:
        require((reader.getnframes(), reader.getframerate(), reader.getnchannels(), reader.getsampwidth()) == (192000,48000,2,3), 'PCM format drift')
        pcm = reader.readframes(reader.getnframes())
    require(sha(pcm) == derivative['decoded_pcm24_sha256'] and pcm[:6] == pcm[-6:] == bytes(6), 'PCM/fade drift')
    settings = (ROOT / 'game/assets/audio/rain/R01.wav.import').read_text()
    require(all(x in settings for x in ['edit/loop_mode=0', 'edit/trim=false', 'edit/normalize=false', 'compress/mode=0', 'force/mono=false', 'force/8_bit=false', 'force/max_rate=false']), 'Native import drift')
    require((ROOT / 'game/assets/audio/rain/provenance.json').read_bytes() == (ROOT / 'audio-direction/rain-source/provenance.json').read_bytes(), 'Packaged provenance mismatch')


def evidence_bindings(rows):
    """Bind source, installed inputs and actual retained measurements independently of status fields."""
    for row in rows:
        require(sha((ROOT / row['path']).read_bytes()) == row['sha256'], 'Changed review/installed binding: ' + row['path'])


def rejection_probes(scene, ui, spatial, schedule, controller, provenance, rows):
    """Execute ten independent mutation rejection groups without changing repository inputs."""
    def rejects(action):
        try:
            action()
        except (ValueError, KeyError, FileNotFoundError):
            return
        raise ValueError('Mutation passed')
    rejects(lambda: retained_scene(scene.replace('points = PackedVector2Array(572, 460, 1500, 1400)', 'points = PackedVector2Array(572, 460, 1501, 1400)')))
    changed = copy.deepcopy(provenance); changed['original']['sha256'] = '0' * 64; rejects(lambda: source_bindings(changed))
    for a, b, c in [(spatial.replace('RangePixels = 256;', 'RangePixels = 257;'), schedule, controller),
                    (spatial, schedule.replace('IntervalSeconds = 20;', 'IntervalSeconds = 19;'), controller),
                    (spatial, schedule, controller.replace('Bus = "Ambience"', 'Bus = "Master"'))]:
        rejects(lambda: contracts(a, b, c))
    changed = copy.deepcopy(rows); changed[0]['truthAfter'] += ' '; rejects(lambda: parity(changed))
    rejects(lambda: retained_ui(ui[0].replace('remaining = Math.Max(0, remaining - delta);', 'remaining = 0;'), *ui[1:]))
    rejects(lambda: require(sha(b'forged proposal') == PROPOSAL_SHA, 'Proposal hash drift'))
    measured = load('audio-direction/rain-runtime-review/output/measurements.json')
    rejects(lambda: matching_output(measured, {'status': 'passed'}))
    rejects(lambda: retained_ui(ui[0], ui[1].replace('IReadOnlyList<string> cues =', 'IReadOnlyList<string> forged ='), *ui[2:]))
    rejects(lambda: contracts(spatial.replace('new(1000, 350)', 'new(1001, 350)'), schedule, controller))
    changed = copy.deepcopy(provenance); changed['derivative']['decoded_pcm24_sha256'] = '0' * 64; rejects(lambda: source_bindings(changed))
    bindings = load('audio-direction/rain-runtime-verification.json')['evidence_bindings']
    changed = copy.deepcopy(bindings)
    source_row = next(row for row in changed if row['path'] == 'game/OpeningRegionRainAudioSource.cs')
    source_row['sha256'] = '0' * 64
    rejects(lambda: evidence_bindings(changed))
    return 10


def main():
    """Check current boundary, review evidence and all retained product bytes before closure/publication."""
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--source-only', action='store_true'); parser.add_argument('--reviewed', action='store_true'); parser.add_argument('--output-only', action='store_true')
    args = parser.parse_args()
    if args.output_only:
        output_only(); print('PASS: rain native output reanalysis'); return
    proposal = load('audio-direction/rain-owner-scope.json')
    authority = load('audio-direction/rain-runtime-authority.json')
    for row in authority['proposal']:
        require(sha((ROOT / row['path']).read_bytes()) == row['sha256'], 'Granted proposal changed')
    require(sha((ROOT / 'audio-direction/rain-owner-scope.md').read_bytes()) == PROPOSAL_SHA, 'Proposal changed')
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
    def generated_uid(name):
        # Godot's current import can recreate ignored-by-history sidecars; root quality
        # removes only verified new ones in finally, preserving pre-existing user files.
        path = ROOT / name
        return (name.startswith('game/') and name.endswith('.gd.uid') and not path.is_symlink()
                and path.is_file() and re.fullmatch(rb'uid://[a-z0-9]+\s*', path.read_bytes())
                and (ROOT / name[:-4]).read_bytes() == old(name[:-4]))
    require(all(p in allowed or p.startswith('audio-direction/rain-runtime-review/')
                or generated_uid(p) for p in extra), 'Unapproved extra path')
    scene = (ROOT / 'game/Main.tscn').read_text(); retained_scene(scene)
    ui = [(ROOT / n).read_text() for n in ['game/MaterialSoundCaptionPanel.cs', 'game/PositionalSoundCueHistoryPanel.cs', 'game/MaterialAudioCreditsPanel.cs', 'game/export_presets.cfg']]
    retained_ui(*ui)
    spatial = (ROOT / 'src/Application/Audio/RainSpatialProjection.cs').read_text()
    schedule = (ROOT / 'src/Application/Audio/RainPresentationSchedule.cs').read_text()
    controller = (ROOT / 'game/OpeningRegionRainAudioController.cs').read_text(); contracts(spatial, schedule, controller)
    history = (ROOT / 'src/Application/Audio/RainCueHistory.cs').read_text()
    require('Capacity = 16;' in history and 'IntervalSeconds = 20;' in history, 'History contract drift')
    source = (ROOT / 'game/OpeningRegionRainAudioSource.cs').read_text()
    require(all(x in source for x in ['ReferenceEquals(clearing.Binding, binding)', 'ReferenceEquals(clearing.Layout, layout)', 'CapturePhysical(clearing)', 'boundaries.GetChildCount() == 8', 'layout.OpeningWidthPixels == 96', 'layout.WallThicknessPixels == 8', 'child.Name != "ConcealedRareSpecimen"']), 'Clearing binding guard missing')
    provenance = load('audio-direction/rain-source/provenance.json'); source_bindings(provenance)
    if args.reviewed:
        rows = [load(f'audio-direction/rain-runtime-review/live/{m}.json') for m in MODES]; parity(rows)
        output_only()
        views = load('audio-direction/rain-runtime-review/views/native.json')
        require(views['status'] == 'passed' and views['crossOwnerClear'] and views['missingAudioHistory'] and len(views['bearings']) == 20, 'Missing native UI/bearings/access')
        require(set(r['text'] for r in views['bearings']) == {'Rain falling' + b for b in ['', ' above/left', ' above', ' above/right', ' left', ' right', ' below/left', ' below', ' below/right']}, 'Native bearings/edge omission')
        package = load('audio-direction/rain-runtime-review/packaging/inventory.json')
        require(package['native_streams'] == 39 and package['individual_notices'] == 32 and package['notice_groups'] == 20
                and package['retained_entries_byte_identical'] == 105, 'Wrong package inventory/parity')
        native = load('audio-direction/rain-runtime-review/packaging/native-decode.json')
        require(native['exact'] and native['frames'] == 192000 and native['sample_rate'] == 48000, 'Mounted native conversion failed')
        require([row['preset'] for row in package['presets']] == ['Linux Development', 'macOS Development', 'Windows Development']
                and all(row['native_streams'] == 39 and row['notice_files'] == 32 and row['retained_entries_byte_identical'] == 105 for row in package['presets']), 'Preset packaging evidence missing')
        with ZipFile(REVIEW / 'packaging/audio-subset.zip') as package_zip:
            require(len(package_zip.namelist()) == 110 and len([n for n in package_zip.namelist() if n.endswith('.sample')]) == 39
                    and len([n for n in package_zip.namelist() if n.endswith(('.txt', '.json', '.html'))]) == 32, 'Native archive inventory drift')
            for row in package['entries']:
                require(sha(package_zip.read(row['path'])) == row['sha256'], 'Packaged entry drift')
            with ZipFile(ROOT / 'audio-direction/wind-runtime-review/packaging/audio-subset.zip') as prior:
                require(all(package_zip.read(name) == prior.read(name) for name in prior.namelist()), 'Prior packaged audio changed')
            for name in ['CREDITS.txt', 'provenance.json', 'deed.html']:
                require(package_zip.read('assets/audio/rain/' + name) == (ROOT / 'game/assets/audio/rain' / name).read_bytes(), 'New packaged notice mismatch')
        verification = load('audio-direction/rain-runtime-verification.json')
        evidence_bindings(verification['evidence_bindings'])
        groups = rejection_probes(scene, ui, spatial, schedule, controller, provenance, rows)
        require(groups == verification['rejection_groups'], 'Lost rejection probes')
    print(f'PASS: rain boundary: source/proposal/retained owners; {count} protected tracked inputs' + ('; seven live modes/native/packaging/rejection groups' if args.reviewed else ''))


if __name__ == '__main__':
    main()
