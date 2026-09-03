# Allrpcs Handover

## Current state

Phase 2k5 is implemented, reviewed without findings, and locally validated on
`codex/phase-2k5-focused-search-start-transition`, based directly on the PR #52 merge at
`2dd8664`. Feature commit `4f5d7a0` is pushed, and draft PR #53
(`https://github.com/hherb/allrpcs/pull/53`) is open. Pull-request merge remains separately
unauthorized.

`FocusedSearchStartTransition.StartSearch` delegates eligibility to
`FocusedSearchStartPolicy`. For an eligible request it returns a new
`FocusedSearchActivitySnapshot` retaining existing activity instances in stable order before one
new `FocusedSearchActivity` that retains the exact request instance. It rejects an ineligible
request with `InvalidOperationException`, rejects null inputs through the policy, and never mutates
the request, existing activities, or input snapshot.

The test-first build failed only because `FocusedSearchStartTransition` did not exist. All nine
focused tests then passed on the host. Host-shell `./scripts/quality.sh` passed cleanly with 432
Domain tests, 100 Application tests, six architecture tests, four Godot smokes, and the bounded
graphical launch. The sandboxed focused test run reached the known VSTest loopback denial; the
identical host run passed. No manual play acceptance is required for this engine-independent Domain
slice. `Domain -> []` remains intact, and hosted desktop outputs remain unsigned development
evidence, not releases.

## Exact next bounded slice

After this pull request is reported merged, take **Phase 2k6: pure focused-search cancellation
eligibility**.

Acceptance boundary:

1. Add a static `FocusedSearchCancellationPolicy` in `Allrpcs.Domain.Discovery` with one pure
   `CanCancelSearch(FocusedSearchRequest, FocusedSearchActivitySnapshot)` operation.
2. Return true exactly when the snapshot contains an activity whose complete request is value-equal
   to the supplied request. Return false for an empty snapshot, a different target for the same
   searcher, a different searcher for the same target, and all unrelated activities.
3. Reject null inputs and never mutate the request, existing activity instances, or snapshot.
4. Add focused pure tests for empty and populated snapshots, exact matching, value equality,
   same-searcher/different-target rejection, different-searcher/same-target rejection, unrelated
   entries, null rejection, non-mutation, and the deliberately limited static public surface.
5. Preserve `Domain -> []`; document the pure eligibility policy in `README.md`, leave the broader
   bounded-focused-search roadmap item incomplete, and replace this file with current evidence,
   limits, and one next bounded slice.
6. Use test-first development, run `./scripts/quality.sh`, review every intended path, and commit
   only the reviewed slice.

Explicit exclusions:

- Do not cancel or remove an activity, return a changed snapshot, mutate collections, or add a
  cancellation command, event, effect, error, reason, permission, or presentation behavior.
- Do not treat cancellation as completion or add progress, completion, replacement, scheduling,
  duration, timing, status values, outcomes, or lifecycle transitions.
- Do not define target exclusivity, range, radius, location, coordinates, movement, animation,
  search-condition evaluation, discovery grants, visibility changes, or presentation policy.
- Do not inspect a complete character, perception snapshot, ownership/capacity snapshot, world
  state, object existence, Godot scene, or UI state.
- Do not add passive noticing, reinterpretation, configurable Boolean expressions, new discovery
  requirement types, scenario content, serialization, persistence, Godot, UI, providers, or
  plugins.
- Keep point-and-click movement, disease-time pausing, and the world-time debug overlay deferred
  until their own retained prototype or authoritative-system prerequisites exist.
- Do not merge, release, distribute, sign, notarize, change repository visibility, or add
  credentials without explicit user authorization.
