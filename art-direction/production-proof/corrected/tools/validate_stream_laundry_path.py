#!/usr/bin/env python3
"""Bind the approved Stream–Laundry promotion to exact pixels and presentation scope."""

import argparse
import hashlib
import json

from check_stream_laundry_path import validate as validate_archived
from check_stream_laundry_path import TARGETS
from check_village_edge_square_connector_feasibility import ROOT, PROOF, REVIEW, require
from validate_shrine_southern_road_path import HELPERS, digest, rgba, validate_master

ASSET_ID = 'terrain.stream_laundry_path'
SHA = '6422441f218b6ff0062269f55583ef488a0d3c877787d0b7892a4605950b2185'
RECORD_HASH = '2a014a696e676f9043abf71b2ab6eceb6331297ac4ab532198f9e6b794487c40'


def validate(check_captures=False, check_promotion_scope=False):
    """Preserve archived proofs; check master, export, imports and optional runtime views/scope."""
    validate_archived()
    manifest = json.loads((PROOF / 'asset-manifest.json').read_text())
    entries = [asset for asset in manifest['assets'] if asset['id'] == ASSET_ID]
    require(len(entries) == 1, 'expected one Stream–Laundry asset')
    asset = entries[0]
    rect = dict(x=0, y=0, width=432, height=416)
    require(asset['export'] == 'exports/terrain/stream-laundry-path.png'
            and asset['source'] == 'tools/build_stream_laundry_path_correction.py'
            and asset['pivot'] == dict(x=216, y=208)
            and asset['logical_rect'] == rect and asset['export_rect_pixels'] == rect
            and asset['master_dimensions_pixels'] == dict(width=864, height=832, scale=2)
            and asset['contact_baseline'] == 416 and asset['godot_import_proof'] is True,
            'Stream–Laundry geometry/provenance changed')
    proof = REVIEW / 'stream-laundry-path-correction-overlay.png'
    accepted = rgba(proof)
    require(accepted.size == (432, 416) and digest(proof) == SHA, 'accepted pixels changed')
    require(digest(PROOF / asset['export']) == asset['sha256'] == SHA, 'export bytes changed')
    provenance = manifest['stream_laundry_path_provenance']
    require(provenance == {
        'accepted_proof': 'review/stream-laundry-path-correction-overlay.png',
        'accepted_proof_sha256': SHA, 'master': 'masters/terrain-stream-laundry-path.ora',
        'recipe_sha256': digest(PROOF / 'tools/build_stream_laundry_path_correction.py'),
        'world_origin': [1424, 152], 'world_position': [1640, 360], 'z_index': -75,
        'import_fix_alpha_border': False,
        'master_scaling': 'nearest-neighbour 2x of exact accepted export',
    }, 'promotion provenance changed')
    validate_master(PROOF / provenance['master'], accepted)
    for directory in ('runtime', 'direction-a-import-proof'):
        texture = ROOT / f'game/art/{directory}/terrain/stream-laundry-path.png'
        require(not texture.is_symlink() and digest(texture) == SHA, 'import source changed')
        options = HELPERS['read_import_options'](texture.with_suffix('.png.import'))
        cache = HELPERS['imported_cache_path'](texture)
        require(options.get(('deps', 'source_file')) == f'"{HELPERS["resource_path"](texture)}"'
                and options.get(('remap', 'path')) == f'"{cache}"'
                and options.get(('deps', 'dest_files')) == f'["{cache}"]', 'import paths changed')
        for key, value in {'compress/mode': '0', 'mipmaps/generate': 'false',
                           'process/fix_alpha_border': 'false', 'process/premult_alpha': 'false',
                           'process/size_limit': '0',
                           **{f'process/channel_remap/{c}': str(i) for i, c in
                              enumerate(('red', 'green', 'blue', 'alpha'))}}.items():
            require(options.get(('params', key)) == value, f'lossless import drift: {key}')
        uid = options.get(('remap', 'uid'))
        require(options.get(('remap', 'importer')) == '"texture"'
                and uid is not None and uid.startswith('"uid://')
                and sum(HELPERS['read_import_options'](p).get(('remap', 'uid')) == uid
                        for p in (ROOT / 'game/art').rglob('*.png.import')) == 1,
                'import UID missing or shared')
    if check_promotion_scope:
        validate_scope(manifest)
    if check_captures:
        for view in TARGETS:
            for zoom in ('0.8', '1.0', '2.0'):
                for frame, height in (('wide', 720), ('tall', 827)):
                    suffix = f'{view}-{zoom}x-{frame}.png'
                    runtime = REVIEW / f'stream-laundry-path-runtime-{suffix}'
                    proof = REVIEW / f'stream-laundry-path-correction-{suffix}'
                    require(digest(runtime) == digest(proof), f'runtime PNG differs: {suffix}')
                    image = rgba(runtime)
                    require(image.size == (1280, height) and image.getchannel('A').getextrema() == (255, 255),
                            f'invalid runtime capture: {suffix}')
    print('Stream–Laundry promotion valid: exact accepted asset, editable master and independent imports.')


def validate_scope(manifest):
    """Check retained baseline, manifest/scene normalization and physical navigation bodies."""
    path = REVIEW / 'stream-laundry-path-promotion-evidence.json'
    require(digest(path) == RECORD_HASH, 'promotion scope record changed')
    record = json.loads(path.read_text())
    require(len(record['retained_files']) == 447, 'retained baseline file set changed')
    for relative, sha in record['retained_files'].items():
        require(digest(ROOT / relative) == sha, f'retained file changed: {relative}')
    retained = {k: v for k, v in manifest.items() if k != 'stream_laundry_path_provenance'}
    retained['assets'] = [a for a in manifest['assets'] if a['id'] != ASSET_ID]
    require(hashlib.sha256((json.dumps(retained, indent=2) + '\n').encode()).hexdigest()
            == record['manifest_sha256'], 'unrelated manifest changed')
    scene = (ROOT / 'game/Main.tscn').read_text()
    resource = '[ext_resource type="Texture2D" path="res://art/runtime/terrain/stream-laundry-path.png" id="434_runtime_stream_laundry_path"]\n'
    binding = 'StreamLaundryPathTexture = ExtResource("434_runtime_stream_laundry_path")\n'
    require(scene.count(resource) == 1 and scene.count(binding) == 1, 'scene binding changed')
    scene = scene.replace(resource, '').replace(binding, '').replace(
        '[gd_scene load_steps=351 format=3]', '[gd_scene load_steps=350 format=3]', 1)
    require(hashlib.sha256(scene.encode()).hexdigest() == record['main_scene_sha256'], 'unrelated scene changed')
    nav = (ROOT / 'game/GreyboxNavigation.cs').read_text()
    tail = nav[nav.index('    private static Vector2 FindSquareBoundaryExit('):]
    enter = nav[nav.index('    public override void _EnterTree()'):
                nav.index('    /// <inheritdoc />', nav.index('    public override void _EnterTree()'))]
    require(hashlib.sha256(tail.encode()).hexdigest() == record['navigation_tail_sha256']
            and hashlib.sha256(enter.encode()).hexdigest() == record['navigation_enter_sha256'],
            'physical/navigation authority changed')


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--check-captures', action='store_true')
    parser.add_argument('--check-promotion-scope', action='store_true')
    args = parser.parse_args()
    validate(args.check_captures, args.check_promotion_scope)
