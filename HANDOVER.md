# Allrpcs Handover

## Current state

Phase 2k4 is implemented, reviewed without findings, and locally validated on
`codex/phase-2k4-focused-search-start-policy`, based directly on the PR #51 merge at `0b51647`.
Feature commit `27662f9` is pushed, and draft PR #52
(`https://github.com/hherb/allrpcs/pull/52`) is open. Pull-request merge remains separately
unauthorized.

`FocusedSearchStartPolicy.CanStartSearch` accepts an existing `FocusedSearchRequest` and
`FocusedSearchActivitySnapshot`. It permits the request exactly when no existing activity has a
value-equal `SearcherCharacterId`, so one character cannot start a second active search while
different characters may search the same target. It rejects null inputs, ignores unrelated
activities, and does not mutate or start anything.

The test-first build failed only because `FocusedSearchStartPolicy` did not exist. All nine focused
tests then passed on the host. Host-shell `./scripts/quality.sh` passed cleanly with 423 Domain
tests, 100 Application tests, six architecture tests, four Godot smokes, and the bounded graphical
launch. The sandboxed focused test run reached the known VSTest loopback denial; the identical host
run passed. No manual play acceptance is required for this engine-independent Domain slice.
`Domain -> []` remains intact, and hosted desktop outputs remain unsigned development evidence,
not releases.

## Exact next bounded slice

After this pull request is reported merged, take **Phase 2k5: pure focused-search start
transition**.

Acceptance boundary:

1. Add a static `FocusedSearchStartTransition` in `Allrpcs.Domain.Discovery` with one pure
   `StartSearch(FocusedSearchRequest, FocusedSearchActivitySnapshot)` operation returning a new
   `FocusedSearchActivitySnapshot`.
2. Delegate eligibility to `FocusedSearchStartPolicy`. When eligible, retain existing activity
   instances in stable order and append exactly one new `FocusedSearchActivity` retaining the exact
   supplied request instance. Reject an ineligible request with `InvalidOperationException`.
3. Reject null inputs and never mutate the request, existing activity instances, or input snapshot.
4. Add focused pure tests for empty and populated eligible snapshots, stable append order, exact
   instance retention, same-searcher rejection across the same and different targets, value
   equality, different-searcher/same-target permission, null rejection, non-mutation, and the
   deliberately limited static public surface.
5. Preserve `Domain -> []`; document the pure transition in `README.md`, leave the broader
   bounded-focused-search roadmap item incomplete, and replace this file with current evidence,
   limits, and one next bounded slice.
6. Use test-first development, run `./scripts/quality.sh`, review every intended path, and commit
   only the reviewed slice.

Explicit exclusions:

- Do not add commands, mutable collections, in-place updates, progress, completion, cancellation,
  replacement, scheduling, duration, timing, status values, outcomes, errors beyond the named
  ineligible-start rejection, events, or effects.
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
