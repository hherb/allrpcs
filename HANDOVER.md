# Allrpcs Handover

## Current state

Phase 2l9 is implemented on `codex/phase-2l9-world-object-resolution-transition`, based directly
on PR #105's verified merge commit `9825bbe2`.

`WorldObjectBoundFocusedSearchOutcomeResolutionTransition.Resolve` is the pure Domain boundary for
world-object-bound resolution. It delegates complete eligibility to the existing bound policy,
rejects every mismatched or otherwise ineligible resolution, delegates unchanged state creation to
`FocusedSearchOutcomeResolutionTransition`, and returns its exact result. Distinct value-equal
target identifiers are accepted; the world object, all other inputs, stable order, and unrelated
retained instances remain unchanged.

`FocusedSearchActivityController.TryResolveOutcome` retains its Boolean fail-closed eligibility
gate, then delegates successful state creation to the new bound transition and installs the exact
returned perception and outcome snapshots. Its credential-free smoke now also covers a retained
outcome whose target is absent from perception. Activity, world-object, input, and presentation
state remain unchanged on success or rejection as applicable.

The eight focused tests failed first because the transition type was absent. After implementation
and code review, host-shell `./scripts/quality.sh` passed with a zero-warning build, 661 Domain
tests, 107 Application tests, six architecture tests, all thirteen Godot smokes, and the bounded
graphical launch. No manual visual inspection was needed because visible behavior did not change.

The temporary source, controllers, and identified-marker presentation remain non-authoritative.

## Exact next bounded slice

Take **Phase 2l10: world-object-bound resolved-perception presentation**.

Acceptance boundary:

1. Make `TemporaryResolvedPerceptionIndicator.Present` receive an existing nullable `WorldObject`
   instead of a raw `ObjectId`, then use only that object's exact stable identifier with the
   existing perception lookup and exact-`Identified` presentation rule.
2. In `Main.RefreshResolvedPerceptionPresentation`, resolve each existing visible target through
   `WorldObjectController.FindWorldObject` and pass only the exact returned object plus the
   controller's current perception. A missing object must hide the marker and change no state.
3. Extend focused credential-free evidence for null, absent, unrelated, non-identified, exact, and
   distinct value-equal identifiers; exact world-object/input retention; successful target-only
   refresh; and failed or repeated-resolution presentation preservation.
4. Update README and ROADMAP evidence, run `./scripts/quality.sh`, review every intended path, and
   prune this file again to current evidence plus one next slice.

Explicit exclusions:

- Do not display or expose world-object identity text or descriptions; change visibility ranking,
  perception or outcome transition semantics, world-object snapshots or lookup, outcome ordering,
  or focused-search selection and presentation rules.
- Do not make the temporary source or controllers authoritative; add descriptions, locations, item
  associations, filtering, knowledge, evidence, events, persistence, serialization, providers,
  plugins, credentials, or new UI.
- Keep point-and-click movement, disease-time pausing, and the world-time debug overlay deferred.
- Do not merge, release, distribute, sign, notarize, or change repository visibility without
  explicit user authorization.
