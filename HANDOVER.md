# Allrpcs Handover

## Current state

`main` includes merged PR #309 at `bdcfbd4`. Branch
`codex/phase-7-delay-recovery-activation` continues target-delay recovery through two bounded
slices:

- Phase 7nn activates the existing exact OpenAI delay-recovery composition from selected game
  settings. OpenAI initialization and replacement install fresh settings-bound models; supported
  Ollama selection clears only that composition's binding and restores the same stable offline
  model. Unsupported Anthropic remains inert, and queued one-shot precedence survives replacement.
- Phase 7no dependency-orders the existing delay-recovery source, decision task, and explicit
  consideration flow in `Main.tscn`. Ordinary startup stays idle. A dedicated switch-gated smoke
  alone supplies one exact selection, observes its accepted result, resets explicitly, and proves a
  later offline failure leaves the request's immutable world state unchanged.

Nothing applies or acknowledges recovery, owns authoritative recovery state, observes completed
results automatically, schedules decision work, authors candidates or evidence, persists state, or
adds input/UI, navigation, movement, perception, dialogue, or other world behavior. The unrelated
untracked `scripts/code_statistics.py` remains untouched and must stay unstaged.

## Validation and review

- Both slices began with a focused missing-scene-contract failure and pass their credential-free
  Godot smokes after implementation.
- The sandbox reached a zero-warning build before reproducing the known VSTest loopback
  `SocketException (13): Permission denied`; the identical host-local `./scripts/quality.sh` passed.
- Host validation passes workflow checks, restore, formatting, a zero-warning build, 2,720 Domain
  tests, 578 Application tests, 1,160 Infrastructure tests, all 8 architecture tests, scenario
  validation, every credential-free Godot smoke, and the Compatibility launch.
- Session review covers exact selected-setting retention, binding initialization/replacement/clear,
  queued one-shot precedence, stable offline identity, no construction-time credential/HTTP/model
  work, scene dependency order, idle task ownership, exact request/result retention, explicit reset,
  offline failure, immutable request world state, closed exclusions, documentation, whitespace, and
  scoped paths. Review found and fixed one contradictory README ownership sentence. No unresolved
  finding remains.

## Exact next bounded slice

Take **Phase 7np: own one exact authoritative target-delay recovery world state in an isolated
scene controller**.

Acceptance boundary:

1. Test-first add `CharacterTargetSabotageDelayRecoveryController` with one-time exact
   `WorldEventLogSnapshot` and `WorldTimeSnapshot` initialization plus nullable latest applied
   recovery result.
2. Resolve each exact accepted consideration request at most once. Require the controller's exact
   current event-log and world-time instances to match the request before delegating selection to
   `CharacterTargetSabotageDelayRecoveryConsiderationTransition.Apply`.
3. An exact selection installs the returned fresh event log, world time, and exact result together.
   Accepted abstention consumes only that exact request while preserving both authoritative snapshot
   instances and the prior latest applied result. Stale, rebuilt/value-equal, uninitialized, repeated,
   and null inputs fail closed without changing source graphs.
4. Prove the closed controller surface and exact-reference behavior in a dedicated isolated Godot
   smoke. Update README/ROADMAP and run `./scripts/quality.sh`.
5. Do not yet add this authority, a result-resolution coordinator, automatic observation or
   acknowledgement, candidate authoring, persistence, scheduling, or input/UI behavior to
   `Main.tscn`.
