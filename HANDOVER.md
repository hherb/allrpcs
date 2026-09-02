# Allrpcs Handover

## Current state

Phase 2i9 is implemented, reviewed, and locally validated on
`codex/phase-2i9-inventory-use-policy`, based directly on the PR #39 merge at `f0c63e2`.
Draft PR #40 (`https://github.com/hherb/allrpcs/pull/40`) is open. Pull-request merge remains
separately authorized.

`InventoryUsePolicy.CanUseObject` is the single public, stateless decision for whether an existing
`CharacterId` may use an existing `ObjectId`. It rejects null inputs and permits use exactly when
the `InventoryOwnershipSnapshot` contains the value-equal object identifier owned by the
value-equal intended character. It ignores unrelated entries, returns only eligibility, and does
not mutate the snapshot or execute, consume, equip, remove, or alter the object.

The test-first build failed because the policy type did not exist. All eight focused tests then
passed on the host. The restricted full-quality run completed a zero-warning build before reaching
the known VSTest loopback denial. The identical host-shell `./scripts/quality.sh` passed with a
zero-warning build, 339 Domain tests, 100 Application tests, six architecture tests, four Godot
smokes, and the bounded graphical launch. No manual play acceptance is required for this
engine-independent Domain slice. `Domain -> []` remains intact, and hosted desktop outputs remain
unsigned development evidence, not releases.

## Exact next bounded slice

After this pull request is reported merged, take **Phase 2j1: immutable capability discovery
requirement**.

Acceptance boundary:

1. Add an immutable `DiscoveryCapabilityRequirement` in `Allrpcs.Domain.Discovery` that associates
   one existing target `ObjectId` with one required canonical character-capability key.
2. Require and retain the supplied object identifier, validate the capability key with the shared
   canonical-identifier text contract, retain the key exactly, and expose only get-only values.
3. Add focused pure tests for valid construction, object-instance retention, value equality,
   canonical-key boundaries and rejection, null rejection, and the deliberately limited public
   surface.
4. Preserve `Domain -> []`; document the boundary in `README.md`, leave the broader capability-
   and tool-dependent discovery roadmap item incomplete, and replace this file with current
   evidence, limits, and one next bounded slice.
5. Use test-first development, run `./scripts/quality.sh`, review every intended path, and commit
   only the reviewed slice.

Explicit exclusions:

- Do not evaluate the requirement against a character, capability set, inventory, perception, or
  world state, and do not grant discovery or change visibility.
- Do not add requirement collections, multi-requirement logic, tool requirements, searches,
  passive noticing, reinterpretation, action requests, results, errors, events, or effects.
- Do not add scenario content, mutable state, serialization, persistence, Godot, UI, providers, or
  plugins.
- Keep point-and-click movement, disease-time pausing, and the world-time debug overlay deferred
  until their own retained prototype or authoritative-system prerequisites exist.
- Do not merge, release, distribute, sign, notarize, change repository visibility, or add
  credentials without explicit user authorization.
