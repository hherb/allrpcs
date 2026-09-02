# Allrpcs Handover

## Current state

Phase 2i8 is implemented, reviewed, and locally validated on
`codex/phase-2i8-inventory-give-policy`, based directly on the PR #38 merge at `34f4bd4`.
Draft PR #39 (`https://github.com/hherb/allrpcs/pull/39`) is open. Pull-request merge remains
separately authorized.

`InventoryGivePolicy.CanGiveObject` is the single public, stateless decision for whether one
existing giver `CharacterId` may give an existing `ObjectId` to one distinct existing recipient
`CharacterId`. It rejects null inputs and self-gives, requires the value-equal giver to own the
value-equal object, and delegates recipient capacity eligibility to
`InventoryCapacityPolicy.CanOwnAdditionalObject`. It ignores unrelated entries, returns only
eligibility, and does not mutate either snapshot.

The test-first build failed because the policy type did not exist. All 14 focused tests then passed
on the host. The restricted full-quality run completed a zero-warning build before reaching the
known VSTest loopback denial. The identical host-shell `./scripts/quality.sh` passed with a
zero-warning build, 331 Domain tests, 100 Application tests, six architecture tests, four Godot
smokes, and the bounded graphical launch. No manual play acceptance is required for this
engine-independent Domain slice. `Domain -> []` remains intact, and hosted desktop outputs remain
unsigned development evidence, not releases.

## Exact next bounded slice

After this pull request is reported merged, take **Phase 2i9: pure one-object use eligibility**.

Acceptance boundary:

1. Add a pure `InventoryUsePolicy` in `Allrpcs.Domain.Inventory` with one operation that determines
   whether an existing `CharacterId` may use an existing `ObjectId` from an
   `InventoryOwnershipSnapshot`.
2. Permit use exactly when the ownership snapshot contains the value-equal object identifier owned
   by the value-equal intended character. Reject an unowned object or an object owned by any other
   character; unrelated entries must not affect the decision.
3. Add focused pure tests for owned and unowned objects, same-owner eligibility, other-owner
   rejection, unrelated-entry isolation, value-based character and object matching, null
   rejection, input immutability, and the deliberately limited public surface.
4. Preserve `Domain -> []`; update `README.md`, and mark the broader inventory ownership, capacity,
   give, take, drop, and use-validation roadmap item complete only after the slice passes full
   validation. Do not imply that inventory actions or transfers execute.
5. Replace this file with evidence, limits, and one next bounded slice; run
   `./scripts/quality.sh`, review every intended path, and commit only the reviewed slice.

Explicit exclusions:

- Return only eligibility; do not mutate or return the snapshot, consume, equip, remove, or alter
  the object, or add an action request, result, error taxonomy, event, target, effect, or duration.
- Do not validate item classification, function, consumability, location, reach, visibility,
  perception, discovery, capability, tool requirements, equipment, reservations, size, weight,
  slots, quantities, stacks, containers, or aggregate state.
- Do not add crafting, consumption, action or transfer execution, mutable state, scenario content,
  serialization, persistence, Godot, UI, providers, or plugins.
- Keep point-and-click movement, disease-time pausing, and the world-time debug overlay deferred
  until their own retained prototype or authoritative-system prerequisites exist.
- Do not merge, release, distribute, sign, notarize, change repository visibility, or add
  credentials without explicit user authorization.
