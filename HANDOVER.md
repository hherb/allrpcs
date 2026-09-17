# Allrpcs Handover

## Current state

`main` includes merged PR #303 at `df75dcf`. Branch
`codex/phase-7-delay-provider-model` advances the character-targeted delay path through four bounded
slices:

- Phase 7mp executes one stateless target-delay request through caller-owned OpenAI credential,
  HTTP, settings, and endpoint dependencies, requires semantic SSE, and creates its exact binding
  without construction-time input/output.
- Phase 7mq explicitly initializes, replaces, or clears only one exact binding in the existing
  temporary delay model source, retaining the installed binding as its compare-and-swap token.
- Phase 7mr activates or replaces that binding from selected OpenAI game settings and restores the
  stable offline delay model for Ollama.
- Phase 7ms dependency-orders the delay model source, task, and explicit consideration flow in
  `Main.tscn`; ordinary startup is inert, while a switch-gated credential-free smoke proves one
  exact selection, explicit reset, offline failure, and unchanged authoritative world state.

Implementation commit `9ac07ee` is published in draft PR #304 targeting `main`.

The branch does not apply or recover a delay, observe task completion automatically, author
evidence, persist state, or add input/UI, navigation, movement, perception, dialogue, or other
world behavior. The unrelated untracked `scripts/code_statistics.py` remains untouched and must
stay unstaged.

## Validation and review

- Test-first focused runs recorded the expected missing adapter/factory and composition compiler
  failures plus the selected-provider smoke failure before implementation.
- Focused host-local validation passes 10 target-delay adapter/factory tests, the isolated exact
  composition smoke, selected-provider activation/replacement/clearing, and the gated main-scene
  exact consideration flow. The initial sandboxed Godot run reproduced the known `user://logs`
  failure and signal 11; the identical host-local smoke passes without product changes.
- Final host-local `./scripts/quality.sh` passes workflow validation, restore, formatting, a
  zero-warning build, 2,687 Domain tests, 569 Application tests, 1,091 Infrastructure tests, all 8
  architecture tests, scenario validation, every credential-free Godot smoke, and the final
  Compatibility renderer launch.
- Session-wide review covers credential-before-send behavior, caller cancellation, response and
  transport redaction, exact custom endpoint and dependency retention, construction-time I/O
  absence, expected-binding replacement/clearing, provider switching, node dependency order, inert
  startup, exact request/candidate retention, unchanged world state, documentation, file size, line
  length, formatting, whitespace, and scoped paths. One test-precision finding was fixed by proving
  the custom endpoint, default endpoint retention, null rejection, and exact factory overloads; no
  unresolved finding remains.
- Draft PR #304 is open, draft, and mergeable. Its five hosted jobs failed immediately with empty
  step lists, so they are Actions/account infrastructure failures rather than hosted product-test
  evidence; the complete host-local quality result above remains the executable validation.

## Exact next bounded slice

Take **Phase 7mt: own one exact authoritative target-delay transition**.

Acceptance boundary:

1. Test-first add one isolated scene controller that initializes exactly one current
   `WorldEventLogSnapshot` and `WorldTimeSnapshot` pair and exposes only those exact current
   instances plus the latest exact applied `CharacterTargetSabotageDelayResult`, if any.
2. Resolve one accepted `CharacterTargetSabotageDelayConsiderationResult` at most once. Require its
   request event log and world time to be the exact current instances; keep abstention referentially
   inert while still consuming that exact request, and install only the existing pure transition's
   fresh selected result.
3. Reject stale, repeated, uninitialized, null, and distinct value-equivalent inputs without
   changing state. Prove exact-reference behavior and unchanged rejected sources in an isolated
   credential-free smoke.
4. Update README/ROADMAP, run `./scripts/quality.sh`, and review the complete diff. Do not yet wire
   the controller into `Main.tscn`, observe task completion automatically, schedule consideration,
   recover elapsed time, author evidence, persist state, or add input/UI, navigation, movement,
   perception, dialogue, or other world behavior.
