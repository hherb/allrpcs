# Allrpcs Handover

## Current state

`main` is verified at merged PR #257 (`39ce7ff`). Branch
`codex/phase-7-world-object-contamination` completes the first four bounded contamination slices:

- `WorldObjectContaminationSnapshot` retains at most 64 value-unique exact `ObjectId` instances in
  stable caller order and exposes only immutable entries plus value-based lookup;
- the pure contamination policy accepts only a fresh, evidence-bound `contaminate` occurrence that
  passes the existing exact authoritative-object, actor-access, capability, and knowledge gates;
- the atomic transition returns fresh contamination and evidence snapshots containing the exact
  target identifier and exact evidence while leaving both inputs unchanged; and
- visible authoritative exact evidence permits recovery into a fresh snapshot that removes only
  the value-matching contaminated target without consuming evidence.

Implementation commit `62f6069` is published in mergeable draft PR #258. Hosted run
`34941813631` failed before executing any steps in all five credential-free jobs (`steps: []`), so
it is Actions/account infrastructure state rather than product-test evidence and was not retried.

No provider was invoked. The work adds no candidate selection, consideration/model/provider path,
controller, scene ownership, presentation, persistence, input, UI, contamination contents or
effects, ingredient-state coupling, timing, scheduling, events, time, transcripts, navigation, or
actor movement. Authoritative world objects and every supplied object, occurrence, evidence,
snapshot, and evidence view remain unchanged. The unrelated untracked
`scripts/code_statistics.py` remains untouched and must stay unstaged.

## Validation and review

- Tests began with expected missing snapshot, policy, transition/result, and recovery contract
  failures before each implementation.
- Focused host-local tests pass 5 snapshot, 4 policy, 3 atomic-transition, and 5 recovery cases.
- Host-local `./scripts/quality.sh` passes workflow validation, restore, formatting, a zero-warning
  build, 2,472 Domain tests, 524 Application tests, 757 Infrastructure tests, 8 architecture tests,
  scenario validation, every credential-free headless Godot smoke, and the final Compatibility
  renderer launch. No live-conformance switch was enabled and no provider was invoked.
- Every changed authored C# file is below 500 lines with no lines longer than 120 characters.
  `git diff --check` passes. Complete changed-file review found no unresolved findings.

## Exact next bounded slice

Take **Phase 7hi: select one exact eligible contamination candidate**.

Acceptance boundary:

1. Add one pure `CharacterObjectSabotageContaminationSelection.SelectFirstEligible` operation that
   accepts the exact character, physical-access, world-object, contamination, and evidence
   snapshots plus a caller-ordered candidate sequence.
2. Fully materialize and validate at most 64 `CharacterObjectSabotageEvidence` candidates before
   selection; reject null input, null entries, value-equal duplicate evidence identifiers, and
   excess entries without changing any input.
3. Return the exact first candidate accepted by
   `CharacterObjectSabotageContaminationPolicy.CanApply`, or `null` when none is eligible. Prove
   stable order, exact-instance return, every ineligibility class, boundary counts, complete
   prevalidation, unchanged inputs, null rejection, and one closed static query surface.
4. Do not add a new candidate value when the existing exact evidence-bound occurrence already is
   the complete policy input. Do not apply contamination, define a sequence, consideration,
   decision, result binding, controller, task, model/provider path, scene ownership, presentation,
   persistence, input, UI, contamination effect, ingredient coupling, timing, scheduling, events,
   time, transcripts, navigation, or actor movement.
