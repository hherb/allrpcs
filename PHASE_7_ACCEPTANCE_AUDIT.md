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
- `CharacterSabotageOrdinaryTurnAuthoringCoordinator` now preflights one exact current awaiting enemy
  turn and complete scene graph, invokes the seven existing authors in canonical order, and composes
  only their exact eligible outputs without partial publication.
- `CharacterSabotageOrdinaryTurnConsiderationStartCoordinator` starts only that exact composed request
  through the existing provider-neutral generic task flow while retaining the exact turn provenance.
- `CharacterSabotageOrdinaryTurnResultObservationCoordinator` observes only the exact task-installed
  completed selection while the complete turn, authoring, canonical selected output, candidate binding,
  and scene graph remain current.
- `CharacterSabotageOrdinaryTurnResultBindingCoordinator` delegates the existing selected-author binding
  and acknowledges only that exact observed result, retaining its exact Delay request and candidate while
  resetting only the completed generic task.
- `TemporaryCharacterSabotageOrdinaryTurnAuthoringSource` supplies representative exact Delay and
  Influence inputs for the switch-gated proof; it is not an ordinary-play scheduler or live authority.
- Domain policy tests cover rejected physical, informational, identity, target, and stale-state inputs.
  Application and Infrastructure tests cover provider-neutral decisions, strict decoding, bounded
  request serialization, SSE completion, and model bindings.
- `CharacterSabotageActionRequestMainSceneSmokeCheck` proves one explicitly invoked Conceal chain.
  Action-specific and outcome smoke checks prove the other branches. These checks are disabled during
  ordinary startup. The separate ordinary-turn smoke now evaluates all seven authors, selects, observes,
  binds, and acknowledges one exact Delay result, but does not yet compose or start its typed request.

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

The reusable policies and seven exact author owners enforce this statement. The new explicit ordinary-
turn path evaluates current scene authority and exposes only eligible canonical kinds, including a
mixed eligible/ineligible graph. Its only caller remains a switch-gated smoke, so the roadmap item stays
unchecked until ordinary play supplies a real enemy turn and proceeds through the guarded path.

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

`Main.tscn` now owns explicit ordinary-turn authoring, canonical composition, and generic consideration
start seams, but only a smoke environment variable calls them. No ordinary scheduler supplies a real
enemy turn, and the exact completed generic result is not yet observed or routed through an action-
specific request. Therefore the enemy agent still cannot perform and expose consequential sabotage
during ordinary play, and the final acceptance statement remains unchecked.

## Next genuine acceptance gap

Observe only the exact completed generic result started by the ordinary-turn coordinator while its exact
turn, actor, request, author outputs, task result, and current scene graph remain unchanged. Retain that
result once without acknowledgement, action-specific request composition, typed consideration, outcome
application, turn completion, cadence, provider work, or UI. Rebuilt, foreign, stale, running, failed,
replaced, and repeated inputs must remain inert, and ordinary startup must still perform no work.
