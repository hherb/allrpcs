# Allrpcs Handover

## Current state

`main` contains merged PR #221 at `97bc4cf`. The current branch is
`codex/phase-7-independent-task-arrival`.

Three reviewed slices now close the arrival-to-occurrence boundary:

- Phase 7by adds `CharacterIndependentTaskTravelOccurrenceEligibilityPolicy`, which permits an
  occurrence only for the exact active activity retained by the exact plan and exact completed
  progress;
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
  null inputs produce no partial result and leave every supplied immutable value unchanged;
- Phase 7ca adds `CharacterIndependentTaskOccurrenceController.TryArriveAndOccur`, which delegates
  the pure combined transition and installs completed travel, task, event-log, qualitative-time,
  and transcript state only from the exact compare-and-swapped scene values;
- the expanded occurrence smoke proves stale snapshots, mismatched travel, an unrecognized event,
  pre-arrival occurrence, repetition, and nulls preserve every surrounding scene snapshot.

`Main` still initializes but does not advance travel or trigger the occurrence. The departed healer
remains visible and continues existing ordinary body behavior. No elapsed-time, speed,
interpolation, collision, navigation-agent, off-screen-location, return, rejoin,
dialogue/model-context, persistence, or provider policy was added.

## Validation and review

- Phases 7by, 7bz, and 7ca were driven through focused missing-type compile failures before
  implementation.
- All 18 focused Application travel and arrival-occurrence tests pass host-local.
- The zero-warning solution build and focused occurrence-controller Godot smoke pass host-local.
- `./scripts/quality.sh` passes host-local: restore and formatting; zero-warning solution build;
  2,212 Domain, 461 Application, 532 Infrastructure, and 8 architecture tests; all 5 scenario
  definitions; the complete headless Godot smoke matrix; and Compatibility-renderer launch.
- Restricted VSTest reproduced the known loopback socket denial, and restricted Godot reproduced
  the known `user://logs` crash; the identical focused commands passed host-local.
- Cumulative `git diff --check` and the practical 120-character authored-code check pass.
- No live-conformance environment variable was enabled and no provider was invoked.

## Exact next bounded slice

Take **Phase 7cb: define exact completed-task return travel**.

Acceptance boundary:

1. Add one bounded provider-neutral Application return-travel plan that retains the exact successful
   arrival-occurrence result, its exact completed task activity, validated world bounds and origin,
   and 1-32 stable copied waypoints.
2. Require the return origin to equal the outbound plan's final waypoint and require the return plan
   to end at the outbound origin. Reject null, incomplete or mismatched outbound travel, failed or
   mismatched task results, invalid coordinates, and empty or oversized routes.
3. Add immutable exact-plan return progress and a pure one-next-waypoint transition with focused
   Application tests for stable retention, completion, skips, repeats, and distinct-plan identity.
4. Do not yet scene-own, author, or advance return travel; rejoin a party member; call arrival or
   occurrence from `Main`; choose elapsed time, speed, interpolation, collision, or navigation-agent
   policy; move or hide a body; change dialogue/model context; persist state; or invoke a provider.
