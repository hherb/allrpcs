# Corrected proof image-generation prompts

These five calls used Codex's built-in image-generation tool on 2026-09-28. Each supplied project
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
