# Allrpcs Handover

Read [AGENTS.md](AGENTS.md), [CONTINUATION.json](CONTINUATION.json) and the named remaining
roadmap item, then run `python3 scripts/check-continuation.py`. Direct owner instructions and
terminal acceptance take precedence. Keep every persistent closure.

## Current checkpoint

[Optional Pocket reply playback](speech-direction/dialogue-integration/README.md) is integrated in
Main: Settings → Dialogue → Speak replies, English/German and Stop speech. It speaks exact committed
reply text on the Speech bus, with bounded PCM, interruption and text fallback. Local developer
configuration is documented in that package; no packaged interpreter/model installer is provided.
The owner selected Moonshine Small Streaming. [Player speech integration](speech-direction/player-asr-integration/README.md)
adds opt-in Hold to talk, Listen (headphones), Stop mic and editable English/German drafts. Submit and
Whisper still use the existing dialogue owner. Native/synthetic Main and full quality pass; linked
verification binds the final evidence.
The [Space successor](speech-direction/space-hold-to-talk/README.md) keeps native taps and starts voice
after a 300ms hold regardless of game control or popup focus, without a voice mode. Release finalizes
the editable draft. Its current checks and full quality pass. The owner reported good live recognition;
the owner confirms English and German recognition through held Space. The [partial human trial record](speech-direction/player-microphone-qualification/README.md)
also records correction before Submit and Stop mic during a hold as working. The owner confirms
built-in microphone/speakers and both languages in a slightly noisy room, and now confirms live draft
updates in English and German. Release stops listening with an editable, unsent draft. Brief taps
insert normal spaces without starting the microphone. The Whisper/Submit attempt clears the draft
but produces no observed response; end-to-end delivery remains inconclusive. Read-only follow-up on
2026-10-16 found the saved Ollama model `gemma3` missing from local inventory. The owner selected
`gemma4:e4b`; saved settings were updated, local model availability passed and the game restarted.
The screenshot confirms local acceptance without a reply. The owner approved the [Ollama
participation repair](speech-direction/ollama-participation-repair/README.md): its missing binding
and cached-usage metadata support are implemented. A fictional-input `gemma4:e4b` Main diagnostic
commits a reply. The recovered full gate passed 217 recorded stages and 5,629 unit tests; the repair
is closed. Human Whisper/echo outcomes remain unconfirmed.
Selector/permission-host intake and remaining lifecycle/device/echo qualification remain incomplete.

Closed [dialogue accessibility](input-direction/dialogue-accessibility-runtime-review/README.md),
[controls](input-direction/remapping-runtime-review/README.md), Pocket service and earlier art/audio/domain
owners remain unchanged. Historical full-quality results belong to their linked packages. VoiceOver
and physical-controller qualification remain unperformed. Only built-in keyboard/trackpad are available;
retain the [hardware deferral](input-direction/physical-controls-deferral.json) while the owner procures a controller.

[Privacy](release-direction/privacy.md) and [provider disclosures](release-direction/model-providers.md)
are complete; [reply playback](release-direction/voice-privacy-update.md) and
[microphone input](release-direction/player-asr-privacy-update.md) supplement them.
OpenAI/local Ollama composition exists; Anthropic and an offline conversational fallback remain
uninstalled. Development release documentation is deferred under the owner's speech priority.
Future walking work uses the accepted [reference gait model](art-direction/reference-gait-model.md).

Standing publication authority: “ALWAYS commit and push when you finish a task.” Retain
[the recorded grant](audio-direction/footstep-runtime-review/publication/authority.json).

<!-- active-slice:start -->
## Next bounded slice

**phase10-player-microphone-qualification — Qualify a named human microphone and echo session** (in_progress).

Roadmap item: - [ ] Test device loss, permission denial, network failure, feedback loops, accents,
and noisy   input.

Missing deliverable: Remaining human selector/permission-host intake, final text and Whisper/reply
observation with the repaired runtime, cancellation/load/device/noise/accent/headphone/speaker cases
and a reviewable finite conclusion. The Ollama prerequisite is closed.

The owner-approved Ollama participation prerequisite is complete and closed in speech-
direction/ollama-participation-repair/closure.json. Selected gemma4:e4b commits an ordinary Main
reply on fictional input; the recovered full gate passed 217 recorded stages and 5629 unit tests. Do
not reopen this repair or convert synthetic/local-model checks into physical passes. Resume only
remaining operator checks in the human trial guide. Preserve the three complete qualitative passes,
five partially completed cases and seven unperformed cases; do not repeat confirmed bilingual
recognition/live updates, corrected Submit, Stop mic or Space tap/release checks. The previous
accepted/no-response screenshot is historical; a new nearby-listener Whisper/reply outcome remains
required. Keep this human slice open until remaining cases and its conclusion are reviewable.
Existing capture opt-in remains authorized; OS privacy/device changes or personal-audio persistence
need explicit enablement.

Done when: Named finite session records permission/device intake, natural English/German
partial/final correction and explicit Submit/Whisper, stop/release/close/focus/load, input
loss/recovery and quiet/noisy/accent/headphone/speaker cases. Each result has honest pass/fail/not-
performed status and source/runtime provenance. Retain permitted evidence, limits and conclusion,
then close and publish; hands-free speaker qualification requires actual echo-safe results.

Expected outputs:

- `speech-direction/player-microphone-qualification/authority.json`
- `speech-direction/player-microphone-qualification/session.json`
- `speech-direction/player-microphone-qualification/results.json`
- `speech-direction/player-microphone-qualification/README.md`

Boundary: Existing named owner-operated physical trial only; preserve the closed Ollama repair. No
new runtime fixes, automatic transcript execution, personal-audio persistence, OS privacy/device
changes beyond the grant, model comparison/requalification, broad platform/reader/controller
certification or packaged release.
<!-- active-slice:end -->

## Verification and local exceptions

After ledger maintenance run `python3 scripts/check-continuation.py --sync`, its checker and
`--self-test`. Speech evidence and reproduction: [reply playback](speech-direction/dialogue-integration/verification.json) and
[player recognition](speech-direction/player-asr-integration/verification.json) and
[Space gesture](speech-direction/space-hold-to-talk/verification.json).
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
