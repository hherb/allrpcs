# Allrpcs Handover

## Current state

Phase 2k3 is implemented, reviewed without findings, and locally validated on
`codex/phase-2k3-focused-search-activity-snapshot`, based directly on the PR #50 merge at
`6ae2a2e`. Feature commit `dfcf11d` is pushed, and draft PR #51
(`https://github.com/hherb/allrpcs/pull/51`) is open. Pull-request merge remains separately
unauthorized.

`FocusedSearchActivitySnapshot` accepts existing `FocusedSearchActivity` values, explicitly
permits an empty sequence, copies caller input, preserves stable supplied order, retains exact
activity instances, and exposes them only through a get-only
`IReadOnlyList<FocusedSearchActivity>` named `Entries`. It rejects a null sequence, null entries,
and value-equal duplicate requests. It permits one character to have activities for distinct
targets; the snapshot records state but does not decide whether that state may be started.

The test-first build failed only because `FocusedSearchActivitySnapshot` did not exist. All seven
focused tests then passed on the host. Host-shell `./scripts/quality.sh` passed cleanly with 414
Domain tests, 100 Application tests, six architecture tests, four Godot smokes, and the bounded
graphical launch. The sandbox run reached the known VSTest loopback denial; the identical host
run passed. No manual play acceptance is required for this engine-independent Domain slice.
`Domain -> []` remains intact, and hosted desktop outputs remain unsigned development evidence,
not releases.

## Exact next bounded slice

After this pull request is reported merged, take **Phase 2k4: pure focused-search start
eligibility**.

Acceptance boundary:

1. Add a static `FocusedSearchStartPolicy` in `Allrpcs.Domain.Discovery` with one pure
   `CanStartSearch(FocusedSearchRequest, FocusedSearchActivitySnapshot)` operation.
2. Permit the request exactly when no existing activity has a value-equal
   `SearcherCharacterId`. This enforces at most one active focused search per character while
   permitting different characters to search the same target object.
3. Reject null inputs, compare character identifiers by value, ignore unrelated activities, and
   never mutate the request, activity instances, or snapshot.
4. Add focused pure tests for eligible empty/unrelated snapshots, same-searcher rejection across
   the same and different targets, value equality, different-searcher/same-target permission,
   null rejection, non-mutation, and the deliberately limited static public surface.
5. Preserve `Domain -> []`; document the eligibility boundary in `README.md`, leave the broader
   bounded-focused-search roadmap item incomplete, and replace this file with current evidence,
   limits, and one next bounded slice.
6. Use test-first development, run `./scripts/quality.sh`, review every intended path, and commit
   only the reviewed slice.

Explicit exclusions:

- Do not add a start command or result, create or append an activity, or start, progress,
  complete, cancel, replace, or schedule a search.
- Do not define search capacity beyond one active search per character; do not define target
  exclusivity, range, radius, duration, location, coordinates, movement, animation, progress,
  timing, status values, outcomes, errors, events, effects, completion, lookup, or scheduling
  policy.
- Do not evaluate discovery conditions, grant discovery, change visibility, reveal a target to a
  character or UI, or inspect a complete character, perception snapshot, ownership/capacity
  snapshot, world state, or object existence.
- Do not add passive noticing, reinterpretation, configurable Boolean expressions, new discovery
  requirement types, scenario content, mutable state, serialization, persistence, Godot, UI,
  providers, or plugins.
- Keep point-and-click movement, disease-time pausing, and the world-time debug overlay deferred
  until their own retained prototype or authoritative-system prerequisites exist.
- Do not merge, release, distribute, sign, notarize, change repository visibility, or add
  credentials without explicit user authorization.
