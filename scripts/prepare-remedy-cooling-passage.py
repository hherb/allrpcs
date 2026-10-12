#!/usr/bin/env python3
"""Reproduce C01 from a lossless source; exact listening acceptance gates installation."""

import argparse
import array
import gzip
import hashlib
import json
import math
from pathlib import Path
import re
import subprocess
import sys
import wave

ROOT = Path(__file__).resolve().parents[1]
SOURCE = ROOT / 'audio-direction/remedy-cooling-source'
REVIEW = ROOT / 'audio-direction/remedy-cooling-runtime-review/source'
RATE, FRAMES, START = 48000, 36000, 4800
ONSET_FADE, END_FADE = 480, 2400
PEAK_TARGET_DBFS = -30.1


def digest(data):
    """Hash exact originals, decoded samples and delivery bytes."""
    return hashlib.sha256(data).hexdigest()


def capture_bytes(name):
    """Read exact raw capture bytes from plain or lossless compressed evidence."""
    path = SOURCE / 'rights' / name
    compressed = path.with_suffix(path.suffix + '.gz')
    return gzip.decompress(compressed.read_bytes()) if compressed.exists() else path.read_bytes()


def prepare(install=False):
    """Trim 0.10-0.85 seconds, fade/attenuate once and retain reproducible measurements."""
    authority = json.loads((ROOT / 'audio-direction/remedy-cooling-runtime-authority.json').read_text())
    if authority['owner_statement'] != 'approved':
        raise ValueError('Recorded cooling source/runtime grant missing')
    selected = authority['selected_source']
    original = ROOT / selected['original_path']
    if digest(original.read_bytes()) != selected['original_sha256']:
        raise ValueError('Selected lossless original changed')
    qualification = authority.get('source_qualification')
    if install and (not qualification or qualification['status'] != 'qualified'):
        raise ValueError('Exact owner listening acceptance required before installation')
    probe = json.loads(subprocess.check_output([
        'ffprobe', '-v', 'quiet', '-show_streams', '-show_format', '-of', 'json', str(original)]))
    info = probe['streams'][0]
    if (len(probe['streams']) != 1 or info['codec_name'] != 'pcm_s16le'
            or info['sample_rate'] != str(RATE) or info['channels'] != 1 or info['bits_per_sample'] != 16):
        raise ValueError('Lossless original metadata changed')
    decoded = subprocess.check_output([
        'ffmpeg', '-v', 'error', '-i', str(original), '-f', 'f32le', '-acodec', 'pcm_f32le', '-'])
    window = decoded[START * 4:(START + FRAMES) * 4]
    if len(window) != FRAMES * 4:
        raise ValueError('No complete 0.75-second window')
    values = array.array('f', window)
    if sys.byteorder != 'little':
        values.byteswap()
    if not all(math.isfinite(value) for value in values):
        raise ValueError('Nonfinite source samples')
    peak = max(abs(value) for value in values)
    if not 0 < peak < 1:
        raise ValueError('Silent or clipped window')
    gain = min(1, 10 ** (PEAK_TARGET_DBFS / 20) / peak)
    pcm = bytearray()
    for frame, value in enumerate(values):
        fade = min(1, frame / ONSET_FADE, (FRAMES - 1 - frame) / END_FADE)
        sample = round(value * gain * fade * 8388608)
        pcm.extend(sample.to_bytes(3, 'little', signed=True) * 2)
    REVIEW.mkdir(parents=True, exist_ok=True)
    candidate = REVIEW / 'C01-candidate.wav'
    with wave.open(str(candidate), 'wb') as writer:
        writer.setnchannels(2)
        writer.setsampwidth(3)
        writer.setframerate(RATE)
        writer.writeframes(pcm)
    loudness = subprocess.run([
        'ffmpeg', '-hide_banner', '-nostats', '-i', str(candidate), '-af', 'ebur128=peak=true',
        '-f', 'null', '-'], capture_output=True, check=True).stderr.decode()
    loudness_bytes = loudness.encode()
    loudness_stored = gzip.compress(loudness_bytes, mtime=0)
    (REVIEW / 'loudness.log.gz').write_bytes(loudness_stored)
    peak_out = max(abs(int.from_bytes(pcm[i:i + 3], 'little', signed=True))
                   for i in range(0, len(pcm), 3))
    record = {
        'schema_version': 1,
        'status': 'qualified' if qualification and qualification.get('status') == 'qualified'
                  else 'technical_checks_passed_listening_pending',
        **selected,
        'original_metadata': probe, 'decoded_original_frames': len(decoded) // 4,
        'decoded_original_float32_sha256': digest(decoded), 'window_float32_sha256': digest(window),
        'original_frame_start': START, 'original_frame_end_exclusive': START + FRAMES,
        'conversion': 'No resampling required; original 48 kHz retained. Identical mono duplication.',
        'ffmpeg': subprocess.check_output(['ffmpeg', '-version'], text=True).splitlines()[0],
        'python': sys.version.split()[0], 'uniform_gain_db': 20 * math.log10(gain),
        'onset_fade_frames': ONSET_FADE, 'ending_fade_frames': END_FADE, 'fade_shape': 'linear',
        'rate_hz': RATE, 'frames': FRAMES, 'channels': 2, 'encoding': 'PCM24', 'loop': False,
        'wav_sha256': digest(candidate.read_bytes()), 'pcm24_sha256': digest(pcm),
        'sample_peak_dbfs': 20 * math.log10(peak_out / 8388608),
        'integrated_loudness_lufs': float(re.findall(r'I:\s*(-?[\d.]+) LUFS', loudness)[-1]),
        'loudness_log': {
            'path': 'audio-direction/remedy-cooling-runtime-review/source/loudness.log.gz',
            'raw_sha256': digest(loudness_bytes), 'stored_sha256': digest(loudness_stored),
            'compression': 'gzip, mtime 0; exact unmodified tool output',
        },
        'source_qualification': qualification,
        'qualification_limit': 'Technical checks only. Catalogue and measurements do not establish acoustic '
                               'suitability; exact owner listening acceptance is required before installation.',
    }
    for name in ['catalogue.html', 'licence.html', 'deed.html']:
        record[name + '_sha256'] = digest(capture_bytes(name))
    for target in [SOURCE / 'provenance.json', REVIEW / 'passage-measurements.json']:
        target.write_text(json.dumps(record, indent=2) + '\n')
    if install:
        if (qualification.get('candidate_sha256') != record['wav_sha256']
                or qualification.get('window_seconds') != record['window_seconds']):
            raise ValueError('Listening acceptance does not bind this exact derivative/window')
        delivery = ROOT / 'game/assets/audio/remedy-cooling'
        delivery.mkdir(parents=True, exist_ok=True)
        (delivery / 'C01.wav').write_bytes(candidate.read_bytes())
        (delivery / 'provenance.json').write_text(json.dumps(record, indent=2) + '\n')
        (delivery / 'deed.html').write_bytes((SOURCE / 'rights/deed.html').read_bytes())
        (delivery / 'CREDITS.txt').write_text(
            'C01 — Cooling step committed (interface confirmation)\n'
            'Joseph SARDIN — Finger clashes (#0483)\n'
            + selected['catalogue_url'] + '\nOriginal: ' + selected['original_url'] + '\n'
            'Source-specific licence: https://bigsoundbank.com/licenses.html\n'
            'CC0 1.0 Universal: https://creativecommons.org/publicdomain/zero/1.0/\n'
            'Changes: lossless PCM16 WAV decode; contiguous 0.10–0.85s trim; 48kHz retained; '
            '10ms onset and 50ms ending linear fades; one attenuation-only gain; '
            'identical mono duplication to stereo PCM24; no loop. No creator endorsement implied.\n')
    print(json.dumps({key: record[key] for key in [
        'status', 'wav_sha256', 'frames', 'sample_peak_dbfs', 'integrated_loudness_lufs', 'uniform_gain_db']},
        indent=2))


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--install', action='store_true')
    prepare(parser.parse_args().install)
