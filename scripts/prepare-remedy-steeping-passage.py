#!/usr/bin/env python3
"""Reproduce S01 from one hash-bound lossless source; installation requires recorded listening qualification."""
import argparse
import array
import hashlib
import json
import math
from pathlib import Path
import re
import subprocess
import sys
import wave

ROOT = Path(__file__).resolve().parents[1]
SOURCE = ROOT / 'audio-direction/remedy-steeping-source'
REVIEW = ROOT / 'audio-direction/remedy-steeping-runtime-review/source'
ORIGINAL = SOURCE / 'originals/0150-water-bubbles.wav'
RATE, FRAMES, START, INPUT_FRAMES, FADE = 48000, 144000, 0, 132300, 4800


def digest(data):
    """Hash exact originals, decoded samples and delivery bytes."""
    return hashlib.sha256(data).hexdigest()


def prepare(install=False):
    """Trim 0-3 seconds, resample/fade/attenuate once and retain reproducible measurements."""
    authority = json.loads((ROOT / 'audio-direction/remedy-steeping-runtime-authority.json').read_text())
    if authority['owner_statement'] != 'approved.':
        raise ValueError('Recorded runtime grant missing')
    original_sha = authority['selected_source']['original_sha256']
    if digest(ORIGINAL.read_bytes()) != original_sha:
        raise ValueError('Selected lossless original changed')
    qualification = authority.get('source_qualification')
    if install and (not qualification or qualification['status'] != 'qualified'):
        raise ValueError('Acoustic source/window qualification required before installation')
    probe = json.loads(subprocess.check_output(['ffprobe', '-v', 'quiet', '-show_streams', '-show_format', '-of', 'json', str(ORIGINAL)]))
    info = probe['streams'][0]
    if info['codec_name'] != 'pcm_s16le' or info['sample_rate'] != '44100' or info['channels'] != 1 or info['bits_per_sample'] != 16:
        raise ValueError('Lossless original metadata changed')
    decoded = subprocess.check_output(['ffmpeg', '-v', 'error', '-i', str(ORIGINAL), '-f', 'f32le', '-acodec', 'pcm_f32le', '-'])
    window = decoded[START * 4:(START + INPUT_FRAMES) * 4]
    if len(window) != INPUT_FRAMES * 4:
        raise ValueError('No complete three-second window')
    conversion = 'aresample=48000:resampler=swr:filter_size=32:phase_shift=10:exact_rational=1'
    resampled = subprocess.run(['ffmpeg', '-v', 'error', '-f', 'f32le', '-ar', '44100', '-ac', '1', '-i', '-', '-af', conversion, '-f', 'f32le', '-acodec', 'pcm_f32le', '-'], input=window, capture_output=True, check=True).stdout
    values = array.array('f', resampled)
    if sys.byteorder != 'little': values.byteswap()
    if len(values) != FRAMES or not all(math.isfinite(x) for x in values):
        raise ValueError('Conversion length/nonfinite failure')
    peak = max(abs(x) for x in values)
    if not 0 < peak < 1: raise ValueError('Silent or clipped window')
    gain = min(1, 10 ** (-24.1 / 20) / peak)
    pcm = bytearray()
    for frame in range(FRAMES):
        fade = min(1, frame / FADE, (FRAMES - 1 - frame) / FADE)
        for channel in range(2):
            sample = round(values[frame] * gain * fade * 8388608)
            pcm.extend(sample.to_bytes(3, 'little', signed=True))
    REVIEW.mkdir(parents=True, exist_ok=True)
    candidate = REVIEW / 'S01-candidate.wav'
    with wave.open(str(candidate), 'wb') as writer:
        writer.setnchannels(2); writer.setsampwidth(3); writer.setframerate(RATE); writer.writeframes(pcm)
    loudness = subprocess.run(['ffmpeg', '-hide_banner', '-nostats', '-i', str(candidate), '-af', 'ebur128=peak=true', '-f', 'null', '-'], capture_output=True, check=True).stderr.decode()
    (REVIEW / 'loudness.log').write_text(loudness)
    peak_out = max(abs(int.from_bytes(pcm[i:i+3], 'little', signed=True)) for i in range(0,len(pcm),3))
    record = {'schema_version':1,'status':'qualified' if qualification and qualification.get('status') == 'qualified' else 'technical_checks_passed_listening_pending',
              'creator':'Joseph SARDIN','title':'Water bubbles','licence':'CC0-1.0',
              'source_url':'https://bigsoundbank.com/UPLOAD/bwf-en/0150.wav',
              'catalogue_url':'https://bigsoundbank.com/water-bubbles-s0150.html',
              'licence_url':'https://bigsoundbank.com/licenses.html',
              'retrieved':'2026-10-12','original_sha256':original_sha,'original_bytes':ORIGINAL.stat().st_size,
              'original_metadata':probe,'decoded_original_frames':len(decoded)//4,
              'decoded_original_float32_sha256':digest(decoded),'window_float32_sha256':digest(window),
              'resampled_float32_sha256':digest(resampled),'original_frame_start':START,
              'original_frame_end_exclusive':START+INPUT_FRAMES,'window_seconds':[0,3],
              'conversion':conversion,'ffmpeg':subprocess.check_output(['ffmpeg','-version'],text=True).splitlines()[0],
              'python':sys.version.split()[0],'uniform_gain_db':20*math.log10(gain),
              'fade_frames_each_end':FADE,'fade_shape':'linear','rate_hz':RATE,'frames':FRAMES,'channels':2,
              'encoding':'PCM24','loop':False,'wav_sha256':digest(candidate.read_bytes()),'pcm24_sha256':digest(pcm),
              'sample_peak_dbfs':20*math.log10(peak_out/8388608),
              'integrated_loudness_lufs':float(re.findall(r'I:\s*(-?[\d.]+) LUFS',loudness)[-1]),
              'source_qualification':qualification,
              'qualification_limit':'Agent verifies lossless decode/format/frames/peak; acoustic assessment is recorded from owner listening, not inferred from catalogue or measurements.'}
    for name in ['catalogue.html','licence.html','deed.html']: record[name+'_sha256']=digest((SOURCE/'rights'/name).read_bytes())
    for target in [SOURCE/'provenance.json', REVIEW/'passage-measurements.json']:
        target.write_text(json.dumps(record,indent=2)+'\n')
    if install:
        if qualification.get('candidate_sha256') != record['wav_sha256'] or qualification.get('window_seconds') != record['window_seconds']:
            raise ValueError('Listening qualification does not bind this exact derivative/window')
        delivery=ROOT/'game/assets/audio/remedy-steeping'; delivery.mkdir(parents=True,exist_ok=True)
        (delivery/'S01.wav').write_bytes(candidate.read_bytes())
        (delivery/'provenance.json').write_text(json.dumps(record,indent=2)+'\n')
        (delivery/'deed.html').write_bytes((SOURCE/'rights/deed.html').read_bytes())
        (delivery/'CREDITS.txt').write_text('S01 — Steeping preparation\nJoseph SARDIN — Water bubbles (#0150)\nhttps://bigsoundbank.com/water-bubbles-s0150.html\nSource-specific licence: https://bigsoundbank.com/licenses.html\nOriginal: '+record['source_url']+'\nCC0 1.0 Universal: https://creativecommons.org/publicdomain/zero/1.0/\nChanges: lossless PCM16 WAV decode; contiguous 0–3s trim; 44.1→48kHz swr resampling; 100ms linear fades; one attenuation-only gain; identical mono duplication to stereo PCM24, no loop. No creator endorsement implied.\n')
    print(json.dumps({k:record[k] for k in ['status','wav_sha256','frames','sample_peak_dbfs','integrated_loudness_lufs','uniform_gain_db']},indent=2))


if __name__ == '__main__':
    parser=argparse.ArgumentParser(description=__doc__);parser.add_argument('--install',action='store_true');args=parser.parse_args()
    prepare(args.install)
