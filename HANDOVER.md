# Allrpcs Handover

## Current state

`main` includes merged PR #304 at `1e8c500`. Branch `codex/phase-7-delay-authority` advances the
character-targeted delay path through three bounded slices:

- Phase 7mt adds `CharacterTargetSabotageDelayController`, which initializes one exact authoritative
  `WorldEventLogSnapshot` and `WorldTimeSnapshot` pair, consumes every exact accepted consideration
  request once, keeps abstention referentially inert, and atomically installs only the existing pure
  transition's fresh pair plus exact applied result for a selection.
- Phase 7mu adds `CharacterTargetSabotageDelayResolutionCoordinator`, which observes each exact
  completed task result once and permits explicit task reset only while the exact resolved event log,
  world time, and latest-applied-result tokens remain current.
- Phase 7mv dependency-orders the authority and resolution coordinator in `Main.tscn`. Ordinary
  startup remains uninitialized and inert; the switch-gated credential-free smoke alone initializes
  one exact request state and proves selected application, guarded acknowledgement, and unchanged
  applied authority after a later offline failure.

No slice constructs or schedules requests automatically, recovers elapsed time, authors evidence,
persists world state, or adds input/UI, navigation, movement, perception, dialogue, or other world
behavior. The unrelated untracked `scripts/code_statistics.py` remains untouched and must stay
unstaged.

## Validation and review

- Test-first focused runs recorded the expected missing authority and resolution types and the
  missing main-scene authority nodes before implementation.
- Focused zero-warning game builds pass, as do the isolated exact authority, completed-result
  resolution, and switch-gated main-scene smokes.
- Sandboxed Godot reproduced the known `user://logs` failure and signal 11; identical
  credential-free host runs pass without product changes.
- Sandboxed VSTest reproduced the known loopback `SocketException (13)`; the identical host-local
  `./scripts/quality.sh` passes workflow validation, restore, formatting, a zero-warning build,
  2,687 Domain tests, 569 Application tests, 1,091 Infrastructure tests, all 8 architecture tests,
  scenario validation, every credential-free Godot smoke, and the final Compatibility launch.
- Session-wide review covers atomic pair initialization/application, exact current-state gating,
  selection and abstention, non-consecutive request replay, weak request retention, one-shot result
  observation, three-token acknowledgement, dependency order, inert startup, unchanged sources,
  public surfaces, documentation, file size, line length, formatting, whitespace, and scoped paths.
  One test-precision finding was fixed by proving failed null initialization leaves the complete
  authority pair uninitialized; no unresolved finding remains.

## Exact next bounded slice

Take **Phase 7mw: define one exact target-delay recovery candidate**.

Acceptance boundary:

1. Test-first add the smallest pure Domain vocabulary that binds one exact applied
   `CharacterTargetSabotageDelayResult` to one exact caller-authored recovery world event without
   applying or recording that event.
2. Require the applied result's exact delay event to remain the current qualitative-time event and
   the exact tail of the current event log. Require the recovery event to be an authoritative,
   unrecorded advancing definition so a later transition can append recovery without deleting or
   rewriting the delay occurrence.
3. Reject stale, rebuilt, already-recorded, non-authoritative, wrong-time-effect, null, and
   mismatched state without changing any source. Retain the exact applied result and recovery event
   instances in the accepted candidate.
4. Update README/ROADMAP, run `./scripts/quality.sh`, and review the complete diff. Do not yet select
   or apply recovery, construct or schedule recovery consideration, invoke a model, wire recovery
   into the scene, author evidence, persist state, or add input/UI, navigation, movement,
   perception, dialogue, or other world behavior.
