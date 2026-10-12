#!/usr/bin/env python3
"""Measure each independent native rain Master case, including admission and stopped tails."""
import argparse
import hashlib
import json
from pathlib import Path
import subprocess
import wave
import numpy as np

ROOT = Path(__file__).resolve().parents[1]
CASES = ['distance-0', 'distance-64', 'distance-128', 'distance-192', 'distance-255', 'distance-256', 'distance-257',
         'left', 'right', 'mono', 'zoom-08', 'zoom-2', 'camera-offset', 'quiet', 'ambience-half', 'master-half', 'other-bus-zero',
         'ambience-zero', 'ambience-muted', 'master-zero', 'master-muted', 'missing-stream', 'missing-bus', 'captions-off',
         'foreground-duck', 'foreground-recovery', 'enter-range-mid', 'leave-range-mid', 'pause', 'focus-loss', 'reading-overlay', 'text-entry',
         'load-success', 'load-failure', 'teleport', 'listener-stale', 'listener-replaced', 'listener-unsupported', 'source-hidden', 'source-moved',
         'source-replaced', 'clearing-area-drift', 'map-disabled', 'invalid-delta', 'mute-unmute', 'retained-overlap', 'source-unsupported', 'source-disabled', 'source-nonfinite', 'map-drift', 'journal-overlay', 'map-overlay', 'dialogue-overlay', 'customization-overlay', 'settings-overlay', 'missing-stream-repair', 'ambience-unmute', 'master-unmute']
REJECTED = {'distance-256', 'distance-257', 'enter-range-mid', 'listener-stale', 'listener-replaced', 'listener-unsupported',
            'source-hidden', 'source-moved', 'source-replaced', 'clearing-area-drift', 'map-disabled', 'source-unsupported', 'source-disabled', 'source-nonfinite', 'map-drift'}
SILENT = REJECTED | {'ambience-zero', 'ambience-muted', 'master-zero', 'master-muted', 'missing-stream', 'missing-bus', 'missing-stream-repair', 'ambience-unmute', 'master-unmute'}


def require(value, message):
    """Fail incomplete measurements even under Python optimization."""
    if not value:
        raise ValueError(message)


def sha(data):
    """Bind the actual capture and executed timeline bytes."""
    return hashlib.sha256(data).hexdigest()


def measure(wave_path, timeline_path):
    """Reanalyse Master samples and compare native gain, pan, mono and cancellation."""
    with wave.open(str(wave_path)) as reader:
        require((reader.getnchannels(), reader.getframerate(), reader.getsampwidth()) == (2, 48000, 2), 'Wrong native format')
        samples = np.frombuffer(reader.readframes(reader.getnframes()), dtype='<i2').reshape(-1, 2).astype(np.float64) / 32768
    rows = json.loads(timeline_path.read_text())
    require([r['name'] for r in rows] == CASES, 'Missing/reordered independent cases')
    metrics = {}
    for row in rows:
        name = row['name']; start = round(row['start'] * 48000)
        data = samples[start:start + round(row['duration'] * 48000)]
        require(len(data) == 240000, 'Truncated case: ' + name)
        peak = float(np.max(np.abs(data))); rms_channels = np.sqrt(np.mean(data ** 2, axis=0))
        signal = np.flatnonzero(np.max(np.abs(data), axis=1) > 1e-6)
        require(peak < .99, 'Clipping: ' + name)
        require(peak < 1e-6 if name in SILENT else peak > 1e-6, 'Wrong native output admission: ' + name)
        require(row['presentations'] == (0 if name in REJECTED else 1), 'Wrong presentation count: ' + name)
        if name == 'mono':
            require(float(np.max(np.abs(data[:, 0] - data[:, 1]))) < 1e-6, 'Mono channel mismatch')
        if row['cancel_at'] is not None:
            tail = data[round((row['cancel_at'] + .12) * 48000):]
            require(float(np.max(np.abs(tail))) < 1e-6, 'Stopped owner retained/restarted audio: ' + name)
        meter = subprocess.run(['ffmpeg', '-hide_banner', '-f', 's16le', '-ar', '48000', '-ac', '2', '-i', 'pipe:0', '-af', 'loudnorm=I=-23:LRA=7:TP=-1:print_format=json', '-f', 'null', '-'], input=(data * 32768).astype('<i2').tobytes(), capture_output=True, check=True).stderr.decode()
        case_loudness = json.loads(meter[meter.rfind('{'):meter.rfind('}') + 1])
        dbfs = float(20 * np.log10(peak)) if peak > 0 else None
        metrics[name] = dict(loudness=case_loudness, peak_dbfs=dbfs, tail_peak=float(np.max(np.abs(data[-4800:]))), peak=peak, left_rms=float(rms_channels[0]), right_rms=float(rms_channels[1]),
                             rms=float(np.sqrt(np.mean(data ** 2))), presentations=row['presentations'],
                             first_signal_seconds=float(signal[0] / 48000) if len(signal) else None,
                             last_signal_seconds=float(signal[-1] / 48000) if len(signal) else None)
    baseline = metrics['distance-0']['rms']
    for name, expected in [('distance-64', .75), ('distance-128', .5), ('distance-192', .25), ('distance-255', 1/256),
                           ('ambience-half', .5), ('master-half', .5), ('other-bus-zero', 1), ('zoom-08', 1), ('zoom-2', 1),
                           ('captions-off', 1), ('load-failure', 1), ('foreground-duck', 10 ** (-3/20))]:
        ratio = metrics[name]['rms'] / baseline
        tolerance = .05 * expected if name in {'distance-64', 'distance-128', 'distance-192'} else .05
        require(abs(ratio - expected) <= tolerance, f'Wrong relative native gain {name}: {ratio} vs {expected}')
        metrics[name]['ratio_to_center'] = ratio
    for channel in ('left_rms', 'right_rms'):
        metrics['left'][channel + '_relative'] = metrics['left'][channel] / metrics['distance-0'][channel]
        metrics['right'][channel + '_relative'] = metrics['right'][channel] / metrics['distance-0'][channel]
    require(metrics['left']['left_rms_relative'] > metrics['left']['right_rms_relative'], 'Left pan reversed')
    require(metrics['right']['right_rms_relative'] > metrics['right']['left_rms_relative'], 'Right pan reversed')
    recovery = metrics['foreground-recovery']['rms'] / baseline
    require(10 ** (-2/20) < recovery < 1.03, 'Foreground release missing')
    metrics['foreground-recovery']['ratio_to_center'] = recovery
    # Match the installed passage before its four-active-second deadline. The native mixer
    # begins after Play and releases when Stop reaches it; measure that final fade separately.
    with wave.open(str(ROOT / 'game/assets/audio/rain/R01.wav')) as reader:
        pcm = np.frombuffer(reader.readframes(reader.getnframes()), dtype=np.uint8).reshape(-1, 3).astype(np.int32)
    signed = ((pcm[:, 0] | pcm[:, 1] << 8 | pcm[:, 2] << 16) ^ 0x800000) - 0x800000
    native = (signed >> 8).reshape(-1, 2).astype(np.float64) / 32768
    center = rows[0]; data = samples[round(center['start'] * 48000):round((center['start'] + 5) * 48000)]
    length = 1 << (len(data) + len(native) - 1).bit_length()
    correlation = np.fft.irfft(np.fft.rfft(data[:, 0], length) * np.conj(np.fft.rfft(native[:, 0], length)), length)
    offset = int(np.argmax(correlation[:len(data) - len(native) + 1]))
    matched = data[offset:offset + len(native)]
    # Exclude only the source's recorded final 100 ms fade from exact waveform comparison.
    exact_frames = len(native) - 4800
    error = float(np.max(np.abs(matched[:exact_frames] - native[:exact_frames] * .5)))
    require(error <= 8 / 32768, 'Wrong installed native passage before final fade')
    final_fade_error = float(np.max(np.abs(matched[exact_frames:] - native[exact_frames:] * .5)))
    require(float(np.max(np.abs(matched[exact_frames:]))) <= float(np.max(np.abs(native[exact_frames:]))) * .5 + 8 / 32768,
            'Native release exceeds recorded source fade')
    expiry = round((center['trigger'] + 4 + .12) * 48000)
    require(float(np.max(np.abs(data[expiry:]))) < 1e-6, 'Native center exceeded schedule/release deadline')
    meter = subprocess.run(['ffmpeg', '-hide_banner', '-i', str(wave_path), '-af',
                            'loudnorm=I=-23:LRA=7:TP=-1:print_format=json', '-f', 'null', '-'], capture_output=True, text=True, check=True).stderr
    loudness = json.loads(meter[meter.rfind('{'):meter.rfind('}') + 1])
    return dict(schema_version=1, status='passed', cases=len(rows), metrics=metrics,
                native_pcm_maximum_error_lsb=error * 32768, native_alignment_samples=offset,
                exact_matched_frames=exact_frames, final_fade_maximum_error_lsb=final_fade_error * 32768,
                final_fade_note="Final 100 ms source fade includes native mixer release at the four-active-second deadline; full PCM equality checked in mounted archive.",
                wave_sha256=sha(wave_path.read_bytes()), timeline_sha256=sha(timeline_path.read_bytes()),
                native_master_loudness=loudness, cancellation_allowance_seconds=.12,
                limits=['Controlled native fixture, with diagnostic support overrides for distance/pan only.',
                        'Ordinary supported listener/motion is separately exercised in Main.',
                        'No ordinary-play/device loudness compliance or executable release claim.'])


def main():
    """Persist only fully checked native measurements."""
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--wave', type=Path, required=True); parser.add_argument('--timeline', type=Path, required=True)
    parser.add_argument('--output', type=Path, required=True); args = parser.parse_args()
    result = measure(args.wave, args.timeline); args.output.write_text(json.dumps(result, indent=2) + '\n')
    print(f'PASS: rain native Master: {result["cases"]} independently measured cases')


if __name__ == '__main__':
    main()
