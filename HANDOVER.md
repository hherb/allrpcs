# Allrpcs Handover

## Current state

Phase 8 and 9.1 are complete; Direction A is selected. Phase 9.2 has 77 manifest-bound exports
and 28 editable OpenRaster masters. Square–Garden, Square–Sickhouse, Square–Southern Road,
and Southern Road–Wagon ordinary runtime views are owner-accepted. The last checkpoint was
accepted on 2026-10-02 ("yes"); see the [runtime result](art-direction/production-proof/corrected/review/southern-road-wagon-path-runtime-result.md).

The external-only Shrine–Southern Road correction now has 18 midpoint/shrine-exit/junction
views at 0.8x, 1.0x, 2.0x in both 1280x720 and 1280x827 frames. It removes the exact greybox
route line and upstream brown fill only in a disposable scene, and aligns the textured entry
with the physical route and retained shrine opening. The correction sits below both accepted
Square/Road–Wagon overlays. All production/runtime/import/physical inputs remain unchanged.
See the [correction views, limitation and recipe](art-direction/production-proof/corrected/review/shrine-southern-road-path-correction.md).
A thin diagonal seam remains at the Southern Road junction, clearest at 2.0x. It is not
owner-accepted and does not justify promotion. The six failed-strip views/evidence stay immutable.

Both Shrine validators are in regular quality. Use Pillow-enabled `/opt/homebrew/bin/python3`
and `check_shrine_southern_road_path_correction.py --check-live-baseline` for the pinned 417-file
runtime/production/import/physical baseline. Archived old proofs remain intact; the archived
path builder and current shared renderer retain their existing digest checks. Old Road–Wagon
`--check-live-baseline` rejects its promoted baseline by design; its runtime validator applies.

Session code review fixed the correction draw order; no further implementation issue remains.
The junction seam is a recorded visual limitation. Repeated host Compatibility capture matched
all 18 PNG hashes, and frozen live baseline validation passed. Full host `./scripts/quality.sh`
passed asset validators, formatting, zero-warning build, all 5,033 tests, scenario validation,
the complete credential-free Godot smoke suite, and Compatibility launch. The sandbox run
passed through .NET/scenario checks but failed on Godot log/settings writes; host retry passed.

## Exact next bounded slice

Obtain owner visual review of all 18 correction views, explicitly including the junction seam.
If correction is requested, develop only an external Shrine–Road endpoint blend correction,
keeping accepted Square/Road–Wagon sprites and shrine shell/opening intact. Retain midpoint
and both endpoint views at all established zooms/frame heights; preserve this correction and
failed-strip evidence. Obtain acceptance before proposing a separately approved named promotion.

Preserve navigation, camera, scenario, dialogue, clues, discovery, treatment, save, provider,
UI, audio, release, and signing. No new production/runtime promotion is authorized. Do not infer
pacing, usability, accessibility, or next-asset acceptance. Direct-main commit/push covers reviewed work.
