# Allrpcs Handover

## Current state

`main` contains merged PR #221 at `97bc4cf`. The current branch is
`codex/phase-7-independent-task-arrival`.

Two reviewed slices now close the pure arrival-to-occurrence boundary:

- Phase 7by adds `CharacterIndependentTaskTravelOccurrenceEligibilityPolicy`, which permits an
  occurrence only for the
  exact active activity retained by the exact plan and exact completed progress;
- incomplete progress, a distinct plan, and distinct or value-equal activity instances fail closed;
- `CharacterIndependentTaskOccurrenceController` now compare-and-swaps the exact scene-owned plan
  and progress before applying the existing atomic task, event-log, qualitative-time, and
  transcript transition;
- the occurrence smoke proves no occurrence before arrival, exact one-step final arrival, one
  eligible atomic occurrence, and stale, mismatched, repeated, and null failure without partial
  scene mutation;
- Phase 7bz adds `CharacterIndependentTaskArrivalOccurrenceTransition` and its immutable result,
  composing an exact final-waypoint advance with the existing authoritative task occurrence only
  after every input validates;
- non-final, skipped, repeated, distinct-plan, distinct-activity, stale-task, ineligible-event, and
  null inputs produce no partial result and leave every supplied immutable value unchanged.

`Main` still initializes but does not advance travel or trigger the occurrence. The departed healer
remains visible and continues existing ordinary body behavior. No elapsed-time, speed,
interpolation, collision, navigation-agent, off-screen-location, return, rejoin,
dialogue/model-context, persistence, or provider policy was added.

## Validation and review

- Phase 7by and 7bz were driven through focused missing-type compile failures before implementation.
- All 18 focused Application travel and arrival-occurrence tests pass host-local.
- The zero-warning solution build and focused occurrence-controller Godot smoke pass host-local.
- Restricted VSTest reproduced the known loopback socket denial, and restricted Godot reproduced
  the known `user://logs` crash; the identical focused commands passed host-local.
- Complete formatting, suite, smoke-matrix, diff, and practical line-length review remain due before
  publication.
- No live-conformance environment variable was enabled and no provider was invoked.

## Exact next bounded slice

Take **Phase 7ca: install exact final arrival and occurrence atomically in scene state**.

Acceptance boundary:

1. Add an exact compare-and-swap scene method that delegates Phase 7bz and installs its completed
   travel, task, event-log, world-time, and transcript snapshots only from the exact expected current
   plan, progress, activity, and authoritative snapshots.
2. Extend the occurrence smoke to exercise one exact authored final arrival and occurrence through
   that single method, plus stale, mismatched, ineligible, repeated, and null no-ops with every
   surrounding scene snapshot preserved.
3. Preserve the Phase 7by already-completed `TryOccur` boundary and the pure Phase 7bz transition.
4. Do not yet call the new method from `Main`; auto-advance travel; choose elapsed time, speed,
   interpolation, collision, or navigation-agent policy; move or hide a body; create a return route;
   rejoin a party member; change dialogue/model context; persist state; or invoke a provider.
