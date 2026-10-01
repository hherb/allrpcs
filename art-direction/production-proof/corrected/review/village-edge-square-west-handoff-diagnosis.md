# Village Edge–Village Square west-handoff diagnosis

The retained external correction fixes the east seam and removes the underlying route fringe,
but its west RGBA fade introduces opaque shoulders and a dark band. This diagnostic slice
measures the existing proof; it creates no alternative artwork or runtime change.

| West band | Alpha raised | Transparent source pixels made visible | Source raw RGB luma | Corrected raw RGB luma |
| --- | ---: | ---: | ---: | ---: |
| 1 column | 96 | 13 | 146.675 | 101.316 |
| 8 columns | 766 | 97 | 148.226 | 106.688 |
| 16 columns | 1,533 | 208 | 146.793 | 112.104 |
| 32 columns | 2,613 | 381 | 148.840 | 125.485 |

In the outermost top and bottom rows, 31 formerly transparent pixels become visible in each
west 32-column band. The source west column has alpha values from zero to 252; the corrected
west column forces all 96 values to 255. This directly explains the rectangular shoulders.
The raw RGB luma reduction quantifies the tint change, not final composited brightness or a
perceptual acceptance score. The six graphical views supply evidence of the conspicuous band.

The yellow opening marker remains above the strip and covers part of the handoff. The large
Village Edge grey room is existing presentation, not the brown route target used by the blend.
Neither is changed by this experiment. In particular, matching the route colour does not imply
matching the adjacent room presentation. Those retained elements contribute to the visible
handoff, but cannot account for the newly raised strip alpha measured here.

Reproduce the retained [JSON measurements](village-edge-square-west-handoff-diagnosis.json):

```sh
python3 art-direction/production-proof/corrected/tools/diagnose_village_edge_square_west_handoff.py
```

## Follow-on comparison scope (now implemented)

Create a **separate external-only comparison** that retains the corrected strip except for its
west 32 columns. Preserve source alpha throughout those columns. Blend only RGB toward the same
retained greybox route RGB `(114,100,77)` over columns `0..7`, with weights `(7-x)/7` and integer
rounding to nearest. Columns `8..31` return to the original rotated source crop. Keep columns
`32..91` unchanged and columns `92..123` identical to the corrected proof's exact east blend.
This narrows the colour fade while eliminating introduced opaque shoulders. The west boundary
RGB matches the route; its alpha deliberately returns to source alpha, superseding the failed
opaque-west requirement only for this new comparison.

Use the same `124x96` footprint and temporary shortened preview line. Retain both previous proof
sets, marker/label layers, production sources/masters/exports/imports, and every runtime file.
Validate source-alpha parity in the west band, unchanged original pixels in columns `8..91`,
exact retained east pixels, and six deterministic 0.8x/1.0x/2.0x wide/tall views. Inspect all six
views against this retained failed correction. If the shortened fade or transparent west
handoff is still unsuitable, document that result and stop rather than widening or substituting.
This external comparison may begin without separate owner pre-start approval. Visual acceptance
and runtime promotion remain explicit later boundaries.

The comparison is now complete in [village-edge-square-connector-alpha-result.md](village-edge-square-connector-alpha-result.md).
