# Allrpcs Handover

## Current state

Phase 2j3 is implemented, reviewed, and locally validated on
`codex/phase-2j3-discovery-capability-policy`, based directly on the PR #42 merge at `07d2878`.
A draft pull request will be opened after this reviewed slice is committed and pushed.
Pull-request merge remains separately authorized.

`DiscoveryCapabilityPolicy.IsSatisfied` is one pure, fail-closed evaluation of an existing
`DiscoveryCapabilityRequirement` against an existing `CharacterCapabilitySet`. It returns true
exactly when a capability's canonical key equals the requirement key by ordinal value. It ignores
authored descriptions, order, and unrelated capabilities and does not mutate either input. It
does not evaluate tools, combine requirements, grant discovery, or change visibility.

The test-first build failed because the policy type did not exist. All eight focused tests then
passed on the host. The restricted full-quality invocation reached restore/build but could not
provide completion evidence for the full VSTest-capable workflow. The identical host-shell
`./scripts/quality.sh` passed cleanly with a zero-warning build, 363 Domain tests, 100 Application
tests, six architecture tests, four Godot smokes, and the bounded graphical launch. No manual play
acceptance is required for this engine-independent Domain slice. `Domain -> []` remains intact,
and hosted desktop outputs remain unsigned development evidence, not releases.

## Exact next bounded slice

After this pull request is reported merged, take **Phase 2j4: pure tool-requirement evaluation**.

Acceptance boundary:

1. Add a static `DiscoveryToolPolicy` in `Allrpcs.Domain.Discovery` with one pure `IsSatisfied`
   operation accepting an existing `DiscoveryToolRequirement` and `CharacterInventory`.
2. Return true exactly when the inventory contains an object identifier equal by value to the
   requirement's required-tool identifier; otherwise fail closed. Ignore the target object,
   authored order, and unrelated inventory entries, and do not mutate either input.
3. Add focused pure tests for matching, empty and nonmatching inventories, unrelated entries,
   value-based identifier matching, null rejection, input retention, and the deliberately limited
   public surface.
4. Preserve `Domain -> []`; document the boundary in `README.md`, leave the broader capability-
   and tool-dependent discovery roadmap item incomplete, and replace this file with current
   evidence, limits, and one next bounded slice.
5. Use test-first development, run `./scripts/quality.sh`, review every intended path, and commit
   only the reviewed slice.

Explicit exclusions:

- Do not combine capability and tool requirements, evaluate multiple requirements, grant
  discovery, change visibility, or inspect a complete character, ownership/capacity snapshots,
  perception, world state, or object existence.
- Do not add requirement collections, tool classifications or properties, searches, passive
  noticing, reinterpretation, action requests, results, errors, events, or effects.
- Do not add scenario content, mutable state, serialization, persistence, Godot, UI, providers, or
  plugins.
- Keep point-and-click movement, disease-time pausing, and the world-time debug overlay deferred
  until their own retained prototype or authoritative-system prerequisites exist.
- Do not merge, release, distribute, sign, notarize, change repository visibility, or add
  credentials without explicit user authorization.
