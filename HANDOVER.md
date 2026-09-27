# Allrpcs Handover

## Current state

`main` includes merged PR #408 at `cc50c7c`. Branch
`codex/phase-8-investigation-route-completion-observation` adds two bounded Phase 8 scene slices:

- `OpeningRegionInvestigationRouteCompletionCoordinator` explicitly calls the existing pure
  completion policy only for the route controller's exact current progress snapshot. It retains one
  exact token per exact route in observation order and never replaces an earlier token.
- `OpeningRegionInvestigationRouteCompletionConsumptionCoordinator` explicitly consumes one exact
  token already retained by that scene owner. It retains the exact token once without removing it
  from the accepted list or causing any gameplay effect.

Both owners are inert during ordinary startup. Nothing initializes or advances route progress,
observes or consumes completion automatically, selects a preferred route, presents or persists
completion, or triggers testimony, stabilization, or another gameplay effect. `README.md` documents
the boundary. The multiple-route roadmap item remains unticked because the routes are not yet driven
through ordinary play.

## Validation and review

- Both slices were written test-first: each focused game build first failed on its absent scene
  coordinator, then passed after the minimum implementation.
- The completion-observation smoke proves inert startup, exact current-snapshot authorization,
  one-shot per-route observation, ordered retention across later routes, and null, partial, stale,
  rebuilt, foreign, and repeated rejection without changing route progress, map, or journal state.
- The completion-consumption smoke proves inert startup, exact accepted-token binding, one-shot
  retention, and null, unaccepted, rebuilt-token, repeated, and later-route rejection without
  changing progress, accepted completions, map, or journal state.
- Both focused smokes passed host-local. The restricted complete suite passed all managed checks and
  then stopped only at Godot's known `user://logs` sandbox boundary; the identical host-local
  `./scripts/quality.sh` passed formatting, a zero-warning build, 2,824 Domain, 898 Application,
  1,249 Infrastructure, and 8 architecture tests, scenario validation, every credential-free Godot
  smoke, and the graphical Compatibility launch.
- Session-wide review checked the complete API and scene diff, exact-reference ownership, unchanged
  inputs, closed coordinator surfaces, changed-file size and 120-character limits, and whitespace.

## Preserved boundaries

- The accepted and consumed tokens retain exact existing completion provenance only. They expose no
  completion Boolean and do not rebuild routes, progress, or tokens.
- No automatic visit/discovery capture, route choice, clue order, objective arrow, progress or
  completion presentation, testimony, stabilization, or completion gameplay effect was added.
- Save schema 6 remains unchanged; routes, route progress, accepted completions, and consumption are
  not persisted.
- Map, discovery, journal, first-patient, provider, input, and UI owners remain unchanged.
- Do not tick the multiple-investigation-routes roadmap item until routes are authoritatively
  exercised through ordinary play and its acceptance is actually met.
- Do not merge, release, sign, notarize, publish, or run credential-backed provider activity without
  separate authorization.

## Exact next bounded slice

Add explicit catalogue-ordered routing of one authoritative observation to every exact route that
contains it, without connecting that routing to ordinary play yet.

Acceptance boundary:

1. Add one inert scene coordinator that receives the exact current route-progress snapshot plus one
   already-authorized exact location or evidence observation. It must derive matching routes only
   from the exact catalogue owned by `OpeningRegionInvestigationRouteProgressController`.
2. Record the observation into each exact matching route in authored catalogue order through the
   existing progress controller's compare-and-swap operations. Do not pick a preferred route, stop at
   the first match, or pool partial progress across routes.
3. After each successful record, explicitly ask the existing completion coordinator to observe that
   exact resulting current snapshot and route. Preserve already accepted tokens and their observation
   order; incomplete and already accepted routes remain inert.
4. Reject null, stale, rebuilt, foreign, unverified/unseen, nonmember, and repeated inputs without
   changing progress, accepted/consumed completion, map, discovery, or journal owners.
5. Keep ordinary startup inert. Do not wire map visits or evidence discovery to this coordinator yet,
   infer a clue order, consume a completion, present or persist progress/completion, or trigger
   testimony, stabilization, or other gameplay effects.
6. Work test-first, add one focused switch-gated Godot smoke, run focused tests and
   `./scripts/quality.sh`, review the complete diff, update `README.md`, and leave one next slice for
   the first ordinary-play call site to submit one already-authorized observation.
