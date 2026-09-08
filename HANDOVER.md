# Allrpcs Handover

## Current state

PR #143 merged the Phase 3c5-3c10 character-turn response contracts into `main` at `001dfe7`.
The branch `codex/phase-3-deterministic-character` continues Phase 3 through these reviewed slices:

- Phase 3d1 adds the pure Application `DeterministicCharacterTurnResponseSelector`. It maps only
  the explicitly declared dialogue-act type to fixed responder-authored speech and accept, refuse,
  clarify, or conditionally-accept disposition; every response waits and has no commitments or
  state proposals. Prompt-shaped text cannot override the declared type.
- Phases 3e1-3e3 add immutable `CharacterCommitmentProgress` plus pure progression eligibility and
  transition rules. Only the single value-equal pending intention advances, exactly one position
  per observation, without executing actions or mutating prior state.
- Phases 3f1-3f4 add closed character-turn activity states, exact request/response composition, and
  pure cancellation eligibility and transition. Awaiting-response and speaking activity can become
  cancelled; completed and already-cancelled activity fail closed.

`ROADMAP.md` now marks the deterministic character behaviors and one-observation-at-a-time
commitment progression complete. Runtime cancellation, player input, scene wiring, transcripts,
event integration, providers, persistence, streaming, and UI remain incomplete.

## Validation

Each contract was added test-first after its expected missing-type failure. Focused host tests pass;
the sandbox reproduces the known VSTest loopback denial. After code review, the complete host
`./scripts/quality.sh` run passed workflow and formatting validation, a zero-warning build, 1,265
Domain tests, 139 Application tests, 23 Infrastructure tests, eight architecture tests, the
five-evidence representative scenario validator, all 33 opt-in Godot smokes, and the bounded
graphical launch. Draft PR #144 was opened. Its first published head `0378cec` was mergeable, but
all five hosted jobs were rejected before running any steps because an Actions budget prevented
further use; this is hosted infrastructure/account state, not a hosted test result.

## Exact next bounded slice

Take **Phase 3g1: define the closed character-turn transcript-entry kind vocabulary**.

Acceptance boundary:

1. Add exact singleton kinds for spoken statement and physical action, with stable canonical text,
   order, parsing, and fail-closed null, unknown, and noncanonical parsing.
2. Prove the complete public surface without composing a transcript entry or changing an event log.
3. Do not add payloads, transcript storage, physical-action execution, world-event conversion,
   persistence, providers, streaming, scene wiring, player input, cancellation orchestration, or UI.
