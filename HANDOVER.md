# Allrpcs Handover

## Current state

`main` contains merged PR #160 at `cf2f36a`. The branch
`codex/phase-4-generation-diagnostics` advances Phase 4 through Phase 4d11:

- Phase 4d9 adds an immutable provider-neutral generation diagnostic with only canonical provider,
  terminal outcome/failure code, elapsed duration, and optional input/output/total token counts.
  Elapsed time is bounded to one hour; token counts are wholly present or absent, non-negative,
  additive, and bounded to one million. Its string form is explicitly redacted.
- The caller-owned cancellation-aware diagnostic sink is observational. A best-effort recorder
  converts ordinary sink failure or cancellation to `false` without replacing or reinterpreting
  the authoritative generation result.
- Phase 4d10 validates optional OpenAI `response.completed.response.usage`. It retains only the
  three bounded counts, discards response IDs and all other provider metadata, accepts omitted/null
  usage as unknown, and maps malformed, duplicate, fractional, inconsistent, negative, or excessive
  counts to redacted `InvalidResponse`.
- Phase 4d11 optionally instruments `OpenAiCharacterModel` with a caller-owned `TimeProvider` and
  diagnostic sink. Completed and failed provider attempts emit bounded diagnostics; sink failure
  cannot alter the exact terminal update. The existing non-diagnostic constructor remains intact.

The official OpenAI Responses reference checked on 2026-09-09 documents the completed response
usage object with `input_tokens`, `output_tokens`, and `total_tokens`. See the
[Responses create reference][openai-responses].

No real keychain or provider call is part of standard validation. Controller-synthesized
cancellation/timeout diagnostics, Windows Credential Manager, Linux Secret Service, settings UI or
persistence, logging/telemetry export, runtime composition, partial display streaming, Godot
wiring, intention execution, Anthropic/Ollama adapters, and gameplay persistence or mutation remain
absent. The OpenAI adapter is not marked complete.

## Validation

Every boundary began with an expected focused compile failure. Focused host-local results:

- 15 diagnostic value, sink, cancellation, and failure-isolation tests pass.
- 56 OpenAI HTTP adapter, terminal usage, SSE completion, and existing transport tests pass.
- The complete Application and Infrastructure projects pass 298 and 159 tests respectively.

The complete host-local `./scripts/quality.sh` passes workflow and formatting validation, restore,
a zero-warning build, 1,388 Domain tests, 298 Application tests, 159 Infrastructure tests, eight
architecture tests, the five-evidence representative scenario validator, all 51 opt-in Godot
smokes, and the bounded graphical launch.

The restricted sandbox still reproduces the known .NET IPC/VSTest denial; identical host-local
commands pass. GitHub Actions budget is exhausted for the month: do not dispatch hosted workflows,
and treat zero-step hosted failures as account infrastructure rather than test results. No live API
request or real credential-store integration was exercised.

## Exact next bounded slice

Take **Phase 4d12: record controller-synthesized cancellation and timeout diagnostics without
duplicating provider-attempt diagnostics**.

Acceptance boundary:

1. Add an opt-in Application generation-controller diagnostic boundary over caller-owned canonical
   provider, monotonic time, sink, and diagnostic cancellation. Preserve the existing controller
   entry point and return type.
2. Record exactly one diagnostic when the controller synthesizes `Cancelled` or `TimedOut`, with
   bounded elapsed duration and unknown usage. Do not emit a duplicate controller diagnostic for a
   completed or failed provider terminal update already observable at the adapter boundary.
3. Prove caller cancellation versus deadline timeout, zero/boundary/excessive elapsed clamping,
   independent diagnostic cancellation, sink/time-source failure isolation, retry behavior, and
   unchanged generation results.
4. Do not add UI, logging frameworks, file/network persistence, telemetry export, runtime
   composition, real provider calls, partial display streaming, Godot wiring, intention execution,
   Windows/Linux credential stores, or Anthropic/Ollama adapters.

[openai-responses]: https://developers.openai.com/api/reference/cli/resources/responses/methods/create
