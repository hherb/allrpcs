# Allrpcs Handover

## Current state

`main` includes merged PR #288 at `93e014e`. Branch
`codex/phase-7-influence-composition` advances the influence scene path through three bounded
slices:

- Phase 7kv adds an inert composition seam over one exact temporary influence model source. It
  creates bindings only through `OpenAiCharacterTargetSabotageInfluenceModelBindingFactory`, retains
  the exact installed binding as its compare-and-swap token, and supports exact initialize, replace,
  and clear paths.
- Phase 7kw composes that seam from selected game settings. OpenAI activates or replaces one exact
  caller-owned binding; unsupported replacement fails closed without changing current activation,
  while a supported local-provider replacement restores the stable offline influence model.
- Phase 7kx dependency-orders the influence model source, task, and explicit consideration flow in
  `Main.tscn`. Ordinary startup is inert; the gated credential-free smoke proves one exact explicit
  selection, reset, offline failure, and unchanged relationship state.

No slice schedules a decision, applies influence, authors evidence, selects or applies recovery,
persists relationship state, or adds input/UI, timing, navigation, movement, perception, dialogue,
or other world behavior. The unrelated untracked `scripts/code_statistics.py` remains untouched and
must stay unstaged.

## Validation and review

- Focused zero-warning game builds pass, as do the isolated composition smoke, the selected-provider
  composition smoke, and the gated main-scene influence consideration smoke.
- The initial sandboxed Godot run reproduced the known `user://logs` failure and signal 11; identical
  credential-free host runs pass without product changes.
- Session-wide review covers exact dependency retention, stale-safe expected-instance replacement
  and clearing, provider activation/offline restoration, dependency ordering, inert startup, exact
  request/candidate retention, and unchanged relationship state. No unresolved finding remains.
- Final host-local `./scripts/quality.sh` passes workflow validation, restore, formatting, a
  zero-warning build, 2,633 Domain tests, 551 Application tests, 959 Infrastructure tests, 8
  architecture tests, scenario validation, every credential-free Godot smoke, and the final
  Compatibility renderer launch.

## Exact next bounded slice

Take **Phase 7ky: own one exact authoritative influence relationship transition**.

Acceptance boundary:

1. Test-first add one isolated scene controller that initializes exactly one current
   `CharacterRelationshipMemory` and exposes only that exact current instance plus the latest exact
   applied `CharacterTargetSabotageInfluenceResult`, if any.
2. Resolve one accepted `CharacterTargetSabotageInfluenceConsiderationResult` at most once. Require
   its request relationship to be the exact current instance; keep abstention numerically and
   referentially inert while still consuming that exact request, and install only the existing pure
   transition's fresh result for a selected candidate.
3. Reject stale, repeated, uninitialized, null, and distinct value-equivalent inputs without
   changing state. Prove exact-reference behavior and unchanged rejected sources in an isolated
   credential-free smoke.
4. Update README/ROADMAP, run `./scripts/quality.sh`, and review the complete diff. Do not yet wire
   the controller into `Main.tscn`, observe task completion automatically, schedule consideration,
   author evidence, select/apply recovery, persist relationships, or add input/UI, timing,
   navigation, movement, perception, dialogue, or other world behavior.
