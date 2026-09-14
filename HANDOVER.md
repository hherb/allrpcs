# Allrpcs Handover

## Current state

`main` contains merged PR #245 at `6176bc3`. The current branch is
`codex/phase-7-move-decision-flow` and advances five bounded Phase 7 slices:

- Phase 7fi defines `CharacterObjectSabotageMoveDecision`, retaining the exact deciding character
  identifier and exact selected move-candidate evidence identifier, or `null` for abstention.
- Phase 7fj accepts only that exact considering character and, for selection, an exact candidate
  that remains eligible under the complete current move policy; exact abstention is accepted.
- Phase 7fk binds one accepted decision to its exact request and selected exact request candidate,
  or retains `null` for abstention.
- Phase 7fl defines one provider-neutral cancellation-aware move-decision model interface.
- Phase 7fm obtains one model decision and returns only its exact validated consideration result.

These slices do not apply or recover a move; mutate placement, evidence, world-object, or scene
state; choose sabotage behavior or timing; add tasks, scene wiring, serialization, provider adapters,
models, persistence, input, UI, events, time, transcripts, navigation, or actor movement.

## Validation and review

- Tests began with the expected missing decision, policy, result, model interface, and coordinator
  failures.
- The combined focused host-local regression sets pass 13 Domain and 6 Application tests, covering
  exact selection, abstention, value-equal identity rejection, current eligibility, immutable
  result binding, cancellation, invalid model output, nulls, and closed public surfaces.
- Host-local `./scripts/quality.sh` passes workflow validation, restore, formatting, a zero-warning
  build, 2,403 Domain tests, 512 Application tests, 607 Infrastructure tests, 8 architecture tests,
  scenario validation, every credential-free headless Godot smoke, and the final Compatibility-
  renderer launch. No live-conformance switch was enabled and no provider was invoked.
- All ten new C# files are 12–199 lines with no lines longer than 120 characters.
  `git diff --check` passes, and manual review found no unresolved findings.
- Sandboxed MSBuild/VSTest was blocked or stalled on local IPC; each identical host-local focused
  command passed. Product code was not changed to work around the sandbox.
- Unrelated untracked `scripts/code_statistics.py` remains untouched and must stay unstaged.

## Exact next bounded slice

Take **Phase 7fn: apply an accepted move selection through the existing immutable transition**.

Acceptance boundary:

1. Add one pure Domain transition accepting an exact
   `CharacterObjectSabotageMoveConsiderationResult`.
2. For selection, delegate the result's exact request authority snapshots and exact selected
   candidate to `CharacterObjectSabotageMoveTransition.Apply`, returning its fresh placement and
   evidence result; for abstention, return `null` without changing any retained input.
3. Add focused Domain tests for exact selected application, explicit abstention, stale or invalid
   construction remaining impossible through the existing result contract, null rejection,
   unchanged inputs, and one closed pure operation.
4. Do not resolve against scene-owned state; add controllers, tasks, scene wiring, model sources,
   serialization, provider adapters, model invocation, recovery, presentation, timing, persistence,
   input, UI, events, transcripts, navigation, or actor movement.
