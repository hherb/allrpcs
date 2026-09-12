# Allrpcs Handover

## Current state

`main` contains merged PR #212 at `3a1f3c4`. The current branch is
`codex/phase-7-participation-bindings`.

Draft PR #213 is open and mergeable at `50a21df`; its five hosted jobs were queued when last
queried.

This branch completes four bounded Phase 7 slices:

- immutable Application participation-model bindings, a non-empty three-provider-bounded snapshot,
  and exact provider and selected-settings selectors;
- a zero-I/O OpenAI participation-binding factory over exact caller-owned dependencies;
- configured, replaceable, selectable participation bindings in the temporary scene source, with
  one-shot precedence and an explicit return to its stable offline model;
- existing provider composition now installs the exact OpenAI participation adapter and clears it
  when selecting supported ordinary providers without participation support.

Both Phase 7 parent items remain open. This branch does not invoke participation models during
composition; start a participation round from ordinary dialogue; initialize or advance an ordinary
autonomous budget; select, start, generate, speak, or complete a follow-up; add local Chat
Completions, Anthropic, or Ollama participation dialects; add diagnostics or retries; add
multi-character scene hearing/address authoring; drive a loop; add whispering/private
conversations, independent tasks, sabotage, or Phase 8 work; release; sign; or distribute.

## Validation and review

- each Application and Infrastructure contract was introduced test-first and failed on its missing
  type before implementation; the configured-source and composition smokes likewise failed on
  missing behavior before implementation;
- focused host-local suites pass 424 Application and 522 Infrastructure tests;
- focused host-local Godot smokes pass exact binding initialization, selection, replacement,
  clearing, one-shot precedence, stable offline behavior, OpenAI composition, and Ollama fallback
  without model calls, credential loads, HTTP, or unrelated scene mutation;
- final host-local `./scripts/quality.sh` passes workflow validation, restore, formatting, the
  zero-warning solution build, 2,093 Domain, 424 Application, 522 Infrastructure, and 8 architecture
  tests, all 5 representative scenario definitions, the complete headless smoke matrix, and the
  bounded Compatibility-renderer launch;
- `git diff --check`, practical 500-line/120-character limits, immutable collection copying,
  canonical-provider and exact-settings identity, fail-closed unsupported-provider behavior,
  construction-time zero-I/O, one-shot preservation, and scope review pass.

Restricted execution reproduced the known VSTest loopback-socket and Godot `user://logs` failures;
the identical focused and full validations passed host-local. Hosted Actions remained account-budget
blocked with `steps: []` through PR #212; treat that as infrastructure state and do not retry or
dispatch hosted workflows unless it changes.

## Exact next bounded slice

Take **Phase 7an: ordinary player-dialogue participation-round launch**.

Acceptance boundary:

1. Add one scene-owned coordinator that accepts the exact newly occurred player `DialogueEvent`,
   the exact scheduled LLM healer, its value-equal direct address, the participation-model source,
   and the existing bounded participation-round task controller.
2. Start at most one round for that exact event with the existing positive finite deadline, retain
   no duplicate coordinator state, and consume no model when the event has no eligible scheduled
   listener or another round/turn is active.
3. On a complete round, hand its exact result to the existing budget-gated follow-up-start seam;
   meaningful silence, offline/provider failure, timeout, cancellation, interruption, and an
   exhausted budget must start no turn and must reset only terminal round-task state.
4. Add one credential-free ordinary-flow smoke covering exact request/result/event identity,
   direct response, meaningful silence, fail-closed terminal outcomes, overlap/repeat rejection,
   player interruption, unchanged transcript/world/presentation state until a turn actually starts,
   and the smallest public surface.
5. Do not yet generate or complete the selected follow-up, advance the budget, add further scene
   characters or address parsing, alter provider/settings composition, retry decisions, drive a
   loop, or tick either Phase 7 parent item.
