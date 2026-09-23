# Allrpcs Handover

## Current state

`main` includes merged PR #336 at `eb1f41b`. Branch
`codex/phase-7-sabotage-action-result-followup` completes **Phase 7qh** through
**Phase 7qk**. Implementation commit `0fea221` is published in draft PR #337 targeting `main`. The
unrelated untracked `scripts/code_statistics.py` remains untouched and must stay unstaged.

The fresh generic follow-up Move task now has one exact observation seam. It accepts only the exact
completed result for `CharacterSabotageActionMoveCompositionStartCoordinator.StartedRequest` while
the complete admission, authoring, candidate-binding, composition, and start graph remains current.
It retains the exact result once without acknowledging it.

`CharacterSabotageActionMoveResultBindingCoordinator` binds only that observed result to the guarded
author's exact `FollowUpCandidate` binding. It then acknowledges only the same bound result, resets
the scheduled generic task to idle, clears the observer, and retains the exact acknowledged result
and candidate-binding tokens.

`CharacterSabotageActionMoveSpecificRequestCoordinator` composes those tokens into one fresh typed
Move request containing the exact actor, access, world objects, current authoritative placements and
evidence, and only the exact follow-up candidate. Its start coordinator revalidates the complete
graph and starts only that exact request on a separate fresh scene-owned Move task. Rejected
preflight paths preserve the queued one-shot model; the successful path calls it once.

The new typed task is left running and its result is not observed or acknowledged. Neither author
is reset, the empty ordinary-turn slot remains unreserved, all first-cycle owners remain retained,
and ordinary startup remains inert. No cadence, timer, provider, credential, persistence, UI/input,
navigation, perception, dialogue, recovery, or new Domain behavior was added.

## Validation and review

- All four slices were test-first: each focused game build failed only for the deliberately missing
  next verifier before its implementation was added.
- The focused switch-gated Move proof passes host-local through exact generic observation, typed
  candidate binding, acknowledgement, fresh typed request composition, and explicit fresh-task
  start. Generic and typed follow-up models are each called once; rejected preflight calls consume
  neither model.
- Sandboxed Godot still cannot open `user://logs`; the identical focused command passes host-local
  with Godot 4.6.3 Mono and .NET 8.
- Final host-local `./scripts/quality.sh` passes restore, formatting, a zero-warning build, 2,731
  Domain tests, 586 Application tests, 1,220 Infrastructure tests, all 8 architecture tests,
  scenario validation, every credential-free Godot smoke, and the Compatibility renderer launch.
- Session review covered exact-reference provenance, completed-task authority, acknowledgement
  order, candidate binding, preflight model preservation, separate task ownership, unchanged prior
  owners and ordinary-turn state, inert startup, docstrings, 120-character limits, the under-500-line
  focused smoke, formatting, and whitespace. No unresolved review findings remain.

## Exact next bounded slice

Take **Phase 7ql: observe one exact completed applied typed follow-up Move result without
acknowledging or routing it**.

Acceptance boundary:

1. Test-first add one scene-owned, one-shot observation seam for the scheduled typed Move task. It
   must accept only the result whose request is the exact
   `CharacterSabotageActionMoveSpecificRequestStartCoordinator.StartedRequest` after that exact task
   reaches `Completed`.
2. Require the selected candidate to be the exact retained `FollowUpCandidate` and require the live
   Move controller to own the result's exact fresh placement and evidence snapshots. Revalidate the
   retained acknowledged generic result, typed candidate binding, composed request, and start tokens
   without incorrectly requiring the request's pre-application snapshots to remain current.
3. Reject null, running, failed, cancelled, abstaining, rebuilt-result, rebuilt-request,
   foreign-candidate, stale-authority, replaced-task, and repeated paths without another model call
   or changes to any retained owner.
4. Extend only the focused switch-gated Move proof. Do not acknowledge or reset the typed task,
   bind or publish its authoritative outcome, recover the move, retain/reset a terminal route,
   complete/admit another sabotage turn, reserve the ordinary turn, or start another cycle.
5. Keep ordinary startup inert and preserve all existing exclusions: no cadence, timers, provider
   transport, credentials, persistence, UI/input, navigation, perception, dialogue delivery, or new
   Domain behavior.
6. Update README/ROADMAP, run `./scripts/quality.sh`, review the complete diff, and leave the
   following slice to acknowledge and bind only that exact observed typed result to its current
   authoritative Move outcome.
