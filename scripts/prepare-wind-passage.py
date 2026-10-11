#!/usr/bin/env python3
"""Prepare the exact retained wind window; runtime output requires recorded listening qualification."""
import argparse
import array
import hashlib
import json
import math
from pathlib import Path
import re
import subprocess
import sys
import tempfile
import wave

ROOT = Path(__file__).resolve().parents[1]
SOURCE = ROOT / 'audio-direction/wind-source'
REVIEW = ROOT / 'audio-direction/wind-runtime-review/source'
ORIGINAL = ROOT / 'audio-direction/material-audition/originals/wind/wind background noise 2.wav'
ORIGINAL_SHA = '9f1fd5c8b37949e6889a7f6acbd822f66e0ab4e918d72bda58d8d8c2c7e85a80'
INPUT_RATE = 44100
START = 529200
INPUT_FRAMES = 176400
RATE = 48000
FRAMES = 192000
FADE = 4800
GAIN_DB = -4.0
RESAMPLE = 'aresample=48000:resampler=swr:filter_size=32:phase_shift=10:linear_interp=0:exact_rational=1:dither_method=none'


def digest(path):
    """Hash exact retained or newly prepared bytes."""
    return hashlib.sha256(path.read_bytes()).hexdigest()


def run(arguments):
    """Run the recorded decoder without shell interpolation."""
    return subprocess.run(arguments, check=True, capture_output=True).stdout


def qualification(provenance, candidate_sha):
    """Refuse installation without a matching explicit assessment of the preview bytes."""
    listening = provenance['listening']
    if (listening.get('status') != 'passed' or listening.get('start_frame') != START
            or listening.get('end_frame_exclusive') != START + INPUT_FRAMES
            or listening.get('candidate_sha256') != candidate_sha
            or not listening.get('owner_statement')):
        raise ValueError('Matching recorded owner listening qualification required before runtime installation')


def prepare(preview):
    """Decode/trim/resample, apply one attenuation and endpoint fades; never filter or layer."""
    provenance = json.loads((SOURCE / 'provenance.json').read_text())
    if digest(ORIGINAL) != ORIGINAL_SHA or provenance['original']['sha256'] != ORIGINAL_SHA:
        raise ValueError('Original source hash drift')
    for item in provenance['rights']:
        if digest(ROOT / item['path']) != item['sha256']:
            raise ValueError('Retained rights capture drift')
    ffmpeg = run(['ffmpeg', '-version']).decode().splitlines()[0]
    decoded = run(['ffmpeg', '-v', 'error', '-i', str(ORIGINAL), '-f', 'f32le', '-c:a', 'pcm_f32le', '-'])
    if len(decoded) % 8 or START + INPUT_FRAMES > len(decoded) // 8:
        raise ValueError('Invalid decoded stereo source or window')
    passage = decoded[START * 8:(START + INPUT_FRAMES) * 8]
    with tempfile.TemporaryDirectory(prefix='allrpcs-wind-') as directory:
        trim = Path(directory) / 'trim.f32'; trim.write_bytes(passage)
        resampled = run(['ffmpeg', '-v', 'error', '-f', 'f32le', '-ar', str(INPUT_RATE), '-ac', '2',
                         '-i', str(trim), '-af', RESAMPLE, '-f', 'f32le', '-c:a', 'pcm_f32le', '-'])
    values = array.array('f'); values.frombytes(resampled)
    if sys.byteorder != 'little':
        values.byteswap()
    if len(values) != FRAMES * 2 or any(not math.isfinite(v) for v in values):
        raise ValueError('Resampled frame count/nonfinite drift')
    gain = 10 ** (GAIN_DB / 20)
    pcm = bytearray(); peak = 0; squares = 0
    for frame in range(FRAMES):
        fade = min(1.0, frame / FADE, (FRAMES - 1 - frame) / FADE)
        for channel in range(2):
            sample = round(values[frame * 2 + channel] * gain * fade * 8388608)
            if abs(sample) >= 8388607:
                raise ValueError('Clipped sample')
            peak = max(peak, abs(sample)); squares += (sample / 8388608) ** 2
            pcm.extend(sample.to_bytes(3, 'little', signed=True))
    peak_db = 20 * math.log10(peak / 8388608) if peak else None
    if peak_db is None or peak_db > -24:
        raise ValueError('Silent passage or sample peak exceeds -24 dBFS')
    REVIEW.mkdir(parents=True, exist_ok=True)
    candidate = REVIEW / 'G01-candidate.wav'
    with wave.open(str(candidate), 'wb') as writer:
        writer.setnchannels(2); writer.setsampwidth(3); writer.setframerate(RATE); writer.writeframes(pcm)
    loudness = subprocess.run(['ffmpeg', '-hide_banner', '-nostats', '-i', str(candidate),
                               '-af', 'ebur128=peak=true', '-f', 'null', '-'], check=True, capture_output=True).stderr.decode()
    match = re.findall(r'I:\s*(-?[\d.]+) LUFS', loudness)
    record = {'status': 'review_candidate_not_qualified' if preview else 'qualified_runtime_derivative',
              'path': str(candidate.relative_to(ROOT)), 'sha256': digest(candidate),
              'decoder': ffmpeg, 'python': sys.version.split()[0], 'resampling_filter': RESAMPLE,
              'original_sha256': ORIGINAL_SHA, 'decoded_original_frames': len(decoded) // 8,
              'decoded_original_float32_sha256': hashlib.sha256(decoded).hexdigest(),
              'trim_float32_sha256': hashlib.sha256(passage).hexdigest(),
              'resampled_float32_sha256': hashlib.sha256(resampled).hexdigest(),
              'decoded_pcm24_sha256': hashlib.sha256(pcm).hexdigest(),
              'start_frame': START, 'end_frame_exclusive': START + INPUT_FRAMES,
              'input_sample_rate': INPUT_RATE, 'frames': FRAMES, 'sample_rate': RATE,
              'channels': 2, 'encoding': 'PCM24', 'duration_seconds': 4,
              'uniform_gain_db': GAIN_DB, 'linear_fade_frames_each_end': FADE,
              'loop': False, 'sample_peak_dbfs': peak_db,
              'rms_dbfs': 20 * math.log10(math.sqrt(squares / (FRAMES * 2))),
              'integrated_loudness_lufs': float(match[-1]) if match else None,
              'clipped_samples': 0,
              'qualification_limit': 'Measurements do not establish absence of bags, buffeting or other excluded audible sounds.'}
    if not preview:
        qualification(provenance, record['sha256'])
        destination = ROOT / 'game/assets/audio/wind/G01.wav'
        destination.parent.mkdir(parents=True, exist_ok=True); destination.write_bytes(candidate.read_bytes())
        record['path'] = str(destination.relative_to(ROOT)); provenance['derivative'] = record
        (SOURCE / 'provenance.json').write_text(json.dumps(provenance, indent=2) + '\n')
    (REVIEW / 'passage-measurements.json').write_text(json.dumps(record, indent=2) + '\n')
    print(json.dumps(record, indent=2))


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--preview', action='store_true', help='Prepare review audio only, without runtime installation')
    prepare(parser.parse_args().preview)
