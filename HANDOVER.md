# Allrpcs Handover

## Current state

`main` includes merged PR #343 at `66cd06d`. Branch
`codex/phase-7-third-move-typed-route` completes **Phase 7rg** through **Phase 7ri**. The
implementation is committed at `965917f` and published in draft PR #344 targeting `main`. The
unrelated untracked `scripts/code_statistics.py` remains untouched and must stay unstaged.

The exact acknowledged third-cycle generic Move result now advances through three bounded seams:

- `CharacterSabotageActionMoveCycleSpecificRequestCoordinator` composes one typed request from only
  the retained exact result/candidate binding and current recovered placement/evidence authority.
- An explicit start coordinator uses a separate one-shot typed model source and task, preserving the
  queued model across rejected preflight and starting only that exact request.
- `CharacterSabotageActionMoveCycleAppliedResultObservationCoordinator` accepts only the exact
  completed selected result and the exact fresh authoritative snapshots installed by that task.

Null, rebuilt-result/request/binding, foreign-candidate, stale-authority, ineligible, abstaining,
running, and repeated paths remain inert. Retained acknowledgement provenance remains exact after
application without incorrectly requiring the request's pre-application recovered snapshots to
remain current. Each of the four bounded model calls occurs once, the ordinary turn stays empty,
and ordinary startup remains inert. No acknowledgement or task reset, recovery, terminal route,
author reset, turn reservation, cadence, provider, credential, persistence, UI/input, navigation,
perception, dialogue, timer, or new Domain behavior was added.

## Validation and review

- Each slice was test-first: the focused game build failed only for the deliberately absent
  composition, start, or observation coordinator before its implementation.
- The focused switch-gated Move proof passes host-local with Godot 4.6.3 Mono and .NET 8 through
  exact typed composition, explicit start, application, and observation of fresh authority.
- Sandboxed Godot could not open `user://logs`; the identical focused commands passed host-local.
- Final host-local `./scripts/quality.sh` passes a zero-warning build, 2,731 Domain, 586 Application,
  1,220 Infrastructure, and 8 architecture tests, scenario validation, every credential-free Godot
  smoke, and the Compatibility launch. Formatting and whitespace checks also pass.
- Draft PR #344's hosted jobs failed with `steps: []`; no hosted product step executed, so this is
  Actions/account infrastructure state rather than contradictory product-test evidence.

## Exact next bounded slice

Take **Phase 7rj: bind and acknowledge the exact observed applied third-cycle Move result**.

Acceptance boundary:

1. Test-first add one binding coordinator over the exact observation, typed start/task, request
   owner, and current `CharacterObjectSabotageMoveController` state.
2. Bind only the exact observed result together with its exact observed placement/evidence snapshots
   while those same snapshots remain authoritative and task-owned provenance remains exact.
3. Acknowledge only that same bound graph, reset only the third-cycle typed task, clear only the
   observation, and retain the exact result plus authoritative snapshots as downstream tokens.
4. Reject null, rebuilt-result/request/candidate/snapshots, stale authority, running, foreign,
   abstaining, and repeated inputs without changing prior owners, authoritative state, ordinary
   turn, or any model call count.
5. Extend only the focused switch-gated Move proof. Do not author or publish a recovery candidate,
   recover or route the result, reset any author, reserve the ordinary turn, or add cadence.
6. Keep ordinary startup inert and preserve all existing exclusions: no timers, provider transport,
   credentials, persistence, UI/input, navigation, perception, dialogue delivery, or new Domain
   behavior.
7. Update README/ROADMAP, run `./scripts/quality.sh`, review the complete diff, and leave the next
   slice to author only one exact recovery candidate from the retained applied provenance.
