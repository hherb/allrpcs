# Allrpcs Handover

## Current state

Phase 2i7 is implemented, reviewed, and locally validated on
`codex/phase-2i7-inventory-drop-policy`, based directly on the PR #37 merge at `1777f60`.

`InventoryDropPolicy.CanDropObject` is the single public, stateless decision for whether an
existing `CharacterId` may drop an existing `ObjectId`. It rejects null inputs and permits the
drop exactly when the `InventoryOwnershipSnapshot` contains the value-equal object identifier
owned by the value-equal intended character. It ignores unrelated entries, returns only
eligibility, and does not mutate the snapshot.

The test-first build failed because the policy type did not exist. All eight focused tests then
passed on the host. The restricted full-quality run completed a zero-warning build before reaching
the known VSTest loopback denial. The identical host-shell `./scripts/quality.sh` passed with a
zero-warning build, 317 Domain tests, 100 Application tests, six architecture tests, four Godot
smokes, and the bounded graphical launch. No manual play acceptance is required for this
engine-independent Domain slice. `Domain -> []` remains intact, and hosted desktop outputs remain
unsigned development evidence, not releases.

## Exact next bounded slice

After this pull request is reported merged, take **Phase 2i8: pure one-object give eligibility**.

Acceptance boundary:

1. Add a pure `InventoryGivePolicy` in `Allrpcs.Domain.Inventory` with one operation that determines
   whether one existing giver `CharacterId` may give an existing `ObjectId` to one distinct
   existing recipient `CharacterId` under `InventoryOwnershipSnapshot` and
   `InventoryCapacitySnapshot` inputs.
2. Permit the give exactly when the ownership snapshot contains the value-equal object identifier
   owned by the value-equal giver, the giver and recipient identifiers differ by value, and the
   recipient may own one additional object under `InventoryCapacityPolicy`. Reject an unowned
   object, another character's object, a self-give, and a recipient with missing or exhausted
   capacity; unrelated entries must not affect the decision.
3. Add focused pure tests for each eligibility and rejection path, unrelated-entry isolation,
   value-based identifier matching, null rejection, input immutability, and the deliberately
   limited public surface.
4. Preserve `Domain -> []`; update `README.md`, leave the broader inventory roadmap item
   incomplete, and replace this file with evidence, limits, and one next bounded slice.
5. Run `./scripts/quality.sh`, review every intended path, and commit only the reviewed slice.

Explicit exclusions:

- Return only eligibility; do not mutate or return either snapshot, change ownership, reserve
  capacity, or add an action request, result, error taxonomy, event, acceptance flow, or transfer.
- Do not validate location, reach, visibility, perception, discovery, capability, equipment,
  reservations, item size, weight, slots, quantities, stacks, containers, or aggregate state.
- Do not add use, crafting, consumption, transfer execution, mutable state, scenario content,
  serialization, persistence, Godot, UI, providers, or plugins.
- Keep point-and-click movement, disease-time pausing, and the world-time debug overlay deferred
  until their own retained prototype or authoritative-system prerequisites exist.
- Do not merge, release, distribute, sign, notarize, change repository visibility, or add
  credentials without explicit user authorization.
