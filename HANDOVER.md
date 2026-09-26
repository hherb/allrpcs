# Allrpcs Handover

## Current state

`main` includes merged PR #398 at `bf60593`. Branch
`codex/phase-8-journal-persistence` continues Phase 8 through three bounded persistence slices:
content-safe journal capture, versioned save-root integration/migration, and canonical JSON.

`JournalSaveStateCaptureProjection` accepts only a complete exact safe `JournalSnapshot` and an
optional exact canonical section from that journal. It returns fresh independent immutable copies of
all five ordered safe content arms. `JournalSaveState` exposes only the Seeker identifier, safe entry
values, and optional canonical selected-section key; it retains no complete catalogue, perception or
world binding, dialogue audience, authoritative experiment event, investigation aggregate,
controller, or UI state.

Save schema version 5 requires exact first-patient and journal state. The root rejects a journal for
a different discovery character while accepting a separately materialized value-equal identifier.
The one-way version-4 migration creates fresh empty section collections, no selection, and the saved
discovery owner. Stable canonical JSON reads and writes every journal value, retains version-4
dialogue delivery modes, rejects unknown/malformed properties and oversized arrays, and migrates
versions 1 through 4 to current version 5.

No scene save capture, journal restoration, controller installation, automatic journal composition,
source refresh, panel change, editable note, map, gameplay mutation, authoritative source exposure,
provider call, or credential access was added. The Phase 8 journal roadmap checkbox remains open.

## Validation and review

- Test-first capture work observed the missing persistence types and projection before implementation.
- Six focused Application tests pass complete/empty capture, exact selection, independent immutable
  copies, incomplete/foreign/rebuilt/null rejection, and closed persistence surfaces.
- Fifty-seven focused Domain tests pass version-5 root invariants, supported-version/load behavior,
  exact retained state, and fresh one-way version-4 journal migration.
- Seven focused Infrastructure tests pass stable populated and empty JSON, complete round trips,
  legacy current migration, and malformed journal rejection.
- Session review checked every new production/test file, version branch, migration, JSON order and
  strictness, documentation, 120-character limits, sub-500-line files, and whitespace. It added the
  missing save-root invariant tying journal ownership to the saved discovery character.
- Final host-local `./scripts/quality.sh` passed workflow validation, formatting, a zero-warning
  build, 2,817 Domain, 819 Application, 1,227 Infrastructure, and 8 architecture tests, scenario
  validation, every credential-free Godot smoke, and the graphical Compatibility launch. The
  restricted run passed every pure check before the known Godot `user://logs` restriction.

## Exact next bounded slice

Add one pure journal save-state restoration projection without changing scene controllers or save
capture yet.

Acceptance boundary:

1. Rebuild one fresh complete safe `JournalSnapshot` from a valid `JournalSaveState` using a fresh
   canonical catalogue and its exact section definitions.
2. Restore every persisted safe entry in stable order and return the optional exact canonical
   selected definition matching `SelectedSectionKey`.
3. Require the saved Seeker and canonical selection key; reject null, malformed, duplicate-identity,
   non-canonical, or cross-section-inconsistent state without partial output.
4. Prove fresh independent immutable journals, exact restored selection ownership, all five content
   arms, explicit no-selection, and closed safe surfaces with focused Application tests.
5. Do not change scene save capture/restoration controllers, schema version 5, JSON, journal source
   refresh, panel behavior, editable notes, map, gameplay, or providers.
6. Run `./scripts/quality.sh`, review the complete diff, update `README.md`, and leave one exact next
   slice for installing restored journal state and selection atomically in the scene.
