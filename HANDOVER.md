# Allrpcs Handover

## Current state

Phase 8 and Phase 9.1 remain complete. The approved 3200x1800 opening-region proposal is the live
greybox layout with proposal SHA-256
`a5ebd5e23b5f41a8958477c953ba2763afffcb636f3bb6f69a969c0c1d57f8f1`, eleven exact anchors,
twelve topology edges, 96-pixel route corridors, 24-pixel navigation clearance, bounded camera
framing, and 0.8/1.0/2.0 zoom tuning. The north-west Closed Forest Path remains physically gated
until the existing Forester access removes its gate.

Direction A remains selected and Direction D is only the contingency. The 66 proof exports,
seventeen editable OpenRaster masters, and isolated `DirectionAImportProof.tscn` remain proof-only;
`Main.tscn` still references none of that art. The proposal's five empty-area budgets remain
non-authoritative.

## Completed product-owner checkpoint and corrections

- The product owner traversed all eleven anchors and every open connection. Square-to-Sickhouse,
  Square-to-Garden, Stream/Laundry/Woodland, Southern Road/Wagon, and the remaining connections
  were clear after the corrections below. The Closed Forest Path read as intentionally closed and
  the roughly 6.5-second Shrine-to-Southern-Road connection did not feel too long.
- Camera framing remained acceptable at 0.8, 1.0, and 2.0 zoom in the 1280x720 window and while the
  window was resized through representative taller and other shapes.
- The checkpoint exposed a generated-route-mask defect: the visually filled Village Edge and
  Sickhouse interiors admitted only narrow route-centreline movement. Both represented interiors
  now participate in physical and navigation masks while their existing walls, approved exits,
  Sickhouse doorway, anchors, topology, collision authority, and gameplay rules remain unchanged.
- The Stream was physically reachable but not discoverable. The authored scene and runtime now
  show the same blue stream band, brown crossing, and `STREAM` label; stale pre-reflow Stream and
  Laundry editor placeholders were corrected. The product owner then reached and crossed it.

## Review, evidence, and limits

- Focused regressions prove off-centre Village Edge and Sickhouse movement on both navigation and
  physical surfaces, retain the Sickhouse doorway and patient route, retain all village openings,
  and bind the visible Stream presentation to its exact geometry. Adjacent Stream/Laundry and
  movement-boundary checks pass.
- `python3 art-direction/world-layout-proposal/tools/validate.py` and host-local
  `./scripts/quality.sh` pass the proposal checks, zero-warning build, all 5,033 .NET tests,
  scenario validation, every credential-free Godot smoke, formatting, and Compatibility launch.
- Cumulative review removed Godot editor-generated UID/layout normalization from `Main.tscn` and
  `project.godot`; only the scoped runtime, smoke, documentation, and authored Stream changes remain.
  It also increased the Southern Road smoke's bounded synchronization allowance after one cold full
  run timed out; the exact retry and the subsequent complete cold run passed without assertion changes.
- This checkpoint supplies product-owner greybox traversal, pacing, gate-readability, and framing
  evidence. It is not independent-new-player or final accessibility acceptance.

## Exact next bounded slice

Promote only the existing manifest-bound `terrain.quiet_moss` tile into a runtime-owned terrain
directory with exact source/hash parity and repeat-enabled lossless import. Replace only the flat
3200x1800 `Ground` polygon with that repeating tile behind all existing greybox presentation, then
add a credential-free smoke proving exact texture identity, 256x256 source geometry, full world
coverage, draw ordering, and no reference to the proof-only directory.

Do not integrate leaf litter, transitions, packed-earth path modules, water art, structures,
foliage, props, characters, animation, UI, effects, or audio in that slice. Do not change collision,
navigation, locations, topology, scenario, dialogue, clue, discovery, treatment, save, provider, or
gameplay authority; do not fill the five non-authoritative content budgets. Keep the proof imports
isolated and request one graphical product-owner review of the runtime ground at 0.8/1.0/2.0 before
continuing to another production asset.
