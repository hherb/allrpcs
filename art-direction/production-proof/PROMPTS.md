# Direction A production-proof prompts

All four calls used Codex's built-in image-generation tool with a transparent background request.
Each used `../representative-scene/previews/selected-direction-a-refinement.png` as Image 1, a visual
reference rather than an edit target.

## Terrain and path modules

```text
Use case: stylized-concept
Asset type: transparent source sheet for a modular 2D game terrain/path production proof
Primary request: Create a clean six-module terrain and path source sheet matching Allrpcs selected Direction A.
Input images: Image 1 is the approved visual-style, palette, brushwork, lighting-direction, and high-oblique projection reference only.
Subject: six isolated equal-footprint square modules arranged on an invisible 3-by-2 grid with generous transparent gutters: quiet moss ground; worn packed-earth ground; straight packed-earth path through moss; ninety-degree path bend; T path junction; soft path-to-ground transition edge.
Style/medium: painterly storybook gouache, broad low-frequency brush shapes, warm late-afternoon material response, restrained grain, no photorealism, no black outlines.
Projection: orthographic-like high-oblique 2D terrain, consistent scale and orientation across every module, no horizon, no perspective convergence.
Modularity: outer ground edges and corresponding path widths must align visually; no unique landmarks; no object shadows; modules must be recomposable without a monolithic background.
Color palette: moss #536F3C, leaf #7F9B4E, weathered brown #6E4B2F, ochre #D2A84D, restrained cream highlights #E8D8B8.
Constraints: genuinely transparent canvas outside the six module squares; exactly six modules; no plants, rocks, fences, buildings, characters, UI, labels, text, grid lines, logos, trademarks, or watermark.
Avoid: perspective scenes, decorative focal objects, hard tile borders, obvious repeated stamps, baked global lighting, or noisy grass detail.
```

## Herb-bed and foliage modules

```text
Use case: stylized-concept
Asset type: transparent source sheet for modular 2D game herb-bed and foliage production proof
Primary request: Create a clean six-module plant source sheet matching Allrpcs selected Direction A.
Input images: Image 1 is the approved visual-style, palette, brushwork, lighting-direction, herb-bed design, and high-oblique projection reference only.
Subject: six isolated modules arranged on an invisible 3-by-2 grid with generous transparent gutters: one empty rectangular weathered-timber raised bed; the same bed with restrained mixed medicinal herbs; one low broadleaf herb cluster; one upright purple-flowering herb cluster; one small cream-flowering herb cluster; one low quiet grass/leaf edge cluster.
Style/medium: painterly storybook gouache, simplified readable leaf masses, selective warm-dark one-to-two-pixel-equivalent edges, broad shapes before detail, no photorealism.
Projection: orthographic-like high-oblique 2D, consistent orientation and scale with the approved scene; each plant cluster has a clear base/contact point.
Lighting/mood: warm late-afternoon side light with compact local contact shadows only.
Color palette: moss #536F3C, leaf #7F9B4E, weathered brown #6E4B2F, ochre #D2A84D, cream #E8D8B8, restrained purple accent.
Constraints: genuinely transparent background; exactly six isolated modules; no terrain squares, path, building, character, table, pots, labels, text, grid lines, logo, trademark, or watermark; preserve alpha around leaves.
Avoid: dense one-off scenery, identical cloned leaf stamps, cropped modules, translucent halos, excessive fine grass, bloom, or cinematic depth of field.
```

## Preparation table and state cues

```text
Use case: stylized-concept
Asset type: transparent source sheet for one 2D game interactable and non-colour state-cue production proof
Primary request: Create one preparation-table source and four exact visual-state treatments matching Allrpcs selected Direction A.
Input images: Image 1 is the approved visual-style, palette, brushwork, high-oblique projection, stone preparation-table design, and focus-cue reference only.
Subject: four cells arranged in one horizontal row with generous transparent gutters, each containing the same exact waist-high square stone herb-preparation table with the same mortar, pestle, and three herb sprigs: ordinary has only material silhouette and contact shadow; noticeable adds one short ground tick plus a local value edge; identified/focused adds a warm-ivory double contour plus a separate solid diamond marker; dangerous adds a stable triangular chevron plus diagonal hatch overlay. Hidden is intentionally absent from this sheet because hidden state renders no object.
Style/medium: painterly storybook gouache, simplified stone blocks, selective warm-dark edges, crisp geometric cue overlays, no photorealism.
Projection: orthographic-like high-oblique 2D, identical scale, orientation, silhouette, contents, pivot, and contact point in all four cells.
Color palette: stone cream #E8D8B8, weathered brown #6E4B2F, focus ivory #F7E3A1, danger brick #A84432 with dark hatch; all state differences must remain recognizable in grayscale.
Constraints: genuinely transparent background; exactly four table cells; the table must not change between cells; cue shapes remain outside the core silhouette; no labels, text, cell borders, grid, environment, character, extra prop, glow, sparkle, logo, trademark, or watermark.
Avoid: hue-only state differences, changing camera angle, changing object contents, noisy effects, bloom, opacity-only cues, or cropped markers.
```

## Seeker four-direction walk

```text
Use case: stylized-concept
Asset type: transparent four-direction character walk-cycle source sheet for a 2D game production proof
Primary request: Create one consistent Seeker walk-cycle proof matching Allrpcs selected Direction A.
Input images: Image 1 is the approved Seeker identity, painterly storybook style, palette, lighting, proportions, and high-oblique world reference only.
Subject: the same exact Seeker in a teal travel cloak, cream shirt, dark trousers, boots, and small weathered-brown satchel; roughly 4.5 to 5 heads tall. Arrange exactly 24 full-body frames on an invisible 6-column by 4-row grid with generous transparent gutters. Row order: facing down/south, facing left/west, facing right/east, facing up/north. Each row is one coherent six-frame walk loop: contact, down, passing, contact, down, passing.
Style/medium: painterly storybook gouache 2D character sprites, simplified readable silhouette, selective warm-dark edges, broad shapes with minimal facial detail, no photorealism.
Scale/pivot: every frame represents a 48-to-56-logical-pixel-tall character at 960x540; identical canvas footprint, fixed foot baseline and collision-foot midpoint; no bobbing of the pivot.
Lighting: neutralized warm late-afternoon key consistent in every direction and frame; compact self-contained contact shadow only if identical in every cell.
Constraints: genuinely transparent background; exactly four directional rows and six frames per row; same identity, costume, body proportions, satchel side, palette, scale, baseline, and brush treatment throughout; no weapon, extra pose, labels, text, grid lines, environment, UI, logo, trademark, or watermark.
Avoid: perspective turntable, duplicated identical frames, changing costume, changing handedness, drifting scale, cropped cloak or feet, cinematic rendering, heavy texture noise, or blurry edges.
```
