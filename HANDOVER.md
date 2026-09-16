# Allrpcs Handover

## Current state

PR #284 is merged into `main` at `b8fa5fc`. Branch
`codex/phase-7-influence-effect-flow` advances Phase 7 through four bounded Domain-only slices:

- Phase 7ke binds one exact eligible `influence` occurrence to one exact applicable directional
  target-to-actor relationship change without applying it.
- Phase 7kf applies that exact eligible candidate through the existing immutable relationship
  transition and retains both the exact candidate and fresh relationship result.
- Phase 7kg defines recovery only for the exact current applied result, exact influence-reason
  history tail, same direction and dimension, exact inverse delta, and otherwise applicable change.
- Phase 7kh applies that exact-current recovery as fresh numerically restored relationship state
  while retaining both the influence and recovery reasons.

The branch changes only Domain contracts, Domain tests, README, ROADMAP, and this handover. It adds
no Application, Infrastructure, scene, input, UI, provider, persistence, selection, consideration,
evidence, dialogue, elapsed-time, delay, navigation, movement, world, or perception behavior.
Ordinary startup remains unchanged. The unrelated untracked `scripts/code_statistics.py` remains
untouched and must stay unstaged.

## Validation and review

- Test-first focused host runs pass all 20 influence candidate, transition, recovery-candidate, and
  recovery-transition tests. Their sandbox runs reached VSTest and hit only the known loopback
  `SocketException (13): Permission denied` restriction.
- Final host-local `./scripts/quality.sh` passes workflow validation, restore, formatting, a
  zero-warning build, 2,611 Domain tests, 542 Application tests, 897 Infrastructure tests, 8
  architecture tests, scenario validation, every credential-free Godot smoke, and the final
  Compatibility renderer launch.
- Session review found and fixed a recovery-authority gap: exact-current recovery now additionally
  requires the exact applied influence reason at the relationship-history tail, so a fabricated
  result with unrelated history fails closed. A tautological stale-state assertion was also replaced
  with exact retained-reference checks. The updated 20-test influence suite passes.
- `git diff --check` and the 120-column check pass. Every new file is below 500 lines. No unresolved
  review finding remains.
- PR #284's final run `35097625911` failed all five jobs immediately with `steps: []`; no hosted
  product or packaging step ran. Treat this as Actions/account infrastructure and do not retry an
  unchanged run.

## Exact next bounded slice

Take **Phase 7ki: select one exact eligible influence candidate without applying it**.

Acceptance boundary:

1. Test-first add the smallest Domain-only pure selection over supplied
   `CharacterTargetSabotageInfluenceCandidate` instances, returning the exact first candidate for
   which `CharacterTargetSabotageInfluencePolicy.CanApply` succeeds in stable caller order, or null.
2. Materialize and validate the complete enumerable exactly once before selection; accept at most
   64 candidates, reject null entries and duplicate exact candidate references, and permit distinct
   structurally equivalent candidates. Reuse `ReferenceEqualityComparer`.
3. Prove complete-validation-before-selection, one-pass enumeration, exact returned identity,
   stable ordering, null when none is eligible, unchanged inputs, and a reflection-locked closed
   surface.
4. Update README/ROADMAP, run `./scripts/quality.sh`, and review the complete diff. Do not yet apply
   or recover a selected candidate; add delay duration or elapsed-time state; require or expose
   evidence; add consideration, decision/model, Application, Infrastructure, scene, input, UI,
   provider, persistence, dialogue, navigation, movement, world, or perception behavior.
