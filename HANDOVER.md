# Allrpcs Handover

## Current state

Phase 2l6 is implemented on `codex/phase-2l6-world-object-gated-resolution`, based directly on
PR #102's verified merge commit `00c6ce2`.

Slice commit `e6601f2` is pushed, and draft PR #103
(`https://github.com/hherb/allrpcs/pull/103`) is open. Pull-request merge remains separately
unauthorized.

The temporary R path still selects the first retained Seeker outcome in recorded order. It now
asks `WorldObjectController.FindWorldObject` for that outcome's target and invokes the existing
focused-search resolution boundary only when the object exists. Both scene-authored targets still
resolve independently of current focus. An absent target reports `UNCHANGED` and retains the exact
activity, outcome, perception, world-object, source, target, and presentation state.

The interaction smoke failed on the controlled absent-world-object case before the gate existed.
After implementation and code review, host-shell `./scripts/quality.sh` passed with a zero-warning
build, 644 Domain tests, 107 Application tests, six architecture tests, all thirteen Godot smokes,
and the bounded graphical launch. The initial sandbox-only Godot attempt hit the known
`user://logs` access restriction before scene startup; no product failure was inferred. No manual
visual inspection was needed because visible behavior did not change.

The temporary source and controller remain non-authoritative. The looked-up world object is only an
existence gate and is not yet supplied to or validated by the focused-search resolution controller.

## Exact next bounded slice

Take **Phase 2l7: world-object-bound focused-search controller resolution**.

Acceptance boundary:

1. Require the existing focused-search controller resolution boundary to receive the exact looked-up
   `WorldObject` alongside the recorded outcome, and reject null or target-mismatched objects before
   applying or consuming anything.
2. Make only the temporary R path pass the exact instance returned by `FindWorldObject`; preserve
   first-recorded selection, focus independence, successful resolution for both authored targets,
   and all current presentation behavior.
3. Extend focused controller and interaction smokes to prove exact input use plus unchanged snapshot,
   object, source, target, and presentation identity for mismatched and rejected inputs.
4. Keep the broader authoritative-world roadmap item incomplete, run `./scripts/quality.sh`, review
   every intended path, and prune this file again to current evidence plus one next slice.

Explicit exclusions:

- Do not change Domain eligibility or transition contracts, outcome ordering, world-object snapshots
  or lookup semantics, perception transitions, or presentation rules.
- Do not make the temporary source or controller authoritative; add descriptions, locations, item
  associations, filtering, visibility ranking, knowledge, evidence, events, persistence,
  serialization, providers, plugins, credentials, or new UI.
- Keep point-and-click movement, disease-time pausing, and the world-time debug overlay deferred.
- Do not merge, release, distribute, sign, notarize, or change repository visibility without
  explicit user authorization.
