#!/usr/bin/env python3
"""Resume after a bound controller API/fixture correction; reuse unchanged frozen evidence."""
import copy
import importlib.util
import json
from pathlib import Path
import sys
import unittest

ROOT = Path(__file__).resolve().parents[2]
PACKAGE = Path(__file__).resolve().parent
ARCHIVE = PACKAGE / 'quality/attempt1'
spec = importlib.util.spec_from_file_location('prior_recovery', ROOT / 'speech-direction/herb-examination-action-repair/finish-quality.py')
base = importlib.util.module_from_spec(spec)
spec.loader.exec_module(base)
CORRECTED = {'game/DialogueEventController.cs', 'game/DialogueEventControllerSmokeCheck.cs'}


def validate(prior, inputs=None, corrections=None):
    """Require the exact failure, corrections, full input inventory and successful prefix receipts."""
    if prior.get('status') != 'failed' or prior.get('failed_stage') != 'product-10':
        raise ValueError('Expected the retained controller-surface failure')
    rows = prior['stages']
    if len(rows) != 35 or rows[-1]['arguments'] != ['sh', '-c', './scripts/smoke-godot.sh']:
        raise ValueError('Stage inventory changed')
    for path, digest in prior['inputs'].items():
        if base.sha((ROOT / path).read_bytes()) != digest:
            raise ValueError('CI input changed: ' + path)
    if base.sha((ROOT / 'game/project.godot').read_bytes()) != '36bfb7c7a54399cc8d27c02b73c7990766d7db44e7452655cb6a496866653fca':
        raise ValueError('Editor exception changed')
    inputs = inputs if inputs is not None else json.loads((ARCHIVE / 'source-inputs.json').read_text())['inputs']
    corrections = corrections if corrections is not None else json.loads((ARCHIVE / 'corrections.json').read_text())
    if base.inventory() != set(inputs) or set(corrections) != CORRECTED:
        raise ValueError('Input inventory changed')
    for path in CORRECTED:
        record = corrections[path]
        before = (ARCHIVE / Path(path).name).read_bytes()
        after = (ROOT / path).read_bytes()
        if before.decode().count(record['old']) != 1 or before.decode().replace(record['old'], record['new'], 1).encode() != after \
                or base.sha(before) != inputs[path] or base.sha(before) != record['before_sha256'] \
                or base.sha(after) != record['after_sha256']:
            raise ValueError('Correction differs: ' + path)
    for path, digest in inputs.items():
        expected = corrections[path]['after_sha256'] if path in CORRECTED else digest
        if base.sha((ROOT / path).read_bytes()) != expected:
            raise ValueError('Other input changed: ' + path)
    for index, row in enumerate(rows):
        if not row['attempts']:
            raise ValueError('Missing attempt')
        for attempt in row['attempts']:
            name = attempt['log']
            if Path(name).name != name or base.sha((ARCHIVE / name).read_bytes()) != attempt['sha256']:
                raise ValueError('Retained log changed')
        if index < 34 and (row['attempts'][-1]['exit_code'] != 0 or row['attempts'][-1]['completion_seen'] is not True):
            raise ValueError('Unsuccessful prefix')
    failed = rows[-1]['attempts'][-1]
    tail = (ARCHIVE / failed['log']).read_text().rsplit('Godot Engine v', 1)[-1]
    if failed['exit_code'] != 1 or [line for line in tail.splitlines() if line.startswith('ERROR:')] != [
            'ERROR: Dialogue event controller exposed an unexpected public surface.']:
        raise ValueError('Unexpected failure output')
    if rows[21]['name'] != 'current-art-imports' or rows[22]['name'] != 'product-1':
        raise ValueError('Boundary/product split changed')
    return copy.deepcopy(rows[:22])


class GuardTests(unittest.TestCase):
    """Prove source, correction and receipt drift cannot pass the recovery guard."""
    def test_rejects_drift(self):
        prior = json.loads((ARCHIVE / 'summary.json').read_text())
        self.assertEqual(len(validate(prior)), 22)
        inputs = json.loads((ARCHIVE / 'source-inputs.json').read_text())['inputs']
        inputs['AGENTS.md'] = '0' * 64
        with self.assertRaises(ValueError): validate(prior, inputs=inputs)
        corrections = json.loads((ARCHIVE / 'corrections.json').read_text())
        corrections['game/DialogueEventController.cs']['new'] += '\n'
        with self.assertRaises(ValueError): validate(prior, corrections=corrections)
        for field in ('sha256', 'exit_code', 'completion_seen'):
            changed = copy.deepcopy(prior)
            changed['stages'][0]['attempts'][0][field] = 'changed'
            with self.assertRaises(ValueError): validate(changed)
        changed = copy.deepcopy(prior)
        changed['failed_stage'] = 'other'
        with self.assertRaises(ValueError): validate(changed)


if __name__ == '__main__':
    if '--self-test' in sys.argv:
        unittest.main(argv=[sys.argv[0]], verbosity=2)
    else:
        base.ARCHIVE = ARCHIVE
        base.validate = validate
        base.RECOVERY_REASON = ('Keep the new history accessor internal and allow Godot lifecycle callbacks in the current controller API fixture. Reuse 21 hash-verified unchanged historical/art boundaries; rerun continuation and every current product, smoke and runtime stage.')
        base.main()
