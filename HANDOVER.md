# Allrpcs Handover

## Current state

Phase 2l2 is implemented on `codex/phase-2l2-exact-world-object-lookup`, based directly on PR
#98's verified merge commit `27d797c`.

Slice commit `3f2f7b7` is pushed, and draft PR #99
(`https://github.com/hherb/allrpcs/pull/99`) is open. Pull-request merge remains separately
unauthorized.

`Allrpcs.Domain.WorldObjects.WorldObjectLookup.FindById` rejects either null input and otherwise
matches only a value-equal `WorldObject.Identity.Id`. It returns the exact retained `WorldObject`
instance, including its exact identity, identifier, and description instances, or `null` for an
empty or unrelated snapshot. It does not change either input or compare authored text.

Tests were written first and failed on the missing type. After implementation and code review, the
host-shell `./scripts/quality.sh` gate passed with a zero-warning build, 644 Domain tests, 107
Application tests, six architecture tests, all eleven Godot smokes, and the bounded graphical
launch. The sandbox-only run reached the known VSTest loopback denial after building cleanly; the
identical host-shell gate passed. No manual visual inspection was needed for this Domain-only slice.

## Exact next bounded slice

Take **Phase 2l3: temporary world-object source**.

Acceptance boundary:

1. Add one narrow Godot `TemporaryWorldObjectSource` node that, on `_Ready`, reads only the existing
   herb-patch and stone-marker `GreyboxInteractionTarget` nodes and exposes one retained
   `WorldObjectSnapshot` through a get/private-set property.
2. Construct exactly one `WorldObject` per target in herb-patch then stone-marker order. Retain each
   target's exact existing `ObjectId` and exact authored `DisplayName`; use an empty
   `WorldObjectDescription` to state explicitly that no authoritative description is authored yet.
3. Register the source and one opt-in smoke check in `Main.tscn` and `scripts/smoke-godot.sh`. Cover
   stable order, exact identifier retention, exact names, empty descriptions, distinct object
   instances, one stable snapshot, unchanged target inputs, and no controller or presentation
   changes.
4. Document Phase 2l3 compactly, leave the broader authoritative-world and focused-search roadmap
   items incomplete, run `./scripts/quality.sh`, review every intended path, and prune this file to
   current evidence plus one next slice.

Explicit exclusions:

- Do not make the temporary source authoritative, add object location/item associations, mutate or
  replace targets, add lookup policy beyond Phase 2l2, or wire the snapshot into `Main`, controllers,
  focus selection, perception resolution, discovery presentation, or ordinary-play behavior.
- Do not add non-empty authored descriptions, visibility filtering or ranking, knowledge, evidence,
  persistence, serialization, providers, plugins, credentials, or new UI.
- Keep point-and-click movement, disease-time pausing, and the world-time debug overlay deferred
  until their retained prototype or authoritative-system prerequisites exist.
- Do not merge, release, distribute, sign, notarize, or change repository visibility without
  explicit user authorization.
