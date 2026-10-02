# Woodland–Shrine external Clearing-junction correction

The owner reported a small gap between Herb Patch and Stone Marker in the
[previous shrine-entry correction](woodland-shrine-path-correction.md). The finite new strip
starts at `(1000,350)`, while the accepted Laundry–Clearing strip and rounded end cap approach
at a different heading. The uncovered wedge exposes the retained greybox road beneath them.

This external-only pass extends the unchanged straight-path source 24px backwards along the
Woodland–Shrine heading, beneath the accepted end cap at z=-74. The extension starts at
approximately `(1023.92233,351.92922)`. It is composited beneath the previous proof only inside
world rectangle `(994,300)..(1028,402)`. The canvas remains 704x208 at `(352,224)`, center
`(704,328)`. Every opaque original pixel and every pixel outside that rectangle is unchanged;
2,241 pixels gain coverage or blend at an existing translucent edge. Texture phase in the
short addition comes from the source's first rows; no recoloring or asset editing is involved.
The shrine-entry clipping remains exact. Neither accepted Laundry texture nor end cap changes.

**Result: gap covered; owner accepted the external views on 2026-10-02 ("Acceptable").** Three samples
inside the reported wedge previously matched the greybox road exactly; they now show opaque
packed earth. All six Clearing-junction views show the local correction. Sixteen of 18 views
change only within the independently projected rectangle, allowing two screen pixels for
filtering. The two close Shrine-entry views remain byte-identical. Ordinary camera framing
and limits match the previous proof; no saved Main scene is created. Earlier evidence,
all 432 runtime/production/import/physical baseline files and gameplay authority remain intact.
No production/runtime promotion is approved.

## Clearing junction

| Zoom | Wide | Tall |
| --- | --- | --- |
| 0.8x | [view](woodland-shrine-path-junction-clearing-junction-0.8x-wide.png) | [view](woodland-shrine-path-junction-clearing-junction-0.8x-tall.png) |
| 1.0x | [view](woodland-shrine-path-junction-clearing-junction-1.0x-wide.png) | [view](woodland-shrine-path-junction-clearing-junction-1.0x-tall.png) |
| 2.0x | [view](woodland-shrine-path-junction-clearing-junction-2.0x-wide.png) | [view](woodland-shrine-path-junction-clearing-junction-2.0x-tall.png) |

## Midpoint

| Zoom | Wide | Tall |
| --- | --- | --- |
| 0.8x | [view](woodland-shrine-path-junction-midpoint-0.8x-wide.png) | [view](woodland-shrine-path-junction-midpoint-0.8x-tall.png) |
| 1.0x | [view](woodland-shrine-path-junction-midpoint-1.0x-wide.png) | [view](woodland-shrine-path-junction-midpoint-1.0x-tall.png) |
| 2.0x | [view](woodland-shrine-path-junction-midpoint-2.0x-wide.png) | [view](woodland-shrine-path-junction-midpoint-2.0x-tall.png) |

## Shrine entry

| Zoom | Wide | Tall |
| --- | --- | --- |
| 0.8x | [view](woodland-shrine-path-junction-shrine-entry-0.8x-wide.png) | [view](woodland-shrine-path-junction-shrine-entry-0.8x-tall.png) |
| 1.0x | [view](woodland-shrine-path-junction-shrine-entry-1.0x-wide.png) | [view](woodland-shrine-path-junction-shrine-entry-1.0x-tall.png) |
| 2.0x | [view](woodland-shrine-path-junction-shrine-entry-2.0x-wide.png) | [view](woodland-shrine-path-junction-shrine-entry-2.0x-tall.png) |

## Reproduction and evidence

```sh
/opt/homebrew/bin/python3 art-direction/production-proof/corrected/tools/build_woodland_shrine_path_junction.py
godot --path game --script "$PWD/scripts/capture-woodland-shrine-path-junction.gd"
/opt/homebrew/bin/python3 art-direction/production-proof/corrected/tools/check_woodland_shrine_path_junction.py --check-live-baseline
```

The new evidence pins the recipe, capture harness, previous correction evidence and all
19 PNG hashes. The validator recursively preserves earlier proof inputs/images and checks
reproduction, coverage, opaque-pixel preservation, local world/screen bounds and reported
wedge samples. Its optional live-baseline check covers all 432 frozen files; regular quality
allows future separately approved promotions. Repeated host Compatibility captures reproduced
all 18 view hashes. Focused validation and the frozen baseline passed again after captures.
Full host `./scripts/quality.sh` passed all asset validators, formatting, a zero-warning build,
all 5,033 tests, scenario validation, the complete credential-free Godot smoke suite and
Compatibility graphical launch. Cumulative code/visual review found no further implementation
issue; owner subsequently accepted the external views on 2026-10-02 ("Acceptable"). The staged diff is whitespace-clean.

Next slice: obtain separate approval for the [exact named Woodland–Shrine promotion](woodland-shrine-path-promotion-scope.md).
External visual acceptance is recorded; routine external corrections retain standing authorization. Preserve navigation, camera,
scenario, dialogue, clues, discovery, treatment, save, provider, UI, audio, release and signing.
