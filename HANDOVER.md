# Allrpcs Handover

## Current state

Phase 8 and Phase 9.1 are complete. Direction A is selected. The approved 3200x1800 opening region,
eleven anchors, twelve topology edges, closed forest gate, collision, navigation, camera, scenario,
and gameplay authority remain unchanged. Seven manifest-bound terrain/path assets have separate
runtime copies. The corrected external proof now has 67 exports and eighteen editable OpenRaster
masters; every export has an exact isolated Godot import copy.

After explicit product-owner approval, `terrain.opening_junction` was generated for the live
Stream–Laundry–Woodland turn. It remains isolated in `DirectionAImportProof.tscn`; it has no runtime
copy and `Main.tscn` does not reference it.

## Review, evidence, and limits

- Live headings remain `-136.974934°` and `165.425784°`, requiring the exact `-57.599282°` turn.
  The new 512x512 export uses canonical `90°` and `32.400718°` headings; a rigid `133.025066°`
  world rotation aligns both live routes without scaling or shearing.
- The manifest records exact 96-pixel ports, 32-pixel connector depth, a `(224, 224)` pivot, a
  336x458 visible footprint, and SHA-256
  `10fa7b38789eea75342c8eb26024e922e30dc9c7773642afc1222aa89b5e52c2`. Validation reconstructs
  both connector pixels and rejects any visible pixel beyond the unchanged 96-pixel physical-route
  bands. Rebuilds preserve the export, master, review, and manifest hashes.
- `review/opening-junction-review.png` shows exact textured continuation on the left and the route
  containment mask on the right. It has been mechanically and session-reviewed, but the product
  owner has not yet accepted its visual curve or texture continuity.
- Host-local `./scripts/quality.sh` passed the 67-asset proof/live-geometry/proposal/import/runtime
  validators, formatting, zero-warning build, all 5,033 .NET tests, scenario validation, every
  credential-free Godot smoke, and the Compatibility launch. The sandbox run reached Godot but
  could not write `user://` settings/logs; the identical host run passed.
- Hosted CI remains Actions/account infrastructure state: implementation run `36572631551` and
  handover run `36572758137` both reported failure with `steps: []` in all five jobs and ran no
  checkout or product command. They are not product-test evidence or product failures.
- Automated checks do not establish visual acceptance, pacing, usability, or human accessibility
  acceptance.

## Exact next bounded slice

Stop at the isolated-proof review gate. Present
`art-direction/production-proof/corrected/review/opening-junction-review.png` and obtain explicit
product-owner acceptance or one bounded correction for the curve, texture continuity, apparent
96-pixel width, and lack of visible seams. The right panel is mechanical containment evidence, not
a proposed game presentation.

Even after visual acceptance, obtain separate explicit approval before creating a runtime copy or
changing ordinary presentation. Do not change `Main.tscn` in the review slice.

Do not promote the path bend, T-junction, cross-junction, another end cap, either inset ground corner,
another route, banks, water, structures, foliage, props, characters, animation, UI systems, effects,
audio, or content-budget fills. Do not change collision, navigation, locations, topology, camera
tuning, scenario, dialogue, clue, discovery, treatment, save, provider, or gameplay authority. Do
not claim pacing, usability, or human accessibility acceptance.
