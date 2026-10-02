# Allrpcs Handover

## Current state

Phase 8 and 9.1 are complete; Direction A is selected. Phase 9.2 now has 77 manifest-bound
exports and 28 editable OpenRaster masters. Square–Garden, Square–Sickhouse, and
Square–Southern Road runtime views are owner-accepted.

On 2026-10-02 the owner accepted the Southern Road–Wagon external tip trim and separately
approved `terrain.southern_road_wagon_path` promotion. The exact 560x432 accepted export,
1120x864 layered master, and independent lossless proof/runtime imports are implemented.
Ordinary startup replaces the route line with one unscaled sprite at `(1680,1496)`, `z=-70`,
and shortens only the presentation road fill to `(572,460)`–`(1500,1400)`. All six runtime
PNG hashes match the accepted external views. Physical/gameplay authority remains unchanged.
See the [runtime result and six views](art-direction/production-proof/corrected/review/southern-road-wagon-path-runtime-result.md).

The focused promotion validator checks exact export/master/import identity, capture parity,
and optional `--check-promotion-scope` restoration of the pre-promotion scene/manifest and
retained geometry/navigation hashes. Use Pillow-enabled `/opt/homebrew/bin/python3`.
Archived proof images/records remain intact; their old full builder is retained in
`review/path-proof-build-source.py.gz`, with its original digest and byte-exact live renderer
checked by regular validators. Old `--check-live-baseline` intentionally rejects the promoted
scene/manifest/builder; use the new promotion validator for current scope instead.

Session review found no unresolved issue. Quality asset checks, formatting, zero-warning
build, all 5,033 .NET tests, scenario validation, the full Godot smoke suite, and Compatibility
launch passed. Sandbox Godot logger/GUI failures required identical host smoke/capture/launch
retries. Negative checks rejected altered pivot and alpha-border processing. Accessibility
report regeneration changed only its manifest digest; previous samples/views stayed exact.

## Exact next bounded slice

Obtain owner visual acceptance of all six ordinary Southern Road–Wagon runtime views.
The external proof acceptance and implementation approval do not complete this checkpoint.
Apply only specifically requested presentation corrections, retaining archived accepted proof.
After acceptance, propose one next bounded asset/proof slice before any further promotion.

Preserve collision, navigation, camera, scenario, dialogue, clues, discovery, treatment, save,
provider, UI, audio, release, and signing. Do not infer pacing, usability, accessibility, or
next-asset acceptance. Direct-main commit/push covers reviewed work only.
