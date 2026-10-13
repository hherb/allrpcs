# Moonshine player speech integration

The owner selected **Moonshine Small Streaming** on 2026-10-14 under the existing dialogue integration
grant. [Authority](authority.json) binds that request, the closed comparison and standing publication.
The selected implementation is Moonshine Voice 0.1.5, streaming architecture 4, with the evaluated
English `quantized_26_08_21` and German `quantized_26_08_24` models. [models.json](models.json) pins all
sixteen asset files. Accepted Pocket service/playback, ASR comparison, art/audio and domain packages
remain unchanged.

## Player interaction

Open **Settings → Dialogue**. The added row contains **Hold to talk**, **Listen (headphones)**,
**Stop mic** and a live microphone state. Speech language uses the existing English/German selector.
Capture starts off. Hold the talk button, or focus it and hold Space; release to finalize the draft.
Wait for **Listening** before speaking: each session starts the recognizer and verifies its models
before capture opens, so cold Python/model startup can miss words spoken during **Starting microphone**.
Moving keyboard focus away from a held talk button also releases capture. Stop mic immediately retires
capture/recognition while preserving the visible draft. Optional listening ends after at most about
30 seconds, or when explicitly stopped.

Partial and final recognition update an editable draft, appending to existing typed text. Manual
changes fence all later automatic writes for that session. Final utterances remain stable; earlier
words are not replaced by a late partial. **Submit is still the original explicit action**, using the
current corrected words and unchanged Whisper choice. Recognition never submits dialogue, invokes
intention tools, changes acoustic/domain hearing rules or executes a voice-command vocabulary.
Typed interaction remains available when Python, models, permissions or devices are unavailable.

Hold to talk interrupts reply audio immediately. Optional listening also interrupts the Pocket
producer and buffered PCM on a capture-level activity cue: at least 96 ms of continuous mono RMS
at or above 0.02, rearmed after 500 ms of quiet. Moonshine utterance-start events provide a later
additional cancellation cue. This early detector is a prototype **audio-activity** trigger, not
speaker identification or echo cancellation. The closed comparison showed that 100 ms decoder
updates alone did not make Moonshine's utterance-start callback immediate. Headphones and actual
human trials are required before claiming reliable hands-free barge-in with this prototype.

A new session, Stop, Submit, panel close, focus loss, successful load or language switch drops retired
text generations. Default-input identity changes, stream/read failures, buffer overflow, malformed
PCM/protocol or recognition failure stop input safely. The UI retains starting/listening/finalizing/
stopping indicators rather than claiming a live microphone has already retired. These controls join
the contained ten-control dialogue keyboard cycle and preserve existing pointer operation/world gates.

## Capture and local worker boundary

Provider-neutral `IMicrophoneCapture` supplies immutable mono PCM to `ISpeechRecognizer`.
`PlayerSpeechSession` owns bounded per-generation text queues, cancellation, explicit capture finish
and a 30-second audio limit. No application speech contract contains executable/model paths or
Domain action authority. Replaced generations cannot update the new draft even if a provider ignores
cancellation. Scene teardown stops input and awaits every outstanding producer.

Infrastructure owns separate shell-free Python process roles. The capture role uses sounddevice 0.5.6
and PortAudio's default input, with 32 ms blocking mono reads at the device's supported default rate,
capped at 48 kHz. It never opens an output stream or monitors microphone audio through the game mixer.
The recognizer role uses explicit local paths and architecture 4, verifies every asset hash, and feeds
PCM to Moonshine with a 100 ms update floor. Moonshine handles sample-rate conversion. Processes and
stderr are retired/drained without ordinary audio/text diagnostic storage. IPC messages are bounded
to 32,000 characters and PCM frames to 4,096 samples; recognition text is capped at 2,000 characters.
SDK event callbacks are translated into opaque utterance IDs with ordered partial/final notifications.
No downloader, remote recognizer, SDK agent/intent framework or automatic execution is composed.

Permission is managed by the OS when the user opens capture. Errors/denial or unavailable devices
return text fallback through the platform-neutral boundary; this slice does not modify OS privacy
settings or claim a separately verified permission flow on every platform. Device-change detection
covers changes reported by the default-device descriptor and read/stream errors. Indistinguishable
physical devices and silent OS denial still need real host qualification.

## Developer setup

The ignored `artifacts/player-speech-runtime.json` composes the staged evaluation Python environment
and model cache. `ALLRPCS_PLAYER_SPEECH_CONFIG` may select another fully qualified local configuration.
The JSON is bounded to 16 KiB and requires exactly:

```json
{
  "schemaVersion": 1,
  "pythonPath": "/absolute/path/to/python",
  "workerPath": "/absolute/path/to/scripts/player-speech-worker.py",
  "englishModelPath": "/absolute/path/to/small-streaming-en/quantized_26_08_21",
  "germanModelPath": "/absolute/path/to/small-streaming-de/quantized_26_08_24",
  "manifestPath": "/absolute/path/to/speech-direction/player-asr-integration/models.json"
}
```

Use the retained evaluation environment with `moonshine-voice==0.1.5`, `sounddevice==0.5.6` and
`numpy==2.5.4`. It is a developer composition, not a packaged installer. Restore/provision assets
explicitly using the [closed comparison setup](../asr-evaluation/README.md), then verify the manifest;
ordinary startup never fetches missing models. Only documented Finder `.DS_Store` metadata is excluded
from model-directory inventory; model bytes remain hash-bound. Existing Pocket config stays separate.
Normal `godot --path game` discovers both ignored configurations. OS microphone permission may be
associated with the local Python/app host identity; confirm that identity in a real session.

## Verification

[Verification](verification.json) binds final product sources, models/runtime, actual Main reports,
source/fixture provenance, native screenshots, focused tests and the full quality report. The final
root gate passed 206 stages and 5,594 unit tests; all six retained Main modes passed.
All qualification input is synthetic: neither native fixture nor the Python capture tests open a
physical microphone. Actual English/German Moonshine inference is fed retained WAV PCM through
Main's ordinary draft and explicit-submit pathway. Exact tested English/German WAV inputs are
retained in `inputs/` and hash-bound in verification. Supplied role/reply models and synthetic reply
PCM keep tests independent of conversational-provider credentials.

Application tests cover draft ordering/finality, manual correction, bounded text, opt-in capture,
finish versus cancellation, late obsolete callbacks, early audio activity/rearming and capture failure.
Infrastructure tests cover exact PCM/language, worker retirement, simulated permission/device errors,
malformed event sequences and capture-source failure. Python tests exercise explicit start, PCM bounds,
stream closure, synthetic permission denial/default-device change and malformed IPC without hardware.
Native 1280×720 and 1920×1080 tests exercise keyboard hold-to-talk, pointer listening/Stop, editable
correction, unchanged Submit/Whisper semantics, reply interruption, lifecycle/failure and focus containment.
A participation-idle gate, valid addressed/silent decisions and per-submission model-use check
prevent old dialogue-round completion from satisfying the new fixture. No-auto-submit checks count
player speech specifically, allowing unrelated world/physical transcript entries to continue. Native
tests cap rendering at 60 FPS so their frame watchdog cannot expire before paced synthetic clips finish.

The actual-backend fixtures preserve negation but make game-vocabulary substitutions (English
“moonwork” and German “Mondrautenfahn” in the retained reports). This is integration evidence, not
a natural-speaker accuracy benchmark. Manual correction and explicit submission remain necessary.

The accepted seven-control Pocket fixture runs unchanged in snapshot `07bd7a48`, with cold
restore/build/import. Current ASR checks supplement it; no frozen validator or source receipt was
rewritten. The unrelated `game/project.godot` editor SHA remains
`36bfb7c7a54399cc8d27c02b73c7990766d7db44e7452655cb6a496866653fca` and is excluded from publication.

```sh
dotnet test tests/Application.Tests/Allrpcs.Application.Tests.csproj --filter FullyQualifiedName~Tests.Speech
dotnet test tests/Infrastructure.Tests/Allrpcs.Infrastructure.Tests.csproj --filter FullyQualifiedName~Tests.Speech
artifacts/asr-evaluation/python/bin/python scripts/test-player-speech-worker.py
python3 quality.py --self-test
python3 scripts/check-continuation.py --self-test
./quality.sh
```

The opt-in Main fixture requires `ALLRPCS_PLAYER_ASR_CHECK=1`, `ALLRPCS_PLAYER_ASR_MODE` and an absolute
`ALLRPCS_PLAYER_ASR_REPORT`. Modes are `headless`, `unavailable`, `native-1280`, `native-1920`,
`moonshine-english-1280`, `moonshine-german-1280`. Set ordinary provider composition disabled and the
shared participation supplier to 0. Use missing player/reply configuration paths to permit injection.
Actual Moonshine modes additionally require `ALLRPCS_PLAYER_ASR_RUNTIME` and `ALLRPCS_PLAYER_ASR_FILE`
(the retained mono PCM16 16 kHz synthetic clip). Run native windows serially on the authorized host.

## Physical qualification still missing

The implementation is ready for an explicitly enabled human session. Start with headphones or
hold-to-talk and verify: microphone permission/host identity, one real English/German utterance,
partial/final draft correction, exact explicit Submit and Whisper, Stop/release/close/focus/load,
default-device loss/recovery, quiet/noisy input and accents. Speaker-on feedback must be tested
separately; an energy/VAD cue can recognize the game's own output. Do not disable/revoke OS privacy
settings or persist personal audio without an explicit session grant. Record honest not-performed
results when hardware/operator conditions are absent.

Actual microphone/permission and echo behavior, other-platform execution, M1/16 GB, combined
local-LLM/speech memory/frame budgets, finalized casting and packaged Python/model distribution
remain unverified. The new [privacy update](../../release-direction/player-asr-privacy-update.md)
describes what changed. The model catalogue's current MIT designation does not complete future
binary-distribution attribution or release certification.

Primary references: [Moonshine transcription](https://moonshine-voice.readthedocs.io/en/latest/using/transcription/)
and [PortAudio/sounddevice input streams](https://python-sounddevice.readthedocs.io/en/0.5.5/api/streams.html).
Pinned installed runtime and source/model hashes in verification are authoritative for this prototype.
