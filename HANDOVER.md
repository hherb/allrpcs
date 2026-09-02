# Allrpcs Handover

## Current state

Phase 2i5 is implemented, reviewed, and locally validated on
`codex/phase-2i5-inventory-capacity-policy`, based directly on the PR #35 merge at `9192c43`.

`InventoryCapacityPolicy.CanOwnAdditionalObject` is the single public, stateless decision for
whether an existing `CharacterId` may own one additional distinct object. It rejects null inputs,
fails closed without a matching capacity, counts only value-matched ownership entries, and returns
true exactly while that count is below the character's recorded maximum. It ignores unrelated
owners and capacities and does not accept an `ObjectId` or mutate either snapshot.

The test-first build failed because the policy type did not exist. All ten focused tests then
passed on the host. The restricted full-quality run completed a zero-warning build before reaching
the known VSTest loopback denial. The identical host-shell `./scripts/quality.sh` passed with a
zero-warning build, 296 Domain tests, 100 Application tests, six architecture tests, four Godot
smokes, and the bounded graphical launch. No manual play acceptance is required for this
engine-independent Domain slice. `Domain -> []` remains intact, and hosted desktop outputs remain
unsigned development evidence, not releases.

## Exact next bounded slice

After this pull request is reported merged, take **Phase 2i6: pure one-object take eligibility**.

Acceptance boundary:

1. Add a pure `InventoryTakePolicy` in `Allrpcs.Domain.Inventory` with one operation that determines
   whether an existing `CharacterId` may take an existing `ObjectId` from an
   `InventoryOwnershipSnapshot` and an `InventoryCapacitySnapshot`.
2. Treat absence of the value-equal object identifier from the ownership snapshot as unowned.
   Reject an object already owned by any character; otherwise permit the take exactly when
   `InventoryCapacityPolicy.CanOwnAdditionalObject` permits the intended owner. Unrelated entries
   must not affect the decision.
3. Add focused pure tests for unowned and already-owned objects, same-owner and other-owner
   rejection, missing/zero/below/equal/above capacity, unrelated-entry isolation, value-based
   identifier matching, null rejection, input immutability, and the deliberately limited public
   surface.
4. Preserve `Domain -> []`; update `README.md`, leave the broader inventory roadmap item
   incomplete, and replace this file with evidence, limits, and one next bounded slice.
5. Run `./scripts/quality.sh`, review every intended path, and commit only the reviewed slice.

Explicit exclusions:

- Return only eligibility; do not mutate or return either snapshot, add an action request, result,
  error taxonomy, event, or decide that existing ownership is a no-op or transfer.
- Do not validate location, reach, visibility, perception, discovery, capability, equipment,
  reservations, item size, weight, slots, quantities, stacks, containers, or aggregate state.
- Do not add give, drop, use, crafting, consumption, transfer execution, mutable state, scenario
  content, serialization, persistence, Godot, UI, providers, or plugins.
- Keep point-and-click movement, disease-time pausing, and the world-time debug overlay deferred
  until their own retained prototype or authoritative-system prerequisites exist.
- Do not merge, release, distribute, sign, notarize, change repository visibility, or add
  credentials without explicit user authorization.
