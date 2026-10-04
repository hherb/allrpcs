#!/usr/bin/env python3
"""Inspect residual ground candidate integrity without acceptance without runtime claims."""
from io import BytesIO
import json
import zipfile
from xml.etree import ElementTree
from PIL import Image
from build_review import (CELLS, CROP_ALPHA_THRESHOLD, CROP_MARGIN, FAMILIES, PADDING,
                          REPO, ROOT, SIZES, cells, digest, paint_from_cell)


def require(condition, message):
    """Keep authoring checks active under optimized Python."""
    if not condition:
        raise ValueError(message)


def inspect_scale_boards(inventory):
    """Verify every displayed export against its 1:1 composited pixels and position."""
    reference = inventory['scale_reference']
    expected_positions = {f'{family}-{index + 1:02}': [300 + index * 190, 78 + row * 190]
                          for row, family in enumerate(FAMILIES) for index in range(6)}
    require(reference['placements'] == expected_positions, 'Full-series scale coverage')
    require(reference['units'] == 'pixels at 1:1 export scale', 'Scale units')
    groups = [['ground-surfaces-01', 'ground-surfaces-02', 'ground-variation-01'],
              ['ground-surfaces-03', 'ground-surfaces-05', 'ground-variation-04'],
              ['surface-transitions-01', 'surface-transitions-02', 'surface-transitions-03'],
              ['surface-transitions-04', 'surface-transitions-05', 'surface-transitions-06']]
    expected_groups = [{'id': asset_id, 'position': [46 + (group % 2) * 630 + offset * 185,
                                                   170 + (group // 2) * 300]}
                       for group, ids in enumerate(groups) for offset, asset_id in enumerate(ids)]
    require(reference['grouped_placements'] == expected_groups, 'Grouped scale coverage')
    require(reference['canvases'] == {'light-scale': [1500, 680], 'dark-scale': [1500, 680],
                                    'grouped-scale': [1280, 720]}, 'Scale canvas metadata')
    for title, ground in [('light-scale', '#e8e3d5'), ('dark-scale', '#424a40'),
                          ('grouped-scale', None)]:
        placements = expected_groups if ground is None else [
            {'id': asset_id, 'position': position} for asset_id, position in expected_positions.items()]
        with Image.open(ROOT / 'review' / f'{title}.png') as board:
            require(board.mode == 'RGB' and list(board.size) == reference['canvases'][title],
                    'Scale board format')
            for placement in placements:
                x, y = placement['position']
                with Image.open(ROOT / 'exports' / f"{placement['id']}.png") as export:
                    require(0 <= x < x + export.width <= board.width and
                            0 <= y < y + export.height <= board.height, 'Scale board clipping')
                    background = ground or ('#e8e3d5' if y < 400 else '#303831')
                    expected = Image.new('RGBA', export.size, background)
                    expected.alpha_composite(export)
                    actual = board.crop((x, y, x + export.width, y + export.height))
                    require(actual.tobytes() == expected.convert('RGB').tobytes(),
                            f"Scale board export mismatch: {title}/{placement['id']}")


def main():
    """Check complete coverage, review hashes and exact editable master/export parity."""
    inventory = json.loads((ROOT / 'inventory.json').read_text())
    manifest = json.loads((ROOT / 'review-manifest.json').read_text())
    require(inventory['status'] == 'external candidate package; collective owner review pending',
            'Historical candidate status')
    acceptance_path = ROOT / 'owner-acceptance.json'
    approved = acceptance_path.exists()
    if approved:
        acceptance = json.loads(acceptance_path.read_text())
        require(acceptance['status'] == 'approved' and
                acceptance['approval_scope'] == 'complete 18-candidate residual ground artwork series',
                'Approval scope')
        require(digest(ROOT / 'review-manifest.json') == acceptance['review_manifest_sha256'],
                'Frozen review package changed')
        artwork_files = {relative: sha256 for relative, sha256 in manifest['files'].items()
                         if relative.endswith(('.png', '.ora'))}
        require(acceptance['approved_artwork'] == artwork_files, 'Artwork acceptance coverage')
    expected = {f'{family}-{index:02}' for family in FAMILIES for index in range(1, 7)}
    assets = inventory['assets']
    require(len(assets) == inventory['asset_count'] == len(expected) == 18, 'Incorrect count')
    require({asset['id'] for asset in assets} == expected, 'Missing or duplicate IDs')
    for folder, suffix in [('exports', 'png'), ('masters', 'ora')]:
        require({path.stem for path in (ROOT / folder).glob(f'*.{suffix}')} == expected, folder)
    require({p.stem for p in (ROOT / 'sources').glob('*.png')} == set(FAMILIES), 'Source coverage')
    require({p.stem for p in (ROOT / 'review').glob('*.png')} ==
            set(FAMILIES) | {'light-scale', 'dark-scale', 'grouped-scale'}, 'Review coverage')
    expected_files = {str(p.relative_to(ROOT)) for folder in ['sources', 'exports', 'masters', 'review', 'evidence']
                      for p in (ROOT / folder).glob('*') if p.is_file()}
    expected_files |= {'inventory.json', 'prompts.json', 'build_review.py', 'inspect_package.py', 'README.md'}
    require(set(manifest['files']) == expected_files, 'Hash coverage')
    for relative, sha256 in manifest['files'].items():
        require(digest(ROOT / relative) == sha256, f'Changed review input: {relative}')
    provenance = json.loads((ROOT / 'sources/provenance.json').read_text())
    require(set(provenance['sources']) == set(FAMILIES), 'Provenance coverage')
    for family, metadata in provenance['sources'].items():
        require(digest(ROOT / 'sources' / f'{family}.png') == metadata['sha256'], 'Original source hash')
        with Image.open(ROOT / 'sources' / f'{family}.png') as source:
            coverage = Image.new('L', source.size)
            for box in CELLS.get(family, cells(source)):
                require(0 <= box[0] < box[2] <= source.width and
                        0 <= box[1] < box[3] <= source.height, 'Source cell bounds')
                require(coverage.crop(box).getbbox() is None, 'Overlapping source cells')
                coverage.paste(255, box)
            require(coverage.getextrema() == (255, 255), 'Source cells omit pixels')
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
            expected_cell = CELLS.get(family, cells(source))[int(index) - 1]
            require(tuple(asset['source_cell']) == expected_cell, 'Changed source cell')
            original_paint, bounds, excluded = paint_from_cell(source, expected_cell, asset['id'])
            require(asset['source_crop_in_cell'] == bounds and
                    asset['excluded_faint_pixels'] == excluded, 'Crop margin and source accounting')
            cell = source.crop(asset['source_cell'])
            alpha = cell.getchannel('A')
            alpha.paste(0, tuple(asset['source_crop_in_cell']))
            require(asset['excluded_faint_pixels'] == {'pixel_count': sum(alpha.histogram()[1:]),
                    'max_alpha': alpha.getextrema()[1]}, 'Excluded faint pixels accounting')
            original_paint = cell.crop(asset['source_crop_in_cell'])
            width, height = SIZES[family][int(index) - 1]
            ratio = min(width / original_paint.width, height / original_paint.height)
            require(asset['size'] == [max(1, round(original_paint.width * ratio)) + PADDING * 2,
                                     max(1, round(original_paint.height * ratio)) + PADDING * 2],
                    'Proposed size')
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
    require(prompts['transparent_background'] is True and
            all(isinstance(prompt, str) and prompt.strip() for prompt in prompts['families'].values()),
            'Recorded generation prompts')
    require(digest(REPO / prompts['reference']) == prompts['reference_sha256'], 'Style reference')
    require(prompts['transition_reference'] == 'sources/ground-surfaces.png', 'Transition reference')
    reference = provenance['transition_material_reference']
    require(reference['path'] == prompts['transition_reference'] and
            digest(ROOT / reference['path']) == reference['sha256'], 'Transition source reference hash')
    for folder in ['sources', 'review']:
        for path in (ROOT / folder).glob('*.png'):
            with Image.open(path) as image:
                image.verify()
    inspect_scale_boards(inventory)
    print('PASS: 18 ground candidates, 18 exact 2x masters, three sources and six review images.')
    print('PASS: candidate hashes, RGBA, OpenRaster layers and master/export parity.')
    print('PASS: all 18 exports on both 1:1 scale boards and 12 grouped placements.')
    print('PASS: exact complete-series owner artwork acceptance.' if approved else
          'Collective owner artwork approval pending.')
    print('Seams, tileability and runtime suitability remain unverified.')



if __name__ == '__main__':
    main()
