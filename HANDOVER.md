# Allrpcs Handover

## Current state

Phase 2k36 is implemented and reviewed without findings on
`codex/phase-2k36-outcome-resolution-transition`, based directly on PR #83's verified merge
commit `2ba022b1`. Pull-request merge remains separately unauthorized.

`Allrpcs.Domain.Discovery.FocusedSearchOutcomeResolutionTransition.Resolve` is one pure static
operation accepting an existing `FocusedSearchOutcome`, `CharacterPerceptionSnapshot`, and
`FocusedSearchOutcomeSnapshot`. It delegates complete eligibility to
`FocusedSearchOutcomeResolutionPolicy`, rejects ineligible resolution with
`InvalidOperationException`, delegates perception replacement to
`FocusedSearchOutcomeApplicationTransition`, and delegates exact-outcome removal to
`FocusedSearchOutcomeConsumptionTransition`. The returned
`FocusedSearchOutcomeResolutionResult` retains the exact two transitioned snapshots. Every input,
stable order, and unrelated retained instance remains unchanged.

The seven focused tests failed first with `CS0103`/`CS0246` while the transition type was absent,
then passed after the implementation. Host-shell `./scripts/quality.sh` passed with a zero-warning
build, 595 Domain tests, 107 Application tests, six architecture tests, all six Godot smokes, and
the bounded graphical launch. The sandbox-only focused test attempt was inconclusive after build
because VSTest could not use its required local process/socket facilities. No runtime Godot
behavior changed, so no separate manual visual acceptance was performed. Hosted desktop outputs
remain unsigned development evidence, not releases.

## Exact next bounded slice

Take **Phase 2k37: pure focused-search outcome recording eligibility**.

Acceptance boundary:

1. Add one static `Allrpcs.Domain.Discovery.FocusedSearchOutcomeRecordingPolicy` with one
   `CanRecord` operation accepting an existing `FocusedSearchOutcome` and
   `FocusedSearchOutcomeSnapshot`.
2. Delegate exact-request selection to `FocusedSearchOutcomeLookup.FindByRequest` and return true
   only when no outcome for that request is already retained. Reject null inputs through the
   delegated lookup and leave every input and retained instance unchanged.
3. Add focused Domain tests for empty and unrelated snapshots, both near-match request categories,
   exact-instance and value-equal duplicate requests, unchanged inputs, null rejection, and the
   intended single-operation static surface.
4. Document Phase 2k37 in `README.md` and compact `ROADMAP.md`, leave the broader focused-search
   item incomplete, run `./scripts/quality.sh`, review every intended path, and replace this file
   with current evidence, limits, and one next bounded slice.

Explicit exclusions:

- Do not add the recording transition, produce an outcome, connect production to focused-search
  completion, resolve an outcome automatically, grant discovery, or add new Godot/Application
  behavior.
- Do not add outcome replacement, visibility ranking, complete-character inspection,
  ownership/capacity, world state, location, coordinates, range policy, passive noticing,
  reinterpretation, serialization, persistence, providers, plugins, credentials, or final
  interaction UI.
- Keep point-and-click movement, disease-time pausing, and the world-time debug overlay deferred
  until their retained prototype or authoritative-system prerequisites exist.
- Do not merge, release, distribute, sign, notarize, or change repository visibility without
  explicit user authorization.
