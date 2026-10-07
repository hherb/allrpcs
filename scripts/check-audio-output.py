#!/usr/bin/env python3
"""Measure actual Godot offline output, including final Master gain and mute."""

import argparse
import hashlib
import json
from pathlib import Path
import wave

import numpy as np


def require(condition, message):
    """Reject invalid measurements with a useful diagnostic."""
    if not condition:
        raise ValueError(message)


def main():
    """Analyze steady portions of explicitly started disposable mixer probes."""
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--wave', required=True, type=Path)
    parser.add_argument('--timeline', required=True, type=Path)
    parser.add_argument('--output', required=True, type=Path)
    args = parser.parse_args()
    with wave.open(str(args.wave), 'rb') as source:
        channels, width, rate = source.getnchannels(), source.getsampwidth(), source.getframerate()
        require(channels == 2 and width in (2, 4), 'Expected uncompressed stereo PCM')
        samples = np.frombuffer(source.readframes(source.getnframes()), dtype='<i2' if width == 2 else '<i4')
        samples = samples.reshape(-1, channels).astype(np.float64) / (2 ** (width * 8 - 1))
    timeline = json.loads(args.timeline.read_text())
    measured = {}
    for step in timeline:
        start = step['start']
        segment = samples[round((start + 0.80) * rate):round((start + 0.95) * rate)]
        require(len(segment) == round(0.15 * rate), 'Incomplete probe capture: ' + step['name'])
        rms = np.sqrt(np.mean(segment * segment, axis=0))
        peak = np.max(np.abs(segment))
        measured[step['name']] = {'rms': rms.tolist(), 'peak': float(peak)}

    def level(name):
        return measured[name]['rms'][0]

    for category in ('Music', 'Ambience', 'Effects', 'Dialogue', 'Speech'):
        baseline = level(category + '-normal')
        require(0.006 < baseline < 0.008, 'Unexpected reference output: ' + category)
        require(abs(level(category + '-half') / baseline - 0.5) < 0.015, 'Independent gain failed: ' + category)
        for state in ('zero', 'mute'):
            require(measured[category + '-' + state]['peak'] < 1e-8, 'Non-silent ' + category + '-' + state)
    for name in ('master-zero', 'master-mute', 'duck-muted'):
        require(measured[name]['peak'] < 1e-8, 'Final output was not silent: ' + name)
    left = measured['stereo-left']['rms']
    mono = measured['mono-left']['rms']
    restored = measured['stereo-restored']['rms']
    require(left[0] > 0.006 and left[1] < 1e-8, 'Panned probe was not left-only')
    require(abs(mono[0] - mono[1]) < 1e-7, 'Mono output channels differ')
    require(abs(mono[0] / left[0] - 0.5) < 0.02, 'Mono fold-down doubled amplitude or changed normalization')
    require(abs(restored[0] / left[0] - 1) < 0.02 and restored[1] < 1e-8, 'Stereo did not restore')
    default_contrast = 20 * np.log10(level('default-high') / level('default-low'))
    reduced_contrast = 20 * np.log10(level('reduced-high') / level('reduced-low'))
    require(default_contrast - reduced_contrast > 5, 'Reduced range did not reduce signal contrast')
    normal = level('duck-normal')
    for name, reduction in (('duck-speech', 6), ('duck-overlap', 6), ('reading', 3)):
        actual = 20 * np.log10(normal / level(name))
        require(abs(actual - reduction) < 0.2, 'Incorrect foreground reduction: ' + name)
    for name in ('duck-cancel-release', 'reading-release'):
        require(abs(level(name) / normal - 1) < 0.02, 'Foreground did not release: ' + name)
    effects = level('Effects-normal')
    require(abs(20 * np.log10(effects / level('optional-effects')) - 3) < 0.2, 'Optional effect did not duck')
    require(abs(level('required-effects') / effects - 1) < 0.02, 'Required effect was attenuated by speech')
    result = {
        'status': 'passed', 'measurement': 'Actual offline final stereo mixer output, not pre-gain bus capture',
        'sample_rate': rate, 'sample_width_bytes': width, 'probe_count': len(timeline),
        'default_probe_contrast_db': float(default_contrast),
        'reduced_probe_contrast_db': float(reduced_contrast), 'steady_probe_measurements': measured,
        'wave_sha256': hashlib.sha256(args.wave.read_bytes()).hexdigest(),
        'timeline_sha256': hashlib.sha256(args.timeline.read_bytes()).hexdigest(),
        'limitations': 'Disposable sine probes establish routing/control behavior; no listening, platform-device, LRA or production true-peak compliance claim.',
    }
    args.output.write_text(json.dumps(result, indent=2) + '\n')
    print(f'PASS: {len(timeline)} rendered-output probes; gain/mutes, mono, dynamics, foreground and release')


if __name__ == '__main__':
    main()
