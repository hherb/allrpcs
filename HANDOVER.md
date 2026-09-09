# Allrpcs Handover

## Current state

`main` contains merged PR #164 at `76fbdd1`. The current branch is
`codex/phase-4-offline-ordinary-flow`, published as draft PR #165, and advances Phase 4 through
Phase 4g4. The reviewed implementation commit is `20785d6`.

- Phase 4g3 routes each ordinary heard healer request through the authored character's canonical
  LLM assignment, `TemporaryCharacterModelGenerationInputSource`, the existing generation flow,
  and the stable credential-free unavailable model.
- Each unavailable result cancels only its exact pending turn, shows canonical content-free failure
  status, emits no healer speech, and changes no world event, world object, discovery, examination,
  or collection state. Routine healer movement continues.
- The deterministic healer-response source is no longer an ordinary runtime fallback. Its focused
  Phase 3 smokes install responses directly as explicit test scaffolding.
- Phase 4g4 permits a fully observed terminal scene stream and generation flow to reset exactly
  once to fresh hidden empty state. Two distinct ordinary offline requests therefore receive
  distinct generation inputs and results and both fail closed without retaining prior model text or
  presentation.

The default ordinary model remains deliberately offline. The OpenAI HTTP adapter, bounded settings,
redacted connection test and diagnostics, and macOS Keychain store remain caller-composed only. No
live key, credential access, network call, or hosted workflow was used. Anthropic/Ollama adapters,
Windows/Linux credential stores, settings UI/persistence, live provider selection, physical
generated-intention execution, telemetry export, and gameplay persistence or mutation remain
absent. The OpenAI adapter and full Phase 4 acceptance remain incomplete.

## Validation

The repeated ordinary-flow smoke began with the expected bounded failure while the scene flow was
still one-shot. The complete host-local `./scripts/quality.sh` passes:

- workflow and formatting validation, restore, and a zero-warning build;
- 1,388 Domain tests, 323 Application tests, 159 Infrastructure tests, and eight architecture
  tests;
- the five-evidence representative scenario validator;
- all 60 opt-in Godot smoke invocations, including direct deterministic scaffolding, terminal reset,
  and two distinct ordinary offline healer turns; and
- the bounded graphical Compatibility-renderer launch.

The restricted sandbox still reproduces the known .NET VSTest IPC and Godot log-directory denial;
identical host-local commands pass. GitHub Actions budget is exhausted for the month: do not
dispatch hosted workflows, and treat zero-step hosted failures as account infrastructure rather
than test evidence.

## Exact next bounded slice

Take **Phase 4h1: prove one successful ordinary model-backed Wait response through a caller-owned
scene model seam**.

Acceptance boundary:

1. Decouple `Main` from the concrete temporary offline source through the smallest scene-owned seam
   that supplies one exact caller-composed `ICharacterModel`. Keep the stable unavailable model as
   the ordinary default; do not select settings or construct a provider.
2. Add an opt-in credential-free scripted model smoke that submits player dialogue through the
   ordinary path and returns one exact completed, validated, target-free `Wait` response.
3. Prove the exact healer speech occurs and appends once, the exact turn completes, no physical
   action or deterministic fallback runs, no hidden context is exposed, and world-event,
   world-object, discovery, examination, and collection state remain unchanged. Prove a later
   default-offline request still fails closed after terminal reset.
4. Do not construct OpenAI/Anthropic/Ollama adapters, access credentials or network, select or
   persist settings, execute generated non-wait intentions, add telemetry/persistence, or add
   Windows/Linux credential facilities.
