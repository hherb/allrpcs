#!/usr/bin/env python3
"""Verify approved artwork bytes and the external PNG/OpenRaster package contract."""
from io import BytesIO
from pathlib import Path
import hashlib
import json
import zipfile
from xml.etree import ElementTree

from PIL import Image

ROOT = Path(__file__).resolve().parent


def main():
    inventory = json.loads((ROOT / 'inventory.json').read_text())
    acceptance = json.loads((ROOT / 'owner-acceptance.json').read_text())
    assert len(inventory['assets']) == inventory['asset_count'] == 42
    assert len({asset['id'] for asset in inventory['assets']}) == 42
    for relative, digest in acceptance['approved_files'].items():
        assert hashlib.sha256((ROOT / relative).read_bytes()).hexdigest() == digest, relative
    for asset in inventory['assets']:
        path = ROOT / asset['export']
        assert hashlib.sha256(path.read_bytes()).hexdigest() == asset['sha256'], path
        with Image.open(path) as export:
            export.load()
            assert export.mode == 'RGBA' and list(export.size) == asset['size'], path
            alpha = export.getchannel('A')
            assert alpha.getextrema()[0] == 0 and alpha.getbbox(), path
            x, y = asset['proposed_pivot']
            assert 0 <= x < export.width and 0 <= y < export.height, path
            with zipfile.ZipFile(ROOT / asset['master']) as master:
                assert master.testzip() is None
                assert master.read('mimetype') == b'image/openraster'
                tree = ElementTree.fromstring(master.read('stack.xml'))
                assert (int(tree.attrib['w']), int(tree.attrib['h'])) == (export.width*2, export.height*2)
                assert len(tree.findall('./stack/layer')) == 2
                with Image.open(BytesIO(master.read('mergedimage.png'))) as merged:
                    expected = merged.resize(export.size, Image.Resampling.LANCZOS)
                    assert expected.tobytes() == export.tobytes(), path
    assert len(list((ROOT / 'sources').glob('*.png'))) == 7
    assert len(list((ROOT / 'review').glob('*.png'))) == 9
    for path in (ROOT / 'review').glob('*.png'):
        with Image.open(path) as review:
            review.verify()
    print('PASS: 42 approved RGBA exports, exact 2x editable masters, seven sources, nine review images and acceptance hashes.')
    print('External artwork only: runtime imports, placement, collision and accessibility remain separate work.')


if __name__ == '__main__':
    main()
