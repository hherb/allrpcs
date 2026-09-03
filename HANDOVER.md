# Allrpcs Handover

## Current state

Phase 2k2 is implemented, reviewed without findings, and locally validated on
`codex/phase-2k2-focused-search-activity`, based directly on the PR #49 merge at `66ab871`.
Feature commit `4b5e195` is pushed, and draft PR #50
(`https://github.com/hherb/allrpcs/pull/50`) is open. Pull-request merge remains separately
unauthorized.

`FocusedSearchActivity` retains one existing `FocusedSearchRequest` through a get-only `Request`
property. It preserves the exact supplied request instance, compares by request value, rejects a
null request, and denotes only that the focused search is active. The broader bounded-focused-
search roadmap item remains incomplete.

The test-first build failed only because `FocusedSearchActivity` did not exist. All four focused
tests then passed on the host. Host-shell `./scripts/quality.sh` passed cleanly with 407 Domain
tests, 100 Application tests, six architecture tests, four Godot smokes, and the bounded graphical
launch. No manual play acceptance is required for this engine-independent Domain slice.
`Domain -> []` remains intact, and hosted desktop outputs remain unsigned development evidence,
not releases.

## Exact next bounded slice

After this pull request is reported merged, take **Phase 2k3: immutable focused-search activity
snapshot**.

Acceptance boundary:

1. Add a sealed immutable `FocusedSearchActivitySnapshot` class in
   `Allrpcs.Domain.Discovery` that accepts existing `FocusedSearchActivity` values.
2. Explicitly permit an empty sequence, copy caller input, preserve stable supplied order, retain
   the exact supplied activity instances, and expose them only through a get-only
   `IReadOnlyList<FocusedSearchActivity>` named `Entries`.
3. Reject a null input sequence, null entries, and value-equal duplicate requests. Permit one
   character to have activities for distinct target objects; do not decide multiple-active-search
   policy in this slice.
4. Add focused pure tests for empty input, ordering and exact-instance retention, caller-copy
   isolation, duplicate rejection, null rejection, and the deliberately limited read-only public
   surface.
5. Preserve `Domain -> []`; document the snapshot boundary in `README.md`, leave the broader
   bounded-focused-search roadmap item incomplete, and replace this file with current evidence,
   limits, and one next bounded slice.
6. Use test-first development, run `./scripts/quality.sh`, review every intended path, and commit
   only the reviewed slice.

Explicit exclusions:

- Do not start, progress, complete, cancel, or schedule a search; define range, radius, duration,
  location, coordinates, movement, animation, progress, timing, status values, outcomes, errors,
  events, or effects; or add a focused-search request collection.
- Do not enforce at most one active search per character or add start, replacement, cancellation,
  completion, capacity, lookup, or scheduling policy.
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
