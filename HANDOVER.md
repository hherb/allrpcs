# Allrpcs Handover

## Current state

Phase 2k32 is implemented and reviewed without findings on
`codex/phase-2k32-outcome-consumption`, based directly on PR #79's merge commit `17c8a8d7`.

`Allrpcs.Domain.Discovery.FocusedSearchOutcomeConsumptionPolicy.CanConsume` is one pure static
operation accepting an existing `FocusedSearchOutcome` and `FocusedSearchOutcomeSnapshot`. It
delegates request selection to `FocusedSearchOutcomeLookup.FindByRequest` and returns true only
when the exact retained outcome is value-equal to the supplied outcome, including its resulting
visibility state. It leaves both inputs and every retained instance unchanged. It does not remove
or otherwise consume an outcome, change perception, or connect consumption to focused-search
completion.

The 9 focused tests failed first with `CS0103`/`CS0246` while the policy type was absent, then
passed after the implementation. Host-shell `./scripts/quality.sh` passed with a zero-warning
build, 562 Domain tests, 107 Application tests, six architecture tests, all six Godot smokes, and
the bounded graphical launch. The sandbox-only focused-test attempt was inconclusive because
VSTest could not bind its required local socket. No runtime Godot behavior changed, so no separate
manual visual acceptance was performed. Hosted desktop outputs remain unsigned development
evidence, not releases.

## Exact next bounded slice

Take **Phase 2k33: focused-search outcome consumption transition**.

Acceptance boundary:

1. Add one pure static
   `Allrpcs.Domain.Discovery.FocusedSearchOutcomeConsumptionTransition.Consume` operation accepting
   an existing `FocusedSearchOutcome` and `FocusedSearchOutcomeSnapshot` and returning a new
   `FocusedSearchOutcomeSnapshot`.
2. Delegate the complete decision to `FocusedSearchOutcomeConsumptionPolicy`; throw
   `InvalidOperationException` when the supplied complete outcome is not eligible for consumption.
3. For an eligible outcome, remove only the retained outcome for its exact request while preserving
   stable order and every unrelated outcome instance. Leave both inputs unchanged.
4. Add focused Domain tests for empty, absent, same-request/different-state, and near-match
   rejection; exact-instance and value-equal acceptance; stable unrelated-entry retention;
   unchanged inputs and exact retained instances; null rejection through the policy; and the
   intended single-operation pure static API surface.
5. Do not apply an outcome to perception, produce outcomes, or connect consumption to
   focused-search completion.
6. Document Phase 2k33 in `README.md` and compact `ROADMAP.md`, leave the broader focused-search
   item incomplete, run `./scripts/quality.sh`, review every intended path, and replace this file
   with current evidence, limits, and one next bounded slice.

Explicit exclusions:

- Do not add lifecycle application, outcome production, discovery grants, advancement policy,
  events, effects, reason/error taxonomies, progress, duration, automatic lifecycle behavior,
  authoritative world objects, or new Godot/Application behavior.
- Do not add complete-character inspection, ownership/capacity, world state, location,
  coordinates, range policy, passive noticing, reinterpretation, serialization, persistence,
  providers, plugins, credentials, or final interaction UI.
- Keep point-and-click movement, disease-time pausing, and the world-time debug overlay deferred
  until their retained prototype or authoritative-system prerequisites exist.
- Do not merge, release, distribute, sign, notarize, or change repository visibility without
  explicit user authorization.
