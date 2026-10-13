#!/usr/bin/env python3
"""Verify the finite approved remapping slice and its retained synthetic evidence.

Historical audio/art evidence is immutable. This validator checks current input
sources against the new grant and preserves every other tracked baseline byte.
Native screenshots and mounted packs certify their stated synthetic/resource
scope only; they never certify a physical controller or executable installation.
"""
from __future__ import annotations

import argparse
import gzip
import hashlib
import io
import json
import re
from pathlib import Path
import struct
import subprocess
import unittest
import zipfile

ROOT = Path(__file__).resolve().parents[1]
REVIEW = ROOT / 'input-direction/remapping-runtime-review'
AUTHORITY = ROOT / 'input-direction/remapping-runtime-authority.json'
VERIFICATION = ROOT / 'input-direction/remapping-runtime-verification.json'
LIMITS = 'Synthetic events only; physical controller support and executable installation unverified.'


def require(condition, message):
    if not condition:
        raise ValueError(message)


def sha(data):
    return hashlib.sha256(data).hexdigest()


def read_json(path):
    return json.loads(path.read_text())


def bound(row):
    path = Path(row['path'])
    require(not path.is_absolute() and '..' not in path.parts, 'Unsafe evidence path')
    data = (ROOT / path).read_bytes()
    require(sha(data) == row['sha256'], 'Evidence drift: ' + str(path))
    return data


def authority():
    grant = read_json(AUTHORITY)
    require(grant['owner_statement'] == 'approved' and grant['slice_id'] ==
            'phase11-control-remapping-runtime-integration', 'Exact owner grant missing')
    for row in grant['proposal_bindings']:
        bound(row)
    proposal = read_json(ROOT / 'input-direction/remapping-owner-scope.json')['runtime_proposal']
    require(grant['scope'] == proposal, 'Grant differs from frozen proposal')
    require(sha((ROOT / 'game/project.godot').read_bytes()) == grant['preserved_editor_sha256'],
            'Unrelated editor rewrite changed')
    return grant


def sources(evidence, grant):
    for row in evidence['tested_inputs']:
        bound(row)
    scope = grant['scope']
    metadata = {'CONTINUATION.json', 'HANDOVER.md', 'README.md', 'ROADMAP.md'}
    expected = set(scope['new_files'] + scope['modified_files']) - metadata
    expected.update(row['path'] for row in grant['verification_only_adjustments'])
    expected.update(p + '.uid' for p in scope['new_files'] if p.startswith('game/') and p.endswith('.cs'))
    require(set(row['path'] for row in evidence['tested_inputs']) == expected, 'Source coverage incomplete')
    baseline = json.loads(gzip.decompress(bound(evidence['protected_baseline'])))
    allowed = set(scope['modified_files']) | {row['path'] for row in grant['verification_only_adjustments']}
    for path, digest in baseline.items():
        if path in allowed or path.startswith('speech-direction/'):
            continue
        require(sha((ROOT / path).read_bytes()) == digest, 'Protected baseline changed: ' + path)
    for row in grant['verification_only_adjustments']:
        data = subprocess.check_output(['git', 'show', grant['baseline_commit'] + ':' + row['path']], cwd=ROOT)
        require(sha(data) == row['baseline_sha256'], 'Verification baseline drift')
    require(evidence['actions'] == 29 and evidence['physical_controller_verified'] is False
            and evidence['executable_installation_verified'] is False and evidence['limits'] == LIMITS,
            'Unsupported completion/device claim')


def product_report(report, mode):
    require(report['status'] == 'passed' and report['mode'] == mode and report['actions'] == 29,
            'Incomplete Main report')
    require(len(report['checks']) == (9 if mode == 'headless' else 10), 'Missing Main checks')
    require(report['provenance'] == 'synthetic Godot input events and actual Main owners; no physical controller qualification',
            'Unsupported input provenance')


def outputs(evidence):
    require(len(evidence['main_reports']) == 3, 'Missing Main mode')
    reports = {row['mode']: row for row in evidence['main_reports']}
    require(set(reports) == {'headless', 'native-1280', 'native-1920'}, 'Missing Main mode')
    for mode, row in reports.items():
        product_report(json.loads(bound(row['report'])), mode)
        log = gzip.decompress(bound(row['log']))
        require(sha(log) == row['raw_log_sha256'] and ('PASS: controls Main: ' + mode).encode() in log,
                'Incomplete native log')
    require(len(evidence['screenshots']) == 6, 'Missing native view')
    states = set()
    for row in evidence['screenshots']:
        data = bound(row)
        require(data[:8] == b'\x89PNG\r\n\x1a\n', 'Not PNG')
        size = struct.unpack('>II', data[16:24])
        require(list(size) == row['size'] and size in {(1280, 720), (1920, 1080)}, 'Incorrect native size')
        require(row['visually_reviewed'] is True, 'Native UI not inspected')
        states.add((size, row['state']))
    require(states == {(size, state) for size in ((1280, 720), (1920, 1080))
                       for state in ('mixed-bindings', 'last-row', 'capture')}, 'Incomplete native UI coverage')


def packages(evidence):
    require(len(evidence['packages']) == 3, 'Missing mounted preset')
    baseline = read_json(ROOT / 'audio-direction/herb-collection-runtime-review/packaging/inventory.json')['entries']
    retained = {row['path']: row['sha256'] for row in baseline}
    require(len(retained) == 130, 'Audio inventory changed')
    manifest = json.loads(bound(evidence['package_manifest']))
    expected = {row['path']: row['sha256'] for row in manifest['new_files']}
    require(len(expected) == 10, 'Input resource coverage incomplete')
    for path, digest in expected.items():
        require(sha((ROOT / 'game' / path).read_bytes()) == digest, 'Mounted source now stale: ' + path)
    expected.update(retained)
    require({row['preset'] for row in evidence['packages']} ==
            {'Linux Development', 'macOS Development', 'Windows Development'}, 'Missing preset')
    with zipfile.ZipFile(io.BytesIO(bound(evidence['mounted_subset']))) as archive:
        retained_members = {name: sha(archive.read(name)) for name in archive.namelist()}
    require(set(retained_members) == set(expected), 'Mounted subset coverage incomplete')
    converted_hashes = None
    for row in evidence['packages']:
        report = json.loads(bound(row['report']))
        require(report['status'] == 'passed' and report['new_members'] == 10 and report['retained_audio'] == 130,
                'Incomplete mounted report')
        require(len(report['entries']) == 140 and
                {item['path']: item['sha256'] for item in report['entries']} == expected, 'Packed resource drift')
        require({item['path']: item['packed_sha256'] for item in report['entries']} == retained_members, 'Retained mounted bytes drift')
        converted = {item['path']: item['packed_sha256'] for item in report['entries'] if item['converted_scene']}
        require(set(converted) == {'Main.tscn', 'ControlSettingsPanel.tscn', 'ControlSettingsMainSmokeCheck.tscn'}, 'Missing converted scene')
        if converted_hashes is None: converted_hashes = converted
        require(converted == converted_hashes, 'Cross-preset scene conversion drift')
        for item in report['entries']:
            if not item['converted_scene']:
                require(item['packed_sha256'] == item['sha256'], 'Raw packed bytes differ')
        require(len(row['pack_sha256']) == 64, 'Pack hash missing')
        log = gzip.decompress(bound(row['mount_log']))
        require(sha(log) == row['raw_mount_log_sha256'] and b'PASS: controls mounted resources: 140' in log,
                'Mount did not complete')


def complete(evidence):
    result = evidence['quality']
    summary = json.loads(bound(result['summary']))
    require(summary['status'] == 'passed' and len(summary['stages']) == 188, 'Full quality incomplete')
    require(all(row['attempts'][-1]['exit_code'] == 0 and row['attempts'][-1]['completion_seen'] for row in summary['stages']), 'Quality stage failed')
    for row in result['logs']:
        raw = gzip.decompress(bound(row))
        require(sha(raw) == row['raw_sha256'], 'Quality raw log binding drift')
    baseline = read_json(ROOT / 'audio-direction/herb-collection-runtime-review/quality/summary.json')['stages']
    names = [row['name'] for row in summary['stages']]
    cursor = 0
    for row in baseline:
        cursor = names.index(row['name'], cursor) + 1
    # Derive the complete current suites, including separately owned additive tests.
    logs = {row['original_name']: row for row in result['logs']}
    total = 0
    for stage in summary['stages']:
        if stage['name'] not in {'product-5', 'product-6', 'product-7', 'product-8'}:
            continue
        raw = gzip.decompress(bound(logs[stage['attempts'][-1]['log']]))
        count = re.search(rb'Total:\s*(\d+)', raw)
        require(count is not None, 'Missing full unit suite count')
        total += int(count[1])
    require(total >= 5555 and result['unit_tests'] == total, 'Full unit count incomplete')


class Rejections(unittest.TestCase):
    """Reject incomplete, stale or overstated evidence rather than blessing metadata."""
    def setUp(self):
        self.report = {'status': 'passed', 'mode': 'headless', 'actions': 29, 'checks': list(range(9)),
                       'provenance': 'synthetic Godot input events and actual Main owners; no physical controller qualification'}

    def test_complete_report(self):
        product_report(self.report, 'headless')

    def test_missing_check(self):
        self.report['checks'].pop()
        with self.assertRaises(ValueError): product_report(self.report, 'headless')

    def test_wrong_action_count(self):
        self.report['actions'] = 28
        with self.assertRaises(ValueError): product_report(self.report, 'headless')

    def test_wrong_mode(self):
        with self.assertRaises(ValueError): product_report(self.report, 'native-1280')

    def test_forged_hardware_provenance(self):
        self.report['provenance'] = 'physical controller'
        with self.assertRaises(ValueError): product_report(self.report, 'headless')

    def test_incomplete_status(self):
        self.report['status'] = 'running'
        with self.assertRaises(ValueError): product_report(self.report, 'headless')

    def test_unsafe_path(self):
        with self.assertRaises(ValueError): bound({'path': '../outside', 'sha256': '0' * 64})

    def test_stale_source(self):
        with self.assertRaises(ValueError): bound({'path': 'game/ControlInputMatcher.cs', 'sha256': '0' * 64})


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    for name in ('source-only', 'output-only', 'package-only', 'reviewed', 'complete', 'self-test'):
        parser.add_argument('--' + name, action='store_true')
    args = parser.parse_args()
    if args.self_test:
        suite = unittest.defaultTestLoader.loadTestsFromTestCase(Rejections)
        require(unittest.TextTestRunner(verbosity=2).run(suite).wasSuccessful(), 'Rejection self-tests failed')
        return
    evidence = read_json(VERIFICATION)
    grant = authority()
    sources(evidence, grant)
    if args.reviewed or args.complete or args.output_only: outputs(evidence)
    if args.reviewed or args.complete or args.package_only: packages(evidence)
    if args.complete: complete(evidence)
    print('PASS: approved control remapping runtime; bounded source/evidence checks')


if __name__ == '__main__':
    main()
