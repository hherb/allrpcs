#!/usr/bin/env python3
"""Check the rejected side-view defect, accepted pixels and editable revision replay."""

from io import BytesIO
import json
import math
from xml.etree import ElementTree as ET
import zipfile

from PIL import Image

from build import DIRECTIONS, GAME, OUT, PREVIOUS, REPO, render, require, sha, boot_parts, paint_parts, side, bounds


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
    pelvis_candidate = REPO / 'art-direction/seeker-cloak-pelvis-hinge-revision/candidate.json'
    for path, digest in json.loads(pelvis_candidate.read_text())['bindings'].items():
        require(sha(REPO / path) == digest, 'Pelvis candidate evidence changed: ' + path)
    middle_candidate = REPO / 'art-direction/seeker-cloak-gait-side-revision/candidate.json'
    for path, digest in json.loads(middle_candidate.read_text())['bindings'].items():
        require(sha(REPO / path) == digest, 'Middle candidate evidence changed: ' + path)
    for path, digest in rig['source_masters'].items():
        require(sha(REPO / path) == digest, 'Original accepted paint changed')
    require((OUT / 'rig.json').read_bytes() == (GAME / 'rig.json').read_bytes(), 'Import rig drift')
    summary = []
    for direction in DIRECTIONS:
        frames, records, _ = render(direction)
        diagnostics, _, _ = render(direction, diagnostic=True)
        stored = next(row for row in rig['rigs'] if row['direction'] == direction)
        require(records == stored['phases'], 'Stored articulation does not match rendered geometry')
        require(len({frame.tobytes() for frame in frames}) == (16 if direction in ('west','east') else 8), 'Duplicated revision pose')
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
                    require(-1e-7 <= bend <= 100 and abs(bend - leg['knee_flexion_degrees']) < 1e-6,
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
                require(min(thigh_angles) <= -20 and max(thigh_angles) >= 30, 'Thigh never scissors across the hip')
                require(min(bends) < 1e-4 and max(bends) >= 50, 'Knee never reaches straight and bent poses')
            transfers = []
            for contact_index, leader in ((0,0),(8,1)):
                legs = records[contact_index]['legs']
                require(legs[leader]['knee_flexion_degrees'] < 18, 'Contact knee fails to extend')
                require(sign*(legs[leader]['foot_centroid_2x'][0]-legs[1-leader]['foot_centroid_2x'][0]) > 65,
                        'Rendered feet do not exchange leading and trailing roles')
                for leg_index, leg in enumerate(legs):
                    left, top, right, bottom = leg['boot_box_2x']
                    color = [(209,61,56),(40,121,205)][leg_index]
                    pixels = [(x,y) for y in range(top,bottom) for x in range(left,right)
                              if diagnostics[contact_index].getpixel((x,y))[:3] == color
                              and diagnostics[contact_index].getpixel((x,y))[3] > 220]
                    require(len(pixels) >= 30, 'Actual visible foot loses anatomical identity')
            body, native_boots, _ = paint_parts(direction)
            rolls = []
            for pose, leg_index, expected in ((0,0,-1),(8,1,-1),(1,1,1),(9,0,1)):
                leg = records[pose]['legs'][leg_index]
                foot = boot_parts(native_boots[leg_index], direction)[1]
                ankle = leg['ankle_2x']; angle = math.radians(leg['foot_rotation_degrees'])
                source = side.BOOT_ANCHORS[direction][1]
                paint,_ = side.rotate_boot(frames[pose].size, foot, source, (source[0]+1,source[1]),
                                            ankle,(ankle[0]+math.cos(angle),ankle[1]+math.sin(angle)))
                box = bounds(paint); y = box[3]-1
                xs = [x for x in range(box[0],box[2]) if paint.getpixel((x,y))[3]>96]
                contact_offset = sign*(sum(xs)/len(xs)-leg['foot_centroid_2x'][0])
                require(expected*contact_offset > 8, 'Actual foot paint fails heel contact / trailing toe roll')
                rolls.append({'pose':pose+1,'leg':leg['leg'],'contact':'heel' if expected<0 else 'toe',
                              'lowest_paint_offset_from_centroid_2x':contact_offset})
            double_support = [i for i,r in enumerate(records) if sum(l['supporting'] for l in r['legs'])==2]
            require(double_support == [0,1,8,9], 'Missing brief bilateral double support')
            for index in (0,1):
                for r in records:
                    l=r['legs'][index]
                    if not l['supporting']:
                        require(l['boot_box_2x'][3] < 336 and l['swing_lift_2x'] > 0,
                                'Swing foot drags on ground')
            for start, advancing in ((0,1),(8,0)):
                changes = []
                for offset in range(8):
                    legs = records[start+offset]['legs']; a,b=legs[advancing],legs[1-advancing]
                    changes.append((sign*(a['knee_2x'][0]-b['knee_2x'][0]),
                                    sign*(a['foot_centroid_2x'][0]-b['foot_centroid_2x'][0])))
                knee_cross = next(i for i,(k,f) in enumerate(changes) if k > 4)
                foot_cross = next(i for i,(k,f) in enumerate(changes) if f > 4)
                require(knee_cross < foot_cross, 'Foot passes before the advancing bent knee')
                require(changes[-1][1] > 50, 'Swing foot never becomes clearly leading')
                transfers.append({'advancing_leg':'AB'[advancing], 'knee_cross_pose':start+knee_cross+1,
                                  'foot_cross_pose':start+foot_cross+1,
                                  'sequence':'bent knee passes first; foot follows with knee extension'})
            prior = json.loads((PREVIOUS/'rig.json').read_text())
            prior_phases=next(row for row in prior['rigs'] if row['direction']==direction)['phases']
            require(not any(sum(l['supporting'] for l in r['legs'])==2 for r in prior_phases),
                    'Retained prior single-support defect probe changed')
            summary.append({'direction':direction,'double_support_pose_indices':[i+1 for i in double_support],
                            'knee_then_foot_transfers':transfers,'painted_heel_and_toe_contact':rolls,'actual_visible_contact_feet':'persistent A/B identity',
                            'pelvis_and_one_way_knee_hinges':'passed','grounded_support_and_clear_swing':'passed',
                            'predecessor_missing_double_support_rejection':'passed'})
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
        from build import PELVIS
        body, _, _ = paint_parts(direction)
        sign = 1 if direction == 'east' else -1
        x, y = PELVIS[direction]
        require(body.getpixel((round(x + sign * 24), round(y + 9)))[3] > 200,
                'Missing paint still leaves the sharp front pelvic cut')
    report = {'status': 'passed', 'revised_sides': summary, 'accepted_front_back_frames': 16,
              'accepted_front_back_exports_and_masters': 'exact predecessor bytes',
              'predecessor_bindings': 'all unchanged', 'editable_replay': '48 exact native masters and exports',
              'pelvic_front_contour': 'retained source paint fills former cut edge',
              'authority': 'front/back accepted; revised sides await owner visual selection; Main unchanged'}
    (OUT / 'authoring-check.json').write_text(json.dumps(report, indent=2) + '\n')
    print('PASS: natural support/swing, brief double support, knee-first then rendered-foot exchanges')
    print('PASS: fixed bone lengths and supporting sole baseline, 16 unchanged accepted views and 48 exact editable replays')
    print('PASS: all predecessor candidate bindings and original paint unchanged')


if __name__ == '__main__':
    main()
