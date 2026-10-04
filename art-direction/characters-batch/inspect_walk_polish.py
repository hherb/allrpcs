#!/usr/bin/env python3
"""Check candidate bytes against actual rendering, not just the recorded gait metadata."""

from io import BytesIO
import json
import zipfile

from PIL import Image, ImageDraw

from build_review import ROOT, digest
from inspect_package import require
from inspect_walk_rig import inspect
from polish_walk import OUT, connected_limb, prepare_polished, render


def check_connection(layer, socket, hem):
    """Require substantial boot paint to remain connected to the concealed hip."""
    mask = layer.getchannel('A').point(lambda value: 255 if value > 96 else 0)
    total = mask.histogram()[255]
    ImageDraw.floodfill(mask, (round(socket), hem - 12), 128)
    # Tiny isolated generated fringe specks are permissible; a detached boot is not.
    require(total and mask.histogram()[128] / total >= 0.99, 'Detached hip/boot paint')


def main():
    """Replay all 36 directional rigs and verify preserved predecessor snapshots."""
    inspect(OUT)
    inventory = json.loads((OUT / 'inventory.json').read_text())
    for rig in inventory['rigs']:
        frames, phases, sources = render(rig['role'], rig['direction'])
        require(json.loads(json.dumps(phases)) == rig['phases'], 'Stored phase metadata differs from renderer')
        require(set(sources) == set(rig['sources']), 'Stored source set differs from renderer')
        body, feet, textures, sockets, hem, floor, _ = prepare_polished(rig['role'], rig['direction'])
        for phase in phases:
            for index, leg in enumerate(phase['legs']):
                layer = connected_limb(feet[index], textures[index], sockets[index], hem, floor,
                                       leg['advance'], leg['lift'] - leg['depth'], leg['knee_bend_2x'])
                check_connection(layer, sockets[index], hem)
                require(body.getpixel((round(sockets[index]), hem - 12))[3] > 96,
                        'Hip socket is outside the torso paint')
        for index, frame in enumerate(frames):
            asset_id = f"{rig['role']}-{rig['direction']}-walk-{index + 1:02}"
            with zipfile.ZipFile(OUT / 'masters' / f'{asset_id}.ora') as archive:
                with Image.open(BytesIO(archive.read('mergedimage.png'))) as stored:
                    require(stored.tobytes() == frame.tobytes(), f'Render mismatch: {asset_id}')
    # Original extraction and first mechanical packages remain immutable lineage.
    for predecessor in [ROOT, ROOT / 'walk-rig']:
        manifest = json.loads((predecessor / 'review-manifest.json').read_text())
        for path, recorded_hash in manifest['files'].items():
            # The first rig records its original parent authoring scripts. The
            # current shared writer/checker are bound in this candidate manifest;
            # a separate replay verifies byte-identical predecessor output.
            if predecessor == ROOT / 'walk-rig' and path in {'../rig_walk.py', '../inspect_walk_rig.py'}:
                continue
            require(digest(predecessor / path) == recorded_hash, f'Changed predecessor: {path}')
    historical = json.loads((OUT / 'predecessor-review-manifest.json').read_text())
    refreshed = {'README.md', 'rig_walk.py', 'inspect_walk_rig.py'}
    for path, recorded_hash in historical['files'].items():
        if path not in refreshed:
            require(digest(ROOT / path) == recorded_hash, f'Changed historical artwork: {path}')
    manifest = json.loads((OUT / 'review-manifest.json').read_text())
    expected = {str(path.relative_to(OUT)) for path in OUT.rglob('*')
                if path.is_file() and path not in {OUT / 'review-manifest.json', OUT / 'owner-acceptance.json'}}
    require(set(manifest['files']) == expected, 'Incomplete polish review manifest')
    for path, recorded_hash in manifest['files'].items():
        require(digest(OUT / path) == recorded_hash, f'Changed polish review: {path}')
    for path, recorded_hash in manifest['authoring_scripts'].items():
        require(digest(ROOT / path) == recorded_hash, f'Changed authoring script: {path}')
    approval_path = OUT / 'owner-acceptance.json'
    if approval_path.exists():
        approval = json.loads(approval_path.read_text())
        require(approval['runtime_promotion'] is False, 'Artwork approval must not grant runtime promotion')
        require(approval['review_manifest_sha256'] == digest(OUT / 'review-manifest.json'),
                'Approved snapshot differs from current review manifest')
        require(set(approval['approved_files']) == {
            path for path in manifest['files'] if path.startswith(('exports/', 'masters/', 'review/'))
        }, 'Incomplete approved artwork scope')
        for path, recorded_hash in approval['approved_files'].items():
            require(digest(OUT / path) == recorded_hash, f'Changed approved artwork: {path}')
        print('PASS: exact owner-approved walking artwork and review snapshot hashes.')
    print('PASS: all 288 stored masters match replayed pixels; phase/source metadata matches.')
    print('PASS: all 576 limb poses connect hip to boot; sockets overlap torso paint.')
    print('PASS: predecessor artwork unchanged; complete polish review/script hashes match.')


if __name__ == '__main__':
    main()
