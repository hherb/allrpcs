# Allrpcs Handover

## Current state

`main` includes merged PR #310 at `ed39385`. Branch
`codex/phase-7-delay-recovery-authority` adds Phase 7np: one isolated
`CharacterTargetSabotageDelayRecoveryController` owns an exact initialized world-event log and
qualitative-time pair. It consumes each exact accepted request at most once, requires both request
snapshot instances to remain current, installs a selected recovery's fresh snapshots and exact
result together, and leaves current authority plus any prior applied result unchanged for accepted
abstention.

Stale rebuilt/value-equal authority, repeated exact requests, uninitialized state, and null input
fail closed without changing source graphs. The controller is not in `Main.tscn` and is not connected
to the existing task or flow. Nothing observes or acknowledges completed results, schedules work,
authors candidates or evidence, persists state, or adds input/UI or other world behavior. The
unrelated untracked `scripts/code_statistics.py` remains untouched and must stay unstaged.

## Validation and review

- The slice began with a focused missing-controller compile failure; its dedicated isolated Godot
  smoke passes exact selection, later abstention with prior-result preservation, stale value-equal,
  repeated, uninitialized, null, source-immutability, and closed-surface checks.
- The sandbox reached a zero-warning build before the known VSTest loopback `SocketException (13)`;
  the identical host-local `./scripts/quality.sh` passed.
- Host validation passes workflow checks, restore, formatting, a zero-warning build, 2,720 Domain
  tests, 578 Application tests, 1,160 Infrastructure tests, all 8 architecture tests, scenario
  validation, every credential-free Godot smoke, and the Compatibility launch.
- Session review covers initialization atomicity, exact request consumption, dual authority-token
  validation, selected transition delegation, atomic snapshot/result installation, post-selection
  abstention, stale and null rejection, source immutability, public surface, documentation,
  whitespace, line limits, and scoped paths. No unresolved finding remains.

## Exact next bounded slice

Take **Phase 7nq: observe and explicitly acknowledge exact completed target-delay recovery results
in an isolated resolution coordinator**.

Acceptance boundary:

1. Test-first add `CharacterTargetSabotageDelayRecoveryResolutionCoordinator` over the existing
   recovery task and new authoritative controller. `_Process` may attempt resolution but ordinary
   idle, running, cancelled, failed, null, and already-observed states remain inert.
2. Resolve each fresh exact completed consideration result once through
   `CharacterTargetSabotageDelayRecoveryController.TryResolveConsideration`. Retain the exact
   resolved event-log, world-time, and latest-applied-result instances as acknowledgement tokens.
3. Explicit acknowledgement resets the task only while its exact observed result and all three
   resolved authority tokens remain current. Repeated, unobserved, or stale authority fails closed.
   Selected recovery changes authority once; abstention preserves both snapshots and any prior result.
4. Prove the closed coordinator surface, exact selection and abstention, one-shot observation,
   guarded reset, stale-authority rejection, and unchanged sources in a dedicated isolated Godot
   smoke. Update README/ROADMAP and run `./scripts/quality.sh`.
5. Do not yet add the controller or coordinator to `Main.tscn`, author candidates, schedule work,
   persist recovery, or add input/UI behavior.
