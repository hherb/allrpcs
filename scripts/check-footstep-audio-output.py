#!/usr/bin/env python3
"""Measure actual installed contact output and test routing, gains and cancellation."""

import argparse
import hashlib
import json
import subprocess
import numpy as np
import wave
from pathlib import Path


def require(value, message):
    """Reject failed actual-output behavior, not merely a player's requested settings."""
    if not value:
        raise ValueError(message)


def main():
    """Analyze each retained Movie Maker timeline window in the final Master waveform."""
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--wave', type=Path, required=True)
    parser.add_argument('--timeline', type=Path, required=True)
    parser.add_argument('--output', type=Path, required=True)
    args = parser.parse_args()
    with wave.open(str(args.wave), "rb") as reader:
        rate = reader.getframerate()
        require(reader.getnchannels() == 2 and reader.getsampwidth() == 4, "Expected stereo PCM32 Master capture")
        audio = np.frombuffer(reader.readframes(reader.getnframes()), dtype="<i4").reshape(-1, 2).astype(np.float64) / 2147483648
    require(rate == 48000 and audio.shape[1] == 2 and np.isfinite(audio).all(), 'Invalid actual Master format')
    steps = json.loads(args.timeline.read_text())
    require(len(steps) == 67, 'Incomplete contact output sequence')
    results = {}

    def region(step, begin=0, end=None):
        return audio[round((step['start'] + begin) * rate):round((step['start'] + (end or step['duration'])) * rate)]

    for step in steps:
        data = region(step)
        peak = float(np.max(np.abs(data)))
        rms = float(np.sqrt(np.mean(data ** 2)))
        active = np.flatnonzero(np.max(np.abs(data), axis=1) > 0.0001)
        results[step['name']] = {'peak_dbfs': float(20 * np.log10(max(peak, 1e-12))),
            'rms_dbfs': float(20 * np.log10(max(rms, 1e-12))),
            'first_signal_seconds': float(active[0] / rate) if len(active) else None,
            'contact_window_to_signal_seconds': float(active[0] / rate - 0.2) if len(active) else None}
        if step['mode'] in ['zero', 'mute', 'master-zero', 'master-mute', 'missing']:
            require(float(np.max(np.abs(region(step, 0.1)))) < 0.000001, 'Silent contact mode produced output: ' + step['name'])
        else:
            require(peak > 0.0001, 'Missing audible contact case: ' + step['name'])
        require(peak < 0.99, 'Clipped contact mix: ' + step['name'])
        if step['mode'] == 'mono':
            require(float(np.max(np.abs(data[:, 0] - data[:, 1]))) < 0.000001, 'Mono output is not dual mono')
        if step['mode'] in ['stop', 'mute-unmute']:
            require(float(np.max(np.abs(region(step, 0.55, 3.8)))) < 0.000001, 'Cancel/unmute replayed a contact tail')
        if step['mode'] == 'sample':
            require(float(np.max(np.abs(region(step, 1.0, 1.4)))) < 0.000001, 'Contact window looped')
    for material in ['Earth', 'Gravel', 'Leaves', 'Timber']:
        normal = next(s for s in steps if s['name'] == material + '-normal')
        half = next(s for s in steps if s['name'] == material + '-half')
        isolated = next(s for s in steps if s['name'] == material + '-ambience-zero')
        energy = lambda s: float(np.sqrt(np.mean(region(s, 0.1, 1.2) ** 2)))
        ratio = energy(half) / energy(normal)
        require(0.47 < ratio < 0.53, f'Effects half gain incorrect for {material}: {ratio}')
        independence = energy(isolated) / energy(normal)
        require(0.98 < independence < 1.02, 'Ambience gain changed footstep output')
        results[material + '-half']['measured_rms_ratio'] = ratio
        results[material + '-ambience-zero']['measured_rms_ratio'] = independence
    meter = subprocess.run(['ffmpeg', '-hide_banner', '-i', str(args.wave), '-af',
        'loudnorm=I=-23:LRA=7:TP=-1:print_format=json', '-f', 'null', '-'], capture_output=True, text=True, check=True).stderr
    measurement = json.loads(meter[meter.rfind('{'):meter.rfind('}') + 1])
    args.output.write_text(json.dumps({'schema_version': 1, 'cases': len(steps), 'rate_hz': rate,
        'wave_sha256': hashlib.sha256(args.wave.read_bytes()).hexdigest(),
        'timeline_sha256': hashlib.sha256(args.timeline.read_bytes()).hexdigest(),
        'duration_seconds': len(audio) / rate, 'metrics': results, 'final_master_loudness': measurement,
        'limits': ['Measured diagnostic sequence, not ordinary-play LUFS compliance or device listening.',
                   'Full accepted contact windows retain leading samples; no anatomical animation synchronization claim.',
                   'Overlap/fast contact/mix output uses the actual controller; actual movement/cadence is checked in Main and pure tests.']}, indent=2) + '\n')
    print('PASS: 67 final-Master contact cases; 24 variants, routing/gain/mute/mono, tails and no replay; metrics retained')


if __name__ == '__main__':
    main()
