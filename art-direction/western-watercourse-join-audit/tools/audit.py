#!/usr/bin/env python3
"""Audit the external B1/retained Stream interface without changing accepted pixels."""

import argparse
import hashlib
import importlib.util
import io
import json
from pathlib import Path
import subprocess
import sys

from PIL import Image, ImageDraw, ImageFont

ROOT = Path(__file__).resolve().parents[3]
HERE = Path(__file__).resolve().parents[1]
PREFIX = 'art-direction/western-watercourse-join-audit/'
BASE_COMMIT = '8691d7ce50e9b3284f5ecb7c6c6cf68622b12e6b'
VIEW = 'art-direction/western-outlet-proof/review/engineering.png'
VIEW_SHA = '14a1397d3b1c9d2aa9da85fd266373a183528bd3c7497daf5a6f870668407bfe'
SOURCES = (
    VIEW, 'art-direction/b1-bridge-proof/tools/proof.py',
    'art-direction/b1-bridge-proof/exports/water.png',
    'art-direction/b1-bridge-proof/exports/bank.png',
    'art-direction/western-watercourse-proposal/proposal.json',
    'game/Main.tscn', 'game/art/runtime/terrain/stream-water.png',
    'game/art/runtime/terrain/stream-bank.png',
    'art-direction/b1-bridge-proof/exports/joined.png',
    'art-direction/production-proof/corrected/exports/terrain/quiet-moss.png',
    'art-direction/b1-bridge-proof/masters/b1-joined.ora',
)


def require(condition, message):
    """Keep all checks active under optimized Python."""
    if not condition:
        raise ValueError(message)


def sha(path):
    """Read one artifact's digest without modifying it."""
    return hashlib.sha256(path.read_bytes()).hexdigest()


def inventory():
    """Protect all tracked predecessors except the handover and this successor audit."""
    names = subprocess.check_output(['git', 'ls-files', '-z'], cwd=ROOT).decode().split('\0')
    return sorted(name for name in names if name and name != 'HANDOVER.md'
                  and not name.startswith(PREFIX) and Path(name).name != '.DS_Store')


def scope(build):
    """Freeze the named baseline once; reject changed, missing or unscoped files."""
    path = HERE / 'baseline.json'
    if not path.exists():
        require(build, 'baseline missing')
        head = subprocess.check_output(['git', 'rev-parse', 'HEAD'], cwd=ROOT).decode().strip()
        require(head == BASE_COMMIT, 'unexpected baseline head')
        path.write_text(json.dumps({name: sha(ROOT / name) for name in inventory()}, indent=2) + '\n')
    baseline = json.loads(path.read_text())
    require(set(baseline) == set(inventory()), 'protected inventory changed')
    for name, digest in baseline.items():
        require(sha(ROOT / name) == digest, f'protected file changed: {name}')
    names = subprocess.check_output(['git', 'ls-files', '--others', '--exclude-standard', '-z'], cwd=ROOT)
    for name in names.decode().split('\0'):
        require(not name or name.startswith(PREFIX) or Path(name).name == '.DS_Store',
                f'unscoped addition: {name}')
    return baseline


def node(scene, name):
    """Extract one exact authored node, preventing matches in a different scene owner."""
    header = f'[node name="{name}" '
    matches = [block for block in scene.split('\n\n') if block.startswith(header)]
    require(len(matches) == 1, f'missing or ambiguous scene node: {name}')
    return matches[0]


def inputs():
    """Bind source geometry and UV facts before reporting any interface measurements."""
    require(sha(ROOT / VIEW) == VIEW_SHA, 'shown outlet digest changed')
    config = json.loads((ROOT / SOURCES[4]).read_text())
    require(config['retained_water'] == [1600, 452, 2000, 548], 'retained bounds changed')
    require(config['western_water_polygon'] == [[0, 552], [1400, 552], [1600, 452],
                                               [1600, 548], [1400, 648], [0, 648]],
            'western envelope changed')
    scene = (ROOT / 'game/Main.tscn').read_text()
    water = node(scene, 'StreamWater')
    require('polygon = PackedVector2Array(1600, 452, 2000, 452, 2000, 548, 1600, 548)' in water
            and 'uv = PackedVector2Array(0, 0, 400, 0, 400, 96, 0, 96)' in water,
            'retained water geometry/UV changed')
    for name, y in (('NorthWestBank', 452), ('SouthWestBank', 516)):
        bank = node(scene, name)
        require('region_rect = Rect2(60, 0, 68, 32)' in bank
                and f'position = Vector2(1600, {y})' in bank, 'retained west bank changed')
        require('parent="OpeningRegionStreamLaundrySceneSource"' in bank,
                'retained bank owner changed')
    require('flip_v = true' in node(scene, 'SouthWestBank'), 'south bank mirroring changed')


def measurements():
    """Compare texture phases at common coordinates, not adjacent naturally unequal columns."""
    with Image.open(ROOT / 'game/art/runtime/terrain/stream-water.png') as source:
        tile = source.convert('RGBA')
    require(tile.size == (128, 96), 'water tile dimensions changed')
    # One complete tile period at the interface. This is a counterfactual extension
    # of each recipe over the same rectangle, not an overlap of authored exports.
    different = sum(tile.getpixel((x % 128, y % 96)) !=
                    tile.getpixel(((x - 1600) % 128, (y - 452) % 96))
                    for y in range(452, 548) for x in range(1600, 1728))
    with Image.open(ROOT / SOURCES[2]) as stream:
        b1_water = stream.convert('RGBA')
    with Image.open(ROOT / 'art-direction/b1-bridge-proof/exports/bank.png') as stream:
        b1_bank = stream.convert('RGBA')
    # The final B1 export column is x=1599. Check the exact world UV recipe across
    # its fully covered interior, binding the formula to frozen pixels. Fractional
    # edge rows have BOX-filtered coverage and are verified by predecessor reproduction.
    for y in range(453, 548):
        require(b1_water.getpixel((511, y - 384)) == tile.getpixel((1599 % 128, y % 96)),
                'B1 boundary water differs from world phase')
    require(all(b1_bank.getpixel((511, y - 384))[3] == 0 for y in range(453, 548)),
            'B1 boundary bank intrudes into water')
    return {
        'interface_x': 1600,
        'western_water_edge_limit': [452, 548],
        'retained_water_edges': [452, 548],
        'geometric_water_envelopes_meet': True,
        'western_water_uv_origin': [0, 0],
        'retained_water_uv_origin': [1600, 452],
        'phase_offset_mod_tile': [64, 68],
        'comparison_kind': 'same-coordinate recipe comparison; not authored overlap or seam acceptance',
        'comparison_rectangle': [1600, 452, 1728, 548],
        'compared_texels': 128 * 96,
        'different_texels': different,
        'western_bank_limit_north': [436, 452],
        'western_bank_limit_south': [548, 564],
        'retained_bank_north': [452, 484],
        'retained_bank_south': [516, 548],
        'bank_convention_matches': False,
        'conclusion': 'geometry meets; water phase and inside/outside bank conventions need a bounded join proof',
        'runtime_layout_physical_promotion_authorized': False,
    }


def predecessor(baseline):
    """Reproduce frozen outlet and spring with only the scope audit replaced by a verified superset."""
    path = ROOT / 'art-direction/western-outlet-proof/tools/proof.py'
    spec = importlib.util.spec_from_file_location('join_audit_outlet', path)
    module = importlib.util.module_from_spec(spec)
    sys.modules[spec.name] = module
    spec.loader.exec_module(module)
    old = json.loads((module.HERE / 'baseline.json').read_text())
    require(all(baseline.get(name) == digest for name, digest in old.items()), 'predecessor baseline changed')
    module.check_scope = lambda build: len(old)
    module.run(False)
    spring_root = ROOT / 'art-direction/spring-curved-bank-proof'
    spring_spec = importlib.util.spec_from_file_location('join_audit_spring', spring_root / 'tools/proof.py')
    spring = importlib.util.module_from_spec(spring_spec)
    sys.modules[spring_spec.name] = spring
    spring_spec.loader.exec_module(spring)
    spring_baseline = json.loads((spring_root / 'baseline.json').read_text())
    require(all(baseline.get(name) == digest for name, digest in spring_baseline.items()),
            'spring predecessor baseline changed')
    spring.check_scope = lambda build: len(spring_baseline)
    spring.run(False)


def diagnostic(origin=(1536, 400), size=(128, 192)):
    """Show frozen B1 pixels beside exact retained west water/banks; do not repair the seam."""
    with Image.open(ROOT / SOURCES[8]) as source:
        left = source.convert('RGBA').crop((448, origin[1] - 384, 512, origin[1] - 384 + size[1]))
    textures = {}
    for key, path in (('ground', SOURCES[9]), ('water', SOURCES[6]), ('bank', SOURCES[7])):
        with Image.open(ROOT / path) as source:
            textures[key] = source.convert('RGBA')
    result = Image.new('RGBA', size)
    result.paste(left, (0, 0))
    for y in range(size[1]):
        wy = origin[1] + y
        for x in range(64, size[0]):
            wx = origin[0] + x
            ground = textures['ground']
            pixel = ground.getpixel((wx % ground.width, wy % ground.height))
            if 452 <= wy < 548:
                pixel = textures['water'].getpixel(((wx - 1600) % 128, (wy - 452) % 96))
                row = wy - 452 if wy < 484 else 547 - wy if wy >= 516 else None
                if row is not None:
                    bank = textures['bank'].getpixel((60 + wx - 1600, row))
                    underlay = Image.new('RGBA', (1, 1), pixel)
                    underlay.alpha_composite(Image.new('RGBA', (1, 1), bank))
                    pixel = underlay.getpixel((0, 0))
            result.putpixel((x, y), pixel)
    require(result.crop((0, 0, 64, size[1])).tobytes() == left.tobytes(), 'diagnostic changed B1 pixels')
    large = result.resize((size[0] * 2, size[1] * 2), Image.Resampling.NEAREST)
    annotated = large.copy()
    draw = ImageDraw.Draw(annotated)
    draw.line((128, 0, 128, large.height - 1), fill='#ffd66e')
    draw.text((4, 6), 'B1 | retained Stream', fill='white', font=ImageFont.load_default(size=15))
    draw.text((4, large.height - 24), 'x=1600 / diagnostic only', fill='white', font=ImageFont.load_default(size=15))
    return {'review/interface-1x.png': result, 'review/interface-2x.png': large,
            'review/engineering.png': annotated}


def run(build):
    """Write only this audit's deterministic records, or verify them without writes."""
    baseline = scope(build)
    inputs()
    records = {
        'outlet-visual-acceptance.json': {
            'owner_statement': 'proceed', 'date': '2026-10-03', 'review_path': VIEW,
            'review_sha256': VIEW_SHA, 'review_commit': 'a748b1126a70ea60b8030e90aa85b82b4bea237b',
            'context': 'response after the exact engineering view and request for owner visual assessment',
            'scope': 'bounded visual acceptance of the shown external outlet engineering view only',
            'layout_runtime_physical_promotion_authorized': False,
        },
        'evidence.json': {
            'status': 'read-only source audit; no new art or visual seam acceptance',
            'baseline_commit': BASE_COMMIT, 'protected_file_count': len(baseline),
            'source_sha256': {name: sha(ROOT / name) for name in SOURCES},
            'interface': measurements(),
        },
    }
    payloads = {}
    for name, image in diagnostic().items():
        stream = io.BytesIO()
        image.save(stream, format='PNG')
        payloads[name] = stream.getvalue()
    records['evidence.json']['diagnostic'] = {
        'world_origin': [1536, 400], 'size': [128, 192],
        'left': 'exact frozen B1 joined crop',
        'right': 'retained scene west water/bank recipe; no caps/deck in this crop',
        'limitation': 'external pixel reconstruction, not a Godot capture or approved correction',
        'artifacts_sha256': {name: hashlib.sha256(content).hexdigest() for name, content in payloads.items()},
    }
    for name, record in records.items():
        payloads[name] = (json.dumps(record, indent=2) + '\n').encode()
    for name, content in payloads.items():
        path = HERE / name
        if build:
            path.parent.mkdir(parents=True, exist_ok=True)
            path.write_bytes(content)
        else:
            require(path.read_bytes() == content, f'non-reproducible record: {name}')
    predecessor(baseline)
    print(f'PASS: join audit; {len(baseline)} preserved files; outlet acceptance bound; interface mismatch recorded')


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--build', action='store_true')
    run(parser.parse_args().build)
