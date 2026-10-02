# Shrine–Southern Road external world-aligned correction

This correction follows the [failed strip proof](shrine-southern-road-path-feasibility.md),
whose six views, recipe and evidence remain immutable. It is external-only: the disposable
scene hides the exact 96px `(380,300)` → `(1500,1400)` route presentation line and the retained
upstream-only brown `RoadArea` fill `(572,460)` → `(1500,1400)`. It adds one review PNG sprite
at `z=-72`. No scene is saved. Production assets/masters, manifest, imports, runtime assets,
physical routes, collisions, camera code, gameplay and UI code remain unchanged.

The 1152x1080 overlay has world origin `(432,400)` and samples the unchanged 96x256 straight
path along the physical route heading, retaining its sampling phase from `(380,300)`.
It clips above shrine bottom `y=460` and to the unchanged 96px opening while underneath the
8px-deep opening marker. The entry center is `(542.9091,460)`, derived from the physical route,
rather than the old fill's `(572,460)`. The shrine shell/opening, accepted Square–Road and
Road–Wagon overlays, all boundary outlines/collisions and world labels retain their owners.
Only disposable screen-overlay/Seeker visibility and review framing also change.

**Result: ready for owner review, with a recorded junction limitation.** Mid-route views no
longer show the greybox fringe or brown-fill darkening. The shrine entry meets the retained
opening without retaining the old offset. The junction is connected, but a thin diagonal seam
is visible against the accepted Road–Wagon texture, especially at 2.0x. These endpoint views
expose that limitation; they do not establish seam acceptance or authorize promotion.
The retained dark boundary outlines at the Road–Wagon branch are ordinary accepted geometry.

The three Seeker/camera target positions are midpoint `(940,850)`, shrine-exit `(543,460)`,
and junction `(1500,1400)`. Ordinary world limits still clamp the camera at low zooms near
world edges. Targets describe requested framing, not an override of those limits.
Each view uses both 1280x720 and 1280x827 frames at all three established zooms.

## Midpoint

| Zoom | Wide | Tall |
| --- | --- | --- |
| 0.8x | [view](shrine-southern-road-path-correction-midpoint-0.8x-wide.png) | [view](shrine-southern-road-path-correction-midpoint-0.8x-tall.png) |
| 1.0x | [view](shrine-southern-road-path-correction-midpoint-1.0x-wide.png) | [view](shrine-southern-road-path-correction-midpoint-1.0x-tall.png) |
| 2.0x | [view](shrine-southern-road-path-correction-midpoint-2.0x-wide.png) | [view](shrine-southern-road-path-correction-midpoint-2.0x-tall.png) |

## Shrine exit

| Zoom | Wide | Tall |
| --- | --- | --- |
| 0.8x | [view](shrine-southern-road-path-correction-shrine-exit-0.8x-wide.png) | [view](shrine-southern-road-path-correction-shrine-exit-0.8x-tall.png) |
| 1.0x | [view](shrine-southern-road-path-correction-shrine-exit-1.0x-wide.png) | [view](shrine-southern-road-path-correction-shrine-exit-1.0x-tall.png) |
| 2.0x | [view](shrine-southern-road-path-correction-shrine-exit-2.0x-wide.png) | [view](shrine-southern-road-path-correction-shrine-exit-2.0x-tall.png) |

## Southern Road junction

| Zoom | Wide | Tall |
| --- | --- | --- |
| 0.8x | [view](shrine-southern-road-path-correction-junction-0.8x-wide.png) | [view](shrine-southern-road-path-correction-junction-0.8x-tall.png) |
| 1.0x | [view](shrine-southern-road-path-correction-junction-1.0x-wide.png) | [view](shrine-southern-road-path-correction-junction-1.0x-tall.png) |
| 2.0x | [view](shrine-southern-road-path-correction-junction-2.0x-wide.png) | [view](shrine-southern-road-path-correction-junction-2.0x-tall.png) |

## Reproduction and evidence

```sh
/opt/homebrew/bin/python3 art-direction/production-proof/corrected/tools/build_shrine_southern_road_path_correction.py
godot --path game --script "$PWD/scripts/capture-shrine-southern-road-path-correction.gd"
/opt/homebrew/bin/python3 art-direction/production-proof/corrected/tools/check_shrine_southern_road_path_correction.py --check-live-baseline
```

The pinned evidence record binds the overlay and all 18 RGBA views, exact recipe and source,
shared renderer, entry clipping and route geometry. It retains the failed proof validation.
The optional live check pins 417 files: runtime/physical sources, scene, camera, manifest,
production exports/masters and every existing runtime/proof asset and import. Regular quality
validates archived evidence and reproducibility without freezing future approved promotions.

Session code review fixed the correction draw order; no further implementation issue remains.
The junction seam is a recorded visual limitation. Repeated host Compatibility capture matched
all 18 PNG hashes, and frozen live baseline validation passed. Full host `./scripts/quality.sh`
passed asset validators, formatting, zero-warning build, all 5,033 tests, scenario validation,
the complete credential-free Godot smoke suite, and Compatibility launch. The sandbox run
passed through .NET/scenario checks but failed on Godot log/settings writes; host retry passed.

Next boundary: obtain owner review of these views, explicitly including the junction seam.
If a correction is required, keep it external and bounded to the Shrine–Road endpoint blend,
preserving the accepted Square/Road–Wagon sprites and all physical/gameplay authority. Do not
propose a named promotion until the resulting external views are accepted. No pacing,
usability, accessibility, gameplay, further asset, release or signing acceptance is inferred.
