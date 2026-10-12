# First remedy-heating sound owner and source proposal

Date: 2026-10-12 (Australia/Sydney).

The owner's “Read HANDOVER.md and follow the instructions - scope is approved”
authorizes the ledger-selected **read-only heating proposal**. This document and
[its hash-bound contract](remedy-heating-owner-scope.json) complete that slice.
The implementation decision at the end is new; this agent has not downloaded,
decoded, prepared, heard or installed source audio, and no product or CI input
changed. The owner subsequently listened to the nominated original and accepted
it with **“I just listened to it - sounds perfect!”**; carry that acceptance forward.

## Exact existing owner and missing deliverables

`RepresentativeFirstPatientScenarioSource.Create()` authors
`cooling-moonwort-remedy` with four exact steps: grinding, heating, steeping,
cooling. Heating is `recipe.Plan.Steps[1]`, whose kind is the retained singleton
`IngredientPreparationKind.Heating`. Inputs are 250 mg moonwort leaf and 100 ml
clean water; the output is 100 ml cooled moonwort preparation. Preserve them.

The retained owner is
`Main/OpeningRegionRemedyPreparationSceneSource.Binding`, at station **(2300,1300)**
in the healer garden. Its source Node2D is at that coordinate with unchanged
identity rotation/scale. Its garden-anchor distance cap is 72 pixels. The
`PreparationTable` sprite is decorative: its local `(0,18)` position and image
offset do not redefine the binding's station. Retain the scene, binding,
projection, garden anchor, treatment context, recipe, table, all planting,
child order, transforms, collision and navigation unchanged. Nothing establishes
a physical stove, burner, flame, heated vessel or boiling water in this world.

`OpeningRegionRemedyPreparationInputCoordinator.TryProgress()` first validates
exact current binding/context/recipe/ingredient and first-patient state references,
finite position, and distance **at most 48 pixels** from the station. It calls
`FirstPatientPreparationController.TryAdvance(step)` on the exact next step.
The domain transition returns a new immutable activity and advances exactly once.
Heating therefore commits **index 1 → 2**, immediately, without duration or
temperature. Starting, grinding, steeping, cooling, completing and installing the
batch are separate inputs. From fresh state the ordinary mapped `P` sequence is
start, grind, **heat**, steep, cool, finish, install: seven presses.

The coordinator currently emits only `GrindingCompleted`. The missing deliverables
are a once-only presentation notification for that successful heating commit and
a new short heating texture with independent accessible feedback. A04 grinding,
A05 treatment, their shared action line and every earlier audio owner stay closed.
The named roadmap item “Add footsteps by terrain, water, wind, foliage, interiors,
tools, and remedy preparation” remains partial after this one heating owner.

## One nominated source and rights boundary

Nominate **TinyWorlds, boiling water loops**, specifically
[`cooking_with_cover_01.ogg`](https://opengameart.org/sites/default/files/cooking_with_cover_01.ogg)
from the [creator's catalogue](https://opengameart.org/content/boiling-water-loops),
checked on 2026-10-12. The page lists CC0 and describes a covered/uncovered cooking
recording. Use only the covered file; the uncovered file is not a fallback.
The [CC0 deed](https://creativecommons.org/publicdomain/zero/1.0/) permits copying,
modification and commercial distribution. Retain creator, title, exact source URL,
licence/deed and derivative change notice without implying endorsement.

**The catalogue supplies Ogg without documenting a lossless original master.**
The codec and whether this particular Ogg encoding is lossy remain unverified
until acquisition. The grant explicitly includes use of the supplied file even
if its encoding is lossy. New decoded/working/delivery WAVs preserve the decode;
they cannot recover any information discarded by a lossy source encoding.
Catalogue format is observed, but codec, sample rate, channels, frame count,
duration, exact source bytes and acoustic suitability are unverified until
acquisition. Do not infer these from filename or download size. If the supplied file is lossy, its use is a narrow
exception to the audio direction's preference for lossless originals, not a
change to accepted sources or a claim that Ogg itself establishes lossless quality. The owner's
listening acceptance covers the nominated covered-pot original, not a claimed
lossless recording or an already prepared frame-exact H01.

After the grant, acquire that exact public file without purchase/account actions;
retain original bytes, retrieval metadata, source-specific catalogue/deed captures
and hashes in a new heating-source package. Require a valid decodable recording
with a contiguous three-second clean window. Listen for a restrained warm liquid
texture suitable as a brief illustration of the already committed heating step.
Reject prominent rolling-boil/pressure/whistle cues, appliance motor/switch clicks,
fire, metal handling, voices, music, wildlife, clipping, decoding faults, distracting
room sound or compression artifacts. Catalogue cooking tags alone do not qualify
it. Carry the owner's original listening acceptance forward without another
original-source review. Record the new window's assessment and exact offsets and
present the exact H01 derivative in the same implementation session. No clean
window or unsuitable decoded quality means a source blocker; no substitution,
filtering repair or runtime installation is authorized by this proposal.

Prepare exactly **H01.wav**, a three-second nonlooping stereo PCM24 file at 48 kHz
(144,000 frames). Permit deterministic decode, contiguous trim, resampling if
needed, mono duplication if needed, 100-ms linear endpoint fades and one uniform
attenuation-only gain. Reject other channel layouts rather than inventing a remix.
No synthesis, layering, denoising, filtering, pitch/time change or widening.
Provisional sample-peak ceiling is -24 dBFS; never boost a quieter source. Record
tools/versions, actual original/decode/window metadata and hashes, offset convention,
conversion settings, gain/fades, derivative/import hashes, peak and loudness.
Import without compression, normalization or looping and verify mounted PCM16
against the documented PCM24 conversion. These numerical targets are provisional;
controlled measurements do not establish ordinary-play or fresh-device compliance.

## Commit admission, playback and cancellation

Add a separate presentation-only `HeatingCompleted` notification inside the
existing successful `TryAdvance` branch. Capture exact before/after activities,
step and current binding; require the exact retained recipe and step, before index
1 and after index 2, unchanged plan and a genuinely new activity. Do not infer an
event by polling `NextStepIndex == 2`, observing a status label, hearing A04,
approaching the table or restoring a save. Calls directly to the preparation
controller do not themselves publish an ordinary mapped-input notification.

The coordinator owns an immutable transient `HeatingPreparationCommit` token,
retaining exact coordinator/binding/context/recipe/step/before/after references.
Only it can construct and admit its current token. The receiver requires exact
live source/coordinator/controller/Seeker references and the same current activity,
and consumes token identity once, even if muted, missing audio, blocked or cancelled.
At notification require the actual finite Seeker body within inclusive 48 pixels,
consistent with the supplied input position within one pixel; a forged position
must not gain presentation. Token validation never changes `TryProgress`'s gameplay
result. Null, copied, rebuilt, stale, wrong-owner/wrong-step/wrong-recipe or repeated
notifications produce no sound, caption or history. Isolate subscriber exceptions
after the successful commit, as for grinding; failure never rolls back preparation,
blocks the next input or adds a second operation. Leave `GrindingCompleted` intact.

One new sibling controller owns one nonspatial `AudioStreamPlayer` on **Effects**,
PitchScale=1, MaxPolyphony=1, unity gain and no loop. It is local action confirmation,
with no spatial emitter, bearing, listener mutation, hearing/perception event,
Ambience routing, ducking lease or music hold. Existing Master/Effects controls,
Default/Quiet and mono apply once. Play H01 once on admission; no delayed start,
queue, overlap, polling replay or catch-up. Missing/malformed H01 or missing Effects
is silent while otherwise eligible text/history remain. Never fall back to Master.

An Always-process watcher cancels only this passage/caption on pause, focus loss,
dialogue, reading/customization/map/journal/settings overlay, text entry, invalid
delta outside (0,0.1], owner replacement/disable/hide/transform drift, nonfinite
positions or a Seeker discontinuity over 96 pixels. Normal later recipe progress
does not invalidate an already admitted short passage; the token remains consumed.
Opening a blocker at commit discards the presentation with no later replay.
Master/Effects mute or zero stops audio immediately; its admitted text may finish.
Unmute/resource repair never resumes. Successful `PresentationReset`/teardown clears
only heating player, caption, history, sequence and token; restored progression emits
nothing. Failed load changes nothing. No timer, token or history is serialized.

## Independent accessible presentation

Own a **thirteenth** caption line, **“Heating preparation”**, lasting three active
seconds. No actor/site/ingredient/remedy name, coordinate, direction, world time,
temperature, readiness, boiling or treatment success is added. This generic line
illustrates a completed recipe operation, not a continuing physical simulation.
The ordinary preparation status text remains authoritative and always available.
Caption preference hides only the new sound line; output mute/missing audio never
blocks eligible text. Preserve all twelve prior lines, timers and wording; expand
only the caption panel's own bounds if coexistence/wrapping needs it.

`RemedyHeatingCueHistory` owns a detached read-only oldest-first capacity-16 list,
with local sequence plus the exact anonymous phrase and no bearing/timestamp.
Each unique eligible commit creates one record immediately, without a time limiter
that could drop a second legitimate batch; reject token duplicates before insertion.
Add **Remedy heating cues** after existing sections in Audio → Recent sound cues.
Retain all prior ordering, capacities, limiters, scrolling, keyboard navigation,
Escape, Back to Audio and origin focus. History reading admits no cue. Blockers
retain earlier entries; successful reset/teardown clears heating only. Muted,
captions-off and missing-stream users keep access. Exercise capacity via stopped
diagnostics without inventing an ordinary repeated-batch gameplay capability.

## Finite implementation and verification

The JSON enumerates new exact paths and narrowly permitted existing edits. No
Domain, recipe/progression, physical world, artwork, station, navigation, save schema,
provider or hearing/perception edit is needed. The future coordinator edit adds only
the notification/token lifecycle; it must preserve all input checks/status/outcomes.
Preserve the local `game/project.godot` rewrite and every frozen accepted package.

After the single implementation grant, complete these dependent steps together:

1. Acquire/qualify the exact original and three-second H01; retain rights, listening,
   measurements and reproducible conversion/import evidence. Carry the owner's
   covered-pot source acceptance forward; derivative preparation must still meet
   the stated clean-window conditions. Accepted sources need no repeated review.
2. Pure and ordinary live checks for exact 1→2 progression/reference identity,
   48/49-pixel input, wrong/forged/stale/rebuilt/repeated tokens, direct-controller
   progression without notification, blocked starts, throwing subscribers, mute,
   missing stream/bus, expiry, cancellation, successful/failed load and no replay.
   Confirm that fast subsequent steep/cool inputs preserve the admitted short cue.
3. Seven identical actual movement plus seven-press preparation/treatment schedules:
   Default, Quiet, Master-muted, Effects-muted, missing H01, captions-off and
   heating-disabled baseline. Compare complete preparation/ingredient/batch/outcome,
   disease/care/administration, save roots, transcripts, movement and protected
   inventory/discovery/sabotage/time/hearing/perception truth. First six modes have
   one eligible heating event; disabled baseline has equal gameplay and no heating
   presentation. Preserve grinding/treatment event counts and all prior owners.
4. Native measured Master cases for unity/half/zero/mute Master and Effects,
   Ambience independence, Default/Quiet, mono, foreground coexistence, missing
   stream/bus, onset/tail, pause/focus/overlays, stalls/teleport, source drift,
   load/reset, and next-step input while the passage plays. Verify linear gain
   factors within 5%, mono parity, cancellation silence, and measured peaks/loudness
   both alone and with A04/A05/retained beds. Check no new ducking/hold lease.
5. Native thirteen-caption coexistence and independent history at 1280×720 and
   1920×1080: wrapping, visibility, capacity/sequence/duplicates, scrolling,
   keyboard/return focus, muted/captions-off/missing-audio access and isolated clear.
   Inject only in stopped diagnostics; mapped input must prove genuine admission.
6. Mount all three retained resource presets: preserve **110 existing audio entries**
   byte for byte and all 39 streams/32 notices in 20 groups. Add one stream/import
   pair and three notices for **40 streams, 35 notices in 21 groups, 115 entries**.
   Verify exact native H01 PCM16 and keep source/review audio outside runtime exports.
   Resource packaging does not establish an executable release.
7. Reject proposal/source/derivative/commit/bus/duration/phrase/truth drift and missing
   measurements; protect prior captions/history/controls and acceptance bytes.
   Run focused checks, `python3 quality.py --self-test` and root `./quality.sh`,
   retaining all 109 current stages and their completion markers. Pin unchanged
   rain fixtures/validators to this proposal's baseline via the existing historical
   runner; preserve prior hand-tool/Healer/foliage/wind pins and frozen validators.
8. Record verification/review and scope closure, sync/check the ledger and handover,
   update partial roadmap state, and commit/push reviewed paths under standing
   publication authority. Complete authorized dependent steps in the same session.

## One concrete new implementation grant

Authorize acquisition/qualification of **TinyWorlds' exact covered-pot Ogg**
(including the disclosed source-format exception if the encoding is lossy), preparation of **three-second
H01**, and one **local Effects cue only after the exact existing heating-step commit**
at the unchanged (2300,1300) station, with the **thirteenth “Heating preparation”
caption**, independent capacity-16 history, the named files/checks, new-passage
listening and standing Git publication. If the original/window cannot qualify,
report the blocker without substitution. This grants no burner/fire/boiling or
temperature simulation, new action/recipe/gameplay state, positional emitter,
other preparation step, wider audio, voice, artwork or executable release.

## Planning verification

The JSON binds this prose, exact retained recipe/station/input/controller sources,
all current runtime audio files, source observations and product evidence. Its
protected tracked-input snapshot compares baseline Git blobs and current SHA256s,
excluding only the three maintained continuation documents and recorded editor
exception. Current audio packaging has 110 entries; all 109 retained root stages'
final attempts and required completion markers pass, with 5,383 unit tests.
Continuation checks and 11 regression probes pass. Reuse this unchanged product
evidence for prose alone. The owner's original-source listening acceptance is recorded;
no new agent listening, PCM, native-output, device, runtime or physical-world
verification is claimed here.
