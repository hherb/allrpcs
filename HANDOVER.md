# Allrpcs Handover

## Current state

`main` contains merged PR #209 at `f85a38d`. The current branch is
`codex/phase-7-ordinary-autonomous-chain`.

This branch adds four bounded Phase 7 slices:

- `AutonomousConversationConsiderationSelectionPolicy` budget-gates the existing stable speaking
  decision choice and returns its exact aligned consideration result;
- `AutonomousConversationBudgetController` can use one complete result round to start the selected
  decision's exact heard-event follow-up without advancing the budget before turn completion;
- `CharacterTurnParticipationDecisionJsonDeserializer` strictly decodes a closed bounded JSON
  decision, retaining the exact scheduled-character identifier and exact authored motive;
- `OpenAiCharacterTurnParticipationRequestSerializer` writes one credential-free stateless
  Responses request with bounded output, exact trusted character context, untrusted heard dialogue,
  and a strict matching decision schema.

The Phase 7 parent items remain open. This branch does not complete an OpenAI participation SSE
response, define an OpenAI participation model adapter, add local Chat Completions or Anthropic/
Ollama participation dialects, add retries or timeouts, invoke a participation model from the scene,
initialize or drive an ordinary autonomous chain, change existing turn-response provider behavior,
add whispering/private conversations or independent tasks, implement sabotage, start Phase 8,
release, sign, or distribute.

Draft PR #210 contains this work.

## Validation and review

- test-first compile failures were observed before the missing Domain selection policy, scene
  overload, strict decision decoder, and OpenAI request serializer;
- 6 focused Domain cases pass for response/interjection selection, stable first-speaker order,
  exact result and decision identity, empty/all-silent rounds, exhausted/interrupted budgets,
  nulls, unchanged inputs, and smallest public surface;
- 22 focused Infrastructure decoder cases pass for all three participation kinds, exact identifier
  and motive retention, direct-address rules, malformed/unknown/duplicate/missing structure,
  unavailable motives, transport bounds, nulls, and smallest public surface;
- 3 focused OpenAI serializer cases pass for stateless credential-free request shape, exact bounded
  trusted context, untrusted heard dialogue, strict decoder-aligned schema, and invalid inputs;
- the focused controller game build is warning-free and its host-local Godot smoke passes exact
  result-round start, unchanged budget, exact heard-event retention, nulls, and closed surface;
- final host-local `./scripts/quality.sh` passes workflow validation, restore, formatting, the
  zero-warning solution build, 2,093 Domain, 409 Application, 472 Infrastructure, and 8 architecture
  tests, all 5 representative scenario definitions, the complete headless smoke matrix, and the
  bounded Compatibility-renderer launch;
- `git diff --check`, practical 500-line/120-character limits, exact-identity boundaries, immutable
  input preservation, strict JSON structure, credential-free serialization, and scope review pass.

Restricted execution reproduced the known VSTest local-IPC denial and Godot `user://logs` failure;
the identical focused commands and full quality spine passed host-local. Hosted Actions remained
account-budget blocked with `steps: []` on PR #209; treat any unchanged recurrence as infrastructure
rather than test evidence and do not retry or dispatch workflows.

## Exact next bounded slice

Take **Phase 7ad: strict OpenAI Responses participation SSE completion**.

Acceptance boundary:

1. Add one bounded OpenAI semantic-event accumulator/completion path for an exact
   `CharacterTurnParticipationConsiderationRequest`, using the existing SSE framing reader and the
   new strict decision JSON decoder.
2. Accept only one verified `response.output_text.done` document followed by one successful
   `response.completed`; keep structured JSON deltas non-presentational, bound event/document sizes,
   retain no provider text or identifiers, and return the exact decoded decision.
3. Add focused Infrastructure tests for response, interjection, and meaningful silence; exact
   scheduled-character and motive identity; supported metadata; malformed, duplicate, incomplete,
   refusal, failed, oversized, and trailing events; I/O failure; cancellation; nulls; and smallest
   public surface.
4. Do not send HTTP, load credentials, define the `ICharacterTurnParticipationModel` adapter, add
   local Chat Completions or other-provider completion paths, retry, invoke models from the scene,
   initialize a budget, create/start/complete a follow-up, or drive a loop.

Leave both Phase 7 parent items unticked until later slices adapt provider models and drive every
ordinary autonomous chain through the existing bounded scene seams.
