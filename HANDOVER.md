# Allrpcs Handover

## Current state

`main` includes merged PR #291 at `5ae7f78`. Branch
`codex/phase-7-influence-recovery-decision` advances the influence-recovery path through four
bounded slices in draft PR #292:

- Phase 7lf adds the provider-neutral
  `ICharacterTargetSabotageInfluenceRecoveryDecisionModel` and pure consideration controller. The
  controller forwards one exact request and caller cancellation and constructs the existing Domain
  result so invalid decisions fail closed.
- Phase 7lg adds an exact OpenAI recovery-model binding and a temporary model source with configured,
  one-shot, and stable fail-closed offline selection. Exact expected-binding replacement and
  clearing preserve any queued one-shot model.
- Phase 7lh adds one isolated recovery-decision task with idle/running/completed/cancelled/failed
  state, main-thread result installation, overlap rejection, explicit cancellation and reset, and
  fail-closed model or policy-invalid output.
- Phase 7li adds one explicit isolated consideration flow. It checks task idleness before taking the
  next model, so rejected overlap preserves a queued one-shot model; ordinary startup remains inert.

No slice applies or acknowledges recovery in scene authority, decodes provider output, writes a
schema, parses SSE, sends HTTP, composes a configured binding, wires `Main.tscn`, persists
relationships, authors recovery candidates or evidence, or adds input/UI, elapsed time, navigation,
movement, perception, dialogue, or other world behavior. The unrelated untracked
`scripts/code_statistics.py` remains untouched and must stay unstaged.

## Validation and review

- Test-first checks failed for each missing Application/game contract before implementation, then
  passed.
- Focused validation passes 6 recovery-controller Application tests, 3 recovery-binding Application
  tests, a zero-warning game build, and credential-free model-source, task, and explicit-flow Godot
  smokes.
- Final host-local `./scripts/quality.sh` passes workflow validation, restore, formatting, a
  zero-warning build, 2,655 Domain tests, 560 Application tests, 959 Infrastructure tests, 8
  architecture tests, scenario validation, every credential-free Godot smoke, and the final
  Compatibility renderer launch.
- Session-wide review covers exact request/token/model forwarding, wrong-owner and distinct-ID
  rejection, exact candidate/reason retention, settings/binding identity, stable offline behavior,
  queued one-shot precedence and preservation, main-thread completion, overlap, cancellation,
  reset, model and validation failure, inert startup, public surfaces, file size, line length,
  documentation, and whitespace. No unresolved finding remains.
- Draft PR #292 is open. Its initial five hosted jobs all completed as failures with empty step
  lists, so no hosted product test or development-build step ran; this is Actions/account
  infrastructure state. Re-query the final documentation head before relying on hosted evidence.

## Exact next bounded slice

Take **Phase 7lj: strictly decode one provider-neutral influence-recovery decision**.

Acceptance boundary:

1. Test-first add one Infrastructure JSON schema writer for a closed object containing the exact
   actor text and nullable zero-based selected recovery-candidate index, bounded to the existing
   request candidate count.
2. Add one strict decoder that accepts only one complete bounded JSON object, rebinds the actor text
   and index to the exact request identifier and candidate instances, delegates final acceptance to
   `CharacterTargetSabotageInfluenceRecoveryDecisionPolicy`, and collapses malformed, unknown,
   duplicate, over-depth, oversized, out-of-range, wrong-actor, stale, foreign, or policy-invalid
   input into one redacted error.
3. Prove exact selection and abstention, distinct value-equal identifiers/candidates, boundary
   candidate counts, malformed and adversarial JSON, unchanged request/relationship/reason state,
   a closed public surface, and no credential, HTTP, streaming, task, scene, or filesystem behavior.
4. Update README/ROADMAP, run `./scripts/quality.sh`, and review the complete diff. Do not yet add SSE
   accumulation, request serialization, HTTP execution, credential access, binding composition,
   `Main.tscn` integration, recovery application/acknowledgement, persistence, UI, elapsed time,
   navigation, movement, perception, dialogue, or other world behavior.
