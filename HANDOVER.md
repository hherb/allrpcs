# Allrpcs Handover

## Current state

Phase 2k19 is implemented and reviewed without findings on
`codex/phase-2k19-temporary-focused-search-context`, based directly on the PR #66 merge at
`9457c74`. Feature commit `7dac54c` is pushed, and draft PR #67
(`https://github.com/hherb/allrpcs/pull/67`) is open. Pull-request merge remains separately
unauthorized.

`TemporaryFocusedSearchContextSource` is the sole scene source for the temporary greybox Seeker's
three focused-search inputs. It reuses the existing herb patch's exact `ObjectId` in one
capability-only requirement and the stone marker's exact `ObjectId` in one tool-only requirement.
The source retains the exact created capability, tool identifier, requirements, requirement sets,
`DiscoveryConditionSet`, `CharacterCapabilitySet`, and `CharacterInventory` instances through the
three exposed aggregates. It does not start a search, poll focus, or own lifecycle state. Ordinary
play still does not invoke `InteractionFocusController.TryStartFocusedSearch`.

Host-shell `./scripts/quality.sh` passed with a zero-warning build, 491 Domain tests, 107
Application tests, six architecture tests, six credential-free Godot smokes, and the bounded
graphical launch. The interaction-focus smoke proves the stable authored values; exact target,
tool, requirement, set, and aggregate instance retention; eligible starts for both visible target
contexts; unchanged inputs and failure-state identity; and unchanged focus and activity-indicator
presentation. The test-first Game build failed only for the deliberately missing context-source
type before implementation. A sandboxed direct Godot launch failed before scene loading because
`user://logs` was unavailable, and the sandboxed quality run reached the known VSTest loopback
restriction; the identical smoke and complete quality spine passed on the host. No manual visual
acceptance was needed because ordinary scene presentation is unchanged. Hosted desktop outputs
remain unsigned development evidence, not releases.

## Exact next bounded slice

Take **Phase 2k20: temporary focused-search start input**.

Acceptance boundary:

1. Add one explicit temporary Godot input action and one narrow Godot-layer call site for starting
   a focused search from ordinary greybox play.
2. On that action only, pass the exact `DiscoveryConditionSet`, `CharacterCapabilitySet`, and
   `CharacterInventory` instances retained by `TemporaryFocusedSearchContextSource` to the existing
   `InteractionFocusController.TryStartFocusedSearch` boundary. Do not reconstruct or replace any
   context input.
3. A valid herb-patch or stone-marker focus may reach the existing condition-aware start,
   immutable activity state, and indicator presentation. Clear, hidden, disabled, or out-of-range
   focus and an already-active Seeker must leave the existing snapshot instance unchanged.
4. Update only the temporary greybox instruction text needed to expose the test control. Do not add
   final interaction UI, status/reason output, automatic polling, or automatic start behavior.
5. Extend only credential-free input/focus smoke coverage needed to prove the action mapping,
   exact context forwarding, both eligible target starts, fail-closed invalid focus, repeated-start
   identity, existing input handling, and unchanged movement/healer/focus behavior.
6. Document Phase 2k20 in `README.md` and `ROADMAP.md`, leave the broader bounded-focused-search
   item incomplete, run `./scripts/quality.sh`, review every intended path, and replace this file
   with current evidence, limits, and one next bounded slice.

Explicit exclusions:

- Do not add cancellation, completion, progress, duration, outcomes, discovery grants, visibility
  changes, events, effects, reason values, authoritative world-object aggregates, complete-character
  inspection, perception, ownership/capacity, world state, location, coordinates, or new range
  policy.
- Do not add passive noticing, reinterpretation, configurable Boolean expressions, alternative
  condition groups, serialization, persistence, providers, plugins, or credentials.
- Keep point-and-click movement, disease-time pausing, and the world-time debug overlay deferred
  until their retained prototype or authoritative-system prerequisites exist.
- Do not merge, release, distribute, sign, notarize, or change repository visibility without
  explicit user authorization.
