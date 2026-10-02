# Proposed Shrine–Southern Road production/runtime promotion

Status: proposed on 2026-10-02; separate owner approval is pending. The accepted
[endpoint blend](shrine-southern-road-path-blend.md) supplies the exact presentation.
Approval of this scope would authorize only the two assets and wiring described below.

## Exact assets and placement

Keep two independently imported textures under one `ShrineSouthernRoadPath` presentation
owner beneath `GreyboxNavigation/RoutePresentation`. The owner has identity transform and
z=0; its children retain the accepted absolute draw ordering. Do not flatten the layers.

| Proposed manifest ID | Export/runtime filename | Export size | World origin | Sprite center | z |
| --- | --- | --- | --- | --- | --- |
| `terrain.shrine_southern_road_path` | `shrine-southern-road-path.png` | 1152x1080 | (432,400) | (1008,940) | -72 |
| `terrain.shrine_southern_road_endpoint_blend` | `shrine-southern-road-endpoint-blend.png` | 160x160 | (1400,1280) | (1480,1360) | -69 |

The first export must match `shrine-southern-road-path-correction-overlay.png`, SHA-256
`0b73ae24f5f0c985b29399e2889dd12210ec4f221bc827a4cae7da60b635b61e`.
The second must match `shrine-southern-road-path-blend-overlay.png`, SHA-256
`a01cb5089d67a9e9bf108a2dd18a84b0f98b11c6eb7e7f3d2b3d5844bbabc720`.
Preserve the accepted slight triangle, transparent gutters, clean alpha, source sampling,
shrine-entry clipping and all 3,354 visible patch pixels. No repaint or further trim.

## Production and runtime changes after approval

Extend the corrected production builder and manifest with both named assets, exact source
recipe/proof provenance and centered pivots. Use the retained correction/blend recipes;
do not modify either recipe or its pinned evidence. Add editable 2x OpenRaster masters
(2304x2160 and 320x320) to the existing terrain master/export pipeline, preserving exact
export pixels and existing masters/exports. Export into `exports/terrain/`, then copy only
these two manifest-bound PNGs to `game/art/runtime/terrain/` with independent lossless
Godot imports and the existing runtime terrain filtering convention.

Wire the two textures through `game/Main.tscn` and the existing `game/GreyboxNavigation.cs`
presentation owner. Replace only the visual presentation of the exact 96px Shrine–Road
route `(380,300)` → `(1500,1400)` with the two-child owner. Hide only the upstream brown
`OpeningRegionSouthernRoadWagonSceneSource/RoadArea` fill `(572,460)` → `(1500,1400)`,
width 96. Preserve its points, width and ordinary physical ownership. Retain startup
validation of the exact route and texture geometry, following existing terrain promotions.

Existing Square–Southern Road and Southern Road–Wagon sprites, texture bytes, transforms
and layers remain intact. The local patch overlays their crossing exactly as accepted.
Shrine shell/opening marker, labels, all outlines/collisions, navigation, camera and every
other accepted runtime asset remain intact. Proof-only hiding of the Seeker/HUD and review
framing must never enter ordinary startup.

## Verification and completion boundary

Add focused production/runtime validation for both assets, master/export/runtime hash
parity, clean alpha, geometry, draw order, independent lossless imports and the restricted
presentation changes. Preserve all archived proof PNGs, captures, recipes and evidence
records. Existing Shrine validators must continue validating archived evidence without
rewriting their frozen 417-file baseline to accommodate promotion. Their optional
`--check-live-baseline` is pre-promotion evidence only.

Capture the ordinary runtime presentation at the same three targets (midpoint, shrine
exit, junction), three zooms (0.8x/1.0x/2.0x), and two frames (1280x720/1280x827).
Use separate runtime capture files; compare all 18 PNGs with the accepted blend views
under equivalent capture-only Seeker/HUD visibility and framing. Run `./scripts/quality.sh`,
review the cumulative diff, and obtain owner visual confirmation of the resulting runtime
views before publishing the promoted runtime slice. Direct-main commit/push authority
covers reviewed work; this proposal itself changes documentation only.

Approval does not cover navigation, camera, scenario, dialogue, clues, discovery, treatment,
save, provider, UI, audio, another asset, release or signing. It establishes no pacing,
usability, accessibility or gameplay acceptance. After runtime visual acceptance, leave
one next bounded proposal in `HANDOVER.md`; do not infer the next asset's approval.
