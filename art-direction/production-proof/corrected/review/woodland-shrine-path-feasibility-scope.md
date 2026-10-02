# Proposed Woodland Clearing–Shrine external path feasibility

Status: proposed on 2026-10-02; named scope approval is pending. This first test answers
whether the existing straight-path texture can present the retained route while preserving
both physical openings and every accepted adjacent asset. It does not authorize a correction
or production/runtime promotion.

## Retained geometry and known risk

The exact 96px route runs from Woodland Clearing `(1000,350)` to Old Shrine `(380,300)`;
length is 622.012862px and heading is -175.389351 degrees. The woodland's west boundary is
x=600; its Shrine opening is derived from the route at `(600,317.741935)`. The shrine's east
boundary is x=620; its Woodland Clearing opening remains centered at `(620,300)`.
Both openings are 96px high, with 8px wall thickness and markers extending 8px to either
side of the boundary. The two area bounds overlap
by 20px in x; this is retained geometry, not a proposed separation or relocation.

The route intersects the shrine boundary at `(620,319.354839)`, 19.354839px below its
opening center. A full nominal 96px strip has a 96.311669px vertical cross-section at this
heading, so it cannot fit exactly inside the unchanged shrine opening. The woodland
intersection is centered on its opening but has the same small projected width excess.
These are geometric risks to inspect, not a rendered failure or permission to alter physics.

## Exact disposable proof

Use only the retained runtime `game/art/runtime/terrain/straight-path.png` (96x256), SHA-256
`3d12ffdfb32fbbda2e9d0c2029803691a4eb4f8484a4ee5245a729f2fc8278bd`, and the unchanged
shared `render_path_leg` sampler. Repeat the source along the finite route with sampling
phase starting at `(1000,350)`; retain the exact route width and heading. No source repaint,
scale change, new endpoint cap, bend, clipping correction or seam patch in this first test.
Keep zero-alpha RGB clean without changing visible sampled pixels.

Render into one padded RGBA review overlay: world origin `(352,224)`, canvas 704x208,
center `(704,328)`. In a disposable Main instance, hide only the exact two-point 96px
Woodland Clearing–Shrine route presentation line, requiring exactly one match. Add the
review overlay beneath `GreyboxNavigation/RoutePresentation` at z=-74. It draws above
ordinary route lines (z=-75) and woodland terrain, while remaining below all accepted path
sprites. Never save/pack the modified Main instance.

Leave ShrineArea, both gold opening markers, shrine/woodland shells, labels, every outline,
collision, physical route, and accepted Laundry–Clearing/end-cap, Shrine–Road and other
terrain nodes/textures unchanged. Inspect any shell tinting, endpoint overlap, gap or fringe
as evidence. Do not hide additional presentation to disguise a failed fit. Only capture-only
Seeker/HUD visibility and camera framing may change; retain ordinary camera world limits.

## Views, evidence and completion boundary

Capture 18 review views: three targets at 0.8x, 1.0x and 2.0x, each in 1280x720 and 1280x827:

| Target | Requested Seeker/camera position | Purpose |
| --- | --- | --- |
| Midpoint | (690,325) | Route continuity and the overlapping area bounds |
| Clearing junction | (1000,350) | Join to the accepted Laundry–Clearing endpoint presentation |
| Shrine entry | (620,319.354839) | Offset against the unchanged gold opening and shell |

Low zoom and near-world-edge targets remain clamped by ordinary camera limits. Document
requested targets and effective clamping, never alter the camera to force framing.

Save separate `woodland-shrine-path-*` review PNGs, a deterministic recipe, capture harness,
and pinned evidence/validator. Pin source bytes, shared renderer, exact route/opening
geometry, projected review footprint and all capture hashes. Snapshot the current runtime,
physical, import, production and manifest baseline; all must remain unchanged. Register
archived proof validation in regular quality without freezing future approved promotions.
Run focused validation and `./scripts/quality.sh`, review the cumulative diff, and present
all views with an honest feasibility verdict. Retain failed evidence if the strip fails.
Any correction needs its own bounded scope; named promotion needs accepted external views
and separate approval. Direct-main publication covers reviewed, authorized proof work.

Preserve navigation, camera, scenario, dialogue, clues, discovery, treatment, save, provider,
UI, audio, release and signing. No pacing, usability, accessibility, gameplay or next-asset
acceptance is inferred. This proposal changes documentation only; generating this proof
requires owner approval of this exact named scope.
