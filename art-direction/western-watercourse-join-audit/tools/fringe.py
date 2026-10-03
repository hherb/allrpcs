#!/usr/bin/env python3
"""Remove the owner-rejected blue grass/bank fringe in a separate external successor."""

import argparse
import io
import json
import tempfile

from PIL import Image

import audit
import join

REQUEST = 'correction/review-request.json'
REFERENCE = 'correction/review/joined-2x.png'
ORDER = ('rejected_reference', 'ground_under_bank_fringe')


def render():
    """Ground the translucent grass/brown transition rows; retain water-facing bank rows."""
    request = json.loads((audit.HERE / REQUEST).read_text())
    audit.require(request['correction_details'] ==
                  'there is a small blue (water) line between the brown bank and the green grass',
                  'owner correction request changed')
    audit.require(audit.sha(audit.HERE / REFERENCE) == request['review_sha256'], 'rejected image changed')
    with Image.open(audit.HERE / 'correction/review/joined-1x.png') as source:
        reference = source.convert('RGBA')
    with Image.open(audit.HERE / 'correction/exports/banks.png') as source:
        west_bank = source.convert('RGBA')
    textures = {}
    for name, path in (('ground', audit.SOURCES[9]), ('bank', audit.SOURCES[7]), ('water', audit.SOURCES[6])):
        with Image.open(audit.ROOT / path) as source:
            textures[name] = source.convert('RGBA')
    overlay = Image.new('RGBA', reference.size)
    footprint = set()
    for x in range(4, 128):
        wx = 1536 + x
        top, bottom, depth, amount = join.profile(wx + .5) if x < 64 else (484, 516, 32, 1)
        for y in range(256):
            wy = 384 + y + .5
            if top - depth <= wy < top:
                row = int((wy - top + depth) * 32 / depth)
            elif bottom <= wy < bottom + depth:
                row = int((bottom + depth - wy) * 32 / depth)
            else:
                continue
            if not 11 <= row <= 21:
                continue
            bank = west_bank.getpixel((x, y)) if x < 60 else textures['bank'].getpixel(((wx - 1540) % 128, row))
            ground = textures['ground'].getpixel((wx % textures['ground'].width, (384 + y) % textures['ground'].height))
            water = join.mix(textures['water'].getpixel((wx % 128, (384 + y) % 96)),
                             textures['water'].getpixel(((wx - 1600) % 128, (384 + y - 452) % 96)), amount)
            # The source's row 14 is transparent between grass and brown bank.
            # Ground beneath it removes the blue line; rows 15..21 smoothly return
            # to the existing water underlay, leaving rows 22..31 untouched.
            t = max(0, min(1, (row - 14) / 8))
            underlay = join.mix(ground, water, amount * t * t * (3 - 2 * t))
            target = join.compose([Image.new('RGBA', (1, 1), underlay), Image.new('RGBA', (1, 1), bank)]).getpixel((0, 0))
            if target != reference.getpixel((x, y)):
                overlay.putpixel((x, y), target)
                footprint.add((x, y))
    candidate = join.compose([reference, overlay])
    audit.require(footprint, 'no fringe correction made')
    for y in range(256):
        for x in range(128):
            changed = candidate.getpixel((x, y)) != reference.getpixel((x, y))
            audit.require(changed == ((x, y) in footprint), 'change escaped fringe footprint')
    # Exact transparent source row on the retained side now shows ground, not water.
    for x in range(64, 128):
        wx = 1536 + x
        for wy in (466, 533):
            raw = textures['bank'].getpixel(((wx - 1540) % 128, 14))
            if raw[3] == 0:
                expected = textures['ground'].getpixel((wx % textures['ground'].width, wy % textures['ground'].height))
                audit.require(candidate.getpixel((x, wy - 384)) == expected, 'blue fringe remains')
    return reference, overlay, candidate, footprint


def run(build):
    """Reproduce the frozen first candidate and bind the successor to the exact owner request."""
    baseline = audit.scope(False)
    join.run(False)
    reference, overlay, candidate, footprint = render()
    images = {'fringe-correction/exports/ground-under-bank-fringe.png': overlay,
              'fringe-correction/exports/joined.png': candidate,
              'fringe-correction/review/joined-1x.png': candidate,
              'fringe-correction/review/joined-2x.png': candidate.resize((256, 512), Image.Resampling.NEAREST)}
    payloads = {}
    for name, image in images.items():
        stream = io.BytesIO()
        image.save(stream, format='PNG')
        payloads[name] = stream.getvalue()
    with tempfile.TemporaryDirectory() as directory:
        path = audit.Path(directory) / 'join.ora'
        join.helpers().builder.create_ora(path, (256, 512), [
            (ORDER[0], reference.resize((256, 512), Image.Resampling.NEAREST), 0, 0, True),
            (ORDER[1], overlay.resize((256, 512), Image.Resampling.NEAREST), 0, 0, True)])
        payloads['fringe-correction/masters/join.ora'] = path.read_bytes()
    record = {
        'status': 'external blue-fringe successor; owner visual assessment pending',
        'origin': [1536, 384], 'size': [128, 256], 'master_scale': 2,
        'master_note': '2x nearest exact reference and source-derived correction layers; not newly painted detail',
        'request_sha256': audit.sha(audit.HERE / REQUEST), 'rejected_reference_sha256': audit.sha(audit.HERE / REFERENCE),
        'source_sha256': {name: audit.sha(audit.ROOT / name) for name in audit.SOURCES},
        'protected_file_count': len(baseline), 'changed_pixels': len(footprint),
        'changed_retained_reference_pixels': sum(x >= 64 for x, y in footprint),
        'change_limit': 'bank source rows 11..21 only; west first four columns, water interior and all other pixels preserved',
        'runtime_files_changed': 0, 'layout_runtime_physical_promotion_authorized': False,
        'draw_order': ORDER,
        'artifacts_sha256': {name: audit.hashlib.sha256(content).hexdigest() for name, content in payloads.items()},
    }
    payloads['fringe-correction/manifest.json'] = (json.dumps(record, indent=2) + '\n').encode()
    acceptance_path = audit.HERE / 'fringe-correction/visual-acceptance.json'
    if acceptance_path.exists():
        acceptance = json.loads(acceptance_path.read_text())
        audit.require(acceptance['owner_statement'] == 'Yes, visually acceptable as shown',
                      'owner acceptance statement changed')
        audit.require(acceptance['review_path'] ==
                      'art-direction/western-watercourse-join-audit/fringe-correction/review/joined-2x.png'
                      and acceptance['review_sha256'] ==
                      audit.hashlib.sha256(payloads['fringe-correction/review/joined-2x.png']).hexdigest(),
                      'accepted successor changed')
        audit.require(acceptance['layout_runtime_physical_promotion_authorized'] is False,
                      'visual acceptance grants no promotion')
    for name, content in payloads.items():
        path = audit.HERE / name
        if build:
            path.parent.mkdir(parents=True, exist_ok=True)
            path.write_bytes(content)
        else:
            audit.require(path.read_bytes() == content, f'non-reproducible successor: {name}')
    print(f'PASS: blue fringe removed; {len(footprint)} bounded external pixels; rejected candidate preserved')


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--build', action='store_true')
    run(parser.parse_args().build)
