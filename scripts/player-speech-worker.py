"""Bounded offline recognition or explicit PortAudio capture; stdout is private IPC, not diagnostics."""
import argparse
import base64
import contextlib
import hashlib
import importlib.metadata
import json
import math
import sys
import time
from pathlib import Path

OUTPUT = sys.stdout
MAX_LINE = 32000


def emit(value):
    """Write bounded protocol data only; never retain PCM/text on disk."""
    line = json.dumps(value, separators=(",", ":"))
    if len(line) > MAX_LINE:
        raise ValueError("message bound")
    OUTPUT.write(line + "\n")
    OUTPUT.flush()


def messages():
    """Reject oversized, incomplete and malformed caller messages."""
    while True:
        line = sys.stdin.readline(MAX_LINE + 2)
        if not line:
            return
        if len(line) > MAX_LINE or not line.endswith("\n"):
            raise ValueError("message bound")
        value = json.loads(line)
        if not isinstance(value, dict):
            raise ValueError("message object")
        yield value


def local_model(model, manifest, language):
    """Require every selected asset to match the evaluated immutable local model."""
    root = model.resolve(strict=True)
    if manifest.stat().st_size > 32000:
        raise ValueError('manifest bound')
    rows = json.loads(manifest.read_text())[language]
    if len(rows) != 8 or {p.name for p in root.iterdir() if p.is_file() and p.name != '.DS_Store'} != {r['name'] for r in rows}:
        raise ValueError("model inventory")
    for row in rows:
        path = (root / row['name']).resolve(strict=True)
        if not path.is_relative_to(root) or path.stat().st_size != row['size']:
            raise ValueError("model path/size")
        with path.open('rb') as stream:
            if hashlib.file_digest(stream, 'sha256').hexdigest() != row['sha256']:
                raise ValueError("model hash")
    return root


def recognizer(args):
    """Use explicit local architecture 4; no downloader, mic, agent or intent API is used here."""
    if importlib.metadata.version('moonshine-voice') != '0.1.5':
        raise ValueError("recognizer version")
    import numpy as np
    from moonshine_voice import Transcriber, ModelArch
    root = local_model(args.model, args.manifest, args.language)
    transcriber = Transcriber(root, ModelArch.SMALL_STREAMING, update_interval=0.1)
    stream = transcriber.create_stream(update_interval=0.1)
    sequence = 0
    utterances = set()
    fault = False

    def notice(event):
        nonlocal sequence, fault
        line = getattr(event, 'line', None)
        name = type(event).__name__
        if name == 'Error':
            fault = True
            return
        kind = {'LineStarted': 'start', 'LineTextChanged': 'partial', 'LineCompleted': 'final'}.get(name)
        if line is None or kind is None:
            return
        identifier = str(line.line_id)
        text = line.text or ''
        if len(text) > 2000 or '\0' in text:
            fault = True
            return
        utterances.add(identifier)
        if len(utterances) > 32:
            fault = True
            return
        emit({'type':'event','sequence':sequence,'kind':kind,'utterance':identifier,'text':text})
        sequence += 1

    stream.add_listener(notice)
    stream.start()
    emit({'type':'ready'})
    expected = 0
    duration = 0
    try:
        for request in messages():
            if request.get('type') == 'finish':
                stream.stop()
                if fault:
                    raise ValueError('recognition event bound')
                emit({'type':'finished'})
                return
            if (set(request) != {'type','sequence','sampleRate','samples','pcm'} or request['type'] != 'audio'
                    or type(request['sequence']) is not int or request['sequence'] != expected
                    or type(request['samples']) is not int or not 1 <= request['samples'] <= 4096
                    or type(request['sampleRate']) is not int or not 8000 <= request['sampleRate'] <= 48000):
                raise ValueError("audio header")
            raw = base64.b64decode(request['pcm'], validate=True)
            if len(raw) != 4 * request['samples']:
                raise ValueError("audio length")
            pcm = np.frombuffer(raw, dtype='<f4')
            if not np.isfinite(pcm).all() or np.abs(pcm).max() > 1:
                raise ValueError("audio values")
            duration += len(pcm) / request['sampleRate']
            if duration > 31:
                raise ValueError("session duration")
            expected += 1
            stream.add_audio(pcm.tolist(), request['sampleRate'])
            if fault:
                raise ValueError('recognition event bound')
            emit({'type':'heartbeat'})
    finally:
        stream.close()
        transcriber.close()


def microphone(device_module=None):
    """Open only after start; blocked PCM read is retired by parent-owned process cancellation."""
    import numpy as np
    if device_module is None:
        if importlib.metadata.version('sounddevice') != '0.5.6':
            raise ValueError("capture version")
        import sounddevice as device_module
    sd = device_module
    emit({'type':'ready'})
    first = next(messages(), None)
    if first != {'type':'start'}:
        raise ValueError("explicit microphone start required")
    info = sd.query_devices(kind='input')
    fingerprint = tuple(info[key] for key in ('name','hostapi','max_input_channels','default_samplerate'))
    rate = int(min(48000, info['default_samplerate']))
    if not 8000 <= rate <= 48000:
        raise ValueError("sample rate")
    block = int(rate * 0.032)
    checked = time.monotonic()
    began = checked
    with sd.InputStream(samplerate=rate, channels=1, dtype='float32', blocksize=block, latency='low') as stream:
        for sequence in range(math.ceil(30 / 0.032)):
            data, overflow = stream.read(block)
            if overflow or not stream.active or time.monotonic() - began > 31:
                raise ValueError("capture unavailable")
            if time.monotonic() - checked >= 1:
                current = sd.query_devices(kind='input')
                if tuple(current[key] for key in ('name','hostapi','max_input_channels','default_samplerate')) != fingerprint:
                    raise ValueError("device changed")
                checked = time.monotonic()
            samples = np.asarray(data[:, 0], dtype='<f4')
            if samples.size != block or not np.isfinite(samples).all():
                raise ValueError("capture values")
            samples = np.clip(samples, -1, 1)
            emit({'type':'audio','sequence':sequence,'sampleRate':rate,'samples':len(samples),
                  'pcm':base64.b64encode(samples.tobytes()).decode('ascii')})
    emit({'type':'finished'})


def main():
    """Select exactly one explicit local process role."""
    parser = argparse.ArgumentParser()
    parser.add_argument('--mode', choices=('recognizer','capture'), required=True)
    parser.add_argument('--language', choices=('en','de'))
    parser.add_argument('--model', type=Path)
    parser.add_argument('--manifest', type=Path)
    args = parser.parse_args()
    try:
        with contextlib.redirect_stdout(sys.stderr):
            if args.mode == 'capture':
                microphone()
            else:
                if args.model is None or args.manifest is None or args.language is None:
                    raise ValueError("local configuration")
                recognizer(args)
    except (Exception, KeyboardInterrupt):
        emit({'type':'error','code':'input_unavailable'})
        return 1
    return 0


if __name__ == '__main__':
    raise SystemExit(main())
