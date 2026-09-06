# Allrpcs Handover

## Current state

Phase 2l8 is implemented on `codex/phase-2l8-world-object-resolution-policy`, based directly on
PR #104's verified merge commit `cbdfbf8`.

`WorldObjectBoundFocusedSearchOutcomeResolutionPolicy.CanResolve` is the pure Domain boundary for
world-object-bound resolution eligibility. It rejects null inputs and a target-mismatched object,
accepts distinct value-equal target identifiers, and otherwise delegates unchanged to
`FocusedSearchOutcomeResolutionPolicy`. It retains the exact outcome, world object, perception,
outcome snapshot, and nested instances.

`FocusedSearchActivityController.TryResolveOutcome` now delegates its complete bound eligibility
decision to that policy before invoking the unchanged resolution transition. Focused controller
evidence covers value-equal recorded outcomes and world-object identifiers, mismatch and null
rejection, exact input retention, and fail-closed unchanged activity, outcome, perception, and
presentation state.

Host-shell `./scripts/quality.sh` passed with a zero-warning build, 653 Domain tests, 107 Application
tests, six architecture tests, all thirteen Godot smokes, and the bounded graphical launch. No
manual visual inspection was needed because visible behavior did not change.

The temporary source and controllers remain non-authoritative. The existing resolution transition
still does not receive or validate a world object.

## Exact next bounded slice

Take **Phase 2l9: world-object-bound focused-search resolution transition**.

Acceptance boundary:

1. Add one pure Domain transition that receives an existing `FocusedSearchOutcome`, `WorldObject`,
   `CharacterPerceptionSnapshot`, and `FocusedSearchOutcomeSnapshot`; delegate complete eligibility
   to `WorldObjectBoundFocusedSearchOutcomeResolutionPolicy`, reject ineligible resolution, then
   delegate unchanged resolution to `FocusedSearchOutcomeResolutionTransition` and return its exact
   result.
2. Make `FocusedSearchActivityController.TryResolveOutcome` invoke that transition after its
   uninitialized-perception check, then retain the transition result's exact perception and outcome
   snapshots.
3. Add focused Domain tests and extend controller evidence for value-equal identifiers, all
   delegated rejection paths, exact result/input retention, and fail-closed unchanged state; update
   README and ROADMAP evidence, run `./scripts/quality.sh`, review every intended path, and prune
   this file again to current evidence plus one next slice.

Explicit exclusions:

- Do not change the existing resolution policy, transition, or result signatures; perception or
  outcome transition semantics; world-object snapshots or lookup; outcome ordering; or presentation
  rules.
- Do not make the temporary source or controllers authoritative; add descriptions, locations, item
  associations, filtering, visibility ranking, knowledge, evidence, events, persistence,
  serialization, providers, plugins, credentials, or new UI.
- Keep point-and-click movement, disease-time pausing, and the world-time debug overlay deferred.
- Do not merge, release, distribute, sign, notarize, or change repository visibility without
  explicit user authorization.
