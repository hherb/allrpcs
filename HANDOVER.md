# Allrpcs Handover

## Current state

PR #136 merged the completed Phase 2o world-event sequence into `main` at `658c881`. The branch
`codex/phase-2p1-plus-versioned-saves` implements the first bounded versioned-save foundation,
Phases 2p1–2p9.

`Allrpcs.Domain.Persistence` now provides:

- an immutable positive-integer `SaveSchemaVersion`, with version `1` as the explicit first
  supported value and strict invariant canonical parsing;
- explicit version and save-root load policies that reject structurally valid future versions;
- `DiscoverySaveState`, `WorldEventSaveState`, and the versioned `SaveSnapshot` root, each retaining
  exact existing immutable state instances;
- a discovery restoration policy requiring one value-equal Seeker owner and only authoritative
  world-object targets;
- a world-event restoration policy requiring every logged identifier to resolve and reproduce the
  saved qualitative time; and
- one composed save-root restoration policy delegating all version, discovery, and event checks.

These are pure engine-independent values and decisions. They do not serialize, migrate, access
files or paths, restore controllers, replay events, or change runtime state.

After code review, the host `./scripts/quality.sh` passed workflow and formatting validation, a
zero-warning build, 1,087 Domain tests, 107 Application tests, six architecture tests, all 27
opt-in Godot smokes, and the bounded graphical launch. The restricted run reached the same clean
build and then hit the known VSTest loopback denial; the identical host run is authoritative.

## Exact next bounded slice

Take **Phase 2p10: restore exact discovery save state into its existing controller**.

Acceptance boundary:

1. Add one `FocusedSearchActivityController` operation that accepts an existing
   `DiscoverySaveState` and authoritative `WorldObjectSnapshot`, rejects null inputs, and delegates
   complete eligibility to `DiscoverySaveStateRestorationPolicy` using the controller's exact
   Seeker identifier.
2. On success, atomically install the save state's exact perception, activity, outcome,
   passive-noticing candidate, and observation-reinterpretation candidate snapshots, then present
   the restored activity state through the existing indicator. Do not reconstruct or copy nested
   values.
3. Prove successful exact-instance installation plus unsupported owner, missing target, null, and
   repeated/failure preservation with one focused opt-in Godot smoke. Preserve surrounding
   world-object, world-event, movement, input, and presentation state outside the existing activity
   indicator.
4. Document only this controller boundary, update evidence, run `./scripts/quality.sh`, review the
   intended paths, and prune this file again.

Explicit exclusions: no world-event controller restoration, cross-controller coordinator,
serialization, JSON, files, paths, migration, autosave, replay, providers, credentials, final save
UI, or startup/load orchestration. Do not merge, release, distribute, sign, notarize, or change
repository visibility without explicit authorization.
