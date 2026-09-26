# Allrpcs Handover

## Current state

`main` includes merged PR #386 at `5f424f0`. Branch
`codex/phase-8-background-route-variations` continues the open Phase 8 background-route roadmap
item through two bounded slices.

Application now owns four immutable representative `SeekerBackgroundRouteVariation` definitions.
They retain exact background-choice and opening-region anchor instances in representative order:
Forester to the closed deeper-forest path, Scribe to the old shrine, Apothecary's Assistant to the
healer's garden, and Traveller to the southern road. The pure projection accepts only the exact
current selection state, its applied customization token, the exact opening-region projection, and
one complete exact-choice catalogue. Empty, unapplied, rebuilt, foreign, missing-anchor, duplicate,
and null inputs fail closed.

An immutable `SeekerBackgroundRouteVariationState` starts empty over that exact catalogue and
advances once from exact applied customization provenance. The main scene owns that state in
`SeekerBackgroundRouteVariationSceneSource`. The existing synchronous customization coordinator
preflights the route transition, then installs the selected profile, customized live character, and
selected route definition through exact compare-and-swap owners. The real U-key menu path proves the
Apothecary's Assistant definition is installed with its exact healer-garden anchor; a separate smoke
proves the Forester/closed-path mapping and one-shot preservation.

This work defines and selects an optional route only. It does not open the forest gate, change
collision or navigation, hide or remove any baseline route, grant capabilities, alter discovery,
render appearance, project pronouns into dialogue, persist customization, or add provider behavior.
The Phase 8 route-variation roadmap item therefore remains open.

## Validation and review

- Focused Application coverage passes seven tests for ordered exact definitions, all four exact
  selections, immutable state transition, and null, empty, unapplied, rebuilt, foreign-projection,
  missing-anchor, duplicate-catalogue, repeated, and malformed rejection.
- The new main-scene smoke passes inert exact four-route startup, one exact Forester selection,
  closed-path definition retention, and repeated-input preservation without navigation or
  presentation changes.
- The existing menu smoke passes the real modal flow with atomic exact customized-character and
  Apothecary's Assistant route-definition installation.
- Final host-local `./scripts/quality.sh` passed workflow validation, formatting, a zero-warning
  build, 2,811 Domain, 667 Application, 1,220 Infrastructure, and 8 architecture tests, scenario
  validation, every credential-free Godot smoke, and the graphical Compatibility launch. The
  restricted first-slice run reached all .NET tests and scenario validation before the known Godot
  `user://logs` restriction; the identical host runs supplied the focused and full Godot evidence.
- Session review checked the complete diff, exact-reference catalogue and state gates, synchronous
  three-owner preflight/install ordering, public API shape, scene dependency order, documentation
  claims, 120-character limits, sub-500-line C# files, formatting, and whitespace. No remaining
  correctness issue was found.

## Exact next bounded slice

Add one pure, additive route-access decision for the selected representative variation. Do not
change a Godot scene or activate a route yet.

Acceptance boundary:

1. Add one immutable access token retaining the exact selected route state, selected variation,
   and its exact destination anchor.
2. Add one pure policy/projection that returns that token only for the exact current selected state
   and the exact supplied destination anchor. Reject empty, stale, rebuilt, foreign, non-destination,
   repeated, and null inputs without changing route, customization, character, or navigation state.
3. Treat this token as additive authorization only: it must not imply that another baseline route is
   closed, that one background is mandatory, or that the destination is currently reachable.
4. Add focused Application coverage. Do not open the forest gate, change collision/navigation,
   grant capabilities, alter discovery, add presentation, persist customization, or add provider/UI
   behavior.
5. Run `./scripts/quality.sh`, review the complete diff, update `README.md`, and leave one exact next
   slice for a scene owner to consume the token at one representative destination without yet
   generalizing all four effects.
