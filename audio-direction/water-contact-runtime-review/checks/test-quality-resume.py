#!/usr/bin/env python3
"""Exercise retained resume bookkeeping against a real completed current-quality inventory."""
import ast
import copy
import hashlib
import importlib.util
import json
from pathlib import Path
import shutil
import tempfile

ROOT = Path(__file__).resolve().parents[3]
REVIEW = ROOT / 'audio-direction/water-contact-runtime-review'


def main():
    """Reuse actual successful stage rows and verify the boundary after product nine."""
    spec = importlib.util.spec_from_file_location('water_quality', ROOT / 'quality.py')
    quality = importlib.util.module_from_spec(spec); spec.loader.exec_module(quality)
    report = json.loads((ROOT / 'artifacts/quality-current/summary.json').read_text())
    if report['status'] != 'passed':
        raise ValueError('Requires a completed real current-quality run')
    art, product = quality.commands((ROOT / 'scripts/quality.sh').read_text())
    # A native-smoke resume inserts a fresh boundary after the nine already completed product commands.
    completed = copy.deepcopy(report['stages'][:37])
    if completed[-1]['name'] != 'product-19':
        raise ValueError('Unexpected real completed inventory')
    boundary = copy.deepcopy(next(row for row in completed if row['name'] == 'boundary-water-contact-current'))
    boundary['name'] = 'boundary-water-contact-resume'
    completed.insert(next(index for index, row in enumerate(completed) if row['name'] == 'product-10'), boundary)
    prior = copy.deepcopy(report); prior.update(status='failed', failed_stage='interior-normal', resumed_from={'fixture': True})
    with tempfile.TemporaryDirectory() as directory:
        directory = Path(directory)
        for stage in completed:
            for attempt in stage['attempts']:
                shutil.copyfile(ROOT / 'artifacts/quality-current' / attempt['log'], directory / attempt['log'])
        failed = b'Sickhouse profile not live\n'; (directory / 'failed-interior.log').write_bytes(failed)
        prior['stages'] = completed + [{'name': 'interior-normal',
            'arguments': ['godot', '--headless', '--path', 'game', '--quit-after', '1200'],
            'revision': None, 'environment_overrides': {}, 'attempts': [{'exit_code': 1,
            'log': 'failed-interior.log', 'sha256': quality.sha(failed)}]}]
        reused = quality.resume_prefix(prior, directory, art, product, report, finish_interior=True)
        if reused != completed:
            raise ValueError('Changed or omitted completed commands during valid resume')
        mutated = copy.deepcopy(prior); mutated['stages'][0]['arguments'] = ['omitted-check']
        try:
            quality.resume_prefix(mutated, directory, art, product, report, finish_interior=True)
        except ValueError:
            pass
        else:
            raise ValueError('Changed command accepted')
    executed = (REVIEW / 'quality/executed-runner.py').read_bytes()
    if hashlib.sha256(executed).hexdigest() != report['inputs']['quality.py']:
        raise ValueError('Executed runner source differs from real report')
    def normal_path(data):
        tree = ast.parse(data)
        tree.body = [node for node in tree.body if not isinstance(node, ast.FunctionDef) or node.name != 'resume_prefix']
        return ast.dump(tree, include_attributes=False)
    if normal_path(executed) != normal_path((ROOT / 'quality.py').read_bytes()):
        raise ValueError('Ordinary full-quality code changed; cannot reuse completed run')
    record = {'schema_version': 1, 'status': 'passed', 'real_completed_stages': len(completed),
              'resume_position': 'after product-9, before product-10', 'changed_command_rejected': True,
              'ordinary_pipeline_ast_unchanged': True, 'executed_runner_sha256': report['inputs']['quality.py'],
              'current_runner_sha256': quality.sha((ROOT / 'quality.py').read_bytes()),
              'qualification': 'Focused resume-inventory repair only; all ordinary pipeline code, current product and commands are unchanged from the real completed run.'}
    (REVIEW / 'quality/resume-verification.json').write_text(json.dumps(record, indent=2) + '\n')
    print('PASS: real 38-stage resumed inventory, changed-command rejection and byte-structure-equivalent ordinary quality pipeline')


if __name__ == '__main__':
    main()
