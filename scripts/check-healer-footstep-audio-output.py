#!/usr/bin/env python3
"""Measure actual native Master output for every approved Healer contact group/subcase."""
import argparse
import hashlib
import json
from pathlib import Path
import subprocess
import wave
import numpy as np

ROOT = Path(__file__).resolve().parents[1]


def require(value, message):
    """Reject incomplete or incorrect native measurements, including under optimization."""
    if not value:
        raise ValueError(message)


def main():
    """Check silence, native distance/pan/mono, category gains, variants and cancellation."""
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--wave', type=Path, required=True)
    parser.add_argument('--timeline', type=Path, required=True)
    parser.add_argument('--output', type=Path, required=True)
    args = parser.parse_args()
    with wave.open(str(args.wave), 'rb') as reader:
        require((reader.getnchannels(), reader.getframerate(), reader.getsampwidth()) == (2, 48000, 2), 'Wrong native capture format')
        samples = np.frombuffer(reader.readframes(reader.getnframes()), dtype='<i2').reshape(-1, 2).astype(np.float64) / 32768
    rows = json.loads(args.timeline.read_text())
    proposal = json.loads((ROOT / 'audio-direction/healer-footstep-owner-scope.json').read_text())
    groups = proposal['verification_required']['master_output_cases']
    require(set(r['group'] for r in rows) == set(groups) and len(groups) == 43, 'Missing approved group')
    require(len(rows) == len({(r['group'], r['name']) for r in rows}), 'Duplicate native subcase')
    rejected = {'distance-192', 'distance-193', 'stationary-wait-arrival', 'wall-pressure', 'unsupported-segment', 'interior-source', 'interior-listener',
                'duplicate-foreign-motion', 'teleport-invalid-delta', 'listener-stale-replaced', 'source-hidden-moved-replaced'}
    silent = rejected | {'effects-zero', 'effects-muted', 'master-zero', 'master-muted', 'missing-stream', 'missing-effects-bus'}
    metrics = {}
    for row in rows:
        begin = round(row['start'] * 48000); end = round((row['start'] + row['duration']) * 48000)
        data = samples[begin:end]
        require(len(data) == 96000, 'Truncated capture: ' + row['name'])
        peak = float(np.max(np.abs(data)))
        rms = np.sqrt(np.mean(data ** 2, axis=0))
        active = np.flatnonzero(np.max(np.abs(data), axis=1) > 0.000001)
        key = row['group'] + '/' + row['name']
        require(peak < 0.99, 'Clipping: ' + key)
        require(peak < 0.000001 if row['group'] in silent else peak > 0.000001, 'Wrong native output admission: ' + key)
        require(row['presentations'] == (0 if row['group'] in rejected else 2 if row['group'] == 'two-healer-tails' else 1), 'Wrong presentation count: ' + key)
        if row['group'] == 'mono':
            require(float(np.max(np.abs(data[:, 0] - data[:, 1]))) < 0.000001, 'Not dual mono')
        if row['cancel_at'] is not None:
            tail = data[round((row['cancel_at'] + 0.12) * 48000):]
            require(float(np.max(np.abs(tail))) < 0.000001, 'Cancelled/restarted tail: ' + key)
        metrics[key] = dict(peak=peak, left_rms=float(rms[0]), right_rms=float(rms[1]),
                            rms=float(np.sqrt(np.mean(data ** 2))), presentations=row['presentations'],
                            first_signal_seconds=float(active[0] / 48000) if len(active) else None,
                            last_signal_seconds=float(active[-1] / 48000) if len(active) else None)
        if row['group'].endswith('six-variants'):
            material = ('Earth', 'Gravel', 'Leaves', 'Timber').index(row['material']) + 1
            clip = f"W{material:02}-{row['variant']:02}"
            with wave.open(str(ROOT / f'game/assets/audio/footsteps/{clip}.wav'), 'rb') as reader:
                pcm = np.frombuffer(reader.readframes(reader.getnframes()), dtype=np.uint8).reshape(-1, 3).astype(np.int32)
            signed = pcm[:, 0] | pcm[:, 1] << 8 | pcm[:, 2] << 16
            signed = (signed ^ 0x800000) - 0x800000
            native = (signed >> 8).reshape(-1, 2).astype(np.float64) / 32768
            length = 1 << (len(data) + len(native) - 1).bit_length()
            correlation = np.fft.irfft(np.fft.rfft(data[:, 0], length)
                                       * np.conj(np.fft.rfft(native[:, 0], length)), length)
            offset = int(np.argmax(correlation[:len(data) - len(native) + 1]))
            error = float(np.max(np.abs(data[offset:offset + len(native)] - native * 0.5)))
            require(error <= 8 / 32768, 'Wrong captured variant/native PCM: ' + clip)
            metrics[key].update(native_variant=clip, native_alignment_samples=offset,
                                maximum_pcm16_error_lsb=error * 32768)
    for material in ('earth', 'gravel', 'leaves', 'timber'):
        variants = [r['variant'] for r in rows if r['group'] == material + '-six-variants']
        require(variants == list(range(1, 7)), 'Missing independent variant measurement: ' + material)
    def metric(group):
        return metrics[group + '/' + group]
    baseline = metric('distance-zero')['rms']
    for group, expected in [('distance-72', 1 - 72/192), ('distance-96', 0.5), ('distance-191', 1/192),
                            ('effects-half', 0.5), ('master-half', 0.5), ('other-bus-zero', 1),
                            ('zoom-08', 1), ('zoom-2', 1)]:
        ratio = metric(group)['rms'] / baseline
        require(abs(ratio - expected) < 0.035, 'Wrong measured distance/gain: ' + group + ': ' + str(ratio))
        metric(group)['rms_ratio_to_center'] = ratio
    require(metric('left-bearing')['left_rms'] > metric('left-bearing')['right_rms'], 'Left pan reversed')
    require(metric('right-bearing')['right_rms'] > metric('right-bearing')['left_rms'], 'Right pan reversed')
    meter = subprocess.run(['ffmpeg', '-hide_banner', '-i', str(args.wave), '-af',
                            'loudnorm=I=-23:LRA=7:TP=-1:print_format=json', '-f', 'null', '-'], capture_output=True, text=True, check=True).stderr
    loudness = json.loads(meter[meter.rfind('{'):meter.rfind('}') + 1])
    args.output.write_text(json.dumps({'schema_version':1,'groups':43,'cases':len(rows),'status':'passed',
        'metrics':metrics,'wave_sha256':hashlib.sha256(args.wave.read_bytes()).hexdigest(),
        'timeline_sha256':hashlib.sha256(args.timeline.read_bytes()).hexdigest(),
        'native_master_loudness':loudness,'cancellation_allowance_seconds':0.12,
        'native_variant_pcm16_tolerance_lsb':8,
        'limits':['Explicit stopped native output fixture, not ordinary-play/device loudness compliance.',
                  'Distance/pan positions isolate native player behavior; supported actual motion/ownership separately proved in Main.',
                  'No executable export or release.']},indent=2)+'\n')
    print(f'PASS: Healer native Master: 43 groups/{len(rows)} subcases, exact variants, distance/pan/mono/gains/silence/cancellation')


if __name__ == '__main__':
    main()
