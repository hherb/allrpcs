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
    first_candidate = REPO / 'art-direction/seeker-cloak-gait-correction/candidate.json'
    for path, digest in json.loads(first_candidate.read_text())['bindings'].items():
        require(sha(REPO / path) == digest, 'First candidate evidence changed: ' + path)
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
            sign = 1 if direction == 'east' else -1
            centre = rig['pelvis_source_2x'][direction]
            pad = rig['side_top_padding_2x']
            for record in records:
                hip = [centre[0], centre[1] + pad + record['body_bob_2x']]
                require(all(leg['hip_2x'] == hip for leg in record['legs']), 'Roots do not share painted pelvic centre')
                require(abs(centre[0] - frames[0].width / 2) >= 19, 'Canvas centre substituted for anatomical pelvis')
                for index, leg in enumerate(record['legs']):
                    require(leg['anatomical_side'] == ('left' if index == 0 else 'right'), 'Anatomical side swapped')
                    h, k, a = [leg[key] for key in ('hip_2x', 'knee_2x', 'ankle_2x')]
                    thigh = (k[0] - h[0], k[1] - h[1])
                    shin = (a[0] - k[0], a[1] - k[1])
                    length = math.hypot(*thigh) * math.hypot(*shin)
                    cross = sign * (thigh[0] * shin[1] - thigh[1] * shin[0]) / length
                    dot = (thigh[0] * shin[0] + thigh[1] * shin[1]) / length
                    bend = math.degrees(math.atan2(cross, dot))
                    require(-1e-7 <= bend <= 90 and abs(bend - leg['knee_flexion_degrees']) < 1e-6,
                            'Knee bends forward or hyperextends')
                    require(abs(math.hypot(*thigh) - leg['thigh_length_2x']) < 1e-6
                            and abs(math.hypot(*shin) - leg['shin_length_2x']) < 1e-6, 'Bone lengths vary')
                    angle = math.degrees(math.atan2(sign * thigh[0], thigh[1]))
                    require(abs(angle - leg['thigh_angle_degrees']) < 1e-6, 'Painted thigh angle differs from hinge')
                    if leg['supporting']:
                        require(abs(leg['boot_box_2x'][3] - (312 + pad)) <= 1, 'Supporting sole leaves baseline')
            for index in range(2):
                thigh_angles = [record['legs'][index]['thigh_angle_degrees'] for record in records]
                bends = [record['legs'][index]['knee_flexion_degrees'] for record in records]
                require(min(thigh_angles) <= -24 and max(thigh_angles) >= 24, 'Thigh never scissors across the hip')
                require(min(bends) < 1e-6 and max(bends) >= 60, 'Knee never reaches straight and bent poses')
            for record_index, forward_index in ((0, 0), (4, 1)):
                legs = records[record_index]['legs']
                require(sign * (legs[forward_index]['knee_2x'][0] - legs[1-forward_index]['knee_2x'][0]) > 30,
                        'Thighs fail to exchange forward/backward position')
                require(legs[forward_index]['knee_flexion_degrees'] < 1e-6, 'Forward contact leg fails to straighten')
            summary.append({'direction': direction, 'anatomical_pelvis_source_2x': centre,
                            'thigh_scissoring': 'forward/backward exchanges around one root',
                            'signed_knee_hinge': 'zero/straight to backward flexion only; no hyperextension',
                            'bone_lengths_and_support_baseline': 'passed'})
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
    # Check the native pelvic bridge itself, beyond trusting joint coordinates.
    for direction in ('west', 'east'):
        from build import paint_parts, PELVIS
        body, _, _ = paint_parts(direction)
        sign = 1 if direction == 'east' else -1
        x, y = PELVIS[direction]
        require(body.getpixel((round(x + sign * 24), round(y + 9)))[3] > 200,
                'Missing paint still leaves the sharp front pelvic cut')
    report = {'status': 'passed', 'revised_sides': summary, 'accepted_front_back_frames': 16,
              'accepted_front_back_exports_and_masters': 'exact predecessor bytes',
              'predecessor_bindings': 'all unchanged', 'editable_replay': '32 exact native masters and exports',
              'pelvic_front_contour': 'retained source paint fills former cut edge',
              'authority': 'front/back accepted; revised sides await owner visual selection; Main unchanged'}
    (OUT / 'authoring-check.json').write_text(json.dumps(report, indent=2) + '\n')
    print('PASS: painted pelvis centre, forward/back thigh scissoring, straight/backward-only knees and filled front contour')
    print('PASS: fixed bone lengths and supporting sole baseline, 16 unchanged accepted views and 32 exact editable replays')
    print('PASS: all predecessor candidate bindings and original paint unchanged')


if __name__ == '__main__':
    main()
