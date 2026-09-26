# Allrpcs Handover

## Current state

`main` includes merged PR #389 at `b4c7788`. Branch
`codex/phase-8-apothecary-traveller-access` completes the open Phase 8 background-route roadmap
item through three bounded slices. Draft PR #390 targets `main`.

The existing healer-garden remedy-preparation scene now owns one nullable
`OpeningRegionRemedyPreparationAccess`. Exact compare-and-swap preflight and installation retain
only the current Apothecary's Assistant route access, exact scene binding, treatment context, and
recipe. `SeekerCustomizationApplicationCoordinator` includes this token in the existing synchronous
customization transaction and fails before mutation when the destination binding is unavailable.
The preparation controller, ingredients, patient, evidence, geometry, and presentation stay inert.

Application now owns the scene-neutral `OpeningRegionSouthernRoadWagonAccess`. Its pure policy
accepts only exact current Traveller route access and exact authoritative southern-road/wagon
binding while no prior token exists. The existing road/wagon scene retains that token once through
compare-and-swap, and the customization coordinator installs it within the same transaction.
Road/wagon geometry, collision, navigation, marker presentation, discovery, and evidence remain
unchanged.

All four background variations now have their intended additive boundary: Forester alone removes
the closed-path gate; Scribe retains inert old-shrine inspection access; Apothecary's Assistant
retains inert preparation access; and Traveller retains inert southern-road/wagon access. No
background is mandatory, no baseline route closes, and none of these access tokens grants
inventory, capabilities, discovery, persistence, provider behavior, or downstream interactions.
The Phase 8 route-variation roadmap item is complete.

## Validation and review

- Four focused Application tests pass for Traveller authorization, exact retention, other
  backgrounds, stale, rebuilt, foreign, malformed, non-destination, repeated, and null rejection.
- The Apothecary main-scene smoke passes inert startup; other-background, stale, rebuilt, null,
  repeated, and pre-binding rejection; one exact real customization and token; exact route,
  variation, binding, treatment-context, and recipe retention; and unchanged preparation state.
- The Traveller main-scene smoke passes the equivalent exact and fail-closed paths, one exact real
  customization and token, and unchanged road/wagon layout, collision, and marker presentation.
- Final host-local `./scripts/quality.sh` passed workflow validation, formatting, a zero-warning
  build, 2,811 Domain, 686 Application, 1,220 Infrastructure, and 8 architecture tests, scenario
  validation, every credential-free Godot smoke, and the graphical Compatibility launch. The
  restricted run passed all .NET tests and scenario validation before the known Godot
  `user://logs` restriction; the identical host run supplied the complete Godot evidence.
- Session review covered the complete diff, transaction preflight/commit ordering, exact route and
  scene provenance, one-shot ownership, absent-binding behavior, other-background isolation,
  preparation and road/wagon inertness, public API documentation, 120-character limits,
  sub-500-line C# files, formatting, and whitespace. Review replaced the one duplicated Apothecary
  key literal in the coordinator with a named constant; no further issues remain.
- Draft PR #390's credential-free CI run `36229065373` at implementation commit `82b161c` failed
  all five jobs before any step was instantiated (`steps: []`), so it is Actions/account
  infrastructure and not product-test evidence.

## Exact next bounded slice

Start the Phase 8 journal item with one asset-neutral, content-free five-section catalogue. Do not
add journal entries, scene ownership, UI, persistence, or provider behavior yet.

Acceptance boundary:

1. Add one immutable Application journal-section definition with a canonical key and bounded,
   nonblank display label, plus a representative source returning exactly `observations`,
   `testimony`, `hypotheses`, `experiments`, and `open-questions` in that stable order.
2. Reject null, empty, whitespace-only, overlong, duplicate, reordered, or unknown canonical
   definitions at the catalogue boundary. Copy caller input and expose it read-only.
3. Add focused tests for exact authored values and instances, stable order, independent returned
   catalogues, caller-list isolation, and every invalid path. Keep the public surface minimal and
   fully documented.
4. Keep this slice semantic and asset-neutral. Do not project perception, dialogue, investigation,
   or experiment records; add editable notes; open a panel; handle input; serialize state; change
   any authoritative gameplay state; or call a provider.
5. Run `./scripts/quality.sh`, review the complete diff, update `README.md`, and leave one exact
   next slice to bind the catalogue's observations section to an exact filtered Seeker perception
   snapshot without UI or persistence.
