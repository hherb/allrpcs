# Allrpcs Handover

## Current state

`main` contains merged PR #227 at `ee6c6dd`. The current branch is
`codex/phase-7-main-scene-sabotage` and advances three bounded Phase 7 scene slices:

- Phase 7dc adds the water cask at the stable tail of the existing scene world-object snapshot;
  the temporary sabotage source retains that exact object and complete snapshot, and `Main.tscn`
  initializes the source, concealment state, and flow without applying an effect;
- the next slice adds a reusable exact-snapshot concealment presentation controller whose
  compare-and-swap projection hides only its retained body and restores it on recovery;
- the composed temporary main-scene flow explicitly conceals and hides the water-cask body once,
  then recovers and reveals it only through noticed exact evidence.

The water-cask body remains visible during ordinary scene initialization. No sabotage action,
recovery, behavior choice, or timing occurs automatically. The new body is deliberately not yet an
interaction-focus target, and the focused-search context still covers only the herb patch and stone
marker.

## Validation and review

- Both implementation slices with new behavior began with the expected missing-type compilation
  failure.
- The game project builds with zero warnings and zero errors.
- Focused host-local Godot checks pass for exact scene-world binding, the two existing world-object
  seams, isolated sabotage source/flow compatibility, stale-safe concealment presentation, inert
  main-scene initialization, and explicit conceal/hide plus evidence-authorized recover/reveal.
- The restricted focused Godot run reproduced the known `user://logs` crash before project code;
  the identical host-local checks pass.
- `./scripts/quality.sh` passes workflow validation, restore, formatting, zero-warning build, all
  Domain, Application, Infrastructure, and Architecture tests, scenario validation, every
  credential-free headless Godot smoke, and the Compatibility-renderer graphical launch.
- Authored C# remains below 500 lines per file and within 120 characters per line;
  `git diff --check` passes.
- No live-conformance environment variable was enabled and no provider was invoked.

## Exact next bounded slice

Take **Phase 7df: bind the visible water cask to existing interaction focus without granting
sabotage or recovery authority**.

Acceptance boundary:

1. Make the existing `InteractionCandidates/WaterCask` body one `GreyboxInteractionTarget` whose
   independently parsed `ObjectIdText` is value-equal to the exact scene water-cask identifier.
2. Preserve the existing herb-patch and stone-marker target instances and their authored order;
   append the water cask only at the stable eligible tail while leaving hidden/excluded markers and
   their behavior unchanged.
3. Add one opt-in main-scene smoke that moves no actor but directly refreshes focus from a captured
   in-range Seeker position, proving the exact authoritative water-cask identity and display name
   are presented through the existing `WorldObjectController` lookup.
4. Prove out-of-range refresh clears that focus and that focus operations do not change sabotage
   source, concealment, evidence, presentation, world-object, or existing-target state.
5. Do not call either sabotage flow, add a focused-search condition or outcome for the cask,
   manufacture perception/evidence, recover concealment, choose behavior or timing, infer physical
   access from coordinates, add events/time/transcripts/persistence, or invoke a provider.
