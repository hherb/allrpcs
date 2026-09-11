# Allrpcs Handover

## Current state

`main` contains merged PR #197 at `dc3c999`. The current branch is
`codex/phase-6-scene-initialization`.

This branch completes three bounded Phase 6 scene-integration slices.

Phase 6y3 adds `TemporaryFirstPatientTreatmentContextSource`. The ordinary scene retains its one
exact representative treatment context and initializes `FirstPatientStateController` exactly once
from that context's exact disease truth plus explicit empty prepared-batch, administration-specific
experiment, and universal experiment snapshots. Startup does not synthesize a preparation or dose.

Phase 6y4 adds `FirstPatientPreparationController`, initialized from the same context's exact remedy
recipe and ingredient snapshot. It delegates atomic recipe-input consumption, exact ordered
preparation-step progression, and exact completion to the existing Domain transitions. Invalid, repeated,
value-equal-step, out-of-order, incomplete, and post-completion calls preserve all installed state.

Phase 6y5 adds the pure `PreparedIngredientBatchSnapshotAppendTransition`. It appends one exact
completed preparation as a full batch in stable order and rejects exact duplicates or capacity
exhaustion. `FirstPatientStateController.TryAddPreparedBatch` accepts that result only when its
context retains the exact current disease truth and its preparation retains the exact authored
remedy recipe; only the prepared-batch snapshot changes on success.

These slices do not occur administration events, choose or administer a dose, automatically
stabilize the patient, execute investigation consequences, persist Phase 6 state, add scene input or
final UI, compose a provider-specific hypothesis interpreter, begin Phase 7, release, or distribute.

## Validation and review

- focused append-transition tests passed: 3/3
- focused exact scene-initialization, preparation lifecycle, batch-installation, and prior atomic
  stabilization Godot smokes passed on Godot 4.6.3 .NET
- `./scripts/quality.sh` passed cleanly:
  - workflow validation, restore, formatting, and zero-warning solution build passed
  - 1,913 Domain, 402 Application, 440 Infrastructure, and 8 architecture tests passed
  - representative discovery scenario validated 5 evidence definitions
  - all 80 credential-free Godot smokes passed
  - the bounded 60-frame Compatibility-renderer launch passed
- `git diff --check`, the practical 120-character limit, exact-identity, stable-order, immutability,
  fail-closed/no-partial-state, minimal-surface, credential-free, and network-free review passed

The restricted focused VSTest and Godot runs reproduced the known localhost-bind and `user://logs`
restrictions; the identical host-local commands passed. No hosted workflow was dispatched while the
known Actions account-budget condition remained unchanged.

## Exact next bounded slice

Take **Phase 6y6: atomic authored administration-event and first-patient stabilization flow**.

Acceptance boundary:

1. Add one exact authored first-patient administration world event to the temporary scene source;
   retain it in the source's stable authoritative snapshot without occurring it at startup.
2. Add the smallest pure composite result/transition that preflights occurrence of that exact event,
   constructs a dosage and administration outcome from the exact retained prepared batch, and
   delegates to `FirstPatientStabilizationTransition` using the prospective latest event log. Return
   the exact new world-event log/time and all four exact first-patient result snapshots together.
3. Add one scene coordinator operation that installs the already-preflighted world-event and
   first-patient results without a second fallible boundary. Invalid, stale, repeated, unavailable,
   duplicate, full, wrong-context, or wrong-preparation calls must preserve both controllers and all
   unrelated scene state.
4. Prove the ordinary credential-free flow can prepare the exact authored remedy, install its exact
   full batch, occur exactly one authored administration event, consume exactly one bounded dose,
   stabilize the exact patient infection, and append both exact experiment records atomically. Add
   focused Domain tests and one opt-in Godot smoke; update README/HANDOVER evidence.
5. Do not add player input or final UI, interpret observed text as efficacy, execute consequences,
   persist Phase 6 state, call a provider, begin Phase 7, release, or distribute.
