# First exterior wind source and emitter proposal

Prepared 2026-10-12 (Australia/Sydney). “Read HANDOVER.md and follow the
instructions - scope is approved” authorizes the ledger's finite read-only
wind proposal. This completes that scope. Exact inputs, file permissions and
checks are bound in [wind-owner-scope.json](wind-owner-scope.json).

## Concrete result

Propose one occasional four-second **wind passage** at the existing projected
`southern-road` anchor **(1500,1400)**. Bind it through
`Main/OpeningRegionSouthernRoadWagonSceneSource.Binding.SouthernRoadAnchor`
and the exact retained projection. The source Node2D has identity transform;
its existing hidden `RoadArea` line ends at that anchor. Preserve that line's
visibility, points and 96-pixel width, the source's children and order, road
boundaries, wagon shell/marker, accepted ground/path artwork and navigation.
Add only Main sibling audio source/controller/check nodes before Channel.
No new marker, scenery child, collision, animation or physical wind is required.

The source is the exposed road, not the abandoned wagon or a concealed clue.
No background, access variation, inventory, investigation or treatment state
selects it. Reading the anchor supplies presentation coordinates only. This
advances “Add positional ambience and meaningful off-screen sound cues” with
one distinct wind owner; regional identity and wider emitter coverage remain
unfinished. The garden foliage owner and all prior owners stay closed.

## Missing dedicated passage and source qualification

No dedicated wind passage, source-window acceptance or emitter is installed.
Nominate the **existing untouched original**, `wind background noise 2.wav`,
from “Mild Wind Background Noise” by Bashar3A, acquired 2026-10-07. The local
source-specific catalogue capture and provenance record CC0-1.0 and the supplied
credit “Bashar from Skirmish.io”. The original is 44.509615 seconds, stereo,
44.1 kHz float32 WAV; its recorded SHA256 is
`9f1fd5c8b37949e6889a7f6acbd822f66e0ab4e918d72bda58d8d8c2c7e85a80`.
This planning slice verifies retained bytes and rights-record hashes; it does
not claim a fresh remote licence check, listening or new source acceptance.
The captured catalogue explicitly warns of intermittent bag noise; a clean
four-second window is therefore a qualification condition, not an assumption.

B04 is an accepted wind/rain/creak composite. Do not extract from, subtract
layers from, relabel, reprocess or replace B04. A separate derivative made
from the untouched original is the new permission proposed here. F01 and all
accepted auditions/derivatives/provenance remain unchanged.

After the implementation grant, inspect the captured catalogue/deed and notices,
decode the original, listen and select one contiguous usable four-second
passage. Reject voices, identifiable wildlife, rain/leaf impacts, footsteps,
handling/microphone buffeting, machinery, clipping and abrupt edits. The title
and existing B04 acceptance do not qualify this new passage. Record the chosen
frame window and listening assessment and present only the new passage with
the dependent review. If the candidate cannot qualify, report the blocker;
no second original, substitute, synthesis, purchase or account is included.
Standing sound search/download permission remains recorded for other authorized
work, but acquisition is unnecessary for this proposal.

Prepare exactly one **G01.wav**, 192,000 stereo PCM24 frames at 48 kHz
(four seconds), nonlooping. Allowed edits: recorded contiguous trim,
deterministic resampling, 100 ms linear endpoint fades and one uniform gain
that only attenuates. No filtering, layering, denoising, pitch/time change or
stereo widening. Provisional source sample-peak ceiling: **-24 dBFS**; do not
boost a quieter source. Record decoder/tool versions, original/decoded/window/
derivative/import hashes, frames, parameters, peak and loudness. Preserve
source-specific CC0 evidence, supplied credit and derivative change notice in
new wind provenance, credits and packaged deed. No prior notice is edited.
Controlled source/output measurements do not establish device listening or
ordinary-play loudness compliance.

## Admission and native playback

`OpeningRegionWindAudioSource` retains the exact live road source, binding,
layout, projection/anchor identity, source transform/children, RoadArea endpoint,
road/wagon geometry and unchanged ready navigation map/polygon. Bind source
and physical prerequisites by reference and contents, rejecting replacement,
hide/disable, transform/topology/shape drift, nonfinite inputs, inactive map
or teardown. `RoadArea.visible=false` is retained data, not a sound gate;
source `IsVisibleInTree` permits off-screen playback. No wagon emitter exists.

Require a finite actual Seeker body position on the unchanged ready active map,
with nearest supported point within one projection-local pixel, world bounds
[0,0,3200,1800], outside inclusive sickhouse [2480,1310,2800,1502] and laundry
[1420,120,1580,360]. Retain and validate those room owners; no transmission or
full room model is added. Reuse only the exact current, top-level,
unrotated/unit-scale `Main/OpeningRegionPositionalAudioController/SeekerListener`,
within one pixel of the body. Never create/move the listener or call MakeCurrent.
Use new physics priority **120**, following foliage (110) and Healer (100).

Admit world distance **strictly less than 256 pixels**. At 256 and beyond there
is no sound, current caption or history. Validate support, owner identity,
listener and range throughout a passage; failure cancels it without replay.
A source outside the actual viewport can produce a generic bearing. No hearing,
weather state, wind direction, propagation, wall paths or disease time is read.

Own one `AudioStreamPlayer2D` fixed at (1500,1400), **Ambience**, MaxDistance=256,
Attenuation=1, PanningStrength=0.6, PitchScale=1, MaxPolyphony=1, AreaMask=0,
unity player gain and no loop. Expected native distance factor is
max(0,1-distance/256). Apply no second attenuation, manual pan, enclosure gain,
boost, ducking lease or music hold. Existing Master/Ambience controls, mono,
Default/Quiet and foreground ducking apply once through the retained buses.
B04 and other beds remain audible according to their existing owners. Measure
coexistence without reducing or rescheduling them. Missing/malformed G01 or
missing Ambience bus means silent audio with otherwise admitted text/history;
never route through Master as a fallback.

## Presentation clock, cancellation and accessibility

Start at **18 active seconds**, then every 18 active seconds, with one
four-second passage and at least 14 seconds between passages. This is a local
presentation clock, independent of random/weather/actions/world time. Advance
only finite deltas (0,0.1]; invalid delta/stall cancels and rebases the wait.
Consume each scheduled start even unsupported, out of range, muted, missing
resources or captions-off. Approach/unmute/resource repair mid-passage never
starts sound or text. Fresh admission occurs only at the next scheduled start.
There is no overlap, catch-up queue or ordinary injection API.

An Always-process watcher cancels this owner on pause, focus loss, dialogue,
reading/customization/map/journal/settings overlays, text entry, prerequisite
replacement/disable or listener discontinuity over 96 pixels. Blockers freeze
history's limiter, preserve existing entries, and resume with a fresh 18-second
wait. Mute or zero Master/Ambience stops only this audio immediately; admitted
text may continue, and unmute does not resume it. Successful PresentationReset
clears the new player/schedule/caption/history/sequence/limiter; failed load
changes nothing. Teardown clears the new transient state. Serialize none of it.

Add an independently owned **eleventh** caption line, **“Wind blowing”**, for
the admitted four seconds. Capture its optional eight-way bearing at start,
using GlobalCanvasTransform × CanvasTransform, inclusive viewport edges and
the retained pure positional caption tie rule. No site, actor, identity,
coordinates, wind strength/direction/weather fact, clue or world timestamp is
shown. Captions preference hides only the line; mute/resource failure preserves
admitted history. Clear only wind. Expand only the panel's own bound if needed
for eleven wrapped lines, preserving all ten prior lines/timers.

`WindCueHistory` owns a separate capacity-16 history, oldest-first eviction,
detached read-only entries, local sequence and generic phrase/bearing only.
Record the first admitted start immediately, then at most once per 18 active
seconds; discard rate-limited starts without queueing. Add **Nearby wind cues**
to Audio → Recent sound cues, retaining each existing section, capacity, order,
limiter and clearing behavior. Muted/captions-off/missing audio history remains
readable. Preserve scrolling, selection, Back to Audio, Escape, Tab/Shift-Tab
and return focus. Reading that overlay produces no cue. No journal/transcript
integration or screen-reader compliance claim is included.

## Implementation files and finite completion

The JSON names every permitted new file and narrow existing edit. New pure
Application spatial/schedule/history contracts and tests, Game source/controller/
ordinary smoke/stopped native fixture, G01 preparation/provenance/notices,
Main sibling wiring, one caption/history section, credits and resource filters
are the implementation. Pin unchanged foliage validators/fixtures to this
baseline through the existing isolated historical runner; retain prior pins.
Do not rewrite frozen validators or other controllers, movement, geometry,
art/gait, Domain, hearing/perception, providers or persistence.

Complete the following after the one new grant, within the same session:

1. Qualify the nominated original and selected passage; retain rights/notices;
   reproduce and measure exact PCM24 G01 and nonlooping import; present the new
   passage and record its listening/selection. Report an unqualified-source
   blocker instead of substituting audio.
2. Focused pure tests and live checks: 18-second schedule, four-second expiry,
   stall/block/reset/no catch-up, exclusive 256 edge, supported exterior listener,
   exact road/anchor/map/boundary binding, replacement/drift, history capacity/
   limiter and isolated cancellation. Assert the source remains (1500,1400).
3. Seven identical actual-motion schedules: Default, Quiet, Master-muted,
   Ambience-muted, missing G01, captions-off and wind-disabled baseline. Compare
   complete saves/transcripts/movement and protected inventory/discovery/sabotage/
   time/hearing/perception truth. First six consume equal eligible starts; the
   disabled baseline has no new presentation. Retain all prior fixtures unchanged.
4. Measured native Master cases: distances 0/64/128/192/255/256/257; left/right,
   mono, zoom/bearings, Default/Quiet, independent gains/mutes, foreground ducking/
   recovery, missing stream/bus, range entry/exit, pause/focus/overlay/load/failure/
   teleport, replaced/unsupported sources/listeners and overlap with B04 plus
   retained owners. Keep distinct subcases and onset/tail/cancellation measures;
   compare native relative gains at 64/128/192 within 5%, pan direction and mono
   parity. Rejected/stopped cases must be silent; capture final peaks/loudness.
5. Native eleven-caption coexistence and independent history at 1280×720 and
   1920×1080, all eight off-screen bearings/inclusive edges, muted/captions-off/
   missing-audio access, capacity/limiter, keyboard/focus and cross-owner clearing.
   Inject only in stopped diagnostics; ordinary motion establishes admission.
6. Mounted resource archive: **38 streams**, preserve all **100 current audio
   entries** byte for byte, compare imported G01 to recorded PCM24→PCM16 conversion,
   retain **26 notices/18 groups** and add exactly wind CREDITS.txt, provenance.json
   and deed.html: **29 notice files/19 groups**. Original/decode/review audio stays
   outside runtime exports. Verify wind credits and notices in every retained
   preset; resource packaging is distinct from an executable release.
7. New rejection probes: source/proposal/PCM/binding hash drift, changed emitter/
   range/schedule/bus, protected-truth drift, missing measured evidence and old
   caption/history mutation. Run focused checks, `python3 quality.py --self-test`
   and complete root `./quality.sh`, retaining all 82 current stages/completion
   markers. Record grant, review, verification and whole-slice closure; sync/check
   continuation; commit and push under standing publication authority.

Planning verification reuses the passing 82-stage/5,287-unit-test product
record after checking its input hashes and all protected tracked files. Run
continuation's 11 regression probes and whitespace/binding checks. Product/CI
inputs are unchanged, so no full quality rerun is needed for these documents.

## Single new implementation decision

Authorize qualification and preparation of **G01 from the exact retained wind
original, one southern-road-only 256-pixel Ambience emitter, four-second passages
every 18 active seconds, the eleventh anonymous caption and independent transient
history**, plus the named checks/review/publication. This is the missing new
runtime/source-use permission. The approved active planning scope is complete;
no prior owner needs approval again. Additional sources/emitters, physical wind,
weather, full acoustics, gameplay, art/gait, voice and release remain separate.
