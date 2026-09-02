# Allrpcs Handover

## Current state

Phase 2j7 is implemented, reviewed, and locally validated on
`codex/phase-2j7-discovery-condition-set`, based directly on the PR #46 merge at `b3ba511`.
Feature commit `1ae36cb` is pushed, and draft PR #47
(`https://github.com/hherb/allrpcs/pull/47`) is open. Pull-request merge remains separately
authorized.

`DiscoveryConditionSet` accepts the existing `DiscoveryCapabilityRequirementSet` and
`DiscoveryToolRequirementSet`, retains the exact supplied instances, and exposes them through
get-only `CapabilityRequirements` and `ToolRequirements` properties. It rejects either null set,
preserves their empty-set behavior, and permits a value-equal target-object identifier once in
each constituent set. It does not evaluate requirements or define Boolean combination semantics.

The test-first build failed only because the aggregate did not exist. All five focused tests then
passed on the host. The restricted full-quality invocation completed a zero-warning build but
could not run VSTest because its loopback socket was denied. The identical host-shell
`./scripts/quality.sh` passed cleanly with 390 Domain tests, 100 Application tests, six
architecture tests, four Godot smokes, and the bounded graphical launch. No manual play acceptance
is required for this engine-independent Domain slice. `Domain -> []` remains intact, and hosted
desktop outputs remain unsigned development evidence, not releases.

## Exact next bounded slice

After this pull request is reported merged, take **Phase 2j8: pure one-target combined discovery-
condition decision**.

Acceptance boundary:

1. Add a static `DiscoveryConditionPolicy.IsSatisfied` decision in
   `Allrpcs.Domain.Discovery` for one existing target `ObjectId`, one existing
   `DiscoveryConditionSet`, one existing `CharacterCapabilitySet`, and one existing
   `CharacterInventory`.
2. Match capability and tool requirements to the target identifier by value, ignoring unrelated
   target entries. The existing set invariants guarantee at most one match of each kind.
3. Fail closed when neither constituent set contains a requirement for the target.
4. When exactly one matching requirement exists, delegate to its existing requirement policy.
   When both exist, require both policies to be satisfied; this is AND only across the two
   authored requirements for that target.
5. Reject every null input and do not mutate any input.
6. Add focused pure tests for capability-only, tool-only, both-satisfied, either-side failure,
   absent-target failure, unrelated entries, value-equal target identifiers, null rejection, and
   the deliberately limited public surface.
7. Preserve `Domain -> []`; document the boundary in `README.md`, mark the capability- and tool-
   dependent discovery-condition roadmap item complete only if the full acceptance boundary
   passes, and replace this file with current evidence, limits, and one next bounded slice.
8. Use test-first development, run `./scripts/quality.sh`, review every intended path, and commit
   only the reviewed slice.

Explicit exclusions:

- Do not grant discovery, change visibility, inspect a complete character, ownership or capacity
  snapshots, perception, world state, or object existence.
- Do not add configurable Boolean expressions, alternative requirement groups, further
  requirement types, tool classifications or properties, searches, passive noticing,
  reinterpretation, action requests, results, errors, events, or effects.
- Do not add scenario content, mutable state, serialization, persistence, Godot, UI, providers,
  or plugins.
- Keep point-and-click movement, disease-time pausing, and the world-time debug overlay deferred
  until their own retained prototype or authoritative-system prerequisites exist.
- Do not merge, release, distribute, sign, notarize, change repository visibility, or add
  credentials without explicit user authorization.
