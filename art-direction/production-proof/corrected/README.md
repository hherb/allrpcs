# Direction A corrected production-pipeline proof

Status: bounded corrected proof, 2026-09-28. This directory remains outside `game/`; exact copies
are imported only by the isolated proof scene and none is runtime-authoritative.

This pass corrects the sheet-dependent failures recorded by the first Phase 9.2 proof. Eighteen
individual painterly inputs are retained under `sources/imagegen`; deterministic Python tooling
turns them and the earlier Seeker source sheet into individual exports, exact geometry, editable
OpenRaster masters, and review evidence.

## Result

- `asset-manifest.json` contains all 66 asset IDs, logical/export rectangles, exact 2x master
  dimensions, contact baselines, pivots, hashes, animation order/timing, review placements, and the
  96-pixel navigation-clearance corridor. Every entry is included in the isolated import proof.
- `exports` contains one file per module: quiet moss, opaque woodland leaf litter, one deterministic
  moss-to-leaf straight transition, two outer-corner transitions, two inset-corner transitions,
  straight path, one 90-degree path bend, one T-junction, one
  cross-junction, one end-cap, empty herb bed, broadleaf foliage, ordinary preparation
  table, three
  separate cue overlays, and 24 normalized
  south-, west-, east-, and north-facing Seeker walk frames, eight two-frame idle/breathe frames,
  and 16 four-frame generic reach-and-inspect frames.
- `masters` contains seventeen editable layered OpenRaster files for terrain/path, the leaf-litter
  ground and five transitions, the path bend, the
  path T-junction, the path cross-junction, the path end-cap, planted herb bed,
  preparation-table states, walk,
  idle/breathe, interaction strips, and the composed review scene.
- `review/module-review-scene.png` is a 960x540 composition made only from corrected exports. Its
  grayscale companion preserves the path, character, props, and identified-state cue hierarchy.
- `review/state-cues-grayscale.png` shows ordinary, noticeable, identified, and dangerous table
  treatments without relying on hue.
- `review/path-bend-review.png` joins the bend to vertical and horizontal copies of the straight
  module over quiet moss without adding another terrain asset.
- `review/path-t-junction-review.png` joins the junction to vertical and two horizontal copies of
  the straight module over quiet moss without adding another terrain asset.
- `review/path-cross-junction-review.png` joins the cross-junction to two vertical and two
  horizontal copies of the straight module over quiet moss without adding another terrain asset.
- `review/path-end-cap-review.png` joins the end-cap below one exact vertical copy of the straight
  module over quiet moss without adding another terrain asset.
- `review/path-module-topology-review.png` connects rotated T-junction and bend modules with the
  straight, cross-junction, and end-cap exports as one deterministic network over quiet moss.
- `review/woodland-leaf-litter-review.png` shows the opaque leaf-litter tile in an exact 2x2 seam
  proof beside quiet moss and the packed-earth modules.
- `review/woodland-leaf-litter-transition-review.png` repeats the exact quiet-moss, deterministic
  edge transition, and leaf-litter sequence in two connected rows.
- The four corner-transition review files connect each outer or inset corner to exact source-ground
  and rotated straight-transition neighbours in a 3x3 composition.
- `review/terrain-accessibility` contains exact original, grayscale, protanopia, deuteranopia, and
  tritanopia views for quiet moss, leaf litter, packed earth, and all five ground transitions. Its
  report binds every view to the current manifest hashes and records descriptive luminance and
  neighbouring-pixel texture measurements without making an accessibility acceptance claim.

![Corrected module review scene](review/module-review-scene.png)

## Mechanical validation

Run from the repository root:

```sh
python3 art-direction/production-proof/corrected/tools/build.py
python3 art-direction/production-proof/corrected/tools/validate.py
python3 art-direction/production-proof/corrected/tools/terrain_accessibility_review.py
python3 art-direction/production-proof/corrected/tools/validate_terrain_accessibility_review.py
```

The scripts require Python 3 and Pillow. The current run passed all of these gates:

- all 66 exports are RGBA, have visible and fully opaque pixels, and retain no RGB data beneath
  fully transparent pixels;
- non-terrain cutouts have transparent export borders;
- ground left/right and top/bottom edges match exactly, and the straight path's top/bottom edges
  match exactly;
- the leaf-litter tile is fully opaque, has exact opposing-edge parity, useful tonal variation,
  measurable moss/path contrast, and no 96-pixel full-span path-like corridor;
- the deterministic ground transition retains exact 32-pixel quiet-moss and leaf-litter connector
  strips, full opacity, and an exact repeatable top/bottom seam;
- both outer and both inset corners retain exact straight-transition edge connectors,
  exact unused source-ground edges, full opacity, and connected 3x3 review seams;
- the 256x256 path bend has transparent unused edges and outer corners, plus exact 96-pixel
  straight-path parity at its centered top entry and compatible centered right exit;
- the 256x256 path T-junction has transparent lower outer regions and bottom edge, plus exact
  96-pixel straight-path parity at its centered top, left, and right ports;
- the 256x256 path cross-junction has transparent outer corners and exact 96-pixel straight-path
  parity at its centered top, bottom, left, and right ports;
- the 256x256 path end-cap has exact 96-pixel straight-path parity at its centered top port, a
  rounded terminus before the lower half, transparent side and bottom edges, and a transparent
  lower 64-pixel band;
- one 1024x768 topology composition connects every path shape using only exact exports and
  deterministic rotations, without introducing another terrain asset;
- all six frames in each of the four directional walks are unique, share a 52-pixel contact
  baseline, and use one documented 120 ms loop timing;
- both idle/breathe frames in each direction are distinct, preserve the direction-specific fixed
  origin and 52-pixel baseline, and use one documented 600 ms loop timing;
- all four reach-and-inspect frames in each direction are distinct, all 16 are unique, preserve the
  direction-specific fixed origin and 52-pixel baseline, and use 160 ms non-looping timing;
- the four table states remain measurably distinct in grayscale;
- the external terrain accessibility review exactly reproduces 40 simulation views from the current
  manifest exports, preserves fixed sheet order, and reproduces every recorded descriptive metric;
- no fixed review-scene obstacle overlaps the 96x540 path corridor; and
- all seventeen `.ora` files expose a valid OpenRaster mimetype, layer stack, merged image, and
  editable PNG layers.

## Review conclusion and boundaries

The corrected proof passes the art bible's alpha, seam, pivot, editable-master, grayscale-cue,
four-direction walk, idle/breathe, and interaction gates. The terrain-only colour-vision review is
reproducible external evidence, not human accessibility acceptance. Direction A remains feasible,
so the Direction D contingency does not activate.

This is still a deliberately small pipeline proof. It does not prove gameplay-triggered interaction,
a complete terrain transition set, production estimates for the whole opening region, ordinary
runtime animation selection, collision or navigation behavior, UI, or final production quality.
All 66 exports have a separate isolated Godot import proof, and `Main.tscn` references neither
proof. This does not authorize bulk-producing the opening region, changing gameplay/UI behavior,
starting audio, or performing release or signing work.

## Provenance

- The eighteen image-generation inputs were created with Codex's built-in image tool using only
  repository-owned proof images as references. Exact prompts are in [PROMPTS.md](PROMPTS.md).
- The 24 walk frames are derived from the earlier repository-owned source sheet; the eight idle
  frames and 16 interaction frames come from the eight retained character image-tool sources.
- Cue overlays, cleanup, geometry, manifests, composites, hashes, and `.ora` files are produced by
  [tools/build.py](tools/build.py); no third-party content is used.
- The image tool did not expose a model/version identifier, so none is claimed.
