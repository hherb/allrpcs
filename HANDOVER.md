# Allrpcs Handover

## Current state

`main` includes merged PR #409 at `0c4d4ad`. Branch
`codex/phase-8-investigation-route-observation-routing` adds two bounded Phase 8 slices:

- `OpeningRegionInvestigationRouteObservationRoutingCoordinator` receives one exact current route
  snapshot plus one already-verified map location or visible evidence identity. It preflights every
  exact matching catalogue route, records the observation through the existing compare-and-swap
  authority owner in authored order, and asks the existing completion owner to observe each exact
  resulting snapshot.
- `OpeningRegionMapVisitVerificationCoordinator` now submits one successfully verified exact live
  location through that routing path when route progress was already explicitly initialized. Map
  verification remains authoritative even when no route state is active.

Ordinary startup still does not initialize or advance route progress. No evidence-discovery caller,
preferred route, clue order, completion consumption, presentation, persistence, testimony,
stabilization, or other gameplay effect was added. `README.md` documents the boundary. The
multiple-route roadmap item remains unticked because a complete route is not yet driven through
ordinary play.

## Validation and review

- Both slices were test-first. The routing smoke first failed to compile because the coordinator was
  absent; the map-visit smoke then failed because successful visits did not submit route progress.
  Both passed after the minimum implementations.
- The routing smoke proves catalogue-ordered multi-route location and evidence recording, ordered
  completion observation, inert startup, and null, stale, rebuilt, foreign, unverified, unseen,
  nonmember, and repeated rejection without changing completion consumption, map, discovery, or
  journal state.
- The updated map-visit smoke proves exact successful live visits submit canonical locations when
  route progress is explicitly active; invalid, out-of-range, stale, and repeated visits preserve
  route state, and ordinary startup remains inert.
- The restricted complete suite passed all managed checks and then stopped only at Godot's known
  `user://logs` sandbox boundary. The identical host-local `./scripts/quality.sh` passed formatting,
  a zero-warning build, 2,824 Domain, 898 Application, 1,249 Infrastructure, and 8 architecture
  tests, scenario validation, every credential-free Godot smoke, and the graphical Compatibility
  launch.
- Session-wide review checked the complete diff, exact-reference ownership, authored catalogue
  ordering, full preflight before multi-route mutation, unchanged rejected inputs, closed public
  surfaces, file-size and 120-character limits, formatting, and whitespace. No issue remained.

## Preserved boundaries

- Routing derives matches only from the exact catalogue retained by the exact current progress
  snapshot. It does not rebuild observations or routes, choose a preferred route, stop after the
  first match, or pool progress across routes.
- Live map verification submits only its exact successfully installed verified location and does
  not initialize route state. Evidence discovery still has no ordinary-play route submission.
- Accepted and consumed completion tokens remain exact existing provenance. The new routing path
  observes but never consumes a completion or triggers an effect.
- Save schema 6 remains unchanged; routes, progress, accepted completions, and consumption are not
  persisted.
- Discovery, journal, first-patient, provider, input, and UI owners remain unchanged. The map owner
  changes only through its pre-existing visit-verification operation.
- Do not tick the multiple-investigation-routes roadmap item until routes are authoritatively
  exercised through ordinary play and its acceptance is actually met.
- Do not merge, release, sign, notarize, publish, or run credential-backed provider activity without
  separate authorization.

## Exact next bounded slice

Submit one already-authorized exact visible evidence observation from the ordinary old-shrine
layered-inscription discovery flow to the existing catalogue-routing coordinator. Do not add another
evidence source yet.

Acceptance boundary:

1. After the existing old-shrine flow atomically commits its exact discovery and filtered evidence
   exposure, submit only that exact visible `partial-inscription` identity with the exact current
   route-progress snapshot and an exact filtered Seeker evidence view.
2. Require route progress to have been explicitly initialized already. If it is absent, preserve the
   existing successful discovery behavior without initializing routes or fabricating progress.
3. Prove one active exact shrine route records the evidence once, asks the existing completion owner
   to observe the exact resulting snapshot, and leaves unrelated routes unchanged. The route may
   remain incomplete until its exact location is recorded.
4. Reject stale, rebuilt, foreign, unseen, missing-progress, and repeated routing inputs without
   changing discovery, route progress, accepted/consumed completion, map, journal, presentation, or
   environmental state.
5. Keep ordinary startup inert. Do not wire focused-search, passive-noticing, testimony, or any other
   evidence source; initialize or persist routes; infer clue order; consume or present completion; or
   trigger testimony, stabilization, or another gameplay effect.
6. Work test-first, extend the focused switch-gated old-shrine smoke, run focused tests and
   `./scripts/quality.sh`, review the complete diff, update `README.md`, and leave one next slice for
   explicit ordinary-play route initialization.
