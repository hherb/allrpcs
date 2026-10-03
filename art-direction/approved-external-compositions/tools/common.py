#!/usr/bin/env python3
"""Shared deterministic external-proof artifacts and immutable predecessor auditing."""

import hashlib
import importlib.util
import io
import json
from pathlib import Path
import subprocess
import sys
import tempfile
import xml.etree.ElementTree as ET
import zipfile

from PIL import Image

ROOT = Path(__file__).resolve().parents[3]
HERE = Path(__file__).resolve().parents[1]
PREFIX = HERE.relative_to(ROOT).as_posix() + '/'
BASE_COMMIT = '18894f81d836a380a3642ec481b264baaccba6aa'


def require(condition, message):
    """Keep verification active under optimized Python."""
    if not condition:
        raise ValueError(message)


def sha(path):
    """Hash one local file without changing it."""
    return hashlib.sha256(path.read_bytes()).hexdigest()


def load(name, relative):
    """Import a frozen repository helper without running its command-line entry point."""
    path = ROOT / relative
    sys.path.insert(0, str(path.parent))
    try:
        spec = importlib.util.spec_from_file_location(name, path)
        result = importlib.util.module_from_spec(spec)
        sys.modules[name] = result
        spec.loader.exec_module(result)
        return result
    finally:
        sys.path.pop(0)


def rgba(relative):
    """Read an RGBA source and close its file handle."""
    with Image.open(ROOT / relative) as image:
        return image.convert('RGBA')


def equal(first, second, message):
    """Compare all RGBA bytes, including alpha, with matching geometry."""
    require(first.mode == second.mode == 'RGBA' and first.size == second.size
            and first.tobytes() == second.tobytes(), message)


def compose(layers, size):
    """Composite named image/offset layers in their declared back-to-front order."""
    result = Image.new('RGBA', size)
    for _, image, position in layers:
        result.alpha_composite(image, position)
    return result


def scope(build=False):
    """Freeze the named head once; preserve every predecessor except the handover."""
    names = subprocess.check_output(['git', 'ls-files', '-z'], cwd=ROOT).decode().split('\0')
    names = sorted(n for n in names if n and n != 'HANDOVER.md'
                   and not n.startswith(PREFIX) and Path(n).name != '.DS_Store')
    path = HERE / 'baseline.json'
    if not path.exists():
        require(build, 'baseline missing')
        head = subprocess.check_output(['git', 'rev-parse', 'HEAD'], cwd=ROOT).decode().strip()
        require(head == BASE_COMMIT, 'unexpected starting head')
        path.write_text(json.dumps({n: sha(ROOT / n) for n in names}, indent=2) + '\n')
    baseline = json.loads(path.read_text())
    require(set(names) == set(baseline), 'protected inventory changed')
    for name, digest in baseline.items():
        require(sha(ROOT / name) == digest, f'protected predecessor changed: {name}')
    untracked = subprocess.check_output(
        ['git', 'ls-files', '--others', '--exclude-standard', '-z'], cwd=ROOT).decode().split('\0')
    require(all(not n or n.startswith(PREFIX) or Path(n).name == '.DS_Store' for n in untracked),
            'unscoped addition')
    return baseline


def predecessors():
    """Reproduce the accepted join/outlet/spring using a verified preservation superset."""
    baseline = scope()
    audit = load('audit', 'art-direction/western-watercourse-join-audit/tools/audit.py')
    frozen = json.loads((audit.HERE / 'baseline.json').read_text())
    require(all(baseline.get(n) == digest for n, digest in frozen.items()),
            'predecessor baseline is not preserved')
    audit.scope = lambda build: frozen
    audit.run(False)
    fringe = load('composition_fringe', 'art-direction/western-watercourse-join-audit/tools/fringe.py')
    fringe.run(False)


def tile(source, origin, size):
    """Repeat a repository texture in absolute world phase, including negative x."""
    image = Image.new('RGBA', size)
    ox, oy = origin
    for y in range(-(oy % source.height), size[1], source.height):
        for x in range(-(ox % source.width), size[0], source.width):
            image.paste(source, (x, y))
    return image


def write_artifacts(folder, origin, size, layers, images, evidence, sources, build):
    """Bind deterministic PNGs and a verified exact-nearest 2x editable assembly master."""
    builder = load('composition_builder', 'art-direction/production-proof/corrected/tools/build.py')
    joined = compose(layers, size)
    payloads = {}
    for name, image in images.items():
        require(image.mode == 'RGBA', f'{name}: mode')
        require(all(p[3] or p == (0, 0, 0, 0) for p in image.get_flattened_data()),
                f'{name}: hidden RGB')
        stream = io.BytesIO()
        image.save(stream, format='PNG')
        payloads[name] = stream.getvalue()
    with tempfile.TemporaryDirectory() as directory:
        master = Path(directory) / 'composition.ora'
        doubled = [(name, image.resize((image.width * 2, image.height * 2), Image.Resampling.NEAREST),
                    position[0] * 2, position[1] * 2, True) for name, image, position in layers]
        builder.create_ora(master, (size[0] * 2, size[1] * 2), doubled)
        with zipfile.ZipFile(master) as archive:
            stack = ET.fromstring(archive.read('stack.xml'))
            require((int(stack.attrib['w']), int(stack.attrib['h'])) == (size[0] * 2, size[1] * 2),
                    'master dimensions')
            require([n.attrib['name'] for n in stack.findall('./stack/layer')] ==
                    [n for n, _, _ in reversed(layers)], 'master order')
            with archive.open('mergedimage.png') as stream:
                equal(Image.open(stream).convert('RGBA'),
                      joined.resize((size[0] * 2, size[1] * 2), Image.Resampling.NEAREST),
                      'master/export parity')
        payloads['masters/composition.ora'] = master.read_bytes()
    manifest = {
        'schema_version': 1, 'status': 'external proof; owner visual assessment pending',
        'baseline_commit': BASE_COMMIT, 'world_origin': origin, 'export_size': size,
        'master_scale': 2, 'master_note': 'exact nearest enlargement; no invented painted detail',
        'draw_order': [name for name, _, _ in layers],
        'source_sha256': {name: sha(ROOT / name) for name in sorted(set(sources))},
        'artifact_sha256': {name: hashlib.sha256(data).hexdigest() for name, data in payloads.items()},
        'layout_runtime_physical_promotion_authorized': False,
    }
    for name, value in (('manifest.json', manifest), ('evidence.json', evidence)):
        payloads[name] = (json.dumps(value, indent=2) + '\n').encode()
    for name, data in payloads.items():
        path = HERE / folder / name
        if build:
            path.parent.mkdir(parents=True, exist_ok=True)
            path.write_bytes(data)
        else:
            require(path.read_bytes() == data, f'non-reproducible artifact: {folder}/{name}')
