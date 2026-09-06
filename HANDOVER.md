# Allrpcs Handover

## Current state

Phase 2l3 is implemented on `codex/phase-2l3-temporary-world-object-source`, based directly on PR
#99's verified merge commit `f35b6a1`.

Slice commit `2c2a5b1` is pushed, and draft PR #100
(`https://github.com/hherb/allrpcs/pull/100`) is open. Pull-request merge remains separately
unauthorized.

The scene now owns one `TemporaryWorldObjectSource`. During `_Ready`, it reads only the existing
visible herb-patch and stone-marker targets and creates one retained `WorldObjectSnapshot` in that
order. Its two distinct `WorldObject` instances retain each target's exact parsed `ObjectId` and
exact authored `DisplayName`; both have an empty `WorldObjectDescription` because no authoritative
description is authored yet. The source does not mutate targets, call a controller, change
presentation, or participate in ordinary play.

The test-first game build failed with `CS0246` before the source existed. After implementation and
code review, host-shell `./scripts/quality.sh` passed with a zero-warning build, 644 Domain tests,
107 Application tests, six architecture tests, all twelve Godot smokes, and the bounded graphical
launch. The sandbox-only gate reached the known VSTest loopback denial after building cleanly. No
manual visual inspection was needed because visible behavior did not change.

## Exact next bounded slice

Take **Phase 2l4: controller-owned world-object initialization**.

Acceptance boundary:

1. Add one separate `WorldObjectController` Godot node owning an optional immutable
   `WorldObjectSnapshot` through a get/private-set property and one narrow Boolean one-time
   initialization method accepting an existing snapshot.
2. Retain the exact supplied snapshot instance. Null initialization must throw, while repeated
   initialization must fail closed and preserve the first snapshot and all existing presentation
   and focused-search controller state.
3. Register the controller in `Main.tscn`; during `Main._Ready`, pass the temporary source's exact
   retained snapshot once and fail explicitly if the scene cannot establish that invariant.
4. Add one opt-in credential-free smoke covering the initial boundary, exact scene-wired instance,
   repeated and null rejection, and unchanged source, targets, focused-search state, and
   presentation.
5. Document Phase 2l4 compactly, leave the broader authoritative-world and focused-search roadmap
   item incomplete, run `./scripts/quality.sh`, review every intended path, and prune this file to
   current evidence plus one next slice.

Explicit exclusions:

- Do not query or expose world objects through the controller yet; inspect focus, perception, or
  outcomes; resolve discovery; add input; change presentation; or alter ordinary-play behavior.
- Do not make the temporary source or controller authoritative, add non-empty descriptions,
  locations, item associations, filtering, visibility ranking, knowledge, evidence, events,
  persistence, serialization, providers, plugins, credentials, or new UI.
- Keep point-and-click movement, disease-time pausing, and the world-time debug overlay deferred
  until their retained prototype or authoritative-system prerequisites exist.
- Do not merge, release, distribute, sign, notarize, or change repository visibility without
  explicit user authorization.
