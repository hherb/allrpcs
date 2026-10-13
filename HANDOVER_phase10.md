# Phase 10 handover — offline speech

Updated 2026-10-13. Repository: `/Users/hherb/src/allrpcs`.

## Resume here

The first **Pocket TTS service component is implemented, verified, committed and pushed**.
Game-scene speech playback, expressive character acting, microphone capture and ASR remain
unimplemented. Continue from this service; do not repeat the completed model auditions or
reopen the service component as a new comparison/audit.

Before selecting work, read [CONTINUATION.json](CONTINUATION.json), [HANDOVER.md](HANDOVER.md),
[AGENTS.md](AGENTS.md), this document and [Phase 10 in ROADMAP.md](ROADMAP.md#phase-10--voice-ready-interaction-and-optional-asrtts).
Run `python3 scripts/check-continuation.py` and inspect current Git status and owner instructions.
The ledger retains closure `phase10-pocket-tts-service-component`.

At this checkpoint the global active slice is **Phase 11 dialogue accessibility**, awaiting
its exact runtime/UI grant. The 29-action controls implementation and accessibility planning
are closed. This Phase 10 handover records a checkpoint and suggested successors; it does
not replace that queue or grant the separate accessibility changes. A new owner instruction
to continue speech takes precedence and must be recorded against a finite remaining Phase 10
deliverable. Preserve other work and every closure; avoid concurrent edits to Main/dialogue UI.

## Owner decisions and constraints

- English **and German**, fully offline inference, targeting M1 with **16 GB RAM** and
  comparable desktop platforms. Expressive acting is important.
- The current machine is **M3 Max, 128 GB unified RAM, 16 logical processors**. The owner
  could not find the M1 and explicitly approved continuing on M3. Physical M1 suitability,
  combined game/LLM/speech memory and Windows/Linux execution are not established.
- **Qwen2.5-Omni-7B was explicitly discarded.** Preserve its original audition; do not restart it.
- Pocket is selected for the first service component. The owner understood both auditioned
  models in both languages, preferred Pocket and heard an English accent in Qwen's German voice.
  The later comparison was Pocket versus Qwen3-TTS CustomVoice; Qwen3 is not implemented here.
- Whisper base transcripts are diagnostic recognizer outputs. The owner attributed the
  mismatches to Whisper, not unintelligible synthesis. Do not turn them into established
  pronunciation failures or reopen the accepted intelligibility assessment.
- Host execution was approved: **“I approve host execution.”** Standing publication authority
  is **“ALWAYS commit and push when you finish a task.”** Retain both authorities in context;
  do not ask for the same approval again. Publication authority does not expand runtime scope.

Current decisions: [model-decisions.json](speech-direction/model-decisions.json).
Listening assessment: [owner-listening-assessment.json](speech-direction/offline-tts-spike/owner-listening-assessment.json).
Continuation request: [owner-request.json](speech-direction/pocket-tts-service/owner-request.json).

## Implemented service

| Area | Entry point | Behavior |
| --- | --- | --- |
| Application contract | [ISpeechSynthesizer.cs](src/Application/Speech/ISpeechSynthesizer.cs) | Exact text, English/German, opaque profile → asynchronous immutable mono PCM chunks |
| Request/audio types | [Speech directory](src/Application/Speech/) | Maximum 2,000 UTF-16 text units; nonempty/no NUL; bounded profile IDs and finite normalized PCM copies |
| Backend | [PocketSpeechSynthesizer.cs](src/Infrastructure/Speech/PocketSpeechSynthesizer.cs) | One serialized active utterance, persistent worker, cancellation, retirement and recovery |
| Process composition | [PocketSpeechWorkerProcessFactory.cs](src/Infrastructure/Speech/PocketSpeechWorkerProcessFactory.cs) | Fully qualified infrastructure paths, structured arguments, UTF-8 pipes, no shell, unlogged stderr |
| Python worker | [pocket-speech-worker.py](scripts/pocket-speech-worker.py) | Two resident CPU models; local assets; bounded NDJSON; 24 kHz float32 LE PCM encoded as base64 |
| Executable probe | [SpeechServiceProbe](tools/SpeechServiceProbe/) | Real C# → worker synthesis, cancel/reuse, early consumer exit, failure/restart and shutdown; six WAVs |

Both language models stay resident; PyTorch intra-op and inter-op threads are each set to one.
The worker starts lazily on first enumeration. A future enabled game composition can warm it
during loading by consuming and discarding a short synthesis request.

Every request has an ID and consecutive PCM sequence numbers. Corrupt/stale/oversized output
fails before delivery. Startup/read deadlines are 30 seconds; cancellation retirement is bounded
to two seconds. An unusable channel is killed and restarted on the next request. An unsupported
profile currently also causes that conservative restart, adding cleanup and startup latency.

Caller cancellation and early enumerator disposal retire output. The Python worker must
**exhaust the stopped generator**, joining Pocket's decoder before model reuse; do not replace
this with an early `break`. Cancelled requests waiting at the gate leave the active request intact.
Use `await foreach`/proper enumerator disposal and dispose the service at shutdown.
Cancellation of synthesis does **not** flush audio already buffered by a future playback sink.

No game node currently composes this service. `selected_game_backend` remains null in the
decision record because scene integration has not happened. No microphone or ASR backend exists.
Production service diagnostics exclude dialogue and local paths. The explicit probe records
only its fixed review text. Inference validates local paths and sets offline library flags;
network access was not independently blocked at OS level during the tests.

## Performance and verification

Final [service probe](speech-direction/pocket-tts-service/output/service-probe.json):

| Measurement | M3 result |
| --- | ---: |
| Warm first PCM | 33.7–35.5 ms |
| Warm complete generation | 0.324–0.500 s for 2.8–4.48 s speech |
| Sampled worker RSS, both models resident | 1.355 GiB |
| Process ready, final trials | 2.746 / 2.790 s |
| Cancel after first chunk | 23.6 ms English / 6.2 ms German |

These are single trials under background load. PCM availability is not speaker latency;
sampled RSS is not a continuous lifetime peak or the combined game/LLM budget. Earlier startup
was slower, so allow startup variation rather than assuming every boot takes 2.7 seconds.

The speech checkpoint passed **21 focused tests** (6 Application, 15 Infrastructure), **5,511
solution unit tests**, actual English/German process streaming, cancellation/reuse, early exit,
error recovery, worker exit, missing-runtime/assets handling and all six WAV hash/format checks.

The original speech root quality run stopped after 117 passing stages at a native Godot
`ResourceImporterWAV` worker-thread SIGSEGV in an unchanged historical heating snapshot.
Remaining checks were completed with source/log guards and exact original functions; the herb
contract ran in its accepted `db298c8f` snapshot because its frozen file-scope guard rejected new
speech evidence. **All 177 required stage names were accounted for**, plus three herb bootstrap
stages. The original failed reports remain failed; do not describe that original command as an
uninterrupted pass or weaken frozen validators. The finish scripts are hash-bound records of
that specific recovery, not generic commands to replay at a new HEAD.

Authoritative links:

- [Component README](speech-direction/pocket-tts-service/README.md),
  [finite closure and source hashes](speech-direction/pocket-tts-service/closure.json).
- [Qualified verification summary](speech-direction/pocket-tts-service/checks/verification-summary.json),
  [native crash diagnosis](speech-direction/pocket-tts-service/checks/native-import-diagnosis.json).
- [Runtime/assets metadata](speech-direction/pocket-tts-service/checks/runtime.json),
  [WAV verification](speech-direction/pocket-tts-service/checks/wav-verification.json).

Compressed logs/state JSON have raw/decompressed hash bindings. Complete raw final artifacts
are retained locally under ignored `artifacts/pocket-tts-service-quality-20261013/`.
The managed speech worktree was archived after source integration and artifact preservation;
use the main checkout paths, not its deleted worktree path.

Since the speech closure, the controls chat completed and published its own work. Its later
[quality report](input-direction/remapping-runtime-review/quality/summary.json) reports **188
passing stages and 5,576 unit tests**, with product base `f7cb365c` (which includes speech).
The root quality runner has since advanced; use the current runner for new product changes,
and distinguish that later combined checkpoint from the original speech recovery evidence.

## Local setup and reproducible commands

Verified present when this handover was written:

- Interpreter: `/private/tmp/allrpcs-pocket-venv/bin/python` (Python 3.12; Pocket TTS 3.3.0).
- Assets: `artifacts/speech-models/pocket-tts-3.3.0/{english,german}/`, each containing
  `config.yaml`, `model.safetensors`, `tokenizer.json`, `voice.safetensors`.
- Models are ignored, **not committed**. Each weight file is about 219 MB.
- Test mapping: [voice-profiles.json](speech-direction/pocket-tts-service/voice-profiles.json).

Temporary environments can disappear. Recreate one with
[requirements-pocket.txt](speech-direction/offline-tts-spike/requirements-pocket.txt).
The [preparation module](speech-direction/offline-tts-spike/prepare_models.py) exposes `pocket()`
for only Pocket assets. Its full script entry point also downloads Qwen and rewrites the old
verification file; do not invoke that whole entry point merely to restore Pocket. Call `pocket()`
through an imported module and save any new preparation evidence outside the frozen package.
Preparation requires networking; provision all files before offline inference. Configs contain
absolute weight/tokenizer paths and must be regenerated if the assets root moves. Use the
[retained pins/hashes](speech-direction/offline-tts-spike/model-verification.json) for verification.

From `/Users/hherb/src/allrpcs`, run the actual service into a **new ignored output directory**:

```sh
dotnet run --project tools/SpeechServiceProbe/Allrpcs.SpeechServiceProbe.csproj -- \
  /private/tmp/allrpcs-pocket-venv/bin/python \
  "$PWD/scripts/pocket-speech-worker.py" \
  "$PWD/artifacts/speech-models/pocket-tts-3.3.0" \
  "$PWD/speech-direction/pocket-tts-service/voice-profiles.json" \
  "$PWD/artifacts/speech-service-next-probe"
```

Focused checks when speech source changes:

```sh
dotnet test tests/Application.Tests/Allrpcs.Application.Tests.csproj \
  --filter FullyQualifiedName~Allrpcs.Application.Tests.Speech
dotnet test tests/Infrastructure.Tests/Allrpcs.Infrastructure.Tests.csproj \
  --filter FullyQualifiedName~Allrpcs.Infrastructure.Tests.Speech
dotnet build tools/SpeechServiceProbe/Allrpcs.SpeechServiceProbe.csproj
python3 scripts/check-continuation.py --self-test
```

Use the authorized host retry for genuine sandbox native/.NET failures. In a fresh isolated
checkout, preserve the recorded editor exception and complete `godot --headless --editor
--path game --import` before smoke tests; timed editor scans can stop before cold imports finish.
For new product/CI changes run current root `./quality.sh`. Reuse passing product evidence for
prose-only handover maintenance; do not repeat the full suite for this document.

## Suggested next Phase 10 slice

After reconciling the next owner's instruction with the ledger, the missing game deliverable
is **optional streamed character speech playback with interruption and text fallback**.
Prepare a concrete bounded composition rather than reopening this service implementation.

1. Inspect current [Main.cs](game/Main.cs), [DialogueInputController.cs](game/DialogueInputController.cs),
   [TemporaryDialogueInputPanel.cs](game/TemporaryDialogueInputPanel.cs),
   [CharacterTurnActivityController.cs](game/CharacterTurnActivityController.cs),
   [CharacterTurnTranscriptController.cs](game/CharacterTurnTranscriptController.cs) and model
   generation/response flow. Find the exact accepted dialogue commit boundary before adding
   presentation. Do not speak speculative provider stream text or trigger duplicate turns.
2. Add a Godot PCM sink and presentation-owned actor/profile resolver. Keep paths, checkpoint
   names and provider voice IDs outside domain rules. Keep text/subtitles and typed input working
   with speech disabled, unavailable or failed. Respect existing control/modal contexts.
3. On interruption, a newer accepted reply or scene disposal, cancel synthesis **and clear the
   playback buffer**. Verify stale callbacks cannot feed a newer utterance and shutdown leaves
   no worker. Test device failure and actual audible cancellation separately from PCM delivery.
4. Verify dialogue/delivery/hearing/transcript/save behavior is unchanged. Acoustic acting or
   volume must not determine domain whisper/hearing semantics; retain exact `DialogueDelivery`
   identities and existing authoritative event flow.
5. Measure startup, repeated latency and RSS with **Godot and the actual local dialogue model**
   on M3. Keep M1/16 GB and other-platform claims pending real measurements. Close only the new
   finite slice, maintain the ledger/handover, commit and push the intended files.

Expressive acting needs its own finite qualification before final casting. The current `default`
profile uses **Alba for English and Juergen for German**: different speakers, not one bilingual
character. The owner accepted intelligibility, not controlled acting or character identity.
The public no-cloning checkpoint and stock voice states are available; the cloning-enabled
checkpoint previously returned gated-access denial. Evaluate usable reference conditioning and
matched English/German performances when access is available. Compare controlled deliveries
with identical text and actor identity; punctuation changes alone do not prove acting control.

Engine MIT / public weights CC BY 4.0 and the voice-source attribution notes are recorded in the
[spike](speech-direction/offline-tts-spike/README.md#primary-sources-and-attribution).
Resolve the remaining German source-provenance question before choosing/distributing a game cast.
No final actor voice assignment or packaged-model redistribution is established by this checkpoint.

ASR remains undecided. Later work needs `ISpeechRecognizer`, permission/capture, push-to-talk,
partial/final events and correction/confirmation before consequential actions. Spoken input must
enter the existing typed dialogue/intention flow. Benchmark German, accents, names, negation and
noise; do not silently adopt the diagnostic Whisper base configuration as the production recognizer.

## Publication and preserved state

Speech service: `33c5d2b4`; ledger closure: `f7cb365c` — both pushed to `origin/main`.
Later controls runtime: `1b0cab40`; dialogue accessibility proposal: `2e1d52a6`.
These are checkpoint references, not an instruction to reset to them; inspect current HEAD.

At handover creation the only tracked local modification was the unrelated `game/project.godot`
editor rewrite, SHA-256 `36bfb7c7a54399cc8d27c02b73c7990766d7db44e7452655cb6a496866653fca`.
Preserve it and any later work. Never update frozen acceptance hashes/validators to accommodate
successors, overwrite retained audition output, prune ledger closures, or infer an executable
release from source tests/mounted-resource evidence.
