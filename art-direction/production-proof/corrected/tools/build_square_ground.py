#!/usr/bin/env python3
"""Install only the named approved Square underlay and independent editable master."""

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
SCOPE = ROOT / 'art-direction/square-ground-runtime-scope'
PACKET = ROOT / 'art-direction/square-ground-runtime-promotion'
PRODUCTION = ROOT / 'art-direction/production-proof/corrected'


def sha(path):
    """Bind exact persisted source bytes."""
    return hashlib.sha256(path.read_bytes()).hexdigest()


def require(condition, message):
    """Fail before writes on scope, approval or source drift."""
    if not condition:
        raise ValueError(message)


def approved():
    """Require digest-bound named authorization and every retained selected proof input."""
    auth = json.loads((PACKET / 'authorization.json').read_text())
    for name, key in [('README.md', 'scope_sha256'), ('source-bindings.json', 'bindings_sha256'),
                      ('owner-approval.json', 'owner_approval_sha256')]:
        require(sha(SCOPE / name) == auth[key], 'Approved Square scope changed')
    require(sha(PACKET / 'baseline.json') == auth['baseline_sha256'], 'Implementation baseline changed')
    spec = json.loads((SCOPE / 'source-bindings.json').read_text())
    for name, expected in spec['source_sha256'].items():
        if name.startswith('art-direction/square-ground-feasibility/') and Path(name).name != '.DS_Store':
            require(sha(ROOT / name) == expected, 'Selected external proof changed: ' + name)
    return spec, auth


def png(image):
    """Encode deterministic editable planes."""
    stream = BytesIO()
    image.save(stream, format='PNG')
    return stream.getvalue()


def master(asset):
    """Preserve masked source, independent coverage/raw paint and empty Corrections at 2x."""
    size = (1120, 1120)
    with Image.open(ROOT / asset['source']) as source:
        require(source.mode == 'RGBA' and source.size == (560, 560), 'Source geometry changed')
        paint = source.copy().resize(size, Image.Resampling.NEAREST)
    with Image.open(ROOT / asset['world_paint']) as source:
        raw = source.convert('RGBA').resize(size, Image.Resampling.NEAREST)
    coverage = Image.merge('RGBA', (paint.getchannel('A'),) * 3 + (Image.new('L', size, 255),))
    xml = Element('image', w='1120', h='1120', name=asset['asset_id'], version='0.0.3')
    stack = SubElement(xml, 'stack')
    members = [('mimetype', b'image/openraster')]
    for index, (name, image, visible) in enumerate([
        ('Corrections', Image.new('RGBA', size), True), ('Accepted masked field', paint, True),
        ('Coverage editing reference', coverage, False), ('Unmasked world paint', raw, False),
    ]):
        path = f'data/layer-{index}.png'
        SubElement(stack, 'layer', name=name, src=path, x='0', y='0', opacity='1.0',
                   visibility='visible' if visible else 'hidden', **{'composite-op': 'svg:src-over'})
        members.append((path, png(image)))
    members.extend([('stack.xml', tostring(xml)), ('mergedimage.png', png(paint))])
    stream = BytesIO()
    with zipfile.ZipFile(stream, 'w') as archive:
        for name, data in members:
            archive.writestr(zipfile.ZipInfo(name, date_time=(2026, 10, 5, 0, 0, 0)), data)
    return stream.getvalue()


def expected():
    """Derive exact copies and an append-only manifest from the authorized predecessor."""
    spec, auth = approved()
    asset = spec['asset']
    source = ROOT / asset['source']
    require(sha(source) == asset['source_sha256'], 'Selected PNG source drift')
    original = subprocess.check_output(['git', 'show', auth['baseline_commit'] +
                                       ':art-direction/production-proof/corrected/asset-manifest.json'], cwd=ROOT)
    manifest = json.loads(original)
    require(len(manifest['assets']) == 91, 'Unexpected predecessor inventory')
    outputs = {asset[key]: source.read_bytes() for key in ('production_export', 'proof_texture', 'runtime_texture')}
    outputs[asset['production_master']] = master(asset)
    rect = dict(x=0, y=0, width=560, height=560)
    manifest['assets'].append(dict(id=asset['asset_id'], category='terrain',
                                  export='exports/terrain/village-square-ground.png',
                                  master_dimensions_pixels=dict(width=1120, height=1120, scale=2),
                                  logical_rect=rect, export_rect_pixels=rect, pivot=dict(x=0, y=0),
                                  contact_baseline=560, source='tools/build_square_ground.py',
                                  sha256=asset['source_sha256'], godot_import_proof=True))
    manifest['square_ground_provenance'] = dict(
        authorization='art-direction/square-ground-runtime-promotion/authorization.json',
        authorization_sha256=sha(PACKET / 'authorization.json'), specification_sha256=sha(SCOPE / 'source-bindings.json'),
        recipe_sha256=sha(Path(__file__)), world_origin=[1920, 620], paint_phase=[0, 0],
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
            path.parent.mkdir(parents=True, exist_ok=True)
            path.write_bytes(data)
    print('PASS: one exact Square underlay, isolated editable master and append-only manifest')


if __name__ == '__main__':
    main()
