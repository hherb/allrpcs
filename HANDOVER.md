# Allrpcs Handover

## Current state

Phase 2k20 is implemented and reviewed without findings on
`codex/phase-2k20-focused-search-input`, based directly on the PR #67 merge at `0b4f373`.
Feature commit `f82919f` is pushed, and draft PR #68
(`https://github.com/hherb/allrpcs/pull/68`) is open. Pull-request merge remains separately
unauthorized.

Physical F now maps to the sole temporary `focused_search_start` action. On that pressed action
only, `Main._UnhandledInput` passes the exact `DiscoveryConditionSet`, `CharacterCapabilitySet`,
and `CharacterInventory` instances retained by `TemporaryFocusedSearchContextSource` to the
existing `InteractionFocusController.TryStartFocusedSearch` boundary. Valid herb-patch and
stone-marker focus reaches the existing condition-aware start, immutable activity state, and cyan
indicator. Clear, hidden, disabled, or out-of-range focus and an already-active Seeker preserve the
existing snapshot instance. Released and unrelated actions do not start a search. No automatic
polling, automatic start, result/status presentation, or further lifecycle input was added.

Host-shell `./scripts/quality.sh` passed with a zero-warning build, 491 Domain tests, 107
Application tests, six architecture tests, six credential-free Godot smokes, and the bounded
graphical launch. The interaction-focus smoke proves the physical-F-only mapping, exact retained
context remaining unchanged through the input path, both eligible target starts, fail-closed
invalid focus, repeated-start identity, activity presentation, and unchanged focus/input behavior.
The test-first smoke failed on the deliberately absent input mapping before implementation. A
sandboxed direct Godot launch failed before scene loading because `user://logs` was unavailable,
and the sandboxed quality run reached the known VSTest loopback restriction; the identical smoke
and complete quality spine passed on the host. The instruction text changed only to expose F; the
graphical launch passed, but no separate manual visual acceptance was performed. Hosted desktop
outputs remain unsigned development evidence, not releases.

## Exact next bounded slice

Take **Phase 2k21: temporary focused-search cancellation input**.

Acceptance boundary:

1. Add one explicit temporary Godot input action and one narrow Godot-layer call site for
   cancelling the Seeker's current focused search from ordinary greybox play.
2. On that action only, pass the exact active `FocusedSearchRequest` instance already retained by
   the controller's immutable snapshot to `FocusedSearchActivityController.TryCancelSearch`. Do
   not reconstruct the request or make cancellation depend on the current interaction focus.
3. One active Seeker search must reach the existing cancellation policy, transition, empty
   replacement snapshot, and hidden indicator. No active search and repeated cancellation must
   leave the existing snapshot instance and presentation unchanged.
4. Update only the temporary greybox instruction text needed to expose the test control. Do not add
   final interaction UI, status/reason output, automatic cancellation, or completion behavior.
5. Extend only credential-free input/focus smoke coverage needed to prove the action mapping, exact
   active-request forwarding, successful cancellation after either eligible target start,
   failure-state identity, focus independence, existing input handling, and unchanged
   movement/healer/focus behavior.
6. Document Phase 2k21 in `README.md` and `ROADMAP.md`, leave the broader bounded-focused-search
   item incomplete, run `./scripts/quality.sh`, review every intended path, and replace this file
   with current evidence, limits, and one next bounded slice.

Explicit exclusions:

- Do not add completion, progress, duration, outcomes, discovery grants, visibility changes,
  events, effects, reason values, authoritative world-object aggregates, complete-character
  inspection, perception, ownership/capacity, world state, location, coordinates, or new range
  policy.
- Do not add passive noticing, reinterpretation, configurable Boolean expressions, alternative
  condition groups, serialization, persistence, providers, plugins, or credentials.
- Keep point-and-click movement, disease-time pausing, and the world-time debug overlay deferred
  until their retained prototype or authoritative-system prerequisites exist.
- Do not merge, release, distribute, sign, notarize, or change repository visibility without
  explicit user authorization.
