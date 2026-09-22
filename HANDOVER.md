# Allrpcs Handover

## Current state

`main` includes merged PR #308 at `a58375e`. Branch
`codex/phase-7-delay-recovery-stream` continues target-delay recovery through four bounded slices:

- Phase 7nj accepts only one bounded ordered OpenAI Responses semantic-event lifecycle and framed
  UTF-8 SSE stream, returning an exact request-bound decision only after verified output and response
  completion. Malformed, unknown, duplicate, out-of-order, incomplete, refused, failed, excessive,
  trailing, cancelled, and policy-invalid streams fail closed without provider detail.
- Phase 7nk executes one exact request through caller-owned OpenAI settings, credential source, HTTP
  client, and validated Responses endpoint. It validates credentials before sending, requires SSE,
  redacts provider and transport failure details, and preserves caller cancellation.
- Phase 7nl creates one exact OpenAI delay-recovery model binding from caller-owned dependencies
  without construction-time credential access, HTTP, or model invocation.
- Phase 7nm explicitly initializes, exact-CAS replaces, and exact-CAS clears only that composition's
  binding in the existing temporary delay-recovery model source, preserving queued one-shot model
  precedence and the stable offline fallback.

Nothing activates the binding from selected game settings, adds delay-recovery ownership to
`Main.tscn`, invokes a model automatically, applies or acknowledges recovery, owns authoritative
recovery state, schedules work, authors evidence, persists state, or adds input/UI, navigation,
movement, perception, dialogue, or other world behavior. The unrelated untracked
`scripts/code_statistics.py` remains untouched and must stay unstaged.

## Validation and review

- Each slice began with a focused missing-contract compile failure. The completed provider boundary
  passes 40 focused delay-recovery transport tests, the binding factory's 2 focused tests, and the
  dedicated credential-free Godot composition smoke.
- The sandbox reproduced the known VSTest local-IPC restriction and Godot `user://logs` crash;
  identical host-local focused commands passed without product changes.
- Host-local `./scripts/quality.sh` passes workflow validation, restore, formatting, a zero-warning
  build, 2,720 Domain tests, 578 Application tests, 1,160 Infrastructure tests, all 8 architecture
  tests, scenario validation, every credential-free Godot smoke, and the final Compatibility launch.
- Session review covers lifecycle ordering and terminality, event/document bounds, exact actor and
  candidate rebinding, malformed and provider-failed content redaction, credential-before-send,
  HTTP method/endpoint/media type, caller cancellation, exact dependency retention, construction
  I/O absence, composition CAS behavior, one-shot/offline preservation, closed public surfaces,
  documentation, formatting, whitespace, line limits, and scoped paths. Review found and fixed one
  metadata-after-output-completion ordering defect; its regression case passes. No unresolved
  finding remains.

## Exact next bounded slice

Take **Phase 7nn: activate the target-delay recovery binding from selected game settings**.

Acceptance boundary:

1. Test-first add the existing temporary delay-recovery model source and its inert composition to
   `TemporaryCharacterModelCompositionController`. Selected OpenAI initialization and replacement
   must install fresh exact bindings; a supported Ollama selection must clear only the installed
   binding and restore the exact stable offline fallback.
2. Add only the delay-recovery model source needed by that composition to `Main.tscn` in dependency
   order. Extend the credential-free model-composition smoke to prove unsupported Anthropic stays
   inert, OpenAI initialization/replacement retains the exact selected settings, and Ollama restores
   the same offline model without credential access, HTTP, or model invocation.
3. Preserve any queued one-shot model across binding replacement or clearing, leave ordinary startup
   inert, and keep existing ordinary and specialized provider bindings unchanged.
4. Do not yet add the delay-recovery task, consideration flow, authoritative controller, resolution
   coordinator, automatic decision work, recovery application or acknowledgement, evidence,
   persistence, or input/UI behavior to `Main.tscn`. Update README/ROADMAP, run
   `./scripts/quality.sh`, and review the complete diff.
