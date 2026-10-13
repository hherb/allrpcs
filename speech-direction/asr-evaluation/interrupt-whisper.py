#!/usr/bin/env python3
"""Abort a fixed synthetic request by closing its loopback connection, then prove server recovery."""
import importlib.util
import json
from pathlib import Path
import socket
import time

import numpy as np

HERE = Path(__file__).resolve().parent
spec = importlib.util.spec_from_file_location('asr_evaluation', HERE/'evaluate.py')
module = importlib.util.module_from_spec(spec)
spec.loader.exec_module(module)
case = next(row for row in module.cases('en') if '-game-clean' in row['id'])
source = module.read_audio(Path(case['file']))
path = module.INPUTS/'interrupt-repeated.wav'
module.write_audio(path, np.tile(source, 8))
boundary = 'allrpcs-interruption-boundary'
body = bytearray()
for key,value in {'language':'en','temperature':'0.0','response_format':'json','vad':'true'}.items():
    body.extend(f'--{boundary}\r\nContent-Disposition: form-data; name="{key}"\r\n\r\n{value}\r\n'.encode())
body.extend(f'--{boundary}\r\nContent-Disposition: form-data; name="file"; filename="input.wav"\r\nContent-Type: audio/wav\r\n\r\n'.encode())
body.extend(path.read_bytes()); body.extend(f'\r\n--{boundary}--\r\n'.encode())
connection = socket.create_connection(('127.0.0.1',18599), timeout=15)
header = f'POST /inference HTTP/1.1\r\nHost: 127.0.0.1:18599\r\nContent-Type: multipart/form-data; boundary={boundary}\r\nContent-Length: {len(body)}\r\nConnection: close\r\n\r\n'.encode()
started = time.perf_counter(); connection.sendall(header+body)
time.sleep(0.1)
close_started = time.perf_counter(); connection.shutdown(socket.SHUT_RDWR); connection.close()
closed = time.perf_counter()-close_started
text, elapsed = module.whisper_request(Path(case['file']), 'en')
result = {'status':'passed' if 'not take' in text.casefold() else 'failed',
          'audio_seconds':len(source)*8/module.RATE, 'connection_closed_after_seconds':time.perf_counter()-started-elapsed,
          'socket_close_seconds':closed, 'subsequent_request_seconds_including_prior_cleanup':elapsed,
          'subsequent_hypothesis':text,
          'abort_mechanism':'whisper-server v1.9.5 request socket close -> native abort_callback req.is_connection_closed()',
          'scope':'connection close/recovery, not a physical barge-in or a direct kernel-stop timestamp'}
output = module.OUTPUT/'whisper-interruption.json';output.write_text(json.dumps(result,indent=2)+'\n')
print(json.dumps(result,indent=2))
