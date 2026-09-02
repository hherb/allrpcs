# Allrpcs Handover

## Current state

Phase 2j8 is implemented, reviewed, and locally validated on
`codex/phase-2j8-discovery-condition-policy`, based directly on the PR #47 merge at `3d59533`.
Feature commit `dfa316e` is pushed, and draft PR #48
(`https://github.com/hherb/allrpcs/pull/48`) is open. Pull-request merge remains separately
authorized.

`DiscoveryConditionPolicy.IsSatisfied` evaluates one existing target `ObjectId` against one
existing `DiscoveryConditionSet`, `CharacterCapabilitySet`, and `CharacterInventory`. It finds
requirements by value-equal target identifier, ignores unrelated targets, and fails closed when
neither kind is authored. A sole capability or tool requirement delegates to its existing policy;
when both are authored for the target, both policies must pass. It rejects every null input and
does not mutate any input. The capability- and tool-dependent discovery-condition roadmap item is
now complete, but the policy does not grant discovery or change world state.

The test-first build failed only because the policy did not exist. All nine focused tests then
passed on the host. The restricted focused invocation completed the build but could not run
VSTest because its loopback socket was denied. The identical host-shell focused invocation passed,
and host-shell `./scripts/quality.sh` passed cleanly with 399 Domain tests, 100 Application tests,
six architecture tests, four Godot smokes, and the bounded graphical launch. No manual play
acceptance is required for this engine-independent Domain slice. `Domain -> []` remains intact,
and hosted desktop outputs remain unsigned development evidence, not releases.

## Exact next bounded slice

After this pull request is reported merged, take **Phase 2k1: immutable focused-search request**.

Acceptance boundary:

1. Add a sealed immutable `FocusedSearchRequest` record in `Allrpcs.Domain.Discovery` that accepts
   one existing searcher `CharacterId` and one existing target `ObjectId`.
2. Retain the exact supplied identifier instances and expose them through get-only
   `SearcherCharacterId` and `TargetObjectId` properties.
3. Reject a null searcher identifier or target identifier. Preserve value equality across
   separately constructed equal identifiers.
4. Add focused pure tests for exact instance retention, value equality, null rejection, and the
   deliberately limited immutable public surface.
5. Preserve `Domain -> []`; document the request boundary in `README.md`, leave the broader
   bounded-focused-search roadmap item incomplete, and replace this file with current evidence,
   limits, and one next bounded slice.
6. Use test-first development, run `./scripts/quality.sh`, review every intended path, and commit
   only the reviewed slice.

Explicit exclusions:

- Do not evaluate discovery conditions, grant discovery, change visibility, reveal the target to
  a character or UI, or inspect a complete character, perception snapshot, ownership/capacity
  snapshot, world state, or object existence.
- Do not start, progress, complete, cancel, or schedule a search; define range, radius, duration,
  location, coordinates, movement, animation, visible activity, status, outcomes, errors, events,
  or effects; or add request collections.
- Do not add passive noticing, reinterpretation, configurable Boolean expressions, new discovery
  requirement types, scenario content, mutable state, serialization, persistence, Godot, UI,
  providers, or plugins.
- Keep point-and-click movement, disease-time pausing, and the world-time debug overlay deferred
  until their own retained prototype or authoritative-system prerequisites exist.
- Do not merge, release, distribute, sign, notarize, change repository visibility, or add
  credentials without explicit user authorization.
