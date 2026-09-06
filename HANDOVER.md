# Allrpcs Handover

## Current state

Phase 2l20 is implemented on `codex/phase-2l20-resolved-description-presentation`, based directly
on PR #116's merge commit `28a8d76`.

Each existing `TemporaryResolvedPerceptionIndicator` now clears its reused label by default and
presents the exact looked-up `WorldObject.Description.Text` only while the controller's retained
perception has that object at the exact `VisibilityState.Identified` singleton. Null or absent
objects, null or unrelated perception, and every other visibility singleton clear and hide the
label. A distinct value-equal world object supplies its own replacement description without
changing either object or any perception snapshot. The two labels wrap within separate bounds and
no UI node was added.

Test-first interaction evidence failed on the old authored `IDENTIFIED` text, then passed with
positive coverage for both source-authored descriptions and replacement text. Every negative case
must clear an already-presented description, and lifecycle evidence preserves marker labels, focus
status, focused-search result text, controller snapshots, source objects, geometry, visibility, and
ordering. After final code review, host-shell `./scripts/quality.sh` passed with workflow and
formatting validation, a zero-warning build, 661 Domain tests, 107 Application tests, six
architecture tests, all fourteen Godot smokes, and the bounded graphical launch. A captured final
interaction-smoke frame showed both wrapped descriptions simultaneously without overlap or
clipping.

The roadmap's bounded focused-search connection to world objects, perception change, and discovery
presentation is checked. Temporary sources, controllers, F/C/E/R controls, greybox focus, and the
resolved-description labels remain non-authoritative and are not final discovery UI.

## Exact next bounded slice

Take **Phase 2m1: immutable passive-noticing candidate**.

Acceptance boundary:

1. Add one sealed immutable `Allrpcs.Domain.Discovery.PassiveNoticingCandidate` containing only an
   existing `CharacterId ObserverCharacterId` and `ObjectId TargetObjectId`.
2. Reject either null input and otherwise retain the exact supplied identifier instances. Add no
   equality, ordering, lookup, eligibility, visibility, or mutation behavior beyond the existing
   identifier semantics.
3. Add focused tests first for exact nested-instance retention, distinct value-equal identifiers,
   both null boundaries, input preservation, and the minimal public API surface.
4. Update README and ROADMAP evidence, keep passive noticing incomplete, run
   `./scripts/quality.sh`, review every intended path, and prune this file again to current evidence
   plus one next slice.

Explicit exclusions:

- Do not select, collect, rank, or order passive-noticing candidates; inspect world objects,
  locations, coordinates, distance, line of sight, environmental state, capabilities, inventory,
  focus, input, or time; or define passive-noticing eligibility, outcomes, or transitions.
- Do not change perception or visibility, grant discovery, create knowledge or evidence, or add
  Application/Godot sources, controllers, runtime wiring, events, persistence, serialization,
  providers, plugins, credentials, or UI.
- Keep point-and-click movement, disease-time pausing, and the world-time debug overlay deferred.
- Do not merge, release, distribute, sign, notarize, or change repository visibility without
  explicit user authorization.
