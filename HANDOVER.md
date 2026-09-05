# Allrpcs Handover

## Current state

Phase 2k34 is implemented and reviewed without findings on
`codex/phase-2k34-outcome-resolution-eligibility`, based directly on PR #81's merge commit
`d66f68d2`.

`Allrpcs.Domain.Discovery.FocusedSearchOutcomeResolutionPolicy.CanResolve` is one pure static
operation accepting an existing `FocusedSearchOutcome`, `CharacterPerceptionSnapshot`, and
`FocusedSearchOutcomeSnapshot`. It delegates perception applicability and visibility-state change
detection completely to `FocusedSearchOutcomeChangePolicy`, delegates exact recorded-outcome
eligibility completely to `FocusedSearchOutcomeConsumptionPolicy`, and returns true only when both
decisions succeed. It leaves every input and retained instance unchanged and does not apply a
visibility change, remove an outcome, or connect resolution to focused-search completion.

The 11 focused tests failed first with `CS0103`/`CS0246` while the policy type was absent, then
passed after the implementation. Host-shell `./scripts/quality.sh` passed with a zero-warning
build, 584 Domain tests, 107 Application tests, six architecture tests, all six Godot smokes, and
the bounded graphical launch. One earlier host-shell run reported a transient interaction-focus
smoke failure in unchanged runtime code; its isolated rerun and the complete final rerun passed.
The sandbox-only test attempts were inconclusive because VSTest could not bind its required local
socket. No runtime Godot behavior changed, so no separate manual visual acceptance was performed.
Hosted desktop outputs remain unsigned development evidence, not releases.

## Exact next bounded slice

Take **Phase 2k35: immutable focused-search outcome resolution result**.

Acceptance boundary:

1. Add one sealed immutable record
   `Allrpcs.Domain.Discovery.FocusedSearchOutcomeResolutionResult` accepting an existing
   `CharacterPerceptionSnapshot` and `FocusedSearchOutcomeSnapshot`.
2. Retain both supplied snapshot instances through get-only properties; reject either null input
   and add no resolution, transition, lookup, validation, or mutation behavior.
3. Add focused Domain tests for exact instance retention, record equality using the retained
   snapshot instances, null rejection, and the intended constructor/get-only property surface.
4. Do not apply a visibility change, remove an outcome, produce outcomes, or connect resolution to
   focused-search completion.
5. Document Phase 2k35 in `README.md` and compact `ROADMAP.md`, leave the broader focused-search
   item incomplete, run `./scripts/quality.sh`, review every intended path, and replace this file
   with current evidence, limits, and one next bounded slice.

Explicit exclusions:

- Do not add the combined resolution transition, lifecycle wiring, outcome production, discovery
  grants, advancement policy, events, effects, reason/error taxonomies, progress, duration,
  automatic lifecycle behavior, authoritative world objects, or new Godot/Application behavior.
- Do not add complete-character inspection, ownership/capacity, world state, location,
  coordinates, range policy, passive noticing, reinterpretation, serialization, persistence,
  providers, plugins, credentials, or final interaction UI.
- Keep point-and-click movement, disease-time pausing, and the world-time debug overlay deferred
  until their retained prototype or authoritative-system prerequisites exist.
- Do not merge, release, distribute, sign, notarize, or change repository visibility without
  explicit user authorization.
