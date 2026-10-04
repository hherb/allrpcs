#!/usr/bin/env python3
"""Check object package integrity and, when present, exact owner acceptance hashes."""

from io import BytesIO
import json
import zipfile
from xml.etree import ElementTree

from PIL import Image

from build_review import FAMILIES, REPO, ROOT, digest


def require(condition, message):
    """Keep checks active under Python optimization."""
    if not condition:
        raise ValueError(message)


def main():
    """Check exact series coverage, hashes, alpha and editable master/export parity."""
    inventory = json.loads((ROOT / 'inventory.json').read_text())
    manifest = json.loads((ROOT / 'review-manifest.json').read_text())
    approved = (ROOT / 'owner-acceptance.json').exists()
    if approved:
        acceptance = json.loads((ROOT / 'owner-acceptance.json').read_text())
        require(acceptance['status'] == 'approved', 'Invalid acceptance status')
        require(digest(ROOT / 'review-manifest.json') == acceptance['review_manifest_sha256'],
                'Changed historical review manifest')
        manifest = {'files': acceptance['approved_files']}
    expected = {f'{family}-{index:02}' for family in FAMILIES for index in range(1, 7)}
    assets = inventory['assets']
    require(len(assets) == inventory['asset_count'] == len(expected) == 42, 'Incorrect object count')
    require({asset['id'] for asset in assets} == expected, 'Missing or duplicate object IDs')
    for folder, extension in [('exports', 'png'), ('masters', 'ora')]:
        require({path.stem for path in (ROOT / folder).glob(f'*.{extension}')} == expected,
                f'Orphan or missing {folder}')
    require({path.stem for path in (ROOT / 'sources').glob('*.png')} == set(FAMILIES), 'Wrong source series')
    require({path.stem for path in (ROOT / 'review').glob('*.png')} ==
            set(FAMILIES) | {'apothecary-scale', 'investigation-scale'}, 'Incomplete review series')
    expected_files = {str(path.relative_to(ROOT)) for folder in ['sources', 'exports', 'masters', 'review']
                      for path in (ROOT / folder).iterdir() if path.is_file()}
    expected_files |= {'inventory.json', 'prompts.json', 'build_review.py', 'inspect_package.py', 'README.md'}
    require(set(manifest['files']) == expected_files, 'Incomplete review hash coverage')
    for relative, sha256 in manifest['files'].items():
        require(digest(ROOT / relative) == sha256, f'Changed review input: {relative}')
    for asset in assets:
        path = ROOT / asset['export']
        require(digest(path) == asset['sha256'], f'Changed export: {path}')
        require(digest(ROOT / asset['master']) == asset['master_sha256'], f'Changed master: {path}')
        with Image.open(path) as export:
            export.load()
            require(export.mode == 'RGBA' and list(export.size) == asset['size'], f'Invalid export: {path}')
            require(export.getchannel('A').getextrema()[0] == 0 and export.getchannel('A').getbbox(),
                    f'Invalid alpha: {path}')
            x, y = asset['proposed_pivot']
            require(0 <= x < export.width and 0 <= y < export.height, f'Pivot outside canvas: {path}')
            with zipfile.ZipFile(ROOT / asset['master']) as archive:
                require(archive.testzip() is None, f'Corrupt master: {path}')
                require(archive.infolist()[0].filename == 'mimetype' and
                        archive.infolist()[0].compress_type == zipfile.ZIP_STORED and
                        archive.read('mimetype') == b'image/openraster', f'Invalid OpenRaster header: {path}')
                tree = ElementTree.fromstring(archive.read('stack.xml'))
                master_size = (export.width * 2, export.height * 2)
                require((int(tree.attrib['w']), int(tree.attrib['h'])) == master_size ==
                        tuple(asset['master_size']), f'Wrong master size: {path}')
                layers = tree.findall('./stack/layer')
                require([(layer.attrib['name'], layer.attrib['src']) for layer in layers] ==
                        [('Corrections', 'data/corrections.png'), ('Source paint', 'data/source.png')],
                        f'Wrong layer contract: {path}')
                with Image.open(BytesIO(archive.read('mergedimage.png'))) as merged, \
                     Image.open(BytesIO(archive.read('data/source.png'))) as paint, \
                     Image.open(BytesIO(archive.read('data/corrections.png'))) as corrections:
                    require(merged.mode == paint.mode == corrections.mode == 'RGBA' and
                            merged.size == paint.size == corrections.size == master_size,
                            f'Invalid raster layers: {path}')
                    require(paint.tobytes() == merged.tobytes() and
                            corrections.getchannel('A').getbbox() is None, f'Layer merge mismatch: {path}')
                    require(merged.resize(export.size, Image.Resampling.LANCZOS).tobytes() == export.tobytes(),
                            f'Master/export mismatch: {path}')
    reference = inventory['scale_reference']
    require(digest(REPO / reference['path']) == reference['sha256'], 'Changed scale reference')
    prompts = json.loads((ROOT / 'prompts.json').read_text())
    require(set(prompts['families']) == set(FAMILIES), 'Incomplete prompt series')
    require(digest(REPO / prompts['reference']) == prompts['reference_sha256'], 'Changed style reference')
    for folder in ['sources', 'review']:
        for path in (ROOT / folder).glob('*.png'):
            with Image.open(path) as image:
                image.verify()
    print('PASS: 42 candidate exports, exact 2x editable masters, seven sources and nine review images.')
    print('PASS: review hashes, source/correction layers, scale reference, alpha and provisional pivot bounds.')
    print('PASS: exact owner-approved package hashes.' if approved else 'Authoring evidence only; owner review pending.')
    print('Runtime scale/imports and scenario semantics remain separate.')


if __name__ == '__main__':
    main()
