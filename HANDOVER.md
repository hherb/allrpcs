# Allrpcs Handover

## Current state

`main` contains merged PR #179 at `93e27a0`. The current branch is
`codex/phase-4-live-provider-acceptance`.

Phase 4 is complete. Numeric-loopback OpenAI-compatible endpoints now use deterministic,
non-streaming Chat Completions with strict structured output and all locally supported reasoning
controls disabled. HTTPS endpoints, including official OpenAI, retain the existing Responses API
path. Both paths still feed the same bounded strict local response decoder and provider-neutral
validation boundary.

The fix also makes the action schema express its actual invariant: `wait` requires JSON `null`,
while `examine-object` and `collect-object` require a target string. The provider-independent
instructions state the exact response vocabulary and forbid invented state. Responses streams now
ignore bounded reasoning-summary lifecycle events without retaining them and treat bounded
`response.output_text.done` as authoritative over transport-specific delta rendering.

Local oMLX `LFM2.5-8B-A1B-MLX-8bit` and `Qwen3.6-35B-A3B-8bit` each passed the fixed three-request
wait and action conformance series and the ordinary Godot wait and herb-patch action flows. Native
Ollama `qwen3.5:4b-q8_0` passed the same evidence using its existing `think: false`, strict schema,
and temperature-zero request. No model output, reasoning, prompt, credential, provider metadata,
or raw failure is retained or reported.

No endpoint persistence/UI, automatic live recomposition, Anthropic credential store, generated
action beyond the existing herb-patch flow, native platform claim, or Phase 5 memory was added.

## Validation

- `dotnet format Allrpcs.sln --no-restore --verify-no-changes`
- complete Infrastructure suite: 414 tests
- OpenAI-compatible LFM: three wait, three action, ordinary wait, and ordinary action passes
- OpenAI-compatible Qwen: three wait, three action, ordinary wait, and ordinary action passes
- native Ollama Qwen: three wait, three action, ordinary wait, and ordinary action passes
- complete host-local `./scripts/quality.sh`, including the representative scenario validator, all
  standard Godot smoke checks, and bounded Compatibility-renderer launch

The first sandboxed VSTest and Godot attempts can hit the known local-socket and user-log
restrictions; retry only the identical host command. GitHub Actions budget was exhausted for the
prior draft PR. Do not dispatch hosted workflows solely to compensate for that account
infrastructure state; jobs with zero steps are not test evidence.

## Exact next bounded slice

Take **Phase 5a: bounded recent local conversation window contracts**.

Acceptance boundary:

1. Define the smallest provider-neutral immutable per-character recent-conversation window needed
   to retain completed local dialogue turns, with explicit count/text bounds, exact character and
   turn identity, deterministic ordering, and copied read-only collections.
2. Add contract and policy tests for empty, append, eviction-at-bound, invalid identity/text, and
   immutability behavior. Do not integrate persistence, retrieval, provider context, or Godot scene
   state in this slice.
3. Update `README.md`, `ROADMAP.md`, and this handover after the complete quality spine passes.
4. Do not add beliefs, relationships, promises, task tracking, summarization, embeddings, semantic
   retrieval, provider-side conversation state, UI, migration, or native-platform claims.
