# Allrpcs Handover

## Current state

`main` contains merged PR #177 at `66f42e4`. The current branch is
`codex/phase-4-live-conformance` and advances Phase 4 through Phase 4r4.

- Phase 4r2 separates the existing permissive OpenAI-compatible fail-closed safety probe from an
  explicitly enabled three-request conformance series. Conformance requires one `Completed`
  target-free `Wait` response per request, exact request identity, bounded healer speech, no
  commitments, and no state proposals. Its report contains only bounded outcome counts.
- Phase 4r3 adds a separate three-request live action series. Each response must complete with the
  expected known `examine-object` target and pass the existing provider-neutral world-object and
  current-affordance validation policy. It likewise reports only bounded aggregate counts.
- Phase 4r4 exposes the identified herb patch as the ordinary healer's sole targeted affordance.
  After validation, a generated distinct value-equal object identifier can pass through the
  existing exact response/action chain to authoritative examination and safe deterministic
  collection. The ordinary generated-action smoke proves world objects and perception remain
  unchanged, while unknown and unafforded targets still fail closed.

The two strict live series were run against the already-running local oMLX
`gemma-4-26b-a4b-it-4bit` endpoint. Each produced the bounded aggregate
`completed=0; stream-violations=0; termination-failures=3; response-violations=0`. This is negative
model conformance evidence, not a passing Phase 4 acceptance result. No provider content, prompt,
credential, or raw failure detail was retained or reported.

No response-schema widening, provider-specific parsing, model/server discovery, model installation
or process ownership, API-base persistence/UI, automatic live recomposition, Anthropic credential
store, arbitrary generated action execution, or Phase 5 memory was added. Native Linux/Windows
credential behavior remains unproved, and full Phase 4 acceptance is incomplete.

## Validation

The complete host-local `./scripts/quality.sh` passes:

- workflow and formatting validation, restore, and a zero-warning build;
- 1,388 Domain tests, 365 Application tests, 403 Infrastructure tests, and eight architecture tests;
- the five-evidence representative scenario validator;
- all 76 opt-in Godot smoke invocations, including the generated distinct-target examination and
  safe collection flow; and
- the bounded graphical Compatibility-renderer launch.

The standard suite remains network-free because all live probes return before constructing a client
unless their explicit environment switch is set. GitHub Actions budget is exhausted for the month:
do not dispatch hosted workflows, and treat zero-step hosted failures as account infrastructure
rather than test evidence.

## Exact next bounded slice

Take **Phase 4r5: strict Ollama live-conformance parity**.

Acceptance boundary:

1. Add separately enabled fixed three-request Ollama conformance series for the same target-free
   `Wait` and known-target `examine-object` contracts used by the OpenAI-compatible checks.
   `InvalidResponse` and every other non-`Completed` termination are conformance failures.
2. Reuse the provider-neutral evaluator and bounded count-only summary. Do not retain or report
   provider content, prompts, model metadata, credentials, endpoint details, or raw failures.
3. Keep the existing permissive Ollama safety probe intact and keep the standard suite network-free
   when live environment variables are absent. If an already-running explicitly selected local
   model is available, record its positive or negative aggregate without discovering, downloading,
   starting, or changing that model.
4. Do not widen the response schema or parser, add provider-specific accepted variants, persist an
   endpoint, add endpoint UI, alter startup composition, execute any generated action other than the
   already-bounded known herb-patch examination flow, claim natural-conversation acceptance, or add
   Phase 5 memory.
