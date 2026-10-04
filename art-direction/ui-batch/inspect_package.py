#!/usr/bin/env python3
"""Inspect candidate UI artwork integrity without making usability or runtime claims."""
from io import BytesIO
import json
import zipfile
from xml.etree import ElementTree
from PIL import Image
from build_review import CROP_ALPHA_THRESHOLD, CROP_MARGIN, FAMILIES, PADDING, REPO, ROOT, digest


def require(condition, message):
    """Keep authoring checks active under optimized Python."""
    if not condition:
        raise ValueError(message)


def main():
    """Check complete coverage, review hashes and exact editable master/export parity."""
    inventory = json.loads((ROOT / 'inventory.json').read_text())
    manifest = json.loads((ROOT / 'review-manifest.json').read_text())
    expected = {f'{family}-{index:02}' for family in FAMILIES for index in range(1, 7)}
    assets = inventory['assets']
    require(len(assets) == inventory['asset_count'] == len(expected) == 36, 'Incorrect count')
    require({asset['id'] for asset in assets} == expected, 'Missing or duplicate IDs')
    for folder, suffix in [('exports', 'png'), ('masters', 'ora')]:
        require({path.stem for path in (ROOT / folder).glob(f'*.{suffix}')} == expected, folder)
    require({p.stem for p in (ROOT / 'sources').glob('*.png')} == set(FAMILIES), 'Source coverage')
    require({p.stem for p in (ROOT / 'review').glob('*.png')} ==
            set(FAMILIES) | {'research-scale', 'conversation-scale'}, 'Review coverage')
    expected_files = {str(p.relative_to(ROOT)) for folder in ['sources', 'exports', 'masters', 'review', 'evidence']
                      for p in (ROOT / folder).iterdir() if p.is_file()}
    expected_files |= {'inventory.json', 'prompts.json', 'build_review.py', 'inspect_package.py', 'README.md'}
    require(set(manifest['files']) == expected_files, 'Hash coverage')
    for relative, sha256 in manifest['files'].items():
        require(digest(ROOT / relative) == sha256, f'Changed review input: {relative}')
    provenance = json.loads((ROOT / 'sources/provenance.json').read_text())
    require(set(provenance['sources']) == set(FAMILIES), 'Provenance coverage')
    for family, metadata in provenance['sources'].items():
        require(digest(ROOT / 'sources' / f'{family}.png') == metadata['sha256'], 'Original source hash')
    require(inventory['crop_policy'] == {'alpha_threshold': CROP_ALPHA_THRESHOLD,
            'margin_pixels': CROP_MARGIN, 'original_sources_preserved': True}, 'Crop policy')
    for asset in assets:
        family, index = asset['id'].rsplit('-', 1)
        require(asset['source'] == f'sources/{family}.png' and
                asset['export'] == f"exports/{asset['id']}.png" and
                asset['master'] == f"masters/{asset['id']}.ora" and
                asset['name'] == FAMILIES[family][int(index) - 1], 'Asset mapping')
        with Image.open(ROOT / asset['source']) as source:
            require(source.mode == 'RGBA' and source.size == (1536, 1024), 'Source format')
            cell = source.crop(asset['source_cell'])
            alpha = cell.getchannel('A')
            alpha.paste(0, tuple(asset['source_crop_in_cell']))
            require(asset['excluded_faint_pixels'] == {'pixel_count': sum(alpha.histogram()[1:]),
                    'max_alpha': alpha.getextrema()[1]}, 'Excluded faint pixels accounting')
            original_paint = cell.crop(asset['source_crop_in_cell'])
        require(asset['excluded_faint_pixels']['max_alpha'] < inventory['crop_policy']['alpha_threshold'],
                'Visible paint excluded from crop')
        require(digest(ROOT / asset['export']) == asset['sha256'], 'Export hash')
        require(digest(ROOT / asset['master']) == asset['master_sha256'], 'Master hash')
        with Image.open(ROOT / asset['export']) as export:
            require(export.mode == 'RGBA' and list(export.size) == asset['size'], 'Export format')
            alpha = export.getchannel('A')
            require(alpha.getextrema()[0] == 0 and alpha.getbbox() is not None, 'Export alpha')
            require(all(alpha.crop(box).getextrema()[1] == 0 for box in
                        [(0, 0, export.width, 1), (0, export.height - 1, export.width, export.height),
                         (0, 0, 1, export.height), (export.width - 1, 0, export.width, export.height)]),
                    'Export padding clips alpha')
            require(asset['proposed_pivot'] == [0, 0], 'Provisional top-left anchor')
            with zipfile.ZipFile(ROOT / asset['master']) as archive:
                require(archive.testzip() is None, 'Corrupt master')
                first = archive.infolist()[0]
                require(first.filename == 'mimetype' and first.compress_type == zipfile.ZIP_STORED and
                        archive.read('mimetype') == b'image/openraster', 'OpenRaster header')
                tree = ElementTree.fromstring(archive.read('stack.xml'))
                size = (export.width * 2, export.height * 2)
                require((int(tree.attrib['w']), int(tree.attrib['h'])) == size ==
                        tuple(asset['master_size']), 'Master size')
                layers = tree.findall('./stack/layer')
                require([(p.attrib['name'], p.attrib['src']) for p in layers] ==
                        [('Corrections', 'data/corrections.png'), ('Source paint', 'data/source.png')], 'Layers')
                require(all(layer.attrib['opacity'] == '1.0' and
                            layer.attrib['visibility'] == 'visible' and
                            layer.attrib['composite-op'] == 'svg:src-over' and
                            layer.attrib['x'] == layer.attrib['y'] == '0' for layer in layers), 'Layer placement')
                with Image.open(BytesIO(archive.read('mergedimage.png'))) as merged, \
                     Image.open(BytesIO(archive.read('data/source.png'))) as paint, \
                     Image.open(BytesIO(archive.read('data/corrections.png'))) as corrections:
                    require(merged.mode == paint.mode == corrections.mode == 'RGBA' and
                            merged.size == paint.size == corrections.size == size, 'Raster formats')
                    expected_paint = Image.new('RGBA', size)
                    expected_paint.alpha_composite(original_paint.resize((size[0] - PADDING * 4, size[1] - PADDING * 4),
                                                   Image.Resampling.LANCZOS), (PADDING * 2, PADDING * 2))
                    require(expected_paint.tobytes() == paint.tobytes(), 'Source to master parity')
                    require(paint.tobytes() == merged.tobytes() and
                            corrections.getchannel('A').getbbox() is None, 'Layer merge')
                    require(merged.resize(export.size, Image.Resampling.LANCZOS).tobytes() == export.tobytes(),
                            'Exact 2x parity')
    prompts = json.loads((ROOT / 'prompts.json').read_text())
    require(set(prompts['families']) == set(FAMILIES), 'Prompt coverage')
    require(digest(REPO / prompts['reference']) == prompts['reference_sha256'], 'Style reference')
    for folder in ['sources', 'review']:
        for path in (ROOT / folder).glob('*.png'):
            with Image.open(path) as image:
                image.verify()
    print('PASS: 36 UI candidates, 36 exact 2x masters, six sources and eight review images.')
    print('PASS: candidate hashes, RGBA, OpenRaster layers and master/export parity.')
    print('Owner review pending; runtime and usability remain unverified.')


if __name__ == '__main__':
    main()
