# Allrpcs Handover

## Current state

Phase 2k12 is implemented, reviewed without findings, and locally validated on
`codex/phase-2k12-focused-search-activity-presentation`, based directly on the PR #59 merge at
`16d7cde`. Feature commit `1077a80` is pushed, and draft PR #60
(`https://github.com/hherb/allrpcs/pull/60`) is open. Pull-request merge remains separately
unauthorized.

`FocusedSearchActivityIndicator.Present` is the one Godot presentation operation under the
existing Seeker. It accepts an existing `CharacterId` and `FocusedSearchActivitySnapshot`,
delegates the complete decision to `FocusedSearchActivityIndicatorPolicy.ShouldShow`, and changes
only the visibility of one temporary authored cyan ring. The ring is hidden both in the scene and
on adapter readiness. Ordinary play constructs or injects no focused-search activity.

The focused host smoke passed hidden-by-default, empty, unrelated, one matching, multiple matching,
and subsequent-empty presentation. It also proved exactly one indicator remains authored and the
supplied identifiers, activities, and snapshots retain their exact instances and order. Host-shell
`./scripts/quality.sh` passed with 480 Domain tests, 107 Application tests, six architecture tests,
five credential-free Godot smokes, and the bounded graphical launch. The sandboxed run restored,
formatted, and built successfully before VSTest was denied its local loopback bind. No manual visual
acceptance was performed; the cyan ring remains temporary greybox evidence. Hosted desktop outputs
remain unsigned development evidence, not releases.

## Exact next bounded slice

After this pull request is reported merged, take **Phase 2k13: focused-search activity state
controller**.

Acceptance boundary:

1. Add one small Godot-side `FocusedSearchActivityController` as the sole live owner of an immutable
   `FocusedSearchActivitySnapshot`, initialized empty for the existing Seeker and existing indicator
   adapter.
2. Give the controller explicit start, cancel, and complete operations accepting an existing
   `FocusedSearchRequest`. Reject requests for any character other than the controller's exact
   Seeker identifier, delegate state changes only to the existing Domain transitions, replace the
   owned snapshot only after a successful transition, and then present that snapshot through the
   existing adapter.
3. Author only the Seeker's stable `CharacterId` needed by that controller. Do not construct a fake
   request or active search during ordinary play, automatically invoke a lifecycle operation, or add
   player input in this slice.
4. Add a credential-free Godot smoke proving empty initial state and a hidden indicator; successful
   start and visible presentation; fail-closed unrelated and ineligible starts with unchanged state;
   successful cancellation and completion paths that hide the last activity; failed cancellation
   and completion with unchanged state; and retention of supplied request/activity instances without
   input mutation.
5. Preserve `Godot -> Application -> Domain` and `Domain -> []`; document the temporary controller
   in `README.md`, record Phase 2k13 in `ROADMAP.md`, leave the broader bounded-focused-search item
   incomplete, and replace this file with current evidence, limits, and one next bounded slice.
6. Use test-first development, run `./scripts/quality.sh`, review every intended path, and commit
   only the reviewed slice. Treat headless scene-state checks separately from native visual
   acceptance; claim the latter only if it is actually performed.

Explicit exclusions:

- Do not add search targets to the scene, display text, status values, player input, animation,
  progress, duration, timing, outcomes, discovery grants, visibility changes outside the temporary
  indicator, events, effects, reasons, or final UI/art.
- Do not add new lifecycle policy or transition logic, mutate snapshots in place, or change existing
  Domain snapshots, policies, transitions, the Application indicator policy, or the presentation
  adapter.
- Do not inspect a complete character, perception snapshot, ownership/capacity snapshot, world
  state, object existence, location, coordinates, range, radius, interaction focus, or other UI
  state.
- Do not add search-condition evaluation, passive noticing, reinterpretation, configurable Boolean
  expressions, scenario content, serialization, persistence, providers, or plugins.
- Keep point-and-click movement, disease-time pausing, and the world-time debug overlay deferred
  until their own retained prototype or authoritative-system prerequisites exist.
- Do not merge, release, distribute, sign, notarize, change repository visibility, or add credentials
  without explicit user authorization.
