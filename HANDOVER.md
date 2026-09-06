# Allrpcs Handover

## Current state

Phase 2l10 is implemented on `codex/phase-2l10-world-object-resolved-presentation`, based directly
on PR #106's verified merge commit `dfacfd04`.

`TemporaryResolvedPerceptionIndicator.Present` now receives an existing nullable `WorldObject` and
uses only its exact stable identifier with the existing perception lookup. It shows the temporary
marker only for the exact `VisibilityState.Identified` singleton. Null objects or perception,
absent or unrelated entries, and every other visibility state hide the marker. Distinct value-equal
object identifiers are accepted, and object identity, description, perception, and nested inputs
remain unchanged.

`Main.RefreshResolvedPerceptionPresentation` resolves each visible target through
`WorldObjectController.FindWorldObject` and passes only the exact returned object plus the
controller's current perception. A missing object hides only its marker without changing
world-object, focused-search, target, result-text, or unrelated presentation state. Successful
resolution refreshes only the resolved target; failed and repeated resolution preserve the prior
presentation.

The focused smoke failed first because the presentation boundary still required `ObjectId`. After
implementation and code review, host-shell `./scripts/quality.sh` passed with a zero-warning build,
661 Domain tests, 107 Application tests, six architecture tests, all thirteen Godot smokes, and the
bounded graphical launch. No manual visual inspection was needed because the existing marker's
authored appearance did not change.

The temporary sources, controllers, and identified-marker presentation remain non-authoritative.

## Exact next bounded slice

Take **Phase 2l11: world-object-bound temporary focused-search outcome creation**.

Acceptance boundary:

1. Make `TemporaryFocusedSearchOutcomeSource.CreateOutcome` receive an existing
   `FocusedSearchRequest` plus an existing nullable `WorldObject`. Preserve null-request rejection,
   return no outcome for a null or target-mismatched object, and otherwise create a distinct outcome
   retaining the exact request and existing `VisibilityState.Identified` singleton.
2. In `Main`'s completion path, resolve the active request target through
   `WorldObjectController.FindWorldObject`, pass only the exact returned object to the temporary
   source, and call `TryCompleteSearchWithOutcome` only when an outcome was created. A missing object
   must report `UNCHANGED` and preserve all focused-search, world-object, target, and presentation
   state.
3. Extend focused credential-free evidence for null, absent, mismatched, exact, and distinct
   value-equal identifiers; exact request, world-object, and nested input retention; successful
   completion; and fail-closed missing-object state and presentation preservation.
4. Update README and ROADMAP evidence, run `./scripts/quality.sh`, review every intended path, and
   prune this file again to current evidence plus one next slice.

Explicit exclusions:

- Do not change Domain outcome, completion, or resolution policies, transitions, or result types;
  the hard-coded temporary `Identified` result; perception semantics; world-object snapshots or
  lookup; outcome ordering; or resolved-perception presentation rules.
- Do not make the temporary source or controllers authoritative; expose object identity text or
  descriptions; add locations, item associations, filtering, knowledge, evidence, events,
  persistence, serialization, providers, plugins, credentials, or new UI.
- Keep point-and-click movement, disease-time pausing, and the world-time debug overlay deferred.
- Do not merge, release, distribute, sign, notarize, or change repository visibility without
  explicit user authorization.
