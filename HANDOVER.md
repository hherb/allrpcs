# Allrpcs Handover

## Current state

Phase 2l4 is implemented on `codex/phase-2l4-world-object-controller`, based directly on PR #100's
verified merge commit `f1fa5a2`.

Slice commit `05f78bc` is pushed, and draft PR #101
(`https://github.com/hherb/allrpcs/pull/101`) is open. Pull-request merge remains separately
unauthorized.

The scene now owns one separate `WorldObjectController`. `Main._Ready` passes the temporary
world-object source's exact retained snapshot to its one-time initialization boundary and fails
explicitly if initialization cannot be established. The controller retains the exact first
snapshot instance through an optional get/private-set property, throws for null, and fails closed
on repeated initialization without changing the source, targets, focused-search state, or
presentation. It does not yet query or otherwise use the world objects.

The test-first game build failed with `CS0246` before the controller existed. After implementation
and code review, host-shell `./scripts/quality.sh` passed with a zero-warning build, 644 Domain
tests, 107 Application tests, six architecture tests, all thirteen Godot smokes, and the bounded
graphical launch. The sandbox-only gate reached the known VSTest loopback denial after building
cleanly. No manual visual inspection was needed because visible behavior did not change.

## Exact next bounded slice

Take **Phase 2l5: controller-owned exact world-object lookup**.

Acceptance boundary:

1. Add one narrow controller lookup accepting an existing `ObjectId` and delegating selection to
   `WorldObjectLookup.FindById` against the initialized snapshot.
2. Return the exact retained `WorldObject` instance for a value-equal identifier; return no object
   when the controller is uninitialized or the identifier is absent; reject null without changing
   controller state.
3. Extend the opt-in credential-free controller smoke for both scene-authored targets, a
   value-equal distinct identifier, absent and uninitialized lookups, null rejection, stable exact
   snapshot retention, and unchanged source, targets, focused-search state, and presentation.
4. Document Phase 2l5 compactly, leave the broader authoritative-world and focused-search roadmap
   item incomplete, run `./scripts/quality.sh`, review every intended path, and prune this file to
   current evidence plus one next slice.

Explicit exclusions:

- Do not call the lookup from `Main`, interaction focus, focused-search lifecycle or outcome
  resolution; change perception or presentation; grant discovery; or alter ordinary-play behavior.
- Do not make the temporary source or controller authoritative, add non-empty descriptions,
  locations, item associations, filtering, visibility ranking, knowledge, evidence, events,
  persistence, serialization, providers, plugins, credentials, or new UI.
- Keep point-and-click movement, disease-time pausing, and the world-time debug overlay deferred
  until their retained prototype or authoritative-system prerequisites exist.
- Do not merge, release, distribute, sign, notarize, or change repository visibility without
  explicit user authorization.
