# Allrpcs Handover

## Current state

Phase 8 and 9.1 are complete; Direction A is selected. Phase 9.2 has 74 manifest-bound
exports and 25 editable OpenRaster masters. Square–Garden production/runtime promotion
and its six runtime views are owner-accepted.

The 2026-10-02 Square–Sickhouse slice is external-only; the owner accepted all six corrected
views with "accepted" after their display. This is external visual acceptance only.
See the [feasibility result](art-direction/production-proof/corrected/review/square-sickhouse-path-feasibility.md)
for six failed strip views, six corrected 0.8x/1.0x/2.0x wide/tall views, recipes, and hashes.
The 380x360 overlay from `(2320,1140)` sits at `(2510,1320)`, `z=-71`, matches 1,713 visible
junction-overlap pixels, and clips at Sickhouse `(2640,1450)`. Runtime/production is unchanged.
The disposable capture hides the Seeker, screen overlays, and only the corrected route line;
world markers/labels retain layering. The Sickhouse obscures the far terminus. Existing
Garden-label overlap at high zoom and partial Sickhouse-label cropping at 2.0x wide remain
recorded limitations; the tall view shows the full Sickhouse label.

Use Pillow-enabled Python (`/opt/homebrew/bin/python3` here). Regular quality validates the
archived proof; the new feasibility check's `--check-live-baseline` additionally verifies this
slice's frozen ordinary scene/code hashes. Review fixed screen-panel obstruction; no unresolved
code finding remains. All validators, lint, negative evidence checks, formatting, zero-warning
build, 5,033 tests, scenario validation, and the full host Godot smoke suite passed.
Sandbox Godot log access required host retry. Compatibility launch passed.

## Exact next bounded slice

Obtain separate approval of the [named Square–Sickhouse promotion proposal](art-direction/production-proof/corrected/review/square-sickhouse-path-promotion-proposal.md).
It names one exact 380x360 export, one 760x720 editable master, independent imports, and one
presentation sprite at `(2510,1320)`, `z=-71`, replacing only the matching greybox line.
After approval, implement only that scope, validate/review it, and obtain a separate ordinary-scene
visual checkpoint. No promotion is authorized yet.

Preserve physical/gameplay authority, topology, collision, navigation, camera, scenario,
dialogue, clues, discovery, treatment, save, provider, UI, audio, release, and signing.
Do not infer pacing, usability, or accessibility acceptance. Direct-main commit/push is
authorized for reviewed work only.
