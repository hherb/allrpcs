#!/usr/bin/env python3
"""Validate the persistent continuation ledger and its exact handover projection."""

import argparse
import copy
import hashlib
import json
from pathlib import Path
import subprocess
import textwrap
import tempfile

REPO = Path(__file__).resolve().parents[1]
# Accepted checkpoint: earlier legacy rewrites do not redefine this baseline.
# It must already be on main, so squash or rebase merges of later branches keep it as an ancestor.
# Descendant ledger commits and merge states contribute persistent entries.
# Older divergent branches do not resurrect superseded pre-baseline records.
HISTORY_BASELINE = 'e55463b0ad22dbb79487766877a0ddfa3384d662'
START = '<!-- active-slice:start -->'
END = '<!-- active-slice:end -->'


def require(value, message):
    """Reject invalid continuation state even with optimized Python."""
    if not value:
        raise ValueError(message)


def local_path(name):
    """Accept only repository-relative paths with no traversal or symlink escape."""
    path = Path(name)
    require(not path.is_absolute() and '..' not in path.parts, 'Unsafe evidence/output path')
    result = REPO / path
    require(result.resolve().is_relative_to(REPO), 'Evidence/output escapes repository')
    return result


def previous_state():
    """Union the pinned checkpoint and subsequent committed history; never trust HEAD alone."""
    def read_git(*args):
        result = subprocess.run(['git', *args], cwd=REPO, capture_output=True, text=True)
        require(result.returncode == 0,
                'Continuation history unavailable; fetch full history including ' + HISTORY_BASELINE)
        return result.stdout.strip()
    require(read_git('rev-parse', '--is-shallow-repository') == 'false',
            'Continuation validation requires full history; fetch --unshallow first')
    read_git('merge-base', '--is-ancestor', HISTORY_BASELINE, 'HEAD')
    commits = [HISTORY_BASELINE] + read_git('rev-list', '--reverse', '--full-history', '--ancestry-path',
        HISTORY_BASELINE + '..HEAD', '--', 'CONTINUATION.json').splitlines()
    retained = {key: {} for key in ('closed_scopes', 'closed_workstreams', 'reopenings')}
    for revision in commits:
        committed = json.loads(read_git('show', revision + ':CONTINUATION.json'))
        for key, entries in retained.items():
            for row in committed[key]:
                require(row['id'] not in entries or entries[row['id']] == row,
                        'Previously recorded closure rewritten in committed history: ' + row['id'])
                entries[row['id']] = row
    return {key: list(entries.values()) for key, entries in retained.items()}


def validate(state, previous=None, check_files=True):
    """Reject closed work, missing evidence, fabricated progress and lost closures."""
    require(state['schema_version'] == 1, 'Unknown continuation schema')
    closed = state['closed_scopes']
    ids = [row['id'] for row in closed]
    require(len(ids) == len(set(ids)), 'Duplicate closed scope')
    stopped = state['closed_workstreams']
    require(len(stopped) == len({row['id'] for row in stopped}), 'Duplicate closed workstream')
    reopenings = state['reopenings']
    require(len(reopenings) == len({row['id'] for row in reopenings}), 'Duplicate owner reopening')
    for row in reopenings:
        require(row['owner_statement'] and row['evidence'], 'Owner reopening lacks evidence')
        require(set(row['scope_ids']) <= set(ids), 'Owner reopening names an unknown closed scope')
        require(set(row['workstream_ids']) <= {item['id'] for item in stopped},
                'Owner reopening names an unknown closed workstream')
    active = state['active_slice']
    reopening_id = active.get('reopening_id')
    reopening = next((row for row in reopenings if row['id'] == reopening_id), None)
    require(reopening_id is None or reopening is not None, 'Unrecorded owner reopening')
    reopened_scopes = reopening['scope_ids'] if reopening else []
    reopened_streams = reopening['workstream_ids'] if reopening else []
    require(active['id'] not in ids or active['id'] in reopened_scopes, 'Active slice reopens a closed scope')
    require(active['workstream'] not in {row['id'] for row in stopped} or active['workstream'] in reopened_streams,
            'Active slice reopens a closed workstream under another name')
    require(active['status'] in ('ready', 'in_progress'), 'Active slice has no executable status')
    require(active['missing_deliverable'] and active['completion_condition'] and active['outputs'],
            'Active slice lacks a finite missing deliverable')
    require(isinstance(active['roadmap_item'], str) and active['roadmap_item'].startswith('- [ ] ')
            and active['roadmap_item'][6:].strip()
            and active['roadmap_item'] in (REPO / 'ROADMAP.md').read_text(), 'Missing roadmap item')
    require(len(active['outputs']) == len(set(active['outputs'])), 'Duplicate active output')
    outputs = [local_path(name) for name in active['outputs']]
    if check_files and active['status'] == 'ready':
        require(not all(path.exists() for path in outputs), 'Ready slice deliverables already exist')
    for row in closed + stopped + reopenings:
        require(row.get('reason', row.get('owner_statement')) and row['evidence'],
                'Closure or reopening lacks reason or evidence')
        for binding in row['evidence']:
            path = local_path(binding['path'])
            if check_files:
                require(path.is_file(), 'Missing closure evidence: ' + binding['path'])
                require(hashlib.sha256(path.read_bytes()).hexdigest() == binding['sha256'],
                        'Closure evidence changed: ' + binding['path'])
    if previous:
        preserve_history(state, previous)


def preserve_history(state, previous):
    """Require every retained entry verbatim, even after a pruning commit."""
    for key in ('closed_scopes', 'closed_workstreams', 'reopenings'):
        current = {row['id']: row for row in state[key]}
        for row in previous[key]:
            require(current.get(row['id']) == row, 'Previously recorded closure removed or rewritten')


def render_active(state):
    """Produce one exact active section so a second prose queue cannot drift."""
    active = state['active_slice']
    lines = [START, '## Next bounded slice', '',
             f"**{active['id']} — {active['title']}** ({active['status']}).", '',
             textwrap.fill('Roadmap item: ' + active['roadmap_item'], width=100), '',
             textwrap.fill('Missing deliverable: ' + active['missing_deliverable'], width=100), '',
             textwrap.fill(active['instructions'], width=100), '',
             textwrap.fill('Done when: ' + active['completion_condition'], width=100), '',
             'Expected outputs:', '']
    lines.extend('- `' + name + '`' for name in active['outputs'])
    lines.extend(['', textwrap.fill('Boundary: ' + active['boundary'], width=100), END])
    return '\n'.join(lines)


def check_handover(state, text):
    """Require exactly one active section and byte-exact agreement with the ledger."""
    require(text.count(START) == text.count(END) == 1, 'Missing or duplicate active handover section')
    start, end = text.index(START), text.index(END) + len(END)
    require(start < end and text[start:end] == render_active(state), 'Handover active slice differs from ledger')
    require(text.count('## Next bounded slice') == 1, 'Multiple next-slice queues in handover')


def history_self_test():
    """Use real commits, including a future closure pruned in a later commit."""
    global REPO, HISTORY_BASELINE
    original_repo = REPO
    original_baseline = globals().get('HISTORY_BASELINE')
    try:
        for key in ('closed_scopes', 'closed_workstreams', 'reopenings'):
            with tempfile.TemporaryDirectory() as directory:
                REPO = Path(directory)
                def git_fixture(*args):
                    return subprocess.check_output(['git', *args], cwd=REPO, text=True,
                                                   stderr=subprocess.DEVNULL).strip()
                git_fixture('init', '-q')
                git_fixture('config', 'user.name', 'Fixture')
                git_fixture('config', 'user.email', 'fixture@example.invalid')
                state = {name: [] for name in ('closed_scopes', 'closed_workstreams', 'reopenings')}
                def commit():
                    (REPO / 'CONTINUATION.json').write_text(json.dumps(state))
                    git_fixture('add', 'CONTINUATION.json')
                    git_fixture('commit', '-qm', 'fixture')
                state[key].append({'id': 'baseline', 'reason': 'legacy'})
                commit()
                git_fixture('branch', 'legacy')
                state[key][0]['reason'] = 'preserved'
                commit()
                HISTORY_BASELINE = git_fixture('rev-parse', 'HEAD')
                state[key].append({'id': 'future', 'reason': 'newly committed'})
                commit()
                require(previous_state()[key] == state[key], 'Preserved history must pass: ' + key)
                preserve_history(state, previous_state())
                state[key].pop()
                commit()
                try:
                    preserve_history(state, previous_state())
                except ValueError:
                    pass
                else:
                    raise ValueError('Committed pruning admitted: ' + key)
                require(any(row['id'] == 'future' for row in previous_state()[key]),
                        'Committed pruning lost future history: ' + key)
                state[key].clear()
                commit()
                require(any(row['id'] == 'baseline' for row in previous_state()[key]),
                        'Committed pruning lost baseline history: ' + key)
                # Older divergent ledger commits merged with the accepted state must not
                # resurrect pre-baseline rewrites. Only descendants and merge states count.
                branch = git_fixture('branch', '--show-current')
                git_fixture('checkout', '-q', 'legacy')
                state[key] = [{'id': 'baseline', 'reason': 'older divergent rewrite'}]
                commit()
                git_fixture('checkout', '-q', branch)
                git_fixture('merge', '-q', '-s', 'ours', 'legacy', '-m', 'retain accepted state')
                previous_state()
                (REPO / '.git/shallow').write_text(HISTORY_BASELINE + '\n')
                try:
                    previous_state()
                except ValueError as error:
                    require('full history' in str(error), 'Shallow history diagnostic missing')
                else:
                    raise ValueError('Shallow history silently admitted')
        print('PASS: temporary Git closure/workstream/reopening history probes')
    finally:
        REPO = original_repo
        HISTORY_BASELINE = original_baseline


def self_test(state):
    """Exercise real regressions that caused completed work to be repeated."""
    history_self_test()
    # Probes must still work after the real queue advances or gains owner reopening.
    state = copy.deepcopy(state)
    state['active_slice'].update(id='regression-unfinished-slice',
                                 workstream='regression-open-workstream', status='ready',
                                 outputs=['continuation-regression-missing-output'])
    state['active_slice'].pop('reopening_id', None)
    passed = []

    def reject(name, action):
        try:
            action()
        except ValueError:
            passed.append(name)
        else:
            raise ValueError('Regression probe admitted: ' + name)

    bad = copy.deepcopy(state)
    bad['active_slice']['id'] = state['closed_scopes'][-1]['id']
    reject('closed scope', lambda: validate(bad, check_files=False))
    bad = copy.deepcopy(state)
    bad['active_slice']['workstream'] = state['closed_workstreams'][0]['id']
    reject('renamed closed workstream', lambda: validate(bad, check_files=False))
    bad = copy.deepcopy(state)
    bad['closed_scopes'].pop()
    reject('pruned acceptance history', lambda: validate(bad, previous=state, check_files=False))
    for item in ('', '   ', 'a', '- [ ] '):
        bad = copy.deepcopy(state)
        bad['active_slice']['roadmap_item'] = item
        reject('empty roadmap item', lambda: validate(bad, check_files=False))
    bad = copy.deepcopy(state)
    bad['active_slice']['missing_deliverable'] = ''
    reject('no missing deliverable', lambda: validate(bad, check_files=False))
    bad = copy.deepcopy(state)
    bad['active_slice']['outputs'] = ['../escape']
    reject('outside output', lambda: validate(bad, check_files=False))
    bad = copy.deepcopy(state)
    bad['active_slice']['outputs'] = ['README.md', 'ROADMAP.md']
    reject('already existing deliverables', lambda: validate(bad))
    rendered = render_active(state)
    reject('stale handover successor', lambda: check_handover(state, rendered.replace(
        state['active_slice']['title'], 'Repeat closed-forest approach dressing feasibility')))
    reject('second active queue', lambda: check_handover(state, rendered + '\n## Next bounded slice\n'))
    bad = copy.deepcopy(state)
    bad['closed_scopes'][0]['evidence'][0]['sha256'] = '0' * 64
    reject('changed acceptance evidence', lambda: validate(bad))
    bad = copy.deepcopy(state)
    bad['active_slice']['reopening_id'] = 'unrecorded-request'
    reject('invented owner reopening', lambda: validate(bad, check_files=False))
    reopened = copy.deepcopy(state)
    reopened['reopenings'].append({'id': 'explicit-owner-request',
                                  'owner_statement': 'Please reopen this named scope.',
                                  'scope_ids': [state['closed_scopes'][-1]['id']],
                                  'workstream_ids': [state['closed_workstreams'][0]['id']],
                                  'evidence': state['closed_scopes'][-1]['evidence']})
    reopened['active_slice'].update(id=state['closed_scopes'][-1]['id'],
                                    workstream=state['closed_workstreams'][0]['id'],
                                    reopening_id='explicit-owner-request')
    validate(reopened, previous=state)
    passed.append('explicit evidenced reopening preserves old closure')
    print('PASS: continuation regression probes:', len(passed))


def main():
    """Validate by default; render only the marked section with explicit --sync."""
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--sync', action='store_true', help='Update only the marked handover section')
    parser.add_argument('--self-test', action='store_true')
    args = parser.parse_args()
    state = json.loads((REPO / 'CONTINUATION.json').read_text())
    validate(state, previous_state())
    path = REPO / 'HANDOVER.md'
    text = path.read_text()
    require(text.count(START) == text.count(END) == 1, 'Missing or duplicate handover markers')
    start, end = text.index(START), text.index(END) + len(END)
    require(start < end, 'Reversed handover markers')
    if args.sync:
        text = text[:start] + render_active(state) + text[end:]
        path.write_text(text)
    check_handover(state, text)
    if args.self_test:
        self_test(state)
    print('PASS: one unfinished roadmap slice; persistent closed scopes; handover matches ledger')


if __name__ == '__main__':
    main()
