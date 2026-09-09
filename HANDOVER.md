# Allrpcs Handover

## Current state

`main` contains merged PR #157 at `5d78e3f`. The branch
`codex/phase-4-affordance-projection` is published as draft PR #158 and advances Phase 4 through
Phase 4d2:

- Phase 4c6 projects the exact `CharacterIntentionAffordanceSnapshot` into fresh bounded model
  context entries and makes `CharacterModelContextAssembler` accept that same validation snapshot.
  Entries contain only canonical intention type and target identifier text.
- Phase 4c7-c8 require a successful model-stream termination to retain one exact complete
  provider-neutral response for the exact request. The generation controller validates that
  response against the supplied authoritative world objects and affordances; mismatched, unknown,
  unavailable, or otherwise invalid responses become response-free `InvalidResponse` failures.
- Phase 4c9 maps bounded response JSON to either an exact completed response or a redacted
  provider-neutral failure.
- Phase 4d1-d2 serialize a credential-free, stateless, streaming OpenAI Responses request with
  strict structured output and accumulate its semantic events. Exact player dialogue remains a
  separate untrusted user message. Structured JSON deltas are bounded and buffered, final text is
  checked, and refusals, failures, malformed events, unknown events, excess input, and updates
  after termination fail closed without retaining provider details.

The OpenAI adapter is not complete. No HTTP or SSE framing, endpoint, credential, settings,
connection test, usage/latency logging, live provider call, display streaming, Godot wiring,
intention execution, persistence, or gameplay/world-state mutation has been added. The request
serializer and semantic accumulator are Infrastructure-only; provider-specific values do not enter
Domain or Application state. The official OpenAI documentation checked on 2026-09-09 confirms
Responses streaming uses SSE semantic events and strict Structured Outputs uses
`text.format.type=json_schema`, requires all fields, and requires `additionalProperties:false`.
See the official [Structured Outputs guide][openai-structured-output] and
[streaming Responses guide][openai-streaming].

## Validation

Every boundary began with an expected focused compile failure. Focused host-local results:

- 14 affordance projection and context-assembler tests pass.
- 45 generation, stream, retry, and completion-contract tests pass.
- 27 response JSON decoder/completion tests pass.
- 21 OpenAI request serializer and semantic-event accumulator tests pass.

The complete host-local `./scripts/quality.sh` passes workflow and formatting validation, a
zero-warning build, 1,388 Domain tests, 267 Application tests, 66 Infrastructure tests, eight
architecture tests, the five-evidence representative scenario validator, all 51 opt-in Godot
smokes, and the bounded graphical launch.

The restricted sandbox still reproduces the known .NET IPC/VSTest denial; identical host-local
commands pass. GitHub Actions budget is exhausted for the month: do not dispatch hosted workflows,
and treat zero-step hosted failures as account infrastructure rather than test results. Draft PR
#158 is open and mergeable. All five jobs in hosted run
[`34306867890`][hosted-budget-run] failed with empty step lists without executing repository code.

## Exact next bounded slice

Take **Phase 4d3: parse bounded OpenAI server-sent-event frames into semantic event documents**.

Acceptance boundary:

1. Add one Infrastructure-only asynchronous SSE reader over a caller-owned text or byte stream.
   Support LF and CRLF framing, blank-line event termination, comment heartbeats, `event:` fields,
   repeated `data:` fields joined with the SSE newline rule, and caller cancellation.
2. Return only complete bounded JSON data documents in source order for
   `OpenAiCharacterModelStreamAccumulator`. Reject invalid UTF-8 if reading bytes, missing data,
   embedded NUL, excessive lines/events/documents, and truncated final frames without returning
   partial provider content.
3. Prove stable multiple-event order, split data, comments, cancellation, malformed fields,
   boundary lengths, unchanged caller ownership, and no credential, header, endpoint, provider
   error text, or response-body logging.
4. Do not open a network connection; add an API key or credential store; choose a production model;
   complete or mark complete the OpenAI adapter; stream raw structured JSON to UI; wire Godot;
   execute intentions; add Anthropic/Ollama; persist; or mutate gameplay state.

[openai-streaming]: https://developers.openai.com/api/docs/guides/streaming-responses
[openai-structured-output]: https://developers.openai.com/api/docs/guides/structured-outputs
[hosted-budget-run]: https://github.com/hherb/allrpcs/actions/runs/34306867890
