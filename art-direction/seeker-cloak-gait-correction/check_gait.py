#!/usr/bin/env python3
"""Inspect real cloak leg crossings and native editable paint, not just frame counts."""

from io import BytesIO
import json
from xml.etree import ElementTree as ET
import zipfile

from PIL import Image

from build import DIRECTIONS, GAME, OUT, REPO, bounds, render, require, sha


def main():
    """Replay all four gaits and independently check contact, passing and master pixels."""
    rig = json.loads((OUT / 'rig.json').read_text())
    for path, digest in rig['source_masters'].items():
        require(sha(REPO / path) == digest, 'Original cloak source paint changed')
    require(len(rig['assets']) == 32 and len(rig['rigs']) == 4, 'Incomplete corrected cloak set')
    require((OUT / 'rig.json').read_bytes() == (GAME / 'rig.json').read_bytes(), 'Game rig metadata drifted')
    summary = []
    for direction in DIRECTIONS:
        frames, records, _layers = render(direction)
        diagnostic, _, _ = render(direction, diagnostic=True)
        stored = next(row for row in rig['rigs'] if row['direction'] == direction)
        require(json.loads(json.dumps(records)) == stored['phases'], 'Stored leg metadata does not match actual renderer')
        require(len({frame.tobytes() for frame in frames}) == 8, 'Duplicated rendered pose')
        first, opposite = records[0]['legs'], records[4]['legs']
        passes = [records[2]['legs'], records[6]['legs']]
        for record in records:
            require([leg['anatomical_side'] for leg in record['legs']] == ['left', 'right'], 'Anatomical ownership switched')
            require(record['legs'][0]['hip_2x'] == first[0]['hip_2x']
                    and record['legs'][1]['hip_2x'] == first[1]['hip_2x'], 'Hip identity moved')
        if direction in ('west', 'east'):
            deltas = [legs[0]['shaft_2x'][0] - legs[1]['shaft_2x'][0] for legs in [first, opposite]]
            require(deltas[0] * deltas[1] < 0 and min(abs(delta) for delta in deltas) >= 60,
                    'Boots retain the same leading leg')
            for passing in passes:
                require(abs(passing[0]['shaft_2x'][0] - passing[1]['shaft_2x'][0]) <= 8,
                        'Boots never reach a passing position')
            require(stored['phases'][2]['legs'][0]['swing_lift_2x'] == 0
                    and stored['phases'][2]['legs'][1]['swing_lift_2x'] == 12
                    and stored['phases'][6]['legs'][0]['swing_lift_2x'] == 12
                    and stored['phases'][6]['legs'][1]['swing_lift_2x'] == 0, 'Swing foot does not alternate')
        else:
            deltas = [legs[0]['boot_box_2x'][3] - legs[1]['boot_box_2x'][3] for legs in [first, opposite]]
            require(deltas[0] * deltas[1] < 0 and min(abs(delta) for delta in deltas) >= 16,
                    'Front/rear leading contact does not alternate')
            require(passes[0][0]['swing_lift_2x'] == 0 and passes[0][1]['swing_lift_2x'] == 8
                    and passes[1][0]['swing_lift_2x'] == 8 and passes[1][1]['swing_lift_2x'] == 0,
                    'Front/rear swing foot does not alternate')
        for index, image in enumerate(frames, 1):
            identity = f'seeker-cloak-{direction}-walk-{index:02d}'
            asset = next(row for row in rig['assets'] if row['id'] == identity)
            exported = REPO / asset['export']
            master = REPO / asset['master']
            require(sha(exported) == asset['sha256'] and sha(master) == asset['master_sha256'], 'Candidate file changed')
            require((REPO / asset['texture']).read_bytes() == exported.read_bytes(), 'Candidate import PNG changed')
            expected = image.resize((image.width // 2, image.height // 2), Image.Resampling.LANCZOS)
            require(Image.open(exported).convert('RGBA').tobytes() == expected.tobytes(), 'Native export differs from replay')
            with zipfile.ZipFile(master) as archive:
                require(archive.namelist()[0] == 'mimetype', 'Invalid OpenRaster MIME layout')
                root = ET.fromstring(archive.read('stack.xml'))
                paint = Image.new('RGBA', image.size)
                names = {layer.get('name') for layer in root.iter('layer')}
                require(names == {'corrections', 'torso and cloak', 'anatomical left A', 'anatomical right B'},
                        'Semantic editable anatomy layers missing')
                for layer in reversed(list(root.iter('layer'))):
                    paint.alpha_composite(Image.open(BytesIO(archive.read(layer.get('src')))).convert('RGBA'))
                require(paint.tobytes() == image.tobytes(), 'Editable layers do not composite to actual renderer')
                require(Image.open(BytesIO(archive.read('mergedimage.png'))).convert('RGBA').tobytes() == image.tobytes(),
                        'Stored merged paint does not match actual gait')
            alpha_bounds = Image.open(exported).getchannel('A').getbbox()
            require(alpha_bounds[0] > 0 and alpha_bounds[1] > 0
                    and alpha_bounds[2] < expected.width and alpha_bounds[3] < expected.height,
                    'Candidate touches canvas perimeter')
        summary.append({'direction': direction, 'opposite_contact_deltas_2x': deltas,
                        'passing_phases': [90, 270], 'unique_rendered_poses': 8,
                        'semantic_masters': 'exact layer composition and decoded replay'})
    report = {'status': 'passed', 'checks': summary,
              'scope': 'Authoring/lineage/gait checks only; owner selection and Main integration not inferred'}
    (OUT / 'authoring-check.json').write_text(json.dumps(report, indent=2) + '\n')
    print('PASS: boots exchange lead, two passing poses, alternating swing/contact and fixed anatomy in all four directions')
    print('PASS: 32 replayed native exports, semantic editable masters and unchanged original source paint')


if __name__ == '__main__':
    main()
