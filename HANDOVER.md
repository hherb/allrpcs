# Allrpcs Handover

## Current state

Phase 8 and Phase 9.1 are complete. Direction A is selected. The approved 3200x1800 opening
region retains eleven anchors, twelve topology edges, the closed forest gate, and unchanged
collision, navigation, camera, scenario, and gameplay authority. Seven manifest-bound terrain/path
assets have separate runtime copies; the other Direction A assets remain isolated proofs. The
product owner approved the rounded Woodland Clearing perimeter and final Laundry-to-Clearing
straight path/end-cap presentation in the supplied 0.8x, 1.0x, and 2.0x wide/tall views.

## Review, evidence, and limits

- The proof-only junction check derives `-136.974934°` for Stream-to-Laundry and `165.425784°`
  for Laundry-to-Clearing from live anchors. The required `-57.599282°` turn differs from the
  exact bend's 90° ports by `32.400718°`. No rigid placement of the existing straight and bend
  modules can join both 96-pixel route headings without a seam or presentation drift. See
  `art-direction/production-proof/corrected/review/opening-junction-feasibility.md` and rerun
  `python3 art-direction/production-proof/corrected/tools/check_opening_junction_feasibility.py`.
- The corrected 66-asset proof validator passed its export hashes and exact 96-pixel bend port
  checks. Host-local `./scripts/quality.sh` passed the art/proposal/import/runtime validators,
  zero-warning build, all 5,033 .NET tests, scenario validation, credential-free Godot smokes,
  formatting, and the Compatibility launch. Its sandbox run reached Godot but could not write
  `user://` settings/logs; the identical host run passed.
- Hosted CI run `36566160795` had `steps: []` in all five jobs and ran no product validation.
  Automated checks and supplied visual approvals do not establish pacing, usability, or human
  accessibility acceptance.

## Exact next bounded slice

Stop at the junction proof gate and obtain product-owner approval before generating one
junction-specific proof module with two exact 96-pixel ports following the live 57.599282° turn.
If approved, first demonstrate its footprint, pixel seams, and containment in the unchanged
physical route outside `Main.tscn`; keep it isolated until separately authorized for runtime use.

Do not promote the path bend, T-junction, cross-junction, another end cap, either inset ground corner,
another route, banks, water, structures, foliage, props, characters, animation, UI systems, effects,
audio, or content-budget fills. Do not change collision, navigation, locations, topology, camera
tuning, scenario, dialogue, clue, discovery, treatment, save, provider, or gameplay authority. Do
not claim pacing, usability, or human accessibility acceptance.
