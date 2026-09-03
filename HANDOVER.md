# Allrpcs Handover

## Current state

Phase 2k6 is implemented, reviewed without findings, and locally validated on
`codex/phase-2k6-focused-search-cancellation-policy`, based directly on the PR #53 merge at
`8ae2263`. Feature commit `8cf0459` contains the reviewed implementation, tests, README, and
roadmap update. Pull-request merge remains separately unauthorized.

`FocusedSearchCancellationPolicy.CanCancelSearch` returns true exactly when the activity snapshot
contains an activity whose complete request is value-equal to the supplied request. It returns
false for empty snapshots, either same-searcher/different-target or
different-searcher/same-target near misses, and unrelated activities. It rejects null inputs and
never mutates the request, existing activity instances, or snapshot.

The test-first build failed only because `FocusedSearchCancellationPolicy` did not exist. All nine
focused tests then passed on the host. Host-shell `./scripts/quality.sh` passed cleanly with 441
Domain tests, 100 Application tests, six architecture tests, four Godot smokes, and the bounded
graphical launch. The sandboxed focused test run reached the known VSTest loopback denial; the
identical host run passed. No manual play acceptance is required for this engine-independent Domain
slice. `Domain -> []` remains intact, and hosted desktop outputs remain unsigned development
evidence, not releases.

## Exact next bounded slice

After this pull request is reported merged, take **Phase 2k7: pure focused-search cancellation
transition**.

Acceptance boundary:

1. Add a static `FocusedSearchCancellationTransition` in `Allrpcs.Domain.Discovery` with one pure
   `CancelSearch(FocusedSearchRequest, FocusedSearchActivitySnapshot)` operation returning a
   `FocusedSearchActivitySnapshot`.
2. Delegate eligibility to `FocusedSearchCancellationPolicy`. For an eligible request, return a
   new snapshot containing every nonmatching existing activity in stable order and retaining the
   exact existing activity instances. Reject an ineligible request with `InvalidOperationException`.
3. Reject null inputs through the policy and never mutate the request, existing activity
   instances, or input snapshot.
4. Add focused tests for rejecting empty and near-match snapshots, removing a value-equal complete
   request from each representative position, stable order and instance retention, null rejection,
   non-mutation on both success and rejection, policy delegation behavior, and the deliberately
   limited static public surface.
5. Preserve `Domain -> []`; document the pure cancellation transition in `README.md`, leave the
   broader bounded-focused-search roadmap item incomplete, and replace this file with current
   evidence, limits, and one next bounded slice.
6. Use test-first development, run `./scripts/quality.sh`, review every intended path, and commit
   only the reviewed slice.

Explicit exclusions:

- Do not add a cancellation command, event, effect, reason, permission, presentation behavior, or
  in-place collection mutation.
- Do not treat cancellation as completion or add progress, completion, replacement, scheduling,
  duration, timing, status values, outcomes, or any other lifecycle transition.
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
