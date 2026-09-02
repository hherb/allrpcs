# Allrpcs Handover

## Current state

Phase 2i6 is implemented, reviewed, and locally validated on
`codex/phase-2i6-inventory-take-policy`, based directly on the PR #36 merge at `11dd46f`.

`InventoryTakePolicy.CanTakeObject` is the single public, stateless decision for whether an
existing `CharacterId` may take an existing `ObjectId`. It rejects null inputs and an object
already owned by any character, then delegates unowned-object capacity eligibility to
`InventoryCapacityPolicy.CanOwnAdditionalObject`. It matches identifiers by value, ignores
unrelated entries, returns only eligibility, and does not mutate either snapshot.

The test-first build failed because the policy type did not exist. All 13 focused tests then
passed on the host. The restricted full-quality run completed a zero-warning build before reaching
the known VSTest loopback denial. The identical host-shell `./scripts/quality.sh` passed with a
zero-warning build, 309 Domain tests, 100 Application tests, six architecture tests, four Godot
smokes, and the bounded graphical launch. No manual play acceptance is required for this
engine-independent Domain slice. `Domain -> []` remains intact, and hosted desktop outputs remain
unsigned development evidence, not releases.

## Exact next bounded slice

After this pull request is reported merged, take **Phase 2i7: pure one-object drop eligibility**.

Acceptance boundary:

1. Add a pure `InventoryDropPolicy` in `Allrpcs.Domain.Inventory` with one operation that determines
   whether an existing `CharacterId` may drop an existing `ObjectId` from an
   `InventoryOwnershipSnapshot`.
2. Permit the drop exactly when the ownership snapshot contains the value-equal object identifier
   owned by the value-equal intended character. Reject an unowned object or an object owned by any
   other character; unrelated entries must not affect the decision.
3. Add focused pure tests for owned and unowned objects, same-owner eligibility, other-owner
   rejection, unrelated-entry isolation, value-based character and object matching, null
   rejection, input immutability, and the deliberately limited public surface.
4. Preserve `Domain -> []`; update `README.md`, leave the broader inventory roadmap item
   incomplete, and replace this file with evidence, limits, and one next bounded slice.
5. Run `./scripts/quality.sh`, review every intended path, and commit only the reviewed slice.

Explicit exclusions:

- Return only eligibility; do not mutate or return the snapshot, remove ownership, add an action
  request, result, error taxonomy, event, destination, capacity decision, recipient, or transfer.
- Do not validate location, reach, visibility, perception, discovery, capability, equipment,
  reservations, item size, weight, slots, quantities, stacks, containers, or aggregate state.
- Do not add give, use, crafting, consumption, transfer execution, mutable state, scenario content,
  serialization, persistence, Godot, UI, providers, or plugins.
- Keep point-and-click movement, disease-time pausing, and the world-time debug overlay deferred
  until their own retained prototype or authoritative-system prerequisites exist.
- Do not merge, release, distribute, sign, notarize, change repository visibility, or add
  credentials without explicit user authorization.
