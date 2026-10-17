#!/usr/bin/env python3
"""Run frozen contracts at recorded revisions and full current product quality."""

import argparse
from contextlib import contextmanager, nullcontext
from datetime import datetime, timezone
import hashlib
import json
import math
import os
from pathlib import Path
import re
import shlex
import shutil
import signal
import time
import uuid
import subprocess
import sys
import tarfile
import tempfile
import unittest

# One hour per stage, including retries: generous even relative to the whole retained
# gate (under an hour). Tests inject short deadlines; no stage is unbounded.
STAGE_TIMEOUT_SECONDS = 60 * 60
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
RAIN_REVISION = '9115a64199b4838390caaa7b8a56ba64e903544c'
HEATING_REVISION = 'b7912dd46a91e4b101c44cf56701d00f45b9ffd8'
STEEPING_REVISION = '7836911042760409e181df80b4bfdd9951a8e7b2'
COOLING_REVISION = 'e92e6e085e4d498e1339e8babc49a6623a9b7613'
HERB_REVISION = 'db298c8fa5dfe220467c4cc113d0998e1c1aec01'
CONTROLS_REVISION = '1b0cab4060dd1eaca9c780cf85c38be1662d8513'
DIALOGUE_REVISION = 'e7b17575479be3f9061979c3269168f956782566'
DIALOGUE_SPEECH_REVISION = '07bd7a487fa52af992f47481e6b8a3eccad84bb4'
PLAYER_ASR_REVISION = 'ad10fcd34eab0520492591bef78c1fea1ea62572'
EDITOR_REVISION = 'a55e9e8dae73e9ec65472cb6a631a350d2ef1aba'
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
    # smoke-lib reports its own exit status as "FAIL: smoke launch exited"; only a check's FAIL forbids retry.
    return (returncode in (-6, 134) and 'mutex lock failed' in last_launch
            and not re.search(r'^FAIL:(?! smoke launch exited )', last_launch, re.M)
            and not any(marker in last_launch for marker in ('ERROR:', 'AssertionError', 'Unhandled exception')))


def native_launch(real, arguments, original_path):
    """Retry only one identical native command on diagnosed shutdown; propagate every other failure."""
    environment = dict(os.environ, PATH=original_path)
    for attempt in range(1, 4):
        result = subprocess.run([real, *arguments], env=environment, stdout=subprocess.PIPE,
                                stderr=subprocess.STDOUT)
        sys.stdout.buffer.write(result.stdout)
        sys.stdout.buffer.flush()
        if result.returncode == 0:
            return 0
        if attempt < 3 and native_shutdown(result.returncode, result.stdout.decode(errors='replace')):
            print('RETRY identical native launch after diagnosed shutdown:', arguments, flush=True)
            continue
        return result.returncode if result.returncode >= 0 else 128 - result.returncode
    raise AssertionError('Unreachable native retry fallthrough')


@contextmanager
def native_retry_shim():
    """Keep the frozen smoke script unchanged while retrying its individual real Godot launches."""
    real = shutil.which('godot')
    if real is None:
        raise ValueError('Godot executable unavailable')
    original_path = os.environ['PATH']
    with tempfile.TemporaryDirectory(prefix='allrpcs-native-retry-') as directory:
        wrapper = Path(directory) / 'godot'
        code = ('#!/usr/bin/env python3\nimport runpy, sys\n'
                + 'scope = runpy.run_path(' + repr(str(ROOT / 'quality.py')) + ')\n'
                + 'raise SystemExit(scope["native_launch"](' + repr(real)
                + ', sys.argv[1:], ' + repr(original_path) + '))\n')
        wrapper.write_text(code)
        wrapper.chmod(0o755)
        os.environ['PATH'] = directory + os.pathsep + original_path
        try:
            yield {'real_godot': real, 'wrapper_sha256': sha(code.encode()), 'max_identical_attempts': 3,
                   'reason': 'Individual frozen smoke launches retry only diagnosed native shutdown; real errors remain fatal.'}
        finally:
            os.environ['PATH'] = original_path


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


def stop_stage(process):
    """Terminate only this stage's new session/group, including children after leader exit."""
    for signum in (signal.SIGTERM, signal.SIGKILL):
        try:
            os.killpg(process.pid, signum)
        except ProcessLookupError:
            pass
        if signum == signal.SIGTERM:
            try:
                process.wait(timeout=0.2)
            except subprocess.TimeoutExpired:
                pass
    process.wait()


class Run:
    """Persist all attempts, stop on failed checks and publish success only at the end."""

    def __init__(self, directory, preserve_failed=False):
        """Start a run; explicit resumes retain the failed summary until recovery passes."""
        self.directory = directory
        self.preserve_failed = preserve_failed
        self.log_prefix = uuid.uuid4().hex + '-' if preserve_failed else ''
        directory.mkdir(parents=True, exist_ok=True)
        self.report = {'schema_version': 1, 'status': 'running', 'started': now(), 'stages': []}
        self.save()

    def save(self):
        """Replace the report atomically so readers never see partial JSON."""
        pending = self.directory / 'summary.pending.json'
        pending.write_text(json.dumps(self.report, indent=2) + '\n')
        target = 'summary.resume.json' if self.preserve_failed and self.report['status'] != 'passed' else 'summary.json'
        pending.replace(self.directory / target)

    def stage(self, name, args, cwd=ROOT, env=None, revision=None, retry_native=False, completion=None,
              timeout_seconds=STAGE_TIMEOUT_SECONDS):
        """Run an exact command, log every attempt and propagate every unresolved failure."""
        if not math.isfinite(timeout_seconds) or timeout_seconds <= 0:
            raise ValueError('Stage timeout must be finite and positive')
        if os.name != 'posix':
            raise ValueError('Quality stages require POSIX owned process groups')
        deadline = time.monotonic() + timeout_seconds
        print('RUN:', name, flush=True)
        row = {'name': name, 'arguments': args, 'revision': revision,
               'environment_overrides': env or {}, 'attempts': []}
        self.report['stages'].append(row)
        environment = dict(os.environ, **(env or {}))
        for attempt in range(1, 4 if retry_native else 2):
            log = self.directory / f'{self.log_prefix}{len(self.report["stages"]):02d}-{name}-{attempt}.log'
            timed_out = False
            with log.open('wb') as output:
                try:
                    result = subprocess.Popen(args, cwd=cwd, env=environment, stdout=output,
                                              stderr=subprocess.STDOUT, start_new_session=True)
                except OSError as error:
                    output.write(str(error).encode())
                    result = subprocess.CompletedProcess(args, 127)
                else:
                    try:
                        result.wait(timeout=max(0, deadline - time.monotonic()))
                    except subprocess.TimeoutExpired:
                        timed_out = True
                        stop_stage(result)
                    except BaseException:
                        stop_stage(result)
                        raise
                if timed_out:
                    output.write(b'\nFAIL: stage wall-clock timeout\n')
            data = log.read_bytes()
            completed = completion is None or completion in data.decode(errors='replace')
            row['attempts'].append({'exit_code': result.returncode, 'log': log.name, 'sha256': sha(data),
                                    'required_completion': completion, 'completion_seen': completed,
                                    'timed_out': timed_out, 'timeout_seconds': timeout_seconds})
            self.save()
            if result.returncode == 0 and completed and not timed_out:
                print('PASS:', name, flush=True)
                return
            if not timed_out and retry_native and attempt < 3 and native_shutdown(result.returncode, data.decode(errors='replace')):
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
            data = git('show', f'{EDITOR_REVISION}:game/project.godot')
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
    controls_grant = ROOT / 'input-direction/remapping-runtime-authority.json'
    controls_paths, controls_baseline = set(), None
    if controls_grant.exists():
        grant = json.loads(controls_grant.read_text())
        if grant['owner_statement'] != 'approved' or grant['slice_id'] != 'phase11-control-remapping-runtime-integration':
            raise ValueError('Controls successor lacks the exact recorded owner grant')
        for binding in grant['proposal_bindings']:
            if sha((ROOT / binding['path']).read_bytes()) != binding['sha256']:
                raise ValueError('Approved controls proposal changed')
        proposal = json.loads((ROOT / 'input-direction/remapping-owner-scope.json').read_text())
        if grant['scope'] != proposal['runtime_proposal']:
            raise ValueError('Controls grant differs from the approved finite proposal')
        controls_paths = set(grant['scope']['modified_files'])
        controls_baseline = grant['baseline_commit']
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
            if path.name == 'rain-runtime-verification.json' and row['path'] in (
                    'quality.py', 'game/Main.tscn', 'game/MaterialAudioCreditsPanel.cs',
                    'game/MaterialSoundCaptionPanel.cs', 'game/PositionalSoundCueHistoryPanel.cs', 'game/export_presets.cfg'):
                # The heating grant permits only additive owners; retain the exact rain predecessor.
                if sha(git('show', RAIN_REVISION + ':' + row['path'])) != row['sha256']:
                    raise ValueError('Changed frozen rain predecessor evidence: ' + row['path'])
                continue
            if path.name == 'remedy-heating-runtime-verification.json' and row['path'] in (
                    'quality.py', 'game/Main.tscn', 'game/MaterialAudioCreditsPanel.cs',
                    'game/MaterialSoundCaptionPanel.cs', 'game/PositionalSoundCueHistoryPanel.cs',
                    'game/OpeningRegionRemedyPreparationInputCoordinator.cs', 'game/export_presets.cfg'):
                # The steeping grant permits only these additive successors; retain exact heating evidence.
                if sha(git('show', HEATING_REVISION + ':' + row['path'])) != row['sha256']:
                    raise ValueError('Changed frozen heating predecessor evidence: ' + row['path'])
                continue
            if path.name == 'remedy-steeping-runtime-verification.json' and row['path'] in (
                    'quality.py', 'game/Main.tscn', 'game/MaterialAudioCreditsPanel.cs',
                    'game/MaterialSoundCaptionPanel.cs', 'game/PositionalSoundCueHistoryPanel.cs',
                    'game/OpeningRegionRemedyPreparationInputCoordinator.cs', 'game/export_presets.cfg'):
                # The cooling grant permits only these additive successors; retain exact heating evidence.
                if sha(git('show', STEEPING_REVISION + ':' + row['path'])) != row['sha256']:
                    raise ValueError('Changed frozen steeping predecessor evidence: ' + row['path'])
                continue
            if path.name == 'remedy-cooling-runtime-verification.json' and row['path'] in (
                    'quality.py', 'game/Main.tscn', 'game/MaterialAudioCreditsPanel.cs',
                    'game/MaterialSoundCaptionPanel.cs', 'game/PositionalSoundCueHistoryPanel.cs', 'game/export_presets.cfg'):
                # The herb grant permits only these additive successors; preserve exact cooling evidence.
                if sha(git('show', COOLING_REVISION + ':' + row['path'])) != row['sha256']:
                    raise ValueError('Changed frozen cooling predecessor evidence: ' + row['path'])
                continue
            if path.name == 'herb-collection-runtime-verification.json' and row['path'] == 'game/export_presets.cfg':
                # Phase 2 shipping exclusions are authorized; retain the exact herb export predecessor.
                if sha(git('show', HERB_REVISION + ':' + row['path'])) != row['sha256']:
                    raise ValueError('Changed frozen herb predecessor evidence: ' + row['path'])
                continue
            if row['path'] in controls_paths:
                # Input-only successor changes are granted; retained predecessor evidence stays exact.
                if sha(git('show', controls_baseline + ':' + row['path'])) != row['sha256']:
                    raise ValueError('Changed controls predecessor evidence: ' + row['path'])
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
    boundary_names = {'boundary-hand-tool-resume', 'boundary-healer-resume', 'boundary-foliage-resume'}
    if any(row['name'] in boundary_names for row in prior['stages']):
        index = len(plan) - count + 9
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
    run = Run(directory, preserve_failed=resume)
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
            if index == 10:
                if not resume:
                    # Retain the three accepted checkpoint boundaries in a complete fresh run too.
                    for label, revision, script in (
                            ('hand-tool', HAND_TOOL_REVISION, 'scripts/validate-hand-tool-runtime.py'),
                            ('healer', HEALER_REVISION, 'scripts/validate-healer-footstep-runtime.py'),
                            ('foliage', FOLIAGE_REVISION, 'scripts/validate-foliage-runtime.py')):
                        with snapshot(revision, editor=True) as (tree, env):
                            run.stage('boundary-' + label + '-resume', [sys.executable, script, '--reviewed'], tree, env, revision)
                with native_retry_shim() as audit:
                    run.report['native_launch_retry'] = audit
                    run.stage(f'product-{index}', ['sh', '-c', command], env=environment, retry_native=True)
            else:
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
        heating_runtime(run, environment)
        steeping_runtime(run, environment)
        cooling_runtime(run, environment)
        herb_runtime(run, environment)
        controls_runtime(run, environment)
        dialogue_accessibility_runtime(run, environment)
        dialogue_speech_runtime(run, environment)
        player_asr_runtime(run, environment)
        space_hold_runtime(run, environment)
        ollama_participation_runtime(run, environment)
        run.report.update(interior_save_sha256=sha(saves[0]),
                          transcript=transcripts[0])
    except BaseException:
        run.report.update(status='failed', finished=now())
        run.save()
        raise
    finally:
        try:
            generated = git('ls-files', '--others', '--exclude-standard', '--', 'game').decode().splitlines()
            run.report.setdefault('generated_uid_cleanup', []).extend(
                cleanup_generated_uids(ROOT, generated, existed, sources))
        except BaseException:
            run.report.update(status='failed', finished=now())
            raise
        finally:
            run.save()
    run.report.update(status='passed', finished=now())
    run.save()
    print('PASS: full current quality; frozen contracts, current product, four interior modes, six water/control modes and six cask/control modes and six hand-tool/control modes and seven Healer motion modes, seven foliage modes and seven wind modes and seven rain modes', flush=True)


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


def heating_runtime(run, environment):
    """Retain every heating check in its exact accepted private source tree."""
    with snapshot(HEATING_REVISION, editor=True) as (tree, env):
        run.stage('heating-historical-restore', ['dotnet', 'restore', 'tests/Application.Tests/Allrpcs.Application.Tests.csproj'], tree, env, HEATING_REVISION)
        run.stage('heating-historical-build', ['dotnet', 'build', 'game/Allrpcs.Game.csproj', '-m:1', '/nr:false'], tree, env, HEATING_REVISION)
        run.stage('heating-historical-import', ['godot', '--headless', '--editor', '--path', 'game', '--import'], tree, env, HEATING_REVISION, retry_native=True)
        frozen_heating_runtime(run, dict(environment, **env), tree)


def frozen_heating_runtime(run, environment, tree):
    """Check the live successor without replacing any retained stage."""
    run.stage('heating-focused', ['dotnet', 'test', 'tests/Application.Tests/Allrpcs.Application.Tests.csproj', '--no-restore', '--filter', 'FullyQualifiedName~RemedyHeating', '--logger', 'console;verbosity=minimal'], tree, environment, HEATING_REVISION)
    run.stage('boundary-heating-source', [sys.executable, 'scripts/validate-remedy-heating-runtime.py', '--source-only'], tree, environment, HEATING_REVISION)
    modes = ('default', 'quiet', 'master-muted', 'effects-muted', 'missing-stream', 'captions-off', 'heating-disabled')
    for mode in modes:
        output = run.directory / f'heating-{mode}.json'
        run.stage('heating-' + mode, ['godot', '--headless', '--fixed-fps', '60', '--path', 'game', '--quit-after', '1200'],
                  cwd=tree, revision=HEATING_REVISION, env=dict(environment, ALLRPCS_HEATING_MAIN_CHECK='1', ALLRPCS_HEATING_MODE=mode, ALLRPCS_HEATING_REPORT=str(output)),
                  retry_native=True, completion='PASS: heating Main:')
    reports = [json.loads((run.directory / f'heating-{mode}.json').read_text()) for mode in modes]
    saves = [(run.directory / f'heating-{mode}.json.save.json').read_bytes() for mode in modes]
    if any(data != saves[0] for data in saves) or any(row['motion'] != reports[0]['motion'] or row['truthBefore'] != row['truthAfter']
            or row['transcript'] != reports[0]['transcript'] for row in reports):
        raise ValueError('Heating changed complete motion/save/transcript/protected truth')
    if any(row['commits'] != 1 or row['presentations'] != 1 or row['grinding'] != 1 or row['treatment'] != 1 for row in reports[:-1]) or reports[-1]['presentations'] != 0:
        raise ValueError('Heating exact commit/presentation contract changed')
    run.report.update(heating_modes=list(modes), heating_save_sha256=sha(saves[0]))
    run.stage('heating-master-output', [sys.executable, 'scripts/validate-remedy-heating-runtime.py', '--output-only'], tree, environment, HEATING_REVISION)
    run.stage('boundary-heating-reviewed', [sys.executable, 'scripts/validate-remedy-heating-runtime.py', '--reviewed'], tree, environment, HEATING_REVISION)

def steeping_runtime(run, environment):
    """Preserve the complete steeping predecessor at its accepted revision."""
    with snapshot(STEEPING_REVISION, editor=True) as (tree, env):
        run.stage('steeping-historical-restore', ['dotnet', 'restore', 'tests/Application.Tests/Allrpcs.Application.Tests.csproj'], tree, env, STEEPING_REVISION)
        run.stage('steeping-historical-build', ['dotnet', 'build', 'game/Allrpcs.Game.csproj', '-m:1', '/nr:false'], tree, env, STEEPING_REVISION)
        run.stage('steeping-historical-import', ['godot', '--headless', '--editor', '--path', 'game', '--import'], tree, env, STEEPING_REVISION, retry_native=True)
        frozen_steeping_runtime(run, dict(environment, **env), tree)


def frozen_steeping_runtime(run, environment, tree):
    """Check the live successor without replacing any retained stage."""
    run.stage('steeping-focused', ['dotnet', 'test', 'tests/Application.Tests/Allrpcs.Application.Tests.csproj', '--no-restore', '--filter', 'FullyQualifiedName~RemedySteeping', '--logger', 'console;verbosity=minimal'], tree, environment, STEEPING_REVISION)
    run.stage('boundary-steeping-source', [sys.executable, 'scripts/validate-remedy-steeping-runtime.py', '--source-only'], tree, environment, STEEPING_REVISION)
    modes = ('default', 'quiet', 'master-muted', 'effects-muted', 'missing-stream', 'captions-off', 'steeping-disabled')
    for mode in modes:
        output = run.directory / f'steeping-{mode}.json'
        run.stage('steeping-' + mode, ['godot', '--headless', '--fixed-fps', '60', '--path', 'game', '--quit-after', '1200'],
                  cwd=tree, revision=STEEPING_REVISION, env=dict(environment, ALLRPCS_STEEPING_MAIN_CHECK='1', ALLRPCS_STEEPING_MODE=mode, ALLRPCS_STEEPING_REPORT=str(output)),
                  retry_native=True, completion='PASS: steeping Main:')
    reports = [json.loads((run.directory / f'steeping-{mode}.json').read_text()) for mode in modes]
    saves = [(run.directory / f'steeping-{mode}.json.save.json').read_bytes() for mode in modes]
    if any(data != saves[0] for data in saves) or any(row['motion'] != reports[0]['motion'] or row['truthBefore'] != row['truthAfter']
            or row['transcript'] != reports[0]['transcript'] for row in reports):
        raise ValueError('Steeping changed complete motion/save/transcript/protected truth')
    if any(row['commits'] != 1 or row['presentations'] != 1 or row['grinding'] != 1 or row['treatment'] != 1 or row['heating'] != 1 for row in reports[:-1]) or reports[-1]['presentations'] != 0:
        raise ValueError('Steeping exact commit/presentation contract changed')
    run.report.update(steeping_modes=list(modes), steeping_save_sha256=sha(saves[0]))
    run.stage('steeping-master-output', [sys.executable, 'scripts/validate-remedy-steeping-runtime.py', '--output-only'], tree, environment, STEEPING_REVISION)
    run.stage('boundary-steeping-reviewed', [sys.executable, 'scripts/validate-remedy-steeping-runtime.py', '--reviewed'], tree, environment, STEEPING_REVISION)


def cooling_runtime(run, environment):
    """Preserve all cooling predecessor stages at their accepted revision."""
    with snapshot(COOLING_REVISION, editor=True) as (tree, env):
        run.report['historical_cooling_retained_uids'] = seed_cooling_uids(tree)
        run.stage('cooling-historical-restore', ['dotnet', 'restore', 'tests/Application.Tests/Allrpcs.Application.Tests.csproj'], tree, env, COOLING_REVISION)
        run.stage('cooling-historical-build', ['dotnet', 'build', 'game/Allrpcs.Game.csproj', '-m:1', '/nr:false'], tree, env, COOLING_REVISION)
        run.stage('cooling-historical-import', ['godot', '--headless', '--editor', '--path', 'game', '--import'], tree, env, COOLING_REVISION, retry_native=True)
        frozen_cooling_runtime(run, dict(environment, **env), tree)


def seed_cooling_uids(tree, root=ROOT):
    """Supply only exact accepted sidecars missing from the cooling commit before cold import."""
    verification = json.loads((root / 'audio-direction/remedy-cooling-runtime-verification.json').read_text())
    rows = [row for row in verification['evidence_bindings'] if row['path'].endswith('.cs.uid')]
    if len(rows) != 4 or any(not row['path'].startswith('game/') for row in rows):
        raise ValueError('Unexpected retained cooling sidecar inventory')
    for row in rows:
        data = (root / row['path']).read_bytes()
        if sha(data) != row['sha256'] or not re.fullmatch(rb'uid://[a-z0-9]+\s*', data):
            raise ValueError('Accepted cooling sidecar changed: ' + row['path'])
        source = row['path'][:-4]
        if (tree / source).read_bytes() != (root / source).read_bytes():
            raise ValueError('Cooling UID source changed: ' + source)
    for row in rows:
        (tree / row['path']).write_bytes((root / row['path']).read_bytes())
    return rows


def resume_cooling_uid_prefix(prior, directory):
    """Reuse exact completed product stages only for the recorded missing-sidecar snapshot failure."""
    review = ROOT / 'audio-direction/herb-collection-runtime-review/quality/cold-cooling-uid-failure'
    original = json.loads((review / 'summary.json').read_text())
    if prior != original or prior.get('status') != 'failed' or prior.get('failed_stage') != 'boundary-cooling-reviewed':
        raise ValueError('Cooling UID resume requires the exact retained failure report')
    rows = prior['stages']
    expected = json.loads((ROOT / 'audio-direction/remedy-cooling-runtime-review/quality/summary.json').read_text())['stages']
    expected_names = [row['name'] for row in expected]
    slot = expected_names.index('cooling-focused')
    expected_names[slot:slot] = ['cooling-historical-restore', 'cooling-historical-build', 'cooling-historical-import']
    if [row['name'] for row in rows] != expected_names or len(rows) != 157:
        raise ValueError('Cooling UID resume stage inventory changed')
    for row in rows:
        for attempt in row['attempts']:
            if Path(attempt['log']).name != attempt['log'] or sha((directory / attempt['log']).read_bytes()) != attempt['sha256']:
                raise ValueError('Cooling UID resume log changed')
        if row is not rows[-1] and (row['attempts'][-1]['exit_code'] != 0 or not row['attempts'][-1]['completion_seen']):
            raise ValueError('Cooling UID resume prefix incomplete')
    failure = rows[-1]['attempts'][-1]
    if failure['exit_code'] != 1 or 'ValueError: Binding drift: game/RemedyCoolingAudioRuntimeSmokeCheck.cs.uid' not in (directory / failure['log']).read_text():
        raise ValueError('Cooling UID resume failure differs')
    qa = {row['path']: row for row in json.loads((review / 'qa-repair-inputs.json').read_text())}
    if prior['inputs']['quality.py'] != qa['quality.py']['previous_sha256'] or sha((ROOT / 'quality.py').read_bytes()) != qa['quality.py']['sha256']:
        raise ValueError('Cooling UID resume runner repair changed')
    for row in json.loads((review / 'product-inputs.json').read_text()):
        repair = qa.get(row['path'])
        expected_sha = repair['sha256'] if repair and repair['previous_sha256'] == row['sha256'] else row['sha256']
        if sha((ROOT / row['path']).read_bytes()) != expected_sha:
            raise ValueError('Cooling UID resume product changed: ' + row['path'])
    return rows[:143]


def finish_cooling_uids():
    """Rerun the affected cold cooling suite and new herb stages, retaining unchanged product passes."""
    directory = ROOT / 'artifacts/quality-current'
    prior_bytes = (directory / 'summary.json').read_bytes(); prior = json.loads(prior_bytes)
    reused = resume_cooling_uid_prefix(prior, directory)
    if prior['product_revision'] != git('rev-parse', 'HEAD').decode().strip() or prior['python'] != sys.version:
        raise ValueError('Cooling UID resume revision or interpreter changed')
    for name, digest in prior['inputs'].items():
        if name != 'quality.py' and sha((ROOT / name).read_bytes()) != digest:
            raise ValueError('Cooling UID resume quality input changed: ' + name)
    frozen_evidence()
    run = Run(directory, preserve_failed=True)
    metadata = {key: value for key, value in prior.items() if key not in
                ('status', 'stages', 'started', 'finished', 'failed_stage', 'resumed_from')}
    metadata['inputs'] = dict(prior['inputs'], **{'quality.py': sha((ROOT / 'quality.py').read_bytes())})
    run.report.update(metadata, stages=reused, resumed_from={'report_sha256': sha(prior_bytes),
        'prior_source_inputs': prior['inputs'], 'verified_completed_stages': len(reused),
        'reason': 'Exact unchanged product prefix retained after cooling snapshot generated missing accepted UID sidecars. Seed exact accepted bytes before import and rerun every cold cooling stage plus all herb stages.'})
    run.save()
    print(f'REUSE: {len(reused)} completed stages with exact source/log bindings; repeat cold cooling and finish herb', flush=True)
    try:
        environment = {'ALLRPCS_CHARACTER_MODEL_COMPOSITION_DISABLED': '1', 'ALLRPCS_ORDINARY_PARTICIPATION_SMOKE_MODEL': '1'}
        cooling_runtime(run, environment)
        herb_runtime(run, environment)
        run.report.update(status='passed', finished=now()); run.save()
        print('PASS: full current quality; verified unchanged product prefix, exact retained cooling UIDs and all herb stages', flush=True)
    except Exception:
        run.report.update(status='failed', finished=now()); run.save(); raise


def frozen_cooling_runtime(run, environment, tree):
    """Check the live successor without replacing any retained stage."""
    run.stage('cooling-focused', ['dotnet', 'test', 'tests/Application.Tests/Allrpcs.Application.Tests.csproj', '--no-restore', '--filter', 'FullyQualifiedName~RemedyCooling', '--logger', 'console;verbosity=minimal'], tree, environment, COOLING_REVISION)
    run.stage('boundary-cooling-source', [sys.executable, 'scripts/validate-remedy-cooling-runtime.py', '--source-only'], tree, environment, COOLING_REVISION)
    modes = ('default', 'quiet', 'master-muted', 'effects-muted', 'missing-stream', 'captions-off', 'cooling-disabled')
    for mode in modes:
        output = run.directory / f'cooling-{mode}.json'
        run.stage('cooling-' + mode, ['godot', '--headless', '--fixed-fps', '60', '--path', 'game', '--quit-after', '1200'],
                  cwd=tree, revision=COOLING_REVISION, env=dict(environment, ALLRPCS_COOLING_MAIN_CHECK='1', ALLRPCS_COOLING_MODE=mode, ALLRPCS_COOLING_REPORT=str(output)),
                  retry_native=True, completion='PASS: cooling Main:')
    reports = [json.loads((run.directory / f'cooling-{mode}.json').read_text()) for mode in modes]
    saves = [(run.directory / f'cooling-{mode}.json.save.json').read_bytes() for mode in modes]
    if any(data != saves[0] for data in saves) or any(row['motion'] != reports[0]['motion'] or row['truthBefore'] != row['truthAfter']
            or row['transcript'] != reports[0]['transcript'] for row in reports):
        raise ValueError('Cooling changed complete motion/save/transcript/protected truth')
    if any(row['commits'] != 1 or row['presentations'] != 1 or row['grinding'] != 1 or row['treatment'] != 1 or row['heating'] != 1 or row['steeping'] != 1 for row in reports[:-1]) or reports[-1]['presentations'] != 0:
        raise ValueError('Cooling exact commit/presentation contract changed')
    run.report.update(cooling_modes=list(modes), cooling_save_sha256=sha(saves[0]))
    run.stage('cooling-master-output', [sys.executable, 'scripts/validate-remedy-cooling-runtime.py', '--output-only'], tree, environment, COOLING_REVISION)
    run.stage('boundary-cooling-reviewed', [sys.executable, 'scripts/validate-remedy-cooling-runtime.py', '--reviewed'], tree, environment, COOLING_REVISION)


def herb_runtime(run, environment):
    """Retain every herb check in its exact accepted private source tree after input-only succession."""
    with snapshot(HERB_REVISION, editor=True) as (tree, env):
        run.stage('herb-historical-restore', ['dotnet', 'restore', 'tests/Application.Tests/Allrpcs.Application.Tests.csproj'], tree, env, HERB_REVISION)
        run.stage('herb-historical-build', ['dotnet', 'build', 'game/Allrpcs.Game.csproj', '-m:1', '/nr:false'], tree, env, HERB_REVISION)
        run.stage('herb-historical-import', ['godot', '--headless', '--editor', '--path', 'game', '--import'], tree, env, HERB_REVISION, retry_native=True)
        frozen_herb_runtime(run, dict(environment, **env), tree)


def frozen_herb_runtime(run, environment, tree):
    """Preserve the accepted source, mode schedule and native evidence without relaxing its validator."""
    run.stage('herb-focused', ['dotnet', 'test', 'tests/Application.Tests/Allrpcs.Application.Tests.csproj', '--no-restore', '--filter', 'FullyQualifiedName~HerbCollection', '--logger', 'console;verbosity=minimal'], tree, environment, HERB_REVISION)
    run.stage('boundary-herb-source', [sys.executable, 'scripts/validate-herb-collection-runtime.py', '--source-only'], tree, environment, HERB_REVISION)
    run.stage('herb-source-rejections', [sys.executable, 'scripts/prepare-herb-collection-passage.py', '--self-test'], tree, environment, HERB_REVISION)
    run.stage('herb-validator-rejections', [sys.executable, 'scripts/validate-herb-collection-runtime.py', '--self-test'], tree, environment, HERB_REVISION)
    modes = ('default', 'quiet', 'master-muted', 'effects-muted', 'missing-stream', 'captions-off', 'audio-disabled')
    from importlib.util import module_from_spec, spec_from_file_location
    spec = spec_from_file_location('herb_validator', tree / 'scripts/validate-herb-collection-runtime.py')
    validator = module_from_spec(spec); spec.loader.exec_module(validator)
    for placement in ('near', 'remote'):
        for mode in modes:
            output = run.directory / f'herb-{placement}-{mode}.json'
            run.stage(f'herb-{placement}-{mode}', ['godot', '--headless', '--fixed-fps', '60', '--path', 'game', '--quit-after', '1200'],
                      cwd=tree, revision=HERB_REVISION, env=dict(environment, ALLRPCS_HERB_MAIN_CHECK='1', ALLRPCS_HERB_MODE=mode,
                               ALLRPCS_HERB_PLACEMENT=placement, ALLRPCS_HERB_REPORT=str(output)),
                      retry_native=True, completion='PASS: herb Main:')
        rows = [json.loads((run.directory / f'herb-{placement}-{mode}.json').read_text()) for mode in modes]
        saves = [(run.directory / f'herb-{placement}-{mode}.json.save.json').read_bytes() for mode in modes]
        gameplay = [(run.directory / f'herb-{placement}-{mode}.json.gameplay.json').read_bytes() for mode in modes]
        validator.parity(rows, saves, gameplay, placement)
        run.report['herb_' + placement + '_save_sha256'] = sha(saves[0])
    run.report['herb_modes'] = list(modes)
    run.stage('herb-master-output', [sys.executable, 'scripts/validate-herb-collection-runtime.py', '--output-only'], tree, environment, HERB_REVISION)
    run.stage('boundary-herb-reviewed', [sys.executable, 'scripts/validate-herb-collection-runtime.py', '--reviewed'], tree, environment, HERB_REVISION)


def controls_runtime(run, environment):
    """Preserve the accepted controls validator and all eight stages in its published baseline."""
    if (ROOT / 'input-direction/dialogue-accessibility-runtime-authority.json').exists():
        with snapshot(CONTROLS_REVISION, editor=True) as (tree, env):
            run.stage('controls-historical-restore', ['dotnet', 'restore', 'Allrpcs.sln'], tree, env, CONTROLS_REVISION)
            run.stage('controls-historical-build', ['dotnet', 'build', 'game/Allrpcs.Game.csproj', '--no-restore'], tree, env, CONTROLS_REVISION)
            run.stage('controls-historical-import', ['godot', '--headless', '--editor', '--path', 'game', '--import'],
                      tree, env, CONTROLS_REVISION, retry_native=True)
            controls_runtime_checks(run, dict(environment, **env), tree, CONTROLS_REVISION)
    else:
        controls_runtime_checks(run, environment, ROOT, None)


def controls_runtime_checks(run, environment, tree, revision):
    """Keep the accepted stage names, commands, inputs and completion markers unchanged."""
    for label, project in (('application', 'Application'), ('infrastructure', 'Infrastructure')):
        run.stage('controls-' + label, ['dotnet', 'test', f'tests/{project}.Tests/Allrpcs.{project}.Tests.csproj',
                  '--no-restore', '--filter', 'FullyQualifiedName~Tests.Input', '--logger', 'console;verbosity=minimal'], tree, environment, revision)
    for name, flag in (('source', '--source-only'), ('validator-rejections', '--self-test')):
        run.stage('controls-' + name, [sys.executable, 'scripts/validate-control-remapping-runtime.py', flag], tree, environment, revision)
    report = run.directory / 'controls-main.json'
    run.stage('controls-main', ['godot', '--headless', '--fixed-fps', '60', '--path', 'game', '--quit-after', '1800'],
              tree, dict(environment, ALLRPCS_CONTROLS_MAIN_CHECK='1', ALLRPCS_CONTROLS_MODE='headless', ALLRPCS_CONTROLS_REPORT=str(report)),
              revision, retry_native=True, completion='PASS: controls Main: headless')
    for name, flag in (('native-output', '--output-only'), ('mounted-resources', '--package-only'), ('reviewed', '--reviewed')):
        run.stage('controls-' + name, [sys.executable, 'scripts/validate-control-remapping-runtime.py', flag], tree, environment, revision)


def dialogue_accessibility_runtime(run, environment):
    """Preserve accepted dialogue presentation checks in their unmodified published baseline."""
    if not (ROOT / 'input-direction/dialogue-accessibility-runtime-authority.json').exists():
        return
    if (ROOT / 'speech-direction/dialogue-integration/authority.json').exists():
        with snapshot(DIALOGUE_REVISION, editor=True) as (tree, env):
            run.stage('dialogue-accessibility-historical-restore', ['dotnet', 'restore', 'Allrpcs.sln'], tree, env, DIALOGUE_REVISION)
            run.stage('dialogue-accessibility-historical-build', ['dotnet', 'build', 'game/Allrpcs.Game.csproj', '--no-restore'], tree, env, DIALOGUE_REVISION)
            run.stage('dialogue-accessibility-historical-import', ['godot', '--headless', '--editor', '--path', 'game', '--import'],
                      tree, env, DIALOGUE_REVISION, retry_native=True)
            dialogue_accessibility_checks(run, dict(environment, **env), tree, DIALOGUE_REVISION)
    else:
        dialogue_accessibility_checks(run, environment, ROOT, None)


def dialogue_accessibility_checks(run, environment, tree, revision):
    """Retain all accepted command names, validator bytes and completion criteria."""
    for name, flag in (('source', '--source-only'), ('validator-rejections', '--self-test')):
        run.stage('dialogue-accessibility-' + name, [sys.executable, 'scripts/validate-dialogue-accessibility-runtime.py', flag], tree, environment, revision)
    report = run.directory / 'dialogue-accessibility-main.json'
    run.stage('dialogue-accessibility-main', ['godot', '--headless', '--fixed-fps', '60', '--path', 'game', '--quit-after', '1800'],
              cwd=tree, revision=revision, env=dict(environment, ALLRPCS_DIALOGUE_ACCESSIBILITY_CHECK='1', ALLRPCS_DIALOGUE_ACCESSIBILITY_MODE='headless',
                       ALLRPCS_DIALOGUE_ACCESSIBILITY_REPORT=str(report)), retry_native=True,
              completion='PASS: dialogue accessibility Main: headless')
    run.stage('dialogue-accessibility-reviewed', [sys.executable, 'scripts/validate-dialogue-accessibility-runtime.py', '--reviewed'], tree, environment, revision)


def dialogue_speech_runtime(run, environment):
    """Keep the closed seven-control reply-playback fixture in its accepted source snapshot."""
    run.stage('pocket-speech-worker-boundary', [sys.executable, 'scripts/test-pocket-speech-worker.py'])
    if (ROOT / 'speech-direction/player-asr-integration/authority.json').exists():
        with snapshot(DIALOGUE_SPEECH_REVISION, editor=True) as (tree, env):
            run.stage('dialogue-speech-historical-restore', ['dotnet', 'restore', 'Allrpcs.sln'], tree, env, DIALOGUE_SPEECH_REVISION)
            run.stage('dialogue-speech-historical-build', ['dotnet', 'build', 'game/Allrpcs.Game.csproj', '--no-restore'], tree, env, DIALOGUE_SPEECH_REVISION)
            run.stage('dialogue-speech-historical-import', ['godot', '--headless', '--editor', '--path', 'game', '--import'],
                      tree, env, DIALOGUE_SPEECH_REVISION, retry_native=True)
            dialogue_speech_checks(run, dict(environment, **env), tree, DIALOGUE_SPEECH_REVISION)
    else:
        dialogue_speech_checks(run, environment)


def dialogue_speech_checks(run, environment, tree=ROOT, revision=None):
    """Retain the exact completed reply/fallback modes and their completion criteria."""
    if not (ROOT / 'speech-direction/dialogue-integration/authority.json').exists():
        return
    with (tempfile.TemporaryDirectory(prefix='allrpcs-live-speech-') if revision is None else nullcontext(None)) as config_dir:
        config = str(Path(config_dir) / 'absent.json') if config_dir else '/private/tmp/allrpcs-speech-not-configured.json'
        current_environment = dict(environment, ALLRPCS_CHARACTER_MODEL_SETTINGS_TEST_STORAGE='1') if revision is None else environment
        for mode in ('headless', 'unavailable'):
            report = run.directory / ('dialogue-speech-' + mode + '.json')
            pacing = ['--max-fps', '60'] if revision is None else ['--fixed-fps', '60']
            run.stage('dialogue-speech-' + mode, ['godot', '--headless', *pacing, '--path', 'game', '--quit-after', '3600'],
                      cwd=tree, revision=revision, env=dict(current_environment, ALLRPCS_ORDINARY_PARTICIPATION_SMOKE_MODEL='0', ALLRPCS_DIALOGUE_SPEECH_CHECK='1', ALLRPCS_DIALOGUE_SPEECH_MODE=mode,
                               ALLRPCS_DIALOGUE_SPEECH_REPORT=str(report), ALLRPCS_SPEECH_CONFIG=config),
                      retry_native=True, completion='PASS: dialogue speech Main: ' + mode)


def player_asr_runtime(run, environment):
    """Qualify current optional input/draft/interruption without opening a physical microphone."""
    if not (ROOT / 'speech-direction/player-asr-integration/authority.json').exists():
        return
    run.stage('player-asr-capture-boundary', [sys.executable, 'scripts/test-player-speech-worker.py'])
    if (ROOT / 'speech-direction/space-hold-to-talk/authority.json').exists():
        with snapshot(PLAYER_ASR_REVISION, editor=True) as (tree, env):
            run.stage('player-asr-historical-restore', ['dotnet', 'restore', 'Allrpcs.sln'], tree, env, PLAYER_ASR_REVISION)
            run.stage('player-asr-historical-build', ['dotnet', 'build', 'game/Allrpcs.Game.csproj', '--no-restore'], tree, env, PLAYER_ASR_REVISION)
            run.stage('player-asr-historical-import', ['godot', '--headless', '--editor', '--path', 'game', '--import'], tree, env, PLAYER_ASR_REVISION, retry_native=True)
            player_asr_checks(run, dict(environment, **env), tree, PLAYER_ASR_REVISION)
    else:
        player_asr_checks(run, environment)


def player_asr_checks(run, environment, tree=ROOT, revision=None):
    """Preserve the accepted button-focused Space fixture in its original source tree."""
    for mode in ('headless', 'unavailable'):
        report = run.directory / ('player-asr-' + mode + '.json')
        run.stage('player-asr-' + mode, ['godot', '--headless', '--path', 'game', '--quit-after', '3600'],
                  cwd=tree, revision=revision, env=dict(environment, ALLRPCS_ORDINARY_PARTICIPATION_SMOKE_MODEL='0', ALLRPCS_PLAYER_ASR_CHECK='1',
                           ALLRPCS_PLAYER_ASR_MODE=mode, ALLRPCS_PLAYER_ASR_REPORT=str(report),
                           ALLRPCS_SPEECH_CONFIG='/private/tmp/allrpcs-speech-missing.json',
                           ALLRPCS_PLAYER_SPEECH_CONFIG='/private/tmp/allrpcs-asr-missing.json'),
                  retry_native=True, completion='PASS: player ASR Main: ' + mode)


def ollama_participation_runtime(run, environment):
    """Verify ordinary Ollama dialogue with isolated settings and synthetic HTTP, never a microphone."""
    for mode in ('respond', 'silence', 'failure'):
        report = run.directory / ('ollama-participation-' + mode + '.json')
        run.stage('ollama-participation-' + mode,
                  ['godot', '--headless', '--path', 'game', 'res://OllamaParticipationMainCheck.tscn'],
                  env=dict(environment, ALLRPCS_ORDINARY_PARTICIPATION_SMOKE_MODEL='0',
                           ALLRPCS_CHARACTER_MODEL_SETTINGS_TEST_STORAGE='1',
                           ALLRPCS_OLLAMA_PARTICIPATION_MODE=mode,
                           ALLRPCS_OLLAMA_PARTICIPATION_REPORT=str(report),
                           ALLRPCS_SPEECH_CONFIG='/private/tmp/allrpcs-speech-missing.json',
                           ALLRPCS_PLAYER_SPEECH_CONFIG='/private/tmp/allrpcs-asr-missing.json'),
                  retry_native=True, completion='PASS: Ollama participation Main: ' + mode)

    report = run.directory / 'dialogue-response-repair.json'
    run.stage('dialogue-response-repair',
              ['godot', '--headless', '--path', 'game', 'res://DialogueResponseRepairMainCheck.tscn'],
              env=dict(environment, ALLRPCS_ORDINARY_PARTICIPATION_SMOKE_MODEL='0',
                       ALLRPCS_CHARACTER_MODEL_SETTINGS_TEST_STORAGE='1',
                       ALLRPCS_RESPONSE_REPAIR_MODE='controlled', ALLRPCS_RESPONSE_REPAIR_REPORT=str(report),
                       ALLRPCS_SPEECH_CONFIG='/private/tmp/allrpcs-speech-missing.json',
                       ALLRPCS_PLAYER_SPEECH_CONFIG='/private/tmp/allrpcs-asr-missing.json'),
              retry_native=True, completion='PASS: dialogue response repair Main: controlled')
    result = json.loads(report.read_text())
    if result['status'] != 'passed' or len(result['turns']) != 8 or any(
            not turn['newResult'] or turn['replyCalls'] != 1 for turn in result['turns']):
        raise ValueError('Repeated response fixture did not produce eight distinct generations')
    log = run.directory / run.report['stages'][-1]['attempts'][-1]['log']
    if 'ObjectDB instances leaked' in log.read_text():
        raise ValueError('Repeated response fixture leaked native objects at exit')

    report = run.directory / 'herb-examination-action.json'
    run.stage('herb-examination-action',
              ['godot', '--headless', '--path', 'game', '--quit-after', '10800',
               'res://HerbExaminationActionMainCheck.tscn'],
              env=dict(environment, ALLRPCS_ORDINARY_PARTICIPATION_SMOKE_MODEL='0',
                       ALLRPCS_CHARACTER_MODEL_SETTINGS_TEST_STORAGE='1',
                       ALLRPCS_EXAMINATION_MODE='controlled', ALLRPCS_EXAMINATION_REPORT=str(report),
                       ALLRPCS_SPEECH_CONFIG='/private/tmp/allrpcs-speech-missing.json',
                       ALLRPCS_PLAYER_SPEECH_CONFIG='/private/tmp/allrpcs-asr-missing.json'),
              retry_native=True, completion='PASS: herb examination action Main: controlled')
    result = json.loads(report.read_text())
    if result['status'] != 'passed' or len(result['checks']) < 50:
        raise ValueError('Examination action fixture did not complete its ordinary Main cases')
    log = run.directory / run.report['stages'][-1]['attempts'][-1]['log']
    if 'ObjectDB instances leaked' in log.read_text():
        raise ValueError('Examination action fixture leaked native objects at exit')

    report = run.directory / 'conversation-context.json'
    run.stage('conversation-context',
              ['godot', '--headless', '--path', 'game', '--quit-after', '10800',
               'res://ConversationContextMainCheck.tscn'],
              env=dict(environment, ALLRPCS_ORDINARY_PARTICIPATION_SMOKE_MODEL='0',
                       ALLRPCS_CHARACTER_MODEL_SETTINGS_TEST_STORAGE='1',
                       ALLRPCS_CONVERSATION_MODE='controlled', ALLRPCS_CONVERSATION_REPORT=str(report),
                       ALLRPCS_SPEECH_CONFIG='/private/tmp/allrpcs-speech-missing.json',
                       ALLRPCS_PLAYER_SPEECH_CONFIG='/private/tmp/allrpcs-asr-missing.json'),
              retry_native=True, completion='PASS: conversation context Main: controlled')
    result = json.loads(report.read_text())
    if result['status'] != 'passed' or len(result['checks']) < 35:
        raise ValueError('Conversation context fixture did not complete its Main cases')
    log = run.directory / run.report['stages'][-1]['attempts'][-1]['log']
    if 'ObjectDB instances leaked' in log.read_text():
        raise ValueError('Conversation context fixture leaked native objects at exit')


def space_hold_runtime(run, environment):
    """Qualify the new tap/hold owner without opening physical microphone input."""
    if not (ROOT / 'speech-direction/space-hold-to-talk/authority.json').exists():
        return
    for mode in ('headless', 'unavailable'):
        report = run.directory / ('space-hold-' + mode + '.json')
        run.stage('space-hold-' + mode, ['godot', '--headless', '--path', 'game', '--quit-after', '7200'],
                  env=dict(environment, ALLRPCS_ORDINARY_PARTICIPATION_SMOKE_MODEL='0', ALLRPCS_SPACE_HOLD_CHECK='1',
                           ALLRPCS_SPACE_HOLD_MODE=mode, ALLRPCS_SPACE_HOLD_REPORT=str(report),
                           ALLRPCS_SPEECH_CONFIG='/private/tmp/allrpcs-speech-missing.json',
                           ALLRPCS_PLAYER_SPEECH_CONFIG='/private/tmp/allrpcs-asr-missing.json'),
                  retry_native=True, completion='PASS: Space hold Main: ' + mode)


def rain_runtime(run, environment):
    """Retain every rain stage in its unchanged baseline source tree."""
    with snapshot(RAIN_REVISION, editor=True) as (tree, env):
        run.stage('rain-historical-restore', ['dotnet', 'restore', 'tests/Application.Tests/Allrpcs.Application.Tests.csproj'], tree, env, RAIN_REVISION)
        run.stage('rain-historical-build', ['dotnet', 'build', 'game/Allrpcs.Game.csproj', '-m:1', '/nr:false'], tree, env, RAIN_REVISION)
        run.stage('rain-historical-import', ['godot', '--headless', '--editor', '--path', 'game', '--import'], tree, env, RAIN_REVISION, retry_native=True)
        frozen_rain_runtime(run, dict(environment, **env), tree)


def frozen_rain_runtime(run, environment, tree):
    """Retain the seven new ordinary movement modes plus measured native and bounded review guards."""
    modes = ('default', 'quiet', 'master-muted', 'ambience-muted', 'missing-stream', 'captions-off', 'rain-disabled')
    run.stage('rain-focused', ['dotnet', 'test', 'tests/Application.Tests/Allrpcs.Application.Tests.csproj',
                             '--no-restore', '--filter', 'FullyQualifiedName~Audio.Rain', '--logger', 'console;verbosity=minimal'], tree, environment, RAIN_REVISION)
    run.stage('boundary-rain-source', [sys.executable, 'scripts/validate-rain-runtime.py', '--source-only'], tree, environment, RAIN_REVISION)
    for mode in modes:
        output = run.directory / f'rain-{mode}.json'
        run.stage('rain-' + mode, ['godot', '--headless', '--fixed-fps', '60', '--path', 'game', '--quit-after', '7600'],
                  cwd=tree, revision=RAIN_REVISION, env=dict(environment, ALLRPCS_RAIN_MAIN_CHECK='1', ALLRPCS_RAIN_MODE=mode, ALLRPCS_RAIN_REPORT=str(output)),
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
    run.stage('rain-master-output', [sys.executable, 'scripts/validate-rain-runtime.py', '--output-only'], tree, environment, RAIN_REVISION)
    run.stage('boundary-rain-reviewed', [sys.executable, 'scripts/validate-rain-runtime.py', '--reviewed'], tree, environment, RAIN_REVISION)


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
    run = Run(directory, preserve_failed=True)
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
    run = Run(directory, preserve_failed=True)
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
    except BaseException:
        run.report.update(status='failed', finished=now()); run.save(); raise
    finally:
        try:
            generated = git('ls-files', '--others', '--exclude-standard', '--', 'game').decode().splitlines()
            run.report.setdefault('generated_uid_cleanup', []).extend(
                cleanup_generated_uids(ROOT, generated, existed, sources))
        except BaseException:
            run.report.update(status='failed', finished=now())
            raise
        finally:
            run.save()
    run.report.update(status='passed', finished=now())
    run.save()
    print('PASS: full current quality; verified unchanged product prefix, restored historical wind and all current rain stages', flush=True)


class ContractTests(unittest.TestCase):
    """Reject omitted checks and failed stages, and constrain the exceptional native retry."""

    @contextmanager
    def herb_export_fixture(self, manifest='herb-collection-runtime-verification.json',
                            retained_path='game/export_presets.cfg', corrupt_binding=False,
                            corrupt_predecessor=False):
        """Exercise the real guard with private evidence and an isolated Git read seam."""
        from unittest.mock import patch
        authority_path = 'audio-direction/material-runtime-review/publication/authority.json'
        authority = (ROOT / authority_path).read_bytes()
        predecessor = git('show', HERB_REVISION + ':game/export_presets.cfg')
        self.assertEqual(sha(predecessor), 'e10c14cfde8ed35b7ae80293c4fecd32309d2f4ddb8cdb74621faacad31d27ba')
        binding = '0' * 64 if corrupt_binding else sha(predecessor)
        with tempfile.TemporaryDirectory() as directory:
            repository = Path(directory)
            target = repository / retained_path
            target.parent.mkdir(parents=True)
            target.write_bytes(b'authorized current successor\n')
            authority_file = repository / authority_path
            authority_file.parent.mkdir(parents=True)
            authority_file.write_bytes(authority)
            (repository / 'audio-direction' / manifest).write_text(json.dumps({
                'evidence_bindings': [{'path': retained_path, 'sha256': binding}]}))

            def historical_read(*arguments):
                if arguments == ('show', AUDIO_REVISIONS[1][1] + ':' + authority_path):
                    return authority
                self.assertEqual(arguments, ('show', HERB_REVISION + ':game/export_presets.cfg'))
                return predecessor + (b'corrupt' if corrupt_predecessor else b'')

            with patch.object(sys.modules[__name__], 'ROOT', repository), \
                    patch.object(sys.modules[__name__], 'git', side_effect=historical_read) as read:
                yield read

    def test_herb_export_successor_preserves_pinned_predecessor(self):
        """Changed live presets pass only after the herb revision's exact bytes are read."""
        with self.herb_export_fixture() as read:
            frozen_evidence()
            self.assertIn(unittest.mock.call('show', HERB_REVISION + ':game/export_presets.cfg'),
                          read.call_args_list)

    def test_herb_export_corrupt_history_is_fatal(self):
        """Neither a changed binding nor changed historical bytes may authorize a successor."""
        message = 'Changed frozen herb predecessor evidence: game/export_presets.cfg'
        for corruption in ('corrupt_binding', 'corrupt_predecessor'):
            with self.subTest(corruption=corruption), self.herb_export_fixture(**{corruption: True}):
                with self.assertRaisesRegex(ValueError, message):
                    frozen_evidence()

    def test_herb_export_exception_does_not_admit_unrelated_drift(self):
        """The same manifest with another path, or same path with another manifest, stays exact."""
        for changes in ({'retained_path': 'game/retained.cfg'},
                        {'manifest': 'unrelated-runtime-verification.json'}):
            with self.subTest(changes=changes), self.herb_export_fixture(**changes):
                with self.assertRaisesRegex(ValueError, 'Changed retained evidence: '):
                    frozen_evidence()

    def test_final_cleanup_interruption_preserves_failure(self):
        """Drive both real entry points to final cleanup with product execution stubbed."""
        from contextlib import ExitStack
        from types import SimpleNamespace
        from unittest.mock import patch
        scope = sys.modules[__name__]
        for entry in ('main', 'finish_rain'):
            with self.subTest(entry=entry), tempfile.TemporaryDirectory() as directory:
                repository = Path(directory)
                output = repository / 'artifacts/quality-current'
                output.mkdir(parents=True)
                for name in ('quality.py', 'quality.sh', 'requirements-quality.txt',
                             'scripts/quality.sh', 'scripts/smoke-godot.sh'):
                    target = repository / name
                    target.parent.mkdir(parents=True, exist_ok=True)
                    target.write_bytes((ROOT / name).read_bytes())
                prior = dict(status='failed', failed_stage='product-10', stages=[], inputs={},
                             product_revision='fixture', python=sys.version)
                summary = output / 'summary.json'
                summary.write_text(json.dumps(prior))
                original = summary.read_bytes()
                log = output / 'original.log'
                log.write_bytes(b'original failed launch')
                for group, count, modes in (
                        ('interior', 0, ('normal', 'muted', 'zero', 'mono-reduced')),
                        ('water', 5, ('default', 'quiet', 'master-muted', 'effects-muted', 'missing-stream', 'no-action')),
                        ('cask', 7, ('default', 'quiet', 'master-muted', 'effects-muted', 'missing-stream', 'no-action')),
                        ('tool', 6, ('default', 'quiet', 'master-muted', 'effects-muted', 'missing-stream', 'no-action')),
                        ('healer', 4, ('default', 'quiet', 'master-muted', 'effects-muted', 'missing-resources',
                                       'captions-off', 'observer-disabled'))):
                    for mode in modes:
                        number = 0 if mode in ('no-action', 'observer-disabled') else count
                        (output / f'{group}-{mode}.json').write_text(json.dumps(dict(
                            transcript=[], commits=number, presentations=number, contacts=number,
                            motion=[], truthBefore={}, truthAfter={})))
                        (output / f'{group}-{mode}.json.save.json').write_bytes(b'same save')
                pillow = SimpleNamespace(__version__='12.3.0',
                    Image=SimpleNamespace(Image=SimpleNamespace(get_flattened_data=True)),
                    features=SimpleNamespace(check_feature=lambda _: False, version=lambda _: 'fixture'))
                with ExitStack() as stack:
                    stack.enter_context(patch.dict(sys.modules, PIL=pillow, numpy=SimpleNamespace(__version__='2.5.3')))
                    stack.enter_context(patch.object(scope, 'ROOT', repository))
                    stack.enter_context(patch.object(scope, 'git', side_effect=lambda *args:
                        b'fixture' if args[0] == 'rev-parse' else b''))
                    stack.enter_context(patch.object(scope, 'snapshot', side_effect=lambda *a, **k:
                        nullcontext((repository, {}))))
                    stack.enter_context(patch.object(scope, 'native_retry_shim', side_effect=lambda: nullcontext({})))
                    stack.enter_context(patch.object(Run, 'stage'))
                    for name in ('resume_prefix', 'resume_rain_prefix'):
                        stack.enter_context(patch.object(scope, name, return_value=[]))
                    for name in ('frozen_evidence', 'foliage_runtime', 'wind_runtime', 'rain_runtime',
                                 'heating_runtime', 'steeping_runtime', 'cooling_runtime', 'herb_runtime',
                                 'controls_runtime', 'dialogue_accessibility_runtime', 'dialogue_speech_runtime',
                                 'player_asr_runtime', 'space_hold_runtime', 'ollama_participation_runtime'):
                        stack.enter_context(patch.object(scope, name))
                    cleanup = stack.enter_context(patch.object(scope, 'cleanup_generated_uids',
                        side_effect=[[], KeyboardInterrupt()] if entry == 'main' else [KeyboardInterrupt()]))
                    with self.assertRaises(KeyboardInterrupt):
                        main(resume_smoke=True) if entry == 'main' else finish_rain()
                    self.assertEqual(cleanup.call_count, 2 if entry == 'main' else 1)
                self.assertEqual(summary.read_bytes(), original)
                self.assertEqual(log.read_bytes(), b'original failed launch')
                self.assertEqual(json.loads((output / 'summary.resume.json').read_text())['status'], 'failed')

    def test_fail_marker_forbids_native_retry(self):
        wrapper = 'FAIL: smoke launch exited 134: run_check 30 godot\n'
        for output in ('FAIL: fixture\nmutex lock failed',
                       'Godot Engine v4.6\nFAIL: fixture\nmutex lock failed\n' + wrapper):
            self.assertFalse(native_shutdown(134, output))
        # The harness's own exit report and a quarantined earlier launch's FAIL do not block retry.
        self.assertTrue(native_shutdown(134, 'FAIL: quarantined\nGodot Engine v4.6\nmutex lock failed\n' + wrapper))

    def test_resume_keeps_failed_report_until_success(self):
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            original = Run(root)
            with self.assertRaises(RuntimeError):
                original.stage('product-10', [sys.executable, '-c', 'raise SystemExit(134)'])
            before = (root / 'summary.json').read_bytes()
            old_log = root / original.report['stages'][0]['attempts'][0]['log']
            log_bytes = old_log.read_bytes()
            resumed = Run(root, preserve_failed=True)
            resumed.save()  # A preparation failure can happen before any stage starts.
            self.assertEqual((root / 'summary.json').read_bytes(), before)
            with self.assertRaises(RuntimeError):
                resumed.stage('product-10', [sys.executable, '-c', 'print("new"); raise SystemExit(7)'])
            self.assertEqual((root / 'summary.json').read_bytes(), before)
            self.assertEqual(old_log.read_bytes(), log_bytes)
            self.assertEqual(json.loads((root / 'summary.resume.json').read_text())['failed_stage'], 'product-10')
            resumed.report.update(status='passed')
            resumed.save()
            self.assertEqual(json.loads((root / 'summary.json').read_text())['status'], 'passed')

    def test_stage_timeout_cleans_descendants(self):
        import time
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            heartbeat = root / 'heartbeat'
            child = 'import time,signal; signal.signal(signal.SIGTERM, signal.SIG_IGN); from pathlib import Path; p=Path(' + repr(str(heartbeat)) + '); ' + \
                    '\nwhile True: p.write_text(str(time.monotonic())); time.sleep(0.02)'
            code = 'import subprocess,sys,time; subprocess.Popen([sys.executable,"-c",' + repr(child) + ']); time.sleep(60)'
            run = Run(root)
            started = time.monotonic()
            with self.assertRaises(RuntimeError):
                run.stage('hung', [sys.executable, '-c', code], timeout_seconds=0.5)
            self.assertLess(time.monotonic() - started, 5)
            report = json.loads((root / 'summary.json').read_text())
            self.assertEqual(report['failed_stage'], 'hung')
            self.assertTrue(report['stages'][0]['attempts'][0]['timed_out'])
            stamp = heartbeat.read_text()
            time.sleep(0.15)
            self.assertEqual(heartbeat.read_text(), stamp)

    def test_fresh_product_ten_resume(self):
        from types import SimpleNamespace
        from unittest.mock import patch
        with tempfile.TemporaryDirectory() as directory:
            repository = Path(directory)
            root = repository / 'artifacts/quality-current'
            root.mkdir(parents=True)
            for name in ('quality.py', 'quality.sh', 'requirements-quality.txt',
                         'scripts/quality.sh', 'scripts/smoke-godot.sh'):
                target = repository / name
                target.parent.mkdir(parents=True, exist_ok=True)
                target.write_bytes((ROOT / name).read_bytes())
            art, product = commands((ROOT / 'scripts/quality.sh').read_text())
            plan = [('continuation', [sys.executable, 'scripts/check-continuation.py', '--self-test'], None)]
            plan += [(f'art-{i}', [sys.executable, *args], ART_REVISION) for i, args in enumerate(art, 1)]
            plan += [('boundary-' + name, [sys.executable, validator] +
                      ([] if name in ('buses', 'material') else ['--reviewed']), rev)
                     for name, rev, validator in AUDIO_REVISIONS]
            boundaries = [('hand-tool', HAND_TOOL_REVISION, 'scripts/validate-hand-tool-runtime.py'),
                          ('healer', HEALER_REVISION, 'scripts/validate-healer-footstep-runtime.py'),
                          ('foliage', FOLIAGE_REVISION, 'scripts/validate-foliage-runtime.py')]
            plan += [('boundary-' + name + '-current', [sys.executable, script, '--reviewed'], rev)
                     for name, rev, script in boundaries]
            plan += [('current-art-imports', [sys.executable, 'scripts/validate-direction-a-import-proof.py'], None)]
            plan += [(f'product-{i}', ['sh', '-c', cmd], None) for i, cmd in enumerate(product[:9], 1)]
            plan += [('boundary-' + name + '-resume', [sys.executable, script, '--reviewed'], rev)
                     for name, rev, script in boundaries]
            plan += [('product-10', ['sh', '-c', product[9]], None)]
            rows = []
            for i, (name, args, revision) in enumerate(plan):
                data = (b'Godot Engine v4.6.3\nmutex lock failed: Invalid argument\n'
                        b'FAIL: smoke launch exited 134: run_check 30 godot\n') if name == 'product-10' else b'passed'
                log = f'{i}.log'
                (root / log).write_bytes(data)
                rows.append(dict(name=name, arguments=args, revision=revision,
                    environment_overrides={'ALLRPCS_CHARACTER_MODEL_COMPOSITION_DISABLED': '1',
                    'ALLRPCS_ORDINARY_PARTICIPATION_SMOKE_MODEL': '1'} if 'godot' in ' '.join(args) else {},
                    attempts=[dict(exit_code=134 if name == 'product-10' else 0, log=log, sha256=sha(data))]))
            metadata = dict(product_revision='fixture', python=sys.version, pillow='12.3.0', numpy='2.5.3',
                            zlib='fixture', inputs={name: sha((repository / name).read_bytes()) for name in
                            ('quality.py', 'quality.sh', 'requirements-quality.txt',
                             'scripts/quality.sh', 'scripts/smoke-godot.sh')})
            prior = dict(metadata, status='failed', failed_stage='product-10', stages=rows)
            self.assertEqual(resume_prefix(prior, root, art, product, metadata), rows[:-1])
            summary = root / 'summary.json'
            summary.write_text(json.dumps(prior))
            before = {path.name: path.read_bytes() for path in root.iterdir()}
            pillow = SimpleNamespace(__version__='12.3.0',
                Image=SimpleNamespace(Image=SimpleNamespace(get_flattened_data=True)),
                features=SimpleNamespace(check_feature=lambda _: False, version=lambda _: 'fixture'))
            scope = sys.modules[__name__]
            with patch.dict(sys.modules, PIL=pillow, numpy=SimpleNamespace(__version__='2.5.3')), \
                 patch.object(scope, 'ROOT', repository), \
                 patch.object(scope, 'git', side_effect=lambda *args: b'fixture' if args[0] == 'rev-parse' else b''), \
                 patch.object(scope, 'frozen_evidence', side_effect=ValueError('early resume probe')):
                with self.assertRaisesRegex(ValueError, 'early resume probe'):
                    main(resume_smoke=True)
            for name, data in before.items():
                self.assertEqual((root / name).read_bytes(), data)


    def test_live_speech_pacing_preserves_historical_commands(self):
        class SpeechRun:
            directory = Path('/tmp')
            def __init__(self):
                self.calls = []
            def stage(self, name, arguments, **kwargs):
                self.calls.append((name, arguments, kwargs))
                if kwargs['revision'] is None:
                    assert not Path(kwargs['env']['ALLRPCS_SPEECH_CONFIG']).exists()
        live = SpeechRun()
        historical = SpeechRun()
        dialogue_speech_checks(live, {})
        dialogue_speech_checks(historical, {}, ROOT, DIALOGUE_SPEECH_REVISION)
        self.assertEqual(len(live.calls), 2)
        for (_, current_args, current), (_, old_args, old) in zip(live.calls, historical.calls):
            self.assertIn('--max-fps', current_args)
            self.assertNotIn('--fixed-fps', current_args)
            self.assertEqual(current['env']['ALLRPCS_CHARACTER_MODEL_SETTINGS_TEST_STORAGE'], '1')
            self.assertNotIn('ALLRPCS_CHARACTER_MODEL_SETTINGS_TEST_STORAGE', old['env'])
            self.assertNotIn('--max-fps', old_args)
            self.assertEqual(old_args, ['godot', '--headless', '--fixed-fps', '60', '--path', 'game', '--quit-after', '3600'])
            self.assertEqual(old['env']['ALLRPCS_SPEECH_CONFIG'], '/private/tmp/allrpcs-speech-not-configured.json')
            self.assertNotEqual(current['env']['ALLRPCS_SPEECH_CONFIG'], old['env']['ALLRPCS_SPEECH_CONFIG'])

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

    def test_individual_native_retry_is_identical_and_fail_closed(self):
        """A frozen launch must really exit zero; test errors and exhausted shutdown retries remain fatal."""
        from types import SimpleNamespace
        from unittest.mock import patch
        shutdown = SimpleNamespace(returncode=-6, stdout=b'Godot Engine v4.6.3\nsmoke passed\nmutex lock failed')
        passed = SimpleNamespace(returncode=0, stdout=b'smoke passed\n')
        args = ['--headless', '--quit-after', '30']
        with patch.object(subprocess, 'run', side_effect=[shutdown, passed]) as run:
            self.assertEqual(native_launch('/actual/godot', args, '/original/path'), 0)
            self.assertEqual(run.call_args_list[0], run.call_args_list[1])
            self.assertEqual(run.call_args.kwargs['env']['PATH'], '/original/path')
            self.assertEqual(run.call_args.args[0], ['/actual/godot', *args])
        failed = SimpleNamespace(returncode=134, stdout=b'ERROR: fixture failed; mutex lock failed')
        with patch.object(subprocess, 'run', return_value=failed) as run:
            self.assertEqual(native_launch('/actual/godot', args, '/original/path'), 134)
            self.assertEqual(run.call_count, 1)
        with patch.object(subprocess, 'run', return_value=shutdown) as run:
            self.assertEqual(native_launch('/actual/godot', args, '/original/path'), 134)
            self.assertEqual(run.call_count, 3)

    def test_native_shim_restores_environment_on_failure(self):
        """The temporary launcher cannot change later stage resolution or survive an exception."""
        from unittest.mock import patch
        before = os.environ['PATH']
        with patch.object(shutil, 'which', return_value='/actual/godot'):
            with self.assertRaisesRegex(RuntimeError, 'probe'):
                with native_retry_shim() as audit:
                    wrapper = Path(os.environ['PATH'].split(os.pathsep)[0]) / 'godot'
                    self.assertTrue(wrapper.is_file())
                    self.assertEqual(audit['real_godot'], '/actual/godot')
                    self.assertEqual(sha(wrapper.read_bytes()), audit['wrapper_sha256'])
                    raise RuntimeError('probe')
        self.assertEqual(os.environ['PATH'], before)
        self.assertFalse(wrapper.exists())

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

    def test_cooling_uid_seed_preserves_exact_bytes(self):
        """Cold import receives all accepted IDs; tampered input fails before any snapshot write."""
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory) / 'source'; tree = Path(directory) / 'tree'
            (root / 'game').mkdir(parents=True); (tree / 'game').mkdir(parents=True)
            (root / 'audio-direction').mkdir()
            rows = []
            for i in range(4):
                name = f'game/Cooling{i}.cs'
                (root / name).write_text('source'); (tree / name).write_text('source')
                data = f'uid://accepted{i}\n'.encode()
                (root / (name + '.uid')).write_bytes(data)
                rows.append({'path': name + '.uid', 'sha256': sha(data)})
            (root / 'audio-direction/remedy-cooling-runtime-verification.json').write_text(json.dumps({'evidence_bindings': rows}))
            self.assertEqual(seed_cooling_uids(tree, root), rows)
            self.assertTrue(all((tree / row['path']).read_bytes() == (root / row['path']).read_bytes() for row in rows))
            (root / rows[2]['path']).write_text('uid://changed\n')
            for row in rows: (tree / row['path']).unlink()
            with self.assertRaises(ValueError): seed_cooling_uids(tree, root)
            self.assertTrue(all(not (tree / row['path']).exists() for row in rows))

    def test_cooling_uid_resume_refuses_other_failures(self):
        """Only the retained failed report and unchanged runtime inputs can resume the product prefix."""
        from unittest.mock import patch
        record = json.loads((ROOT / 'audio-direction/herb-collection-runtime-review/quality/cold-cooling-uid-failure/summary.json').read_text())
        for changed in (dict(record, status='passed'), dict(record, failed_stage='other')):
            with self.assertRaises(ValueError): resume_cooling_uid_prefix(changed, ROOT / 'artifacts/quality-current')
        read = Path.read_bytes
        source = ROOT / 'game/OpeningRegionHerbCollectionAudioController.cs'
        review = ROOT / 'audio-direction/herb-collection-runtime-review/quality/cold-cooling-uid-failure'
        # Materialize immutable retained logs in a private fixture; fresh clones have no local artifacts.
        import gzip
        with tempfile.TemporaryDirectory() as directory:
            fixture = Path(directory)
            for row in record['stages']:
                for attempt in row['attempts']:
                    (fixture / attempt['log']).write_bytes(gzip.decompress((review / (attempt['log'] + '.gz')).read_bytes()))
            # This regression targets product drift; use the immutable accepted runner bytes,
            # since the authorized controls successor now changes the live runner itself.
            accepted_runner = git('show', HERB_REVISION + ':quality.py')
            with patch.object(Path, 'read_bytes', lambda path: accepted_runner if path == ROOT / 'quality.py'
                              else read(path) + b'changed' if path == source else read(path)):
                with self.assertRaisesRegex(ValueError, 'product changed'):
                    resume_cooling_uid_prefix(record, fixture)

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
    parser.add_argument('--finish-cooling-uids', action='store_true')
    arguments = parser.parse_args()
    if arguments.self_test:
        unittest.main(argv=[sys.argv[0]])
    elif arguments.finish_foliage:
        finish_foliage()
    elif arguments.finish_cooling_uids:
        finish_cooling_uids()
    elif arguments.finish_rain:
        finish_rain()
    else:
        main(arguments.resume_smoke, arguments.finish_interior)
