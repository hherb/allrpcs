# Allrpcs Handover

## Current state

Phase 8 and Phase 9.1 are complete; Direction A is selected. Phase 9.2 has 72 manifest-bound
exports and 23 layered OpenRaster masters. The latest Stream water-and-bank proof is external to
ordinary gameplay. Its three assets, exact 400x96 band, centered 128x96 crossing, two collision
polygons, 200-pixel clear opening, and six 0.8x/1.0x/2.0 wide/tall views passed deterministic
validation and isolated Godot import parity. The product owner accepted those six external views
without requesting changes. This did not authorize runtime promotion or establish pacing,
usability, or human accessibility acceptance.

`Main.tscn` still uses the greybox Stream presentation. Runtime art, collision, navigation,
topology, camera behavior, scenario, and gameplay authority are unchanged. The previous host-local
`./scripts/quality.sh` run passed all validators, a zero-warning build, 5,033 .NET tests, scenario
validation, credential-free Godot smoke, formatting, and Compatibility launch. The sandbox Godot
`user://logs` permission crash required that identical host-local retry.

## Exact next bounded slice

The proposal in
[`stream-runtime-promotion-proposal.md`](art-direction/production-proof/corrected/review/stream-runtime-promotion-proposal.md)
specifies the three assets, exact placement, validation, and exclusions. Obtain separate explicit
product-owner approval of that proposal before editing runtime files. If approved, implement only
that slice, run its acceptance checks, and provide the six ordinary-scene views for separate visual
review. Do not infer runtime approval from the external feasibility acceptance or this handover.
