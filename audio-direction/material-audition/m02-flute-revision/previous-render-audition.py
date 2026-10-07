#!/usr/bin/env python3
"""Render complete material auditions through a byte-identical accepted Godot layout in a disposable project."""
from pathlib import Path
import argparse
import hashlib
import json
import os
import shutil
import subprocess
import tempfile
import numpy as np
import soundfile as sf

ROOT = Path(__file__).resolve().parent
REPO = ROOT.parents[1]
RATE = 48000
LIMIT = 2000000000
BUSES = ('Music', 'Ambience', 'Effects', 'Dialogue')


def digest(path):
    """Bind retained input and final output bytes."""
    return hashlib.sha256(path.read_bytes()).hexdigest()


def storage_check(additional=0):
    """Enforce the approved working cap before any large audition exports."""
    assert sum(p.stat().st_size for p in ROOT.rglob('*') if p.is_file()) + additional <= LIMIT


def write_tracks(candidates, destination, smoke=False):
    """Prepare named source-derived category tracks; forbid a partial five-minute series."""
    by_id = {c['id']: c for c in candidates['candidates']}
    length = 4 if smoke else 300
    if not smoke:
        assert len(by_id) == 16 and not candidates['missing_source_ids'], 'Complete sixteen-candidate series required'
    tracks = {bus: np.zeros((length * RATE, 2), dtype=np.float32) for bus in BUSES}

    def add(identity, start, seconds=None, loop=False):
        """Route exact declared components once, retaining silence and relative levels."""
        candidate = by_id[identity]
        components = candidate['routed_components'] or [{
            'bus': candidate['audition_buses'][0], 'path': candidate['path'], 'sha256': candidate['sha256']}]
        for part in components:
            path = REPO / part['path']
            assert digest(path) == part['sha256']
            data, rate = sf.read(path, dtype='float32', always_2d=True)
            assert rate == RATE
            count = min(round((seconds or len(data) / RATE) * RATE), length * RATE - round(start * RATE))
            if loop:
                data = np.tile(data, (int(np.ceil(count / len(data))), 1))[:count]
                # Fade segment boundaries only; circular loop seams keep the authored crossfade.
                ramp = np.linspace(0, 1, RATE)[:, None]
                data[:RATE] *= ramp
                data[-RATE:] *= ramp[::-1]
            else:
                data = data[:count].copy()
                if len(data) < count:
                    count = len(data)
                if count < round(candidate['actual_seconds'] * RATE) and identity.startswith('M'):
                    fade_count = min(2 * RATE, len(data))
                    data[-fade_count:] *= np.linspace(1, 0, fade_count)[:, None]
            begin = round(start * RATE)
            tracks[part['bus']][begin:begin + count] += data[:count]

    if smoke:
        add('B01', 0, seconds=4)
    else:
        for index in range(4):
            start = index * 45
            add('B0' + str(index + 1), start, seconds=45, loop=True)
            for offset in (5, 17, 29):
                add('W0' + str(index + 1), start + offset)
        for identity, start in [('A01', 60), ('A01', 78), ('A02', 182), ('A03', 196), ('A04', 210),
                                ('A05', 228), ('A06', 242), ('M01', 255), ('M02', 278)]:
            add(identity, start, seconds=23 if identity == 'M01' else 22 if identity == 'M02' else None)
    paths = []
    for bus, data in tracks.items():
        if not np.any(data):
            continue
        path = destination / (bus + '.wav')
        storage_check(data.size * 3)
        sf.write(path, data, RATE, subtype='PCM_24')
        paths.append({'bus': bus, 'path': str(path), 'sha256': digest(path)})
    summed = sum(tracks.values())
    peak = float(np.max(np.abs(summed)))
    assert peak < 10 ** (-6 / 20), 'Combined working peak exceeds headroom; edit candidate gains explicitly'
    return paths, float(20 * np.log10(peak)), length


def main():
    """Capture the actual final Master output, retaining logs and conversion details for review."""
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--smoke', action='store_true', help='Four-second B01 tooling check, never a partial owner audition')
    args = parser.parse_args()
    candidates = json.loads((ROOT / 'candidates.json').read_text())
    review = ROOT / ('tooling-check' if args.smoke else 'review')
    review.mkdir(exist_ok=True)
    tracks_dir = review / 'category-tracks'
    tracks_dir.mkdir(exist_ok=True)
    tracks, peak, duration = write_tracks(candidates, tracks_dir, args.smoke)
    record = {'schema_version': 1, 'status': 'tooling_check_only' if args.smoke else 'captured_pending_measurements',
              'measurement_point': 'Godot offline stereo output after the copied Master effects and gain',
              'bus_layout_path': 'game/AudioBusLayout.tres', 'bus_layout_sha256': digest(REPO / 'game/AudioBusLayout.tres'),
              'renderer_script_sha256': digest(ROOT / 'tooling/audition.gd'),
              'python_script_sha256': digest(Path(__file__)), 'candidates_sha256': digest(ROOT / 'candidates.json'),
              'godot_version': subprocess.check_output(['godot', '--version'], text=True).strip(),
              'combined_pre_master_sample_peak_dbfs': peak, 'tracks': tracks, 'outputs': []}
    variants = [('smoke', False, False)] if args.smoke else [
        ('default', False, False), ('reduced-range', True, False), ('music-free', False, True)]
    with tempfile.TemporaryDirectory(prefix='allrpcs-audition-') as workspace:
        project = Path(workspace)
        shutil.copyfile(REPO / 'game/AudioBusLayout.tres', project / 'AudioBusLayout.tres')
        shutil.copyfile(ROOT / 'tooling/audition.gd', project / 'audition.gd')
        (project / 'project.godot').write_text(
            '[application]\nconfig/name="Disposable material audition"\nrun/main_scene="res://audition.tscn"\n'
            '[display]\nwindow/size/viewport_width=64\nwindow/size/viewport_height=64\n'
            '[rendering]\nrenderer/rendering_method="gl_compatibility"\n'
            '[audio]\ndriver/mix_rate=48000\n')
        (project / 'audition.tscn').write_text('[gd_scene load_steps=2 format=3]\n[ext_resource type="Script" path="res://audition.gd" id="1"]\n[node name="Audition" type="Node"]\nscript = ExtResource("1")\n')
        assert digest(project / 'AudioBusLayout.tres') == record['bus_layout_sha256']
        for name, reduced, music_free in variants:
            config = {'tracks': [t for t in tracks if not (music_free and t['bus'] == 'Music')],
                      'reduced_range': reduced, 'mono': False}
            config_path = review / (name + '-config.json')
            config_path.write_text(json.dumps(config, indent=2) + '\n')
            frames = review / (name + '-frames')
            frames.mkdir(exist_ok=True)
            environment = dict(os.environ, ALLRPCS_AUDITION_CONFIG=str(config_path))
            command = ['godot', '--path', str(project), '--write-movie', str(frames / 'capture.png'),
                       '--fixed-fps', '60', '--resolution', '64x64', '--rendering-method', 'gl_compatibility',
                       '--log-file', str(review / (name + '-godot.log')), '--quit-after', str(duration * 60)]
            print('Rendering', name, duration, 'seconds', flush=True)
            storage_check(duration * RATE * 8 + duration * 60 * 2048)
            with (review / (name + '-stdout.log')).open('w') as output:
                subprocess.run(command, env=environment, stdout=output, stderr=subprocess.STDOUT, check=True)
            log = (review / (name + '-stdout.log')).read_text()
            assert 'AUDITION_INPUTS' in log and 'SCRIPT ERROR' not in log and 'ERROR:' not in log, log
            captured = frames / 'capture.wav'
            captured_hash = digest(captured)
            captured_subtype = sf.info(captured).subtype
            path = review / (name + '.wav')
            # Keep review exports in the planned PCM24 format; preserve the raw-capture identity in the record.
            subprocess.run(['ffmpeg', '-v', 'error', '-nostdin', '-y', '-i', str(captured),
                            '-c:a', 'pcm_s24le', str(path)], check=True)
            info = sf.info(path)
            assert abs(info.duration - duration) < .1 and info.samplerate == RATE and info.channels == 2
            record['outputs'].append({'name': name, 'path': str(path.relative_to(REPO)), 'sha256': digest(path),
                                      'seconds': info.duration, 'subtype': info.subtype,
                                      'raw_capture_sha256': captured_hash, 'raw_capture_subtype': captured_subtype,
                                      'export_conversion': 'ffmpeg PCM32 to PCM24; no gain or other processing',
                                      'engine_input_formats': [line for line in log.splitlines() if line.startswith('AUDITION_INPUTS')],
                                      'config_sha256': digest(config_path)})
            # Frames are disposable renderer transport, not artwork or review evidence.
            shutil.rmtree(frames)
            storage_check()
    (review / 'render-record.json').write_text(json.dumps(record, indent=2) + '\n')
    print('Retained', len(record['outputs']), 'actual final mixer captures', flush=True)


if __name__ == '__main__':
    main()
