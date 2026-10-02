# Proposed Woodland Clearing–Village Edge production/runtime promotion

Status: owner-approved on 2026-10-03 ("approved"); runtime views owner-confirmed on 2026-10-03 ("confirmed"). The owner accepted the
[18 corrected external views](woodland-village-edge-path-correction.md) on 2026-10-03
("approved"). Routine external-proof approval is waived; the standing authorization retains
separate approval for this named production/runtime boundary.

## Exact asset and placement

Promote one manifest-bound `terrain.woodland_village_edge_path` asset, exported as
`exports/terrain/woodland-village-edge-path.png` and copied to
`game/art/runtime/terrain/woodland-village-edge-path.png`. The 816x688 PNG must exactly match
`woodland-village-edge-path-correction-overlay.png`, SHA-256 `ce4cdb9aed6494490ebfba4efa33d38aff950856ae60e7af735e5149ddcb062c`.
Retain world origin `(944,288)`, centered pivot, sprite center `(1352,632)`, z=-75, identity
scale/rotation, Village shell/opening clipping and all accepted Clearing-junction pixels.
No repaint, resampling, additional trim or marker relocation.

Extend the existing production builder/manifest using the retained correction recipe and
proof provenance. Add an editable 1632x1376 OpenRaster master through the established 2x
pipeline, preserving exact export bytes and every existing master/export. Copy only the new
manifest-bound PNG to runtime, with independent lossless, mipmap-free proof/runtime imports,
alpha-border processing disabled and existing terrain filtering. Expected totals: 81 exports
and 32 editable masters.

## Runtime wiring and ordering

Wire `WoodlandVillageEdgePathTexture` through `Main.tscn` and `GreyboxNavigation.cs`. Omit only
the exact 96px visual line `(1700,900)` → `(1000,350)` in the route loop. Validate the retained
route and 816x688 texture geometry at startup. Append the one `WoodlandVillageEdgePath` sprite
as the final child of `GreyboxNavigation/RoutePresentation`, after all other route presentation
children. This preserves the accepted proof's ordering against remaining greybox lines at
the same z=-75. Do not move, hide or reorder any other route child. The accepted Woodland–Shrine
z=-74 and Laundry z=-70 sprites continue drawing above it.

Preserve all route points, navigation/collision ownership, Village/Clearing bounds and markers,
Village floor, Clearing feather, Herb Patch, Stone Marker, labels and adjacent accepted assets.
Keep review-only camera framing and Seeker/HUD hiding outside ordinary startup.

## Validation and completion boundary

Add focused master/export/runtime hash parity, alpha, geometry, draw-order, independent import,
restricted presentation-scope and actual runtime-tree checks, including final sibling order.
Preserve both external proof stages, recipes, capture scripts and evidence records, the frozen
438-file baseline and the three supplemental Village port-owner bindings. Archived default
validators retain their evidence; optional live-baseline checks remain pre-promotion checks
and must not be rewritten to accommodate this promotion.

Capture 18 separate ordinary runtime views at Clearing junction, midpoint and Village entry,
zooms 0.8/1.0/2.0 and frames 1280x720/1280x827. Require exact PNG parity with the accepted
correction views under equivalent capture-only framing and visibility. Run full quality,
review/fix the cumulative diff, and obtain owner visual confirmation of the runtime views
before publishing under the existing direct-main authority.

Approval covers this single asset and its bounded visual wiring. Preserve camera, scenario,
dialogue, clues, discovery, treatment, save, provider, UI, audio, other assets, release and
signing. Visual acceptance establishes no pacing, usability, accessibility or gameplay
acceptance. This proposal changes documentation only.
