# Allrpcs Handover

## Current state

Phase 2j2 is implemented, reviewed, and locally validated on
`codex/phase-2j2-discovery-tool-requirement`, based directly on the PR #41 merge at `ac0e031`.
Draft pull-request publication is pending. Pull-request merge remains separately authorized.

`DiscoveryToolRequirement` is an immutable value associating one existing target `ObjectId` with
one existing required-tool `ObjectId`. It requires and retains both supplied identifier instances,
compares by value, exposes only two get-only properties, and deliberately adds no same-object
validity policy. It does not evaluate eligibility, grant discovery, or change visibility.

The test-first build failed because the tool-requirement type did not exist. All five focused
tests then passed on the host. The restricted full-quality run completed a zero-warning build
before reaching the known VSTest loopback denial. The identical host-shell
`./scripts/quality.sh` passed with a zero-warning build, 355 Domain tests, 100 Application tests,
six architecture tests, four Godot smokes, and the bounded graphical launch. No manual play
acceptance is required for this engine-independent Domain slice. `Domain -> []` remains intact,
and hosted desktop outputs remain unsigned development evidence, not releases.

## Exact next bounded slice

After this pull request is reported merged, take **Phase 2j3: pure capability-requirement
evaluation**.

Acceptance boundary:

1. Add a static `DiscoveryCapabilityPolicy` in `Allrpcs.Domain.Discovery` with one pure
   `IsSatisfied` operation accepting an existing `DiscoveryCapabilityRequirement` and
   `CharacterCapabilitySet`.
2. Return true exactly when the supplied capability set contains a capability whose canonical
   key equals the requirement key; otherwise fail closed. Match by ordinal value, ignore authored
   descriptions, order, and unrelated capabilities, and do not mutate either input.
3. Add focused pure tests for matching, empty and nonmatching sets, unrelated entries, value-based
   key matching, null rejection, input retention, and the deliberately limited public surface.
4. Preserve `Domain -> []`; document the boundary in `README.md`, leave the broader capability-
   and tool-dependent discovery roadmap item incomplete, and replace this file with current
   evidence, limits, and one next bounded slice.
5. Use test-first development, run `./scripts/quality.sh`, review every intended path, and commit
   only the reviewed slice.

Explicit exclusions:

- Do not evaluate tool requirements, combine multiple requirements, grant discovery, change
  visibility, or inspect character identity, inventory, perception, world state, or object
  existence.
- Do not add requirement collections, tool classifications or properties, searches, passive
  noticing, reinterpretation, action requests, results, errors, events, or effects.
- Do not add scenario content, mutable state, serialization, persistence, Godot, UI, providers, or
  plugins.
- Keep point-and-click movement, disease-time pausing, and the world-time debug overlay deferred
  until their own retained prototype or authoritative-system prerequisites exist.
- Do not merge, release, distribute, sign, notarize, change repository visibility, or add
  credentials without explicit user authorization.
