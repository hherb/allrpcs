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
insert normal spaces without starting the microphone. The owner reported two successful spoken dialogue turns followed by failure. The closed
[Ollama participation repair](speech-direction/ollama-participation-repair/README.md) supplies the
missing ordinary binding and cached metadata support. The separately approved
[response repair](speech-direction/dialogue-response-repair/README.md) constrains Ollama
speech/mode pairing and completes silent turns without blocking later input. Eight distinct
fictional controlled and selected `gemma4:e4b` local-model generations and the recovered full
quality gate passed. The cancellation fixture now holds unrecorded speech, preserving its
assertions. Both runtime repairs are closed; human repeated Whisper/reply and echo outcomes
remain partial or unperformed. Preserve the successful recognition/typing observations and
reported two-turn success followed by failure. The latest post-restart report still finds occasional
missing Healer responses. The owner now clarifies that ordinary answers work, while examination
requests produce neither visible action nor reply. Source tracing finds a speech-dependent action
occurrence gate and no navigation handoff in the temporary logical examination flow. The exact
human model output is unknown. The owner approved the [examination action successor](speech-direction/herb-examination-action-repair/README.md).
Its implementation now executes accepted spoken/silent examinations through movement and arrival,
with visible result/failure and no automatic collection. Controlled native Main, a fictional
`gemma4:e4b` examination and the recovered full gate (216 stages; 5,644 unit tests) pass.
This runtime repair is closed; resume the existing physical trial and preserve its case totals.
The ordinary trial game was restarted from published `fd0910b2`; startup is verified.
The next owner observation is an examination request, followed by remaining physical cases.
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

Missing deliverable: Remaining permission/selector intake, language-specific final text and post-
repair human repeated Whisper/reply observation, lifecycle/device/noise/accent/headphone/speaker
cases and a finite conclusion.

The reply runtime prerequisites are closed: speech-direction/ollama-participation-
repair/closure.json and speech-direction/dialogue-response-repair/closure.json. The latter fixes
malformed speech/mode pairs and silent-turn stalls, with eight distinct fictional controlled/local-
model generations and recovered full quality. Preserve all closed receipts and the three complete
physical passes, five partial cases and seven unperformed cases. Preserve the owner report of two
successful spoken turns followed by failure; its exact live failure was unlogged. Resume only
remaining owner-operated checks, starting with repeated Whisper/reply dialogue beside the listener.
Do not repeat confirmed bilingual recognition/live updates, correction, Stop mic or Space
tap/release. Do not infer physical, echo or audibility passes from synthetic/local-model checks.
Existing capture opt-in remains authorized; personal-audio persistence and new OS changes require
explicit enablement. The owner clarified ordinary answers work but examination requests lacked
action or reply. The separately approved herb examination action repair is now complete and closed
at speech-direction/herb-examination-action-repair/closure.json. Controlled native Main, actual
selected gemma4:e4b observation and recovered full quality verify accepted spoken/silent examination
movement, arrival, repeat/cancel/load ownership and visible outcomes without collection. Preserve
every prior report and all 15 physical statuses. Resume the owner-operated trial on the published
repaired runtime; no synthetic/local-model result is a physical pass. Do not reopen the closed
repairs or infer a new runtime scope.

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

Boundary: Existing named owner-operated physical trial only; preserve both closed runtime repairs
and all frozen evidence. No new runtime fixes, automatic transcript execution, personal-audio
persistence, OS privacy/device changes beyond the grant, model comparison/requalification, broad
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
