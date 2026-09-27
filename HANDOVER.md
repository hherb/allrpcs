# Allrpcs Handover

## Current state

This direct-main continuation closes automated save/reopen continuity for the representative
ordinary first-patient recovery loop:

- Save schema 8 retains one closed recovery stage: `not-heard`,
  `patient-testimony-heard`, or `sage-consulted`. Versions 1-7 migrate to the inert not-heard stage.
- Saves captured before preparation reset the transient activity to exact authored initial
  ingredients. A restored full batch retains its rebound exact completed preparation and can be
  administered once; a preparation already used by an administration cannot be installed again.
- Restoring untouched disease state reuses the exact authored initial snapshot only when every
  disease-state field matches. Progressed disease truth remains freshly rebound.
- Post-stabilization restore leaves testimony pending. Post-testimony and post-consultation restores
  rebuild exact one-shot runtime tokens only from the closed stage plus canonical treatment,
  administration-event, patient, sage, and Seeker provenance.
- Capture appends the exact heard runtime recovery events to fresh content-safe journal save state,
  preserving existing entries and selection without using journal text as recovery authority.
- The switch-gated main-scene proof captures and restores all five boundaries: before preparation,
  full prepared batch, immediately after stabilization, after testimony, and after consultation.
  Each state continues once or stays complete; an incompatible stage fails before scene mutation.

## Validation and review

- Post-review host-local `./scripts/quality.sh` passed formatting, a zero-warning build, 2,839 Domain,
  924 Application, 1,261 Infrastructure, and 8 architecture tests, the five-evidence scenario
  validator, every credential-free Godot smoke, and the graphical Compatibility launch.
- Focused schema, migration, strict JSON, journal-copy, canonical-token restoration, authored initial
  disease reuse, capture, restoration, and serialized-save regression coverage passed.
- Session review inspected the complete diff, strict schema shapes and migrations, exact-reference
  authority, atomic rejection, safe journal ordering, scene dependency order, line lengths, file
  sizes, documentation, and whitespace. It fixed authored initial disease identity after restore and
  stale already-open journal capture. No known issue remains inside this automated boundary.
- Restricted direct Godot execution still hits the known `user://logs` sandbox boundary; the
  identical host-local smoke and full quality run passed.

## Preserved boundaries

- No provider, hidden disease truth, conclusion inference, clue-order prescription, production UI,
  tuning change, release, signing, or remote-service behavior was added.
- Schema 8 retains only the closed recovery stage. It does not serialize dialogue text as authority,
  runtime dialogue graphs, preparation activities, positions, UI, providers, or credentials.
- Automated smokes do not prove a new player's end-to-end experience, pacing, clue visibility,
  dialogue load, disease pressure, signing, release readiness, or provider-backed behavior.
- Do not release, sign, notarize, publish a build, invoke credential-backed providers, or change
  remote services without separate authorization.

## Exact next bounded slice

Run the first instrumented fresh-save manual end-to-end Phase 8 playtest. Stop before Phase 9,
production art/audio, provider work, broad tuning, or release activity.

Acceptance boundary:

1. Launch the current graphical Compatibility build with ordinary startup settings and create one
   fresh customized Seeker. Record the chosen background and route decisions.
2. Without developer-only state injection, objective arrows, a prescribed clue order, or knowledge
   unavailable to the Seeker, complete one valid investigation route, prepare and administer the
   remedy, hear the patient testimony, and complete the optional sage consultation.
3. Exercise one real save/close/reopen boundary during that run and confirm the remaining ordinary
   steps continue once without duplicate safe journal testimony or replayed completed dialogue.
4. Record concrete observations for dead ends, clue visibility, pacing, dialogue overload, and
   disease pressure. Distinguish observed manual evidence from automated coverage.
5. If the run exposes a blocker, document the smallest reproducible next fix instead of broadening
   into general tuning. Mark the remaining Phase 8 roadmap playtest and acceptance items complete
   only if the manual evidence actually satisfies them.
