# Accepted material sounds: prepared runtime scope

2026-10-08 · Phase 9 · `phase9-material-audio-runtime-scope` planning complete.
Implementation decision: **pending owner authority**.

Implement four regional ambience beds and two successful-action cues in ordinary
Main: **B01–B04, A04 grinding, and A05 treatment feedback**. These six accepted
excerpts have existing environmental or action owners. Add captions and use the
existing mixer. This is the smallest useful complete boundary that supplies rain,
water, wind, foliage, shelter and remedy feedback without inventing terrain or
tool actions. It advances, but does **not complete**, the material-sound roadmap item.

[The structured inventory](material-runtime-scope.json) binds inspected code and
all sixteen accepted candidates, including the ten excluded from this implementation.
[Terminal listening acceptance](material-audition/owner-acceptance.json) closes
selection. No excerpt, source edit, acceptance record, policy or existing mixer
evidence is revised. The new authority would cover runtime composition only.

## Actual owners and missing prerequisites

`RepresentativeOpeningRegionEnvironmentalSource.Create` already assigns every
opening location to one of four regions and gives each region a canonical sound
state. `OpeningRegionEnvironmentalStateSource.CurrentRegions` owns the live
snapshot. `OpeningRegionProjectionSource.Projection` supplies the eleven exact
authored location anchors. There is no continuous authoritative current-location
or sound-emitter map. `OpeningRegionMapVisitVerificationCoordinator` checks a
24-pixel visit radius and skips already verified visits; its map history cannot
drive continuous audio or repeat visits.

`Seeker._PhysicsProcess` presents requested velocity **before** `MoveAndSlide`;
`Healer._PhysicsProcess` has several stop paths. Requested velocity or a playing
walk animation therefore cannot prove a foot contact or actual displacement.
The navigation bindings inspected provide geometry, routes and clearances, not
a physical footstep-material lookup. Rendered soil/gravel/wood names and accepted
artwork are insufficient authority to assign physical surfaces. The accepted
cloak gait is also not installed by this proposal.

`RepresentativeFirstPatientScenarioSource.Create` authors grinding, heating,
steeping and cooling, in that order. `OpeningRegionRemedyPreparationInputCoordinator.TryProgress`
advances an exact requested step through `FirstPatientPreparationController.TryAdvance`.
`OpeningRegionFirstPatientAdministrationInputCoordinator.TryAdminister` returns
the existing administration result. Those two **successful live operations** can
emit feedback without deriving it from input, status text or restored save state.

| Candidate | Existing owner / possible call site | Decision in this grant |
| --- | --- | --- |
| B01 sheltered rain | `village-shelter`, sound key `muted-village-rain`; sickhouse, healer-garden, village-square | Install unchanged as Ambience. Shelter is qualitative; no new interior/reverb simulation. |
| B02 water and rain | `waterway`, `running-water-and-rain`; village-edge, stream, abandoned-laundry | Install unchanged as Ambience. It is one composite regional bed, without another full rain layer. |
| B03 canopy rain | `woodland-edge`, `rain-under-trees`; woodland-clearing, old-shrine, closed-deeper-forest-path | Install unchanged as Ambience; handle the existing revealed-sound variation below. |
| B04 wind/rain/creak | `southern-road`, `road-wind-and-wagon`; southern-road, abandoned-wagon | Install unchanged as Ambience. The baked-in creak is inert material texture, without a character/motion event. |
| W01 earth | Seeker/Healer after actual movement | Defer: explicit material coverage and contact/cadence contract are missing. |
| W02 gravel | Same movement owners | Defer: no runtime gravel classifier; do not infer it from the sickhouse gravel sprite. |
| W03 leaf litter | Same movement owners | Defer: a woodland sound region does not establish leaf-covered footing. |
| W04 hollow timber | Same movement owners; stream crossing is geometric | Defer: do not infer timber from bridge, laundry or wagon appearance. |
| A01 water contact | Stream/laundry scene source defines geometry, not a splash operation | Defer: no authorized wading, water-contact or laundry action. Crossing a route does not create one. |
| A02 container/cloth | Generic interaction and world-object controllers | Defer: focus/inspection does not establish a wooden container or cloth-handling action. |
| A03 wood/tool | Focused search includes a field-lens requirement | Defer: eligibility to use a lens does not establish a wood strike or tool-work event. |
| A04 grinding | Successful exact Grinding advance in `TryProgress` | Install unchanged once as Effects. It represents completed grinding; no claim that a stone mortar object now exists. |
| A05 treatment feedback | Successful exact `TryAdminister` result | Install unchanged once as Effects. Existing treatment text owns the outcome; sound cannot diagnose or stabilize anything. |
| A06 UI/conversation | Optional panels, dialogue input and delivered transcript have different owners | Defer: separate UI and conversation delivery scope needed. Never play the combined audition on both buses. |
| M01 exploration | No production music scheduler | Defer to the separate unfinished restrained-music roadmap item. |
| M02 shakuhachi rest | No authored rest/music trigger | Defer with M01. Acceptance of this excerpt supplies no rest or disease-pause semantics. |

## Proposed regional playback contract

Add `OpeningRegionMaterialAudioController` after the sources and mixer are ready.
Read the exact current region snapshot and projection; convert Seeker and anchor
positions into the same coordinate space. Select the closest authored anchor by
squared distance, resolving an exact tie by ordinal location ID. Find its region
through `OpeningRegionEnvironmentalRegionLookup.FindByLocation` using the exact
retained location instance. Selection is **a presentation approximation between
landmarks**, not a new world location, visibility, visit, hearing or terrain fact.
Approval includes this audio-only selection rule. It requires no changed anchors,
collision shapes, navigation, routes or landmark bounds.

Start at the live Seeker position, and reevaluate on movement and current-snapshot
replacement, including return visits. Target one region at gain 1 and the other
three at zero. Ramp the four weights over **0.75 seconds**, from their current
values; interrupting a ramp retargets without restarting it from an old region.
Weights stay nonnegative and sum to at most 1. Use linear amplitude crossfades,
not equal-power amplification. Four non-positional players are the hard maximum;
each bed loops its entire unchanged 30-second waveform. This is regional ambience,
without source bearing, occlusion, off-screen clue eligibility or a second rain bed.

Do not restart a bed on every frame, gain change or loop. Preserve loop phase
through mute/unmute. Set loop metadata at playback/import, without editing WAV
samples. Retain existing decoder precision limitations and measure the actual
engine output at seams; neither numerical policy compliance nor a smooth runtime
seam is claimed by this planning document.

The existing inscription variation replaces the **whole woodland region** sound
state with `inscription-revealed-sound` after authorized presentation. Map that key
to the same B03 bytes at **−3 dB emitter gain**, ramped over 0.75 seconds. This is
the proposed concrete interpretation of the existing authored “rain thins” state,
and is included in the requested grant. Do not create a proximity beacon, expose
the inscription name in captions, or install the variation from audio. Unknown
sound keys, nonfinite positions, mismatched topology or missing resources fail
quietly with a diagnostic; they never substitute new world facts or block actions.

## Proposed action and caption contract

Add a presentation notification after a successful exact Grinding advance in
`OpeningRegionRemedyPreparationInputCoordinator.TryProgress`; include the retained
recipe/step and station position. Starting, heating, steeping, cooling, completing
the activity, adding a batch, failed requests and repeated inputs emit no A04.
Add a notification after successful `OpeningRegionFirstPatientAdministrationInputCoordinator.TryAdminister`,
carrying the retained patient binding. Play the complete accepted eight-second
A04 or A05 waveform once, at emitter gain 1, on Effects. Their initial silence,
spacing and tails remain accepted sequence timing; they are feedback passages,
not a new eight-second gameplay action. Do not slice contacts, normalize, add
pitch changes or resynthesize. Each cue owns one player, maximum two; no queue
of stale action sounds. Repeated unsuccessful requests cannot restart playback.

Playback and caption failures cannot undo, deny, delay or throw through a committed
game action. Notifications are transient and never stored in world saves or event
logs. Add a presentation reset notification after successful `SaveSnapshotRestorationController.TryRestore`:
stop action cues, clear captions and end their own leases, without emitting success
cues from restored history. Recompute ambience from current live sources. Failed
restoration produces neither reset nor cue. Teardown disconnects handlers and
ends owned leases. No replay of old action feedback at startup or load.

An audible A05 takes one existing `AudioForeground.ConsequentialCue` lease while
playing, ending on Finished, cancellation, missing resource, reset or teardown.
A04 does not acquire a foreground lease. If Effects or Master is zero/muted, A05
does not acquire an audible-foreground lease; muting during it ends that lease,
and unmuting does not acquire another for the same cue. No speech/reading producer
or new ducking policy is added. Effects remains exempt from global ducking.

Add `MaterialSoundCaptionPanel` on a CanvasLayer as two noninteractive, bounded,
readable lines that do not take focus or obstruct existing controls at 1280×720.
`AudioSettingsController.Preferences.Captions` gates only these optional captions.
Caption delivery depends on the same selected presentation/committed action,
independently of user gains and mutes. Existing dialogue and preparation/treatment
status text stays available when captions are off.

Regional text, updated on selected state change rather than every loop: “Sheltered
rain”, “Running water and rain”, “Rain in leaves”, “Wind, rain and timber creak”; the
revealed woodland variant uses “Rain in leaves, quieter”. Action text persists
for the eight-second cue interval: “Grinding” or “Treatment handling”. No hidden
names, locations, diagnoses, characters, direction arrows or new discovered facts.
Keep one current regional line and one latest action line; caption toggle/reset
clears them promptly. The later view check establishes placement and legibility.

## Assets, rights and exact implementation files

Copy only B01.wav, B02.wav, B03.wav, B04.wav, A04.wav and A05.wav byte-for-byte from
`audio-direction/material-audition/clips/` to `game/assets/audio/material/`.
Check them against terminal acceptance, not stale frozen candidate status fields.
Retain hashes, import settings and decoded engine format in a new runtime manifest.
No source download, purchase, accepted edit, original container or unused candidate
is needed in the game export.

Create `game/assets/audio/material/CREDITS.txt` and `provenance.json` from retained
provenance and rights records, without altering their frozen predecessors. Include
the source titles, authors, original source and licence URLs, and existing edit
notices for every contributor to these six files. B02 includes kurt's Stream Sounds
under recorded CC BY 3.0; B03 includes Alexander's Rain On Dry Leaves Sound under
recorded CC BY 4.0. Other selected sources are recorded CC0. Preserve Bashar's
supplied credit. These are retained licensing records, not a new rights audit.
Package the two attribution licence notices and CC0 notice alongside the runtime
credits and explicitly include text/JSON/HTML notices in exports. Provide an
“Audio source credits” button in the existing Audio panel opening a readable
scrolling view with source links; verify the exported files and visible credits.
No misleading pending-selection wording or rejected M02 guitar enters runtime credits.

| Files | Bounded change after authority |
| --- | --- |
| `game/Main.tscn` | Add material controller and caption scene references/nodes only; retain existing art, physical owners and ordering dependencies. |
| `game/OpeningRegionMaterialAudioController.cs` and `.uid` | New transient player orchestration, exact live-source lookup, ramps, action subscriptions and lease cleanup. |
| `game/MaterialSoundCaptionPanel.cs`, `.uid`, `.tscn` | New noninteractive caption presentation. |
| `game/OpeningRegionRemedyPreparationInputCoordinator.cs` | Success notification at the exact Grinding branch only. |
| `game/OpeningRegionFirstPatientAdministrationInputCoordinator.cs` | Success notification after the existing administration result only. |
| `game/SaveSnapshotRestorationController.cs` | Presentation reset notification after a complete successful restoration only. |
| `src/Application/Audio/OpeningRegionMaterialAudioProjection.cs` | New pure selection/mapping and bounded ramp/cue contracts using existing domain inputs; no Godot dependency. |
| `game/AudioSettingsPanel.cs` | Activate truthful caption help and add credits entry without changing gains, storage or mixer semantics. |
| `game/MaterialAudioCreditsPanel.cs`, `.uid`, `.tscn` | Readable source-credit view owned by the Audio panel, with working return/focus. |
| `game/assets/audio/material/` | Exactly six WAVs, their generated `.import` metadata, credits, provenance and three retained licence notices. No Godot cache. |
| `game/export_presets.cfg` | Add only `assets/audio/material/*.txt`, `*.json` and `*.html` notice include filters to the three existing development presets; retain all other preset settings. |
| `tests/Application.Tests/Audio/OpeningRegionMaterialAudioProjectionTests.cs` | Meaningful selection, malformed/exact-identity inputs, retargeting and event eligibility tests. |
| `game/MaterialAudioRuntimeSmokeCheck.cs`, `.uid`; `game/MaterialAudioRuntimeProof.cs`, `.uid`, `.tscn` | Main wiring/action/load negative probes and stopped-by-default disposable output proof. |
| `scripts/validate-material-audio-runtime.py`; `scripts/check-material-audio-output.py` | Successor boundary/hash/export checks and final-output analysis; do not rewrite predecessor validators. |
| `audio-direction/material-runtime-authority.json`, `material-runtime-verification.json`, `material-runtime-review/` | Record actual owner grant and scoped evidence; review files are diagnostics, not shipping assets. |
| `CONTINUATION.json`, `HANDOVER.md`, `ROADMAP.md` | Scope/verification maintenance. Leave material, positional-cue and music items unchecked. |

Implicit new prerequisites are excluded: world material classification, movement or
gait changes, positional emitters, NPC audio, meaningful off-screen cues, water/tool
actions, UI/conversation sound, production music, voice/providers, microphones,
new save schema, time/disease/perception changes, artwork/layout changes, release,
signing, purchases and Git publication. `game/project.godot` remains untouched.
Use the existing development-export script for disposable packaging checks only;
its unsigned outputs are validation evidence, without release authority.

For later terrain work, retain W01–W04 byte-identical. Each ten-second audition has
six separated contacts; it must not run as a free-running movement loop. A later
explicit terrain scope must define surface coverage, material precedence and
unmapped behavior, actual displacement, grounded contact/cadence, fixed leg identity
where animation supplies it, and teleport/load/blocked/stop cancellation. Any derived
contact resources need a separately recorded extraction boundary and sample parity
to accepted PCM; acceptance of the audition alone does not authorize those edits.

## Finite verification and completion

This planning change needs only continuation/self-test, whitespace and structured
inventory/hash checks. Product inputs are unchanged; reuse passing product evidence.
The later implementation must supply all of the following before closing its slice:

1. Verify six installed WAV hashes against terminal acceptance, retained input
   snapshots and full closed-scope evidence. Check additive Main composition,
   unchanged physical/art owners, unchanged mixer and untouched local editor rewrite.
   New validator rejects replaced assets, double routing and added world ownership.
2. Exercise all eleven anchors, deterministic midpoint ties, return visits, rapid
   retargeting, invalid coordinates/topology, startup and reset. Confirm bounded
   nonnegative weights, no stacked rain and exactly the existing revealed-state mapping.
3. Exercise successful grinding and treatment through the **ordinary coordinators**;
   negative cases include failed/stale/remote requests, other steps, repeated treatment,
   batch completion and restored history. Assert identical gameplay results/event
   counts/time/perception with audio enabled, muted, missing and captions disabled.
4. Render final Master output in a disposable project with byte-identical installed
   clips and mixer: four beds including loop seams, crossfades/retargets/revealed
   attenuation, both one-shot sequences, independent Effects/Ambience gain/zero/mute,
   Master zero/mute, mono, Default/Reduced range and A05 lease release including
   mute/reset/teardown. Measure level/true peak/LRA and record deviations honestly.
   Retained audition measurements are not a measurement of this new runtime mix.
5. Inspect actual Main captions, successful-action view and credits at base and
   expanded viewport; verify focus/return and caption toggle with muted output.
   Test the exported resource inventory for six clips, provenance and readable
   attribution/licence notices. Record environment/device/listening limitations;
   offer the concrete changed runtime for applicable view confirmation without
   reopening accepted sound selection or unchanged artwork.
6. Run restore, format, build, four unit-test projects, ScenarioValidator and the
   retained Godot smoke suite plus the new material Main checks. Use the new
   successor boundary validator before the retained `scripts/quality-audio.sh`
   product commands. Existing publication validators intentionally bind their
   old snapshots and cannot bless new source files. Do not weaken them or frozen
   art validators. Graphical movie output and sandbox retries follow the existing
   host constraints. Record commands and actual results in runtime verification.

Completion is six unchanged assets integrated with the above live triggers,
captions, attribution, working existing controls and passing successor/product/output
checks. This is not completion of terrain footsteps, positional ambience, music,
the whole audio identity or production device validation.

## Single implementation decision

The next decision is approval to implement **`phase9-material-audio-runtime-integration`**,
exactly the six-sound boundary, nearest-anchor presentation rule, revealed-state
attenuation, success/reset notifications, captions, credits and verification defined
here. The handover explicitly says new runtime integration needs its own concrete
scope; current listening acceptance expressly retains that separation. Record the
owner's actual response in `material-runtime-authority.json` before implementation.
Until then the proposal is frozen for review, and no runtime change is authorized.
