# Dialogue response repair

The owner approved this distinct runtime successor on 2026-10-16 after two successful spoken turns
were followed by a failed response. [Authority](authority.json) binds the published
[diagnosis and proposal](../player-microphone-qualification/response-failure-diagnosis/README.md).
The earlier missing Ollama participation binding and cached usage repair remain closed.

## Behavior

The Ollama request schema now presents two complete closed response alternatives: speech with a
valid delivery mode, or null speech with null delivery. Supplying all six response properties in
each alternative is necessary for the observed local grammar compiler. Other provider schema
callers retain their prior output. Strict deserialization and intention/world validation remain
unchanged; invalid output is rejected, without forced speech or additional retries.

A valid response without speech now completes through the existing turn/budget transitions. It
creates no speech event, transcript statement or playback notification. Spoken responses still
require their exact recorded occurrence before completion. This prevents a silent response from
occupying the shared turn slot indefinitely and blocking later explicit submissions.

## Regression coverage

Seven delivery-pair examples compare the emitted schema contract to the strict decoder, including
both valid speech modes, valid silence and invalid pairings. Every grammar alternative must declare
the complete closed six-property object. Existing provider serialization checks remain in place.

`game/DialogueResponseRepairMainCheck.tscn` exercises ordinary Main with isolated model settings,
fictional input, a nearby Whisper listener and disabled microphone/playback configuration. It first
checks unbudgeted silence and the spoken-event completion fence, then eight budgeted submissions:
ordinary speech, whispered reply, silence, a deliberately invalid pair, another reply, silence and
two later replies. It checks fresh result identity, exactly one model request per submission,
correct terminal state, transcript counts, budget counts and no duplicate completion.

The same fixture's explicit `live` mode uses selected local `gemma4:e4b` with eight fictional inputs.
It requires a distinct completed generation and terminal activity for each submission. Silence
remains valid, and later submissions must still start. No model comparison or personal dialogue is
used. The shared diagnostic HTTP observer recognizes the updated grammar and preserves the three
existing response/silence/provider-failure checks.

The fixture waits for settled participation, terminal activity and completed generation before
releasing Main, then allows native cleanup before exit. Quality checks the eight distinct result
receipts and rejects an ObjectDB cleanup-leak warning. This addresses the disposable prototype's
premature teardown warning without changing product lifecycle or cancellation owners.

## Verification checkpoint

The focused 18 provider serializer tests, game build and old response/silence/failure checks passed.
The new controlled fixture passed eight distinct requests, including the deliberate InvalidResponse
and successful later generations. The local-model fixture passed eight distinct completed requests;
turn six was valid silence and turns seven/eight produced new completed generations. Both new
fixtures exited without an ObjectDB cleanup warning. The verbose host logs include a default
hostfxr lookup notice before successful resolution of the installed Homebrew .NET runtime.

The first full root `./quality.sh` run reached the cancellation smoke fixture and exposed an old
setup assumption: it used a silent response to hold a speaking turn open. The fixture now holds
unrecorded speech instead, with all existing cancellation/state assertions retained. Its timeout,
cancel, complete and failed modes pass. The [retained failure](quality/attempt1/summary.json), exact
fixture diff and 4,943-input receipt document the adaptation. [Guarded recovery](finish-quality.py)
checks all source/CI/log bytes and the exact fixture change, reuses only 21 unchanged historical/art
boundary passes, and reruns continuation plus every product, smoke and remaining runtime stage.
The guard self-test rejects source, log, failure and fixture drift. This is a recovered full gate,
not an uninterrupted first-pass result.

The recovered full gate passed 215 recorded stages and all 5,636 unit tests, with
no skipped unit tests. [Verification](verification.json) binds the final source, raw logs and gate
receipt. Every product and runtime stage was rerun after the fixture adaptation; 21 unchanged
historical/art boundary passes were reused. The complete smoke script and the new eight-turn
controlled regression passed. The final eight-turn local-model check also passed with settled
cleanup. Some retained legacy smoke fixtures emit nonfatal ObjectDB warnings; the new controlled
and live repeated-turn fixtures emitted none. This is not a warning-free platform certification.

[Closure](closure.json) closes only this approved runtime successor. The human trial remains
partial: three complete qualitative passes, five partial cases and seven unperformed cases.
These fictional-input checks do not qualify microphone accuracy, echo safety or playback audibility.
The exact unlogged human failure cause remains unconfirmed. Resume the owner-operated repeated
Whisper/reply observation beside the listener, retaining all earlier successes and the failure.

Raw logs retain their exact bytes and are excluded from source whitespace checks. The historical
fixture diff is retained verbatim for review. No frozen receipts, prior closure hashes or unrelated
editor/Finder state were modified.
