# Allrpcs Handover

## Current state

`main` contains merged PR #144 at `803503b`. The branch
`codex/phase-3-transcript-boundary` continues Phase 3 through these reviewed slices:

- Phase 3g1 defines the closed `CharacterTurnTranscriptEntryKind` vocabulary with canonical
  `spoken-statement` and `physical-action` singleton values, stable order, strict parsing, and a
  bounded public surface.
- Phase 3g2 defines an immutable tagged `CharacterTurnTranscriptEntry`. Speech retains its exact
  `DialogueUtterance`; a physical action retains exact actor and non-wait `CharacterIntention`
  instances. The constructors make the alternate payload unavailable and reject nulls.
- Phase 3g3 defines an immutable ordered `CharacterTurnTranscriptSnapshot` that copies its caller
  collection, rejects nulls, retains exact entry instances, and permits repeated occurrences.
- Phase 3g4 defines the pure `CharacterTurnTranscriptAppendTransition`. It returns a new snapshot
  with the exact supplied occurrence appended and never mutates its inputs.

`ROADMAP.md` marks transcript separation complete while leaving event-log separation incomplete.
No response proposal is automatically recorded as an executed action. Runtime cancellation,
event-log integration, action execution, persistence, providers, streaming, scene wiring, player
input, and UI remain incomplete.

## Validation

All four slices were added test-first after their expected missing-type failures. Focused host tests
pass; the restricted sandbox reproduces the known VSTest loopback denial. The final host
`./scripts/quality.sh` run passed workflow and formatting validation, a zero-warning build, 1,289
Domain tests, 139 Application tests, 23 Infrastructure tests, eight architecture tests, the
five-evidence representative scenario validator, all 33 opt-in Godot smokes, and the bounded
graphical launch.

The GitHub Actions budget is exhausted for the month, so hosted CI is not expected to execute.
Treat the local results above as the available validation evidence, not as hosted-platform proof.

## Exact next bounded slice

Take **Phase 3h1: define one immutable character-turn world-event occurrence boundary**.

Acceptance boundary:

1. Retain one exact authoritative `WorldEvent` definition and one exact
   `CharacterTurnTranscriptEntry` occurrence without copying, normalizing, or flattening either.
2. Prove null rejection and the complete immutable public surface. Preserve the transcript entry's
   structural speech/action distinction; do not infer occurrence from a proposed response.
3. Do not record the event identifier, append the transcript, execute an action, assign identifiers,
   decide qualitative-time effects, add persistence, providers, streaming, scene wiring, player
   input, cancellation orchestration, or UI.
