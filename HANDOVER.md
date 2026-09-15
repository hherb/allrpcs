# Allrpcs Handover

## Current state

PR #271 is merged into `main` at `66cc05a`. Branch
`codex/phase-7-misquotation-evidence-observation` advances Phase 7 through exact completed-result
observation, acknowledgement, and inert main-scene exposure ownership:

- `CharacterDialogueMisquotationEvidenceExposureCoordinator` observes each fresh exact completed
  task result once, delegates it only from the exposure controller's exact current snapshot, and
  retains the exact post-resolution snapshot required for acknowledgement;
- selection installs a fresh exposure snapshot, abstention retains the exact current instance, and
  stale, duplicate, full, idle, running, cancelled, failed, null, or repeated results are inert;
- acknowledgement resets only the exact observed completed result while its exact resolved exposure
  remains current; and
- `TemporaryCharacterDialogueMisquotationEvidenceExposureFlowController` initializes exact empty
  main-scene exposure state without starting a task. The main scene owns the exposure controller,
  initialization flow, and coordinator in dependency order.

No result is presented, discovered, corrected, or persisted. Ordinary startup remains inert. No
dialogue, truth, transcript, character, event, audience, world object, perception, or other
authoritative state is mutated. No automatic task, credential or provider I/O, input, retry, timing,
scheduling, navigation, or movement behavior was added. The unrelated untracked
`scripts/code_statistics.py` remains untouched and must stay unstaged.

## Validation and review

- Phase 7ix began with the expected missing-coordinator build failure. Its isolated credential-free
  smoke proves exact one-shot selection, abstention, acknowledgement, stale acknowledgement,
  stale-request, duplicate, capacity, idle, running, cancellation, failure, repeat, closed-surface,
  nested-reference, and unchanged-evidence behavior.
- The main-scene continuation began with the expected missing exposure-flow build failure. Its
  switch-gated credential-free smoke proves inert startup, exact source-owned selection and
  abstention, exposure identity, explicit acknowledgement, overlap preservation, stable offline
  failure, closed flows, and unchanged dialogue evidence.
- Sandboxed validation was blocked only by the known VSTest loopback restriction. The identical
  host-local `./scripts/quality.sh` passes workflow validation, restore, formatting, a zero-warning
  build, 2,546 Domain tests, 542 Application tests, 897 Infrastructure tests, 8 architecture tests,
  scenario validation, every credential-free Godot smoke, and the final Compatibility renderer
  launch.
- Session-wide review checked one-shot observation, exact current/post-resolution exposure identity,
  selection and abstention identity, acknowledgement ordering, stale and failure inertness, nested
  evidence retention, main-scene dependency order, ordinary-startup behavior, closed surfaces, file
  sizes, line lengths, formatting, and whitespace. No unresolved issue remains.

## Exact next bounded slice

Take **Phase 7iz: define one bounded witness-specific view of exposed misquotation evidence**.

Acceptance boundary:

1. Add one immutable view snapshot that defensively copies at most 64 exact exposed
   `CharacterDialogueMisquotationEvidence` entries in stable order, rejecting nulls and duplicate
   exact delivered events while allowing distinct value-equal events.
2. Add one pure projection that returns only entries whose exact `WitnessCharacterId` is the exact
   requested character identifier, retaining entry and nested occurrence/event identities and
   stable exposure order. A value-equal distinct character identifier must not match.
3. Prove exact filtering, empty output, bounds, null rejection, stable order, distinct value-equal
   allowance, and a closed immutable surface in focused Domain tests. Do not compose the view into
   Godot; present, discover, correct, consume, or persist evidence; deliver dialogue; change truth,
   transcripts, characters, events, audiences, world state, or perception; or add provider, input,
   retry, timing, scheduling, navigation, or movement behavior.
