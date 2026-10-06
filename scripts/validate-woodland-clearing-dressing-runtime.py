#!/usr/bin/env python3
"""Validate the exact sixteen grouped additions and every retained predecessor byte."""

import argparse
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
SCOPE = 'art-direction/woodland-clearing-dressing-runtime-scope/'
PACKET = 'art-direction/woodland-clearing-dressing-runtime-promotion/'
PRODUCTION = 'art-direction/production-proof/corrected/'
BUILDER = PRODUCTION + 'tools/build_woodland_clearing_dressing.py'
DOCUMENTS = {'HANDOVER.md', 'README.md', 'ROADMAP.md', 'game/art/direction-a-import-proof/README.md'}
MODIFIED = DOCUMENTS | {'game/Main.tscn', 'game/DirectionAImportProof.tscn', PRODUCTION + 'asset-manifest.json',
                        'scripts/quality.sh', 'scripts/validate-direction-a-import-proof.py'}
NEW = {SCOPE + 'owner-approval.json', BUILDER, 'scripts/validate-woodland-clearing-dressing-runtime.py',
       'scripts/capture-woodland-clearing-dressing-runtime.gd', 'scripts/capture-woodland-clearing-dressing-runtime.gd.uid',
       'game/RuntimeWoodlandClearingDressingSmokeCheck.cs', 'game/RuntimeWoodlandClearingDressingSmokeCheck.cs.uid'}
CODE = {BUILDER, 'scripts/validate-woodland-clearing-dressing-runtime.py',
        'scripts/capture-woodland-clearing-dressing-runtime.gd', 'scripts/capture-woodland-clearing-dressing-runtime.gd.uid',
        'game/RuntimeWoodlandClearingDressingSmokeCheck.cs', 'game/RuntimeWoodlandClearingDressingSmokeCheck.cs.uid',
        PACKET + 'review.py', PACKET + 'cold_import.py', PACKET + 'cold-check.gd'}
PARAMS = {'compress/mode': '0', 'mipmaps/generate': 'false', 'process/fix_alpha_border': 'false',
          'process/premult_alpha': 'false', 'process/channel_remap/red': '0', 'process/channel_remap/green': '1',
          'process/channel_remap/blue': '2', 'process/channel_remap/alpha': '3', 'process/size_limit': '0'}


def require(condition, message):
    """Fail closed with an actionable contract error."""
    if not condition:
        raise ValueError(message)


def sha(path):
    """Compare persisted bytes."""
    return hashlib.sha256(path.read_bytes()).hexdigest()


def read(root, name):
    """Read one bound JSON input."""
    return json.loads((root / name).read_text())


def original(commit, name):
    """Read committed authority independent of the working tree."""
    return subprocess.check_output(['git', 'show', commit + ':' + name], cwd=ROOT)


def load_builder():
    """Reuse the exact-byte installer without invoking its write entry point."""
    spec = importlib.util.spec_from_file_location('dressing_builder', ROOT / BUILDER)
    module = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(module)
    return module


def blocks(assets, main):
    """Derive finite scene additions from the digest-bound ordered proposal."""
    key, first = ('runtime_texture', 458) if main else ('proof_texture', 97)
    resources = '\n' + ''.join(
        f'[ext_resource type="Texture2D" path="res://{a[key][5:]}" id="{first+i}_woodland_{a["node_name"]}"]\n'
        for i, a in enumerate(assets))
    parent = 'WoodlandClearingDressingPresentation' if main else 'WoodlandClearingDressingProof'
    nodes = f'\n[node name="{parent}" type="Node2D" parent="."]\n'
    if not main:
        nodes += 'position = Vector2(6800, 3500)\n'
    for i, asset in enumerate(assets):
        x, y = asset['origin']
        nodes += f'\n[node name="{asset["node_name"]}" type="Sprite2D" parent="{parent}"]\n'
        nodes += f'position = Vector2({x}, {y})\n'
        if main:
            nodes += 'z_index = -60\n'
        nodes += 'texture_filter = 2\ntexture_repeat = 1\n'
        nodes += f'texture = ExtResource("{first+i}_woodland_{asset["node_name"]}")\ncentered = false\n'
    if main:
        resources += '[ext_resource type="Script" path="res://RuntimeWoodlandClearingDressingSmokeCheck.cs" id="474_woodland_dressing_check"]\n'
        nodes += '\n[node name="RuntimeWoodlandClearingDressingSmokeCheck" type="Node" parent="."]\nscript = ExtResource("474_woodland_dressing_check")\n'
    return resources, nodes


def tooling(root, commit, assets):
    """Permit only frozen-predecessor dispatch, the new smoke and explicit new imports."""
    quality = original(commit, 'scripts/quality.sh').decode().replace(
        'python3 scripts/validate-village-edge-ground-runtime.py --historical-laundry\n'
        'python3 scripts/validate-village-edge-ground-runtime.py --check-rejections',
        'python3 scripts/validate-woodland-clearing-dressing-runtime.py --historical-village-edge\n'
        'python3 scripts/validate-woodland-clearing-dressing-runtime.py --check-rejections')
    quality = quality.replace('ALLRPCS_RUNTIME_VILLAGE_EDGE_GROUND_CHECK=1 godot --headless --path game --quit-after 30',
        'ALLRPCS_RUNTIME_VILLAGE_EDGE_GROUND_CHECK=1 godot --headless --path game --quit-after 30\n'
        'ALLRPCS_RUNTIME_WOODLAND_CLEARING_DRESSING_CHECK=1 godot --headless --path game --quit-after 30')
    require((root / 'scripts/quality.sh').read_text() == quality, 'Unscoped quality change')
    imports = original(commit, 'scripts/validate-direction-a-import-proof.py').decode()
    imports = imports.replace('== 97', '== 113').replace('expected 97', 'expected 113')
    token = '"terrain.village_edge_entrance_ground",'
    imports = imports.replace(token, token + '\n' + ''.join(
        '                ' + json.dumps(a['asset_id']) + ',\n' for a in assets).rstrip())
    require((root / 'scripts/validate-direction-a-import-proof.py').read_text() == imports, 'Unscoped import check change')


def options(path):
    """Read importer settings independently of Godot."""
    section, values = '', {}
    for line in path.read_text().splitlines():
        if line.startswith('[') and line.endswith(']'):
            section = line[1:-1]
        elif '=' in line:
            key, value = line.split('=', 1)
            values[section, key] = value
    return values


def validate(root=ROOT, preservation=True):
    """Verify approval, exact RGBA/masters/imports, normalized scenes and closed membership."""
    builder = load_builder()
    spec, auth = builder.approved(root)
    baseline = read(root, PACKET + 'baseline.json')
    require(baseline['commit'] == auth['baseline_commit'], 'Baseline commit drift')
    bindings = read(root, SCOPE + 'source-bindings.json')
    require(bindings['proposal'] == auth['proposal_sha256'] and bindings['readme'] == auth['scope_sha256'],
            'Frozen scope digest drift')
    owner = read(root, SCOPE + 'owner-approval.json')
    require(owner['owner_statement'] == 'approved' and owner['status'] == builder.STATUS
            and owner['proposal_sha256'] == auth['proposal_sha256']
            and owner['selection_sha256'] == auth['selection_sha256'], 'Owner authorization drift')
    assets = spec['assets']
    code = read(root, PACKET + 'tool-bindings.json')
    require(set(code) == CODE, 'Incomplete runtime tool bindings')
    for name, digest in code.items():
        require(sha(root / name) == digest, 'Reviewed runtime tool changed: ' + name)
    tooling(root, baseline['commit'], assets)
    for name, digest in baseline['files'].items():
        if name not in MODIFIED:
            require((root / name).is_file() and sha(root / name) == digest, 'Protected predecessor drift: ' + name)
    added = set(NEW)
    for asset in assets:
        added.update(asset[k] for k in ('production_export', 'production_master', 'proof_texture', 'runtime_texture'))
        added.update(asset[k] + '.import' for k in ('proof_texture', 'runtime_texture'))
    if preservation:
        names = subprocess.check_output(['git', 'ls-files', '-co', '--exclude-standard', '-z'], cwd=ROOT).decode().split('\0')
        require(all(not n or n in baseline['files'] or n in added or n.startswith(PACKET) for n in names), 'Unscoped new file')
        require(not any(Path(n).name == '.DS_Store' for n in names if n), 'Finder metadata must remain excluded')
        tree = subprocess.check_output(['git', 'ls-tree', '-r', '--name-only', baseline['commit']], cwd=ROOT, text=True).splitlines()
        require(set(baseline['files']) == {n for n in tree if Path(n).name != '.DS_Store'}, 'Incomplete baseline')
    for main, filename, steps in [(True, 'Main.tscn', (374, 391)), (False, 'DirectionAImportProof.tscn', (96, 112))]:
        resources, nodes = blocks(assets, main)
        live = (root / 'game' / filename).read_text()
        require(live.count(resources) == live.count(nodes) == 1, 'Missing/duplicate/reordered scene additions')
        boundary = min(live.index(tag) for tag in ('\n[sub_resource', '\n[node') if tag in live)
        require(live.index(resources) < boundary, 'Declarations must precede subresources/nodes')
        normalized = live.replace(resources, '', 1).replace(nodes, '', 1)
        normalized = normalized.replace(f'load_steps={steps[1]}', f'load_steps={steps[0]}', 1)
        require(normalized.encode() == original(baseline['commit'], 'game/' + filename), 'Old scene or physical owner drift')
        if main:
            require(live.index(nodes) > live.index('[node name="VillageEdgeGroundPresentation"'), 'New owner order drift')
            previous = original(baseline['commit'], 'game/Main.tscn').decode()
            marker = '[node name="WholeChannelPresentation"'
            require(live[live.index(marker):] == previous[previous.index(marker):],
                    'WholeChannel must remain final and identical')
    for name, data in builder.expected(root).items():
        require((root / name).read_bytes() == data, 'Exact production/source parity drift: ' + name)
    require(len(list((root / PRODUCTION / 'masters').glob('*.ora'))) == 64, 'Master membership drift')
    old_uids = {options(root / n).get(('remap', 'uid')) for n in baseline['files'] if n.endswith('.png.import')}
    old_uids.update('"' + (root / n).read_text().strip() + '"' for n in baseline['files'] if n.endswith('.uid'))
    for asset in assets:
        image = Image.open(root / asset['export']).convert('RGBA')
        require(list(image.size) == asset['size'], 'Native dimensions drift')
        pixels = list(zip(*(image.getchannel(channel).tobytes() for channel in 'RGBA')))
        facts = dict(visible_bbox=list(image.getchannel('A').getbbox()),
                     hidden_rgb_pixels=sum(a == 0 and any((r, g, b)) for r, g, b, a in pixels),
                     opaque_pixels=sum(a == 255 for r, g, b, a in pixels))
        require(facts == asset['native_alpha'], 'Approved native alpha facts drift')
        require([a + b for a, b in zip(asset['origin'], asset['proposed_pivot'])] == asset['world_pivot'], 'Pivot drift')
        with zipfile.ZipFile(root / asset['production_master']) as archive:
            xml = fromstring(archive.read('stack.xml'))
            require([int(xml.attrib['w']), int(xml.attrib['h'])] == asset['master_size'], 'Master dimensions drift')
        for key in ('proof_texture', 'runtime_texture'):
            path = root / asset[key]
            values = options(path.with_suffix('.png.import'))
            resource = 'res://' + asset[key][5:]
            cache = 'res://.godot/imported/' + path.name + '-' + hashlib.md5(resource.encode(), usedforsecurity=False).hexdigest() + '.ctex'
            uid = values.get(('remap', 'uid'))
            require(uid and uid.startswith('"uid://') and uid.endswith('"') and uid not in old_uids, 'Import UID missing/reused')
            old_uids.add(uid)
            require(values.get(('remap', 'importer')) == '"texture"' and
                    values.get(('remap', 'type')) == '"CompressedTexture2D"', 'Importer type drift')
            require(values.get(('deps', 'source_file')) == '"' + resource + '"' and
                    values.get(('remap', 'path')) == '"' + cache + '"' and
                    values.get(('deps', 'dest_files')) == '["' + cache + '"]', 'Import/cache path drift')
            for name, expected in PARAMS.items():
                require(values.get(('params', name)) == expected, 'Import processing drift: ' + name)
    for name in ('game/RuntimeWoodlandClearingDressingSmokeCheck.cs.uid', 'scripts/capture-woodland-clearing-dressing-runtime.gd.uid'):
        uid = '"' + (root / name).read_text().strip() + '"'
        require(uid.startswith('"uid://') and uid not in old_uids, 'Script UID missing/reused')
        old_uids.add(uid)
    print('PASS: sixteen exact grouped modules; 113 assets/64 masters; all predecessor bytes and scene blocks retained')


def historical():
    """Run unchanged closed predecessor validators at the terminally accepted runtime head."""
    revision = read(ROOT, PACKET + 'authorization.json')['frozen_runtime_predecessor']
    require(revision == 'cbc02d06585a88b52483aa5025788ae2bd0ffded', 'Accepted predecessor revision drift')
    with tempfile.TemporaryDirectory(prefix='dressing-village-predecessor-') as directory:
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
        for flags in (['--historical-laundry'], ['--check-captures', '--check-rejections']):
            subprocess.run([sys.executable, 'scripts/validate-village-edge-ground-runtime.py', *flags],
                           cwd=snapshot, env=env, check=True)
    print('PASS: frozen accepted Village Edge and inherited chain at', revision)


def rejections():
    """Reject exact-scope regressions with copy-on-write disposable file overlays."""
    baseline = read(ROOT, PACKET + 'baseline.json')
    assets = read(ROOT, SCOPE + 'proposal-inputs.json')['assets']
    names = set(baseline['files']) | NEW | CODE | {PACKET + 'authorization.json', PACKET + 'baseline.json', PACKET + 'tool-bindings.json'}
    for asset in assets:
        names.update(asset[k] for k in ('production_export', 'production_master', 'proof_texture', 'runtime_texture'))
        names.update(asset[k] + '.import' for k in ('proof_texture', 'runtime_texture'))
    with tempfile.TemporaryDirectory(prefix='dressing-rejections-') as directory:
        fixture = Path(directory)
        for name in names:
            target = fixture / name
            target.parent.mkdir(parents=True, exist_ok=True)
            target.symlink_to(ROOT / name)
        validate(fixture, preservation=False)
        resources, nodes = blocks(assets, True)
        scene = (ROOT / 'game/Main.tscn').read_text()
        first = nodes[nodes.index('\n[node name="shrubs_01"'):nodes.index('\n[node name="fallen_wood_07"')]
        second = nodes[nodes.index('\n[node name="fallen_wood_07"'):nodes.index('\n[node name="rocks_08"')]
        reordered = scene.replace(first + second, second + first, 1)
        mutants = [
            ('omitted placement', scene.replace(nodes.split('\n[node name="fallen_wood_07"')[0], '\n', 1)),
            ('duplicate owner', scene.replace(nodes, nodes + nodes)),
            ('moved sprite', scene.replace('position = Vector2(920, 132)', 'position = Vector2(921, 132)')),
            ('resized sprite', scene.replace('position = Vector2(920, 132)', 'position = Vector2(920, 132)\nscale = Vector2(2, 1)')),
            ('flipped sprite', scene.replace('position = Vector2(920, 132)', 'position = Vector2(920, 132)\nflip_h = true')),
            ('reordered overlap', reordered),
            ('added authority', scene.replace('type="Node2D" parent="."]\n\n[node name="shrubs_01"', 'type="StaticBody2D" parent="."]\n\n[node name="shrubs_01"')),
            ('physical owner', scene.replace('position = Vector2(2300, 1300)', 'position = Vector2(2301, 1300)')),
        ]
        def probe(label, name, data):
            target = fixture / name
            target.unlink()
            target.write_bytes(data)
            try:
                validate(fixture, preservation=False)
            except (ValueError, AssertionError):
                print('PASS: rejects', label)
            else:
                raise ValueError('Mutation accepted: ' + label)
            finally:
                target.unlink()
                target.symlink_to(ROOT / name)
        for label, changed in mutants:
            require(changed != scene, 'Ineffective rejection fixture: ' + label)
            probe(label, 'game/Main.tscn', changed.encode())
        for label, key in [('changed paint', 'runtime_texture'), ('changed master', 'production_master')]:
            path = assets[0][key]
            probe(label, path, (ROOT / path).read_bytes() + b'changed')
        path = assets[0]['runtime_texture'] + '.import'
        probe('import processing', path, (ROOT / path).read_bytes().replace(b'process/fix_alpha_border=false', b'process/fix_alpha_border=true'))
        proposal = read(ROOT, SCOPE + 'proposal-inputs.json')
        proposal['selected_variant'] = 'sparse'
        probe('wrong variant', SCOPE + 'proposal-inputs.json', json.dumps(proposal).encode())
        authorization = read(ROOT, PACKET + 'authorization.json')
        authorization['baseline_commit'] = authorization['frozen_runtime_predecessor']
        probe('authorization baseline', PACKET + 'authorization.json', json.dumps(authorization).encode())
        name = 'game/RuntimeWoodlandClearingDressingSmokeCheck.cs'
        probe('inactive smoke gate', name, (ROOT / name).read_bytes().replace(b'== "1"', b'!= "1"'))
    print('PASS: fourteen finite runtime-scope rejection probes')


def main():
    """Run live validation or the explicit frozen predecessor route."""
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--historical-village-edge', action='store_true')
    parser.add_argument('--check-rejections', action='store_true')
    args = parser.parse_args()
    if args.historical_village_edge:
        historical()
    else:
        validate()
        if args.check_rejections:
            rejections()


if __name__ == '__main__':
    main()
