#!/usr/bin/env python3
"""Check the rejected side-view defect, accepted pixels and editable revision replay."""

from io import BytesIO
import json
import math
from xml.etree import ElementTree as ET
import zipfile

from PIL import Image

from build import DIRECTIONS, GAME, OUT, PREVIOUS, REPO, render, require, sha


def main():
    """Verify attachment, bone lengths, knee excursion, contact and unchanged accepted views."""
    rig = json.loads((OUT / 'rig.json').read_text())
    predecessor = json.loads((PREVIOUS / 'candidate.json').read_text())
    require(sha(PREVIOUS / 'candidate.json') == rig['predecessor_candidate_sha256'], 'Predecessor candidate changed')
    for path, digest in predecessor['bindings'].items():
        require(sha(REPO / path) == digest, 'Predecessor evidence changed: ' + path)
    for path, digest in rig['source_masters'].items():
        require(sha(REPO / path) == digest, 'Original accepted paint changed')
    require((OUT / 'rig.json').read_bytes() == (GAME / 'rig.json').read_bytes(), 'Import rig drift')
    summary = []
    for direction in DIRECTIONS:
        frames, records, _ = render(direction)
        stored = next(row for row in rig['rigs'] if row['direction'] == direction)
        require(records == stored['phases'], 'Stored articulation does not match rendered geometry')
        require(len({frame.tobytes() for frame in frames}) == 8, 'Duplicated revision pose')
        if direction in ('west', 'east'):
            for record in records:
                hips = [leg['hip_2x'] for leg in record['legs']]
                require(hips[0] == hips[1] == [frames[0].width / 2, 204 + record['body_bob_2x']],
                        'Legs do not attach to the central pelvis with the torso')
                for index, leg in enumerate(record['legs']):
                    require(leg['anatomical_side'] == ('left' if index == 0 else 'right'), 'Anatomical side swapped')
                    hip, knee, ankle = [leg[key] for key in ('hip_2x', 'knee_2x', 'ankle_2x')]
                    require(abs(math.dist(hip, knee) - leg['thigh_length_2x']) < 1e-6
                            and abs(math.dist(knee, ankle) - leg['shin_length_2x']) < 1e-6,
                            'Thigh/shin stretches instead of joint articulation')
                    if leg['stance']:
                        require(abs(leg['boot_box_2x'][3] - 312) <= 1, 'Stance sole loses ground baseline')
                    require(0 < leg['knee_flexion_degrees'] < 120, 'Knee locks or folds through itself')
            first, opposite = [records[i]['legs'] for i in (0, 4)]
            separations = [legs[0]['ankle_2x'][0] - legs[1]['ankle_2x'][0] for legs in (first, opposite)]
            require(separations[0] * separations[1] < 0 and min(abs(v) for v in separations) >= 60,
                    'The same leg still leads both contacts')
            for passing, swing_index in ((records[2], 1), (records[6], 0)):
                stance, swing = passing['legs'][1 - swing_index], passing['legs'][swing_index]
                require(abs(stance['ankle_2x'][0] - swing['ankle_2x'][0]) < 1,
                        'Ankles never pass under the pelvis')
                require(swing['knee_flexion_degrees'] > stance['knee_flexion_degrees'] + 35
                        and swing['ankle_2x'][1] < stance['ankle_2x'][1] - 15,
                        'Passing foot rises without a flexing swing knee')
            for index in range(2):
                angles = [record['legs'][index]['boot_rotation_degrees'] for record in records]
                flexion = [record['legs'][index]['knee_flexion_degrees'] for record in records]
                require(max(angles) - min(angles) > 35 and max(flexion) - min(flexion) > 40,
                        'Inherited constant backward shaft orientation remains')
            summary.append({'direction': direction, 'ankle_contact_separation_2x': separations,
                            'central_pelvis': 'both legs attached; follows torso bob',
                            'rigid_bone_lengths': 'passed', 'alternating_knee_flexion_extension': 'passed',
                            'actual_shin_rotation': 'passed', 'stance_sole_baseline': 'passed'})
        for index, image in enumerate(frames, 1):
            asset = next(row for row in rig['assets'] if row['direction'] == direction and row['index'] == index)
            export, master, texture = [REPO / asset[key] for key in ('export', 'master', 'texture')]
            require(sha(export) == asset['sha256'] and sha(master) == asset['master_sha256'], 'Candidate bytes drifted')
            require(export.read_bytes() == texture.read_bytes(), 'Runtime copy differs')
            expected = image.resize((image.width // 2, image.height // 2), Image.Resampling.LANCZOS)
            require(Image.open(export).convert('RGBA').tobytes() == expected.tobytes(), 'Export differs from replay')
            with zipfile.ZipFile(master) as archive:
                root = ET.fromstring(archive.read('stack.xml'))
                merged = Image.new('RGBA', image.size)
                for layer in reversed(list(root.iter('layer'))):
                    merged.alpha_composite(Image.open(BytesIO(archive.read(layer.get('src')))).convert('RGBA'))
                require(merged.tobytes() == image.tobytes(), 'Native layers do not compose to revision')
                require(Image.open(BytesIO(archive.read('mergedimage.png'))).convert('RGBA').tobytes() == image.tobytes(),
                        'Stored native merged paint differs')
            perimeter = Image.open(export).getchannel('A').getbbox()
            require(perimeter[0] > 0 and perimeter[1] > 0
                    and perimeter[2] < expected.width and perimeter[3] < expected.height, 'Canvas clipping')
            if direction in ('south', 'north'):
                for key, current in (('exports', export), ('masters', master)):
                    require(current.read_bytes() == (PREVIOUS / key / current.name).read_bytes(),
                            'Accepted front/back changed')
    report = {'status': 'passed', 'revised_sides': summary, 'accepted_front_back_frames': 16,
              'accepted_front_back_exports_and_masters': 'exact predecessor bytes',
              'predecessor_bindings': 'all unchanged', 'editable_replay': '32 exact native masters and exports',
              'authority': 'front/back accepted; revised sides await owner visual selection; Main unchanged'}
    (OUT / 'authoring-check.json').write_text(json.dumps(report, indent=2) + '\n')
    print('PASS: central pelvis, fixed thigh/shin lengths, alternating knee flexion/extension and actual shin rotation')
    print('PASS: planted stance soles, both passing poses, 16 unchanged accepted views and 32 exact editable replays')
    print('PASS: all predecessor candidate bindings and original paint unchanged')


if __name__ == '__main__':
    main()
