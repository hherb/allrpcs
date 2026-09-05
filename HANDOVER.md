# Allrpcs Handover

## Current state

Phase 2k35 is implemented and reviewed without findings on
`codex/phase-2k35-outcome-resolution-result`, based directly on PR #82's verified merge commit
`2896ad6`.

`Allrpcs.Domain.Discovery.FocusedSearchOutcomeResolutionResult` is one sealed immutable record
retaining exact existing `CharacterPerceptionSnapshot` and `FocusedSearchOutcomeSnapshot`
instances through get-only `Perception` and `Outcomes` properties. It rejects either null input;
record equality uses the retained snapshot instances. It performs no resolution, transition,
lookup, validation, or mutation behavior.

The four focused tests failed first with `CS0246` while the result type was absent, then passed
after the implementation. Host-shell `./scripts/quality.sh` passed with a zero-warning build, 588
Domain tests, 107 Application tests, six architecture tests, all six Godot smokes, and the bounded
graphical launch. The sandbox-only focused test attempt was inconclusive because VSTest could not
bind its required local socket. No runtime Godot behavior changed, so no separate manual visual
acceptance was performed. Hosted desktop outputs remain unsigned development evidence, not
releases.

## Exact next bounded slice

Take **Phase 2k36: pure focused-search outcome resolution transition**.

Acceptance boundary:

1. Add one static `Allrpcs.Domain.Discovery.FocusedSearchOutcomeResolutionTransition` with one
   `Resolve` operation accepting an existing `FocusedSearchOutcome`,
   `CharacterPerceptionSnapshot`, and `FocusedSearchOutcomeSnapshot`, and returning a
   `FocusedSearchOutcomeResolutionResult`.
2. Delegate complete eligibility to `FocusedSearchOutcomeResolutionPolicy.CanResolve`; reject an
   ineligible resolution with `InvalidOperationException` and leave every input unchanged.
3. For an eligible resolution, delegate the perception change to
   `FocusedSearchOutcomeApplicationTransition.Apply`, delegate outcome removal to
   `FocusedSearchOutcomeConsumptionTransition.Consume`, and return one result retaining those
   exact returned snapshot instances. Do not duplicate their selection or transition rules.
4. Add focused Domain tests for successful combined resolution, stable order and exact retained
   unrelated instances, every delegated rejection category, unchanged inputs, exact result
   snapshot retention, and the intended single-operation static surface.
5. Document Phase 2k36 in `README.md` and compact `ROADMAP.md`, leave the broader focused-search
   item incomplete, run `./scripts/quality.sh`, review every intended path, and replace this file
   with current evidence, limits, and one next bounded slice.

Explicit exclusions:

- Do not connect resolution to focused-search completion or add outcome production, discovery
  grants, advancement policy, events, effects, reason/error taxonomies, progress, duration,
  automatic lifecycle behavior, authoritative world objects, or new Godot/Application behavior.
- Do not add complete-character inspection, ownership/capacity, world state, location,
  coordinates, range policy, passive noticing, reinterpretation, serialization, persistence,
  providers, plugins, credentials, or final interaction UI.
- Keep point-and-click movement, disease-time pausing, and the world-time debug overlay deferred
  until their retained prototype or authoritative-system prerequisites exist.
- Do not merge, release, distribute, sign, notarize, or change repository visibility without
  explicit user authorization.
