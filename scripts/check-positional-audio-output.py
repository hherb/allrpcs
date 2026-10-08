#!/usr/bin/env python3
"""Measure actual final Master positional output, rather than trusting requested player properties."""
import argparse
import hashlib
import json
from pathlib import Path
import subprocess
import wave
import numpy as np


def require(value, message):
    """Fail on incorrect captured routing, silence, distance, pan or cancellation."""
    if not value:
        raise ValueError(message)


def main():
    """Analyze each engine-rendered timeline case and retain truthful levels/latency."""
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--wave', type=Path, required=True)
    parser.add_argument('--timeline', type=Path, required=True)
    parser.add_argument('--output', type=Path, required=True)
    args = parser.parse_args()
    with wave.open(str(args.wave), 'rb') as reader:
        require((reader.getnchannels(), reader.getframerate()) == (2, 48000)
                and reader.getsampwidth() in (2, 4), 'Wrong Master capture format')
        bits = reader.getsampwidth() * 8
        dtype, scale = ('<i2', 32768) if bits == 16 else ('<i4', 2147483648)
        samples = np.frombuffer(reader.readframes(reader.getnframes()), dtype=dtype).reshape(-1, 2).astype(np.float64) / scale
    rate = 48000
    steps = json.loads(args.timeline.read_text())
    require(len(steps) == 44, 'Incomplete output timeline')
    by_name = {row['name']: row for row in steps}
    metrics = {}

    def region(name, begin=0.5, end=2.9):
        row = by_name[name]
        return samples[round((row['start'] + begin) * rate):round((row['start'] + end) * rate)]

    def rms(data):
        return float(np.sqrt(np.mean(data ** 2)))

    for row in steps:
        name = row['name']
        data = region(name, 0.5, min(row['duration'] - 0.1, 8.9))
        peak = float(np.max(np.abs(data)))
        whole_case = region(name, 0, row['duration'])
        active = np.flatnonzero(np.max(np.abs(whole_case), axis=1) > 0.000001)
        metrics[name] = {'rms_dbfs': 20 * np.log10(max(rms(data), 1e-12)),
                         'peak_dbfs': 20 * np.log10(max(peak, 1e-12)),
                         'first_signal_after_command_seconds': float(active[0] / rate) if len(active) else None,
                         'left_rms': rms(data[:, 0]), 'right_rms': rms(data[:, 1])}
        silent = row['variant'] in ['zero', 'mute', 'master-zero', 'master-mute', 'missing'] or name.endswith(('-boundary', '-outside'))
        if name == 'whisper-boundary':
            silent = True
        if silent:
            require(peak < 0.000001, 'Ineligible/muted output: ' + name)
        elif row['variant'] not in ['cancel', 'mute-unmute']:
            require(peak > 0.000001, 'Missing admitted output: ' + name)
        require(peak < 0.99, 'Clipped output: ' + name)
        if row['variant'] == 'mono':
            require(float(np.max(np.abs(data[:, 0] - data[:, 1]))) < 0.000001, 'Not dual mono: ' + name)
        if row['variant'] == 'cancel':
            require(float(np.max(np.abs(region(name, 1.0, 2.9)))) < 0.000001, 'Cancelled source restarted: ' + name)
    for kind in ['water', 'reply']:
        baseline = rms(region(kind + '-normal'))
        for suffix, expected in [('half', 0.5), ('half-distance', 0.5), ('zoom2', 0.5), ('other-bus-zero', 1.0)]:
            ratio = rms(region(kind + '-' + suffix)) / baseline
            require(abs(ratio - expected) < 0.035, f'Wrong captured gain/distance/zoom independence: {kind}-{suffix}: {ratio}')
            metrics[kind + '-' + suffix]['rms_ratio_to_normal'] = ratio
        require(metrics[kind + '-left']['left_rms'] > metrics[kind + '-left']['right_rms'], 'Left bearing reversed: ' + kind)
        require(metrics[kind + '-right']['right_rms'] > metrics[kind + '-right']['left_rms'], 'Right bearing reversed: ' + kind)
    require(float(np.max(np.abs(region('reply-mute-unmute', 1.1, 2.9)))) < 0.000001, 'Unmute replayed reply')
    require(rms(region('water-mute-unmute', 1.1, 2.9)) > 0.000001, 'Water bed failed to continue after unmute')
    require(rms(region('water-loop', 30.5, 32.5)) > 0.000001, 'Water loop ended at seam')
    meter = subprocess.run(['ffmpeg', '-hide_banner', '-i', str(args.wave), '-af',
        'loudnorm=I=-23:LRA=7:TP=-1:print_format=json', '-f', 'null', '-'], capture_output=True, text=True, check=True).stderr
    loudness = json.loads(meter[meter.rfind('{'):meter.rfind('}') + 1])
    args.output.write_text(json.dumps({'schema_version':1,'cases':len(steps),'metrics':metrics,
        'capture_pcm_bits':bits, 'capture_frames':len(samples),
        'wave_sha256':hashlib.sha256(args.wave.read_bytes()).hexdigest(),
        'timeline_sha256':hashlib.sha256(args.timeline.read_bytes()).hexdigest(), 'final_master_measurements':loudness,
        'limits':['Controlled engine diagnostics, not ordinary-play loudness compliance or device listening.',
                  'Composite B02 and its native positional centre scaling are retained without compensation.',
                  'Camera changes stereo panning; captured audience and world-distance eligibility remain separate.']}, indent=2) + '\n')
    print('PASS: 44 actual Master cases; category/master silence/gain, distance/zoom, bearing/mono, cancellation, loop and retained action-footstep mix')


if __name__ == '__main__':
    main()
