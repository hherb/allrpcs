#!/usr/bin/env python3
"""Copy accepted frames and deterministically register an isolated Godot proof."""

import argparse
import hashlib
import json
from pathlib import Path
import re
import shutil

from PIL import Image

REPO = Path(__file__).resolve().parents[2]
PACKAGE = Path('art-direction/character-animation-import-proof')
GAME_ART = Path('game/art/character-animation-import-proof')
SOURCE = REPO / 'art-direction/characters-batch'
FAMILIES = ['seeker-cloak', 'seeker-plain', 'seeker-apron', 'healer', 'elian',
            'sera', 'orin', 'vale', 'rowan', 'mira']
DIRECTIONS = ['south', 'west', 'east', 'north']
COUNTS = {'idle': 2, 'inspect': 2, 'walk': 8}
PARAMS = {'compress/mode': '0', 'mipmaps/generate': 'false',
          'process/fix_alpha_border': 'false', 'process/premult_alpha': 'false',
          'process/channel_remap/red': '0', 'process/channel_remap/green': '1',
          'process/channel_remap/blue': '2', 'process/channel_remap/alpha': '3',
          'process/size_limit': '0'}
DEFAULT_PARAMS = {'compress/mode': '0', 'compress/high_quality': 'false',
                  'compress/lossy_quality': '0.7', 'compress/uastc_level': '0',
                  'compress/rdo_quality_loss': '0.0', 'compress/hdr_compression': '1',
                  'compress/normal_map': '0', 'compress/channel_pack': '0',
                  'mipmaps/generate': 'false', 'mipmaps/limit': '-1',
                  'roughness/mode': '0', 'roughness/src_normal': '""',
                  'process/channel_remap/red': '0', 'process/channel_remap/green': '1',
                  'process/channel_remap/blue': '2', 'process/channel_remap/alpha': '3',
                  'process/fix_alpha_border': 'false', 'process/premult_alpha': 'false',
                  'process/normal_map_invert_y': 'false', 'process/hdr_as_srgb': 'false',
                  'process/hdr_clamp_exposure': 'false', 'process/size_limit': '0',
                  'detect_3d/compress_to': '1'}


def require(value, message):
    """Reject incomplete or changed accepted inputs before writing proof files."""
    if not value:
        raise ValueError(message)


def sha(path):
    """Hash exact file bytes."""
    return hashlib.sha256(path.read_bytes()).hexdigest()


def write_json(path, value):
    """Write a stable, readable JSON artifact."""
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(value, indent=2) + '\n')


def uid_for(resource):
    """Encode a path-owned 63-bit identifier using Godot's canonical base 34."""
    alphabet = 'abcdefghijklmnopqrstuvwxy012345678'
    number = int(hashlib.sha256(resource.encode()).hexdigest()[:16], 16) & ((1 << 63) - 1)
    encoded = ''
    while number:
        encoded = alphabet[number % 34] + encoded
        number //= 34
    return 'uid://' + (encoded or 'a')


def sidecar(resource):
    """Return deterministic lossless metadata with all RGBA processing disabled."""
    cache_hash = hashlib.md5(resource.encode(), usedforsecurity=False).hexdigest()
    cache = f'res://.godot/imported/{Path(resource).name}-{cache_hash}.ctex'
    return ('[remap]\n\nimporter="texture"\ntype="CompressedTexture2D"\n'
            f'uid="{uid_for(resource)}"\npath="{cache}"\nmetadata={{\n"vram_texture": false\n}}\n'
            f'\n[deps]\n\nsource_file="{resource}"\ndest_files=["{cache}"]\n'
            '\n[params]\n\n' + '\n'.join(f'{key}={value}' for key, value in DEFAULT_PARAMS.items()) + '\n')


def measure(path, pivot):
    """Measure retained alpha, substantial paint and bottom-band support without cropping."""
    image = Image.open(path).convert('RGBA')
    alpha = image.getchannel('A')
    strong = alpha.point(lambda value: 255 if value > 96 else 0)
    bounds = strong.getbbox()
    require(bounds is not None, f'Empty substantive paint: {path}')
    band = strong.crop((0, image.height - 24, image.width, image.height)).getbbox()
    require(band is not None, f'No bottom-band support: {path}')
    edges = [alpha.crop(box).getbbox() is not None for box in
             [(0, 0, image.width, 1), (0, 0, 1, image.height),
              (image.width - 1, 0, image.width, image.height),
              (0, image.height - 1, image.width, image.height)]]
    return {'size': list(image.size), 'rgba_sha256': hashlib.sha256(image.tobytes()).hexdigest(),
            'alpha_bounds': list(alpha.getbbox()), 'paint_bounds_alpha_gt_96': list(bounds),
            'bottom_band_x_bounds_alpha_gt_96': [band[0], band[2]],
            'paint_bottom_relative_to_pivot': bounds[3] - 1 - pivot[1],
            'alpha_touches_top_left_right_bottom': edges,
            'transparent_pixels': alpha.histogram()[0],
            'partial_alpha_pixels': sum(alpha.histogram()[1:255])}


def accepted_frames():
    """Resolve only approved directional assets, retaining their editable master lineage."""
    approval = json.loads((SOURCE / 'owner-acceptance.json').read_text())
    frames = []
    for family in FAMILIES[1:]:
        for direction in DIRECTIONS:
            for action, count in COUNTS.items():
                for index in range(1, count + 1):
                    identity = f'{family}-{direction}-{action}-{index:02d}'
                    accepted = approval['active_assets'][identity]
                    frames.append(dict(id=identity, family=family, direction=direction,
                                       action=action, index=index, pivot=[64, 191],
                                       source=str((SOURCE / accepted['export']).relative_to(REPO)),
                                       sha256=accepted['sha256'],
                                       master=str((SOURCE / accepted['master']).relative_to(REPO)),
                                       master_sha256=accepted['master_sha256']))
    manifest = json.loads((REPO / 'art-direction/production-proof/corrected/asset-manifest.json').read_text())
    assets = {row['export']: row for row in manifest['assets']}
    for inherited in approval['inherited_seeker_frames']:
        path = Path(inherited['path'])
        match = re.fullmatch(r'(idle-breathe|interact-reach|walk)-(\d\d)', path.stem)
        require(match is not None, 'Unrecognized inherited frame')
        action = {'idle-breathe': 'idle', 'interact-reach': 'inspect', 'walk': 'walk'}[match[1]]
        direction = path.parent.name
        asset = assets['exports/characters/seeker/' + direction + '/' + path.name]
        master = 'art-direction/production-proof/corrected/masters/seeker-four-direction-' + match[1] + '.ora'
        frames.append(dict(id=f'seeker-cloak-{direction}-{action}-{match[2]}', family='seeker-cloak',
                           direction=direction, action=action, index=int(match[2]),
                           source=inherited['path'], sha256=inherited['sha256'],
                           master=master, master_sha256=sha(REPO / master),
                           pivot=[asset['pivot']['x'], asset['pivot']['y']]))
    require(len(frames) == 480, 'Directional set must contain 480 exact frames')
    for frame in frames:
        require(sha(REPO / frame['source']) == frame['sha256'], 'Changed accepted export: ' + frame['id'])
        require(sha(REPO / frame['master']) == frame['master_sha256'], 'Changed master: ' + frame['id'])
        frame['texture'] = str(GAME_ART / 'frames' / (frame['id'] + '.png'))
        frame['measurements'] = measure(REPO / frame['source'], frame['pivot'])
    return frames


def animations_for(family, frames):
    """Create ordered complete loops and non-looping gestures at documented cadences."""
    animations = []
    for direction in DIRECTIONS:
        for action in COUNTS:
            selected = sorted([frame for frame in frames if frame['family'] == family
                               and frame['direction'] == direction and frame['action'] == action],
                              key=lambda frame: frame['index'])
            ms = {'idle': 600, 'inspect': 160, 'walk': 120 if family == 'seeker-cloak' else 90}[action]
            animations.append(dict(name=f'{action}_{direction}', frames=[f['id'] for f in selected],
                                   frame_ms=ms, loop=action != 'inspect'))
    return animations


def sprite_frames(frames, animations):
    """Serialize SpriteFrames with independent path-owned textures and exact frame order."""
    identities = {frame['id']: str(index + 1) for index, frame in enumerate(frames)}
    lines = [f'[gd_resource type="SpriteFrames" load_steps={len(frames) + 1} format=3]', '']
    for frame in frames:
        resource = 'res://' + frame['texture'].removeprefix('game/')
        lines.append(f'[ext_resource type="Texture2D" path="{resource}" id="{identities[frame["id"]]}"]')
    lines += ['', '[resource]', 'animations = [']
    for index, animation in enumerate(animations):
        references = ', '.join('{"duration": 1.0, "texture": ExtResource("' + identities[name] + '")}'
                               for name in animation['frames'])
        lines += ['{', '"frames": [' + references + '],',
                  '"loop": ' + str(animation['loop']).lower() + ',',
                  '"name": &"' + animation['name'] + '",',
                  f'"speed": {1000 / animation["frame_ms"]:.9f}',
                  '},' if index < len(animations) - 1 else '}']
    return '\n'.join(lines + [']', ''])


def build(output):
    """Write only this proof's additive artifacts; refuse to overwrite retained verification."""
    require(not (output / PACKAGE / 'verification.json').exists(), 'Verified proof is frozen; replay elsewhere')
    frames = accepted_frames()
    families = {}
    for family in FAMILIES:
        subset = [frame for frame in frames if frame['family'] == family]
        animations = animations_for(family, subset)
        path = GAME_ART / 'animations' / (family + '.tres')
        (output / path).parent.mkdir(parents=True, exist_ok=True)
        (output / path).write_text(sprite_frames(subset, animations))
        families[family] = {'resource': str(path), 'animations': animations}
    for frame in frames:
        target = output / frame['texture']
        target.parent.mkdir(parents=True, exist_ok=True)
        shutil.copyfile(REPO / frame['source'], target)
        resource = 'res://' + frame['texture'].removeprefix('game/')
        target.with_suffix('.png.import').write_text(sidecar(resource))
    registration = {'schema_version': 1, 'date': '2026-10-06', 'scope': 'isolated proof only',
                    'artwork_acceptance': 'art-direction/characters-batch/owner-acceptance.json',
                    'artwork_acceptance_sha256': sha(SOURCE / 'owner-acceptance.json'),
                    'registration_policy': 'fixed approved canvas pivot per direction; retain authored bob and reach',
                    'walk_policy': 'new 8 x 90ms matches inherited 6 x 120ms cycle; compare approved 8 x 100ms preview',
                    'inspect_policy': '160ms per frame; non-looping gesture replayed only in proof',
                    'import_parameters': PARAMS, 'family_order': FAMILIES, 'direction_order': DIRECTIONS,
                    'families': families, 'frames': frames}
    write_json(output / PACKAGE / 'registration.json', registration)
    write_json(output / GAME_ART / 'registration.json', registration)
    print('PASS: deterministic proof recipe: 432 approved directional + 48 inherited frames; 120 animations')


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--output-root', type=Path, default=REPO)
    build(parser.parse_args().output_root.resolve())
