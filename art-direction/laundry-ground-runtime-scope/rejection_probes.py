#!/usr/bin/env python3
"""Exercise proposal rejection paths in disposable metadata fixtures."""

from copy import deepcopy
import json
from pathlib import Path
import tempfile

import check


def main():
    """Reject altered authority, paint identity, geometry, sampling and missing bindings."""
    root = check.ROOT
    original = json.loads((root / 'source-bindings.json').read_text())
    probes = {
        'wrong owner position': lambda spec: spec['owner'].update(position=[1565, 219]),
        'scripted owner': lambda spec: spec['owner'].update(script='res://Unexpected.cs'),
        'wrong depth': lambda spec: spec['asset'].update(z_index=-90),
        'wrong dimensions': lambda spec: spec['asset'].update(dimensions=[92, 211]),
        'repeat enabled': lambda spec: spec['asset'].update(texture_repeat='enabled'),
        'wrong phase': lambda spec: spec.update(phase=[1, 0]),
        'wrong manifest count': lambda spec: spec['manifest_counts'].update(after_assets=97),
        'wrong selected source': lambda spec: spec['asset'].update(source=spec['asset']['source'].replace('24', '16')),
        'missing bound input': lambda spec: spec['source_sha256'].pop('game/Main.tscn'),
        'wrong baseline head': lambda spec: spec.update(prepared_against_commit='0' * 40),
    }
    results = []
    with tempfile.TemporaryDirectory(prefix='laundry-scope-rejections-') as directory:
        fixture = Path(directory)
        (fixture / 'baseline.json').write_bytes((root / 'baseline.json').read_bytes())
        (fixture / 'README.md').write_bytes((root / 'README.md').read_bytes())
        check.ROOT = fixture
        try:
            for name, mutate in probes.items():
                spec = deepcopy(original)
                mutate(spec)
                (fixture / 'source-bindings.json').write_text(json.dumps(spec))
                try:
                    check.main()
                except ValueError as error:
                    results.append({'probe': name, 'status': 'rejected', 'reason': str(error)})
                else:
                    raise AssertionError('Accepted invalid proposal: ' + name)
        finally:
            check.ROOT = root
    print(json.dumps({'status': 'passed', 'rejection_probes': results}, indent=2))


if __name__ == '__main__':
    main()
