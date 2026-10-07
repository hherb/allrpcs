#!/usr/bin/env python3
"""Run the proof in an empty-cache disposable project and retain exact Godot evidence."""

import argparse
import hashlib
import json
from pathlib import Path
import re
import shutil
import subprocess
import tempfile

from build import GAME_ART, PACKAGE, REPO, require, sha, write_json

MARKER = 'PASS: 480 exact imported RGBA textures; canonical UIDs; 120 complete ordered animations and fixed pivots'
SCRIPTS = ['CharacterAnimationImportProof.tscn', 'CharacterAnimationImportProof.gd',
           'CharacterAnimationImportProofCheck.gd']


def run(attempt, capture, evidence_root=None):
    """Import exact live proof resources, run ordered playback, and check cold metadata parity."""
    package = REPO / PACKAGE
    evidence = (evidence_root.resolve() if evidence_root else package / 'evidence') / attempt
    frozen = (package / 'verification.json').exists()
    require(not evidence.exists(), 'Evidence is immutable')
    require(not frozen or not evidence.is_relative_to(package), 'Use --evidence-root outside the frozen package')
    evidence.mkdir(parents=True)
    report = {'status': 'failed', 'cold_cache': True, 'commands': [], 'inputs': {}}
    try:
        with tempfile.TemporaryDirectory(prefix='allrpcs-character-cold-') as temporary:
            project = Path(temporary) / 'game'
            project.mkdir()
            shutil.copytree(REPO / GAME_ART, project / GAME_ART.relative_to('game'))
            for name in SCRIPTS:
                shutil.copyfile(REPO / 'game' / name, project / name)
            published = subprocess.check_output(['git', 'show', 'HEAD:game/project.godot'], cwd=REPO)
            settings = published.replace(b'res://Main.tscn', b'res://CharacterAnimationImportProof.tscn')
            (project / 'project.godot').write_bytes(settings)
            report['published_project_sha256'] = hashlib.sha256(published).hexdigest()
            report['disposable_project_change'] = 'Only startup scene redirected to the isolated proof'
            bound_paths = sorted((REPO / GAME_ART).rglob('*')) + [REPO / 'game' / name for name in SCRIPTS]
            report['inputs'] = {str(path.relative_to(REPO)): sha(path) for path in bound_paths if path.is_file()}
            require(not (project / '.godot').exists(), 'Cold cache already exists')
            commands = [(['--headless', '--editor', '--quit'], 'import'),
                        (['--fixed-fps', '100', '--script', 'res://CharacterAnimationImportProofCheck.gd'], 'playback')]
            for arguments, label in commands:
                if label == 'playback' and not capture:
                    arguments = ['--headless', *arguments]
                command = ['godot', '--path', str(project), '--log-file', str(project / (label + '.log')), *arguments]
                if label == 'playback':
                    command += ['--', '--output=' + str(evidence)]
                    if capture:
                        command.append('--captures')
                result = subprocess.run(command, stdout=subprocess.PIPE, stderr=subprocess.STDOUT, timeout=180)
                log = result.stdout.decode(errors='replace')
                (evidence / (label + '.log')).write_text(log)
                report['commands'].append({'arguments': arguments, 'exit_code': result.returncode,
                                           'log': label + '.log'})
                require(result.returncode == 0 and not re.search(r'SCRIPT ERROR|Parse Error|ERROR:', log),
                        'Godot diagnostic failure; see ' + str(evidence / (label + '.log')))
                if label == 'playback':
                    require(MARKER in log, 'Godot omitted explicit pass marker')
            sidecars = list((REPO / GAME_ART).rglob('*.png.import'))
            for original in sidecars:
                copied = project / original.relative_to(REPO / 'game')
                require(sha(original) == sha(copied), 'Godot changed deterministic sidecar: ' + original.name)
            report['stable_sidecars'] = len(sidecars)
        for path, digest in report['inputs'].items():
            require(sha(REPO / path) == digest, 'Live proof changed during verification: ' + path)
        report['status'] = 'passed'
    finally:
        write_json(evidence / 'run.json', report)
    print('PASS: empty-cache proof import, 480 stable sidecars, exact RGBA and 120 real animation sequences')


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--attempt', required=True)
    parser.add_argument('--capture', action='store_true')
    parser.add_argument('--evidence-root', type=Path)
    args = parser.parse_args()
    require(re.fullmatch(r'[a-z0-9-]+', args.attempt), 'Unsafe attempt name')
    run(args.attempt, args.capture, args.evidence_root)
