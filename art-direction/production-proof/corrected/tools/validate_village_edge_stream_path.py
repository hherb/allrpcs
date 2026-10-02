#!/usr/bin/env python3
"""Bind the approved Village Edge–Stream promotion to exact pixels and presentation scope."""

import argparse
import hashlib
import json

from check_village_edge_stream_path import validate as validate_archived
from check_village_edge_stream_path import TARGETS
from check_village_edge_square_connector_feasibility import ROOT, PROOF, REVIEW, require
from validate_shrine_southern_road_path import HELPERS, digest, rgba, validate_master

ASSET_ID = 'terrain.village_edge_stream_path'
SHA = 'b943d35576e2116a635b0d91cd2b8123f8cba90c8a1d873fcc98079223594e4f'
RECORD_HASH = 'e2510f26d420747700b9c9ae6cadebff8c38aeb1a1ee128322e7877fc06b05c0'


def validate(check_captures=False, check_promotion_scope=False):
    """Preserve archived proofs; check master, export, imports and optional runtime views/scope."""
    validate_archived()
    manifest = json.loads((PROOF / 'asset-manifest.json').read_text())
    entries = [asset for asset in manifest['assets'] if asset['id'] == ASSET_ID]
    require(len(entries) == 1, 'expected one Village Edge–Stream asset')
    asset = entries[0]
    rect = dict(x=0, y=0, width=224, height=528)
    require(asset['export'] == 'exports/terrain/village-edge-stream-path.png'
            and asset['source'] == 'tools/build_village_edge_stream_path_correction.py'
            and asset['pivot'] == dict(x=112, y=264)
            and asset['logical_rect'] == rect and asset['export_rect_pixels'] == rect
            and asset['master_dimensions_pixels'] == dict(width=448, height=1056, scale=2)
            and asset['contact_baseline'] == 528 and asset['godot_import_proof'] is True,
            'Village Edge–Stream geometry/provenance changed')
    proof = REVIEW / 'village-edge-stream-path-correction-overlay.png'
    accepted = rgba(proof)
    require(accepted.size == (224, 528) and digest(proof) == SHA, 'accepted pixels changed')
    require(digest(PROOF / asset['export']) == asset['sha256'] == SHA, 'export bytes changed')
    provenance = manifest['village_edge_stream_path_provenance']
    require(provenance == {
        'accepted_proof': 'review/village-edge-stream-path-correction-overlay.png',
        'accepted_proof_sha256': SHA, 'master': 'masters/terrain-village-edge-stream-path.ora',
        'recipe_sha256': digest(PROOF / 'tools/build_village_edge_stream_path_correction.py'),
        'world_origin': [1640, 440], 'world_position': [1752, 704], 'z_index': -75,
        'import_fix_alpha_border': False,
        'master_scaling': 'nearest-neighbour 2x of exact accepted export',
    }, 'promotion provenance changed')
    validate_master(PROOF / provenance['master'], accepted)
    for directory in ('runtime', 'direction-a-import-proof'):
        texture = ROOT / f'game/art/{directory}/terrain/village-edge-stream-path.png'
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
                    runtime = REVIEW / f'village-edge-stream-path-runtime-{suffix}'
                    proof = REVIEW / f'village-edge-stream-path-correction-{suffix}'
                    require(digest(runtime) == digest(proof), f'runtime PNG differs: {suffix}')
                    image = rgba(runtime)
                    require(image.size == (1280, height) and image.getchannel('A').getextrema() == (255, 255),
                            f'invalid runtime capture: {suffix}')
    print('Village Edge–Stream promotion valid: exact accepted asset, editable master and independent imports.')


def validate_scope(manifest):
    """Check retained baseline, manifest/scene normalization and physical navigation bodies."""
    path = REVIEW / 'village-edge-stream-path-promotion-evidence.json'
    require(digest(path) == RECORD_HASH, 'promotion scope record changed')
    record = json.loads(path.read_text())
    require(len(record['retained_files']) == 441, 'retained baseline file set changed')
    require(len(record['excluded_finder_metadata']) == 7
            and all(relative.endswith('/.DS_Store') for relative in record['excluded_finder_metadata']),
            'only transient Finder metadata may be excluded')
    for relative, sha in record['retained_files'].items():
        require(digest(ROOT / relative) == sha, f'retained file changed: {relative}')
    retained = {k: v for k, v in manifest.items() if k != 'village_edge_stream_path_provenance'}
    retained['assets'] = [a for a in manifest['assets'] if a['id'] != ASSET_ID]
    require(hashlib.sha256((json.dumps(retained, indent=2) + '\n').encode()).hexdigest()
            == record['manifest_sha256'], 'unrelated manifest changed')
    scene = (ROOT / 'game/Main.tscn').read_text()
    resource = '[ext_resource type="Texture2D" path="res://art/runtime/terrain/village-edge-stream-path.png" id="433_runtime_village_edge_stream_path"]\n'
    binding = 'VillageEdgeStreamPathTexture = ExtResource("433_runtime_village_edge_stream_path")\n'
    require(scene.count(resource) == 1 and scene.count(binding) == 1, 'scene binding changed')
    scene = scene.replace(resource, '').replace(binding, '').replace(
        '[gd_scene load_steps=350 format=3]', '[gd_scene load_steps=349 format=3]', 1)
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
