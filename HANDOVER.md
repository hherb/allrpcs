# Allrpcs Handover

## Current state

`main` contains merged PR #150 at `d230905`. The branch
`codex/phase-3-examination-response` advances Phase 3 through Phase 3m4:

- Phase 3l4 selects a fresh accepted examination response for one exact request and explicitly
  supplied target. Fixed healer speech is paired with an exact-target `examine-object` intention;
  prompt-shaped dialogue remains inert and commitments/proposals remain empty.
- Phase 3l5-l6 permit that exact immediate physical intention to occur only while its exact response
  is speaking and its exact speech is the latest transcript occurrence, then delegate atomic
  authoritative world-event, qualitative-time, and transcript recording.
- Phase 3l7 wires only response selection into the temporary live loop using the exact authored
  herb-patch identifier. The scene still does not execute the examination intention.
- Phase 3m1-m2 define closed `safe-to-collect` / `unsafe-to-collect` examination results linked to an
  exact targeted examination occurrence, then select exact-target collection only for safe results
  and targetless waiting for unsafe results.
- Phase 3m3-m4 permit collection occurrence only after the exact safe examination occurrence is
  latest in the transcript, then delegate the same atomic authoritative occurrence transition.

The live loop still does not source an examination result, execute or record examination or
collection, mutate inventory/world objects, infer targets from text, classify text, stream content,
apply proposals, advance commitments, evaluate general safety, add providers/persistence, or alter
movement and discovery.

## Validation

Every new boundary began with an expected focused failure. Host-local focused checks pass the
exact-target response selector, immediate-intention occurrence policy/transition, closed
examination safety/result contracts, deterministic safe/unsafe next-intention selection, and
safe ordered collection occurrence policy/transition. The affected live-response build and six
Godot dialogue smokes pass with the exact herb-patch examination proposal while proving no action
or world mutation occurs.

The final host-local `./scripts/quality.sh` passes workflow and formatting validation, a
zero-warning build, 1,381 Domain tests, 146 Application tests, 23 Infrastructure tests, eight
architecture tests, the five-evidence representative scenario validator, all 48 opt-in Godot
smokes, and the bounded graphical launch. The restricted sandbox reproduces the known VSTest
loopback denial and Godot `user://logs` crash, so executable test evidence must run host-local. The
GitHub Actions budget is exhausted for the month: do not dispatch hosted workflows, and treat
zero-step hosted failures as account infrastructure rather than test results.

## Exact next bounded slice

Take **Phase 3n1: select one safe examination result for the exact observed herb-patch action**.

Acceptance boundary:

1. Add a pure Application selector accepting one exact observed examination occurrence and one
   exact explicitly supplied safe target `ObjectId`.
2. Return a fresh `CharacterExaminationResult` with `safe-to-collect` only when the occurrence is a
   targeted `examine-object` action whose target is that exact identifier; otherwise fail closed.
3. Prove exact occurrence/target/safety retention, distinct value-equal target rejection, null and
   malformed occurrence rejection, unchanged inputs, and a narrow static public surface.
4. Do not inspect dialogue text or world-object descriptions, wire the selector into Godot, author
   or occur a scene world event, execute/record examination or collection, mutate inventory/world
   objects, infer broader safety, add providers/persistence/streaming, or alter movement/discovery.
