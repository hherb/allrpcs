# Positional ambience and heard conversation cues: prepared scope

2026-10-08 · `phase9-positional-audio-cue-scope` planning complete.
Implementation **awaits one new owner grant** for `phase9-positional-audio-cue-runtime-integration`.

The first boundary is **one spatial waterway ambience owner using unchanged B02**, plus
**one short conversation indication after a heard Healer/Sage response is recorded**.
Use an explicit Seeker listener, existing Ambience/Dialogue controls, safe directional
captions and a bounded recent-cue view. This advances the positional/off-screen roadmap
item; it does not complete every possible environmental or consequential sound.

[The inventory](positional-cue-runtime-scope.json) binds inspected sources, accepted audio,
one proposed PCM range and the exact implementation files. All runtime inputs remain
unchanged in this planning task. The standing [publication instruction](footstep-runtime-review/publication/authority.json)
authorizes publishing the completed proposal; it supplies no new runtime authority.

## Actual sources and prerequisites

`OpeningRegionMaterialAudioController` currently creates four **non-positional** looping
beds and two Effects players. `OpeningRegionMaterialAudioProjection` selects the nearest
exact landmark, maps its current sound key and ramps four weights over 0.75 seconds.
That selection is presentation, without a character hearing or physical material fact.
B02 combines running water and rain; B04 combines wind, rain and inert timber creaks.
Neither permits inventing a separately moving wagon, water-contact event or isolated
water stem. The live environment source already owns the quieter woodland state.

`OpeningRegionStreamLaundryGeometry.StreamWater` gives the existing water polygon:
(1600,452), (2000,452), (2000,548), (1600,548), in the region projection space.
This supplies a concrete spatial ambience extent without another physical owner.
The stream interaction rectangle is narrower; use the water polygon, not bridge
navigation, laundry walls, accepted sprites or a new material lookup.

`TemporaryCharacterTurnResponseSpeechRecorder` records a fresh complete Healer or Sage
response by calling `DialogueEventController.TryOccur`, then appending its exact delivery
to `CharacterTurnTranscriptController`. It suppresses repeat processing by retained
delivery identity. Its hearing query uses **192 pixels ordinary / 48 pixels whisper**.
The ordinary roster is retained healer-first; Healer and `SageHearingPosition` own the
actual positions. `DialogueEvent.Audience` is the authoritative admitted audience.
Object visibility snapshots describe unnoticed/noticed/identified objects, and have no
general environmental hearing rule. Do not reinterpret them as a hearing-radius grant.

Add an **internal, exception-isolated notification after both occurrence and transcript
append succeed and `lastRecordedDelivery` is assigned**. Carry the exact event, retained
speaker/Seeker IDs, speaker node/world position and selected existing hearing range.
Do not poll `LastOccurredEvent`: it can be overwritten, restored or produced by unrelated
call sites. Do not notify on provider chunks, response preview, request, turn completion,
physical-action intention or model participation. These are different events.

| Source / accepted material | First implementation decision |
| --- | --- |
| B01 sheltered rain | Retain its existing player, selection, waveform and caption. No room/reverb simulation. |
| B02 water and rain | Replace its non-positional playback owner with exactly one positional owner using the same installed WAV and existing weight. The composite rain moves with the waterway too; approval includes this approximation and distance reduction. |
| B03 canopy rain | Retain regional playback and the exact revealed-state gain. No inscription bearing/beacon. |
| B04 wind/rain/inert creak | Retain regional playback. A creak baked into a bed cannot indicate a new wagon event. |
| W01–W04 terrain contacts | Preserve all 24 installed slices, Seeker cadence, cancellation, captions and credits. Healer/Sage steps remain outside this scope. |
| A01 water contact | Defer: no committed wading/splash/laundry action owner. |
| A02 container/cloth; A03 tools | Defer: focus, examination and a field lens do not establish those physical sound-producing actions. |
| A04 grinding; A05 treatment | Preserve existing successful-action triggers, full passages, Effects routing and A05 lease. Their grinding/treatment gates (48/64 pixels) do not authorize distant NPC action cues. |
| A06 conversation component | One exact 0.5-second start-click slice for heard ordinary-roster replies, once on Dialogue. It is a nonverbal delivery indication, without voice or a conversation-end claim. |
| A06 Effects component / UI | Defer general interface sound; neither the combined A06 nor its UI component ships in this grant. |
| M01 / M02 shakuhachi | Preserve accepted material; defer music scheduling/rest semantics. |
| Patient testimony, Sage consultation, misquotation, sabotage and world-event cues | Defer these additional producers. They call occurrence through distinct commit, exposure or hearing paths; this grant subscribes only to the ordinary response recorder. |

## Waterway position, distance and playback

Transform the Seeker into the projection source's space, choose its nearest point on
`StreamWater` (inclusive rectangle clamp), then transform that point back to global
coordinates. Keep the geometry owner and its coordinates unchanged. The nearest point
is an approximation of an extended water source; it never becomes an interactable
emitter, target or discovery fact. Reject nonfinite, absent or foreign topology/geometry
inputs rather than substituting a hidden landmark.

Add one `AudioListener2D` at the Seeker's global position, with zero rotation and unit
scale, made current only in the ordinary audio viewport. Distance follows the Seeker
through camera smoothing, limits and zoom. A default screen-centre listener would give
different hearing distances; [Godot's listener documentation](https://docs.godotengine.org/en/stable/classes/class_audiolistener2d.html)
describes this distinction. The listener is presentation ownership, without world hearing
or visibility mutation. No second viewport/listener may double the mix.

B02 uses `AudioStreamPlayer2D`, `Ambience`, `MaxDistance=768`, `Attenuation=1`,
`PanningStrength=0.6`, `PitchScale=1`, `MaxPolyphony=1`, `AreaMask=0`. Its base volume is
exactly the existing B02 ramp weight. With distance `d`, attenuation is
`max(0, 1-d/768)`; no extra distance multiplier or compensation gain is added.
[Godot's 4.6 player implementation](https://github.com/godotengine/godot/blob/4.6/scene/2d/audio_stream_player_2d.cpp)
computes distance in world coordinates and panning in scaled viewport coordinates.
Panning can vary with zoom/viewport; **admission and attenuation cannot**. Installed
Godot is 4.6.3; implementation must measure its actual output, including centre-channel
scaling, rather than treating the formula as final Master loudness evidence.

`OpeningRegionMaterialAudioController` keeps its exact selection and ramp ownership,
but forwards B02 weight/selected-state to the positional controller and stops creating
or starting a non-positional B02 player. No extra rain layer, second B02 path, fresh
source acquisition, normalization, pitch or loop edit. Preserve whole-WAV looping,
phase through mute/unmute, retargeting and startup/load recomputation. Maximum looping
bed owners remains four; A04/A05 and the footstep pool remain as implemented.

The region caption for a selected waterway is “Running water and rain” while presentation
weight and distance are eligible; append a direction only if its nearest source point
is off screen. At zero weight or `d >= 768`, clear this waterway wording. Output gain,
mute, missing resources and mono do not change caption eligibility. Non-waterway region
captions remain unchanged. No loop-by-loop caption or history spam.

## Heard response eligibility and meaningful off-screen presentation

Subscribe only after scene sources, settings, caption panel and recorder are ready.
Require a live recorded event carrying the exact retained delivery, registered Healer or
Sage speaker and a Seeker ID equal to an entry in **that event's admitted audience**.
Reject self speech, absent listener, unknown/foreign speaker binding, stale notification,
missing retained transcript entry, malformed coordinates and source-query/live-position
mismatch. Resolve IDs through the existing retained roster; no invented character or
object eligibility. Convert positions into a common world space. The recorder currently
uses Main-local positions; do not silently treat those as global after a parent transform.

Recheck live listener/source separation against the captured existing range: inclusive
192 or 48 pixels. This may only **remove** eligibility; moving closer cannot admit an
unheard event. Do not re-run hearing to recruit a listener after the event. Render the
same eligible cue on screen or off screen, so a camera boundary does not create or
suppress a world occurrence. Determine off-screen status by the actual viewport canvas
transform and visible rectangle, including camera smoothing/limits/zoom/resize.

The ordinary 192-pixel audience usually fits inside a 1280×720 view at default zoom.
At zoom 2, the vertical half-view is 180 world pixels: a valid reply at 190 pixels can
be off screen. Camera smoothing/limits also affect presentation. This is a bounded,
occasionally off-screen cue; **there is no new long-range overhearing**. Tests must cover
both a real ordinary recorded reply and a controlled off-screen camera case, and report
which is demonstrated. Do not move characters or widen hearing to manufacture a demo.

For an eligible live event, play the proposed start-click once at its registered speaker's
current position using one `AudioStreamPlayer2D`, `Dialogue`, `MaxDistance` equal to the
captured hearing range, `Attenuation=1`, `PanningStrength=0.6`, `PitchScale=1`,
`MaxPolyphony=1`, `AreaMask=0`, base gain 1. No second Effects/Speech route and no ducking
lease: this optional delivery marker is not voice or a treatment consequence.
At the inclusive hearing boundary the event/text/caption remains admitted although the
positional amplitude reaches zero. No gain boost to compensate for the quiet accepted click.

Maximum one click start per presentation second and one active click; no delayed queue.
A new eligible event may replace the short player only after this rate limit. Each fresh
recorded event still updates caption/history when a click is rate-limited, muted or
missing. The recorder's existing once-only delivery guard remains authoritative;
never clear it from audio code. Sound history is not an event deduplication store.

For three presentation seconds, show “Conversation nearby” on screen, or “Conversation
above/right/below/left” (including four diagonal combinations) when off screen. Choose
the nearest of eight 45-degree screen-direction sectors from viewport centre to source;
exact sector ties use clockwise order. A degenerate bearing uses “nearby”. Do not print
speaker identity, hidden location, utterance text, diagnosis or clue name in sound captions.
Ordinary and whispered wording is identical; the hearing grant still differs.
Existing response text remains available through `TemporaryCharacterTurnResponseLabel`.
No per-letter click, stream-fragment playback or synthetic speech.

## Cancellation, captions, history and failure isolation

Keep the listener/source positions live during the 0.5-second click and three-second
caption. Stop/clear on departure beyond the captured range, invalid binding, turn
cancellation, successful restoration, scene teardown or Seeker teleport (a world
position discontinuity over 96 pixels in one physics sample). Never replay on return,
unmute, caption re-enable, restored transcript, restarting a provider or a finished turn.
A normal completed recorded turn is not cancellation. Failed restore does not reset.
Mute affects output only; it must not erase admitted caption/history. Missing stream
fails quietly, preserving gameplay, full text and eligible caption/history.

Extend `MaterialSoundCaptionPanel` with one independently timed conversation line and
bounded height sufficient for four lines. Preserve region/action/footstep text, timing,
focus exclusion and each controller's cleanup ownership. Optional Captions hides the
new line immediately; it does not suppress history or existing dialogue/action text.
Resuming captions may show only a still-live interval, without replaying sound.

Add an accessible “Recent sound cues” view from the existing Audio panel: at most 16
plain entries containing the admitted generic cue wording and sequence order, oldest
first, with scroll, keyboard/controller return and focus restoration. No world time,
new dialogue transcript or persistence. Record the emission-time wording once per fresh
eligible event; expiry/departure keeps history, successful load/teardown clears it.
History remains available with all audio muted or captions off. It adds no new required
information: existing full response text owns the statement. Do not add ambience loops
or footstep contacts to this view. No changed journal, discovery, save or provider contract.

Every new presentation subscriber is exception-isolated at the recorder's completed
commit notification. Audio/history failures cannot throw through occurrence/transcript,
deny a reply, change participation, advance time/disease, or turn a provider failure into
success. Disconnect handlers, stop owned players and release only the owned current
listener on teardown. Do not disturb A05 leases or other mixer state.

## Exactly one derivative and retained rights

Proposed `game/assets/audio/positional/A06-D01.wav` is stereo PCM24, 48 kHz,
24,000 frames / 144,000 PCM bytes. Copy **frames [192000,216000)** (4.0–4.5 seconds)
from accepted `material-audition/clips/A06-Dialogue.wav`. Expected PCM SHA-256:
`587f7ba3c73f6f92eee0b2a07d24cd324484118571a202703f4281cc9f8c9de6`.
This range contains the already edited start click and its retained silence. Exclude the
end click at 6 seconds: a recorded reply is not a conversation-end event. No resampling,
fades, channel conversion, normalization, gain bake, source regeneration or source-WAV
change. The derivative header hash is recorded only after authorized extraction.
No derivative has been created by planning. Sample extraction itself is part of the
single requested new grant; audition acceptance alone did not authorize it.

B02 stays at `game/assets/audio/material/B02.wav`, byte-identical to terminal acceptance.
Keep its existing contributor notices. The new positional directory gets provenance,
`CREDITS.txt` and the retained CC0 notice for **Interface Sounds by Kenney** from the
accepted rights record. Cite the original source and edit/extraction lineage, without
reopening licensing or sound selection. Append those credits to the existing source view
and explicitly package positional TXT/JSON/HTML notices. No purchase/download/release.

## Exact allowed implementation files

The structured inventory enumerates every path; the following explains the changes.

| Files | Allowed change after the new grant |
| --- | --- |
| `game/Main.tscn` | Add positional controller, response-cue history scene and diagnostics references/nodes only. Runtime controller owns a transient listener at the Seeker. No physical/art/camera/input rewrite. |
| `game/OpeningRegionMaterialAudioController.cs` | Hand off only B02 playback/selected caption; keep the existing ramp, B01/B03/B04, A04/A05, reset and lease semantics. Adapt standalone diagnostics to the same handoff. |
| `game/TemporaryCharacterTurnResponseSpeechRecorder.cs` | Internal notification after completed occurrence/transcript/once-only marker; exception isolation. Preserve hearing, recording and participant logic. |
| `game/OpeningRegionPositionalAudioController.cs` and `.uid` | One B02 owner, one reply player, explicit listener, subscriptions, position/eligibility/cancellation and safe caption/history orchestration. |
| `src/Application/Audio/OpeningRegionPositionalAudioProjection.cs`; matching Application tests | Pure closest-source, distance, audience/identity, bearing, rate, caption/history bounds and cancellation helpers; no Godot dependency or new Domain facts. |
| `game/MaterialSoundCaptionPanel.cs`; `game/AudioSettingsPanel.cs` | One caption line and one recent-cue entry; retain existing settings/storage/buses, labels and controls. |
| `game/PositionalSoundCueHistoryPanel.cs`, `.uid`, `.tscn`; `game/MaterialAudioCreditsPanel.cs` | Bounded accessible cue view, return/focus; append packaged positional credits. |
| `game/assets/audio/positional/` | Exactly A06-D01 WAV/import, provenance, credits and retained CC0 HTML. No cache or other audio. |
| `game/export_presets.cfg` | Add only positional notice include filters to existing development presets. |
| `game/PositionalAudioRuntimeSmokeCheck.cs`, `.uid`; `game/PositionalAudioRuntimeProof.cs`, `.uid`, `.tscn` | Ordinary recorder/audience/camera/load negatives and stopped-by-default final-output diagnostics. |
| `game/MaterialAudioRuntimeProof.cs`, `game/MaterialAudioRuntimeProof.tscn`; `game/MaterialAudioRuntimeSmokeCheck.cs` | Minimal B02 handoff/listener wiring and updated split-owner/caption assertions. Preserve every other retained case; the new successor check independently proves single B02 ownership. |
| `scripts/prepare-positional-cue.py`, `validate-positional-audio-runtime.py`, `check-positional-audio-output.py` | Exact extraction, successor boundary/negative checks and actual Master-output analysis. Frozen predecessor validators stay intact. |
| `audio-direction/positional-cue-runtime-authority.json`, `positional-cue-runtime-verification.json`, `positional-cue-runtime-review/`; ledger/handover/roadmap | Record actual grant and evidence; close only the finite named implementation. Keep the wider positional/material/music roadmap items unfinished. |

Geometry, Seeker/Healer movement, gait/art, footstep product files, Main dialogue scheduling,
Domain hearing/perception/save contracts, mixer/settings storage, existing accepted assets,
other producers, music, speech, release/signing and the local `game/project.godot` rewrite
are outside the implementation grant. A successor validator compares the protected input
manifest to the inspected commit, permitting only the exact changes above. It does not
rewrite or rerun frozen predecessor publication validators against a new successor.

## Finite checks and completion

Planning requires inventory/hash/range checks, protected product-byte parity, continuation
self-test and whitespace review. The footstep successor check passed before writing this
proposal. Reuse its passing product evidence: a documentation task needs no new full suite.

After approval, implementation closes only when all these checks are recorded:

1. Accepted B02 and A06 parent hashes, exact derivative PCM/header/import and packaged
   contributor notices pass. Reject wrong interval, changed sample, duplicate B02 route,
   unknown producer, changed hearing/camera/geometry/footstep/mixer and extra shipping WAV.
2. Exercise water polygon nearest points/corners/inside, all eleven anchors, ties,
   0.75-second ramps/rapid retargets, return visits, reset and invalid/foreign inputs.
   Verify one B02 owner and four total looping beds, no added rain, retained other cues.
3. Through the ordinary response recorder, prove exact event/transcript completion,
   healer/sage mapping, heard/unheard ordinary/whisper boundaries, self/foreign/stale
   negatives, provider fragment/failure/cancel, repeat frames and rapid fresh replies.
   No actual provider/network call is needed: use existing deterministic response fixtures.
   Compare full gameplay/save roots, transcripts, event counts, perception, disease/time
   and participation with enabled, muted, missing resources and captions-off output.
4. Prove range and gain independence from camera smoothing/limits/zoom (0.8/1/2) and
   both 1280×720 and 1600×900; test actual transformed on/off-screen classification and
   all eight caption sectors. Include the 190-pixel vertical / zoom-2 off-screen case.
   Cover parent transforms, teleport/load, failure teardown, rate cap, history rollover,
   no restored/re-entry replay, missing listener/resource/bus and single-viewport mix.
5. Render actual final Master stereo/mono for centre/left/right/above/below water and
   reply sources, distance zero/half/boundary/outside, B02 crossfade/seams and mixed
   footsteps/A04/A05. Check existing category and Master gain/zero/mute, presets,
   source-to-output latency, reset/cancellation, channel balance and A05 lease isolation.
   Record measured levels/true peaks and deviations; no new loudness/device claim follows
   from unchanged accepted clips. Caption/history gating must pass even if output is zero.
6. Inspect ordinary Main with four caption lines, recent cues, credits, mute/caption
   toggles and focus/return at both viewports. Package the new derivative/notices plus all
   existing six sounds/24 contacts. Keep the known executable-export/device limits explicit.
   Run meaningful pure tests, format/build/four unit projects, ScenarioValidator, retained
   Godot smoke suite and new recorder/output checks using successor boundary validation.

The only decision now needed is approval of the **new positional owner, captured-audience
reply notification, one exact PCM extraction, listener/caption/history/credits and finite
verification boundary above**. Record the actual response in the authority artifact before
changing runtime files. Closed artwork, listening selection, six-sound action behavior and
footsteps keep their terminal acceptance; no repeated approval of those decisions is sought.
