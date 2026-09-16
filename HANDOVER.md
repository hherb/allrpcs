# Allrpcs Handover

## Current state

PR #286 is merged into `main` at `a1c282a`. Branch
`codex/phase-7-influence-decision` advances Phase 7 through five dependency-ordered slices:

- Phase 7km adds the provider-neutral influence decision-model interface and pure Application
  controller. It forwards one exact request and caller token to one model invocation and constructs
  the existing Domain result so invalid decisions fail closed.
- Phase 7kn retains one exact OpenAI setting and caller-composed model without invoking it.
- Phase 7ko supplies exact-instance initialized/replaced/cleared bindings, one-shot precedence,
  configured fallback, and one stable fail-closed offline model without construction-time I/O.
- Phase 7kp owns one isolated bounded decision task. It rejects overlap, supports one cancellation
  request and explicit terminal reset, and installs only an accepted completed result on the main
  thread while model, cancellation, and validation failures expose no result.
- Phase 7kq explicitly composes one already-authored exact request with the model source and task,
  preserving a queued one-shot model across rejected overlap and remaining inert at startup.

The branch changes only Application contracts/tests, isolated game nodes and smokes, the
credential-free smoke script, README, ROADMAP, and this handover. None of the new nodes are in
`Main.tscn`. It adds no OpenAI transport/decoder, binding factory, settings composition, automatic
scheduling, influence application, evidence, recovery selection, persistence, input/UI, time,
navigation, movement, perception, dialogue, or other world behavior. The unrelated untracked
`scripts/code_statistics.py` remains untouched and must stay unstaged.

## Validation and review

- Test-first host runs pass all 9 new Application controller and binding tests. Focused zero-warning
  game builds and all three new isolated model-source, task, and consideration-flow smokes pass.
- Final host-local `./scripts/quality.sh` passes workflow validation, restore, formatting, a
  zero-warning build, 2,633 Domain tests, 551 Application tests, 897 Infrastructure tests, 8
  architecture tests, scenario validation, every credential-free Godot smoke, and the final
  Compatibility renderer launch.
- Session-wide review verified exact request/token/model retention, validation through the existing
  Domain result, pre-cancellation, exception propagation, exact-instance binding CAS, one-shot
  preservation across overlap, stable offline failure, main-thread-only result installation,
  cancellation/reset state, inert startup, and unchanged request/relationship state. One weak
  self-reference smoke assertion found during review was replaced with a concrete relationship-value
  check; no unresolved finding remains.
- `git diff --check`, the 120-column check, and per-file 500-line checks pass.

## Exact next bounded slice

Take **Phase 7kr: strictly decode one provider influence decision JSON document**.

Acceptance boundary:

1. Test-first add one Infrastructure deserializer that accepts an exact
   `CharacterTargetSabotageInfluenceConsiderationRequest` and a bounded JSON document containing
   exactly `actorId` and nullable zero-based `selectedCandidateIndex`.
2. Rebind value-matching actor text to the request's exact actor identifier, resolve any index to the
   exact retained candidate, construct `CharacterTargetSabotageInfluenceDecision`, and delegate
   final acceptance to the existing Domain policy without changing request state.
3. Reject null, oversized, malformed, trailing, commented, over-depth, unknown, duplicate, missing,
   wrong-type, invalid-actor, negative, and out-of-range input through one bounded redacted argument
   error; reflection-lock the strict public surface and length bound.
4. Update README/ROADMAP, run `./scripts/quality.sh`, and review the complete diff. Do not yet add SSE
   accumulation, HTTP/OpenAI model execution, prompts/request serialization, binding factories,
   settings composition, `Main.tscn` wiring, automatic scheduling, influence application, evidence,
   recovery selection, persistence, input/UI, timing, or other world behavior.
