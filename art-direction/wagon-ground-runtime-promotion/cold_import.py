#!/usr/bin/env python3
"""Check actual underlay decoding in a new disposable cache without proof UID authority."""

import hashlib
import json
from pathlib import Path
import re
import shutil
import subprocess
import tempfile

ROOT = Path(__file__).resolve().parent
REPO = ROOT.parents[1]


def sha(path):
    """Hash source/sidecar/check bytes for the retained import evidence."""
    return hashlib.sha256(path.read_bytes()).hexdigest()


def require(condition, message):
    """Fail cold-import validation without writing product resources."""
    if not condition:
        raise ValueError(message)


def main():
    """Import real copies from empty state and regenerate only the disposable proof UID."""
    require(not (ROOT / 'owner-runtime-acceptance.json').exists(), 'Accepted evidence is immutable')
    report = {'empty_cache_before_import': True, 'inputs': {}, 'commands': []}
    with tempfile.TemporaryDirectory(prefix='allrpcs-sickhouse-cold-') as temporary:
        project = Path(temporary)
        shutil.copy2(ROOT / 'fresh-project.godot', project / 'project.godot')
        shutil.copy2(ROOT / 'fresh-import-check.gd', project / 'check.gd')
        proof_name = 'game/art/direction-a-import-proof/terrain/wagon-entrance-ground.png'
        runtime_name = 'game/art/runtime/terrain/wagon-entrance-ground.png'
        for name in (proof_name, runtime_name):
            destination = project / name.removeprefix('game/')
            destination.parent.mkdir(parents=True, exist_ok=True)
            for suffix in ('', '.import'):
                source = REPO / (name + suffix)
                shutil.copy2(source, Path(str(destination) + suffix))
                report['inputs'][name + suffix] = sha(source)
            if name == proof_name:
                sidecar = Path(str(destination) + '.import')
                sidecar.write_text(re.sub(r'^uid=.*\n', '', sidecar.read_text(), flags=re.MULTILINE))
        require(not (project / '.godot').exists(), 'Cold project cache was populated')
        for arguments in (['--headless', '--editor', '--path', str(project), '--quit'],
                          ['--headless', '--path', str(project), '--script', str(project / 'check.gd')]):
            result = subprocess.run(['godot', *arguments], stdout=subprocess.PIPE, stderr=subprocess.STDOUT)
            report['commands'].append({'arguments': arguments, 'exit_code': result.returncode,
                                       'output': result.stdout.decode(errors='replace')})
            marker = "PASS: empty-cache actual runtime RGBA equals source; proof UID regenerated independently"
            if result.returncode or ('--script' in arguments and marker not in result.stdout.decode(errors='replace')):
                (ROOT / 'fresh-import-evidence.json').write_text(json.dumps(report, indent=2) + '\n')
                raise ValueError('Cold Godot command failed or omitted its explicit pass marker: ' + str(result.returncode))
        generated = (project / proof_name.removeprefix('game/')).with_suffix('.png.import')
        live = REPO / (proof_name + '.import')
        generated_uid = re.search(r'^uid="([^"]+)"', generated.read_text(), re.MULTILINE).group(1)
        live_uid = re.search(r'^uid="([^"]+)"', live.read_text(), re.MULTILINE).group(1)
        require(generated_uid != live_uid, 'Disposable proof UID failed to regenerate')
        require(sha(project / (runtime_name.removeprefix('game/') + '.import')) == sha(REPO / (runtime_name + '.import')),
                'Cold runtime sidecar changed')
        report.update(status='passed', original_proof_uid=live_uid, regenerated_proof_uid=generated_uid,
                      decoded_runtime_rgba='exact source pixels')
    for name, digest in report['inputs'].items():
        require(sha(REPO / name) == digest, 'Live cold-import input changed')
    report['inputs'].update({str(path.relative_to(REPO)): sha(path) for path in
                            (ROOT / 'fresh-project.godot', ROOT / 'fresh-import-check.gd', Path(__file__))})
    (ROOT / 'fresh-import-evidence.json').write_text(json.dumps(report, indent=2) + '\n')
    print('PASS: cold import, regenerated proof UID and exact actual-runtime RGBA')


if __name__ == '__main__':
    main()
