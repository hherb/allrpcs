# Allrpcs Handover

## Current state

`main` contains merged PR #149 at `5bb91bd`. The branch
`codex/phase-3-dialogue-response-flow` advances Phase 3 through:

- Phase 3k7 receives one fresh deterministic response for the exact live awaiting request on the
  following process pass, preserving a cancellable awaiting window and producing exact speaking
  activity without executing its fixed wait intention.
- Phase 3k8 presents only the complete exact healer speech in the dialogue panel.
- Phase 3k9 records that exact speech once as its own heard `DialogueEvent` and transcript entry;
  cancelled or non-speaking activity cannot fabricate speech.
- Phase 3k10 completes the exact speaking activity only after its event and transcript occurrence
  have been observed, preserving a one-frame speaking cancellation window.
- Phase 3l1-l3 add pure start eligibility/transition contracts, delegate them from the Godot
  controller, and allow later heard utterances to start distinct turns only after the current
  activity is completed or cancelled. Awaiting and speaking activity remains non-replaceable.

The live deterministic loop now supports consecutive player/healer speech pairs through awaiting,
speaking, and completed states. It still does not classify text, stream content, select an
examination target, execute or record response intentions, apply proposals, advance commitments,
evaluate safety, collect objects, add providers/persistence, or alter movement and discovery.

## Validation

Every slice began with an expected focused failure at its missing boundary. Focused host-local
checks pass response reception, complete-response presentation, exact one-time healer speech,
post-speech completion, consecutive turns, active-turn preservation, and awaiting/speaking
cancellation. The pure start policy/transition pass 13 focused Domain tests. The final host-local
`./scripts/quality.sh` passes workflow and formatting validation, a zero-warning build, 1,343 Domain
tests, 139 Application tests, 23 Infrastructure tests, eight architecture tests, the five-evidence
representative scenario validator, all 48 opt-in Godot smokes, and the bounded graphical launch.

The restricted sandbox reproduces the known VSTest loopback denial and Godot `user://logs` crash,
so executable test evidence must run host-local. The GitHub Actions budget is exhausted for the
month: do not dispatch hosted workflows, and treat zero-step hosted failures as account
infrastructure rather than test results.

## Exact next bounded slice

Take **Phase 3l4: select one deterministic examination response for an exact supplied target**.

Acceptance boundary:

1. Add a pure Application selector that accepts one exact `CharacterTurnRequest` and one exact
   supplied target `ObjectId`, and returns a fresh accepted `CharacterTurnResponse` retaining that
   request with fixed healer speech and an immediate `examine-object` intention retaining that
   exact target.
2. Prove null rejection, exact identity retention, deterministic fixed speech, valid intention
   shape, empty commitments/proposals, unchanged inputs, and prompt-shaped request text containment.
3. Do not parse or classify text, look up or mutate a world object, wire the selector into the live
   scene, execute or record the intention, infer an examination result, decide safety or collection,
   advance a commitment, add providers/persistence/streaming, or alter movement and discovery.
