#!/usr/bin/env python3
"""Check one exact grouped scope and selection without changing frozen predecessors."""

from copy import deepcopy
import hashlib
import json
from pathlib import Path
import subprocess

from PIL import Image

ROOT = Path(__file__).resolve().parent
REPO = ROOT.parents[1]
PROOF = REPO / 'art-direction/woodland-clearing-dressing'
ART = REPO / 'art-direction/woodland-environment-batch'
SELECTION = str((PROOF / 'owner-selection.json').relative_to(REPO))
DOCS = {'HANDOVER.md', 'README.md', 'ROADMAP.md'}


def require(value, message):
    """Fail on drift even when Python optimization is enabled."""
    if not value:
        raise ValueError(message)


def read(path):
    """Read authored metadata without executing it."""
    return json.loads(path.read_text())


def sha(path):
    """Bind exact persisted bytes."""
    return hashlib.sha256(path.read_bytes()).hexdigest()


def metadata(record):
    """Require the exact selected sixteen modules, native origins and draw order."""
    require(record['status'] == 'proposal only; named runtime approval absent'
            and record['selected_variant'] == 'grouped', 'Wrong scope state/variant')
    require(record['owner'] == {'name': 'WoodlandClearingDressingPresentation', 'type': 'Node2D',
                               'parent': '.', 'position': [0, 0], 'z_index': 0, 'scale': [1, 1],
                               'rotation': 0, 'script': None}, 'Owner changed')
    require(record['sprite_policy'] == {'z_index': -60, 'z_as_relative': True, 'centered': False,
            'offset': [0, 0], 'scale': [1, 1], 'rotation': 0, 'modulate': [1, 1, 1, 1],
            'region_enabled': False, 'filter': 'linear', 'repeat': 'disabled', 'script': None,
            'children': 0}, 'Sprite policy changed')
    require(record['root_order'] == 'after VillageEdgeGroundPresentation; before final WholeChannelPresentation',
            'Root order changed')
    require(record['counts'] == {'before': [97, 48], 'after': [113, 64]}, 'Inventory counts changed')
    require(record['runtime_confirmation'] == 'Eight actual imported-runtime grouped views and eight paired baselines matching selected external RGBA; separate owner confirmation',
            'Confirmation boundary changed')
    rows = read(PROOF / 'inventory.json')['variants']['grouped']
    require(len(record['assets']) == len(rows) == 16, 'Omitted/duplicated placement')
    for entry, row in zip(record['assets'], rows):
        require(all(entry.get(key) == value for key, value in row.items()), 'Placement/order changed')
        family = row['asset'].rsplit('-', 1)[0]
        category = 'objects' if family in ('fallen-wood', 'rocks') else 'plants'
        name = 'woodland-' + row['asset']
        expected = {'node_name': row['asset'].replace('-', '_'),
            'asset_id': category + '.' + name.replace('-', '_'), 'category': category,
            'source_master': str((ART / row['master']).relative_to(REPO)),
            'production_export': f'art-direction/production-proof/corrected/exports/{category}/{name}.png',
            'production_master': f'art-direction/production-proof/corrected/masters/{name}.ora',
            'proof_texture': f'game/art/direction-a-import-proof/{category}/{name}.png',
            'runtime_texture': f'game/art/runtime/{category}/{name}.png',
            'master_size': [row['size'][0] * 2, row['size'][1] * 2],
            'master_sha256': sha(ART / row['master'])}
        require(set(entry) == set(row) | set(expected) | {'native_alpha'}, 'Unexpected asset field')
        require(all(entry.get(key) == value for key, value in expected.items()), 'Asset target/pivot drift')
        with Image.open(REPO / row['export']) as image:
            require(image.mode == 'RGBA' and list(image.size) == row['size'], 'Source dimensions/mode drift')
            pixels = list(image.getdata())
            alpha = {'visible_bbox': list(image.getchannel('A').getbbox()),
                'hidden_rgb_pixels': sum(a == 0 and bool(r or g or b) for r, g, b, a in pixels),
                'opaque_pixels': sum(a == 255 for _, _, _, a in pixels)}
            require(entry['native_alpha'] == alpha, 'Bound native alpha facts changed')


def inspect():
    """Check selection, exact proposal inputs and all published predecessor Git objects."""
    bindings = read(ROOT / 'source-bindings.json')
    require(bindings['readme'] == sha(ROOT / 'README.md') and bindings['checker'] == sha(ROOT / 'check.py')
            and bindings['proposal'] == sha(ROOT / 'proposal-inputs.json'), 'Proposal source drift')
    record, baseline = read(ROOT / 'proposal-inputs.json'), read(ROOT / 'baseline.json')
    require(record['prepared_against_commit'] == bindings['head'] == baseline['head'], 'Head binding drift')
    tree = subprocess.check_output(['git', 'ls-tree', '-r', baseline['head']], cwd=REPO, text=True)
    blobs = {name: metadata.split()[-1] for metadata, name in (line.split('\t', 1) for line in tree.splitlines())}
    require(set(blobs) == set(baseline['files']), 'Incomplete predecessor inventory')
    for name, digest in baseline['files'].items():
        data = subprocess.check_output(['git', 'show', baseline['head'] + ':' + name], cwd=REPO) \
            if name in DOCS else (REPO / name).read_bytes()
        require(hashlib.sha256(data).hexdigest() == digest, 'Frozen predecessor changed: ' + name)
        require(hashlib.sha1(b'blob ' + str(len(data)).encode() + b'\0' + data).hexdigest() == blobs[name],
                'Predecessor differs from published Git object: ' + name)
    changed = subprocess.check_output(['git', 'diff', baseline['head'], '--name-only'], cwd=REPO, text=True).splitlines()
    changed += subprocess.check_output(['git', 'ls-files', '--others', '--exclude-standard'], cwd=REPO, text=True).splitlines()
    prefix = str(ROOT.relative_to(REPO)) + '/'
    require(all(name in DOCS or name == SELECTION or name.startswith(prefix) for name in changed),
            'Change outside selection/proposal scope')
    require(not any(name.endswith('.DS_Store') for name in changed), 'Finder metadata added')
    selection = read(PROOF / 'owner-selection.json')
    require(selection['selected_variant'] == 'grouped' and selection['placement_count'] == 16
            and selection['owner_statement'] == 'Actually - changed my mind - grouped'
            and selection['placements'] == read(PROOF / 'inventory.json')['variants']['grouped'], 'Selection changed')
    require(selection['published_comparison_head'] == baseline['head'], 'Selection head drift')
    for name, digest in {**selection['bound_files'], **record['bound_files']}.items():
        require(sha(REPO / name) == digest, 'Selected source/capture drift: ' + name)
    require(record['bound_files'][SELECTION] == sha(PROOF / 'owner-selection.json'), 'Unbound owner selection')
    metadata(record)
    return record, len(blobs) - len(DOCS)


def main():
    """Verify current proposal and reject seven independent scope mutations."""
    record, count = inspect()
    mutations = [('selected_variant', 'sparse'), ('owner', {'script': 'runtime.gd'}),
        ('sprite_policy', {'scale': [2, 2]}), ('sprite_policy', {'z_index': -90}),
        ('sprite_policy', {'filter': 'nearest'}), ('drop', None), ('reorder', None)]
    for key, value in mutations:
        candidate = deepcopy(record)
        if key == 'drop':
            candidate['assets'].pop()
        elif key == 'reorder':
            candidate['assets'][0], candidate['assets'][1] = candidate['assets'][1], candidate['assets'][0]
        elif isinstance(value, dict):
            candidate[key].update(value)
        else:
            candidate[key] = value
        try:
            metadata(candidate)
        except ValueError:
            continue
        raise ValueError('Mutation accepted: ' + key)
    print(f'PASS: exact grouped selection/scope; {count} byte-exact published predecessors')
    print('PASS: seven variant/authority/scale/depth/sampling/membership/order mutations rejected')


if __name__ == '__main__':
    main()
