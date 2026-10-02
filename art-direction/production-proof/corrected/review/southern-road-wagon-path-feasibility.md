# Southern Road–Wagon external path feasibility

The owner approved all six Square–Southern Road ordinary-scene views with "approved" after
display on 2026-10-02. The next terrain slice tests only the unchanged straight-path texture
on Southern Road–Wagon in a disposable ordinary-scene instance. No production asset, master,
manifest/import, runtime scene/code, or physical/gameplay authority changed.

The physical route remains `(1500,1400)`–`(1900,1640)`, width 96, length `466.476152`,
heading `30.963757°`. A repeated `96x466.476152` region of the runtime straight-path texture
is centered at `(1700,1520)`, rotated `-59.036243°`, `z=-70`. The original greybox route,
road presentation/outline, accepted Square continuation, wagon markers, and world labels
remain visible. Only the Seeker and screen overlays are hidden and the camera reframed.

The strip is not a promotion candidate. The road presentation bends through `(1820,1580)`
between the two physical anchors, with segment headings `29.357754°` and `36.869898°`.
That middle point lies `10.289915` pixels off the direct physical centerline. Its wider visible
shoulders and outlines remain exposed around the direct strip. The road presentation at its
retained ordinary layer also covers and darkens the texture; the start meets the accepted flat
Square-path endpoint without a resolved junction. The wagon shell covers part of the far end.
These six views record the failed direct-strip approach and its retained layering, not owner
visual acceptance, usability, pacing, or accessibility acceptance.

| Zoom | Wide | Tall |
| --- | --- | --- |
| 0.8x | [view](southern-road-wagon-path-proof-0.8x-wide.png) | [view](southern-road-wagon-path-proof-0.8x-tall.png) |
| 1.0x | [view](southern-road-wagon-path-proof-1.0x-wide.png) | [view](southern-road-wagon-path-proof-1.0x-tall.png) |
| 2.0x | [view](southern-road-wagon-path-proof-2.0x-wide.png) | [view](southern-road-wagon-path-proof-2.0x-tall.png) |

```sh
godot --path game --script "$PWD/scripts/capture-southern-road-wagon-path-proof.gd"
/opt/homebrew/bin/python3 art-direction/production-proof/corrected/tools/check_southern_road_wagon_path_feasibility.py --check-live-baseline
```

The evidence record pins six PNGs, the capture recipe and source texture, plus the optional
live runtime/manifest/physical baseline. Regular quality validates archived evidence; the live
option additionally proves this slice has not changed its runtime inputs. Captures use Godot
4.6.3 .NET Compatibility on this macOS host; PNG bytes depend on the renderer.

Review covered the capture recipe, source ownership, retained route and road layering, frozen
runtime/manifest/geometry baseline, and all six views. Repeated capture reproduced every PNG
byte exactly. No unresolved code finding remains. Evidence/live-baseline checks, all quality
validators, formatting, zero-warning build, 5,033 tests, scenario validation, all 281 host Godot
smokes, and Compatibility launch passed. Sandbox Godot user-log restrictions required the
identical host retry; no product workaround was introduced.

Next bounded slice: develop one external-only world-aligned Road–Wagon correction using the
existing straight-path source. Evaluate coverage of the retained bent road presentation and
its junction with the accepted Square continuation. Retain the original physical straight route,
96-pixel width, road geometry/outline, wagon markers, and all other assets. Any temporary
presentation substitution must occur only in the disposable proof, be explicitly documented,
and preserve the failed strip. Capture six corrected wide/tall views at the same zooms and
obtain owner review before naming any separate production/runtime promotion. No promotion,
road topology, physical navigation, scene UI, gameplay, audio, release, or signing change is
currently authorized.
