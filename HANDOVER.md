# Allrpcs Handover

## Current state

`main` includes merged PR #295 at `e2e87f7`. Branch
`codex/phase-7-influence-recovery-resolution` advances recovery authority through two bounded game
slices:

- Phase 7lr adds `CharacterTargetSabotageInfluenceRecoveryResolutionCoordinator`, which observes
  each exact completed recovery result once and permits explicit task reset only while the exact
  resolved relationship and latest applied recovery-result tokens remain current.
- Phase 7ls dependency-orders recovery authority and result resolution in `Main.tscn`. Ordinary
  startup remains uninitialized and inert; the switch-gated credential-free smoke alone initializes
  one exact request relationship and proves selected recovery, guarded acknowledgement, and
  unchanged restored authority after a later offline failure.

The branch changes only the bounded coordinator, its isolated smoke, main-scene dependency order
and smoke, the Godot smoke script, README, ROADMAP, and this handover. It does not automatically
schedule work, author candidates or evidence, connect influence output to recovery input, persist
relationships, or add UI/input, elapsed time, navigation, movement, perception, dialogue, or other
world behavior. The unrelated untracked `scripts/code_statistics.py` remains untouched and must stay
unstaged.

Implementation commit `147b687` is published in draft PR #296 targeting `main`.

## Validation and review

- Test-first host-local smokes failed on the absent coordinator and absent main-scene authority
  nodes before their smallest implementations were added.
- Focused host-local builds and the isolated recovery-resolution and switch-gated main-scene smokes
  pass with zero warnings.
- Final host-local `./scripts/quality.sh` passes workflow validation, restore, formatting, a
  zero-warning build, 2,655 Domain tests, 560 Application tests, 1,028 Infrastructure tests, 8
  architecture tests, scenario validation, every credential-free Godot smoke, and the final
  Compatibility renderer launch.
- Draft PR #296 is open. All five hosted jobs completed as failures with empty step lists, so no
  hosted product test or development-build step ran; this is Actions/account infrastructure state
  and the unchanged jobs were not retried.
- Session-wide review covers exact one-shot observation, selected and abstaining resolution,
  relationship and latest-result authority tokens, stale and duplicate failure, explicit reset,
  dependency order, inert startup, unchanged source graphs, closed public surfaces, documentation,
  file size, line length, formatting, and whitespace. One documentation-precision issue was fixed;
  no unresolved finding remains.

## Exact next bounded slice

Take **Phase 7lt: produce one exact recovery candidate from current applied influence authority**.

Acceptance boundary:

1. Test-first add one isolated scene controller that accepts the influence controller's exact
   current `LatestAppliedResult` and one exact caller-authored inverse relationship change.
2. Publish a fresh `CharacterTargetSabotageInfluenceRecoveryCandidate` only while the influence
   result's exact relationship remains the influence controller's current authority and the
   unchanged recovery policy accepts the inverse change.
3. Reject uninitialized, missing, stale, value-equal rebuilt, repeated, misdirected,
   wrong-dimension, wrong-delta, and null input without replacing an already current candidate or
   changing the influence result, relationship, change, or reason graph.
4. Update README/ROADMAP, run `./scripts/quality.sh`, and review the complete diff. Do not yet wire
   this source into `Main.tscn`, create a consideration request, start recovery work automatically,
   author discoverable evidence, persist relationships, or add UI/input, timing, navigation,
   movement, perception, dialogue, or other world behavior.
