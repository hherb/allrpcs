# Allrpcs Handover

## Current state

`main` contains merged PR #166 at `d5af8f4`. The current branch is
`codex/phase-4-ordinary-provider-failures` and advances Phase 4 through Phase 4j1.

- Phases 4h4-4h7 extend credential-free ordinary-scene proof: a hidden text-free `Unavailable`
  attempt safely retries the same request; a terminal provider rejection runs once; the real
  30-second scene deadline produces canonical timeout; an unterminated stream with exposed text
  cannot retry; and two caller-composed models serve distinct consecutive turns. All unsuccessful
  paths cancel only their exact turn without fabricated dialogue, action, fallback, or mutation.
- Phases 4i1-4i3 add a caller-composed Anthropic Messages adapter. Shared prompt/schema writers keep
  trusted context separate from the exact untrusted utterance. The Anthropic schema omits provider-
  unsupported bounds while the local decoder retains them; bounded SSE lifecycle accumulation,
  terminal usage, HTTP status/transport mapping, diagnostics, `x-api-key`, and the pinned
  `anthropic-version: 2023-06-01` header remain redacted and provider-neutral above Infrastructure.
- Phase 4j1 serializes a native Ollama `/api/chat` request with system/user separation, thinking
  disabled, streaming enabled, deterministic temperature zero, and the closed bounded response
  schema. It does not yet define transport or parse NDJSON.

OpenAI and Anthropic adapters, settings, diagnostics, credential sources, and the macOS OpenAI
Keychain store remain caller-composed only. No live key, credential access, network call, local
model process, model download, or hosted workflow was used. Ollama transport, Anthropic credential
storage, Windows/Linux credential stores, settings UI/persistence, live provider selection,
generated non-wait intention execution, telemetry export, and gameplay persistence/mutation remain
absent. Full Phase 4 acceptance remains incomplete.

## Validation

The complete host-local `./scripts/quality.sh` passes:

- workflow and formatting validation, restore, and a zero-warning build;
- 1,388 Domain tests, 323 Application tests, 207 Infrastructure tests, and eight architecture
  tests;
- the five-evidence representative scenario validator;
- all 68 opt-in Godot smoke invocations, including ordinary retry/provider failure, the real
  30-second timeout, consecutive model replacement, and malformed-stream rejection; and
- the bounded graphical Compatibility-renderer launch.

The restricted sandbox reproduces the known Godot log-directory denial; identical host-local
smokes pass. GitHub Actions budget is exhausted for the month: do not dispatch hosted workflows,
and treat zero-step hosted failures as account infrastructure rather than test evidence.

## Exact next bounded slice

Take **Phase 4j2: bounded Ollama native NDJSON completion**.

Acceptance boundary:

1. Add a strict UTF-8 newline-delimited JSON reader with explicit line/document/count bounds. It
   must consume a caller-owned readable stream without closing it, reject blank/truncated,
   malformed, NUL-containing, invalid-UTF-8, or excessive input, and propagate caller cancellation.
2. Accumulate only native `/api/chat` assistant `message.content` chunks in order. Reject thinking,
   tool calls, non-assistant roles, premature or repeated `done`, unknown error/content shapes, and
   any update after termination; never expose partial structured JSON as display text.
3. Require one final `done:true` with a successful stop reason, decode the complete bounded document
   through `CharacterModelJsonResponseCompletion`, and retain only wholly present bounded
   `prompt_eval_count`, `eval_count`, and derived total usage. Map mid-stream `error` to one redacted
   provider failure and I/O to `Transport` without retaining details.
4. Do not add the HTTP adapter or configurable endpoint yet; do not start Ollama, inspect/pull a
   model, access network/credentials, wire scenes or provider selection, persist settings, execute
   generated intentions, add telemetry, or add platform credential facilities.
