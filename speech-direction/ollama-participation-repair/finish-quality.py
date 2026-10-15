#!/usr/bin/env python3
"""Reuse the exact passed prefix after the recorded fixture-only whitespace correction."""
import copy
import hashlib
import importlib.util
import json
from pathlib import Path
import re
import sys
import tempfile
import unittest

ROOT = Path(__file__).resolve().parents[2]
ARCHIVE = Path(__file__).resolve().parent / 'quality/attempt1'
FIXTURE = 'game/OllamaParticipationMainCheck.cs'
TOKEN = re.compile(r'//[^\n]*|/\*.*?\*/|@"(?:[^"]|"")*"|"(?:\\.|[^"\\])*"|\'(?:\\.|[^\'\\])*\'|[^\s]', re.S)


def sha(data):
    """Bind exact bytes independently of modification times."""
    return hashlib.sha256(data).hexdigest()


def validate(prior, root, archive):
    """Reject CI, source, token, completed-check or retained-log drift."""
    if prior.get('status') != 'failed' or prior.get('failed_stage') != 'product-3':
        raise ValueError('Expected the recorded formatting-stage failure')
    rows = prior['stages']
    if len(rows) != 25 or rows[-1]['arguments'] != ['sh', '-c', 'dotnet format Allrpcs.sln --no-restore --verify-no-changes']:
        raise ValueError('Unexpected failed command or stage inventory')
    for name, digest in prior['inputs'].items():
        if sha((root / name).read_bytes()) != digest:
            raise ValueError('CI input drift: ' + name)
    inputs = json.loads((archive / 'source-inputs.json').read_text())['inputs']
    if FIXTURE not in {row['path'] for row in inputs}:
        raise ValueError('Missing fixture receipt')
    for row in inputs:
        path = row['path']
        if Path(path).is_absolute() or '..' in Path(path).parts:
            raise ValueError('Unsafe source path')
        current = (root / path).read_bytes()
        if path == FIXTURE:
            baseline = (archive / 'OllamaParticipationMainCheck.cs').read_bytes()
            if sha(baseline) != row['sha256'] or TOKEN.findall(baseline.decode()) != TOKEN.findall(current.decode()):
                raise ValueError('Fixture changed beyond whitespace')
        elif sha(current) != row['sha256']:
            raise ValueError('Product input drift: ' + path)
    for index, row in enumerate(rows):
        if not row['attempts']:
            raise ValueError('Missing stage attempts')
        for attempt in row['attempts']:
            name = attempt['log']
            if Path(name).name != name or sha((archive / name).read_bytes()) != attempt['sha256']:
                raise ValueError('Stage log drift')
        last = row['attempts'][-1]
        if index < 24 and (last['exit_code'] != 0 or not last['completion_seen']):
            raise ValueError('Prefix contains an unsuccessful check')
    failure = rows[-1]['attempts'][-1]
    errors = (archive / failure['log']).read_text().splitlines()
    if failure['exit_code'] != 2 or not errors or any(
            'game/OllamaParticipationMainCheck.cs(' not in line or 'error WHITESPACE:' not in line for line in errors):
        raise ValueError('Failure was not limited to fixture formatting')
    return copy.deepcopy(rows[:24])


def main():
    """Run the unchanged full pipeline, retaining only signature-matched completed stages."""
    prior_bytes = (ARCHIVE / 'summary.json').read_bytes()
    prior = json.loads(prior_bytes)
    directory = ROOT / 'artifacts/quality-current'
    if (directory / 'summary.json').read_bytes() != prior_bytes:
        raise ValueError('Current failed report differs from its archive')
    prefix = validate(prior, ROOT, ARCHIVE)
    spec = importlib.util.spec_from_file_location('allrpcs_quality', ROOT / 'quality.py')
    quality = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(quality)
    if prior['product_revision'] != quality.git('rev-parse', 'HEAD').decode().strip() or prior['python'] != sys.version:
        raise ValueError('Revision or interpreter changed')
    original = quality.Run.stage
    cursor = 0

    def stage(run, name, args, cwd=ROOT, env=None, revision=None, retry_native=False, completion=None):
        nonlocal cursor
        if cursor >= len(prefix):
            return original(run, name, args, cwd, env, revision, retry_native, completion)
        row = prefix[cursor]
        cursor += 1
        # Snapshot indexes are disposable; their exact commit identity is checked separately.
        old_env = {k: v for k, v in row['environment_overrides'].items() if k != 'GIT_INDEX_FILE'}
        new_env = {k: v for k, v in (env or {}).items() if k != 'GIT_INDEX_FILE'}
        if (name, args, revision, new_env) != (row['name'], row['arguments'], row['revision'], old_env):
            raise ValueError('Completed stage signature changed: ' + name)
        if any(a['required_completion'] != completion for a in row['attempts']):
            raise ValueError('Completion contract changed: ' + name)
        if cursor == 1:
            run.report['resumed_from'] = {'report_sha256': sha(prior_bytes), 'verified_completed_stages': 23,
                'reason': 'Fixture-only whitespace correction with identical C# tokens. Reuse 23 verified unchanged boundary/restore passes; rerun continuation, formatting and every remaining product/runtime stage.'}
        if name == 'continuation':
            return original(run, name, args, cwd, env, revision, retry_native, completion)
        for attempt in row['attempts']:
            (run.directory / attempt['log']).write_bytes((ARCHIVE / attempt['log']).read_bytes())
        run.report['stages'].append(row)
        run.save()
        print('REUSE:', name, flush=True)

    quality.Run.stage = stage
    quality.main()


class GuardTests(unittest.TestCase):
    """Exercise recovery rejection against the real retained failure and disposable source copies."""
    def test_rejects_changed_tokens_source_logs_and_failure(self):
        prior = json.loads((ARCHIVE / 'summary.json').read_text())
        with tempfile.TemporaryDirectory() as value:
            root = Path(value)
            inputs = json.loads((ARCHIVE / 'source-inputs.json').read_text())['inputs']
            for name in set(prior['inputs']) | {r['path'] for r in inputs}:
                target = root / name
                target.parent.mkdir(parents=True, exist_ok=True)
                target.write_bytes((ROOT / name).read_bytes())
            self.assertEqual(len(validate(prior, root, ARCHIVE)), 24)
            fixture = root / FIXTURE
            baseline = fixture.read_bytes()
            fixture.write_bytes(baseline + b' changed_token')
            with self.assertRaises(ValueError): validate(prior, root, ARCHIVE)
            fixture.write_bytes(baseline)
            runner = root / 'quality.py'
            runner.write_bytes(runner.read_bytes() + b'\n# drift')
            with self.assertRaises(ValueError): validate(prior, root, ARCHIVE)
            runner.write_bytes((ROOT / 'quality.py').read_bytes())
            changed = copy.deepcopy(prior)
            changed['stages'][0]['attempts'][0]['sha256'] = '0' * 64
            with self.assertRaises(ValueError): validate(changed, root, ARCHIVE)
            changed = copy.deepcopy(prior)
            changed['failed_stage'] = 'product-4'
            with self.assertRaises(ValueError): validate(changed, root, ARCHIVE)


if __name__ == '__main__':
    if '--self-test' in sys.argv:
        unittest.main(argv=[sys.argv[0]], verbosity=2)
    else:
        main()
