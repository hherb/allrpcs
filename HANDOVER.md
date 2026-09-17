# Allrpcs Handover

## Current state

`main` includes merged PR #298 at `4560552`. Branch
`codex/phase-7-influence-recovery-coordination` advances the exact influence-recovery scene chain
through two bounded slices:

- Phase 7lz adds `CharacterTargetSabotageInfluenceRecoveryStartCoordinator`. One explicit call
  requires the influence controller's exact latest result and relationship, unused candidate and
  request sources, uninitialized recovery authority, and an idle recovery task. It validates a
  prospective inverse candidate and request before publishing state or consuming the queued model,
  then creates the exact candidate/request, initializes recovery authority, and starts that request
  once.
- Phase 7ma dependency-orders that coordinator in `Main.tscn` after its exact scene dependencies.
  The existing switch-gated influence smoke now uses the coordinator for its full
  influence-to-recovery chain, while ordinary startup remains inert.

The branch changes only the bounded coordinator, its isolated smoke, the existing switch-gated
main-scene influence smoke, main-scene dependency order, smoke script, README, ROADMAP, and this
handover. It does not autonomously start recovery, infer or author inverse evidence, apply elapsed
time, persist relationships, or add UI/input, navigation, movement, perception, dialogue, or other
world behavior. The unrelated untracked `scripts/code_statistics.py` remains untouched.

## Validation and review

- Test-first validation produced the expected `CS0246` for the absent coordinator and the expected
  missing-node failure for the absent `Main.tscn` owner before the smallest implementations were
  added.
- The focused zero-warning game build, isolated coordinator smoke, and switch-gated main-scene
  influence-to-recovery smoke pass host-locally.
- Final host-local `./scripts/quality.sh` passes workflow validation, restore, formatting, a
  zero-warning build, 2,655 Domain tests, 560 Application tests, 1,028 Infrastructure tests, all 8
  architecture tests, scenario validation, every credential-free Godot smoke, and the final
  Compatibility renderer launch. The first sandboxed run reached the same zero-warning build but
  VSTest could not bind its loopback socket; the identical host-local run is the product evidence.
- Session-wide review covers preflight-before-publication ordering, exact result/relationship and
  nested graph retention, queued-model preservation, one-shot ownership, task/authority gates,
  inert startup, scene dependency order, unchanged source graphs, closed public surfaces,
  documentation, file size, line length, formatting, and whitespace. No unresolved finding remains.

The implementation is not yet committed or published.

## Exact next bounded slice

Take **Phase 7mb: define one exact target-delay effect candidate without applying it**.

Acceptance boundary:

1. Test-first add one immutable Domain candidate that binds an exact eligible
   `CharacterTargetSabotageOccurrence` to one exact caller-authored `WorldEvent` whose time effect
   advances meaningful world time.
2. Add one pure policy that accepts only `delay`, delegates the unchanged character-target
   occurrence/access gate, and rejects influence, wrong actor/target, inaccessible, non-advancing,
   null, or rebuilt/mismatched input without changing the occurrence, actor, target, access, event,
   or world-time graph.
3. Retain exact nested instances and keep the public surface get-only, bounded, and free of elapsed
   application, selection, recovery, provider, scene, persistence, UI/input, navigation, movement,
   perception, dialogue, or other world behavior.
4. Update README/ROADMAP, run `./scripts/quality.sh`, and review the complete diff. Do not yet apply
   the delay event, advance world time, create a consideration request/model flow, wire `Main.tscn`,
   or infer/authenticate evidence.
