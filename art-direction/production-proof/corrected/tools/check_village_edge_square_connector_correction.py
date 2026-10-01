#!/usr/bin/env python3
"""Validate the bounded external correction; passing is not visual acceptance."""
import argparse
import hashlib
import json
from PIL import Image
from check_village_edge_square_connector_feasibility import ROOT, REVIEW, require, validate as validate_original
from build_village_edge_square_connector_correction import compose, OUTPUT


def validate(check_live_baseline: bool = False) -> None:
    validate_original(check_live_baseline)
    with Image.open(OUTPUT) as image:
        require(image.size == (124, 96), 'incorrect connector footprint')
        strip = image.convert('RGBA')
    with Image.open(ROOT / 'game/art/runtime/terrain/straight-path.png') as image:
        original = image.convert('RGBA').crop((0, 0, 96, 124)).transpose(Image.Transpose.ROTATE_270)
    with Image.open(ROOT / 'game/art/runtime/terrain/village-square-junction.png') as image:
        junction = image.convert('RGBA')
    require(strip.crop((32, 0, 92, 96)).tobytes() == original.crop((32, 0, 92, 96)).tobytes(),
            'central 60 columns changed')
    require(strip.crop((31, 0, 93, 96)).tobytes() == original.crop((31, 0, 93, 96)).tobytes(),
            'inner blend boundaries changed')
    require(strip.crop((123, 0, 124, 96)).tobytes() == junction.crop((0, 208, 1, 304)).tobytes(),
            'east seam mismatch')
    require(all(strip.getpixel((0, y)) == (114, 100, 77, 255) for y in range(96)),
            'west greybox handoff mismatch')
    require(strip.tobytes() == compose().tobytes(), 'non-deterministic blend')
    evidence = json.loads((REVIEW / 'village-edge-square-connector-correction-evidence.json').read_text())
    groups = ['evidence', 'baseline'] if check_live_baseline else ['evidence']
    for group in groups:
        for relative, digest in evidence[group].items():
            require(hashlib.sha256((ROOT / relative).read_bytes()).hexdigest() == digest,
                    f'{group} hash changed: {relative}')
    for zoom in ['0.8', '1.0', '2.0']:
        for frame, height in [('wide', 720), ('tall', 827)]:
            with Image.open(REVIEW / f'village-edge-square-connector-corrected-{zoom}x-{frame}.png') as image:
                require(image.size == (1280, height), 'review frame size changed')
    print('Correction evidence valid: central pixels retained, exact seams, six captures.')
    print('Opaque-fade limitation: west fade remains conspicuous; this candidate stays unpromoted.')


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--check-live-baseline', action='store_true')
    validate(parser.parse_args().check_live_baseline)
