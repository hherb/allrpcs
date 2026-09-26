# Allrpcs Handover

## Current state

`main` includes merged PR #388 at `bf09347`. Branch
`codex/phase-8-scribe-inspection-access` continues the open Phase 8 background-route roadmap item
through three bounded slices. Draft PR #389 targets `main`.

Application now owns `OpeningRegionOldShrineInspectionAccess`. Its pure policy accepts only the
exact current Scribe route access, exact authoritative old-shrine binding, and exact authoritative
layered-inscription binding while no prior token exists. The old-shrine scene owner retains that
token once through compare-and-swap. The real customization coordinator installs it only for the
Scribe destination; the inscription remains concealed and discovery, presentation, evidence, and
environmental state remain unchanged. Ordinary I-key discovery still uses its existing transition.

Application also owns the scene-neutral `OpeningRegionRemedyPreparationAccess`. Its pure policy
accepts only exact current Apothecary's Assistant route access and the exact authoritative existing
healer-garden preparation binding while no prior token exists. The token retains the exact selected
variation, treatment context, and recipe without starting or advancing preparation. No Godot owner
or coordinator consumes this token yet.

Forester remains the only background that changes geometry: its existing exact access removes the
closed-path gate once. Traveller still has no effect. All access remains additive: no background is
mandatory; no baseline route closes; and no token grants capabilities, inventory, discovery,
navigation reachability, persistence, or provider behavior.

## Validation and review

- Eight focused Application tests pass for Scribe and Apothecary authorization, exact retention,
  all other backgrounds, stale, rebuilt, foreign, non-destination, repeated, malformed, and null
  rejection without changing discovery or treatment provenance.
- The Scribe main-scene smoke passes inert startup, one exact real customization, one exact
  old-shrine token, concealed inscription, unchanged discovery/environment state, unchanged
  Forester gate, and repeated-input preservation.
- Final host-local `./scripts/quality.sh` passed workflow validation, formatting, a zero-warning
  build, 2,811 Domain, 682 Application, 1,220 Infrastructure, and 8 architecture tests, scenario
  validation, every credential-free Godot smoke, and the graphical Compatibility launch. The
  restricted run passed every .NET test and scenario validation before the known Godot
  `user://logs` restriction; the identical host run supplied the complete Godot evidence.
- Session review covered the complete diff, exact predecessor/catalogue/destination and scene
  provenance, compare-and-swap ordering, ordinary and other-background inertness, node dependency
  order, public API shape, documentation accuracy, 120-character limits, sub-500-line C# files,
  formatting, and whitespace. Review found and fixed one exactness gap: the Scribe policy now
  separately requires authoritative and requested shrine/inscription identities, rejecting a
  fully rebuilt internally consistent pair.
- Draft PR #389's credential-free CI run `36228043377` at handover commit `3dc7bcb` failed all five
  jobs before any step was instantiated (`steps: []`), so it is Actions/account infrastructure and
  not product-test evidence.

## Exact next bounded slice

Let the existing healer-garden remedy-preparation scene own the already-tested exact Apothecary's
Assistant access token. Do not implement Traveller or change preparation behavior.

Acceptance boundary:

1. Add one nullable scene-owned `OpeningRegionRemedyPreparationAccess` and exact compare-and-swap
   `CanInstall`/`TryInstall` operations to `OpeningRegionRemedyPreparationSceneSource` after its
   existing binding becomes available.
2. Extend `SeekerCustomizationApplicationCoordinator` to preflight the token only when exact
   current route access targets that exact scene-owned healer-garden binding, then install it once
   with the existing selection, character, route-state, and generic-access transaction. An absent
   scene binding must fail before any partial state change.
3. Add one switch-gated main-scene smoke proving the exact Apothecary selection retains the exact
   route access, variation, binding, treatment context, and recipe while the preparation controller
   remains inert. Prove Forester, Scribe, Traveller, null, stale, rebuilt, repeated, and pre-binding
   paths cannot install or replace it.
4. Keep the effect authorization-only. Do not start or advance preparation, consume ingredients,
   grant inventory/capabilities, change navigation or geometry, alter patient/evidence state, add
   UI/dialogue/journal/map content, persist customization, invoke a provider, or implement the
   Traveller effect.
5. Run `./scripts/quality.sh`, review the complete diff, update `README.md`, and leave one exact
   next slice for a scene-neutral Traveller southern-road/wagon access decision.
