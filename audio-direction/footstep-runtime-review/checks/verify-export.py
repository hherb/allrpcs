#!/usr/bin/env python3
"""Inspect an actual resource export and retain its audio-only entries unchanged."""

import hashlib
import json
from pathlib import Path
import re
import sys
from zipfile import ZipFile, ZIP_DEFLATED

ROOT = Path(__file__).resolve().parents[3]
REVIEW = ROOT / 'audio-direction/footstep-runtime-review'


def digest(data):
    """Hash exact exported bytes without interpretation."""
    return hashlib.sha256(data).hexdigest()


def main():
    """Require all 30 native streams and eight packaged notices to match local inputs."""
    package = Path(sys.argv[1])
    selected = {}
    streams = []
    notices = []
    with ZipFile(package) as archive:
        for folder, ids in [('footsteps', [f'W{m:02}-{v:02}' for m in range(1, 5) for v in range(1, 7)]),
                            ('material', ['B01', 'B02', 'B03', 'B04', 'A04', 'A05'])]:
            for clip in ids:
                source = f'assets/audio/{folder}/{clip}.wav'
                local = (ROOT / 'game' / (source + '.import')).read_text()
                imported = re.search(r'path="res://([^"]+)"', local)[1]
                packed_map = archive.read(source + '.import')
                assert imported in packed_map.decode()
                assert re.search(r'uid="([^"]+)"', local)[1] in packed_map.decode()
                cache = archive.read(imported)
                assert cache == (ROOT / 'game' / imported).read_bytes()
                selected[source + '.import'] = packed_map
                selected[imported] = cache
                streams.append({'source': source, 'imported_path': imported,
                                'cache_sha256': digest(cache), 'packed_uid_matches': True})
            for path in sorted((ROOT / 'game/assets/audio' / folder).iterdir()):
                if path.suffix not in ('.txt', '.json', '.html'):
                    continue
                name = path.relative_to(ROOT / 'game').as_posix()
                data = archive.read(name)
                assert data == path.read_bytes()
                selected[name] = data
                notices.append({'path': name, 'sha256': digest(data)})
    assert len(streams) == 30 and len(notices) == 8
    subset = REVIEW / 'audio-resource-subset.zip'
    with ZipFile(subset, 'w', ZIP_DEFLATED) as archive:
        for name, data in sorted(selected.items()):
            archive.writestr(name, data)
    inventory = {'schema_version': 1, 'resource_pack_sha256': digest(package.read_bytes()),
                 'footstep_streams': 24, 'retained_material_streams': 6, 'streams': streams,
                 'notices': notices, 'retained_subset_path': subset.relative_to(ROOT).as_posix(),
                 'retained_subset_sha256': digest(subset.read_bytes()), 'retained_subset_entries': len(selected),
                 'limit': 'Resource packaging only; no runnable application export or release claim.'}
    (REVIEW / 'export-inventory.json').write_text(json.dumps(inventory, indent=2) + '\n')
    print('PASS: 24 footstep streams, six retained streams, eight notices; 68 exact exported entries retained')


if __name__ == '__main__':
    main()
