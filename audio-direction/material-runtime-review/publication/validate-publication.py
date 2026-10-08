#!/usr/bin/env python3
"""Validate the authorized publication and exact index without altering frozen evidence."""

import argparse
import hashlib
import json
from pathlib import Path
import subprocess

ROOT = Path(__file__).resolve().parents[3]
AUTHORITY = 'audio-direction/material-runtime-review/publication/authority.json'


def require(condition, message):
    """Reject unsafe publication regardless of Python optimization."""
    if not condition:
        raise ValueError(message)


def git(*args):
    """Read one Git object or inventory without changing repository state."""
    return subprocess.check_output(['git', *args], cwd=ROOT)


def main():
    """Check the actual owner request, retained evidence, file sizes and staged bytes."""
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--index', action='store_true', help='Verify the exact publication tree in the index')
    args = parser.parse_args()
    authority = json.loads((ROOT / AUTHORITY).read_text())
    require(authority['owner_statement'] == 'approved - commit and push', 'Missing explicit publication request')
    require(authority['branch'] == 'main', 'Wrong authorized destination')
    paths = {row['path'] for row in authority['publication_bindings']} | {AUTHORITY}
    excluded = authority['excluded_evidence_bindings']
    require(all(Path(row['path']).name == '.DS_Store' for row in excluded),
            'Only incidental Finder metadata may be excluded from frozen review bindings')
    excluded_rows = {row['path']: row for row in excluded}
    review = json.loads((ROOT / 'audio-direction/material-runtime-verification.json').read_text())
    recorded = {row['path']: row for row in review['reviewed_inputs'] + review['evidence_bindings']}
    require(all(recorded.get(path) == row for path, row in excluded_rows.items()), 'Excluded metadata differs')
    require(set(recorded) - set(excluded_rows) <= paths, 'Publication drops reviewed implementation/evidence')
    for path, row in recorded.items():
        if path not in excluded_rows:
            require(hashlib.sha256((ROOT / path).read_bytes()).hexdigest() == row['sha256'],
                    'Changed frozen review input: ' + path)
    require('game/project.godot' not in paths, 'Editor rewrite included in publication')
    require(not any('.DS_Store' in path or path.startswith('game/.godot/') for path in paths),
            'Unrelated editor/cache state included')
    for row in authority['publication_bindings']:
        path = Path(row['path'])
        require(not path.is_absolute() and '..' not in path.parts, 'Unsafe publication path')
        data = (ROOT / path).read_bytes()
        require(hashlib.sha256(data).hexdigest() == row['sha256'], 'Changed publication input: ' + str(path))
        require(len(data) < 100_000_000, 'Publication file exceeds GitHub size limit: ' + str(path))
        if args.index:
            require(git('show', ':' + str(path)) == data, 'Staged bytes differ: ' + str(path))
    preserved = authority['preserved_local_editor_rewrite']
    require(hashlib.sha256((ROOT / preserved['path']).read_bytes()).hexdigest() == preserved['sha256'],
            'Recorded editor rewrite changed')
    if args.index:
        require(git('show', ':' + AUTHORITY) == (ROOT / AUTHORITY).read_bytes(), 'Staged authority differs')
        changed = set(git('diff', '--cached', '--name-only', authority['baseline_commit']).decode().splitlines())
        require(changed == paths, 'Staged inventory mismatch: ' + str(sorted(changed ^ paths)))
        require(git('rev-parse', ':game/project.godot') ==
                git('rev-parse', authority['baseline_commit'] + ':game/project.godot'),
                'Staged editor rewrite differs from predecessor')
    subprocess.run(['python3', 'scripts/validate-material-audio-runtime.py'], cwd=ROOT, check=True)
    subprocess.run(['python3', 'scripts/check-continuation.py', '--self-test'], cwd=ROOT, check=True)
    subprocess.run(['git', 'diff', '--check'], cwd=ROOT, check=True)
    if args.index:
        subprocess.run(['git', 'diff', '--cached', '--check'], cwd=ROOT, check=True)
    print(f'PASS: {len(paths)} authorized publication paths; exact bindings; frozen evidence; editor rewrite excluded'
          + ('; staged bytes verified' if args.index else ''))


if __name__ == '__main__':
    main()
