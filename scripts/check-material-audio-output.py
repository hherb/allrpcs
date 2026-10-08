#!/usr/bin/env python3
"""Analyze final Godot Master captures from the installed-player material proof."""

import argparse
import hashlib
import json
from pathlib import Path
import subprocess

import numpy as np


def require(value, message):
    """Reject failed output behavior rather than trusting mixer configuration."""
    if not value:
        raise ValueError(message)


def main():
    """Measure actual gain, silence, mono, compression and loop/cancellation cases."""
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--wave', type=Path, required=True)
    parser.add_argument('--timeline', type=Path, required=True)
    parser.add_argument('--output', type=Path, required=True)
    args = parser.parse_args()
    rate = 48000
    raw = subprocess.check_output(['ffmpeg', '-v', 'error', '-i', str(args.wave), '-f', 'f32le',
                                   '-ar', str(rate), '-ac', '2', '-'])
    samples = np.frombuffer(raw, dtype='<f4').reshape(-1, 2)
    steps = json.loads(args.timeline.read_text())
    metrics = {}
    passed = []

    def region(step, begin, end):
        start = step['start']
        return samples[int((start + begin) * rate):int((start + end) * rate)]

    def rms(values):
        return float(np.sqrt(np.mean(values.astype(np.float64) ** 2)))

    for step in steps:
        data = region(step, 1, min(7.9, step['duration'] - 0.1))
        metrics[step['name']] = {'rms': rms(data), 'peak': float(np.max(np.abs(data)))}
    for clip in ['B01', 'A04', 'A05']:
        baseline = metrics[clip + '-normal']['rms']
        require(baseline > 0.000001, 'Silent normal playback: ' + clip)
        ratio = metrics[clip + '-half']['rms'] / baseline
        require(abs(ratio - 0.5) < 0.035, 'Incorrect captured half gain: ' + clip)
        for variant in ['zero', 'mute', 'master-zero', 'master-mute']:
            require(metrics[clip + '-' + variant]['peak'] < 0.000001, 'Output not silent: ' + clip + '-' + variant)
            passed.append(clip + '-' + variant)
        step = next(row for row in steps if row['name'] == clip + '-mono')
        mono = region(step, 1, 7.9)
        require(float(np.max(np.abs(mono[:, 0] - mono[:, 1]))) < 0.000001, 'Mono channels differ: ' + clip)
        require(metrics[clip + '-reduced']['rms'] < baseline, 'Reduced range did not apply existing compressor: ' + clip)
        passed.extend([clip + '-normal', clip + '-half', clip + '-mono', clip + '-reduced'])
    for clip in ['B01', 'B02', 'B03', 'B04']:
        step = next(row for row in steps if row['name'] == clip + '-loop')
        before = region(step, 28.5, 29.5)
        after = region(step, 30.5, 31.5)
        require(rms(before) > 0.000001 and rms(after) > 0.000001, 'Loop stopped at seam: ' + clip)
        seam = region(step, 29.8, 30.2)
        metrics[clip + '-loop']['seam_max_sample_delta'] = float(np.max(np.abs(np.diff(seam, axis=0))))
        metrics[clip + '-loop']['seam_min_10ms_rms'] = min(rms(chunk) for chunk in np.array_split(seam, 40))
        passed.append(clip + '-loop')
    revealed = next(row for row in steps if row['name'] == 'revealed')
    woodland = next(row for row in steps if row['name'] == 'B03-loop')
    ratio = rms(region(revealed, 1, 7.9)) / rms(region(woodland, 1, 7.9))
    require(abs(ratio - 10 ** (-3 / 20)) < 0.035, 'Revealed attenuation differs from approved -3 dB')
    metrics['revealed']['ratio_to_normal'] = ratio
    for name in ['treatment-reset', 'treatment-missing']:
        step = next(row for row in steps if row['name'] == name)
        require(float(np.max(np.abs(region(step, 4, 8)))) < 0.000001, 'Cancelled/missing cue left audible output: ' + name)
        passed.append(name)
    for name in ['crossfade', 'retarget']:
        step = next(row for row in steps if row['name'] == name)
        require(rms(region(step, 2, 3)) > 0.000001 and rms(region(step, 4, 6)) > 0.000001,
                'Regional transition lost output: ' + name)
        passed.append(name)
    muted = next(row for row in steps if row['name'] == 'treatment-muted-release')
    require(float(np.max(np.abs(region(muted, 3.2, 3.8)))) < 0.000001, 'Mid-cue mute failed')
    normal_treatment = next(row for row in steps if row['name'] == 'A05-normal')
    require(float(np.max(np.abs(region(muted, 4.2, 7.8)
                               - region(normal_treatment, 4.2, 7.8)))) < 0.00001,
            'Unmute restarted the accepted cue instead of retaining its phase')
    released = next(row for row in steps if row['name'] == 'treatment-release')
    require(float(np.max(np.abs(region(released, 9, 11)))) < 0.000001, 'One-shot repeated after completion')
    passed.extend(['revealed', 'treatment-muted-release', 'treatment-release'])
    phase = next(row for row in steps if row['name'] == 'bed-muted-phase')
    normal_bed = next(row for row in steps if row['name'] == 'B01-normal')
    require(float(np.max(np.abs(region(phase, 3.2, 3.8)))) < 0.000001, 'Mid-loop mute failed')
    reference = region(normal_bed, 5, 6).mean(axis=1)
    restored = region(phase, 5, 6).mean(axis=1)
    correlation = np.fft.irfft(np.fft.rfft(reference) * np.conj(np.fft.rfft(restored)), n=len(reference))
    near_zero = np.concatenate((correlation[:1024], correlation[-1024:]))
    coherence = float(np.max(near_zero) / np.sqrt(np.sum(reference ** 2) * np.sum(restored ** 2)))
    require(coherence > 0.98, 'Loop phase restarted or paused during user mute')
    metrics['bed-muted-phase']['retained_phase_correlation'] = coherence
    passed.append('bed-muted-phase')
    meter = subprocess.run(['ffmpeg', '-hide_banner', '-i', str(args.wave), '-af',
                            'loudnorm=I=-23:LRA=7:TP=-1:print_format=json', '-f', 'null', '-'],
                           capture_output=True, text=True, check=True).stderr
    measurement = json.loads(meter[meter.rfind('{'):meter.rfind('}') + 1])
    args.output.write_text(json.dumps({'schema_version': 1, 'wave_sha256': hashlib.sha256(args.wave.read_bytes()).hexdigest(),
        'timeline_sha256': hashlib.sha256(args.timeline.read_bytes()).hexdigest(), 'rate_hz': rate,
        'seconds': len(samples) / rate, 'cases': len(steps), 'passed_output_checks': passed,
        'metrics': metrics, 'final_master_loudness': measurement,
        'limits': ['Technical final-output evidence only; no new owner sound selection or device-listening claim.',
                   'Loop seam deltas are reported measurements, not human listening judgments.',
                   'Retained policy targets are provisional; this diagnostic sequence is not ordinary-play LUFS compliance.']}, indent=2) + '\n')
    print('PASS:', len(passed), 'final-output checks;', len(steps), 'installed-player cases; actual Master metrics retained')


if __name__ == '__main__':
    main()
