# Terrain-footstep runtime proposal

Prepared on 2026-10-08 for `phase9-footstep-runtime-scope`. This completes the
read-only planning slice for the unfinished Phase 9.3 material-sound item.
[Structured inventory](footstep-runtime-scope.json) binds this document, current
runtime prerequisites, terminal sound acceptance, reference gait and exact PCM
extraction windows. No runtime or sample resource was changed or installed.

The proposed implementation is **`phase9-terrain-footstep-runtime-integration`**:
Seeker-only, displacement-driven material contacts, four explicit material rules,
24 unchanged-PCM contact resources, Effects routing, bounded captions, credits
and finite verification. Approval must cover both the new material declarations
and the separately bounded extraction below. It does not reopen accepted artwork,
sound selection, ground studies or the reference gait.

## Existing owners and missing prerequisites

`Main/Seeker` is a `CharacterBody2D`. `Seeker._PhysicsProcess` computes requested
velocity at 240 world pixels/second by default, calls
`SeekerWalkAnimationPresenter.PresentMovement(Velocity)`, then `MoveAndSlide()`.
Input and the current animation can therefore persist against a blocking wall.
There is no post-collision motion notification or ground-material registry.

`Main/Healer` also calls `MoveAndSlide()`, at a default 180 pixels/second, but its
ordinary Main presentation is a polygon placeholder with no contact animation.
Healer/NPC footsteps are deferred: there is no bounded perceived-source contract
for them. Only Seeker motion may produce this slice's audio or captions.

`GreyboxNavigation` owns the existing navigation/collision surface;
`OpeningRegionProjectionSource.Projection` retains eleven exact anchors and twelve
connections. Neither ambience's nearest-anchor rule nor the artwork establishes
footing. Use those exact source instances and transforms; do not change their
geometry, route cells, clearance, walls or world facts.

Ordinary Main still uses the six-frame `game/art/runtime/characters/seeker/walk-animations.tres`
(approximately 720 ms/cycle). It does not use the accepted sixteen/eight-frame
cloak reference resource. The [reference model](../art-direction/reference-gait-model.md)
stays unchanged. This proposal uses its half-cycle contact spacing as an audio
cadence benchmark, without promoting the cloak, changing animation playback, or
claiming anatomical synchronization with Main's current painted frames.

Existing `AudioSettingsController`, `MaterialSoundCaptionPanel`,
`MaterialAudioCreditsPanel` and `SaveSnapshotRestorationController.PresentationReset`
provide settings, presentation, credits and successful-load cancellation. New
transient state belongs to presentation, never to world saves or event logs.

## Exact proposed material coverage

All coordinates below are in the retained projection's local world-pixel space
(3200 × 1800). Convert the Seeker body origin and navigation queries into a common
space through the actual scene transforms. The body origin is the support-position
proxy; no invented individual sole offsets or texture sampling are used.

First require finite coordinates, the exact retained topology/geometry binding,
and a ready navigation map. A point must be within **1 pixel** of that map's closest
point. This deliberately covers the navigation surface rather than every physically
reachable route edge; collision-only margins remain unmapped. This query observes
the current surface and never changes a navigation destination or path.

Then apply this ordered registry. Rectangles include their boundaries; capsule
membership is distance to the clamped finite segment ≤ radius. Intersect every
positive rule with the navigation gate above. No nearest-landmark fallback exists.

| Priority | Declared audio material | Exact proposed coverage |
| --- | --- | --- |
| 100 | Unmapped interior | Sickhouse `Layout.RoomBounds` = [2480, 1310, 2800, 1502]; laundry `LaundryBounds` = [1420, 120, 1580, 360]. Silence, including their boundary unless another owner explicitly changes this proposal. |
| 80 | W04 hollow timber | `StreamBounds` = [1736, 452, 1864, 548], intersected with the live navigation gate. Explicit new declaration of the existing traversable crossing's audio material; no water contact or bridge construction. |
| 70 | W02 gravel | North sickhouse approach rectangle [2480, 1214, 2576, 1310]. Derive from exact `Layout.EntranceCenter` = (2528, 1310): x ±48, y −96 to y. Priority 100 wins at the room edge. |
| 60 | W03 leaf litter | Radius-24 capsules on the exact clearing→shrine segment (1000,350)→(380,300) and clearing→closed-path segment (1000,350)→(650,150), including their round ends. |
| 10 | W01 packed earth | All remaining points passing the live navigation gate, including remaining route interiors and village-edge open area, subject to the explicit interior exclusions. This is an owner-approved audio default for this retained navigation surface only. |

These are **proposed new declarations**, not deductions from accepted pixels or
sound-region names. Approval would author this audio registry without creating a
physical terrain system, changing movement cost, or publishing material knowledge
to Domain, perception, maps or saves. The changed material boundaries are part of
the concrete result being approved; no terrain coverage study is reopened.

The twelve connections are accounted for: clearing→shrine and clearing→closed-path
receive the leaf override; square→sickhouse receives the bounded gravel override;
edge→stream and stream→laundry receive timber only inside the crossing rectangle.
Square→garden, square→edge, square→road, edge→clearing, laundry→clearing,
shrine→road and road→wagon use earth. Other portions of all five overridden routes
use earth. Sickhouse/laundry interiors, collision margins outside the navigation
tolerance, nonfinite/out-of-world points, unready maps and mismatched sources are
unmapped. Unknown material IDs produce silence and a bounded diagnostic, never a
substitute material, denied action, splash, hidden clue or new surface.

## Displacement, modeled contacts and cancellation

Add a transient notification in `Seeker` immediately **after** its existing
`MoveAndSlide()` call. Supply pre/post world position, physics delta, movement-speed
limit and a monotonically increasing physics tick. Preserve the existing movement
and animation call order. Subscribers cannot throw through movement. No polling
input, pre-collision velocity, animation timer, visit history or saved event triggers
audio. No observer is attached to Healer.

The new controller observes this notification and retains the previous end position.
An unexpected difference >1 pixel between that end and the next start rebases
silently. This catches external teleports, including small relocations between
ticks; changes ≤1 pixel are ignored as tolerance and never counted as displacement.
A duplicate/out-of-order tick, nonfinite input, delta ≤0 or >0.1 seconds, nonpositive
speed, or displacement > speed × delta +1 pixel cancels the presentation step.
No catch-up audio is produced after stalls. A teleport performed during a physics
step within these bounds is indistinguishable from normal movement; future code
that adds an explicit teleport must call the same reset before relocating.

For each valid tick, measure the post-collision start→end displacement. Distance
≤0.01 pixel is stationary: clear partial cadence, stop owned tails and clear the
footstep caption. Wall pressure without displacement produces no steps. Sliding
along a wall can count its actual displacement. A disabled/paused Seeker cancels
through controller lifecycle checks even when no notification arrives. Set only
the new controller's process mode to Always so its watcher can cancel while the
tree is paused or `Seeker.CanProcess()` is false; it cannot advance cadence there.

Require mapped support throughout the measured segment: sample start, end and
intermediate points no more than 4 pixels apart with the same navigation/interior
gate. If any is unmapped, cancel and rebase; do not accumulate movement across water,
walls or gaps. This is a grounded **top-down presentation assumption on the current
static walk surface**, not `IsOnFloor()` gravity, an elevation/contact sensor, or a
claim that a specific painted heel touched the ground.

Accumulate eligible displacement, with **86.4 pixels per contact**: 240 × 0.360.
The first contact occurs after that distance, not immediately on keypress. On each
threshold crossing alternate fixed logical left/right identities; a full cycle is
172.8 pixels and, at default speed, 720 ms. At other speeds cadence follows distance
(a 120-pixel/second walk takes 720 ms/contact); it never changes locomotion or art.
Resolve material at the interpolated threshold-crossing point, not the end of a
long tick. Crossing a material boundary alone emits no sound and does not reset
cadence. Turning also preserves accumulated distance and logical identity.

At most one contact is emitted per physics tick and contacts are separated by at
least **0.180 seconds** of eligible physics time. Any additional crossings advance
logical identity/variant state and consume distance but are discarded without a
queue. Reset returns cadence and identity to their initial state. Variants have no
recorded anatomical identity; logical left/right labels never relabel the samples
as recordings of a particular foot. Main's visible animation is not resynchronized
by this contract. A later animation-contact integration needs its own scope.

Stop/blocked motion, unmapped support, discontinuity, pause, disable, successful
load (`PresentationReset`), reset or teardown stop both footstep players, clear
partial distance, eligibility time and captions, and discard pending work. Failed
load emits no reset. On resume/rebase establish a baseline without a contact.
Fresh ordinary movement is required before another step. Audio failure cannot alter
movement, event counts, disease/time, route permissions or restoration success.

## Separately bounded sample extraction

W01–W04 are accepted ten-second stereo PCM24/48 kHz passages with six separated
half-second windows. Keep the four parent WAVs and their sources byte-identical.
Extract **six resources per material, 24 total**, solely by copying PCM frames
from these accepted passage intervals (end exclusive):

| Variant | Passage interval in seconds | PCM frame interval at 48 kHz |
| --- | --- | --- |
| 01 | 1.0–1.5 | [48000, 72000) |
| 02 | 2.3–2.8 | [110400, 134400) |
| 03 | 3.6–4.1 | [172800, 196800) |
| 04 | 4.9–5.4 | [235200, 259200) |
| 05 | 6.2–6.7 | [297600, 321600) |
| 06 | 7.5–8.0 | [360000, 384000) |

Each resource has 24,000 stereo frames, 144,000 PCM bytes and duration 0.5 seconds.
Write standard PCM WAV headers without decoding/re-encoding, resampling, mixing,
normalizing, filtering, pitch shifting, adding fades or regenerating from originals.
The existing accepted fades and gain survive exactly. The inventory records each
expected PCM hash; final WAV/header hashes are obtained only after authorized
extraction. Planning inspected headers and PCM bytes, without rendering or listening
again. No derivative WAV was created during planning.

The parent preparation placed windows starting about 100 ms before detected RMS
peaks. A scheduler event starts the **whole retained contact window**; exact heel
onset or zero perceptual delay is not claimed. Output verification must document
onset latency and overlap. Correcting it by trimming/fading would exceed this grant
and require a concrete successor, rather than silently changing accepted samples.
W04 retains its recorded MP3 ancestry. Accepted listening is carried forward.

Install only `game/assets/audio/footsteps/W01-01.wav` through `W04-06.wav`, with
their import metadata, separate provenance/credits and the retained CC0 notice.
Do not ship whole ten-second passages or source originals as footstep loops.
Cycle variants 01→06 independently per material, starting at 01 on reset. Material
changes retain each material's counter. No randomness or sample modification.

## Playback, captions and credits

Use exactly two non-positional footstep players on Effects, each with emitter gain
1; reuse an idle player, otherwise replace the oldest. No more than two half-second
tails or stale queue. Existing A04/A05 players remain independent. Footsteps take
no foreground/ducking lease. Existing Effects/Master attenuation, mono and
Default/Reduced range apply once. Muting stops owned tails while logical cadence
and optional captions continue; unmute never replays a missed contact. Missing or
malformed audio resources fail quietly, without stopping movement or caption logic.

Extend the existing caption panel with one separate noninteractive footstep line;
retain regional/action ownership and their timers. Show “Footsteps on earth”,
“Footsteps on gravel”, “Footsteps in leaves” or “Footsteps on timber” after an
eligible contact for 0.75 seconds, renewed by later contacts. It clears promptly
on movement cancellation. User output mute/zero and resource failure do not disable
the caption; the existing Captions preference does. No Healer/off-screen/source
direction/hidden-location information is added. Verify the resulting three-line
layout at 1280×720 and 1600×900; extend only the caption panel's own bounds.

Reuse retained source records for simonjeffery13, BenDrain, Dominik_W and
florianreichelt (all recorded CC0-1.0). Derivative provenance records accepted parent
hash, passage/source intervals, original ancestry and the new header-only extraction.
Add these four contributors and derivative paths to the existing visible credits
view; keep the six-sound credits/resources intact. Package a separate footsteps
`CREDITS.txt`, `provenance.json` and CC0 notice via additive export include filters.
No downloads, purchases, account activity or fresh rights audit are required.

## Exact implementation boundary after approval

| Files | Bounded change |
| --- | --- |
| `game/Seeker.cs` | Exception-isolated post-slide transient motion notification only; same input, speed, animation order and collision operation. |
| `game/OpeningRegionFootstepMaterialSource.cs` and `.uid` | Exact-source/transform adapter and navigation gate for the explicit registry; no navigation/physics mutation. |
| `src/Application/Audio/OpeningRegionFootstepMaterialProjection.cs` | Pure declared-region precedence/mapping, no Godot or new Domain world-material model. |
| `src/Application/Audio/FootstepContactCadence.cs` | Pure bounded displacement, continuity, logical identity, crossing-point and variant state. |
| `game/OpeningRegionFootstepAudioController.cs` and `.uid` | Seeker subscription, two players, existing reset/settings, cancellation and caption ownership. |
| `game/Main.tscn` | Add only source/controller nodes and scripts, preserving existing art, actors and physical nodes. |
| `game/MaterialSoundCaptionPanel.cs` | Add independent footstep line/timer and its clear operation; preserve region/action API and existing ownership. Its `.tscn` remains unchanged. |
| `game/MaterialAudioCreditsPanel.cs` | Add the footsteps credits text beside retained material credits. |
| `game/export_presets.cfg` | Add footsteps TXT/JSON/HTML include patterns to the three existing development presets. |
| `game/assets/audio/footsteps/` | Exactly 24 WAVs and `.import` files, provenance, credits and CC0 notice; no engine cache. |
| `tests/Application.Tests/Audio/OpeningRegionFootstepMaterialProjectionTests.cs`, `FootstepContactCadenceTests.cs` | Meaningful registry/cadence/cancellation tests. |
| `game/FootstepAudioRuntimeSmokeCheck.cs`, `FootstepAudioRuntimeProof.cs`, their `.uid` files and `FootstepAudioRuntimeProof.tscn` | Ordinary Main movement/reset/parity checks; stopped-by-default disposable output proof. |
| `scripts/prepare-footstep-contacts.py`, `validate-footstep-audio-runtime.py`, `check-footstep-audio-output.py` | Exact frame-byte extraction, successor boundaries and output measurements. Do not weaken frozen predecessor validators. |
| `audio-direction/footstep-runtime-authority.json`, `footstep-runtime-verification.json`, `footstep-runtime-review/` | Actual grant, changed-runtime evidence and limitations. |
| `CONTINUATION.json`, `HANDOVER.md`, `ROADMAP.md` | Scope maintenance; retain every closure and keep the larger material-sound item unchecked. |

No change to Healer, `SeekerWalkAnimationPresenter`, accepted gait/art resources,
`GreyboxNavigation`, regional/action controller, bus/control policy, physical
geometry, save schema, world/event/perception rules or `game/project.godot` is
included. Water/tools, other interiors, NPCs, positional/off-screen cues, music,
voice, release/signing/distribution and Git publication remain separate.

## Finite implementation checks and completion

1. Check accepted parent/source/reference hashes, all 24 exact PCM ranges, WAV
   format, imports/decoded precision and successor file boundaries. Reject wrong
   slice offsets, changed PCM, overlapping material precedence, wrong source binding,
   added physical/art changes, Main whole-passage loops and Healer subscriptions.
2. Exercise all twelve retained routes and eleven anchors, interior exclusions,
   every material edge/corner/tie, transform conversion, nav-map readiness,
   collision margins, invalid coordinates and unknown IDs. Boundary decisions must
   be deterministic and independent of pixel colours, ambience, map visits and input.
3. Drive **ordinary Seeker physics**: normal/diagonal movement, sliding and wall
   pressure, variable delta/speed, partial steps, turns, boundary crossing, rapid
   stops, teleports, pause/disable, duplicate ticks, stalls, load success/failure and
   teardown. Test threshold interpolation, fixed logical identity, variant cycling,
   rate limits and no backlog. Compare positions, time/events and committed save
   roots with audio normal, muted, missing and captions off. Healer remains silent.
4. Render actual final Master output for all four materials and six variants, two
   overlapping tails, fastest supported cadence, material changes, cancellation,
   A04/A05 plus footsteps plus beds, Effects/Ambience isolation, category/Master
   zero/mute, mono and both ranges. Record peak/true peak/levels and contact-window
   onset delay; report policy deviations without changing accepted PCM. Caption
   and output proofs do not claim device listening or anatomical frame synchronization.
5. Inspect changed ordinary Main captions/credits at both viewports, including
   muted/missing sound, toggles, action overlap and prompt clearing. Export only the
   24 contact resources plus their notice files, retain existing six assets/notices,
   and verify visible credits and export inventory. Preserve known application-export
   limitations; disposable unsigned resource proof is not release authority.
6. Run applicable pure tests, restore/format/build, all four unit-test projects,
   ScenarioValidator, retained Godot smoke suite and new ordinary movement probes.
   Use a successor boundary validator around retained quality commands; predecessor
   validators remain frozen. Record actual results and changed-runtime review.

Complete when the approved registry and all 24 exact contact resources operate
from real Seeker displacement with cancellation, captions, credits, existing
controls and passing finite checks. This does not complete the whole audio identity,
every interior/material, anatomical animation contact work or the wider roadmap item.
Planning itself needs only inventory/acceptance/hash checks, continuation self-test
and whitespace checks. Product inputs were not changed; no new product run is needed.

## One next decision

Approve the exact **Seeker-only material registry, modeled cadence and 24 PCM-slice
extraction/runtime boundary** above as `phase9-terrain-footstep-runtime-integration`.
Record the owner's actual response against both proposal hashes in
`audio-direction/footstep-runtime-authority.json` before extraction or runtime work.
The proposal's current status supplies no such grant. This is a new runtime/material
decision; unchanged accepted sound and art need no repeated selection.
