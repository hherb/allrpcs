#!/usr/bin/env python3
"""Render a source-bound placement plan without authoring paint or runtime assets."""

import argparse
import hashlib
from io import BytesIO
import json
from pathlib import Path
import re

from PIL import Image, ImageDraw, ImageFilter, ImageFont

ROOT = Path(__file__).resolve().parent
REPO = ROOT.parents[1]
FONT = Path('/System/Library/Fonts/Supplemental/Arial.ttf')
TILING = REPO / 'art-direction/residual-ground-tiling-proof'
PRODUCTION = REPO / 'art-direction/production-proof/corrected'
ORIGIN = (1920, 620)
SIZE = (560, 560)
INK = '#25312e'
PAPER = '#eee9df'


def sha(path):
    """Return a persisted-file digest."""
    return hashlib.sha256(path.read_bytes()).hexdigest()


def require(condition, message):
    """Reject drift even when Python is optimized."""
    if not condition:
        raise ValueError(message)


def text(draw, xy, value, size=18, fill=INK):
    """Use the bound font for reproducible labels outside source swatches."""
    draw.text(xy, value, font=ImageFont.truetype(str(FONT), size), fill=fill)


def png(image):
    """Serialize reproducible review bytes."""
    buffer = BytesIO()
    image.save(buffer, format='PNG')
    return buffer.getvalue()


def generate():
    """Bind live geometry and approved pixels, then draw two planning boards."""
    relative_sources = [
        'game/Main.tscn', 'game/GreyboxNavigation.cs', 'game/OpeningRegionLayoutGeometry.cs',
        'game/WholeChannelPresentation.cs',
        'art-direction/residual-ground-tiling-proof/owner-acceptance.json',
        'art-direction/production-proof/corrected/asset-manifest.json',
        'art-direction/garden-ground-runtime-promotion/owner-runtime-acceptance.json',
        'art-direction/residual-ground-compatibility/interfaces.md',
    ]
    approval = json.loads((TILING / 'owner-acceptance.json').read_text())
    require(approval['status'] == 'approved', 'Tiling artwork is not approved')
    materials = ['packed-earth', 'pale-gravel', 'dry-grass']
    tiles = []
    for name in materials:
        path = TILING / f'exports/{name}.png'
        require(sha(path) == approval['approved_artwork'][f'exports/{name}.png'], 'Approved paint drift')
        relative_sources.append(str(path.relative_to(REPO)))
        with Image.open(path) as source:
            tile = source.convert('RGBA')
        require(tile.size == (128, 128), 'Unexpected field period')
        tiles.append(tile)
    manifest = json.loads((PRODUCTION / 'asset-manifest.json').read_text())
    junction = next(a for a in manifest['assets'] if a['id'] == 'terrain.village_square_junction')
    source = PRODUCTION / junction['export']
    require(sha(source) == junction['sha256'], 'Junction paint drift')
    relative_sources.append(str(source.relative_to(REPO)))
    runtime = REPO / 'game/art/runtime/terrain/village-square-junction.png'
    require(sha(runtime) == sha(source), 'Runtime junction differs from production')
    relative_sources.append(str(runtime.relative_to(REPO)))
    scene = (REPO / 'game/Main.tscn').read_text()
    resources = {key: path for path, key in re.findall(
        r'\[ext_resource type="Texture2D" path="([^"]+)" id="([^"]+)"\]', scene)}
    assignment = re.search(r'^VillageSquareJunctionTexture = ExtResource\("([^"]+)"\)', scene, re.MULTILINE)
    require(assignment is not None and resources[assignment.group(1)] ==
            'res://art/runtime/terrain/village-square-junction.png', 'Unexpected live junction assignment')
    owner = (REPO / 'game/WholeChannelPresentation.cs').read_text()
    require('SpriteCenter = new(1238, 522)' in owner and 'TextureWidth = 2604' in owner
            and 'TextureHeight = 620' in owner, 'Channel placement recipe drift')
    layout = (REPO / 'game/OpeningRegionLayoutGeometry.cs').read_text()
    anchors = {name: (int(x), int(y)) for name, x, y in re.findall(
        r'\["([^"]+)"\] = new\((\d+)\.0f, (\d+)\.0f\)', layout)}
    require(anchors['village-square'] == (2200, 900), 'Square anchor drift')
    connections = re.findall(r'\("([^"]+)", "([^"]+)"\)', layout)
    require(len(anchors) == 11 and len(connections) == 12, 'Layout topology drift')
    with Image.open(source) as image:
        require(image.size == (512, 512), 'Junction dimension drift')
        support = image.convert('RGBA').getchannel('A').point(lambda a: 255 if a else 0)
    silhouette = Image.new('L', SIZE)
    silhouette.paste(support, (24, 24))
    silhouette = silhouette.filter(ImageFilter.MaxFilter(49))
    channel_overlap = {}
    guarded = Image.new('L', (SIZE[0] + 4, SIZE[1] + 4))
    guarded.paste(silhouette, (2, 2))
    guarded = guarded.filter(ImageFilter.MaxFilter(5))
    for name in ('water', 'banks', 'decks', 'detail', 'fringe'):
        path = REPO / f'game/art/runtime/terrain/channel-{name}.png'
        relative_sources.append(str(path.relative_to(REPO)))
        with Image.open(path) as image:
            require(image.size == (2604, 620), 'Channel atlas geometry drift')
            channel = Image.new('L', guarded.size)
            channel.paste(image.convert('RGBA').getchannel('A'), (-1982, -406))
        overlap = sum(a > 0 and b > 0 for a, b in zip(guarded.tobytes(), channel.tobytes()))
        require(overlap == 0, 'Proposed support reaches accepted channel paint')
        channel_overlap[name] = overlap

    board = Image.new('RGB', (1120, 840), PAPER)
    draw = ImageDraw.Draw(board)
    text(draw, (28, 18), 'Proposed next ground use: Village Square / packed earth', 25)
    text(draw, (28, 55), 'Authored anchor map at 0.30x. Lines show connectivity, not route shape.', 17)
    text(draw, (28, 80), 'Orange box: maximum proof envelope. No terrain or layout is changed.', 17)
    scale, offset = 0.3, (64, 138)

    def point(world):
        return tuple(round(offset[i] + world[i] * scale) for i in range(2))

    draw.rectangle((*point((0, 0)), *point((3200, 1800))), fill='#d9e1d1', outline=INK, width=2)
    for first, second in connections:
        draw.line((point(anchors[first]), point(anchors[second])), fill='#89957e', width=3)
    draw.rectangle((*point(ORIGIN), *point((2480, 1180))), outline='#b45f24', width=4)
    # Exact accepted Garden paint envelope is separate from the proposed square patch.
    draw.rectangle((*point((2164, 1243)), *point((2416, 1387))), outline='#387852', width=3)
    for name, anchor in anchors.items():
        x, y = point(anchor)
        draw.ellipse((x - 4, y - 4, x + 4, y + 4), fill=INK)
        dy = -25 if name in ('old-shrine', 'village-edge', 'healer-garden', 'sickhouse') else 8
        text(draw, (x + 7, y + dy), name, 13)
    text(draw, (64, 710), 'Square anchor (2200,900) | envelope (1920,620)-(2480,1180), right/bottom exclusive', 18)
    text(draw, (64, 743), 'Green box: accepted Garden apron. Minimum vertical envelope separation: 63px.', 17)
    text(draw, (64, 778), 'Stream, Clearing, banks, closed forest and Garden remain outside the proposed patch.', 17)

    detail = Image.new('RGB', (1130, 810), PAPER)
    draw = ImageDraw.Draw(detail)
    text(draw, (24, 16), 'Geometry and material choice / planning evidence', 24)
    text(draw, (24, 51), 'Left: native 560px silhouette diagram. Right: unchanged 128px tiles repeated 2x2.', 16)
    footprint = Image.new('RGB', SIZE, '#d8dfd1')
    footprint.paste('#dba675', (0, 0), silhouette)
    outline = ImageDraw.Draw(footprint)
    outline.rectangle((24, 24, 535, 535), outline='#48524c', width=1)
    outline.line((270, 280, 290, 280), fill=INK, width=2)
    outline.line((280, 270, 280, 290), fill=INK, width=2)
    detail.paste(footprint, (24, 88))
    for index, (name, tile) in enumerate(zip(materials, tiles)):
        x = 610 + (index % 2) * 260
        y = 112 + (index // 2) * 294
        label = name + (' / recommended' if index == 0 else ' / alternative')
        text(draw, (x, y - 24), label, 15)
        for row in range(2):
            for col in range(2):
                detail.paste(tile.convert('RGB'), (x + col * 128, y + row * 128))
    text(draw, (24, 673), 'Orange is a schematic support mask, not packed-earth paint or a visual feasibility result.', 17)
    text(draw, (24, 704), 'Mask proposal: junction alpha > 0, expanded 24px in a square neighbourhood; inward feather 16px.', 17)
    text(draw, (24, 735), 'Future proof: world phase (0,0), 128px period, z=-90; quiet moss stays at z=-100.', 17)
    text(draw, (24, 766), 'Retain route paint above the patch. No new connector, dressing or runtime owner is selected.', 17)
    outputs = {'review/location-plan.png': png(board), 'review/geometry-materials.png': png(detail)}
    inputs = {name: sha(REPO / name) for name in sorted(relative_sources)}
    inputs[str(FONT)] = sha(FONT)
    record = {
        'status': 'external placement proposal; owner selection pending',
        'baseline_head': '7942e7da578fc459345576e1d267a58f949382eb',
        'date': '2026-10-05', 'timezone': 'Australia/Sydney',
        'recommended_material': 'packed-earth', 'anchor': [2200, 900],
        'maximum_envelope': [1920, 620, 2480, 1180],
        'mask_recipe': 'junction alpha > 0; square-neighbourhood dilation radius 24; inward smoothstep feather 16',
        'world_phase': [0, 0], 'period': 128, 'proposed_depth': -90,
        'channel_overlap_pixels_with_two_pixel_support_guard': channel_overlap,
        'sources': inputs,
        'outputs': {name: hashlib.sha256(data).hexdigest() for name, data in outputs.items()},
        'limitations': 'Map is topology only; mask is schematic; tiles are separate native swatches; no composed/GPU preview.',
    }
    outputs['proposal.json'] = (json.dumps(record, indent=2) + '\n').encode()
    return outputs


def main():
    """Reproduce boards or check all pinned sources and output bytes without writing."""
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--check', action='store_true')
    args = parser.parse_args()
    outputs = generate()
    saved = ROOT / 'proposal.json'
    if saved.exists():
        require(json.loads(saved.read_text())['sources'] == json.loads(outputs['proposal.json'])['sources'],
                'Proposal input drift; prepare a separately reviewed successor')
    for name, data in outputs.items():
        path = ROOT / name
        if args.check:
            require(path.exists() and path.read_bytes() == data, f'Proposal output drift: {name}')
        else:
            path.parent.mkdir(parents=True, exist_ok=True)
            path.write_bytes(data)
    print('PASS: bound proposal inputs and two reproducible planning boards')


if __name__ == '__main__':
    main()
