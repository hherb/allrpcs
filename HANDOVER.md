# Allrpcs Handover

## Current state

`main` contains merged PR #176 at `1171e94`. The current branch is
`codex/phase-4-live-provider-activation` and advances Phase 4 through Phase 4r1.

- Phase 4q4 atomically replaces one exact validated settings/binding pair in
  `TemporaryCharacterModelSource`. Replacement preserves one-shot precedence and affects only
  future model takes; an already-running turn retains its already-taken model.
- Phase 4q5 exposes explicit **ACTIVATE FUTURE TURNS** composition for the current supported
  in-memory selection. It performs no save, credential read, connection test, generation, or
  provider request, and a failed activation preserves the current live model.
- Phase 4r1 adds caller-composed `OpenAiEndpoint` support. The official API remains the default;
  alternatives are restricted to HTTPS or numeric HTTP loopback version-one bases, and the exact
  endpoint is shared by the model and connection tester.
- An opt-in OpenAI-compatible live probe exercises the production Responses/SSE boundary with a
  fixed non-secret local credential. Local oMLX `LFM2.5-8B-A1B-MLX-8bit` and
  `gemma-4-26b-a4b-it-4bit` both reached it but returned non-conforming output that was rejected as
  `InvalidResponse` without a response object.
- Strict diagnostic runs against local Ollama `granite4:small-h` and `granite4.1:8b-q8_0` also did
  not produce an accepted `Completed` response. Do not treat any of these live runs as Phase 4
  acceptance evidence.

No automatic live recomposition, endpoint persistence/UI, server discovery, model installation or
process ownership, Anthropic credential store, provider metadata read, generated non-wait intention
execution, or authoritative gameplay mutation was added. Native Linux/Windows credential behavior
remains unproved, and full Phase 4 acceptance is incomplete.

## Validation

The complete host-local `./scripts/quality.sh` passes:

- workflow and formatting validation, restore, and a zero-warning build;
- 1,388 Domain tests, 365 Application tests, 397 Infrastructure tests, and eight architecture tests;
- the five-evidence representative scenario validator;
- all 75 opt-in Godot smoke invocations, including exact live-binding replacement, activation, and
  active-turn isolation; and
- the bounded graphical Compatibility-renderer launch.

The opt-in oMLX Gemma safety probe passes when enabled with
`ALLRPCS_LIVE_OPENAI_API_BASE=http://127.0.0.1:8000/v1` and
`ALLRPCS_LIVE_OPENAI_MODEL=gemma-4-26b-a4b-it-4bit`; its allowed outcome was the fail-closed
`InvalidResponse` path. GitHub Actions budget is exhausted for the month: do not dispatch hosted
workflows, and treat zero-step hosted failures as account infrastructure rather than test evidence.

## Exact next bounded slice

Take **Phase 4r2: strict opt-in live conformance evidence**.

Acceptance boundary:

1. Separate live conformance from fail-closed transport safety: an explicitly enabled conformance
   check must require `Completed`, exact request identity, a target-free `Wait`, bounded healer
   speech, no commitments, and no state proposals. `InvalidResponse` is a conformance failure.
2. Run a small fixed series through one explicitly selected already-running local model and report
   only bounded aggregate outcomes. Do not retain provider content, prompts, credentials, or raw
   failure details.
3. Keep the standard suite network-free and passing when live environment variables are absent.
   Preserve the existing safety probes and strict production parser/validator without widening the
   response schema or accepting provider-specific variants.
4. Do not download or start models, persist an API base, add endpoint UI, alter automatic startup,
   execute a non-wait intention, claim natural-conversation acceptance, or add Phase 5 memory.
