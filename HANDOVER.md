# Allrpcs Handover

## Current state

Phase 2i4 is implemented, reviewed, and locally validated on
`codex/phase-2i4-inventory-capacity-snapshot`, based directly on the PR #34 merge at `16e49b6`.
Draft PR #35 (`https://github.com/hherb/allrpcs/pull/35`) is open. Pull-request merge remains
separately authorized.

`InventoryCapacitySnapshot` is a sealed immutable collection of existing `InventoryCapacity`
entries. It permits an empty sequence, copies caller input, preserves stable supplied order and
each entry instance, exposes only a read-only `Entries` collection, and rejects a null sequence,
null entries, and duplicate `CharacterId` values. It adds no implicit capacity, lookup behavior,
or cross-snapshot policy.

The test-first build failed because the type did not exist. All six focused tests then passed on
the host. The restricted full-quality run completed a zero-warning build before reaching the
known VSTest loopback denial. The identical host-shell `./scripts/quality.sh` passed with a
zero-warning build, 286 Domain tests, 100 Application tests, six architecture tests, four Godot
smokes, and the bounded graphical launch. No manual play acceptance is required for this
engine-independent Domain slice. `Domain -> []` remains intact, and hosted desktop outputs remain
unsigned development evidence, not releases.

## Exact next bounded slice

After this pull request is reported merged, take **Phase 2i5: pure one-object inventory-capacity
decision**.

Acceptance boundary:

1. Add a pure `InventoryCapacityPolicy` in `Allrpcs.Domain.Inventory` with one operation that
   determines whether an existing `CharacterId` may own one additional distinct object from an
   `InventoryOwnershipSnapshot` and an `InventoryCapacitySnapshot`.
2. Fail closed when the character has no capacity entry. Otherwise count only ownership entries
   with a value-equal `OwnerCharacterId` and permit the addition exactly when that count is less
   than `MaximumObjectCount`; unrelated owners and capacities must not affect the decision.
3. Add focused pure tests for missing and zero capacity, below/equal/above-capacity counts,
   unrelated-entry isolation, value-based identifier matching, null rejection, input
   immutability, and the deliberately limited public surface.
4. Preserve `Domain -> []`; update `README.md`, leave the broader inventory roadmap item
   incomplete, and replace this file with evidence, limits, and one next bounded slice.
5. Run `./scripts/quality.sh`, review every intended path, and commit only the reviewed slice.

Explicit exclusions:

- Treat the requested object as a new distinct ownership entry; do not accept an `ObjectId`,
  decide whether an existing association is a no-op, or mutate either snapshot.
- Do not add default or unlimited capacity, capacity reservation, per-item size, weight, slots,
  quantities, stacks, containers, equipment, or aggregate integration.
- Do not add give, take, drop, use, crafting, consumption, transfer, action requests, outcome
  validation, mutable state, actions, events, scenario content, serialization, persistence,
  Godot, UI, providers, or plugins.
- Keep point-and-click movement, disease-time pausing, and the world-time debug overlay deferred
  until their own retained prototype or authoritative-system prerequisites exist.
- Do not merge, release, distribute, sign, notarize, change repository visibility, or add
  credentials without explicit user authorization.
