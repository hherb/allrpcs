# Allrpcs Handover

## Current state

Phase 2k15 is implemented and reviewed without findings on
`codex/phase-2k15-condition-aware-focused-search-controller-start`, based directly on the PR #62
merge at `6dae883`.

The existing Godot `FocusedSearchActivityController.TryStartSearch` now accepts an existing
`DiscoveryConditionSet`, `CharacterCapabilitySet`, and `CharacterInventory` with the request. It
keeps the owned-character rejection, delegates the complete remaining decision to
`FocusedSearchEligibilityPolicy`, and reaches the existing start transition and presentation only
when eligible. Rejected requests retain the exact prior snapshot. Cancellation, completion, the
scene, and ordinary play are unchanged.

Host-shell `./scripts/quality.sh` passed with a zero-warning build, 491 Domain tests, 107
Application tests, six architecture tests, six credential-free Godot smokes, and the bounded
graphical launch. The extended controller smoke covers capability-only, tool-only, combined,
absent, unsatisfied, unrelated, already-active, and wrong-character starts; cancellation and
completion; presentation visibility; and exact input, activity, order, and snapshot retention. The
sandboxed sequence restored, formatted, and built successfully before VSTest was denied its local
loopback bind. No manual visual acceptance was needed because this slice changes no presentation.
Hosted desktop outputs remain unsigned development evidence, not releases.

## Exact next bounded slice

Take **Phase 2k16: stable focused-search target bridge**.

Acceptance boundary:

1. Give each existing `GreyboxInteractionTarget` one authored canonical `ObjectId` text value,
   parse it once through the existing Domain identifier boundary, and retain that exact identifier
   instance while the node is ready.
2. Extend only the existing `InteractionFocusController` selection result so the currently focused
   visible, enabled target exposes its existing `ObjectId`; expose no identifier when focus is
   absent, hidden, disabled, or out of range.
3. Author distinct stable identifiers on the four existing scene markers and extend the existing
   credential-free interaction-focus smoke for nearest selection, authored-order ties, focus
   changes, clearing, hidden and disabled exclusion, and exact identifier-instance retention.
4. Do not construct a `FocusedSearchRequest`, invoke `FocusedSearchActivityController`, or add
   search input, discovery conditions, capabilities, inventory, lifecycle changes, or outcomes in
   this slice. Do not change focus range, ordering, geometry, labels, scene layout, or ordinary
   input behavior.
5. Document the target bridge in `README.md`, record Phase 2k16 in `ROADMAP.md`, leave the broader
   bounded-focused-search item incomplete, and replace this file with current evidence, limits,
   and one next bounded slice.
6. Use test-first development, run `./scripts/quality.sh`, review every intended path, and commit
   only the reviewed slice.

Explicit exclusions:

- Do not add search duration, progress, timing, outcomes, discovery grants, visibility changes,
  events, effects, reason values, or final UI/art.
- Do not add complete-character, perception, ownership/capacity, or world-state inspection;
  object-existence validation; location, coordinate, radius, or interaction-range policy; or
  scenario object aggregates.
- Do not add passive noticing, reinterpretation, configurable Boolean expressions, alternative
  condition groups, serialization, persistence, providers, plugins, or credentials.
- Keep point-and-click movement, disease-time pausing, and the world-time debug overlay deferred
  until their retained prototype or authoritative-system prerequisites exist.
- Do not merge, release, distribute, sign, notarize, or change repository visibility without
  explicit user authorization.
