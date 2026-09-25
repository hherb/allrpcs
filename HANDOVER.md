# Allrpcs Handover

## Current state

`main` includes merged PR #362 at `85d1699`. Branch
`codex/phase-7-ordinary-turn-sabotage-authoring` completes the next two bounded Phase 7 acceptance
slices without extending the repeated Move-cycle chain.

`CharacterSabotageOrdinaryTurnAuthoringCoordinator` accepts only the exact current awaiting enemy-
agent turn and exact scene-owned actor. It preflights the complete graph, invokes the existing
Conceal, Move, Substitute, Contaminate, Misquote, Delay, and Influence authors in canonical order,
and composes only their exact eligible current outputs. Empty, null, rebuilt-turn, replacement-actor,
stale, already-authored, and repeated graphs remain inert without partial publication.

`TemporaryCharacterSabotageOrdinaryTurnAuthoringSource` supplies representative exact Delay and
Influence inputs only for this switch-gated proof; it is not ordinary-play authority or scheduling.
The focused graph admits exact Conceal, Delay, and Influence outputs and leaves Move, Substitute,
Contaminate, and Misquote absent because those temporary sources retain distinct exact actors.

`CharacterSabotageOrdinaryTurnConsiderationStartCoordinator` retains that exact turn and composed
request, then delegates one explicit provider-neutral generic decision through the existing task
flow. Rejected preflight does not consume the queued one-shot model. The completed result remains
unobserved and unacknowledged; no typed request, action, outcome, recovery, turn completion, cadence,
provider transport, persistence, UI, navigation, or presentation was added.

`README.md`, `ROADMAP.md`, and `PHASE_7_ACCEPTANCE_AUDIT.md` record only this evidence. The broad
Phase 7 outcomes and final acceptance remain unchecked because no ordinary scheduler calls the new
seams. Ordinary startup remains inert.

Do not add a sixth or later Move candidate, author, request, task, result, recovery, terminal route,
turn completion, scheduling authorization, cycle boundary, or admission. The repeated Move proof is
closed; continue only along the acceptance path.

The unrelated untracked `scripts/code_statistics.py` remains untouched and must stay unstaged.

## Validation and review

- Test-first evidence began with the expected missing-type build failure, followed by a zero-warning
  focused game build and the switch-gated main-scene smoke.
- The smoke proves rejected turn/actor tokens stay inert; all seven authors are invoked in canonical
  order; exact eligible and absent outputs compose once; invalid start preflight preserves the model;
  and only the exact composed request starts and completes without result observation or routing.
- Host-local `./scripts/quality.sh` passes workflow validation, formatting, a zero-warning build,
  2,731 Domain, 586 Application, 1,220 Infrastructure, and 8 architecture tests, scenario validation,
  every credential-free Godot smoke, and the Compatibility launch.
- The sandbox-only quality run reached the zero-warning build and then hit the known VSTest loopback
  `SocketException (13)`; the identical host-local command passed.
- Session review corrected the representative source name, made an all-ineligible graph return inert
  before author invocation, and added downstream current-scene revalidation before start. The focused
  smoke and full quality suite passed again afterward.
- Complete-diff review found no partial-publication path, unsupported ordinary-play claim, broad
  acceptance overstatement, Move-chain extension, stale next slice, line-length issue, or whitespace
  error. `git diff --check` passes; staged-path and final draft-PR checks remain before publication.

## Exact next bounded slice

Add one explicit ordinary-turn generic-result observation coordinator. It must retain only the exact
completed result started by `CharacterSabotageOrdinaryTurnConsiderationStartCoordinator` while the
exact turn, actor, composed request, canonical author outputs, task result, and current scene graph
remain unchanged.

Acceptance boundary:

1. Start test-first by extending the existing switch-gated ordinary-turn authoring main-scene smoke.
2. Require the exact start coordinator turn/request, exact authoring provenance, completed generic
   task, and exact task-installed result. Reject null, rebuilt, foreign, replaced, running, failed,
   stale, and repeated inputs without changing any retained owner.
3. Delegate only the existing exact completed-result observation behavior; retain one exact result
   token and leave the task completed.
4. Preserve the selected canonical kind and corresponding exact author output/candidate binding, but
   do not acknowledge or reset the result and do not compose an action-specific request.
5. Stop before typed consideration, outcome application, evidence exposure, recovery, terminal
   routing, turn completion, scheduling, cadence, provider work, persistence, UI, or presentation.
6. Keep ordinary startup inert and add no sixth or later Move-cycle seam.
7. Update `README.md`, `ROADMAP.md`, and `PHASE_7_ACCEPTANCE_AUDIT.md` only for evidence actually
   added; leave broad acceptance items unchecked.
8. Run `./scripts/quality.sh`, review the complete diff, preserve `scripts/code_statistics.py`, and
   leave one exact next acceptance slice.
