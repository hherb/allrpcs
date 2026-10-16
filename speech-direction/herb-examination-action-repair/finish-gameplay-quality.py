#!/usr/bin/env python3
"""Retain unchanged frozen boundaries after two exact scene/scaffold composition corrections."""
import copy
import importlib.util
import json
from pathlib import Path
import sys
import unittest

ROOT = Path(__file__).resolve().parents[2]
PACKAGE = Path(__file__).resolve().parent
ARCHIVE = PACKAGE / 'quality/attempt2'
spec = importlib.util.spec_from_file_location('format_recovery', PACKAGE / 'finish-quality.py')
base = importlib.util.module_from_spec(spec)
spec.loader.exec_module(base)
CORRECTIONS = {
    'game/Main.cs': ('        AddChild(examinationAction);',
                     '        AddChild(examinationAction);\n        MoveChild(examinationAction, GetChildCount() - 2);'),
    'game/LiveCharacterModelOrdinaryFlowSmokeCheck.cs': (
        '        GetParent<Main>().ConditionalCollectionScaffolding = OS.GetEnvironment(LiveOllamaCheckEnvironmentVariable) == EnabledEnvironmentValue\n'
        '                || OS.GetEnvironment(LiveOpenAiCheckEnvironmentVariable) == EnabledEnvironmentValue;',
        '        if (ollamaActionEnabled || OS.GetEnvironment(LiveOpenAiCheckEnvironmentVariable) == EnabledEnvironmentValue)\n'
        '        { GetParent<Main>().ConditionalCollectionScaffolding = true; }')
}


def validate(prior, inputs=None, corrections=None):
    """Reject drift beyond exact controller placement and inactive-fixture flag preservation."""
    if prior.get('status') != 'failed' or prior.get('failed_stage') != 'product-10':
        raise ValueError('Expected the exact retained scene-order failure')
    rows = prior['stages']
    if len(rows) != 35 or rows[-1]['arguments'] != ['sh', '-c', './scripts/smoke-godot.sh']:
        raise ValueError('Stage inventory changed')
    for path, digest in prior['inputs'].items():
        if base.sha((ROOT / path).read_bytes()) != digest:
            raise ValueError('CI input changed: ' + path)
    if base.sha((ROOT / 'game/project.godot').read_bytes()) != \
            '36bfb7c7a54399cc8d27c02b73c7990766d7db44e7452655cb6a496866653fca':
        raise ValueError('Editor exception changed')
    inputs = inputs if inputs is not None else json.loads((ARCHIVE / 'source-inputs.json').read_text())['inputs']
    corrections = corrections if corrections is not None else json.loads((ARCHIVE / 'corrections.json').read_text())
    if base.inventory() != set(inputs) or set(corrections) != set(CORRECTIONS):
        raise ValueError('Input inventory changed')
    for path, (old, new) in CORRECTIONS.items():
        record = corrections[path]
        before = (ARCHIVE / Path(path).name).read_bytes()
        after = (ROOT / path).read_bytes()
        if record['old'] != old or record['new'] != new or before.decode().count(old) != 1 \
                or before.decode().replace(old, new, 1).encode() != after \
                or base.sha(before) != inputs[path] or base.sha(before) != record['before_sha256'] \
                or base.sha(after) != record['after_sha256']:
            raise ValueError('Correction differs from the exact permitted change: ' + path)
    for path, digest in inputs.items():
        expected = corrections[path]['after_sha256'] if path in corrections else digest
        if base.sha((ROOT / path).read_bytes()) != expected:
            raise ValueError('Other source changed: ' + path)
    for index, row in enumerate(rows):
        if not row['attempts']:
            raise ValueError('Missing attempt')
        for attempt in row['attempts']:
            name = attempt['log']
            if Path(name).name != name or base.sha((ARCHIVE / name).read_bytes()) != attempt['sha256']:
                raise ValueError('Retained log changed')
        last = row['attempts'][-1]
        if index < 34 and (last['exit_code'] != 0 or last['completion_seen'] is not True):
            raise ValueError('Unsuccessful prefix')
    failed = rows[-1]['attempts'][-1]
    tail = (ARCHIVE / failed['log']).read_text().rsplit('Godot Engine v', 1)[-1]
    if failed['exit_code'] != 1 or [line for line in tail.splitlines() if line.startswith('ERROR:')] != [
            'ERROR: Channel owner must be final root child with exactly five sprites']:
        raise ValueError('Unexpected failure output')
    if rows[21]['name'] != 'current-art-imports' or rows[22]['name'] != 'product-1':
        raise ValueError('Boundary/product split changed')
    return copy.deepcopy(rows[:22])


class GuardTests(unittest.TestCase):
    """Verify that corrections, source inputs, failure identity and successful receipts are required."""
    def test_rejects_drift(self):
        prior = json.loads((ARCHIVE / 'summary.json').read_text())
        self.assertEqual(len(validate(prior)), 22)
        inputs = json.loads((ARCHIVE / 'source-inputs.json').read_text())['inputs']
        inputs['AGENTS.md'] = '0' * 64
        with self.assertRaises(ValueError): validate(prior, inputs=inputs)
        corrections = json.loads((ARCHIVE / 'corrections.json').read_text())
        corrections['game/Main.cs']['new'] += '\n'
        with self.assertRaises(ValueError): validate(prior, corrections=corrections)
        for field in ('sha256', 'exit_code', 'completion_seen'):
            changed = copy.deepcopy(prior)
            changed['stages'][0]['attempts'][0][field] = 'changed'
            with self.assertRaises(ValueError): validate(changed)
        changed = copy.deepcopy(prior)
        changed['failed_stage'] = 'product-3'
        with self.assertRaises(ValueError): validate(changed)


if __name__ == '__main__':
    if '--self-test' in sys.argv:
        unittest.main(argv=[sys.argv[0]], verbosity=2)
    else:
        base.ARCHIVE = ARCHIVE
        base.validate = validate
        base.RECOVERY_REASON = ('Correct new controller placement to preserve the final channel owner and '
                                'make the legacy live fixture enable scaffolding only when active. Reuse 21 '
                                'hash-verified unchanged historical/art boundary passes; rerun continuation '
                                'and every current product, smoke and runtime stage.')
        base.main()
