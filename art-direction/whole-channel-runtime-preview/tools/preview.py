#!/usr/bin/env python3
"""Build/check isolated preview artifacts and audit immutable predecessor bytes."""

import argparse
import hashlib
import io
import json
from pathlib import Path
import subprocess
import tempfile

from PIL import Image, ImageChops, ImageDraw, ImageFilter
import layers

ROOT = layers.ROOT
HERE = Path(__file__).resolve().parents[1]
BASE = '0c0d8330b287e4ea551f108202eeb4ee1fb47bd9'
DOCS = {'HANDOVER.md', 'README.md', 'ROADMAP.md'}


def require(condition, message):
    """Keep checks active with optimized Python."""
    if not condition:
        raise ValueError(message)


def sha(path):
    """Bind exact source/artifact bytes."""
    return hashlib.sha256(path.read_bytes()).hexdigest()


def scope():
    """Verify the entire approved starting tree and historical preservation subsets."""
    approval = json.loads((HERE / 'stage-1-authorization.json').read_text())
    require(approval['owner_statement'] == 'approved' and approval['stage_1_disposable_preview_authorized']
            and not approval['stage_2_production_runtime_authorized'], 'Stage 1 authority differs')
    require(sha(ROOT / approval['scope_path']) == approval['scope_sha256'], 'approved proposal changed')
    require(sha(HERE / 'baseline.json') == approval['baseline_sha256'], 'predecessor inventory bytes changed')
    baseline = json.loads((HERE / 'baseline.json').read_text())
    require(baseline['base_commit'] == approval['baseline_commit'] == BASE, 'baseline head changed')
    names = subprocess.check_output(['git', 'ls-tree', '-r', '--name-only', BASE], cwd=ROOT).decode().splitlines()
    require(set(baseline['files']) == {n for n in names if Path(n).name != '.DS_Store'}, 'incomplete inventory')
    for name, digest in baseline['files'].items():
        if name not in DOCS:
            require(sha(ROOT / name) == digest, f'protected predecessor changed: {name}')
    historical = json.loads((ROOT / 'art-direction/forest-path-runtime-promotion/baseline.json').read_text())['files']
    # The forest promotion changed only its declared files; bind that historical inventory unchanged.
    require(sha(ROOT / 'art-direction/forest-path-runtime-promotion/baseline.json') ==
            baseline['files']['art-direction/forest-path-runtime-promotion/baseline.json'], 'historical inventory changed')
    require(set(historical) <= set(baseline['files']), 'historical names absent from successor')
    all_files = []
    for args in (['git', 'ls-files', '-z'], ['git', 'ls-files', '--others', '--exclude-standard', '-z']):
        all_files.extend(subprocess.check_output(args, cwd=ROOT).decode().split('\0'))
    prefix = HERE.relative_to(ROOT).as_posix() + '/'
    require(all(not n or n in baseline['files'] or n.startswith(prefix) or Path(n).name == '.DS_Store'
                for n in all_files), 'unscoped new file')
    return baseline


def sources():
    """Bind all frozen sources plus the current reproducible preview recipes."""
    paths = set(json.loads((ROOT / 'art-direction/whole-channel-runtime-scope/source-bindings.json').read_text())['source_sha256'])
    paths.update(['art-direction/whole-channel-runtime-scope/README.md',
                  'art-direction/western-watercourse-join-audit/tools/join.py',
                  'art-direction/western-watercourse-join-audit/tools/audit.py',
                  'art-direction/western-watercourse-join-audit/tools/fringe.py',
                  'art-direction/western-watercourse-join-audit/correction/review-request.json',
                  'art-direction/production-proof/corrected/tools/build.py'])
    paths.update(p.relative_to(ROOT).as_posix() for p in (HERE / 'tools').glob('*.py'))
    return {p: sha(ROOT / p) for p in sorted(paths)}


def png(image):
    """Encode deterministic detached PNG bytes."""
    stream = io.BytesIO()
    image.save(stream, format='PNG')
    return stream.getvalue()


def payloads(runtime, atlases, reference):
    """Create exact 2x independently editable masters and source comparison exports."""
    result = {'reference/accepted-reconstruction.png': png(reference)}
    builder = layers.load('preview_builder', 'art-direction/production-proof/corrected/tools/build.py')
    with tempfile.TemporaryDirectory(prefix='channel-masters-') as directory:
        for name, image in runtime.items():
            result[f'exports/channel-{name}.png'] = png(image)
            path = Path(directory) / f'channel-{name}.ora'
            doubled = image.resize((layers.SIZE[0] * 2, layers.SIZE[1] * 2), Image.Resampling.NEAREST)
            builder.create_ora(path, doubled.size, [(name, doubled, 0, 0, True)])
            result[f'masters/channel-{name}.ora'] = path.read_bytes()
    return result


def check_runtime(runtime):
    """Reject source drift, duplicate retained presentation and invisible RGB."""
    expected, _, _ = layers.derive()
    require(set(runtime) == set(layers.NAMES), 'layer names changed')
    for name, image in runtime.items():
        layers.c.equal(image, expected[name], f'{name}: source layer/mask/UV drift')
        require(all(p[3] or p == (0, 0, 0, 0) for p in image.get_flattened_data()), f'{name}: hidden RGB')
        require(image.crop((1664 - layers.ORIGIN[0], 0, 2000 - layers.ORIGIN[0], layers.SIZE[1]))
                .getchannel('A').getbbox() is None, f'{name}: duplicate retained crossing/caps')


def mutations(runtime):
    """Exercise independent retained-region and exact source checks on disposable images."""
    for name, point, value, label in (
        ('water', (1800 - layers.ORIGIN[0], 500 - layers.ORIGIN[1]), (158, 125, 79, 209), 'duplicate CPU crossing'),
        ('water', (500 - layers.ORIGIN[0], 600 - layers.ORIGIN[1]), (0, 0, 0, 0), 'water gap'),
        ('banks', (400 - layers.ORIGIN[0], 540 - layers.ORIGIN[1]), (0, 0, 0, 0), 'bank/UV drift'),
        ('decks', (100, 0), (100, 90, 60, 255), 'out-of-footprint deck'),
        ('fringe', (1600 - layers.ORIGIN[0], 466 - layers.ORIGIN[1]), (20, 80, 120, 255), 'blue fringe'),
    ):
        changed = {key: image.copy() for key, image in runtime.items()}
        changed[name].putpixel(point, value)
        try:
            for key in runtime:
                layers.c.equal(changed[key], runtime[key], f'{key}: source layer drift')
        except ValueError:
            continue
        raise ValueError(f'vacuous or accepted mutation: {label}')
    print('PASS: five disposable pixel mutations rejected')


def check_captures(manifest, record=False):
    """Require source-stamped paired captures with exact dimensions and retained-region parity."""
    context = json.loads((HERE / 'capture-context.json').read_text())
    require(context['manifest_sha256'] == sha(HERE / 'manifest.json'), 'stale capture manifest')
    require(context['capture_script_sha256'] == sha(HERE / 'tools/capture.gd'), 'stale capture script')
    require(context['main_sha256'] == sha(ROOT / 'game/Main.tscn'), 'stale live scene')
    require(len(context['views']) == 37, 'incomplete paired view matrix')
    hashes, differences = {}, {}
    support = Image.new('L', layers.SIZE)
    for layer in manifest['layers']:
        with Image.open(HERE / 'exports' / layer['filename']) as image:
            support = ImageChops.lighter(support, image.getchannel('A'))
    support = support.point(lambda alpha: 255 if alpha else 0)
    ImageDraw.Draw(support).rectangle((1600 - layers.ORIGIN[0], 452 - layers.ORIGIN[1],
                                       1664 - layers.ORIGIN[0], 548 - layers.ORIGIN[1]), fill=255)
    support = support.filter(ImageFilter.MaxFilter(5))
    require(context['physical_nodes_unchanged'] is True, 'captured physical nodes changed')
    require(context['route_siblings'][-1] == 'ClosedForestPath', 'accepted route order changed')
    uids = set()
    for layer in manifest['layers']:
        name = layer['filename']
        imported = HERE / 'imports' / name
        sidecar = HERE / 'imports' / (name + '.import')
        metadata = context['imports'][name]
        require(sha(sidecar) == metadata['import_sha256'], 'import metadata changed')
        require(sha(HERE / 'exports' / name) == metadata['png_sha256'], 'imported source changed')
        require(metadata['uid'] not in uids, 'preview import UID reused')
        uids.add(metadata['uid'])
        text = sidecar.read_text()
        for field in ('compress/mode=0', 'mipmaps/generate=false', 'process/fix_alpha_border=false', 'process/premult_alpha=false'):
            require(field in text, 'preview import is not exact lossless alpha')
        with Image.open(imported) as image, Image.open(HERE / 'exports' / name) as source:
            layers.c.equal(image.convert('RGBA'), source.convert('RGBA'), 'imported RGBA differs')
        hashes['imports/' + name] = sha(imported)
        hashes['imports/' + name + '.import'] = sha(sidecar)
    for view in context['views']:
        images = []
        for stage in ('ordinary', 'candidate'):
            relative = f"captures/{stage}-{view['name']}.png"
            path = HERE / relative
            with Image.open(path) as source:
                image = source.convert('RGBA')
            require(image.size == tuple(view['size']), f'capture dimensions: {relative}')
            hashes[relative] = sha(path)
            images.append(image)
        count = sum(a != b for a, b in zip(images[0].get_flattened_data(), images[1].get_flattened_data()))
        differences[view['name']] = count
        center, zoom = view['camera_center'], view['zoom']
        projected = support.transform(images[0].size, Image.Transform.AFFINE,
            (1 / zoom, 0, center[0] - layers.ORIGIN[0] - images[0].width / (2 * zoom),
             0, 1 / zoom, center[1] - layers.ORIGIN[1] - images[0].height / (2 * zoom)),
            resample=Image.Resampling.NEAREST)
        outside = projected.point(lambda value: 0 if value else 255)
        diff = ImageChops.difference(images[0], images[1])
        channels = diff.split()
        changed = ImageChops.lighter(ImageChops.lighter(channels[0], channels[1]),
                                     ImageChops.lighter(channels[2], channels[3]))
        require(ImageChops.multiply(changed, outside).getbbox() is None, 'GPU change outside recorded footprint')
        # Preserved live crossing interior, projected using the actual recorded camera transform.
        center, zoom = view['camera_center'], view['zoom']
        left = math_floor((1740 - center[0]) * zoom + images[0].width / 2)
        top = math_floor((488 - center[1]) * zoom + images[0].height / 2)
        right = math_floor((1860 - center[0]) * zoom + images[0].width / 2)
        bottom = math_floor((512 - center[1]) * zoom + images[0].height / 2)
        rect = (max(0, left), max(0, top), min(images[0].width, right), min(images[0].height, bottom))
        if rect[2] > rect[0] and rect[3] > rect[1]:
            layers.c.equal(images[0].crop(rect), images[1].crop(rect), 'live crossing interior changed')
    evidence = {'capture_sha256': hashes, 'changed_rgba_pixels_by_view': differences,
                'context_sha256': sha(HERE / 'capture-context.json'),
                'cpu_gpu_note': 'CPU source parity is exact; GPU comparison is ordinary versus candidate, not a GPU equality claim for the CPU reference.'}
    path = HERE / 'capture-evidence.json'
    if record:
        path.write_text(json.dumps(evidence, indent=2) + '\n')
    else:
        require(json.loads(path.read_text()) == evidence, 'capture evidence bytes/content changed')
    print('PASS: 74 source-stamped captures and preserved actual crossing interior')


def math_floor(value):
    """Use conservative pixel boundaries for projected retained-region checks."""
    import math
    return math.floor(value)


def run(build=False, captures=False, record=False, reject=False):
    """Build explicitly or reproduce all preview assets without ordinary check mutations."""
    baseline = scope()
    runtime, atlases, reference = layers.derive()
    check_runtime(runtime)
    artifacts = payloads(runtime, atlases, reference)
    manifest = {'schema_version': 1, 'status': 'Stage 1 disposable preview only', 'baseline_commit': BASE,
                'world_origin': layers.ORIGIN, 'size': layers.SIZE, 'center': [1238, 522],
                'layers': [{'name': n, 'filename': f'channel-{n}.png', 'z_index': z}
                           for n, z in zip(layers.NAMES, (-68, -67, -66, -66, -65))],
                'source_sha256': sources(),
                'artifact_sha256': {p: hashlib.sha256(data).hexdigest() for p, data in artifacts.items()},
                'cpu_reference_rgba_exact': True, 'production_runtime_authorized': False}
    data = (json.dumps(manifest, indent=2) + '\n').encode()
    artifacts['manifest.json'] = data
    for relative, content in artifacts.items():
        path = HERE / relative
        if build:
            path.parent.mkdir(parents=True, exist_ok=True)
            path.write_bytes(content)
        else:
            require(path.read_bytes() == content, f'non-reproducible artifact: {relative}')
    print(f"PASS: exact CPU channel, five transparent atlases/2x masters, {len(baseline['files']) - len(DOCS)} protected files")
    if reject:
        mutations(runtime)
    if captures or record:
        check_captures(manifest, record)


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--build', action='store_true')
    parser.add_argument('--check-captures', action='store_true')
    parser.add_argument('--record-captures', action='store_true')
    parser.add_argument('--check-rejections', action='store_true')
    args = parser.parse_args()
    run(args.build, args.check_captures, args.record_captures, args.check_rejections)
