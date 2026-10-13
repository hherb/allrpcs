# Pocket TTS service component

The [owner request](owner-request.json) continues the preferred Pocket path on the M3 Max.
This component implements the Phase 10 provider-neutral synthesizer and its first local
TTS implementation. It includes a runnable C# service probe and lifecycle verification.
Game playback, microphone/ASR and character casting remain separate missing deliverables.
The concurrent controls slice keeps its existing queue and files.

## Contract and implementation

[`ISpeechSynthesizer`](../../src/Application/Speech/ISpeechSynthesizer.cs) accepts exact
dialogue text, English/German and an opaque voice profile. It yields immutable mono PCM
chunks through `IAsyncEnumerable<SpeechAudioChunk>`. Application code has no Python paths,
checkpoint names or provider voice IDs. Requests reject empty text, NUL and text exceeding
2,000 UTF-16 code units. PCM is finite, bounded and copied before exposure to consumers.

[`PocketSpeechSynthesizer`](../../src/Infrastructure/Speech/PocketSpeechSynthesizer.cs)
serializes requests through one persistent local process. Both language models remain
resident. The process starts lazily on first enumeration; the retained probe measures that
startup separately. Host composition can warm it with a short request before dialogue.
The current CPU worker uses one PyTorch intra-op and one inter-op thread.

The worker receives bounded UTF-8 NDJSON and returns 24 kHz float32 little-endian PCM in
base64. Each utterance has a unique ID and consecutive chunk numbers. Missing, stale,
oversized or malformed output raises a `SpeechSynthesisException`. Startup and reads have
a 30-second deadline; cancellation cleanup has a two-second deadline. A channel that
cannot be retired is killed and replaced on the next request. A rejected profile currently
also replaces the worker after that cleanup deadline; it is a conservative failure path.

Caller cancellation stops delivery of further PCM. Early consumer disposal also retires
the utterance. The Python worker exhausts the stopped generator before acknowledging it,
joining Pocket's decoder before the same model is reused. Cancelling a request waiting
for the service gate leaves the current utterance intact. Consumers must dispose their
enumerators (`await foreach` does this on exit) and dispose the service at shutdown.
PCM already queued by a future playback sink must be flushed by that sink on interruption.

The worker verifies that weight, tokenizer and voice-state paths resolve beneath the
configured local assets root, disables remote checkpoint fallback and sets
`HF_HUB_OFFLINE=1` / `TRANSFORMERS_OFFLINE=1`. There is no download path in this service.
Process arguments use `ArgumentList`, without a shell. Runtime diagnostics omit dialogue
and local paths; library stderr is drained without retaining it. The explicit probe alone
records its fixed test text in review evidence. OS networking was not independently blocked.

## M3 service measurement

The final [probe report](output/service-probe.json) was captured on an Apple M3 Max,
128 GB unified RAM, 16 logical processors. The timings are single trials under uncontrolled
background load, including repository checks. They measure PCM delivery rather than
speaker latency. RSS is sampled at protocol reads, so it does not establish a lifetime
startup peak or the combined game/LLM budget.

| Case | First PCM | Complete generation | Audio duration |
| --- | ---: | ---: | ---: |
| English, new process | 2.817 s | 3.108 s | 2.96 s |
| German, warm | 35.5 ms | 0.419 s | 3.68 s |
| English, after cancellation | 33.7 ms | 0.324 s | 2.80 s |
| German, after cancellation | 34.0 ms | 0.500 s | 4.48 s |
| German, after early consumer exit | 33.9 ms | 0.456 s | 4.16 s |
| English, after worker error/restart | 2.838 s | 3.135 s | 3.04 s |

Both loaded models plus worker allocations reached **1,454,964,736 bytes (1.355 GiB)**
sampled RSS. Cancellation completed in 23.6 ms (English) and 6.2 ms (German), with no
post-cancellation PCM delivered to the consumer. The healthy, cancelled and early-exit
cases reused one worker. The error case restarted it; both processes exited on disposal.
The final process-ready times were 2.746 and 2.790 seconds. An earlier process startup
observed 6.3 seconds, so game loading should allow for startup variation.

## Reproduce and deploy locally

Prepare the pinned public assets using the earlier
[model preparation script](../offline-tts-spike/prepare_models.py) and
[Pocket requirements](../offline-tts-spike/requirements-pocket.txt) in a separate Python
3.12 environment. Preparation uses networking; inference does not fetch missing files.
The current interpreter is `/private/tmp/allrpcs-pocket-venv/bin/python`; temporary
environments can disappear. Weights remain in ignored `artifacts/speech-models/`.
The preparation script writes absolute local paths into each language config; regenerate
them if the assets root moves. Deployment must provision these dependencies and assets.

From the repository root:

```sh
dotnet run --project tools/SpeechServiceProbe/Allrpcs.SpeechServiceProbe.csproj -- \
  /absolute/path/to/pocket-env/bin/python \
  "$PWD/scripts/pocket-speech-worker.py" \
  "$PWD/artifacts/speech-models/pocket-tts-3.3.0" \
  "$PWD/speech-direction/pocket-tts-service/voice-profiles.json" \
  /absolute/path/to/new-probe-output
```

The probe writes six PCM16 WAVs, hashes and measured lifecycle cases. Use a new output
directory to preserve the retained evidence. Its OS/architecture fields describe the
actual host rather than assuming an Apple model. Normal application use streams PCM
directly; it does not write WAVs or test dialogue logs.

## Voice and next integration boundary

The [test profile](voice-profiles.json) maps `default` to Alba in English and Juergen in
German. These are language fixtures with different speaker identities, not a bilingual
character cast. The exact public checkpoints and voice states retain the earlier
[pins and hashes](../offline-tts-spike/model-verification.json).
Engine/weight/voice attribution and the remaining German source-provenance question are
documented in the [spike](../offline-tts-spike/README.md#primary-sources-and-attribution).

Expressive acting remains an explicit acceptance requirement. This component preserves
exact text and adds no emotion tags, punctuation rewriting, whisper instructions or
acoustic gain rules. Stock-voice generation does not establish controlled acting or
stable bilingual character identity. Reference conditioning with suitable English and
German performances still needs a separate experiment and usable model access; the
earlier cloning-enabled checkpoint was gated. The owner's accepted intelligibility
assessment remains valid; Whisper transcripts are not used as synthesis acceptance here.

The next game integration should consume only committed dialogue replies, retain their
existing full text/subtitles, and stream PCM through a cancellable Godot playback sink.
Interruption, a newer reply or scene disposal must clear both synthesis and playback
buffers. Typed events and domain hearing/delivery rules stay with existing dialogue code.
If speech is unavailable, the caller keeps text interaction available. This service does
not yet implement that scene composition or demonstrate fallback UI behavior.

Actual M1 16 GB, Windows/Linux, packaged Python/models, sound-device lifecycle and game
plus local LLM coexistence remain unmeasured. The CPU implementation and memory sample
support continuing on M3; they do not certify those targets. ASR is still undecided.

## Verification

Focused application contracts, worker lifecycle tests, the actual service probe and WAV
verification are retained in [checks](checks/) and [output](output/). Full root quality
and finite closure are recorded in [closure.json](closure.json).
The [verification summary](checks/verification-summary.json) accounts for all 177 required
stage names across the retained reports, plus three historical herb bootstrap stages.

The 21 focused speech tests and all 5,511 solution unit tests passed. The root quality
command completed 117 stages before a native SIGSEGV in Godot's WAV importer while
importing the unchanged historical heating commit. The
[crash diagnosis](checks/native-import-diagnosis.json) identifies that editor worker
thread; the historical snapshot contains no speech code. The bounded
[finish runner](checks/finish_quality.py) verifies the original inputs, completed log
hashes and unchanged current source, then calls the original heating, steeping,
cooling and herb check functions. The original root report stays marked failed;
the separate recovery reports record the remaining checks. No frozen validator,
game source or root quality runner is changed for this recovery.
The herb validator's original file-scope guard rejects new speech evidence paths;
[the final helper](checks/finish_herb_quality.py) runs its complete unchanged checks
in accepted commit `db298c8f`, including restore, build, cold import and mounted proof.
