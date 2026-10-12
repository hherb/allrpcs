# Cooling-step confirmation proposal

Date: 2026-10-12 (Australia/Sydney).

The owner's “Read HANDOVER.md and follow the instructions - scope is approved”
authorizes the ledger-selected read-only cooling proposal. This document and
[its hash-bound contract](remedy-cooling-owner-scope.json) complete that slice.
No audio was acquired, decoded, prepared or heard, and no product or CI input
changed. The separate source/event/runtime decision below remains ungranted.

## Existing operation and useful feedback

`RepresentativeFirstPatientScenarioSource.Create()` authors the exact
`cooling-moonwort-remedy`: grinding, heating, steeping, cooling. Cooling is retained
`recipe.Plan.Steps[3]`, singleton `IngredientPreparationKind.Cooling`. Keep the
250 mg moonwort-leaf and 100 ml clean-water requirements and 100 ml
cooled-moonwort-preparation output.

The owner is `Main/OpeningRegionRemedyPreparationSceneSource.Binding` at unchanged
**(2300,1300)**, with identity rotation/scale and the existing 72-pixel garden-anchor
cap. The decorative table at local `(0,18)`, offset `(-48,-92)`, is not the input
anchor. Preserve exact source/binding/projection/garden/context/recipe identities,
children, art, transforms, collision and navigation.

`OpeningRegionRemedyPreparationInputCoordinator.TryProgress()` checks current exact
binding/context/recipe/ingredients/first-patient references and a finite supplied
position within inclusive **48 pixels**. Successful `TryAdvance(step)` installs
a new immutable activity immediately: cooling is exact **index 3 → 4**. It does
not measure elapsed time, temperature or readiness. From fresh state the seven
mapped `P` inputs are start, grind, heat, steep, **cool**, finish, install batch.
Cooling leaves `Outcome` null. The next input calls `TryComplete()`; the following
input installs the batch. Treatment remains separate.

Existing visible status after cooling is “Preparation: step 4/4 complete. Press P
to finish.” It remains authoritative. There is no cooling notification or cue.
Passive heat loss has no justified material sound here. Literal pouring, stirring,
steam, ice, vessel handling or a timer would invent an action or process.

**Recommendation:** one quiet, unpitched, soft tactile confirmation of the player's
committed input, explicitly interface feedback on Effects. Its meaning is only
“the cooling step input committed”, without asserting physical cooling or treatment
success. The accepted policy already assigns general interface feedback to Effects
and permits brief player-action confirmation. This applies that policy without
reopening it. Silence is reasonable compositionally, but supplies no additional
audible confirmation of this mapped input. A new short acknowledgement offers the
same redundant access as earlier preparation inputs; it must remain optional through
existing audio controls, with ordinary status and an independent text equivalent.
No pitched success chime, motif, alarm, countdown or ambience loop is proposed.

## New source and derivative boundary

After the separate grant, select one free, downloadable **lossless mono/stereo**
recording of a soft, dry, unpitched contact suitable as neutral interface feedback.
No original is nominated or qualified. Require source-specific **CC0-1.0 or
CC-BY-4.0** evidence, creator/title, exact catalogue/download URLs, retrieval metadata,
original bytes/hash, and retained catalogue/deed captures. No purchase or account
action. A WAV conversion of a lossy original does not satisfy the requirement.
Heating's original-specific exception and all earlier listening acceptances remain
limited to their exact sources. Do not reuse A03, A04/A05, H01, S01 or ambient PCM.

Require one clean contiguous **0.75-second** window with a restrained contact and
natural decay. Reject recognizable pouring/splashing/stirring, boiling/steam/fire,
ice/temperature-machine effects, vessel or metal handling, hard tool strikes,
keyboard/appliance clicks, sustained hiss, tonal/melodic success signals, voices,
music, wildlife, clipping, decode faults and distracting room sound. The final
owner listening must judge both the isolated derivative and its meaning beside
H01/S01 at the unchanged station. Catalogue words are not acoustic evidence.
Present the exact derivative inline before runtime installation. No qualifying
source/window or owner listening acceptance means a recorded blocker; do not
relax the contract or silently substitute another accepted source.

CC0 permits commercial copying/adaptation. CC BY 4.0 permits those uses with
appropriate attribution, a licence link and changes indicated. Preserve supplied
creator/attribution parties, title, source, copyright/licence/disclaimer and prior
change notices; add our edits without implying endorsement. The official
[CC0 deed](https://creativecommons.org/publicdomain/zero/1.0/) and
[CC BY 4.0 deed](https://creativecommons.org/licenses/by/4.0/) were checked on
2026-10-12. Retain applicable source-specific evidence and ship notices in credits
and every export; the generic deed alone does not qualify an original.

Prepare exactly `game/assets/audio/remedy-cooling/C01.wav`: **48 kHz stereo PCM24,
36,000 frames, 0.75 seconds**, nonlooping. Allowed edits are deterministic decode,
contiguous trim, required resampling, mono duplication, 10-ms linear onset and
50-ms linear ending fades, and one uniform attenuation-only gain. Reject other
channel layouts, synthesis, layering, filtering, denoising, pitch/time edits and
widening. Provisional sample-peak ceiling: **-30 dBFS**, without boosting quieter
sources. Record source/decode/window/derivative/import hashes, offsets, tools,
versions and measured peak/loudness. Import without normalization, compression
or looping; compare mounted PCM16 with documented PCM24 conversion. Quietness and
audibility are listening decisions; controlled measurements prove no device standard.

## Once-only commit presentation

Add `CoolingCompleted` only after the existing successful `TryAdvance` branch.
Require retained step 3/Cooling, exact current recipe and before/after recipe/plan
identities, a new activity, and indices 3 → 4. A coordinator-owned immutable
`CoolingPreparationCommit` retains owner, binding, before/after, step and actual
Seeker position. Admit only the current owned token identity and current after
activity at notification, with exact live source/coordinator/controller/Seeker/
binding/context/recipe. Require finite actual body within inclusive 48 pixels and
supplied position within one pixel of it. Presentation rejection must not change
the successful gameplay result. Consume token identity once before output/blocker
checks; reject null, copied, rebuilt, forged, foreign, stale, wrong-step and duplicate
tokens. Isolate subscriber exceptions after commit without rollback or another step.

Polling index 4, complete status, direct-controller calls, save reconstruction,
approach, S01 tail, finish/install/treatment inputs and diagnostic injection produce
no ordinary event. Preserve all earlier notification/token lifecycles. This is a
presentation observer, not another preparation controller.

One sibling `OpeningRegionRemedyCoolingAudioController` owns one nonspatial
`AudioStreamPlayer`: **Effects**, unity gain, PitchScale=1, MaxPolyphony=1. Play C01
once immediately, without delay, looping, queue, overlap within C01, replay or
catch-up. Existing Master/Effects, Default/Quiet and mono apply once. No listener,
bearing, hearing/perception event, lease, ducking or music hold. Missing/malformed
C01 or missing Effects is silent, retaining otherwise eligible text/history, with
no Master fallback. Master/Effects mute or zero stops C01 immediately; admitted
text may finish. Repair/unmute does not resume.

An Always-process watcher cancels only cooling playback/caption on pause, focus
loss, dialogue, reading/customization/map/journal/settings overlay, text entry,
delta outside finite `(0,0.1]`, nonfinite positions, Seeker displacement over
96 pixels, or source replacement/disable/hide/transform/child drift. Blocked tokens
are consumed/discarded. Successful `PresentationReset` or teardown clears only
cooling player/caption/history/sequence/token and unsubscribes; failed load changes
nothing. Nothing is saved. Subsequent normal finish/install inputs do not cancel
an admitted cue or claim batch readiness. Rapid heat→steep→cool may retain all
three cues; measure their mix without changing accepted PCM, gain or input speed.

## Independent accessible equivalent

Own a **fifteenth** line, **“Cooling step committed”**, for **two active seconds**.
It reports the already committed input, with no actual temperature, duration,
readiness, actor/site/ingredient/remedy name, coordinate, bearing, timestamp or
treatment success. Existing status stays available. Caption preference hides only
this line; mute/missing audio retains eligible text/history. Preserve all fourteen
earlier lines/timers/wording. Adjust only caption-panel bounds for simultaneous
wrapping and visibility.

`RemedyCoolingCueHistory` owns a detached read-only oldest-first capacity-16 list
of local sequence and exact phrase. Insert each unique eligible commit immediately;
no time limiter drops a legitimate batch. Append **Remedy cooling cues** to Audio →
Recent sound cues. Preserve earlier section order, limits, scrolling, keyboard,
Escape/Back to Audio/origin focus. Reading emits nothing. Cancellation retains
prior entries; successful reset clears cooling only. Capacity injection belongs
only in stopped diagnostics, with genuine mapped input evidence kept separately.

## Finite implementation and checks

The JSON names all new paths and narrow existing edits. After one distinct grant,
finish source qualification/listening, implementation, checks, review, closure and
standing Git publication together. No Domain, preparation controller, recipe,
station, art, navigation, save-schema, provider or hearing/perception edit is needed.

1. Source-specific rights and lossless original; reproducible window/derivative/
   import measurements; owner acceptance of exact C01 before installation.
2. Exact 3→4/current-reference/token admission and rejection, 48/49 pixels,
   supplied-position forgery, duplicates, stale/wrong/copied tokens, direct advance,
   failed transition, subscriber exception, blockers, expiry, missing stream/bus,
   mute, reset/load and subsequent finish/install. Preserve earlier event counts.
3. Seven genuine movement and seven-press preparation/treatment schedules: Default,
   Quiet, Master-muted, Effects-muted, missing C01, captions-off and cooling-disabled
   baseline. Compare full preparation, ingredients/batches/outcome, disease/care/
   administration, saves/transcripts/movement and protected truth state. First six
   have one eligible cooling notification; baseline has identical gameplay.
4. Measure native Master unity/half/zero/mute and Effects gain/mute, Ambience
   independence, Default/Quiet/mono, onset/tail, blockers and cancellation silence,
   load/reset and no replay. Gain factors within 5%, mono parity and measured
   isolated/mixed peaks/loudness, including rapid H01+S01+C01, A04/A05 and beds.
5. Native fifteen-caption/history views at 1280×720 and 1920×1080: wrapping,
   coexistence, capacity/sequence/duplicates, scrolling, keyboard/return focus,
   muted/captions-off/missing-audio access and isolated reset.
6. Mount three presets. Preserve all **120** prior entries; add stream/import and
   three notices: **42 streams, 39 notices, 23 groups, 125 entries**. Verify exact
   mounted C01 PCM16 and exclude original/review audio. No executable release claim.
7. Source/rights/PCM/event/bus/duration/phrase/protected-owner rejection probes;
   focused tests, root `./quality.sh`, and `python3 quality.py --self-test` if the
   runner changes. Retain all **140** current stages in order and previous pins.
   Pin frozen steeping checks at baseline `78369110` through the existing historical
   runner where necessary; preserve frozen validator bytes. Sync/check continuation
   and publish reviewed paths, excluding editor state and `.DS_Store`.

Planning verified every protected baseline Git blob and all 140 retained final
quality logs, including their hashes and required completion strings. The JSON
binds the existing 5,442-test evidence and 120-entry inventory. These are reused
passing product results; there is no fresh product/native/listening/full-quality
run for this prose-only slice.

## Concrete separate decision

**Authorize one new free lossless CC0-1.0 or CC-BY-4.0 soft, dry, unpitched source
under this contract; prepare/present 0.75-second C01 and obtain exact owner listening
acceptance before installation; add only the existing fourth-step 3→4 cooling input
confirmation at unchanged (2300,1300), on Effects, with the fifteenth “Cooling step
committed” line and independent capacity-16 history; complete named source, runtime,
parity, native/UI, packaging, quality, review and publication checks.**

This distinct grant is not recorded. It introduces no physical cooling mechanism,
station/vessel/action, elapsed time, temperature, readiness, recipe/progression/save/
hearing/perception change, another operation, wider emitter, voice or release.
Failure to qualify/accept C01 remains a source blocker before installation.
