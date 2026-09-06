# Allrpcs Handover

## Current state

Phase 2l11 is implemented on `codex/phase-2l11-world-object-outcome-source`, based directly on
PR #107's verified merge commit `6e1b40e8`.

`TemporaryFocusedSearchOutcomeSource.CreateOutcome` now receives an existing
`FocusedSearchRequest` plus an existing nullable `WorldObject`. It rejects a null request, returns
no outcome for a null or target-mismatched object, and accepts exact or distinct value-equal object
identifiers. Successful calls return distinct outcomes retaining the exact request and existing
`VisibilityState.Identified` singleton without changing the world object or its nested inputs.

The temporary E path resolves only the active request target through `WorldObjectController` and
passes the exact returned object to the source. It calls `TryCompleteSearchWithOutcome` only when an
outcome was created. A missing object reports `UNCHANGED` and preserves focused-search,
world-object, target, perception, and presentation state.

The source smoke failed first on the intentionally unsupported two-argument call. After
implementation and code review, host-shell `./scripts/quality.sh` passed with a zero-warning build,
661 Domain tests, 107 Application tests, six architecture tests, all thirteen Godot smokes, and the
bounded graphical launch. No manual visual inspection was needed because authored appearance did
not change.

The temporary sources, controllers, lifecycle controls, and identified-marker presentation remain
non-authoritative.

## Exact next bounded slice

Take **Phase 2l12: world-object-bound temporary Seeker perception authoring**.

Acceptance boundary:

1. Make `TemporarySeekerPerceptionSource` derive its initial object entries from the existing
   `TemporaryWorldObjectSource.WorldObjects` snapshot rather than reading greybox interaction
   targets directly. Retain the exact controller-owned Seeker identifier, exact world-object
   identifier instances in snapshot order, and existing `VisibilityState.Noticed` singleton.
2. Keep one stable retained perception snapshot and leave the supplied world-object snapshot,
   objects, identities, descriptions, Seeker identifier, controllers, targets, and presentation
   unchanged.
3. Extend credential-free evidence for exact empty and populated snapshot behavior, stable order and
   nested input retention, and unchanged controller and presentation state.
4. Update README and ROADMAP evidence, run `./scripts/quality.sh`, review every intended path, and
   prune this file again to current evidence plus one next slice.

Explicit exclusions:

- Do not change Domain perception, outcome, completion, or resolution contracts; initial `Noticed`
  or outcome `Identified` semantics; world-object snapshots or lookup; or resolved-perception
  presentation rules.
- Do not make temporary sources or controllers authoritative; expose object identity text or
  descriptions; add discovery filtering, knowledge, evidence, events, persistence, serialization,
  providers, plugins, credentials, or new UI.
- Keep point-and-click movement, disease-time pausing, and the world-time debug overlay deferred.
- Do not merge, release, distribute, sign, notarize, or change repository visibility without
  explicit user authorization.
