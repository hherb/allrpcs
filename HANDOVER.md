# Allrpcs Handover

## Current state

PR #141 merged the remaining Phase 2 scenario-validation work into `main` at `35f4bb9`. The branch
`codex/phase-3-dialogue-foundation` completes the reviewed Phase 3a1-3c4 dialogue foundation:

- `DialogueUtterance`, `DialogueAudienceSnapshot`, and the pure Application
  `DialogueHearingSelector` retain exact typed identifiers and exact bounded Unicode speech,
  select every listener inside an inclusive finite range in stable order, and compose a matching
  immutable `DialogueEvent` without Godot or scene state.
- The closed `DialogueActType` vocabulary represents commands, requests, questions, speculation,
  warnings, reported claims, promises, and conditional plans. `DialogueAct` retains the exact
  utterance plus an explicitly declared type; it does not classify text or grant instruction or
  action authority.
- `CharacterTurnRequest` is provider-neutral and accepts only a responder in the event's exact
  audience plus an act for the same utterance.
- The initial closed immediate-intention types are wait, examine object, and collect object.
  `CharacterIntention` retains an optional exact target, and the pure validation policy reports
  missing or unexpected target errors without world lookup, authorization, execution, or mutation.

`ROADMAP.md` marks only the five completed Phase 3 contracts. The dialogue panel, response schema,
deterministic character and behaviours, commitments, transcript/event-log integration, streaming,
cancellation, persistence, scene wiring, player input, and providers remain incomplete. No release,
signing, or distribution work was performed.

## Validation

After code review, `dotnet format Allrpcs.sln --no-restore --verify-no-changes` passed. The complete
host `./scripts/quality.sh` run passed workflow and formatting validation, a zero-warning build,
1,180 Domain tests, 129 Application tests, 23 Infrastructure tests, eight architecture tests, the
five-evidence representative scenario validator, all 33 opt-in Godot smokes, and the bounded
graphical launch. The first focused VSTest run reproduced the known sandbox loopback denial;
focused and complete host runs are authoritative.

## Exact next bounded slice

Take **Phase 3c5: define the immutable character-turn disposition vocabulary**.

Acceptance boundary:

1. Add a closed provider-neutral vocabulary for accept, refuse, clarify, and conditionally accept,
   with stable canonical text, order, parsing, and exact singleton identity.
2. Prove the complete closed surface and fail-closed null, unknown, and noncanonical parsing.
3. Do not compose the character-turn response or add speech, intention execution, commitments,
   state proposals, behavior selection, a deterministic character, world lookup, model/provider
   contracts, persistence, scene wiring, Godot input, streaming, cancellation, transcript, or UI.
