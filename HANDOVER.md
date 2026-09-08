# Allrpcs Handover

## Current state

PR #137 merged Phases 2p1–2p9 into `main` at `33c2df3`. The branch
`codex/phase-2p10-plus-save-restoration` completes Phases 2p10–2p12:

- `FocusedSearchActivityController.TryRestoreDiscoveryState` preflights and installs the exact five
  saved discovery snapshots, then updates only the existing activity indicator.
- `WorldEventController.TryRestoreWorldEventState` preflights and installs the exact saved event log
  and qualitative time while retaining its authored event definitions.
- `SaveSnapshotRestorationController.TryRestore` preflights the complete supported save root before
  coordinating both live controller operations, so rejected roots cannot partially restore state.

This is exact in-memory restoration only. No serialization, storage, migration, replay, or automatic
startup/load behavior exists.

After code review, the host `./scripts/quality.sh` passed workflow and formatting validation, a
zero-warning build, 1,087 Domain tests, 107 Application tests, six architecture tests, all 30 opt-in
Godot smokes, and the bounded graphical launch. The restricted run reached the same clean build and
then hit the known VSTest loopback denial; the identical host run is authoritative.

## Exact next bounded slice

Take **Phase 2p13: capture the exact current live state as an in-memory save root**.

Acceptance boundary:

1. Add one scene-owned capture operation that returns no snapshot until discovery perception,
   authored world events, event log, and qualitative time are all initialized.
2. On success, compose `DiscoverySaveState`, `WorldEventSaveState`, and a version-1 `SaveSnapshot`
   from the controllers' exact current snapshot instances; do not reconstruct or copy nested values.
3. Prove exact-instance capture, repeated distinct roots over the same exact nested state,
   uninitialized rejection, unchanged controllers/inputs/presentation, and restoration of the
   captured root through the existing coordinator with one focused opt-in Godot smoke.
4. Document only this in-memory capture boundary, run `./scripts/quality.sh`, review intended paths,
   and prune this file again.

Explicit exclusions: no serialization or JSON contract, files, paths, storage provider, migration,
autosave, replay, final save UI, startup/load orchestration, providers, or credentials. Do not merge,
release, distribute, sign, notarize, or change repository visibility without explicit authorization.
