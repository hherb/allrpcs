# Optional Pocket speech in player dialogue

The owner requested integration of the previously selected dialogue speech pipeline, then asked
for ASR evaluation with streaming, voice activation and interruption. [Authority](authority.json)
records those instructions. This slice integrates **Pocket reply playback** and completes the
[English/German ASR comparison](../asr-evaluation/README.md); actual microphone/ASR composition
still follows the recognizer choice. The closed service and auditions remain unchanged.

## Implemented behavior

Open Settings → Dialogue and enable **Speak replies**. Select English or German; **Stop speech**
interrupts the current reply. Speech starts disabled each session. These controls join the existing
keyboard focus cycle and retain native pointer operation, text editing, Submit, Close and the
explicit Whisper delivery choice. The existing Audio panel's **Speech** channel controls gain/mute.

Only a recorder notification emitted **after the exact reply event and transcript commit** can
start synthesis. The Seeker must be in the recorded audience and remain within the retained
speaker/range. The text passed to Pocket is the exact accepted reply; speculative model deltas
are never spoken. Default stock profiles remain the accepted English Alba/German Juergen fixtures;
Healer/Sage currently share that prototype profile, not a finalized bilingual cast or acting style.

The Godot sink feeds mono 24 kHz PCM into a custom-rate stereo generator on the existing Speech
bus. At most four immutable chunks wait behind the consumer, plus one current chunk and the
0.25-second generator buffer. It takes and ends the existing Speech ducking lease. Stop/disable,
a new reply, a newly submitted player delivery, mapped character-turn cancellation when admitted,
explicit language change, successful load, focus loss, mute/range loss, reported output-device
change or playback failure discard obsolete queued PCM and release the lease. Stop does not
cancel an already committed dialogue event or change its audience, transcript, recipe, save or world.

Scene teardown stops the mixer synchronously and begins asynchronous provider retirement; the
fixture explicitly awaits retirement before quitting. The unchanged Pocket process also handles
closed input by shutting down. Provider failure, malformed/missing local configuration or unavailable
assets leaves typed dialogue and the existing full-text response/transcript owners available.
The existing anonymous conversation cue/caption/history is retained as its own accepted owner.
No new microphone recording, automatic transcript submission, command vocabulary, speech-driven
hearing rule, physical controller evaluation or packaged release is installed by this slice.

## Local development setup

The current checkout has an ignored `artifacts/speech-runtime.json` pointing at the already staged
Pocket interpreter and assets. Normal `godot --path game` discovers it; a fully qualified
`ALLRPCS_SPEECH_CONFIG` can select another explicit local configuration. The UI never displays
runtime paths. Configuration is bounded to 16 KiB and requires exactly these fields:

```json
{
  "schemaVersion": 1,
  "pythonPath": "/absolute/path/to/pocket-python",
  "workerPath": "/absolute/path/to/allrpcs/scripts/pocket-speech-worker.py",
  "assetsPath": "/absolute/path/to/allrpcs/artifacts/speech-models/pocket-tts-3.3.0",
  "profilesPath": "/absolute/path/to/allrpcs/speech-direction/pocket-tts-service/voice-profiles.json"
}
```

All four paths must be fully qualified; the required files/directory must exist. This is an explicit
developer composition, not a shipped model installer or automatic download. The current temporary
Python environment can disappear; follow [the Phase 10 setup](../../HANDOVER_phase10.md#local-setup-and-reproducible-commands)
to restore only the selected Pocket assets, preserving frozen preparation evidence.
Start with speech disabled if unavailable; text works without Python/models or a speech device.

## Verification and retained evidence

[Verification](verification.json) binds the final product sources, authority, unit checks, actual
Main reports, native UI/audio and root quality. Native tests use synthetic input and supplied
credential-free role/reply models through ordinary Main; the Pocket component then generates
real PCM. This is integration evidence, not a live LLM conversation, microphone/player trial or
physical-device qualification.

- Application tests cover bounded producer backpressure, stop/disposal, empty/failed synthesis,
  exact PCM and replacement even when an old provider ignores cancellation.
- Current headless Main checks disabled speech/text behavior, committed reply output, exact words,
  one synthesis per commit, stop/retired PCM, new player submission, language/disable, master mute,
  contained keyboard focus and scene retirement. Missing runtime has a separate text-fallback mode.
- Native 1280×720/1920×1080 tests additionally use actual engine pointer events for Speak replies
  and Stop, inspect the complete seven-control focus cycle and retain screenshots. Their scaled
  pointer coordinates use the viewport's final transform.
- Real English/German Pocket runs record the actual native Master mixer at 48 kHz stereo PCM16,
  prove completion/drain and stop a subsequent long reply. Mixer recordings have nonzero output
  and remain byte-for-byte retained. They measure integration, not speaker latency or loudness
  qualification of a fresh physical device.
- Frozen dialogue-accessibility checks now run at the unchanged accepted `e7b17575` snapshot,
  including restore/build/import. Current speech checks are additional stages. No accepted validator,
  acceptance hash, service source, artwork or audio asset was edited.
- The first full run correctly rejected an appended speech node after the accepted final
  `WholeChannelPresentation` root owner. Its failed report/logs and diagnosis remain in
  [attempt1](quality/attempt1/diagnosis.json). Only the new speech nodes moved before that owner;
  its five sprites, transforms, masks, child order and original validator remain unchanged.
  The fresh full run passed the corrected owner order and all 198 preceding stages. Its new speech
  fixture then exposed an inherited participation supplier flag collision, retained in
  [attempt2](quality/attempt2/diagnosis.json). Only the two new speech modes override that flag to 0.
  [The guarded finish tool](finish-quality.py) verifies the exact prior report, every log, 5,130 product
  input hashes and a runner diff limited to that one flag. It reuses the 198 unchanged passes and
  reruns both speech modes. The final [200-stage summary](quality/final/summary.json) passed, with
  **5,581 unit tests** and no skipped tests. Both failed attempts remain evidence.

The unrelated editor rewrite remains SHA-256
`36bfb7c7a54399cc8d27c02b73c7990766d7db44e7452655cb6a496866653fca` and is excluded from publication.
The original dialogue/controls/audio scopes remain closed. New ASR source must use the completed
comparison, preserve explicit corrected-text submission and address real speaker/microphone echo
before claiming reliable voice-activated barge-in.

## Reproduction

```sh
dotnet test tests/Application.Tests/Allrpcs.Application.Tests.csproj \
  --filter FullyQualifiedName~Tests.Speech
dotnet test tests/Infrastructure.Tests/Allrpcs.Infrastructure.Tests.csproj \
  --filter FullyQualifiedName~Tests.Speech
dotnet build game/Allrpcs.Game.csproj
python3 scripts/check-continuation.py --self-test
python3 quality.py --self-test
./quality.sh
```

The explicit Main fixture uses `ALLRPCS_DIALOGUE_SPEECH_CHECK=1`, a fully qualified
`ALLRPCS_DIALOGUE_SPEECH_REPORT`, and modes `headless`, `unavailable`, `native-1280`, `native-1920`,
`pocket-english-1280` or `pocket-german-1280`. Disable ordinary provider composition/settings/credential
reads with the retained test flags. Use a missing `ALLRPCS_SPEECH_CONFIG` for synthetic and unavailable
modes; use the staged local config for Pocket. Run native windows serially on the authorized host.

M1/16 GB, other-platform execution, combined game/local-LLM/speech memory/frame times, physical
microphone permission/device loss, accent/noise performance, echo control, finalized expressive
casting and packaged Python/model distribution remain unverified. This closure concerns the named
Pocket Main composition and ASR evaluation, not all remaining Phase 10 items.
