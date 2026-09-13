# Allrpcs Handover

## Current state

`main` contains merged PR #228 at `bf9a6b5`. The current branch is
`codex/phase-7-water-cask-focus` and advances three bounded Phase 7 slices:

- Phase 7df converts the existing visible water-cask body into the stable third eligible
  `GreyboxInteractionTarget`; its independently parsed identifier is value-equal to the exact tail
  scene world object, and explicit-position focus refresh presents that authoritative identity and
  display name without moving the Seeker;
- Phase 7dg appends one exact `inspect-water-sources` capability requirement and matching Seeker
  capability for the water cask while retaining the existing herb capability, stone-marker tool,
  authored order, and exact instances;
- Phase 7dh routes only successful resolution of the exact water-cask focused-search outcome to the
  existing evidence-gated concealment recovery flow. Resolving another target leaves the cask
  concealed; exact cask resolution recovers and reveals it.

Sabotage still never occurs automatically. The new focused-search route does not select enemy
behavior or timing, infer physical access from coordinates, move an actor or object, rewrite an
authoritative world object, consume evidence, persist sabotage state, or invoke a provider.

## Validation and review

- Each behavior slice began with an expected failing check: Phase 7df lacked the explicit-position
  refresh seam, Phase 7dg lacked cask investigation context, and Phase 7dh left the cask
  concealed after exact outcome resolution.
- The game project builds with zero warnings and zero errors.
- Focused host-local Godot checks pass for exact water-cask focus, appended investigation context,
  the existing interaction-focus and world-object source contracts, and exact investigation-driven
  recovery with unrelated-outcome rejection.
- The restricted focused Godot run reproduced the known `user://logs` crash before project code;
  the identical host-local check passes.
- Host-local `./scripts/quality.sh` passes workflow validation, restore, formatting, zero-warning
  build, 2,276 Domain tests, 478 Application tests, 532 Infrastructure tests, 8 architecture tests,
  scenario validation, every credential-free headless Godot smoke, and the Compatibility-renderer
  graphical launch.
- Authored changed C# files are within 120 characters per line; both new smoke files remain below
  500 lines. No live-conformance environment variable was enabled and no provider was invoked.

## Exact next bounded slice

Take **Phase 7di: select one exact currently eligible concealment candidate without applying it**.

Acceptance boundary:

1. Add one pure Domain selection operation over a caller-supplied bounded ordered collection of
   `CharacterObjectSabotageEvidence` candidates plus the complete existing character, access,
   world-object, concealment, and evidence snapshots.
2. Materialize and validate the complete collection before selection: reject null inputs, null
   entries, over-bound collections, and value-equal duplicate evidence identifiers without
   mutating any input.
3. Delegate eligibility to the existing complete concealment policy and return the exact first
   eligible retained candidate in stable caller order, or `null` when none is eligible.
4. Prove in focused Domain tests that inaccessible, incapable, unknown, missing-target,
   already-concealed, stale-evidence, and non-conceal candidates are skipped; an earlier ineligible
   candidate must not block a later eligible one.
5. Do not apply or recover concealment, append evidence, mutate truth or perception, choose timing,
   wire the scene, add input/UI/events/transcripts/persistence, construct model context, or invoke a
   provider.
