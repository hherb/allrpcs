# Shrine–Southern Road external strip feasibility

This external-only proof follows the accepted Southern Road–Wagon runtime checkpoint.
It tests the unchanged 96x256 straight-path texture on the retained 96-pixel physical route
from the Old Shrine anchor `(380,300)` to Southern Road `(1500,1400)`. The disposable sprite
repeats along the exact route length, centered at `(940,850)`, rotated along the route, `z=-70`.
No scene is saved and no runtime, production, manifest, import, collision, or gameplay file changes.

The route line stays visible beneath the strip. The retained translucent upstream `RoadArea`
fill from `(572,460)` to `(1500,1400)` stays above it. All shrine/wagon shells, openings,
boundary outlines/collisions, world labels, and the accepted Square/road/wagon textures retain
ordinary ownership. Only screen overlays/Seeker visibility and camera framing change.

**Result: unsuitable for promotion.** The greybox fringe remains visible along the ragged
texture edges, and the translucent brown road fill darkens much of the texture. Its shrine-exit
centerline is 20.384 pixels off the physical route; the offset converges toward the shared
Southern Road endpoint. The mid-route views do not establish endpoint seam or shrine-opening
coverage: this long diagonal exceeds the frame at the tested zooms. Do not claim those gates.

| Zoom | Wide | Tall |
| --- | --- | --- |
| 0.8x | [view](shrine-southern-road-path-proof-0.8x-wide.png) | [view](shrine-southern-road-path-proof-0.8x-tall.png) |
| 1.0x | [view](shrine-southern-road-path-proof-1.0x-wide.png) | [view](shrine-southern-road-path-proof-1.0x-tall.png) |
| 2.0x | [view](shrine-southern-road-path-proof-2.0x-wide.png) | [view](shrine-southern-road-path-proof-2.0x-tall.png) |

```sh
godot --path game --script "$PWD/scripts/capture-shrine-southern-road-path-proof.gd"
/opt/homebrew/bin/python3 art-direction/production-proof/corrected/tools/check_shrine_southern_road_path_feasibility.py --check-live-baseline
```

The pinned evidence record binds all six RGBA captures, exact source and recipe hashes,
retained route/fill geometry, and optional live runtime/manifest/physical baseline checks.
Regular quality includes this validator. All previous accepted assets and authority stay intact.

Code and visual review found no unresolved implementation issue in this failed-proof slice.
Repeated host Compatibility capture reproduced all six PNG hashes, and frozen live baseline
validation passed. Full host `./scripts/quality.sh` passed asset validators, formatting,
zero-warning build, all 5,033 .NET tests, scenario validation, the complete credential-free
Godot smoke suite, and Compatibility launch. No runtime/production change was needed.

Next bounded slice: develop one external-only world-aligned correction for the retained
Shrine–Southern Road presentation, using the existing straight-path source. In a disposable
scene only, hide the exact physical-route presentation line and replace only the upstream
road fill with texture; preserve the accepted Road–Wagon overlay, shrine shell/opening,
all outlines/collisions, and physical routes. Add endpoint-specific shrine-exit and Southern
Road junction views alongside midpoint views at 0.8x, 1.0x, and 2.0x in both frame heights.
Keep this failed evidence immutable; obtain owner review of the correction before proposing
any separately approved named production/runtime promotion.
