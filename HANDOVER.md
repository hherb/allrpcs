# Allrpcs Handover

## Current state

`main` contains merged PR #163 at `affb762`. The current branch is
`codex/phase-4-healer-generation-input`, published as draft PR #164, and advances Phase 4 through
Phase 4g2. The reviewed implementation commit is `3a99394`.

- Phase 4g1 authors one stable complete temporary healer. Its identity shares the exact healer ID
  retained by the live turn source, its controller assignment is the canonical LLM type, and its
  authored background, capability, limitation, knowledge reference, and goal are minimal and
  explicit.
- Phase 4g1 also adds a temporary provider-neutral generation-input source for an exact awaiting
  healer activity. It delegates to `CharacterModelContextAssembler` and retains the exact request,
  authoritative world-object snapshot, and response-validation affordance snapshot.
- The bounded input exposes only the identified herb-patch description, the healer's referenced
  knowledge, one goal, exact recent heard speech, and exact prompt-shaped player text marked as
  untrusted. The unnoticed stone marker, display names, unreferenced knowledge, memories, hidden
  state, and targeted actions remain absent. The empty targeted-affordance snapshot leaves only the
  validation policy's inherent target-free Wait intention eligible.
- Phase 4g2 adds one stable credential-free unavailable model and an opt-in source-to-flow smoke.
  Its terminal unavailable result cancels the exact pending turn without fabricated dialogue,
  response text, transcript entries, world changes, or discovery changes, while routine healer
  movement continues.

The ordinary player-dialogue path still uses the deterministic response source. The offline model
is composed only by the opt-in smoke, and the OpenAI HTTP adapter, bounded settings, redacted
connection test and diagnostics, and macOS Keychain store remain caller-composed only. No live key,
credential access, network call, or hosted workflow was used. Anthropic/Ollama adapters,
Windows/Linux credential stores, settings UI/persistence, ordinary-turn model selection, physical
generated-intention execution, telemetry export, and gameplay persistence or mutation remain
absent. The OpenAI adapter and full Phase 4 acceptance remain incomplete.

## Validation

Both new boundaries began with expected focused compile failures. The complete host-local
`./scripts/quality.sh` passes:

- workflow and formatting validation, restore, and a zero-warning build;
- 1,388 Domain tests, 323 Application tests, 159 Infrastructure tests, and eight architecture
  tests;
- the five-evidence representative scenario validator;
- all 60 opt-in Godot smoke invocations, including exact healer input and offline unavailable-flow
  coverage; and
- the bounded graphical Compatibility-renderer launch.

The restricted sandbox still reproduces the known .NET VSTest IPC and Godot log-directory denial;
identical host-local commands pass. GitHub Actions budget is exhausted for the month: do not
dispatch hosted workflows, and treat zero-step hosted failures as account infrastructure rather
than test evidence.

## Exact next bounded slice

Take **Phase 4g3: route ordinary heard healer requests through the temporary offline model**.

Acceptance boundary:

1. Make the canonical LLM assignment select the existing temporary offline model for the ordinary
   awaiting healer turn. Build its input through
   `TemporaryCharacterModelGenerationInputSource` and start the existing one-shot generation flow
   with the exact returned request, world objects, and affordances.
2. Prove through player dialogue submission that an unavailable terminal result cancels the exact
   pending turn, shows only canonical failure status, emits no healer speech, and changes no
   transcript, world-event, world-object, discovery, examination, collection, or presentation state
   beyond the fixed model status. Routine movement must continue.
3. Keep the Phase 3 deterministic response source as explicit test scaffolding only. Adjust its
   focused smokes so their deterministic contract remains directly tested without making it the
   ordinary runtime fallback.
4. Do not construct OpenAI/Anthropic/Ollama adapters, access credentials or network, select or
   persist settings, execute generated intentions, add telemetry/persistence, or add Windows/Linux
   credential facilities.
