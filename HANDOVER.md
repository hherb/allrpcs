# Allrpcs Handover

## Current state

Phase 2k33 is implemented and reviewed without findings on
`codex/phase-2k33-outcome-consumption-transition`, based directly on PR #80's merge commit
`e1c7c87b`.

`Allrpcs.Domain.Discovery.FocusedSearchOutcomeConsumptionTransition.Consume` is one pure static
operation accepting an existing `FocusedSearchOutcome` and `FocusedSearchOutcomeSnapshot`. It
delegates the complete decision to `FocusedSearchOutcomeConsumptionPolicy`, throws
`InvalidOperationException` when the supplied complete outcome is not exactly retained, and
otherwise returns a new snapshot removing only the outcome for that exact request. Stable order
and every unrelated outcome instance are preserved, and both inputs remain unchanged. It does not
change perception or connect consumption to focused-search completion.

The 11 focused tests failed first with `CS0103`/`CS0246` while the transition type was absent,
then passed after the implementation. Host-shell `./scripts/quality.sh` passed with a zero-warning
build, 573 Domain tests, 107 Application tests, six architecture tests, all six Godot smokes, and
the bounded graphical launch. The sandbox-only focused-test attempt was inconclusive because
VSTest could not bind its required local socket. No runtime Godot behavior changed, so no separate
manual visual acceptance was performed. Hosted desktop outputs remain unsigned development
evidence, not releases.

## Exact next bounded slice

Take **Phase 2k34: focused-search outcome resolution eligibility**.

Acceptance boundary:

1. Add one pure static `Allrpcs.Domain.Discovery.FocusedSearchOutcomeResolutionPolicy.CanResolve`
   operation accepting an existing `FocusedSearchOutcome`, `CharacterPerceptionSnapshot`, and
   `FocusedSearchOutcomeSnapshot` and returning only a Boolean eligibility decision.
2. Delegate perception applicability and change detection completely to
   `FocusedSearchOutcomeChangePolicy`, and delegate exact recorded-outcome eligibility completely
   to `FocusedSearchOutcomeConsumptionPolicy`; return true only when both decisions succeed.
3. Add focused Domain tests for wrong-character, absent-target, unchanged-state, absent-outcome,
   same-request/different-state, and near-match rejection; exact-instance and value-equal eligible
   outcomes; unchanged inputs and retained instances; null rejection through the delegated
   policies; and the intended single-operation pure static API surface.
4. Do not apply a visibility change, remove an outcome, produce outcomes, or connect resolution to
   focused-search completion.
5. Document Phase 2k34 in `README.md` and compact `ROADMAP.md`, leave the broader focused-search
   item incomplete, run `./scripts/quality.sh`, review every intended path, and replace this file
   with current evidence, limits, and one next bounded slice.

Explicit exclusions:

- Do not add a combined resolution transition or result aggregate, lifecycle wiring, outcome
  production, discovery grants, advancement policy, events, effects, reason/error taxonomies,
  progress, duration, automatic lifecycle behavior, authoritative world objects, or new
  Godot/Application behavior.
- Do not add complete-character inspection, ownership/capacity, world state, location,
  coordinates, range policy, passive noticing, reinterpretation, serialization, persistence,
  providers, plugins, credentials, or final interaction UI.
- Keep point-and-click movement, disease-time pausing, and the world-time debug overlay deferred
  until their retained prototype or authoritative-system prerequisites exist.
- Do not merge, release, distribute, sign, notarize, or change repository visibility without
  explicit user authorization.
