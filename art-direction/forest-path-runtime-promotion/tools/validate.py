#!/usr/bin/env python3
"""Validate the named forest promotion, preserved predecessors and exact runtime views."""

import argparse
import hashlib
import importlib.util
import json
from pathlib import Path
import subprocess
import sys
import tarfile
import tempfile
import zipfile
from xml.etree import ElementTree as ET

from PIL import Image

ROOT = Path(__file__).resolve().parents[3]
HERE = Path(__file__).resolve().parents[1]
PRODUCTION = ROOT / 'art-direction/production-proof/corrected'
CORRECTION = ROOT / 'art-direction/forest-path-ending-correction'
ASSET_ID = 'terrain.clearing_closed_forest_path'
FILENAME = 'clearing-closed-forest-path.png'
SHA = '3984916a864042f54b5ccbf0d745c150f7807d064a44f9022f96f83b7d081451'
BASE_COMMIT = '0944b8b79ad2f94c52c5013939a47440f20b57a2'
MODIFIED = {
    'HANDOVER.md', 'README.md', 'ROADMAP.md',
    'art-direction/forest-path-ending-correction/runtime-promotion-scope.md',
    'art-direction/production-proof/corrected/asset-manifest.json',
    'art-direction/production-proof/corrected/tools/build.py',
    'art-direction/production-proof/corrected/tools/validate.py',
    'art-direction/production-proof/corrected/tools/validate_terrain_accessibility_review.py',
    'game/DirectionAImportProof.tscn', 'game/DirectionAImportProofSmokeCheck.cs',
    'game/GreyboxNavigation.cs', 'game/Main.tscn', 'game/RuntimeTerrainGroundSmokeCheck.cs',
    'scripts/check-stream-laundry-runtime.gd', 'scripts/check-village-edge-stream-runtime.gd',
    'scripts/check-woodland-village-edge-runtime.gd', 'scripts/smoke-godot.sh',
    'scripts/quality.sh', 'scripts/validate-direction-a-import-proof.py',
    'scripts/validate-runtime-terrain.py',
}
ADDED = {
    'art-direction/production-proof/corrected/tools/build_closed_forest_path.py',
    'art-direction/production-proof/corrected/exports/terrain/' + FILENAME,
    'art-direction/production-proof/corrected/masters/terrain-clearing-closed-forest-path.ora',
    'scripts/check-closed-forest-runtime.gd', 'scripts/capture-closed-forest-path-runtime.gd',
    'scripts/check-closed-forest-runtime.gd.uid', 'scripts/capture-closed-forest-path-runtime.gd.uid',
    *{f'game/art/{folder}/terrain/{FILENAME}{suffix}'
      for folder in ('runtime', 'direction-a-import-proof') for suffix in ('', '.import')},
}


def require(condition, message):
    """Fail closed with one concise invariant name."""
    if not condition:
        raise ValueError(message)


def sha(path):
    """Digest exact file bytes."""
    return hashlib.sha256(path.read_bytes()).hexdigest()


def original(relative):
    """Read pinned Git bytes without replacing live files or rewriting inventories."""
    return subprocess.check_output(['git', 'show', f'{BASE_COMMIT}:{relative}'], cwd=ROOT)


def rgba(path):
    """Load detached RGBA pixels."""
    with Image.open(path) as image:
        return image.convert('RGBA')


def scope():
    """Protect the complete frozen successor inventory outside the named presentation changes."""
    require(sha(HERE / 'baseline.json') == 'ce7932d64963a3eab8c940aeaf259133c25ae1998954f2d7261a2e0133fcb9a2', 'frozen successor inventory bytes changed')
    baseline = json.loads((HERE / 'baseline.json').read_text())
    require(baseline['base_commit'] == BASE_COMMIT and baseline['owner_statement'] == 'approved',
            'scope starting head/approval changed')
    # Check the pinned Git inventory as well as the independently bound snapshot bytes.
    names = subprocess.check_output(['git', 'ls-tree', '-r', '--name-only', BASE_COMMIT], cwd=ROOT).decode().splitlines()
    names = {n for n in names if Path(n).name != '.DS_Store'}
    require(set(baseline['files']) == names, 'frozen successor inventory changed')
    for name, digest in baseline['files'].items():
        if name not in MODIFIED:
            require(sha(ROOT / name) == digest, f'protected predecessor changed: {name}')
    protected = json.loads((CORRECTION / 'baseline.json').read_text())
    require(all(baseline['files'].get(n) == digest for n, digest in protected.items()),
            'historical inventory is not a preserved subset')
    tracked = subprocess.check_output(['git', 'ls-files', '-z'], cwd=ROOT).decode().split('\0')
    untracked = subprocess.check_output(['git', 'ls-files', '--others', '--exclude-standard', '-z'], cwd=ROOT).decode().split('\0')
    prefix = HERE.relative_to(ROOT).as_posix() + '/'
    require(all(not n or n in names or n in ADDED or n.startswith(prefix) or Path(n).name == '.DS_Store'
                for n in tracked + untracked), 'unscoped new file')
    scene = (ROOT / 'game/Main.tscn').read_text()
    resource = '[ext_resource type="Texture2D" path="res://art/runtime/terrain/clearing-closed-forest-path.png" id="435_runtime_closed_forest_path"]\n'
    binding = 'ClosedForestPathTexture = ExtResource("435_runtime_closed_forest_path")\n'
    require(scene.count(resource) == scene.count(binding) == 1, 'forest scene binding changed')
    normalized = scene.replace(resource, '').replace(binding, '').replace('load_steps=352', 'load_steps=351', 1)
    require(normalized.encode() == original('game/Main.tscn'), 'unrelated Main scene change')
    nav = (ROOT / 'game/GreyboxNavigation.cs').read_text()
    before = original('game/GreyboxNavigation.cs').decode()
    for start, end in [('    public override void _EnterTree()', '    /// <inheritdoc />'),
                       ('    private static Vector2 FindSquareBoundaryExit(', None),
                       ('    private Sprite2D CreateStraightPathPresentation(', '    private Sprite2D CreateClosedForestPathPresentation(')]:
        def region(text, finish):
            offset = text.index(start)
            return text[offset:text.index(finish, offset)] if finish else text[offset:]
        prior_end = '    private static Vector2 FindSquareBoundaryExit(' if start.endswith('CreateStraightPathPresentation(') else end
        require(region(nav, end) == region(before, prior_end), 'retained navigation/presentation method changed')
    manifest = json.loads((PRODUCTION / 'asset-manifest.json').read_text())
    retained = {k: v for k, v in manifest.items() if k != 'clearing_closed_forest_path_provenance'}
    retained['assets'] = [a for a in manifest['assets'] if a['id'] != ASSET_ID]
    require(retained == json.loads(original('art-direction/production-proof/corrected/asset-manifest.json')),
            'unrelated production manifest changed')
    print(f'PASS: frozen successor scope; {len(names - MODIFIED)} unchanged protected files; physical owners unchanged')


def asset():
    """Require one exact export, editable 2x master and independent lossless imports."""
    manifest = json.loads((PRODUCTION / 'asset-manifest.json').read_text())
    entries = [a for a in manifest['assets'] if a['id'] == ASSET_ID]
    require(len(entries) == 1 and len(manifest['assets']) == 84, 'asset inventory changed')
    entry = entries[0]
    rect = dict(x=0, y=0, width=552, height=384)
    require(entry['export'] == 'exports/terrain/' + FILENAME and entry['sha256'] == SHA
            and entry['pivot'] == dict(x=276, y=192) and entry['logical_rect'] == rect
            and entry['export_rect_pixels'] == rect and entry['contact_baseline'] == 384
            and entry['master_dimensions_pixels'] == dict(width=1104, height=768, scale=2)
            and entry['source'] == 'tools/build_closed_forest_path.py' and entry['godot_import_proof'] is True,
            'forest asset geometry/provenance changed')
    source = CORRECTION / 'result/exports/corrected.png'
    require(sha(source) == SHA and sha(PRODUCTION / entry['export']) == SHA, 'accepted export bytes changed')
    pixels = rgba(source)
    require(pixels.size == (552, 384) and pixels.crop((0, 164, 552, 384)).getbbox() is None,
            'forest texture intrudes into gate/Clearing')
    adapter = PRODUCTION / 'tools/build_closed_forest_path.py'
    spec = importlib.util.spec_from_file_location('forest_adapter', adapter)
    module = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(module)
    require(module.compose().tobytes() == pixels.tobytes(), 'retained recipe pixel parity changed')
    provenance = manifest['clearing_closed_forest_path_provenance']
    require(provenance == {
        'accepted_proof': 'art-direction/forest-path-ending-correction/result/exports/corrected.png',
        'accepted_proof_sha256': SHA, 'master': 'masters/terrain-clearing-closed-forest-path.ora',
        'recipe_sha256': sha(adapter), 'retained_recipe_sha256': sha(CORRECTION / 'tools/proof.py'),
        'world_origin': [520, 32], 'world_position': [796, 224], 'z_index': -75,
        'import_fix_alpha_border': False, 'master_scaling': 'nearest-neighbour 2x of exact accepted export',
    }, 'forest production provenance changed')
    with zipfile.ZipFile(PRODUCTION / provenance['master']) as archive:
        stack = ET.fromstring(archive.read('stack.xml'))
        require((int(stack.attrib['w']), int(stack.attrib['h'])) == (1104, 768), 'master dimensions changed')
        layers = stack.findall('./stack/layer')
        require(len(layers) == 1 and layers[0].attrib['x'] == layers[0].attrib['y'] == '0', 'master layer placement changed')
        expected = pixels.resize((1104, 768), Image.Resampling.NEAREST)
        for name in ('mergedimage.png', layers[0].attrib['src']):
            with archive.open(name) as stream:
                decoded = Image.open(stream).convert('RGBA')
                require(decoded.size == expected.size and decoded.tobytes() == expected.tobytes(), 'master/export parity changed')
    helpers = __import__('runpy').run_path(str(ROOT / 'scripts/validate-direction-a-import-proof.py'))
    all_uids = [helpers['read_import_options'](p).get(('remap', 'uid')) for p in (ROOT / 'game/art').rglob('*.png.import')]
    for folder in ('runtime', 'direction-a-import-proof'):
        path = ROOT / f'game/art/{folder}/terrain/{FILENAME}'
        require(not path.is_symlink() and sha(path) == SHA, 'forest imported source changed')
        options = helpers['read_import_options'](path.with_suffix('.png.import'))
        cache = helpers['imported_cache_path'](path)
        require(options.get(('remap', 'path')) == f'"{cache}"'
                and options.get(('deps', 'dest_files')) == f'["{cache}"]'
                and options.get(('deps', 'source_file')) == f'"{helpers["resource_path"](path)}"', 'forest import paths changed')
        require(options.get(('remap', 'importer')) == '"texture"', 'forest importer changed')
        uid = options.get(('remap', 'uid'))
        require(uid is not None and uid.startswith('"uid://') and all_uids.count(uid) == 1, 'forest import UID shared/missing')
        for key, value in {'compress/mode': '0', 'mipmaps/generate': 'false',
                           'process/fix_alpha_border': 'false', 'process/premult_alpha': 'false',
                           'process/size_limit': '0', **{f'process/channel_remap/{c}': str(i)
                            for i, c in enumerate(('red', 'green', 'blue', 'alpha'))}}.items():
            require(options.get(('params', key)) == value, f'forest import option changed: {key}')
    print('PASS: exact accepted forest asset, recipe, editable master and independent imports')


def captures(record=False):
    """Bind current capture inputs and require all 18 runtime views to equal accepted pixels."""
    context = json.loads((HERE / 'capture-context.json').read_text())
    expected_inputs = {'game/Main.tscn', 'game/GreyboxNavigation.cs', 'scripts/capture-closed-forest-path-runtime.gd',
                       'game/art/runtime/terrain/' + FILENAME, 'game/art/runtime/terrain/' + FILENAME + '.import'}
    require(set(context) == expected_inputs and all(sha(ROOT / p) == digest for p, digest in context.items()),
            'runtime captures have stale inputs')
    evidence = {}
    for target in ('gate', 'midpoint', 'closed-end'):
        for zoom in ('0.8', '1.0', '2.0'):
            for frame, height in (('wide', 720), ('tall', 827)):
                suffix = f'{target}-{zoom}x-{frame}.png'
                path = HERE / 'captures' / ('runtime-' + suffix)
                actual, expected = rgba(path), rgba(CORRECTION / 'result/captures' / ('corrected-' + suffix))
                require(actual.size == (1280, height) and actual.getchannel('A').getextrema() == (255, 255)
                        and actual.tobytes() == expected.tobytes(), f'runtime view differs: {suffix}')
                evidence[path.name] = sha(path)
    path = HERE / 'capture-evidence.json'
    if record:
        path.write_text(json.dumps(evidence, indent=2) + '\n')
    else:
        require(json.loads(path.read_text()) == evidence, 'runtime capture evidence changed')
    print('PASS: 18 fresh runtime captures exactly match accepted external RGBA pixels')


def archived():
    """Reproduce historical proofs on their pinned complete tree without modifying live sources."""
    with tempfile.TemporaryDirectory(prefix='allrpcs-forest-history-') as directory:
        snapshot = Path(directory) / 'tree'
        snapshot.mkdir()
        archive = Path(directory) / 'tree.tar'
        with archive.open('wb') as output:
            subprocess.run(['git', 'archive', BASE_COMMIT], cwd=ROOT, stdout=output, check=True)
        with tarfile.open(archive) as package:
            package.extractall(snapshot, filter='data')
        subprocess.run(['git', 'init', '-q'], cwd=snapshot, check=True)
        subprocess.run(['git', 'add', '--force', '--all'], cwd=snapshot, check=True)
        for tool, option in [('proof.py', '--check-predecessors'), ('check.py', None)]:
            command = [sys.executable, str(snapshot / 'art-direction/forest-path-ending-correction/tools' / tool)]
            if option:
                command.append(option)
            subprocess.run(command, cwd=snapshot, check=True)
    print('PASS: historical correction and predecessors reproduced in pinned disposable tree')


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--check-captures', action='store_true')
    parser.add_argument('--record-captures', action='store_true')
    parser.add_argument('--check-archived', action='store_true')
    args = parser.parse_args()
    scope()
    asset()
    if args.check_captures or args.record_captures:
        captures(args.record_captures)
    if args.check_archived:
        archived()
