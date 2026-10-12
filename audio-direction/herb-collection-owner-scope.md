# Herb-collection feedback proposal

Date: 2026-10-12 (Australia/Sydney).

The owner's “Read HANDOVER.md and follow the instructions - scope is approved”
authorizes the ledger-selected planning slice. This proposal and its
[source-bound contract](herb-collection-owner-scope.json) complete that slice.
No audio was acquired, decoded, prepared or heard; no product or CI input changed.
The source/use/runtime grant quoted below remains distinct and ungranted.

## Existing committed occurrence

`TemporaryCharacterExaminationFlowController` retains the exact first object ID
from `TemporaryWorldObjectSource`: **herb-patch**. After the exact examination
occurrence is installed, `TemporaryCharacterExaminationResultSource` delegates
to `DeterministicExaminationResultSelector`. It returns `SafeToCollect` only for
an `ExamineObject` action targeting the supplied object ID. This is the accepted
temporary authoritative result, not a new botanical or safety assessment.

On a later process frame, `ProcessCollection(result)` selects `CollectObject`
with the result's exact target and calls
`CharacterTurnWorldEventOccurrenceController.TryOccurPostExaminationCollection`.
The Domain policy requires the exact retained examination event, its recorded ID,
the examination's exact transcript entry still last, the same actor-ID reference,
the exact target-ID reference, and an eligible retained collection event.
`TemporaryWorldEventSource.HerbPatchCollectedWorldEvent` has the ID
**healer-collected-herb-patch** and `WorldEventTimeEffect.Advance`.

Success installs the new event log/world time, appends the exact collection
physical-action transcript entry and assigns `LastOccurrence`; only after return
does the flow assign `LastCollectionOccurrence`. The collection event cannot be
recorded twice in the same log. Preserve that ordering, existing qualitative-time
advance, transcript, safe-result selection and turn completion. The operation
does not itself mutate herb inventory, vegetation, ingredient quantities or
recipe state. Accepted preparation and treatment owners stay closed.

## Actor/site qualification and silence

The ordinary retained Healer identity comes from
`TemporaryHealerCharacterSource.Character.Identity.Id`, shared with
`TemporaryCharacterTurnSource.Activity.Request.RespondingCharacterId` and the
Healer-first ordinary roster. The occurrence controller also accepts other
internally consistent actor IDs: the event name alone does not bind a body.

The candidate site is `Main/InteractionCandidates/HerbPatch`, a
`GreyboxInteractionTarget` at unchanged **(920,350)**, identity parent transform,
rotation zero and scale one. Its separately parsed marker ID is value-equal to
the retained `herb-patch` ID, not the same object reference. Keep exact Domain
references and use canonical value equality only for this marker linkage.
The initial Healer body is at **(2000,950)** and the Seeker at **(2120,950)**.
They are about 1,235 and 1,342 pixels from the patch respectively.

Neither the collection method nor its policy/result selector checks a body,
position, distance, hand contact, plant material, animation or navigation arrival.
The flow can therefore commit collection remotely. The separate independent
task route names this patch as its destination but commits examination through
`CharacterIndependentTaskOccurrenceController`; it does not populate the turn
occurrence controller's `LastOccurrence` or connect itself to this flow. It is
not a source of collection audio authority. Follow movement, task arrival,
narrative intent, focused markers and decorative foliage supply no such authority.

**Current decision: silence.** There is no qualified literal picking source or
acoustic binding. Do not emit a remotely positioned picking sound, a global
collection success chime, or a caption revealing collection/safety simply because
the authoritative event exists.

**Recommended successor:** separately authorize one restrained acoustic
representation of the already committed collection, only when the retained
Healer body is actually within inclusive **32 pixels** of the retained patch at
that commit and the actual Seeker is within exclusive **192 pixels** of the
patch. These are proposed audio-admission bounds, not new gameplay reach rules.
If the existing flow never commits with those positions, it remains silent;
do not move actors, delay/retry collection or repair gameplay to obtain sound.
Proximity alone still emits nothing. The new grant explicitly supplies the
otherwise missing acoustic interpretation; it grants no new physical action.

## Proposed source and accessible meaning

Select one free lossless mono/stereo recording of a brief soft leaf/stem handling
rustle, with one clean contiguous **1.0-second** window and natural decay.
No original is nominated or qualified. Require source-specific **CC0-1.0 or
CC-BY-4.0**, catalogue/download URLs, creator/title, retrieval metadata,
original bytes/encoding/hash, licence capture, edit lineage and shipped notices.
No purchase/account action or lossy-source exception. CC0 permits commercial
copying/adaptation; CC BY 4.0 requires appropriate credit, a licence link and
changes indicated. Preserve supplied notices and prior modifications without
implying endorsement. The official [CC0 deed](https://creativecommons.org/publicdomain/zero/1.0/)
and [CC BY 4.0 deed](https://creativecommons.org/licenses/by/4.0/) were checked on
2026-10-12; a deed does not establish any candidate's source-specific rights.

Reject wind beds, walking through bushes, footfalls, tearing/violent snapping,
tools, bag/cloth handling, water, voices, music, wildlife, tonal success signals,
clipping, decode faults and distracting background sound. Do not reuse accepted
F01, footstep, recipe, treatment or other PCM. The exact derivative is
`game/assets/audio/herb-collection/HC01.wav`: 48 kHz stereo PCM24, 48,000 frames,
no loop/compression/normalization. Permit only deterministic decode, contiguous
trim, necessary resampling, mono duplication, 10-ms onset/50-ms ending linear
fades and one uniform attenuation-only gain. Provisional sample-peak ceiling:
−24 dBFS; final suitability/level needs the owner's exact listening acceptance.
No synthesis, layering, denoising, filtering, pitch/time change or widening.
Present the exact derivative inline before installation. If no suitable window
or listening acceptance exists, record the blocker and retain silence.

The sound means only a nearby soft rustle. The independent sixteenth caption is
**Soft rustle**, lasting two active seconds. Its history section is **Nearby
rustle cues**, with a detached oldest-first capacity-16 snapshot containing only
local sequence and that phrase. Do not name herbs, collection, safety, actor,
site, ingredient, quantity, bearing, coordinates, timestamp or outcome. Anonymous
text does not itself justify admission: both text and history require the same
qualified exact commit and spatial bounds. Captions-off hides only this line;
mute or missing audio preserves otherwise admitted text/history. History reads
emit nothing and confer no perception, knowledge, hearing or journal authority.

## Proposed once-only delivery

Add an exception-isolated `CollectionCommitted` notification to the exact flow,
immediately after successful `LastCollectionOccurrence` assignment. Mint one
producer-owned `HerbCollectionCommit` retaining the exact flow, result,
installed occurrence and installed event-log/world-time/transcript references. Do not change the occurrence controller or
Domain transition. At synchronous notification require the current producer
token, exact `LastExaminationResult`, `LastCollectionOccurrence`, occurrence
controller's `LastOccurrence`, retained event and actor/target identities, and
the installed last transcript entry/log/time. Read actual retained bodies/site
at notification; caller-supplied coordinates cannot establish eligibility.
Consume each genuine token before blockers/spatial/resource checks. Reject
copied, rebuilt, foreign, stale, forged and duplicate tokens. A blocked/outside
commit is discarded permanently. Observer failure must never roll back the
commit, affect flow/turn completion or retry the occurrence.

Bind exact source/roster/body/site/listener/settings/restoration nodes and their
retained parents/names/identities. Require finite positions, visible enabled
site/body and unchanged site/ancestor transform and child topology; reject
replacement, rename, disable/hide or transform drift. Use the existing current
Seeker listener. Read the retained navigation and Sickhouse/Laundry layouts and
reuse `OpeningRegionInteriorAudioProjection` without changing its owner: both
bodies must have supported exterior positions, with valid layout/navigation.
Unknown/invalid support is silent, not neutral exterior. Add no listener, area/bearing/hearing model.

Use one `AudioStreamPlayer2D` on **Effects**, fixed at the patch commit position,
`MaxDistance=192`, `Attenuation=1`, `PanningStrength=0.6`, `PitchScale=1`,
`MaxPolyphony=1`, `AreaMask=0`, unity player gain. Apply existing Master/Effects,
Default/Quiet and mono preferences once. No lease, ducking or music hold changes.
Missing stream or Effects bus is silent; never fall back to Master. Mute/zero
stops HC01 immediately; unmute/repair never resumes it. No queue, delayed start,
replay, loop or catch-up.

An Always watcher cancels only this player's tail/current caption on pause,
focus loss, dialogue, reading, customization, map, journal, settings/text-entry
overlay, nonfinite or <=0 or >0.1-second delta, nonfinite position, listener
displacement over 96 pixels, leaving the listener range/interior eligibility,
actor leaving 32-pixel reach, or binding drift. Cancellation retains history.
Successful presentation reset/load clears only this owner’s player/caption,
history/sequence and token generation; failed load changes nothing. Baseline the
current producer token as already consumed on reset so it cannot replay. Reset does
not clear/rebuild gameplay or emit from pre-existing `LastCollectionOccurrence`.
Teardown unsubscribes; late subscription and re-enable do not catch up.

## Bounded files and finite verification

The JSON names the exact new source, Application, game/proof, preparation/output/
validation and review paths. Existing edits are limited to the flow notification,
Main sibling controller/smoke resources, independent caption/history append,
source-specific credits/export filters, necessary quality stages and continuation
documents. Keep Main's Channel last, prior node/art/body/marker/navigation bytes,
all prior audio owners and frozen validators intact. No actor travel/action,
examination policy, inventory, object removal, perception, truth, knowledge,
world-time, recipe, transcript, save-format, provider or voice change.

Required checks after a separate grant:

1. Source-specific rights/lossless original, reproducible derivative/import
   measurement and exact owner listening acceptance before installation.
2. Pure and live exact-token/reference/installed-state checks, 32/33-pixel actor
   and 191/192-pixel listener cases, foreign actor/site, remote commit, unsafe or
   failed examination/transition, duplicate/direct calls, stale/forged positions,
   independent-task examination, reset/restoration and throwing subscriber.
3. Seven identical genuine dialogue/examination/collection flow modes: Default,
   Quiet, Master-muted, Effects-muted, missing HC01, captions-off and audio-disabled.
   Retain separate qualified-near and ordinary remote-silence runs. Compare full
   save, event-log/world-time, transcript, inventory, recipe, task/party,
   perception/knowledge/truth and turn completion; audio cannot enable collection.
   Controlled fixtures must explicitly identify scaffolding and never claim an
   ordinary physical picking animation or autonomous approach exists.
4. Measured native Master unity/half/zero/mute and Effects gain/mute, Ambience
   independence, Default/Quiet/mono, left/right/centre and edge admission, onset/
   tail/cancellation silence, mixed HC01 with retained beds/footsteps/recipe cues.
   Check gain ratios within 5%; no fresh-device or ordinary-play compliance claim.
5. Native 1280x720 and 1920x1080 sixteen-caption coexistence/wrapping, anonymous
   history capacity/duplicates/scroll/keyboard/Escape/Back to Audio/origin focus
   and isolated reset. Preserve all fifteen prior caption owners and histories.
6. Three mounted presets retain all **125** current audio entries byte-identically:
   43 streams, 42 individual notices, 24 groups and 130 entries after the proposed
   single stream/four-notice addition. Verify exact PCM16 import conversion and
   exclusion of original/review audio. No executable-release claim.
7. Rejection probes, focused tests and root `./quality.sh`, retaining all 154
   previous stages/order/pins; runner self-test if changed; continuation sync/
   check/self-test, reviewed scoped commit/push and remote parity.

The planning checks reuse complete passing cooling product evidence (154 stages,
5,472 unit tests). The JSON binds current inspected sources, retained evidence and
all protected tracked bytes. Only these two proposal documents and the ledger,
handover and roadmap change; preserve the recorded editor rewrite and `.DS_Store`.

## Concrete new grant

> Authorize the bounded herb-collection source/use/runtime successor described
> here: qualify one new lossless HC01 with my exact listening acceptance before
> installation; represent only the existing exact flow-committed collection as
> a nearby soft rustle when the retained Healer is within 32 pixels of the retained
> (920,350) patch and the Seeker is within exclusive 192 pixels; add the anonymous
> Soft rustle caption/history and finite files/checks. Remote or unqualified commits
> stay silent. Do not change collection/examination gameplay, actor movement,
> physical actions, perception/hearing/truth/save or any accepted audio owner.

Completion is one qualified source and once-only conditional feedback, full
dependent verification and authorized publication. This planning approval does
not supply that new grant. The closed planning scope needs no repeated audit.
