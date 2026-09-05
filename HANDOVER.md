# Allrpcs Handover

## Current state

Phase 2k40 is implemented and reviewed without findings on
`codex/phase-2k40-completion-outcome-result`, based directly on PR #87's verified merge commit
`deec39c`. Publication of this slice is pending. Pull-request merge remains separately
unauthorized.

`Allrpcs.Domain.Discovery.FocusedSearchOutcomeCompletionResult` is one sealed immutable record
retaining exact existing `FocusedSearchActivitySnapshot` and `FocusedSearchOutcomeSnapshot`
instances through get-only `Activities` and `Outcomes` properties. It rejects either null input,
and its record equality uses the retained snapshot instances. It does not inspect, copy, validate,
or change either snapshot or any retained entry.

The four focused tests failed first with eight `CS0246` errors while the result was absent, then
passed after the implementation. Host-shell `./scripts/quality.sh` passed with a zero-warning
build, 624 Domain tests, 107 Application tests, six architecture tests, all six Godot smokes, and
the bounded graphical launch. The sandbox-only focused test attempt was inconclusive after the
build because VSTest could not use its required local socket facilities. No runtime Godot behavior
changed, so no separate manual visual acceptance was performed. Hosted desktop outputs remain
unsigned development evidence, not releases.

## Exact next bounded slice

Take **Phase 2k41: focused-search completion-with-outcome transition**.

Acceptance boundary:

1. Add one pure static
   `Allrpcs.Domain.Discovery.FocusedSearchOutcomeCompletionTransition.CompleteWithOutcome`
   operation accepting an existing `FocusedSearchOutcome`, `FocusedSearchActivitySnapshot`, and
   `FocusedSearchOutcomeSnapshot`, and returning `FocusedSearchOutcomeCompletionResult`.
2. Delegate the complete eligibility decision to
   `FocusedSearchOutcomeCompletionPolicy.CanCompleteWithOutcome` and reject an ineligible
   transition. For an eligible outcome, delegate activity removal to
   `FocusedSearchCompletionTransition.CompleteSearch` with the outcome's exact request and outcome
   recording to `FocusedSearchOutcomeRecordingTransition.Record` with the exact supplied outcome.
   Return one result retaining the exact two snapshots returned by those transitions. Do not
   change any input or retained unrelated entry.
3. Add focused Domain tests for successful exact-request removal and exact-outcome append; stable
   order and exact unrelated-instance retention; unchanged inputs; inactive-request,
   duplicate-request, and null rejection; and the intended pure static API surface.
4. Document Phase 2k41 in `README.md` and compact `ROADMAP.md`, leave the broader focused-search
   item incomplete, run `./scripts/quality.sh`, review every intended path, and replace this file
   with current evidence, limits, and one next bounded slice.

Explicit exclusions:

- Do not produce or resolve an outcome, apply a visibility change, connect outcome completion to
  the Godot controller, grant discovery, or add new Godot/Application behavior.
- Do not add outcome replacement, visibility ranking, complete-character inspection,
  ownership/capacity, world state, location, coordinates, range policy, passive noticing,
  reinterpretation, serialization, persistence, providers, plugins, credentials, or final
  interaction UI.
- Keep point-and-click movement, disease-time pausing, and the world-time debug overlay deferred
  until their retained prototype or authoritative-system prerequisites exist.
- Do not merge, release, distribute, sign, notarize, or change repository visibility without
  explicit user authorization.
