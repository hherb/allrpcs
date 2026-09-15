# Allrpcs Handover

## Current state

`main` is verified at merged PR #262 (`a695ac7`). Branch
`codex/phase-7-contamination-request-model` advances the exact Phase 7 contamination-decision
chain through Phase 7hu–7hw:

- a pure OpenAI Responses serializer emits one stateless streaming request with bounded trusted
  character and exact caller-ordered candidate context plus a closed schema whose only selection
  values are the request's evidence identifiers in stable order followed by explicit JSON null;
- a caller-composed OpenAI adapter obtains one credential only on explicit invocation, posts that
  exact request to the default or supplied validated Responses endpoint, requires a successful
  event stream, and returns only the request-owned decision from the existing strict SSE path; and
- an immutable Application binding plus Infrastructure factory retains exact OpenAI settings and
  composes the adapter over caller-owned HTTP, credential, and endpoint dependencies without
  construction-time input/output or model invocation.

The slice adds no model-source ownership, selected-settings composition, automatic invocation,
retry, fallback, timeout policy, scene wiring, application or recovery, presentation,
acknowledgement, persistence, input, UI, contamination contents or effects, ingredient coupling,
scheduling, events, time, transcripts, navigation, or movement. No provider was invoked. The
unrelated untracked `scripts/code_statistics.py` remains untouched and must stay unstaged.

## Validation and review

- Tests began with the expected missing contamination adapter and binding compile failures.
- Focused host-local checks pass 6 serializer tests, 11 HTTP adapter tests, 3 Application binding
  tests, and 5 Infrastructure binding-factory tests. The first sandboxed VSTest run hit the known
  IPC `SocketException (13)`; identical host-local commands passed without product changes.
- Host-local `./scripts/quality.sh` passes workflow validation, restore, formatting, a zero-warning
  build, 2,502 Domain tests, 533 Application tests, 826 Infrastructure tests, 8 architecture tests,
  scenario validation, every credential-free headless Godot smoke, and the final Compatibility
  renderer launch. No live-conformance switch was enabled and no provider was invoked.
- Every new authored C# file is below 500 lines with no lines longer than 120 characters.
  `git diff --check` passes. Source-to-source comparison with the established concealment and
  substitution provider paths plus complete changed-file review found no unresolved findings.

## Exact next bounded slice

Take **Phase 7hx: supply one exact contamination model or a stable offline default**.

Acceptance boundary:

1. Add `TemporaryCharacterObjectSabotageContaminationModelSource` with one exact configured
   `CharacterObjectSabotageContaminationModelBinding`, initialized once and replaceable or
   clearable only by exact-reference compare-and-swap operations.
2. Permit one exact caller-supplied model to take precedence for the next decision only; consume
   only that one-shot model, return the configured model stably for later decisions, and otherwise
   return one stable fail-closed offline model.
3. Prove stale/value-equal replacement and clearing fail closed, clearing preserves a queued
   one-shot model, selection performs no model invocation or request mutation, caller cancellation
   precedes offline request validation, and the source exposes only the five bounded operations.
4. Add only an isolated credential-free Godot smoke for this source. Do not compose settings into
   `Main.tscn`, load credentials, perform HTTP or model input/output, start a task, apply or recover
   contamination, present or acknowledge a result, or add persistence, input, UI, retry, timing,
   scheduling, events, time, transcripts, navigation, or movement.
