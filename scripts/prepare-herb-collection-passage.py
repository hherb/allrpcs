#!/usr/bin/env python3
"""Prepare the exact HC01 candidate; owner listening gates runtime installation."""

import argparse
import array
import copy
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
AUTHORITY = ROOT / 'audio-direction/herb-collection-runtime-authority.json'
SOURCE = ROOT / 'audio-direction/herb-collection-source'
REVIEW = ROOT / 'audio-direction/herb-collection-runtime-review/source'
RATE, FRAMES, START = 48000, 48000, 4800
ONSET_FADE, END_FADE = 480, 2400
MEMBER = 'rustle/rustle14.flac'
TARGET_DBFS = -24.0


def digest(data):
    """Bind exact archive, lossless member, PCM and capture bytes."""
    return hashlib.sha256(data).hexdigest()


def require(condition, message):
    """Reject a changed grant or source before preparing any output."""
    if not condition:
        raise ValueError(message)


def capture_bytes(name):
    """Read immutable HTML from its exact plain or losslessly compressed storage."""
    plain = SOURCE / 'rights' / name
    compressed = plain.with_suffix(plain.suffix + '.gz')
    return gzip.decompress(compressed.read_bytes()) if compressed.exists() else plain.read_bytes()


def validate_contract(authority):
    """Require this producer's source/runtime grant and its frozen proposal."""
    require(authority['scope_id'] == 'phase9-herb-collection-audio-runtime-integration'
            and authority['owner_statement'] == 'approved'
            and authority['source_use_runtime_authorized'] is True,
            'Exact herb-collection source/runtime grant missing')
    for binding in authority['proposal_bindings']:
        require(digest((ROOT / binding['path']).read_bytes()) == binding['sha256'],
                'Frozen proposal changed')
    selected = authority['selected_source']
    require(selected is not None and selected['licence'] == 'CC0-1.0',
            'Selected source-specific CC0 evidence missing')
    require(selected['original_archive_member'] == MEMBER
            and selected['window_seconds'] == [0.1, 1.1]
            and selected['gain_target_sample_peak_dbfs'] == TARGET_DBFS,
            'Selected member/window/level changed')
    for name in ['catalogue.html', 'deed.html']:
        retained = next(row for row in selected['rights_retrieval']
                        if row['url'] == (selected['catalogue_url'] if name == 'catalogue.html'
                                          else 'https://creativecommons.org/publicdomain/zero/1.0/'))
        require(digest(capture_bytes(name)) == retained['sha256'],
                'Source-specific rights capture changed')
    return selected


def validate_metadata(probe):
    """Qualify the selected member's lossless encoding, never a converted preview."""
    streams = probe.get('streams', [])
    require(len(streams) == 1, 'Expected one lossless audio stream')
    info = streams[0]
    require(info['codec_name'] == 'flac' and info['sample_rate'] == '96000'
            and info['channels'] == 2 and info['bits_per_raw_sample'] == '24',
            'Original FLAC metadata changed')


def installation_allowed(authority, candidate_sha256):
    """Require exact accepted bytes/window; scope approval alone cannot install PCM."""
    qualification = authority.get('source_qualification', {})
    return (qualification.get('status') == 'qualified'
            and bool(qualification.get('owner_statement'))
            and qualification.get('candidate_sha256') == candidate_sha256
            and qualification.get('window_seconds') == [0.1, 1.1])


def encode_window(values):
    """Keep stereo identity; apply only uniform attenuation and the agreed linear fades."""
    require(len(values) == FRAMES * 2 and all(math.isfinite(value) for value in values),
            'Incomplete or nonfinite resampled window')
    peak = max(abs(value) for value in values)
    require(0 < peak < 1, 'Silent or clipped window')
    gain = min(1.0, 10 ** (TARGET_DBFS / 20) / peak)
    pcm = bytearray()
    for frame in range(FRAMES):
        fade = min(1.0, frame / ONSET_FADE, (FRAMES - 1 - frame) / END_FADE)
        for channel in range(2):
            sample = round(values[frame * 2 + channel] * gain * fade * 8388608)
            require(-8388608 <= sample < 8388608, 'PCM24 overflow')
            pcm.extend(sample.to_bytes(3, 'little', signed=True))
    return bytes(pcm), gain


def prepare(install=False):
    """Reproduce one candidate from the retained download archive and unchanged FLAC member."""
    authority = json.loads(AUTHORITY.read_text())
    selected = validate_contract(authority)
    require(not install or authority.get('source_qualification', {}).get('status') == 'qualified',
            'Exact owner listening acceptance required before installation')
    original = ROOT / selected['original_path']
    require(digest(original.read_bytes()) == selected['original_sha256'], 'Original archive changed')
    member = subprocess.check_output(['bsdtar', '-xOf', str(original), MEMBER])
    require(digest(member) == selected['lossless_member_sha256'], 'Original lossless member changed')
    probe = json.loads(subprocess.check_output([
        'ffprobe', '-v', 'quiet', '-show_streams', '-show_format', '-of', 'json',
        '-i', 'pipe:0'], input=member))
    validate_metadata(probe)
    original_decoded = subprocess.check_output([
        'ffmpeg', '-v', 'error', '-i', 'pipe:0', '-f', 'f32le', '-acodec', 'pcm_f32le', '-'],
        input=member)
    resampled = subprocess.check_output([
        'ffmpeg', '-v', 'error', '-i', 'pipe:0', '-ar', str(RATE), '-f', 'f32le',
        '-acodec', 'pcm_f32le', '-'], input=member)
    window = resampled[START * 8:(START + FRAMES) * 8]
    values = array.array('f', window)
    if sys.byteorder != 'little':
        values.byteswap()
    pcm, gain = encode_window(values)
    REVIEW.mkdir(parents=True, exist_ok=True)
    candidate = REVIEW / 'HC01-candidate.wav'
    with wave.open(str(candidate), 'wb') as writer:
        writer.setnchannels(2)
        writer.setsampwidth(3)
        writer.setframerate(RATE)
        writer.writeframes(pcm)
    candidate_sha256 = digest(candidate.read_bytes())
    require(not install or installation_allowed(authority, candidate_sha256),
            'Listening acceptance does not bind exact candidate bytes/window')
    loudness = subprocess.run([
        'ffmpeg', '-hide_banner', '-nostats', '-i', str(candidate),
        '-af', 'ebur128=peak=true', '-f', 'null', '-'], capture_output=True, check=True).stderr
    compressed = gzip.compress(loudness, mtime=0)
    (REVIEW / 'loudness.log.gz').write_bytes(compressed)
    peak = max(abs(int.from_bytes(pcm[index:index + 3], 'little', signed=True))
               for index in range(0, len(pcm), 3)) / 8388608
    record = {
        'schema_version': 1,
        'status': 'qualified' if installation_allowed(authority, candidate_sha256)
                  else 'technical_checks_passed_listening_pending',
        **selected,
        'original_metadata': probe,
        'decoded_original_frames': len(original_decoded) // 8,
        'decoded_original_float32_sha256': digest(original_decoded),
        'resampled_float32_sha256': digest(resampled),
        'window_float32_sha256': digest(window),
        'original_frame_start': 9600,
        'original_frame_end_exclusive': 105600,
        'resampled_frame_start': START,
        'resampled_frame_end_exclusive': START + FRAMES,
        'conversion': 'Lossless FLAC decode; required ffmpeg 96-to-48 kHz resampling; stereo retained.',
        'ffmpeg': subprocess.check_output(['ffmpeg', '-version'], text=True).splitlines()[0],
        'bsdtar': subprocess.check_output(['bsdtar', '--version'], text=True).strip(),
        'python': sys.version.split()[0],
        'uniform_gain_db': 20 * math.log10(gain),
        'onset_fade_frames': ONSET_FADE,
        'ending_fade_frames': END_FADE,
        'fade_shape': 'linear',
        'rate_hz': RATE, 'frames': FRAMES, 'channels': 2, 'encoding': 'PCM24', 'loop': False,
        'wav_sha256': candidate_sha256, 'pcm24_sha256': digest(pcm),
        'sample_peak_dbfs': 20 * math.log10(peak),
        'integrated_loudness_lufs': float(re.findall(rb'I:\s*(-?[\d.]+) LUFS', loudness)[-1]),
        'loudness_log': {
            'path': str((REVIEW / 'loudness.log.gz').relative_to(ROOT)),
            'raw_sha256': digest(loudness), 'stored_sha256': digest(compressed),
            'compression': 'gzip, mtime 0; exact unmodified tool output',
        },
        'source_qualification': authority.get('source_qualification'),
        'qualification_limit': ('Exact unchanged sound/window/level accepted with suitable. ' if installation_allowed(authority, candidate_sha256) else 'Technical checks only. ') + 'Catalogue/metadata/envelope/levels do not '
                               'establish acoustic suitability; exact owner listening is required.',
    }
    for target in [SOURCE / 'provenance.json', REVIEW / 'passage-measurements.json']:
        target.write_text(json.dumps(record, indent=2) + '\n')
    if install:
        delivery = ROOT / 'game/assets/audio/herb-collection'
        delivery.mkdir(parents=True, exist_ok=True)
        (delivery / 'HC01.wav').write_bytes(candidate.read_bytes())
        (delivery / 'provenance.json').write_text(json.dumps(record, indent=2) + '\n')
        (delivery / 'deed.html').write_bytes((SOURCE / 'rights/deed.html').read_bytes())
        (delivery / 'CREDITS.txt').write_text(
            'HC01 — Soft rustle (nearby collection acoustic feedback)\n'
            'qubodup — 20 Rustles of dry leaves; rustle/rustle14.flac\n'
            + selected['catalogue_url'] + '\nOriginal archive: ' + selected['original_url'] + '\n'
            'CC0 1.0 Universal: https://creativecommons.org/publicdomain/zero/1.0/\n'
            'Changes: contiguous 0.10–1.10s window, required 96-to-48kHz resampling, retained stereo, '
            '10ms onset/50ms ending linear fades, uniform attenuation to −24dBFS sample peak, '
            'stereo PCM24. No loop. No creator endorsement implied.\n')
    print(json.dumps({key: record[key] for key in [
        'status', 'wav_sha256', 'frames', 'sample_peak_dbfs',
        'integrated_loudness_lufs', 'uniform_gain_db']}, indent=2))


def self_test():
    """Probe changed grants/source contracts, unsafe encodings and pre-acceptance installation."""
    authority = json.loads(AUTHORITY.read_text())
    validate_contract(authority)
    rejected = 0
    for field, value in [('owner_statement', 'pending'), ('source_use_runtime_authorized', False)]:
        bad = copy.deepcopy(authority)
        bad[field] = value
        try:
            validate_contract(bad)
        except ValueError:
            rejected += 1
    for field, value in [('licence', 'CC-BY-NC-4.0'), ('original_archive_member', '../escape.flac'),
                         ('window_seconds', [0, 1]), ('gain_target_sample_peak_dbfs', 0)]:
        bad = copy.deepcopy(authority)
        bad['selected_source'][field] = value
        try:
            validate_contract(bad)
        except ValueError:
            rejected += 1
    valid = {'streams': [{'codec_name': 'flac', 'sample_rate': '96000',
                          'channels': 2, 'bits_per_raw_sample': '24'}]}
    for field, value in [('codec_name', 'mp3'), ('sample_rate', '44100'), ('channels', 6)]:
        bad = copy.deepcopy(valid)
        bad['streams'][0][field] = value
        try:
            validate_metadata(bad)
        except ValueError:
            rejected += 1
    candidate_hash = '1' * 64
    bad = copy.deepcopy(authority)
    bad['source_qualification'] = {'status': 'awaiting_exact_owner_listening'}
    require(not installation_allowed(bad, candidate_hash), 'Pending source could install')
    rejected += 1
    bad['source_qualification'] = {'status': 'qualified', 'owner_statement': 'suitable',
                                 'candidate_sha256': candidate_hash, 'window_seconds': [0.1, 1.1]}
    require(installation_allowed(bad, candidate_hash), 'Exact acceptance rejected')
    require(not installation_allowed(bad, '2' * 64), 'Different candidate could install')
    rejected += 1
    require(rejected == 11, 'A required rejection probe failed')
    print('PASS: herb-collection preparation rejection probes: 11')


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--install', action='store_true')
    parser.add_argument('--self-test', action='store_true')
    options = parser.parse_args()
    self_test() if options.self_test else prepare(options.install)
