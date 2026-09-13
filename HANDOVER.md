# Allrpcs Handover

## Current state

`main` contains merged PR #221 at `97bc4cf`. The current branch is
`codex/phase-7-independent-task-arrival`.

Phase 7by makes exact completed travel a prerequisite for the existing independent-task physical
occurrence:

- `CharacterIndependentTaskTravelOccurrenceEligibilityPolicy` permits an occurrence only for the
  exact active activity retained by the exact plan and exact completed progress;
- incomplete progress, a distinct plan, and distinct or value-equal activity instances fail closed;
- `CharacterIndependentTaskOccurrenceController` now compare-and-swaps the exact scene-owned plan
  and progress before applying the existing atomic task, event-log, qualitative-time, and
  transcript transition;
- the occurrence smoke proves no occurrence before arrival, exact one-step final arrival, one
  eligible atomic occurrence, and stale, mismatched, repeated, and null failure without partial
  scene mutation.

`Main` still initializes but does not advance travel or trigger the occurrence. The departed healer
remains visible and continues existing ordinary body behavior. No elapsed-time, speed,
interpolation, collision, navigation-agent, off-screen-location, return, rejoin,
dialogue/model-context, persistence, or provider policy was added.

## Validation and review

- Phase 7by was driven through the focused missing-policy compile failure before implementation.
- All 14 focused Application travel tests pass host-local.
- The zero-warning solution build and focused occurrence-controller Godot smoke pass host-local.
- Restricted VSTest reproduced the known loopback socket denial, and restricted Godot reproduced
  the known `user://logs` crash; the identical focused commands passed host-local.
- Complete formatting, suite, smoke-matrix, diff, and practical line-length review remain due before
  publication.
- No live-conformance environment variable was enabled and no provider was invoked.

## Exact next bounded slice

Take **Phase 7bz: make final arrival plus independent-task occurrence one pure atomic result**.

Acceptance boundary:

1. Add one immutable Application result and pure transition that first validates an exact final
   next waypoint and the complete existing task occurrence, then returns fresh completed travel,
   task, event-log, world-time, and transcript state without partial results.
2. Reject non-final, skipped, repeated, distinct-plan, distinct-activity, stale, null, and otherwise
   ineligible occurrence inputs while preserving every supplied immutable value.
3. Add focused Application tests only; do not replace or weaken the Phase 7by policy or its existing
   scene controller boundary.
4. Do not yet install this combined result in Godot or `Main`; auto-advance travel; choose elapsed
   time, speed, interpolation, collision, or navigation-agent policy; move or hide a body; create a
   return route; rejoin a party member; change dialogue/model context; persist state; or invoke a
   provider.
