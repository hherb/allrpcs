# Allrpcs Handover

## Current state

`main` includes merged PR #333 at `c33ef3a`. Branch
`codex/phase-7-sabotage-scheduling-authorization` completes **Phase 7qa**,
**Phase 7qb**, and **Phase 7qc**. The unrelated untracked `scripts/code_statistics.py`
remains untouched and must stay unstaged.

`CharacterSabotageOrdinaryTurnSchedulingAuthorizationCoordinator` retains one fresh
authorization only from the exact still-current admission and ordinary-turn boundary.
The token contains those exact references, including an empty slot, without copying
actor, request, action kind, or route output.

`CharacterSabotageCycleBoundaryCoordinator` consumes only that coordinator's exact
current authorization once at an explicit sabotage-cycle boundary. Its fresh boundary
token retains the authorization itself and no copied provenance.

`CharacterSabotageCycleStartEligibilityPolicy` purely revalidates the exact current
boundary, authorization, admission, and ordinary-turn graph immediately before future
action authoring. Rebuilt, stale, changed-slot, and exact-active-slot graphs fail closed.
Ordinary startup remains inert. Nothing in this branch reserves a turn; clears a prior
owner; authors, starts, observes, acknowledges, dispatches, applies, resets, or schedules
an action; or claims ordinary enemy-agent sabotage is enabled.

## Validation and review

- Test-first game builds failed only for the deliberately missing Phase 7qa authorization
  coordinator, Phase 7qb cycle-boundary coordinator, and Phase 7qc pure eligibility policy
  before each implementation was added.
- The focused switch-gated Move proof passes host-local through exact recovery, terminal
  reset, turn completion, scheduling admission, empty ordinary-turn binding, one-shot
  authorization, explicit cycle-boundary entry, and pure start revalidation. Null, rebuilt,
  stale, changed-slot, exact-active-slot, malformed-provenance, and repeated graphs remain
  inert while the ordinary turn and every prior owner remain unchanged.
- Sandboxed Godot still hits the known `user://logs` permission crash, and sandboxed
  VSTest still cannot bind its local communication socket. The identical focused and full
  commands pass host-local with Godot 4.6.3 Mono and .NET 8.
- Final host-local `./scripts/quality.sh` passes restore, formatting, a zero-warning build,
  2,731 Domain tests, 586 Application tests, 1,220 Infrastructure tests, all 8 architecture
  tests, scenario validation, every credential-free Godot smoke, and the Compatibility
  renderer launch.
- Session review covers exact identity through every new boundary, one-shot ownership,
  null-slot semantics, rebuilt/stale/active rejection, unchanged upstream owners, scene
  dependency order, inert startup, docstrings, 120-character limits, the 500-line focused
  smoke, formatting, and whitespace. Review added the missing coherent exact-active graph
  proof; focused format/build/smoke validation passed afterward.

## Exact next bounded slice

Take **Phase 7qd: admit the exact current sabotage-cycle boundary at the first action-
authoring seam without invoking any action-specific author**.

Acceptance boundary:

1. Test-first add one scene-owned, one-shot action-authoring admission coordinator that
   accepts only `CharacterSabotageCycleBoundaryCoordinator.Boundary` after
   `CharacterSabotageCycleStartEligibilityPolicy` accepts that exact still-current
   boundary, authorization, admission, and ordinary-turn graph.
2. Retain one fresh admission token containing only that exact cycle boundary. Expose the
   actor only by following the retained boundary's exact authorization and completion;
   do not copy actor, request, action kind, ordinary-turn reference, or route output.
3. Reject null, rebuilt-boundary, rebuilt-authorization, replaced-admission, changed-slot,
   exact-active-slot, malformed-provenance, and repeated inputs without changing any
   boundary, admission, turn controller, terminal route, prior branch owner, authoritative
   state, task, result, model source, or model count.
4. Extend only the focused switch-gated Move proof. Ordinary startup must remain inert;
   do not invoke conceal, move, substitute, contaminate, misquote, delay, or influence
   authors, compose a generic request, reserve a turn, or start consideration.
5. Do not add cadence, timers, provider transport, credentials, persistence, UI/input,
   navigation, movement, perception, dialogue delivery, new Domain behavior, author reset,
   or a claim that ordinary enemy-agent sabotage is enabled.
6. Update README/ROADMAP, run `./scripts/quality.sh`, review the complete diff, and leave
   the next slice to let only this exact admission guard one existing author invocation.
