# Allrpcs Handover

## Current state

Phase 8 and 9.1 are complete; Direction A is selected. Phase 9.2 has 76 manifest-bound
exports and 27 editable OpenRaster masters. Square–Garden, Square–Sickhouse, and
Square–Southern Road runtime views are owner-accepted.

Southern Road–Wagon now has six retained failed-strip views and one external-only
world-aligned correction with six wide/tall views at 0.8x/1.0x/2.0x. No production/runtime,
manifest/import, or physical/gameplay file changed. Owner review is pending.
See the [correction and all six views](art-direction/production-proof/corrected/review/southern-road-wagon-path-correction.md)
for recipes, geometry, disposable substitutions, and remaining visual limits.

The correction follows the bent road fill while the physical route stays straight and 96px
wide. Only its disposable scene hides the route line and substitutes the final two road-fill
segments. Upstream brown fill, straight boundary outlines, and wagon shell remain visible.
Use Pillow-enabled `/opt/homebrew/bin/python3`; the correction validator is in regular quality,
and `--check-live-baseline` additionally freezes the unchanged runtime/manifest/geometry inputs.

Review found no unresolved code issue. Final captures reproduced all six PNG hashes; evidence,
live baseline, quality validators, formatting, zero-warning build, 5,033 tests, scenario,
full host Godot smokes, and Compatibility launch passed. Sandbox logger access required host
retries; the headless editor emitted its existing Android settings diagnostic and exited zero.

## Exact next bounded slice

Obtain owner review of all six corrected Southern Road–Wagon views, explicitly including the
upstream brown endpoint, straight-outline/bent-texture mismatch, junction texture overlap,
and wagon occlusion. If corrections are requested, keep them external-only and retain both
archived evidence sets. After visual acceptance, propose a separately approved named
production/runtime promotion with exact asset, master, import, and disposable-substitution
scope. Neither acceptance nor promotion is currently authorized; do not infer either from
a generic continuation request.

Preserve physical/gameplay authority, collision, navigation, camera, scenario, dialogue, clues,
discovery, treatment, save, provider, UI, audio, release, and signing. Do not infer pacing,
usability, or accessibility acceptance. Direct-main commit/push covers reviewed work only.
