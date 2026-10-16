# Repeated response failures — diagnosis and proposed repair

The owner reported two successful spoken dialogue turns followed by failure on 2026-10-16.
The screenshot shows an ACCEPTED submission, Whisper enabled, FAILED character response,
interrupted reply speech, English selected and the previous reply still displayed. No personal
speech, audio or screenshot is retained. These observations do not establish a playback fault.

## Reproduction

[Eight fictional turns](before.json) through ordinary Main at `1f90251a` with local
`gemma4:e4b` produced seven completed generation results and one `InvalidResponse` on turn six.
Turns seven and eight completed, so this test found no fixed two-turn exhaustion. The failing
response had `speech: null` with `deliveryMode: "whispering"`, a valid wait intention and empty
commitment/proposal arrays. The deserializer requires both speech and mode to be null for no
speech, so rejection is correct. The schema currently permits both properties independently;
instructions request the correct pairing but do not constrain the structured output grammar.

This reproduces the failure class; the human session retained no detailed failure code, so an
exact match to that failure is unproven. The diagnostic uses fictional input, isolated settings,
a nearby listener and disabled microphone/playback configuration. An initial exploratory attempt
selected an examine action and was stopped before a repeated-turn conclusion; the retained run
requests waiting and repositions the listener within Whisper range before each submission.
The diagnostic checks generation outcomes, not physical hearing, audio or full turn completion.

## Disposable prototype result

The combined [prototype](prototype.json) completed eight distinct generation requests. The
sixth input returned valid silence and later submissions started and completed new generations.
This is not eight spoken replies or a physical pass. Each schema alternative contains the complete
six-property response object. An earlier partial-property alternative was incompatible with local
Ollama grammar compilation; a subsequent observer-only attempt did not reach reply HTTP until
the diagnostic observer was adapted to recognize the new schema shape. Neither exploratory attempt
changed product source. The retained prototype logs correspond to the final combined result. The engine emitted an
ObjectDB cleanup-leak warning at diagnostic exit; this is not a clean full runtime gate. A successor
regression fixture must await/cancel outstanding work and verify cleanup before publication.

## Proposed bounded runtime successor — approval required

The existing human slice explicitly excludes new runtime fixes. The previously approved missing
participation binding and cached metadata repair remain closed. This proposal is a distinct
response-schema defect found during the resumed physical trial; it is not a reopening of those
completed deliverables.

[Proposed patch](proposal.patch) constrains the Ollama structured response schema to the two
existing valid delivery shapes: speech with a delivery mode, or null speech with null mode.
It preserves the existing deserializer, legitimate silence, hearing, participation decisions,
intention eligibility, retries, cancellation, ASR, playback and other provider schemas.
The second finding is a silent-response lifecycle stall: response reception enters Speaking even
when speech is absent, but TemporaryCharacterTurnCompletionController waits for an exact spoken
event and returns early forever when there is no speech. The [schema-only prototype](schema-only.json)
completed six new generations, including valid silence on turn six; submissions seven/eight
started no new generation. Their stale Completed values must not be counted as new successes.
The proposed patch also completes silent responses through the existing turn/budget completion
owners without inventing a speech event or triggering playback. The patch is tested only in a
disposable snapshot until the owner approves implementation.

Authorized implementation would be limited to:

- `src/Infrastructure/Dialogue/CharacterTurnResponseJsonSchemaWriter.cs`: optional Ollama-only
  delivery-pair constraint, retaining unchanged schema output for other callers.
- `src/Infrastructure/Dialogue/OllamaCharacterModelRequestSerializer.cs`: opt into that constraint.
- `game/TemporaryCharacterTurnCompletionController.cs`: complete valid silent responses through
  existing completion and budget transitions, preserving exact speech-event checks for spoken turns.
- Focused Infrastructure regression tests for both valid delivery shapes and both invalid pairings;
  retain existing malformed-response rejection and provider serialization tests.
- `game/OllamaParticipationMainCheck.cs` and a bounded repeated-turn successor fixture: adapt
  the test HTTP observer to recognize complete schema alternatives, add controlled multiple-turn
  coverage, corresponding quality integration, and source-bound successor verification/closure
  documentation.

Completion requires meaningful deterministic regression coverage, controlled multi-turn Main
coverage, rerunning the eight fictional local-model turns with a new result for each submission, the full root quality gate, review,
commit and push. Keep the physical session partial; the owner then resumes the human dialogue
check. No retries or forced speech are added to conceal malformed output, and no physical pass
is inferred from the local-model diagnostic.

## Repeat commands and verification

Run `python3 speech-direction/player-microphone-qualification/response-failure-diagnosis/reproduce.py`
for the unpatched committed baseline, or `prototype.py` in the same directory for the disposable
proposed patch. Both create temporary source snapshots and use fictional inputs and isolated
settings. They write ignored diagnostic artifacts, never the running game source or saved settings.
The scripts run local inference and require the already selected `gemma4:e4b`. They check generation
results; review newResult and replyCalls rather than interpreting process exit or stale terminal
status alone as a success gate.

Documentation checks: continuation sync/check and 11 regression probes, JSON parsing, 15-case
totals, diagnostic script syntax, patch applicability and diff whitespace passed. Product and CI
inputs are unchanged, so the prior full quality evidence is reused. The prototype has only its
isolated build/import/eight-turn evidence; full product quality is required after implementation.

Raw logs and unified-patch context bytes are retained unchanged and excluded from source whitespace
checks. Their hashes are bound in verification.json; the proposed patch passes git apply --check.
