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
)
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
    plan += [('boundary-interior-current', [sys.executable, 'scripts/validate-interior-audio-runtime.py', '--reviewed'], None),
             ('current-art-imports', [sys.executable, 'scripts/validate-direction-a-import-proof.py'], None)]
    count = 19 if finish_interior else 9
    plan += [(f'product-{index}', ['sh', '-c', command], None) for index, command in enumerate(product[:count], 1)]
    if finish_interior and prior.get('resumed_from'):
        plan.insert(26, ('boundary-interior-resume',
                        [sys.executable, 'scripts/validate-interior-audio-runtime.py', '--reviewed'], None))
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
    elif failed['arguments'] != ['sh', '-c', product[9]] or not native_shutdown(failed['attempts'][-1]['exit_code'], output):
        raise ValueError('Resume is limited to the diagnosed native smoke abort')
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
                          'reason': 'Complete unchanged checks reused after verified native classification or fixed-FPS invocation repair; current product boundary runs again.'})
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
            run.stage('boundary-interior-current', [sys.executable, 'scripts/validate-interior-audio-runtime.py', '--reviewed'])
            run.stage('current-art-imports', [sys.executable, 'scripts/validate-direction-a-import-proof.py'])
        else:
            print(f'REUSE: {len(reused)} completed stages with unchanged commands and verified logs', flush=True)
            run.stage('boundary-interior-resume', [sys.executable, 'scripts/validate-interior-audio-runtime.py', '--reviewed'])
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
        generated = git('ls-files', '--others', '--exclude-standard', '--', 'game').decode().splitlines()
        run.report['generated_uid_cleanup'] = cleanup_generated_uids(ROOT, generated, existed, sources)
        run.stage('boundary-interior-after-runtime', [sys.executable, 'scripts/validate-interior-audio-runtime.py', '--reviewed'])
        run.report.update(status='passed', finished=now(), interior_save_sha256=sha(saves[0]),
                          transcript=transcripts[0])
        run.save()
        print('PASS: full current quality; frozen contracts, current product and four-mode parity', flush=True)
    except Exception:
        run.report.update(status='failed', finished=now())
        run.save()
        raise
    finally:
        generated = git('ls-files', '--others', '--exclude-standard', '--', 'game').decode().splitlines()
        run.report.setdefault('generated_uid_cleanup', []).extend(
            cleanup_generated_uids(ROOT, generated, existed, sources))
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
    arguments = parser.parse_args()
    if arguments.self_test:
        unittest.main(argv=[sys.argv[0]])
    else:
        main(arguments.resume_smoke, arguments.finish_interior)
