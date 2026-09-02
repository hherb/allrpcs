# Allrpcs Handover

## Current state

Phase 2i1 is implemented, reviewed, and locally validated on
`codex/phase-2i1-inventory-ownership`, based directly on the PR #31 merge at `4038e7e`. Draft PR
creation is pending.

`InventoryOwnership` is a sealed immutable association between exactly one existing `ObjectId`
and one existing owner `CharacterId`. It requires and retains both supplied identifiers, exposes
only get-only `ObjectId` and `OwnerCharacterId` properties, and compares associations by both
values. It adds no default owner, unowned state, collection, or cross-association policy.

The test-first build failed because the namespace and type did not exist. All four focused tests
then passed on the host. The restricted focused run was cancelled after VSTest stalled at its
known loopback boundary. The identical host-shell `./scripts/quality.sh` passed with a zero-warning
build, 269 Domain tests, 100 Application tests, six architecture tests, four Godot smokes, and the
bounded graphical launch. No manual play acceptance is required for this engine-independent
Domain slice. Hosted desktop outputs remain unsigned development evidence, not releases.

## Exact next bounded slice

After this pull request is reported merged, take **Phase 2i2: immutable inventory-ownership
snapshot**.

Acceptance boundary:

1. Add a sealed immutable `InventoryOwnershipSnapshot` in `Allrpcs.Domain.Inventory` containing
   existing `InventoryOwnership` entries in stable supplied order.
2. Explicitly permit an empty sequence, copy caller input, expose only a read-only `Entries`
   collection, and reject a null sequence, null entries, and duplicate `ObjectId` values. Retain
   each supplied entry instance and add no implicit ownership or default owner.
3. Add focused pure tests for empty behavior, order and reference retention, caller-input
   isolation, duplicate and null rejection, and the deliberately limited public surface.
4. Preserve `Domain -> []`; update `README.md`, leave the broader inventory roadmap item
   incomplete, and replace this file with evidence, limits, and one next bounded slice.
5. Run `./scripts/quality.sh`, review every intended path, and commit only the reviewed slice.

Explicit exclusions:

- Do not integrate ownership into `CharacterInventory`, `Character`, `Item`, a world aggregate,
  or any mutable registry, and do not cross-check object or character existence.
- Do not add capacity, quantities, stacks, slots, weight, containers, equipment, give, take,
  drop, use, crafting, consumption, transfer, or action validation.
- Do not interpret ownership as location, possession history, visibility, discovery, permission,
  or a world-event outcome.
- Do not add mutable state, actions, events, scenario content, serialization, persistence, Godot,
  UI, providers, or plugins.
- Keep point-and-click movement, disease-time pausing, and the world-time debug overlay deferred
  until their own retained prototype or authoritative-system prerequisites exist.
- Do not merge, release, distribute, sign, notarize, change repository visibility, or add
  credentials without explicit user authorization.
