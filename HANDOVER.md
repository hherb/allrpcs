# Allrpcs Handover

## Current state

`main` contains merged PR #242 at `7e43d52`. The current branch is
`codex/phase-7-move-sabotage-recovery` and advances four bounded Phase 7 slices:

- Phase 7ex adds `CharacterObjectSabotageMoveRecoveryCandidate`, retaining one exact move candidate
  and one exact pre-move source placement while rejecting replacement targets and destination reuse.
- Phase 7ey adds `CharacterObjectSabotageMoveRecoveryPolicy`, requiring the exact moved target at
  the exact authored destination plus exact authoritative evidence visible to one character.
- Phase 7ez adds the immutable move-recovery transition and result, restoring the exact retained
  source placement at its stable index without consuming evidence or changing unrelated placement.
- Phase 7fa adds `CharacterObjectSabotageMoveController`, scene-owning exact placement and evidence
  snapshots with one-time initialization and exact compare-and-swap apply and recovery.

These slices do not infer prior location, destination eligibility, path, reachability, travel
duration, timing, or scene coordinates; move an actor or Godot body; change an authoritative world
object; consume evidence; select sabotage behavior; or add main-scene wiring, persistence, input,
UI, event, time, transcript, navigation, provider, model, or presentation behavior.

Draft PR #243 is open from implementation commit `dd250cf`. Hosted run `34840402983` failed all
five jobs immediately with `steps: []`; this is Actions/account infrastructure state, not product-
test or unsigned-build evidence, and the unchanged run must not be retried.

## Validation and review

- Tests began with the expected missing-type compile failures for the candidate, policy, transition,
  result, and scene controller.
- Focused host-local tests pass: 4 recovery-candidate cases and 8 move-recovery policy/transition
  cases (12 combined). The first sandboxed VSTest run was blocked by its local socket; the identical
  focused command passed host-local.
- The isolated `CharacterObjectSabotageMoveControllerSmokeCheck.tscn` passes one-time ownership,
  exact compare-and-swap apply/recovery, stable unrelated entries, stale and ineligible rejection,
  null rejection, minimal surface, and atomic failure host-local. The sandboxed Godot run failed at
  `user://logs` with signal 11; the identical host-local smoke passed after the C# assembly build.
- Host-local `./scripts/quality.sh` passes workflow validation, restore, formatting, a zero-warning
  build, 2,385 Domain tests, 506 Application tests, 607 Infrastructure tests, 8 architecture tests,
  scenario validation, every credential-free headless Godot smoke, and the final Compatibility-
  renderer launch. No live-conformance switch was enabled and no provider was invoked.
- All authored C# files are below 500 lines with no lines longer than 120 characters.
- Unrelated untracked `scripts/code_statistics.py` remains untouched and must stay unstaged.

## Exact next bounded slice

Take **Phase 7fb: author one exact temporary move-sabotage source**.

Acceptance boundary:

1. Add one isolated Godot source that resolves the exact tail `village-water-cask` from the existing
   `TemporaryWorldObjectSource` snapshot and authors exact source and destination `Location` values.
2. Expose one exact representative enemy, its bounded physical access, the existing exact world-
   object snapshot, exact initial placements and empty evidence, one eligible move candidate, and
   its exact recovery candidate; initialize no scene-owned state and apply nothing.
3. Prove stable exact instance retention, water-cask tail binding, candidate/recovery alignment,
   complete move eligibility from the authored initial state, bounded closed read-only output, and
   inert construction/startup in one isolated credential-free smoke scene.
4. Do not add the source or move controller to `Main.tscn`; choose when sabotage occurs; apply or
   recover a move; project locations to coordinates or bodies; infer paths, reachability, duration,
   or timing; or add persistence, input, UI, event, time, transcript, navigation, provider, model,
   or presentation behavior.
