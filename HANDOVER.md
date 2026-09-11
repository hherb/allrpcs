# Allrpcs Handover

## Current state

`main` contains merged PR #199 at `6d95bba`. The current branch is
`codex/phase-6-first-patient-save-state`.

This branch completes two bounded Phase 6 persistence prerequisites:

- **Phase 6y7** adds immutable `FirstPatientSaveState` composition and exact live scene capture.
- **Phase 6y8** adds the version-3 in-memory save root and pure version-2 migration prerequisite.

`FirstPatientSaveState` retains the exact disease-truth, prepared-batch,
administration-experiment, and universal-experiment snapshots. `FirstPatientStateController.Capture`
returns a fresh composition only when all four snapshots are initialized and otherwise fails closed.
The opt-in ordinary-scene smoke proves repeated exact capture without changing first-patient,
world-event, preparation, discovery, memory, dialogue, movement, model, or presentation state.

`SaveSchemaVersion.FirstPatient` is version `3`. Only that exact version requires and retains the
first-patient composition; versions 1 and 2 reject it. `SaveSnapshotFirstPatientMigration` accepts
only a version-2 root plus an explicit exact authored initial first-patient state, then returns a
fresh version-3 root retaining every prior exact state composition. It does not invent disease
truth. Version-3 load and JSON support remain disabled, and the existing serializer explicitly
rejects the new root so no first-patient state can be silently discarded.

This branch does not serialize, deserialize, load, restore, or persist version-3 state; change save
files or storage; alter gameplay input or UI; execute investigation consequences; call a provider;
begin Phase 7; release; or distribute.

Draft PR #200 contains this work.

## Validation and review

- 9 new focused Domain tests passed for exact composition, immutable surfaces, root constraints,
  and pure migration
- the focused version-3 serializer guard passed
- the new opt-in first-patient save-state capture Godot smoke passed on Godot 4.6.3 .NET
- `./scripts/quality.sh` passed cleanly:
  - workflow validation, restore, formatting, and zero-warning solution build passed
  - 1,928 Domain, 402 Application, 441 Infrastructure, and 8 architecture tests passed
  - the representative discovery scenario validated 5 evidence definitions
  - all 82 credential-free Godot smokes passed
  - the bounded 60-frame Compatibility-renderer launch passed
- `git diff --check`, the practical 120-character limit, exact-identity, immutability,
  fail-closed/no-silent-discard, migration-source, minimal-surface, credential-free, and network-free
  review passed

The restricted focused VSTest and Godot runs reproduced the known localhost-bind and `user://logs`
restrictions; the identical host-local commands passed. Draft PR #200 automatically triggered run
`34652085962`; all five jobs completed with `steps: []` under the unchanged Actions account-budget
condition. Do not retry or dispatch hosted workflows unless that external condition changes.

## Exact next bounded slice

Take **Phase 6y9: canonical version-3 first-patient JSON serialization**.

Acceptance boundary:

1. Extend `SaveSnapshotJsonSerializer` to write a stable complete version-3 `firstPatient`
   representation while preserving the exact version-1 and version-2 documents byte-for-byte.
2. Serialize every canonical disease definition and infection, retained prepared batch and its
   completed preparation, administration-specific experiment record, and universal experiment
   record needed for later exact reconstruction. Preserve supplied order, optional values, units,
   qualitative states, and identifiers without serializing authored object references.
3. Prove exact empty and representative stabilized documents, stable value-equal output, bounds,
   unchanged inputs, and absence of provider data, credentials, hidden reasoning, paths, or UI text
   with focused Infrastructure tests.
4. Do not yet enable version-3 load support, deserialize or execute migration, change storage or
   scene save-root capture, restore first-patient controllers, add UI, or alter version-1/version-2
   documents.
