# Corrected proof image-generation prompts

The first five calls used Codex's built-in image-generation tool on 2026-09-28. Each supplied project
art was an edit/style input, not a third-party reference. The tool did not expose a model or version
identifier, so none is asserted. Generated files are retained under `sources/imagegen`; the build
script performs the deterministic clean-alpha, sizing, pivot, baseline, layer, and export work.

## Empty herb bed

Input: `../sources/herb-bed-foliage-modules.png`

```text
Use case: background-extraction
Asset type: individual clean-alpha 2D game module source
Primary request: Extract and cleanly reconstruct only the empty raised herb bed from the upper-left cell of the supplied source sheet.
Input image: the supplied herb-bed and foliage sheet is the edit target.
Style/medium: preserve its painterly storybook gouache rendering, exact high-oblique projection, timber and soil palette, lighting direction, proportions, and detail.
Composition: one complete empty rectangular weathered-timber raised bed centered with generous transparent margin.
Constraints: genuinely transparent background with no colored haze or halo; preserve the object edge and contact shadow only; no plants, extra objects, terrain square, text, grid, logo, watermark, or cropped edge.
```

## Broadleaf foliage

Input: `../sources/herb-bed-foliage-modules.png`

```text
Use case: background-extraction
Asset type: individual clean-alpha 2D game module source
Primary request: Extract and cleanly reconstruct only the low broadleaf herb cluster from the upper-right cell of the supplied source sheet.
Input image: the supplied herb-bed and foliage sheet is the edit target.
Style/medium: preserve its painterly storybook gouache rendering, exact high-oblique projection, leaf palette, lighting direction, proportions, and detail.
Composition: one complete low broadleaf herb cluster centered with generous transparent margin and a clear ground contact point.
Constraints: genuinely transparent background with no colored haze or halo between or around leaves; preserve leaf edges and compact contact shadow only; no bed, flowers, terrain square, text, grid, logo, watermark, or cropped leaf.
```

## Preparation table

Input: `../sources/preparation-table-state-cues.png`

```text
Use case: background-extraction
Asset type: individual clean-alpha 2D game interactable source
Primary request: Extract and cleanly reconstruct only the ordinary preparation table from the leftmost cell of the supplied source sheet.
Input image: the supplied table-state sheet is the edit target.
Style/medium: preserve its painterly storybook gouache stone, mortar, pestle, herb sprigs, high-oblique projection, lighting direction, silhouette, proportions, and detail.
Composition: one complete ordinary table centered with generous transparent margin and its compact local contact shadow.
Constraints: genuinely transparent background with no halo; no state cue, contour, tick, diamond, chevron, hatch, label, text, grid, logo, watermark, or cropped edge.
```

## Quiet moss ground

Input: `../sources/terrain-path-modules.png`

```text
Use case: stylized-concept
Asset type: individual seamless 2D game terrain master
Primary request: Create one quiet moss-and-short-grass terrain tile matching the supplied terrain sheet.
Input image: the supplied terrain and path sheet is the visual-style and texture reference.
Style/medium: painterly storybook gouache, broad low-frequency brush shapes, restrained grain, moss #536F3C and leaf #7F9B4E, no focal landmark.
Composition: square orthographic-like top/high-oblique ground texture filling the entire canvas to every edge.
Constraints: exactly seamless on opposite edges, uniform value hierarchy, no path, plant cluster, rock, prop, shadow, object, text, border, logo, trademark, or watermark.
```

## Straight path overlay

Input: `../sources/terrain-path-modules.png`

```text
Use case: background-extraction
Asset type: individual transparent 2D game path overlay master
Primary request: Create one straight worn packed-earth path overlay matching the supplied terrain/path sheet.
Input image: the supplied terrain and path sheet is the visual-style, texture, palette, width, and high-oblique reference.
Style/medium: painterly storybook gouache, broad low-frequency packed-earth shapes, warm ochre and weathered brown, restrained grain.
Composition: a vertical straight path of consistent width crossing the entire square canvas from top edge to bottom edge, centered, with soft blendable lateral edges.
Constraints: genuinely transparent outside the path with no colored environmental haze; top and bottom path edges align exactly for repetition; no moss background, plants, rocks, props, text, border, logo, trademark, or watermark.
```

## Packed-earth path bend

Inputs: `sources/imagegen/quiet-moss-ground.png` and
`sources/imagegen/straight-path-overlay.png`

```text
Use case: stylized-concept
Asset type: external transparent 2D game path-overlay source for a deterministic modular pipeline
Primary request: Create one single 90-degree worn packed-earth path bend. The path enters vertically through the exact center of the top edge, curves smoothly clockwise, and leaves horizontally through the exact center of the right edge.
Input images: Image 1 is the repository-owned quiet-moss palette and painterly texture reference only; Image 2 is the repository-owned straight packed-earth path reference for path palette, width, texture, stone flecks, soft irregular edge treatment, and visual scale.
Scene/backdrop: genuinely transparent outside the path; do not paint moss or any other background.
Style/medium: painterly storybook gouache, broad low-frequency packed-earth shapes, warm ochre and weathered brown, restrained grain, sparse muted stone flecks matching Image 2.
Composition/framing: one complete square module; centered top entry and centered right exit have the same consistent width as the straight-path reference; the inside and outside curve remain smooth and readable; generous transparent outer regions occupy the top-right and bottom-left corners away from the path.
Constraints: one path bend only; preserve transparent alpha outside the path; no colored environmental haze; no moss fill; no plants; no rocks beyond the restrained embedded flecks already characteristic of the path; no props; no border; no text; no logo; no trademark; no watermark; no extra modules; no grid; no shadow outside the path; no cropped curve.
```

This output came from Codex's built-in image tool on 2026-09-28 with transparent-background output
requested. The quiet-moss and straight-path images were the only references. The deterministic
builder owns the exact 512x512 master, 256x256 export, 96-pixel ports, clean alpha, and digest. The
tool exposed no model or version identifier, so none is asserted.

## Packed-earth path T-junction

Inputs: `exports/terrain/quiet-moss.png`, `exports/terrain/straight-path.png`, and
`exports/terrain/path-bend.png`

```text
Use case: stylized-concept
Asset type: external transparent 2D game path-overlay source for a deterministic modular pipeline
Primary request: Create one single T-shaped worn packed-earth path junction. The path enters vertically through the exact center of the top edge, reaches one smooth junction, and leaves horizontally through the exact centers of both the left and right edges.
Input images: Image 1 is the repository-owned quiet-moss palette and painterly texture reference only; Image 2 is the repository-owned straight packed-earth path reference for path palette, width, texture, sparse stone flecks, soft irregular edge treatment, and visual scale; Image 3 is the repository-owned 90-degree bend reference for curve language and clean transparent treatment. Use no other visual references.
Scene/backdrop: genuinely transparent outside the path; do not paint moss or any other background.
Style/medium: painterly storybook gouache, broad low-frequency packed-earth shapes, warm ochre and weathered brown, restrained grain, sparse muted stone flecks matching the path references.
Composition/framing: one complete square module; centered top entry and centered left and right exits each have the same consistent width as the straight-path reference; the junction shoulders are smooth and readable; generous transparent outer regions occupy both lower corners and the full bottom edge away from the path.
Constraints: one T-junction only; preserve genuine transparent alpha outside the path; no bottom exit; no cross-junction; no colored environmental haze; no moss fill; no plants; no rocks beyond restrained embedded flecks already characteristic of the path; no props; no border; no text; no logo; no trademark; no watermark; no extra modules; no grid; no shadow outside the path; no cropped junction shoulders.
```

This output came from Codex's built-in image tool on 2026-09-28 with transparent-background output
requested. The quiet-moss, straight-path, and path-bend exports were the only references. The
deterministic builder owns the exact 512x512 master, 256x256 export, three 96-pixel ports, clean
alpha, and digest. The tool exposed no model or version identifier, so none is asserted.

## Packed-earth path cross-junction

Inputs: `exports/terrain/quiet-moss.png`, `exports/terrain/straight-path.png`,
`exports/terrain/path-bend.png`, and `exports/terrain/path-t-junction.png`

```text
Use case: stylized-concept
Asset type: external transparent 2D game path-overlay source for a deterministic modular pipeline
Primary request: Create one single cross-shaped worn packed-earth path junction. The path enters vertically through the exact centers of both the top and bottom edges and horizontally through the exact centers of both the left and right edges, meeting in one smooth four-way junction.
Input images: Image 1 is the repository-owned quiet-moss palette and painterly texture reference only; Image 2 is the repository-owned straight packed-earth path reference for path palette, width, texture, sparse stone flecks, soft irregular edge treatment, and visual scale; Image 3 is the repository-owned 90-degree bend reference for curve language and clean transparent treatment; Image 4 is the repository-owned T-junction reference for junction shoulder language and clean transparent treatment. Use no other visual references.
Scene/backdrop: genuinely transparent outside the path; do not paint moss or any other background.
Style/medium: painterly storybook gouache, broad low-frequency packed-earth shapes, warm ochre and weathered brown, restrained grain, sparse muted stone flecks matching the path references.
Composition/framing: one complete square module; centered top, bottom, left, and right entries each have the same consistent width as the straight-path reference; the four junction shoulders are smooth and readable; generous transparent regions occupy all four outer corners.
Constraints: one cross-junction only; preserve genuine transparent alpha outside the path; exactly four centered exits; no T-junction; no colored environmental haze; no moss fill; no plants; no rocks beyond restrained embedded flecks already characteristic of the path; no props; no border; no text; no logo; no trademark; no watermark; no extra modules; no grid; no shadow outside the path; no cropped junction shoulders.
```

This output came from Codex's built-in image tool on 2026-09-28 with transparent-background output
requested. The quiet-moss, straight-path, path-bend, and path-T-junction exports were the only
references. The deterministic builder owns the exact 512x512 master, 256x256 export, four 96-pixel
ports, clean alpha, and digest. The tool exposed no model or version identifier, so none is
asserted.

## Packed-earth path end-cap

Inputs: `exports/terrain/quiet-moss.png`, `exports/terrain/straight-path.png`,
`exports/terrain/path-bend.png`, `exports/terrain/path-t-junction.png`, and
`exports/terrain/path-cross-junction.png`

```text
Use case: stylized-concept
Asset type: external transparent 2D game path-overlay source for a deterministic modular pipeline
Primary request: Create one single worn packed-earth path end-cap. The path enters vertically through the exact center of the top edge, continues downward, and ends in one soft rounded naturally worn-earth terminus before the lower half of the square module.
Input images: Image 1 is the repository-owned quiet-moss palette and painterly texture reference only; Image 2 is the repository-owned straight packed-earth path reference for path palette, width, texture, sparse stone flecks, soft irregular edge treatment, and visual scale; Image 3 is the repository-owned 90-degree bend reference for curve language and clean transparent treatment; Image 4 is the repository-owned T-junction reference for shoulder language and clean transparent treatment; Image 5 is the repository-owned cross-junction reference for surface continuity and clean transparent treatment. Use no other visual references.
Scene/backdrop: genuinely transparent outside the path; do not paint moss or any other background.
Style/medium: painterly storybook gouache, broad low-frequency packed-earth shapes, warm ochre and weathered brown, restrained grain, sparse muted stone flecks matching the path references.
Composition/framing: one complete square module; one centered top entry has the same consistent width as the straight-path reference; it descends less than halfway into the square, then resolves into a smooth softly rounded worn terminus; generous transparent regions occupy both sides, the lower half, and the entire bottom edge.
Constraints: one end-cap only; preserve genuine transparent alpha outside the path; exactly one centered top entry; no bottom, left, or right exit; no junction; no colored environmental haze; no moss fill; no plants; no rocks beyond restrained embedded flecks already characteristic of the path; no props; no border; no text; no logo; no trademark; no watermark; no extra modules; no grid; no shadow outside the path; no cropped terminus.
```

This output came from Codex's built-in image tool on 2026-09-28 with transparent-background output
requested. The quiet-moss, straight-path, path-bend, path-T-junction, and path-cross-junction
exports were the only references. The deterministic builder owns the exact 512x512 master,
256x256 export, one 96-pixel top port, clean alpha, transparent lower half, and digest. The tool
exposed no model or version identifier, so none is asserted.

## Stream–Laundry–Woodland opening junction

Inputs: `exports/terrain/straight-path.png` and the approved live anchor geometry in
`game/OpeningRegionLayoutGeometry.cs`

No image-generation prompt was used. The deterministic builder composes the repository-owned
straight-path texture into a 512x512 junction export and 1024x1024 editable master. Its exact
96-pixel incoming port follows canonical heading `90°`; its outgoing port follows `32.400718°`,
which preserves the live `-57.599282°` turn under rigid placement. The builder also records the
336x458 visible footprint and generates the seam/containment review. No external visual source was
introduced.

## Woodland leaf-litter ground

Inputs: `exports/terrain/quiet-moss.png` and
`review/path-module-topology-review.png`

```text
Use case: style-transfer
Asset type: external opaque 2D game ground-texture source for a deterministic modular pipeline
Input images: Image 1 is the edit target and repository-owned quiet-moss tile; Image 2 is a repository-owned packed-earth path-set reference only for palette compatibility, visual scale, restrained gouache grain, and contrast.
Primary request: Transform the ground material in Image 1 into woodland leaf litter while preserving its top-down/high-oblique game-texture projection, scale, painterly storybook gouache rendering, broad low-frequency texture rhythm, and full square coverage. Replace the green moss clusters with muted brown woodland leaf litter: weathered oak-brown and umber leaves over subdued earth, with restrained olive remnants and sparse softly painted leaf silhouettes. It must read clearly as leaf litter beside the quiet moss and remain darker and less path-like than the warm ochre packed earth in Image 2.
Style/medium: painterly storybook gouache; broad low-frequency shapes; restrained grain; softly simplified leaf forms; no photorealism.
Composition/framing: one full square ground texture with useful detail distributed across the entire canvas; no single focal leaf cluster, no directional flow, no central clearing, no corridor, no trail.
Constraints: change only the ground material; keep the projection, scale, full coverage, and painterly treatment; opaque edge-to-edge visual content; no transparency; no path; no corridor; no bare-earth stripe; no border; no grid; no scene framing; no rocks; no logs; no plants taller than flat ground cover; no props; no animals; no text; no logo; no trademark; no watermark.
Avoid: obvious repeated stamps, strong diagonal bands, radial composition, bright orange path colors, high-frequency photographic noise, sharp isolated hero leaves, lighting gradients, shadows that imply raised objects.
```

This output came from Codex's built-in image tool on 2026-09-28 with opaque output requested. The
quiet-moss export was the edit target, and the repository-owned packed-earth topology review was the
only other reference. The deterministic builder owns the exact 512x512 master, 256x256 export,
opposing-edge parity, opacity, review composition, digest, and derived quiet-moss transition. The
tool exposed no model or version identifier, so none is asserted.

## Seeker idle/breathe frames

Input for all four calls: `../../sources/seeker-four-direction-walk.png`

The following prompt was called for west, east, and north. The bracketed clauses were replaced with
the exact direction-specific values listed below; all other text was identical.

```text
Use case: precise-object-edit
Asset type: external two-frame 2D game animation source sheet
Primary request: Derive exactly two [direction]-facing idle/breathe frames of the same Seeker shown in the supplied four-direction walk sheet. Frame 1 is a relaxed neutral standing pose; frame 2 is the same pose with only a subtle natural inhale: shoulders and cloak rise slightly, with no step and both boots planted in exactly the same place.
Input image: the supplied Seeker four-direction walk sheet is the identity, costume, palette, projection, scale, and painterly-style reference.
Style/medium: preserve the painterly storybook gouache character rendering, high-oblique [view], brown hair, cream shirt, dark trousers, brown boots and satchel, teal cloak, lighting direction, proportions, and detail; [costume clause].
Composition: exactly two complete full-body frames in one horizontal two-cell sheet, neutral frame on the left and inhale frame on the right, centered separately with generous transparent margins and equal ground-contact baselines.
Constraints: genuinely transparent background; same character identity and costume in both cells; fixed feet and body position; subtle breathing motion only; no walking stride, turn, gesture, prop change, facial redesign, shadow haze, labels, text, grid, border, logo, trademark, watermark, extra frame, duplicate character within a cell, or cropped edge.
```

| Direction | View | Costume clause |
| --- | --- | --- |
| west | left-facing side profile | satchel visible on the near side and teal cloak trailing behind the torso |
| east | right-facing side profile | satchel and cloak placement consistent with the supplied east-facing walk row, without mechanically mirroring story details |
| north | north-facing back view | teal cloak and rear satchel silhouette matching the supplied north-facing walk row |

The south call used the same prompt with `south-facing view` and ended the style line after
`detail`, without the semicolon/costume-clause suffix. All four outputs came from Codex's built-in
image tool on 2026-09-28. The tool exposed no model or version identifier, so none is asserted.

## Seeker reach-and-inspect interaction frames

Input for all four calls: `review/seeker-four-direction-walk.png`

Each call used the following exact shared structure. The direction-specific clauses are retained in
the table below; no other image reference was supplied.

```text
Use case: stylized-concept
Asset type: external four-frame 2D game interaction-animation source sheet
Primary request: Derive exactly four [direction]-facing frames of one reusable reach-and-inspect interaction gesture for the same Seeker shown in the supplied walk sheet. Frame order from left to right: 1) brief anticipation with torso settling and [start clause]; 2) reach forward and slightly downward toward an imaginary object directly [target]; 3) clear inspect/contact pose with [contact clause] and the head inclined toward the imaginary object; 4) partial recovery toward neutral with the hand withdrawing. Keep every frame visibly distinct.[readability clause]
Input image: the supplied Seeker walk sheet is a reference only for identity, costume, palette, projection, scale, painterly style, proportions, and [direction]-facing design; do not edit or reproduce the full sheet.
Style/medium: [style clause]
Composition: exactly four complete full-body frames in one horizontal four-cell sheet, ordered left to right, each centered separately with generous transparent gutters and the same ground-contact baseline. The imaginary interaction origin [origin clause].
Constraints: genuinely transparent background; same character identity, costume, handedness, scale, pivot, [placement clause] in all four cells; both boots remain planted in exactly the same place; upper-body and arm gesture only; no held prop and no depicted target object; no locomotion, turn, attack, harvesting, casting, speech bubble, state marker, contact shadow haze, labels, text, grid, border, logo, trademark, watermark, extra frame, duplicate character within a cell, or cropped edge.
Avoid: [avoid clause]
```

| Direction | Start/contact and readability clauses | Exact remaining substitutions |
| --- | --- | --- |
| south | `hands beginning to move`; `the right hand lowest`; no added readability clause | target `in front of the character`; style `preserve the painterly storybook gouache character rendering, high-oblique south-facing view, brown hair, cream shirt, dark trousers, brown boots and satchel, teal cloak, warm lighting direction, roughly 4.5-to-5-head proportions, and simplified readable silhouette`; origin `remains centered directly in front of the Seeker`; placement `and contact-foot position`; avoid `walking stride, kneeling, tool use, weapon use, dramatic pose, costume drift, changing satchel side, changing cloak length, moving feet, camera changes, perspective convergence, environmental backdrop, blurry edges, or duplicated frames.` |
| west | `the forward hand beginning to move`; `the forward hand farthest left and lowest`; no added readability clause | target `west/left of the character`; style `preserve the painterly storybook gouache character rendering, high-oblique west-facing left side profile, brown hair, cream shirt, dark trousers, brown boots, satchel visible on the near side, teal cloak trailing behind the torso, warm lighting direction, roughly 4.5-to-5-head proportions, and simplified readable silhouette`; origin `remains directly west/left in front of the Seeker`; placement `satchel side, and contact-foot position`; avoid `walking stride, kneeling, tool use, weapon use, dramatic pose, costume drift, mechanical mirroring, changing satchel side, changing cloak length, moving feet, camera changes, perspective convergence, environmental backdrop, blurry edges, or duplicated frames.` |
| east | `the forward hand beginning to move`; `the forward hand farthest right and lowest`; no added readability clause | target `east/right of the character`; style `preserve the painterly storybook gouache character rendering, high-oblique east-facing right side profile, brown hair, cream shirt, dark trousers, brown boots, satchel and cloak placement consistent with the supplied east-facing walk row without mechanically mirroring story details, warm lighting direction, roughly 4.5-to-5-head proportions, and simplified readable silhouette`; origin `remains directly east/right in front of the Seeker`; placement `satchel side, and contact-foot position`; avoid `walking stride, kneeling, tool use, weapon use, dramatic pose, costume drift, mechanical mirroring, changing satchel side, changing cloak length, moving feet, camera changes, perspective convergence, environmental backdrop, blurry edges, or duplicated frames.` |
| north | `the forward hand beginning to move`; `the forward hand farthest north`; `, using shoulder, elbow, cloak, and head silhouette changes that remain readable from behind` | target `north/above the character`; style `preserve the painterly storybook gouache character rendering, high-oblique north-facing back view, brown hair, cream shirt accents, dark trousers, brown boots, teal cloak and rear satchel silhouette matching the supplied north-facing walk row, warm lighting direction, roughly 4.5-to-5-head proportions, and simplified readable silhouette`; origin `remains directly north/above in front of the Seeker`; placement `satchel side, and contact-foot position`; avoid `walking stride, kneeling, tool use, weapon use, dramatic pose, costume drift, front-facing turn, changing satchel side, changing cloak length, moving feet, camera changes, perspective convergence, environmental backdrop, blurry edges, or duplicated frames.` |

All four outputs came from Codex's built-in image tool on 2026-09-28 with transparent-background
output requested. The tool exposed no model or version identifier, so none is asserted.
