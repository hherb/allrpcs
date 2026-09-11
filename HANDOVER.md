# Allrpcs Handover

## Current state

`main` contains merged PR #196 at `6b16d36`. The current branch is
`codex/phase-6-first-patient-atomic-dose`.

This branch completes two bounded Phase 6 integration slices.

Phase 6y1 extends first-patient stabilization over the existing prepared-batch and universal
experiment contracts. A successful transition now requires the administration dosage's exact
preparation to exist with enough remaining quantity and delegates consumption plus both journal
records to `IngredientAdministrationExperimentExecutionTransition`. Its result returns exact new
disease truth, prepared batches, administration-specific experiments, and universal experiments.
The universal record retains bounded authored procedure and observed-outcome text without deriving
efficacy. The representative recovery path records an ineffective experiment universally before
deterioration and later successful stabilization.

Phase 6y2 adds `FirstPatientStateController` to the ordinary scene. It initializes those four exact
immutable snapshots once. `TryStabilize` requires the treatment context to retain the controller's
exact current disease truth, delegates the pure Phase 6y1 transition, and installs all four result
snapshots only after complete success. Invalid, unavailable, stale, duplicate, full, and
uninitialized attempts preserve every installed snapshot.

These slices do not occur administration events, infer efficacy from observations, execute
investigation consequences, initialize authored first-patient scene state, persist Phase 6 state,
add provider-specific hypothesis composition, expose final UI, begin Phase 7, release, or
distribute.

## Validation and review

- focused stabilization and recovery tests passed, including reflection-backed API checks
- the focused first-patient controller smoke passed on Godot 4.6.3 .NET
- `./scripts/quality.sh` passed cleanly on an aggregate retry:
  - workflow validation, restore, formatting, and zero-warning solution build passed
  - 1,910 Domain, 402 Application, 440 Infrastructure, and 8 architecture tests passed
  - representative discovery scenario validated 5 evidence definitions
  - all 78 credential-free Godot smokes passed
  - the bounded 60-frame Compatibility-renderer launch passed
- `git diff --check`, the practical 120-character limit, exact-identity, immutability, redaction,
  no-partial-state, and minimal-surface review passed

The first aggregate quality run passed workflow validation, restore, formatting, the zero-warning
build, and all .NET suites, then reproduced the known transient stall before emitting the already
built scenario validator. The exact validator, all Godot smokes, and graphical launch passed
directly. One clean aggregate retry then passed end to end without a product change. The restricted
focused VSTest and Godot runs reproduced the known localhost-bind and `user://logs` restrictions;
the identical host-local commands passed.

GitHub Actions was still budget-rejected on merged PR #196: final-head run `34589507300` had five
failed jobs with `steps: []`. Treat that as account/infrastructure state, not executed test evidence,
and do not retry or dispatch hosted workflows unless the external condition changes.

## Exact next bounded slice

Take **Phase 6y3: exact authored first-patient scene initialization**.

Acceptance boundary:

1. Add one temporary source that creates and retains one exact
   `RepresentativeFirstPatientTreatmentContextSource` context for the ordinary scene.
2. Initialize `FirstPatientStateController` exactly once from that context's exact disease truth and
   explicit empty prepared-batch, administration-experiment, and universal-experiment snapshots.
   Do not synthesize a completed preparation or dose at startup.
3. Prove source/context/disease identity retention, explicit empty journals and batches, stable
   repeated access, exact controller installation, and fail-closed repeated initialization without
   changing world events, memory, movement, dialogue, model, or presentation state.
4. Keep startup credential-free and network-free. Add one focused opt-in Godot smoke, wire it into
   `scripts/smoke-godot.sh`, and update README/HANDOVER evidence.
5. Do not yet occur administration events, prepare or administer a remedy from scene input, execute
   consequences, persist Phase 6 state, compose a provider-specific hypothesis interpreter, expose
   final UI, begin Phase 7, release, or distribute.
