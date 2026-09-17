# Allrpcs Handover

## Current state

`main` includes merged PR #293 at `2e71006`. Branch
`codex/phase-7-influence-recovery-openai` advances the recovery provider path through three bounded
Infrastructure slices:

- Phase 7ll accumulates one request-bound ordered OpenAI Responses semantic-event lifecycle and
  exposes an exact decoded selection or abstention only after successful response completion.
- Phase 7lm completes one bounded framed SSE lifecycle, rejects incomplete or trailing streams, and
  preserves caller cancellation.
- Phase 7ln posts the existing stateless request through caller-owned credential, HTTP, settings,
  and endpoint dependencies, requires a successful event stream, and exposes an inert exact OpenAI
  recovery model binding without construction-time input/output.

The branch changes only Infrastructure contracts/tests, README, ROADMAP, and this handover. It does
not compose game settings, add scene nodes or `Main.tscn` wiring, schedule work, apply or acknowledge
recovery, persist relationships, author candidates or evidence, or add input/UI, elapsed time,
navigation, movement, perception, dialogue, or other world behavior. The unrelated untracked
`scripts/code_statistics.py` remains untouched and must stay unstaged.

## Validation and review

- Test-first focused runs failed on each absent contract, then all 40 recovery OpenAI serializer,
  semantic-event, SSE, HTTP-adapter, and binding-factory tests passed; the full suite also passed the
  existing strict decoder tests.
- Final host-local `./scripts/quality.sh` passes workflow validation, restore, formatting, a
  zero-warning build, 2,655 Domain tests, 560 Application tests, 1,027 Infrastructure tests, 8
  architecture tests, scenario validation, every credential-free Godot smoke, and the final
  Compatibility renderer launch.
- Session-wide review covers exact request rebinding, ordered terminal lifecycle behavior, event and
  document bounds, SSE completion and cancellation, credential-before-send behavior, HTTP/content
  validation, provider/transport redaction, exact dependency retention, closed public surfaces,
  documentation, file size, line length, and whitespace. No unresolved finding remains.

## Exact next bounded slice

Take **Phase 7lo: compose one influence-recovery model binding from explicit game settings**.

Acceptance boundary:

1. Test-first add one isolated composition seam that accepts exact already-authored OpenAI settings,
   caller-owned HTTP and credential dependencies, and the existing temporary recovery model source.
2. Create the binding through
   `OpenAiCharacterTargetSabotageInfluenceRecoveryModelBindingFactory` and install it only through
   the source's existing exact expected-instance initialize, replace, and clear operations.
3. Keep construction and ordinary startup inert: no credential load, HTTP send, model invocation,
   automatic consideration, recovery application, or acknowledgement. Prove explicit configured,
   offline, replacement, and clearing paths in one isolated credential-free smoke.
4. Update README/ROADMAP, run `./scripts/quality.sh`, and review the complete diff. Do not yet wire
   `Main.tscn` or the global model-composition controller, automatically schedule work, apply or
   acknowledge recovery, persist relationships, or add input/UI, timing, or other world behavior.
