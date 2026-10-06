#!/usr/bin/env python3
"""Copy the sixteen approved grouped modules and append their production inventory."""

import argparse
import hashlib
import json
from pathlib import Path
import subprocess

ROOT = Path(__file__).resolve().parents[4]
SCOPE = 'art-direction/woodland-clearing-dressing-runtime-scope/'
PACKET = 'art-direction/woodland-clearing-dressing-runtime-promotion/'
MANIFEST = 'art-direction/production-proof/corrected/asset-manifest.json'
STATUS = 'exact sixteen-placement grouped runtime scope approved'


def require(condition, message):
    """Reject input drift before writing any output."""
    if not condition:
        raise ValueError(message)


def sha(path):
    """Bind persisted bytes rather than image appearance alone."""
    return hashlib.sha256(path.read_bytes()).hexdigest()


def read(root, name):
    """Read one retained JSON input."""
    return json.loads((root / name).read_text())


def approved(root=ROOT):
    """Require exact owner authorization, frozen proposal and grouped selection."""
    auth = read(root, PACKET + 'authorization.json')
    for name, key in [('README.md', 'scope_sha256'), ('source-bindings.json', 'bindings_sha256'),
                      ('proposal-inputs.json', 'proposal_sha256'), ('owner-approval.json', 'owner_approval_sha256')]:
        require(sha(root / SCOPE / name) == auth[key], 'Approved scope drift: ' + name)
    require(auth['status'] == STATUS, 'Named approval missing')
    require(sha(root / PACKET / 'baseline.json') == auth['baseline_sha256'], 'Baseline drift')
    selection = 'art-direction/woodland-clearing-dressing/owner-selection.json'
    require(sha(root / selection) == auth['selection_sha256'], 'Grouped selection drift')
    spec = read(root, SCOPE + 'proposal-inputs.json')
    require(spec['selected_variant'] == 'grouped' and len(spec['assets']) == 16, 'Wrong variant or membership')
    for asset in spec['assets']:
        require(sha(root / asset['export']) == asset['sha256'], 'Approved PNG drift')
        require(sha(root / asset['source_master']) == asset['master_sha256'], 'Approved master drift')
    return spec, auth


def expected(root=ROOT):
    """Derive exact copies and append-only metadata from the published predecessor."""
    spec, auth = approved(root)
    original = subprocess.check_output(['git', 'show', auth['baseline_commit'] + ':' + MANIFEST], cwd=ROOT)
    manifest = json.loads(original)
    require(len(manifest['assets']) == 97, 'Unexpected predecessor inventory')
    outputs = {}
    for asset in spec['assets']:
        for key in ('production_export', 'proof_texture', 'runtime_texture'):
            outputs[asset[key]] = (root / asset['export']).read_bytes()
        outputs[asset['production_master']] = (root / asset['source_master']).read_bytes()
        width, height = asset['size']
        rect = dict(x=0, y=0, width=width, height=height)
        manifest['assets'].append(dict(
            id=asset['asset_id'], category=asset['category'],
            export=asset['production_export'].split('corrected/')[1],
            master_dimensions_pixels=dict(width=width * 2, height=height * 2, scale=2),
            logical_rect=rect, export_rect_pixels=rect,
            pivot=dict(zip(('x', 'y'), asset['proposed_pivot'])),
            contact_baseline=asset['proposed_pivot'][1], source='tools/build_woodland_clearing_dressing.py',
            sha256=asset['sha256'], godot_import_proof=True))
    manifest['woodland_clearing_dressing_provenance'] = dict(
        authorization=PACKET + 'authorization.json', authorization_sha256=sha(root / PACKET / 'authorization.json'),
        specification_sha256=sha(root / SCOPE / 'proposal-inputs.json'), recipe_sha256=sha(Path(__file__)),
        selected_variant='grouped', placements=[dict(asset_id=a['asset_id'], origin=a['origin'],
        world_pivot=a['world_pivot'], order=i + 1) for i, a in enumerate(spec['assets'])],
        master_policy='exact approved 2x OpenRaster bytes; all layers and merged paint retained')
    outputs[MANIFEST] = (json.dumps(manifest, indent=2) + '\n').encode()
    return outputs


def main():
    """Preflight every destination, then install; check mode performs no writes."""
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--check', action='store_true')
    args = parser.parse_args()
    outputs = expected()
    baseline = read(ROOT, PACKET + 'baseline.json')
    if not args.check:
        require(not (ROOT / PACKET / 'owner-runtime-acceptance.json').exists(), 'Accepted outputs are immutable')
    for name, data in outputs.items():
        path = ROOT / name
        if args.check:
            require(path.is_file() and path.read_bytes() == data, 'Installed output drift: ' + name)
        elif path.exists():
            require(path.read_bytes() == data or (name == MANIFEST and sha(path) == baseline['files'][name]),
                    'Refuse output overwrite: ' + name)
    if not args.check:
        for name, data in outputs.items():
            path = ROOT / name
            path.parent.mkdir(parents=True, exist_ok=True)
            path.write_bytes(data)
    print('PASS: sixteen unchanged grouped PNGs/masters and append-only 113-asset manifest')


if __name__ == '__main__':
    main()
