#!/usr/bin/env python3
"""Bind the approved Woodland–Shrine promotion to exact pixels and presentation scope."""

import argparse
import hashlib
import json

from check_woodland_shrine_path_junction import validate as validate_archived
from check_woodland_shrine_path_feasibility import TARGETS
from check_village_edge_square_connector_feasibility import ROOT, PROOF, REVIEW, require
from validate_shrine_southern_road_path import HELPERS, digest, rgba, validate_master

ASSET_ID = 'terrain.woodland_shrine_path'
SHA = 'dd38db38f7a11d8d621ac938e822650c3b5a5d1be9ea190e60e39d101906d18c'
RECORD_HASH = '8b630e8b16e9c7dad53135a599c31db57ba4ca4ab6d70540d9ea724bb2e1b312'


def validate(check_captures=False, check_promotion_scope=False):
    """Preserve archived proofs; check master, export, imports and optional runtime views/scope."""
    validate_archived()
    manifest = json.loads((PROOF / 'asset-manifest.json').read_text())
    entries = [asset for asset in manifest['assets'] if asset['id'] == ASSET_ID]
    require(len(entries) == 1, 'expected one Woodland–Shrine asset')
    asset = entries[0]
    rect = dict(x=0, y=0, width=704, height=208)
    require(asset['export'] == 'exports/terrain/woodland-shrine-path.png'
            and asset['source'] == 'tools/build_woodland_shrine_path_junction.py'
            and asset['pivot'] == dict(x=352, y=104)
            and asset['logical_rect'] == rect and asset['export_rect_pixels'] == rect
            and asset['master_dimensions_pixels'] == dict(width=1408, height=416, scale=2)
            and asset['contact_baseline'] == 208 and asset['godot_import_proof'] is True,
            'Woodland–Shrine geometry/provenance changed')
    proof = REVIEW / 'woodland-shrine-path-junction-overlay.png'
    accepted = rgba(proof)
    require(accepted.size == (704, 208) and digest(proof) == SHA, 'accepted pixels changed')
    require(digest(PROOF / asset['export']) == asset['sha256'] == SHA, 'export bytes changed')
    provenance = manifest['woodland_shrine_path_provenance']
    require(provenance == {
        'accepted_proof': 'review/woodland-shrine-path-junction-overlay.png',
        'accepted_proof_sha256': SHA, 'master': 'masters/terrain-woodland-shrine-path.ora',
        'recipe_sha256': digest(PROOF / 'tools/build_woodland_shrine_path_junction.py'),
        'world_origin': [352, 224], 'world_position': [704, 328], 'z_index': -74,
        'import_fix_alpha_border': False,
        'master_scaling': 'nearest-neighbour 2x of exact accepted export',
    }, 'promotion provenance changed')
    validate_master(PROOF / provenance['master'], accepted)
    for directory in ('runtime', 'direction-a-import-proof'):
        texture = ROOT / f'game/art/{directory}/terrain/woodland-shrine-path.png'
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
                    runtime = REVIEW / f'woodland-shrine-path-runtime-{suffix}'
                    proof = REVIEW / f'woodland-shrine-path-junction-{suffix}'
                    require(digest(runtime) == digest(proof), f'runtime PNG differs: {suffix}')
                    image = rgba(runtime)
                    require(image.size == (1280, height) and image.getchannel('A').getextrema() == (255, 255),
                            f'invalid runtime capture: {suffix}')
    print('Woodland–Shrine promotion valid: exact accepted asset, editable master and independent imports.')


def validate_scope(manifest):
    """Check retained baseline, manifest/scene normalization and physical navigation bodies."""
    path = REVIEW / 'woodland-shrine-path-promotion-evidence.json'
    require(digest(path) == RECORD_HASH, 'promotion scope record changed')
    record = json.loads(path.read_text())
    require(len(record['retained_files']) == 429, 'retained baseline file set changed')
    for relative, sha in record['retained_files'].items():
        require(digest(ROOT / relative) == sha, f'retained file changed: {relative}')
    retained = {k: v for k, v in manifest.items() if k != 'woodland_shrine_path_provenance'}
    retained['assets'] = [a for a in manifest['assets'] if a['id'] != ASSET_ID]
    require(hashlib.sha256((json.dumps(retained, indent=2) + '\n').encode()).hexdigest()
            == record['manifest_sha256'], 'unrelated manifest changed')
    scene = (ROOT / 'game/Main.tscn').read_text()
    resource = '[ext_resource type="Texture2D" path="res://art/runtime/terrain/woodland-shrine-path.png" id="431_runtime_woodland_shrine_path"]\n'
    binding = 'WoodlandShrinePathTexture = ExtResource("431_runtime_woodland_shrine_path")\n'
    require(scene.count(resource) == 1 and scene.count(binding) == 1, 'scene binding changed')
    scene = scene.replace(resource, '').replace(binding, '').replace(
        '[gd_scene load_steps=348 format=3]', '[gd_scene load_steps=347 format=3]', 1)
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
