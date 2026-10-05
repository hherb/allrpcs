#!/usr/bin/env python3
"""Validate exact approved Laundry additions while preserving every predecessor owner."""

import argparse
from io import BytesIO
import hashlib
import importlib.util
import json
import os
from pathlib import Path
import subprocess
import sys
import tarfile
import tempfile
import zipfile
from xml.etree.ElementTree import fromstring

from PIL import Image

ROOT = Path(__file__).resolve().parents[1]
PACKET = 'art-direction/laundry-ground-runtime-promotion/'
SCOPE = 'art-direction/laundry-ground-runtime-scope/'
PRODUCTION = 'art-direction/production-proof/corrected/'
MODIFIED = {'HANDOVER.md', 'README.md', 'ROADMAP.md', 'game/Main.tscn', 'game/DirectionAImportProof.tscn',
            PRODUCTION + 'asset-manifest.json', 'scripts/quality.sh', 'scripts/validate-direction-a-import-proof.py',
            'game/art/direction-a-import-proof/README.md'}
NEW = {SCOPE + 'owner-approval.json', PRODUCTION + 'tools/build_laundry_ground.py', 'scripts/validate-laundry-ground-runtime.py',
       'scripts/capture-laundry-ground-runtime.gd', 'scripts/capture-laundry-ground-runtime.gd.uid',
       'game/RuntimeLaundryGroundSmokeCheck.cs', 'game/RuntimeLaundryGroundSmokeCheck.cs.uid'}
MAIN_RESOURCE = '\n[ext_resource type="Texture2D" path="res://art/runtime/terrain/laundry-entrance-ground.png" id="454_laundry_ground"]\n[ext_resource type="Script" path="res://RuntimeLaundryGroundSmokeCheck.cs" id="455_laundry_ground_check"]\n'
MAIN_NODES = '\n[node name="LaundryGroundPresentation" type="Node2D" parent="."]\nposition = Vector2(1564, 219)\n\n[node name="PackedEarth" type="Sprite2D" parent="LaundryGroundPresentation"]\nz_index = -78\ntexture_filter = 2\ntexture_repeat = 1\ntexture = ExtResource("454_laundry_ground")\ncentered = false\n\n[node name="RuntimeLaundryGroundSmokeCheck" type="Node" parent="."]\nscript = ExtResource("455_laundry_ground_check")\n'
PROOF_RESOURCE = '\n[ext_resource type="Texture2D" path="res://art/direction-a-import-proof/terrain/laundry-entrance-ground.png" id="95_laundry_ground"]\n'
PROOF_NODES = '\n[node name="LaundryGroundProof" type="Sprite2D" parent="Terrain"]\nposition = Vector2(5400, 3500)\ntexture_filter = 2\ntexture_repeat = 1\ntexture = ExtResource("95_laundry_ground")\ncentered = false\n'


def require(condition, message):
    """Fail closed on paint, authority, ownership or preservation drift."""
    if not condition:
        raise ValueError(message)


def sha(path):
    """Hash exact persisted bytes."""
    return hashlib.sha256(path.read_bytes()).hexdigest()


def read(root, name):
    """Read immutable approval/specification data."""
    return json.loads((root / name).read_text())


def original(commit, name):
    """Read predecessor authority from the actual Git object store."""
    return subprocess.check_output(['git', 'show', commit + ':' + name], cwd=ROOT)


def options(path):
    """Read scalar import processing independently from Godot."""
    section, values = '', {}
    for line in path.read_text().splitlines():
        if line.startswith('[') and line.endswith(']'):
            section = line[1:-1]
        elif '=' in line:
            name, value = line.split('=', 1)
            values[section, name] = value
    return values


def tool_additions(root, commit):
    """Permit only scoped Laundry checks plus frozen Shrine dispatch and one new asset."""
    quality = original(commit, 'scripts/quality.sh').decode()
    quality = quality.replace('python3 scripts/validate-shrine-ground-runtime.py --check-rejections',
                              'python3 scripts/validate-laundry-ground-runtime.py --historical-shrine\n'
                              'python3 scripts/validate-laundry-ground-runtime.py --check-rejections')
    quality = quality.replace('ALLRPCS_RUNTIME_SHRINE_GROUND_CHECK=1 godot --headless --path game --quit-after 30',
                              'ALLRPCS_RUNTIME_SHRINE_GROUND_CHECK=1 godot --headless --path game --quit-after 30\n'
                              'ALLRPCS_RUNTIME_LAUNDRY_GROUND_CHECK=1 godot --headless --path game --quit-after 30')
    require((root / 'scripts/quality.sh').read_text() == quality, 'Unscoped quality orchestration change')
    imports = original(commit, 'scripts/validate-direction-a-import-proof.py').decode()
    imports = imports.replace('== 95', '== 96').replace('expected 95', 'expected 96')
    imports = imports.replace('"terrain.shrine_entrance_ground",',
                              '"terrain.shrine_entrance_ground", "terrain.laundry_entrance_ground",')
    require((root / 'scripts/validate-direction-a-import-proof.py').read_text() == imports,
            'Unscoped import-validator change')


def validate(root=ROOT, preservation=True):
    """Check approved inputs, append-only production, masters, scenes and imports."""
    auth = read(root, PACKET + 'authorization.json')
    for name, key in [('README.md', 'scope_sha256'), ('source-bindings.json', 'bindings_sha256'),
                      ('owner-approval.json', 'owner_approval_sha256')]:
        require(sha(root / SCOPE / name) == auth[key], 'Approved scope changed')
    require(sha(root / PACKET / 'baseline.json') == auth['baseline_sha256'], 'Baseline drift')
    baseline = read(root, PACKET + 'baseline.json')
    require(baseline['commit'] == auth['baseline_commit'], 'Baseline commit drift')
    require(auth['status'] == 'named one-asset Laundry runtime scope approved', 'Named approval missing')
    tool_additions(root, baseline['commit'])
    spec = read(root, SCOPE + 'source-bindings.json')
    asset = spec['asset']
    for name, expected in spec['source_sha256'].items():
        if name.startswith('art-direction/laundry-ground-feasibility/') and Path(name).name != '.DS_Store':
            require(sha(root / name) == expected, 'Selected proof changed: ' + name)
    for name, expected in baseline['files'].items():
        if name not in MODIFIED:
            require((root / name).is_file() and sha(root / name) == expected, 'Protected predecessor changed: ' + name)
    added = set(NEW)
    added.update(asset[key] for key in ('production_export', 'production_master', 'proof_texture', 'runtime_texture'))
    added.update(asset[key] + '.import' for key in ('proof_texture', 'runtime_texture'))
    if preservation:
        names = subprocess.check_output(['git', 'ls-files', '-co', '--exclude-standard', '-z'], cwd=ROOT).decode().split('\0')
        require(all(not name or name in baseline['files'] or name in added or name.startswith(PACKET) for name in names),
                'Unscoped new file')
        tree = subprocess.check_output(['git', 'ls-tree', '-r', '--name-only', baseline['commit']], cwd=ROOT, text=True).splitlines()
        require(set(baseline['files']) == {name for name in tree if Path(name).name != '.DS_Store'}, 'Incomplete baseline')
    for filename, resource, nodes, before, after in [('Main.tscn', MAIN_RESOURCE, MAIN_NODES, 370, 372),
                                                   ('DirectionAImportProof.tscn', PROOF_RESOURCE, PROOF_NODES, 94, 95)]:
        live = (root / 'game' / filename).read_text()
        require(live.count(resource) == live.count(nodes) == 1, 'Scene additions missing/duplicated')
        declarations_end = min(live.index(tag) for tag in ('\n[sub_resource', '\n[node') if tag in live)
        require(live.index(resource) < declarations_end, 'Texture/script resources must precede subresources and nodes')
        normalized = live.replace(resource, '', 1).replace(nodes, '', 1).replace(f'load_steps={after}', f'load_steps={before}', 1)
        require(normalized.encode() == original(baseline['commit'], 'game/' + filename), 'Old scene/physical/order drift: ' + filename)
        if filename == 'Main.tscn':
            require(live.endswith(original(baseline['commit'], 'game/Main.tscn').decode().split('[node name="WholeChannelPresentation"')[1]),
                    'WholeChannelPresentation ceased to be final root child')
    manifest = read(root, PRODUCTION + 'asset-manifest.json')
    retained = {key: value for key, value in manifest.items() if key != 'laundry_ground_provenance'}
    selected = [entry for entry in manifest['assets'] if entry['id'] == asset['asset_id']]
    require(len(selected) == 1 and selected[0]['sha256'] == asset['source_sha256'], 'New manifest entry drift')
    rectangle = dict(x=0, y=0, width=92, height=210)
    require(selected[0] == dict(id='terrain.laundry_entrance_ground', category='terrain',
            export='exports/terrain/laundry-entrance-ground.png',
            master_dimensions_pixels=dict(width=184, height=420, scale=2),
            logical_rect=rectangle, export_rect_pixels=rectangle, pivot=dict(x=0, y=0),
            contact_baseline=210, source='tools/build_laundry_ground.py',
            sha256=asset['source_sha256'], godot_import_proof=True), 'New manifest metadata drift')
    retained['assets'] = [entry for entry in manifest['assets'] if entry['id'] != asset['asset_id']]
    require(retained == json.loads(original(baseline['commit'], PRODUCTION + 'asset-manifest.json')), 'Retained manifest changed')
    require(len(manifest['assets']) == 96 and len(list((root / PRODUCTION / 'masters').glob('*.ora'))) == 47, 'Asset/master count drift')
    for key in ('source', 'production_export', 'proof_texture', 'runtime_texture'):
        require(sha(root / asset[key]) == asset['source_sha256'], 'Accepted PNG byte drift: ' + key)
    source = Image.open(root / asset['source']).convert('RGBA').resize((184, 420), Image.Resampling.NEAREST)
    raw = Image.open(root / asset['world_paint']).convert('RGBA').resize((184, 420), Image.Resampling.NEAREST)
    approval = read(root, 'art-direction/laundry-ground-feasibility/owner-selection.json')
    require(sha(root / asset['production_master']) == approval['selected_files']['masters/feather-24.ora'],
            'Selected master bytes changed')
    with zipfile.ZipFile(root / asset['production_master']) as archive:
        xml = fromstring(archive.read('stack.xml'))
        require((xml.attrib['w'], xml.attrib['h']) == ('184', '420'), 'Master geometry drift')
        layers = list(xml.find('stack'))
        require([node.attrib['name'] for node in layers] == ['Corrections', 'Masked packed earth', 'Coverage reference', 'Unmasked world paint'],
                'Editable master layers drift')
        require([node.attrib['visibility'] for node in layers] == ['visible', 'visible', 'hidden', 'hidden'], 'Master visibility drift')
        require(all(node.attrib['x'] == node.attrib['y'] == '0' and node.attrib['opacity'] == '1.0'
                    and node.attrib['composite-op'] == 'svg:src-over' for node in layers), 'Master blend/placement drift')
        images = [Image.open(BytesIO(archive.read(node.attrib['src']))).convert('RGBA') for node in layers]
        require(all(image.size == (184, 420) for image in images), 'Master layer dimensions drift')
        require(images[0].getbbox() is None and images[1].tobytes() == source.tobytes()
                and images[2].getchannel('R').tobytes() == source.getchannel('A').tobytes()
                and images[3].tobytes() == raw.tobytes(), 'Editable paint/mask drift')
        require(Image.open(BytesIO(archive.read('mergedimage.png'))).convert('RGBA').tobytes() == source.tobytes(), 'Master merge drift')
    old_uids = {options(root / name).get(('remap', 'uid')) for name in baseline['files'] if name.endswith('.png.import')}
    for key in ('proof_texture', 'runtime_texture'):
        path = root / asset[key]
        values = options(path.with_suffix('.png.import'))
        resource = 'res://' + str(Path(asset[key]).relative_to('game'))
        cache = 'res://.godot/imported/' + path.name + '-' + hashlib.md5(resource.encode(), usedforsecurity=False).hexdigest() + '.ctex'
        uid = values.get(('remap', 'uid'))
        require(uid and uid.startswith('"uid://') and uid.endswith('"') and uid not in old_uids, 'Import UID reused/missing')
        old_uids.add(uid)
        require(values.get(('remap', 'importer')) == '"texture"'
                and values.get(('remap', 'type')) == '"CompressedTexture2D"', 'Import resource type drift')
        require(values.get(('deps', 'source_file')) == '"' + resource + '"'
                and values.get(('remap', 'path')) == '"' + cache + '"'
                and values.get(('deps', 'dest_files')) == '["' + cache + '"]', 'Import/cache paths drift')
        for name, expected in spec['import_parameters'].items():
            require(values.get(('params', name)) == expected, 'Import processing drift: ' + name)
    old_uids.update('"' + (root / name).read_text().strip() + '"'
                    for name in baseline['files'] if name.endswith('.uid'))
    for name in ('game/RuntimeLaundryGroundSmokeCheck.cs.uid', 'scripts/capture-laundry-ground-runtime.gd.uid'):
        uid = (root / name).read_text().strip()
        require(uid.startswith('uid://') and '"' + uid + '"' not in old_uids, 'Script UID missing/reused')
        old_uids.add('"' + uid + '"')
    print('PASS: exact Laundry source/master/imports, 96 assets/47 masters, normalized scenes and preserved predecessor')


def historical_shrine():
    """Run unchanged accepted Shrine checks and the whole inherited frozen art chain."""
    revision = '5e5e3cab5a45a6461f6d804c871d09fee5f423b9'
    with tempfile.TemporaryDirectory(prefix='laundry-shrine-predecessor-') as directory:
        snapshot = Path(directory) / 'tree'
        snapshot.mkdir()
        archive = Path(directory) / 'tree.tar'
        with archive.open('wb') as output:
            subprocess.run(['git', 'archive', revision], cwd=ROOT, stdout=output, check=True)
        with tarfile.open(archive) as package:
            package.extractall(snapshot, filter='data')
        (snapshot / '.git').symlink_to(ROOT / '.git', target_is_directory=True)
        env = dict(os.environ, GIT_INDEX_FILE=str(Path(directory) / 'index'))
        subprocess.run(['git', 'read-tree', revision], cwd=snapshot, env=env, check=True)
        for flags in (['--historical-wagon'], ['--check-captures', '--check-rejections']):
            subprocess.run([sys.executable, 'scripts/validate-shrine-ground-runtime.py', *flags],
                           cwd=snapshot, env=env, check=True)
    print('PASS: frozen Shrine checks on approved', revision)


def rejections():
    """Use disposable baseline copies to reject paint/phase/depth/import/physical drift."""
    import shutil
    baseline = read(ROOT, PACKET + 'baseline.json')
    spec = read(ROOT, SCOPE + 'source-bindings.json')
    required = set(baseline['files']) | {SCOPE + 'owner-approval.json', PACKET + 'authorization.json', PACKET + 'baseline.json'}
    required.update(spec['asset'][key] for key in ('production_export', 'production_master', 'proof_texture', 'runtime_texture'))
    required.update(spec['asset'][key] + '.import' for key in ('proof_texture', 'runtime_texture'))
    required.update(NEW)
    required.update({'game/RuntimeLaundryGroundSmokeCheck.cs.uid', 'scripts/capture-laundry-ground-runtime.gd.uid'})
    with tempfile.TemporaryDirectory(prefix='square-rejections-') as directory:
        fixture = Path(directory)
        for name in required:
            destination = fixture / name
            destination.parent.mkdir(parents=True, exist_ok=True)
            shutil.copy2(ROOT / name, destination)
        validate(fixture, preservation=False)
        scene = fixture / 'game/Main.tscn'
        original_scene = scene.read_bytes()
        mutations = [('phase', 'position = Vector2(1564, 219)', 'position = Vector2(1565, 219)'),
                     ('depth', 'z_index = -78\ntexture_filter = 2\ntexture_repeat = 1\ntexture = ExtResource("454_laundry_ground")',
                      'z_index = -77\ntexture_filter = 2\ntexture_repeat = 1\ntexture = ExtResource("454_laundry_ground")'),
                     ('duplicate owner', MAIN_NODES, MAIN_NODES + MAIN_NODES),
                     ('dropped owner', MAIN_NODES, ''),
                     ('transform', 'texture = ExtResource("454_laundry_ground")\ncentered = false',
                      'texture = ExtResource("454_laundry_ground")\ncentered = false\nscale = Vector2(2, 1)'),
                     ('extent', 'texture = ExtResource("454_laundry_ground")\ncentered = false',
                      'texture = ExtResource("454_laundry_ground")\ncentered = false\nregion_enabled = true\nregion_rect = Rect2(0, 0, 93, 210)'),
                     ('physical scope', 'position = Vector2(2300, 1300)', 'position = Vector2(2301, 1300)')]
        for name, before, after in mutations:
            scene.write_text(original_scene.decode().replace(before, after, 1))
            try:
                validate(fixture, preservation=False)
            except ValueError:
                print('PASS: rejects', name)
            else:
                raise ValueError('Mutation accepted: ' + name)
            scene.write_bytes(original_scene)
        proof_scene = fixture / 'game/DirectionAImportProof.tscn'
        original_proof = proof_scene.read_bytes()
        proof_scene.write_text(original_proof.decode().replace(PROOF_RESOURCE, '', 1) + PROOF_RESOURCE)
        try:
            validate(fixture, preservation=False)
        except ValueError:
            print('PASS: rejects proof resource order')
        else:
            raise ValueError('Mutation accepted: proof resource order')
        proof_scene.write_bytes(original_proof)
        for name, path, mutate in [('paint', spec['asset']['runtime_texture'], lambda data: data + b'changed'),
                                   ('mask/source', spec['asset']['coverage'], lambda data: data + b'changed'),
                                   ('import processing', spec['asset']['runtime_texture'] + '.import',
                                    lambda data: data.replace(b'process/fix_alpha_border=false', b'process/fix_alpha_border=true'))]:
            target = fixture / path
            original_data = target.read_bytes()
            target.write_bytes(mutate(original_data))
            try:
                validate(fixture, preservation=False)
            except ValueError:
                print('PASS: rejects', name)
            else:
                raise ValueError('Mutation accepted: ' + name)
            target.write_bytes(original_data)


def captures():
    """Require all six actual imported runtime views and baselines to match external pixels."""
    import math
    import numpy as np
    evidence = read(ROOT, PACKET + 'capture-evidence.json')
    expected_inputs = {'res://Main.tscn', 'res://RuntimeLaundryGroundSmokeCheck.cs',
                       'res://art/runtime/terrain/laundry-entrance-ground.png',
                       'res://art/runtime/terrain/laundry-entrance-ground.png.import',
                       'scripts/capture-laundry-ground-runtime.gd'}
    require(set(evidence['inputs']) == expected_inputs, 'Runtime capture input set drift')
    for name, expected in evidence['inputs'].items():
        path = ROOT / 'game' / name.removeprefix('res://') if name.startswith('res://') else ROOT / name
        require(sha(path) == expected, 'Runtime capture source drift: ' + name)
    names = {f'captures/{variant}-{zoom:.1f}x-{frame}.png'
             for variant in ('baseline', 'runtime') for zoom in (0.8, 1.0, 2.0) for frame in ('wide', 'tall')}
    require(set(evidence['captures']) == set(evidence['frames']) == names, 'Runtime capture matrix incomplete')
    require(evidence['center'] == [1620, 320], 'Capture centre drift')
    proof = ROOT / 'art-direction/laundry-ground-feasibility'
    support = np.asarray(Image.open(proof / 'proof-inputs/feather-24-layer.png').convert('RGBA'))[:, :, 3] > 0
    origin = read(ROOT, SCOPE + 'source-bindings.json')['owner']['position']
    results = []
    for zoom in (0.8, 1.0, 2.0):
        for frame, height in [('wide', 720), ('tall', 827)]:
            base_name = f'captures/baseline-{zoom:.1f}x-{frame}.png'
            runtime_name = f'captures/runtime-{zoom:.1f}x-{frame}.png'
            transform = evidence['frames'][base_name]
            external_meta = read(proof, 'capture-evidence.json')['frames']
            require(transform == external_meta[base_name] == external_meta[f'captures/feather-24-{zoom:.1f}x-{frame}.png'], 'External camera transform drift')
            require(transform == evidence['frames'][runtime_name] and transform['viewport'] == [1280, height]
                    and transform['zoom'] == zoom, 'Unpaired runtime camera')
            a, b, c, d, tx, ty = transform['world_to_canvas']
            require(abs(a - zoom) < 1e-6 and abs(d - zoom) < 1e-6 and b == c == 0, 'Unsupported capture transform')
            radius = 2
            pad = np.pad(support, radius)
            for _ in range(radius):
                p = np.pad(pad, 1)
                mask_height, mask_width = pad.shape
                pad = np.logical_or.reduce([p[y:y + mask_height, x:x + mask_width] for y in range(3) for x in range(3)])
            x = np.floor((np.arange(1280) + 0.5 - tx) / zoom - origin[0]).astype(int) + radius
            y = np.floor((np.arange(height) + 0.5 - ty) / zoom - origin[1]).astype(int) + radius
            valid = (x[None, :] >= 0) & (x[None, :] < pad.shape[1]) & (y[:, None] >= 0) & (y[:, None] < pad.shape[0])
            allowed = pad[np.clip(y, 0, pad.shape[0] - 1)[:, None], np.clip(x, 0, pad.shape[1] - 1)[None, :]] & valid
            pixels = []
            row = {'zoom': zoom, 'frame': frame}
            for variant, name, external in [('baseline', base_name, 'baseline'), ('runtime', runtime_name, 'feather-24')]:
                path = ROOT / PACKET / name
                require(sha(path) == evidence['captures'][name], 'Runtime PNG hash drift')
                with Image.open(path) as image:
                    require(image.size == (1280, height), 'Runtime PNG dimensions drift')
                    candidate = np.asarray(image.convert('RGBA'))
                with Image.open(proof / f'captures/{external}-{zoom:.1f}x-{frame}.png') as image:
                    reference = np.asarray(image.convert('RGBA'))
                delta = int(np.any(candidate != reference, axis=2).sum())
                require(delta == 0, 'Runtime/external pixels differ: ' + name)
                row[variant + '_external_changed_pixels'] = delta
                pixels.append(candidate)
            changed = np.any(pixels[0] != pixels[1], axis=2)
            require(changed.any() and not np.any(changed & ~allowed), 'Runtime difference outside accepted support')
            row['changed_pixels'] = int(changed.sum())
            results.append(row)
    print(json.dumps(results, indent=2))
    return results


def main():
    """Run only the requested bounded successor or pinned historical checks."""
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--historical-shrine', action='store_true')
    parser.add_argument('--check-rejections', action='store_true')
    parser.add_argument('--check-captures', action='store_true')
    args = parser.parse_args()
    if args.historical_shrine:
        historical_shrine()
        return
    subprocess.run([sys.executable, str(ROOT / PRODUCTION / 'tools/build_laundry_ground.py'), '--check'], check=True)
    validate()
    if args.check_rejections:
        rejections()
    if args.check_captures:
        captures()


if __name__ == '__main__':
    main()
