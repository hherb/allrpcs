# Allrpcs Handover

## Current state

Phase 8 and 9.1 are complete; Direction A is selected. Phase 9.2 has 77 manifest-bound exports
and 28 editable OpenRaster masters. Square–Garden, Square–Sickhouse, Square–Southern Road,
and Southern Road–Wagon ordinary runtime views are owner-accepted. The last checkpoint was
accepted on 2026-10-02 ("yes"); see the [runtime result](art-direction/production-proof/corrected/review/southern-road-wagon-path-runtime-result.md).
The named Road–Wagon promotion is implemented and validated; physical/gameplay authority is intact.

The next unchanged-texture test produced six external-only Shrine–Southern Road strip views.
It is unsuitable for promotion: retained greybox fringe, translucent brown-fill darkening, and
20.384px shrine-exit centerline offset remain. Mid-route framing does not prove either endpoint.
See the [failed proof and recipe](art-direction/production-proof/corrected/review/shrine-southern-road-path-feasibility.md).
No runtime, production, manifest/import, or physical/gameplay input changed in this proof slice.
The validator is in regular quality; use Pillow-enabled `/opt/homebrew/bin/python3` and
`check_shrine_southern_road_path_feasibility.py --check-live-baseline` for its frozen inputs.

Previous proof records/images remain intact. The old full builder is archived in
`review/path-proof-build-source.py.gz`; regular validators pin its digest and exact live renderer.
Old Road–Wagon `--check-live-baseline` rejects the now-promoted baseline by design; its new
`validate_southern_road_wagon_path.py --check-captures --check-promotion-scope` validates the promotion.

Session code/visual review found no unresolved implementation issue. Repeated Compatibility
capture reproduced all six proof hashes; frozen live baseline checks passed. Full host
`./scripts/quality.sh` passed: asset validators, formatting, zero-warning build, all 5,033 tests,
scenario validation, complete credential-free Godot smokes, and Compatibility launch.

## Exact next bounded slice

Develop one external-only world-aligned Shrine–Southern Road presentation correction from
the existing straight-path source. In a disposable scene only, hide the exact physical-route
presentation line and replace only the upstream road fill with texture. Preserve the accepted
Square/Road–Wagon overlays, shrine shell/opening, all outlines/collisions, and physical routes.
Add explicit shrine-exit and Southern Road junction views alongside midpoint views, at all
three zooms (0.8x, 1.0x, 2.0x) and both 1280x720/1280x827 frames. Keep failed-strip evidence
immutable. Obtain owner visual review before proposing a separately approved named promotion.

Preserve navigation, camera, scenario, dialogue, clues, discovery, treatment, save, provider,
UI, audio, release, and signing. No new production/runtime promotion is authorized. Do not infer
pacing, usability, accessibility, or next-asset acceptance. Direct-main commit/push covers reviewed work.
