# Allrpcs Handover

## Current state

Phase 2l5 is implemented on `codex/phase-2l5-world-object-controller-lookup`, based directly on
PR #101's verified merge commit `548687a`.

Slice commit `edbc9c9` is pushed, and draft PR #102
(`https://github.com/hherb/allrpcs/pull/102`) is open. Pull-request merge remains separately
unauthorized.

`WorldObjectController.FindWorldObject` rejects a null identifier and returns no object while the
controller is uninitialized or the identifier is absent. Otherwise it delegates to
`WorldObjectLookup.FindById` against the initialized snapshot and returns the exact retained
`WorldObject` for a value-equal identifier. No ordinary-play path calls the lookup yet.

The expanded opt-in controller smoke covers both scene-authored objects, a distinct value-equal
identifier, absent and uninitialized lookups, null rejection in both controller states, exact
snapshot and object retention, and unchanged source, targets, focused-search state, and
presentation.

The test-first game build failed with `CS1061` before the lookup existed. After implementation and
code review, host-shell `./scripts/quality.sh` passed with a zero-warning build, 644 Domain tests,
107 Application tests, six architecture tests, all thirteen Godot smokes, and the bounded
graphical launch. The sandbox-only gate reached the known VSTest loopback denial after building
cleanly. No manual visual inspection was needed because visible behavior did not change.

## Exact next bounded slice

Take **Phase 2l6: world-object-gated recorded-outcome resolution input**.

Acceptance boundary:

1. In only the temporary R input path, retain the existing first-recorded Seeker outcome
   selection, ask `WorldObjectController.FindWorldObject` for that outcome's target identifier,
   and invoke the existing focused-search resolution boundary only when the object exists.
2. Preserve successful, focus-independent resolution for both scene-authored targets. Add a
   controlled absent-world-object outcome case to the interaction smoke and prove R reports
   unchanged while retaining the exact activity, outcome, perception, world-object, source,
   target, and presentation state.
3. Preserve F/C/E, movement, healer input, ordinary focus behavior, exact-instance ownership, and
   all existing failure behavior. Do not add another input, source, controller, or presentation.
4. Document Phase 2l6 compactly, leave the broader authoritative-world and focused-search roadmap
   item incomplete, run `./scripts/quality.sh`, review every intended path, and prune this file to
   current evidence plus one next slice.

Explicit exclusions:

- Do not change Domain eligibility or transition contracts, focused-search controller signatures,
  world-object snapshots, lookup semantics, perception transitions, outcome ordering, or
  presentation rules.
- Do not make the temporary source or controller authoritative, add non-empty descriptions,
  locations, item associations, filtering, visibility ranking, knowledge, evidence, events,
  persistence, serialization, providers, plugins, credentials, or new UI.
- Keep point-and-click movement, disease-time pausing, and the world-time debug overlay deferred
  until their retained prototype or authoritative-system prerequisites exist.
- Do not merge, release, distribute, sign, notarize, or change repository visibility without
  explicit user authorization.
