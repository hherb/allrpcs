# Allrpcs Handover

## Current state

`main` contains merged PR #152 at `70b5849`. The branch
`codex/phase-4-model-contracts` advances Phase 4 through Phase 4b3:

- Phase 4a1 adds one exact non-empty streamed text delta. It permits whitespace-only provider
  fragments and caps both individual and combined stream text at the 2,000 UTF-16-unit dialogue
  bound.
- Phase 4a2-a4 add closed completed/cancelled/timed-out/failed terminal outcomes, bounded
  provider-neutral failure codes, `ICharacterModel`, an exact generation request, and immutable
  stream-prefix validation. No provider DTO enters Domain state.
- Phase 4b1-b3 add bounded typed model-context entries and snapshots, project exact player dialogue
  as untrusted in-world content, and assemble it after already-filtered entries. Generation requests
  require exactly that retained text instance and reject missing, duplicate, changed, or distinct
  value-equal replacements.

The first and third Phase 4 roadmap items are complete. No adapter, provider call, timeout executor,
retry, credential, logging, structured-output validation, Godot wiring, partial intention execution,
or gameplay/world-state mutation has been added.

## Validation

Each production boundary began with an expected focused compile failure. The final host-local
`./scripts/quality.sh` passes workflow and formatting validation, a zero-warning build, 1,381 Domain
tests, 201 Application tests, 23 Infrastructure tests, eight architecture tests, the five-evidence
representative scenario validator, all 51 opt-in Godot smokes, and the bounded graphical launch.

The restricted sandbox reproduced the known VSTest loopback denial; the identical host-local test
command passed. GitHub Actions budget is exhausted for the month: do not dispatch hosted workflows,
and treat zero-step hosted failures as account infrastructure rather than test results.

## Exact next bounded slice

Take **Phase 4b4: project only the responding character's authored goals into model context**.

Acceptance boundary:

1. Add one pure Application projection from an exact `Character` to fresh
   `CharacterModelContextEntryKind.Goal` entries in the character's stable authored goal order.
2. Retain each exact `CharacterGoal.Description` string without trimming, formatting, ranking,
   summarizing, or key disclosure; permit an empty goal set and reject a null character.
3. Prove exact text-instance retention, stable order, empty behavior, unchanged inputs, and that no
   public/private background, capability, limitation, condition, inventory, knowledge, belief,
   relationship, controller, perception, memory, speech, or affordance data is projected.
4. Do not yet combine goals with the live assembler, project perception or knowledge, define memory
   or affordance storage, call a provider, wire Godot, execute intentions, add retries/timeouts,
   persist state, handle credentials, log provider data, or mutate gameplay state.
