# Allrpcs Handover

## Current state

`main` contains merged PR #240 at `bc63c88`. The current branch is
`codex/phase-7-incompatibility-consideration-controller` and advances three bounded Phase 7 slices:

- Phase 7eq adds `CharacterBeliefGoalIncompatibilityConsiderationController`, forwarding one exact
  request and caller cancellation token to one exact provider-neutral response model exactly once,
  then validating the response through the existing immutable result contract.
- Phase 7er adds `CharacterBeliefGoalIncompatibilityResponseRoundController`, projecting one exact
  declaration into its two exact perspectives and obtaining one validated response for each in
  stable declaration order before returning the existing complete response round.
- Phase 7es adds one finite shared-deadline overload for that complete sequential round, reporting
  deadline expiry separately from caller cancellation and returning no partial round.

Together these slices complete the roadmap's provider-neutral incompatible-character test seam.
They do not implement or bind a model, serialize provider data, retry, judge or apply a response,
resolve the incompatibility, change belief, goal, relationship, or allegiance state, or add
Infrastructure, scene, persistence, input, UI, dialogue, event, time, transcript, movement, or
sabotage-effect behavior.

## Validation and review

- Tests began with the expected missing-controller and missing-overload compile failures.
- Focused host-local tests pass: 9 single-response and 9 complete-round/deadline cases.
- Host-local `./scripts/quality.sh` passes workflow validation, restore, formatting, a zero-warning
  build, 2,352 Domain tests, 506 Application tests, 607 Infrastructure tests, 8 architecture tests,
  scenario validation, every credential-free headless Godot smoke, and the final Compatibility-
  renderer launch. No live-conformance switch was enabled and no provider was invoked.
- All four authored C# files are below 500 lines with no lines longer than 120 characters.
  `git diff --cached --check` passes, and manual staged review found no unresolved findings.
- Unrelated untracked `scripts/code_statistics.py` remains untouched and must stay unstaged.

## Exact next bounded slice

Take **Phase 7et: bind one exact move-sabotage occurrence to one authored destination**.

Acceptance boundary:

1. Add one immutable Domain value that accepts one exact `CharacterObjectSabotageOccurrence` and one
   exact authored `Location` destination, retaining both without copying or applying state.
2. Accept only an occurrence whose proposal uses the exact canonical
   `CharacterSabotageActionKind.Move`; reject nulls and conceal, substitute, or contaminate
   occurrences before retaining state.
3. Prove exact nested actor, access requirement, target, and destination retention; non-move
   rejection; independently composed and value-equal destination distinction; unchanged source
   objects; and a closed get-only surface.
4. Do not infer current location, path, reachability, travel duration, or destination eligibility;
   do not move a body or world object, treat binding as occurrence or success, leave evidence, remove
   a route, change truth or perception, or add selection, application, recovery, scheduling, model,
   provider, Application, Infrastructure, scene, persistence, input, UI, event, time, transcript,
   navigation, or presentation behavior.
