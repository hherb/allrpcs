#!/usr/bin/env python3
"""Lightweight authoring checks, not owner approval or consolidated product validation."""

from collections import Counter
from io import BytesIO
import json
import zipfile
from xml.etree import ElementTree

from PIL import Image

from build_review import ANIMATED, CAST, DIRECTIONS, FRAMES, REPO, ROOT, digest


def require(condition, message):
    """Fail loudly even when Python assertions are disabled."""
    if not condition:
        raise ValueError(message)


def main():
    inventory = json.loads((ROOT / 'inventory.json').read_text())
    assets = inventory['assets']
    expected_ids = {f'{role}-{kind}' for role in CAST for kind in ['cast', 'portraits']}
    expected_ids.update(f'{role}-{direction}-{frame}' for role in ANIMATED
                        for direction in DIRECTIONS for frame in FRAMES)
    expected_ids.update(f'{role}-dormant-{frame:02}' for role in ['rowan', 'mira'] for frame in range(1, 5))
    require(len(assets) == inventory['asset_count'] == len(expected_ids) == 312, 'Incorrect asset count')
    require({asset['id'] for asset in assets} == expected_ids, 'Missing or duplicate cast/animation assets')
    require({path.stem for path in (ROOT / 'exports').glob('*.png')} == expected_ids, 'Orphan or missing exports')
    require({path.stem for path in (ROOT / 'masters').glob('*.ora')} == expected_ids, 'Orphan or missing masters')
    expected_sources = {'cast', 'portraits', 'dormant', *ANIMATED}
    require({path.stem for path in (ROOT / 'sources').glob('*.png')} == expected_sources, 'Wrong source series')
    edge_candidates = []
    for asset in assets:
        path = ROOT / asset['export']
        require(digest(path) == asset['sha256'], f'Changed export: {path}')
        require(digest(ROOT / asset['master']) == asset['master_sha256'], f'Changed master: {path}')
        with Image.open(path) as export:
            export.load()
            require(export.mode == 'RGBA' and list(export.size) == asset['size'], f'Invalid export: {path}')
            alpha = export.getchannel('A')
            require(alpha.getextrema()[0] == 0 and alpha.getbbox(), f'Invalid alpha: {path}')
            x, y = asset['proposed_pivot']
            require(0 <= x < export.width and 0 <= y < export.height, f'Pivot outside export: {path}')
            with zipfile.ZipFile(ROOT / asset['master']) as archive:
                require(archive.testzip() is None, f'Corrupt master: {path}')
                require(archive.read('mimetype') == b'image/openraster', f'Wrong master type: {path}')
                tree = ElementTree.fromstring(archive.read('stack.xml'))
                require((int(tree.attrib['w']), int(tree.attrib['h'])) == (export.width * 2, export.height * 2),
                        f'Wrong master size: {path}')
                require(len(tree.findall('./stack/layer')) == 2, f'Wrong layer count: {path}')
                with Image.open(BytesIO(archive.read('mergedimage.png'))) as merged:
                    require(merged.resize(export.size, Image.Resampling.LANCZOS).tobytes() == export.tobytes(),
                            f'Master/export pixel mismatch: {path}')
        with Image.open(ROOT / asset['source']) as source:
            crop = source.crop(asset['source_cell']).convert('RGBA').getchannel('A')
            # Report substantial paint crossing a cell edge; harmless near-zero alpha is still retained.
            edges = [crop.crop((0, 0, crop.width, 1)), crop.crop((0, crop.height - 1, crop.width, crop.height)),
                     crop.crop((0, 0, 1, crop.height)), crop.crop((crop.width - 1, 0, crop.width, crop.height))]
            if any(edge.getextrema()[1] > 64 for edge in edges):
                edge_candidates.append(asset['id'])
    inherited = inventory['inherited_seeker_frames']
    require(len(inherited) == 48 and len({entry['path'] for entry in inherited}) == 48, 'Inherited Seeker count')
    for entry in inherited:
        require(digest(REPO / entry['path']) == entry['sha256'], f'Changed inherited frame: {entry["path"]}')
    counts = Counter(path.suffix for path in (ROOT / 'review').iterdir())
    require(counts['.png'] == 14 and counts['.gif'] == 27, f'Incomplete review series: {counts}')
    for path in (ROOT / 'review').iterdir():
        with Image.open(path) as image:
            image.verify()
    print('PASS: 312 candidate RGBA exports and exact 2x masters; 48 unchanged inherited Seeker frames.')
    print('PASS: 14 labelled PNG boards/compositions and 27 animated cycle previews.')
    print(f'Source cells with substantial paint on an edge (inspect visually): {edge_candidates}')
    print('Authoring checks only; owner artwork approval, timing/alignment and runtime integration remain pending.')
    print('KNOWN ARTWORK DEFECT: walk frames are incomplete stride studies, not full alternating cycles.')


if __name__ == '__main__':
    main()
