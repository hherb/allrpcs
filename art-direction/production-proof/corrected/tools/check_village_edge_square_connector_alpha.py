#!/usr/bin/env python3
"""Check the separate alpha-preserving proof without inferring visual acceptance."""
import argparse
import hashlib
import json
from PIL import Image
from check_village_edge_square_connector_feasibility import ROOT, REVIEW, require
from check_village_edge_square_connector_correction import validate as validate_corrected
from build_village_edge_square_connector_alpha import OUTPUT, compose


def validate(check_live_baseline: bool = False) -> None:
    validate_corrected(check_live_baseline)
    with Image.open(OUTPUT) as image:
        require(image.size == (124, 96), 'alpha comparison footprint changed')
        strip = image.convert('RGBA')
    with Image.open(ROOT / 'game/art/runtime/terrain/straight-path.png') as image:
        source = image.convert('RGBA').crop((0, 0, 96, 124)).transpose(Image.Transpose.ROTATE_270)
    with Image.open(REVIEW / 'village-edge-square-connector-corrected-strip.png') as image:
        corrected = image.convert('RGBA')
    require(strip.crop((8, 0, 92, 96)).tobytes() == source.crop((8, 0, 92, 96)).tobytes(),
            'original columns 8..91 changed')
    require(strip.crop((92, 0, 124, 96)).tobytes() == corrected.crop((92, 0, 124, 96)).tobytes(),
            'retained east blend changed')
    require(strip.getchannel('A').crop((0, 0, 32, 96)).tobytes() ==
            source.getchannel('A').crop((0, 0, 32, 96)).tobytes(), 'west source alpha changed')
    require(all(strip.getpixel((0, y))[:3] == (114, 100, 77) for y in range(96)),
            'west boundary RGB changed')
    require(strip.tobytes() == compose().tobytes(), 'alpha comparison recipe drift')
    evidence = json.loads((REVIEW / 'village-edge-square-connector-alpha-evidence.json').read_text())
    for relative, digest in evidence.items():
        require(hashlib.sha256((ROOT / relative).read_bytes()).hexdigest() == digest,
                f'alpha evidence hash changed: {relative}')
    for zoom in ['0.8', '1.0', '2.0']:
        for frame, height in [('wide', 720), ('tall', 827)]:
            with Image.open(REVIEW / f'village-edge-square-connector-alpha-{zoom}x-{frame}.png') as image:
                require(image.size == (1280, height), 'alpha review frame size changed')
    print('Alpha proof valid: west alpha retained, original columns 8..91, exact east blend, six views.')
    print('Owner visual acceptance and runtime promotion remain unrecorded.')


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--check-live-baseline', action='store_true')
    validate(parser.parse_args().check_live_baseline)
