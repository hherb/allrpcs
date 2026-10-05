#!/usr/bin/env python3
"""Author the entire optional Village Edge apron comparison from approved paint."""

import argparse
import hashlib
import importlib.util
import json
from pathlib import Path

import numpy as np
from PIL import Image, ImageFilter

ROOT = Path(__file__).resolve().parent
REPO = ROOT.parents[1]
AUDIT = REPO / 'art-direction/village-edge-ground-coverage'
ORIGIN = (1480, 620)
SIZE = (500, 390)
CONTEXT_ORIGIN = ORIGIN
VARIANTS = ('baseline', 'hard-support', 'feather-16', 'feather-24')
SOURCE = 'art-direction/residual-ground-tiling-proof/exports/packed-earth.png'
FONT = Path('/System/Library/Fonts/Supplemental/Arial.ttf')
CROPS = {'woodland': (10, 110, 150, 260), 'stream': (190, 40, 350, 150),
         'square': (280, 195, 468, 365)}


def module(name, path):
    """Load frozen repository-owned authoring helpers without modifying their files."""
    spec = importlib.util.spec_from_file_location(name, path)
    result = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(result)
    return result


laundry = module('laundry_helpers', REPO / 'art-direction/laundry-ground-feasibility/build.py')
audit = module('entrance_audit_helpers', AUDIT / 'build.py')
laundry.SIZE = SIZE
png, raster, require, sha, text = audit.png, audit.raster, audit.require, audit.sha, audit.text


def masks():
    """Apply the audit's three native exterior alpha segments and strictly inward feathers."""
    evidence = json.loads((AUDIT / 'capture-evidence.json').read_text())
    support = Image.new('L', SIZE)
    for node, box in audit.SEGMENTS.values():
        owner = evidence['owners']['GreyboxNavigation/RoutePresentation/' + node]
        alpha = audit.world_alpha(owner)
        local = tuple(box[i] - ORIGIN[i % 2] for i in range(4))
        selected = Image.new('L', SIZE)
        selected.paste(alpha.crop(local).point(lambda a: 255 if a else 0), local[:2])
        expanded = selected.filter(ImageFilter.MaxFilter(49))
        support = Image.fromarray(np.maximum(np.asarray(support), np.asarray(expanded)))
    result = {'hard-support': support}
    for width in (16, 24):
        padding = width + 1
        padded = Image.new('L', (SIZE[0] + 2 * padding, SIZE[1] + 2 * padding))
        padded.paste(support, (padding, padding))
        box = (padding, padding, padding + SIZE[0], padding + SIZE[1])
        distance = np.zeros((SIZE[1], SIZE[0]), dtype=np.float64)
        for _ in range(width):
            distance += np.asarray(padded.crop(box)) > 0
            padded = padded.filter(ImageFilter.MinFilter(3))
        fraction = np.clip((distance - 0.5) / width, 0, 1)
        fraction[np.asarray(padded.crop(box)) > 0] = 1
        result[f'feather-{width}'] = Image.fromarray(
            np.rint(255 * fraction * fraction * (3 - 2 * fraction)).astype('uint8'))
    return result


def generate():
    """Write three masks, unchanged world-phase paint and independent editable masters."""
    source_inputs = json.loads((AUDIT / 'inputs.json').read_text())['sources']
    paths = list(source_inputs) + [str((AUDIT / name).relative_to(REPO)) for name in
                                 ['build.py', 'audit.json', 'capture-evidence.json', 'baseline.json']]
    paths += ['art-direction/laundry-ground-feasibility/build.py',
              'art-direction/square-ground-feasibility/build.py',
              'art-direction/village-edge-ground-feasibility/build.py',
              'art-direction/production-proof/corrected/asset-manifest.json']
    sources = {name: sha(REPO / name) for name in sorted(set(paths))}
    for name, digest in source_inputs.items():
        require(sources[name] == digest, 'Audit input drift: ' + name)
    frozen = json.loads((AUDIT / 'audit.json').read_text())
    require(frozen['candidate_recipe']['origin'] == list(ORIGIN)
            and frozen['candidate_recipe']['analysis_size'] == list(SIZE)
            and frozen['candidate_recipe']['z_index'] == -78, 'Audited recipe changed')
    # Reproduction is a local authoring check; product/CI files remain untouched.
    expected = audit.generate()
    require((AUDIT / 'audit.json').read_bytes() == expected['audit.json'], 'Frozen audit drift')
    field = laundry.repeat(REPO / SOURCE, 128, ORIGIN, SIZE)
    outputs = {'proof-inputs/world-paint.png': png(field)}
    measurements = {}
    for name, coverage in masks().items():
        layer = laundry.mask_field(field, coverage)
        outputs[f'proof-inputs/{name}-coverage.png'] = png(coverage)
        outputs[f'proof-inputs/{name}-layer.png'] = png(layer)
        outputs[f'masters/{name}.ora'] = laundry.master(name, layer, field, coverage)
        pixels = np.asarray(layer)
        require(np.all(pixels[pixels[:, :, 3] == 0] == 0), 'Hidden RGB introduced')
        measurements[name] = {'size': list(layer.size), 'support_texels': int(np.count_nonzero(pixels[:, :, 3])),
                              'translucent_texels': int(np.count_nonzero((pixels[:, :, 3] > 0) & (pixels[:, :, 3] < 255)))}
    inventory = {'status': 'complete external comparison; owner preference pending',
                 'date': '2026-10-05', 'timezone': 'Australia/Sydney',
                 'baseline_head': frozen['baseline_head'], 'sources': sources,
                 'origin': list(ORIGIN), 'size': list(SIZE), 'variants': list(VARIANTS),
                 'world_phase': [0, 0], 'period': 128, 'z_index': -78,
                 'recipe': frozen['candidate_recipe'], 'measurements': measurements,
                 'outputs': {name: hashlib.sha256(data).hexdigest() for name, data in outputs.items()}}
    outputs['inventory.json'] = (json.dumps(inventory, indent=2) + '\n').encode()
    return outputs


def main():
    """Author once, or verify exact bytes without overwriting retained evidence."""
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--check', action='store_true')
    args = parser.parse_args()
    require(args.check or not (ROOT / 'inventory.json').exists(), 'Existing comparison is immutable')
    for name, data in generate().items():
        path = ROOT / name
        if args.check:
            require(path.read_bytes() == data, 'Output drift: ' + name)
        else:
            require(not path.exists(), 'Refusing to overwrite: ' + name)
            path.write_bytes(data)
    print('PASS: complete Village Edge comparison inputs and three editable 2x masters')


if __name__ == '__main__':
    main()
