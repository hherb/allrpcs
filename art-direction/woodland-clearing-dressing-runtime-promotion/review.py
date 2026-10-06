#!/usr/bin/env python3
"""Bind actual imported captures, exact external parity and complete native review boards."""

import argparse
import hashlib
import json
from pathlib import Path
import re
import sys

import numpy as np
from PIL import Image, ImageDraw

ROOT = Path(__file__).resolve().parent
REPO = ROOT.parents[1]
EXTERNAL = REPO / 'art-direction/woodland-clearing-dressing'
SCOPE = REPO / 'art-direction/woodland-clearing-dressing-runtime-scope'
sys.path.insert(0, str(EXTERNAL))
import boards
import build
import verify


def require(condition, message):
    """Fail on stale input, incomplete frames or any pixel outside the approved footprint."""
    if not condition:
        raise ValueError(message)


def sha(path):
    """Bind bytes for the owner-facing review packet."""
    return hashlib.sha256(path.read_bytes()).hexdigest()


def read(path):
    """Read retained structured evidence."""
    return json.loads(path.read_text())


def input_names():
    """Bind inherited rendering inputs plus every new approved installation input."""
    names = set(read(EXTERNAL / 'inventory.json')['sources'])
    names.update(str(SCOPE.relative_to(REPO)) + '/' + name for name in
                 ('README.md', 'source-bindings.json', 'proposal-inputs.json', 'owner-approval.json'))
    names.update(str(ROOT.relative_to(REPO)) + '/' + name for name in ('authorization.json', 'baseline.json'))
    names.update({'game/RuntimeWoodlandClearingDressingSmokeCheck.cs',
                  'game/RuntimeWoodlandClearingDressingSmokeCheck.cs.uid',
                  'scripts/capture-woodland-clearing-dressing-runtime.gd',
                  'scripts/capture-woodland-clearing-dressing-runtime.gd.uid',
                  'art-direction/production-proof/corrected/asset-manifest.json'})
    for asset in read(SCOPE / 'proposal-inputs.json')['assets']:
        names.update(asset[k] for k in ('export', 'source_master', 'production_export', 'production_master',
                                       'proof_texture', 'runtime_texture'))
        names.update(asset[k] + '.import' for k in ('proof_texture', 'runtime_texture'))
    return names


def prepare():
    """Freeze final runtime capture inputs once, after product build/import checks."""
    target = ROOT / 'capture-inputs.json'
    require(not target.exists() and not (ROOT / 'owner-runtime-acceptance.json').exists(), 'Capture inputs are immutable')
    target.write_text(json.dumps({name: sha(REPO / name) for name in sorted(input_names())}, indent=2) + '\n')
    print('PASS: frozen', len(input_names()), 'exact runtime capture inputs')


def frames():
    """Return exactly eight camera frames for each actual-runtime/control variant."""
    return {name for name in boards.expected_frames() if '/sparse-' not in name}


def check():
    """Require exact actual import, source parity, camera recipe, owners and clearance."""
    evidence = read(ROOT / 'capture-evidence.json')
    previous = read(EXTERNAL / 'capture-evidence.json')
    inputs = read(ROOT / 'capture-inputs.json')
    require(set(inputs) == input_names() and evidence['inputs'] == inputs, 'Capture input membership drift')
    require(evidence['input_binding_sha256'] == sha(ROOT / 'capture-inputs.json'), 'Input binding drift')
    for name, digest in inputs.items():
        require(sha(REPO / name) == digest, 'Runtime capture input drift: ' + name)
    require(set(evidence['captures']) == set(evidence['frames']) == frames(), 'Incomplete runtime/control series')
    require(evidence['owners'] == previous['owners'], 'Retained actual owners changed')
    assets = read(SCOPE / 'proposal-inputs.json')['assets']
    dressing = evidence['dressing']
    require(dressing['name'] == 'WoodlandClearingDressingPresentation' and dressing['origin'] == [0, 0]
            and dressing['z_index'] == 0 and len(dressing['placements']) == 16, 'Runtime owner/membership drift')
    for asset, actual in zip(assets, dressing['placements']):
        sidecar = REPO / (asset['runtime_texture'] + '.import')
        uid = re.search(r'^uid="([^"]+)"', sidecar.read_text(), re.MULTILINE).group(1)
        expected = dict(node_name=asset['node_name'], texture='res://' + asset['runtime_texture'][5:], uid=uid,
                        origin=asset['origin'], size=asset['size'], z_index=-60, z_as_relative=True,
                        centered=False, offset=[0, 0], scale=[1, 1], rotation=0, filter=2, repeat=1,
                        flip_h=False, flip_v=False, region_enabled=False, modulate=[1, 1, 1, 1],
                        script_free=True, child_count=0)
        require(actual == expected, 'Recorded placement drift: ' + asset['node_name'])
    rows = {'sparse': [], 'grouped': assets}
    build.check_placements(rows, build.clearance())
    clearance = verify.check_geometry({'variants': rows}, evidence)
    results = []
    for name in sorted(frames()):
        require(sha(ROOT / name) == evidence['captures'][name], 'Capture bytes changed: ' + name)
        require(evidence['frames'][name] == previous['frames'][name], 'Camera transform changed: ' + name)
        runtime = np.array(Image.open(ROOT / name).convert('RGBA'))
        external = np.array(Image.open(EXTERNAL / name).convert('RGBA'))
        require(runtime.shape == external.shape and np.array_equal(runtime, external), 'External/runtime RGBA mismatch: ' + name)
        frame = evidence['frames'][name]
        require(list(Image.open(ROOT / name).size) == frame['viewport'], 'Viewport/PNG dimensions changed')
        if '/grouped-' in name:
            control = np.array(Image.open(ROOT / name.replace('/grouped-', '/baseline-')).convert('RGBA'))
            changes = verify.check_difference(control, runtime, frame['world_to_canvas'], assets)
            results.append(dict(frame=name, external_runtime_changed_pixels=0, changed_pixels=changes))
    return dict(status='passed', actual_runtime_views=8, paired_baselines=8, external_rgba_mismatches=0,
                retained_owners=len(evidence['owners']), route_clearance_beyond_48px=clearance,
                raster_clearance='complete native rectangles plus 2px filtering margin outside all retained masks',
                differences=results, capture_evidence_sha256=sha(ROOT / 'capture-evidence.json'))


def png(image):
    """Reuse deterministic board encoding."""
    return build.png(image)


def generate():
    """Show all six overview frames plus complete native/2x envelope and both clusters."""
    report = check()
    evidence = read(ROOT / 'capture-evidence.json')
    outputs = {}
    for kind, height in [('wide', 720), ('tall', 827)]:
        sample_height = round(height * .4)
        board = Image.new('RGB', (1072, 82 + 3 * (sample_height + 40)), '#eee9df')
        draw = ImageDraw.Draw(board)
        build.label(draw, (16, 12), 'Actual imported grouped runtime / ' + kind, 24)
        build.label(draw, (16, 46), 'Retained baseline left; actual grouped right. Overview 0.4x; native details separate.')
        for row, zoom in enumerate((.8, 1, 2)):
            for col, variant in enumerate(('baseline', 'grouped')):
                name = f'captures/{variant}-{zoom:.1f}x-{kind}.png'
                image = Image.open(ROOT / name)
                x, y = 16 + col * 528, 82 + row * (sample_height + 40)
                board.paste(image.resize((512, sample_height), Image.Resampling.LANCZOS).convert('RGB'), (x, y))
                build.label(draw, (x, y + sample_height + 8), f'{variant} / camera {zoom:.1f}x')
        outputs[f'review/runtime-series-{kind}.png'] = png(board)
    bounds_set = {'envelope': (524, 0, 1324, 480), **boards.CLUSTERS}
    for scale in (1, 2):
        for label, bounds in bounds_set.items():
            name = f'captures/grouped-{scale:.1f}x-placement.png'
            image = Image.open(ROOT / name).convert('RGBA')
            detail = boards.crop(image, evidence['frames'][name]['world_to_canvas'], bounds, scale)
            outputs[f'review/grouped-{label}-{scale}x.png'] = png(detail)
    bindings = dict(status='complete actual imported runtime series', native_capture_sha256=evidence['captures'],
                    source_sha256={name: sha(ROOT / name) for name in ('capture-evidence.json', 'capture-inputs.json', 'review.py')},
                    font_sha256=sha(build.FONT), review_sha256={n: hashlib.sha256(b).hexdigest() for n, b in outputs.items()},
                    verification=report)
    outputs['series-bindings.json'] = (json.dumps(bindings, indent=2) + '\n').encode()
    return outputs


def main():
    """Prepare immutable input bindings or generate/check reproducible review boards."""
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--prepare-inputs', action='store_true')
    parser.add_argument('--check', action='store_true')
    args = parser.parse_args()
    if args.prepare_inputs:
        prepare()
        return
    outputs = generate()
    for name, data in outputs.items():
        path = ROOT / name
        require(not path.exists() or path.read_bytes() == data, 'Refusing board/evidence overwrite: ' + name)
        if args.check:
            require(path.is_file() and path.read_bytes() == data, 'Missing board/evidence: ' + name)
    if not args.check:
        require(not (ROOT / 'owner-runtime-acceptance.json').exists(), 'Accepted review is immutable')
        for name, data in outputs.items():
            path = ROOT / name
            path.parent.mkdir(parents=True, exist_ok=True)
            path.write_bytes(data)
    print(json.dumps(check(), indent=2))
    print('PASS: all 16 runtime/control views match external RGBA; complete native/2x review series')


if __name__ == '__main__':
    main()
