#!/usr/bin/env python3
"""Install only the named approved VillageEdge underlay and independent editable master."""

import argparse
import hashlib
from io import BytesIO
import json
from pathlib import Path
import subprocess
import zipfile
from xml.etree.ElementTree import Element, SubElement, tostring

from PIL import Image

ROOT = Path(__file__).resolve().parents[4]
SCOPE = ROOT / 'art-direction/village-edge-ground-runtime-scope'
PACKET = ROOT / 'art-direction/village-edge-ground-runtime-promotion'
PRODUCTION = ROOT / 'art-direction/production-proof/corrected'


def sha(path):
    """Bind exact persisted source bytes."""
    return hashlib.sha256(path.read_bytes()).hexdigest()


def require(condition, message):
    """Fail before writes on scope, approval or source drift."""
    if not condition:
        raise ValueError(message)



def specification(root):
    """Project the approved immutable scope onto exact-byte installer/validator paths."""
    spec = json.loads((root / 'art-direction/village-edge-ground-runtime-scope/source-bindings.json').read_text())
    asset = spec['asset']
    for key in ('source', 'coverage', 'master'):
        asset[key] = 'art-direction/village-edge-ground-feasibility/' + asset[key]
    asset['world_paint'] = 'art-direction/village-edge-ground-feasibility/proof-inputs/world-paint.png'
    asset['source_sha256'] = spec['bound_files'][asset['source']]
    asset.update(production_export='art-direction/production-proof/corrected/exports/terrain/village-edge-entrance-ground.png',
                 production_master='art-direction/production-proof/corrected/masters/village-edge-entrance-ground.ora',
                 proof_texture='game/art/direction-a-import-proof/terrain/village-edge-entrance-ground.png',
                 runtime_texture='game/art/runtime/terrain/village-edge-entrance-ground.png')
    spec['source_sha256'] = spec['bound_files']
    spec['import_parameters'] = {'compress/mode': '0', 'mipmaps/generate': 'false', 'process/fix_alpha_border': 'false', 'process/premult_alpha': 'false', 'process/channel_remap/red': '0', 'process/channel_remap/green': '1', 'process/channel_remap/blue': '2', 'process/channel_remap/alpha': '3', 'process/size_limit': '0'}
    return spec

def approved():
    """Require digest-bound named authorization and every retained selected proof input."""
    auth = json.loads((PACKET / 'authorization.json').read_text())
    for name, key in [('README.md', 'scope_sha256'), ('source-bindings.json', 'bindings_sha256'),
                      ('owner-approval.json', 'owner_approval_sha256')]:
        require(sha(SCOPE / name) == auth[key], 'Approved VillageEdge scope changed')
    require(sha(PACKET / 'baseline.json') == auth['baseline_sha256'], 'Implementation baseline changed')
    require(auth['status'] == 'named one-asset VillageEdge runtime scope approved', 'Named authorization absent')
    spec = specification(ROOT)
    for name, expected in spec['source_sha256'].items():
        if name.startswith('art-direction/village-edge-ground-feasibility/') and Path(name).name != '.DS_Store':
            require(sha(ROOT / name) == expected, 'Selected external proof changed: ' + name)
    return spec, auth


def png(image):
    """Encode deterministic editable planes."""
    stream = BytesIO()
    image.save(stream, format='PNG')
    return stream.getvalue()


def master(asset):
    """Copy the selected editable master unchanged, as expressly required by scope."""
    source = ROOT / 'art-direction/village-edge-ground-feasibility/masters/feather-16.ora'
    acceptance = json.loads((source.parent.parent / 'owner-selection.json').read_text())
    require(sha(source) == acceptance['selected_files']['masters/feather-16.ora'], 'Selected master drift')
    return source.read_bytes()


def expected():
    """Derive exact copies and an append-only manifest from the authorized predecessor."""
    spec, auth = approved()
    asset = spec['asset']
    source = ROOT / asset['source']
    require(sha(source) == asset['source_sha256'], 'Selected PNG source drift')
    original = subprocess.check_output(['git', 'show', auth['baseline_commit'] +
                                       ':art-direction/production-proof/corrected/asset-manifest.json'], cwd=ROOT)
    manifest = json.loads(original)
    require(len(manifest['assets']) == 96, 'Unexpected predecessor inventory')
    outputs = {asset[key]: source.read_bytes() for key in ('production_export', 'proof_texture', 'runtime_texture')}
    outputs[asset['production_master']] = master(asset)
    rect = dict(x=0, y=0, width=500, height=390)
    manifest['assets'].append(dict(id=asset['asset_id'], category='terrain',
                                  export='exports/terrain/village-edge-entrance-ground.png',
                                  master_dimensions_pixels=dict(width=1000, height=780, scale=2),
                                  logical_rect=rect, export_rect_pixels=rect, pivot=dict(x=0, y=0),
                                  contact_baseline=390, source='tools/build_village_edge_ground.py',
                                  sha256=asset['source_sha256'], godot_import_proof=True))
    manifest['village_edge_ground_provenance'] = dict(
        authorization='art-direction/village-edge-ground-runtime-promotion/authorization.json',
        authorization_sha256=sha(PACKET / 'authorization.json'), specification_sha256=sha(SCOPE / 'source-bindings.json'),
        recipe_sha256=sha(Path(__file__)), world_origin=[1480, 620], paint_phase=[0, 0],
        master_policy='exact nearest2x masked paint; hidden coverage/raw paint; empty Corrections')
    outputs['art-direction/production-proof/corrected/asset-manifest.json'] = (json.dumps(manifest, indent=2) + '\n').encode()
    return outputs


def main():
    """Install only approved named outputs or check their identical bytes without writing."""
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--check', action='store_true')
    args = parser.parse_args()
    outputs = expected()
    for name, data in outputs.items():
        path = ROOT / name
        if args.check:
            require(path.is_file() and path.read_bytes() == data, 'Installed output drift: ' + name)
        else:
            require(not (PACKET / 'owner-runtime-acceptance.json').exists(), 'Accepted runtime outputs are immutable')
            if path.exists():
                original_manifest = name == 'art-direction/production-proof/corrected/asset-manifest.json'
                baseline = json.loads((PACKET / 'baseline.json').read_text())
                require(path.read_bytes() == data or (original_manifest and sha(path) == baseline['files'][name]), 'Refuse output overwrite: ' + name)
            path.parent.mkdir(parents=True, exist_ok=True)
            path.write_bytes(data)
    print('PASS: one exact VillageEdge underlay, isolated editable master and append-only manifest')


if __name__ == '__main__':
    main()
