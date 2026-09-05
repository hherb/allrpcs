# Allrpcs Handover

## Current state

Phase 2k38 is implemented and reviewed without findings on
`codex/phase-2k38-outcome-recording-transition`, based directly on PR #85's verified merge commit
`c751e62c`. Slice commit `7cb4404` is pushed, and draft PR #86
(`https://github.com/hherb/allrpcs/pull/86`) is open. Pull-request merge remains separately
unauthorized.

`Allrpcs.Domain.Discovery.FocusedSearchOutcomeRecordingTransition.Record` is one pure static
operation accepting an existing `FocusedSearchOutcome` and `FocusedSearchOutcomeSnapshot`. It
delegates complete eligibility to `FocusedSearchOutcomeRecordingPolicy`, rejects a duplicate
request with `InvalidOperationException`, and otherwise returns a new snapshot preserving every
retained outcome instance in stable order before the exact supplied outcome instance. Every input
and retained instance remains unchanged.

The eight focused tests failed first with `CS0103`/`CS0246` while the transition type was absent,
then passed after the implementation. Host-shell `./scripts/quality.sh` passed with a zero-warning
build, 610 Domain tests, 107 Application tests, six architecture tests, all six Godot smokes, and
the bounded graphical launch. The sandbox-only focused test attempt was inconclusive after the
Domain build because VSTest could not use its required local socket facilities. No runtime Godot
behavior changed, so no separate manual visual acceptance was performed. Hosted desktop outputs
remain unsigned development evidence, not releases.

## Exact next bounded slice

Take **Phase 2k39: pure focused-search completion-with-outcome eligibility**.

Acceptance boundary:

1. Add one static `Allrpcs.Domain.Discovery.FocusedSearchOutcomeCompletionPolicy` with one
   `CanCompleteWithOutcome` operation accepting an existing `FocusedSearchOutcome`,
   `FocusedSearchActivitySnapshot`, and `FocusedSearchOutcomeSnapshot`.
2. Delegate active-request eligibility to
   `FocusedSearchCompletionPolicy.CanCompleteSearch(outcome.Request, activities)`, delegate
   recording eligibility to `FocusedSearchOutcomeRecordingPolicy.CanRecord(outcome, outcomes)`,
   and return true only when both decisions succeed. Reject every null input before delegation and
   leave every input and retained instance unchanged.
3. Add focused Domain tests for the eligible active-and-unrecorded case, every delegated inactive
   request category, exact-instance and value-equal recorded-request rejection, unchanged inputs,
   null rejection, and the intended single-operation static surface.
4. Document Phase 2k39 in `README.md` and compact `ROADMAP.md`, leave the broader focused-search
   item incomplete, run `./scripts/quality.sh`, review every intended path, and replace this file
   with current evidence, limits, and one next bounded slice.

Explicit exclusions:

- Do not add a combined completion result or transition, produce an outcome, change either
  snapshot, connect recording to the Godot controller, resolve an outcome automatically, grant
  discovery, or add new Godot/Application behavior.
- Do not add outcome replacement, visibility ranking, complete-character inspection,
  ownership/capacity, world state, location, coordinates, range policy, passive noticing,
  reinterpretation, serialization, persistence, providers, plugins, credentials, or final
  interaction UI.
- Keep point-and-click movement, disease-time pausing, and the world-time debug overlay deferred
  until their retained prototype or authoritative-system prerequisites exist.
- Do not merge, release, distribute, sign, notarize, or change repository visibility without
  explicit user authorization.
