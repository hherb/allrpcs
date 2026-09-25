# Allrpcs Handover

## Current state

`main` includes merged PR #366 at `871dd57`. Branch
`codex/phase-7-ordinary-turn-delay-recovery` continues the switch-gated Phase 7 acceptance path without
extending the closed repeated Move-cycle chain.

The ordinary-turn Delay path now preserves the exact generic acknowledgement, selected typed candidate,
acknowledged typed result, applied Delay result, fresh event log and world time, and scene-owned recovery
event. Four adjacent coordinators delegate the existing owners to publish one exact recovery candidate
and eligibility token, compose its exact provider-neutral request, explicitly start it once, and retain
and acknowledge its exact completed applied recovery with fresh authoritative snapshots.

Rejected null, rebuilt, foreign, replaced-authority, already-published, and repeated inputs remain inert.
Rejected recovery starts preserve the queued one-shot model; the exact path invokes the generic, Delay,
and Delay-recovery models once each. The switch-gated smoke stops before terminal routing, while ordinary
startup remains inert. `README.md`, `ROADMAP.md`, and `PHASE_7_ACCEPTANCE_AUDIT.md` record only this
evidence. Broad Phase 7 outcomes and final acceptance remain unchecked because no ordinary scheduler
invokes the path and no player-visible investigation surface exists.

Do not add a sixth or later Move candidate, author, request, task, result, recovery, terminal route, turn
completion, scheduling authorization, cycle boundary, or admission. Preserve unrelated untracked
`scripts/code_statistics.py`.

## Validation and review

- Test-first focused builds failed on each intentionally absent recovery coordinator before its
  implementation; the completed focused game build is zero-warning.
- The first focused sandboxed Godot smoke hit the known `user://logs` crash before scene load; every
  identical host-local focused run passed.
- Sandboxed `./scripts/quality.sh` reached a zero-warning build and then hit the known VSTest loopback
  `SocketException (13)`; the identical host-local command passed workflow validation, formatting, a
  zero-warning build, 2,731 Domain, 586 Application, 1,220 Infrastructure, and 8 architecture tests,
  scenario validation, every credential-free Godot smoke, and Compatibility launch.
- Complete-diff review found no partial candidate/request publication, queued-model loss, stale or
  rebuilt-token admission, authority mismatch, extra model consumption, premature terminal routing,
  acceptance overstatement, Move-chain extension, line-length issue, or whitespace error.

## Exact next bounded slice

Retain only the exact completed ordinary-turn Delay recovery as one terminal route.

Acceptance boundary:

1. Start test-first by extending the existing switch-gated ordinary-turn authoring main-scene smoke.
2. Require the exact retained generic acknowledgement, selected typed candidate, recovery request,
   completed recovery result, applied recovery, current event log and world time, and canonical `Delay`
   kind.
3. Delegate only `CharacterSabotageTerminalRouteCoordinator.TryRetain`; retain its exact author, kind,
   and applied Delay recovery once.
4. Reject null, rebuilt, foreign, stale, replaced-authority, already-retained, wrong-kind, and repeated
   inputs without changing retained owners or consuming another model.
5. Stop before terminal reset, turn completion, scheduling, cadence, provider work, persistence, UI, or
   presentation.
6. Keep ordinary startup inert, keep all three ordinary-turn model-call counts at one, and add no sixth
   or later Move-cycle seam.
7. Update `README.md`, `ROADMAP.md`, and `PHASE_7_ACCEPTANCE_AUDIT.md` only for evidence actually added;
   leave broad acceptance items unchecked.
8. Run `./scripts/quality.sh`, review the complete diff, preserve `scripts/code_statistics.py`, and leave
   one exact next acceptance slice.
