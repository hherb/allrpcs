# Southern Road–Wagon external world-aligned correction

This external-only correction follows the retained road presentation through `(1500,1400)`,
`(1820,1580)`, and `(1900,1640)` using the existing 96x256 straight-path texture. The physical
route remains straight between the endpoints, width 96. The [six failed strip views](southern-road-wagon-path-feasibility.md)
remain unchanged. No runtime, production export/master, manifest, import, collision, or gameplay
file changed. Owner visual review is pending; no named promotion is authorized.

The overlay is 560x432, world origin `(1400,1280)`, unscaled center `(1680,1496)`, `z=-70`.
Two finite road legs use 48-pixel backward overlaps. A local 48-pixel continuation of the
accepted Square leg retains its source sampling angle/phase beyond its endpoint. Overlapping
new legs blend RGB weighted by alpha and retain maximum alpha. The incoming side of that
Square continuation is clipped at its existing endpoint to avoid rendering it twice; the new
road leg overlaps the existing sprite locally. This is not an exact-pixel Square seam contract.

Only in the disposable scene, the exact straight greybox route line is hidden and the road
fill's last two bent segments are replaced by the overlay. The upstream fill remains from
`(572,460)` to `(1500,1400)`. Its flat brown end still partly covers the junction. All boundary
outlines/collisions, wagon shell/opening/marker, and world labels retain their ordinary scene
ownership. Only screen overlays and the Seeker are hidden, with camera reframing for capture.
The capture script never saves the scene.

The bent shoulders now receive texture without the previous road-fill darkening, and the
Square leg connects into the road. Retained straight boundary outlines do not follow the bent
texture edges. The upstream brown end, subtle overlap/texture changes at the junction, and the
wagon shell covering/darkening the far end remain visible for owner assessment. These views
are feasibility evidence, not pacing, usability, accessibility, or promotion acceptance.

| Zoom | Wide | Tall |
| --- | --- | --- |
| 0.8x | [view](southern-road-wagon-path-correction-0.8x-wide.png) | [view](southern-road-wagon-path-correction-0.8x-tall.png) |
| 1.0x | [view](southern-road-wagon-path-correction-1.0x-wide.png) | [view](southern-road-wagon-path-correction-1.0x-tall.png) |
| 2.0x | [view](southern-road-wagon-path-correction-2.0x-wide.png) | [view](southern-road-wagon-path-correction-2.0x-tall.png) |

```sh
/opt/homebrew/bin/python3 art-direction/production-proof/corrected/tools/build_southern_road_wagon_path_correction.py
godot --path game --script "$PWD/scripts/capture-southern-road-wagon-path-correction.gd"
/opt/homebrew/bin/python3 art-direction/production-proof/corrected/tools/check_southern_road_wagon_path_correction.py --check-live-baseline
```

Regular quality validates both the failed strip and corrected evidence, exact source/recipe
hashes, overlay reproduction, image dimensions, transparent RGB, and unclipped canvas edges.
The optional live-baseline check freezes the previous runtime/manifest/geometry inputs.
PNG capture bytes depend on this host's Godot 4.6.3 .NET Compatibility renderer.

Session code/visual review fixed a GDScript variable conflict, duplicate incoming rendering,
and edge-mask pixel holes; no unresolved code finding remains. Repeated final capture matched
all six archived PNG hashes. Correction/failed-strip evidence and live-baseline checks passed.
Regular quality validators, formatting, zero-warning build, all 5,033 tests, scenario validation,
the full host Godot smoke suite, and Compatibility launch passed. Sandbox Godot logger access
required identical host smoke/capture retries; no product workaround was introduced. The
headless editor import emitted its existing Android EditorSettings diagnostic and exited zero.

Next slice: obtain owner review of all six corrected views, including the retained upstream
brown end and outline mismatch. Apply only specifically requested external corrections before
proposing a separate named production/runtime promotion. No promotion is currently authorized.
