# Allrpcs Handover

## Current state

`main` includes merged PR #332 at `1c74861`. Branch
`codex/phase-7-sabotage-scheduling-admission` completes **Phase 7px**, **Phase 7py**,
and **Phase 7pz**. Implementation commit `af23070` is published in draft PR #333 targeting `main`.
The unrelated untracked `scripts/code_statistics.py` remains untouched and must stay unstaged.

`CharacterSabotageTurnAdmissionCoordinator` retains the completion coordinator's exact current
token once only after the existing pure scheduling gate accepts that same instance. It copies no
actor, request, kind, or route output and changes no prior owner.

`CharacterSabotageOrdinaryTurnBoundaryCoordinator` binds that exact admission once to the existing
`CharacterTurnActivityController` start boundary only while the caller supplies its exact current
activity and the existing turn-start policy accepts it. An empty ordinary turn slot is represented
by the retained admission plus an exact null activity reference; the turn controller is unchanged.

`CharacterSabotageOrdinaryTurnBoundaryEligibilityPolicy` purely revalidates the exact admission,
the bound/current ordinary turn reference, the completion provenance, and ordinary start
eligibility. Ordinary startup remains inert. No new boundary reserves a slot or authors, starts,
observes, acknowledges, dispatches, applies, resets, or schedules a sabotage action.

## Validation and review

- Test-first game builds failed only for the deliberately missing Phase 7px admission coordinator,
  Phase 7py ordinary-turn boundary coordinator, and Phase 7pz revalidation policy before each
  implementation was added.
- The focused switch-gated Move proof passes host-local through recovery, terminal reset, turn
  completion, exact one-shot admission, exact empty ordinary-turn binding, and stale-safe pure
  revalidation. Null, rebuilt, malformed-provenance, replaced, exact-active, stale, and repeated
  inputs remain inert while route owners, authoritative state, tasks, results, and model counts stay
  unchanged.
- Sandboxed Godot still hits the known `user://logs` permission crash, and sandboxed VSTest still
  cannot bind its local communication socket. The identical focused and full commands pass
  host-local with Godot 4.6.3 Mono and .NET 8.
- Final host-local `./scripts/quality.sh` passes restore, formatting, a zero-warning build, 2,731
  Domain tests, 586 Application tests, 1,220 Infrastructure tests, all 8 architecture tests,
  scenario validation, every credential-free Godot smoke, and the Compatibility renderer launch.
- Session review covers exact completion/admission/activity identity, malformed and stale graphs,
  exact active-turn rejection, one-shot ownership, delegation through one pure final gate, ordinary
  node ordering, inert startup, unchanged prior owners, docstrings, the 499-line focused smoke,
  120-character limits, formatting, and whitespace. Review removed duplicated boundary checks and
  added the missing same-reference active-turn rejection; the full suite passed again afterward.
- Draft PR #333's five hosted jobs failed before execution with `steps: []`. No checkout, build,
  test, smoke, or unsigned development export step ran, so this is Actions/account infrastructure
  state rather than hosted product-test evidence.

## Exact next bounded slice

Take **Phase 7qa: retain one exact ordinary-turn scheduling authorization from the still-current
revalidated sabotage admission without reserving the slot or starting another sabotage action**.

Acceptance boundary:

1. Test-first add one scene-owned, one-shot authorization coordinator that accepts only the exact
   `CharacterSabotageOrdinaryTurnBoundaryCoordinator.Admission` and its exact bound ordinary turn
   reference after `CharacterSabotageOrdinaryTurnBoundaryEligibilityPolicy` accepts the still-current
   admission and `CharacterTurnActivityController.Activity`.
2. Retain one fresh authorization token containing only those exact admission and ordinary-turn
   references. A null ordinary-turn reference is valid only when both the bound and current slot are
   still null; do not copy actor, request, kind, route output, or create a character-turn request.
3. Reject null, rebuilt, replaced-admission, changed-slot, exact-active-slot, malformed-provenance,
   and repeated inputs without changing the admission, boundary, completion, ordinary turn
   controller, terminal route, any prior route owner, authoritative state, task, result, or model.
4. Extend only the focused switch-gated Move proof. Ordinary startup must remain inert; do not
   author, start, observe, acknowledge, dispatch, apply, reset, reserve, or schedule an action.
5. Do not add cadence, timers, provider transport, credentials, persistence, UI/input, navigation,
   movement, perception, dialogue delivery, new Domain behavior, or a claim that ordinary
   enemy-agent sabotage is enabled.
6. Update README/ROADMAP, run `./scripts/quality.sh`, review the complete diff, and leave the next
   slice to consume only this exact authorization at a future explicit sabotage-cycle boundary.
