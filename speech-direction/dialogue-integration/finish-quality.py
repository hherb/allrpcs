#!/usr/bin/env python3
"""Finish only the hash-bound two-mode speech tail after its recorded supplier-flag collision."""
import argparse
import copy
import hashlib
import importlib.util
import json
from pathlib import Path
import sys
import tempfile
import unittest

ROOT = Path(__file__).resolve().parents[2]
REVIEW = ROOT / 'speech-direction/dialogue-integration/quality/attempt2'
OLD = "env=dict(environment, ALLRPCS_DIALOGUE_SPEECH_CHECK='1', ALLRPCS_DIALOGUE_SPEECH_MODE=mode,"
NEW = "env=dict(environment, ALLRPCS_ORDINARY_PARTICIPATION_SMOKE_MODEL='0', ALLRPCS_DIALOGUE_SPEECH_CHECK='1', ALLRPCS_DIALOGUE_SPEECH_MODE=mode,"


def digest(data):
    """Bind exact bytes, including raw stage logs."""
    return hashlib.sha256(data).hexdigest()


def validate_prefix(prior, archived, directory, baseline, current, inputs, root):
    """Reject any failure, runner, product or log drift outside the single speech-tail flag repair."""
    if prior != archived or prior.get('status') != 'failed' or prior.get('failed_stage') != 'dialogue-speech-headless':
        raise ValueError('Requires the exact retained dialogue-speech failure')
    if digest(baseline.encode()) != prior['inputs']['quality.py'] or baseline.count(OLD) != 1 or current != baseline.replace(OLD, NEW):
        raise ValueError('Runner change must be exactly the speech-tail supplier flag')
    for row in inputs:
        path = Path(row['path'])
        if path.is_absolute() or '..' in path.parts or not (root / path).resolve().is_relative_to(root.resolve()):
            raise ValueError('Unsafe product input')
        if digest((root / path).read_bytes()) != row['sha256']:
            raise ValueError('Completed product input changed: ' + str(path))
    for name, value in prior['inputs'].items():
        if name != 'quality.py' and digest((root / name).read_bytes()) != value:
            raise ValueError('Completed CI input changed: ' + name)
    rows = prior['stages']
    if len(rows) != 199 or rows[-1]['name'] != 'dialogue-speech-headless':
        raise ValueError('Recorded stage inventory changed')
    for row in rows:
        for attempt in row['attempts']:
            name = attempt['log']
            if Path(name).name != name or digest((directory / name).read_bytes()) != attempt['sha256']:
                raise ValueError('Stage log changed: ' + name)
        if row is not rows[-1] and (row['attempts'][-1]['exit_code'] != 0 or not row['attempts'][-1]['completion_seen']):
            raise ValueError('Completed prefix is incomplete')
    failure = rows[-1]['attempts'][-1]
    if (failure['exit_code'] != 1 or rows[-1]['environment_overrides'].get('ALLRPCS_ORDINARY_PARTICIPATION_SMOKE_MODEL') != '1'
            or 'FAIL: dialogue speech Main: Participation fixture not available' not in (directory / failure['log']).read_text()):
        raise ValueError('Failure is not the recorded shared supplier collision')
    return rows[:-1]


def finish():
    """Retain the complete unchanged prefix and execute both corrected speech modes through Run.stage."""
    directory = ROOT / 'artifacts/quality-current'
    prior_bytes = (directory / 'summary.json').read_bytes()
    prior = json.loads(prior_bytes)
    archived = json.loads((REVIEW / 'summary.json').read_text())
    reused = validate_prefix(prior, archived, directory, (REVIEW / 'quality.py').read_text(),
                             (ROOT / 'quality.py').read_text(), json.loads((REVIEW / 'product-inputs.json').read_text()), ROOT)
    spec = importlib.util.spec_from_file_location('allrpcs_quality', ROOT / 'quality.py')
    quality = importlib.util.module_from_spec(spec); spec.loader.exec_module(quality)
    if prior['product_revision'] != quality.git('rev-parse', 'HEAD').decode().strip() or prior['python'] != sys.version:
        raise ValueError('Revision or interpreter changed')
    import PIL
    import numpy
    from PIL import features
    if (prior['pillow'], prior['numpy'], prior['zlib']) != (PIL.__version__, numpy.__version__, features.version('zlib')):
        raise ValueError('Pinned quality dependencies changed')
    quality.frozen_evidence()
    run = quality.Run(directory)
    metadata = {key: value for key, value in prior.items() if key not in
                ('status', 'stages', 'started', 'finished', 'failed_stage', 'resumed_from')}
    metadata['inputs'] = dict(prior['inputs'], **{'quality.py': digest((ROOT / 'quality.py').read_bytes())})
    run.report.update(metadata, stages=reused, resumed_from={'report_sha256': digest(prior_bytes),
        'prior_source_inputs': prior['inputs'], 'verified_completed_stages': len(reused),
        'reason': 'Exact completed prefix retained after shared participation supplier collision. Product hashes/logs unchanged; the sole runner diff disables that supplier for the two new speech modes.'})
    run.save()
    print(f'REUSE: {len(reused)} verified unchanged stages; rerun both speech modes', flush=True)
    try:
        quality.dialogue_speech_runtime(run, {'ALLRPCS_CHARACTER_MODEL_COMPOSITION_DISABLED': '1',
                                             'ALLRPCS_ORDINARY_PARTICIPATION_SMOKE_MODEL': '1'})
        saves = [(directory / f'interior-{mode}.json.save.json').read_bytes()
                 for mode in ('normal', 'muted', 'zero', 'mono-reduced')]
        if any(data != saves[0] for data in saves):
            raise ValueError('Retained interior parity changed')
        run.report.update(status='passed', finished=quality.now(), interior_save_sha256=digest(saves[0]),
                          transcript=json.loads((directory / 'interior-normal.json').read_text())['transcript'])
        run.save(); print('PASS: full current quality; 198 verified unchanged stages plus both repaired speech modes', flush=True)
    except Exception:
        run.report.update(status='failed', finished=quality.now()); run.save(); raise


class FinishTests(unittest.TestCase):
    """Ensure passing-prefix reuse cannot hide changed source, logs or unsuccessful checks."""
    def test_guards(self):
        with tempfile.TemporaryDirectory() as value:
            root = Path(value); baseline = OLD + '\n'; (root / 'source.cs').write_text('exact source')
            rows=[]
            for i in range(199):
                data=b'passed' if i < 198 else b'FAIL: dialogue speech Main: Participation fixture not available'
                name=f'{i}.log'; (root/name).write_bytes(data)
                rows.append({'name':f'prefix-{i}' if i < 198 else 'dialogue-speech-headless',
                    'environment_overrides':{'ALLRPCS_ORDINARY_PARTICIPATION_SMOKE_MODEL':'1'},
                    'attempts':[{'exit_code':0 if i < 198 else 1,'completion_seen':i < 198,'log':name,'sha256':digest(data)}]})
            prior={'status':'failed','failed_stage':'dialogue-speech-headless','inputs':{'quality.py':digest(baseline.encode())},'stages':rows}
            inputs=[{'path':'source.cs','sha256':digest((root/'source.cs').read_bytes())}]
            def check(candidate=prior, current=baseline.replace(OLD,NEW)):
                return validate_prefix(candidate,candidate,root,baseline,current,inputs,root)
            self.assertEqual(len(check()),198)
            with self.assertRaises(ValueError):check(current=baseline.replace(OLD,NEW)+'unexpected change')
            bad=copy.deepcopy(prior);bad['stages'][0]['attempts'][0]['completion_seen']=False
            with self.assertRaises(ValueError):check(candidate=bad)
            (root/'source.cs').write_text('changed source')
            with self.assertRaises(ValueError):check()
            (root/'source.cs').write_text('exact source');(root/'0.log').write_bytes(b'changed log')
            with self.assertRaises(ValueError):check()
            with self.assertRaises(ValueError):validate_prefix(prior,{},root,baseline,baseline.replace(OLD,NEW),inputs,root)


if __name__ == '__main__':
    parser=argparse.ArgumentParser(description=__doc__);parser.add_argument('--self-test',action='store_true');args=parser.parse_args()
    if args.self_test:unittest.main(argv=[sys.argv[0]])
    else:finish()
