# Allrpcs Handover

## Current state

`main` is verified at merged PR #255 (`70c0e36`). Branch
`codex/phase-7-substitution-provider-flow` advances the exact Phase 7 substitution-decision chain
through these bounded slices:

- a request-bound accumulator accepts only the supported bounded OpenAI Responses semantic
  lifecycle, validates one complete output-text document through the existing strict decoder, and
  exposes the exact accepted substitution decision only after one successful completion event;
- an asynchronous completion seam applies those events from the existing bounded SSE reader,
  rejects incomplete or trailing input, preserves caller cancellation and I/O failure identity,
  leaves the caller-owned stream open, and retains no provider text after materialization;
- a caller-composed OpenAI adapter serializes one exact request, obtains one credential only on
  explicit invocation, requires a successful event-stream response, redacts transport failure
  details, and returns only the strict request-owned decision; and
- an immutable Application binding plus Infrastructure factory retains exact OpenAI settings and
  composes the adapter over caller-owned HTTP, credential, and default or validated endpoint
  dependencies without construction-time input/output or model invocation.

The slice does not add configured model-source ownership, settings composition, scene wiring,
automatic invocation, acknowledgement, presentation, persistence, input, UI, timing, scheduling,
events, time, transcripts, navigation, actor movement, another provider, or another sabotage
effect. Authoritative world objects and every supplied request, candidate, substitution, evidence,
and scene snapshot remain unchanged. The unrelated untracked `scripts/code_statistics.py` remains
untouched and must stay unstaged.

## Validation and review

- Tests began with the expected missing substitution accumulator/completion, HTTP adapter, binding,
  and factory compile failures.
- Focused host-local checks pass 31 SSE tests, 48 substitution provider-path tests, 3 Application
  binding tests, and 5 Infrastructure factory tests. The first sandboxed VSTest run hit the known
  IPC `SocketException (13)`; the identical host-local command passed without product changes.
- Host-local `./scripts/quality.sh` passes workflow validation, restore, formatting, a zero-warning
  build, 2,455 Domain tests, 524 Application tests, 757 Infrastructure tests, 8 architecture tests,
  scenario validation, every credential-free headless Godot smoke, and the final Compatibility
  renderer launch. No live-conformance switch was enabled and no provider was invoked.
- Every new authored C# file is below 500 lines with no lines longer than 120 characters.
  `git diff --check` passes. Source-to-source comparison with the established move provider path
  and a complete changed-file review found no unresolved findings.

## Exact next bounded slice

Take **Phase 7hb: own one exact configured substitution-model binding in the existing source**.

Acceptance boundary:

1. Extend `TemporaryCharacterObjectSabotageSubstitutionModelSource` with one exact configured
   `CharacterObjectSabotageSubstitutionModelBinding`, initialized once and replaceable or clearable
   only by exact-reference compare-and-swap operations.
2. Preserve the existing one-shot supplied-model precedence, consume only that one-shot on take,
   return the configured model stably for later decisions, and retain the same stable fail-closed
   offline model when no configured or one-shot model exists.
3. Prove stale/value-equal replacement and clearing fail closed, clearing preserves any queued
   one-shot model, selection performs no model invocation or request mutation, cancellation retains
   precedence in the offline model, and the source exposes only the five bounded operations.
4. Do not compose settings into the source or `Main.tscn`; load credentials; perform HTTP or model
   I/O; start a decision; apply, recover, acknowledge, or present a substitution; or add persistence,
   input, UI, timing, scheduling, events, time, transcripts, navigation, or actor movement.
