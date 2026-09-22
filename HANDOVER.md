# Allrpcs Handover

## Current state

`main` includes merged PR #312 at `4b7e234`. Branch
`codex/phase-7-delay-recovery-request` advances the exact target-delay recovery scene chain through
five bounded slices:

- Phase 7nt adds a one-shot request owner for only the recovery-candidate source's exact current
  candidate, delayed target, caller-supplied authoritative events, event log, and qualitative time.
- Phase 7nu dependency-orders the candidate and request owners in `Main.tscn`; only the existing
  switch-gated delay smoke creates their exact state after one applied delay.
- Phase 7nv starts consideration only for that request owner's exact current request while the task
  is idle, preserving the queued model across rejected starts.
- Phase 7nw dependency-orders that exact-request flow with the existing recovery model, task,
  authority, and resolution owners. The switch-gated smoke alone proves one explicit append-only
  delay-to-recovery selection chain; ordinary startup remains inert.
- Phase 7nx adds an isolated start coordinator that preflights the complete exact-current graph
  before publishing candidate/request state, initializing recovery authority, or consuming a model.

Nothing autonomously starts recovery, chooses or authors a recovery event, creates evidence,
persists state, or adds input/UI, timing, navigation, movement, perception, dialogue, or other world
behavior. The start coordinator is not in `Main.tscn`. The unrelated untracked
`scripts/code_statistics.py` remains untouched and must stay unstaged.

## Validation and review

- Test-first builds failed only on each absent controller, and main-scene smokes failed only on each
  absent dependency-ordered node before the smallest implementations were added.
- Focused zero-warning builds, all three new isolated smokes, and the strengthened switch-gated
  main-scene delay-to-recovery smoke pass host-locally.
- The sandbox reached a zero-warning build before the known VSTest loopback
  `SocketException (13): Permission denied`; the identical host-local `./scripts/quality.sh` passed.
- Host validation passes workflow checks, restore, formatting, a zero-warning build, 2,720 Domain
  tests, 578 Application tests, 1,160 Infrastructure tests, all 8 architecture tests, scenario
  validation, every credential-free Godot smoke, and the Compatibility renderer launch.
- Session review covers exact candidate/request/result/character/event/log/time retention, unchanged
  Domain-policy delegation, one-shot ownership, queued-model preservation, task/status and complete
  preflight gates, main-scene dependency order and ordinary inertness, append-only recovery, source
  immutability, closed surfaces, documentation, file and line limits, whitespace, and scoped paths.
  No unresolved finding remains.

## Exact next bounded slice

Take **Phase 7ny: own the delay-recovery start coordinator inertly in the main scene**.

Acceptance boundary:

1. Test-first dependency-order `CharacterTargetSabotageDelayRecoveryStartCoordinator` after its
   exact candidate, request, recovery authority, task, model-source, and exact-request flow
   dependencies in `Main.tscn`; ordinary startup must remain uninitialized and inert.
2. Route only the existing switch-gated target-delay smoke through `TryStart` after its exact delay
   selection, supplying the already authored authoritative world events, exact current applied
   delay result, and caller-authored recovery event.
3. Prove the coordinator publishes the exact candidate/request graph, initializes recovery from
   the exact delay event-log/time pair, consumes the queued model once, applies and acknowledges one
   append-only recovery, and leaves the original delay authority plus all request sources unchanged.
4. Reject repeated or occupied start without replacing state or consuming another model; update
   README/ROADMAP, run `./scripts/quality.sh`, and review the complete diff.
5. Do not trigger recovery autonomously, infer or author a recovery event or discoverable evidence,
   persist state, or add input/UI, elapsed-time, navigation, movement, perception, dialogue, or
   other world behavior.
