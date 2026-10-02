# Allrpcs Handover

## Current state

Phase 8 and 9.1 are complete; Direction A is selected. Phase 9.2 has 76 manifest-bound
exports and 27 editable OpenRaster masters. Square–Garden, Square–Sickhouse, and
Square–Southern Road runtime views are owner-accepted.

On 2026-10-02 the owner accepted the Southern Road–Wagon external correction and its
16-pixel incoming-tip trim ("Looks good"). The intended-side/visual gate is complete.
See the [accepted trim and six views](art-direction/production-proof/corrected/review/southern-road-wagon-path-trim.md).
Failed-strip and original correction evidence/recipes remain intact. No production/runtime,
manifest/import, or physical/gameplay file changed. Runtime promotion still needs separate approval.

Focused validation with `/opt/homebrew/bin/python3` and `--check-live-baseline` passed:
2,789 changed pixels confined to the incoming tip, no increased alpha, unchanged archived
proofs and runtime/manifest/geometry baseline. The trim builder, validator, and capture script
were reviewed without actionable findings. Earlier full quality and repeated six-view capture
evidence remain documented in the linked reviews; this acceptance update changes only docs.

## Exact next bounded slice — pending separate approval

Proposed named promotion: `terrain.southern_road_wagon_path`, using the exact accepted
560x432 `review/southern-road-wagon-path-trim-overlay.png` bytes. Add the manifest export
`exports/terrain/southern-road-wagon-path.png`, provenance binding the trim recipe and proof,
and a 1120x864 editable `masters/terrain-southern-road-wagon-path.ora` with nearest-neighbour
2x accepted pixels, editable road/junction layers, and a hidden source reference.

Copy the exact export to `game/art/direction-a-import-proof/terrain/southern-road-wagon-path.png`
and `game/art/runtime/terrain/southern-road-wagon-path.png`; use independent Godot UIDs,
lossless imports, no mipmaps, and `process/fix_alpha_border=false`. Extend the isolated proof
and ordinary presentation ownership only as needed for this named texture.

Match the accepted disposable substitutions in ordinary startup: replace the exact 96-pixel
Southern Road–Wagon route line with the unscaled sprite centered at `(1680,1496)`, `z=-70`;
shorten only the presentation `RoadArea` fill to `(572,460)`–`(1500,1400)`. Retain upstream
brown fill, boundary outlines, wagon occlusion, and all physical route/boundary geometry.
Validate export/master/import hash and pixel parity, authority boundaries, ordinary startup
smokes, and all six runtime captures against the accepted views; run `./scripts/quality.sh`.
Obtain a separate owner checkpoint for the resulting ordinary runtime views before advancing.

Preserve collision, navigation, camera, scenario, dialogue, clues, discovery, treatment, save,
provider, UI, audio, release, and signing. Do not infer pacing, usability, or accessibility
acceptance. Direct-main commit/push covers reviewed work only. No promotion is yet authorized.
