#!/usr/bin/env python3
"""Derive/install only the two scope-approved Garden underlays and isolated 2x masters."""

import argparse
import hashlib
from io import BytesIO
import json
from pathlib import Path
import zipfile
from xml.etree.ElementTree import Element, SubElement, tostring

from PIL import Image

ROOT = Path(__file__).resolve().parents[4]
PRODUCTION = ROOT / 'art-direction/production-proof/corrected'
SCOPE = ROOT / 'art-direction/garden-ground-runtime-scope'
PACKET = ROOT / 'art-direction/garden-ground-runtime-promotion'
SIZE = (512, 384)
MASTER_SIZE = (1024, 768)


def sha(path):
    """Bind exact source bytes rather than just decoded pixels."""
    return hashlib.sha256(path.read_bytes()).hexdigest()


def require(condition, message):
    """Fail before installing anything if a named source or contract has changed."""
    if not condition:
        raise ValueError(message)


def approved():
    """Load immutable approved scope and verify every selected paint/coverage input."""
    auth = json.loads((PACKET / 'authorization.json').read_text())
    for path, key in [('README.md', 'scope_sha256'), ('source-bindings.json', 'bindings_sha256')]:
        require(sha(SCOPE / path) == auth[key], 'Approved Garden scope changed')
    spec = json.loads((SCOPE / 'source-bindings.json').read_text())
    for name, digest in spec['source_sha256'].items():
        if name.startswith('art-direction/garden-ground-feasibility/'):
            require(sha(ROOT / name) == digest, 'Selected Garden proof changed: ' + name)
    return spec


def png(image):
    """Encode deterministic editable plane bytes."""
    stream = BytesIO()
    image.save(stream, format='PNG')
    return stream.getvalue()


def master(asset):
    """Store exact masked paint plus independent hidden coverage and empty Corrections."""
    source = ROOT / asset['source']
    require(sha(source) == asset['source_sha256'], 'Selected underlay bytes changed')
    with Image.open(source) as image:
        require(image.mode == 'RGBA' and image.size == SIZE, 'Selected underlay geometry changed')
        paint = image.copy().resize(MASTER_SIZE, Image.Resampling.NEAREST)
    xml = Element('image', w='1024', h='768', name=asset['asset_id'], version='0.0.3')
    stack = SubElement(xml, 'stack')
    coverage = Image.merge('RGBA', (paint.getchannel('A'),) * 3 + (Image.new('L', MASTER_SIZE, 255),))
    members = [('mimetype', b'image/openraster')]
    for index, (name, image, visible) in enumerate([
        ('Corrections', Image.new('RGBA', MASTER_SIZE), True),
        ('Coverage (editing reference)', coverage, False),
        ('Accepted masked field paint', paint, True),
    ]):
        path = f'data/layer-{index}.png'
        SubElement(stack, 'layer', name=name, src=path, x='0', y='0', opacity='1.0',
                   visibility='visible' if visible else 'hidden', **{'composite-op': 'svg:src-over'})
        members.append((path, png(image)))
    members.extend([('stack.xml', tostring(xml)), ('mergedimage.png', png(paint))])
    stream = BytesIO()
    with zipfile.ZipFile(stream, 'w') as archive:
        for name, data in members:
            item = zipfile.ZipInfo(name, date_time=(2026, 10, 4, 0, 0, 0))
            archive.writestr(item, data, compress_type=zipfile.ZIP_STORED)
    return stream.getvalue()


def expected():
    """Build expected bytes and append-only manifest without changing retained entries."""
    spec = approved()
    baseline = json.loads((PACKET / 'baseline.json').read_text())
    import subprocess
    original = subprocess.check_output(['git', 'show', baseline['commit'] + ':art-direction/production-proof/corrected/asset-manifest.json'], cwd=ROOT)
    manifest = json.loads(original)
    require(len(manifest['assets']) == 89, 'Unexpected Garden predecessor inventory')
    outputs = {}
    for asset in spec['assets']:
        source = ROOT / asset['source']
        for key in ('production_export', 'proof_texture', 'runtime_texture'):
            outputs[asset[key]] = source.read_bytes()
        outputs[asset['production_master']] = master(asset)
        rect = dict(x=0, y=0, width=512, height=384)
        manifest['assets'].append(dict(
            id=asset['asset_id'], category='terrain',
            export=str(Path(asset['production_export']).relative_to(PRODUCTION.relative_to(ROOT))),
            master_dimensions_pixels=dict(width=1024, height=768, scale=2),
            logical_rect=rect, export_rect_pixels=rect, pivot=dict(x=0, y=0),
            contact_baseline=384, source='tools/build_garden_ground.py',
            sha256=asset['source_sha256'], godot_import_proof=True))
    manifest['garden_ground_provenance'] = dict(
        authorization='art-direction/garden-ground-runtime-promotion/authorization.json',
        authorization_sha256=sha(PACKET / 'authorization.json'),
        specification_sha256=sha(SCOPE / 'source-bindings.json'),
        recipe_sha256=sha(Path(__file__)), world_origin=[2048, 1120], paint_phase=[0, 0],
        master_policy='nearest 2x masked paint; hidden independent coverage reference; empty Corrections')
    outputs['art-direction/production-proof/corrected/asset-manifest.json'] = (json.dumps(manifest, indent=2) + '\n').encode()
    return outputs


def main():
    """Install only named files, or reproduce every output read-only."""
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--check', action='store_true')
    args = parser.parse_args()
    outputs = expected()
    for name, data in outputs.items():
        path = ROOT / name
        if args.check:
            require(path.read_bytes() == data, 'Garden output drift: ' + name)
        else:
            path.parent.mkdir(parents=True, exist_ok=True)
            path.write_bytes(data)
    print('PASS: two exact Garden exports/copies, isolated masters and append-only manifest')


if __name__ == '__main__':
    main()
