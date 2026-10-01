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

## Exact next bounded slice

Ask the product owner to review the six ordinary-scene Stream views. Record only the visual
observations supplied, including any correction requested, in `HANDOVER.md` and relevant art docs.
Do not infer pacing, usability, or human accessibility acceptance from automated checks or visual
inspection. Do not change accepted terrain, paths, junctions, clearing perimeter, foliage, table,
beds, Seeker art or animation, structures, props, other character art, UI systems, effects, audio,
collision, navigation, locations, topology, camera tuning, scenario, dialogue, clue, discovery,
treatment, save, provider, or gameplay authority without a separately approved bounded slice.
