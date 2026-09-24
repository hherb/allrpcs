# Phase 7 Acceptance Audit

## Purpose

This audit maps the remaining Phase 7 roadmap statements to current production owners and validation
evidence. It distinguishes reusable contracts from switch-gated smoke paths and from behavior available
during an ordinary play session. A checked foundation or passing smoke is not treated as player-visible
acceptance.

## Shared action boundary

- `CharacterSabotageActionKind.All` closes the vocabulary over Conceal, Move, Substitute, Contaminate,
  Misquote, Delay, and Influence in roadmap order.
- `CharacterObjectSabotageEligibilityPolicy` delegates object actions to `CharacterActionAccessPolicy`,
  which requires the actor's exact physical and informational access snapshots.
- `CharacterTargetSabotageEligibilityPolicy` delegates Delay and Influence to
  `CharacterTargetAccessPolicy`, including actor/target identity and interaction access.
- The seven `CharacterSabotageAction*RequestController` owners revalidate their action-specific policy
  against exact current state before publishing one eligible kind.
- `CharacterSabotageActionRequestComposer` accepts only exact current author outputs, preserves canonical
  order, and rejects an empty or malformed set.
- Domain policy tests cover rejected physical, informational, identity, target, and stale-state inputs.
  Application and Infrastructure tests cover provider-neutral decisions, strict decoding, bounded
  request serialization, SSE completion, and model bindings.
- `CharacterSabotageActionRequestMainSceneSmokeCheck` proves one explicitly invoked Conceal chain.
  Action-specific and outcome smoke checks prove the other branches. These checks are disabled during
  ordinary startup and therefore do not prove that an enemy turn evaluates the seven authors.

## Canonical-kind traceability

### Conceal

- Production owners: `CharacterObjectSabotageConcealmentPolicy`,
  `CharacterObjectSabotageConcealmentTransition`, `CharacterObjectSabotageConcealmentController`, and
  `CharacterObjectSabotageConcealmentRecoveryTransition`.
- Safety and evidence: the occurrence must retain exact access and authoritative-object provenance;
  application changes concealment state and appends fresh target-bound evidence without rewriting the
  authoritative object snapshot.
- Validation: Domain policy, transition, sequence, evidence, and recovery tests; Application decision
  and model-binding tests; strict Infrastructure request/response/SSE tests; isolated controller checks;
  and `WaterCaskSabotageInvestigationFlowSmokeCheck` through ordinary focused search.
- Acceptance status: reusable and investigable when explicitly invoked, but not entered from an ordinary
  enemy turn.

### Move

- Production owners: `CharacterObjectSabotageMovePolicy`, `CharacterObjectSabotageMoveTransition`,
  `CharacterObjectSabotageMoveController`, and `CharacterObjectSabotageMoveRecoveryTransition`.
- Safety and evidence: only the exact authoritative object placement changes; actor placement is not
  changed, destination eligibility is revalidated, and fresh target-bound evidence is retained. Recovery
  restores the exact prior source placement without changing world-object truth.
- Validation: Domain candidate, destination, policy, transition, evidence, and recovery tests;
  Application and strict Infrastructure tests; isolated controller and presentation checks; and
  `WaterCaskMoveSabotageInvestigationFlowSmokeCheck` through ordinary focused search.
- Acceptance status: reusable and investigable when explicitly invoked. Five repeated Move cycles add
  provenance evidence, not broader acceptance, and no further Move cycle is required.

### Substitute

- Production owners: `CharacterObjectSabotageSubstitutionPolicy`,
  `CharacterObjectSabotageSubstitutionTransition`, `CharacterObjectSabotageSubstitutionController`, and
  `CharacterObjectSabotageSubstitutionRecoveryTransition`.
- Safety and evidence: application records an exact evidence-bound substitution snapshot without
  replacing authoritative world-object truth or changing source placements. Recovery retains the source
  objects and exact evidence graph.
- Validation: Domain candidate, snapshot, decision, transition, and recovery tests; Application and
  strict Infrastructure tests; isolated controller/presentation checks; and
  `WaterCaskSubstitutionSabotageInvestigationFlowSmokeCheck` through ordinary focused search.
- Acceptance status: reusable and investigable when explicitly invoked, but not entered from an ordinary
  enemy turn.

### Contaminate

- Production owners: `CharacterObjectSabotageContaminationPolicy`,
  `CharacterObjectSabotageContaminationTransition`, `CharacterObjectSabotageContaminationController`,
  and `CharacterObjectSabotageContaminationRecoveryTransition`.
- Safety and evidence: application records bounded contamination and fresh target-bound evidence while
  retaining authoritative objects. Recovery requires an exact visible-evidence view and does not consume
  evidence or rewrite object truth.
- Validation: Domain policy, sequence, decision, transition, evidence, and recovery tests; Application
  and strict Infrastructure tests; isolated controller checks; and
  `WaterCaskContaminationSabotageInvestigationFlowSmokeCheck` through ordinary focused search.
- Acceptance status: reusable and investigable when explicitly invoked, but not entered from an ordinary
  enemy turn.

### Misquote

- Production owners: `CharacterDialogueMisquotationOccurrencePolicy`,
  `CharacterDialogueMisquotationEvidencePolicy`, `CharacterDialogueMisquotationOccurrenceCoordinator`,
  and the evidence exposure, witness-view, review, presentation, and acknowledgement owners.
- Safety and evidence: the delivered event is retained beside the exact source event rather than
  replacing dialogue truth. Only an exact non-actor witness that heard the delivery can receive evidence;
  the review surface labels source and delivered statements separately.
- Validation: Domain proposal, occurrence, evidence, decision, exposure, and presentation tests;
  Application and strict Infrastructure tests; isolated controller checks; and the switch-gated
  main-scene occurrence-to-review-selection-and-dismissal flow.
- Acceptance status: explicitly invoked evidence is player-reviewable, but no ordinary enemy turn authors
  or delivers the sabotage occurrence.

### Delay

- Production owners: `CharacterTargetSabotageDelayPolicy`, `CharacterTargetSabotageDelayTransition`,
  `CharacterTargetSabotageDelayController`, and the delay-recovery candidate, decision, resolution, and
  acknowledgement owners.
- Safety and evidence: exact interaction access and an authoritative advancing event are required;
  application appends an occurrence and advances qualitative world time. Recovery appends a fresh exact
  recovery occurrence while preserving delay history.
- Validation: Domain access, selection, decision, transition, and recovery tests; Application and strict
  Infrastructure tests; isolated task/resolution/recovery checks; and switch-gated main-scene apply and
  recovery paths.
- Acceptance status: consequential and recoverable when explicitly invoked, but it has no ordinary
  player-visible investigation surface and is not entered from an ordinary enemy turn.

### Influence

- Production owners: `CharacterTargetSabotageInfluencePolicy`,
  `CharacterTargetSabotageInfluenceTransition`, `CharacterTargetSabotageInfluenceController`, and the
  influence-recovery candidate, decision, resolution, and acknowledgement owners.
- Safety and evidence: exact interaction access and the authoritative relationship snapshot are required;
  application appends an exact relationship change. Recovery uses the retained inverse change while
  preserving relationship history.
- Validation: Domain access, selection, decision, transition, and recovery tests; Application and strict
  Infrastructure tests; isolated task/resolution/recovery checks; and switch-gated main-scene apply and
  recovery paths.
- Acceptance status: consequential and recoverable when explicitly invoked, but it has no ordinary
  player-visible investigation surface and is not entered from an ordinary enemy turn.

## Remaining roadmap statements

### Physical and informational ability

The reusable policies and seven exact author owners enforce this statement, but only tests and explicit
smoke drivers currently call the owners. The roadmap item remains unchecked until an ordinary enemy-turn
path evaluates current scene authority and exposes only the eligible canonical kinds.

### Truth, movement, and critical-route safety

The typed transitions preserve truth snapshots, do not move actors, retain append-only histories, and
provide exact recovery routes. The object investigation smokes prove focused-search recovery for Conceal,
Move, Substitute, and Contaminate. Misquote preserves source truth and exposes a witness review. Delay and
Influence retain recoverable history, but neither has a normal player investigation surface. The roadmap
item remains unchecked because a complete critical conclusion has not been exercised through ordinary
sabotage, investigation, and recovery.

### Potentially discoverable evidence

Object actions create fresh target-bound evidence and Misquote creates witness-bound evidence. Delay and
Influence currently retain exact recovery provenance for internal routing, not evidence that an attentive
player can discover. The roadmap item remains unchecked.

### Final acceptance

No ordinary runtime owner currently starts this chain. `Main.tscn` owns the contracts, but each complete
sabotage path is activated only by a smoke environment variable or an isolated test scene. Therefore the
enemy agent cannot yet perform and expose consequential sabotage during ordinary play, and the final
acceptance statement remains unchecked.

## First genuine acceptance gap

The first missing seam is provider-neutral ordinary-turn authoring, not another action implementation.
Add one explicit, one-shot coordinator that accepts the exact current enemy-agent ordinary-turn token and
the current scene authority required by the seven existing request authors. It must invoke those authors in
canonical order, then compose only their exact current outputs through
`CharacterSabotageActionRequestComposer`. Rejected, stale, foreign, repeated, or empty inputs must remain
inert. The slice must stop before supplying a model, starting consideration, reserving another turn,
applying an outcome, adding cadence, or adding UI. Wire it inertly into `Main.tscn` and prove it only with a
switch-gated smoke containing both eligible and ineligible kinds.
