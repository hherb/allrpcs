# First water action: touch the stream

Prepared 2026-10-10 · `phase9-water-tool-action-owner-scope` · planning complete.
One implementation decision is pending for `phase9-stream-water-contact-runtime-integration`.
The [structured proposal](water-tool-owner-scope.json) binds this document, inspected
owners, the proposed PCM range and unchanged repository bytes.

The Seeker can deliberately **touch the flowing water beside the existing bridge**.
A successful engine commit produces the text “You touch the flowing water.” and one
small local splash. Walking over the bridge, standing nearby, inspecting a clue and
loading a save produce no contact. This is the first bounded water action owner;
collection, washing, drinking, wading and tool actions remain future scopes.

## Existing facts and the missing owner

| Inspected contract | Current fact / consequence |
| --- | --- |
| `OpeningRegionStreamLaundrySceneSource.Layout` and `OpeningRegionStreamLaundryGeometry` | Water polygon `[1600,452,2000,548]`, crossing bounds `[1736,452,1864,548]`, collision banks `[1600,1700]` and `[1900,2000]` across the water's Y range. Laundry remains an inert shell. Geometry supplies no water action. |
| `GreyboxNavigation` and `OpeningRegionLayoutGeometry` | Existing 96-pixel routes, 8-pixel cells and 24-pixel navigation inset. The stream-to-laundry route joins `(1800,500)` to `(1500,220)`. Observe live navigation; add no route or collision. |
| `InteractionFocusController` | Its 112-pixel focus range selects existing world objects for examination. It supplies neither hand reach nor a stream object. Keep its target list and focus unchanged. |
| `TemporaryWorldObjectSource` / discovery scenario | The water cask is a covered village container, not the stream. `stream-residue` is evidence, not water-contact authority. A field lens supports discovery eligibility; it is not a splash-producing tool. |
| `InventoryUsePolicy` / `DiscoveryToolPolicy` | Ownership and required-tool predicates do not execute a use action. The ordinary Seeker source initially has an empty inventory; discovery has a separate field-lens context. Do not synthesize a vessel or transfer either inventory. |
| `RepresentativeFirstPatientScenarioSource` | Preparation already supplies 250 ml of uncontaminated `clean-water` and requires 100 ml per remedy. Stream contact must not refill, consume or change that supply, certify stream safety or advance preparation. |
| `CharacterIntentionType` | Existing model intentions are wait, examine-object and collect-object. This proposal adds no model intention, provider schema or autonomous action. |
| Material/audio owners | B02 is a running-water/rain ambience composite, not a contact. Accepted A01 contains two splashes and is not installed in ordinary runtime. Use its first splash for a new contact owner; preserve the sole B02 bed. |

The missing deliverable is an engine-owned, explicitly requested contact commit with
independent presentation. Existing eligibility, geometry, ambience and accepted art
cannot supply that commit. The proposed reach and action semantics below require new
owner authority even though the physical geometry is retained.

## One contact site and its proposed reach

Use projection-local coordinates from the exact live projection and stream layout.
Derive the sole contact point `q = (crossing.Left - 2, crossing.Top + 32)`;
the present result is **`(1734,484)`**, in exposed water just west of the crossing.
The proposed standing footprint is the crossing inset by 12 pixels on all sides:
`[1748,464,1852,536]`. Admit a finite foot position in that closed footprint only when
its Euclidean distance to `q` is **at most 32 pixels** and the current navigation map's
closest supported point is within **one projection pixel**. The radius is a new
hand-contact rule, not a reuse of 112-pixel focus or treatment/preparation reach.

Retain the exact projection, stream binding/layout, Seeker node, current Seeker character,
navigation polygon and transforms. Require the current water/crossing/bank coordinates
above, the stream anchor and original route network. Require `q` inside water, outside
both bank collision polygons and outside the crossing. Reject absent/replaced sources,
changed geometry/transforms, unready map/region iterations and nonfinite positions.
Re-read actual Seeker position at commitment; never accept a caller-supplied position.

Source inspection gives one feasible example: `(1756,484)` is a route-cell centre,
18.3251 pixels from the stream-to-laundry route (less than its 24-pixel navigation
half-width), in the dry standing footprint and 22 pixels from `q`. This is a static
calculation, **not live reach or collision verification**. Implementation must prove
ordinary movement can reach this example and return along the existing route. If it
cannot, stop and report the unmet prerequisite; do not enlarge navigation, reach or
water geometry to make the proposal pass. No bridge-height simulation is asserted:
approval includes the small abstract hand reach from this existing top-down crossing.

## Action commit, player controls and authority

Add pure immutable `StreamWaterContactRequest` and `StreamWaterContactCommit` types,
a pure reach policy and an application controller. A request retains the exact current
Seeker character, bound contact site and current transient action-state instance.
The controller admits only that exact current request/state once, commits a fresh result
and replaces the transient state **before** notifying observers. Value-equal copied,
stale, repeated, wrong-character and wrong-site requests fail without notifications.
Recheck the current human-controller assignment and live access on every request.

The game coordinator is the only ordinary producer. A dedicated contextual **“Touch
water”** button calls it. Mouse activation and keyboard Tab/Enter/Space use the same
commit path; key echo, held input and double activation during the cooldown cannot
repeat it. Add a small native UI panel below the existing top-right overlay controls,
with a status line; resolve its placement at both retained viewports. It creates no new
world-object marker, object ID, evidence sparkle, map visit or shortcut/InputMap change.
Keep the recorded `game/project.godot` editor file out of implementation changes.

Show the button only with valid access, an active human Seeker and no reading/input
conflict. Suppress requests while the tree is paused, the Seeker cannot process physics,
the window lacks focus, a text-entry control owns focus, customization/map/audio/credits/
history is visible, or the shared character turn is nonterminal/reserved. Recheck those
conditions at activation, not just when displaying the button. Button focus itself is
allowed; return focus safely when it hides. Use native button accessibility labels.

A successful commit starts a **two-second transient cooldown**. Invalid requests do
not start it. Advance only finite active deltas in `[0,0.25]`; a larger/invalid delta
leaves the remaining cooldown intact and cancels presentation. Pause/focus loss freezes
the cooldown; resumption requires a fresh activation. Successful load resets the action
state and cooldown without replay; failed load changes neither. Removing/replacing the
owner invalidates all prior request/commit tokens. This avoids catch-up or held-key bursts.

The new commit records only actor/site/request identity and a session-local sequence.
It changes **no inventory, ingredient, contamination, disease, discovery, perception,
knowledge, transcript, meaningful world-time event or durable save field**. No clean-water,
temperature, cleansing, treatment or safety claim follows. The generic status lasts
1.5 seconds. The action remains available with muted/missing audio. Observer exceptions
cannot roll back a commit, retry it, or affect another observer. This is an immediate
abstract contact, not new hand animation or a timed occupation; accepted art stays intact.

## Exact audio and accessible feedback

Propose **A01-W01**, a single 1.5-second nonlooping WAV installed at
`game/assets/audio/water-contact/A01-W01.wav`. Extract frame interval **`[48000,120000)`**
from accepted `audio-direction/material-audition/clips/A01.wav` (1.000–2.500 seconds).
Retain its 48 kHz, stereo, signed 24-bit PCM exactly: 72,000 frames / 432,000 PCM bytes,
SHA-256 `553260f239cce9bd4c5fcb24009512274838b6a3a8faeaffd7fe72f563bf12ea`.
The accepted source WAV SHA-256 is
`a23db3992116b598b5dc652c95b18c51f5409691c747637ddacc5d0320f97a90`.

The first splash starts at source second 1 and its edited source lasts 66,292 frames;
this range contains it and recovery silence, excludes the second splash at second 4
and removes the audition's initial silent second. Preserve existing fades/gain/channels;
no normalization, new synthesis, mixing, resampling, download or re-audition. Only raw
bytes were inspected during planning; no derivative WAV was created or listened to.
Selection of this accepted passage does not establish physical splash realism.

Add one local `AudioStreamPlayer` on **Effects**, player gain 0 dB, no looping, spatial
emitter, hearing radius, new listener, bus or ducking lease. Once per exact successful
live commit, set independent caption **“Water contact”** for 1.5 seconds and play the
slice if the stream and Effects bus exist. Record the presented commit before playback;
missing resources/bus or muted output must not allow a retry. Do not play on request,
eligibility, movement, diagnostics presented as gameplay, startup or restore.

Add one independently owned water-contact line to `MaterialSoundCaptionPanel`, extending
its panel height as needed. It never overwrites regional, grinding/treatment, footstep,
conversation or interior lines. Caption eligibility is independent of Master/Effects
gain, range preset, missing resources and mono; captions-off hides only text. It conveys
the Seeker's own committed action, adds no hearing/perception event or cue history and
cannot disclose residue or an NPC. The status line remains readable with captions off.

Stop only the new player/caption/status on pause, focus loss, disabled or removed Seeker,
departure from valid access, teleport over 96 pixels, invalid/replaced source, successful
load and teardown. Unsubscribe all observers. Completion/expiration also clears only
owned presentation. Cancellation does not undo the contact or replenish the cooldown.
When an overlay opens, stop feedback and reject requests until it closes. Existing B02,
A04/A05, footsteps, enclosure gains, music opportunities and leases retain their owners.

Retain the existing recorded source credit: rubberduck, “40 CC0 water / splash / slime
SFX”, CC0-1.0, catalogue and licence URLs from the accepted provenance. The derivative
notice describes the exact A01 slice; install its own `CREDITS.txt`, copied licence deed
and `provenance.json`. Add that notice to `MaterialAudioCreditsPanel`; do not rewrite
frozen audition rights or existing runtime notices. Existing local rights records are
carried forward; this task makes no new legal or online catalogue assessment.

## Finite implementation files and checks

The JSON lists the exact proposed new/modified files, including adjacent Godot UID/import
sidecars. New source/test/smoke/output/validator files belong to this successor. Frozen
art/audio validators and accepted proof packages remain unchanged. Modify ordinary Main
only to insert the new action/audio/panel nodes before the accepted channel final child.
Do not alter existing scene sources, Seeker motion, preparation or save schemas.

Required implementation evidence:

1. Domain/application tests cover the exact successful request, duplicate/stale/value-equal
   instances, wrong actor/site, human assignment, cooldown, reset, exception isolation,
   footprint edges, 32-pixel inclusive reach, unsupported points and invalid numerics.
2. A stopped standalone proof and ordinary-Main smoke prove one explicit input/one commit,
   no contact from movement/focus/startup/load, a reachable `(1756,484)` and route return,
   missing/replaced geometry, overlay/text focus, echo/repeated inputs and lifecycle stops.
3. Compare identical action/input schedules under Default, Quiet, Master muted, Effects
   muted and missing-resource modes: exact commits, full saves, ingredient/world-time state,
   discovery/perception and transcripts agree. A no-action run has no new commits; durable
   state also agrees with the action run because the action has no durable effect.
4. Render **16 Master-output cases**: five ordinary identical-schedule modes above, Master
   reduced to 0.5, Effects reduced to 0.5, mono, captions-off, unsupported request, repeated request,
   overlay rejection, mid-passage focus loss, successful restore cancellation, missing
   Effects bus and an A04/A05 coexistence case. Bind commit counts, captions/status and PCM;
   muted/missing/rejected cases contain no new cue. Measure the new player separately
   from unchanged B02 ambience; verify active output against the exact PCM range.
5. At 1280x720 and 1920x1080 verify six concurrent captions without clipping, contextual
   button/status placement, keyboard activation/focus return, captions-off status, reading
   guards, credits and existing cue history. Use controlled native focus-loss evidence.
6. Run current `./quality.sh` on the host, plus the new successor validator. Verify prior
   audio/art/geometry/save inputs except the expressly modified UI/Main files, byte-exact
   source/PCM/notice provenance and resource packaging (34 streams / 15 notice bundles).
   Keep executable-export/template/device limitations explicit. Use four validator rejection
   probes: changed proposal/PCM, forged commit, changed reach/geometry and missing output proof.

The inspected `quality.py` runs the frozen interior validator against current sources.
That validator binds the five-line UI/Main boundary and cannot certify this successor.
The proposed quality integration pins that unchanged validator to inspected commit
`7b8dd659d409974126790b8a417533bded9c2e5e`, carries forward all retained product checks
and interior parity launches, and replaces its current-boundary calls with the new
water-contact successor validator. Add the five action parity modes and final successor
boundary check. Do not weaken the old validator or overwrite its recorded verification.

These are future checks, not planning results. The existing passing 2026-10-09 full-quality
evidence is reused for this prose-only task. Frozen historical reviews are not rerun against
successor sources. Planning verifies source and accepted-audio bindings, the static route
calculation, unchanged repository/editor bytes, whitespace and ledger self-tests.

## Planning completion and one new decision

The planning deliverable is complete; implementation and all derived audio remain absent.
Authorize the exact new **Seeker stream-contact action, 32-pixel reach at this one site,
transient commit/cooldown, contextual button/status, A01-W01 slice, Effects feedback,
independent caption/credits and finite validation** together. Record the owner's response
against both proposal hashes before implementing. No inventory, physical geometry, save,
perception, clean-water supply, LLM or broader tool/water action change is proposed.

Authority is required by [AGENTS.md](../AGENTS.md): “A genuinely new runtime/layout/physical
scope still needs the applicable authority.” The standing [publication instruction](footstep-runtime-review/publication/authority.json)
covers this completed proposal's commit/push; it grants no new gameplay scope or release.
Different contact sites, reach, collection/washing semantics or resources require a revised
concrete proposal. Carry all completed art/audio scopes forward unchanged.
