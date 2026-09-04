# Allrpcs Handover

## Current state

Phase 2k22 is implemented and reviewed without findings on
`codex/phase-2k22-focused-search-completion-input`, based directly on PR #69's merge commit
`df8934c`.

Physical E now maps to the sole temporary `focused_search_complete` action. On that pressed
action only, `Main._UnhandledInput` selects the Seeker's active activity from
`FocusedSearchActivityController.Activities` and passes its exact retained
`FocusedSearchRequest` to the existing `TryCompleteSearch` boundary. Completion is independent
of interaction focus. Active herb-patch and stone-marker searches reach the existing completion
policy and transition, replace the immutable snapshot with its empty successor, and hide the
cyan indicator. No active search and repeated completion preserve snapshot identity and
presentation. Existing F start, C cancellation, other input, focus, movement, and healer
behavior remain unchanged.

Host-shell `./scripts/quality.sh` passed with a zero-warning build, 491 Domain tests, 107
Application tests, six architecture tests, six Godot smokes, and the bounded graphical launch.
The focused smoke failed test-first only because the E mapping was absent, then passed with exact
active-request retention, focus-independent cancellation and completion after both eligible
target starts, failure-state identity, context/input stability, and existing presentation
coverage. The instruction panel changed only to expose E. The graphical launch passed, but no
separate manual visual acceptance was performed. Hosted desktop outputs remain unsigned
development evidence, not releases.

## Exact next bounded slice

Take **Phase 2k23: temporary focused-search lifecycle result presentation**.

Acceptance boundary:

1. Add one temporary Godot-layer text presentation for the last explicit F, C, or E
   focused-search attempt; keep it separate from the existing activity ring and healer mode.
2. Use only the existing Boolean return from the start, cancellation, or completion call path to
   present the attempted action as changed or unchanged. Do not add reason values or infer why an
   attempt failed.
3. Initialize the text deterministically and update it only for a pressed focused-search action.
   Released or unrelated inputs must preserve the exact text; existing input-handled behavior
   must remain unchanged.
4. Preserve the current lifecycle behavior, snapshot identity rules, exact retained-request
   forwarding, focus independence, and indicator presentation.
5. Extend only the credential-free input/focus smoke needed to prove initial text, successful and
   unchanged results for all three actions, both eligible targets, and unchanged movement, healer,
   focus, context, and lifecycle behavior.
6. Document Phase 2k23 in `README.md` and compact `ROADMAP.md`, leave the broader focused-search
   item incomplete, run `./scripts/quality.sh`, review every intended path, and replace this file
   with current evidence, limits, and one next bounded slice.

Explicit exclusions:

- Do not add progress, duration, automatic lifecycle behavior, outcomes, discovery grants,
  visibility changes, events, effects, reason/error taxonomies, final interaction UI, or new
  Domain/Application lifecycle contracts.
- Do not add authoritative world objects, complete-character inspection, perception mutation,
  ownership/capacity, world state, location, coordinates, range policy, passive noticing,
  reinterpretation, serialization, persistence, providers, plugins, or credentials.
- Keep point-and-click movement, disease-time pausing, and the world-time debug overlay deferred
  until their retained prototype or authoritative-system prerequisites exist.
- Do not merge, release, distribute, sign, notarize, or change repository visibility without
  explicit user authorization.
