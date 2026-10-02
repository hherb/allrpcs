#!/usr/bin/env python3
"""Build and verify a repository-derived external watercourse topology proposal."""

import argparse
import base64
import hashlib
import html
import json
import math
from pathlib import Path
import subprocess
import xml.etree.ElementTree as ET

from PIL import Image, ImageDraw, ImageFont

ROOT = Path(__file__).resolve().parents[3]
HERE = Path(__file__).resolve().parents[1]
ALLOWED = 'art-direction/western-watercourse-proposal/'


def sha(path):
    """Hash bytes without changing any input asset."""
    return hashlib.sha256(path.read_bytes()).hexdigest()


def preserved_paths():
    """Inventory tracked authorship, excluding only this proposal and the handover."""
    names = subprocess.check_output(['git', 'ls-files', '-z'], cwd=ROOT).decode().split('\0')
    return sorted(n for n in names if n and n != 'HANDOVER.md' and not n.startswith(ALLOWED)
                  and Path(n).name != '.DS_Store')


def distance(point, start, end):
    """Return Euclidean distance to a finite centreline segment."""
    dx, dy = end[0] - start[0], end[1] - start[1]
    progress = max(0, min(1, ((point[0] - start[0]) * dx + (point[1] - start[1]) * dy)
                            / (dx * dx + dy * dy)))
    return math.hypot(point[0] - start[0] - progress * dx, point[1] - start[1] - progress * dy)


def intersection(a, b, c, d):
    """Find a proper finite-segment intersection; parallel segments have none."""
    rx, ry, sx, sy = b[0] - a[0], b[1] - a[1], d[0] - c[0], d[1] - c[1]
    det = rx * sy - ry * sx
    if abs(det) < 1e-12:
        return None
    t = ((c[0] - a[0]) * sy - (c[1] - a[1]) * sx) / det
    u = ((c[0] - a[0]) * ry - (c[1] - a[1]) * rx) / det
    return [a[0] + t * rx, a[1] + t * ry] if 0 <= t <= 1 and 0 <= u <= 1 else None


def inside(point, polygon):
    """Test pixel centres against a polygon using the even-odd rule."""
    x, y = point
    result = False
    for a, b in zip(polygon, polygon[1:] + polygon[:1]):
        if (a[1] > y) != (b[1] > y):
            if x < a[0] + (y - a[1]) * (b[0] - a[0]) / (b[1] - a[1]):
                result = not result
    return result


def load_geometry():
    """Use the approved immutable layout as the route inventory."""
    layout = json.loads((ROOT / 'art-direction/world-layout-proposal/layout-proposal.json').read_text())
    anchors = {a['id']: (a['x'], a['y']) for a in layout['anchors']}
    routes = {r['id']: (anchors[r['first']], anchors[r['second']]) for r in layout['connections']}
    return layout, routes


def measure(config, routes):
    """Measure centreline crossings, alpha footprint coverage, and 8px route-cell exposure."""
    polygon = config['western_water_polygon']
    line = config['western_centerline']
    crossings = {}
    for name, (a, b) in routes.items():
        hits = [intersection(a, b, c, d) for c, d in zip(line, line[1:])]
        hits = [p for p in hits if p is not None]
        if hits:
            assert len(hits) == 1, f'ambiguous crossing: {name}'
            crossings[name] = hits[0]
    assert set(crossings) == {c['route'] for c in config['crossings']}, 'unaccounted crossing'
    bank_polygon = [[p[0], p[1] + (-config['planning_bank_allowance'] if i < 3
                                    else config['planning_bank_allowance'])]
                    for i, p in enumerate(polygon)]
    grid_points = [(4 + 8 * col, 4 + 8 * row)
                   for row in range(math.floor(min(p[1] for p in polygon) / 8),
                                    math.ceil(max(p[1] for p in polygon) / 8))
                   for col in range(math.floor(min(p[0] for p in polygon) / 8),
                                    math.ceil(max(p[0] for p in polygon) / 8))]
    water_cells = [point for point in grid_points if inside(point, polygon)]
    cell_exposure = {}
    route_cells = {}
    for name, (a, b) in routes.items():
        cells = [point for point in water_cells if distance(point, a, b) <= 48]
        if cells:
            cell_exposure[name] = len(cells)
            route_cells[name] = cells
    assert set(cell_exposure) == set(crossings), 'route-band contact without a named crossing'
    for a, b in routes.values():
        assert distance(config['source']['center'], a, b) > config['source']['radius'] + 48
    measurements = []
    for crossing in config['crossings']:
        a, b = routes[crossing['route']]
        center = crossings[crossing['route']]
        length = math.dist(a, b)
        ux, uy = (b[0] - a[0]) / length, (b[1] - a[1]) / length
        # Both crossings lie in the horizontal western band, away from the ramp.
        minimum_length = (96 + 2 * config['planning_bank_allowance']) / abs(uy)
        minimum_length += 2 * config['abutment_allowance']
        assert crossing['deck_length'] >= minimum_length, 'deck does not reach planned abutments'
        counts = {'water_alpha_pixels': 0, 'bank_envelope_alpha_pixels': 0, 'exposed_route_cells': 0}
        max_transverse = 0
        with Image.open(ROOT / crossing['texture']) as image:
            assert image.mode == 'RGBA'
            for y in range(image.height):
                for x in range(image.width):
                    if not image.getpixel((x, y))[3]:
                        continue
                    point = (x + crossing['origin'][0] + .5, y + crossing['origin'][1] + .5)
                    wet = inside(point, polygon)
                    bank = inside(point, bank_polygon)
                    counts['water_alpha_pixels'] += wet
                    counts['bank_envelope_alpha_pixels'] += bank
                    if bank:
                        dx, dy = point[0] - center[0], point[1] - center[1]
                        along, across = abs(dx * ux + dy * uy), abs(-dx * uy + dy * ux)
                        max_transverse = max(max_transverse, across)
                        assert along < crossing['deck_length'] / 2, 'deck clips accepted road fringe'
                        assert across < crossing['deck_width'] / 2, 'deck too narrow for visible road'
        counts['exposed_route_cells'] = cell_exposure[crossing['route']]
        assert counts['water_alpha_pixels'] > 0 and counts['exposed_route_cells'] > 0
        deck = [[center[0] + along * ux - across * uy, center[1] + along * uy + across * ux]
                for along, across in [(-crossing['deck_length'] / 2, -crossing['deck_width'] / 2),
                                      (crossing['deck_length'] / 2, -crossing['deck_width'] / 2),
                                      (crossing['deck_length'] / 2, crossing['deck_width'] / 2),
                                      (-crossing['deck_length'] / 2, crossing['deck_width'] / 2)]]
        for point in route_cells[crossing['route']]:
            assert inside(point, deck), 'bridge omits retained walkable cell'
        assert min(p[1] for p in deck) > 454, 'new deck touches Clearing south collision band'
        measurements.append({**crossing, 'center': center, 'deck_polygon': deck,
                             'minimum_deck_length': minimum_length,
                             'maximum_visible_half_width': max_transverse, **counts})
    return measurements


def diagram(config, layout, routes, measurements):
    """Create an original vector schematic and matching PNG; no source pixels are edited."""
    width, height = 1600, 1070
    scale, ox, oy = .45, 80, 120
    elements = []
    canvas = Image.new('RGB', (width, height), '#f5f1e8')
    draw = ImageDraw.Draw(canvas)

    def xy(point):
        return (ox + point[0] * scale, oy + point[1] * scale)

    def polygon(points, fill, stroke=None):
        transformed = [xy(p) for p in points]
        points_text = ' '.join(f'{x:.3f},{y:.3f}' for x, y in transformed)
        elements.append(f'<polygon points="{points_text}" fill="{fill}" stroke="{stroke or fill}"/>')
        draw.polygon(transformed, fill=fill, outline=stroke)

    def line(points, color, weight=2):
        transformed = [xy(p) for p in points]
        points_text = ' '.join(f'{x:.3f},{y:.3f}' for x, y in transformed)
        elements.append(f'<polyline points="{points_text}" fill="none" stroke="{color}" stroke-width="{weight}"/>')
        draw.line(transformed, fill=color, width=round(weight))

    def label(x, y, text, size=18, color='#263326'):
        elements.append(f'<text x="{x}" y="{y}" font-size="{size}" fill="{color}">{html.escape(text)}</text>')
        draw.text((x, y - size), text, font=ImageFont.load_default(size=size), fill=color)

    label(80, 45, 'Western watercourse: spring in the east, outlet in the west', 28)
    label(80, 78, 'EXTERNAL TOPOLOGY PROPOSAL - new water and B1/B2 bridges require named approval', 19)
    polygon([[0, 0], [3200, 0], [3200, 1800], [0, 1800]], '#e2e7ce', '#6b775b')
    for y in range(0, 1801, 300):
        line([[0, y], [3200, y]], '#c4ccad', 1)
        label(24, round(oy + y * scale + 6), str(y), 15)
    for x in range(0, 3201, 400):
        line([[x, 0], [x, 1800]], '#c4ccad', 1)
        label(round(ox + x * scale - 12), 107, str(x), 15)
    polygon([[600, 200], [1248, 200], [1248, 450], [600, 450]], '#bbcea0')
    polygon([[1420, 120], [1580, 120], [1580, 360], [1420, 360]], '#cdc2af')
    for a, b in routes.values():
        line([a, b], '#b49e7f', 96 * scale)
    # Original band is blue, new western channel and spring basin are cyan.
    polygon(config['western_water_polygon'], '#64bdc9', '#247b88')
    sx, sy = config['source']['center']
    radius = config['source']['radius']
    basin = [[sx + radius * math.cos(i * math.pi / 32), sy + radius * math.sin(i * math.pi / 32)]
             for i in range(64)]
    polygon(basin, '#64bdc9', '#247b88')
    left, top, right, bottom = config['retained_water']
    polygon([[left, top], [right, top], [right, bottom], [left, bottom]], '#438baf')
    polygon([[1736, 452], [1864, 452], [1864, 548], [1736, 548]], '#d4b894', '#675c46')
    for crossing in measurements:
        polygon(crossing['deck_polygon'], '#efc85d', '#7b6221')
        x, y = xy(crossing['center'])
        label(round(x - 18), round(y + 92), crossing['id'] + ' proposed', 18)
        line([crossing['center'], [crossing['center'][0], crossing['center'][1] + 155]], '#7b6221')
    for x, y in [(250, 600), (1000, 600), (1530, 535), (1930, 500)]:
        line([[x + 65, y], [x, y], [x + 22, y - 14]], '#174f67', 3)
    offsets = {
        'stream': (35, -45), 'abandoned-laundry': (-55, -42),
        'woodland-clearing': (-85, -38), 'old-shrine': (-100, -35),
        'village-edge': (-28, 52), 'village-square': (22, -32),
        'healer-garden': (20, 55), 'sickhouse': (20, -26),
        'southern-road': (-125, 65), 'abandoned-wagon': (25, 40),
        'closed-deeper-forest-path': (-120, -30),
    }
    for anchor in layout['anchors']:
        x, y = xy((anchor['x'], anchor['y']))
        draw.ellipse((x - 4, y - 4, x + 4, y + 4), fill='#374931')
        elements.append(f'<circle cx="{x}" cy="{y}" r="4" fill="#374931"/>')
        dx, dy = offsets[anchor['id']]
        label(round(x + dx), round(y + dy), anchor['label'], 17)
    label(1020, 285, 'Proposed spring (2032,500)', 18)
    label(85, 430, 'West outlet (0,600)', 18)
    label(85, 453, 'Water only; world boundary stays closed', 16)
    label(80, 980, 'Blue: retained stream / tan: retained routes and crossing / cyan: proposed water / gold: proposed bridges', 18)
    label(80, 1010, 'Planning diagram, not production art. Roads drawn at 96px; exact alpha and 8px physical-cell measurements are in evidence.json.', 17)
    label(80, 1040, 'The southward bend clears the Woodland Clearing. Closed Forest Path proof remains deferred.', 17)
    svg = (f'<svg xmlns="http://www.w3.org/2000/svg" width="{width}" height="{height}" viewBox="0 0 {width} {height}">'
           '<rect width="100%" height="100%" fill="#f5f1e8"/>'
           '<g font-family="Arial,sans-serif">' + ''.join(elements) + '</g></svg>\n')
    return svg, canvas


def crossing_details(config, routes, measurements):
    """Embed unchanged accepted PNG bytes beneath proposed deck outlines at 1:1 world scale."""
    parts = ['<svg xmlns="http://www.w3.org/2000/svg" width="1300" height="650" viewBox="0 0 1300 650">',
             '<rect width="1300" height="650" fill="#f5f1e8"/>',
             '<g font-family="Arial,sans-serif" fill="#263326">',
             '<text x="40" y="40" font-size="25">Crossing feasibility: unchanged accepted road pixels</text>',
             '<text x="40" y="75" font-size="17">Cyan: proposed water / gold: deck outline / purple: current 96px physical corridor</text>']
    points_text = lambda points: ' '.join(f'{x:.6f},{y:.6f}' for x, y in points)
    for index, crossing in enumerate(measurements):
        x = 40 + index * 640
        center = crossing['center']
        left = center[0] - 300
        parts.append(f'<text x="{x}" y="115" font-size="20">{crossing["id"]}: {html.escape(crossing["route"])}</text>')
        parts.append(f'<svg x="{x}" y="135" width="600" height="450" viewBox="{left} 400 600 450" overflow="hidden">')
        parts.append(f'<rect x="{left}" y="400" width="600" height="450" fill="#e2e7ce"/>')
        parts.append('<rect x="600" y="200" width="648" height="250" fill="#bbcea0" stroke="#566b42"/>')
        parts.append(f'<polygon points="{points_text(config["western_water_polygon"])}" fill="#64bdc9"/>')
        path = ROOT / crossing['texture']
        with Image.open(path) as image:
            width, height = image.size
        encoded = base64.b64encode(path.read_bytes()).decode()
        ax, ay = crossing['origin']
        parts.append(f'<image x="{ax}" y="{ay}" width="{width}" height="{height}" href="data:image/png;base64,{encoded}"/>')
        a, b = routes[crossing['route']]
        dx, dy = b[0] - a[0], b[1] - a[1]
        length = math.hypot(dx, dy)
        nx, ny = -dy / length * 48, dx / length * 48
        for sign in (-1, 1):
            edge = [[a[0] + sign * nx, a[1] + sign * ny], [b[0] + sign * nx, b[1] + sign * ny]]
            parts.append(f'<polyline points="{points_text(edge)}" fill="none" stroke="#795aa2" stroke-width="2" stroke-dasharray="8 4"/>')
        parts.append(f'<polygon points="{points_text(crossing["deck_polygon"])}" fill="none" stroke="#9c740a" stroke-width="4"/>')
        parts.append('</svg>')
        parts.append(f'<text x="{x}" y="618" font-size="17">{crossing["deck_width"]} x {crossing["deck_length"]} deck; {crossing["exposed_route_cells"]} current cells over new water</text>')
    return ''.join(parts) + '</g></svg>\n'


def main():
    """Build only proposal outputs, or verify deterministic output and preserved authorship."""
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--build', action='store_true')
    args = parser.parse_args()
    config = json.loads((HERE / 'proposal.json').read_text())
    layout, routes = load_geometry()
    baseline_path = HERE / 'baseline.json'
    if not baseline_path.exists():
        assert args.build, 'baseline missing'
        baseline_path.write_text(json.dumps({n: sha(ROOT / n) for n in preserved_paths()}, indent=2) + '\n')
    baseline = json.loads(baseline_path.read_text())
    assert set(baseline) == set(preserved_paths()), 'preserved tracked inventory changed'
    for name, expected in baseline.items():
        assert sha(ROOT / name) == expected, f'preserved authorship changed: {name}'
    # Reject untracked additions under runtime/source/test authority as well.
    untracked = subprocess.check_output(['git', 'ls-files', '--others', '--exclude-standard', '-z'], cwd=ROOT)
    for name in untracked.decode().split('\0'):
        assert not name.startswith(('game/', 'src/', 'tests/')), f'unreviewed runtime addition: {name}'
    measurements = measure(config, routes)
    svg, canvas = diagram(config, layout, routes, measurements)
    details = crossing_details(config, routes, measurements)
    evidence = json.dumps({'status': config['status'], 'baseline_commit': config['baseline_commit'],
                           'preserved_file_count': len(baseline), 'crossings': measurements,
                           'clearing_bottom': 450, 'western_horizontal_water_top': 552,
                           'clearing_water_gap': 102,
                           'rejected_straight_band_clearing_gap': 2,
                           'physical_owner': 'GreyboxNavigation: 96px route strips, 8px cell grid',
                           'stream_owner': 'OpeningRegionStreamLaundrySceneSource / StreamCollision',
                           'new_crossings_currently_have_no_water_collision': True}, indent=2) + '\n'
    if args.build:
        (HERE / 'overview.svg').write_text(svg)
        (HERE / 'crossings.svg').write_text(details)
        canvas.save(HERE / 'overview.png')
        (HERE / 'evidence.json').write_text(evidence)
    else:
        assert (HERE / 'overview.svg').read_text() == svg, 'SVG is not reproducible'
        ET.fromstring(svg)
        ET.fromstring(details)
        assert (HERE / 'crossings.svg').read_text() == details, 'crossing details changed'
        with Image.open(HERE / 'overview.png') as image:
            assert image.mode == 'RGB' and image.size == canvas.size
            assert image.tobytes() == canvas.tobytes(), 'PNG is not reproducible'
        assert (HERE / 'evidence.json').read_text() == evidence, 'measurements changed'
    print(f'PASS: external watercourse proposal; {len(baseline)} preserved files; two measured crossings')


if __name__ == '__main__':
    main()
