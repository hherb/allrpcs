# Allrpcs Handover

## Current state

`main` contains merged PR #193 at `c18a492`. The current branch is
`codex/phase-6-experiment-recording`.

Phase 6 now includes authoritative ingredient-administration experiment recording, a bounded
hypothesis-recognition foundation, and recoverable critical-conclusion authoring after the earlier
disease, protection, assessment, care, preparation, journal, and investigation work.

An administration experiment can be recorded only when its exact authored `WorldEvent` is retained
and its exact event identity is the newest authoritative log entry. One pure transition then creates
the exact experiment record and delegates duplicate and capacity enforcement to the existing journal
append transition. It does not infer efficacy, consume dosage, or change disease truth.

A proposed hypothesis match is canonicalized against an exact open question and exact authored
hypothesis while retaining the original natural-language utterance. The result remains only a
possible answer and cannot create a conclusion, evidence, belief, fact, or gameplay mutation. No
provider/interpreter transport supplies the structured match yet, so the roadmap's end-to-end
natural-language recognition item remains unchecked.

Critical evidence now records a distinct authored source identity and whether cooperation is
required. A pure policy requires every critical conclusion plan to have at least two routes using at
least three sources, one cooperation-independent route, and a surviving complete route after any one
source is lost. The representative first-patient investigation passes with four sources and two
disjoint routes. This completes the roadmap's route/recovery authoring item, but it does not yet
author the patient, remedy inventory, treatment events, or recovery flow.

Universal experiment capture, structured hypothesis transport, first-patient disease/remedy state,
treatment-effect application, scene ownership, persistence, provider wiring, consequence execution,
and phase acceptance remain open.

## Validation and review

- complete host-local `./scripts/quality.sh` passed: workflow validation, restore, formatting, and
  zero-warning solution build
- 1,852 Domain, 393 Application, 423 Infrastructure, and 8 architecture tests passed
- representative discovery scenario validated 5 evidence definitions
- all 77 credential-free Godot smokes passed
- bounded 60-frame Compatibility-renderer launch passed on Godot 4.6.3 .NET
- focused test-first checks passed for all five slices; the staged diff passed whitespace and
  120-character review before the full quality run

The first aggregate quality run transiently stalled while launching the already-built scenario
validator after every .NET test had passed. The validator, Godot smokes, and graphical launch then
passed separately, and one clean aggregate retry passed end to end without a code change.

## Exact next bounded slice

Take **Phase 6n1: authored first-patient and stabilizing-remedy foundation**.

Acceptance boundary:

1. Add one immutable stabilizing-remedy definition linking an exact disease definition to an exact
   preparation recipe without directly changing disease truth.
2. Add one immutable first-patient scenario aggregate retaining the exact patient identity, disease
   truth, ingredient state, remedy definition, and existing representative critical-investigation
   scenario.
3. Add one pure structural policy requiring the exact patient infection and disease definition,
   recipe inputs and available exact ingredient definitions/units, and an acceptable critical-route
   scenario. Reject value-equal replacement definitions and mismatched patient state.
4. Add one pure representative source for grey sleep, a pre-stabilization first patient, moonwort
   ingredients, the cooling recipe, and the existing recoverable investigation. Add focused
   reflection-backed tests for exact-instance retention, structural failures, unchanged inputs, and
   the minimal public surfaces.
5. Do not yet occur administration events, consume reusable dosage, apply stabilization or cure,
   mutate disease truth, record experiments automatically, add provider recognition transport,
   execute consequences, wire scene ownership or persistence, or mark the first-patient roadmap item
   complete.
