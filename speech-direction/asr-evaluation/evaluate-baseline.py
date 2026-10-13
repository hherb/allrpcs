#!/usr/bin/env python3
"""Compare local ASR on retained synthetic speech; never opens a microphone or submits game dialogue."""
from __future__ import annotations

import argparse
import hashlib
import importlib.metadata
import json
import os
from pathlib import Path
import re
import resource
import subprocess
import time
import urllib.request
import wave

import numpy as np
from scipy.signal import resample_poly

ROOT = Path(__file__).resolve().parents[2]
RATE = 16000
OUTPUT = ROOT / 'artifacts/asr-evaluation/results'
INPUTS = ROOT / 'artifacts/asr-evaluation/inputs'


def digest(path):
    """Bind exact local input/model bytes."""
    return hashlib.sha256(path.read_bytes()).hexdigest()


def normalize(text):
    """Case/punctuation-insensitive token scoring; German compounds remain single tokens."""
    return re.findall(r"[^\W_]+(?:'[^\W_]+)?", text.casefold(), re.UNICODE)


def errors(reference, hypothesis):
    """Return token edit distance and WER without concealing invented silence text."""
    expected, actual = normalize(reference), normalize(hypothesis)
    previous = list(range(len(actual) + 1))
    for i, word in enumerate(expected, 1):
        current = [i]
        for j, other in enumerate(actual, 1):
            current.append(min(current[-1] + 1, previous[j] + 1, previous[j - 1] + (word != other)))
        previous = current
    return {'word_errors': previous[-1], 'reference_words': len(expected),
            'wer': previous[-1] / len(expected) if expected else None,
            'silence_false_words': len(actual) if not expected else None}


def read_audio(path):
    """Read the retained mono PCM16 audition without changing its raw bytes."""
    with wave.open(str(path), 'rb') as handle:
        assert handle.getnchannels() == 1 and handle.getsampwidth() == 2
        rate = handle.getframerate()
        audio = np.frombuffer(handle.readframes(handle.getnframes()), '<i2').astype(np.float32) / 32768
    if rate != RATE:
        from math import gcd
        divisor = gcd(rate, RATE)
        audio = resample_poly(audio, RATE // divisor, rate // divisor).astype(np.float32)
    return audio


def write_audio(path, audio):
    """Retain deterministic 16 kHz PCM16 evaluation derivatives under ignored artifacts."""
    with wave.open(str(path), 'wb') as handle:
        handle.setnchannels(1); handle.setsampwidth(2); handle.setframerate(RATE)
        handle.writeframes((np.clip(audio, -1, 1) * 32767).astype('<i2').tobytes())


def cases(language):
    """Use two accepted synthetic phrases plus seeded 15 dB noise, pure noise and silence."""
    source = ROOT / 'speech-direction/offline-tts-spike'
    catalog = json.loads((source / 'cases.json').read_text())['pocket']
    INPUTS.mkdir(parents=True, exist_ok=True)
    rows = []
    for item in catalog:
        if not item['id'].startswith(language + '-') or item['style'] != 'normal':
            continue
        original = source / 'output/pocket' / (item['id'] + '.wav')
        clean = read_audio(original)
        noise = np.random.default_rng(20261013).normal(0, 1, len(clean)).astype(np.float32)
        rms = np.sqrt(np.mean(clean ** 2))
        noise *= rms / (10 ** (15 / 20) * np.sqrt(np.mean(noise ** 2)))
        for suffix, samples in [('clean', clean), ('noise15db', clean + noise)]:
            # Leading and trailing silence allow the streaming model's internal VAD to observe endpoints.
            padded = np.concatenate([np.zeros(RATE // 2), samples, np.zeros(RATE * 2)]).astype(np.float32)
            path = INPUTS / (item['id'] + '-' + suffix + '.wav')
            write_audio(path, padded)
            rows.append({'id':item['id'] + '-' + suffix, 'expected':item['text'], 'file':str(path),
                         'sha256':digest(path), 'original':str(original.relative_to(ROOT)),
                         'original_sha256':digest(original), 'speech_end_seconds':0.5 + len(clean) / RATE})
    for name, samples in [('silence', np.zeros(RATE * 4)),
                          ('noise-only', np.random.default_rng(20261013).normal(0, 0.02, RATE * 4))]:
        path = INPUTS / (language + '-' + name + '.wav'); write_audio(path, samples)
        rows.append({'id':language + '-' + name, 'expected':'', 'file':str(path), 'sha256':digest(path),
                     'speech_end_seconds':None})
    return rows


def rss_bytes(pid=None):
    """Observe resident size at explicit points; this is not a lifetime combined-game peak."""
    output = subprocess.check_output(['ps', '-o', 'rss=', '-p', str(pid or os.getpid())], text=True)
    return int(output.strip()) * 1024


def moonshine(language):
    """Exercise actual streaming updates, internal VAD and interruption with local cached models."""
    from moonshine_voice import Transcriber, ModelArch, get_model_for_language
    path, arch = get_model_for_language(language, wanted_model_arch=ModelArch.SMALL_STREAMING)
    started = time.perf_counter()
    transcriber = Transcriber(path, arch, update_interval=0.5)
    loaded = time.perf_counter() - started
    result = {'engine':'moonshine-small-streaming', 'language':language,
              'package_version':importlib.metadata.version('moonshine-voice'), 'load_seconds':loaded,
              'rss_loaded_bytes':rss_bytes(), 'model_files':[{'name':p.name, 'size':p.stat().st_size, 'sha256':digest(p)}
                                                            for p in sorted(Path(path).iterdir()) if p.is_file()],
              'cases':[]}
    for case in cases(language):
        samples = read_audio(Path(case['file']))
        stream = transcriber.create_stream(update_interval=0.5)
        events, latest, finalized = [], {}, {}
        start = time.perf_counter()
        def notice(event):
            line = getattr(event, 'line', None)
            if line is None:
                raise RuntimeError(str(event))
            value = {'event':type(event).__name__, 'wall_seconds':time.perf_counter()-start,
                     'text':line.text, 'line_id':line.line_id, 'audio_start':line.start_time,
                     'audio_duration':line.duration, 'complete':line.is_complete}
            events.append(value); latest[line.line_id] = line.text
            if line.is_complete: finalized[line.line_id] = line.text
        stream.add_listener(notice); stream.start()
        compute = 0.0
        for offset in range(0, len(samples), RATE // 10):
            began = time.perf_counter(); stream.add_audio(samples[offset:offset+RATE//10].tolist(), RATE)
            compute += time.perf_counter() - began
            target = start + min((offset + RATE/10) / RATE, len(samples) / RATE)
            time.sleep(max(0, target-time.perf_counter()))
        stop_start = time.perf_counter(); stream.stop(); stop_seconds = time.perf_counter()-stop_start
        count_at_close = len(events); stream.close(); time.sleep(0.05)
        assert len(events) == count_at_close
        text = ' '.join(finalized.get(key, value) for key,value in sorted(latest.items()))
        changes = [event for event in events if event['event']=='LineTextChanged' and event['text']]
        completions = [event for event in events if event['complete'] and event['text']]
        end = case['speech_end_seconds']
        result['cases'].append({**case, 'hypothesis':text, **errors(case['expected'], text),
            'audio_seconds':len(samples)/RATE, 'compute_seconds':compute, 'stop_seconds':stop_seconds,
            'first_partial_wall_seconds':changes[0]['wall_seconds'] if changes else None,
            'final_after_speech_seconds':max(0, completions[-1]['wall_seconds']-end) if completions and end else None,
            'rss_bytes':rss_bytes(), 'events':events, 'callbacks_after_close':0})
        print(language, case['id'], text, flush=True)
    # Stop can finalize remaining text: invalidate the application utterance before calling it.
    sample = read_audio(Path(cases(language)[2]['file']))[:RATE*2]
    stream = transcriber.create_stream(update_interval=0.5); retired=[]
    stream.add_listener(lambda event: retired.append(type(event).__name__))
    stream.start(); stream.add_audio(sample.tolist(), RATE)
    started=time.perf_counter(); stream.stop(); stream.close(); elapsed=time.perf_counter()-started
    before=len(retired); time.sleep(0.2)
    result['interruption']={'stop_and_close_seconds':elapsed,'late_callbacks_after_close':len(retired)-before,
                            'stop_may_finalize_text':True,'application_must_discard_retired_generation':True}
    result['process_peak_rss_bytes']=resource.getrusage(resource.RUSAGE_SELF).ru_maxrss
    transcriber.close()
    return result


def whisper_request(path, language):
    """Use one persistent loopback model server; no credentials or external inference service."""
    boundary='allrpcs-asr-evaluation-boundary'
    fields={'language':language,'temperature':'0.0','response_format':'json','vad':'true'}
    body=bytearray()
    for key,value in fields.items():
        body.extend(f'--{boundary}\r\nContent-Disposition: form-data; name="{key}"\r\n\r\n{value}\r\n'.encode())
    body.extend(f'--{boundary}\r\nContent-Disposition: form-data; name="file"; filename="input.wav"\r\nContent-Type: audio/wav\r\n\r\n'.encode())
    body.extend(path.read_bytes()); body.extend(f'\r\n--{boundary}--\r\n'.encode())
    request=urllib.request.Request('http://127.0.0.1:18599/inference',data=body,
                                  headers={'Content-Type':'multipart/form-data; boundary='+boundary})
    started=time.perf_counter()
    with urllib.request.urlopen(request,timeout=90) as response: value=json.load(response)
    return value.get('text','').strip(),time.perf_counter()-started


def whisper(language):
    """Measure warm batch recognition and rolling-prefix behavior; Whisper is not intrinsically streaming."""
    # Bind readiness before measuring requests; startup is outside warm recognition latency.
    deadline=time.monotonic()+30
    while True:
        try:
            with urllib.request.urlopen('http://127.0.0.1:18599/',timeout=1): pass
            break
        except OSError:
            if time.monotonic()>deadline: raise
            time.sleep(0.1)
    result={'engine':'whisper.cpp-small-silero-vad','language':language,'runtime_version':'v1.9.5',
            'vad_enabled':True,'vad_model_sha256':digest(ROOT/'artifacts/asr-evaluation/models/ggml-silero-v6.2.0.bin'),
            'source_commit':subprocess.check_output(['git','rev-parse','HEAD'],cwd=ROOT/'artifacts/asr-evaluation/whisper.cpp',text=True).strip(),
            'model_sha256':digest(ROOT/'artifacts/asr-evaluation/models/ggml-small.bin'),'cases':[]}
    for case in cases(language):
        text,elapsed=whisper_request(Path(case['file']),language)
        result['cases'].append({**case,'hypothesis':text,**errors(case['expected'],text),
                                'recognition_seconds':elapsed,'audio_seconds':len(read_audio(Path(case['file'])))/RATE})
        print(language,case['id'],text,flush=True)
    case=next(case for case in cases(language) if '-game-clean' in case['id']); samples=read_audio(Path(case['file']))
    result['rolling_prefix_probe']=[]
    for seconds in (2,4,6):
        path=INPUTS/(language+'-prefix.wav'); write_audio(path,samples[:min(int(seconds*RATE),len(samples))])
        text,elapsed=whisper_request(path,language)
        result['rolling_prefix_probe'].append({'available_audio_seconds':seconds,'hypothesis':text,'recognition_seconds':elapsed})
    result['streaming_kind']='rolling full re-decode; partials are unstable and need an external VAD/commit policy'
    return result


def main():
    """Run exactly one backend/language to isolate process memory and keep experiments finite."""
    parser=argparse.ArgumentParser(); parser.add_argument('backend',choices=['moonshine','whisper']); parser.add_argument('language',choices=['en','de'])
    args=parser.parse_args(); OUTPUT.mkdir(parents=True,exist_ok=True)
    result=(moonshine if args.backend=='moonshine' else whisper)(args.language)
    result.update(host_architecture=os.uname().machine,provenance='retained Pocket synthetic speech; seeded noise and silence; no microphone or accent test',
                  python_version=os.sys.version,load_average=os.getloadavg())
    path=OUTPUT/(args.backend+'-'+args.language+'.json'); path.write_text(json.dumps(result,indent=2,ensure_ascii=False)+'\n')
    print('Saved',path,flush=True)


if __name__=='__main__': main()
