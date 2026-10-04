#!/usr/bin/env python3
"""Validate the approved Garden successor without weakening frozen predecessor checks."""

import argparse
from io import BytesIO
import hashlib
import json
import os
from pathlib import Path
import re
import subprocess
import sys
import tarfile
import tempfile
import zipfile
from xml.etree import ElementTree

from PIL import Image

ROOT = Path(__file__).resolve().parents[1]
PACKET = 'art-direction/garden-ground-runtime-promotion/'
SCOPE = 'art-direction/garden-ground-runtime-scope/'
PRODUCTION = 'art-direction/production-proof/corrected/'
MODIFIED = {'HANDOVER.md', 'README.md', 'ROADMAP.md', PRODUCTION + 'asset-manifest.json',
            'game/Main.tscn', 'game/DirectionAImportProof.tscn', 'scripts/quality.sh',
            'scripts/validate-direction-a-import-proof.py', 'game/art/direction-a-import-proof/README.md'}
NEW = {PRODUCTION + 'tools/build_garden_ground.py', 'scripts/validate-garden-ground-runtime.py',
       'scripts/capture-garden-ground-runtime.gd', 'scripts/capture-garden-ground-runtime.gd.uid',
       'game/RuntimeGardenGroundSmokeCheck.cs', 'game/RuntimeGardenGroundSmokeCheck.cs.uid'}
HISTORICAL_HEAD = '2fcc8492328a3a497d4e9ddee25c944ed1dd07b6'
AUTH_SHA = 'd6c9e2f9f9e58454a1c2c214467c375b03f64dd83e27f65b0431a8a0ee1bd65a'
BASELINE_SHA = 'e22a66d99864dea22695a1d38526585aec0ca1cf1b8b319770ab94f6be8fe1f6'


def require(condition, message):
    """Fail closed on drift or an unapproved ownership change."""
    if not condition:
        raise ValueError(message)


def sha(path):
    """Hash exact authored bytes."""
    return hashlib.sha256(path.read_bytes()).hexdigest()


def read(root, name):
    """Read one packet without modifying historical evidence."""
    return json.loads((root / name).read_text())


def original(commit, name):
    """Read authoritative predecessor bytes directly from the local Git object store."""
    return subprocess.check_output(['git', 'show', commit + ':' + name], cwd=ROOT)


def import_options(path):
    """Read scalar texture-import options independently from Godot."""
    section, result = '', {}
    for line in path.read_text().splitlines():
        line = line.strip()
        if line.startswith('[') and line.endswith(']'):
            section = line[1:-1]
        elif '=' in line and not line.endswith('{'):
            key, value = line.split('=', 1)
            result[section, key] = value
    return result


def scenes(root, commit):
    """Require every old resource/node byte and only the exact new presentation blocks."""
    for filename, old_steps, new_steps in [('Main.tscn', 359, 362), ('DirectionAImportProof.tscn', 88, 90)]:
        live = (root / 'game' / filename).read_text()
        predecessor = original(commit, 'game/' + filename).decode()
        if filename == 'Main.tscn':
            addition = '\n[node name="GardenGroundPresentation" type="Node2D" parent="."]\nposition = Vector2(2048, 1120)\n'
            for name, z, identifier in [('Meadow', -90, '443_garden_meadow'), ('Soil', -89, '444_garden_soil')]:
                addition += (f'\n[node name="{name}" type="Sprite2D" parent="GardenGroundPresentation"]\n'
                             f'z_index = {z}\ntexture_filter = 2\ntexture_repeat = 1\n'
                             f'texture = ExtResource("{identifier}")\ncentered = false\n')
            addition += '\n[node name="RuntimeGardenGroundSmokeCheck" type="Node" parent="."]\nscript = ExtResource("445_garden_ground_check")\n'
            resource = '\n[ext_resource type="Texture2D" path="res://art/runtime/terrain/garden-ground-meadow.png" id="443_garden_meadow"]\n[ext_resource type="Texture2D" path="res://art/runtime/terrain/garden-ground-soil.png" id="444_garden_soil"]\n[ext_resource type="Script" path="res://RuntimeGardenGroundSmokeCheck.cs" id="445_garden_ground_check"]\n'
        else:
            addition = ''
            for index, name in enumerate(('Meadow', 'Soil'), 89):
                addition += (f'\n[node name="GardenGround{name}Proof" type="Sprite2D" parent="Terrain"]\n'
                             f'position = Vector2(4600, {(index-89)*450+2000})\ntexture_filter = 2\n'
                             f'texture_repeat = 1\ntexture = ExtResource("{index}_garden_{name.lower()}")\ncentered = false\n')
            resource = '\n[ext_resource type="Texture2D" path="res://art/direction-a-import-proof/terrain/garden-ground-meadow.png" id="89_garden_meadow"]\n[ext_resource type="Texture2D" path="res://art/direction-a-import-proof/terrain/garden-ground-soil.png" id="90_garden_soil"]\n'
        expected = predecessor.replace(f'load_steps={old_steps}', f'load_steps={new_steps}', 1)
        expected = expected.replace('\n[sub_resource', resource + '\n[sub_resource', 1)
        if filename == 'Main.tscn':
            expected = expected.replace('\n[node name="WholeChannelPresentation"',
                                        addition + '\n[node name="WholeChannelPresentation"', 1)
        else:
            expected += addition
        require(live == expected, 'Unapproved scene/source/depth/ownership change: ' + filename)


def validate(root=ROOT, preservation=True):
    """Verify authorization, complete retained tree and exact asset/master/import contracts."""
    require(sha(root / PACKET / 'authorization.json') == AUTH_SHA, 'Garden authorization changed')
    require(sha(root / PACKET / 'baseline.json') == BASELINE_SHA, 'Frozen predecessor inventory changed')
    auth = read(root, PACKET + 'authorization.json')
    for name, key in [('README.md', 'scope_sha256'), ('source-bindings.json', 'bindings_sha256')]:
        require(sha(root / SCOPE / name) == auth[key], 'Approved scope bytes changed')
    require(sha(root / PACKET / 'baseline.json') == auth['baseline_sha256'], 'Predecessor inventory changed')
    spec = read(root, SCOPE + 'source-bindings.json')
    baseline = read(root, PACKET + 'baseline.json')
    require(baseline['commit'] == auth['baseline_commit'], 'Predecessor commit changed')
    names = subprocess.check_output(['git', 'ls-tree', '-r', '--name-only', baseline['commit']], cwd=ROOT, text=True).splitlines()
    require(set(baseline['files']) == {n for n in names if Path(n).name != '.DS_Store'}, 'Incomplete predecessor inventory')
    added = set(NEW)
    for asset in spec['assets']:
        added.update(asset[k] for k in ('production_export', 'production_master', 'proof_texture', 'runtime_texture'))
        added.update(asset[k] + '.import' for k in ('proof_texture', 'runtime_texture'))
    for name, digest in baseline['files'].items():
        if name not in MODIFIED:
            require((root / name).is_file() and sha(root / name) == digest, 'Protected predecessor changed: ' + name)
    if preservation:
        paths = subprocess.check_output(['git', 'ls-files', '-co', '--exclude-standard', '-z'], cwd=ROOT).decode().split('\0')
        require(all(not n or n in baseline['files'] or n in added or n.startswith(PACKET)
                    for n in paths), 'Unscoped new file')
    manifest = read(root, PRODUCTION + 'asset-manifest.json')
    retained = {k: v for k, v in manifest.items() if k != 'garden_ground_provenance'}
    ids = {a['asset_id'] for a in spec['assets']}
    retained['assets'] = [a for a in manifest['assets'] if a['id'] not in ids]
    require(retained == json.loads(original(baseline['commit'], PRODUCTION + 'asset-manifest.json')), 'Retained manifest changed')
    require(len(manifest['assets']) == 91 and len(list((root / PRODUCTION / 'masters').glob('*.ora'))) == 42,
            'Garden asset/master inventory differs')
    scenes(root, baseline['commit'])
    old_uids = {import_options(root / name).get(('remap', 'uid'))
                for name in baseline['files'] if name.endswith('.png.import')}
    uids = set(old_uids)
    for asset in spec['assets']:
        entry = next(a for a in manifest['assets'] if a['id'] == asset['asset_id'])
        require(entry['sha256'] == asset['source_sha256'] and entry['pivot'] == dict(x=0, y=0)
                and entry['master_dimensions_pixels'] == dict(width=1024, height=768, scale=2)
                and entry['export_rect_pixels'] == dict(x=0, y=0, width=512, height=384)
                and entry['source'] == 'tools/build_garden_ground.py', 'Garden manifest contract changed')
        for key in ('source', 'production_export', 'proof_texture', 'runtime_texture'):
            path = root / asset[key]
            require(path.is_file(), 'Missing Garden texture')
            if root == ROOT:
                require(not path.is_symlink(), 'Garden texture must be owned, not linked')
            require(sha(path) == asset['source_sha256'], 'Selected Garden layer bytes changed: ' + key)
            with Image.open(path) as image:
                require(image.mode == 'RGBA' and image.size == (512, 384), 'Garden native geometry changed')
        with Image.open(root / asset['source']) as source:
            expected = source.resize((1024, 768), Image.Resampling.NEAREST)
        with zipfile.ZipFile(root / asset['production_master']) as archive:
            require(archive.testzip() is None and archive.read('mimetype') == b'image/openraster', 'Invalid Garden master')
            xml = ElementTree.fromstring(archive.read('stack.xml'))
            require(xml.attrib['w'] == '1024' and xml.attrib['h'] == '768', 'Garden master dimensions changed')
            layers = list(xml.find('stack'))
            require([a.attrib['name'] for a in layers] == ['Corrections', 'Coverage (editing reference)', 'Accepted masked field paint'], 'Master editing planes changed')
            composite = Image.new('RGBA', expected.size)
            for layer in reversed(layers):
                require(layer.attrib['x'] == layer.attrib['y'] == '0' and layer.attrib['opacity'] == '1.0'
                        and layer.attrib['composite-op'] == 'svg:src-over', 'Master layer transform changed')
                with Image.open(BytesIO(archive.read(layer.attrib['src']))) as image:
                    if layer.attrib['visibility'] == 'visible':
                        composite.alpha_composite(image)
                    else:
                        require(image.getchannel('R').tobytes() == expected.getchannel('A').tobytes(), 'Independent master coverage changed')
            with Image.open(BytesIO(archive.read('mergedimage.png'))) as merge:
                require(merge.tobytes() == composite.tobytes() == expected.tobytes(), 'Master/source merge mismatch')
        for key in ('proof_texture', 'runtime_texture'):
            path = root / asset[key]
            options = import_options(path.with_suffix('.png.import'))
            res = 'res://' + str(Path(asset[key]).relative_to('game'))
            cache = 'res://.godot/imported/' + path.name + '-' + hashlib.md5(res.encode(), usedforsecurity=False).hexdigest() + '.ctex'
            require(options.get(('deps', 'source_file')) == '"' + res + '"'
                    and options.get(('remap', 'path')) == '"' + cache + '"'
                    and options.get(('deps', 'dest_files')) == '["' + cache + '"]', 'Garden import paths changed')
            uid = options.get(('remap', 'uid'))
            require(uid and uid.startswith('"uid://') and uid.endswith('"') and uid not in uids, 'Garden import UID reused')
            require(options.get(('remap', 'importer')) == '"texture"'
                    and options.get(('remap', 'type')) == '"CompressedTexture2D"', 'Garden importer changed')
            uids.add(uid)
            for name, value in spec['import_parameters'].items():
                expected_value = str(value).lower()
                require(options.get(('params', name)) == expected_value, 'Garden import processing changed: ' + name)
    print('PASS: approved Garden sources, 91 assets/42 masters, imports and complete preserved predecessor tree')


def captures(root=ROOT):
    """Check source-bound view matrix, bounded differences and retained external GPU parity."""
    import numpy as np
    evidence = read(root, PACKET + 'capture-evidence.json')
    expected_inputs = {'res://Main.tscn', 'res://RuntimeGardenGroundSmokeCheck.cs',
                       'scripts/capture-garden-ground-runtime.gd'}
    for name in ('meadow', 'soil'):
        resource = 'res://art/runtime/terrain/garden-ground-' + name + '.png'
        expected_inputs.update({resource, resource + '.import'})
    require(set(evidence['inputs']) == expected_inputs, 'Capture inputs incomplete or unscoped')
    for resource, digest in evidence['inputs'].items():
        path = root / 'game' / resource.removeprefix('res://') if resource.startswith('res://') else root / resource
        require(sha(path) == digest, 'Capture source drift: ' + resource)
    require(evidence['center'] == [2300, 1300], 'Capture centre changed')
    expected_names = {f'captures/{variant}-{zoom:.1f}x-{frame}.png'
                      for variant in ('baseline', 'runtime') for zoom in (0.8, 1.0, 2.0) for frame in ('wide', 'tall')}
    require(set(evidence['captures']) == set(evidence['frames']) == expected_names, 'Incomplete Garden capture matrix')
    results = []
    for zoom in (0.8, 1.0, 2.0):
        for frame, height in [('wide', 720), ('tall', 827)]:
            pixels = []
            for variant in ('baseline', 'runtime'):
                name = f'captures/{variant}-{zoom:.1f}x-{frame}.png'
                path = root / PACKET / name
                require(sha(path) == evidence['captures'][name], 'Capture bytes changed')
                with Image.open(path) as image:
                    require(image.size == (1280, height), 'Capture dimensions changed')
                    pixels.append(np.array(image.convert('RGBA')))
            changed = np.any(pixels[0] != pixels[1], axis=2)
            baseline_frame = evidence['frames'][f'captures/baseline-{zoom:.1f}x-{frame}.png']
            runtime_frame = evidence['frames'][f'captures/runtime-{zoom:.1f}x-{frame}.png']
            require(baseline_frame == runtime_frame and baseline_frame['zoom'] == zoom
                    and baseline_frame['viewport'] == [1280, height], 'Paired camera framing differs')
            transform = baseline_frame['world_to_canvas']
            require(abs(transform[0]-zoom) < 0.00001 and abs(transform[3]-zoom) < 0.00001
                    and transform[1] == transform[2] == 0, 'Unexpected camera transform')
            tx, ty = transform[4:]
            x0, y0 = int(2164*zoom+tx)-2, int(1243*zoom+ty)-2
            x1, y1 = int(2416*zoom+tx)+2, int(1387*zoom+ty)+2
            allowed = np.zeros(changed.shape, dtype=bool)
            allowed[y0:y1, x0:x1] = True
            require(changed.any() and not (changed & ~allowed).any(), 'Runtime changes outside Garden apron')
            row = dict(zoom=zoom, frame=frame, changed_pixels=int(changed.sum()))
            if frame == 'wide' and zoom in (1.0, 2.0):
                for variant, external in [('baseline', 'baseline'), ('runtime', 'meadow-apron')]:
                    with Image.open(root / f'art-direction/garden-ground-feasibility/review/godot-{external}-{zoom:.1f}x.png') as image:
                        reference = np.array(image.convert('RGBA'))
                    row[variant + '_external_changed_pixels'] = int(np.any(reference != pixels[variant == 'runtime'], axis=2).sum())
            results.append(row)
    print(json.dumps(results, indent=2))
    return results


def historical():
    """Run every frozen art check on the exact last unchanged product tree with an isolated Git index."""
    revision = HISTORICAL_HEAD
    with tempfile.TemporaryDirectory(prefix='garden-predecessor-') as directory:
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
        commands = [line.split()[1:] for line in (snapshot / 'scripts/quality.sh').read_text().splitlines()
                    if line.startswith('python3 ')]
        for command in commands:
            subprocess.run([sys.executable, *command], cwd=snapshot, env=env, check=True)
        print(f'PASS: {len(commands)} frozen artwork/runtime validators on pinned {revision}')


def rejections():
    """Probe disposable copies for paint, phase, dropped soil, owner, import and scope mutations."""
    paths = subprocess.check_output(['git', 'ls-files', '-co', '--exclude-standard', '-z'], cwd=ROOT).decode().split('\0')
    with tempfile.TemporaryDirectory(prefix='garden-rejections-') as directory:
        clone = Path(directory)
        for name in set(paths):
            if not name or Path(name).name == '.DS_Store':
                continue
            target = clone / name
            target.parent.mkdir(parents=True, exist_ok=True)
            target.symlink_to(ROOT / name)
        tests = [
            ('paint', 'art-direction/garden-ground-feasibility/proof-inputs/soil-layer.png', lambda b: b + b'drift'),
            ('mask', 'art-direction/garden-ground-feasibility/proof-inputs/apron-coverage.png', lambda b: b + b'drift'),
            ('phase', 'game/Main.tscn', lambda b: b.replace(b'Vector2(2048, 1120)', b'Vector2(2049, 1120)')),
            ('dropped soil', 'game/Main.tscn', lambda b: b.replace(b'parent="GardenGroundPresentation"]\nz_index = -89', b'parent="."]\nz_index = -89')),
            ('depth', 'game/Main.tscn', lambda b: b.replace(b'z_index = -89', b'z_index = -88')),
            ('duplicate owner', 'game/Main.tscn', lambda b: b + b'\n[node name="GardenGroundPresentation2" type="Node2D" parent="."]\n'),
            ('import processing', 'game/art/runtime/terrain/garden-ground-soil.png.import', lambda b: b.replace(b'process/fix_alpha_border=false', b'process/fix_alpha_border=true')),
            ('physical scope', 'game/OpeningRegionRemedyPreparationSceneSource.cs', lambda b: b.replace(b'72.0f', b'73.0f')),
        ]
        for label, name, mutate in tests:
            target = clone / name
            original_link = target.resolve()
            target.unlink()
            target.write_bytes(mutate(original_link.read_bytes()))
            try:
                validate(clone, preservation=False)
            except (ValueError, OSError):
                print('PASS: rejects ' + label)
            else:
                raise ValueError('Mutation accepted: ' + label)
            finally:
                target.unlink()
                target.symlink_to(original_link)


def main():
    """Run read-only successor checks and optional capture, rejection or pinned checks."""
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--historical-quality', action='store_true')
    parser.add_argument('--check-captures', action='store_true')
    parser.add_argument('--check-rejections', action='store_true')
    args = parser.parse_args()
    if args.historical_quality:
        historical()
        return
    validate()
    builder = ROOT / PRODUCTION / 'tools/build_garden_ground.py'
    subprocess.run([sys.executable, str(builder), '--check'], cwd=ROOT, check=True)
    if args.check_captures:
        captures()
    if args.check_rejections:
        rejections()


if __name__ == '__main__':
    main()
