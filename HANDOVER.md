# Allrpcs Handover

## Current state

`main` contains merged PR #210 at `068b222`. The current branch is
`codex/phase-7-openai-participation-flow`.

This branch adds three bounded Phase 7 slices:

- a strict bounded OpenAI Responses semantic SSE accumulator/completion path returns one exact
  accepted participation decision without exposing structured deltas or retaining provider text;
- `OpenAiCharacterTurnParticipationModel` composes the existing strict request serializer with that
  completion path over caller-owned HTTP, credentials, settings, and an exact validated endpoint;
- the provider-neutral consideration controller can apply one shared deadline across a complete
  sequential decision round without retries or partial results.

The Phase 7 parent items remain open. This branch does not add local Chat Completions, Anthropic, or
Ollama participation dialects; diagnostics; retries; scene-owned participation task state; ordinary
dialogue-flow invocation; autonomous-budget initialization; follow-up selection/start/completion;
multi-character scene hearing/address authoring; a loop; whispering/private conversations;
independent tasks; sabotage; Phase 8 work; release; signing; or distribution.

## Validation and review

- test-first compile failures were observed before the missing OpenAI participation stream,
  completion, adapter, and shared-deadline overload;
- 34 focused stream/completion cases pass for response, interjection, meaningful silence, exact
  scheduled-character and authored-motive identity, supported metadata, malformed/duplicate/
  incomplete/refusal/failed/oversized/trailing events, I/O failure, cancellation, nulls, terminal
  state, bounded surfaces, and provider-text/identifier non-retention;
- 12 focused adapter cases pass for exact authenticated stateless Requests HTTP, exact caller-owned
  endpoint use, strict SSE media type and semantic classification, unavailable credentials,
  redacted HTTP/transport failures, caller cancellation, nulls, provider settings, and smallest
  interface-only behavior;
- 8 focused Application controller cases pass, including complete preflight, exact stable result
  identity, empty rounds, invalid model results, shared deadline expiry without partial return,
  distinct caller cancellation, invalid deadlines, nulls, and bounded public surface;
- final host-local `./scripts/quality.sh` passed workflow validation, restore, formatting, the
  zero-warning solution build, 2,093 Domain, 411 Application, 517 Infrastructure, and 8 architecture
  tests, all 5 representative scenario definitions, the complete headless smoke matrix, and the
  bounded Compatibility-renderer launch; after the final adapter-classification regression, the
  zero-warning solution build and all 518 Infrastructure tests pass;
- `dotnet format Allrpcs.sln --no-restore --verify-no-changes`, `git diff --check`, practical
  500-line/120-character limits, exact-identity boundaries, cancellation/timeout distinction,
  credential handling, response-body non-reading, provider-detail redaction, and scope review pass.

Restricted execution reproduced the known VSTest local-IPC denial; identical focused and full
quality commands passed host-local. PR #210 is verified merged at `068b222`. Its five hosted jobs
failed with `steps: []`, unchanged Actions account-budget infrastructure state rather than product
test evidence; do not retry or dispatch hosted workflows unless that condition changes.

## Exact next bounded slice

Take **Phase 7ag: scene-owned bounded participation-round task flow**.

Acceptance boundary:

1. Add one Godot controller that starts at most one exact participation-decision round through the
   existing deadline overload, owns its cancellation source/task, and installs only one complete
   successful `CharacterTurnParticipationConsiderationRoundResult` on the main thread.
2. Expose only bounded start, cancel, completion/result, and reset state; preflight before model
   invocation; reject overlapping starts; distinguish completed, caller-cancelled, timed-out, and
   failed terminal outcomes without retaining provider details or partial decisions.
3. Add a credential-free gated Godot smoke for exact successful round identity/order, cancellation,
   timeout, model failure, reset, nulls, invalid deadlines, overlapping/repeated operations, and
   preservation of turn, budget, dialogue, transcript, world, and presentation state.
4. Do not compose provider settings or credentials, send network requests, add retries or alternate
   provider dialects, wire `Main` ordinary dialogue submission, initialize a conversation budget,
   select/start/complete a follow-up, author additional scene listeners/addresses, or drive a loop.

Leave both Phase 7 parent items unticked until later slices compose this flow and drive every
ordinary autonomous chain through the existing bounded scene seams.
