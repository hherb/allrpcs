# Allrpcs Handover

## Current state

`main` contains merged PR #148 at `c6d09d1`. The branch
`codex/phase-3-dialogue-turn-flow` continues Phase 3 through these bounded slices:

- Phase 3k4 adds `TemporaryCharacterTurnRequestSource`. It accepts an exact occurred
  `DialogueEvent`, exact supplied healer identifier, and explicit `DialogueActType`; value-equal
  audience membership gates a new request that retains those exact inputs and the event's exact
  utterance. Missing listeners and null inputs fail closed. It performs no text classification or
  state mutation.
- Phase 3k5 removes the synthetic startup activity from the live scene. The first fresh accepted
  player utterance now occurs as an exact in-world event, appends as exact transcript speech, and—if
  no turn is active—starts one exact awaiting-response healer activity with the explicitly supplied
  `request` act type. Later dialogue still becomes distinct event/transcript occurrences but cannot
  replace the active turn. Cancellation smokes now start through this live player-input path.
- Phase 3k6 adds `TemporaryCharacterTurnResponseSource`. It delegates an exact request to the
  existing pure deterministic selector and returns a fresh complete response retaining that exact
  request. Prompt-shaped text remains ordinary in-world content. The source does not install,
  display, append, complete, or execute any part of the response.

`README.md` and `ROADMAP.md` record the live request boundary and credential-free response-source
boundary. These slices do not infer act types from text, replace an active turn, receive or display
the selected response, append healer speech or actions, execute intentions or proposals, complete
the Phase 3 acceptance scenario, add providers/persistence/streaming, or alter movement and
discovery behavior.

## Validation

Each slice began with an expected failure at its missing boundary. The complete host-local
`./scripts/quality.sh` run passes workflow and formatting validation, a zero-warning build, 1,330
Domain tests, 139 Application tests, 23 Infrastructure tests, eight architecture tests, the
five-evidence representative scenario validator, all 43 opt-in Godot smokes, and the bounded
graphical launch. The focused coverage includes temporary request/response sources, the
occurred-dialogue request flow, dialogue input/event/transcript integration, and awaiting/speaking
player cancellation through a live request.

The restricted sandbox still reproduces the known Godot `user://logs` crash, so executable Godot
evidence must run host-local. The GitHub Actions budget is exhausted for the month: do not manually
dispatch hosted workflows, and treat any zero-step hosted failures as infrastructure/account state
rather than test results.

## Exact next bounded slice

Take **Phase 3k7: receive one complete deterministic response for the exact live awaiting request**.

Acceptance boundary:

1. After a player event starts an awaiting healer activity, use the temporary response source once
   for that exact request and delegate the exact complete response to
   `CharacterTurnActivityController.TryReceiveResponse`.
2. Prove the resulting speaking activity retains the exact live request and exact selected response,
   including fixed healer speech and the unexecuted wait intention. Prompt-shaped player text must
   remain ordinary input, and later player dialogue must not replace the speaking turn.
3. Preserve the player and healer transcript/event boundaries and unrelated world, discovery,
   movement, and presentation state. Do not display or stream healer speech, complete the speaking
   turn, append healer speech or a physical action, execute the wait intention or any proposal, add
   providers/persistence, or alter movement and discovery behavior.
