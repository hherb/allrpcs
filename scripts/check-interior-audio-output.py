#!/usr/bin/env python3
"""Measure installed-player Master captures for the approved interior enclosure model."""
import argparse
import hashlib
import json
import subprocess
import wave
from pathlib import Path
import numpy as np


def require(value, message):
    """Reject incomplete, clipped, missing or wrongly attenuated engine output."""
    if not value:
        raise ValueError(message)


def rms(data):
    """Compute linear RMS without changing any source samples."""
    return float(np.sqrt(np.mean(data * data)))


def main():
    """Analyze the final stereo PCM16 Master recording and exact case timeline."""
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--wave', type=Path, required=True)
    parser.add_argument('--timeline', type=Path, required=True)
    parser.add_argument('--output', type=Path, required=True)
    parser.add_argument('--decode', type=Path, required=True)
    args = parser.parse_args()
    with wave.open(str(args.wave), 'rb') as capture:
        require((capture.getnchannels(), capture.getframerate(), capture.getsampwidth()) == (2, 48000, 2), 'Wrong Master format')
        samples = np.frombuffer(capture.readframes(capture.getnframes()), dtype='<i2').reshape(-1, 2).astype(float) / 32768
    timeline = json.loads(args.timeline.read_text())
    require(len(timeline) == 39 and len({r['name'] for r in timeline}) == 39, 'Incomplete 39-case matrix')
    rows = {r['name']: r for r in timeline}

    def segment(name, begin, end):
        """Read one retained case interval from the actual Master capture."""
        start = rows[name]['start']
        return samples[round((start + begin) * 48000):round((start + end) * 48000)]

    metrics = {}
    for row in timeline:
        data = segment(row['name'], 0.4, row['duration'] - 0.1)
        require(len(data) > 0, 'Missing output interval')
        peak = float(np.max(np.abs(data)))
        require(peak < 0.99, 'Clipped output: ' + row['name'])
        metrics[row['name']] = {'rms_dbfs': 20 * np.log10(max(rms(data), 1e-12)),
                               'peak_dbfs': 20 * np.log10(max(peak, 1e-12))}
        if row['mode'] in {'wall', 'mute-Master'}:
            require(peak < 1e-6, 'Unsupported/muted output leaked: ' + row['name'])
        else:
            require(peak > 1e-6, 'Missing output: ' + row['name'])
    for room, attenuation in [('Sickhouse', -9), ('Laundry', -3)]:
        for bed in range(1, 5):
            base = rms(segment(f'{room}-B0{bed}-0.0', 1, 3.5))
            for depth in [0.5, 1.0]:
                name = f'{room}-B0{bed}-{depth:.1f}'
                ratio = rms(segment(name, 1, 3.5)) / base
                actual = 20 * np.log10(ratio)
                expected = 0 if bed == 1 else attenuation * depth
                require(abs(actual - expected) <= 0.25, f'Wrong gain/double scaling: {name}, {actual} expected {expected}')
                metrics[name].update(relative_db=actual, expected_db=expected)
    for name in ['timber-contact', 'earth-contact']:
        require(rms(segment(name, 0.22, 0.6)) > 1e-5, 'Missing eligible floor contact: ' + name)
        require(np.max(np.abs(segment(name, 0.85, 1.8))) < 1e-6, 'Contact replayed or looped: ' + name)
    require(np.max(np.abs(segment('mono', 1, 5)[:, 0] - segment('mono', 1, 5)[:, 1])) < 1e-6, 'Mono output differs by channel')
    for name in ['reset', 'owner-loss']:
        # Same playing bed continues; neutral cancellation must increase its source-relative level.
        before = rms(segment(name, 0.5, 1.8))
        after = rms(segment(name, 2.4, 3.8))
        require(after / before > 2.2, 'Owned attenuation did not reset: ' + name)
        metrics[name]['post_reset_rms_ratio'] = after / before
    for name in ['Sickhouse-doorway', 'Laundry-doorway']:
        before = rms(segment(name, 0.1, 0.8))
        inside = rms(segment(name, 1.8, 2.4))
        outside = rms(segment(name, 4.2, 4.8))
        require(inside < before and outside > inside, 'Doorway gain did not enter/exit: ' + name)
        metrics[name].update(exterior_before_rms=before, interior_rms=inside, exterior_after_rms=outside)
    for bus in ['Ambience', 'Effects', 'Music']:
        name = 'mute-' + bus
        require(np.max(np.abs(segment(name, 0.4, 1.8))) < 1e-6, 'Muted category leaked: ' + bus)
        probe = segment(name, 2.22, 2.65) if bus == 'Ambience' else segment(name, 3, 5)
        require(rms(probe) > 1e-5, 'Unrelated category was muted: ' + bus)
        metrics[name]['muted_first_then_unrelated_audible'] = True
    root = Path(__file__).resolve().parents[1]
    scope = json.loads((root / 'audio-direction/interior-runtime-scope.json').read_text())
    native = []
    for clip in scope['reused_resources']:
        path = root / clip['path']
        with wave.open(str(path), 'rb') as source:
            require(source.getsampwidth() == 3, 'Wrong reused PCM format')
            raw = np.frombuffer(source.readframes(source.getnframes()), dtype=np.uint8).reshape(-1, 3).astype(np.int32)
        integers = raw[:, 0] | raw[:, 1] << 8 | raw[:, 2] << 16
        signed = np.where(integers & 0x800000, integers - 0x1000000, integers)
        expected = (signed >> 8).astype('<i2').tobytes()
        actual = (args.decode / (path.stem + '.pcm16')).read_bytes()
        require(actual == expected, 'Native/imported PCM drift: ' + path.stem)
        native.append({'path': clip['path'], 'decoded_bytes': len(actual), 'native_sha256': hashlib.sha256(actual).hexdigest(), 'PCM24_to_PCM16_exact': True})
    meter = subprocess.run(['ffmpeg', '-hide_banner', '-i', str(args.wave), '-af',
        'loudnorm=I=-23:LRA=7:TP=-1:print_format=json', '-f', 'null', '-'], capture_output=True, text=True, check=True).stderr
    measurements = json.loads(meter[meter.rfind('{'):meter.rfind('}') + 1])
    output = {'schema_version': 1, 'cases': 39, 'metrics': metrics, 'native_parity': native,
        'wave_sha256': hashlib.sha256(args.wave.read_bytes()).hexdigest(),
        'timeline_sha256': hashlib.sha256(args.timeline.read_bytes()).hexdigest(),
        'final_master_measurements': {k: measurements[k] for k in ['input_i', 'input_tp', 'input_lra', 'input_thresh']},
        'limits': ['Actual controlled Master output; no physical room acoustic measurement.',
                   'No normalization written; accepted bytes unchanged.', 'No new device listening or ordinary-play loudness compliance claim.']}
    args.output.write_text(json.dumps(output, indent=2) + '\n')
    print('PASS: 39 actual Master cases, proposed bed gains, doorway/reset, contacts/wall, category controls/mono/mixes and 16 native PCM streams')


if __name__ == '__main__':
    main()
