# Allrpcs Handover

## Current state

PR #142 merged the Phase 3a1-3c4 dialogue foundation into `main` at `78195b7`. The branch
`codex/phase-3-turn-response` completes Phase 3c5-3c10:

- `CharacterTurnDisposition` defines the exact closed accept, refuse, clarify, and conditionally
  accept vocabulary.
- `CharacterCommitment` retains a non-empty ordered sequence of exact proposed intentions without
  progression or execution authority.
- `CharacterStateProposal` retains a canonical key and exact nonblank value bounded to 500 UTF-16
  code units; it is proposed character-owned text, not world truth or mutation authority.
- `CharacterTurnResponse` retains the exact request and disposition, optional speech from the
  responder, one immediate intention, copied commitments, and at most eight copied state proposals.

`ROADMAP.md` marks the provider-neutral character-turn response complete. Deterministic behavior,
intention validation or execution, commitment progression, proposal validation or application,
transcripts, events, providers, persistence, scene wiring, player input, and UI remain incomplete.

## Validation

Focused test-first runs passed for each contract after the expected missing-type/member failures.
The sandbox reproduced the known VSTest loopback denial; identical host runs are authoritative.
The complete host `./scripts/quality.sh` run passed workflow and formatting validation, a
zero-warning build, 1,215 Domain tests, 129 Application tests, 23 Infrastructure tests, eight
architecture tests, the five-evidence representative scenario validator, all 33 opt-in Godot
smokes, and the bounded graphical launch.

## Exact next bounded slice

Take **Phase 3d1: select predictable initial deterministic-character dispositions**.

Acceptance boundary:

1. Add one pure Application selector over the request's explicitly declared `DialogueActType`:
   request -> accept, command -> refuse, question -> clarify, conditional plan -> conditionally
   accept, and every other defined act -> refuse.
2. Compose fixed responder-authored speech, `wait` as the one immediate intention, and empty
   commitment/proposal collections; retain the exact request in the response.
3. Prove prompt-shaped text cannot alter selection beyond its separately declared act type.
4. Do not classify text; inspect world, perception, knowledge, safety, or affordances; execute an
   intention; advance commitments; apply state; add providers, persistence, scene wiring, input,
   transcript/event integration, streaming, cancellation, or UI.
