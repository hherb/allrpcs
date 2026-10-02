# Allrpcs Handover

## Current state

Phase 8 and 9.1 are complete; Direction A is selected. Phase 9.2 has 76 manifest-bound
exports and 27 editable OpenRaster masters. Square–Garden, Square–Sickhouse, and
Square–Southern Road runtime views are owner-accepted. The owner replied "approved" after
all six Southern Road runtime views on 2026-10-02; its exact named promotion is complete.
See the [runtime result](art-direction/production-proof/corrected/review/square-southern-road-path-runtime-result.md).

The following Southern Road–Wagon slice is external-only and retains six failed strip views.
The physical route `(1500,1400)`–`(1900,1640)`, width 96, is straight; the road presentation
bends through `(1820,1580)`. A repeated straight texture cannot cover the bent shoulders or
resolve the junction under retained road layering. No production/runtime or physical file changed.
See the [feasibility result](art-direction/production-proof/corrected/review/southern-road-wagon-path-feasibility.md)
for six views, geometry, recipes, hashes, limitations, and the exact correction boundary.

Use Pillow-enabled Python (`/opt/homebrew/bin/python3`). The evidence validator is in regular
quality; `--check-live-baseline` additionally freezes runtime/manifest/geometry inputs.
Repeated captures reproduced all six PNGs exactly. Review found no unresolved issue. Evidence
and live-baseline checks, all quality validators, formatting, zero-warning build, 5,033 tests,
scenario validation, all 281 host Godot smokes, and Compatibility launch passed. Sandbox
user-log restrictions required the identical host retry; no product workaround was introduced.

## Exact next bounded slice

Develop one external-only world-aligned Southern Road–Wagon correction from the existing
straight-path source. Evaluate retained bent-road shoulder coverage and the junction with the
accepted Square continuation. Keep the failed strip and document any disposable presentation
substitution explicitly. Preserve physical straight-route topology/width, road geometry/outline,
wagon markers, and all other assets. Capture corrected 0.8x/1.0x/2.0x wide/tall views for owner
review before proposing any separately approved named promotion. No promotion is authorized.

Preserve physical/gameplay authority, collision, navigation, camera, scenario, dialogue, clues,
discovery, treatment, save, provider, UI, audio, release, and signing. Do not infer pacing,
usability, or accessibility acceptance. Direct-main commit/push covers reviewed work only.
