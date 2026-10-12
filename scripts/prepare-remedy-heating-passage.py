#!/usr/bin/env python3
"""Reproduce the exact accepted-source heating passage without synthesis or source substitution."""
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
SOURCE = ROOT / 'audio-direction/remedy-heating-source'
REVIEW = ROOT / 'audio-direction/remedy-heating-runtime-review/source'
ORIGINAL = SOURCE / 'originals/cooking_with_cover_01.ogg'
ORIGINAL_SHA = 'ac5c065ae8f7b8dd660d9705c688dd1eff6ebc4b58dc81f6ca3c6912d8be81a8'
RATE, FRAMES, START, INPUT_FRAMES, FADE = 48000, 144000, 22050, 132300, 4800


def digest(data):
    """Hash exact originals, decoded samples and derivative bytes."""
    return hashlib.sha256(data).hexdigest()


def prepare():
    """Trim 0.5-3.5 seconds from the owner-accepted covered-pot original and document every conversion."""
    if digest(ORIGINAL.read_bytes()) != ORIGINAL_SHA:
        raise ValueError('Exact nominated original changed')
    authority = json.loads((ROOT / 'audio-direction/remedy-heating-runtime-authority.json').read_text())
    if authority['owner_statement'] != 'runtime approved':
        raise ValueError('Recorded runtime grant missing')
    probe = json.loads(subprocess.check_output(['ffprobe', '-v', 'quiet', '-show_streams', '-show_format', '-of', 'json', str(ORIGINAL)]))
    info = probe['streams'][0]
    if info['codec_name'] != 'vorbis' or info['sample_rate'] != '44100' or info['channels'] != 2:
        raise ValueError('Nominated source metadata changed')
    decoded = subprocess.check_output(['ffmpeg', '-v', 'error', '-i', str(ORIGINAL), '-f', 'f32le', '-acodec', 'pcm_f32le', '-'])
    window = decoded[START * 8:(START + INPUT_FRAMES) * 8]
    if len(window) != INPUT_FRAMES * 8:
        raise ValueError('No complete three-second window')
    conversion = 'aresample=48000:resampler=swr:filter_size=32:phase_shift=10:exact_rational=1'
    resampled = subprocess.run(['ffmpeg', '-v', 'error', '-f', 'f32le', '-ar', '44100', '-ac', '2', '-i', '-',
                                '-af', conversion, '-f', 'f32le', '-acodec', 'pcm_f32le', '-'], input=window, capture_output=True, check=True).stdout
    values = array.array('f', resampled)
    if sys.byteorder != 'little':
        values.byteswap()
    if len(values) != FRAMES * 2 or not all(math.isfinite(x) for x in values):
        raise ValueError('Conversion length/nonfinite failure')
    peak = max(abs(x) for x in values)
    if not 0 < peak < 1:
        raise ValueError('Silent or clipped decoded window')
    gain = min(1, 10 ** (-24.1 / 20) / peak)
    pcm = bytearray()
    for frame in range(FRAMES):
        fade = min(1, frame / FADE, (FRAMES - 1 - frame) / FADE)
        for channel in range(2):
            sample = round(values[frame * 2 + channel] * gain * fade * 8388608)
            pcm.extend(sample.to_bytes(3, 'little', signed=True))
    target = ROOT / 'game/assets/audio/remedy-heating/H01.wav'
    target.parent.mkdir(parents=True, exist_ok=True)
    with wave.open(str(target), 'wb') as writer:
        writer.setnchannels(2); writer.setsampwidth(3); writer.setframerate(RATE); writer.writeframes(pcm)
    REVIEW.mkdir(parents=True, exist_ok=True)
    (REVIEW / 'H01-candidate.wav').write_bytes(target.read_bytes())
    loudness = subprocess.run(['ffmpeg', '-hide_banner', '-nostats', '-i', str(target), '-af', 'ebur128=peak=true', '-f', 'null', '-'], capture_output=True, check=True).stderr.decode()
    (REVIEW / 'loudness.log').write_text(loudness)
    peak_out = max(abs(int.from_bytes(pcm[i:i+3], 'little', signed=True)) for i in range(0,len(pcm),3))
    record = {'schema_version':1,'status':'qualified_owner_accepted_original_derivative','creator':'TinyWorlds',
              'embedded_artist':'kddekadenz','title':'boiling water loops','licence':'CC0-1.0',
              'source_url':'https://opengameart.org/sites/default/files/cooking_with_cover_01.ogg',
              'catalogue_url':'https://opengameart.org/content/boiling-water-loops',
              'retrieved':'2026-10-12','original_sha256':ORIGINAL_SHA,'original_bytes':ORIGINAL.stat().st_size,
              'original_metadata':probe,'decoded_original_frames':len(decoded)//8,
              'decoded_original_float32_sha256':digest(decoded),'window_float32_sha256':digest(window),
              'resampled_float32_sha256':digest(resampled),'original_frame_start':START,
              'original_frame_end_exclusive':START+INPUT_FRAMES,'window_seconds':[0.5,3.5],
              'conversion':conversion,'ffmpeg':subprocess.check_output(['ffmpeg','-version'],text=True).splitlines()[0],
              'python':sys.version.split()[0],'uniform_gain_db':20*math.log10(gain),
              'fade_frames_each_end':FADE,'fade_shape':'linear','rate_hz':RATE,'frames':FRAMES,'channels':2,
              'encoding':'PCM24','loop':False,'wav_sha256':digest(target.read_bytes()),'pcm24_sha256':digest(pcm),
              'sample_peak_dbfs':20*math.log10(peak_out/8388608),
              'integrated_loudness_lufs':float(re.findall(r'I:\s*(-?[\d.]+) LUFS',loudness)[-1]),
              'source_acceptance':authority['source_acceptance'],
              'qualification_basis':'Owner listened to nominated covered-pot original and accepted sounds perfect. Deterministic interior window removes endpoints; technical decode/peak/frame checks pass. Agent does not claim an independent acoustic listening review.',
              'format_limit':'Vorbis is lossy. PCM24 storage preserves the decode without restoring source information.',
              'deed_capture':'Retained exact CC0 deed copied from prior rights evidence after live Creative Commons endpoint returned HTTP 403.'}
    for name in ['catalogue.html','deed.html']:
        record[name+'_sha256']=digest((SOURCE/'rights'/name).read_bytes())
    (SOURCE/'provenance.json').write_text(json.dumps(record,indent=2)+'\n')
    (target.parent/'provenance.json').write_text(json.dumps(record,indent=2)+'\n')
    (target.parent/'deed.html').write_bytes((SOURCE/'rights/deed.html').read_bytes())
    (target.parent/'CREDITS.txt').write_text('H01 — Heating preparation\nTinyWorlds — boiling water loops (embedded artist: kddekadenz)\nhttps://opengameart.org/content/boiling-water-loops\nOriginal: https://opengameart.org/sites/default/files/cooking_with_cover_01.ogg\nCC0 1.0 Universal: https://creativecommons.org/publicdomain/zero/1.0/\nChanges: Vorbis decode; contiguous 0.5–3.5s trim; 44.1→48kHz swr resampling; 100ms linear fades; one attenuation-only gain; stereo PCM24, no loop. Source is lossy; no added precision or creator endorsement is implied.\n')
    (REVIEW/'passage-measurements.json').write_text(json.dumps(record,indent=2)+'\n')
    print(json.dumps({k:record[k] for k in ['wav_sha256','frames','sample_peak_dbfs','integrated_loudness_lufs','uniform_gain_db']},indent=2))


if __name__ == '__main__':
    prepare()
