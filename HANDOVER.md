# Allrpcs Handover

## Current state

`main` contains merged PR #251 at `f5d750f`. The current branch is
`codex/phase-7-substitution-recovery` and advances three bounded Phase 7 slices:

- Phase 7gg permits substitution recovery only when the exact candidate substitution remains
  active, its exact target-bound evidence remains authoritative, and its evidence identifier is
  visible in one supplied character evidence view;
- Phase 7gh removes only that exact substitution into a fresh stable snapshot while retaining
  unrelated exact substitutions, the exact evidence view, the exact candidate, and all
  authoritative evidence; and
- Phase 7gi adds an isolated scene controller that owns exact substitution/evidence snapshots once
  and applies or recovers only through exact-reference compare-and-swap operations.

Implementation commit `f1cfc61` is published in mergeable draft PR #252. Hosted run `34914815173`
failed before executing any steps in all five credential-free jobs (`steps: []`), so it is recorded
as Actions/account infrastructure state rather than product-test evidence and was not retried.

The implementation does not replace an authoritative world object or scene body, consume evidence,
infer inventory, placement, similarity, plausibility, reachability, critical-route safety,
perception, or discovery, or add a source, selection, scheduling, consideration, task, model,
provider, persistence, `Main.tscn` wiring, input, UI, event, time, transcript, navigation, or
presentation behavior. The unrelated untracked `scripts/code_statistics.py` remains untouched and
must stay unstaged.

## Validation and review

- Tests began with the expected missing policy, result, transition, and controller compile failures.
- The focused substitution-recovery suite passes 7 Domain cases. Sandboxed VSTest was blocked by
  local IPC `SocketException (13)`; the identical host-local test passed without product changes.
- The focused substitution-controller Godot smoke passes one-time initialization, exact CAS apply
  and recovery, stable unrelated state, stale/value-equal rejection, null rejection, and atomic
  failure. The first sandboxed smoke hit the known `user://logs` failure and signal 11; the identical
  host-local smoke passed. An initial parallel solution build stalled after Domain output; the
  documented build-server-disabled single-worker invocation produced the expected red controller
  compile failure, then a zero-warning successful build after implementation.
- Host-local `./scripts/quality.sh` passes workflow validation, restore, formatting, a zero-warning
  build, 2,433 Domain tests, 515 Application tests, 682 Infrastructure tests, 8 architecture tests,
  scenario validation, every credential-free headless Godot smoke including the new substitution
  controller, and the final Compatibility-renderer launch. No live-conformance switch was enabled
  and no provider was invoked.
- All changed authored C# files are below 500 lines with no lines longer than 120 characters.
  `git diff --check` passes; final staged-diff review found no unresolved findings.

## Exact next bounded slice

Take **Phase 7gj: author one inert temporary substitution source**.

Acceptance boundary:

1. Add one isolated Godot source that retains the existing exact authoritative world-object
   snapshot and its tail `village-water-cask`, then authors one representative enemy, complete
   physical access, one distinct replacement object, empty substitution/evidence snapshots, and one
   exact eligible substitution candidate with target-bound discoverable evidence.
2. Expose only stable read-only exact outputs needed by the existing substitution policy and
   controller; prove the candidate is currently eligible and every retained collection is immutable
   through one credential-free smoke scene.
3. Keep startup inert: do not initialize the substitution controller, apply or recover a
   substitution, replace a world object or scene body, or mutate evidence, perception, or any
   authoritative source object.
4. Do not wire `Main.tscn` or add selection, timing, scheduling, consideration, task, model,
   provider, persistence, input, UI, event, time, transcript, navigation, or presentation behavior.
