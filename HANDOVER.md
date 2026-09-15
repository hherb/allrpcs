# Allrpcs Handover

## Current state

`main` includes merged PR #267 (`915cd19`). Branch
`codex/phase-7-misquotation-decision-decoding` advances Phase 7 through Phase 7io. Draft PR #268 is
open for this branch:

- strict bounded JSON decoding rebinds value-matching `actorId` and a nullable zero-based candidate
  index to the request's exact actor identifier and evidence instance, then applies the existing
  current-observability policy;
- a stateful OpenAI Responses semantic-event accumulator and SSE completion expose a decision only
  after one complete ordered lifecycle, with no partial or trailing result;
- pure request content, a closed exact schema, and stateless serialization retain candidate order
  without credentials, filesystem paths, provider state, or mutable collections;
- one caller-composed OpenAI HTTP adapter validates credentials before sending, requires a successful
  event stream, returns only the exact decoded decision, and redacts transport or semantic failures;
  and
- an immutable Application binding plus Infrastructure factory retain exact OpenAI settings, HTTP,
  credential, and endpoint dependencies without construction-time input/output or model invocation.

No evidence is exposed or applied, and no dialogue is created or delivered. The work adds no retry,
deadline, fallback, model source, task, scene, persistence, transcript/truth/character/audience/event
mutation, delay, influence, movement, navigation, or settings composition. The unrelated untracked
`scripts/code_statistics.py` remains untouched and must stay unstaged.

## Validation and review

- Tests began with the expected missing decoder, stream, serializer, adapter, binding, and factory
  compile failures.
- Focused host-local tests pass: 24 decoder, 25 semantic-event/SSE, 6 serializer/schema, 11 HTTP
  adapter, 3 Application binding, and 5 factory cases.
- `dotnet format Allrpcs.sln --no-restore --verify-no-changes --verbosity minimal` passes.
- Full host-local `./scripts/quality.sh` passes workflow validation, restore, formatting, a
  zero-warning build, 2,536 Domain tests, 542 Application tests, 897 Infrastructure tests, 8
  architecture tests, scenario validation, every credential-free headless Godot smoke, and the
  final Compatibility renderer launch.
- Every new C# file is below 500 lines with no lines longer than 120 characters.
- Session-wide review confirmed exact request/actor/candidate retention, closed decoder/schema/API
  surfaces, ordered terminal stream handling, bounded output, pre-cancellation, failed-body
  avoidance, redacted failures, exact caller-owned dependency retention, and construction-time
  I/O absence. It also replaced a tautological serializer immutability assertion with exact
  request-candidate reference checks.

## Exact next bounded slice

Take **Phase 7ip: supply one exact misquotation-evidence model or a stable offline default**.

Acceptance boundary:

1. Add `TemporaryCharacterDialogueMisquotationEvidenceModelSource` with one exact configured
   `CharacterDialogueMisquotationEvidenceModelBinding`, initialized once and replaceable or
   clearable only by exact-reference compare-and-swap operations.
2. Permit one exact caller-supplied model to take precedence for the next decision only; consume
   only that one-shot model, return the configured model stably for later decisions, and otherwise
   return one stable fail-closed offline model.
3. Prove stale/value-equal replacement and clearing fail closed, clearing preserves a queued
   one-shot model, selection performs no model invocation or request mutation, caller cancellation
   precedes offline request validation, and the source exposes only the five bounded operations.
4. Add only an isolated credential-free Godot smoke. Do not compose settings into `Main.tscn`, load
   credentials, perform HTTP or model input/output, start a task, expose or apply evidence, deliver
   dialogue, present or acknowledge a result, or add persistence, input, UI, retry, timing, delay,
   influence, scheduling, events, truth mutation, transcripts, navigation, or movement.
