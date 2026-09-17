# Allrpcs Handover

## Current state

`main` includes merged PR #287 at `911a9dc`. Branch `codex/phase-7-influence-openai` advances the
provider path through four dependency-ordered slices:

- Phase 7kr strictly decodes one bounded closed influence-decision JSON document, rebinds actor text
  and a nullable candidate index to exact request instances, and delegates final acceptance to the
  existing Domain policy.
- Phase 7ks accumulates one bounded ordered OpenAI Responses semantic stream and completes framed
  SSE only after an accepted exact selection or abstention.
- Phase 7kt serializes one stateless streaming structured-output request with bounded trusted actor,
  relationship, and candidate context plus a closed exact decision schema.
- Phase 7ku executes that request through caller-owned OpenAI credential, HTTP, settings, and
  endpoint dependencies, and creates an exact binding without construction-time I/O.

The branch changes only Infrastructure contracts/tests, README, ROADMAP, and this handover. It does
not compose game settings, add scene nodes or `Main.tscn` wiring, schedule work, apply influence,
author evidence, select recovery, persist state, or add input/UI, time, navigation, movement,
perception, dialogue, or other world behavior. The unrelated untracked
`scripts/code_statistics.py` remains untouched and must stay unstaged.

## Validation and review

- Focused host runs pass 24 decoder tests, 24 stream/SSE tests, 5 request-serialization tests, and 9
  model/binding tests.
- Final host-local `./scripts/quality.sh` passes workflow validation, restore, formatting, a
  zero-warning build, 2,633 Domain tests, 551 Application tests, 959 Infrastructure tests, 8
  architecture tests, scenario validation, every credential-free Godot smoke, and the final
  Compatibility renderer launch.
- Session-wide review verified exact request-instance rebinding, Domain-policy delegation, complete
  ordered SSE lifecycle enforcement, bounded redacted failures, stable trusted-context indexes,
  credential-before-send behavior, caller cancellation, caller-owned dependency retention, and
  construction-time I/O absence. No unresolved finding remains.

## Exact next bounded slice

Take **Phase 7kv: compose one influence model binding from explicit game settings**.

Acceptance boundary:

1. Test-first add one isolated composition seam that accepts exact already-authored OpenAI settings,
   caller-owned HTTP and credential dependencies, and the existing temporary influence model source.
2. Create the binding through `OpenAiCharacterTargetSabotageInfluenceModelBindingFactory` and install
   it only through the source's existing expected-instance initialize/replace/clear operations.
3. Keep construction and ordinary startup inert: no credential load, HTTP send, model invocation,
   automatic consideration, or influence application. Prove the explicit configured/offline and
   replacement/clear paths in an isolated credential-free smoke.
4. Update README/ROADMAP, run `./scripts/quality.sh`, and review the complete diff. Do not yet add
   `Main.tscn` wiring, global model-composition-controller integration, automatic scheduling,
   influence application, evidence, recovery selection, persistence, input/UI, timing, or other
   world behavior.
