# First Healer positional footstep proposal

Prepared 2026-10-11 (Australia/Sydney). This finite planning slice is complete;
one new runtime/material/emitter grant remains pending. The exact source bindings,
resource inventory, proposed files and checks are in
[healer-footstep-owner-scope.json](healer-footstep-owner-scope.json).

## Proposed result and authority

Add positional **exterior Healer footsteps** heard from the Seeker's existing
listener. Observe only the exact ordinary `Healer` body and its current
`TemporaryHealerCharacterSource.Character`, also retained as the first ordinary
participant. Follow and move-to-destination may produce contacts; wait, arrival
and blocked motion produce none. No new movement command or autonomous actor is
introduced. The unchanged Healer starts at `(2000,950)`, follows the Seeker,
moves at 180 pixels/second and stops within 72 pixels when following or 12 pixels
when commanded to a destination.

This supplies one missing actor/emitter for the remaining “Add positional ambience
and meaningful off-screen sound cues” roadmap item. Approval would newly authorize
the exterior material registry **for this actor**, distance admission, positional
playback and anonymous accessible cues described below. Existing Seeker material
authority, accepted audio and the continuation instruction do not supply this grant.
The wider roadmap item remains unfinished after this subset.

Healer footsteps are a presentation observation of completed physical movement.
They create no `DialogueEvent`, audience, perception, belief, knowledge, passive
noticing, world-clock event, inventory change, saved field or provider input. The
new caption/history permission conveys an anonymous nearby contact and approximate
screen bearing to the human player only. It does not assert that a game character
heard, recognized or learned anything. Existing recorded-reply hearing admission
and its history retain their exact owners and rules.

## Exact producer, cadence and lifecycle

After each existing `Healer.MoveAndSlide()` path, including `StopMoving`, emit one
exception-isolated `FootstepMotionSample`: monotonically increasing physics tick,
actual pre/post global body origins, physics delta and current speed limit. Add
only this observational hook and the plumbing needed to pass delta through stop
paths. Preserve command selection, collision exceptions, velocity, destination,
navigation-agent settings and movement call order. Tick exhaustion cancels further
notifications safely until a fresh body lifetime; it never wraps or stops movement.

`OpeningRegionHealerFootstepAudioController` is the sole ordinary subscriber and
owns a **separate** unchanged `FootstepContactCadence` instance. Its counters cannot
consume or reset Seeker cadence. Bind exact current body, character source/character,
roster entry, Seeker/source, projection/topology, navigation region/map/polygon and
vertex/index data, retained physical layout sources, settings, restoration and the
existing listener. Check node identity, ancestry, processing/visibility and retained
static source transforms. Retain body parent/rotation/scale while allowing measured
body translation. Disabled, replaced, hidden or moved static prerequisites fail silently;
off-screen visibility is allowed. Do not replace source identities with value-equal
objects or infer an actor from an ID alone.

Use the retained cadence's **86.4 pixels/contact**, first contact after sufficient
actual supported displacement following a baseline sample. At the Healer's default
180 pixels/second this is **0.480 seconds/contact**, 172.8 pixels and 0.960 seconds
per modeled cycle. Logical left/right alternates; six variants cycle independently
per material, initially 01. Turning/material changes preserve partial distance.
This is a new modeled Healer audio cadence, not a promotion or resynchronization
of accepted gait artwork. Main's Healer is still the polygon/satchel placeholder;
no painted heel, anatomical contact or animation frame synchronization is claimed.

Retain cadence limits: delta `(0,0.1]` seconds, finite positive speed, stationary
distance at most 0.01 pixel, previous-end/current-start tolerance 1 pixel,
displacement no greater than `speed × delta + 1`, and the 3672-pixel absolute
displacement cap. Reject duplicate/out-of-order ticks and nonfinite input. Require
material/support at both endpoints and segment samples at most 4 pixels apart;
resolve each contact at the interpolated threshold, not the frame end. At most one
contact per tick and at least 0.180 seconds of eligible time between emissions;
excess crossings advance logical/variant state and are discarded without a queue.
The unchanged helper clears partial cadence on stationary/invalid samples and
silently rebases on discontinuity. Explicit lifecycle reset also clears its baseline.

Cadence continues independently of distance, mute, captions and missing playback
resources while physical samples remain valid. Out-of-range contacts are consumed
without output or history, and cannot replay on approach or unmute. Each callback
consumes its exact sample once. Retain at most one private pending contact tagged
with that engine physics frame; present it only later in that same frame after the
existing listener update. A frame mismatch discards it. This bounded handoff never
queues a contact for a later frame. Never expose a live arbitrary-contact injection
API. Replaced/forged/duplicate notices cannot start playback or history.

An Always-process watcher cancels only the new tails/current caption and resets
cadence on pause, focus loss, reading/customization/map/journal/dialogue/settings
overlay, text entry, body/source disablement, unsupported motion, replacement,
nonfinite data or teardown. This changes presentation only; it does not pause or
deny Healer movement. Listener teleport over 96 pixels also resets presentation;
small between-tick actor relocations over the cadence's 1-pixel continuity tolerance
rebase silently. Successful `PresentationReset` clears the new cadence, tails,
caption and history without replay; failed load changes nothing. Pause/focus and
overlay cancellation retain already recorded history, while its rate-limit clock
freezes. Teardown clears all new transient state. Retained owners stay independent.

## Materials and supported exterior scope

`OpeningRegionHealerFootstepMaterialSource` observes the exact retained sources and
calls the unchanged pure `OpeningRegionFootstepMaterialProjection.Select`. It does
**not** call the Seeker-coupled live material source or interior override. Convert
world support positions to projection-local coordinates. Require an active ready
unchanged navigation map/region and finite closest point within 1 local pixel;
every motion-segment sample must pass this gate. Bind the current physical sources
and their geometry to the inspected hashes; no new terrain, cells or routes.

The proposed Healer-only material permission is this ordered exterior registry:

| Priority | Result | Projection-local coverage, intersected with navigation support |
| --- | --- | --- |
| 100 | Silence | Inclusive sickhouse `[2480,1310,2800,1502]` and laundry `[1420,120,1580,360]`, including boundaries |
| 80 | W04 timber | Inclusive crossing `[1736,452,1864,548]` |
| 70 | W02 gravel | Inclusive north approach `[2480,1214,2576,1310]`; room exclusion wins |
| 60 | W03 leaves | Radius-24 closed capsules `(1000,350)→(380,300)` and `(1000,350)→(650,150)` |
| 10 | W01 earth | Remaining supported points inside inclusive `[0,0,3200,1800]` |

Unsupported, nonfinite, foreign or unknown material inputs are silent. These are
explicit audio declarations for a newly authorized actor, not facts deduced from
accepted paint or general physical-material knowledge. Sickhouse/laundry Healer
contacts, source/listener room transmission and full room acoustics are deferred.
Existing Seeker W04/W01 interior behavior and enclosure gains remain unchanged.

## Listener, distance, pan and tails

Reuse the current exact `OpeningRegionPositionalAudioController/SeekerListener`
`AudioListener2D`. The new owner never calls `MakeCurrent`, creates another listener
or modifies the B02/reply controller. Validate it is current, live, top-level,
unrotated, unit-scale and at the actual Seeker global body origin within 1 pixel.
Sample the Seeker position directly for admission; a stale listener causes silence,
not a larger range. Set only the new controller's physics priority to 100, after
the retained priority-0 bodies and positional owner. Drain its same-frame handoff
there without changing Healer/Seeker movement order; recheck immediately before
starting a new player. A tick with stale
listener placement consumes its contact silently without delayed presentation.

Both emitter support point and listener must be on the retained supported exterior
registry. Admit only finite world distance **strictly less than 192 pixels**;
at 192 and beyond there is no audio, caption or history. This range is a new audio
presentation declaration, independent of dialogue hearing's numerically similar
range. No screen-size/zoom-based admission or hearing-selector call is permitted.
This is a Euclidean exterior approximation; it does not model acoustic paths,
wall occlusion, vertical elevation or propagation delay.

Use two independent `AudioStreamPlayer2D` tails on **Effects**: `MaxDistance=192`,
`Attenuation=1`, `PanningStrength=0.6`, `PitchScale=1`, `MaxPolyphony=1`, `AreaMask=0`,
unity player gain and looping disabled. An idle slot wins; otherwise replace the
oldest tail. Each source remains at its contact's actual interpolated global
position for the half-second window; it does not follow the Healer's later body
position. Recheck support/listener/range during playback and stop an ineligible
tail without restart. Keep native positional center scaling and stereo panning;
no compensating gain, manual pan, second distance multiplier or enclosure gain.
The expected relative distance factor is `max(0,1−d/192)`; native Master capture
must verify it, pan direction and zoom-dependent native pan after approval.

Master/Effects gain, mute, Default/Quiet range and mono apply through existing
buses exactly once. Effects/Master mute or zero stops owned tails immediately;
unmute needs a fresh contact. Missing/malformed stream or missing Effects bus
produces silence with eligible caption/history intact; no fallback to Master.
Footsteps take no foreground/ducking lease or music hold and alter no bed/action.

## Captions and separate accessible history

Add one independently owned **ninth** line to `MaterialSoundCaptionPanel` for
0.75 active presentation seconds, renewed by each admitted contact. Use the four
retained generic material phrases (“Footsteps on earth”, “Footsteps on gravel”,
“Footsteps in leaves”, “Footsteps on timber”). Add one of the retained eight
screen bearings only when the contact is outside the actual inclusive viewport,
using `GlobalCanvasTransform × CanvasTransform`, not world-left/right. Clockwise
half-sector ties match the unchanged pure positional caption helper. Keep the
material and bearing captured at contact time until renewed or cancelled. Text
contains no Healer name, ID, coordinates, destination, hidden state or intent.

Caption preference controls the new visible line. Output mute/resource failure
does not remove spatial admission or text. No caption outside the 192-pixel gate,
for unsupported movement, a stationary actor, startup, arrival or rejected notice.
Cancellation clears only this line; all eight existing timers/owners remain intact.
Increase only the panel's own upper bound as needed for nine wrapped lines and
verify coexistence and control separation at 1280×720 and 1920×1080.

Own a separate `HealerFootstepCueHistory`, capacity **16**, oldest-first eviction
and detached read-only entries. Record the first admitted contact immediately,
then at most one entry per **1 second** of valid active presentation time. Discard
rate-limited contacts without a queue, even when wording changes; audio/current
caption still present. Entries contain only a local sequence and that generic
contact caption, no world timestamp or actor identifier. Captions off, mute and
missing stream/bus retain eligible history. Out-of-range movement creates none.

Extend the existing Audio → Recent sound cues view with a separately titled
**Nearby footstep cues** section and its own empty message. Keep reply history
data, ordering, capacity, clearing and click limiter unchanged; render the two
histories independently inside the existing selectable, scrollable text view.
Preserve Back to Audio, Escape, Tab/Shift-Tab and focus return. History remains
readable with captions off. Opening a reading overlay cancels new contact
presentation and adds no new entries until active play resumes. No durable journal
or transcript integration is authorized.

## Accepted samples, files and finite verification

Reuse all **24 existing** `game/assets/audio/footsteps/W01-01.wav` through
`W04-06.wav` and their imports, unchanged: 24,000 stereo PCM24 frames, 48 kHz,
0.5 seconds each. Their exact file and PCM hashes and accepted-parent frame ranges
are listed in JSON. No new sample, extraction, acquisition, edit, gain, fade,
resampling, channel change or repeated listening/selection. Preserve W04's recorded
lossy ancestry and existing source credits/deed/provenance. No new notice folder
or export pattern is needed. Reuse the existing footstep notices in credits.

The exact file list permits new Application admission/history contracts and tests,
the new Game material source/controller, live smoke/stopped output fixture, and
measurement/current-boundary scripts. Existing runtime edits are limited to the
Healer observational hook, Main wiring, caption panel and history display. Root
`quality.py` adds the new checks and pins the unchanged hand-tool validator and
fixtures at this inspected baseline through the existing isolated-history runner.
Keep Channel the last Main child and preserve all prior scene/geometry blocks.
No existing Seeker, B02/reply, interior, action, movement-selector, Domain,
perception, inventory, provider, save or frozen-validator edit is permitted.

After the one implementation grant, completion requires:

1. Focused admission/history checks plus new-owner cadence integration: actual
   post-collision follow/move/stop, 180-pixel speed, independent counters, support
   interpolation, range edge, foreign/duplicate notices, safe observer failure,
   source/listener replacement, rate limit/eviction and lifecycle isolation.
2. Six live schedules: Default, Quiet, Master-muted, Effects-muted, missing
   resources and captions-off, plus a seventh observer-disabled baseline. All
   have identical actual motion/commands, complete durable save/transcript and
   serialized protected inventory/discovery/sabotage/time/hearing/perception truth.
   First six have equal eligible modeled contacts; baseline has no new presentation.
3. The 43 JSON-listed actual Master case groups: all four materials/six variants, distance
   boundary/falloff, left/right/mono, camera changes, bus/master settings, missing
   resources, stopped/rejected/unsupported motion, independent overlap, load,
   pause/focus/overlay and listener/source invalidation. Record native onset/tail
   latency, final peak/loudness and cancellation; require scoped silence, gain and
   pan thresholds. Each material group measures its six variants separately; grouped
   rejection/lifecycle cases retain a result for each named subcase. This does not
   establish ordinary-play loudness/device listening.
4. Native ordinary Follow/Wait/Move commands, supported exterior travel and no
   contact from arrival/wall pressure. Verify off-screen bearing, nine-caption
   coexistence, muted/captions-off history, capacity/rate limit and keyboard/focus
   lifecycle at both viewports. Diagnostic injection remains stopped and explicit.
5. Mounted native resource archive decodes match the existing PCM24-to-PCM16
   conversion for all 24 reused streams. Resource inventory stays **36 streams,
   23 individual notices, 17 notice groups**, with prior entries byte-identical.
   This is resource packaging, not an executable release.
6. New boundary rejection probes for changed proposal/resources, foreign or
   replayed motion, material/geometry drift, wrong listener/range, leaked hearing
   or protected truth, missing output evidence and cross-owner caption/history
   mutation. Full root quality retains all 62 current stages and adds the new
   schedules/output/boundary checks, with explicit fixture completion strings.

Planning verifies source/resource hashes, 82 accepted audio bindings, the protected
tracked-file aggregate and continuation self-tests. Product and CI inputs are
unchanged, so retain the passing 62-stage/5,227-test/293-launch evidence linked in
JSON. No runtime, native acoustic, movement or view proof is claimed for planning.

## One outstanding owner decision

Approve exactly this Healer-only exterior contact/material permission, existing
Seeker-listener spatial playback within 192 pixels, anonymous ninth caption and
separate bounded transient history, with the named implementation and checks.
The repository's new-runtime authority rule requires this one grant. Once granted,
record its exact scope/hashes and complete dependent implementation, validation,
review and standing publication in the same session. Accepted samples and closed
owners need no renewed selection. Interior Healer sounds, other actors, gameplay
hearing, full acoustics, animation integration, voice and release stay deferred.
