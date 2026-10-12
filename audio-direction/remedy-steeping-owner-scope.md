# First remedy-steeping sound owner and source proposal

Date: 2026-10-12 (Australia/Sydney).

The owner's “Read HANDOVER.md and follow the instructions - scope is approved”
authorizes the ledger-selected read-only steeping proposal. This document and
[its hash-bound contract](remedy-steeping-owner-scope.json) complete that slice.
The source/event/runtime decision below is a distinct implementation grant.
No audio was acquired, decoded, prepared or heard in this planning slice;
no product or CI input changed. Accepted heating and earlier owners stay closed.

## Exact existing owner and missing deliverable

`RepresentativeFirstPatientScenarioSource.Create()` authors
`cooling-moonwort-remedy` with grinding, heating, steeping and cooling, in order.
Steeping is exact `recipe.Plan.Steps[2]` with singleton
`IngredientPreparationKind.Steeping`. Preserve the 250 mg moonwort-leaf and
100 ml clean-water requirements and 100 ml cooled-moonwort-preparation output.

The owner is `Main/OpeningRegionRemedyPreparationSceneSource.Binding` at the
unchanged healer-garden station **(2300,1300)**. Retain the source Node2D's
identity rotation/scale and 72-pixel garden-anchor distance cap. The decorative
`PreparationTable` at local `(0,18)` with offset `(-48,-92)` does not relocate
the binding. Preserve exact source/binding/projection/garden-anchor/context/recipe
identities, all art, children, transforms, collision and navigation. Nothing
establishes a vessel, added water, stirring, heat source, boiling, infusion duration,
temperature, readiness or another ingredient operation.

`OpeningRegionRemedyPreparationInputCoordinator.TryProgress()` validates exact
current binding/context/recipe/ingredients/first-patient references, finite supplied
position and distance **at most 48 pixels**. Successful `TryAdvance(step)` returns
a genuinely new immutable activity, advancing steeping **index 2 → 3** immediately.
The seven mapped `P` inputs from fresh state are start, grind, heat, **steep**, cool,
finish and install batch. Treatment is separate. Direct preparation-controller
calls have no mapped coordinator notification.

The coordinator publishes grinding and heating, but no steeping notification.
The missing runtime deliverable is a once-only exact commit notification, distinct
short S01 passage and independent accessible feedback. A04/A05, H01, water contact
and all environmental sources stay unchanged; none substitutes for S01.
The named roadmap sound item remains partial after this owner.

## Source, rights and derivative requirements

After the separate grant, select one freely downloadable **lossless mono/stereo
recording** with source-specific **CC0-1.0 or CC-BY-4.0** evidence. No original is
nominated or acoustically qualified yet. Existing search/download permission does
not grant this runtime. Retain creator/title, exact catalogue/download URLs,
retrieval metadata, original bytes/hash, catalogue licence and deed captures.
Do not infer rights or lossless encoding from tags, extensions or a converted WAV.
No purchase/account action is needed. Heating's lossy-source exception and original
listening acceptance do not extend to steeping.

Require a clean contiguous three-second gentle liquid movement/settling texture,
as illustration of a committed operation. Reject prominent pouring/refilling,
stirring/tool impacts, rolling boil, pressure/whistle, fire, appliances, metal
handling, voices, music, wildlife, clipping, decoding faults and distracting room
sound. The cue must not assert those physical actions. Catalogue descriptions are
candidates, not listening evidence. Assess the exact window, record its offsets
and present final S01 inline in the implementation session. No qualifying source
or window means a source blocker before installation; do not relax the contract
or reuse accepted audio.

Both permitted deeds allow commercial copying and adaptation; CC BY requires
appropriate attribution, the licence link and an indication of changes. See the
[CC0 deed](https://creativecommons.org/publicdomain/zero/1.0/) and
[CC BY 4.0 deed](https://creativecommons.org/licenses/by/4.0/), checked 2026-10-12.
Retain supplied creator/attribution parties, title, source link, copyright/licence/
disclaimer and prior modification notices, plus our derivative changes, without
implying endorsement. Ship the applicable notices in runtime credits and exports.

Prepare exactly `game/assets/audio/remedy-steeping/S01.wav`: **three seconds,
48 kHz stereo PCM24, 144,000 frames**, nonlooping. Permit deterministic decode,
contiguous trim, required resampling, mono duplication, 100-ms linear endpoint
fades and one uniform attenuation-only gain. Reject other channel layouts.
No synthesis, layering, filtering, denoising, time/pitch changes or widening.
Provisional sample-peak ceiling is -24 dBFS; never boost quieter audio. Record
original/decode/window/derivative metadata/hashes, offsets, tools/versions,
resampling/gain/fades, import hash and measured peak/loudness. Import without
compression, normalization or looping; compare mounted PCM16 with documented
PCM24 conversion. Controlled output is not fresh-device/ordinary-play compliance.

## Exact commit admission and independent playback

Add `SteepingCompleted` only in the existing successful `TryAdvance` branch.
Require exact current recipe, retained step at index 2 and Steeping kind, identical
before/after recipe and plan, new activity and indices 2 → 3. A coordinator-owned
immutable transient `SteepingPreparationCommit` retains owner, binding, before/
after activities, step and actual Seeker position. Admit only its current retained
token identity, exact live source/coordinator/controller/Seeker/binding/context/
recipe and current after activity at notification. Actual finite Seeker body must
be within inclusive 48 pixels and supplied position within one pixel of it.
Presentation rejection never changes the gameplay result of `TryProgress`.

Consume token identity once before blocker/output checks. Null, copied, rebuilt,
forged, stale, foreign, wrong-step or duplicate tokens produce no sound/text/history.
Polling index/status, save reconstruction, approaching the table, H01 playback or
direct-controller progression emits nothing. Isolate subscriber exceptions after
commit; no rollback, second operation or blocked next input. Preserve grinding,
heating and its token lifecycle.

One sibling `OpeningRegionRemedySteepingAudioController` owns one nonspatial
`AudioStreamPlayer` on **Effects**, unity gain, PitchScale=1, MaxPolyphony=1 and no
loop. Start S01 once with no delay, queue, overlap within S01, replay or catch-up.
Existing Master/Effects, Default/Quiet and mono apply once. No bearing/listener
mutation, hearing/perception event, ducking lease or music hold. Missing/malformed
S01 or missing Effects is silent with otherwise eligible text/history retained;
never fall back to Master. Master/Effects mute/zero stops S01 immediately; admitted
text may finish. Repair/unmute never resumes.

An Always-process watcher cancels S01/caption on pause, focus loss, dialogue,
reading/customization/map/journal/settings overlay, text entry, invalid delta
outside (0,0.1], source replacement/disable/hide/transform drift, nonfinite positions
or Seeker discontinuity over 96 pixels. Blocked commits are consumed/discarded.
Successful `PresentationReset`/teardown clears only steeping player/caption/history/
sequence/token and unsubscribes; failed load changes nothing. Nothing is serialized.
Normal cool/finish/install inputs do not cancel admitted S01. Rapid heat→steep can
leave H01 playing: neither controller cancels/retriggers the other. Measure this
and A04/A05/bed coexistence; preserve gameplay speed and accepted H01 bytes/gain.

## Independent accessible feedback

Own a **fourteenth** line, **“Steeping preparation”**, for three active seconds.
No actor/site/ingredient/remedy name, coordinates, bearing, timestamp, temperature,
infusion duration, readiness or treatment success. Ordinary preparation status stays
authoritative and available. Caption preference hides only the new line; mute/missing
audio retains eligible text/history. Preserve thirteen prior lines/timers/wording;
adjust only caption-panel bounds for simultaneous wrapping, including heating.

`RemedySteepingCueHistory` owns a detached read-only oldest-first capacity-16 list
with local sequence and exact phrase only. Each unique eligible commit inserts
immediately; no time limiter drops another legitimate batch. Reject duplicates
before insertion. Append **Remedy steeping cues** in Audio → Recent sound cues.
Preserve prior ordering/capacities/limiters/scrolling/keyboard/Escape/Back to Audio/
origin focus. Reading emits nothing. Blockers retain earlier entries; successful
reset/teardown clears steeping only. Capacity fixtures run in stopped diagnostics
without inventing ordinary repeated-batch gameplay.

## Finite files and verification

The JSON names new paths and narrowly permitted existing edits. No Domain,
preparation-controller/recipe/world/art/navigation/save-schema/provider/hearing/
perception change is required. Preserve local `game/project.godot`, frozen
validators and accepted hashes. After one grant, complete these dependent steps
and standing Git publication in the same session:

1. Source-specific rights/acquisition/window listening and reproducible S01/import
   measurements; present exact derivative without repeating earlier source reviews.
2. Pure/live exact 2→3/reference/token admission and rejection, 48/49 pixels, forged
   positions, wrong/copied/stale/duplicate tokens, direct-controller advance, failed
   transitions, subscriber exceptions, blockers, expiry, mute, missing stream/bus,
   cancellation, successful/failed load, no replay and rapid later progress.
3. Seven identical actual movement and seven-press preparation/treatment schedules:
   Default, Quiet, Master-muted, Effects-muted, missing S01, captions-off and
   steeping-disabled baseline. Compare full preparation/ingredients/batches/outcome,
   disease/care/administration, saves/transcripts/movement and protected inventory/
   discovery/sabotage/time/hearing/perception. First six have one eligible steeping
   event; baseline has equal gameplay. Grinding/heating/treatment counts stay equal.
4. Native measured Master unity/half/zero/mute Master/Effects, Ambience independence,
   Default/Quiet/mono, onset/tail, missing stream/bus, pause/focus/overlays/stalls/
   teleport/source drift, load/reset and next inputs. Gain factors within 5%, mono
   parity, cancellation silence, measured isolated/mixed peaks/loudness including
   rapid H01+S01 and A04/A05/beds; no lease or music hold.
5. Native fourteen-caption/history views at 1280×720 and 1920×1080: coexistence,
   wrapping/visibility, capacity/sequence/duplicates, scrolling, keyboard/return
   focus, mute/captions-off/missing-audio access and isolated reset. Inject only
   in stopped diagnostics; retain genuine mapped commit evidence separately.
6. Mount all three presets, preserving **115** exact prior entries. Add stream/import
   and three notices: **41 streams, 38 notices, 22 groups, 120 entries**. Verify exact
   S01 PCM16 and exclude original/review audio. No executable release is established.
7. Rejection probes for source/rights/proposal/PCM/event/bus/duration/phrase/truth/
   protected owners. Root `./quality.sh` retains all **126** current stages, adding
   necessary steeping stages and pinning unchanged heating fixtures at `b7912dd4`
   using the existing historical runner as needed. Preserve earlier pins. Run
   `python3 quality.py --self-test` after runner changes; sync/check continuation,
   maintain remaining-roadmap state and publish only reviewed paths.

Planning verifies unchanged tracked product/CI and accepted audio bytes plus retained
final quality logs. Reuse passing 126-stage/5,412-test evidence for this prose-only
slice; no new native/output/listening or full-quality run is claimed.

## Concrete separate implementation decision

**Authorize selection/acquisition/qualification of one new free lossless CC0-1.0 or
CC-BY-4.0 gentle-liquid source meeting this contract; prepare/present three-second
S01; add exact third-step 2→3 committed steeping feedback at unchanged (2300,1300)
on Effects, the fourteenth “Steeping preparation” caption and separate capacity-16
history; complete named files and finite source, parity, native/UI, packaging,
quality and publication checks. No qualifying source/window means a recorded blocker
before installation without relaxing the contract.**

This grant is not yet recorded. It adds no physical station/vessel/action, infusion
duration, recipe/progression/save/hearing/perception change, cooling feedback,
wider emitter, voice or release. Dependent steps form one finite implementation slice.
