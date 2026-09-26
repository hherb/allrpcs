# Allrpcs Handover

## Current state

`main` includes merged PR #404 at `14b7a17`. Branch `codex/phase-8-map-persistence`
advances the open Phase 8 map roadmap item through two bounded persistence slices.

`MapSaveState` retains only one Seeker identifier, reported and verified location identifiers, and
location/note-text pairs. `MapSaveStateCaptureProjection` accepts only exact same-Seeker map and note
snapshots backed by one exact catalogue, requires every note to target a known location, and emits
fresh immutable values in canonical location order. It exposes no hidden definitions, hidden labels,
coordinates, topology, testimony, scene objects, controllers, or UI state.

`MapSaveStateRestorationProjection` preflights unknown, duplicate, overlapping reported/verified,
hidden-note, duplicate-note, invalid-text, and null state before returning anything. Valid state
creates one fresh representative catalogue plus fresh same-Seeker map and note snapshots, with every
restored location rebound to the exact fresh canonical definition and every arm in canonical order.

`SaveSchemaVersion.Map` is version 6. A version-6 `SaveSnapshot` requires the existing first-patient
and journal state plus same-owner map state; discovery, journal, and map owners must agree.
`SaveSnapshotMapMigration` accepts only version 5, preserves every existing root component by exact
reference, and adds fresh empty same-Seeker map collections. Version 6 intentionally remains outside
`SaveSchemaVersionSupportPolicy` until strict JSON and runtime wiring are complete.

No map JSON, storage, current-loading migration activation, scene capture/restoration, automatic
visit polling or testimony reporting, new UI, provider call, credential access, release, or signing
work was added. Ordinary startup and the Phase 8 map roadmap item remain unchanged.

## Validation and review

- Test-first checkpoints observed the missing map persistence types and missing schema-6 root before
  implementation.
- Thirteen focused Application tests cover empty/populated capture and restoration, canonical order,
  fresh exact definitions, immutable independent values, round trips, closed surfaces, exact
  catalogue/owner provenance, and every malformed-state rejection named above.
- Focused Domain tests cover the schema-6 value, required root components and owner agreement, exact
  version-5 migration retention, fresh empty map collections, wrong-version rejection, and the
  deliberate absence of a version-6 load contract.
- Session review found and fixed a staged-contract inconsistency: older tests that used numeric 6 as
  a future version now use 7, and the README explicitly records that version 6 is not loadable yet.
- Every changed C# file is below 500 lines and every changed C# line is at most 120 characters.
  `git diff --check` passed.
- After the first slice, host-local `./scripts/quality.sh` passed workflow validation, formatting, a
  zero-warning build, 2,817 Domain, 877 Application, 1,227 Infrastructure, and 8 architecture tests,
  scenario validation, every credential-free Godot smoke, and the graphical Compatibility launch.
  The restricted sandbox failed only at Godot's known `user://logs` boundary.
- Final host-local `./scripts/quality.sh` passed workflow validation, formatting, a zero-warning
  build, 2,822 Domain, 877 Application, 1,227 Infrastructure, and 8 architecture tests, scenario
  validation, every credential-free Godot smoke, and the graphical Compatibility launch.

## Exact next bounded slice

Complete schema-6 persistence and runtime integration atomically; do not leave current loading able
to produce map state that scene restoration ignores.

Acceptance boundary:

1. Add strict bounded `MapSaveState` JSON writer/reader support. Canonically write only `seekerId`,
   `reportedLocationIds`, `verifiedLocationIds`, and ordered `{locationId,text}` notes; reject null,
   missing, duplicate-property, unknown-property, wrong-kind, oversized, and malformed values.
2. Extend root JSON serialization/deserialization for exact version 6 with both journal and map.
   Preserve exact version-1-through-5 document contracts.
3. Extend `DeserializeCurrent` through `SaveSnapshotMapMigration` and only then add version 6 to the
   supported-load policy. Prove versions 1 through 5 migrate to fresh empty same-Seeker map state and
   direct version-6 documents round trip without sharing collections.
4. Extend scene capture to require exact initialized same-Seeker map and note state, capture it through
   `MapSaveStateCaptureProjection`, and emit version 6 only after every root component is available.
5. Extend scene restoration with complete map preflight through `MapSaveStateRestorationProjection`,
   then install discovery, world events, memory, first-patient state, journal/selection, and map/notes
   atomically. Add only the minimum exact-state controller restore operation needed for that install.
6. Extend serialized-save and isolated-storage smokes for populated map/report/verify/note round trips,
   version-5 empty-map migration, malformed/unknown/hidden-note rejection, fresh canonical definitions,
   and preservation of every controller on failure.
7. Do not add automatic visit/report behavior, player-visible reporting choices, coordinate/topology
   exposure, new map UI, providers, credentials, release, or signing work.
8. Run focused tests and `./scripts/quality.sh`, review the complete diff, update `README.md`, and leave
   one next bounded slice to close the Phase 8 map roadmap item only if end-to-end persistence is
   complete.
