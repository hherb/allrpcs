# Direction A corrected production-pipeline proof

Status: bounded corrected proof, 2026-09-28. This directory remains outside `game/`; exact copies
are imported only by the isolated proof scene and none is runtime-authoritative.

This pass corrects the sheet-dependent failures recorded by the first Phase 9.2 proof. Thirteen
individual painterly inputs are retained under `sources/imagegen`; deterministic Python tooling
turns them and the earlier Seeker source sheet into individual exports, exact geometry, editable
OpenRaster masters, and review evidence.

## Result

- `asset-manifest.json` contains all 56 asset IDs, logical/export rectangles, exact 2x master
  dimensions, contact baselines, pivots, hashes, animation order/timing, review placements, and the
  96-pixel navigation-clearance corridor. Every entry is included in the isolated import proof.
- `exports` contains one clean-alpha file per module: quiet moss, straight path, empty herb bed,
  broadleaf foliage, ordinary preparation table, three separate cue overlays, and 24 normalized
  south-, west-, east-, and north-facing Seeker walk frames, eight two-frame idle/breathe frames,
  and 16 four-frame generic reach-and-inspect frames.
- `masters` contains seven editable layered OpenRaster files for terrain/path, planted herb bed,
  preparation-table states, walk, idle/breathe, interaction strips, and the composed review scene.
- `review/module-review-scene.png` is a 960x540 composition made only from corrected exports. Its
  grayscale companion preserves the path, character, props, and identified-state cue hierarchy.
- `review/state-cues-grayscale.png` shows ordinary, noticeable, identified, and dangerous table
  treatments without relying on hue.

![Corrected module review scene](review/module-review-scene.png)

## Mechanical validation

Run from the repository root:

```sh
python3 art-direction/production-proof/corrected/tools/build.py
python3 art-direction/production-proof/corrected/tools/validate.py
```

The scripts require Python 3 and Pillow. The current run passed all of these gates:

- all 40 exports are RGBA, have visible and fully opaque pixels, and retain no RGB data beneath
  fully transparent pixels;
- non-terrain cutouts have transparent export borders;
- ground left/right and top/bottom edges match exactly, and the straight path's top/bottom edges
  match exactly;
- all six frames in each of the four directional walks are unique, share a 52-pixel contact
  baseline, and use one documented 120 ms loop timing;
- both idle/breathe frames in each direction are distinct, preserve the direction-specific fixed
  origin and 52-pixel baseline, and use one documented 600 ms loop timing;
- all four reach-and-inspect frames in each direction are distinct, all 16 are unique, preserve the
  direction-specific fixed origin and 52-pixel baseline, and use 160 ms non-looping timing;
- the four table states remain measurably distinct in grayscale;
- no fixed review-scene obstacle overlaps the 96x540 path corridor; and
- all seven `.ora` files expose a valid OpenRaster mimetype, layer stack, merged image, and editable
  PNG layers.

## Review conclusion and boundaries

The corrected proof passes the art bible's alpha, seam, pivot, editable-master, grayscale-cue,
four-direction walk, idle/breathe, and interaction gates. Direction A remains feasible, so the
Direction D contingency does not activate.

This is still a deliberately small pipeline proof. It does not prove gameplay-triggered interaction,
a complete terrain transition set, production estimates for the whole opening region, ordinary
runtime animation selection, collision or navigation behavior, UI, or final production quality.
All 56 exports have a separate isolated Godot import proof, and `Main.tscn` references neither
proof. This does not authorize bulk-producing the opening region, changing gameplay/UI behavior,
starting audio, or performing release or signing work.

## Provenance

- The thirteen image-generation inputs were created with Codex's built-in image tool using only
  repository-owned proof images as references. Exact prompts are in [PROMPTS.md](PROMPTS.md).
- The 24 walk frames are derived from the earlier repository-owned source sheet; the eight idle
  frames and 16 interaction frames come from the eight retained character image-tool sources.
- Cue overlays, cleanup, geometry, manifests, composites, hashes, and `.ora` files are produced by
  [tools/build.py](tools/build.py); no third-party content is used.
- The image tool did not expose a model/version identifier, so none is claimed.
