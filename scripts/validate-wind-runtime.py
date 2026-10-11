#!/usr/bin/env python3
"""Validate granted wind preparation and pure contracts; fail closed on incomplete runtime review."""
import argparse
import copy
import hashlib
import importlib.util
import json
from pathlib import Path
import subprocess
import wave

ROOT = Path(__file__).resolve().parents[1]
REVIEW = ROOT / 'audio-direction/wind-runtime-review'
BASE = 'c4f24ea1ae20e2f06b7f5eae951d506ca4ecb2c3'


def require(value, message):
    """Reject missing evidence or input drift even under optimized Python."""
    if not value:
        raise ValueError(message)


def digest(path):
    """Bind exact file bytes."""
    return hashlib.sha256(path.read_bytes()).hexdigest()


def source_only():
    """Check a reproducible candidate without claiming listening or installed runtime completion."""
    authority = json.loads((ROOT / 'audio-direction/wind-runtime-authority.json').read_text())
    require(authority['scope_id'] == 'phase9-positional-wind-runtime-integration', 'Wrong grant')
    for row in authority['proposal']:
        require(digest(ROOT / row['path']) == row['sha256'], 'Frozen proposal drift')
    proposal = json.loads((ROOT / 'audio-direction/wind-owner-scope.json').read_text())
    allowed = set(proposal['implementation_files']['new_exact_paths']) | set(proposal['implementation_files']['narrow_existing_edits'])
    allowed.add('game/project.godot')
    count = 0
    for item in subprocess.check_output(['git', 'ls-tree', '-rz', BASE], cwd=ROOT).split(b'\0'):
        if not item:
            continue
        header, name = item.split(b'\t', 1); path = name.decode()
        if path in allowed:
            continue
        data = (ROOT / path).read_bytes()
        blob = hashlib.sha1(b'blob ' + str(len(data)).encode() + b'\0' + data).hexdigest()
        require(blob == header.split()[2].decode(), 'Protected tracked input drift: ' + path); count += 1
    extra = subprocess.check_output(['git', 'ls-files', '--others', '--exclude-standard'], cwd=ROOT, text=True).splitlines()
    require(all(p in allowed or p.startswith('audio-direction/wind-runtime-review/') for p in extra), 'Unapproved new path')
    provenance = json.loads((ROOT / 'audio-direction/wind-source/provenance.json').read_text())
    require(provenance['originals_acquired'] == 0 and provenance['retained_original_only'], 'New source acquired')
    for row in [provenance['original'], *provenance['rights']]:
        require(digest(ROOT / row['path']) == row['sha256'], 'Source/rights hash drift')
    candidate = REVIEW / 'source/G01-candidate.wav'
    measured = json.loads((REVIEW / 'source/passage-measurements.json').read_text())
    require(digest(candidate) == measured['sha256'], 'Preview hash drift')
    with wave.open(str(candidate)) as reader:
        require((reader.getnframes(), reader.getframerate(), reader.getnchannels(), reader.getsampwidth())
                == (192000, 48000, 2, 3), 'Preview format drift')
        pcm = reader.readframes(reader.getnframes())
    require(hashlib.sha256(pcm).hexdigest() == measured['decoded_pcm24_sha256'], 'Preview PCM drift')
    require(pcm[:6] == b'\0' * 6 and pcm[-6:] == b'\0' * 6, 'Endpoint fade drift')
    peak = max(abs(int.from_bytes(pcm[i:i + 3], 'little', signed=True)) for i in range(0, len(pcm), 3))
    require(0 < peak <= 8388608 * 10 ** (-24 / 20), 'Preview peak ceiling drift')
    spec = importlib.util.spec_from_file_location('wind_preparation', ROOT / 'scripts/prepare-wind-passage.py')
    preparation = importlib.util.module_from_spec(spec); spec.loader.exec_module(preparation)
    qualified = copy.deepcopy(provenance)
    qualified['listening'].update(status='passed', candidate_sha256=digest(candidate), owner_statement='fixture assessment')
    preparation.qualification(qualified, digest(candidate))
    rejected = 0
    mutations = [('status', 'pending_owner_assessment'), ('candidate_sha256', '0' * 64),
                 ('start_frame', 0), ('end_frame_exclusive', 1), ('owner_statement', '')]
    for key, value in mutations:
        changed = copy.deepcopy(qualified); changed['listening'][key] = value
        try:
            preparation.qualification(changed, digest(candidate))
        except ValueError:
            rejected += 1
    require(rejected == 5, 'Listening gate mutation admitted')
    require(provenance['listening']['status'] == 'pending_owner_assessment' and provenance['derivative'] is None,
            'Source-only verification requires unqualified candidate status')
    require(not (ROOT / 'game/assets/audio/wind/G01.wav').exists(), 'Unqualified runtime PCM installed')
    for path in ['game/Main.tscn', 'game/MaterialSoundCaptionPanel.cs', 'game/PositionalSoundCueHistoryPanel.cs',
                 'game/MaterialAudioCreditsPanel.cs', 'game/export_presets.cfg']:
        require((ROOT / path).read_bytes() == subprocess.check_output(['git', 'show', BASE + ':' + path], cwd=ROOT),
                'Runtime wiring changed before source qualification: ' + path)
    spatial = (ROOT / 'src/Application/Audio/WindSpatialProjection.cs').read_text()
    schedule = (ROOT / 'src/Application/Audio/WindPresentationSchedule.cs').read_text()
    history = (ROOT / 'src/Application/Audio/WindCueHistory.cs').read_text()
    require('new(1500, 1400)' in spatial and 'RangePixels = 256;' in spatial
            and '< RangePixels * RangePixels' in spatial, 'Pure source/range drift')
    require('IntervalSeconds = 18;' in schedule and 'PassageSeconds = 4;' in schedule
            and 'MaximumDeltaSeconds = 0.1;' in schedule, 'Pure schedule drift')
    require('Capacity = 16;' in history and 'IntervalSeconds = 18;' in history, 'Pure history drift')
    print(f'PASS: wind source-only boundary; {count} protected tracked inputs; five listening gate rejections; runtime uninstalled')


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--source-only', action='store_true')
    parser.add_argument('--reviewed', action='store_true')
    args = parser.parse_args()
    if args.reviewed:
        raise SystemExit('Wind runtime review is incomplete: source qualification and runtime/native evidence remain required')
    source_only()
