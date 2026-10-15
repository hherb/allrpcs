#!/usr/bin/env python3
"""Resume after the isolated unchanged sage assertion; rerun the complete failed smoke stage."""
import copy
import hashlib
import importlib.util
import json
from pathlib import Path
import sys
import unittest

ROOT = Path(__file__).resolve().parents[2]
ARCHIVE = Path(__file__).resolve().parent / 'quality/attempt2'
MARKER = 'Ordinary sage interjection smoke passed:'


def sha(data):
    """Bind exact source and log bytes."""
    return hashlib.sha256(data).hexdigest()


def validate(prior, root=ROOT, archive=ARCHIVE, inputs=None):
    """Retain only completed stages with unchanged inputs and the exact diagnosed failure."""
    if prior.get('status') != 'failed' or prior.get('failed_stage') != 'product-10':
        raise ValueError('Expected the retained full-smoke failure')
    rows = prior['stages']
    if len(rows) != 35 or rows[-1]['arguments'] != ['sh', '-c', './scripts/smoke-godot.sh']:
        raise ValueError('Unexpected stage inventory or failed command')
    for path, digest in prior['inputs'].items():
        if sha((root / path).read_bytes()) != digest: raise ValueError('CI input changed: ' + path)
    source_rows = inputs if inputs is not None else json.loads((archive / 'source-inputs.json').read_text())['inputs']
    for row in source_rows:
        path = Path(row['path'])
        if path.is_absolute() or '..' in path.parts or sha((root / path).read_bytes()) != row['sha256']:
            raise ValueError('Product source changed: ' + str(path))
    for index, row in enumerate(rows):
        if not row['attempts']: raise ValueError('Missing attempt')
        for attempt in row['attempts']:
            name = attempt['log']
            if Path(name).name != name or sha((archive / name).read_bytes()) != attempt['sha256']:
                raise ValueError('Changed stage log')
        last = row['attempts'][-1]
        if index < 34 and (last['exit_code'] != 0 or not last['completion_seen']):
            raise ValueError('Unsuccessful prefix stage')
    failed = rows[-1]['attempts'][-1]
    tail = (archive / failed['log']).read_text().split('Godot Engine v')[-1]
    if failed['exit_code'] != 1 or [s for s in tail.splitlines() if s.startswith('ERROR:')] != [
            'ERROR: The ordinary scene did not complete the exact bounded sage interjection.']:
        raise ValueError('Unexpected runtime failure')
    for index in range(1, 21):
        log = archive / 'diagnosis' / f'baseline-{index}.log'
        data = log.read_text()
        if MARKER not in data or 'ERROR:' in data: raise ValueError('Baseline isolation did not pass')
    return copy.deepcopy(rows[:-1])


def main():
    """Use the unchanged normal resume pipeline after independent source/log validation."""
    prior_bytes = (ARCHIVE / 'summary.json').read_bytes()
    prior = json.loads(prior_bytes)
    current = ROOT / 'artifacts/quality-current/summary.json'
    if current.read_bytes() != prior_bytes: raise ValueError('Current report differs from retained failure')
    prefix = validate(prior)
    spec = importlib.util.spec_from_file_location('allrpcs_quality', ROOT / 'quality.py')
    quality = importlib.util.module_from_spec(spec); spec.loader.exec_module(quality)
    original_save = quality.Run.save

    def resume(previous, directory, art, product, metadata, finish_interior=False):
        if finish_interior or previous != prior: raise ValueError('Unexpected recovery request')
        for key in ('product_revision', 'python', 'pillow', 'numpy', 'zlib', 'inputs'):
            if previous[key] != metadata[key]: raise ValueError('Resume environment changed: ' + key)
        return copy.deepcopy(prefix)

    def save(run):
        if 'resumed_from' in run.report:
            run.report['resumed_from']['reason'] = (
                'Unchanged supplied-model sage assertion failed once; isolated identical run and 20 diagnostic-only '
                'baseline runs passed. Preserve the failure; reuse 34 hash-verified completed stages and rerun '
                'the entire failed smoke script plus every remaining normal gate stage without changing assertions.')
            run.report['resumed_from']['prior_recovery'] = prior.get('resumed_from')
        return original_save(run)

    quality.resume_prefix = resume
    quality.Run.save = save
    quality.main(resume_smoke=True)


class GuardTests(unittest.TestCase):
    """Ensure recovery rejects changed sources, logs and unsuccessful prefix evidence."""
    def test_rejects_drift(self):
        prior = json.loads((ARCHIVE / 'summary.json').read_text())
        self.assertEqual(len(validate(prior)), 34)
        inputs = json.loads((ARCHIVE / 'source-inputs.json').read_text())['inputs']
        inputs[0]['sha256'] = '0' * 64
        with self.assertRaises(ValueError): validate(prior, inputs=inputs)
        changed = copy.deepcopy(prior); changed['stages'][0]['attempts'][0]['sha256'] = '0' * 64
        with self.assertRaises(ValueError): validate(changed)
        changed = copy.deepcopy(prior); changed['stages'][1]['attempts'][0]['exit_code'] = 1
        with self.assertRaises(ValueError): validate(changed)


if __name__ == '__main__':
    if '--self-test' in sys.argv: unittest.main(argv=[sys.argv[0]], verbosity=2)
    else: main()
