# Allrpcs Handover

## Current state

`main` includes merged PR #294 at `0619830`. Branch
`codex/phase-7-influence-recovery-composition` advances the influence-recovery path through three
bounded game-composition slices:

- Phase 7lo creates, replaces, and clears one exact OpenAI recovery binding through the existing
  temporary model source without construction-time input/output.
- Phase 7lp activates that binding through selected OpenAI game settings, restores the stable
  offline model for Ollama, and dependency-orders the recovery source, task, and explicit flow in
  `Main.tscn` while ordinary startup remains inert.
- Phase 7lq owns one exact authoritative recovery relationship in an isolated scene controller;
  selection installs fresh numerically restored state, abstention is inert but consumed, and stale
  or repeated requests fail closed.

The branch changes only bounded game composition/controllers/smokes, README, ROADMAP, the Godot
smoke script, and this handover. It does not automatically schedule work, apply or acknowledge a
task result in `Main.tscn`, persist relationships, author candidates or evidence, add UI/input, or
change elapsed time, navigation, movement, perception, dialogue, or other world behavior. The
unrelated untracked `scripts/code_statistics.py` remains untouched and must stay unstaged.

## Validation and review

- Test-first builds failed on the absent composition and recovery-authority contracts before the
  smallest implementations were added.
- Focused host-local smokes pass exact composition, selected-provider initialization/replacement/
  clearing, inert main-scene task flow, and exact-current recovery authority.
- Final host-local `./scripts/quality.sh` passes workflow validation, restore, formatting, a
  zero-warning build, 2,655 Domain tests, 560 Application tests, 1,028 Infrastructure tests, 8
  architecture tests, scenario validation, every credential-free Godot smoke, and the final
  Compatibility renderer launch.
- Session-wide review covers exact expected-binding operations, provider switching, zero-I/O
  construction, node dependency order, inert startup, exact request consumption, abstention,
  stale/value-equal/repeated/null failure, immutable relationship state and reasons, documentation,
  line length, formatting, and whitespace. One test-precision finding was fixed by retaining the
  exact applied influence candidate while varying only the stale relationship instance; its focused
  smoke and formatter verification pass. No unresolved finding remains.

## Exact next bounded slice

Take **Phase 7lr: observe and explicitly acknowledge one completed influence-recovery result**.

Acceptance boundary:

1. Test-first add one isolated coordinator over the existing recovery task and recovery authority
   controller. Observe each fresh exact completed consideration result at most once.
2. Resolve through `CharacterTargetSabotageInfluenceRecoveryController` only while its exact
   relationship authority is current. Permit explicit task reset only while the exact resolved
   relationship and exact latest recovery result remain current.
3. Prove selected recovery, referentially inert abstention, explicit acknowledgement, duplicate
   observation, and stale-authority rejection without changing request, candidate, influence, or
   relationship source graphs.
4. Update README/ROADMAP, run `./scripts/quality.sh`, and review the complete diff. Do not yet wire
   the recovery authority/coordinator into `Main.tscn`, automatically schedule recovery, author or
   discover evidence, persist relationships, or add UI/input, timing, or other world behavior.
