#!/usr/bin/env python3
"""Check external rig coverage, paint ownership, both stride halves and package integrity."""

from io import BytesIO
import hashlib
import json
import math
import zipfile

from PIL import Image

from build_review import ANIMATED, DIRECTIONS, ROOT, digest
from inspect_package import require
from rig_walk import OUT, STEPS


def inspect(out=OUT):
    """Check either mechanical candidate package without granting visual acceptance."""
    inventory = json.loads((out / 'inventory.json').read_text())
    expected = {f'{role}-{direction}-walk-{step:02}' for role in ANIMATED
                for direction in DIRECTIONS for step in range(1, STEPS + 1)}
    require(inventory['asset_count'] == len(inventory['assets']) == len(expected) == 288, 'Wrong rig count')
    require({asset['id'] for asset in inventory['assets']} == expected, 'Incomplete rig assets')
    for folder, extension in [('exports', '*.png'), ('masters', '*.ora')]:
        require({path.stem for path in (out / folder).glob(extension)} == expected, f'Orphan {folder}')
    pixel_hashes = {}
    for asset in inventory['assets']:
        require(digest(out / asset['export']) == asset['sha256'], 'Changed rig export')
        require(digest(out / asset['master']) == asset['master_sha256'], 'Changed rig master')
        with Image.open(out / asset['export']) as export:
            export.load()
            require(export.mode == 'RGBA' and export.size == (128, 192), 'Wrong export format')
            require(export.getchannel('A').getextrema()[0] == 0, 'No transparency')
            require(export.getchannel('A').getbbox(), 'Empty rig')
            pixel_hash = hashlib.sha256(export.tobytes()).hexdigest()
            with zipfile.ZipFile(out / asset['master']) as archive:
                require(archive.testzip() is None, 'Damaged OpenRaster master')
                with Image.open(BytesIO(archive.read('mergedimage.png'))) as merged:
                    require(merged.size == (256, 384), 'Wrong master size')
                    require(merged.resize(export.size, Image.Resampling.LANCZOS).tobytes() == export.tobytes(),
                            'Master/export pixel mismatch')
        key = asset['id'].rsplit('-walk-', 1)[0]
        pixel_hashes.setdefault(key, set()).add(pixel_hash)
    require(all(len(hashes) == STEPS for hashes in pixel_hashes.values()), 'Repeated gait pose')
    require({(rig['role'], rig['direction']) for rig in inventory['rigs']} ==
            {(role, direction) for role in ANIMATED for direction in DIRECTIONS}, 'Incomplete directions')
    require(len(inventory['rigs']) == 36, 'Duplicate directional rigs')
    for rig in inventory['rigs']:
        require(rig['loop_endpoint_pixels_equal'], 'Loop endpoint not verified')
        for source, recorded_hash in rig['sources'].items():
            require(digest(ROOT / 'masters' / f'{source}.ora') == recorded_hash, 'Changed source paint')
        phases = rig['phases']
        require(len(phases) == STEPS, 'Wrong phase count')
        for phase in phases:
            legs = phase['legs']
            require([leg['leg'] for leg in legs] == ['A', 'B'], 'Swapped leg identity')
            require([leg['anatomical_side'] for leg in legs] == ['left', 'right'], 'Swapped anatomical identity')
            require(math.isclose(legs[0]['travel'], -legs[1]['travel'], abs_tol=1e-6), 'Legs move in phase')
            for leg in legs:
                require(not leg['stance'] or leg['lift'] == 0, 'Planted foot lifted')
                require(leg['paint_bounds_2x'] is not None, 'Empty limb layer')
        require(phases[0]['legs'][0]['travel'] == 1 and phases[4]['legs'][0]['travel'] == -1,
                'Same foot still leads at opposite contacts')
        require(phases[1]['legs'][0]['stance'] and not phases[1]['legs'][1]['stance'] and
                not phases[5]['legs'][0]['stance'] and phases[5]['legs'][1]['stance'], 'Swing does not alternate')
    for role in ANIMATED:
        with Image.open(out / 'review' / f'{role}-walk.gif') as preview:
            require(preview.n_frames == STEPS and preview.info['loop'] == 0, 'Incomplete GIF loop')
    with Image.open(out / 'review' / 'healer-leg-ownership.gif') as diagnostic:
        require(diagnostic.n_frames == STEPS, 'Incomplete ownership diagnostic')
    print('PASS: 288 RGBA cutout frames, exact 2x masters and unchanged source-paint hashes.')
    print('PASS: 36 eight-phase loops, alternating contact/swing ownership, unique poses and 10 full-loop GIFs.')
    print('Authoring checks only: knee/arm articulation, cutout style, timing and owner acceptance remain review concerns.')


def main():
    """Inspect the original mechanical candidate checkpoint."""
    inspect()


if __name__ == '__main__':
    main()
