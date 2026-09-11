# Allrpcs Handover

## Current state

`main` contains merged PR #198 at `6c38176`. The current branch is
`codex/phase-6-administration-flow`.

This branch completes **Phase 6y6: atomic authored administration-event and first-patient
stabilization flow**.

The temporary authoritative source retains one exact advancing
`healer-administered-first-patient-remedy` event in stable order without occurring it at startup.
`FirstPatientAdministrationOccurrenceTransition` requires that exact event object and one exact
retained prepared batch, prospectively occurs the event, constructs the dosage and administration,
and delegates to `FirstPatientStabilizationTransition` using the prospective latest event log. Its
single immutable result returns exact new event-log, world-time, disease-truth, prepared-batch,
administration-experiment, and universal-experiment snapshots together.

`FirstPatientAdministrationController.TryAdminister` gathers both initialized scene controllers,
performs that one complete pure preflight, and installs the already-accepted result into both without
a second fallible boundary. The ordinary credential-free smoke prepares the exact authored recipe,
installs its full batch, occurs the exact authored administration event, consumes the exact full dose,
stabilizes the exact patient infection, and appends both exact experiment records atomically.
Replacement events or batches, unavailable quantities, stale or repeated occurrences, wrong contexts
or preparations, duplicate or full journals, and uninitialized state expose no partial result.

This slice does not add player input or final UI, interpret observed text as efficacy, execute
investigation consequences, persist Phase 6 state, call a provider, begin Phase 7, release, or
distribute.

Draft PR #199 contains this work.

## Validation and review

- 6 focused result/transition Domain tests passed
- focused temporary world-event source, initial occurrence, world-event controller, and complete
  first-patient administration Godot smokes passed on Godot 4.6.3 .NET
- `./scripts/quality.sh` passed cleanly:
  - workflow validation, restore, formatting, and zero-warning solution build passed
  - 1,919 Domain, 402 Application, 440 Infrastructure, and 8 architecture tests passed
  - the representative discovery scenario validated 5 evidence definitions
  - all 81 credential-free Godot smokes passed
  - the bounded 60-frame Compatibility-renderer launch passed
- `git diff --check`, the practical 120-character limit, exact-identity, prospective-log,
  immutability, fail-closed/no-partial-state, minimal-surface, credential-free, and network-free
  review passed

The restricted focused VSTest and Godot runs reproduced the known localhost-bind and `user://logs`
restrictions; the identical host-local commands passed. Draft PR #199 automatically triggered
credential-free run `34650547594`; all five jobs failed with `steps: []`, confirming the unchanged
Actions account-budget infrastructure condition rather than executed test failures. Do not retry or
dispatch hosted workflows unless that external condition changes.

## Exact next bounded slice

Take **Phase 6y7: immutable first-patient save-state composition and scene capture**.

Acceptance boundary:

1. Add one immutable Domain `FirstPatientSaveState` composition retaining the exact disease-truth,
   prepared-batch, administration-experiment, and universal-experiment snapshots together.
2. Add the smallest scene capture operation that returns one fresh composition only when all four
   `FirstPatientStateController` snapshots are initialized, retaining those exact snapshots without
   mutation or reinterpretation.
3. Prove exact retention, bounded immutable surface, null rejection, uninitialized failure, repeated
   fresh composition with exact nested snapshots, and unchanged world-event, preparation, discovery,
   memory, dialogue, movement, model, and presentation state with focused Domain tests and one opt-in
   Godot smoke. Update README/HANDOVER evidence.
4. Do not yet change `SaveSnapshot`, schema versions, JSON, save files, restoration, gameplay input,
   UI, consequence execution, providers, Phase 7, release, or distribution.
