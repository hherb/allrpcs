#!/usr/bin/env python3
"""Retain unchanged boundary receipts after the recorded cancellation-fixture setup correction."""
import copy
from contextlib import contextmanager
import hashlib
import importlib.util
import json
from pathlib import Path
import sys
import unittest

ROOT = Path(__file__).resolve().parents[2]
ARCHIVE = Path(__file__).resolve().parent / 'quality/attempt1'
FIXTURE = 'game/CharacterModelGenerationFlowSmokeCheck.cs'
PREFIX_COUNT = 22


def sha(data):
    """Bind exact bytes independently of timestamps."""
    return hashlib.sha256(data).hexdigest()


def validate(prior, inputs=None, adaptation=None):
    """Reject changed inputs, unsuccessful receipts or any other recorded failure."""
    if prior.get('status') != 'failed' or prior.get('failed_stage') != 'product-10':
        raise ValueError('Expected the retained smoke failure')
    rows = prior['stages']
    if len(rows) != 35 or rows[-1]['arguments'] != ['sh', '-c', './scripts/smoke-godot.sh']:
        raise ValueError('Failed stage inventory changed')
    for path, digest in prior['inputs'].items():
        if sha((ROOT / path).read_bytes()) != digest:
            raise ValueError('CI input changed: ' + path)
    inputs = inputs if inputs is not None else json.loads((ARCHIVE / 'source-inputs.json').read_text())['inputs']
    adaptation = adaptation if adaptation is not None else json.loads((ARCHIVE / 'fixture-adaptation.json').read_text())
    if adaptation['path'] != FIXTURE or adaptation['before_sha256'] != inputs[FIXTURE]:
        raise ValueError('Fixture baseline receipt changed')
    if sha((ARCHIVE / Path(FIXTURE).name).read_bytes()) != adaptation['before_sha256']:
        raise ValueError('Retained fixture baseline changed')
    for path, digest in inputs.items():
        if Path(path).is_absolute() or '..' in Path(path).parts:
            raise ValueError('Unsafe source input')
        expected = adaptation['after_sha256'] if path == FIXTURE else digest
        if sha((ROOT / path).read_bytes()) != expected:
            raise ValueError('Source input changed outside the exact fixture correction: ' + path)
    for index, row in enumerate(rows):
        if not row['attempts']:
            raise ValueError('Missing attempt')
        for attempt in row['attempts']:
            name = attempt['log']
            if Path(name).name != name or sha((ARCHIVE / name).read_bytes()) != attempt['sha256']:
                raise ValueError('Retained log changed')
        last = row['attempts'][-1]
        if index < 34 and (last['exit_code'] != 0 or not last['completion_seen']):
            raise ValueError('Unsuccessful prefix stage')
    failed = rows[-1]['attempts'][-1]
    tail = (ARCHIVE / failed['log']).read_text().split('Godot Engine v')[-1]
    if failed['exit_code'] != 1 or [line for line in tail.splitlines() if line.startswith('ERROR:')] != [
            'ERROR: Character-turn input did not cancel the exact active model-backed turn.']:
        raise ValueError('Unexpected recorded smoke failure')
    if rows[PREFIX_COUNT - 1]['name'] != 'current-art-imports' or rows[PREFIX_COUNT]['name'] != 'product-1':
        raise ValueError('Boundary/product split changed')
    return copy.deepcopy(rows[:PREFIX_COUNT])


def main():
    """Run the normal complete pipeline; reuse only exact unchanged boundary commands."""
    prior_bytes = (ARCHIVE / 'summary.json').read_bytes()
    prior = json.loads(prior_bytes)
    if (ROOT / 'artifacts/quality-current/summary.json').read_bytes() != prior_bytes:
        raise ValueError('Current failed receipt differs from its archive')
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
        old_env = {key: value for key, value in row['environment_overrides'].items() if key != 'GIT_INDEX_FILE'}
        new_env = {key: value for key, value in (env or {}).items() if key != 'GIT_INDEX_FILE'}
        if (name, args, revision, new_env) != (row['name'], row['arguments'], row['revision'], old_env):
            raise ValueError('Reused command signature changed: ' + name)
        if any(attempt['required_completion'] != completion for attempt in row['attempts']):
            raise ValueError('Reused completion contract changed')
        if cursor == 1:
            run.report['resumed_from'] = {
                'report_sha256': sha(prior_bytes), 'verified_completed_stages': PREFIX_COUNT - 1,
                'reason': 'Correct the cancellation fixture to hold an unrecorded spoken response; preserve all assertions. Reuse 21 hash-verified unchanged historical/art boundary passes. Rerun continuation and every product, smoke and remaining runtime stage.'}
        if name == 'continuation':
            return original_stage(run, name, args, cwd, env, revision, retry_native, completion)
        for attempt in row['attempts']:
            (run.directory / attempt['log']).write_bytes((ARCHIVE / attempt['log']).read_bytes())
        run.report['stages'].append(row)
        run.save()
        print('REUSE:', name, flush=True)

    @contextmanager
    def snapshot(revision, editor=False, repository=ROOT):
        # Reused snapshots contain only signature-checked stage calls in this bound quality.py.
        # Never launch a command in this placeholder; stage() returns retained receipts instead.
        if cursor < len(prefix):
            expected = prefix[cursor]['revision']
            if revision != expected:
                raise ValueError('Reused snapshot revision changed')
            yield ROOT, {}
        else:
            with original_snapshot(revision, editor, repository) as result:
                yield result

    quality.Run.stage, quality.snapshot = stage, snapshot
    quality.main()


class GuardTests(unittest.TestCase):
    """Verify that recovery refuses source, failure, receipt and allowed-fixture drift."""
    def test_rejects_drift(self):
        prior = json.loads((ARCHIVE / 'summary.json').read_text())
        self.assertEqual(len(validate(prior)), PREFIX_COUNT)
        inputs = json.loads((ARCHIVE / 'source-inputs.json').read_text())['inputs']
        changed_inputs = dict(inputs)
        path = next(path for path in inputs if path != FIXTURE)
        changed_inputs[path] = '0' * 64
        with self.assertRaises(ValueError): validate(prior, inputs=changed_inputs)
        adaptation = json.loads((ARCHIVE / 'fixture-adaptation.json').read_text())
        adaptation['after_sha256'] = '0' * 64
        with self.assertRaises(ValueError): validate(prior, adaptation=adaptation)
        changed = copy.deepcopy(prior)
        changed['stages'][0]['attempts'][0]['sha256'] = '0' * 64
        with self.assertRaises(ValueError): validate(changed)
        changed = copy.deepcopy(prior)
        changed['stages'][1]['attempts'][0]['exit_code'] = 1
        with self.assertRaises(ValueError): validate(changed)
        changed = copy.deepcopy(prior)
        changed['failed_stage'] = 'product-9'
        with self.assertRaises(ValueError): validate(changed)


if __name__ == '__main__':
    if '--self-test' in sys.argv:
        unittest.main(argv=[sys.argv[0]], verbosity=2)
    else:
        main()
