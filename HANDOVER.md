# Allrpcs Handover

## Current state

Phase 2i3 is implemented, reviewed, and locally validated on
`codex/phase-2i3-inventory-capacity`, based directly on the PR #33 merge at `7c91325`.

`InventoryCapacity` is a sealed immutable value containing one existing `CharacterId` and a
non-negative `MaximumObjectCount` for distinct owned objects. It retains the supplied identifier,
explicitly permits zero, rejects negative counts, exposes only get-only properties, and compares
values by both fields. It does not enforce capacity or define a default or unlimited policy.

The test-first build failed because the type did not exist. All five focused tests then passed on
the host after the restricted runner reached the known VSTest loopback denial. The host-shell
`./scripts/quality.sh` passed with a zero-warning build, 280 Domain tests, 100 Application tests,
six architecture tests, four Godot smokes, and the bounded graphical launch. No manual play
acceptance is required for this engine-independent Domain slice. `Domain -> []` remains intact,
and hosted desktop outputs remain unsigned development evidence, not releases.

## Exact next bounded slice

After this pull request is reported merged, take **Phase 2i4: immutable inventory-capacity
snapshot**.

Acceptance boundary:

1. Add a sealed immutable `InventoryCapacitySnapshot` in `Allrpcs.Domain.Inventory` containing
   existing `InventoryCapacity` entries in stable supplied order.
2. Explicitly permit an empty sequence, copy caller input, retain each supplied entry instance,
   expose only a read-only `Entries` collection, and reject a null sequence, null entries, and
   duplicate `CharacterId` values.
3. Add focused pure tests for empty input, order and reference retention, defensive copying,
   duplicate-character rejection, null rejection, collection immutability, and the deliberately
   limited public surface.
4. Preserve `Domain -> []`; update `README.md`, leave the broader inventory roadmap item
   incomplete, and replace this file with evidence, limits, and one next bounded slice.
5. Run `./scripts/quality.sh`, review every intended path, and commit only the reviewed slice.

Explicit exclusions:

- Do not compare capacities with `InventoryOwnershipSnapshot` or `CharacterInventory`, enforce a
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
