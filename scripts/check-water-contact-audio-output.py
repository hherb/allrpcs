#!/usr/bin/env python3
"""Measure sixteen actual live-commit Master captures and byte-exact native source decoding."""
import argparse
import hashlib
import json
from pathlib import Path
import wave
import numpy as np


def require(value, message):
    """Reject absent, leaked, incorrectly scaled or incomplete controlled output."""
    if not value:
        raise ValueError(message)


def main():
    """Bind source/native bytes and measure each actual output interval without changing PCM."""
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--wave', type=Path, required=True)
    parser.add_argument('--timeline', type=Path, required=True)
    parser.add_argument('--native', type=Path, required=True)
    parser.add_argument('--output', type=Path, required=True)
    args = parser.parse_args()
    with wave.open(str(args.wave), 'rb') as stream:
        require((stream.getnchannels(), stream.getsampwidth(), stream.getframerate()) == (2, 2, 48000), 'Wrong Master format')
        samples = np.frombuffer(stream.readframes(stream.getnframes()), '<i2').reshape(-1, 2).astype(float) / 32768
    root = Path(__file__).resolve().parents[1]
    with wave.open(str(root / 'game/assets/audio/water-contact/A01-W01.wav'), 'rb') as stream:
        data = np.frombuffer(stream.readframes(stream.getnframes()), np.uint8).reshape(-1, 3).astype(np.int32)
    unsigned = data[:, 0] | data[:, 1] << 8 | data[:, 2] << 16
    signed = np.where(unsigned & 0x800000, unsigned - 0x1000000, unsigned)
    expected = (signed >> 8).astype('<i2').tobytes()
    require(args.native.read_bytes() == expected, 'Native source differs from exact accepted PCM24-to-PCM16 decoding')
    source = np.frombuffer(expected, '<i2').reshape(-1, 2).astype(float) / 32768
    timeline = json.loads(args.timeline.read_text())
    require(len(timeline) == 16 and len({r['name'] for r in timeline}) == 16, 'Incomplete sixteen-case matrix')
    metrics = {}
    silent = {'master-muted', 'effects-muted', 'missing-stream', 'unsupported-request',
              'duplicate-request', 'overlay-rejection', 'missing-effects-bus'}
    no_commit = {'unsupported-request', 'overlay-rejection'}
    for row in timeline:
        name = row['name']; begin = round(row['start'] * 48000)
        case = samples[begin:begin + round(row['duration'] * 48000)]
        require(len(case) == 192000, 'Missing case interval: ' + name)
        peak = float(np.max(np.abs(case)))
        rms = float(np.sqrt(np.mean(case * case)))
        require(peak < 0.99, 'Clipped Master: ' + name)
        require(row['commits'] == (0 if name in no_commit else 1), 'Wrong exact commit count: ' + name)
        require(row['presentations'] == (0 if name in no_commit else 1), 'Wrong presentation count: ' + name)
        if name in silent:
            require(peak < 1e-6, f'Unexpected new cue: {name}: {peak}')
        else:
            require(peak > 1e-5, 'Absent Master cue: ' + name)
        if name in {'focus-loss', 'restore-cancellation'}:
            require(np.max(np.abs(case[48000:])) < 1e-6, 'Late cue after cancellation: ' + name)
        metrics[name] = {'peak_dbfs': float(20 * np.log10(max(peak, 1e-12))),
                         'rms_dbfs': float(20 * np.log10(max(rms, 1e-12))),
                         'commits': row['commits'], 'presentations': row['presentations'], 'admitted': row['admitted']}
        if name in {'default', 'master-half', 'effects-half', 'captions-off'}:
            # Match the exact native source within the source's finite mixer-delay window.
            window = case[round(0.35 * 48000):round(2.25 * 48000)]
            length = 1 << (len(window) + len(source) - 1).bit_length()
            score = np.fft.irfft(np.fft.rfft(window[:, 0], length) * np.conj(np.fft.rfft(source[:, 0], length)), length)
            lag = int(np.argmax(score[:len(window) - len(source) + 1]))
            actual = window[lag:lag + len(source)]
            gain = float(np.sum(actual * source) / np.sum(source * source))
            error = float(np.sqrt(np.mean((actual - source * gain) ** 2)))
            correlation = float(np.sum(actual * source) / np.sqrt(np.sum(actual * actual) * np.sum(source * source)))
            target = 0.5 if name.endswith('half') else 1
            require(abs(gain - target) < 0.01 and correlation > 0.999, f'Source/gain mismatch: {name}, {gain}, {correlation}')
            require(np.max(np.abs(case[round(2.25 * 48000):])) < 1e-6, 'Loop or replay: ' + name)
            metrics[name].update(source_gain=gain, expected_gain=target, source_correlation=correlation,
                                 residual_rms=error, mixer_start_seconds=0.35 + lag / 48000)
        if name == 'a04-a05-coexistence':
            require(np.max(np.abs(case[round(2.25 * 48000):])) > 1e-5, 'Absent retained action cues after splash ends')
        if name == 'mono':
            require(np.max(np.abs(case[:, 0] - case[:, 1])) < 1e-6, 'Mono differs by channel')
    result = {'schema_version': 1, 'cases': 16, 'metrics': metrics,
              'native_pcm16_sha256': hashlib.sha256(expected).hexdigest(), 'native_exact': True,
              'wave_sha256': hashlib.sha256(args.wave.read_bytes()).hexdigest(),
              'timeline_sha256': hashlib.sha256(args.timeline.read_bytes()).hexdigest(),
              'limits': ['Controlled actual Master output, with unchanged background beds isolated.',
                         'No waveform normalization, new device listening or ordinary-play loudness compliance claim.']}
    args.output.write_text(json.dumps(result, indent=2) + '\n')
    print('PASS: sixteen actual live-commit Master cases, exact native PCM, source correlation/gain, mute/missing/rejection, cancellation, mono and A04/A05 coexistence')


if __name__ == '__main__':
    main()
