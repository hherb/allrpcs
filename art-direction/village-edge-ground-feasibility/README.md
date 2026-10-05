# Village Edge three-entrance packed-earth comparison

Complete external series, 2026-10-05 (Australia/Sydney), based on the
[live coverage audit](../village-edge-ground-coverage/README.md).
New artwork preference is pending. No runtime promotion is selected or authorized.

Review the entire series together:

- [Wide series: all four variants at camera 0.8x/1x/2x](review/gpu-series-wide.png)
- [Tall series: all four variants at camera 0.8x/1x/2x](review/gpu-series-tall.png)
- [All three entrance edges: complete exact GPU 2x comparisons](review/complete-edge-series-2x.png)
- Native comparisons: [Woodland](review/gpu-woodland-1x.png),
  [Stream](review/gpu-stream-1x.png), [Square](review/gpu-square-1x.png)

Each board's columns are baseline, hard-support, feather-16 and feather-24.
All 24 full views and 12 dedicated detail views are retained. Dedicated framing
includes the full candidate apron edges, including the north support beyond the
ordinary 2x overview. Native and 2x crops contain unchanged GPU pixels, without
resampling. The earlier incomplete edge crops are retained as review evidence.

Feather-24 is the agent's visual preference for the softer moss blend across all
three entrances. Baseline/no new apron remains a valid owner choice: opaque quiet
moss already provides continuous ground coverage. The two feather widths differ
in the breadth of the shoulder blend; hard support is a diagnostic comparison.
This preference is not owner selection.

## Recipe and provenance

The external canvas is 500x390 at `(1480,620)`, z=-78, with linear filtering and
repeat disabled. Approved packed-earth paint is sampled at absolute world phase
`(0,0)`, period 128, without scaling, rotation or repainting. Only exact nonzero
route alpha in these world rectangles seeds support, with right/bottom exclusive:

| Entrance | Native route origin | Selected exterior rectangle |
| --- | --- | --- |
| Woodland | `(944,288)` | `(1532,730)-(1580,880)` |
| Stream | `(1640,440)` | `(1678,652)-(1822,700)` |
| Square | `(1820,852)` | `(1820,828)-(1868,972)` |

The union is expanded 24px in a square neighbourhood. Feather coverage erodes
strictly inward with zero padding and smoothstep at widths 16 and 24. A two-pixel
filter guard is clear of all five channel and seven accepted ground sprites.
Retained shell/openings occlude interior portions normally. Existing route paint
and woodland transitions keep their transforms, alpha, depth and ownership.

Three independent editable 1000x780 OpenRaster masters preserve masked paint,
an empty Corrections layer, hidden coverage and hidden raw world-phase paint.
Native exports have zero RGB at zero alpha. `inventory.json` binds the audited
recipe, every tracked game input, predecessor approvals, material, authoring code,
outputs and master hashes. `capture-evidence.json`,
`details/capture-evidence.json` and `review-board-manifest.json` bind captures,
framing and review images.

Both capture scripts instantiate Main only in disposable viewports and add
external ImageTextures. They never save/pack Main or import proof textures.
Only root HUD and characters are hidden. Physical owners, labels, all ground and
channel presentation remain. Each sandbox capture exited 134 without output;
the identical host retry completed the series with Compatibility rendering.

## Review and next slice

Cumulative code/visual review is in `session-review.json`. Initial artwork,
master integrity, baseline parity, capture bindings and visible support checks
pass. Six comparison baselines equal the audit RGBA exactly; all eighteen full
comparison deltas lie inside guarded support. Product/CI inputs are unchanged;
retain preceding passing 5,033-test evidence. No new full-suite/CI run is claimed.

Discuss this entire new series and any revisions before recording a selected
variant and exact approval hashes. Existing ground/channel/Laundry approvals
remain closed. Keep this packet immutable; author revisions in a separate packet.
After selection, perform fresh consolidated scoped checks:

```sh
python3 art-direction/village-edge-ground-coverage/build.py --check
python3 art-direction/village-edge-ground-coverage/verify.py
python3 art-direction/village-edge-ground-feasibility/build.py --check
python3 art-direction/village-edge-ground-feasibility/boards.py --check
python3 art-direction/village-edge-ground-feasibility/inspect_authoring.py
```

Only a selected, validated apron may proceed to a separate named runtime scope
proposal. Artwork selection alone does not authorize ordinary-scene changes.
Preserve paint, masks, masters, rejected evidence, physical owners, route/Garden
sibling order, channel limits, layout, navigation, gameplay, functional UI,
providers, audio, release and signing.
