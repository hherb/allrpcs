# Direction A production-pipeline proof

Status: bounded pre-production source-sheet proof, 2026-09-28. These files are stored outside
`game/`, are not imported, and are not runtime-ready assets.

The recorded failures below are historical evidence from the first sheet workflow. The bounded
[corrected pipeline proof](corrected/README.md) now passes clean-alpha, seam, manifest, layered-master,
grayscale-cue, navigation-clearance, and four-direction walk, idle, and interaction checks. Its exact
exports now include manifest-bound 90-degree and live-angle opening-junction path modules, and all
have an isolated Godot import proof; neither proof is referenced by ordinary gameplay.

Phase 9.1's approved painterly storybook direction was decomposed into four deliberately small
source sheets:

- [Terrain and path modules](sources/terrain-path-modules.png)
- [Herb-bed and foliage modules](sources/herb-bed-foliage-modules.png)
- [Preparation-table state cues](sources/preparation-table-state-cues.png)
- [Seeker four-direction walk sample](sources/seeker-four-direction-walk.png)

The sheets use only the approved representative-scene refinement as a visual reference. They test
whether the chosen language can be decomposed into modular terrain, reusable plants, stable object
states, and directional animation without expanding into opening-region production.

## Inspection results

| Sheet | What succeeded | What blocks runtime use |
| --- | --- | --- |
| Terrain/path | Six distinct quiet-ground, dirt, straight, bend, junction, and transition concepts share palette and path width closely enough to support a modular approach | Coloured low-alpha gutter haze remains; exact opposite-edge continuity and export rectangles are not proven |
| Herb bed/foliage | Six isolated concepts include empty/planted beds and four readable reusable plant clusters with preserved leaf alpha | Soft coloured halos remain; module rectangles, contact baselines, and pivots are not normalized |
| Preparation table | Four table treatments keep the same overall silhouette; ordinary, noticeable, identified, and dangerous states use increasingly explicit geometry; hidden remains correctly absent | The repeated table is not pixel-identical, cues are baked into each cell instead of separate overlays, and cell pivots are not normalized |
| Seeker walk | Exactly 24 full-body frames form six-frame south, west, east, and north rows with a stable identity, costume, palette, and approximate scale | Frame rectangles, foot baselines, pivots, and loop timing are not mechanically normalized; this is a motion/style sample rather than a sprite atlas |

All four PNG files have RGBA channels with non-opaque pixels. Alpha presence alone is not clean-alpha
evidence: the terrain and plant sheets visibly retain soft environmental colour in nominal gutters
and peak below fully opaque alpha, while the table and Seeker sheets do reach full opacity.

## Feasibility conclusion

Direction A remains feasible. The proof demonstrates a coherent decomposition and a plausible
four-direction character treatment, so there is no Direction D fallback trigger. It does not yet
pass the art bible's editable-master, seam, alpha, pivot, or animation-baseline gates.

The problem is the flattened sheet workflow, not the selected art language. No generated sheet may
be copied into `game/` or described as production-ready.

## Completed bounded correction pass

The correction pass completed these pipeline tasks rather than generating more scene art:

1. Produce individual clean-alpha source files for the minimum terrain/path, herb-bed/foliage,
   preparation-table, and cue-overlay modules instead of relying on sheet gutters.
2. Retain or create editable layered masters, then export deterministic flattened PNG files.
3. Define explicit logical rectangles, 2x master dimensions, contact baselines, and pivots in one
   asset manifest.
4. Recompose one 960x540 external review scene from only those modules and check seams, grayscale
   cues, and navigation clearance.
5. Normalize one six-frame direction first, then extend the same measured baseline and timing to all
   four directions only after the first loop passed.

The corrected exports passed before their isolated import proof was added. Walk, idle/breathe, and
generic reach-and-inspect animations now remain proof-only resources in that scene. Do not connect
them to ordinary gameplay, bulk-produce the opening region, change gameplay/UI behaviour, or begin
audio, release, or signing work.

## Provenance and integrity

- Created with Codex's built-in image-generation tool on 2026-09-28 using
  `../representative-scene/previews/selected-direction-a-refinement.png` as the only image reference.
- The tool did not expose a model/version identifier, so none is asserted.
- No third-party image, asset, logo, trademark, or style-reference image was used.
- Exact prompts are preserved in [PROMPTS.md](PROMPTS.md).

| File | Pixels | SHA-256 |
| --- | ---: | --- |
| `sources/terrain-path-modules.png` | 1536x1024 | `4a424c3368374d191cec97bf58b6d0875ecda764598042d043a463980b6ee18e` |
| `sources/herb-bed-foliage-modules.png` | 1536x1024 | `e882e049f9230facc7b1bf8de99b5e1a28bf918c56fbd2eba34f5ed816aece59` |
| `sources/preparation-table-state-cues.png` | 2159x728 | `8d78f6c3feb75963744392d3e509413624eb4f77685e2772cefd47443a1f7c73` |
| `sources/seeker-four-direction-walk.png` | 1491x1055 | `d20a45e1f19e4f49c347f27903a3e0ee2566fe8cfc0890cd2589f8361be1aad9` |
