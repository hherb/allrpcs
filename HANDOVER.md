# Allrpcs Handover

## Current state

`main` includes merged PR #364 at `bf3013f`. Branch
`codex/phase-7-ordinary-turn-delay-composition` completes the next two bounded Phase 7 acceptance
slices without extending the repeated Move-cycle chain. Draft PR #365 carries this branch.

`CharacterSabotageOrdinaryTurnSpecificRequestCoordinator` accepts only the exact generic Delay result
acknowledged by `CharacterSabotageOrdinaryTurnResultBindingCoordinator`, its exact retained selected
author request, and its exact typed candidate binding. It revalidates the idle generic task, complete
ordinary turn and authoring graph, current Delay authority, and candidate eligibility before delegating
the existing action-specific composition seam. It retains the one exact typed Delay request with the
source request's exact actor, access, world-event, event-log, world-time, and candidate references.

`CharacterSabotageOrdinaryTurnSpecificRequestStartCoordinator` accepts only that exact still-current
typed request and delegates the existing action-specific start seam once. Rejected null and rebuilt
requests preserve the queued one-shot Delay model; the exact request consumes it once and leaves its
typed task completion unobserved.

`README.md`, `ROADMAP.md`, and `PHASE_7_ACCEPTANCE_AUDIT.md` record only this switch-gated evidence.
The broad Phase 7 outcomes and final acceptance remain unchecked because no ordinary scheduler invokes
the path. Ordinary startup remains inert.

Do not add a sixth or later Move candidate, author, request, task, result, recovery, terminal route,
turn completion, scheduling authorization, cycle boundary, or admission. The repeated Move proof is
closed; continue only along the acceptance path.

The unrelated untracked `scripts/code_statistics.py` remains untouched and must stay unstaged.

## Validation and review

- Test-first evidence began with the expected missing composition- and start-coordinator build failures;
  each implementation then produced a zero-warning focused game build.
- The focused sandboxed Godot smoke hit the known `user://logs` crash before scene load; the identical
  host-local smoke passed. It proves null, rebuilt, foreign, stale, already-composed, occupied, and
  repeated paths remain inert; only the exact acknowledged Delay graph composes and starts once; and the
  queued typed model is preserved across rejected start preflight.
- The sandboxed `./scripts/quality.sh` reached a zero-warning build and then hit the known VSTest
  loopback `SocketException (13)`; the identical host-local command passed workflow validation,
  formatting, a zero-warning build, 2,731 Domain, 586 Application, 1,220 Infrastructure, and 8
  architecture tests, scenario validation, every credential-free Godot smoke, and Compatibility launch.
- Complete-diff review found no stale-reference admission, partial composition/start, model-consumption
  leak, typed-task overlap, unsupported ordinary-play claim, acceptance overstatement, Move-chain
  extension, line-length issue, or whitespace error.
- Draft PR #365 is mergeable. Its implementation-head workflow completed all five jobs as failures with
  `steps: []`; no checkout, build, test, smoke, or packaging step ran, so this is Actions/account
  infrastructure rather than product-test evidence.

## Exact next bounded slice

Observe and acknowledge only the exact completed typed Delay result started by the ordinary-turn path.

Acceptance boundary:

1. Start test-first by extending the existing switch-gated ordinary-turn authoring main-scene smoke.
2. Require the exact typed start coordinator request, exact task-installed completed result, retained
   generic acknowledgement and candidate binding, and unchanged complete turn, authoring, and scene graph.
3. Delegate only the existing action-specific result observation and Delay resolution/acknowledgement
   behavior; retain its exact result, event-log, world-time, and applied-result references.
4. Reject null, rebuilt, foreign, running, replaced-authority, stale, already-observed, already-
   acknowledged, and repeated inputs without changing any retained owner or consuming another model.
5. Stop before downstream outcome binding, recovery, terminal routing, turn completion, scheduling,
   cadence, provider work, persistence, UI, or presentation.
6. Keep ordinary startup inert and add no sixth or later Move-cycle seam.
7. Update `README.md`, `ROADMAP.md`, and `PHASE_7_ACCEPTANCE_AUDIT.md` only for evidence actually added;
   leave broad acceptance items unchecked.
8. Run `./scripts/quality.sh`, review the complete diff, preserve `scripts/code_statistics.py`, and leave
   one exact next acceptance slice.
