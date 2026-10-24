#!/usr/bin/env python3
"""Validate structure paint provenance and additive presentation-only scene wiring."""
from io import BytesIO
import hashlib
import json
from pathlib import Path
import subprocess
import zipfile
from xml.etree.ElementTree import fromstring

from PIL import Image

ROOT = Path(__file__).resolve().parents[1]
PACKAGE = ROOT / 'art-direction/map-structures-runtime'
CURRENT = ROOT / 'art-direction/buildings-gameplay-integration'
BASE = json.loads((CURRENT / 'authority.json').read_text())['base_commit']
ART_BASE = 'cfed60a8a7372453cb1da56476a5acfdbc7a98ae'


def require(condition, message):
    """Fail with one actionable asset or preservation error."""
    if not condition:
        raise ValueError(message)


def sha(path):
    """Hash exact persisted bytes."""
    return hashlib.sha256(path.read_bytes()).hexdigest()


def original(path):
    """Read the recorded predecessor without trusting a mutable worktree copy."""
    return subprocess.check_output(['git', 'show', f'{ART_BASE}:{path}'], cwd=ROOT)


def validate():
    """Check every new texture/master and preservation of prior accepted paint and physics."""
    manifest = json.loads((PACKAGE / 'manifest.json').read_text())
    require(len(manifest['assets']) == 9, 'Expected exactly nine runtime structure textures')
    for asset in manifest['assets']:
        runtime = ROOT / asset['runtime']
        source = ROOT / asset['source']
        require(sha(runtime) == asset['sha256'], 'Runtime paint changed: ' + asset['id'])
        with Image.open(runtime) as paint:
            require(paint.mode == 'RGBA' and list(paint.size) == asset['size'], 'RGBA/size drift')
            alpha = paint.getchannel('A')
            require(alpha.getextrema() == ((0, 255) if asset.get('transparent', True) else (255, 255)), 'Alpha mode changed')
            if 'generator' in asset:
                require(all(pixel[:3] == (0, 0, 0) for pixel in paint.get_flattened_data() if pixel[3] == 0), 'Existing derived export hidden RGB')
        if 'generator' not in asset:
            require(source.read_bytes() == runtime.read_bytes(), 'Accepted source pixels changed')
            require(source.read_bytes() == original(asset['source']), 'Accepted source no longer historical')
            continue
        require(sha(source) == asset['source_sha256'], 'Generated source changed')
        master = ROOT / asset['master']
        require(sha(master) == asset['master_sha256'], 'Existing editable master changed')
        with zipfile.ZipFile(master) as archive:
            xml = fromstring(archive.read('stack.xml'))
            require((int(xml.attrib['w']), int(xml.attrib['h'])) == tuple(asset['master_size']), 'Master size')
            require([node.attrib['name'] for node in xml.find('stack')] == ['Corrections', 'Generated paint'], 'Editable layers')
            master_paint = Image.open(BytesIO(archive.read('data/paint.png'))).convert('RGBA')
            require(master_paint.tobytes() == Image.open(source).convert('RGBA').tobytes(), 'Master/source paint parity')
            require(master_paint.size == tuple(value * 2 for value in asset['size']), 'Not an exact 2x master')
    scene = (ROOT / 'game/Main.tscn').read_text()
    resource = '[ext_resource type="PackedScene" path="res://MapStructuresPresentation.tscn" id="map_structures"]\n\n'
    node = '[node name="MapStructuresPresentation" parent="." instance=ExtResource("map_structures")]\n\n'
    require(scene.count(resource) == scene.count(node) == 1, 'Missing/duplicate scene wiring')
    scene = scene.replace(resource, '', 1).replace(node, '', 1)
    baseline = subprocess.check_output(['git', 'show', f'{BASE}:game/Main.tscn'], cwd=ROOT).decode()
    import re
    previous_steps = int(re.search(r'load_steps=(\d+)', baseline).group(1))
    require(int(re.search(r'load_steps=(\d+)', scene).group(1)) == previous_steps + 1,
            'Scene resource count drift')
    scene = scene.replace(f'load_steps={previous_steps + 1}', f'load_steps={previous_steps}', 1)
    require(scene == baseline, 'Unscoped old scene or physical change')
    names = subprocess.check_output(['git', 'diff', BASE, '--name-only'], cwd=ROOT, text=True).splitlines()
    require(not any(name.startswith('art-direction/') and not name.startswith(('art-direction/map-structures-runtime/', 'art-direction/buildings-gameplay-integration/'))
                    for name in names), 'Previously accepted artwork/evidence changed')
    require(not any(name.startswith(('src/', 'game/art/runtime/terrain/', 'game/art/runtime/characters/')) for name in names),
            'Gameplay, terrain or character sources changed')
    for path in PACKAGE.rglob('*'):
        if path.is_file() and path.name != '.DS_Store':
            require(path.read_bytes() == original(path.relative_to(ROOT).as_posix()),
                    'Recovered historical artwork package changed: ' + str(path))
    capture_path = CURRENT / 'capture-evidence.json'
    if capture_path.exists():
        capture = json.loads(capture_path.read_text())
        require(len(capture['captures']) == 12, 'Incomplete capture series')
        for name, expected in capture['captures'].items():
            require(sha(CURRENT / name) == expected, 'Capture changed: ' + name)
        for name, expected in capture['inputs'].items():
            path = (ROOT / 'game' / name.removeprefix('res://')).resolve()
            require(path.is_relative_to(ROOT) and len(expected) == 64 and sha(path) == expected,
                    'Capture input changed: ' + name)
    print('PASS: nine runtime textures, six exact approved exports, three existing layered 2x masters, additive scene and preserved accepted art/physics')


if __name__ == '__main__':
    validate()
