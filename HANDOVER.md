# Allrpcs Handover

Read [AGENTS.md](AGENTS.md), [CONTINUATION.json](CONTINUATION.json) and the named remaining
roadmap item, then run `python3 scripts/check-continuation.py`. Direct owner instructions and
terminal acceptance take precedence. Keep every persistent closure.

## Current checkpoint

[Optional Pocket reply playback](speech-direction/dialogue-integration/README.md) is integrated in
Main: Settings → Dialogue → Speak replies, English/German and Stop speech. It speaks exact committed
reply text on the Speech bus, with bounded PCM, interruption and text fallback. Local developer
configuration is documented in that package; no packaged interpreter/model installer is provided.
The [ASR comparison](speech-direction/asr-evaluation/README.md) recommends Moonshine Small Streaming;
Whisper small + Silero VAD is the measured alternative. No microphone or game recognizer is connected.
Real microphone/echo trials are required before claiming hands-free voice interruption.

Closed [dialogue accessibility](input-direction/dialogue-accessibility-runtime-review/README.md),
[controls](input-direction/remapping-runtime-review/README.md), Pocket service and earlier art/audio/domain
owners remain unchanged. Historical full-quality results belong to their linked packages. VoiceOver
and physical-controller qualification remain unperformed. Only built-in keyboard/trackpad are available;
retain the [hardware deferral](input-direction/physical-controls-deferral.json) while the owner procures a controller.

[Privacy](release-direction/privacy.md) and [provider disclosures](release-direction/model-providers.md)
are complete; [changed voice behavior](release-direction/voice-privacy-update.md) supplements them.
OpenAI/local Ollama composition exists; Anthropic and an offline conversational fallback remain
uninstalled. Development release documentation is deferred under the owner's speech priority.
Future walking work uses the accepted [reference gait model](art-direction/reference-gait-model.md).

Standing publication authority: “ALWAYS commit and push when you finish a task.” Retain
[the recorded grant](audio-direction/footstep-runtime-review/publication/authority.json).

<!-- active-slice:start -->
## Next bounded slice

**phase10-streaming-player-asr-integration — Integrate chosen streaming player recognizer and editable microphone drafts** (ready).

Roadmap item: - [ ] Add one ASR implementation and one TTS implementation.

Missing deliverable: Actual provider-neutral speech recognition/capture boundary, one selected
English/German local ASR adapter and opt-in editable microphone drafts in Main, with hold-to-talk
fallback and voice-start cancellation of Pocket reply audio.

Use the completed ASR comparison. Moonshine Small Streaming is recommended; an owner backend-choice
question is pending. The original integration grant remains sufficient: request only unresolved
backend preference, not generic runtime approval. Keep recognition separate from explicit Submit,
preserve manual corrections and reject retired-generation results. Listening defaults off. VAD alone
cannot reject game loudspeakers; qualify voice activation with headphones or a real echo-control
session before hands-free claims.

Done when: Selected ASR and capture compose in Main with bounded partial/final drafts, explicit
listening/hold-to-talk, safe text fallback, reply interruption, lifecycle/device failure handling
and ordinary dialogue parity. Retain focused/native/current quality and honest physical-
microphone/echo limits, then close and publish the finite implementation; human acoustic
qualification remains a distinct evidence requirement.

Expected outputs:

- `speech-direction/player-asr-integration/authority.json`
- `speech-direction/player-asr-integration/README.md`
- `speech-direction/player-asr-integration/verification.json`
- `speech-direction/player-asr-integration/closure.json`

Boundary: Original dialogue integration authority only. No automatic transcript execution, new
voice-command domain vocabulary, actor/model audition restart, packaged model distribution, physical
controller qualification or unmeasured microphone/echo/platform claims.
<!-- active-slice:end -->

## Verification and local exceptions

After ledger maintenance run `python3 scripts/check-continuation.py --sync`, its checker and
`--self-test`. Speech evidence and reproduction: [verification](speech-direction/dialogue-integration/verification.json).
Runner checks: `python3 quality.py --self-test`. Root `./quality.sh` is required for product/CI
changes; reuse completed product evidence for subsequent prose-only planning.

Frozen validators remain unchanged in private historical source trees/indexes, including accepted
dialogue `e7b17575`, controls `1b0cab40` and herb `db298c8f`. Cold checks restore before build/import; cooling seeds its
four accepted UID sidecars. Run Godot on the host, serialize native focus fixtures, and retry only
diagnosed native mutex shutdowns identically. No OS reader setting or broad reader claim is authorized.

Preserve/exclude unrelated `game/project.godot` editor SHA
`36bfb7c7a54399cc8d27c02b73c7990766d7db44e7452655cb6a496866653fca`, `.DS_Store` and concurrent
`speech-direction/` work. UID cleanup removes only newly generated sidecars of unchanged tracked
GD sources. The existing C# executable-export limitation remains; no executable release is verified.
