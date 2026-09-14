# Allrpcs Handover

## Current state

`main` contains merged PR #246 at `23e1026`. The current branch is
`codex/phase-7-move-consideration-application` and advances three bounded Phase 7 slices:

- Phase 7fn applies one exact accepted move selection through the existing immutable placement and
  evidence transition, while accepted abstention returns no application.
- Phase 7fo resolves that result only against its exact current scene-owned placement and evidence
  snapshots, installing both fresh selection snapshots atomically or consuming abstention without
  replacing either snapshot.
- Phase 7fp owns one cancellation-aware move-decision task through closed idle, running, completed,
  cancelled, and failed states, resolving only a non-stale accepted result on the Godot main thread.

These slices do not recover or present a move; construct, supply, serialize, or invoke a model; add
ordinary-flow or main-scene wiring; choose sabotage behavior or timing; or add persistence, input,
UI, events, time, transcripts, navigation, or actor movement.

Implementation commit `cc8bf61` is published in mergeable draft PR #247. Initial hosted run
`34853418068` failed before execution with `steps: []` for all five jobs; this is Actions/account
infrastructure state, not product-test evidence, and the unchanged run was not retried.

## Validation and review

- Tests began with the expected missing transition, scene operation, and task-controller failures.
- The focused host-local Domain regression passes 4 tests for exact application, abstention, stale
  construction rejection, unchanged inputs, null rejection, and the closed pure surface.
- Dedicated host-local Godot smokes pass exact selection and abstention resolution, stale/null/
  uninitialized/repeated rejection, main-thread task installation, overlap rejection, cancellation,
  safe model failure, stale completion rejection, reset, closed surfaces, and unchanged inputs.
- Host-local `./scripts/quality.sh` passes workflow validation, restore, formatting, a zero-warning
  build, 2,407 Domain tests, 512 Application tests, 607 Infrastructure tests, 8 architecture tests,
  scenario validation, every credential-free headless Godot smoke, and the final Compatibility-
  renderer launch. No live-conformance switch was enabled and no provider was invoked.
- All changed authored C# files are at most 456 lines with no lines longer than 120 characters.
  `git diff --cached --check` passes, and manual review found no unresolved findings.
- Sandboxed MSBuild/VSTest and Godot were blocked by local IPC or `user://logs`; each identical
  host-local focused command passed. Product code was not changed to work around the sandbox.
- Hosted run `34853418068` executed no job steps; host-local quality is the product evidence.
- Unrelated untracked `scripts/code_statistics.py` remains untouched and must stay unstaged.

## Exact next bounded slice

Take **Phase 7fq: supply one exact one-shot move-decision model or one stable offline model**.

Acceptance boundary:

1. Add one temporary Godot model source retaining at most one exact caller-supplied
   `ICharacterObjectSabotageMoveDecisionModel` for the next decision only.
2. `TakeModel` must atomically consume that one-shot model; without one, return the same stable
   fail-closed offline model on every call.
3. The offline model must reject null requests, honor caller cancellation first, and otherwise fail
   without returning a fabricated move decision.
4. Prove exact one-shot selection, overlap rejection, later resupply, stable offline identity,
   cancellation, no model invocation during selection, unchanged request state, and the smallest
   public source surface in one focused credential-free Godot smoke.
5. Do not add configured bindings, request serialization, provider adapters or invocation, a task
   flow, main-scene wiring, automatic timing, application/recovery/presentation, persistence, input,
   UI, events, time, transcripts, navigation, or actor movement.
