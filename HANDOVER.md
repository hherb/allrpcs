# Allrpcs Handover

## Current state

`main` includes merged PR #315 at `d1200ad`. Branch
`codex/phase-7-sabotage-action-composition` advances Phase 7 through three bounded slices:

- Phase 7ok composes one explicit OpenAI sabotage-action binding into the exact existing temporary
  model source. It retains its installed binding as the expected-instance token for initialize,
  replacement, and clear, and performs no construction-time credential, HTTP, or model work.
- Phase 7ol activates or replaces that exact binding from selected OpenAI game settings and clears
  only that composition's binding for supported Ollama settings. Replacement preserves a queued
  one-shot model, and clearing restores the same stable fail-closed offline model.
- Phase 7om dependency-orders the sabotage-action source, task, and explicit consideration flow in
  `Main.tscn`. Ordinary startup remains inert; only a switch-gated credential-free smoke starts one
  exact selection, resets it explicitly, and observes a later offline failure.

The chain still only selects one caller-prefiltered sabotage kind or abstains. It does not infer
action-specific eligibility, choose a target or candidate, dispatch or apply an action, observe or
acknowledge completed results automatically, schedule work, author evidence, persist state, or add
input/UI, timing, navigation, movement, perception, dialogue, or other world behavior. The unrelated
untracked `scripts/code_statistics.py` remains untouched and must stay unstaged.

## Validation and review

- Test-first focused runs failed only on the absent composition type, the missing selected-provider
  activation path, and the absent main-scene task/flow nodes before their implementations were added.
- Focused host-local runs pass a zero-warning game build, the isolated exact composition smoke, the
  selected-provider initialization/replacement/clear smoke, and the gated inert main-scene flow.
- The sandboxed full quality run reached a zero-warning build before the known VSTest loopback
  `SocketException (13): Permission denied`; the identical host-local `./scripts/quality.sh` passes
  workflow validation, restore, formatting, a zero-warning build, 2,728 Domain tests, 586
  Application tests, 1,220 Infrastructure tests, all 8 architecture tests, scenario validation,
  every credential-free Godot smoke, and the Compatibility renderer launch.
- Session-wide review covers exact initial and replacement settings retention, expected-binding
  replacement and clear, queued one-shot precedence, stable offline identity, zero construction-time
  input/output, source/task/flow dependency order, inert startup, exact request/result retention,
  explicit reset, stable offline failure, documentation, file and line limits, whitespace, and
  scoped paths. The selected-provider smoke was strengthened to prove the initial sabotage-action
  model retains the exact OpenAI setting; no unresolved finding remains.

## Exact next bounded slice

Take **Phase 7on: observe and explicitly acknowledge one completed sabotage-action selection**.

Acceptance boundary:

1. Test-first add one isolated `CharacterSabotageActionSelectionCoordinator` over the existing
   `CharacterSabotageActionTaskController`.
2. Observe each fresh exact completed consideration result at most once and expose only that exact
   observed result for a later action-specific consumer. Preserve its exact request, decision, and
   selected request-member kind; accepted abstention remains an exact observed result rather than a
   fabricated selection.
3. Permit explicit acknowledgement only for the same exact observed task result, then reset the task
   and clear the observation atomically. Keep idle, running, cancelled, failed, duplicate, stale, and
   null paths inert, and leave the complete request graph unchanged.
4. Prove selected result, abstention, duplicate observation, exact acknowledgement, and terminal
   failure behavior in one isolated credential-free smoke. Update README/ROADMAP, run
   `./scripts/quality.sh`, and review the complete diff.
5. Do not yet wire the coordinator into `Main.tscn`, observe or acknowledge automatically, construct
   candidates, infer action-specific eligibility, dispatch or apply an action, author evidence,
   persist state, or add input/UI, timing, navigation, movement, perception, dialogue, or other world
   behavior.
