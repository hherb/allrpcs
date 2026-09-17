# Allrpcs Handover

## Current state

`main` includes merged PR #301 at `e14046c`. Branch
`codex/phase-7-delay-model-source` advances the character-targeted delay chain through four bounded
slices:

- Phase 7mi adds an immutable `CharacterTargetSabotageDelayModelBinding` that retains the exact
  caller-selected OpenAI settings and exact provider-neutral model without invoking it.
- Phase 7mj adds `TemporaryCharacterTargetSabotageDelayModelSource`, with exact-instance configured
  binding initialization, replacement, and clearing; one-shot precedence; configured fallback; and
  one stable fail-closed offline model.
- Phase 7mk adds an isolated `CharacterTargetSabotageDelayTaskController`. It rejects overlap,
  supports one cancellation request and explicit terminal reset, and installs only an accepted
  completed result on the main thread while model, cancellation, and validation failures expose no
  result.
- Phase 7ml adds an explicit isolated consideration flow that composes one already-authored exact
  request with the model source and task, preserves a queued one-shot across rejected overlap, and
  remains inert at startup.

Implementation commit `8588c94` is published in draft PR #302 targeting `main`.

The branch changes only one Application binding and test, isolated game nodes and smokes, the
credential-free smoke script, README, ROADMAP, and this handover. None of the new nodes are in
`Main.tscn`. It adds no provider decoder or transport, binding factory, settings composition,
automatic scheduling, delay application or recovery, evidence, persistence, input/UI, navigation,
movement, perception, dialogue, or other world behavior. The unrelated untracked
`scripts/code_statistics.py` remains untouched and must stay unstaged.

## Validation and review

- Test-first validation produced the expected missing-type compiler failures before the binding,
  task, and flow implementations were added.
- Focused host-local validation passes all 3 binding tests, zero-warning game builds, and all three
  isolated model-source, task, and consideration-flow smokes. Sandboxed .NET was blocked by the
  known MSBuild IPC restriction; identical host-local commands supply the test evidence.
- Final host-local `./scripts/quality.sh` passes workflow validation, restore, formatting, a
  zero-warning build, 2,687 Domain tests, 569 Application tests, 1,028 Infrastructure tests, all 8
  architecture tests, scenario validation, every credential-free Godot smoke, and the final
  Compatibility renderer launch.
- Session-wide review covers exact settings/model retention, OpenAI-only binding validation,
  exact-instance compare-and-swap, one-shot preservation, stable offline failure, cancellation,
  main-thread-only result installation, overlap rejection, terminal reset, inert startup, unchanged
  authoritative world state, public API closure, documentation, file size, line length, formatting,
  whitespace, and scoped paths. No unresolved finding remains.

## Exact next bounded slice

Take **Phase 7mm: strictly decode one provider target-delay decision JSON document**.

Acceptance boundary:

1. Test-first add one Infrastructure deserializer that accepts an exact
   `CharacterTargetSabotageDelayConsiderationRequest` and a bounded JSON document containing exactly
   `actorId` and nullable zero-based `selectedCandidateIndex`.
2. Rebind value-matching actor text to the request's exact actor identifier, resolve any index to the
   exact retained delay candidate, construct `CharacterTargetSabotageDelayDecision`, and delegate
   final acceptance to the existing Domain policy without changing request world state.
3. Reject null, oversized, malformed, trailing, commented, over-depth, unknown, duplicate, missing,
   wrong-type, invalid-actor, negative, and out-of-range input through one bounded redacted argument
   error; reflection-lock the strict public surface and length bound.
4. Update README/ROADMAP, run `./scripts/quality.sh`, and review the complete diff. Do not yet add SSE
   accumulation, HTTP/OpenAI model execution, prompts/request serialization, binding factories,
   settings composition, `Main.tscn` wiring, automatic scheduling, delay application or recovery,
   evidence, persistence, input/UI, navigation, movement, perception, dialogue, or other world
   behavior.
