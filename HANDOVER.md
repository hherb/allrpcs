# Allrpcs Handover

## Current state

Phase 2j6 is implemented, reviewed, and locally validated on
`codex/phase-2j6-discovery-tool-requirement-set`, based directly on the PR #45 merge at
`a8a66ea`. Draft PR #46 (`https://github.com/hherb/allrpcs/pull/46`) is open. Pull-request merge
remains separately authorized.

`DiscoveryToolRequirementSet` accepts existing `DiscoveryToolRequirement` instances, permits an
empty sequence, copies caller input, preserves authored order, retains the exact instances, and
exposes them through `IReadOnlyList<DiscoveryToolRequirement>`. It rejects a null sequence, null
entries, and value-equal duplicate target-object identifiers, so the set has at most one tool
requirement per target object.

The test-first build failed because the set type did not exist. All seven focused tests then
passed on the host. The restricted full-quality invocation completed a zero-warning build but
could not run VSTest because its loopback socket was denied. The identical host-shell
`./scripts/quality.sh` passed cleanly with 385 Domain tests, 100 Application tests, six architecture
tests, four Godot smokes, and the bounded graphical launch. No manual play acceptance is required
for this engine-independent Domain slice. `Domain -> []` remains intact, and hosted desktop
outputs remain unsigned development evidence, not releases.

## Exact next bounded slice

After this pull request is reported merged, take **Phase 2j7: immutable combined discovery-
condition aggregate**.

Acceptance boundary:

1. Add a sealed `DiscoveryConditionSet` in `Allrpcs.Domain.Discovery` that accepts the existing
   `DiscoveryCapabilityRequirementSet` and `DiscoveryToolRequirementSet` as its two authored
   components.
2. Retain the exact supplied set instances and expose them through get-only
   `CapabilityRequirements` and `ToolRequirements` properties.
3. Reject a null capability-requirement set or tool-requirement set. Preserve the constituent
   sets' existing empty-input behavior.
4. Permit a target-object identifier to occur once in each constituent set; this only composes
   authored requirements and does not define whether capability and tool requirements use AND,
   OR, or any other evaluation semantics.
5. Add focused pure tests for instance retention, null rejection, empty constituent sets,
   cross-set target overlap, and the deliberately limited public surface.
6. Preserve `Domain -> []`; document the boundary in `README.md`, leave the broader capability-
   and tool-dependent discovery roadmap item incomplete, and replace this file with current
   evidence, limits, and one next bounded slice.
7. Use test-first development, run `./scripts/quality.sh`, review every intended path, and commit
   only the reviewed slice.

Explicit exclusions:

- Do not evaluate either requirement set, define Boolean combination semantics, grant discovery,
  change visibility, or inspect a complete character, ownership/capacity snapshots, perception,
  world state, or object existence.
- Do not add further requirement types, tool classifications or properties, searches, passive
  noticing, reinterpretation, action requests, results, errors, events, or effects.
- Do not add scenario content, mutable state, serialization, persistence, Godot, UI, providers, or
  plugins.
- Keep point-and-click movement, disease-time pausing, and the world-time debug overlay deferred
  until their own retained prototype or authoritative-system prerequisites exist.
- Do not merge, release, distribute, sign, notarize, change repository visibility, or add
  credentials without explicit user authorization.
