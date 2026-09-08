# Allrpcs Handover

## Current state

`main` contains merged PR #145 at `2390fb9`. The branch
`codex/phase-3-event-log-boundary` continues Phase 3 through four reviewed pure Domain slices:

- Phase 3h1 adds immutable `CharacterTurnWorldEventOccurrence`, retaining one exact authoritative
  `WorldEvent` and one exact structurally tagged `CharacterTurnTranscriptEntry`.
- Phase 3h2 adds pure eligibility that accepts only the exact event instance retained by the
  authored snapshot and delegates existing event-log and qualitative-time eligibility.
- Phase 3h3 adds an immutable result retaining the exact occurrence, existing world-event result,
  and resulting transcript.
- Phase 3h4 atomically records the authoritative event identifier, applies its authored time
  effect, and appends the exact transcript entry. Speech and physical actions remain structurally
  distinct; failed eligibility leaves every input unchanged.

`ROADMAP.md` now marks event-log separation complete. Response proposals still have no execution
authority. Runtime cancellation, action execution, persistence, providers, streaming, scene
wiring, player input, and UI remain incomplete.

## Validation

All four slices were added test-first after their expected missing-type or missing-member failures.
The 18 focused host-local tests pass. The restricted sandbox still reproduces the known VSTest
loopback denial; host-local runs are required for executable test evidence. The complete host-local
`./scripts/quality.sh` run passed workflow and formatting validation, a zero-warning build, 1,307
Domain tests, 139 Application tests, 23 Infrastructure tests, eight architecture tests, the
five-evidence representative scenario validator, all 33 opt-in Godot smokes, and the bounded
graphical launch.

The GitHub Actions budget is exhausted for the month, so hosted CI is not expected to execute.
Draft PR #146 is open and mergeable. All five hosted jobs on its first published head were rejected
before running any steps because of that budget. Treat local results as the available validation
evidence, not as hosted-platform proof.

## Exact next bounded slice

Take **Phase 3i1: add a focused runtime character-turn activity controller boundary**.

Acceptance boundary:

1. Initialize exactly once from one exact `CharacterTurnActivity`; expose that exact current
   immutable activity and reject null initialization.
2. Delegate cancellation eligibility and transition to the existing Domain contracts. Successful
   cancellation installs the returned activity; uninitialized, completed, and already-cancelled
   cases leave state unchanged.
3. Prove awaiting-response and speaking cancellation in a focused opt-in Godot smoke without
   adding a new player input path.
4. Do not cancel executed actions, generate responses, append transcripts, record world events,
   apply proposals, add persistence, providers, streaming, dialogue UI, or player-input wiring.
