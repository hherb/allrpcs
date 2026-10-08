#!/usr/bin/env python3
"""Run the exact retained smoke commands, retrying only diagnosed native shutdown crashes."""
import hashlib
import json
import os
from pathlib import Path
import shlex
import subprocess

ROOT = Path(__file__).resolve().parents[3]
REVIEW = ROOT / 'audio-direction/interior-runtime-review'
script = ROOT / 'scripts/smoke-godot.sh'
text = script.read_text()
commands = []
for line in text.replace('\\\n', ' ').splitlines():
    if line.startswith(('godot ', 'ALLRPCS_')):
        words = shlex.split(line.replace('$repository_root', str(ROOT)))
        environment = {}
        while words and '=' in words[0] and words[0].startswith('ALLRPCS_'):
            key, value = words.pop(0).split('=', 1)
            environment[key] = value
        assert words and words[0] == 'godot'
        commands.append((environment, words))
assert len(commands) == 293, len(commands)
environment = {**os.environ, 'ALLRPCS_CHARACTER_MODEL_COMPOSITION_DISABLED': '1', 'ALLRPCS_ORDINARY_PARTICIPATION_SMOKE_MODEL': '1'}
rows = []
with (REVIEW / 'logs/retained-godot-smoke.log').open('w') as log:
    for index, (local, args) in enumerate(commands, 1):
        attempts = []
        for attempt in range(1, 4):
            result = subprocess.run(args, env={**environment, **local}, cwd=ROOT, capture_output=True, text=True)
            log.write(f'Launch {index} attempt {attempt}: ' + ' '.join(args) + '\n' + result.stdout + result.stderr)
            log.flush()
            attempts.append({'attempt': attempt, 'exit_code': result.returncode})
            if result.returncode == 0:
                break
            native_shutdown = result.returncode in (-6, 134) and 'mutex lock failed' in result.stderr
            if not native_shutdown or attempt == 3:
                raise RuntimeError(f'Retained smoke launch {index} failed: {result.stderr}')
        rows.append({'launch': index, 'environment': local, 'arguments': args, 'attempts': attempts})
        if index % 10 == 0:
            print(f'PASS: {index}/293 retained Godot launches', flush=True)
(REVIEW / 'retained-smoke.json').write_text(json.dumps({'schema_version': 1, 'script_sha256': hashlib.sha256(script.read_bytes()).hexdigest(),
    'launches': len(rows), 'commands': rows, 'native_shutdown_retry_count': sum(len(row['attempts']) - 1 for row in rows),
    'retry_contract': 'Identical retained command and environment; retry only abort with native mutex-lock shutdown error; no fixture or product changes.'}, indent=2) + '\n')
print('PASS: all 293 exact retained commands completed with successful exit codes', flush=True)
