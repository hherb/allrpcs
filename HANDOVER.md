# Allrpcs Handover

## Current state

Phase 2k18 is implemented and reviewed without findings on
`codex/phase-2k18-focused-search-start-bridge`, based directly on the PR #65 merge at
`986c265`.

`InteractionFocusController.TryStartFocusedSearch` supplies the exact Seeker `CharacterId` owned
by `FocusedSearchActivityController` to the existing focused-request factory. When focus is valid,
it passes the resulting request and the exact supplied `DiscoveryConditionSet`,
`CharacterCapabilitySet`, and `CharacterInventory` instances to the existing condition-aware start
path and returns that path's result. Absent, hidden, disabled, and out-of-range focus returns false
without invoking the start path. No ordinary input or production discovery context calls the bridge.

Host-shell `./scripts/quality.sh` passed with a zero-warning build, 491 Domain tests, 107
Application tests, six architecture tests, six credential-free Godot smokes, and the bounded
graphical launch. The interaction-focus smoke covers eligible, unsatisfied, and already-active
composition; exact request identifier retention; unchanged supplied inputs; failure-state
identity; activity-indicator presentation; and short-circuiting for every existing invalid-focus
case. The test-first Game build failed only for the deliberately missing start method before
implementation. A sandboxed direct Godot launch failed before scene loading because `user://logs`
was unavailable; the same smoke and the complete quality spine passed on the host. No manual visual
acceptance was needed because presentation behavior is unchanged. Hosted desktop outputs remain
unsigned development evidence, not releases.

## Exact next bounded slice

Take **Phase 2k19: temporary focused-search context source**.

Acceptance boundary:

1. Add one narrow Godot-layer source that constructs and retains existing
   `DiscoveryConditionSet`, `CharacterCapabilitySet`, and `CharacterInventory` values for the
   temporary greybox Seeker and the two existing visible interaction targets.
2. Reuse each target's exact authored `ObjectId` instance in its discovery requirement. Give the
   herb patch one capability-only requirement and the stone marker one tool-only requirement;
   retain the exact created capability, tool-object, requirement, set, and aggregate instances.
3. Expose only the three existing aggregate inputs needed by
   `InteractionFocusController.TryStartFocusedSearch`. Do not start a search, poll focus, or own
   lifecycle state in the context source.
4. Extend only credential-free focused-search/focus smoke coverage needed to prove stable
   authoring, exact instance retention, both eligible target contexts, unchanged inputs, and no
   scene behavior change.
5. Do not add an ordinary search input or invoke the start bridge from ordinary play. Do not add
   cancellation, completion, progress, duration, or outcomes.
6. Document Phase 2k19 in `README.md` and `ROADMAP.md`, leave the broader bounded-focused-search
   item incomplete, run `./scripts/quality.sh`, review every intended path, and replace this file
   with current evidence, limits, and one next bounded slice.

Explicit exclusions:

- Do not make the temporary greybox targets authoritative world-object aggregates or add
  complete-character, perception, ownership/capacity, or world-state inspection.
- Do not add discovery grants, visibility changes, events, effects, reason values, final UI/art,
  object-existence validation, location, coordinates, radius, or interaction-range policy.
- Do not add passive noticing, reinterpretation, configurable Boolean expressions, alternative
  condition groups, serialization, persistence, providers, plugins, or credentials.
- Keep point-and-click movement, disease-time pausing, and the world-time debug overlay deferred
  until their retained prototype or authoritative-system prerequisites exist.
- Do not merge, release, distribute, sign, notarize, or change repository visibility without
  explicit user authorization.
