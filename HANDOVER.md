# Allrpcs Handover

## Current state

Phase 2l1 is implemented on `codex/phase-2l1-immutable-world-object-snapshot`, based directly on
PR #97's verified merge commit `d666ec9`.

`Allrpcs.Domain.WorldObjects.WorldObjectSnapshot` copies one supplied sequence exactly once and
exposes the exact `WorldObject` instances in stable order through `IReadOnlyList<WorldObject>`.
Empty input is valid. Null sequences, null entries, and value-equal duplicate
`WorldObject.Identity.Id` values are rejected; display names and descriptions do not participate in
identity. The type adds no lookup, mutation, association, filtering, runtime, persistence, or UI
behavior.

Tests were written first and failed on the missing type. After implementation and code review, the
host-shell `./scripts/quality.sh` gate passed with a zero-warning build, 637 Domain tests, 107
Application tests, six architecture tests, all eleven Godot smokes, and the bounded graphical
launch. The sandbox-only run reached the known VSTest loopback denial after building cleanly; the
identical host-shell gate passed. No manual visual inspection was needed for this Domain-only slice.
Unsigned hosted artifacts remain development evidence, not releases.

## Exact next bounded slice

Take **Phase 2l2: exact world-object lookup**.

Acceptance boundary:

1. Add one public static `Allrpcs.Domain.WorldObjects.WorldObjectLookup` with only
   `FindById(ObjectId objectId, WorldObjectSnapshot snapshot)`.
2. Match only `WorldObject.Identity.Id` by value. Return the exact retained `WorldObject` instance,
   or `null` for an empty or unrelated snapshot. Reject either null input and leave both inputs
   unchanged.
3. Add focused tests first for empty and unrelated snapshots, a distinct value-equal identifier,
   a match among surrounding entries, exact nested-instance retention, unchanged inputs, both null
   boundaries, and the minimal static public API.
4. Document Phase 2l2 compactly, keep the broader authoritative-world and focused-search roadmap
   items incomplete, run `./scripts/quality.sh`, review every intended path, and keep both planning
   documents pruned to current evidence and one exact next slice.

Explicit exclusions:

- Do not compare display names or descriptions, project or filter collections, rank visibility,
  mutate or replace objects, or add selection policy beyond the exact identifier lookup.
- Do not add Godot adapters, runtime sources, world or controller ownership, location or item
  association, perception/discovery wiring, persistence, serialization, providers, plugins,
  credentials, or UI.
- Do not replace the temporary focused-search targets or presentation, grant knowledge or evidence,
  or wire the snapshot into ordinary play.
- Keep point-and-click movement, disease-time pausing, and the world-time debug overlay deferred
  until their retained prototype or authoritative-system prerequisites exist.
- Do not merge, release, distribute, sign, notarize, or change repository visibility without
  explicit user authorization.
