# Allrpcs Handover

## Current state

Phase 2j4 is implemented, reviewed, and locally validated on
`codex/phase-2j4-discovery-tool-policy`, based directly on the PR #43 merge at `d98b651`.
A draft pull request will be opened after this reviewed slice is committed and pushed.
Pull-request merge remains separately authorized.

`DiscoveryToolPolicy.IsSatisfied` is one pure, fail-closed evaluation of an existing
`DiscoveryToolRequirement` against an existing `CharacterInventory`. It returns true exactly
when an inventory object identifier equals the required-tool identifier by value. It ignores the
target object, authored order, and unrelated entries and does not mutate either input. It does not
combine requirements, grant discovery, or change visibility.

The test-first build failed because the policy type did not exist. All eight focused tests then
passed on the host. The restricted full-quality invocation completed a zero-warning build but
could not run VSTest because its loopback socket was denied. The identical host-shell
`./scripts/quality.sh` passed cleanly with 371 Domain tests, 100 Application tests, six architecture
tests, four Godot smokes, and the bounded graphical launch. No manual play acceptance is required
for this engine-independent Domain slice. `Domain -> []` remains intact, and hosted desktop
outputs remain unsigned development evidence, not releases.

## Exact next bounded slice

After this pull request is reported merged, take **Phase 2j5: immutable capability-requirement
set**.

Acceptance boundary:

1. Add a sealed `DiscoveryCapabilityRequirementSet` in `Allrpcs.Domain.Discovery` that accepts a
   sequence of existing `DiscoveryCapabilityRequirement` instances.
2. Permit an empty sequence, copy caller input, preserve authored order, expose entries through
   `IReadOnlyList<DiscoveryCapabilityRequirement>`, and retain the exact supplied instances.
3. Reject a null sequence, null entries, and duplicate target-object identifiers by value so one
   capability requirement is defined per target object within this set.
4. Add focused pure tests for empty and populated sets, ordering, caller-input isolation, instance
   retention, null rejection, duplicate-target rejection, value-based duplicate detection, and
   the deliberately limited public surface.
5. Preserve `Domain -> []`; document the boundary in `README.md`, leave the broader capability-
   and tool-dependent discovery roadmap item incomplete, and replace this file with current
   evidence, limits, and one next bounded slice.
6. Use test-first development, run `./scripts/quality.sh`, review every intended path, and commit
   only the reviewed slice.

Explicit exclusions:

- Do not add the tool-requirement set, combine or evaluate multiple requirements, grant discovery,
  change visibility, or inspect a complete character, ownership/capacity snapshots, perception,
  world state, or object existence.
- Do not add tool classifications or properties, searches, passive noticing, reinterpretation,
  action requests, results, errors, events, or effects.
- Do not add scenario content, mutable state, serialization, persistence, Godot, UI, providers, or
  plugins.
- Keep point-and-click movement, disease-time pausing, and the world-time debug overlay deferred
  until their own retained prototype or authoritative-system prerequisites exist.
- Do not merge, release, distribute, sign, notarize, change repository visibility, or add
  credentials without explicit user authorization.
