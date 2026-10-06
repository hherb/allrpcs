#!/usr/bin/env python3
"""Validate the persistent continuation ledger and its exact handover projection."""

import argparse
import copy
import hashlib
import json
from pathlib import Path
import subprocess
import textwrap

REPO = Path(__file__).resolve().parents[1]
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
    """Use committed state to prevent pruning earlier closure entries."""
    tracked = subprocess.check_output(['git', 'ls-tree', 'HEAD', 'CONTINUATION.json'],
                                      cwd=REPO, text=True)
    if not tracked:
        return None
    result = subprocess.run(['git', 'show', 'HEAD:CONTINUATION.json'], cwd=REPO,
                            capture_output=True, text=True)
    require(result.returncode == 0, 'Cannot read committed continuation ledger')
    return json.loads(result.stdout)


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
    require(active['roadmap_item'] in (REPO / 'ROADMAP.md').read_text(), 'Missing roadmap item')
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


def self_test(state):
    """Exercise real regressions that caused completed work to be repeated."""
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
