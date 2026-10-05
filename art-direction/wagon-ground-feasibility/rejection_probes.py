#!/usr/bin/env python3
"""Run targeted destructive probes only on disposable external-packet copies."""

import json
from pathlib import Path
import shutil
import tempfile

import numpy as np
from PIL import Image

import inspect_package as inspector
from build import ROOT, raster, require


def main():
    """Prove a valid fixture passes before rejecting phase, alpha, paint and camera mutations."""
    results = []
    support, masks = inspector.expected_masks()
    with tempfile.TemporaryDirectory(prefix='allrpcs-wagon-proof-') as temporary:
        fixture = Path(temporary) / 'packet'
        shutil.copytree(ROOT, fixture, ignore=shutil.ignore_patterns('__pycache__'))
        inspector.ROOT = fixture
        inspector.check()
        mutations = [('world phase', 'proof-inputs/world-paint.png', 'phase'),
                     ('coverage escape', 'proof-inputs/feather-16-coverage.png', 'coverage'),
                     ('paint RGB', 'proof-inputs/feather-16-layer.png', 'paint'),
                     ('camera translation', 'capture-evidence.json', 'camera')]
        for title, name, kind in mutations:
            path = fixture / name
            original = path.read_bytes()
            if kind == 'camera':
                record = json.loads(path.read_text())
                record['frames']['captures/feather-16-1.0x-wide.png']['world_to_canvas'][4] += 1
                path.write_text(json.dumps(record))
            elif kind == 'coverage':
                with Image.open(path) as image:
                    pixels = np.asarray(image).copy()
                pixels[0, 0] = 255
                Image.fromarray(pixels).save(path)
            else:
                pixels = np.asarray(raster(path)).copy()
                if kind == 'phase':
                    pixels = np.roll(pixels, 1, axis=1)
                else:
                    y, x = np.argwhere(pixels[:, :, 3] > 0)[0]
                    pixels[y, x, 0] ^= 1
                Image.fromarray(pixels).save(path)
            try:
                if kind == 'camera':
                    inspector.captures(support)
                else:
                    inspector.artwork(masks)
            except ValueError as error:
                results.append({'probe': title, 'status': 'rejected', 'reason': str(error)})
            else:
                raise ValueError('Mutation admitted: ' + title)
            finally:
                path.write_bytes(original)
        require(len(results) == 4, 'Incomplete rejection evidence')
    print(json.dumps({'valid_fixture': 'passed', 'probes': results}, indent=2))


if __name__ == '__main__':
    main()
