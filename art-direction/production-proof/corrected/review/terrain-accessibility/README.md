# Terrain colour-vision and contrast review

Status: deterministic external review evidence, 2026-09-28. This directory is outside `game/` and
does not add or alter a manifest asset, editable master, export, import copy, or runtime reference.

## Method

`tools/terrain_accessibility_review.py` uses the Machado–Oliveira–Fernandes 2009 colour-vision
deficiency matrices at full severity (`1.0`). It decodes each 8-bit sRGB pixel to linear light,
multiplies linear RGB by the documented protanopia, deuteranopia, or tritanopia matrix, clips the
result to `[0, 1]`, re-encodes to 8-bit sRGB, and preserves alpha unchanged. The grayscale view uses
linear relative luminance `Y = 0.2126 R + 0.7152 G + 0.0722 B` in all three output channels.
The named method is from Gustavo M. Machado, Manuel M. Oliveira, and Leandro A. F. Fernandes,
“A Physiologically-based Model for Simulation of Color Vision Deficiency,” IEEE Transactions on
Visualization and Computer Graphics 15(6), 2009. The exact matrices used are stored in
`report.json`, so reproduction does not depend on an external library or service.

Every sheet places five unscaled 256x256 views in this fixed left-to-right order:

1. original;
2. grayscale;
3. protanopia;
4. deuteranopia; and
5. tritanopia.

The bounded input set is quiet moss, woodland leaf litter, the exact straight packed-earth overlay,
and all five quiet-moss/leaf-litter transitions. The packed-earth sample alpha-composites the exact
96x256 straight-path export at x=80 over the exact quiet-moss export; its statistics use only
visible path pixels. Every other sample is an unchanged opaque 256x256 manifest export.

## Reproduction and validation

Run from the repository root:

```sh
python3 art-direction/production-proof/corrected/tools/terrain_accessibility_review.py
python3 art-direction/production-proof/corrected/tools/validate_terrain_accessibility_review.py
```

The builder writes 40 individual exact-pixel views, eight side-by-side sheets, and `report.json`.
The report binds every input to its current manifest path and SHA-256 digest, binds every generated
view and sheet to its own digest, and records per-view mean relative luminance, luminance standard
deviation, the 10th-to-90th-percentile luminance span, and the root-mean-square luminance delta
between selected horizontal and vertical neighbours as a local texture measure. It also records
pairwise differences among quiet moss, leaf litter, and visible packed earth.

The validator independently reloads the current manifest exports, verifies their manifest hashes,
recomputes every transformed pixel and measurement, checks the exact PNG set and sheet ordering,
and compares the complete report. Any source, matrix, layout, view, or measurement drift fails.

## Interpretation boundary

These numbers describe exact current pixels and texture distributions. They are not accessibility
thresholds, a pass/fail result, or evidence of human recognition, route legibility, comfort, or
acceptance. The simulations model three common colour-vision deficiencies; they do not represent
every viewer or substitute for review by people with relevant lived experience. This slice makes no
claim about objects, characters, UI, the complete opening region, or ordinary runtime presentation.
