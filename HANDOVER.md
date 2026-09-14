# Allrpcs Handover

## Current state

`main` contains merged PR #235 at `7ed263f`. The current branch is
`codex/phase-7-model-concealment-presentation` and advances one bounded Phase 7 slice:

- Phase 7ef observes each fresh exact completed concealment-decision task result at most once and
  presents only a selected result through the existing stale-safe compare-and-swap presentation
  seam. `Main.tscn` wires it after the exact state, task, and presentation nodes, and an explicitly
  started selected result hides only the water-cask body.

Draft PR #236 is open from implementation commit `c5b5d12`. Hosted run `34796908911` was queued
when last observed; re-query its final state rather than assuming it executed product checks.

Accepted abstention, idle, running, cancelled, failed, stale-presentation, repeated, and null-result
states do not change presentation or task state. Startup remains inert. Nothing chooses timing,
starts a model or sabotage automatically, resets a completed task, persists sabotage, adds
input/UI/events/time/transcripts/movement, invokes a provider during standard validation, or adds
another provider or sabotage effect.

## Validation and review

- The focused coordinator smoke began with the expected `CS0246` compile failure for the absent
  coordinator. The main-scene smoke then began with the expected missing-node failure before scene
  wiring.
- The isolated smoke passes exact selected presentation plus idle, running, abstaining, cancelled,
  failed, stale, repeated, and null-result preservation. The real-main-scene smoke passes inert
  startup and one explicitly started selected result hiding the exact water-cask body once without
  changing task, source, world-object, or position state.
- Host-local `./scripts/quality.sh` passes workflow validation, restore, formatting, zero-warning
  build, 2,306 Domain tests, 487 Application tests, 607 Infrastructure tests, 8 architecture tests,
  scenario validation, every credential-free headless Godot smoke, and the Compatibility-renderer
  graphical launch.
- All changed authored C# files are below 500 lines with no lines longer than 120 characters. No
  live-conformance switch was enabled and no provider was invoked.

## Exact next bounded slice

Take **Phase 7eg: explicitly acknowledge one observed completed concealment result**.

Acceptance boundary:

1. Extend the presentation coordinator with one explicit operation that resets only the exact
   completed task result it has already observed; do not reset from `_Process`.
2. For a selection, require the exact current scene concealment to be the last-presented snapshot.
   For abstention, require the exact request concealment and evidence snapshots still to be current.
   Stale, unobserved, idle, running, cancelled, failed, null, and repeated acknowledgement must not
   change task, state, or presentation.
3. Prove one exact acknowledged selection or abstention returns the task to idle with no result and
   permits a later explicitly requested decision without changing source or unrelated scene state.
4. Do not add timing or automatic triggers, new input/UI, persistence, events/time/transcripts,
   movement, another provider, another sabotage effect, or provider calls in standard validation.
