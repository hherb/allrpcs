# First fixed woodland rain source and emitter proposal

Date: 2026-10-12 (Australia/Sydney).

The owner's “Read HANDOVER.md and follow the instructions - scope is approved”
authorizes the ledger-selected read-only rain planning slice. This proposal
completes it. [The machine-readable contract](rain-owner-scope.json) binds the
source, baseline, permitted implementation files and finite checks. One new
source-use/runtime decision remains; no new passage has been prepared or heard.

## Retained owner and missing deliverable

Propose one dedicated four-second rain passage at **(1000,350)**, through
`Main/OpeningRegionWoodlandClearingSceneSource.Binding.WoodlandClearingAnchor`.
The existing `woodland-clearing` anchor belongs to the retained projection;
the source Node2D has identity transform and clearing bounds [600,200,1248,450].
Read that presentation coordinate without adding a marker or changing any
child, collision, transition, artwork, route, opening or navigation polygon.
Preserve the ready layout's 96-pixel openings, eight-pixel walls, four route
connections and exact derived boundary segments. Validate live object identity
and unchanged physical contents, not only matching coordinate values.

This owner is the exposed clearing. The concealed specimen is not a source:
its identity, discovery, visibility, access, inventory, investigation and other
gameplay state never select or gate this sound. Preserve its inert marker.
No weather simulation, rainfall fact, wetness, physical effect, hearing or
perception event is added. The anonymous cue reports local audio presentation.
This is one missing dedicated positional rain deliverable under “Add positional
ambience and meaningful off-screen sound cues”; that roadmap item remains partial.
Accepted B03, F01, G01 and every earlier owner stay closed and unchanged.

## Source lineage and rights

Nominate the **untouched retained original**
`audio-direction/material-audition/originals/rain/amb_rain2.flac`, acquired
2026-10-07: **AMB Rain Loop 2 by Kresiek The Furry**. Its SHA256 is
`c33d833842c88e9559882b35f6f149c3a96bbf236eb34491e36ea4cae8879985`.
Retained metadata records 35,185,664 frames, 48 kHz stereo 16-bit FLAC,
733.034667 seconds and 75,502,454 bytes. This slice verifies retained bytes and
metadata records; it does not decode or acoustically qualify the original.

The [current creator catalogue](https://opengameart.org/content/amb-rain-loop-2),
checked on 2026-10-12, lists that author, a lossless FLAC download and CC0,
and describes GoPro raw-audio recording. The retained source-specific catalogue
capture, provenance and [CC0 deed](https://creativecommons.org/publicdomain/zero/1.0/)
are hash-bound in the JSON. CC0 permits modification and commercial distribution;
retain creator/source/licence and an exact derivative change notice without
implying endorsement. The new grant must copy rights evidence and source metadata
to new rain records, leaving every prior catalogue, original, notice and derivative
unchanged. No purchase, account, second source or download is needed.

B03 uses a different leaf-rain source; B01/B02/B04 include the nominated rain
original in accepted composites. Their acceptance does not qualify R01. Never
extract from, subtract layers from, relabel, filter or replace those composites.
A separately prepared passage from the original is the proposed new permission.

After the grant, decode and listen to select one contiguous clean four-second
window. Require recognizable exterior rain without voices, wildlife, traffic,
thunder, footsteps, handling, clipping, abrupt edits or prominent roof/metal
impacts that contradict the exposed clearing. Record exact original frame
offsets and new-passage listening assessment; present R01 for its dependent
selection in the same session. No qualifying window means a source blocker,
without substitution or runtime installation. Catalogue tags do not establish
acoustic suitability.

Prepare exactly **R01.wav**, 192,000 stereo PCM24 frames at 48 kHz, nonlooping.
Permit only contiguous trim, deterministic decoding/sample-format conversion,
100-ms linear endpoint fades and one attenuation-only uniform gain. The source
already has the target rate and channel layout; no resampling or channel remix
is needed. No filtering, denoising, layering, synthesis, pitch/time change or
widening. Provisional sample-peak ceiling: -24 dBFS; never boost a quieter source.
Record tools/versions, original/decoded/window/derivative/import hashes, frame
counts, exact gain/fades, peak and loudness. PCM24 storage adds no source precision.
Controlled output evidence does not establish fresh-device or ordinary-play
loudness compliance.

## Admission and playback

A new sibling source retains the exact live clearing source, binding, layout,
anchor, projection/topology, child order, transforms and geometry after readiness.
Replacement, hide/disable, drift, teardown or nonfinite input cancels only rain.
Off-screen visibility is valid: use `IsVisibleInTree`, not viewport containment.
Do not tie admission to decorative children being on screen. Preserve every
retained child's authored hidden/visible state and the source's ready contents.

Reuse the exact current top-level, unrotated/unit-scale
`Main/OpeningRegionPositionalAudioController/SeekerListener`, within one pixel
of the actual finite Seeker body. Never create/move it or call MakeCurrent.
Require unchanged ready active navigation/map/polygon and nearest support within
one projection-local pixel, world bounds [0,0,3200,1800], and exclusion of the
inclusive sickhouse [2480,1310,2800,1502] and laundry [1420,120,1580,360] rectangles.
Retain their exact room owners without adding transmission or room acoustics.
Use new physics priority **130**, following wind (120).

Admit distance **strictly less than 256 pixels**, throughout each passage.
At 256 or beyond admit no audio, caption or history. Loss of support/binding/
listener/range cancels the current passage without replay. One new
`AudioStreamPlayer2D` remains at (1000,350), on **Ambience**, MaxDistance=256,
Attenuation=1, PanningStrength=0.6, PitchScale=1, MaxPolyphony=1, AreaMask=0,
unity gain and no loop. Native distance factor max(0,1-distance/256) applies
once; no manual pan, second attenuation, boost, enclosure multiplier, ducking
lease or music hold. Existing Master/Ambience, mono, Default/Quiet and foreground
ducking apply once. Preserve all other bed/emitter schedules and volumes.
Missing/malformed R01 or absent Ambience means silent audio while otherwise
admitted caption/history remain; there is no Master fallback.

## Schedule, reset and accessible cues

Start at **20 active seconds**, then every 20 active seconds; four-second passage,
at least 16 seconds of gap. Use a deterministic local presentation clock,
independent of weather, actions, random state or world time. Advance finite
deltas (0,0.1] only; stall/invalid delta cancels and rebases to a fresh wait.
Consume scheduled starts when unsupported, out of range, muted, missing audio
or captions-off. Mid-passage approach/unmute/resource repair starts nothing.
No overlap, catch-up queue or ordinary injection API exists.

An Always-process watcher cancels rain on pause, focus loss, dialogue,
reading/customization/map/journal/settings overlays, text entry, disabled or
replaced prerequisites and listener discontinuity over 96 pixels. Blockers
retain history, freeze its limiter and resume after a fresh 20-second wait.
Mute/zero Master or Ambience immediately stops rain audio; text may continue.
Unmute never resumes. Successful PresentationReset or teardown clears only
the new player/schedule/caption/history/sequence/limiter. Failed load changes
nothing. None of this state is serialized or written to journal/transcript.

Own a **twelfth** independent caption line, **“Rain falling”**, lasting four
seconds. Capture optional eight-way bearing once at admission, with the retained
pure tie rule, transformed screen point and inclusive viewport edges. No site,
actor, coordinates, clue, weather strength/direction or world timestamp is shown.
Caption preference hides only the new line; eligible history remains available.
Preserve all eleven prior lines/timers and expand only the panel's own bounds
if needed for coexistence and wrapping.

`RainCueHistory` owns a separate capacity-16 oldest-first history with detached
read-only local sequence/phrase/bearing entries. First admitted entry is immediate;
later entries are limited to once per 20 active seconds, discarding rejected
starts without queueing. Add **Nearby rain cues** to Audio → Recent sound cues.
Preserve all prior sections, capacities, order, limiters, clearing behavior,
scrolling, keyboard navigation, Escape, Back to Audio and return focus. Reading
this overlay admits no cue. Mute, missing audio and captions-off retain access.

## Finite implementation checks and permitted changes

The JSON lists every new path and narrowly allowed existing edit. Add pure
Application spatial/schedule/history helpers and tests; Game source/controller,
ordinary smoke and stopped diagnostic fixture; source preparation/rights/notices;
Main sibling wiring before Channel; one caption/history section; appended credits
and export filters. Keep all other runtime, gameplay, movement, persistence,
navigation, accepted art/audio and frozen validators unchanged. Preserve the
recorded local `game/project.godot` rewrite.

After the single new grant, complete these dependent steps in the same session:

1. Qualify the original/window and R01; reproduce exact PCM24/import/notices,
   measure source peak/loudness and record new-only listening/selection.
2. Focused pure and live tests for exact clearing/anchor/layout/boundary/map/
   listener identity, drift/replacement/hide/disable/nonfinite inputs, exclusive
   range, 20-second starts, four-second expiry, blockers/reset/no catch-up,
   capacity/limiter and cross-owner cancellation isolation.
3. Seven identical actual-motion schedules: Default, Quiet, Master-muted,
   Ambience-muted, missing R01, captions-off and rain-disabled baseline. Compare
   complete saves/transcripts/movement plus protected inventory/discovery/
   sabotage/time/hearing/perception truth; equal eligible starts in the first
   six, no rain presentation in the disabled baseline. Keep prior fixtures.
4. Native Master measurements for 0/64/128/192/255/256/257 distance, left/right,
   mono, zoom/bearings, independent controls, Default/Quiet, foreground ducking/
   recovery, missing stream/bus, range entry/exit, pause/focus/overlays/reset/
   failed load/teleport and replaced/unsupported owners/listeners. Include
   coexistence with B03/other retained beds. Relative gains at 64/128/192 must
   match native distance factors within 5%; verify pan direction/mono parity,
   onset/tail/cancellation silence and final peaks/loudness.
5. Native twelve-caption coexistence, eight bearings/inclusive edges and
   independent history at 1280×720 and 1920×1080: wrapping, scrolling, capacity,
   limiter, keyboard/return focus, muted/captions-off/missing-audio access and
   cross-owner clearing. Inject only in stopped diagnostics; ordinary motion
   must establish genuine admission.
6. Mount all retained resource export presets. Preserve **all 105 current audio
   archive entries** byte for byte; add one stream/import pair plus three rain
   notices for **39 streams, 32 notices in 20 groups, 110 audio entries**. Compare
   imported R01 with recorded PCM24→PCM16 conversion; retain all 29 existing
   notices. Original/decode/review audio stays outside runtime exports. This
   proves resource packaging, without claiming an executable release.
7. Reject source/proposal/PCM/binding drift, changed range/schedule/bus, protected
   truth drift, missing measurements and prior caption/history mutation. Run
   focused validation, `python3 quality.py --self-test` and root `./quality.sh`,
   retaining all 95 current stages and their completion markers. Freeze current
   wind validators/fixtures at this proposal's baseline through the existing
   isolated historical runner; preserve older hand-tool/Healer/foliage pins.
8. Record review/verification and closure; sync/check the ledger and handover,
   retain remaining roadmap work, then commit/push reviewed intended paths under
   standing publication authority. No unchanged accepted-owner approval repeats.

## One new implementation decision

Authorize qualification/preparation of **R01 from the exact retained FLAC**, one
fixed **(1000,350) clearing-only 256-pixel Ambience emitter**, four seconds every
20 active seconds, the **twelfth “Rain falling” caption** and independent
capacity-16 history, plus the named checks, new-passage review and standing Git
publication. This grant includes only the files and behavior above. If the
candidate cannot qualify, report that blocker. Additional emitters, sources,
weather, physics, gameplay, voice and releases remain separate scopes.

## Planning verification

The JSON binds this proposal, current source/rights/owner inputs, all current
audio inputs and a complete protected tracked-input snapshot. Planning checks
verify unchanged baseline blobs and editor bytes, existing wind quality evidence,
the current 105-entry archive and retained ledger closures. Continuation checks
and all 11 regression probes pass. Reuse the verified 95-stage/5,335-test wind
product evidence because no product or CI input changed. No new native output,
runtime, decoded passage or listening evidence is claimed by this proposal.
