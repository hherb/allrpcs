# Allrpcs Handover

## Current state

Phase 2k21 is implemented and reviewed without findings on
`codex/phase-2k21-focused-search-cancellation-input`, based directly on the PR #68 merge at
`8db417d`. Feature commit `40ba111` adds only the bounded cancellation input slice.

Physical C now maps to the sole temporary `focused_search_cancel` action. On that pressed action
only, `Main._UnhandledInput` selects the Seeker's active activity from
`FocusedSearchActivityController.Activities` and passes that activity's exact retained
`FocusedSearchRequest` instance to the existing `TryCancelSearch` boundary. Cancellation does not
read or depend on interaction focus. An active herb-patch or stone-marker search reaches the
existing cancellation policy and transition, replaces the snapshot with its empty successor, and
hides the cyan indicator. No active search and repeated cancellation preserve the existing
snapshot instance and presentation. No completion input, automatic lifecycle behavior,
result/status presentation, or discovery effect was added.

Host-shell `./scripts/quality.sh` passed with a zero-warning build, 491 Domain tests, 107
Application tests, six architecture tests, six credential-free Godot smokes, and the bounded
graphical launch. The interaction-focus smoke proves the physical-C-only mapping, exact retained
active request path, focus-independent cancellation after both eligible target starts, no-active
and repeated-cancellation identity, activity presentation, and unchanged start/focus/context,
movement, and healer behavior. The test-first host smoke failed specifically on the deliberately
absent cancellation mapping before implementation. A sandboxed direct Godot smoke failed before
scene loading because `user://logs` was unavailable, and the sandboxed quality run built with zero
warnings before reaching the known VSTest loopback restriction; the identical focused smoke and
complete quality spine passed on the host. The instruction text changed only to expose C and was
kept within the existing panel copy budget. The graphical launch passed, but no separate manual
visual acceptance was performed. Hosted desktop outputs remain unsigned development evidence,
not releases.

## Exact next bounded slice

Take **Phase 2k22: temporary focused-search completion input**.

Acceptance boundary:

1. Add one explicit temporary Godot input action and one narrow Godot-layer call site for
   completing the Seeker's current focused search from ordinary greybox play.
2. On that action only, pass the exact active `FocusedSearchRequest` instance already retained by
   the controller's immutable snapshot to `FocusedSearchActivityController.TryCompleteSearch`.
   Do not reconstruct the request or make completion depend on the current interaction focus.
3. One active Seeker search must reach the existing completion policy, transition, empty
   replacement snapshot, and hidden indicator. No active search and repeated completion must
   leave the existing snapshot instance and presentation unchanged.
4. Update only the temporary instruction text needed to expose the test control. Do not add final
   interaction UI, status/reason output, automatic completion, outcomes, or discovery effects.
5. Extend only the credential-free input/focus smoke coverage needed to prove the action mapping,
   exact active-request forwarding, successful focus-independent completion after both eligible
   target starts, failure-state identity, existing cancellation coverage, unchanged input
   handling, and unchanged movement/healer/focus behavior.
6. Document Phase 2k22 in `README.md` and `ROADMAP.md`, leave the broader bounded-focused-search
   item incomplete, run `./scripts/quality.sh`, review every intended path, and replace this file
   with current evidence, limits, and one next bounded slice.

Explicit exclusions:

- Do not add progress, duration, outcomes, discovery grants, visibility changes, events, effects,
  reason values, authoritative world-object aggregates, complete-character inspection,
  perception, ownership/capacity, world state, location, coordinates, or new range policy.
- Do not add passive noticing, reinterpretation, configurable Boolean expressions, alternative
  condition groups, serialization, persistence, providers, plugins, or credentials.
- Keep point-and-click movement, disease-time pausing, and the world-time debug overlay deferred
  until their retained prototype or authoritative-system prerequisites exist.
- Do not merge, release, distribute, sign, notarize, or change repository visibility without
  explicit user authorization.
