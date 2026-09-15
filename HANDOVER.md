# Allrpcs Handover

## Current state

`main` includes merged PR #268 at `663cc19`. Branch
`codex/phase-7-misquotation-model-source` advances Phase 7 through Phase 7ir:

- `TemporaryCharacterDialogueMisquotationEvidenceModelSource` retains one exact configured OpenAI
  binding, replaces or clears it only by exact-reference comparison, consumes one caller-supplied
  model for the next decision, then falls back to the configured model or one stable fail-closed
  offline model;
- `CharacterDialogueMisquotationEvidenceTaskController` owns one cancellable decision at a time,
  installs only a complete accepted result on the Godot main thread, exposes bounded terminal
  states, and requires an explicit reset; and
- `TemporaryCharacterDialogueMisquotationEvidenceConsiderationFlowController` remains inert until
  explicitly given one exact request, takes a model only after task preflight, and preserves a
  queued one-shot model when an overlapping start is rejected.

No node was added to `Main.tscn`. No dialogue was created or delivered, no evidence was sourced,
exposed, applied, presented, or acknowledged, and no credential, HTTP, model input/output,
settings composition, persistence, UI, retry, timing, delay, influence, scheduling, event, truth,
transcript, navigation, or movement behavior was added. The unrelated untracked
`scripts/code_statistics.py` remains untouched and must stay unstaged.

## Validation and review

- Each slice began with its expected missing-type compile failure.
- The isolated model-source, task-controller, and consideration-flow Godot smokes pass.
- `dotnet format Allrpcs.sln --no-restore --verify-no-changes --verbosity minimal` passes.
- Full host-local `./scripts/quality.sh` passes workflow validation, restore, formatting, a
  zero-warning build, 2,536 Domain tests, 542 Application tests, 897 Infrastructure tests, 8
  architecture tests, scenario validation, every credential-free headless Godot smoke, and the
  final Compatibility renderer launch.
- Session-wide review found no remaining issue after checking exact binding/request/result
  retention, one-shot precedence and overlap preservation, stale/value-equal CAS rejection,
  cancellation and failure redaction, explicit reset, closed public surfaces, unchanged dialogue
  evidence, file sizes, C# line lengths, and whitespace.

## Exact next bounded slice

Take **Phase 7is: author one isolated exact misquotation-evidence source**.

Acceptance boundary:

1. Add `TemporaryCharacterDialogueMisquotationEvidenceSource` with one stable representative enemy
   agent, one exact heard source dialogue event, one exact delivered misquotation occurrence, and
   one exact non-actor witness evidence candidate.
2. Retain the same exact actor, source event, delivered event, occurrence, witness identifier, and
   candidate across repeated reads; prove the existing proposal, occurrence, evidence, and
   first-observable selection policies accept them without changing any input.
3. Expose only the bounded immutable source values and one `_Ready` operation, and add one isolated
   credential-free Godot smoke.
4. Do not add the source, model source, task, or flow to `Main.tscn`; start a task; invoke a model;
   create or deliver live scene dialogue; mutate a transcript, truth, character, evidence, event,
   world object, or perception; or add presentation, acknowledgement, credentials, HTTP, settings
   composition, persistence, input, UI, retry, timing, delay, influence, scheduling, navigation,
   or movement.
