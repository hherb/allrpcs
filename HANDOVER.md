# Allrpcs Handover

## Current state

Phase 2k41 is implemented and reviewed without findings on
`codex/phase-2k41-completion-with-outcome-transition`, based directly on PR #88's verified merge
commit `cb38cd8`. Slice commit `f6f5984` is pushed, and draft PR #89
(`https://github.com/hherb/allrpcs/pull/89`) is open. Pull-request merge remains separately
unauthorized.

`Allrpcs.Domain.Discovery.FocusedSearchOutcomeCompletionTransition.CompleteWithOutcome` is one
pure static operation accepting an existing `FocusedSearchOutcome`,
`FocusedSearchActivitySnapshot`, and `FocusedSearchOutcomeSnapshot`. It delegates complete
eligibility to `FocusedSearchOutcomeCompletionPolicy`, rejects inactive or already-recorded
requests, then delegates exact-request activity removal to
`FocusedSearchCompletionTransition.CompleteSearch` and exact-outcome append to
`FocusedSearchOutcomeRecordingTransition.Record`. Its result retains the exact two snapshots
returned by those transitions. Inputs and unrelated retained instances remain unchanged.

The seven focused tests failed first with nine missing-transition compile errors, then passed after
the implementation. Host-shell `./scripts/quality.sh` passed with a zero-warning build, 631 Domain
tests, 107 Application tests, six architecture tests, all six Godot smokes, and the bounded
graphical launch. The sandbox-only focused test attempt was inconclusive after the build because
VSTest could not use its required local socket facilities. No runtime Godot behavior changed, so
no separate manual visual acceptance was performed. Hosted desktop outputs remain unsigned
development evidence, not releases.

## Exact next bounded slice

Take **Phase 2k42: focused-search outcome state and controller completion bridge**.

Acceptance boundary:

1. Extend the existing `Allrpcs.Game.FocusedSearchActivityController` to own one immutable
   `FocusedSearchOutcomeSnapshot`, initialize it empty in `_Ready`, and expose its current value
   without adding another owner or mutable collection.
2. Add one outcome-aware completion operation accepting an existing `FocusedSearchOutcome`. For
   this controller's Seeker only, delegate the complete decision to
   `FocusedSearchOutcomeCompletionPolicy` and the successful state change to
   `FocusedSearchOutcomeCompletionTransition.CompleteWithOutcome`. Replace both owned snapshots
   with the result's exact instances and present the resulting activity snapshot only after
   success. Rejected calls must preserve both snapshots and presentation.
3. Add focused Godot smoke coverage for empty initial outcome state; successful exact-request
   removal and exact-outcome append; stable order and exact unrelated-instance retention; unchanged
   inputs; wrong-character, inactive-request, duplicate-request, and null rejection; and unchanged
   existing start, cancellation, and request-only completion behavior.
4. Document Phase 2k42 in `README.md` and compact `ROADMAP.md`, leave the broader focused-search
   item incomplete, run `./scripts/quality.sh`, review every intended path, and replace this file
   with current evidence, limits, and one next bounded slice.

Explicit exclusions:

- Do not construct or inject an outcome from ordinary play, wire the new operation through
  `Main`, interaction focus, or final UI, or remove the existing request-only temporary completion
  path.
- Do not produce or resolve an outcome, apply a visibility change, grant discovery, consume an
  outcome, or add new perception, world-state, location, inventory, or persistence behavior.
- Do not add outcome replacement, visibility ranking, coordinates, range policy, passive
  noticing, reinterpretation, serialization, providers, plugins, credentials, or final interaction
  UI.
- Keep point-and-click movement, disease-time pausing, and the world-time debug overlay deferred
  until their retained prototype or authoritative-system prerequisites exist.
- Do not merge, release, distribute, sign, notarize, or change repository visibility without
  explicit user authorization.
