# Allrpcs Handover

## Current state

`main` includes merged PR #407 at `d87c51a`. Branch
`codex/phase-8-investigation-route-authority` adds three bounded Phase 8 slices over the existing
opening-region investigation-route catalogue and progress state:

- `OpeningRegionInvestigationRouteAuthorityTransition` records an exact route location only when
  its exact `MapLocationDefinition` is retained by the supplied `SeekerMapSnapshot` verified view,
  and records an exact route evidence identifier only when retained by the supplied filtered
  `CharacterEvidenceViewSnapshot`.
- `OpeningRegionInvestigationRouteAuthorityCoordinator` requires the exact current scene-owned map
  for location progress, validates both authority paths through that pure transition, and installs
  them through the existing route controller's expected-progress compare-and-swap.
- `OpeningRegionInvestigationRouteCompletionPolicy` returns an immutable exact-provenance token only
  when one exact route entry contains every exact authored location and evidence member. It accepts
  any observation order, never pools partial progress across routes, and exposes no completion
  Boolean.

The coordinator and completion policy are explicit only. Ordinary startup still does not initialize
or advance route progress, capture visits or discovery automatically, observe completion, present
progress, or trigger gameplay effects. `README.md` documents the new boundaries. The multiple-route
roadmap item remains unticked because routes are not yet exercised through ordinary play.

## Validation and review

- Test-first focused runs failed at each absent authority/completion contract, then passed after the
  implementation. Nine focused Application tests cover exact success, arbitrary observation order,
  stable unrelated entries, immutable authority inputs, exact completion provenance, and null,
  stale/repeated, partial, pooled, rebuilt, foreign, unverified/unseen, nonmember, and mismatched
  rejection.
- The new switch-gated Godot smoke proves inert startup, exact current-map and progress CAS,
  verified-location and visible-evidence recording, stale/rebuilt/unseen/repeated rejection, a closed
  coordinator surface, and map/journal isolation.
- The restricted `./scripts/quality.sh` run passed the zero-warning build, all managed tests, and
  scenario validation, then stopped only at Godot's known `user://logs` sandbox boundary. The
  identical host-local suite passed formatting, a zero-warning build, 2,824 Domain, 898 Application,
  1,249 Infrastructure, and 8 architecture tests, scenario validation, every credential-free Godot
  smoke, and the graphical Compatibility launch.
- Session-wide review found and fixed two test-coverage gaps: every evidence-authority null parameter
  is now explicit, and the completion token's complete read-only/no-behavior surface is checked.
  All changed C# files are below 500 lines, changed C# lines are at most 120 characters, and
  `git diff --check` passes.

## Preserved boundaries

- Authority inputs are observed only; no map, discovery, journal, first-patient, provider,
  presentation, or UI state is changed.
- No automatic visit/discovery capture, route selection, clue order, objective arrow, route-progress
  presentation, testimony, stabilization, or completion gameplay effect was added.
- The completion token is a pure exact-provenance value. No scene owner observes or persists it.
- Save schema 6 remains unchanged; route definitions, progress, and completion are not persisted.
- Do not tick the multiple-investigation-routes roadmap item until routes are authoritatively
  exercisable through ordinary play and its acceptance is actually met.
- Do not merge, release, sign, notarize, publish, or run credential-backed provider activity without
  separate authorization.

## Exact next bounded slice

Add explicit scene-owned observation of current route-completion tokens. Do not connect completion to
testimony, stabilization, presentation, persistence, or automatic ordinary-play capture yet.

Acceptance boundary:

1. Add one inert scene completion coordinator with an initially empty read-only list of accepted
   `OpeningRegionInvestigationRouteCompletion` tokens.
2. Accept one token only by calling the existing pure completion policy for an exact route and the
   exact current snapshot owned by `OpeningRegionInvestigationRouteProgressController`. Retain the
   returned exact token once and preserve accepted tokens in observation order.
3. Reject null, partial, stale-snapshot, rebuilt-route, foreign-route, and repeated-route inputs
   without changing either owner. An unrelated later route observation must not replace or rebuild a
   previously accepted token.
4. Keep ordinary startup inert and do not call the coordinator automatically. Do not infer a preferred
   route or clue order, pool progress across routes, or expose a completion Boolean.
5. Do not change save schema, map/discovery/journal owners, route-authority operations, input, UI,
   providers, first-patient state, testimony, stabilization, or the roadmap checkbox in this slice.
6. Work test-first, add one focused switch-gated Godot smoke, run focused tests and
   `./scripts/quality.sh`, review the complete diff, update `README.md`, and leave one next slice for
   explicit consumption of one exact accepted completion token without gameplay effects.
