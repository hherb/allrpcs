# Allrpcs Handover

## Current state

PR #283 is merged into `main` at `bfcee57`. Branch
`codex/phase-7-character-targeted-access` advances Phase 7 through three bounded Domain-only slices:

- Phase 7kb retains one actor's bounded, stable, value-unique ordinary-interaction target set and
  validates one distinct target together with the actor's required capability and knowledge.
- Phase 7kc retains only non-self `delay` and `influence` proposals and delegates their complete
  eligibility to that unchanged character-target access gate.
- Phase 7kd binds one exact eligible proposal to its exact retained target-character occurrence
  candidate without applying an effect.

Implementation commit `c4dc338` is published in draft PR #284 targeting `main`.

The branch changes only Domain contracts, Domain tests, README, ROADMAP, and this handover. It adds
no Application, Infrastructure, scene, input, UI, provider, persistence, navigation, movement,
dialogue, evidence, elapsed-time, or relationship mutation. Ordinary startup remains unchanged.
The unrelated untracked `scripts/code_statistics.py` remains untouched and must stay unstaged.

## Validation and review

- Test-first focused host runs pass 11 character-target access tests, 12 proposal tests, and 6
  occurrence tests. Their sandbox runs reached VSTest and hit only the known loopback
  `SocketException (13): Permission denied` restriction.
- Final host-local `./scripts/quality.sh` passes workflow validation, restore, formatting, a
  zero-warning build, 2,591 Domain tests, 542 Application tests, 897 Infrastructure tests, 8
  architecture tests, scenario validation, every credential-free Godot smoke, and the final
  Compatibility renderer launch.
- Session review found and fixed one test-surface gap by locking the access snapshot, requirement,
  and policy signatures with reflection-backed assertions. The updated 11-test access suite passes.
- `git diff --check` and the 120-column check pass. No unresolved review finding remains.
- Draft PR #284 run `35097537395` failed all five jobs immediately with `steps: []`; no hosted
  product or packaging step ran. Treat this as Actions/account infrastructure and do not retry an
  unchanged run.

## Exact next bounded slice

Take **Phase 7ke: define one influence-effect candidate without applying it**.

Acceptance boundary:

1. Test-first add the smallest Domain-only immutable influence candidate retaining one exact
   eligible `CharacterTargetSabotageOccurrence` and one exact existing
   `CharacterRelationshipChange`.
2. Add one pure policy that accepts only the `influence` kind after complete occurrence eligibility,
   requires the directional change to belong to the occurrence target and relate that target to the
   proposal actor, and delegates unchanged applicability to the supplied current
   `CharacterRelationshipMemory` through `CharacterRelationshipChangePolicy`.
3. Null, `delay`, mismatched actor or target, inaccessible occurrence, mismatched relationship,
   inapplicable change, and distinct unsupported inputs fail closed without mutating any input. Add
   reflection-backed closed-surface tests and retain exact nested references.
4. Update README/ROADMAP, run `./scripts/quality.sh`, and review the complete diff. Do not yet apply
   a relationship transition; define delay duration or elapsed-time state; add selection,
   consideration, evidence, recovery, Application, Infrastructure, scene, input, UI, provider,
   persistence, dialogue, navigation, movement, world, or perception behavior.
