# Allrpcs Handover

## Current state

`main` includes merged PR #289 at `6a54534`. Branch
`codex/phase-7-influence-relationship-authority` advances the influence scene path through three
bounded slices:

- Phase 7ky adds `CharacterTargetSabotageInfluenceController`, which owns one exact authoritative
  relationship, consumes every exact accepted consideration request once, leaves abstention
  referentially and numerically inert, and installs only the existing pure transition's fresh
  relationship plus exact applied result for a selection.
- Phase 7kz adds `CharacterTargetSabotageInfluenceResolutionCoordinator`, which observes each exact
  completed task result once and permits explicit task reset only while the exact resolved
  relationship and applied-result tokens remain current.
- Phase 7la dependency-orders the authority and resolution coordinator in `Main.tscn`. Ordinary
  startup remains uninitialized and inert; the switch-gated credential-free smoke alone initializes
  one exact request relationship and proves selected application, guarded acknowledgement, and
  unchanged applied authority after a later offline failure.

Session review found and fixed one non-consecutive abstention replay gap. The controller now uses
weak exact-request consumption, so an older request cannot become eligible after a later inert
abstention without permanently retaining completed requests.

No slice constructs or schedules requests automatically, authors evidence or recovery candidates,
selects or applies recovery, persists relationship state, or adds input/UI, timing, navigation,
movement, perception, dialogue, or other world behavior. The unrelated untracked
`scripts/code_statistics.py` remains untouched and must stay unstaged.

## Validation and review

- Focused game builds pass with zero warnings, and the isolated authority, completed-result
  resolution, and switch-gated main-scene smokes pass.
- The non-consecutive exact-request replay regression failed before the weak-consumption fix and
  passes after it.
- Sandboxed Godot reproduced the known `user://logs` failure and signal 11, while sandboxed VSTest
  reproduced the known loopback `SocketException (13)`. Identical credential-free host runs pass
  without product changes.
- Final host-local `./scripts/quality.sh` passes workflow validation, restore, formatting, a
  zero-warning build, 2,633 Domain tests, 551 Application tests, 959 Infrastructure tests, 8
  architecture tests, scenario validation, every credential-free Godot smoke, and the final
  Compatibility renderer launch.
- Session-wide review covers exact initialization and current-state gating, selection and abstention,
  non-consecutive replay, weak request retention, one-shot completed-result observation, exact
  acknowledgement tokens, dependency ordering, inert startup, unchanged sources, public surfaces,
  file size, line length, documentation, and whitespace. No unresolved finding remains.

## Exact next bounded slice

Take **Phase 7lb: select one exact eligible influence-recovery candidate**.

Acceptance boundary:

1. Test-first add one pure Domain selection that accepts an exact current
   `CharacterRelationshipMemory` and at most 64 supplied
   `CharacterTargetSabotageInfluenceRecoveryCandidate` instances.
2. Materialize the enumerable once and validate the complete collection before selection. Reject
   null candidates, duplicate exact candidate references, and over-limit input while permitting
   distinct structurally equivalent candidates.
3. Delegate unchanged eligibility to `CharacterTargetSabotageInfluenceRecoveryPolicy` and return
   the exact first eligible candidate in stable caller order, or null when none is eligible. Leave
   every relationship, applied result, candidate, change, and reason instance unchanged.
4. Update README/ROADMAP, run `./scripts/quality.sh`, and review the complete diff. Do not yet author
   recovery candidates or evidence, retain a recovery request, invoke a model, apply recovery, wire
   scene recovery, persist relationships, or add input/UI, timing, navigation, movement,
   perception, dialogue, or other world behavior.
