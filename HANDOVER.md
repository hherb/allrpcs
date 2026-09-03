# Allrpcs Handover

## Current state

Phase 2k1 is implemented, reviewed, and locally validated on
`codex/phase-2k1-focused-search-request`, based directly on the PR #48 merge at `f68e0be`.
Feature commit `326ae9d` is pushed, and draft PR #49
(`https://github.com/hherb/allrpcs/pull/49`) is open. Pull-request merge remains separately
unauthorized.

`FocusedSearchRequest` retains one existing searcher `CharacterId` and one existing target
`ObjectId` through get-only properties. It preserves the exact supplied identifier instances,
compares by identifier values, rejects either null input, and adds no search behavior. The
broader bounded-focused-search roadmap item remains incomplete.

The test-first build failed only because `FocusedSearchRequest` did not exist. All four focused
tests then passed on the host. Host-shell `./scripts/quality.sh` passed cleanly with 403 Domain
tests, 100 Application tests, six architecture tests, four Godot smokes, and the bounded
graphical launch. No manual play acceptance is required for this engine-independent Domain
slice. `Domain -> []` remains intact, and hosted desktop outputs remain unsigned development
evidence, not releases.

## Exact next bounded slice

After this pull request is reported merged, take **Phase 2k2: immutable focused-search activity**.

Acceptance boundary:

1. Add a sealed immutable `FocusedSearchActivity` record in `Allrpcs.Domain.Discovery` that
   accepts one existing `FocusedSearchRequest`.
2. Retain the exact supplied request instance and expose it through a get-only `Request`
   property. Preserve value equality across separately constructed value-equal requests.
3. Reject a null request. Let the type denote only that this focused search is active; add no
   lifecycle transition or policy.
4. Add focused pure tests for exact instance retention, value equality, null rejection, and the
   deliberately limited immutable public surface.
5. Preserve `Domain -> []`; document the activity boundary in `README.md`, leave the broader
   bounded-focused-search roadmap item incomplete, and replace this file with current evidence,
   limits, and one next bounded slice.
6. Use test-first development, run `./scripts/quality.sh`, review every intended path, and commit
   only the reviewed slice.

Explicit exclusions:

- Do not start, progress, complete, cancel, or schedule a search; define range, radius, duration,
  location, coordinates, movement, animation, progress, timing, status values, outcomes, errors,
  events, or effects; or add request or activity collections.
- Do not evaluate discovery conditions, grant discovery, change visibility, reveal the target to
  a character or UI, or inspect a complete character, perception snapshot, ownership/capacity
  snapshot, world state, or object existence.
- Do not add multiple-active-search policy, passive noticing, reinterpretation, configurable
  Boolean expressions, new discovery requirement types, scenario content, mutable state,
  serialization, persistence, Godot, UI, providers, or plugins.
- Keep point-and-click movement, disease-time pausing, and the world-time debug overlay deferred
  until their own retained prototype or authoritative-system prerequisites exist.
- Do not merge, release, distribute, sign, notarize, change repository visibility, or add
  credentials without explicit user authorization.
