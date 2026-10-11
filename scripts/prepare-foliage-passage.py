#!/usr/bin/env python3
"""Prepare only the granted, hash-bound contiguous F01 passage after recorded listening."""
import argparse
import hashlib
import json
import math
import wave
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
SOURCE = ROOT / 'audio-direction/foliage-source'
ORIGINAL_SHA256 = '0633f88fa0422030401bd633d5ee73cca7b61687df950e96be55d64eafec7715'
RATE = 48000
FRAMES = 144000
FADE_FRAMES = 4800
CEILING_DBFS = -18.0


def digest(path):
    """Bind exact source, rights and exported container bytes."""
    return hashlib.sha256(path.read_bytes()).hexdigest()


def decoded():
    """Read the one acquired lossless PCM24 original without transforms."""
    path = SOURCE / 'original.wav'
    if digest(path) != ORIGINAL_SHA256:
        raise ValueError('Original source hash drift')
    with wave.open(str(path), 'rb') as reader:
        if (reader.getframerate(), reader.getnchannels(), reader.getsampwidth()) != (RATE, 2, 3):
            raise ValueError('Original format drift')
        return reader.getnframes(), reader.readframes(reader.getnframes())


def samples(pcm):
    """Decode signed little-endian 24-bit samples exactly."""
    return [int.from_bytes(pcm[i:i + 3], 'little', signed=True) for i in range(0, len(pcm), 3)]


def level(pcm):
    """Measure sample peak and whole-passage RMS without claiming listening compliance."""
    values = samples(pcm)
    peak = max(map(abs, values), default=0) / 8388608
    rms = math.sqrt(sum((v / 8388608) ** 2 for v in values) / len(values))
    return {'sample_peak_dbfs': 20 * math.log10(peak) if peak else None,
            'rms_dbfs': 20 * math.log10(rms) if rms else None,
            'clipped_samples': sum(abs(v) >= 8388607 for v in values)}


def prepare(start_frame, gain_db):
    """Export one verified listening window, uniform attenuation and linear endpoint fades."""
    total, pcm = decoded()
    provenance = json.loads((SOURCE / 'provenance.json').read_text())
    listening = provenance['listening']
    if listening['status'] != 'passed' or listening['start_frame'] != start_frame:
        raise ValueError('A matching recorded listening assessment is required before preparation')
    if not isinstance(start_frame, int) or start_frame < 0 or start_frame + FRAMES > total:
        raise ValueError('Contiguous three-second source window required')
    if not math.isfinite(gain_db) or gain_db > 0:
        raise ValueError('Only finite uniform attenuation is permitted; never boost')
    if provenance['original']['sha256'] != ORIGINAL_SHA256:
        raise ValueError('Source provenance drift')
    for item in provenance['rights']:
        if digest(ROOT / item['path']) != item['sha256']:
            raise ValueError('Rights capture drift')
    passage = pcm[start_frame * 6:(start_frame + FRAMES) * 6]
    values = samples(passage)
    gain = 10 ** (gain_db / 20)
    output = bytearray()
    for frame in range(FRAMES):
        fade = min(1.0, frame / FADE_FRAMES, (FRAMES - 1 - frame) / FADE_FRAMES)
        for channel in range(2):
            value = round(values[frame * 2 + channel] * gain * fade)
            output.extend(value.to_bytes(3, 'little', signed=True))
    measured = level(output)
    if measured['sample_peak_dbfs'] is None or measured['sample_peak_dbfs'] > CEILING_DBFS + 1e-6:
        raise ValueError('Prepared source exceeds -18 dBFS ceiling or is silent')
    destination = ROOT / 'game/assets/audio/foliage/F01.wav'
    destination.parent.mkdir(parents=True, exist_ok=True)
    with wave.open(str(destination), 'wb') as writer:
        writer.setnchannels(2); writer.setsampwidth(3); writer.setframerate(RATE); writer.writeframes(output)
    record = {'path': str(destination.relative_to(ROOT)), 'sha256': digest(destination),
              'decoded_pcm_sha256': hashlib.sha256(output).hexdigest(),
              'trim_pcm_sha256': hashlib.sha256(passage).hexdigest(),
              'start_frame': start_frame, 'end_frame_exclusive': start_frame + FRAMES,
              'frames': FRAMES, 'sample_rate': RATE, 'channels': 2, 'encoding': 'PCM24',
              'uniform_gain_db': gain_db, 'linear_fade_frames_each_end': FADE_FRAMES,
              'loop': False, **measured}
    provenance['derivative'] = record
    (SOURCE / 'provenance.json').write_text(json.dumps(provenance, indent=2) + '\n')
    print(json.dumps(record, indent=2))


def main():
    """Inspect by default; explicit frame/gain arguments require passed listening evidence."""
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--start-frame', type=int)
    parser.add_argument('--gain-db', type=float, default=0.0)
    args = parser.parse_args()
    if args.start_frame is None:
        frames, pcm = decoded()
        print(json.dumps({'frames': frames, 'sample_rate': RATE, 'channels': 2,
                          'encoding': 'PCM24', 'duration_seconds': frames / RATE,
                          'decoded_pcm_sha256': hashlib.sha256(pcm).hexdigest(), **level(pcm)}, indent=2))
    else:
        prepare(args.start_frame, args.gain_db)


if __name__ == '__main__':
    main()
