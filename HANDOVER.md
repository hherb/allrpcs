# Allrpcs Handover

## Current state

`main` contains merged PR #159 at `cb13637`. The branch
`codex/phase-4-credential-store` advances Phase 4 through Phase 4d8:

- Phase 4d6 makes credential loading an explicit redacted loaded/missing/failed result and adds a
  save/load/delete store that reuses the same `IOpenAiCredentialSource` path. The macOS adapter
  targets only service `Allrpcs.OpenAI` and account `api-key` through `/usr/bin/security` behind an
  injected process boundary. Save bytes use redirected standard input rather than arguments or
  environment variables; native output is bounded and cleared; stderr and OS details are discarded.
- Phase 4d7 adds immutable Application settings with canonical OpenAI, Anthropic, and Ollama
  provider identities plus one exact bounded model name. A provider identity does not claim its
  adapter exists.
- Phase 4d8 adds a redacted OpenAI connection tester over caller-owned HTTP and credential
  dependencies. It retrieves the exact configured model, reads no response body, and returns only
  connected or an existing provider-neutral failure code. `OpenAiCharacterModel` now consumes the
  same settings value.

No real keychain or provider call is part of standard validation. Windows Credential Manager,
Linux Secret Service, settings UI/persistence, runtime composition, provider switching,
latency/usage diagnostics, partial display streaming, Godot wiring, intention execution,
Anthropic/Ollama adapters, and gameplay persistence or mutation remain absent. The OpenAI adapter
is not marked complete.

The official OpenAI model reference checked on 2026-09-09 documents
`GET /models/{model}` as the model-retrieval boundary used by the connection tester. See the
[retrieve-model reference][openai-model-retrieve].

## Validation

Every boundary began with an expected focused compile failure. Focused host-local results:

- 37 credential lifecycle, macOS adapter/native-boundary, and existing HTTP adapter tests pass.
- 16 provider/model settings and connection-result tests pass.
- 29 OpenAI connection and existing HTTP adapter tests pass.

The complete host-local `./scripts/quality.sh` passes workflow and formatting validation, restore,
a zero-warning build, 1,388 Domain tests, 283 Application tests, 146 Infrastructure tests, eight
architecture tests, the five-evidence representative scenario validator, all 51 opt-in Godot
smokes, and the bounded graphical launch.

The restricted sandbox still reproduces the known .NET IPC/VSTest denial; identical host-local
commands pass. GitHub Actions budget is exhausted for the month: do not dispatch hosted workflows,
and treat zero-step hosted failures as account infrastructure rather than test results.

## Exact next bounded slice

Take **Phase 4d9: define provider-neutral redacted generation diagnostics without adapter wiring**.

Acceptance boundary:

1. Add one immutable Application diagnostic value containing only the canonical provider,
   terminal outcome/failure code, bounded elapsed duration, and bounded input/output/total token
   counts when known. It must not retain model input, response text, model name, credentials,
   provider request/response objects, error bodies, identifiers, or gameplay state.
2. Add one caller-owned diagnostic sink contract that accepts the value with cancellation. Keep
   diagnostic failure observational: it must not redefine model-generation success or failure.
3. Prove success/failure/cancel/timeout outcomes, absent usage, zero and boundary token counts,
   inconsistent/excessive count rejection, exact canonical provider retention, cancellation, and
   redacted `ToString` behavior.
4. Do not parse OpenAI usage yet; add clocks, logging frameworks, file/network persistence, UI,
   telemetry export, runtime composition, real provider calls, partial display streaming, Godot
   wiring, intention execution, Windows/Linux credential stores, or Anthropic/Ollama adapters.

[openai-model-retrieve]: https://developers.openai.com/api/reference/cli/resources/models/methods/retrieve
