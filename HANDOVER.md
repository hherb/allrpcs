# Allrpcs Handover

## Current state

`main` contains merged PR #211 at `20044f7`. The current branch is
`codex/phase-7-participation-round-task-flow`.

This branch adds two bounded Phase 7 slices:

- `CharacterTurnParticipationRoundTaskController` scene-owns at most one complete sequential
  participation-decision task, installs only a complete successful round from the Godot main
  thread, and exposes closed idle, running, completed, caller-cancelled, timed-out, and failed
  states with terminal reset;
- `TemporaryCharacterTurnParticipationModelSource` supplies one exact caller-owned participation
  model once or one stable fail-closed offline model, without settings, credentials, provider
  composition, or network access.

The Phase 7 parent items remain open. This branch does not add participation-model bindings or
provider composition; local Chat Completions, Anthropic, or Ollama participation dialects;
diagnostics; retries; ordinary dialogue-flow invocation; autonomous-budget initialization;
follow-up selection/start/completion; multi-character scene hearing/address authoring; a loop;
whispering/private conversations; independent tasks; sabotage; Phase 8 work; release; signing; or
distribution.

Draft PR #212 contains this work and is mergeable at `08ac3f6`.

## Validation and review

- test-first game builds failed on each missing scene type before implementation;
- the focused task-controller smoke passes exact successful request/result identity and order,
  main-thread-only result installation, complete preflight before model invocation, overlap and
  repeated-operation rejection, caller cancellation, timeout, detail-free failure, terminal reset,
  null and deadline validation, closed public state, and unchanged turn, budget, dialogue,
  transcript, world, model-stream, and presentation state;
- the focused model-source smoke passes exact one-shot retention, null and overlap rejection, one
  stable offline model, safe bounded task failure, closed public surface, and unchanged surrounding
  scene state;
- final host-local `./scripts/quality.sh` passes workflow validation, restore, formatting, the
  zero-warning solution build, 2,093 Domain, 411 Application, 518 Infrastructure, and 8 architecture
  tests, all 5 representative scenario definitions, the complete headless smoke matrix including
  both new credential-free checks, and the bounded Compatibility-renderer launch;
- `dotnet format Allrpcs.sln --no-restore --verify-no-changes`, `git diff --check`, practical
  500-line/120-character limits, exact-identity boundaries, main-thread ownership, cancellation
  precedence, timeout/failure classification, provider-detail non-retention, and scope review pass.

Restricted execution reproduced the known Godot `user://logs` failure before product code loaded;
the identical focused smokes passed host-local. PR #211 is verified merged at `20044f7`. All five
hosted jobs on draft PR #212 failed with `steps: []`, unchanged Actions account-budget
infrastructure state rather than product-test evidence; do not retry or dispatch hosted workflows
unless that condition changes.

## Exact next bounded slice

Take **Phase 7ai: provider-neutral participation-model bindings**.

Acceptance boundary:

1. Add one immutable Application binding that retains one canonical
   `CharacterModelProvider`, its exact matching `CharacterModelSettings`, and one exact
   `ICharacterTurnParticipationModel` without invoking it.
2. Add one non-empty immutable bounded binding snapshot plus pure provider and selected-settings
   selectors. Copy caller collections, reject nulls, duplicate providers, provider/settings
   mismatches, and unavailable selections, and retain exact binding/settings/model instances.
3. Add focused Application tests for stable order, exact and value-equal selection behavior,
   mismatched settings, invalid collections, input preservation, no model calls, and smallest public
   surfaces.
4. Do not create Infrastructure factories, load credentials, send HTTP, change existing ordinary
   `CharacterModelBinding`, install bindings into the temporary source, alter provider composition,
   wire `Main`, start or select follow-ups, initialize/advance a budget, or drive a loop.

Leave both Phase 7 parent items unticked until later slices compose the OpenAI participation model
and drive every ordinary autonomous chain through the existing bounded scene seams.
