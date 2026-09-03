# Allrpcs Handover

## Current state

Phase 2k8 is implemented, reviewed without findings, and locally validated on
`codex/phase-2k8-focused-search-completion-policy`, based directly on the PR #55 merge at
`38e96e1`. Pull-request merge remains separately unauthorized.

`FocusedSearchCompletionPolicy.CanCompleteSearch` returns true exactly when the activity snapshot
contains an activity whose complete request is value-equal to the supplied request. It is separate
from cancellation, rejects null inputs, and never mutates the request, existing activity instances,
or snapshot.

The test-first build failed only because `FocusedSearchCompletionPolicy` did not exist. All nine
focused tests then passed on the host. Host-shell `./scripts/quality.sh` passed cleanly with 461
Domain tests, 100 Application tests, six architecture tests, four Godot smokes, and the bounded
graphical launch. No manual play acceptance is required for this engine-independent Domain slice.
`Domain -> []` remains intact, and hosted desktop outputs remain unsigned development evidence,
not releases.

## Exact next bounded slice

After this pull request is reported merged, take **Phase 2k9: pure focused-search completion
transition**.

Acceptance boundary:

1. Add a static `FocusedSearchCompletionTransition` in `Allrpcs.Domain.Discovery` with one pure
   `CompleteSearch(FocusedSearchRequest, FocusedSearchActivitySnapshot)` operation returning a new
   `FocusedSearchActivitySnapshot`.
2. Delegate eligibility to `FocusedSearchCompletionPolicy`; reject an ineligible request and
   otherwise remove the activity for the complete value-equal request.
3. Retain every nonmatching existing activity instance in stable order, reject null inputs through
   the policy, and never mutate the request, existing activities, or input snapshot.
4. Add focused tests for exact-instance and value-equal completion; stable retention; empty,
   near-match, and unrelated rejection; null rejection; non-mutation; and the deliberately limited
   static public surface.
5. Preserve `Domain -> []`; document the pure completion transition in `README.md`, leave the
   broader bounded-focused-search roadmap item incomplete, and replace this file with current
   evidence, limits, and one next bounded slice.
6. Use test-first development, run `./scripts/quality.sh`, review every intended path, and commit
   only the reviewed slice.

Explicit exclusions:

- Do not add a completion result, outcome, event, effect, reason, discovery grant, visibility
  change, presentation behavior, or in-place collection mutation.
- Do not expose or reuse cancellation as completion, even though the two transitions currently
  share the same complete-request removal shape.
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
