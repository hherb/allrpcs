#!/usr/bin/env python3
"""Verify exact channel promotion, immutable predecessors and ordinary runtime views."""

import argparse
import hashlib
import json
from pathlib import Path
import re
import runpy
import shutil
import subprocess
import sys
import tarfile
import tempfile
import zipfile
from xml.etree import ElementTree as ET

from PIL import Image

ROOT = Path(__file__).resolve().parents[3]
HERE = Path(__file__).resolve().parents[1]
PREVIEW = ROOT / 'art-direction/whole-channel-runtime-preview'
PRODUCTION = ROOT / 'art-direction/production-proof/corrected'
BASE = '476a569d28ffdf2562157ea8af552b7be92ae28f'
BASELINE_SHA = '6ad4de580a6739258eee35227c9bd0529cccebd69aed1627fc46c3e2f706948f'
MODIFIED = {
    'HANDOVER.md', 'README.md', 'ROADMAP.md',
    'art-direction/production-proof/corrected/asset-manifest.json',
    'art-direction/production-proof/corrected/tools/build.py',
    'art-direction/production-proof/corrected/tools/validate.py',
    'art-direction/production-proof/corrected/tools/validate_terrain_accessibility_review.py',
    'game/Main.tscn', 'game/DirectionAImportProof.tscn', 'game/DirectionAImportProofSmokeCheck.cs',
    'scripts/quality.sh', 'scripts/smoke-godot.sh',
    'scripts/validate-runtime-terrain.py', 'scripts/validate-direction-a-import-proof.py',
}
ADDED = {
    'art-direction/production-proof/corrected/tools/build_whole_channel.py',
    'game/WholeChannelPresentation.cs', 'game/WholeChannelPresentation.cs.uid',
    'game/art/runtime/terrain/channel-retained-overlap.gdshader',
    'game/art/runtime/terrain/channel-retained-overlap.gdshader.uid',
    *{f'scripts/{name}.gd{suffix}' for name in ('check-whole-channel-runtime', 'capture-whole-channel-runtime')
      for suffix in ('', '.uid')},
    *{f'art-direction/production-proof/corrected/exports/terrain/channel-{n}.png'
      for n in ('water', 'banks', 'decks', 'detail', 'fringe')},
    *{f'art-direction/production-proof/corrected/masters/terrain-whole-channel-{n}.ora'
      for n in ('water', 'banks', 'decks', 'detail', 'fringe')},
    *{f'game/art/{folder}/terrain/channel-{n}.png{suffix}' for folder in ('runtime', 'direction-a-import-proof')
      for n in ('water', 'banks', 'decks', 'detail', 'fringe') for suffix in ('', '.import')},
}


def require(condition, message):
    """Fail closed without relying on removable Python assertions."""
    if not condition:
        raise ValueError(message)


def sha(path):
    """Bind exact local bytes."""
    return hashlib.sha256(path.read_bytes()).hexdigest()


def original(path):
    """Read pinned predecessor bytes without changing live files."""
    return subprocess.check_output(['git', 'show', f'{BASE}:{path}'], cwd=ROOT)


def rgba(path):
    """Load detached decoded RGBA pixels."""
    with Image.open(path) as image:
        return image.convert('RGBA')


def equal(actual, expected, message):
    """Require exact geometry and every RGBA byte."""
    require(actual.size == expected.size and actual.tobytes() == expected.tobytes(), message)


def specification():
    """Bind implementation authorization to the exact approved Stage 2 packet."""
    auth = json.loads((HERE / 'authorization.json').read_text())
    require(auth['owner_statement'] == 'approved' and auth['production_runtime_implementation_authorized']
            and not auth['physical_layout_navigation_authorized'], 'implementation authority differs')
    require(auth['baseline_commit'] == BASE and auth['baseline_sha256'] == BASELINE_SHA, 'approval baseline differs')
    for path, digest in [(auth['scope'], auth['scope_sha256']),
                         (auth['specification'], auth['specification_sha256'])]:
        require(sha(ROOT / path) == digest, 'approved packet changed')
    return json.loads((PREVIEW / 'stage-2-specification.json').read_text())


def scope(spec):
    """Preserve complete prior tree, exact old manifest and normalized Main/proof scene bytes."""
    require(sha(HERE / 'baseline.json') == BASELINE_SHA, 'frozen inventory rewritten')
    baseline = json.loads((HERE / 'baseline.json').read_text())
    names = subprocess.check_output(['git', 'ls-tree', '-r', '--name-only', BASE], cwd=ROOT).decode().splitlines()
    require(set(baseline['files']) == {n for n in names if Path(n).name != '.DS_Store'}, 'inventory set changed')
    for name, digest in baseline['files'].items():
        if name not in MODIFIED:
            require(sha(ROOT / name) == digest, f'protected predecessor changed: {name}')
    current = []
    for args in (['git', 'ls-files', '-z'], ['git', 'ls-files', '--others', '--exclude-standard', '-z']):
        current.extend(subprocess.check_output(args, cwd=ROOT).decode().split('\0'))
    prefix = HERE.relative_to(ROOT).as_posix() + '/'
    require(all(not n or n in baseline['files'] or n in ADDED or n.startswith(prefix)
                or Path(n).name == '.DS_Store' for n in current), 'unscoped new file')
    manifest = json.loads((PRODUCTION / 'asset-manifest.json').read_text())
    retained = {k: v for k, v in manifest.items() if k != 'whole_channel_provenance'}
    ids = {a['asset_id'] for a in spec['assets']}
    retained['assets'] = [a for a in manifest['assets'] if a['id'] not in ids]
    require(retained == json.loads(original('art-direction/production-proof/corrected/asset-manifest.json')),
            'prior production manifest changed')
    for scene, old_steps, new_steps in [('Main.tscn', 352, 359), ('DirectionAImportProof.tscn', 83, 88)]:
        text = (ROOT / 'game' / scene).read_text()
        if scene == 'Main.tscn':
            block = text[text.index('\n[node name="WholeChannelPresentation"'):]
            expected = '\n[node name="WholeChannelPresentation" type="Node2D" parent="."]\nscript = ExtResource("441_channel_owner")\n'
            for i, name in enumerate(('Water', 'Banks', 'Decks', 'Detail', 'Fringe'), 436):
                expected += f'{name}Texture = ExtResource("{i}_channel_{name.lower()}")\n'
            expected += 'OverlapClipShader = ExtResource("442_channel_clip")\n'
            require(block == expected, 'channel owner scene binding differs')
            text = text[:-len(block)]
        else:
            for i, name in enumerate(('water', 'banks', 'decks', 'detail', 'fringe'), 84):
                block = f'\n[node name="Channel{name.title()}Proof" type="Sprite2D" parent="Terrain"]\nposition = Vector2(1800, {(i-84)*700})\ntexture = ExtResource("{i}_channel_{name}")\ncentered = false\n'
                require(text.count(block) == 1, 'proof placement differs')
                text = text.replace(block, '')
        matches = list(re.finditer(r'\[ext_resource [^\n]* (?:path="res://(?:WholeChannelPresentation.cs|art/(?:runtime|direction-a-import-proof)/terrain/channel-[^"]+)"[^\n]*)\]\n', text))
        require(len(matches) == (7 if scene == 'Main.tscn' else 5), 'channel resource set differs')
        for match in reversed(matches):
            text = text[:match.start()] + text[match.end():]
        # Insertion introduced one separator before the new resource group.
        text = text.replace('\n\n\n[sub_resource', '\n\n[sub_resource', 1)
        text = text.replace(f'load_steps={new_steps}', f'load_steps={old_steps}', 1)
        require(text.encode() == original('game/' + scene), f'unrelated {scene} change')
    print(f"PASS: {len(baseline['files']) - len(MODIFIED)} unchanged protected predecessors; physical/navigation/route owners unchanged")


def assets(spec):
    """Require five exact approved exports, masters, imports and shader, plus recipe reproduction."""
    adapter = runpy.run_path(str(PRODUCTION / 'tools/build_whole_channel.py'))
    runtime = adapter['compose']()
    manifest = json.loads((PRODUCTION / 'asset-manifest.json').read_text())
    require(len(manifest['assets']) == 89, 'production inventory differs')
    provenance = {
        'specification': 'art-direction/whole-channel-runtime-preview/stage-2-specification.json',
        'specification_sha256': sha(PREVIEW / 'stage-2-specification.json'),
        'recipe_sha256': sha(PRODUCTION / 'tools/build_whole_channel.py'),
        'retained_recipe_sha256': sha(PREVIEW / 'tools/layers.py'),
        'world_origin': [-64, 212], 'world_position': [1238, 522],
        'import_fix_alpha_border': False, 'master_scaling': 'nearest-neighbour 2x of exact approved export',
    }
    require(manifest['whole_channel_provenance'] == provenance, 'production provenance differs')
    imports = runpy.run_path(str(ROOT / 'scripts/validate-direction-a-import-proof.py'))
    all_uids = [imports['read_import_options'](p).get(('remap', 'uid'))
                for p in (ROOT / 'game/art').rglob('*.png.import')]
    preview_uids = [imports['read_import_options'](p).get(('remap', 'uid'))
                    for p in (PREVIEW / 'imports').glob('*.png.import')]
    for asset in spec['assets']:
        name = asset['asset_id'].removeprefix('terrain.whole_channel_')
        entry = next(a for a in manifest['assets'] if a['id'] == asset['asset_id'])
        source = PRODUCTION / entry['export']
        require(entry['sha256'] == asset['export_sha256'] == sha(source), 'approved production PNG changed')
        rect = dict(x=0, y=0, width=2604, height=620)
        require(entry['logical_rect'] == entry['export_rect_pixels'] == rect
                and entry['pivot'] == dict(x=1302, y=310) and entry['contact_baseline'] == 620
                and entry['master_dimensions_pixels'] == dict(width=5208, height=1240, scale=2)
                and entry['source'] == 'tools/build_whole_channel.py' and entry['godot_import_proof'],
                'channel manifest geometry/source differs')
        equal(rgba(source), runtime[name], 'retained channel recipe RGBA changed')
        master = PRODUCTION / 'masters' / asset['master_filename']
        require(sha(master) == asset['master_sha256'], 'approved master changed')
        with zipfile.ZipFile(master) as archive:
            stack = ET.fromstring(archive.read('stack.xml'))
            nodes = stack.findall('./stack/layer')
            require(len(nodes) == 1 and nodes[0].attrib['x'] == nodes[0].attrib['y'] == '0', 'master layer placement differs')
            expected = runtime[name].resize((5208, 1240), Image.Resampling.NEAREST)
            for path in ('mergedimage.png', nodes[0].attrib['src']):
                with archive.open(path) as stream:
                    equal(Image.open(stream).convert('RGBA'), expected, 'master/export parity differs')
        for folder in ('runtime', 'direction-a-import-proof'):
            path = ROOT / f"game/art/{folder}/terrain/{asset['export_filename']}"
            require(not path.is_symlink() and sha(path) == asset['export_sha256'], 'import source is not exact owned PNG')
            options = imports['read_import_options'](path.with_suffix('.png.import'))
            uid = options.get(('remap', 'uid'))
            require(uid and all_uids.count(uid) == 1 and uid not in preview_uids, 'import UID shared with proof/runtime/preview')
            for key, value in {'compress/mode': '0', 'mipmaps/generate': 'false',
                               'process/fix_alpha_border': 'false', 'process/premult_alpha': 'false',
                               'process/size_limit': '0'}.items():
                require(options.get(('params', key)) == value, 'lossless alpha import changed')
            cache = imports['imported_cache_path'](path)
            require(options.get(('remap', 'path')) == f'"{cache}"'
                    and options.get(('deps', 'dest_files')) == f'["{cache}"]'
                    and options.get(('deps', 'source_file')) == f'"{imports["resource_path"](path)}"', 'import paths changed')
    require(sha(ROOT / 'game/art/runtime/terrain/channel-retained-overlap.gdshader') == spec['overlap_shader']['sha256'],
            'approved overlap shader changed')
    require(len(list((PRODUCTION / 'masters').glob('*.ora'))) == 40, 'master inventory differs')
    print('PASS: five exact PNGs/2x masters and ten independent lossless imports; accepted shader unchanged')


def captures(record=False):
    """Require all ordinary runtime views to reproduce exact approved candidate RGBA pixels."""
    context = json.loads((HERE / 'capture-context.json').read_text())
    preview_context = json.loads((PREVIEW / 'capture-context.json').read_text())
    names = {f'{t}-{z:.1f}x-{f}' for t in ('outlet', 'B2', 'B1', 'interface', 'crossing', 'spring')
             for z in (.8, 1., 2.) for f in ('wide', 'tall')} | {'overview'}
    require({v['name'] for v in context['views']} == names and len(context['views']) == 37, 'capture matrix incomplete/duplicated')
    require(context['physical_nodes_unchanged'], 'capture changed physical nodes')
    expected_inputs = {'game/Main.tscn', 'game/WholeChannelPresentation.cs', 'scripts/capture-whole-channel-runtime.gd',
                       'game/art/runtime/terrain/channel-retained-overlap.gdshader'}
    expected_inputs.update(f'game/art/runtime/terrain/channel-{n}.png{suffix}'
                           for n in ('water', 'banks', 'decks', 'detail', 'fringe') for suffix in ('', '.import'))
    require(set(context['inputs']) == expected_inputs and all(sha(ROOT / p) == digest for p, digest in context['inputs'].items()),
            'runtime capture inputs stale')
    evidence = {}
    for view in context['views']:
        prior = next(v for v in preview_context['views'] if v['name'] == view['name'])
        require(view == prior, 'runtime camera/framing differs')
        path = HERE / 'captures' / f"runtime-{view['name']}.png"
        equal(rgba(path), rgba(PREVIEW / 'captures' / f"candidate-{view['name']}.png"), f"runtime RGBA differs: {view['name']}")
        evidence[path.name] = sha(path)
    bound = {'context_sha256': sha(HERE / 'capture-context.json'), 'capture_sha256': evidence}
    path = HERE / 'capture-evidence.json'
    if record:
        path.write_text(json.dumps(bound, indent=2) + '\n')
    else:
        require(json.loads(path.read_text()) == bound, 'runtime evidence changed')
    print('PASS: all 37 ordinary runtime views match approved Stage 1 candidate RGBA exactly')


def rejections():
    """Reject stale/altered runtime evidence and baseline rewrites in a disposable fixture."""
    global HERE
    retained = HERE
    with tempfile.TemporaryDirectory(prefix='channel-rejections-') as directory:
        fixture = Path(directory)
        shutil.copytree(retained / 'captures', fixture / 'captures')
        for name in ('capture-context.json', 'capture-evidence.json', 'baseline.json'):
            shutil.copy2(retained / name, fixture / name)
        context_bytes = (fixture / 'capture-context.json').read_bytes()
        HERE = fixture
        try:
            for mutation in ('stale-input', 'missing-view', 'camera-drift', 'physical-drift'):
                context = json.loads(context_bytes)
                if mutation == 'stale-input':
                    context['inputs']['game/Main.tscn'] = '0' * 64
                elif mutation == 'missing-view':
                    context['views'].pop()
                elif mutation == 'camera-drift':
                    context['views'][0]['camera_center'][0] += 1
                else:
                    context['physical_nodes_unchanged'] = False
                (fixture / 'capture-context.json').write_text(json.dumps(context))
                try:
                    captures()
                except ValueError:
                    pass
                else:
                    raise ValueError('runtime mutation accepted: ' + mutation)
            (fixture / 'capture-context.json').write_bytes(context_bytes)
            path = fixture / 'captures/runtime-overview.png'
            modified = rgba(path)
            pixel = modified.getpixel((0, 0))
            modified.putpixel((0, 0), (pixel[0] ^ 1, *pixel[1:]))
            modified.save(path)
            try:
                captures()
            except ValueError:
                pass
            else:
                raise ValueError('altered runtime pixels accepted')
            inventory = json.loads((fixture / 'baseline.json').read_text())
            inventory['files']['game/Main.tscn'] = '0' * 64
            (fixture / 'baseline.json').write_text(json.dumps(inventory))
            try:
                scope({})
            except ValueError:
                pass
            else:
                raise ValueError('rewritten baseline accepted')
        finally:
            HERE = retained
    print('PASS: six disposable mutations rejected stale inputs, missing views, camera/physics drift, pixels and inventory rewrite')


def archived():
    """Reproduce historical forest and Stage 1 preview on pinned runtime/preview trees."""
    for revision, command in [
        ('e976ea00e1e4e121dc726807b8531e455c42d664', ['art-direction/forest-path-runtime-promotion/tools/validate.py', '--check-captures']),
        (BASE, ['art-direction/whole-channel-runtime-preview/tools/preview.py', '--check-captures', '--check-rejections']),
    ]:
        with tempfile.TemporaryDirectory(prefix='channel-preserved-tree-') as directory:
            snapshot = Path(directory) / 'tree'
            snapshot.mkdir()
            archive = Path(directory) / 'tree.tar'
            with archive.open('wb') as output:
                subprocess.run(['git', 'archive', revision], cwd=ROOT, stdout=output, check=True)
            with tarfile.open(archive) as package:
                package.extractall(snapshot, filter='data')
            (snapshot / '.git').symlink_to(ROOT / '.git', target_is_directory=True)
            import os
            env = dict(os.environ, GIT_INDEX_FILE=str(Path(directory) / 'index'))
            subprocess.run(['git', 'read-tree', revision], cwd=snapshot, env=env, check=True)
            subprocess.run([sys.executable, str(snapshot / command[0]), *command[1:]], cwd=snapshot, env=env, check=True)
    # Earlier forest corrections need their own clean temporary index; do not inherit the wrapper index.
    history = runpy.run_path(str(ROOT / 'art-direction/forest-path-runtime-promotion/tools/validate.py'))
    history['archived']()
    print('PASS: archived forest/channel predecessors reproduced; historical inventories unchanged')


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--check-captures', action='store_true')
    parser.add_argument('--record-captures', action='store_true')
    parser.add_argument('--check-archived', action='store_true')
    parser.add_argument('--check-rejections', action='store_true')
    args = parser.parse_args()
    spec = specification()
    scope(spec)
    assets(spec)
    if args.check_captures or args.record_captures:
        captures(args.record_captures)
    if args.check_rejections:
        rejections()
    if args.check_archived:
        archived()
