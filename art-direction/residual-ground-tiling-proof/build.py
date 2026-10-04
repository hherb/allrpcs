#!/usr/bin/env python3
"""Compile native editable ground masters into an external repeat/connector proof."""

from io import BytesIO
from pathlib import Path
import hashlib
import json
import zipfile
from xml.etree.ElementTree import Element, SubElement, tostring

import numpy as np
from PIL import Image

ROOT = Path(__file__).resolve().parent
REPO = ROOT.parents[1]
APPROVED = REPO / 'art-direction/residual-ground-batch'
MASTER_SIZE = 256
EXPORT_SIZE = 128
OVERLAP = 64
INTERIOR_MARGIN = 24
FEATHER = 12
PIN = 2
MATERIALS = ['packed-earth', 'pale-gravel', 'mixed-meadow', 'dry-grass', 'garden-soil', 'damp-mud']
PAIRS = {'earth-gravel': ('packed-earth', 'pale-gravel'),
         'meadow-soil': ('mixed-meadow', 'garden-soil')}
KINDS = ['straight', 'outer', 'inset']


def digest(path):
    """Bind persisted bytes, including predecessor inputs."""
    return hashlib.sha256(path.read_bytes()).hexdigest()


def save_json(path, value):
    """Write readable deterministic metadata."""
    path.write_text(json.dumps(value, indent=2) + '\n')


def check_approved_inputs():
    """Refuse to derive candidates from drifted owner-approved artwork or crop metadata."""
    acceptance = json.loads((APPROVED / 'owner-acceptance.json').read_text())
    manifest_path = APPROVED / 'review-manifest.json'
    if acceptance['status'] != 'approved' or digest(manifest_path) != acceptance['review_manifest_sha256']:
        raise ValueError('Approved ground review packet changed')
    manifest = json.loads(manifest_path.read_text())
    for relative in ('inventory.json', 'sources/ground-surfaces.png'):
        if digest(APPROVED / relative) != manifest['files'][relative]:
            raise ValueError(f'Approved source/crop metadata changed: {relative}')
    for relative, sha256 in acceptance['approved_artwork'].items():
        if digest(APPROVED / relative) != sha256:
            raise ValueError(f'Approved artwork changed: {relative}')


def png_bytes(image):
    """Serialize one editable raster layer."""
    buffer = BytesIO()
    image.save(buffer, format='PNG')
    return buffer.getvalue()


def write_ora(path, name, layers, merged):
    """Write top-first OpenRaster layers with stable archive metadata."""
    xml = Element('image', w=str(MASTER_SIZE), h=str(MASTER_SIZE), name=name, version='0.0.3')
    stack = SubElement(xml, 'stack')
    members = [('mimetype', b'image/openraster')]
    for index, (title, image) in enumerate(layers):
        source = f'data/layer-{index}.png'
        SubElement(stack, 'layer', name=title, src=source, x='0', y='0', opacity='1.0',
                   visibility='visible', **{'composite-op': 'svg:src-over'})
        members.append((source, png_bytes(image)))
    members += [('stack.xml', tostring(xml)), ('mergedimage.png', png_bytes(merged))]
    with zipfile.ZipFile(path, 'w') as archive:
        for name, data in members:
            info = zipfile.ZipInfo(name, date_time=(2026, 10, 4, 0, 0, 0))
            archive.writestr(info, data, compress_type=zipfile.ZIP_STORED)


def material_source(index, metadata):
    """Read approved source paint; preserve its alpha on a sampled opaque underpaint."""
    asset = metadata['assets'][index]
    with Image.open(APPROVED / asset['source']) as sheet:
        cell = sheet.crop(asset['source_cell'])
        left, top, right, bottom = asset['source_crop_in_cell']
        box = (left + INTERIOR_MARGIN, top + INTERIOR_MARGIN,
               right - INTERIOR_MARGIN, bottom - INTERIOR_MARGIN)
        paint = cell.crop(box).convert('RGBA')
    alpha_min = paint.getchannel('A').getextrema()[0]
    if alpha_min < 240:
        raise ValueError('Interior contains background; choose a reviewed opaque material region')
    colour = tuple(np.median(np.asarray(paint)[:, :, :3], axis=(0, 1)).astype(int))
    opaque = Image.new('RGBA', paint.size, (*colour, 255))
    opaque.alpha_composite(paint)
    prepared = opaque.resize((MASTER_SIZE + OVERLAP,) * 2, Image.Resampling.LANCZOS)
    return prepared, {'approved_asset': asset['id'], 'source': asset['source'],
                      'cell': asset['source_cell'], 'interior_crop_in_cell': list(box),
                      'retained_alpha_min': alpha_min, 'opaque_underpaint_rgb': list(map(int, colour)),
                      'source_sha256': digest(APPROVED / asset['source'])}


def repeatable_field(source):
    """Overlap painted margins on a torus, then reconcile two-pixel connector bands."""
    size = MASTER_SIZE + OVERLAP
    ramp = np.ones(size)
    phase = (np.arange(OVERLAP) + 0.5) / OVERLAP
    ramp[:OVERLAP] = np.sin(phase * np.pi / 2) ** 2
    ramp[-OVERLAP:] = np.cos(phase * np.pi / 2) ** 2
    weights = ramp[:, None] * ramp[None, :]
    indices = (np.arange(size) - OVERLAP // 2) % MASTER_SIZE
    yy, xx = np.meshgrid(indices, indices, indexing='ij')
    total = np.zeros((MASTER_SIZE, MASTER_SIZE, 4), dtype=float)
    sums = np.zeros((MASTER_SIZE, MASTER_SIZE), dtype=float)
    np.add.at(total, (yy, xx), np.asarray(source, dtype=float) * weights[:, :, None])
    np.add.at(sums, (yy, xx), weights)
    pixels = np.rint(total / sums[:, :, None]).clip(0, 255).astype('uint8')
    # BOX export consumes two master pixels. Equal full bands establish exact export ports.
    for axis in (1, 0):
        values = np.swapaxes(pixels, axis, 0)
        edge = np.rint((values[:PIN].mean(axis=0) + values[-PIN:].mean(axis=0)) / 2).astype('uint8')
        values[:PIN] = edge
        values[-PIN:] = edge
    return Image.fromarray(pixels)


def connector_masks():
    """Author painted geometric masks with fixed midpoint ports and distinct corner polarity."""
    yy, xx = np.mgrid[:MASTER_SIZE, :MASTER_SIZE].astype(float)
    center = (MASTER_SIZE - 1) / 2
    profile = np.clip((center - np.arange(MASTER_SIZE) + FEATHER / 2) / FEATHER, 0, 1)
    profile = profile ** 2 * (3 - 2 * profile)
    # Restrained boundary waviness fades to zero at the connector bands.
    wobble = 4 * np.sin(2 * np.pi * yy / MASTER_SIZE) * np.sin(np.pi * xx / (MASTER_SIZE - 1))
    distances = {'straight': center - xx + wobble,
                 'outer': center - np.hypot(xx, yy),
                 'inset': np.hypot(MASTER_SIZE - 1 - xx, MASTER_SIZE - 1 - yy) - center}
    masks = {}
    for kind, distance in distances.items():
        value = np.clip((distance + FEATHER / 2) / FEATHER, 0, 1)
        value = value ** 2 * (3 - 2 * value)
        if kind == 'straight':
            value[:PIN] = profile
            value[-PIN:] = profile
            value[:, :PIN], value[:, -PIN:] = 1, 0
        elif kind == 'outer':
            value[:PIN] = profile
            value[:, :PIN] = profile[:, None]
            value[-PIN:], value[:, -PIN:] = 0, 0
        else:
            value[-PIN:] = profile
            value[:, -PIN:] = profile[:, None]
            value[:PIN], value[:, :PIN] = 1, 1
        masks[kind] = Image.fromarray(np.rint(value * 255).astype('uint8'))
    return masks


def composite(a, b, mask):
    """Combine the same world-aligned material paint through an editable coverage mask."""
    layer = a.copy()
    layer.putalpha(mask)
    result = b.copy()
    result.alpha_composite(layer)
    return result, layer


def export_image(master):
    """Downsample authored 2x masters once, preserving exact two-pixel connector bands."""
    return master.resize((EXPORT_SIZE,) * 2, Image.Resampling.BOX)


def main():
    """Build all twelve candidates and their full-series authoring evidence."""
    if (ROOT / 'owner-acceptance.json').exists():
        raise ValueError('Accepted package is frozen; author a separate revision')
    check_approved_inputs()
    for folder in ('sources', 'masters', 'exports', 'review', 'evidence'):
        (ROOT / folder).mkdir(exist_ok=True)
    metadata = json.loads((APPROVED / 'inventory.json').read_text())
    assets, fields, drafts = [], {}, {}
    empty = Image.new('RGBA', (MASTER_SIZE,) * 2)
    for index, name in enumerate(MATERIALS):
        source, provenance = material_source(index, metadata)
        source.save(ROOT / 'sources' / f'{name}-interior.png')
        raw = source.crop((OVERLAP // 2, OVERLAP // 2,
                           OVERLAP // 2 + MASTER_SIZE, OVERLAP // 2 + MASTER_SIZE))
        field = repeatable_field(source)
        fields[name], drafts[name] = field, export_image(raw)
        write_ora(ROOT / 'masters' / f'{name}.ora', name,
                  [('Corrections', empty), ('Periodic paint', field), ('Source interior', raw)], field)
        export_image(field).save(ROOT / 'exports' / f'{name}.png')
        assets.append({'id': name, 'family': 'surfaces', 'provenance': provenance})
    masks = connector_masks()
    for kind, mask in masks.items():
        mask.save(ROOT / 'sources' / f'{kind}-mask.png')
    for family, (a, b) in PAIRS.items():
        for kind in KINDS:
            name = f'{family}-{kind}'
            merged, layer = composite(fields[a], fields[b], masks[kind])
            write_ora(ROOT / 'masters' / f'{name}.ora', name,
                      [('Corrections', empty), (f'{a} coverage', layer), (f'{b} underpaint', fields[b])], merged)
            export_image(merged).save(ROOT / 'exports' / f'{name}.png')
            assets.append({'id': name, 'family': family, 'materials': [a, b],
                           'mask': f'sources/{kind}-mask.png'})
    for asset in assets:
        name = asset['id']
        asset.update({'master': f'masters/{name}.ora', 'export': f'exports/{name}.png',
                      'master_size': [MASTER_SIZE] * 2, 'export_size': [EXPORT_SIZE] * 2,
                      'master_sha256': digest(ROOT / f'masters/{name}.ora'),
                      'export_sha256': digest(ROOT / f'exports/{name}.png')})
    from boards import build_boards
    placements = build_boards(fields, masks, drafts)
    predecessor_paths = set()
    for folder in ('residual-ground-batch', 'residual-ground-readiness'):
        base = REPO / 'art-direction' / folder
        predecessor_paths.update(p for p in base.rglob('*') if p.is_file() and '__pycache__' not in p.parts
                                 and p.name != '.DS_Store')
    manifest = REPO / 'art-direction/production-proof/corrected/asset-manifest.json'
    predecessor_paths.add(manifest)
    production = json.loads(manifest.read_text())
    for asset in production['assets']:
        if asset['id'].startswith('terrain.'):
            predecessor_paths.add(manifest.parent / asset['export'])
    save_json(ROOT / 'inventory.json', {
        'status': 'complete external feasibility candidates; collective owner review pending',
        'authoring': 'native OpenRaster material compilation from approved image_gen paint; no new generation',
        'count': 12, 'assets': assets, 'review_placements': placements,
        'geometry': {'master_size': MASTER_SIZE, 'export_size': EXPORT_SIZE, 'overlap_master_pixels': OVERLAP,
                     'feather_master_pixels': FEATHER, 'connector_band_master_pixels': PIN,
                     'export_filter': 'BOX', 'orientation': 'A left; outer A upper-left; inset B lower-right'},
        'limitations': ['Owner review pending', 'Orientation samples rotate coverage, never material paint',
                       'Finite 128px repeat remains visible; no variation overlays in scope',
                       'No runtime imports, physical authority, geography or accessibility acceptance'],
        'predecessors': {str(p.relative_to(REPO)): digest(p) for p in sorted(predecessor_paths)},
    })
    tracked = [p for folder in ('sources', 'masters', 'exports', 'review', 'evidence')
               for p in (ROOT / folder).glob('*') if p.is_file()]
    tracked += [ROOT / name for name in ('build.py', 'boards.py', 'authoring_check.py', 'inspect_package.py',
                                        'README.md', 'inventory.json')]
    save_json(ROOT / 'review-manifest.json', {'status': 'owner review pending',
              'files': {str(p.relative_to(ROOT)): digest(p) for p in sorted(tracked)}})


if __name__ == '__main__':
    main()
