# Grouped Woodland Clearing dressing runtime scope

The owner selected **grouped** on 2026-10-06, superseding the earlier sparse choice:
[exact selection](../woodland-clearing-dressing/owner-selection.json).
Arrangement selection and the approved 42-module artwork batch are closed decisions.
This proposal introduces no runtime assets or scene changes. Named installation
approval and actual runtime-view confirmation remain separate.

Prepared against published `main` `4d47ac5a4b25db76e24ff9e8358a16dae950c0cd`.
[Source bindings](source-bindings.json) freeze this prose, checker and exact inputs.
[Proposal inputs](proposal-inputs.json) specify all asset paths, native RGBA/alpha
facts, proposed pivots, editable masters and placement order.

## Exact proposed placements

Use one independent, script-free root `WoodlandClearingDressingPresentation` at
`(0,0)`, z=0, unit scale and zero rotation. Insert it after
`VillageEdgeGroundPresentation`, before final `WholeChannelPresentation`.
Its sixteen script-free, childless Sprite2Ds keep this exact draw order:

| Order | Approved module | Native origin | Size | World pivot |
| --- | --- | --- | --- | --- |
| 1 | Rounded shrub / shrubs-01 | (920,132) | 125x88 | (982,219) |
| 2 | Forked branch / fallen-wood-07 | (1050,192) | 97x64 | (1098,255) |
| 3 | Pebble scatter / rocks-08 | (628,382) | 77x40 | (666,421) |
| 4 | Small fern / environmental-details-01 | (1160,156) | 78x56 | (1199,211) |
| 5 | Leaf cluster / environmental-details-05 | (795,395) | 37x22 | (813,416) |
| 6 | Leaf scatter / environmental-details-06 | (836,170) | 40x24 | (856,193) |
| 7 | Dry grass / environmental-details-03 | (1140,80) | 58x48 | (1169,127) |
| 8 | Groundcover / environmental-details-09 | (912,206) | 44x28 | (934,233) |
| 9 | Shade bush / shrubs-08 | (1054,114) | 115x80 | (1111,193) |
| 10 | Hollow log / fallen-wood-03 | (968,186) | 110x72 | (1023,257) |
| 11 | Flat stone / rocks-03 | (1128,94) | 98x72 | (1177,165) |
| 12 | Broad fern / environmental-details-02 | (860,118) | 102x72 | (911,189) |
| 13 | Moss cushion / environmental-details-07 | (954,226) | 50x32 | (979,257) |
| 14 | Stick cluster / fallen-wood-09 | (670,394) | 75x40 | (707,433) |
| 15 | Mushrooms / environmental-details-08 | (772,400) | 59x36 | (801,435) |
| 16 | Young shoots / shrubs-07 | (1020,96) | 76x72 | (1058,167) |

Each child uses effective z=-60, relative depth, `centered=false`, zero offset and
rotation, unit scale, white modulation, no region, linear filtering and repeat
disabled. Preserve the proposed source pivots exactly; positions are the native
origins above, not the foot pivots. No Y-sorting or reordered overlaps.

## Allowed implementation after named approval

Copy precisely these sixteen approved PNG exports and sixteen 2x OpenRaster
masters unchanged into the paths in `proposal-inputs.json`. Use existing `plants`
and `objects` categories. Append sixteen manifest entries and one grouped-dressing
provenance block through a new scoped
`production-proof/corrected/tools/build_woodland_clearing_dressing.py`.
Production counts become **113 assets / 64 masters**, from 97 / 48. Preserve every
old manifest entry, export, master, animation, provenance and runtime asset.

Copy the same PNG bytes to isolated proof and runtime paths. Add independent
lossless `.import` sidecars with distinct canonical UIDs/cache paths: compression0,
no mipmaps, alpha-border fixing, premultiplication, channel remapping or size limit.
Preserve original native alpha edges and exact hidden-RGB counts bound per asset;
the inherited cutouts intentionally have tightly cropped boundaries. Do not
repaint, pad, crop, resample or silently clean transparent texels. New closed
validation must require exact selected RGBA, alpha facts and import parity; it must
not relax any existing asset contract. Retain failures for a bounded correction
if exact selected output cannot be preserved.

Main edits are limited to sixteen texture resources, the one owner and sixteen
children, plus one inactive environment-gated `RuntimeWoodlandClearingDressingSmokeCheck`
resource/node, its C# script and `.uid`. Preserve all previous resource/node blocks
and sibling order. Add sixteen isolated samples/resources in
`DirectionAImportProof.tscn` and narrowly update its exact membership/import parity
to 113 assets. No collision, navigation, interactable or discovery node is added.

New runtime tooling is limited to `scripts/validate-woodland-clearing-dressing-runtime.py`,
`scripts/capture-woodland-clearing-dressing-runtime.gd` and `.uid`, and a separate
`woodland-clearing-dressing-runtime-promotion/` authorization/baseline/evidence packet.
Allow bounded quality orchestration and continuation documentation. Freeze the
actual starting published head after named approval. Validate frozen predecessors
in their accepted checkout where historical closed membership rejects successors;
record that distinction rather than changing frozen inventories or weakening checks.

## Preservation and validation

Keep the entire native rectangles plus their filtering margin outside all retained
96px corridors, entrances, visible labels, interaction footprints and the concealed
specimen. Preserve approved paint, ground origins/depths, route/Garden sibling order,
all physical owners, gate/Forester, and concealed/clue/discovery state.
WholeChannel stays final: five sprites, native three-node x<1664 shader scope,
UVs/caps/crossing, Shrine triangle, Clearing near side and x=1664/x=2000 limits.
No physics/navigation/layout/scenario/gameplay/dialogue/treatment/save/provider/
functional-UI/audio/release/signing change. Human accessibility, pacing and terrain
completeness remain open.

Require predecessor byte and scene-block preservation, append-only manifest/master
parity, independent native imports, exact pivots/origins/depth/order, source-specific
alpha integrity and route/label/specimen clearance. Probe wrong variant, omitted or
duplicate placement, moved/resized/flipped sprite, changed paint/master/import,
reordered overlap, added authority or altered physical owner. Runtime changes require
fresh product quality, build/tests/scenario/smokes/formatting and cold import.

Capture ordinary Main using actual imported runtime textures, with no external
proof sprite: eight grouped views plus eight paired baselines. Keep the selected
external harness's actor/HUD/startup/frozen-controller settings and camera transforms:
0.8x/1x/2x at 1280x720 and 1280x827, target `(924,380)` with native camera clamps;
complete placement views at 800x480 and 1600x960, center `(924,240)`.
Require exact external/runtime RGBA parity, unchanged owners and bounded GPU
differences. Retain exact native/2x envelope and both cluster crops.

Present the complete actual runtime series for separate owner confirmation before
publishing runtime promotion. Do not request grouped arrangement selection again.

## Current checkpoint

The original comparison, approved artwork, accepted runtime and quality evidence
remain byte-exact. [Verification](verification.json) records fresh proposal/selection
bindings and mutation rejection checks; no repeated artwork capture or full product
suite is claimed. This packet is ready for named approval to install exactly the
sixteen grouped placements above.

```sh
python3 art-direction/woodland-clearing-dressing-runtime-scope/check.py
```
