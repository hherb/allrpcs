#!/usr/bin/env python3
"""Decode all approved imports from an empty disposable cache and regenerate proof UIDs."""

import argparse
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
    """Bind exact cold-import sources and sidecars."""
    return hashlib.sha256(path.read_bytes()).hexdigest()


def require(condition, message):
    """Fail on missing markers, import drift or an already populated cache."""
    if not condition:
        raise ValueError(message)


def main():
    """Copy the real 32 texture imports; discard only the disposable proof UIDs."""
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--attempt', choices=('sandbox', 'host', 'review', 'final'), required=True)
    args = parser.parse_args()
    evidence = ROOT / ('cold-import-' + args.attempt + '.json')
    require(not evidence.exists() and not (ROOT / 'owner-runtime-acceptance.json').exists(), 'Evidence is immutable')
    proposal = REPO / 'art-direction/woodland-clearing-dressing-runtime-scope/proposal-inputs.json'
    assets = json.loads(proposal.read_text())['assets']
    report = dict(empty_cache_before_import=True, inputs={}, commands=[], status='failed')
    try:
        with tempfile.TemporaryDirectory(prefix='woodland-dressing-cold-') as temporary:
            project = Path(temporary)
            shutil.copy2(ROOT / 'cold-check.gd', project / 'check.gd')
            (project / 'project.godot').write_text('[application]\nconfig/name="Woodland dressing cold import"\n[rendering]\nrenderer/rendering_method="gl_compatibility"\n')
            (project / 'assets.json').write_text(json.dumps(assets))
            for asset in assets:
                for key in ('proof_texture', 'runtime_texture'):
                    name = asset[key]
                    destination = project / name.removeprefix('game/')
                    destination.parent.mkdir(parents=True, exist_ok=True)
                    for suffix in ('', '.import'):
                        source = REPO / (name + suffix)
                        shutil.copy2(source, Path(str(destination) + suffix))
                        report['inputs'][name + suffix] = sha(source)
                    if key == 'proof_texture':
                        sidecar = Path(str(destination) + '.import')
                        sidecar.write_text(re.sub(r'^uid=.*\n', '', sidecar.read_text(), flags=re.MULTILINE))
            require(not (project / '.godot').exists(), 'Cold cache already populated')
            for arguments in (['--headless', '--editor', '--path', str(project), '--quit'],
                              ['--headless', '--path', str(project), '--script', str(project / 'check.gd')]):
                result = subprocess.run(['godot', *arguments], stdout=subprocess.PIPE, stderr=subprocess.STDOUT)
                output = result.stdout.decode(errors='replace')
                report['commands'].append(dict(arguments=arguments, exit_code=result.returncode, output=output))
                require(result.returncode == 0 and not re.search(r'(?:SCRIPT ERROR|Parse Error|ERROR:)', output), 'Cold Godot diagnostic failure')
                if '--script' in arguments:
                    require('PASS: 32 exact cold RGBA imports; 16 independent proof UIDs' in output, 'Cold check omitted pass marker')
            uids = set()
            for asset in assets:
                for key in ('proof_texture', 'runtime_texture'):
                    name = asset[key] + '.import'
                    generated = project / name.removeprefix('game/')
                    uid = re.search(r'^uid="([^"]+)"', generated.read_text(), re.MULTILINE).group(1)
                    original = re.search(r'^uid="([^"]+)"', (REPO / name).read_text(), re.MULTILINE).group(1)
                    require(uid not in uids, 'Duplicate cold import UID')
                    uids.add(uid)
                    if key == 'runtime_texture':
                        require(sha(generated) == sha(REPO / name), 'Cold runtime sidecar drift')
                    else:
                        require(uid != original, 'Disposable proof UID did not regenerate')
            report.update(decoded_rgba='32 exact source matches', regenerated_proof_uids=16)
        for name, digest in report['inputs'].items():
            require(sha(REPO / name) == digest, 'Live import changed during cold check')
        report['inputs'].update({str(path.relative_to(REPO)): sha(path) for path in (Path(__file__), ROOT / 'cold-check.gd', proposal)})
        report['status'] = 'passed'
    finally:
        evidence.write_text(json.dumps(report, indent=2) + '\n')
    print('PASS: empty-cache 32 exact RGBA imports; 16 regenerated proof UIDs; live resources unchanged')


if __name__ == '__main__':
    main()
