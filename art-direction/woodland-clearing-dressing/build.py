#!/usr/bin/env python3
"""Bind approved paint and external placements; author a complete clearance study."""

import argparse
import hashlib
from io import BytesIO
import json
from pathlib import Path

import numpy as np
from PIL import Image, ImageDraw, ImageFilter, ImageFont

ROOT = Path(__file__).resolve().parent
REPO = ROOT.parents[1]
ART = REPO / 'art-direction/woodland-environment-batch'
PREVIOUS = REPO / 'art-direction/clearing-village-edge-ground-feasibility'
FONT = Path('/System/Library/Fonts/Supplemental/Arial.ttf')
ORIGIN = (524, 0)
SIZE = (800, 480)
VARIANTS = ('baseline', 'sparse', 'grouped')
CONDITIONS = ('normal', 'protanomaly', 'deuteranomaly', 'tritanomaly', 'luminance')


def require(value, message):
    if not value:
        raise ValueError(message)


def sha(path):
    return hashlib.sha256(path.read_bytes()).hexdigest()


def read(path):
    return json.loads(path.read_text())


def raster(path):
    with Image.open(path) as image:
        return image.convert('RGBA')


def png(image):
    output = BytesIO()
    image.save(output, format='PNG')
    return output.getvalue()


def label(draw, xy, text, size=17):
    draw.text(xy, text, font=ImageFont.truetype(str(FONT), size), fill='#25312e')


def write_or_check(outputs, check):
    """Preflight the entire output set before writing; evidence is never replaced."""
    for name, data in outputs.items():
        path = ROOT / name
        require(path.is_file() and path.read_bytes() == data if check else not path.exists(),
                ('Generated drift: ' if check else 'Refusing existing output: ') + name)
    if not check:
        for name, data in outputs.items():
            path = ROOT / name
            path.parent.mkdir(parents=True, exist_ok=True)
            with path.open('xb') as output:
                output.write(data)


def clearance():
    """Guard complete 96px capsules, opening approaches, labels, specimens and channel."""
    layout = read(REPO / 'art-direction/world-layout-proposal/layout-proposal.json')
    require(layout['world']['path_width'] == 96, 'Route width changed')
    anchors = {a['id']: np.array([a['x'], a['y']]) for a in layout['anchors']}
    yy, xx = np.indices((SIZE[1], SIZE[0]))
    xx = xx + ORIGIN[0]
    points = np.stack((xx + .5, yy + .5), -1)
    route = np.zeros(xx.shape, dtype=bool)
    for connection in layout['connections']:
        a, b = (anchors[connection[key]] for key in ('first', 'second'))
        delta = b - a
        t = np.clip(((points - a) * delta).sum(-1) / (delta * delta).sum(), 0, 1)
        distance = np.linalg.norm(points - a - t[:, :, None] * delta, axis=-1)
        # Four pixels beyond each retained 48px half-corridor protect linear edges.
        route |= distance <= 52
    for x, y in ((600, 318), (1248, 285), (1128, 450), (744, 200), (1050, 350)):
        route |= (xx + .5 - x) ** 2 + (yy + .5 - y) ** 2 <= 60 ** 2
    # Conservative envelopes include focus labels, testimony labels and gate/Forester.
    rectangles = [(610, 210, 805, 245), (860, 270, 990, 480), (1010, 268, 1150, 418),
                  (1180, 398, 1324, 480), (524, 0, 832, 145)]
    for left, top, right, bottom in rectangles:
        route |= (xx >= left) & (xx < right) & (yy >= top) & (yy < bottom)
    owners = read(PREVIOUS / 'capture-evidence.json')['owners']
    for name, owner in owners.items():
        if name not in ('GreyboxNavigation/RoutePresentation/WoodlandShrinePath',
                        'GreyboxNavigation/RoutePresentation/WoodlandVillageEdgePath',
                        'GreyboxNavigation/RoutePresentation/ClosedForestPath') \
                and not name.startswith('WholeChannelPresentation/'):
            continue
        require(owner['world_transform'][:4] == [1, 0, 0, 1] and owner['offset'] == [0, 0],
                'Non-native guarded sprite: ' + name)
        image = raster(REPO / 'game' / owner['texture'].removeprefix('res://'))
        require(list(image.size) == owner['size'], 'Guarded texture dimensions changed')
        x, y = owner['world_transform'][4:]
        if owner['centered']:
            x -= image.width / 2
            y -= image.height / 2
        require(x == int(x) and y == int(y), 'Fractional guarded sprite origin')
        mask = Image.new('L', SIZE)
        mask.paste(image.getchannel('A'), (int(x) - ORIGIN[0], int(y)))
        guarded = mask.point(lambda a: 255 if a else 0).filter(ImageFilter.MaxFilter(9))
        route |= np.asarray(guarded) > 0
    return route


def placement_rows(spec):
    """Resolve native origins using the accepted provisional pivots unchanged."""
    require(tuple(spec['variants']) == VARIANTS and spec['variants']['baseline'] == [], 'Variant drift')
    require(spec['z_index'] == -60 and spec['scale'] == [1, 1] and spec['rotation'] == 0
            and spec['filter'] == 'linear' and spec['repeat'] == 'disabled', 'Presentation policy drift')
    inventory = read(ART / 'inventory.json')
    assets = {asset['id']: asset for asset in inventory['assets']}
    approved = read(ART / 'owner-acceptance.json')['approved_files']
    result = {}
    for variant in VARIANTS:
        rows = []
        for placement in spec['variants'][variant]:
            require(placement['asset'] in assets and not placement['asset'].startswith('trees-'),
                    'Unsupported approved dressing asset')
            asset = assets[placement['asset']]
            pivot = asset['proposed_pivot']
            require(approved[asset['export']] == asset['sha256'] == sha(ART / asset['export']),
                    'Approved source paint changed')
            require(sha(ART / asset['master']) == approved[asset['master']], 'Approved master changed')
            world = placement['world_pivot']
            require(len(world) == 2 and all(type(value) is int for value in world), 'Non-native pivot')
            origin = [world[i] - pivot[i] for i in range(2)]
            rows.append({**placement, 'origin': origin, 'size': asset['size'], 'proposed_pivot': pivot,
                         'export': str((ART / asset['export']).relative_to(REPO)),
                         'sha256': asset['sha256'], 'master': asset['master']})
        require(len({row['instance'] for row in rows}) == len(rows), 'Duplicate placement instance')
        result[variant] = rows
    return result


def check_placements(rows, forbidden):
    """Require each full native rectangle plus a 2px filtering margin to be clear."""
    for variant, placements in rows.items():
        for row in placements:
            x, y = row['origin']
            w, h = row['size']
            require(ORIGIN[0] + 2 <= x and 2 <= y and x + w + 2 <= ORIGIN[0] + SIZE[0]
                    and y + h + 2 <= SIZE[1], 'Placement outside complete envelope')
            require(not forbidden[y - 2:y + h + 2, x - ORIGIN[0] - 2:x - ORIGIN[0] + w + 2].any(),
                    'Placement violates retained clearance: ' + variant + '/' + row['instance'])
            require(raster(REPO / row['export']).size == (w, h), 'Placement resized source paint')


def generate():
    spec = read(ROOT / 'placements.json')
    rows = placement_rows(spec)
    forbidden = clearance()
    check_placements(rows, forbidden)
    sources = ['placements.json', 'baseline.json', 'build.py', 'capture.gd']
    sources = {str((ROOT / name).relative_to(REPO)): sha(ROOT / name) for name in sources}
    for path in [ART / 'owner-acceptance.json', ART / 'inventory.json',
                 PREVIOUS / 'capture-evidence.json', PREVIOUS / 'simulation-model.json',
                 REPO / 'art-direction/world-layout-proposal/layout-proposal.json',
                 REPO / 'game/Main.tscn', REPO / 'game/OpeningRegionWoodlandClearingGeometry.cs',
                 REPO / 'game/OpeningRegionLayoutGeometry.cs']:
        sources[str(path.relative_to(REPO))] = sha(path)
    for relative in read(ART / 'owner-acceptance.json')['approved_files']:
        sources[str((ART / relative).relative_to(REPO))] = sha(ART / relative)
    outputs = {'proof-inputs/forbidden.png': png(Image.fromarray(forbidden.astype('uint8') * 255))}
    for variant in VARIANTS:
        # An annotated diagram, separate from the actual-scene comparison images.
        board = Image.new('RGB', SIZE, '#e8e4da')
        board.paste(Image.fromarray(np.where(forbidden, 135, 232).astype('uint8')).convert('RGB'))
        draw = ImageDraw.Draw(board)
        for row in rows[variant]:
            x, y = row['origin']; w, h = row['size']
            draw.rectangle((x - ORIGIN[0], y, x - ORIGIN[0] + w - 1, y + h - 1), outline='#1c693c', width=2)
            label(draw, (x - ORIGIN[0] + 2, y + 2), row['instance'].split('-')[0], 14)
        outputs[f'review/{variant}-clearance.png'] = png(board)
    record = {'status': 'external candidates; approved artwork retained; placement unselected',
              'published_head': read(ROOT / 'baseline.json')['head'], 'origin': ORIGIN, 'size': SIZE,
              'sources': sources, 'variants': rows, 'presentation': {k: spec[k] for k in
                  ('z_index', 'scale', 'rotation', 'filter', 'repeat', 'pivot_policy')},
              'clearance': 'Full native rectangles plus 2px margin; all 96px route capsules plus 4px, 60px opening/specimen guard, existing route/channel alpha plus 4px, conservative labels/interactions/gate/Forester envelopes',
              'outputs': {name: hashlib.sha256(data).hexdigest() for name, data in outputs.items()}}
    outputs['inventory.json'] = (json.dumps(record, indent=2) + '\n').encode()
    return outputs


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--check', action='store_true')
    args = parser.parse_args()
    write_or_check(generate(), args.check)
    print('PASS: approved paint/pivots retained; complete external placement/clearance inputs')


if __name__ == '__main__':
    main()
