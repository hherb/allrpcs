# Streaming ASR evaluation — English and German

The owner requested: “Yes, lets evaluate ASR options. Ideally streaming, voice activated, and
interruptable”. This finite comparison ran on the current Apple M3 Max / macOS 27.0.1. It complements
the [Pocket dialogue playback integration](../dialogue-integration/README.md). No microphone was
opened, no player dialogue was submitted, and no recognizer has yet been installed into Main.

## Recommendation

**Moonshine Small Streaming is the preferred next integration candidate** for the requested live
partial text and voice activation. Keep Whisper small + Silero VAD as the measured alternative if
English word accuracy proves more important in player trials. Both need editable confirmation;
neither is qualified for unattended execution of recognized dialogue. For responsive barge-in,
qualify a capture-level speech-start signal or hold-to-talk trigger separately from the recognizer's
utterance callback; the measured Moonshine onset below is not an immediate interruption signal.

| Measured implementation | Streaming/VAD behavior | Synthetic-reference word mismatches | Interruption/recovery |
| --- | --- | --- | --- |
| Moonshine Voice 0.1.5, quantized Small Streaming, separate English/German models | Actual line-start/change/completion events with internal VAD. First nonempty partial 1.05–1.57 s from stream start, including 0.5 s added leading silence. Final completion 0.20–0.38 s after source clip end | English 3/44 words (6.8%); German 5/42 (11.9%). No text on the two silence/noise controls per language | Stop + close about 1.85 ms English / 2.02 ms German; no callbacks after close in the bounded check. Stop can finalize incomplete text, so invalidate the application generation first |
| whisper.cpp v1.9.5, multilingual small, Metal, four threads, Silero VAD v6.2.0 | Batch model with external VAD and rolling prefix re-decode; this is not intrinsic incremental streaming. Warm complete requests 0.11–0.15 s here; first English request 0.95 s. Prefixes revise earlier wording | English 0/44 (0%); German 5/42 (11.9%). VAD suppresses both silence/noise controls per language | Closing an active HTTP request invokes the native abort callback. A subsequent clean request completed in 0.30 s including previous cleanup; exact kernel-stop time was not measured |

The corpus has **four distinct synthesized phrases**, each clean and with seeded 15 dB white noise,
plus silence/noise-only controls. Word counts above combine the clean/noisy variants. These are
case/punctuation-insensitive mismatches against the text supplied to Pocket, not an independently
human-annotated natural-speech WER study. Noise variants are correlated, not independent speakers.
Both models retained the negations in the tested English/German remedy phrases. Both still need
correction of rare terms: Moonshine wrote “moonwork” and “Mondrautenfahn”; Whisper's German compound
also varied. This does not reopen the accepted TTS intelligibility decision or prove a synthesis defect.

## What the comparison establishes

Moonshine received 100 ms PCM chunks with a 500 ms update interval, paced to the audio clock.
Its actual event timeline is retained, including hypotheses that changed before completion.
The first `LineStarted` notification arrived 1.04–1.56 seconds from stream start, including
0.5 seconds of added silence. That is the observed utterance-start callback at this update interval,
not a sub-100-ms human barge-in result; the short stop/close measurement begins only once cancellation
is requested. Lower-latency voice-start scheduling needs qualification in the microphone composition.
A supplemental **100 ms update** run on the same clips kept the same 3/44 English and 5/42 German
mismatches and empty silence/noise controls. Its `LineStarted` range remained 1.04–1.55 seconds from
stream start; first partials were 1.06–1.63 seconds and finalization 0.18–0.27 seconds after clip end.
Stop/close was 1.87–2.10 ms. This small trial did not demonstrate a materially earlier voice-start
callback from changing the update interval alone. Results and both evaluator versions are retained.
English/German stream loads were about 0.30/0.32 seconds. Observed loaded Python-harness RSS was
561/496 MiB; later process peaks include the harness, resampling, model-file hashing and native
allocations. They are not standalone model weight sizes or a combined game/LLM memory measurement.
The persistent Whisper + VAD server sampled about 747 MiB RSS on the host; Metal/unified-memory
accounting and different process contents prevent treating this as a precise apples-to-apples peak.

Whisper ran with one persistent model server on numeric loopback. The comparison retained both
its initial no-VAD baseline and the subsequent supported Silero-VAD configuration. The no-VAD
baseline emitted annotation-like text on silence/noise; that is why VAD was included in the final
comparison. Do not compare raw no-VAD silence behavior to Moonshine's gated path as equivalent.

The Whisper interruption probe sent 52.64 seconds of repeated synthetic input, closed the socket
about 107 ms after sending, and verified a fresh clean request. The inspected pinned server source
uses `req.is_connection_closed()` for its native `abort_callback`. This demonstrates the chosen
server boundary's abort/recovery path, not a measured human barge-in or a promise that every compute
kernel stops immediately. Moonshine's stream stop can produce final text; an application must drop
those retired-generation events rather than submit them.

## Voice activation and game speech feedback

Internal VAD is useful for splitting player speech into utterances. It does **not** distinguish a
person speaking from the game's own loudspeaker output. This corpus is itself Pocket speech and
both recognizers transcribed it, so an ungated microphone hearing those speakers can cause a false
player turn. These experiments did not include a room, physical microphone, echo cancellation,
headphones, changing distance, accents or a noisy environment.

The next composition should therefore provide:

- An explicit listening toggle with a visible microphone state; default off and local PCM only.
- Streaming draft text, separate from submitted dialogue. Partial/final updates must not overwrite
  text the player has begun editing. Submit remains the existing explicit player action.
- Immediate cancellation of reply synthesis **and its playback buffer** on a qualified player
  speech start. Generation IDs must reject late ASR and TTS results after interruption, language
  switch, device loss, close, focus loss or load.
- A hold-to-talk fallback. Voice activation/barge-in should first be evaluated with headphones;
  built-in speakers need actual echo-control qualification before claiming reliable hands-free use.
- Explicit English/German selection initially, bounded utterances, recognizer/model absence fallback,
  and content-free runtime diagnostics. Acoustic volume must not change the existing Whisper checkbox
  or domain hearing rules.

The voice-start handler can interrupt audio immediately while transcription continues to form an
editable draft. It must not turn the recognition framework's command/intent tools into a new
player-facing command vocabulary or bypass the game's current dialogue/world authority.

## Sources and reproducibility

Current primary sources describe [Moonshine's streaming German/English models](https://moonshine-voice.readthedocs.io/en/latest/models/available-models/),
[explicit local asset loading](https://moonshine-voice.readthedocs.io/en/latest/using/downloading-models/),
and [stream event APIs](https://github.com/moonshine-ai/moonshine/blob/main/language-bindings/python/README.md).
The current catalogue labels the Small Streaming models MIT; older non-streaming non-English
models have different terms. This evaluation does not distribute model binaries or settle all
future packaging attribution requirements.

Whisper references: [v1.9.5](https://github.com/ggml-org/whisper.cpp/releases/tag/v1.9.5),
[model catalogue](https://github.com/ggml-org/whisper.cpp/blob/v1.9.5/models/README.md),
[rolling streaming example](https://github.com/ggml-org/whisper.cpp/blob/v1.9.5/examples/stream/README.md),
and [VAD integration](https://github.com/ggml-org/whisper.cpp/tree/v1.9.5#voice-activity-detection-vad).
Runtime/model/library hashes, requests, results, logs and limits are bound in
[verification.json](verification.json). Public models are staged in ignored `artifacts/asr-evaluation/`.
No missing model is silently downloaded by the game; there is still no game ASR composition.
OS-level network blocking was not independently exercised during this experiment.

[Evaluator](evaluate.py) and [interruption probe](interrupt-whisper.py) are finite developer tools.
Use Python 3.12 with `moonshine-voice==0.1.5`, `numpy==2.5.4` and `scipy==1.16.2` only if these match
the recorded installed package versions in verification; that record is authoritative. Provision
Small Streaming architecture 4 for `en` and `de` using the pinned package's downloader into the
explicit `MOONSHINE_VOICE_CACHE`. The downloader uses networking; inference loads the staged files.

```sh
MOONSHINE_VOICE_CACHE="$PWD/artifacts/asr-evaluation/moonshine-cache" \
  artifacts/asr-evaluation/python/bin/python speech-direction/asr-evaluation/evaluate.py moonshine en
MOONSHINE_VOICE_CACHE="$PWD/artifacts/asr-evaluation/moonshine-cache" \
  artifacts/asr-evaluation/python/bin/python speech-direction/asr-evaluation/evaluate.py moonshine de
# With the pinned Whisper server running on 127.0.0.1:18599 and --vad --vad-model:
artifacts/asr-evaluation/python/bin/python speech-direction/asr-evaluation/evaluate.py whisper en
artifacts/asr-evaluation/python/bin/python speech-direction/asr-evaluation/evaluate.py whisper de
artifacts/asr-evaluation/python/bin/python speech-direction/asr-evaluation/interrupt-whisper.py
```

For the supplemental run, add `--update-interval 0.1 --output-dir artifacts/asr-evaluation/results-100ms`.
The exact original 500 ms source remains in [evaluate-baseline.py](evaluate-baseline.py).
Use a new `--output-dir`/copy when preserving the recorded results; the tools' default output is
ignored artifacts, not this frozen review package. Latency trials ran under uncontrolled background
load. M1/16 GB, Windows/Linux, physical microphone permission/device loss, actual accent/noise behavior,
combined local dialogue-model performance and speaker echo/barge-in remain unverified.
