# Proposed rounded closed-forest path runtime promotion

Status: proposed, awaiting named runtime-promotion approval. The owner's 2026-10-03
“both approved” accepts the two images in [owner-acceptance.json](owner-acceptance.json).
It does not grant runtime authority. This proposal changes documentation only.

## Exact asset

Promote only `terrain.clearing_closed_forest_path`, using the unchanged
`result/exports/corrected.png`, SHA-256
`3984916a864042f54b5ccbf0d745c150f7807d064a44f9022f96f83b7d081451`.
Export/runtime filename: `clearing-closed-forest-path.png`. Retain the 552×384 canvas,
world origin `(520,32)`, centred pivot, sprite centre `(796,224)`, identity transform,
z=-75, transparent gutters, rounded terminus and exact gate-facing alpha easing.
All y≥196 pixels remain transparent; do not extend the texture into the Clearing.

Extend the existing corrected production builder/manifest with this one asset and its
retained source recipe/provenance. Produce an editable 1104×768 nearest-neighbour 2x
OpenRaster master with exact export parity. Preserve all existing production exports
and masters. Expected totals become 84 exports and 35 masters. Add only this manifest-bound
PNG to production `exports/terrain/` and `game/art/runtime/terrain/`, plus its independent
lossless proof/runtime imports. Disable mipmaps, alpha-border processing and premultiplication;
retain the runtime terrain filtering convention. Import the texture before engine checks.

## Presentation wiring

Wire the texture through `game/Main.tscn` and `game/GreyboxNavigation.cs`. Replace only
the exact 96px visual route `(1000,350)` → `(650,150)` for
`woodland-clearing` → `closed-deeper-forest-path`. Validate that connection exactly once
and its endpoints, width and texture dimensions at startup. Append one `ClosedForestPath`
sprite as the final `GreyboxNavigation/RoutePresentation` child, matching the accepted
disposable capture. Preserve all existing children and their relative order.

Update `RuntimeTerrainGroundSmokeCheck` from two lines/thirteen sprites to one line/fourteen
sprites, retaining every other assertion. Update the actual runtime sibling-order checks
in `scripts/check-woodland-village-edge-runtime.gd`, `check-village-edge-stream-runtime.gd`
and `check-stream-laundry-runtime.gd` to require the final consecutive sequence
Woodland–Village Edge → Village Edge–Stream → Stream–Laundry → ClosedForestPath.
Keep all existing decoded texture and placement checks. Historical capture recipes remain
archived evidence; use a separate new capture script for this slice.

## Physical and visual limits

Keep `OpeningRegionClosedForestPathSceneSource`, its geometry/layout, gate collision,
approach outlines, marker and Forester-access behavior unchanged. The retained gate centre
is `(744,200)`, half-width 48px, boundary thickness 8px and approach length 84px.
The proof replaces the entire route line with a closed-side-only texture: the near side
inside the Clearing remains untextured. This exact limitation must be visible in runtime
review; approval does not authorize filling it or changing the open-gate presentation.
Verify both closed startup and existing Forester access without new gameplay behavior.

Preserve navigation, collisions, camera, all accepted art, channel water/banks, scenario,
dialogue, clues, discovery, treatment, saves, providers, UI, audio, release and signing.
Keep capture-only HUD/Seeker hiding and camera framing out of ordinary startup. Do not
promote the whole channel, rounded-reference overlay, failed strips or another asset.

## Validation and publication checkpoint

Add focused master/export/runtime hash parity, alpha, geometry, draw-order, UID/cache
independence and restricted-change checks, plus an actual runtime-tree smoke. Preserve
all historical proof bytes and inventories. For post-promotion checks, use a new frozen
successor inventory and verify preserved predecessor subsets; never rewrite old inventories
or pretend their pre-promotion live-source hashes still match.

Capture 18 separate runtime views at gate `(744,200)`, midpoint `(825,250)` and closed end
`(650,150)`, at 0.8x/1x/2x in 1280×720 and 1280×827 frames. Require exact decoded PNG
parity with the accepted correction captures under equivalent capture-only visibility
and framing. Run `./scripts/quality.sh`, review the cumulative diff, then obtain owner
runtime visual confirmation before publishing the promoted slice directly to main.
Visual acceptance establishes no gameplay, pacing, usability or accessibility acceptance.
