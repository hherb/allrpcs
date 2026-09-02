# Allrpcs Handover

## Current state

Phase 2h3 is implemented, reviewed, and locally validated on
`codex/phase-2h3-character-perception-snapshot`, based directly on the PR #30 merge at
`542fbae`. Draft PR #31 (`https://github.com/hherb/allrpcs/pull/31`) is open. Pull-request merge
remains separately authorized.

`CharacterPerceptionSnapshot` is a sealed immutable association between exactly one existing
`CharacterId` and an ordered collection of existing `WorldObjectVisibility` entries. It retains
the supplied character identifier and entry instances, explicitly permits an empty sequence,
copies caller input, preserves entry order, exposes a read-only collection, and rejects a null
identifier, null sequence, null entries, and duplicate `ObjectId` values. It adds no implicit
`unnoticed` entries, default state, or cross-entry policy.

The test-first build failed only because the type did not exist. All seven focused tests then
passed on the host. The restricted full sequence produced a zero-warning build before VSTest hit
its known loopback restriction. The identical host-shell `./scripts/quality.sh` passed: 265
Domain tests, 100 Application tests, six architecture tests, four Godot smokes, and the bounded
graphical launch. No manual play acceptance is required for this engine-independent Domain slice.
Hosted desktop outputs remain unsigned development evidence, not releases.

## Exact next bounded slice

After this pull request is reported merged, take **Phase 2i1: immutable inventory-ownership
association**.

Acceptance boundary:

1. Add a sealed immutable `InventoryOwnership` in `Allrpcs.Domain.Inventory` that associates
   exactly one existing `ObjectId` with exactly one existing owner `CharacterId`.
2. Require and retain both supplied identifiers, expose only get-only `ObjectId` and
   `OwnerCharacterId` properties, and compare associations by both values. Add no default owner,
   unowned state, collection, or cross-association policy.
3. Add focused pure tests for reference retention, value equality, null rejection, and the
   deliberately limited public surface.
4. Preserve `Domain -> []`; update `README.md`, leave the broader inventory roadmap item
   incomplete, and replace this file with evidence, limits, and one next bounded slice.
5. Run `./scripts/quality.sh`, review every intended path, and commit only the reviewed slice.

Explicit exclusions:

- Do not add an ownership collection or snapshot, enforce global object uniqueness, or integrate
  ownership into `CharacterInventory`, `Character`, `Item`, or a world aggregate yet.
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
