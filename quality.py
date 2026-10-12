#!/usr/bin/env python3
"""Run frozen contracts at recorded revisions and full current product quality."""

import argparse
from contextlib import contextmanager
from datetime import datetime, timezone
import hashlib
import json
import os
from pathlib import Path
import re
import shlex
import subprocess
import sys
import tarfile
import tempfile
import unittest

ROOT = Path(__file__).resolve().parent
ART_REVISION = 'ea63fcc46834e84ffab98aabd76530e60f760955'
AUDIO_REVISIONS = (
    ('buses', '1b21d223d8329ead39973cfb81a290188780e84e', 'scripts/validate-audio-publication.py'),
    ('material', '240f18000a6825e6a46fea27135af2072b82f911', 'audio-direction/material-runtime-review/publication/validate-publication.py'),
    ('footstep', 'f5ae96894ba33cecddd8b7d8c4166a02c088aba5', 'scripts/validate-footstep-audio-runtime.py'),
    ('positional', 'b9b36889c35cb3c7773d8ac3cee7ee650d85af25', 'scripts/validate-positional-audio-runtime.py'),
    ('music', 'c472c0a31548e076bfee0252e4e4970714c8c019', 'scripts/validate-music-audio-runtime.py'),
    ('interior', '7b8dd659d409974126790b8a417533bded9c2e5e', 'scripts/validate-interior-audio-runtime.py'),
    ('water', '50f4a7516f7b5acb20119212985e2132a581c106', 'scripts/validate-water-contact-runtime.py'),
    ('cask', '5495202faecb257148309ad028faa3e0d95bf51c', 'scripts/validate-cask-cover-runtime.py'),
)
HAND_TOOL_REVISION = '75c1aa04e4cfa840d75bee119de11afe7fb2484c'
HEALER_REVISION = 'ae06c9abd939cc1c2df9bdc35654166f66c15c00'
FOLIAGE_REVISION = 'c4f24ea1ae20e2f06b7f5eae951d506ca4ecb2c3'
WIND_REVISION = 'da2826ea8fb49a0464cf177c395de12b19db7998'
EDITOR_SHA = '36bfb7c7a54399cc8d27c02b73c7990766d7db44e7452655cb6a496866653fca'


def sha(data):
    """Hash exact bytes, including stage logs and frozen inputs."""
    return hashlib.sha256(data).hexdigest()


def now():
    """Record UTC timestamps independently of the caller's locale."""
    return datetime.now(timezone.utc).isoformat()


def git(*args):
    """Read the repository's authoritative objects without changing its index."""
    return subprocess.check_output(['git', *args], cwd=ROOT)


def commands(text):
    """Classify every retained check; reject unknown syntax rather than omit it."""
    art, product = [], []
    checking = False
    for line in text.splitlines():
        if line == './scripts/validate-workflows.sh':
            checking = True
            product.append(line)
        elif checking and line.strip():
            if line.startswith('python3 '):
                art.append(shlex.split(line)[1:])
            elif line.startswith(('dotnet ', './scripts/smoke-godot.sh', 'ALLRPCS_', 'godot ')):
                product.append(line)
            else:
                raise ValueError('Unrecognized retained quality command: ' + line)
    if len(art) != 9 or len(product) != 19:
        raise ValueError('Retained quality membership changed')
    return art, product


def native_shutdown(returncode, output):
    """Retry only the diagnosed native mutex abort, never ordinary check failures."""
    last_launch = output.rsplit('Godot Engine v', 1)[-1]
    return (returncode in (-6, 134) and 'mutex lock failed' in last_launch
            and not any(marker in last_launch for marker in ('ERROR:', 'AssertionError', 'Unhandled exception')))


def sandbox_log_startup(returncode, output):
    """Allow explicit host resume only for denied native log startup, never automatic retry or product errors."""
    last_launch = re.split(r'(?m)^Godot Engine v', output)[-1]
    errors = [line for line in last_launch.splitlines() if line.startswith('ERROR:')]
    permitted = ("ERROR: Cannot save file '/Users/hherb/Library/Application Support/Godot/editor_settings-4.6.tres'.",
                 'ERROR: Error saving editor settings to /Users/hherb/Library/Application Support/Godot/editor_settings-4.6.tres',
                 "ERROR: Failed to open 'user://logs/")
    return (returncode in (-6, 134) and errors
            and any(line.startswith(permitted[2]) for line in errors)
            and all(line.startswith(permitted) for line in errors)
            and 'RotatedFileLogger::rotate_file()' in last_launch
            and 'Main::setup(' in last_launch and 'handle_crash: Program crashed with signal 11' in last_launch
            and not any(marker in last_launch for marker in ('AssertionError', 'Unhandled exception')))


class Run:
    """Persist all attempts, stop on failed checks and publish success only at the end."""

    def __init__(self, directory):
        """Start a new report; previous success cannot survive an interrupted run."""
        self.directory = directory
        directory.mkdir(parents=True, exist_ok=True)
        self.report = {'schema_version': 1, 'status': 'running', 'started': now(), 'stages': []}
        self.save()

    def save(self):
        """Replace the report atomically so readers never see partial JSON."""
        pending = self.directory / 'summary.pending.json'
        pending.write_text(json.dumps(self.report, indent=2) + '\n')
        pending.replace(self.directory / 'summary.json')

    def stage(self, name, args, cwd=ROOT, env=None, revision=None, retry_native=False, completion=None):
        """Run an exact command, log every attempt and propagate every unresolved failure."""
        print('RUN:', name, flush=True)
        row = {'name': name, 'arguments': args, 'revision': revision,
               'environment_overrides': env or {}, 'attempts': []}
        self.report['stages'].append(row)
        environment = dict(os.environ, **(env or {}))
        for attempt in range(1, 4 if retry_native else 2):
            log = self.directory / f'{len(self.report["stages"]):02d}-{name}-{attempt}.log'
            with log.open('wb') as output:
                result = subprocess.run(args, cwd=cwd, env=environment, stdout=output, stderr=subprocess.STDOUT)
            data = log.read_bytes()
            completed = completion is None or completion in data.decode(errors='replace')
            row['attempts'].append({'exit_code': result.returncode, 'log': log.name, 'sha256': sha(data),
                                    'required_completion': completion, 'completion_seen': completed})
            self.save()
            if result.returncode == 0 and completed:
                print('PASS:', name, flush=True)
                return
            if retry_native and attempt < 3 and native_shutdown(result.returncode, data.decode(errors='replace')):
                print('RETRY identical command after diagnosed native shutdown:', name, flush=True)
                continue
            self.report.update(status='failed', finished=now(), failed_stage=name)
            self.save()
            raise RuntimeError(f'{name} exited {result.returncode}; see {log}')


@contextmanager
def snapshot(revision, editor=False, repository=ROOT):
    """Extract a commit with private HEAD/index and shared read-only objects; retain the main checkout."""
    with tempfile.TemporaryDirectory(prefix='allrpcs-quality-') as directory:
        tree = Path(directory) / 'tree'
        tree.mkdir()
        archive = Path(directory) / 'tree.tar'
        with archive.open('wb') as output:
            subprocess.run(['git', 'archive', revision], cwd=repository, stdout=output, check=True)
        with tarfile.open(archive) as package:
            package.extractall(tree, filter='data')
        git_dir = Path(directory) / 'git'
        subprocess.run(['git', 'init', '--quiet', '--separate-git-dir', str(git_dir), str(tree)], check=True)
        objects = subprocess.check_output(['git', 'rev-parse', '--path-format=absolute', '--git-path', 'objects'],
                                          cwd=repository, text=True).strip()
        (git_dir / 'objects/info/alternates').write_text(objects + '\n')
        (git_dir / 'HEAD').write_text(revision + '\n')
        environment = {'GIT_INDEX_FILE': str(git_dir / 'index')}
        subprocess.run(['git', 'read-tree', revision], cwd=tree,
                       env=dict(os.environ, **environment), check=True)
        if editor:
            data = (ROOT / 'game/project.godot').read_bytes()
            if sha(data) != EDITOR_SHA:
                raise ValueError('Recorded editor exception changed')
            (tree / 'game/project.godot').write_bytes(data)
        yield tree, environment


def frozen_evidence():
    """Check current retained evidence, so archive validation cannot hide evidence drift."""
    authority_path = 'audio-direction/material-runtime-review/publication/authority.json'
    authority_bytes = (ROOT / authority_path).read_bytes()
    if authority_bytes != git('show', AUDIO_REVISIONS[1][1] + ':' + authority_path):
        raise ValueError('Material publication authority changed')
    excluded = json.loads(authority_bytes)['excluded_evidence_bindings']
    if any(Path(row['path']).name != '.DS_Store' for row in excluded):
        raise ValueError('Only the already-authorized Finder metadata exception is permitted')
    for path in sorted((ROOT / 'audio-direction').glob('*-verification.json')):
        record = json.loads(path.read_text())
        for row in record.get('evidence_bindings', []) + record.get('retained_evidence', []):
            if row in excluded:
                continue
            if path.name == 'foliage-runtime-verification.json' and row['path'] in (
                    'quality.py', 'game/Main.tscn', 'game/MaterialAudioCreditsPanel.cs',
                    'game/MaterialSoundCaptionPanel.cs', 'game/PositionalSoundCueHistoryPanel.cs', 'game/export_presets.cfg'):
                # The wind grant permits these exact additive successors; retain their frozen predecessors.
                if sha(git('show', FOLIAGE_REVISION + ':' + row['path'])) != row['sha256']:
                    raise ValueError('Changed frozen foliage predecessor evidence: ' + row['path'])
                continue
            if path.name == 'wind-runtime-verification.json' and row['path'] in (
                    'quality.py', 'game/Main.tscn', 'game/MaterialAudioCreditsPanel.cs',
                    'game/MaterialSoundCaptionPanel.cs', 'game/PositionalSoundCueHistoryPanel.cs', 'game/export_presets.cfg'):
                # The rain grant permits these additive fields; validate frozen wind bytes at its own revision.
                if sha(git('show', WIND_REVISION + ':' + row['path'])) != row['sha256']:
                    raise ValueError('Changed frozen wind predecessor evidence: ' + row['path'])
                continue
            if sha((ROOT / row['path']).read_bytes()) != row['sha256']:
                raise ValueError('Changed retained evidence: ' + row['path'])


def cleanup_generated_uids(root, candidates, existed, sources):
    """Remove only new valid Godot sidecars of unchanged tracked scripts, preserving prior work."""
    removed = []
    for name in candidates:
        if name in existed or not name.startswith('game/') or not name.endswith('.gd.uid'):
            continue
        source = name[:-4]
        path = root / name
        if source not in sources or not path.is_file() or path.is_symlink():
            continue
        if sha((root / source).read_bytes()) != sources[source]:
            continue
        if not re.fullmatch(rb'uid://[a-z0-9]+\s*', path.read_bytes()):
            continue
        path.unlink()
        removed.append(name)
    return removed


def resume_prefix(prior, directory, art, product, metadata, finish_interior=False):
    """Reuse only verified completed commands before a diagnosed smoke-stage native abort."""
    failed_name = 'interior-normal' if finish_interior else 'product-10'
    if prior.get('status') != 'failed' or prior.get('failed_stage') != failed_name:
        raise ValueError('Resume requires an incomplete smoke-stage run')
    for key in ('product_revision', 'python', 'pillow', 'numpy', 'zlib'):
        if prior.get(key) != metadata[key]:
            raise ValueError('Resume environment or product revision changed: ' + key)
    for name, digest in metadata['inputs'].items():
        if name != 'quality.py' and prior['inputs'].get(name) != digest:
            raise ValueError('Resume check input changed: ' + name)
    plan = [('continuation', [sys.executable, 'scripts/check-continuation.py', '--self-test'], None)]
    plan += [(f'art-{index}', [sys.executable, *args], ART_REVISION) for index, args in enumerate(art, 1)]
    for name, revision, validator in AUDIO_REVISIONS:
        args = [sys.executable, validator] + (['--reviewed'] if name not in ('buses', 'material') else [])
        plan.append(('boundary-' + name, args, revision))
    plan += [('boundary-hand-tool-current', [sys.executable, 'scripts/validate-hand-tool-runtime.py', '--reviewed'], HAND_TOOL_REVISION),
             ('boundary-healer-current', [sys.executable, 'scripts/validate-healer-footstep-runtime.py', '--reviewed'], HEALER_REVISION),
             ('boundary-foliage-current', [sys.executable, 'scripts/validate-foliage-runtime.py', '--reviewed'], FOLIAGE_REVISION),
             ('current-art-imports', [sys.executable, 'scripts/validate-direction-a-import-proof.py'], None)]
    count = 19 if finish_interior else 9
    plan += [(f'product-{index}', ['sh', '-c', command], None) for index, command in enumerate(product[:count], 1)]
    if finish_interior and prior.get('resumed_from'):
        index = len(plan) - 19 + 9
        plan[index:index] = [
            ('boundary-hand-tool-resume', [sys.executable, 'scripts/validate-hand-tool-runtime.py', '--reviewed'], HAND_TOOL_REVISION),
            ('boundary-healer-resume', [sys.executable, 'scripts/validate-healer-footstep-runtime.py', '--reviewed'], HEALER_REVISION),
            ('boundary-foliage-resume', [sys.executable, 'scripts/validate-foliage-runtime.py', '--reviewed'], FOLIAGE_REVISION)]
    rows = prior['stages']
    if len(rows) != len(plan) + 1:
        raise ValueError('Resume stage inventory changed')
    for row, expected in zip(rows, plan):
        if (row['name'], row['arguments'], row['revision']) != expected or row['attempts'][-1]['exit_code'] != 0:
            raise ValueError('Resume command did not complete unchanged: ' + row['name'])
        expected_env = {'ALLRPCS_CHARACTER_MODEL_COMPOSITION_DISABLED': '1',
                        'ALLRPCS_ORDINARY_PARTICIPATION_SMOKE_MODEL': '1'} if 'godot' in ' '.join(row['arguments']) else {}
        if row['revision'] is None and row['environment_overrides'] != expected_env:
            raise ValueError('Resume completed command environment changed')
    for row in rows:
        for attempt in row['attempts']:
            name = attempt['log']
            if Path(name).name != name or sha((directory / name).read_bytes()) != attempt['sha256']:
                raise ValueError('Resume log changed: ' + name)
    failed = rows[-1]
    output = (directory / failed['attempts'][-1]['log']).read_text(errors='replace')
    if finish_interior:
        expected = ['godot', '--headless', '--path', 'game', '--quit-after', '1200']
        if failed['arguments'] != expected or failed['attempts'][-1]['exit_code'] != 1 or 'Sickhouse profile not live' not in output:
            raise ValueError('Finish is limited to the recorded missing fixed-FPS invocation')
    elif failed['arguments'] != ['sh', '-c', product[9]] or not (
            native_shutdown(failed['attempts'][-1]['exit_code'], output)
            or sandbox_log_startup(failed['attempts'][-1]['exit_code'], output)):
        raise ValueError('Resume is limited to diagnosed native shutdown or denied sandbox log startup')
    return rows[:-1]


def main(resume_smoke=False, finish_interior=False):
    """Run all original checks plus accepted audio boundaries and current audio fixtures."""
    import PIL
    import numpy
    from PIL import Image, features
    if PIL.__version__ != '12.3.0' or numpy.__version__ != '2.5.3' or not hasattr(Image.Image, 'get_flattened_data'):
        raise ValueError('Install pinned requirements-quality.txt in artifacts/quality-python')
    if features.check_feature('zlib_ng'):
        raise ValueError('Pillow must use system zlib for retained PNG bytes; reinstall from source')
    art, product = commands((ROOT / 'scripts/quality.sh').read_text())
    directory = ROOT / 'artifacts/quality-current'
    metadata = dict(product_revision=git('rev-parse', 'HEAD').decode().strip(),
                      python=sys.version, pillow=PIL.__version__, numpy=numpy.__version__,
                      zlib=features.version('zlib'), inputs={name: sha((ROOT / name).read_bytes())
                      for name in ('quality.py', 'quality.sh', 'requirements-quality.txt',
                                   'scripts/quality.sh', 'scripts/smoke-godot.sh')})
    resume = resume_smoke or finish_interior
    prior_bytes = (directory / 'summary.json').read_bytes() if resume else None
    prior = json.loads(prior_bytes) if prior_bytes else None
    reused = resume_prefix(prior, directory, art, product, metadata, finish_interior) if prior else []
    run = Run(directory)
    run.report.update(metadata)
    if prior:
        run.report.update(stages=reused, resumed_from={'report_sha256': sha(prior_bytes),
                          'prior_source_inputs': prior['inputs'], 'verified_completed_stages': len(reused),
                          'reason': 'Complete unchanged checks reused after verified native shutdown, denied sandbox log startup or fixed-FPS invocation repair; failed native command runs again.'})
    run.save()
    existed = set(git('ls-files', '--others', '--exclude-standard', '--', 'game').decode().splitlines())
    sources = {name: sha((ROOT / name).read_bytes())
               for name in git('ls-files', '--', 'game/*.gd').decode().splitlines()}
    try:
        frozen_evidence()
        if not resume:
            run.stage('continuation', [sys.executable, 'scripts/check-continuation.py', '--self-test'])
            with snapshot(ART_REVISION) as (tree, env):
                for index, args in enumerate(art, 1):
                    run.stage(f'art-{index}', [sys.executable, *args], tree, env, ART_REVISION)
            for name, revision, validator in AUDIO_REVISIONS:
                with snapshot(revision, editor=name != 'buses') as (tree, env):
                    args = [sys.executable, validator]
                    if name not in ('buses', 'material'):
                        args.append('--reviewed')
                    run.stage('boundary-' + name, args, tree, env, revision)
            with snapshot(HAND_TOOL_REVISION, editor=True) as (tree, env):
                run.stage('boundary-hand-tool-current', [sys.executable, 'scripts/validate-hand-tool-runtime.py', '--reviewed'], tree, env, HAND_TOOL_REVISION)
            with snapshot(HEALER_REVISION, editor=True) as (tree, env):
                run.stage('boundary-healer-current', [sys.executable, 'scripts/validate-healer-footstep-runtime.py', '--reviewed'], tree, env, HEALER_REVISION)
            with snapshot(FOLIAGE_REVISION, editor=True) as (tree, env):
                run.stage('boundary-foliage-current', [sys.executable, 'scripts/validate-foliage-runtime.py', '--reviewed'], tree, env, FOLIAGE_REVISION)
            run.stage('current-art-imports', [sys.executable, 'scripts/validate-direction-a-import-proof.py'])
        else:
            print(f'REUSE: {len(reused)} completed stages with unchanged commands and verified logs', flush=True)
            with snapshot(HAND_TOOL_REVISION, editor=True) as (tree, env):
                run.stage('boundary-hand-tool-resume', [sys.executable, 'scripts/validate-hand-tool-runtime.py', '--reviewed'], tree, env, HAND_TOOL_REVISION)
            with snapshot(HEALER_REVISION, editor=True) as (tree, env):
                run.stage('boundary-healer-resume', [sys.executable, 'scripts/validate-healer-footstep-runtime.py', '--reviewed'], tree, env, HEALER_REVISION)
            with snapshot(FOLIAGE_REVISION, editor=True) as (tree, env):
                run.stage('boundary-foliage-resume', [sys.executable, 'scripts/validate-foliage-runtime.py', '--reviewed'], tree, env, FOLIAGE_REVISION)
        environment = {'ALLRPCS_CHARACTER_MODEL_COMPOSITION_DISABLED': '1',
                       'ALLRPCS_ORDINARY_PARTICIPATION_SMOKE_MODEL': '1'}
        first = 19 if finish_interior else 9 if resume_smoke else 0
        for index, command in enumerate(product[first:], first + 1):
            run.stage(f'product-{index}', ['sh', '-c', command],
                      env=environment if 'godot' in command else None, retry_native='godot' in command)
        for mode in ('normal', 'muted', 'zero', 'mono-reduced'):
            output = run.directory / f'interior-{mode}.json'
            run.stage('interior-' + mode, ['godot', '--headless', '--fixed-fps', '60', '--path', 'game', '--quit-after', '1200'],
                      env=dict(environment, ALLRPCS_INTERIOR_MAIN_CHECK='1', ALLRPCS_INTERIOR_MODE=mode,
                               ALLRPCS_INTERIOR_CHECK_OUTPUT=str(output)), retry_native=True,
                      completion='PASS: interior Main:')
        saves = [(run.directory / f'interior-{mode}.json.save.json').read_bytes()
                 for mode in ('normal', 'muted', 'zero', 'mono-reduced')]
        transcripts = [json.loads((run.directory / f'interior-{mode}.json').read_text())['transcript']
                       for mode in ('normal', 'muted', 'zero', 'mono-reduced')]
        if any(data != saves[0] for data in saves) or any(data != transcripts[0] for data in transcripts):
            raise ValueError('Current audio changed complete saves or transcripts')
        water_modes = ('default', 'quiet', 'master-muted', 'effects-muted', 'missing-stream', 'no-action')
        for mode in water_modes:
            output = run.directory / f'water-{mode}.json'
            run.stage('water-' + mode, ['godot', '--headless', '--fixed-fps', '60', '--path', 'game', '--quit-after', '1200'],
                      env=dict(environment, ALLRPCS_WATER_MAIN_CHECK='1', ALLRPCS_WATER_MODE=mode,
                               ALLRPCS_WATER_CHECK_OUTPUT=str(output)), retry_native=True,
                      completion='PASS: water Main:')
        water_saves = [(run.directory / f'water-{mode}.json.save.json').read_bytes() for mode in water_modes]
        water_reports = [json.loads((run.directory / f'water-{mode}.json').read_text()) for mode in water_modes]
        if any(data != water_saves[0] for data in water_saves) or any(
                row['transcript'] != water_reports[0]['transcript'] for row in water_reports):
            raise ValueError('Water action changed complete saves or transcripts')
        if any(row['commits'] != 5 or row['presentations'] != 5 for row in water_reports[:-1]) or any(
                water_reports[-1][key] != 0 for key in ('commits', 'presentations')):
            raise ValueError('Water action schedule changed or no-action emitted feedback')
        run.report.update(water_save_sha256=sha(water_saves[0]), water_transcript=water_reports[0]['transcript'],
                          water_modes=list(water_modes))
        cask_modes = ('default', 'quiet', 'master-muted', 'effects-muted', 'missing-stream', 'no-action')
        for mode in cask_modes:
            output = run.directory / f'cask-{mode}.json'
            run.stage('cask-' + mode, ['godot', '--headless', '--fixed-fps', '60', '--path', 'game', '--quit-after', '1200'],
                      env=dict(environment, ALLRPCS_CASK_MAIN_CHECK='1', ALLRPCS_CASK_MODE=mode,
                               ALLRPCS_CASK_CHECK_OUTPUT=str(output)), retry_native=True,
                      completion='PASS: cask Main:')
        cask_saves = [(run.directory / f'cask-{mode}.json.save.json').read_bytes() for mode in cask_modes]
        cask_reports = [json.loads((run.directory / f'cask-{mode}.json').read_text()) for mode in cask_modes]
        if any(data != cask_saves[0] for data in cask_saves) or any(
                row['transcript'] != cask_reports[0]['transcript'] for row in cask_reports):
            raise ValueError('Cask handling changed complete saves or transcripts')
        if any(row['commits'] != 7 or row['presentations'] != 7 for row in cask_reports[:-1]) or any(
                cask_reports[-1][key] != 0 for key in ('commits', 'presentations')):
            raise ValueError('Cask action schedule changed or no-action emitted feedback')
        run.report.update(cask_save_sha256=sha(cask_saves[0]), cask_transcript=cask_reports[0]['transcript'],
                          cask_modes=list(cask_modes))
        tool_modes = ('default', 'quiet', 'master-muted', 'effects-muted', 'missing-stream', 'no-action')
        for mode in tool_modes:
            output = run.directory / f'tool-{mode}.json'
            run.stage('hand-tool-' + mode, ['godot', '--headless', '--fixed-fps', '60', '--path', 'game', '--quit-after', '1200'],
                      env=dict(environment, ALLRPCS_TOOL_MAIN_CHECK='1', ALLRPCS_TOOL_MODE=mode,
                               ALLRPCS_TOOL_CHECK_OUTPUT=str(output)), retry_native=True,
                      completion='PASS: hand-tool Main:')
        tool_saves = [(run.directory / f'tool-{mode}.json.save.json').read_bytes() for mode in tool_modes]
        tool_reports = [json.loads((run.directory / f'tool-{mode}.json').read_text()) for mode in tool_modes]
        if any(data != tool_saves[0] for data in tool_saves) or any(
                row['transcript'] != tool_reports[0]['transcript'] for row in tool_reports):
            raise ValueError('Hand-tool tap changed complete saves or transcripts')
        if any(row['commits'] != 6 or row['presentations'] != 6 for row in tool_reports[:-1]) or any(
                tool_reports[-1][key] != 0 for key in ('commits', 'presentations')):
            raise ValueError('Hand-tool action schedule changed or no-action emitted feedback')
        run.report.update(tool_save_sha256=sha(tool_saves[0]), tool_transcript=tool_reports[0]['transcript'],
                          tool_modes=list(tool_modes))
        healer_modes = ('default', 'quiet', 'master-muted', 'effects-muted', 'missing-resources', 'captions-off', 'observer-disabled')
        for mode in healer_modes:
            output = run.directory / f'healer-{mode}.json'
            run.stage('healer-footstep-' + mode, ['godot', '--headless', '--fixed-fps', '60', '--path', 'game', '--quit-after', '1200'],
                      env=dict(environment, ALLRPCS_HEALER_FOOTSTEP_CHECK='1', ALLRPCS_HEALER_FOOTSTEP_MODE=mode,
                               ALLRPCS_HEALER_FOOTSTEP_REPORT=str(output)), retry_native=True,
                      completion='PASS: Healer footstep Main:')
        healer_reports = [json.loads((run.directory / f'healer-{mode}.json').read_text()) for mode in healer_modes]
        healer_saves = [(run.directory / f'healer-{mode}.json.save.json').read_bytes() for mode in healer_modes]
        if any(data != healer_saves[0] for data in healer_saves) or any(
                row['motion'] != healer_reports[0]['motion'] or row['transcript'] != healer_reports[0]['transcript']
                or row['truthBefore'] != row['truthAfter'] or row['truthBefore'] != healer_reports[0]['truthBefore'] for row in healer_reports):
            raise ValueError('Healer presentation changed complete motion/save/transcript/protected truth')
        if (len({row['contacts'] for row in healer_reports[:-1]}) != 1 or healer_reports[0]['contacts'] < 4
                or len({row['presentations'] for row in healer_reports[:-1]}) != 1 or healer_reports[0]['presentations'] < 4) or any(
                healer_reports[-1][key] != 0 for key in ('contacts', 'presentations')):
            raise ValueError('Healer contact owner/mode schedule changed')
        run.report.update(healer_save_sha256=sha(healer_saves[0]), healer_modes=list(healer_modes),
                          healer_contacts=healer_reports[0]['contacts'], healer_presentations=healer_reports[0]['presentations'])
        generated = git('ls-files', '--others', '--exclude-standard', '--', 'game').decode().splitlines()
        run.report['generated_uid_cleanup'] = cleanup_generated_uids(ROOT, generated, existed, sources)
        with snapshot(HAND_TOOL_REVISION, editor=True) as (tree, env):
            run.stage('boundary-hand-tool-after-runtime', [sys.executable, 'scripts/validate-hand-tool-runtime.py', '--reviewed'], tree, env, HAND_TOOL_REVISION)
        with snapshot(HEALER_REVISION, editor=True) as (tree, env):
            run.stage('healer-master-output', [sys.executable, 'scripts/validate-healer-footstep-runtime.py', '--output-only'], tree, env, HEALER_REVISION)
            run.stage('boundary-healer-after-runtime', [sys.executable, 'scripts/validate-healer-footstep-runtime.py', '--reviewed'], tree, env, HEALER_REVISION)
        foliage_runtime(run, environment)
        wind_runtime(run, environment)
        rain_runtime(run, environment)
        run.report.update(status='passed', finished=now(), interior_save_sha256=sha(saves[0]),
                          transcript=transcripts[0])
        run.save()
        print('PASS: full current quality; frozen contracts, current product, four interior modes, six water/control modes and six cask/control modes and six hand-tool/control modes and seven Healer motion modes, seven foliage modes and seven wind modes and seven rain modes', flush=True)
    except Exception:
        run.report.update(status='failed', finished=now())
        run.save()
        raise
    finally:
        generated = git('ls-files', '--others', '--exclude-standard', '--', 'game').decode().splitlines()
        run.report.setdefault('generated_uid_cleanup', []).extend(
            cleanup_generated_uids(ROOT, generated, existed, sources))
        run.save()


def foliage_runtime(run, environment):
    """Build/import the frozen owner, clean verified generated sidecars, and run every retained mode/check."""
    with snapshot(FOLIAGE_REVISION, editor=True) as (tree, env):
        run.stage('foliage-historical-build', ['dotnet', 'build', 'game/Allrpcs.Game.csproj', '-m:1', '/nr:false'], tree, env, FOLIAGE_REVISION)
        run.stage('foliage-historical-import', ['godot', '--headless', '--editor', '--path', 'game', '--import'], tree, env, FOLIAGE_REVISION, retry_native=True)
        candidates = subprocess.check_output(['git', 'ls-files', '--others', '--exclude-standard', '--', 'game'], cwd=tree, env=dict(os.environ, **env), text=True).splitlines()
        tracked_scripts = subprocess.check_output(['git', 'ls-files', '--', 'game/*.gd'], cwd=tree, env=dict(os.environ, **env), text=True).splitlines()
        source_hashes = {name: sha((tree / name).read_bytes()) for name in tracked_scripts}
        run.report['historical_foliage_uid_cleanup'] = cleanup_generated_uids(tree, candidates, set(), source_hashes)
        run.save()
        foliage_modes = ('default', 'quiet', 'master-muted', 'ambience-muted', 'missing-stream', 'captions-off', 'foliage-disabled')
        for mode in foliage_modes:
            output = run.directory / f'foliage-{mode}.json'
            run.stage('foliage-' + mode, ['godot', '--headless', '--fixed-fps', '60', '--path', 'game', '--quit-after', '3600'],
                      cwd=tree, revision=FOLIAGE_REVISION, env=dict(environment, **env, ALLRPCS_FOLIAGE_MAIN_CHECK='1', ALLRPCS_FOLIAGE_MODE=mode,
                               ALLRPCS_FOLIAGE_REPORT=str(output)), retry_native=True,
                      completion='PASS: foliage Main:')
        foliage_reports = [json.loads((run.directory / f'foliage-{mode}.json').read_text()) for mode in foliage_modes]
        foliage_saves = [(run.directory / f'foliage-{mode}.json.save.json').read_bytes() for mode in foliage_modes]
        if any(data != foliage_saves[0] for data in foliage_saves) or any(
                row['motion'] != foliage_reports[0]['motion'] or row['transcript'] != foliage_reports[0]['transcript']
                or row['truthBefore'] != row['truthAfter'] or row['truthBefore'] != foliage_reports[0]['truthBefore'] for row in foliage_reports):
            raise ValueError('Foliage presentation changed complete motion/save/transcript/protected truth')
        if any(row['starts'] != 3 or row['presentations'] != 3 for row in foliage_reports[:-1]) or any(
                foliage_reports[-1][key] != 0 for key in ('starts', 'presentations')):
            raise ValueError('Foliage consumed/admitted schedule changed')
        run.report.update(foliage_modes=list(foliage_modes), foliage_save_sha256=sha(foliage_saves[0]))
        run.stage('foliage-master-output', [sys.executable, 'scripts/validate-foliage-runtime.py', '--output-only'], tree, env, FOLIAGE_REVISION)
        run.stage('boundary-foliage-after-runtime', [sys.executable, 'scripts/validate-foliage-runtime.py', '--reviewed'], tree, env, FOLIAGE_REVISION)


def wind_runtime(run, environment):
    """Keep every frozen wind stage in its own built/imported historical source tree."""
    with snapshot(WIND_REVISION, editor=True) as (tree, env):
        run.stage('wind-historical-restore', ['dotnet', 'restore', 'tests/Application.Tests/Allrpcs.Application.Tests.csproj'], tree, env, WIND_REVISION)
        run.stage('wind-historical-build', ['dotnet', 'build', 'game/Allrpcs.Game.csproj', '-m:1', '/nr:false'], tree, env, WIND_REVISION)
        run.stage('wind-historical-import', ['godot', '--headless', '--editor', '--path', 'game', '--import'], tree, env, WIND_REVISION, retry_native=True)
        candidates = subprocess.check_output(['git', 'ls-files', '--others', '--exclude-standard', '--', 'game'], cwd=tree, env=dict(os.environ, **env), text=True).splitlines()
        scripts = subprocess.check_output(['git', 'ls-files', '--', 'game/*.gd'], cwd=tree, env=dict(os.environ, **env), text=True).splitlines()
        run.report['historical_wind_uid_cleanup'] = cleanup_generated_uids(tree, candidates, set(), {name: sha((tree / name).read_bytes()) for name in scripts})
        run.save()
        wind_checks(run, environment, tree, env)


def wind_checks(run, environment, tree, env):
    """Retain the seven new ordinary movement modes plus measured native and bounded review guards."""
    modes = ('default', 'quiet', 'master-muted', 'ambience-muted', 'missing-stream', 'captions-off', 'wind-disabled')
    run.stage('wind-focused', ['dotnet', 'test', 'tests/Application.Tests/Allrpcs.Application.Tests.csproj',
                             '--no-restore', '--filter', 'FullyQualifiedName~Wind', '--logger', 'console;verbosity=minimal'], tree, env, WIND_REVISION)
    run.stage('boundary-wind-source', [sys.executable, 'scripts/validate-wind-runtime.py', '--source-only'], tree, env, WIND_REVISION)
    for mode in modes:
        output = run.directory / f'wind-{mode}.json'
        run.stage('wind-' + mode, ['godot', '--headless', '--fixed-fps', '60', '--path', 'game', '--quit-after', '7000'],
                  cwd=tree, revision=WIND_REVISION, env=dict(environment, **env, ALLRPCS_WIND_MAIN_CHECK='1', ALLRPCS_WIND_MODE=mode, ALLRPCS_WIND_REPORT=str(output)),
                  retry_native=True, completion='PASS: wind Main:')
    reports = [json.loads((run.directory / f'wind-{mode}.json').read_text()) for mode in modes]
    saves = [(run.directory / f'wind-{mode}.json.save.json').read_bytes() for mode in modes]
    if any(data != saves[0] for data in saves) or any(
            row['motion'] != reports[0]['motion'] or row['transcript'] != reports[0]['transcript']
            or row['truthBefore'] != row['truthAfter'] or row['truthBefore'] != reports[0]['truthBefore'] for row in reports):
        raise ValueError('Wind changed complete motion/save/transcript/protected truth')
    if any(row['starts'] != 3 or row['presentations'] != 3 for row in reports[:-1]) or any(
            reports[-1][key] != 0 for key in ('starts', 'presentations')):
        raise ValueError('Wind consumed/admitted schedule changed')
    run.report.update(wind_modes=list(modes), wind_save_sha256=sha(saves[0]))
    run.stage('wind-master-output', [sys.executable, 'scripts/validate-wind-runtime.py', '--output-only'], tree, env, WIND_REVISION)
    run.stage('boundary-wind-reviewed', [sys.executable, 'scripts/validate-wind-runtime.py', '--reviewed'], tree, env, WIND_REVISION)


def rain_runtime(run, environment):
    """Retain the seven new ordinary movement modes plus measured native and bounded review guards."""
    modes = ('default', 'quiet', 'master-muted', 'ambience-muted', 'missing-stream', 'captions-off', 'rain-disabled')
    run.stage('rain-focused', ['dotnet', 'test', 'tests/Application.Tests/Allrpcs.Application.Tests.csproj',
                             '--no-restore', '--filter', 'FullyQualifiedName~Audio.Rain', '--logger', 'console;verbosity=minimal'])
    run.stage('boundary-rain-source', [sys.executable, 'scripts/validate-rain-runtime.py', '--source-only'])
    for mode in modes:
        output = run.directory / f'rain-{mode}.json'
        run.stage('rain-' + mode, ['godot', '--headless', '--fixed-fps', '60', '--path', 'game', '--quit-after', '7600'],
                  env=dict(environment, ALLRPCS_RAIN_MAIN_CHECK='1', ALLRPCS_RAIN_MODE=mode, ALLRPCS_RAIN_REPORT=str(output)),
                  retry_native=True, completion='PASS: rain Main:')
    reports = [json.loads((run.directory / f'rain-{mode}.json').read_text()) for mode in modes]
    saves = [(run.directory / f'rain-{mode}.json.save.json').read_bytes() for mode in modes]
    if any(data != saves[0] for data in saves) or any(
            row['motion'] != reports[0]['motion'] or row['transcript'] != reports[0]['transcript']
            or row['truthBefore'] != row['truthAfter'] or row['truthBefore'] != reports[0]['truthBefore'] for row in reports):
        raise ValueError('Rain changed complete motion/save/transcript/protected truth')
    if any(row['starts'] != 3 or row['presentations'] != 3 for row in reports[:-1]) or any(
            reports[-1][key] != 0 for key in ('starts', 'presentations')):
        raise ValueError('Rain consumed/admitted schedule changed')
    run.report.update(rain_modes=list(modes), rain_save_sha256=sha(saves[0]))
    run.stage('rain-master-output', [sys.executable, 'scripts/validate-rain-runtime.py', '--output-only'])
    run.stage('boundary-rain-reviewed', [sys.executable, 'scripts/validate-rain-runtime.py', '--reviewed'])


def resume_foliage_prefix(prior, directory):
    """Reuse the exact known cold-import failure's completed prefix, verifying every retained command and log."""
    sidecars = prior.get('failed_stage') == 'boundary-foliage-after-runtime' and prior['inputs']['quality.py'] == 'f736e3521fed3e81ea24f5bdeff2abe8e509efb68cf5409a06be3e2da49ef2fe'
    import_wait = prior.get('failed_stage') == 'foliage-default' and prior['inputs']['quality.py'] == 'e341a856535418838f66d3c98ae6101bef901c54a218ad2899bb0da4f432b414'
    if prior.get('status') != 'failed' or not (sidecars or import_wait):
        raise ValueError('Foliage resume requires the recorded pre-cleanup runner failure')
    previous = json.loads((ROOT / 'audio-direction/foliage-runtime-review/quality/summary.json').read_text())['stages']
    expected = previous[:next(i for i, row in enumerate(previous) if row['name'] == 'foliage-default')]
    expected = [dict(row) for row in expected]
    slot = next(i for i, row in enumerate(expected) if row['name'] == 'product-10')
    checks = [('hand-tool', HAND_TOOL_REVISION, 'scripts/validate-hand-tool-runtime.py'),
              ('healer', HEALER_REVISION, 'scripts/validate-healer-footstep-runtime.py'),
              ('foliage', FOLIAGE_REVISION, 'scripts/validate-foliage-runtime.py')]
    expected[slot:slot] = [{'name': 'boundary-' + name + '-resume', 'arguments': [sys.executable, script, '--reviewed'],
                           'revision': revision} for name, revision, script in checks]
    for row in expected:
        if row['name'] == 'boundary-foliage-current':
            row['revision'] = FOLIAGE_REVISION
    tail = ['foliage-historical-build', 'foliage-historical-import'] + [row['name'] for row in previous[len(previous) - 9:]]
    if import_wait:
        tail = tail[:3]
    rows = prior['stages']
    if [row['name'] for row in rows] != [row['name'] for row in expected] + tail:
        raise ValueError('Cold foliage stage inventory changed')
    for actual, planned in zip(rows, expected):
        if any(actual[key] != planned[key] for key in ('name', 'arguments', 'revision')):
            raise ValueError('Completed prefix command changed: ' + actual['name'])
    for row in rows:
        for attempt in row['attempts']:
            path = directory / attempt['log']
            if path.name != attempt['log'] or sha(path.read_bytes()) != attempt['sha256']:
                raise ValueError('Cold foliage log changed')
        if row is not rows[-1] and (row['attempts'][-1]['exit_code'] != 0 or not row['attempts'][-1]['completion_seen']):
            raise ValueError('Incomplete prefix or native mode')
    failed = rows[-1]['attempts'][-1]
    output = (directory / failed['log']).read_text()
    if failed['exit_code'] != 1:
        raise ValueError('Unexpected cold foliage failure exit')
    if sidecars and 'ValueError: Unapproved extra path' not in output:
        raise ValueError('Foliage resume requires the exact generated-sidecar failure')
    if import_wait:
        imported = next(row for row in rows if row['name'] == 'foliage-historical-import')['attempts'][-1]
        if ('Scan thread aborted...' not in (directory / imported['log']).read_text()
                or 'OpeningRegionFoliageAudioSource.GardenValid()' not in output
                or 'Failed loading resource:' not in output):
            raise ValueError('Foliage resume requires the diagnosed aborted cold import/missing resource failure')
    return rows[:len(expected)]


def finish_foliage():
    """Complete root quality after the isolated cold-import cleanup repair without repeating unchanged product checks."""
    directory = ROOT / 'artifacts/quality-current'
    prior_bytes = (directory / 'summary.json').read_bytes(); prior = json.loads(prior_bytes)
    reused = resume_foliage_prefix(prior, directory)
    if prior['product_revision'] != git('rev-parse', 'HEAD').decode().strip() or prior['python'] != sys.version:
        raise ValueError('Foliage resume product or interpreter changed')
    for name, value in prior['inputs'].items():
        if name != 'quality.py' and sha((ROOT / name).read_bytes()) != value:
            raise ValueError('Foliage resume input changed: ' + name)
    frozen_evidence()
    run = Run(directory)
    metadata = {key: value for key, value in prior.items() if key not in
                ('status', 'stages', 'started', 'finished', 'failed_stage', 'resumed_from')}
    metadata['inputs'] = dict(prior['inputs'], **{'quality.py': sha((ROOT / 'quality.py').read_bytes())})
    run.report.update(metadata, stages=reused, resumed_from={'report_sha256': sha(prior_bytes),
        'prior_source_inputs': prior['inputs'], 'verified_completed_stages': len(reused),
        'reason': 'Preserve the unchanged complete product prefix; rerun the full cold foliage build/import/modes/output/boundary after verified generated-sidecar cleanup.'})
    run.save()
    print(f'REUSE: {len(reused)} verified completed stages; repeat the affected cold foliage stages', flush=True)
    try:
        foliage_runtime(run, {'ALLRPCS_CHARACTER_MODEL_COMPOSITION_DISABLED': '1',
                             'ALLRPCS_ORDINARY_PARTICIPATION_SMOKE_MODEL': '1'})
        run.stage('boundary-wind-source', [sys.executable, 'scripts/validate-wind-runtime.py', '--source-only'])
        run.report.update(status='passed', finished=now()); run.save()
        print('PASS: full current quality with verified product prefix and repaired cold foliage stages', flush=True)
    except Exception:
        run.report.update(status='failed', finished=now()); run.save(); raise


def resume_rain_prefix(prior, directory):
    """Reuse the verified unchanged product prefix only after the recorded missing historical test assets."""
    review = ROOT / 'audio-direction/rain-runtime-review/quality'
    if (prior.get('status') != 'failed' or prior.get('failed_stage') != 'wind-focused'
            or prior['inputs']['quality.py'] != 'a1c81e8e303d84948cec4e4d3f663e4f9fa062db9cf9d9b5056fa68b9194398d'):
        raise ValueError('Rain finish requires the recorded cold wind restore failure')
    original = json.loads((review / 'cold-wind-restore-failed-summary.json').read_text())
    if prior != original:
        raise ValueError('Rain finish report changed')
    expected = json.loads((ROOT / 'audio-direction/wind-runtime-review/quality/runtime/summary.json').read_text())['stages'][:-11]
    rows = prior['stages']
    if [r['name'] for r in rows] != [r['name'] for r in expected] + ['wind-historical-build', 'wind-historical-import', 'wind-focused']:
        raise ValueError('Rain finish retained stage inventory changed')
    for actual, planned in zip(rows, expected):
        if any(actual[key] != planned[key] for key in ('name', 'arguments', 'revision')):
            raise ValueError('Rain finish completed command changed: ' + actual['name'])
    for row in rows:
        for attempt in row['attempts']:
            name = attempt['log']
            if Path(name).name != name or sha((directory / name).read_bytes()) != attempt['sha256']:
                raise ValueError('Rain finish log changed')
        if row is not rows[-1] and (row['attempts'][-1]['exit_code'] != 0 or not row['attempts'][-1]['completion_seen']):
            raise ValueError('Rain finish prefix incomplete')
    failure = rows[-1]['attempts'][-1]
    output = (directory / failure['log']).read_text()
    if failure['exit_code'] != 1 or 'error NETSDK1004: Assets file' not in output or '/tests/Application.Tests/obj/project.assets.json' not in output:
        raise ValueError('Rain finish requires the diagnosed missing historical test restore')
    for row in json.loads((review / 'cold-wind-restore-product-inputs.json').read_text()):
        if sha((ROOT / row['path']).read_bytes()) != row['sha256']:
            raise ValueError('Rain finish product input changed: ' + row['path'])
    return rows[:len(expected)]


def finish_rain():
    """Complete root quality after restoring historical wind tests; preserve all unchanged product passes."""
    directory = ROOT / 'artifacts/quality-current'
    prior_bytes = (directory / 'summary.json').read_bytes(); prior = json.loads(prior_bytes)
    reused = resume_rain_prefix(prior, directory)
    if prior['product_revision'] != git('rev-parse', 'HEAD').decode().strip() or prior['python'] != sys.version:
        raise ValueError('Rain finish revision or interpreter changed')
    for name, digest in prior['inputs'].items():
        if name != 'quality.py' and sha((ROOT / name).read_bytes()) != digest:
            raise ValueError('Rain finish quality input changed: ' + name)
    frozen_evidence()
    run = Run(directory)
    metadata = {key: value for key, value in prior.items() if key not in
                ('status', 'stages', 'started', 'finished', 'failed_stage', 'resumed_from')}
    metadata['inputs'] = dict(prior['inputs'], **{'quality.py': sha((ROOT / 'quality.py').read_bytes())})
    run.report.update(metadata, stages=reused, resumed_from={'report_sha256': sha(prior_bytes),
        'prior_source_inputs': prior['inputs'], 'verified_completed_stages': len(reused),
        'reason': 'Preserve 84 unchanged completed product stages; rerun the entire isolated wind restore/build/import/tests/modes and all current rain stages.'})
    run.save()
    existed = set(git('ls-files', '--others', '--exclude-standard', '--', 'game').decode().splitlines())
    sources = {name: sha((ROOT / name).read_bytes()) for name in git('ls-files', '--', 'game/*.gd').decode().splitlines()}
    try:
        environment = {'ALLRPCS_CHARACTER_MODEL_COMPOSITION_DISABLED': '1', 'ALLRPCS_ORDINARY_PARTICIPATION_SMOKE_MODEL': '1'}
        wind_runtime(run, environment)
        rain_runtime(run, environment)
        run.report.update(status='passed', finished=now()); run.save()
        print('PASS: full current quality; verified unchanged product prefix, restored historical wind and all current rain stages', flush=True)
    except Exception:
        run.report.update(status='failed', finished=now()); run.save(); raise
    finally:
        generated = git('ls-files', '--others', '--exclude-standard', '--', 'game').decode().splitlines()
        run.report.setdefault('generated_uid_cleanup', []).extend(cleanup_generated_uids(ROOT, generated, existed, sources))
        run.save()


class ContractTests(unittest.TestCase):
    """Reject omitted checks and failed stages, and constrain the exceptional native retry."""

    def test_command_membership(self):
        """Unknown commands and lost retained checks cannot silently pass."""
        text = (ROOT / 'scripts/quality.sh').read_text()
        self.assertEqual(tuple(map(len, commands(text))), (9, 19))
        for mutation in (text + '\nunknown-check\n', text.replace('dotnet format Allrpcs.sln --no-restore --verify-no-changes\n', '')):
            with self.assertRaises(ValueError):
                commands(mutation)

    def test_native_retry_boundary(self):
        """Ordinary failures and product errors remain fatal even with a mutex message."""
        self.assertTrue(native_shutdown(134, 'mutex lock failed'))
        for code, output in ((1, 'mutex lock failed'), (134, 'ERROR: product failed; mutex lock failed'), (134, 'other abort')):
            self.assertFalse(native_shutdown(code, output))
        prior_errors = 'ERROR: earlier successful launch cleanup\nGodot Engine v4.6.3\nsmoke passed\nmutex lock failed'
        self.assertTrue(native_shutdown(134, prior_errors))
        self.assertFalse(native_shutdown(134, prior_errors.replace('smoke passed', 'ERROR: current check failed')))

    def test_sandbox_log_startup_resume_is_narrow(self):
        """A host retry may reuse prior passes only for the known settings/log-denial startup crash."""
        output = ("Godot Engine v4.6.3\nERROR: Failed to open 'user://logs/godot-proof.log'.\n"
                  'handle_crash: Program crashed with signal 11\nEngine version: Godot Engine v4.6.3\n'
                  'RotatedFileLogger::rotate_file()\nMain::setup(')
        self.assertTrue(sandbox_log_startup(134, output))
        self.assertFalse(native_shutdown(134, output))
        for code, changed in ((1, output), (134, output.replace('RotatedFileLogger::rotate_file()', 'OtherCrash')),
                              (134, output + '\nERROR: product failed'), (134, output + '\nUnhandled exception'),
                              (134, output.replace("user://logs/", "res://assets/"))):
            self.assertFalse(sandbox_log_startup(code, changed))

    def test_cold_foliage_resume_rejects_missing_stages(self):
        """Only the exact retained 76-stage prefix can precede a repaired cold foliage tail."""
        import copy
        record = json.loads((ROOT / 'audio-direction/wind-runtime-review/quality/cold-import-failed-summary.json').read_text())
        for changed in (dict(record, status='passed'), dict(record, failed_stage='other')):
            with self.assertRaises(ValueError):
                resume_foliage_prefix(changed, ROOT / 'artifacts/quality-current')
        changed = copy.deepcopy(record); changed['stages'].pop(0)
        with self.assertRaises(ValueError):
            resume_foliage_prefix(changed, ROOT / 'artifacts/quality-current')

    def test_rain_finish_rejects_changed_failure_or_product(self):
        """Resume refuses other failures, changed reports and changed product input bindings."""
        import copy
        record = json.loads((ROOT / 'audio-direction/rain-runtime-review/quality/cold-wind-restore-failed-summary.json').read_text())
        for changed in (dict(record, status='passed'), dict(record, failed_stage='other')):
            with self.assertRaises(ValueError):
                resume_rain_prefix(changed, ROOT / 'artifacts/quality-current')
        changed = copy.deepcopy(record); changed['stages'].pop(0)
        with self.assertRaises(ValueError):
            resume_rain_prefix(changed, ROOT / 'artifacts/quality-current')

    def test_failure_persists(self):
        """A nonzero command yields a failed report, not a success or retry."""
        with tempfile.TemporaryDirectory() as directory:
            run = Run(Path(directory))
            with self.assertRaises(RuntimeError):
                run.stage('fails', [sys.executable, '-c', 'raise SystemExit(7)'], retry_native=True)
            self.assertEqual(run.report['status'], 'failed')
            self.assertEqual(len(run.report['stages'][0]['attempts']), 1)

    def test_native_retry_keeps_command(self):
        """One simulated native abort reruns identical argv and records both outcomes."""
        with tempfile.TemporaryDirectory() as directory:
            run = Run(Path(directory))
            marker = Path(directory) / 'attempt'
            code = 'from pathlib import Path; import sys; p=Path(sys.argv[1]); seen=p.exists(); p.touch(); print("ok" if seen else "mutex lock failed"); sys.exit(0 if seen else 134)'
            args = [sys.executable, '-c', code, str(marker)]
            run.stage('native', args, retry_native=True)
            self.assertEqual(run.report['stages'][0]['arguments'], args)
            self.assertEqual([r['exit_code'] for r in run.report['stages'][0]['attempts']], [134, 0])

    def test_incomplete_fixture_is_fatal(self):
        """A timeout or missing fixture completion cannot pass just because the process exited zero."""
        with tempfile.TemporaryDirectory() as directory:
            run = Run(Path(directory))
            with self.assertRaises(RuntimeError):
                run.stage('incomplete', [sys.executable, '-c', 'print("started")'], completion='finished')
            self.assertEqual(run.report['status'], 'failed')

    def test_generated_uid_cleanup_preserves_work(self):
        """Cleanup preserves existing sidecars, user edits and non-UID content."""
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            (root / 'game').mkdir()
            names = ['new', 'existing', 'edited', 'other']
            sources = {}
            for name in names:
                source = 'game/' + name + '.gd'
                (root / source).write_text('extends Node\n')
                sources[source] = sha((root / source).read_bytes())
                (root / (source + '.uid')).write_text('uid://abc123\n')
            (root / 'game/edited.gd').write_text('extends Node2D\n')
            (root / 'game/other.gd.uid').write_text('user notes\n')
            candidates = ['game/' + name + '.gd.uid' for name in names]
            self.assertEqual(cleanup_generated_uids(root, candidates, {'game/existing.gd.uid'}, sources),
                             ['game/new.gd.uid'])
            self.assertTrue(all((root / name).exists() for name in candidates[1:]))

    def test_snapshot_has_its_own_head(self):
        """Historical ledger reads must see the archived commit without moving the live HEAD."""
        with tempfile.TemporaryDirectory() as directory:
            repo = Path(directory)
            subprocess.run(['git', 'init', '--quiet', str(repo)], check=True)
            (repo / 'state.json').write_text('old\n')
            subprocess.run(['git', 'add', 'state.json'], cwd=repo, check=True)
            commit = ['git', '-c', 'user.name=Quality', '-c', 'user.email=quality@example.invalid',
                      'commit', '--quiet', '-m', 'fixture']
            subprocess.run(commit, cwd=repo, check=True)
            old = subprocess.check_output(['git', 'rev-parse', 'HEAD'], cwd=repo, text=True).strip()
            (repo / 'state.json').write_text('new\n')
            subprocess.run(['git', 'add', 'state.json'], cwd=repo, check=True)
            subprocess.run(commit, cwd=repo, check=True)
            live = subprocess.check_output(['git', 'rev-parse', 'HEAD'], cwd=repo, text=True).strip()
            with snapshot(old, repository=repo) as (tree, env):
                isolated = dict(os.environ, **env)
                self.assertEqual(subprocess.check_output(['git', 'rev-parse', 'HEAD'], cwd=tree,
                                                        env=isolated, text=True).strip(), old)
                self.assertEqual((tree / 'state.json').read_text(), 'old\n')
                self.assertEqual(subprocess.check_output(['git', 'diff', '--name-only'], cwd=tree, env=isolated), b'')
            self.assertEqual(subprocess.check_output(['git', 'rev-parse', 'HEAD'], cwd=repo, text=True).strip(), live)


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--self-test', action='store_true')
    parser.add_argument('--resume-smoke', action='store_true')
    parser.add_argument('--finish-interior', action='store_true')
    parser.add_argument('--finish-foliage', action='store_true')
    parser.add_argument('--finish-rain', action='store_true')
    arguments = parser.parse_args()
    if arguments.self_test:
        unittest.main(argv=[sys.argv[0]])
    elif arguments.finish_foliage:
        finish_foliage()
    elif arguments.finish_rain:
        finish_rain()
    else:
        main(arguments.resume_smoke, arguments.finish_interior)
