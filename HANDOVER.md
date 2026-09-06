# Allrpcs Handover

## Current state

Phase 2l7 is implemented on `codex/phase-2l7-world-object-bound-resolution`, based directly on
PR #103's verified merge commit `9b932c7`.

The temporary R path still selects the first retained Seeker outcome in recorded order and remains
independent of current focus. It now passes both that exact outcome and the exact `WorldObject`
returned by `WorldObjectController.FindWorldObject` to the focused-search controller. The controller
rejects null inputs and target-mismatched objects before applying perception or consuming an outcome.

The focused controller and interaction smokes prove matching-object success, mismatched and null
rejection, successful resolution for both authored targets, and unchanged activity, outcome,
perception, world-object snapshot, looked-up object, source, target, and presentation identity on
rejection. Host-shell `./scripts/quality.sh` passed with a zero-warning build, 644 Domain tests, 107
Application tests, six architecture tests, all thirteen Godot smokes, and the bounded graphical
launch. No manual visual inspection was needed because visible behavior did not change.

The temporary source and controllers remain non-authoritative. World-object matching is still a
Godot-controller check; Domain resolution eligibility does not yet receive or validate a world
object.

## Exact next bounded slice

Take **Phase 2l8: world-object-bound focused-search resolution eligibility**.

Acceptance boundary:

1. Add one pure Domain policy that receives an existing `FocusedSearchOutcome`, `WorldObject`,
   `CharacterPerceptionSnapshot`, and `FocusedSearchOutcomeSnapshot`; reject null inputs and a
   target-mismatched world object, then delegate unchanged resolution eligibility to the existing
   `FocusedSearchOutcomeResolutionPolicy`.
2. Make `FocusedSearchActivityController.TryResolveOutcome` delegate its complete world-object-bound
   eligibility decision to that policy before invoking the unchanged resolution transition.
3. Add focused Domain tests and extend controller evidence for matching value-equal identifiers,
   mismatches, nulls, exact input retention, and fail-closed unchanged state; update README and
   ROADMAP evidence, run `./scripts/quality.sh`, review every intended path, and prune this file again
   to current evidence plus one next slice.

Explicit exclusions:

- Do not change resolution transition/result signatures, perception or outcome transition semantics,
  world-object snapshots or lookup, outcome ordering, or presentation rules.
- Do not make the temporary source or controllers authoritative; add descriptions, locations, item
  associations, filtering, visibility ranking, knowledge, evidence, events, persistence,
  serialization, providers, plugins, credentials, or new UI.
- Keep point-and-click movement, disease-time pausing, and the world-time debug overlay deferred.
- Do not merge, release, distribute, sign, notarize, or change repository visibility without
  explicit user authorization.
