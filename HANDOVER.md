# Allrpcs Handover

## Current state

Phase 8 and 9.1 are complete; Direction A is selected. Phase 9.2 has 77 manifest-bound exports
and 28 editable OpenRaster masters. Square–Garden, Square–Sickhouse, Square–Southern Road,
and Southern Road–Wagon ordinary runtime views are owner-accepted. The last runtime checkpoint
was accepted on 2026-10-02 ("yes"); see the [Road–Wagon result](art-direction/production-proof/corrected/review/southern-road-wagon-path-runtime-result.md).

The external Shrine–Southern Road endpoint blend now has 18 midpoint/shrine-exit/junction
views at 0.8x, 1.0x, 2.0x in both 1280x720 and 1280x827 frames. It retains the original incoming
correction at z=-72 and adds only a local 160x160 patch at `(1480,1360)`, z=-69. Its 3,354 visible
pixels stay in a 48px band at the Road–Wagon leading edge. The long diagonal seam is softened;
the close texture transition/edges still require owner review. No named promotion is approved.
See the [blend views and recipe](art-direction/production-proof/corrected/review/shrine-southern-road-path-blend.md).

The original correction's 18 views and six failed-strip views/evidence remain immutable.
All accepted sprite nodes/textures, shrine shell/opening, outlines/collisions and physical
routes remain intact. Production/runtime/import/manifest/gameplay files are unchanged.
Seven blend captures differ, only inside the projected patch; all shrine-exit views are exact.
All three Shrine validators run in regular quality. Use Pillow-enabled `/opt/homebrew/bin/python3`
and `check_shrine_southern_road_path_blend.py --check-live-baseline` for the pinned 417-file baseline.
Old archived path proofs/build source remain intact. Old Road–Wagon live-baseline checking
rejects its promoted baseline by design; its runtime validator applies instead.

Session review tightened capture alpha validation; no code issue remains. Repeated Compatibility
captures matched all 18 hashes, and live baseline checks passed. Full host `./scripts/quality.sh`
passed asset validators, formatting, zero-warning build, all 5,033 tests, scenario validation,
complete credential-free Godot smokes and Compatibility launch. Visual acceptance remains pending.

## Exact next bounded slice

Obtain owner visual review of all 18 blend views, including the 2.0x junction transition and
ragged edges. If accepted, propose the exact named Shrine–Southern Road promotion scope,
including the incoming correction and local patch, editable master/export, imports and one
presentation owner. Obtain separate approval before production/runtime work. If correction
is requested, keep it external and bounded to the endpoint; preserve all prior evidence.

Preserve navigation, camera, scenario, dialogue, clues, discovery, treatment, save, provider,
UI, audio, release, and signing. Do not infer pacing, usability, accessibility, gameplay or
next-asset acceptance. Direct-main commit/push covers reviewed work.
