# Allrpcs Handover

## Current state

`main` contains merged PR #158 at `55b7dbf`. The branch
`codex/phase-4-sse-transport` advances Phase 4 through Phase 4d5:

- Phase 4d3 reads caller-owned UTF-8 OpenAI SSE streams with explicit line, frame, event, and
  document bounds. It supports LF/CRLF termination, comment heartbeats, `event:` fields, repeated
  `data:` joining, and cancellation while rejecting malformed, truncated, NUL-containing, or
  invalid-UTF-8 input without returning partial content.
- Phase 4d4 feeds complete framed documents in source order to the existing semantic accumulator.
  Exactly one complete terminal response is required; malformed framing/semantics map to redacted
  `InvalidResponse`, I/O maps to `Transport`, and caller cancellation propagates.
- Phase 4d5 adds an Infrastructure-only `OpenAiCharacterModel` over caller-owned HTTP and
  credential dependencies. It posts the existing stateless strict-schema request to the official
  Responses endpoint, requires `text/event-stream`, maps HTTP failures to provider-neutral codes,
  never reads non-success bodies, and returns one terminal Application update.

The OpenAI adapter is not runtime-composed or marked complete. No concrete OS credential store,
provider/model settings, connection test, live provider call, usage/latency logging, partial display
stream, Godot wiring, intention execution, persistence, Anthropic/Ollama adapter, or gameplay/world
mutation has been added. API keys are loaded per request through `IOpenAiCredentialSource`, bounded,
placed only on the transient authorization header, and not retained in Domain or Application state.
The HTTP client and input stream remain caller-owned.

The official OpenAI documentation checked on 2026-09-09 confirms Responses streaming uses HTTP
server-sent events and typed semantic events. See the official
[streaming Responses guide][openai-streaming] and [Responses create reference][openai-responses].

## Validation

Each boundary began with an expected focused compile failure. Focused host-local results:

- 20 SSE framing tests pass.
- 12 SSE semantic-completion tests pass.
- 18 authenticated HTTP adapter tests pass.

The complete host-local `./scripts/quality.sh` passes workflow and formatting validation, restore,
a zero-warning build, 1,388 Domain tests, 267 Application tests, 116 Infrastructure tests, eight
architecture tests, the five-evidence representative scenario validator, all 51 opt-in Godot
smokes, and the bounded graphical launch.

The restricted sandbox still reproduces the known .NET IPC/VSTest denial; identical host-local
commands pass. GitHub Actions budget is exhausted for the month: do not dispatch hosted workflows,
and treat zero-step hosted failures as account infrastructure rather than test results. No live API
request or credential-store integration was exercised.

## Exact next bounded slice

Take **Phase 4d6: add the credential-storage lifecycle boundary without composing live gameplay**.

Acceptance boundary:

1. Define one Infrastructure credential-store contract that can save, load, and delete the OpenAI
   API key with cancellation and redacted success/missing/failure outcomes. Make the existing
   `IOpenAiCredentialSource` load path reuse that contract rather than adding a second secret model.
2. Add a macOS Keychain implementation behind an injected native-keychain boundary. Require an
   exact service/account identity, keep secret bytes out of arguments, environment variables, and
   logs, bound all inputs/outputs, and map OS details to redacted outcomes.
3. Prove save/load/delete, missing key, replacement, cancellation, bounded key handling,
   native-boundary safety, no shell invocation, no logs, no save/scene/model-context persistence, and
   unchanged unrelated keychain entries using a deterministic fake command boundary. Do not require
   a real user keychain in the standard local suite.
4. Do not add Windows Credential Manager or Linux Secret Service yet; expose credentials in UI or
   diagnostics; choose a production model; perform a live provider call; add settings/connection
   tests; mark the OpenAI adapter complete; wire Godot; execute intentions; persist gameplay state;
   or add Anthropic/Ollama.

[openai-streaming]: https://developers.openai.com/api/docs/guides/streaming-responses
[openai-responses]: https://developers.openai.com/api/reference/cli/resources/responses/methods/create
