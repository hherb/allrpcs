# Allrpcs Handover

## Current state

Phase 2j1 is implemented, reviewed, and locally validated on
`codex/phase-2j1-discovery-capability-requirement`, based directly on the PR #40 merge at
`dbca6ba`. A draft pull request has not yet been opened. Pull-request merge remains separately
authorized.

`DiscoveryCapabilityRequirement` is an immutable value associating one existing target
`ObjectId` with one required canonical character-capability key. It retains the supplied object
identifier instance, validates the key through the shared canonical-identifier contract,
preserves the key exactly, compares by value, and exposes only two get-only properties. It does
not evaluate discovery eligibility, grant discovery, or change visibility.

The test-first build failed because the discovery type did not exist. All 11 focused tests then
passed on the host. The restricted full-quality run completed a zero-warning build before reaching
the known VSTest loopback denial. The identical host-shell `./scripts/quality.sh` passed with a
zero-warning build, 350 Domain tests, 100 Application tests, six architecture tests, four Godot
smokes, and the bounded graphical launch. No manual play acceptance is required for this
engine-independent Domain slice. `Domain -> []` remains intact, and hosted desktop outputs remain
unsigned development evidence, not releases.

## Exact next bounded slice

After this pull request is reported merged, take **Phase 2j2: immutable tool discovery
requirement**.

Acceptance boundary:

1. Add an immutable `DiscoveryToolRequirement` in `Allrpcs.Domain.Discovery` that associates one
   existing target `ObjectId` with one existing required-tool `ObjectId`.
2. Require and retain both supplied identifier instances, compare requirements by their values,
   and expose only get-only values without adding cross-field policy.
3. Add focused pure tests for valid construction, identifier-instance retention, value equality,
   null rejection, and the deliberately limited public surface.
4. Preserve `Domain -> []`; document the boundary in `README.md`, leave the broader capability-
   and tool-dependent discovery roadmap item incomplete, and replace this file with current
   evidence, limits, and one next bounded slice.
5. Use test-first development, run `./scripts/quality.sh`, review every intended path, and commit
   only the reviewed slice.

Explicit exclusions:

- Do not evaluate either requirement against a character, capability set, inventory, perception,
  or world state, and do not grant discovery or change visibility.
- Do not add same-object validity policy, tool classifications or properties, requirement
  collections, multi-requirement logic, searches, passive noticing, reinterpretation, action
  requests, results, errors, events, or effects.
- Do not add scenario content, mutable state, serialization, persistence, Godot, UI, providers, or
  plugins.
- Keep point-and-click movement, disease-time pausing, and the world-time debug overlay deferred
  until their own retained prototype or authoritative-system prerequisites exist.
- Do not merge, release, distribute, sign, notarize, change repository visibility, or add
  credentials without explicit user authorization.
