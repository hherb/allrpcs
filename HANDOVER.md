# Allrpcs Handover

## Current state

`main` includes merged PR #337 at `137c6e4`. Branch
`codex/phase-7-sabotage-action-applied-result` completes **Phase 7ql** and **Phase 7qm**. The
implementation is committed at `0eb74ae` and published in draft PR #338 targeting `main`. The
unrelated untracked `scripts/code_statistics.py` remains untouched and must stay unstaged.

The scheduled typed follow-up Move task now retains internal-only references to the exact
authoritative placement and evidence snapshots it installs for a completed result. Reset clears
those task-owned tokens. Its public surface remains unchanged.

`CharacterSabotageActionMoveAppliedResultObservationCoordinator` accepts only the exact completed
result for `CharacterSabotageActionMoveSpecificRequestStartCoordinator.StartedRequest`, requires
the exact retained `FollowUpCandidate`, and requires the live Move controller to own the task's
exact resolved snapshots. It retains that result and those snapshots once without acknowledging.
The retained request graph deliberately does not require its pre-application snapshots to remain
current.

`CharacterSabotageActionMoveAppliedResultBindingCoordinator` binds only that exact observation and
its exact authoritative snapshots. It acknowledges only the same bound result, resets the scheduled
typed task to idle, clears the observer, and retains the exact acknowledged result, placement, and
evidence tokens. Rebuilt result, request, candidate, placement, evidence, stale, null, running,
abstaining, and repeated paths remain inert.

No terminal route, recovery candidate, evidence-view eligibility, recovery, turn completion,
ordinary-turn reservation, author reset, cadence, timer, provider, credential, persistence,
UI/input, navigation, perception, dialogue, or new Domain effect was added. Ordinary startup stays
inert and both follow-up models are called once.

## Validation and review

- Both slices were test-first; each focused build first failed only for its deliberately absent next
  coordinator.
- The focused Move proof passes host-local with Godot 4.6.3 Mono and .NET 8 through exact applied
  result observation, snapshot binding, acknowledgement, and typed-task reset.
- Sandboxed Godot cannot open `user://logs`; the identical focused command passes host-local.
- Final host-local `./scripts/quality.sh` passes restore, formatting, a zero-warning build, 2,731
  Domain tests, 586 Application tests, 1,220 Infrastructure tests, all 8 architecture tests,
  scenario validation, every credential-free Godot smoke, and the Compatibility renderer launch.
- Draft PR #338's five hosted jobs for head `0eb74ae` failed before execution with `steps: []` in
  run `35868037777`. No checkout, build, test, smoke, or unsigned development export step ran, so
  this is Actions/account infrastructure state rather than hosted product-test evidence.
- Session review found and fixed one exact-provenance gap by retaining the task's installed snapshot
  references internally. The first full suite then caught and fixed an accidental public-surface
  expansion; the corrected task smoke and complete quality suite pass.
- Review covered exact-reference provenance, selected-result authority, pre/post-application state,
  acknowledgement order, reset clearing, rejected/repeated paths, unchanged prior owners and
  ordinary-turn state, inert startup, docstrings, file/line limits, formatting, and whitespace. No
  unresolved review findings remain.

## Exact next bounded slice

Take **Phase 7qn: author one exact follow-up Move recovery candidate without publishing eligibility
or starting recovery**.

Acceptance boundary:

1. Test-first add one scene-owned, one-shot coordinator that accepts only the exact acknowledged
   result, placement, and evidence tokens retained by
   `CharacterSabotageActionMoveAppliedResultBindingCoordinator` while that full graph remains
   current.
2. Require the selected candidate to be the exact retained `FollowUpCandidate`. Resolve its target's
   exact pre-move source placement from the acknowledged typed request's placement snapshot and
   retain one fresh `CharacterObjectSabotageMoveRecoveryCandidate` containing those exact instances.
3. Reject null, rebuilt-result, rebuilt-placement, rebuilt-evidence, foreign-candidate,
   missing/ambiguous-source-placement, stale-authority, and repeated paths without state changes or
   another model call.
4. Extend only the focused switch-gated Move proof. Do not create or publish an evidence view,
   publish recovery eligibility, recover the move, retain/reset a terminal route, complete/admit
   another sabotage turn, reserve the ordinary turn, reset either author, or start another cycle.
5. Keep ordinary startup inert and preserve all existing exclusions: no cadence, timers, provider
   transport, credentials, persistence, UI/input, navigation, perception, dialogue delivery, or new
   Domain behavior.
6. Update README/ROADMAP, run `./scripts/quality.sh`, review the complete diff, and leave the next
   slice to pair that exact recovery candidate with an explicitly supplied current evidence view
   without invoking recovery.
