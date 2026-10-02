# Allrpcs Handover

## Current state

Phase 8 and 9.1 are complete; Direction A is selected. Phase 9.2 has 75 manifest-bound exports
and 26 editable OpenRaster masters. Square–Garden and Square–Sickhouse runtime views are
owner-accepted; the owner replied "approved" after the Sickhouse views on 2026-10-02.
Its exact named promotion remains complete and validated; see the
[runtime result](art-direction/production-proof/corrected/review/square-sickhouse-path-runtime-result.md).

The Square–Southern Road slice is external-only; the owner approved all six final external
views with "approved" after display on 2026-10-02. Named promotion approval is pending.
See the [feasibility result](art-direction/production-proof/corrected/review/square-southern-road-path-feasibility.md)
for six failed strip views, six final 0.8x/1.0x/2.0x wide/tall views, recipes, and hashes.
The world-aligned 520x416 overlay starts at `(1440,1024)` and draws at `(1700,1232)`, `z=-71`.
It matches all 1,662 visible junction-intersection pixels and clips at Southern Road `(1500,1400)`.
The canvas was expanded to preserve an exposed shoulder; no runtime/production file changed.
Only the matching route line is hidden in the disposable corrected proof. The Seeker/screen
panels are hidden and the camera reframed; world markers/labels retain ownership and layering.
The far end remains a flat textured patch across the untextured road junction, partly covered
by the road presentation. High-zoom labels largely fall outside frame; overview views provide
context. The owner accepted this external presentation, including the recorded flat endpoint limitation.

Use Pillow-enabled Python (`/opt/homebrew/bin/python3` here). The new feasibility validator is
in regular quality; `--check-live-baseline` additionally verifies this slice's frozen runtime
inputs. Lint, deterministic pixels, baseline, and negative evidence/sampler checks passed.
Review fixed the cropped shoulder; no unresolved code finding remains. All validators, formatting, zero-warning build, 5,033 tests, scenario validation, and the full
host Godot smoke suite passed. Sandbox Godot user-log restrictions required the identical host
retry; no product workaround was introduced. Compatibility launch passed.

## Exact next bounded slice

Obtain separate approval of the [named Southern Road promotion proposal](art-direction/production-proof/corrected/review/square-southern-road-path-promotion-proposal.md).
It names one exact 520x416 export, one 1040x832 editable master, independent imports, and
one sprite at `(1700,1232)`, `z=-71`, with repeat disabled, replacing only the matched line.
After approval, implement only that scope, validate/review it, and obtain the separate
ordinary-scene visual checkpoint. No Southern Road promotion is authorized yet.

Preserve physical/gameplay authority, topology, collision, navigation, camera, scenario, dialogue,
clues, discovery, treatment, save, provider, UI, audio, release, and signing. Do not infer pacing,
usability, or accessibility acceptance. Direct-main commit/push covers reviewed work only.
