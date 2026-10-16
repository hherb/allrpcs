#!/usr/bin/env python3
"""Resume normal full quality after a bound whitespace-only correction; reuse immutable boundaries."""
import copy
from contextlib import contextmanager
import hashlib
import importlib.util
import json
from pathlib import Path
import re
import subprocess
import sys
import unittest

ROOT = Path(__file__).resolve().parents[2]
ARCHIVE = Path(__file__).resolve().parent / 'quality/attempt1'
PREFIX_COUNT = 22
RECOVERY_REASON = ('Whitespace-only correction in three bound C# files. Reuse 21 unchanged '
                   'historical/art boundary passes with exact log, input and command guards. '
                   'Rerun continuation and every current product, smoke and runtime stage.')
FORMAT_FILES = ('game/HerbExaminationActionController.cs', 'game/HerbExaminationActionMainCheck.cs',
                'game/OllamaParticipationHttpHandler.cs')
TOKEN = re.compile(r'@"(?:[^"]|"")*"|"(?:[^"\\]|\\.)*"|\'(?:[^\'\\]|\\.)*\'|//[^\n]*|/\*[\s\S]*?\*/|\S')


def sha(data):
    """Hash exact retained bytes."""
    return hashlib.sha256(data).hexdigest()


def tokens(data):
    """Keep string/character/comment bytes while ignoring only whitespace outside them."""
    return TOKEN.findall(data.decode())


def inventory():
    """Include every tracked input and every new product file, excluding the recorded editor exception."""
    names = set(subprocess.check_output(['git', 'ls-files'], cwd=ROOT, text=True).splitlines())
    names.update(subprocess.check_output(['git', 'ls-files', '--others', '--exclude-standard', '--',
                                        'game', 'src', 'tests'], cwd=ROOT, text=True).splitlines())
    return {name for name in names if name != 'game/project.godot' and (ROOT / name).is_file()}


def validate(prior, inputs=None, formatting=None):
    """Reject all drift except the exact recorded formatting correction, without masking failures."""
    if prior.get('status') != 'failed' or prior.get('failed_stage') != 'product-3':
        raise ValueError('Expected the retained formatting failure')
    rows = prior['stages']
    if len(rows) != 25 or rows[-1]['arguments'] != [
            'sh', '-c', 'dotnet format Allrpcs.sln --no-restore --verify-no-changes']:
        raise ValueError('Stage inventory changed')
    for path, digest in prior['inputs'].items():
        if sha((ROOT / path).read_bytes()) != digest:
            raise ValueError('CI input changed: ' + path)
    if sha((ROOT / 'game/project.godot').read_bytes()) != \
            '36bfb7c7a54399cc8d27c02b73c7990766d7db44e7452655cb6a496866653fca':
        raise ValueError('Editor exception changed')
    inputs = inputs if inputs is not None else json.loads((ARCHIVE / 'source-inputs.json').read_text())['inputs']
    formatting = formatting if formatting is not None else json.loads((ARCHIVE / 'formatting.json').read_text())
    if set(formatting) != set(FORMAT_FILES) or inventory() != set(inputs):
        raise ValueError('Input inventory changed')
    for path, record in formatting.items():
        before = (ARCHIVE / Path(path).name).read_bytes()
        after = (ROOT / path).read_bytes()
        if record['before_sha256'] != inputs[path] or sha(before) != record['before_sha256'] \
                or sha(after) != record['after_sha256'] or tokens(before) != tokens(after):
            raise ValueError('Formatting changed tokens or bound bytes: ' + path)
    for path, digest in inputs.items():
        expected = formatting[path]['after_sha256'] if path in formatting else digest
        if sha((ROOT / path).read_bytes()) != expected:
            raise ValueError('Other source input changed: ' + path)
    for index, row in enumerate(rows):
        if not row['attempts']:
            raise ValueError('Missing attempt')
        for attempt in row['attempts']:
            name = attempt['log']
            if Path(name).name != name or sha((ARCHIVE / name).read_bytes()) != attempt['sha256']:
                raise ValueError('Retained log changed')
        last = row['attempts'][-1]
        if index < 24 and (last['exit_code'] != 0 or last['completion_seen'] is not True):
            raise ValueError('Unsuccessful prefix')
    failed = rows[-1]['attempts'][-1]
    lines = (ARCHIVE / failed['log']).read_text().splitlines()
    if failed['exit_code'] != 2 or not lines or any('error WHITESPACE:' not in line for line in lines):
        raise ValueError('Unexpected failure output')
    if rows[PREFIX_COUNT - 1]['name'] != 'current-art-imports' or rows[PREFIX_COUNT]['name'] != 'product-1':
        raise ValueError('Boundary/product split changed')
    return copy.deepcopy(rows[:PREFIX_COUNT])


def main():
    """Invoke the normal pipeline and rerun continuation plus every current product/runtime stage."""
    prior_bytes = (ARCHIVE / 'summary.json').read_bytes()
    prior = json.loads(prior_bytes)
    if (ROOT / 'artifacts/quality-current/summary.json').read_bytes() != prior_bytes:
        raise ValueError('Current failure differs from the retained receipt')
    prefix = validate(prior)
    spec = importlib.util.spec_from_file_location('allrpcs_quality', ROOT / 'quality.py')
    quality = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(quality)
    if prior['product_revision'] != quality.git('rev-parse', 'HEAD').decode().strip() or prior['python'] != sys.version:
        raise ValueError('Revision or interpreter changed')
    original_stage, original_snapshot = quality.Run.stage, quality.snapshot
    cursor = 0

    def stage(run, name, args, cwd=ROOT, env=None, revision=None, retry_native=False, completion=None):
        nonlocal cursor
        if cursor >= len(prefix):
            return original_stage(run, name, args, cwd, env, revision, retry_native, completion)
        row = prefix[cursor]
        cursor += 1
        old_env = {k: v for k, v in row['environment_overrides'].items() if k != 'GIT_INDEX_FILE'}
        new_env = {k: v for k, v in (env or {}).items() if k != 'GIT_INDEX_FILE'}
        if (name, args, revision, new_env) != (row['name'], row['arguments'], row['revision'], old_env) \
                or any(attempt['required_completion'] != completion for attempt in row['attempts']):
            raise ValueError('Reused command contract changed: ' + name)
        if cursor == 1:
            run.report['resumed_from'] = {
                'report_sha256': sha(prior_bytes), 'verified_completed_stages': PREFIX_COUNT - 1,
                'reason': RECOVERY_REASON}
        if name == 'continuation':
            return original_stage(run, name, args, cwd, env, revision, retry_native, completion)
        for attempt in row['attempts']:
            (run.directory / attempt['log']).write_bytes((ARCHIVE / attempt['log']).read_bytes())
        run.report['stages'].append(row)
        run.save()
        print('REUSE:', name, flush=True)

    @contextmanager
    def snapshot(revision, editor=False, repository=ROOT):
        # Only bound, signature-checked reused stage calls may use a placeholder snapshot.
        if cursor < len(prefix):
            if revision != prefix[cursor]['revision']:
                raise ValueError('Snapshot revision changed')
            yield ROOT, {}
        else:
            with original_snapshot(revision, editor, repository) as result:
                yield result

    quality.Run.stage, quality.snapshot = stage, snapshot
    quality.main()


class GuardTests(unittest.TestCase):
    """Refuse source, token, receipt, completion and failure drift before reuse."""
    def test_rejects_drift(self):
        prior = json.loads((ARCHIVE / 'summary.json').read_text())
        self.assertEqual(len(validate(prior)), PREFIX_COUNT)
        inputs = json.loads((ARCHIVE / 'source-inputs.json').read_text())['inputs']
        changed = dict(inputs)
        changed['AGENTS.md'] = '0' * 64
        with self.assertRaises(ValueError): validate(prior, inputs=changed)
        formatting = json.loads((ARCHIVE / 'formatting.json').read_text())
        formatting[FORMAT_FILES[0]]['after_sha256'] = '0' * 64
        with self.assertRaises(ValueError): validate(prior, formatting=formatting)
        for field in ('sha256', 'exit_code', 'completion_seen'):
            changed = copy.deepcopy(prior)
            changed['stages'][0]['attempts'][0][field] = 'changed'
            with self.assertRaises(ValueError): validate(changed)
        changed = copy.deepcopy(prior)
        changed['failed_stage'] = 'product-10'
        with self.assertRaises(ValueError): validate(changed)


if __name__ == '__main__':
    if '--self-test' in sys.argv:
        unittest.main(argv=[sys.argv[0]], verbosity=2)
    else:
        main()
