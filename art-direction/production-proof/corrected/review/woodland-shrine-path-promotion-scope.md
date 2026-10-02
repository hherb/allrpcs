# Proposed Woodland–Shrine production/runtime promotion

Status: owner-approved on 2026-10-02 ("approved"); runtime views were separately accepted on 2026-10-02 ("accepted"). The owner accepted the
[junction-corrected external views](woodland-shrine-path-junction.md) on 2026-10-02
("Acceptable"). Routine external-proof scope approval is waived; this proposal covers the
separate production/runtime boundary retained in the standing authorization.

## Exact asset and placement

Promote one manifest-bound `terrain.woodland_shrine_path` asset, exported as
`exports/terrain/woodland-shrine-path.png` and copied to
`game/art/runtime/terrain/woodland-shrine-path.png`. Its 704x208 PNG must exactly match
`woodland-shrine-path-junction-overlay.png`, SHA-256 `dd38db38f7a11d8d621ac938e822650c3b5a5d1be9ea190e60e39d101906d18c`.
World origin `(352,224)`, centered pivot, sprite center `(704,328)`, z=-74, identity scale
and rotation must match the accepted external presentation. Retain the shrine shell/opening
clip and 24px Clearing extension without repainting or resampling the accepted export.

Extend the existing production builder/manifest with the retained junction recipe and proof
provenance. Add an editable 1408x416 OpenRaster master using the established 2x pipeline,
with exact accepted export pixels and no changes to existing masters or exports. Copy only
the new manifest-bound PNG to runtime, with an independent lossless, mipmap-free Godot import
and existing terrain filtering convention. Expected totals: 80 exports and 31 masters.

Wire the texture through `Main.tscn` and `GreyboxNavigation.cs`. Replace only the exact 96px
Woodland–Shrine visual line `(1000,350)` → `(380,300)` with the named sprite. Validate the
exact route, source texture geometry and placement at startup following existing promotions.
Preserve all route points, physical/navigation ownership, shrine shell/markers, Herb Patch,
Stone Marker, labels and every accepted adjacent sprite, including Laundry's path/end cap.
Keep proof framing and Seeker/HUD hiding outside ordinary startup.

## Validation and completion boundary

Add focused master/export/runtime parity, alpha, geometry, draw-order, independent import,
restricted presentation-scope and actual runtime-tree checks. Preserve all three external
proof stages, recipes, capture scripts, evidence records and the frozen 432-file baseline.
Their default validators continue checking archived evidence; optional live-baseline checks
remain pre-promotion checks and must not be rewritten to accommodate the promotion.

Capture 18 separate runtime views at midpoint, Clearing junction and shrine entry, zooms
0.8x/1.0x/2.0x and frames 1280x720/1280x827. Require exact PNG parity with the accepted junction
views under equivalent capture-only framing and visibility. Run full `./scripts/quality.sh`,
review/fix the cumulative diff, and obtain owner visual confirmation of runtime views before
publishing the promoted slice under existing direct-main authority.

Approval covers this one asset and its bounded visual wiring only. Preserve camera,
scenario, dialogue, clues, discovery, treatment, save, provider, UI, audio, release/signing
and all other assets. Visual acceptance establishes no pacing, usability, accessibility or
gameplay acceptance. This proposal changes documentation only.
