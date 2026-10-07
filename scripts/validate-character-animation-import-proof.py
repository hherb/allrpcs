#!/usr/bin/env python3
"""Independently validate the complete approved directional import and registration proof."""

import argparse
import hashlib
import json
from pathlib import Path
import re
import subprocess
import tempfile

from PIL import Image

REPO = Path(__file__).resolve().parents[1]
PACKAGE = REPO / 'art-direction/character-animation-import-proof'
ART = REPO / 'game/art/character-animation-import-proof'
DIRECTIONS = ['south', 'west', 'east', 'north']
FAMILIES = ['seeker-cloak', 'seeker-plain', 'seeker-apron', 'healer', 'elian',
            'sera', 'orin', 'vale', 'rowan', 'mira']
MAX_CONTACT_GAP_PIXELS = 3


def require(value, message):
    """Reject drift with a concise, actionable diagnostic."""
    if not value:
        raise ValueError(message)


def sha(path):
    """Return the exact-byte digest for one file."""
    return hashlib.sha256(path.read_bytes()).hexdigest()


def options(path):
    """Read scalar Godot import options independently of the writer."""
    section = ''
    values = {}
    for line in path.read_text().splitlines():
        if line.startswith('[') and line.endswith(']'):
            section = line[1:-1]
        elif '=' in line:
            key, value = line.split('=', 1)
            values[(section, key)] = value
    return values


def validate():
    """Check accepted provenance, measured support, exact file sets and retained ordinary inputs."""
    registration = json.loads((PACKAGE / 'registration.json').read_text())
    require((PACKAGE / 'registration.json').read_bytes() == (ART / 'registration.json').read_bytes(),
            'Game/package registration disagree')
    source_root = REPO / 'art-direction/characters-batch'
    approval = json.loads((source_root / 'owner-acceptance.json').read_text())
    require(sha(source_root / 'owner-acceptance.json') == registration['artwork_acceptance_sha256'],
            'Changed artwork authority')
    expected = {}
    for family in FAMILIES[1:]:
        for direction in DIRECTIONS:
            for action, count in [('idle', 2), ('inspect', 2), ('walk', 8)]:
                for index in range(1, count + 1):
                    identity = f'{family}-{direction}-{action}-{index:02d}'
                    row = approval['active_assets'][identity]
                    expected[identity] = {'source': 'art-direction/characters-batch/' + row['export'],
                                          'sha256': row['sha256'], 'master': 'art-direction/characters-batch/' + row['master'],
                                          'master_sha256': row['master_sha256'], 'pivot': [64, 191]}
    for inherited in approval['inherited_seeker_frames']:
        path = Path(inherited['path'])
        name, index = path.stem.rsplit('-', 1)
        action = {'idle-breathe': 'idle', 'interact-reach': 'inspect', 'walk': 'walk'}[name]
        identity = f'seeker-cloak-{path.parent.name}-{action}-{index}'
        width = 192 if action == 'inspect' else 144 if path.parent.name in ('west', 'east') else 108
        expected[identity] = dict(source=inherited['path'], sha256=inherited['sha256'], pivot=[width // 2, 156])
    frames = registration['frames']
    require(len(frames) == 480 and len({f['id'] for f in frames}) == 480, 'Wrong/duplicate frame inventory')
    require({f['id'] for f in frames} == set(expected), 'Missing directional frame')
    require(registration['family_order'] == FAMILIES and registration['direction_order'] == DIRECTIONS,
            'Family/direction order changed')
    old_uids = {options(path).get(('remap', 'uid')) for path in (REPO / 'game').rglob('*.png.import')
                if not path.is_relative_to(ART)}
    uids = set()
    for frame in frames:
        identity = frame['id']
        for key, value in expected[identity].items():
            require(frame[key] == value, 'Authority/registration drift: ' + identity)
        source = REPO / frame['source']
        texture = ART / 'frames' / (identity + '.png')
        require(frame['texture'] == str(texture.relative_to(REPO)), 'Texture mapping drift')
        require(sha(source) == sha(texture) == frame['sha256'], 'PNG byte parity failed: ' + identity)
        require(sha(REPO / frame['master']) == frame['master_sha256'], 'Editable master changed')
        image = Image.open(source).convert('RGBA')
        imported_source = Image.open(texture).convert('RGBA')
        require(image.tobytes() == imported_source.tobytes(), 'Decoded source/copy RGBA mismatch')
        measured = frame['measurements']
        alpha = image.getchannel('A')
        paint = alpha.point(lambda value: 255 if value > 96 else 0)
        bounds = paint.getbbox()
        band = paint.crop((0, image.height - 24, image.width, image.height)).getbbox()
        require(measured['size'] == list(image.size) and measured['rgba_sha256'] == hashlib.sha256(image.tobytes()).hexdigest(),
                'Geometry/decoded hash drift')
        require(measured['alpha_bounds'] == list(alpha.getbbox()) and measured['paint_bounds_alpha_gt_96'] == list(bounds),
                'Alpha measurements drift')
        require(measured['bottom_band_x_bounds_alpha_gt_96'] == [band[0], band[2]], 'Foot support measurement drift')
        relative = bounds[3] - 1 - frame['pivot'][1]
        require(-3 <= relative <= 0 and measured['paint_bottom_relative_to_pivot'] == relative,
                'Measured paint no longer supports fixed registration')
        horizontal_gap = max(band[0] - frame['pivot'][0], frame['pivot'][0] - (band[2] - 1), 0)
        require(horizontal_gap <= MAX_CONTACT_GAP_PIXELS, 'Paint support moved too far from fixed origin')
        if frame['family'] != 'seeker-cloak':
            require(image.size == (128, 192), 'New canvas changed')
        resource = 'res://' + frame['texture'].removeprefix('game/')
        cache = 'res://.godot/imported/' + texture.name + '-' + hashlib.md5(resource.encode(), usedforsecurity=False).hexdigest() + '.ctex'
        settings = options(texture.with_suffix('.png.import'))
        require(settings[('deps', 'source_file')] == json.dumps(resource)
                and settings[('remap', 'path')] == json.dumps(cache)
                and settings[('deps', 'dest_files')] == '[' + json.dumps(cache) + ']', 'Derived import path drift')
        uid = settings.get(('remap', 'uid'))
        require(uid and uid.startswith('"uid://') and uid not in old_uids and uid not in uids, 'Import UID missing/shared')
        uids.add(uid)
        for key, value in {'compress/mode': '0', 'mipmaps/generate': 'false',
                           'process/fix_alpha_border': 'false', 'process/premult_alpha': 'false',
                           'process/channel_remap/red': '0', 'process/channel_remap/green': '1',
                           'process/channel_remap/blue': '2', 'process/channel_remap/alpha': '3',
                           'process/size_limit': '0'}.items():
            require(settings.get(('params', key)) == value, 'Import processing drift: ' + key)
    require(set(ART.rglob('*.png')) == {ART / 'frames' / (identity + '.png') for identity in expected}, 'Extra/missing proof textures')
    require(len(list(ART.rglob('*.png.import'))) == 480, 'Extra/missing sidecars')
    require(set(registration['families']) == set(FAMILIES), 'Wrong animation family set')
    require(set(ART.rglob('*.tres')) == {ART / 'animations' / (family + '.tres') for family in FAMILIES}, 'Wrong SpriteFrames resources')
    for family in FAMILIES:
        animations = registration['families'][family]['animations']
        require(len(animations) == 12, 'Incomplete animation set')
        require({a['name'] for a in animations} == {a + '_' + d for a in ['idle', 'inspect', 'walk'] for d in DIRECTIONS},
                'Missing action/direction animation')
        for animation in animations:
            action, direction = animation['name'].split('_')
            count = {'idle': 2, 'inspect': 4 if family == 'seeker-cloak' else 2,
                     'walk': 6 if family == 'seeker-cloak' else 8}[action]
            require(animation['frames'] == [f'{family}-{direction}-{action}-{i:02d}' for i in range(1, count + 1)],
                    'Incomplete/reordered loop')
            ms = {'idle': 600, 'inspect': 160, 'walk': 120 if family == 'seeker-cloak' else 90}[action]
            require(animation['frame_ms'] == ms and animation['loop'] == (action != 'inspect'), 'Cadence/gesture drift')
    boundary = json.loads((PACKAGE / 'boundary-inputs.json').read_text())
    for path, digest in boundary['files'].items():
        require(sha(REPO / path) == digest, 'Unrelated/ordinary product input changed: ' + path)
    if (PACKAGE / 'verification.json').exists():
        verification = json.loads((PACKAGE / 'verification.json').read_text())
        require(verification['status'] == 'passed', 'Retained verification incomplete')
        for path, digest in verification['bindings'].items():
            require(sha(REPO / path) == digest, 'Verified proof/capture drift: ' + path)
    return registration


def replay():
    """Reproduce every authored import file without touching retained proof evidence."""
    with tempfile.TemporaryDirectory(prefix='character-proof-replay-') as temporary:
        output = Path(temporary)
        subprocess.run(['/opt/homebrew/bin/python3', str(PACKAGE / 'build.py'), '--output-root', temporary], check=True)
        names = {path.relative_to(output) for path in output.rglob('*') if path.is_file()}
        expected = {path.relative_to(REPO) for path in ART.rglob('*') if path.is_file()}
        expected.add((PACKAGE / 'registration.json').relative_to(REPO))
        require(names == expected, 'Recipe does not reproduce exact file set')
        for name in names:
            require((output / name).read_bytes() == (REPO / name).read_bytes(), 'Recipe byte mismatch: ' + str(name))
    print('PASS: byte-exact deterministic recipe replay')


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--replay', action='store_true')
    args = parser.parse_args()
    validate()
    if args.replay:
        replay()
    print('PASS: 480 accepted frames, measured alpha/registration, lossless imports and unchanged ordinary product inputs')
