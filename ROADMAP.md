# Allrpcs Roadmap

## Purpose

Keep the remaining work, milestone acceptance criteria, and settled design constraints here.
Use [CONTINUATION.json](CONTINUATION.json) for the persistent closed scopes and one active slice;
use [HANDOVER.md](HANDOVER.md) for its handoff and current verification;
use [README.md](README.md) for development commands. Detailed completed-work evidence stays
in the linked reviews and asset packages, with earlier checklists retained in Git history.

When work is completed:

- Change its checkbox from `[ ]` to `[x]` only after the listed acceptance condition has been
  verified; prune completed entries once their evidence is recorded.
- Keep closure evidence in `CONTINUATION.json`; choose one missing deliverable from a
  remaining item below, then sync `HANDOVER.md` with `python3 scripts/check-continuation.py --sync`.
- Closed scopes survive handover pruning. Do not create another audit/comparison of the
  same accepted area as a successor; reopen it only at the owner's explicit request.
- Keep partially completed items unticked and describe their current state in `HANDOVER.md`.
- Do not mark platform, provider, packaging, or manual behaviour as verified without current
  evidence.

## Product vision

Allrpcs is a slow-paced, top-down 2D game of exploration, conversation, investigation, and
healing. A contagious disease causes infected people to fall into a deep sleep and waste away.
The player and a party of autonomous characters must find healing plants, recover lost
knowledge, test remedies, and contend with an enemy agent attempting to prolong the outbreak.

There are no conventional NPCs. Every embodied character is a role with a controller that can
be human, LLM, or dormant. The first release is single-player: one human controls the Seeker,
while LLMs control companions and encountered roles. Infected characters use the dormant
controller until successfully healed.

## Settled design decisions

- First release: single-player with LLM companions.
- Initial player role: a customizable Seeker with a selectable background.
- Initial LLM roles: travelling healer, two hermits, sage, enemy agent, and infected characters
  that become responsive only after healing.
- Presentation: continuous top-down 2D movement with smooth camera scrolling in any direction.
- Time model: real-time movement and animation, with disease progression tied to meaningful
  journey and investigation events rather than wall-clock time.
- Dialogue: unrestricted natural human language. Structured intentions are an invisible engine
  protocol, never a player-facing command vocabulary.
- World authority: the game engine owns truth, perception, inventory, movement, discovery,
  disease progression, and action outcomes. LLMs decide only what their characters attempt.
- Knowledge model: world truths, evidence, claims, beliefs, and private knowledge remain
  distinct.
- Quest model: a redundant evidence network with multiple valid routes, not a linear chain of
  quest flags.
- Persistence: durable world and character state is local and provider-neutral.
- LLM support: OpenAI, Anthropic, and Ollama through interchangeable adapters.
- Implementation: Godot 4.6 .NET with C#; Godot handles rendering and authoring while pure C#
  libraries hold domain and application rules.
- Initial renderer: Godot Compatibility renderer, unless prototype evidence requires otherwise.
- Voice: future ASR and TTS use the same dialogue pipeline through replaceable interfaces.
- Art production: validate greybox scale and camera first; select an art direction before
  producing final assets.

## First vertical-slice definition

The first complete milestone lets a player:

1. Create a Seeker and choose a background.
2. Meet and converse naturally with the healer.
3. Explore the afflicted village, stream, laundry, woodland, shrine, and abandoned wagon.
4. Discover hidden and partially understood evidence.
5. Ask the healer to perform a conditional task in natural language.
6. Form and test hypotheses about the disease.
7. Identify ingredients and prepare a stabilizing remedy.
8. Stabilize the first sleeping patient.
9. Save, close, reopen, and continue with character memory intact.
10. Run on macOS, Linux, and Windows.

## Completed foundation

Phases 0 and 2–8 are complete: project/tooling foundations; authoritative world, discovery and
saves; natural-language dialogue; provider adapters; character memory and relationships;
disease and evidence-driven quests; autonomous roles and fair sabotage; and the authored
opening scenario with its human acceptance checkpoint. Phase 1 greybox movement, camera,
collision, navigation, healer tasks, and interaction focus are established.

Do not reopen these milestones as new work. Detailed autonomous-role traceability remains in
[PHASE_7_ACCEPTANCE_AUDIT.md](PHASE_7_ACCEPTANCE_AUDIT.md). Provider, platform, credential,
packaging, and accessibility claims remain limited to their recorded evidence.

## Phase 1 — Remaining exploration work

- [ ] Implement point-and-click movement if retained after prototype testing.
- [ ] Pause disease time and world decisions during dialogue, reading, and inventory use.
- [ ] Add a debug overlay for position, region, navigation target, and current world-time event.

## Code health and playable vertical slice (owner request 2026-10-17)

- [x] Phase 1: smoke launches prove completion and every check is registered.
- [ ] Phase 2: player-facing code-review-1 defects are fixed with pinning tests.
- [x] Phase 3: one structured-decision pipeline and a thin Main composition root.
- [ ] Phase 4: a new player completes slice steps 1–9 and reaches an ending on local Ollama.
  - [x] 4.1 New-game flow: customization opens at the start; Forester opens the closed forest path.
  - [x] 4.2 Memory, transcript, customization and positions survive a save; continue on launch.
  - [x] 4.3 Ingredients: unified inventory, Give/Take/Use, conditional collection by default.
  - [x] 4.4 Hypotheses: journal proposal interpreted through the structured-decision pipeline.
  - [x] 4.5 Stakes and ending: disease progression can fail; explicit slice-complete screen.
  - [x] 4.6 No evidence resolves at boot; residue is noticed on arrival, the remedy dose confirms.
  - [ ] 4.7 Owner playtest from a fresh profile to the ending on local Ollama.
- [ ] Phase 5: game/ is organised by feature and Temporary* scaffolding is resolved.

## Phase 9 — Production art and audio

### Selected direction and acceptance boundaries

Phase 9.1 is complete. Use Direction A (painterly storybook gouache) and the
[art bible](art-direction/representative-scene/ART_BIBLE.md): 1280x720 base viewport,
aspect expansion, smooth filtering and editable masters at exact 2x export dimensions.
Preserve the approved 3200x1800 opening-region layout and independent physical authority.
Direction D remains only the art bible's gated feasibility fallback.

Accepted ground/channel/Clearing placements, character artwork/import proof and the whole
Seeker cloak gait correction stay closed. The [persistent ledger](CONTINUATION.json)
retains their exact acceptance and parked ground/Clearing/forest workstreams. Use the
[reference gait model](art-direction/reference-gait-model.md) for future walking work.
Keep accepted source bytes, pivots, placements and validators unchanged; reproduce historical
proofs on their pinned source trees. Visual approval does not establish gameplay, pacing,
usability, accessibility or whole-region completeness.

### 9.2 Remaining production assets

Woodland details, structures, characters, objects, UI and effects artwork categories are
owner-approved. Their editable masters, exports and provenance remain in linked ledger
packages and the [production manifest](art-direction/production-proof/corrected/asset-manifest.json).
Artwork/source acceptance and isolated imports do not grant ordinary runtime installation.
Ordinary character integration, effects animation/anchors/semantics and functional use of
external UI artwork remain separate implementation boundaries.

Residual terrain studies and repeatable material/transition artwork are accepted; global
terrain suitability and integration remain unfinished. A new terrain scope must name the
field, intended interface/composition and missing deliverable before new feasibility or
placement work. Generic edge differences do not authorize new connectors or geography.
Do not restart parked coverage/apron/dressing studies around accepted placements.
Existing local readability simulations do not establish broader accessibility acceptance.

Artwork workflow: produce the whole selected category first, with separate editable 2x
masters and review exports. Present labelled families, scale references and grouped
compositions together, then discuss/revise with the owner before consolidated validation.
Use lightweight authoring checks while candidates are being produced; do not run full
code CI, Godot smokes or historical suites for each candidate. Once revisions are agreed,
run scoped asset validation and required product checks; repeat only for changes/failures.
Reuse passing product evidence when product/CI inputs are unchanged. Carry existing
acceptance and the routine external-proof approval waiver forward. New runtime/layout/physical
promotions and their runtime visual checkpoints require the applicable authority; finish already authorized work in
the same session without repeating unchanged approval gates.

- [ ] Produce terrain tiles, transitions, paths, banks, water, and ground variation.
- [ ] Validate alpha, seams, scale, pivots, collision silhouettes, and texture import settings.
- [ ] Test important visuals under colour-vision and contrast simulations.
- [ ] Keep editable source assets separate from exported runtime assets.

### 9.3 Remaining sound work

The [audio direction](audio-direction/README.md), buses/independent controls and complete
sixteen-excerpt audition are accepted. Default range and the accepted waveforms stay
unchanged, including M02 shakuhachi. Completed runtime slices cover B01–B04 regional beds,
A04 grinding/A05 treatment, Seeker terrain footsteps, the named spatial B02/recorded-reply
cue subset, restrained optional M01/M02 music and the bounded sickhouse/laundry interior
model (W04/W01 contacts and listener enclosure gains). Approved single-site water, cask-cover
and wooden-mallet actions and exterior Healer positional footsteps are also complete.
Detailed verification is retained in
the ledger and [current interior review](audio-direction/interior-runtime-review/README.md).
No repeat listening or unchanged-runtime acceptance gate remains.

The material and positional items below remain partially complete. Full room acoustics,
additional water/tool actions, wider positional emitters and off-screen cues need concrete new scopes
and actual authorized owners. Qualitative shelter ambience does not establish an interior
model; the completed two-structure enclosure subset does not establish full room acoustics.
Crossing water or tool eligibility does not establish an action event. Keep hearing,
captions/history, ducking and gameplay authority intact. Provisional numerical mix targets,
device listening and ordinary-play loudness compliance remain distinct from controlled
output evidence; resource packaging does not establish an executable release.

The approved [fixed garden foliage runtime](audio-direction/foliage-runtime-review/README.md)
is complete: one new CC0 leaf-rustle passage, the unchanged garden planting, bounded positional
Ambience, an anonymous tenth caption and independent transient history. Its full-quality,
native output/UI and resource packaging checks pass. The first
[exterior wind proposal](audio-direction/wind-owner-scope.md) is complete:
a dedicated derivative from the retained CC0 original and one fixed southern-road emitter,
with anonymous accessible cues and finite checks. The original's catalogue warns of bag
noise. The owner qualified the exact four-second G01 passage with “sounds right”, and the
[fixed southern-road wind runtime](audio-direction/wind-runtime-review/README.md) is complete:
one retained anchor, exclusive 256-pixel Ambience, 18-second starts, eleventh caption and
independent history. Source/binding, seven-mode full parity, 58 native output cases,
both-size accessible UI, mounted 38-stream packaging and all 95 root-quality stages pass.
No new weather, physical wind or gameplay authority is claimed. The first
[woodland rain proposal](audio-direction/rain-owner-scope.md) is complete: one
separate passage from the retained CC0 lossless original and the unchanged
(1000,350) clearing owner, with anonymous cues/history and finite checks.
The owner qualified the exact R01 candidate with “sounds perfect”, and the
[fixed woodland rain runtime](audio-direction/rain-runtime-review/README.md) is complete:
one unchanged clearing owner, exclusive 256-pixel Ambience, four seconds every
20 active seconds, twelfth anonymous caption and separate transient history. All
109 root-quality stages retain the previous 95; 5,383 unit tests, 48 focused cases,
58 measured native cases, both-size accessible UI, seven-mode complete parity and
mounted 39-stream packaging preserving all 105 prior entries pass. Wider emitters
and the items below remain partial. The first
[remedy-heating proposal](audio-direction/remedy-heating-owner-scope.md) is complete:
one exact existing second recipe-step commit at the retained (2300,1300) station,
a nominated owner-listened CC0 covered-pot Ogg with a disclosed source-format exception,
three-second local Effects feedback and independent caption/history, bounded by
finite files/checks and a separately recorded implementation grant. The owner-approved
[committed heating runtime](audio-direction/remedy-heating-runtime-review/README.md)
is complete: one three-second H01 Effects cue after the exact 1-to-2 commit, an
independent thirteenth caption and capacity-16 history. All 126 root-quality stages
retain 109 prior stages; 5,412 unit tests, 29 focused cases, seven complete mapped
preparation/treatment modes, 35 measured native cases, both-size accessible UI,
15 rejection probes and three mounted presets pass. The 110 prior audio entries
remain byte-identical; totals are 40 streams and 35 notices in 21 groups. Other
missing operations remain partial. Physical heater/fire/boiling/temperature
and gameplay changes remain separate. The first
[steeping proposal](audio-direction/remedy-steeping-owner-scope.md) is complete:
one exact third-step 2-to-3 commit at the unchanged station, distinct lossless-source
requirements, three-second Effects feedback, independent fourteenth caption/history
and finite files/checks. Under the separate recorded owner grant, the
[committed steeping runtime](audio-direction/remedy-steeping-runtime-review/README.md)
is complete: the owner accepted the exact replacement S01 with “Yes, this is
suitable” after rejecting the first candidate's wading/river-entry character.
One three-second lossless-derived Effects cue follows only the exact 2-to-3 commit,
with an independent fourteenth caption and capacity-16 history. All 140 root stages
retain the prior 126 in order; 5,442 unit tests, 30 focused cases, seven complete
mapped preparation/treatment modes, 37 measured native cases, both-size native UI,
15 rejection probes and three mounted presets pass. All 115 prior audio entries
remain byte-identical; totals are 41 streams and 38 individual notices in 22 groups.
The [cooling input-confirmation proposal](audio-direction/remedy-cooling-owner-scope.md)
is complete: exact immediate fourth-step 3-to-4, separate from finish/batch
installation; one quiet nonliteral Effects acknowledgement, fifteenth text line
and independent history are proposed. The owner approved the distinct
source/event/runtime grant on 2026-10-12, accepted the audible reference with “Approved”,
then chose “The louder version is more appropriate”. The
[committed cooling runtime](audio-direction/remedy-cooling-runtime-review/README.md)
is complete: one qualified 0.75-second lossless-derived C01 at unity original gain
(−12.23dBFS sample peak) follows only the existing fourth-step 3-to-4 commit. Its independent
fifteenth **Cooling step committed** caption lasts two active seconds; separate history
retains sixteen records. All 154 root stages and 5,472 unit tests pass, preserving the
prior 140 stages in order; 30 focused cases, seven complete mapped parity modes,
37 measured native cases, both-size accessible UI, 15 rejection probes and three mounted
presets pass. All 120 prior audio entries remain byte-identical; totals are 42 streams,
39 individual notices and 23 groups. The first recipe's four preparation operations now
have bounded feedback. Wider sources and other missing operations remain partial;
physical vessel/action, elapsed time, temperature/readiness and gameplay changes remain separate.

The approved [herb-collection runtime](audio-direction/herb-collection-runtime-verification.json)
is complete with the exact HC01 sound/window/level accepted as **suitable**. At the existing
flow commit, only the real retained Healer within inclusive 32 pixels of the unchanged
(920,350) patch and the Seeker within exclusive 192 pixels admit **Soft rustle**. The
sixteenth independent caption and capped anonymous history convey no collection outcome.
Remote or unqualified commits remain silent, with no gameplay, movement or picking action change.
All 177 root stages and 5,490 unit tests pass; fourteen genuine near/remote modes preserve
complete save/gameplay roots, eight admission probes and forty-five native Master cases pass,
and captions/history pass at both screen sizes. Three mounted presets retain all 125 previous
audio entries byte-for-byte, with 130 entries, 43 streams, 42 individual notices and 24 groups.
Wider sound coverage remains unfinished; no executable or fresh-device loudness claim.

- [ ] Add footsteps by terrain, water, wind, foliage, interiors, tools, and remedy preparation.
- [ ] Add positional ambience and meaningful off-screen sound cues.

Acceptance:

- [ ] The complete opening region has a coherent, accessible visual and audio identity, and all
  gameplay-relevant objects remain legible without hidden-object sparkle or pixel hunting.

## Phase 10 — Voice-ready interaction and optional ASR/TTS

- [x] Define `ISpeechRecognizer` independently of any provider.
- [x] Define `ISpeechSynthesizer` independently of any provider.
- [ ] Implement microphone permission and capture behind a platform-neutral boundary.
- [x] Implement push-to-talk first.
- [x] Add focus-independent tap/hold Space for player speech while preserving text entry.
- [x] Add partial and final transcript events.
- [x] Allow correction or confirmation before consequential actions when recognition is
  uncertain.
- [x] Implement streamed or buffered character speech playback.
- [ ] Implement per-character voice profiles without putting voice IDs in domain rules.
- [x] Implement player interruption and speech cancellation.
- [ ] Keep subtitles and full text interaction available at all times.
- [x] Add one ASR implementation and one TTS implementation.
- [ ] Add provider alternatives only after the common path is verified.
- [ ] Test device loss, permission denial, network failure, feedback loops, accents, and noisy
  input.

Pocket TTS service checkpoint (2026-10-13): the [first local TTS component](speech-direction/pocket-tts-service/README.md)
provides a provider-neutral PCM stream contract, a persistent offline English/German CPU worker,
request cancellation/recovery and a runnable C# service probe on M3 Max. ASR was not implemented at
that service checkpoint; its later integration is recorded below. This service checkpoint is closed;
its historical continuation text does not select new work. Character casting, expressive acting,
microphone input, packaged deployment and actual M1/other-platform verification remain.

Dialogue playback checkpoint (2026-10-13): [optional Pocket reply playback](speech-direction/dialogue-integration/README.md)
is composed in Main for English/German committed replies, with bounded PCM, explicit stop, replacement
interruption and text fallback. Current native UI/audio and full-quality evidence are bound in its
verification. That checkpoint did not include player voice-start/barge-in; the later integration
checkpoint below adds it.
The [streaming ASR comparison](speech-direction/asr-evaluation/README.md) recommends Moonshine Small
Streaming, with Whisper small + Silero VAD as the measured alternative. Synthetic English/German
recognition, VAD and retirement checks do not qualify physical microphone input or echo control.

Player speech checkpoint (2026-10-14): [selected Moonshine integration](speech-direction/player-asr-integration/README.md)
adds provider-neutral capture/recognition, opt-in Hold to talk and Listen (headphones), bounded editable
partial/final drafts, unchanged explicit Submit/Whisper, reply interruption and lifecycle fallback.
Actual local English/German inference runs through Main on retained synthetic PCM; native UI and full
quality are bound in its verification. Real OS permission, physical microphone/device/echo and accent/noise
qualification remain missing, so the broad capture/permission and device-test items remain open.

Space keyboard checkpoint (2026-10-14): [tap/hold Space](speech-direction/space-hold-to-talk/README.md)
keeps native short taps and starts voice after a 300ms hold across game and popup viewports, without
requiring a voice mode or button focus. Release finalizes an editable draft; explicit Submit/Whisper
remain the dialogue owner. Current native, selected-backend and full-quality evidence is retained.
Load fencing is exercised through the reset notification seam, alongside separate full-gate restore
coverage; a held-key end-to-end save/load trial remains unqualified. The owner's live recognition
report is positive but does not close the remaining human device/noise/echo qualification.
The [human trial record](speech-direction/player-microphone-qualification/README.md) now records positive
owner feedback on English/German recognition through held Space; device/route intake and remaining physical cases
are still open. Draft correction before Submit and Stop mic during a hold are also owner-confirmed.

Ollama reply prerequisite (2026-10-16): the owner-approved [participation repair](speech-direction/ollama-participation-repair/README.md)
adds the missing ordinary local participation binding and bounded cached-usage metadata support.
Selected `gemma4:e4b` commits a reply through Main on fictional input. The recovered full gate passed
217 recorded stages and 5,629 unit tests; the bounded repair is closed.
This is local model integration evidence, not a new physical microphone/Whisper/echo pass.

Conversation continuity prerequisite (2026-10-16): the owner-requested
[context repair](speech-direction/conversation-context-repair/README.md) supplies bounded heard/own
history with native speaker roles and exact completed examination outcomes. Five fictional
selected `gemma4:e4b` submissions verify earlier-name recall and post-examination safety reporting.
Privacy, budget and successful-load fencing pass with full current quality. The repair is closed;
history is session-local, and the existing human microphone/device/echo cases remain unchanged.

Acceptance:

- [ ] Typed and spoken input produce the same dialogue events and intention flow, and the game
  remains fully playable without microphone or speech services.

## Phase 11 — Cross-platform hardening and release candidate

- [ ] Validate fresh installation on supported Windows systems.
- [ ] Validate fresh installation on Apple Silicon macOS.
- [ ] Validate Intel macOS if retained in the support matrix.
- [ ] Validate representative supported Linux distributions and display servers.
- [ ] Test high-DPI displays, window resizing, full-screen modes, and multiple monitors.
- [ ] Complete remappable keyboard, mouse, and controller controls.
- [ ] Complete keyboard-only navigation and screen-reader evaluation for text-heavy UI.
- [ ] Test long sessions, repeated saves, corrupt-save recovery, and schema migrations.
- [ ] Test local-model performance and constrained hardware behaviour.
- [ ] Test provider rate limits, outages, cancellation, and credential removal.
- [ ] Ensure diagnostics exclude credentials and dialogue content by default.
- [ ] Establish crash reporting and consent policy.
- [ ] Establish package identity, icons, versioning, and update policy.
- [ ] Build platform-native packages on native runners.
- [ ] Sign Windows packages when distribution credentials are authorized.
- [ ] Sign and notarize macOS packages when distribution credentials are authorized.
- [ ] Choose and validate Linux package formats.
- [ ] Run complete opening-scenario smoke tests on packaged builds.
- [ ] Prepare release notes, privacy information, model-provider disclosures, and known
  limitations.

Controls runtime checkpoint (2026-10-13): the exact approved [29-action remapping system](input-direction/remapping-runtime-review/README.md)
is implemented and verified by 188 root quality stages, 5,576 unit tests, genuine Main synthetic
input routing, native 1280×720/1920×1080 UI and three mounted resource presets. The finite
implementation is closed. This checkbox remains open until ordinary physical keyboard, mouse and
named controller evidence establishes the supported-device claim. The [physical-device evaluation proposal](input-direction/physical-controls-owner-scope.md)
is complete. The owner [deferred its execution](input-direction/physical-controls-deferral.json) while procuring a controller;
only a MacBook Pro keyboard/trackpad is currently available. Its hardware/session grant and actual outcomes
remain missing. Controller-only opening-scenario,
broad screen-reader and executable-release certification remain unclaimed. The [dialogue-panel keyboard/accessibility proposal](input-direction/dialogue-accessibility-owner-scope.md)
has now been implemented under the exact owner grant. The [finite dialogue review](input-direction/dialogue-accessibility-runtime-review/README.md)
retains actual Main keyboard routing at 1280×720/1920×1080, default/remapped focus and cancel,
exact existing submission semantics and explicit assistive metadata. VoiceOver was not running;
reader evaluation is recorded not performed. This item remains open for actual named-reader
outcomes and other text-heavy UI; no broad reader certification is claimed.

Acceptance:

- [ ] Signed or explicitly documented unsigned release candidates install, launch, save, load,
  configure supported providers, and complete the opening scenario on every claimed platform.

Privacy/provider disclosure checkpoint (2026-10-13): current-source [privacy information](release-direction/privacy.md)
and [model-provider disclosures](release-direction/model-providers.md) now document local data, credential
controls, outbound dialogue/role context, diagnostics and deletion. Their [source record](release-direction/privacy-provider-disclosures.json)
retains the ordinary OpenAI/Ollama composition and uninstalled Anthropic/offline-conversation limits.
This is documentation evidence, not legal compliance, provider policy, live service or release certification.
The release-information checkbox remains open for development/release notes, known limitations and
actual release-specific inputs; physical controller execution stays deferred.

## Phase 12 — Deferred expansions

These are intentionally outside the first vertical slice and should not be pulled forward
without an explicit roadmap decision:

- [ ] Online cooperative multiplayer with server-authoritative world state.
- [ ] Asynchronous human turns.
- [ ] Shared-computer multi-character play.
- [ ] Human takeover or reassignment of any LLM role during a campaign.
- [ ] Additional realms, diseases, remedies, and evidence networks.
- [ ] Scenario-authoring tools suitable for non-programmers.
- [ ] Mod and community-scenario packaging.
- [ ] Procedurally assisted landscape dressing that preserves authored discovery logic.
- [ ] Additional accessibility modalities.
- [ ] Mobile or web ports.

## Cross-cutting engineering requirements

These standing requirements apply to every phase:

- Keep domain rules deterministic and independently testable wherever possible.
- Keep Godot types at the presentation and engine-integration boundary.
- Keep provider types inside provider adapters.
- Give each LLM only its character's filtered context.
- Validate all world-changing intentions in the engine.
- Never embed cloud-provider credentials in source or distributable assets.
- Preserve full natural-language transcripts locally while sending only relevant bounded
  context to a provider.
- Do not store hidden chain-of-thought.
- Make critical quest knowledge redundant and recoverable.
- Treat silence, refusal, uncertainty, and disagreement as valid character behaviour.
- Ensure the game remains saveable when a provider is unavailable.
- Add automated tests before or with testable behaviour, not only after regressions.
- Use representative manual play evidence for interaction and visual claims.
- Update this file and `HANDOVER.md` at the end of each completed slice.
