# Allrpcs Handover

## Current state

Phase 2k37 is implemented and reviewed without findings on
`codex/phase-2k37-outcome-recording-eligibility`, based directly on PR #84's verified merge
commit `d82e1742`. The reviewed slice is ready for commit and a draft pull request; pull-request
merge remains separately unauthorized.

`Allrpcs.Domain.Discovery.FocusedSearchOutcomeRecordingPolicy.CanRecord` is one pure static
operation accepting an existing `FocusedSearchOutcome` and `FocusedSearchOutcomeSnapshot`. It
delegates exact-request selection to `FocusedSearchOutcomeLookup.FindByRequest` and permits
recording only when no outcome for that request is already retained. Exact-instance and
value-equal duplicate requests are rejected regardless of resulting visibility state; both
near-match request categories remain eligible. Every input and retained instance remains
unchanged.

The seven focused tests failed first with `CS0103`/`CS0246` while the policy type was absent, then
passed after the implementation. Host-shell `./scripts/quality.sh` passed with a zero-warning
build, 602 Domain tests, 107 Application tests, six architecture tests, all six Godot smokes, and
the bounded graphical launch. The sandbox-only focused and full test attempts were inconclusive
after successful builds because VSTest could not use its required local socket facilities. No
runtime Godot behavior changed, so no separate manual visual acceptance was performed. Hosted
desktop outputs remain unsigned development evidence, not releases.

## Exact next bounded slice

Take **Phase 2k38: pure focused-search outcome recording transition**.

Acceptance boundary:

1. Add one static `Allrpcs.Domain.Discovery.FocusedSearchOutcomeRecordingTransition` with one
   `Record` operation accepting an existing `FocusedSearchOutcome` and
   `FocusedSearchOutcomeSnapshot` and returning a `FocusedSearchOutcomeSnapshot`.
2. Delegate complete eligibility to `FocusedSearchOutcomeRecordingPolicy`, reject an ineligible
   recording with `InvalidOperationException`, and otherwise return a new snapshot preserving
   every retained outcome instance in stable order before the exact supplied outcome instance.
   Leave both inputs and every retained instance unchanged.
3. Add focused Domain tests for empty and unrelated snapshots, both near-match request categories,
   exact-instance and value-equal duplicate rejection, stable order and exact-instance retention,
   unchanged inputs, null rejection, and the intended single-operation static surface.
4. Document Phase 2k38 in `README.md` and compact `ROADMAP.md`, leave the broader focused-search
   item incomplete, run `./scripts/quality.sh`, review every intended path, and replace this file
   with current evidence, limits, and one next bounded slice.

Explicit exclusions:

- Do not produce an outcome, connect production or recording to focused-search completion,
  resolve an outcome automatically, grant discovery, or add new Godot/Application behavior.
- Do not add outcome replacement, visibility ranking, complete-character inspection,
  ownership/capacity, world state, location, coordinates, range policy, passive noticing,
  reinterpretation, serialization, persistence, providers, plugins, credentials, or final
  interaction UI.
- Keep point-and-click movement, disease-time pausing, and the world-time debug overlay deferred
  until their retained prototype or authoritative-system prerequisites exist.
- Do not merge, release, distribute, sign, notarize, or change repository visibility without
  explicit user authorization.
