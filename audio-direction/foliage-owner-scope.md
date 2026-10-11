# First fixed foliage emitter proposal

Prepared 2026-10-11 (Australia/Sydney). The owner approved the active planning
scope with “Read HANDOVER.md and follow the instructions - scope is approved”.
This proposal completes that scope. Its exact retained inputs and proposed
implementation files are bound in [foliage-owner-scope.json](foliage-owner-scope.json).

## Concrete result

Add one occasional, quiet **leaf rustle** at the existing
`Main/OpeningRegionRemedyPreparationSceneSource/BroadleafFoliageTopLeft`.
The garden source is at `(2300,1300)` and this sprite's planted origin is
`(-68,28)`, giving world emitter position **`(2232,1328)`**. Retain the exact
72×56 broadleaf texture, offset `(-36,-53)`, unrotated unit scale, parent,
visibility and child order. Add the audio controller as a Main sibling; never
add a child to the sprite, move it, animate it or change its collision authority.
The other five plantings receive no emitter.

This location is ordinary visible scenery, independent of concealed specimens,
garden access, recipes, background choice, investigation and treatment progress.
The cue describes foliage movement only. It does not identify a plant, promise
an ingredient or invite a new interaction. No changes to accepted garden art
or another placement study are needed.

The remaining roadmap item is “Add positional ambience and meaningful off-screen
sound cues”. This subset supplies its first separately owned fixed foliage
emitter, dedicated passage and anonymous accessible history. It does not complete
the opening region's audio identity, wind simulation or wider emitter coverage.

## Missing dedicated source and rights

**No suitable dedicated source has been selected, acquired, listened to or
prepared in this planning slice.** B03 is accepted rain on dry leaves; W03 is
accepted walking on leaf litter; the wind original is an existing B04 component.
None is a new accepted isolated wind-driven leaf rustle. Keep their PCM, source
selection, rights evidence and derivative pipelines unchanged. Do not relabel,
extract, subtract layers from or repurpose them for this owner.

After the new grant, acquire at most one free original lossless WAV recording
of mild wind moving leaves, with an isolated usable passage of at least three
seconds. Require source-specific evidence of **CC0-1.0 or CC-BY-4.0** on the
creator's catalogue, with exact title, author, filename, source/download URL,
retrieval date, original file hash and captured catalogue/deed. A missing or
contradictory licence blocks acquisition/use. Preserve author-supplied notices,
attribution and an explicit derivative change notice in both the repository and
resource package. No purchase, new account, credentials, generative synthesis,
other licence family or second source is included in this grant.

Reject a passage with footsteps, voices, identifiable wildlife, rain impacts,
handling/bag noise, machinery, clipping or abrupt edits. Source catalogue wording
alone is insufficient: inspect decoded audio, listen to the selected passage and
record findings before runtime installation. If no qualifying passage is found,
report that source blocker; do not silently substitute an accepted old sound or
create a new source search slice around the same missing deliverable.

Prepare one derivative **F01.wav**, exactly 144,000 stereo PCM24 frames at 48 kHz
(3 seconds), with looping disabled. Allowed preparation is a recorded contiguous
trim, deterministic resampling, mono-to-dual-mono conversion if necessary,
100 ms linear fades at each end and one documented uniform gain. No layering,
denoising, pitch change, stereo widening or time stretching. Limit source peak
to at most -18 dBFS as a provisional ceiling, without boosting a quiet recording
to a target. Record frame windows, decoder/tool versions, channels, parameters,
original/decoded/derivative hashes, sample peak and measured loudness. This is
a new source and material permission, not an extension of the accepted audition.
Present the new passage with the runtime review; previous samples need no
listening or approval again. Ordinary-play/device loudness is not established by
the source ceiling or controlled output capture.

## Admission and fixed playback owner

`OpeningRegionFoliageAudioSource` binds the exact current garden source, sprite,
texture/import, projection/topology, navigation region/map/polygon and static
transforms, plus retained sickhouse/laundry boundaries. Replacement, hide/disable,
texture/geometry/transform drift, invalid map, nonfinite data or teardown cancels
only this owner. `IsVisibleInTree` allows an off-screen sprite; screen visibility
does not gate sound. The planted emitter need not be navigable: it is stationary
scenery, not a footstep or actor. Validate its exact retained sprite origin.

Require the listener's finite actual Seeker body position on the unchanged ready,
active navigation map, closest-point tolerance at most 1 projection-local pixel,
inside `[0,0,3200,1800]`, and outside inclusive sickhouse
`[2480,1310,2800,1502]` and laundry `[1420,120,1580,360]`. Read these boundaries
without installing an interior material or transmission model. Listener admission
and range confer presentation permission only; do not call gameplay hearing.

Reuse the exact current
`Main/OpeningRegionPositionalAudioController/SeekerListener`, current/top-level,
unrotated/unit-scale and within 1 pixel of the actual Seeker position. Create no
listener and never call `MakeCurrent`. Set only the new controller's physics
priority to 110, after the existing listener and priority-100 Healer audio owner.
Require finite world distance **strictly less than 192 pixels**; 192 and beyond
produce no audio, current caption or history. Recheck support, source identity,
listener and range throughout a passage; failure stops it without delayed replay.
No acoustic paths, walls, propagation delay, weather or canopy physics are modeled.

Own one `AudioStreamPlayer2D`, fixed at `(2232,1328)`, on **Ambience**:
`MaxDistance=192`, `Attenuation=1`, `PanningStrength=0.6`, `PitchScale=1`,
`MaxPolyphony=1`, `AreaMask=0`, unity player gain and no loop. Expected relative
native distance gain is `max(0,1-d/192)`. Apply no second distance multiplier,
manual pan, enclosure gain or corrective boost. Keep native camera-dependent pan.
Ambience/Master settings, Default/Quiet range, mono and existing foreground ducking
apply through existing buses exactly once. Take no ducking lease or music hold;
alter no retained bed/action/footstep owner. Missing/malformed F01 or Ambience bus
means silence with otherwise eligible caption/history; never fall back to Master.

## Presentation schedule and lifecycle

One deterministic presentation-only schedule starts at **12 active seconds**,
then every 12 active seconds; each passage lasts 3 seconds with at least 9 seconds
between starts. It uses no random, weather, action, world-time or disease event.
Advance only finite deltas `(0,0.1]`. A stall/invalid delta cancels and rebases
to 12 seconds; never catch up a missed passage. Consume scheduled starts even
out of range, unsupported, muted, captions-off or missing resources. Approaching,
unmuting or fixing a resource mid-passage never starts or resumes audio/text.
The next scheduled start must pass fresh admission. One player means no overlap,
pending event queue or arbitrary live injection API.

An Always-process watcher cancels the new passage/current caption on pause,
focus loss, dialogue/reading/customization/map/journal/settings overlay, text
entry, disabled/replaced prerequisites or listener teleport over 96 pixels.
Freeze the interval while these presentation blockers are active; resume with a
fresh 12-second wait, without affecting movement or world decisions. Preserve
already recorded history on pause/focus/overlay and freeze its limiter clock.
Mute/zero Ambience or Master immediately stops just the audio; admitted text
can continue. Successful `PresentationReset` clears this schedule, player,
caption, history and limiter; failed load changes nothing. Teardown clears all
new transient state. Never serialize any new counter or entry.

## Anonymous captions and independent history

Add one independently owned **tenth** line to `MaterialSoundCaptionPanel`:
**“Leaves rustling”**, for the admitted three-second passage. Capture material
and optional bearing once at start; keep them until cancellation or expiry.
Append one of the retained eight screen bearings only outside the inclusive
actual viewport, using `GlobalCanvasTransform × CanvasTransform` and the existing
pure positional caption helper's tie convention. No name, site, coordinates,
plant identity, actor, clue, weather state or world timestamp appears in text.

Caption preference affects the visible line only. Mute/resource failure preserves
eligible text and history. An out-of-range or blocked scheduled start supplies
neither. Clear only this line and expand only the panel's own bound if required
for ten wrapped lines; verify coexistence at 1280×720 and 1920×1080.

`FoliageCueHistory` owns a separate capacity-16 history with oldest-first eviction,
detached read-only entries, local sequence and generic caption only. Record the
first admitted start immediately, then at most once per 12 active seconds.
Discard rate-limited starts without queueing. Keep entries accessible with
captions off/muted/missing audio. Add a separate **Nearby foliage cues** section
to Audio → Recent sound cues, preserving reply and footstep history capacities,
order, clearing and rate limits. Preserve scrolling, selection, Back to Audio,
Escape, Tab/Shift-Tab and return focus. Opening that overlay stops presentation;
reading itself creates no cue. No durable journal or transcript integration.

## Exact implementation and finite verification

The JSON lists the permitted new files, narrow existing-file edits and retained
bindings. Add new pure Application admission/schedule/history contracts and tests;
new Game source/controller, ordinary smoke and stopped native output fixture;
source preparation/provenance, F01/import/notices/credits, packaging filters;
Main sibling wiring, tenth caption/history section and credits-panel inclusion.
Keep Channel last. Root quality adds current checks and pins unchanged Healer
validators/fixtures to this inspected baseline through its existing isolated
historical runner. Frozen validators, other controllers, source art, navigation,
movement, Domain, perception, provider and persistence code remain unchanged.

Completion after the one grant requires:

1. Source-specific rights capture and exact original/PCM/trim/derivative/import
   hashes; decoded duration/channel/peak checks, selected-passage listening and
   new-only source presentation. No claimed source acceptance before this exists.
2. Focused pure tests and live checks for 12-second scheduling, stall/reset,
   exclusive range edge, supported/exterior listener, exact fixed binding,
   replacement/drift, rate-limit/eviction and cross-owner cancellation isolation.
3. Seven identical actual-motion schedules: Default, Quiet, Master-muted,
   Ambience-muted, missing F01, captions-off and foliage-disabled baseline.
   Require complete save/transcript/movement and serialized protected
   inventory/discovery/sabotage/time/hearing/perception parity. First six consume
   equal eligible starts; disabled baseline has no new presentation.
4. Native Master cases for distance 0/48/96/144/191/192/193; left/right and mono;
   camera zoom/bearing; Default/Quiet and independent bus/master gains/mutes;
   existing foreground ducking/recovery; missing stream/bus; entering/leaving
   range mid-passage; pause/focus/overlay/load/failure/teleport; replaced or
   unsupported listeners/sources; overlap with retained owners. Compare measured
   relative gain to native linear falloff within 5% at 48/96/144 pixels, pan
   direction and mono channel parity; stopped/rejected cases require silence.
   Retain each named subcase, onset/tail/cancellation measurements and final
   peaks/loudness. Do not assert device listening or ordinary-play compliance.
5. Native ten-caption coexistence, independent history, muted/captions-off access,
   capacity/limiter and keyboard/focus at both viewports, with stopped diagnostic
   injection only. Ordinary play establishes the fixed garden cue behavior.
6. Mounted resource archive decodes F01 against its recorded PCM24-to-PCM16
   conversion; expected inventory is 37 streams, with all 36 old streams and
   23 individual/17 grouped old notices byte-identical. Add one new source notice
   group and one individual notice, giving 24 individual/18 groups. Verify credits
   inclusion on all retained resource export presets. This is resource packaging.
7. New rejection probes for source/proposal hash drift, forged binding, changed
   range/schedule/bus, protected-truth drift, missing measured evidence and prior
   caption/history mutation; `quality.py --self-test` and complete root
   `./quality.sh`, retaining all 72 current stages and explicit completion
   strings. Record implementation grant, review and closure; commit and push
   under standing publication authority.

Planning checks validate retained hashes, unchanged tracked inputs, passing
72-stage quality input hashes and continuation regression probes. Reuse the
existing 5,243-unit-test evidence: product and CI inputs are unchanged. Planning
does not claim source acquisition, native sound output or new runtime verification.

## New implementation decision

The approved active scope is this read-only proposal. A separate grant would
authorize **one new original/derivative F01, its exact garden-only source/emitter
permission, 192-pixel existing-listener playback, tenth anonymous caption and
separate transient history**, plus the named checks and dependent review. That
is the single missing implementation decision required by repository authority;
it is not another approval of a closed owner. Once granted, complete source
preparation, implementation, verification, review and publication in the same
session. Other foliage/wind emitters, world weather/hearing, interiors, full
acoustics, gameplay, art/gait, voice and release are outside this proposal.
