#!/usr/bin/env python3
"""Measure the retained external proof; never alter images or runtime assets."""
import json
from PIL import Image
from check_village_edge_square_connector_feasibility import ROOT, REVIEW
from check_village_edge_square_connector_correction import validate


def diagnose() -> dict:
    with Image.open(ROOT / 'game/art/runtime/terrain/straight-path.png') as image:
        source = image.convert('RGBA').crop((0, 0, 96, 124)).transpose(Image.Transpose.ROTATE_270)
    with Image.open(REVIEW / 'village-edge-square-connector-corrected-strip.png') as image:
        corrected = image.convert('RGBA')
    bands = []
    for width in [1, 8, 16, 32]:
        pairs = [(source.getpixel((x, y)), corrected.getpixel((x, y)))
                 for x in range(width) for y in range(96)]
        # Uncomposited 8-bit RGB luma, not a perceptual or screen-brightness metric.
        def mean_luma(index: int) -> float:
            return round(sum(sum(pixel[index][channel] * weight
                                 for channel, weight in enumerate((0.2126, 0.7152, 0.0722)))
                             for pixel in pairs) / len(pairs), 3)
        bands.append({'west_columns': width,
                      'alpha_raised_pixels': sum(b[3] > a[3] for a, b in pairs),
                      'transparent_source_made_visible': sum(a[3] == 0 and b[3] > 0 for a, b in pairs),
                      'source_mean_raw_rgb_luma': mean_luma(0),
                      'corrected_mean_raw_rgb_luma': mean_luma(1)})
    return {'footprint': [124, 96], 'bands': bands,
            'top_bottom_transparent_source_made_visible': {
                str(y): sum(source.getpixel((x, y))[3] == 0 and corrected.getpixel((x, y))[3] > 0
                            for x in range(32)) for y in [0, 1, 94, 95]},
            'west_source_alpha_values': sorted({source.getpixel((0, y))[3] for y in range(96)})}


if __name__ == '__main__':
    validate(True)
    print(json.dumps(diagnose(), indent=2))
