# Allrpcs Handover

## Current state

Phase 2i2 is implemented, reviewed, and locally validated on
`codex/phase-2i2-inventory-ownership-snapshot`, based directly on the PR #32 merge at `9d80ce0`.

`InventoryOwnershipSnapshot` is a sealed immutable collection of existing
`InventoryOwnership` entries. It permits an empty sequence, copies caller input, preserves stable
supplied order and each entry instance, exposes only a read-only `Entries` collection, and rejects
a null sequence, null entries, and duplicate `ObjectId` values. It adds no implicit ownership or
default owner.

The test-first build failed because the type did not exist. All six focused tests then passed on
the host. Restricted focused and full-quality runs reached the known VSTest loopback denial. The
identical host-shell `./scripts/quality.sh` passed with a zero-warning build, 275 Domain tests, 100
Application tests, six architecture tests, four Godot smokes, and the bounded graphical launch.
No manual play acceptance is required for this engine-independent Domain slice. `Domain -> []`
remains intact, and hosted desktop outputs remain unsigned development evidence, not releases.

## Exact next bounded slice

After this pull request is reported merged, take **Phase 2i3: immutable per-character inventory
capacity**.

Acceptance boundary:

1. Add a sealed immutable `InventoryCapacity` value in `Allrpcs.Domain.Inventory` containing one
   existing `CharacterId` and a non-negative `MaximumObjectCount` for distinct owned objects.
2. Require and retain the supplied character identifier, explicitly permit zero capacity, reject
   negative capacity, expose only get-only properties, and compare values by both fields.
3. Add focused pure tests for reference retention, zero and positive capacities, equality, null
   and negative rejection, and the deliberately limited public surface.
4. Preserve `Domain -> []`; update `README.md`, leave the broader inventory roadmap item
   incomplete, and replace this file with evidence, limits, and one next bounded slice.
5. Run `./scripts/quality.sh`, review every intended path, and commit only the reviewed slice.

Explicit exclusions:

- Do not compare capacity with `InventoryOwnershipSnapshot` or `CharacterInventory`, enforce a
  capacity decision, add lookup/default/unlimited policy, or integrate with an aggregate.
- Do not add per-item size, weight, slots, quantities, stacks, containers, equipment, or
  ownership transfer.
- Do not add give, take, drop, use, crafting, consumption, action requests, outcome validation,
  mutable state, actions, events, scenario content, serialization, persistence, Godot, UI,
  providers, or plugins.
- Keep point-and-click movement, disease-time pausing, and the world-time debug overlay deferred
  until their own retained prototype or authoritative-system prerequisites exist.
- Do not merge, release, distribute, sign, notarize, change repository visibility, or add
  credentials without explicit user authorization.
