#!/usr/bin/env python3
"""Export generated paint at half resolution and retain an exact 2x layered ORA master."""
from io import BytesIO
import hashlib
import json
from pathlib import Path
import zipfile
from xml.etree.ElementTree import Element, SubElement, tostring
from PIL import Image

ROOT = Path(__file__).resolve().parents[2]
PACKAGE = Path(__file__).resolve().parent
SPECS = [
    ('sickhouse-north-entry', 'Sickhouse closed south wall; north entrance remains authoritative', 'sickhouse-north-entry-exterior', True),
    ('cutaway-wood', 'Painted timber floor', 'cutaway-wood', False),
    ('cutaway-stone', 'Painted worn-stone floor', 'cutaway-stone', False),
]


def png(paint):
    """Serialize a layer without changing its paint pixels."""
    output = BytesIO()
    paint.save(output, format='PNG')
    return output.getvalue()


def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def write_entry(archive, name, payload, stored=False):
    """Keep archive bytes reproducible across rebuild dates."""
    info = zipfile.ZipInfo(name, date_time=(1980, 1, 1, 0, 0, 0))
    info.compress_type = zipfile.ZIP_STORED if stored else zipfile.ZIP_DEFLATED
    archive.writestr(info, payload)


def build():
    manifest_path = PACKAGE / 'manifest.json'
    manifest = json.loads(manifest_path.read_text())
    manifest['assets'] = [asset for asset in manifest['assets'] if 'generator' not in asset]
    for identity, name, source_name, transparent in SPECS:
        source = PACKAGE / 'sources' / (source_name + '.png')
        paint = Image.open(source).convert('RGBA')
        if paint.width % 2 or paint.height % 2:
            raise ValueError('Generated master needs even dimensions for exact 2x export')
        size = (paint.width // 2, paint.height // 2)
        exported = paint.resize(size, Image.Resampling.LANCZOS)
        # Mechanical alpha sanitation, not repainting; original generator output stays intact.
        exported.putdata([pixel if pixel[3] else (0, 0, 0, 0) for pixel in exported.get_flattened_data()])
        runtime = ROOT / 'game/art/runtime/structures' / (identity + '.png')
        exported.save(runtime)
        master = PACKAGE / 'masters' / (source_name + '.ora')
        xml = Element('image', w=str(paint.width), h=str(paint.height), name=name, version='0.0.3')
        stack = SubElement(xml, 'stack')
        for layer, path in [('Corrections', 'data/corrections.png'), ('Generated paint', 'data/paint.png')]:
            SubElement(stack, 'layer', name=layer, src=path, opacity='1.0', visibility='visible',
                       attrib={'composite-op': 'svg:src-over', 'x': '0', 'y': '0'})
        with zipfile.ZipFile(master, 'w', compression=zipfile.ZIP_DEFLATED) as archive:
            write_entry(archive, 'mimetype', 'image/openraster', stored=True)
            write_entry(archive, 'stack.xml', tostring(xml, encoding='utf-8'))
            write_entry(archive, 'data/paint.png', png(paint))
            write_entry(archive, 'data/corrections.png', png(Image.new('RGBA', paint.size)))
            write_entry(archive, 'mergedimage.png', png(paint))
            preview = paint.copy()
            preview.thumbnail((256, 256), Image.Resampling.LANCZOS)
            write_entry(archive, 'Thumbnails/thumbnail.png', png(preview))
        asset = dict(id=identity, name=name, source=str(source.relative_to(ROOT)),
                     runtime=str(runtime.relative_to(ROOT)), size=list(size), sha256=sha(runtime),
                     source_sha256=sha(source), master=str(master.relative_to(ROOT)), master_sha256=sha(master),
                     master_size=list(paint.size), generator='built-in image_gen', transparent=transparent,
                     processing='Half-resolution LANCZOS; zero RGB only where alpha is zero',
                     acceptance='New candidate; no owner visual acceptance yet')
        if identity == 'sickhouse-north-entry':
            asset['derivative_of'] = 'art-direction/structures-batch/exports/sickhouse-01.png'
        manifest['assets'].append(asset)
    manifest['notes'] = 'Six exact approved exports plus three new image_gen candidates: sickhouse orientation variant, timber floor and worn-stone floor. Original approved art unchanged; exact 2x layered masters retained.'
    manifest_path.write_text(json.dumps(manifest, indent=2) + '\n')


if __name__ == '__main__':
    build()
