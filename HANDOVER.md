# Allrpcs Handover

## Current state

Phase 8 and Phase 9.1 are complete. Direction A is selected. The approved 3200x1800 opening region,
eleven anchors, twelve topology edges, closed forest gate, collision, navigation, camera, scenario,
and gameplay authority remain unchanged. Seven manifest-bound terrain/path assets have separate
runtime copies. The corrected external proof has 67 exports and eighteen editable OpenRaster
masters; every export has an exact isolated Godot import copy.

The product owner accepted the corrected isolated `terrain.opening_junction` proof for the live
Stream–Laundry–Woodland turn. It has no runtime copy, and `Main.tscn` does not reference it.

## Accepted correction and evidence

- The 512x512 export retains exact 96-pixel ports, 32-pixel connector depth, canonical `90°` and
  `32.400718°` headings, the exact `-57.599282°` live turn, and the `(224, 224)` pivot. A rigid
  `133.025066°` world rotation aligns both live routes without scaling or shearing.
- The rejected hard leg overlap was replaced by an alpha-aware texture warp around an 80-pixel
  tangent-continuous curve. The external preview clips both straight continuations at the module
  boundary, removing the rejected elbow triangle. The accepted visible footprint is 335x458 and
  SHA-256 is `fe2ccc012f3e024ad1cc99c41cefbe90581980239ff31cf1f5d3791a9e66fe68`.
- Validation reconstructs both exact connector pixel regions, rejects a hard texture overlap, dark
  resampling fringe, preview-leg leakage, and any visible pixel outside the curved 96-pixel route
  band. Rebuilds preserve the export, master, review, manifest, and isolated-import hashes.
- Host-local `./scripts/quality.sh` passed the proof/live-geometry/proposal/import/runtime validators,
  formatting, zero-warning build, 2,839 Domain, 925 Application, 1,261 Infrastructure, and 8
  architecture tests, scenario validation, every credential-free Godot smoke, and the Compatibility
  launch. The sandbox run reached Godot but could not write `user://` settings/logs; the identical
  host run passed.
- Final-head hosted run `36583132141` is Actions/account infrastructure state: all five jobs failed
  with `steps: []` and ran no checkout or product command. It is neither product-test evidence nor a
  product failure.
- Product-owner acceptance covers curve shape, texture continuity, apparent width, seams, and the
  removed preview triangle. Automated checks do not establish pacing, usability, or human
  accessibility acceptance.

## Exact next bounded slice

Stop at the runtime-promotion authorization gate. Obtain separate explicit product-owner approval
before creating a runtime copy of `terrain.opening_junction` or changing ordinary presentation.

If approved, promote only this exact manifest-bound export at the Stream–Laundry–Woodland turn,
retain the greybox collision/navigation/topology authority, prove exact source/hash/import parity and
placement/rotation, and present bounded review views before recording ordinary-scene acceptance.

Do not change any other route, path module, terrain, banks, water, structures, foliage, props,
characters, animation, UI systems, effects, audio, content-budget fills, collision, navigation,
locations, topology, camera tuning, scenario, dialogue, clue, discovery, treatment, save, provider,
or gameplay authority. Do not claim pacing, usability, or human accessibility acceptance.
