#!/usr/bin/env python3
"""Bind the approved two-layer Shrine path to exact assets, masters and runtime views."""

import argparse
import hashlib
import io
import json
import runpy
import zipfile
from xml.etree import ElementTree

from PIL import Image

from check_shrine_southern_road_path_blend import validate as validate_archived
from check_village_edge_square_connector_feasibility import ROOT, PROOF, REVIEW, require

HELPERS = runpy.run_path(str(ROOT / 'scripts/validate-runtime-terrain.py'))
ASSETS = (
    ('shrine_southern_road_path', 'shrine-southern-road-path',
     'shrine-southern-road-path-correction-overlay.png', 'build_shrine_southern_road_path_correction.py',
     (1152, 1080), (432, 400), (1008, 940), -72,
     '0b73ae24f5f0c985b29399e2889dd12210ec4f221bc827a4cae7da60b635b61e'),
    ('shrine_southern_road_endpoint_blend', 'shrine-southern-road-endpoint-blend',
     'shrine-southern-road-path-blend-overlay.png', 'build_shrine_southern_road_path_blend.py',
     (160, 160), (1400, 1280), (1480, 1360), -69,
     'a01cb5089d67a9e9bf108a2dd18a84b0f98b11c6eb7e7f3d2b3d5844bbabc720'),
)
RECORD_HASH = 'ab3cdd0173086cd3b99591242beaf2d4249c964634b98ca5f1978bab0e793d07'


def digest(path):
    """Hash exact owned bytes."""
    return hashlib.sha256(path.read_bytes()).hexdigest()


def rgba(path):
    """Load exact RGBA pixels with no conversion."""
    with Image.open(path) as image:
        require(image.mode == 'RGBA', f'not RGBA: {path}')
        return image.copy()


def validate_master(path, accepted):
    """Require two editable nonoverlapping layers reconstructing the nearest 2x master."""
    expected = accepted.resize((accepted.width * 2, accepted.height * 2), Image.Resampling.NEAREST)
    with zipfile.ZipFile(path) as archive:
        stack = ElementTree.fromstring(archive.read('stack.xml'))
        require((stack.get('w'), stack.get('h')) == tuple(map(str, expected.size)), 'master size changed')
        layers = list(stack.find('stack'))
        require(len(layers) == 2, 'expected two editable master layers')
        composed = Image.new('RGBA', expected.size)
        half = expected.height // 2
        for layer, name, top in zip(layers, ['accepted lower path', 'accepted upper path'], [half, 0]):
            image = rgba(io.BytesIO(archive.read(layer.get('src'))))
            require(layer.get('name') == name and layer.get('visibility') == 'visible'
                    and layer.get('opacity') == '1.0' and layer.get('composite_op') == 'svg:src-over'
                    and (layer.get('x'), layer.get('y')) == ('0', str(top))
                    and image.size == (expected.width, half)
                    and image.tobytes() == expected.crop((0, top, expected.width, top + half)).tobytes(),
                    'editable master layer changed')
            composed.alpha_composite(image, (0, top))
        merged = rgba(io.BytesIO(archive.read('mergedimage.png')))
        require(composed.tobytes() == expected.tobytes()
                and merged.size == expected.size and merged.tobytes() == expected.tobytes(),
                'master does not reconstruct accepted pixels')


def validate(check_captures=False, check_promotion_scope=False):
    """Validate archive, both independent assets, imports and optional authority/capture parity."""
    validate_archived()
    manifest = json.loads((PROOF / 'asset-manifest.json').read_text())
    for asset_id, filename, proof_name, recipe, size, origin, center, z, sha in ASSETS:
        entries = [a for a in manifest['assets'] if a['id'] == f'terrain.{asset_id}']
        require(len(entries) == 1, f'expected one {asset_id}')
        asset = entries[0]
        rect = dict(x=0, y=0, width=size[0], height=size[1])
        require(asset['export'] == f'exports/terrain/{filename}.png'
                and asset['source'] == f'tools/{recipe}'
                and asset['pivot'] == dict(x=size[0] // 2, y=size[1] // 2)
                and asset['logical_rect'] == rect and asset['export_rect_pixels'] == rect
                and asset['master_dimensions_pixels'] == dict(width=size[0]*2, height=size[1]*2, scale=2)
                and asset['contact_baseline'] == size[1] and asset['godot_import_proof'] is True,
                f'{asset_id} geometry/provenance changed')
        accepted = rgba(REVIEW / proof_name)
        require(accepted.size == size and digest(REVIEW / proof_name) == sha, 'accepted proof changed')
        export = PROOF / asset['export']
        require(digest(export) == asset['sha256'] == sha, 'accepted export bytes changed')
        require(all(pixel == (0, 0, 0, 0) for pixel in accepted.getdata() if pixel[3] == 0),
                'hidden RGB introduced')
        provenance = manifest[f'{asset_id}_provenance']
        require(provenance == {
            'accepted_proof': f'review/{proof_name}', 'accepted_proof_sha256': sha,
            'master': f'masters/terrain-{filename}.ora',
            'recipe_sha256': digest(PROOF / 'tools' / recipe),
            'world_origin': list(origin), 'world_position': list(center), 'z_index': z,
            'import_fix_alpha_border': False,
            'master_scaling': 'nearest-neighbour 2x of exact accepted export',
        }, 'provenance changed')
        validate_master(PROOF / provenance['master'], accepted)
        for directory in ('runtime', 'direction-a-import-proof'):
            texture = ROOT / f'game/art/{directory}/terrain/{filename}.png'
            require(not texture.is_symlink() and digest(texture) == sha, 'import source changed')
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
        for view in ('midpoint', 'shrine-exit', 'junction'):
            for zoom in ('0.8', '1.0', '2.0'):
                for frame, height in (('wide', 720), ('tall', 827)):
                    suffix = f'{view}-{zoom}x-{frame}.png'
                    runtime = REVIEW / f'shrine-southern-road-path-runtime-{suffix}'
                    proof = REVIEW / f'shrine-southern-road-path-blend-{suffix}'
                    require(digest(runtime) == digest(proof), f'runtime PNG differs: {suffix}')
                    image = rgba(runtime)
                    require(image.size == (1280, height) and image.getchannel('A').getextrema() == (255,255),
                            f'invalid runtime capture: {suffix}')
    print('Shrine–Road promotion valid: exact two-layer assets, editable masters and independent imports.')


def validate_scope(manifest):
    """Prove retained files, manifest, scene and physical navigation are unchanged."""
    record_path = REVIEW / 'shrine-southern-road-path-promotion-evidence.json'
    require(digest(record_path) == RECORD_HASH, 'promotion scope record changed')
    record = json.loads(record_path.read_text())
    for relative, sha in record['retained_files'].items():
        require(digest(ROOT / relative) == sha, f'retained file changed: {relative}')
    retained = {k: v for k, v in manifest.items()
                if k not in {f'{a[0]}_provenance' for a in ASSETS}}
    retained['assets'] = [a for a in manifest['assets'] if a['id'] not in {f'terrain.{a[0]}' for a in ASSETS}]
    require(hashlib.sha256((json.dumps(retained, indent=2) + '\n').encode()).hexdigest()
            == record['manifest_sha256'], 'unrelated manifest changed')
    scene = (ROOT / 'game/Main.tscn').read_text()
    for asset_id, filename, *_ in ASSETS:
        resource_id = f'429_runtime_{asset_id}' if asset_id == ASSETS[0][0] else f'430_runtime_{asset_id}'
        resource = f'[ext_resource type="Texture2D" path="res://art/runtime/terrain/{filename}.png" id="{resource_id}"]\n'
        prop = 'ShrineSouthernRoadPathTexture' if asset_id == ASSETS[0][0] else 'ShrineSouthernRoadEndpointBlendTexture'
        binding = f'{prop} = ExtResource("{resource_id}")\n'
        require(scene.count(resource) == 1 and scene.count(binding) == 1, 'scene texture binding changed')
        scene = scene.replace(resource, '').replace(binding, '')
    marker = '[node name="RoadArea" type="Line2D" parent="OpeningRegionSouthernRoadWagonSceneSource"]\nvisible = false\n'
    require(scene.count(marker) == 1, 'upstream fill visibility changed')
    scene = scene.replace(marker, marker.replace('visible = false\n', '')).replace(
        '[gd_scene load_steps=347 format=3]', '[gd_scene load_steps=345 format=3]', 1)
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
