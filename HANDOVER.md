# Allrpcs Handover

## Current state

`main` contains merged PR #162 at `69ada0d`. The branch
`codex/phase-4-streaming-flow` is published as draft PR #163 and advances Phase 4 through Phase 4f5.

- Phase 4e3 adds a scene-owned one-shot `CharacterModelStreamController`, initialized from an
  explicit empty immutable snapshot and accepting exact updates only through the existing
  Application append transition. Rejected initialization, null updates, and post-terminal updates
  preserve state.
- Phases 4e4-4e5 add a pure text-delta projection and a separate temporary scene label. It shows
  only exact bounded `CharacterModelTextDelta` prefixes, retains no stream or terminal payload, and
  never parses or executes partial structured content. The existing status label remains
  content-free.
- Phase 4f1 adds an opt-in Application progress observer. It sees accepted text updates and only
  the final validated terminal update; retryable text-free failures remain hidden, and observer
  failure or independent cancellation cannot change generation.
- Phases 4f2-4f5 add a one-shot Godot generation flow. Observed updates cross a thread-safe queue
  and are applied to scene state only on the Godot main thread. Completed validated responses enter
  only their exact awaiting turn and can complete speech normally. V cancels both an active model
  generation and its exact turn. Timeout or provider failure cancels the pending turn without
  deterministic fallback or fabricated dialogue, while retaining canonical visible status and any
  accepted streamed text.

The ordinary player-dialogue path still uses the deterministic response source because no scene
model source or generation-input composition exists yet. The OpenAI HTTP adapter, bounded settings,
redacted connection test and diagnostics, and macOS Keychain store remain caller-composed only.
No live key, provider call, credential access, or hosted workflow was used. Anthropic/Ollama
adapters, Windows/Linux credential stores, settings UI/persistence, provider runtime composition,
physical generated-intention execution, telemetry export, and gameplay persistence or mutation
remain absent.
The OpenAI adapter and full Phase 4 acceptance remain incomplete.

## Validation

Every new boundary began with an expected focused compile, test, or smoke failure. The complete
host-local `./scripts/quality.sh` passes:

- workflow and formatting validation, restore, and a zero-warning build;
- 1,388 Domain tests, 323 Application tests, 159 Infrastructure tests, and eight architecture
  tests;
- the five-evidence representative scenario validator;
- all 58 opt-in Godot smoke invocations, including exact stream ownership, text projection, and
  completed, cancelled, timed-out, and failed generation-flow modes; and
- the bounded graphical Compatibility-renderer launch.

The restricted sandbox still reproduces the known .NET VSTest IPC and Godot log-directory denial;
identical host-local commands pass. GitHub Actions budget is exhausted for the month: do not
dispatch hosted workflows, and treat zero-step hosted failures as account infrastructure rather
than test evidence.

## Exact next bounded slice

Take **Phase 4g1: author the temporary LLM healer and its exact provider-neutral generation input
without constructing a model**.

Acceptance boundary:

1. Add one stable complete temporary healer `Character` whose exact identifier matches the live
   healer turn request and whose controller assignment is the canonical LLM type. Keep authored
   capabilities, limitations, knowledge, goals, and other components minimal and explicit.
2. Add a temporary generation-input source that uses the existing
   `CharacterModelContextAssembler` for an exact active healer turn, an explicitly filtered healer
   perception snapshot, the exact authored knowledge it references, exact recent in-world speech,
   and an exact current Wait affordance. Return the matching world-object and affordance validation
   snapshots with the request; do not infer hidden objects or actions from dialogue text.
3. Prove in focused Godot smokes that prompt-shaped player text remains the exact untrusted
   in-world entry, unavailable perception and unreferenced knowledge stay absent, all supplied
   exact instances remain unchanged, and no generation, response reception, transcript/world
   event, discovery, movement, or presentation state changes.
4. Do not construct OpenAI/Anthropic/Ollama models, access credentials or network, select or persist
   settings, start the generation flow, execute intentions, add telemetry/persistence, or add
   Windows/Linux credential facilities.
