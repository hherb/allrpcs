# Allrpcs Handover

## Current state

`main` includes merged PR #387 at `fbdab1c`. Branch
`codex/phase-8-route-access-decision` continues the open Phase 8 background-route roadmap item
through three bounded route-access slices.

Application now owns one immutable `SeekerBackgroundRouteAccess` token. The pure authorization
policy accepts only the exact authoritative selected route-state instance, its exact empty
predecessor, retained catalogue, selected variation, and exact destination anchor while no prior
token exists. Empty, stale, rebuilt, foreign, non-destination, repeated, malformed, and null inputs
fail closed without changing customization, character, route, or navigation state.

The main scene owns the one selected token in `SeekerBackgroundRouteAccessSceneSource` through an
exact compare-and-swap install. Only the representative Forester effect is implemented:
`OpeningRegionClosedForestPathAccess` binds that exact token to the existing exact closed-path
scene, whose owner consumes it once and removes only the gate collision, gate line, and marker.
Ordinary startup remains visibly collision-backed and closed. Apothecary's Assistant selection
retains its exact healer-garden access token while leaving the Forester gate unchanged. Scribe,
Apothecary's Assistant, and Traveller effects remain absent, so the roadmap item remains open.

This access is additive. It does not close any baseline route, make a background mandatory, grant a
character capability, claim the distant closed-path anchor is navigation-reachable, add deeper
forest geometry, alter discovery, persist customization, or add provider behavior.

## Validation and review

- Seven focused Application tests pass for exact authorization and closed-path binding, including
  exact retention and empty, stale, rebuilt, foreign, non-destination, other-background, repeated,
  malformed, and null rejection.
- The Forester main-scene smoke passes inert closed startup, one exact scene-owned access token,
  one exact closed-path consumption, removal of only the existing gate barrier, and repeated-input
  preservation.
- The real U-key Apothecary's Assistant path passes exact token ownership while retaining the
  closed Forester gate. The original closed-path smoke independently passes visibly closed ordinary
  startup and its existing navigation/collision checks.
- Final host-local `./scripts/quality.sh` passed workflow validation, formatting, a zero-warning
  build, 2,811 Domain, 674 Application, 1,220 Infrastructure, and 8 architecture tests, scenario
  validation, every credential-free Godot smoke, and the graphical Compatibility launch. The
  restricted run passed every .NET test and scenario validation before the known Godot
  `user://logs` restriction; the identical host run supplied the complete Godot evidence.
- Session review checked the complete diff, exact predecessor/catalogue/anchor provenance,
  one-shot ownership, coordinator preflight/install ordering, ordinary and non-Forester inertness,
  scene dependency order, public API shape, documentation claims, 120-character limits,
  sub-500-line C# files, formatting, and whitespace. Review found and fixed one policy weakness by
  requiring the selected state's exact empty predecessor, projection, and catalogue provenance.

## Exact next bounded slice

Add one pure, scene-neutral Scribe old-shrine inspection-access decision. Do not change a Godot
scene or trigger discovery yet.

Acceptance boundary:

1. Add one immutable Scribe inspection-access token retaining the exact scene-owned background
   route access, its exact Scribe variation, the exact old-shrine scene binding, and the binding's
   existing layered-inscription provenance.
2. Add one pure policy/projection that returns the token only for the exact current Scribe access
   and exact retained old-shrine destination/binding. Reject absent, Forester, Apothecary's
   Assistant, Traveller, stale, rebuilt, foreign, non-destination, repeated, malformed, and null
   inputs without changing route, discovery, presentation, or evidence state.
3. Treat the token as additive inspection authorization only. It must not identify or reveal the
   inscription, bypass the ordinary discovery state transition, change environmental variation,
   close another route, or claim navigation reachability.
4. Add focused Application coverage. Do not add scene/UI consumption, dialogue copy, journal/map
   entries, persistence, deeper-forest content, capabilities, or provider behavior.
5. Run `./scripts/quality.sh`, review the complete diff, update `README.md`, and leave one exact
   next slice for the old-shrine scene owner to consume the token without generalizing the remaining
   Apothecary's Assistant and Traveller effects.
