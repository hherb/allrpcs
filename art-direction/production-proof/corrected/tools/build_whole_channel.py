#!/usr/bin/env python3
"""Reproduce/install only the five approved whole-channel presentation assets."""

import hashlib
import importlib.util
import json
from pathlib import Path
import sys

ROOT = Path(__file__).resolve().parents[4]
PRODUCTION = ROOT / 'art-direction/production-proof/corrected'
PREVIEW = ROOT / 'art-direction/whole-channel-runtime-preview'


def sha(path):
    """Bind exact accepted bytes."""
    return hashlib.sha256(path.read_bytes()).hexdigest()


def compose():
    """Derive transparent atlases from retained source layers without invoking old scope checks."""
    recipe = PREVIEW / 'tools/layers.py'
    sys.path.insert(0, str(recipe.parent))
    try:
        spec = importlib.util.spec_from_file_location('production_channel_layers', recipe)
        module = importlib.util.module_from_spec(spec)
        spec.loader.exec_module(module)
        runtime, _, _ = module.derive()
        return runtime
    finally:
        sys.path.pop(0)


def install():
    """Append only named entries; preserve exact proof PNG/master bytes and old manifest content."""
    specification = json.loads((PREVIEW / 'stage-2-specification.json').read_text())
    runtime = compose()
    manifest_path = PRODUCTION / 'asset-manifest.json'
    manifest = json.loads(manifest_path.read_text())
    new_ids = {a['asset_id'] for a in specification['assets']}
    manifest['assets'] = [a for a in manifest['assets'] if a['id'] not in new_ids]
    if len(manifest['assets']) != 84:
        raise ValueError('unexpected retained production inventory')
    for asset in specification['assets']:
        name = asset['asset_id'].removeprefix('terrain.whole_channel_')
        source = PREVIEW / 'exports' / asset['export_filename']
        master = PREVIEW / 'masters' / f'channel-{name}.ora'
        from PIL import Image
        with Image.open(source) as image:
            if image.convert('RGBA').tobytes() != runtime[name].tobytes():
                raise ValueError(f'{name}: recipe/source parity changed')
        if sha(source) != asset['export_sha256'] or sha(master) != asset['master_sha256']:
            raise ValueError(f'{name}: approved bytes changed')
        export_path = f"exports/terrain/{asset['export_filename']}"
        (PRODUCTION / export_path).write_bytes(source.read_bytes())
        (PRODUCTION / 'masters' / asset['master_filename']).write_bytes(master.read_bytes())
        rect = dict(x=0, y=0, width=2604, height=620)
        manifest['assets'].append({
            'id': asset['asset_id'], 'category': 'terrain', 'export': export_path,
            'master_dimensions_pixels': dict(width=5208, height=1240, scale=2),
            'logical_rect': rect, 'export_rect_pixels': rect, 'pivot': dict(x=1302, y=310),
            'contact_baseline': 620, 'source': 'tools/build_whole_channel.py',
            'sha256': asset['export_sha256'], 'godot_import_proof': True,
        })
    manifest['whole_channel_provenance'] = {
        'specification': 'art-direction/whole-channel-runtime-preview/stage-2-specification.json',
        'specification_sha256': sha(PREVIEW / 'stage-2-specification.json'),
        'recipe_sha256': sha(Path(__file__)), 'retained_recipe_sha256': sha(PREVIEW / 'tools/layers.py'),
        'world_origin': [-64, 212], 'world_position': [1238, 522],
        'import_fix_alpha_border': False, 'master_scaling': 'nearest-neighbour 2x of exact approved export',
    }
    manifest_path.write_text(json.dumps(manifest, indent=2) + '\n')


if __name__ == '__main__':
    install()
