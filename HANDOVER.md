# Allrpcs Handover

## Current state

Phase 2k31 is implemented and reviewed without findings on
`codex/phase-2k31-outcome-application`, based directly on PR #78's merge commit `6f58353c`.
Slice commit `244385c` is pushed, and draft PR #79
(`https://github.com/hherb/allrpcs/pull/79`) is open.
Pull-request merge remains separately unauthorized.

`Allrpcs.Domain.Discovery.FocusedSearchOutcomeApplicationTransition.Apply` is one pure static
operation accepting an existing `FocusedSearchOutcome` and `CharacterPerceptionSnapshot`. It
delegates the complete decision to `FocusedSearchOutcomeChangePolicy` and throws
`InvalidOperationException` when the outcome is inapplicable or would not change its exact target.
An accepted outcome returns a new perception snapshot retaining the exact character identifier,
stable entry order, and every unrelated entry instance. Only the target is replaced, with a new
`WorldObjectVisibility` retaining the target's exact object identifier and the outcome's exact
resulting visibility-state instance. Both inputs remain unchanged. The transition does not rank
states, produce or consume outcomes, or connect application to focused-search completion.

The 11 focused tests failed first with `CS0103`/`CS0246` while the transition type was absent, then
passed after the implementation. Host-shell `./scripts/quality.sh` passed with a zero-warning
build, 553 Domain tests, 107 Application tests, six architecture tests, all six Godot smokes, and
the bounded graphical launch. No runtime Godot behavior changed, so no separate manual visual
acceptance was performed. Hosted desktop outputs remain unsigned development evidence, not
releases.

## Exact next bounded slice

Take **Phase 2k32: focused-search outcome consumption eligibility**.

Acceptance boundary:

1. Add one pure static
   `Allrpcs.Domain.Discovery.FocusedSearchOutcomeConsumptionPolicy.CanConsume` operation accepting
   an existing `FocusedSearchOutcome` and `FocusedSearchOutcomeSnapshot` and returning `bool`.
2. Delegate request selection to `FocusedSearchOutcomeLookup.FindByRequest`; return true only when
   the retained outcome is value-equal to the supplied outcome, including its resulting visibility
   state.
3. Add focused Domain tests for empty and absent outcomes; same-request/different-state rejection;
   exact-instance and value-equal acceptance; near-match requests; unrelated retained entries;
   unchanged inputs and exact retained instances; null rejection through the lookup; and the
   intended single-operation pure static API surface.
4. Do not remove or otherwise consume an outcome, change perception, decide outcome applicability
   or visibility change, produce outcomes, or connect consumption to focused-search completion.
5. Document Phase 2k32 in `README.md` and compact `ROADMAP.md`, leave the broader focused-search
   item incomplete, run `./scripts/quality.sh`, review every intended path, and replace this file
   with current evidence, limits, and one next bounded slice.

Explicit exclusions:

- Do not add outcome removal or lifecycle application, outcome production, discovery grants,
  advancement policy, events, effects, reason/error taxonomies, progress, duration, automatic
  lifecycle behavior, authoritative world objects, or new Godot/Application behavior.
- Do not add complete-character inspection, ownership/capacity, world state, location,
  coordinates, range policy, passive noticing, reinterpretation, serialization, persistence,
  providers, plugins, credentials, or final interaction UI.
- Keep point-and-click movement, disease-time pausing, and the world-time debug overlay deferred
  until their retained prototype or authoritative-system prerequisites exist.
- Do not merge, release, distribute, sign, notarize, or change repository visibility without
  explicit user authorization.
