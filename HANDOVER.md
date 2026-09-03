# Allrpcs Handover

## Current state

Phase 2k7 is implemented, reviewed without findings, and locally validated on
`codex/phase-2k7-focused-search-cancellation-transition`, based directly on the PR #54 merge at
`a35f941`. Feature commit `385f89e` is pushed, and draft PR #55
(`https://github.com/hherb/allrpcs/pull/55`) is open. Pull-request merge remains separately
unauthorized.

`FocusedSearchCancellationTransition.CancelSearch` delegates eligibility to
`FocusedSearchCancellationPolicy`. It rejects an ineligible request and otherwise returns a new
snapshot with the complete value-equal request removed while retaining every nonmatching existing
activity instance in stable order. It rejects null inputs through the policy and never mutates the
request, existing activities, or input snapshot.

The test-first build failed only because `FocusedSearchCancellationTransition` did not exist. All
11 focused tests then passed on the host. Host-shell `./scripts/quality.sh` passed cleanly with 452
Domain tests, 100 Application tests, six architecture tests, four Godot smokes, and the bounded
graphical launch. The sandboxed focused and full test runs reached the known VSTest loopback
denial; the identical host runs passed. No manual play acceptance is required for this
engine-independent Domain slice. `Domain -> []` remains intact, and hosted desktop outputs remain
unsigned development evidence, not releases.

## Exact next bounded slice

After this pull request is reported merged, take **Phase 2k8: pure focused-search completion
eligibility**.

Acceptance boundary:

1. Add a static `FocusedSearchCompletionPolicy` in `Allrpcs.Domain.Discovery` with one pure
   `CanCompleteSearch(FocusedSearchRequest, FocusedSearchActivitySnapshot)` operation returning a
   Boolean.
2. Return true exactly when the snapshot contains an activity whose complete request is
   value-equal to the supplied request. Return false for an empty snapshot, either complete-request
   near miss, and unrelated activities.
3. Reject null inputs and never mutate the request, existing activity instances, or snapshot.
4. Add focused tests for empty, exact, value-equal, near-match, and unrelated snapshots; null
   rejection; non-mutation; and the deliberately limited static public surface.
5. Preserve `Domain -> []`; document the pure completion-eligibility decision in `README.md`, leave
   the broader bounded-focused-search roadmap item incomplete, and replace this file with current
   evidence, limits, and one next bounded slice.
6. Use test-first development, run `./scripts/quality.sh`, review every intended path, and commit
   only the reviewed slice.

Explicit exclusions:

- Do not add a completion transition, result, outcome, event, effect, reason, permission,
  presentation behavior, or in-place collection mutation.
- Do not expose or reuse cancellation as completion, even though the two eligibility decisions
  currently share the same complete-request matching shape.
- Do not add progress, replacement, scheduling, duration, timing, status values, or any other
  lifecycle transition.
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
