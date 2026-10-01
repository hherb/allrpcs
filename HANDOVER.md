# Allrpcs Handover

## Current state

Phase 8 and Phase 9.1 are complete; Direction A is selected. Phase 9.2 retains 72 manifest-bound
exports and 23 layered OpenRaster masters. The product owner accepted the six external Stream
feasibility views, then separately approved the exact bounded runtime promotion in
[`stream-runtime-promotion-proposal.md`](art-direction/production-proof/corrected/review/stream-runtime-promotion-proposal.md).
Only `terrain.stream_water` (`128x96`), `terrain.stream_bank` (`128x32`), and
`terrain.stream_bank_opening_cap` (`32x32`) were copied into runtime ownership. `Main.tscn`
repeats the water across `(1600,452)-(2000,548)`, retains the centered greybox crossing at
`(1736,452)-(1864,548)` above it, and mirrors clipped banks and caps over the unchanged west/east
collision spans. The 200-pixel physical opening and all route authority remain unchanged.

The three runtime copies match their manifest hashes, have unique lossless, mipmap-free Godot
imports, and leave proof paths outside `Main.tscn`. The Stream/Laundry smoke checks exact texture,
UV, crop, mirror, order, collision, and synchronized physical/navigation traversal. Six
ordinary-scene Compatibility views are saved as `review/stream-runtime-{0.8,1.0,2.0}x-{wide,tall}.png`
under the corrected proof directory; `scripts/capture-stream-runtime.gd` regenerates them at
1280x720 and 1280x827. They have been inspected for seams, crossing readability, route approaches,
labels, and markers. The host-local `./scripts/quality.sh` passed validators, zero-warning build,
all 5,033 .NET tests, scenario validation, Godot smokes, formatting, and Compatibility launch.
The sandbox run passed through the .NET tests but hit the known Godot `user://logs` permission crash;
the identical host-local run passed.

The product owner reviewed the six supplied ordinary-scene views and said they look right, with no
correction requested. This is bounded visual acceptance of the Stream presentation only; it does
not establish pacing, usability, or human accessibility acceptance.

## Exact next bounded slice

Stop at the approval gate for the
[`Village Edge–Village Square connector proposal`](art-direction/production-proof/corrected/review/village-edge-square-connector-proposal.md).
It tests only the existing manifest-bound `terrain.straight_path` as a `124x96` external
feasibility composition across `x=1820..1944`, `y=852..948`, joining the accepted Village Square
west port at `y=900`. Obtain separate product-owner approval before building that proof. If
approved, validate the exact crop and connector seam, the Village Edge greybox handoff, unchanged
physical opening and route, and six deterministic wide/tall views. Keep `Main.tscn`, the accepted
Stream and junction art, other runtime art, collision, navigation, topology, camera, scenario,
dialogue, clue, discovery, treatment, save, provider, and gameplay authority unchanged.
