# Allrpcs Handover

## Current state

`main` includes merged PR #323 at `e6586f7`. Branch
`codex/phase-7-sabotage-action-outcome-binding` completes **Phase 7pg**: one acknowledged,
selected action-specific result can now bind once to its exact current typed authoritative outcome.
Implementation commit `b1d1cbd` is published in draft PR #324 targeting `main`.

`CharacterSabotageActionSpecificOutcomeCoordinator` accepts only Phase 7pf's exact acknowledged
generic author request, strongly typed candidate binding, composed action-specific request, selected
typed result, and branch owner's retained exact acknowledgement tokens. It exposes only the selected
typed outcome:

- concealment, movement, substitution, and contamination retain their exact post-action object-state
  and evidence snapshots;
- misquotation retains its exact current exposed-evidence snapshot; and
- delay and influence retain their existing exact applied result.

The seven existing presentation/resolution coordinators now retain internal exact result and
post-action acknowledgement tokens. This closes the post-acknowledgement race: later unrelated,
rebuilt, replaced, or stale authority cannot be rebound as the acknowledged result's outcome.
Abstention, unavailable, pre-acknowledgement, wrong-kind, foreign, null, non-current, and repeated
paths remain inert.

`Main.tscn` dependency-orders the coordinator after Phase 7pf. The existing switch-gated main-scene
smoke now selects the exact concealment candidate and proves generic selection, typed composition,
start, application, presentation, result observation, acknowledgement, and exact one-shot outcome
binding. Ordinary startup remains inert.

This slice adds no recovery consideration or application, autonomous scheduling, new evidence or
truth mutation, persistence, provider transport, credentials, input/UI, navigation, movement,
perception, dialogue delivery, timing, or new Domain effect logic. The unrelated untracked
`scripts/code_statistics.py` remains untouched and must stay unstaged.

## Validation and review

- Test-first evidence: the game build failed with `CS0246` for the absent
  `CharacterSabotageActionSpecificOutcomeCoordinator` before the production seam was added.
- The final host-local `./scripts/quality.sh` passes restore, formatting, a zero-warning build,
  2,731 Domain tests, 586 Application tests, 1,220 Infrastructure tests, all 8 architecture tests,
  scenario validation, every credential-free Godot smoke, and the Compatibility renderer launch.
- The initial sandboxed quality run reached the zero-warning build and then hit the known VSTest
  loopback `SocketException (13): Permission denied`; the identical host-local run is the passing
  product evidence.
- Session-wide review covered all seven typed branches, exact generic and action-specific request
  provenance, exact candidate membership, acknowledgement-to-outcome tokens, post-acknowledgement
  replacement safety, abstention, null/repeated failure, scene dependency order, ordinary-startup
  inertness, public-surface containment, documentation, file size, formatting, and whitespace.
- Review found and fixed one provenance gap: current branch authority alone could have advanced
  after acknowledgement. Each branch owner now retains the exact acknowledged result and outcome
  tokens, and the binder requires those unchanged instances. The final full quality run passes
  after this fix.
- Draft PR #324's Headless Godot and three unsigned development-build jobs failed with `steps: []`;
  no checkout, build, test, smoke, or export step ran. The Pure C# job was still queued with
  `steps: []` at the recorded head, so hosted checks are Actions/account infrastructure state and
  not hosted product-test evidence.

## Exact next bounded slice

Take **Phase 7ph: gate one retained exact sabotage outcome for its existing recovery or evidence
route without starting recovery**.

Acceptance boundary:

1. Test-first add one explicit, one-shot eligibility coordinator that accepts only Phase 7pg's exact
   retained author request and selected typed outcome while every corresponding branch-owned
   acknowledgement token and current authority remain the same instances.
2. Preserve compile-time-typed branches. For concealment, move, substitution, and contamination,
   retain only the exact inputs needed by their existing recovery policy and require the existing
   evidence/perception gate before publishing eligibility. For delay and influence, accept only the
   exact applied result already recognized by their existing recovery-candidate source. For
   misquotation, retain only the existing exact exposed-evidence/view token; do not invent a new
   recovery contract.
3. Stale, rebuilt, replaced, foreign, wrong-kind, null, repeated, abstaining, unacknowledged,
   ineligible, and non-current-authority paths must fail closed. Rejected paths must not consume a
   model, publish a recovery request, or change any source, evidence, perception, presentation,
   event-log, world-time, or relationship state.
4. Dependency-order the coordinator in `Main.tscn`. Extend the switch-gated concealment path only
   far enough to prove exact eligibility publication after the retained Phase 7pg outcome while
   ordinary startup remains inert.
5. Do not start or apply recovery, remove evidence, reveal or conceal truth, schedule another
   sabotage action, persist state, or add input/UI, provider, timing, navigation, movement,
   perception mutation, dialogue delivery, or new Domain behavior.
6. Update README/ROADMAP, run `./scripts/quality.sh`, review the complete diff, and leave the next
   slice to route one exact eligible branch into its already-existing recovery/evidence operation
   through a separately bounded explicit coordinator.
