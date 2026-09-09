# Allrpcs Handover

## Current state

`main` contains merged PR #165 at `4cd236a`. The current branch is
`codex/phase-4-scripted-ordinary-success` and advances Phase 4 through Phase 4h3.

- Phase 4h1 replaces `Main`'s concrete offline-model dependency with
  `TemporaryCharacterModelSource`. It consumes one exact caller-supplied `ICharacterModel` for the
  next generation only, then returns to one retained credential-free unavailable default.
- A scripted ordinary-path smoke submits player dialogue, completes one exact validated target-free
  `Wait` response, records the healer speech once, completes the exact turn, and proves a later
  default-offline request resets and fails closed.
- Phase 4h2 submits prompt-shaped dialogue and returns a model-completed response targeting an
  invented object. The ordinary validation boundary replaces it with a bounded `InvalidResponse`,
  cancels the exact turn, and emits no speech, deterministic fallback, physical action, or mutation.
- Phase 4h3 drives ordinary player cancellation against a waiting caller-supplied model. The exact
  pending turn and generation cancel, canonical cancelled status appears, and the accepted text
  delta remains presentation only.

The OpenAI adapter, bounded settings, redacted connection test and diagnostics, and macOS Keychain
store remain caller-composed only. No live key, credential access, network call, or hosted workflow
was used. Anthropic/Ollama adapters, Windows/Linux credential stores, settings UI/persistence,
live provider selection, physical generated-intention execution, telemetry export, and gameplay
persistence or mutation remain absent. Full Phase 4 acceptance remains incomplete.

## Validation

The complete host-local `./scripts/quality.sh` passes:

- workflow and formatting validation, restore, and a zero-warning build;
- 1,388 Domain tests, 323 Application tests, 159 Infrastructure tests, and eight architecture
  tests;
- the five-evidence representative scenario validator;
- all 63 opt-in Godot smoke invocations, including scripted ordinary success/default-offline reset,
  prompt-shaped hallucinated-target rejection, and ordinary caller cancellation; and
- the bounded graphical Compatibility-renderer launch.

The restricted sandbox reproduces the known Godot log-directory denial; identical host-local
smokes pass. GitHub Actions budget is exhausted for the month: do not dispatch hosted workflows,
and treat zero-step hosted failures as account infrastructure rather than test evidence.

## Exact next bounded slice

Take **Phase 4h4: prove safe retry and terminal provider failure through the ordinary scene seam**.

Acceptance boundary:

1. Add an opt-in credential-free scripted model smoke that submits one player dialogue through the
   ordinary path, returns one text-free retryable `Unavailable` attempt, then completes the same
   exact generation request with one validated target-free `Wait` response.
2. Prove the hidden first failure never enters the scene stream or presentation, both attempts use
   the same exact request, the healer speech occurs once, and no physical action or world,
   discovery, examination, or collection state changes.
3. In a distinct invocation, supply a scripted non-retryable provider failure and prove it runs
   once, cancels only its exact pending turn, exposes only canonical content-free status, and does
   not fabricate dialogue or deterministic fallback.
4. Do not construct OpenAI/Anthropic/Ollama adapters, access credentials or network, select or
   persist settings, execute generated non-wait intentions, add telemetry/persistence, or add
   Windows/Linux credential facilities.
