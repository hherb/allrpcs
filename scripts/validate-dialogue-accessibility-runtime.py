#!/usr/bin/env python3
"""Validate the exact dialogue presentation grant, native evidence and honest reader limits."""
from __future__ import annotations
import argparse
import gzip
import hashlib
import json
from pathlib import Path
import struct
import subprocess
import unittest

ROOT = Path(__file__).resolve().parents[1]
REVIEW = ROOT / 'input-direction/dialogue-accessibility-runtime-review'
AUTHORITY = ROOT / 'input-direction/dialogue-accessibility-runtime-authority.json'
EVIDENCE = ROOT / 'input-direction/dialogue-accessibility-runtime-verification.json'
SLICE = 'phase11-dialogue-accessibility-runtime-integration'
PROVENANCE = 'synthetic keyboard events through actual Main; metadata inspection only; reader output not evaluated'
CHECKS = {p + ':' + c for p in ('default', 'remapped') for c in
          ('settings-cycle', 'sibling-return', 'dialogue-cycle', 'world-gate', 'submission-semantics', 'close-reopen')} | {'assistive-metadata', 'pointer-submit-and-close'}


def require(value, message):
    if not value:
        raise ValueError(message)


def sha(data):
    return hashlib.sha256(data).hexdigest()


def read(path):
    return json.loads(path.read_text())


def bound(row):
    path = Path(row['path'])
    require(not path.is_absolute() and '..' not in path.parts, 'Unsafe evidence path')
    data = (ROOT / path).read_bytes()
    require(sha(data) == row['sha256'], 'Stale evidence: ' + str(path))
    return data


def sources(evidence):
    grant = read(AUTHORITY)
    require(grant['slice_id'] == SLICE and grant['owner_statement'] ==
            'Read HANDOVER.md and follow the instructions - scope is approved', 'Exact grant missing')
    for row in grant['proposal_bindings']:
        bound(row)
    proposal = read(ROOT / 'input-direction/dialogue-accessibility-owner-scope.json')['runtime_proposal']
    require(grant['scope'] == proposal, 'Approved proposal changed')
    bound(grant['publication_authority'])
    require(sha((ROOT / 'game/project.godot').read_bytes()) == grant['preserved_editor_sha256'], 'Editor exception changed')
    metadata = {'README.md', 'ROADMAP.md', 'HANDOVER.md', 'CONTINUATION.json'}
    expected = set(proposal['new_files'] + proposal['modified_files'] + proposal['new_uid_sidecars']) - metadata
    expected.update(row['path'] for row in grant['verification_only_adjustments'])
    require({row['path'] for row in evidence['tested_inputs']} == expected, 'Tested source coverage incomplete')
    for row in evidence['tested_inputs']:
        bound(row)
    baseline = json.loads(gzip.decompress(bound(evidence['protected_baseline'])))
    allowed = set(proposal['modified_files']) | {row['path'] for row in grant['verification_only_adjustments']}
    for path, digest in baseline.items():
        if path in allowed or path.startswith('speech-direction/'):
            continue
        require(sha((ROOT / path).read_bytes()) == digest, 'Protected predecessor changed: ' + path)
    for row in grant['verification_only_adjustments']:
        raw = subprocess.check_output(['git', 'show', grant['baseline_commit'] + ':' + row['path']], cwd=ROOT)
        require(sha(raw) == row['sha256'], 'Verification predecessor changed')
    for row in proposal['protected_sources']:
        bound(row)
    require(evidence['screen_reader_certified'] is False and evidence['broad_ui_certified'] is False,
            'Unsupported accessibility claim')
    return grant


def product(report, mode):
    require(report['status'] == 'passed' and report['mode'] == mode, 'Incomplete product report')
    require(len(report['checks']) == len(CHECKS) and set(report['checks']) == CHECKS, 'Missing product check')
    require(report['provenance'] == PROVENANCE and report['providersDisabled'] is True, 'Wrong evidence provenance')
    require(len(report['focusTrace']) == 59, 'Incomplete keyboard focus trace')
    metadata = {row['node']: row for row in report['metadata']}
    require(set(metadata) == {'Input', 'Whispering', 'Submit', 'Close', 'Status', 'Response'}, 'Metadata coverage incomplete')
    require(all(row['name'] for row in metadata.values()) and all(not row['visible'] for row in metadata.values()),
            'Missing names or final hidden controls visible')
    require(metadata['Input']['labeledBy'] == ['../Prompt'] and metadata['Input']['describedBy'] == ['../Prompt', '../Status'],
            'Missing input relationships')
    require(metadata['Status']['live'] == 'Polite' and all(row['live'] == 'Off' for key, row in metadata.items() if key != 'Status'),
            'Wrong live region policy')


def reader(record):
    require(record['reader'] == 'VoiceOver' and record['os'] and record['engine'] and record['protocol'], 'Missing named-reader protocol')
    require(record['status'] == 'not_performed' and record['reason'] and record['reader_version'] == 'not observed',
            'Reader outcome requires actual session evidence')
    require(record['os_settings_changed'] is False and record['assistive_session_available'] is False, 'Reader scope exceeded')
    require(set(record['results']) == {'name_role_mode', 'focus_order', 'accepted_status', 'rejected_status',
            'final_response', 'focus_return', 'hidden_siblings'}, 'Missing reader evaluation item')
    require(all(value == 'not_performed' for value in record['results'].values()), 'Unobserved reader claim')


def outputs(evidence):
    require({row['mode'] for row in evidence['main_reports']} == {'headless', 'native-1280', 'native-1920'}
            and len(evidence['main_reports']) == 3, 'Missing native mode')
    for row in evidence['main_reports']:
        report = json.loads(bound(row['report']))
        product(report, row['mode'])
        log = gzip.decompress(bound(row['log']))
        require(sha(log) == row['raw_log_sha256'] and ('PASS: dialogue accessibility Main: ' + row['mode']).encode() in log,
                'Incomplete Main log')
        if row['mode'].startswith('native'):
            require(report['size'] == ([1280, 720] if row['mode'] == 'native-1280' else [1920, 1080]), 'Wrong native report size')
    states = set()
    for row in evidence['screenshots']:
        raw = bound(row)
        require(raw[:8] == b'\x89PNG\r\n\x1a\n', 'Not PNG')
        size = struct.unpack('>II', raw[16:24])
        require(list(size) == row['size'] and row['visually_reviewed'] is True, 'Native view not inspected')
        states.add((size, row['state']))
    require(len(evidence['screenshots']) == 6 and states == {(size, state) for size in ((1280, 720), (1920, 1080))
            for state in ('settings', 'dialogue-input', 'dialogue-close')}, 'Missing native view')
    require(len(bound(evidence['reader_protocol'])) > 0, 'Reader protocol missing')
    reader(json.loads(bound(evidence['reader_evaluation'])))


def complete(evidence, grant):
    result = evidence['quality']
    summary = json.loads(bound(result['summary']))
    require(summary['status'] == 'passed' and len(summary['stages']) == 195, 'Incomplete full quality')
    require(all(row['attempts'][-1]['exit_code'] == 0 and row['attempts'][-1]['completion_seen'] for row in summary['stages']),
            'Failed quality stage')
    logs = {row['original_name']: row for row in result['logs']}
    require(len(logs) == sum(len(row['attempts']) for row in summary['stages']), 'Missing quality log')
    for row in logs.values():
        require(sha(gzip.decompress(bound(row))) == row['raw_sha256'], 'Raw quality log changed')
    old = read(ROOT / 'input-direction/remapping-runtime-review/quality/summary.json')['stages']
    names = [row['name'] for row in summary['stages']]
    cursor = 0
    for row in old:
        cursor = names.index(row['name'], cursor) + 1
    for row in summary['stages']:
        if row['name'].startswith('controls-'):
            require(row['revision'] == grant['controls_revision'], 'Controls validator escaped immutable baseline')
    product(json.loads(bound(result['main_report'])), 'headless')


class Rejections(unittest.TestCase):
    """Mutations must fail closed for missing/stale evidence and exaggerated reader claims."""
    def test_valid_report(self):
        product(read(REVIEW / 'native/headless.json'), 'headless')

    def test_missing_check(self):
        report = read(REVIEW / 'native/headless.json'); report['checks'].pop()
        with self.assertRaises(ValueError): product(report, 'headless')

    def test_wrong_mode(self):
        with self.assertRaises(ValueError): product(read(REVIEW / 'native/headless.json'), 'native-1280')

    def test_wrong_provenance(self):
        report = read(REVIEW / 'native/headless.json'); report['provenance'] = 'screen reader output'
        with self.assertRaises(ValueError): product(report, 'headless')

    def test_wrong_live_policy(self):
        report = read(REVIEW / 'native/headless.json'); report['metadata'][4]['live'] = 'Assertive'
        with self.assertRaises(ValueError): product(report, 'headless')

    def test_missing_focus(self):
        report = read(REVIEW / 'native/headless.json'); report['focusTrace'].pop()
        with self.assertRaises(ValueError): product(report, 'headless')

    def test_unsafe_path(self):
        with self.assertRaises(ValueError): bound({'path': '../outside', 'sha256': '0' * 64})

    def test_stale_source(self):
        with self.assertRaises(ValueError): bound({'path': 'game/TemporaryDialogueInputPanel.cs', 'sha256': '0' * 64})

    def test_missing_file(self):
        with self.assertRaises(FileNotFoundError): bound({'path': 'input-direction/missing-evidence.json', 'sha256': '0' * 64})

    def test_reader_overclaim(self):
        report = read(REVIEW / 'reader-evaluation.json'); report['status'] = 'observed'
        with self.assertRaises(ValueError): reader(report)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    for name in ('source-only', 'reviewed', 'complete', 'self-test'):
        parser.add_argument('--' + name, action='store_true')
    args = parser.parse_args()
    if args.self_test:
        require(unittest.TextTestRunner(verbosity=2).run(unittest.defaultTestLoader.loadTestsFromTestCase(Rejections)).wasSuccessful(),
                'Rejection tests failed')
        return
    evidence = read(EVIDENCE)
    grant = sources(evidence)
    if args.reviewed or args.complete:
        outputs(evidence)
    if args.complete:
        complete(evidence, grant)
    print('PASS: finite approved dialogue accessibility presentation; native focus/metadata only; reader not performed')


if __name__ == '__main__':
    main()
