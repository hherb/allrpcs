# Allrpcs Handover

## Current state

`main` includes merged PR #363 at `dca598d`. Branch
`codex/phase-7-ordinary-turn-result-observation` completes the next two bounded Phase 7 acceptance
slices without extending the repeated Move-cycle chain. Draft PR #364 carries this branch.

`CharacterSabotageOrdinaryTurnResultObservationCoordinator` accepts only the exact completed generic
selection started by `CharacterSabotageOrdinaryTurnConsiderationStartCoordinator`. It revalidates the
exact current turn, actor, composed request, task-installed result, full authoring graph, canonical
selected kind, exact Delay author output, and candidate binding before delegating the existing result
observation behavior. Null, rebuilt, foreign, running, stale, and repeated inputs remain inert.

`CharacterSabotageOrdinaryTurnResultBindingCoordinator` accepts only that exact retained observation,
delegates the existing selected-author binding, and retains the exact selected Delay author request and
candidate. It then acknowledges only the same exact bound result through the existing owner, resetting
only the completed generic task while preserving the result and selected-author provenance locally.
It does not compose or start a typed request.

`README.md`, `ROADMAP.md`, and `PHASE_7_ACCEPTANCE_AUDIT.md` record only this switch-gated evidence.
The broad Phase 7 outcomes and final acceptance remain unchecked because no ordinary scheduler invokes
the new seams. Ordinary startup remains inert.

Do not add a sixth or later Move candidate, author, request, task, result, recovery, terminal route,
turn completion, scheduling authorization, cycle boundary, or admission. The repeated Move proof is
closed; continue only along the acceptance path.

The unrelated untracked `scripts/code_statistics.py` remains untouched and must stay unstaged.

## Validation and review

- Test-first evidence began with the expected missing observation- and binding-coordinator build
  failures; each implementation then produced a zero-warning focused game build.
- The focused host-local main-scene smoke proves rejected result and binding tokens remain inert; only
  the exact task-installed Delay selection with unchanged author output and candidate is observed once;
  only that observation is bound and acknowledged; and only the completed generic task is reset.
- The sandboxed focused Godot smoke hit the known `user://logs` crash before scene load; the identical
  host-local smoke passed.
- The sandboxed `./scripts/quality.sh` reached a zero-warning build and then hit the known VSTest
  loopback `SocketException (13)`; the identical host-local command passed workflow validation,
  formatting, a zero-warning build, 2,731 Domain, 586 Application, 1,220 Infrastructure, and 8
  architecture tests, scenario validation, every credential-free Godot smoke, and Compatibility launch.
- Complete-diff review found no partial observation or acknowledgement path, lost exact-reference
  provenance, premature typed composition, unsupported ordinary-play claim, acceptance overstatement,
  Move-chain extension, stale next slice, line-length issue, or whitespace error.
- Draft PR #364 is mergeable. Its implementation-head workflow completed all five jobs as failures
  with `steps: []`; no checkout, build, test, smoke, or packaging step ran, so this is Actions/account
  infrastructure rather than product-test evidence.

## Exact next bounded slice

Compose one typed Delay consideration request from only the exact acknowledged ordinary-turn generic
result and retained selected-author provenance. Do not start that request.

Acceptance boundary:

1. Start test-first by extending the existing switch-gated ordinary-turn authoring main-scene smoke.
2. Require the exact binding coordinator acknowledgement, selected Delay author request, exact candidate
   binding, idle generic task, and unchanged complete turn, authoring, and current scene graph.
3. Delegate only the existing action-specific request composition behavior and retain its one exact
   typed Delay request with the exact actor, access, world-event, state, and candidate references.
4. Reject null, rebuilt, foreign, replaced, stale, already-composed, and repeated inputs without
   changing any retained owner.
5. Stop before starting or observing the typed task, outcome application, evidence exposure, recovery,
   terminal routing, turn completion, scheduling, cadence, provider work, persistence, UI, or
   presentation.
6. Keep ordinary startup inert and add no sixth or later Move-cycle seam.
7. Update `README.md`, `ROADMAP.md`, and `PHASE_7_ACCEPTANCE_AUDIT.md` only for evidence actually added;
   leave broad acceptance items unchecked.
8. Run `./scripts/quality.sh`, review the complete diff, preserve `scripts/code_statistics.py`, and leave
   one exact next acceptance slice.
