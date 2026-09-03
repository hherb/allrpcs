# Allrpcs Handover

## Current state

Phase 2k10 is implemented, reviewed without findings, and locally validated on
`codex/phase-2k10-focused-search-activity-projection`, based directly on the PR #57 merge at
`3c9b6d9`. Feature commit `1c002c2` is pushed, and draft PR #58
(`https://github.com/hherb/allrpcs/pull/58`) is open. Pull-request merge remains separately
unauthorized.

`FocusedSearchActivityProjection.ForCharacter` returns a new snapshot containing every activity
whose request has a value-equal searcher character identifier. It permits no matches and multiple
distinct matching requests, retains the matching activity instances in stable order, ignores
unrelated activities, rejects null inputs, and never mutates the identifier, activities, or input
snapshot.

The test-first build failed only because `FocusedSearchActivityProjection` did not exist. All seven
focused tests then passed on the host. The sandboxed runner built successfully but could not start
VSTest because local loopback binding was denied. Host-shell `./scripts/quality.sh` passed cleanly
with 480 Domain tests, 100 Application tests, six architecture tests, four Godot smokes, and the
bounded graphical launch. No manual play acceptance is required for this engine-independent Domain
slice. `Domain -> []` remains intact, and hosted desktop outputs remain unsigned development
evidence, not releases.

## Exact next bounded slice

After this pull request is reported merged, take **Phase 2k11: pure focused-search activity
indicator policy**.

Acceptance boundary:

1. Add a static `FocusedSearchActivityIndicatorPolicy` in `Allrpcs.Application.Discovery` with one
   pure `ShouldShow(CharacterId, FocusedSearchActivitySnapshot)` operation returning `bool`.
2. Delegate per-character selection to `FocusedSearchActivityProjection`; return true exactly when
   its new snapshot has at least one entry, including when it contains multiple distinct requests.
3. Return false for empty and unrelated snapshots, reject null inputs, and never mutate the
   identifier, existing activities, or input snapshot.
4. Add focused tests for empty, unrelated, one, and multiple matches; value equality; null
   rejection; non-mutation; delegation behavior; and the deliberately limited static surface.
5. Preserve `Application -> Domain` and `Domain -> []`; document the pure policy in `README.md`,
   leave the broader bounded-focused-search roadmap item incomplete, and replace this file with
   current evidence, limits, and one next bounded slice.
6. Use test-first development, run `./scripts/quality.sh`, review every intended path, and commit
   only the reviewed slice.

Explicit exclusions:

- Do not add display text, status values, Godot nodes, UI state, input controls, animation,
  progress, duration, timing, outcomes, events, effects, or reasons.
- Do not change Domain snapshot construction or any focused-search policy or transition; do not
  collapse or reject multiple matching requests in the indicator policy.
- Do not inspect a complete character, perception snapshot, ownership/capacity snapshot, world
  state, object existence, location, coordinates, range, radius, Godot scene, or UI state.
- Do not add search-condition evaluation, discovery grants, visibility changes, passive noticing,
  reinterpretation, configurable Boolean expressions, scenario content, serialization,
  persistence, providers, or plugins.
- Keep point-and-click movement, disease-time pausing, and the world-time debug overlay deferred
  until their own retained prototype or authoritative-system prerequisites exist.
- Do not merge, release, distribute, sign, notarize, change repository visibility, or add
  credentials without explicit user authorization.
