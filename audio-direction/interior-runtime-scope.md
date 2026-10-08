# Interior audio runtime proposal

Prepared 2026-10-09 · `phase9-interior-audio-runtime-scope` · planning complete.
The new implementation grant is pending. The [structured proposal](interior-runtime-scope.json)
binds this document, the inspected owners, reused resources and protected product snapshot.

The proposed first interior model covers **the existing sickhouse and abandoned laundry**.
Exterior water, foliage and wind become quieter inside; local walking gains a timber
sound in the sickhouse and an earth sound in the laundry. It uses installed WAVs and
the existing Seeker contact cadence. B01 sheltered rain remains at its existing gain.
This is a bounded listener enclosure/material presentation, not measured room acoustics.
Reverb, wall transmission, filtering, doors, roof geometry and new interactions are deferred.

## Current owners and the missing deliverable

| Live owner | Existing fact and proposed observation |
| --- | --- |
| `OpeningRegionFirstPatientSceneSource.Layout` | Collision-aware sickhouse room, one north entrance and exact patient binding. Observe its projection, bounds, entrance and wall thickness. |
| `OpeningRegionStreamLaundrySceneSource.Layout` | Inert laundry shell with two route-derived openings. Observe its scene binding, bounds and openings; no laundry action exists. |
| `OpeningRegionProjectionSource` / `GreyboxNavigation` | Projection-local coordinates and current supported navigation. Retain identity and transform checks; do not edit the surface. |
| `Seeker.MotionCompleted` | Actual post-collision displacement already drives `FootstepContactCadence`. Preserve that producer and cadence. |
| `OpeningRegionFootstepMaterialSource.Select` | Currently excludes both complete structure rectangles. Add a separately scoped interior override before its unchanged exterior projection. |
| `OpeningRegionMaterialAudioController` | Owns B01/B03/B04 players and the existing regional ramp. Apply one extra interior multiplier at each bed player. |
| `OpeningRegionPositionalAudioController` | Sole B02 player, listener and admitted recorded-reply cue. Apply the multiplier to B02 only, after the existing regional weight; preserve source, pan, distance, reply and captions/history. |
| `AudioSettingsController` / `MaterialSoundCaptionPanel` | Existing independent controls and perception-safe captions. Keep bus behavior and add one generic interior caption line. |
| `SaveSnapshotRestorationController.PresentationReset` | Successful-load presentation reset. Reset the new transient model; failed load changes nothing. |

Nearest-anchor shelter selection is not a room membership test. Neither accepted artwork,
its floor colouring nor existing footstep exclusions establish a floor material or roof.
The two material assignments and two attenuation profiles below are new proposed audio
authority. They create no persisted world material, indoor knowledge or weather immunity.
Shrine, wagon, garden and square have no room owner in this scope and remain unchanged.

## Exact structures and supported floor regions

All coordinates are in the retained 3200 × 1800 opening-region projection. The source
must retain the exact live projection/layout references and verify these prerequisites.
Read portals from live layouts; decimal values below are explanations, not substitute
hardcoded portal coordinates. Derived float comparisons may use at most 0.01 pixel tolerance.

| Proposed profile | Live shell / openings | New audio material and full-depth external-bed attenuation |
| --- | --- | --- |
| Sickhouse | Bounds `[2480,1310,2800,1502]`; north entrance `(2528,1310)`, width 64; walls 8 | W04 timber contacts; B01 0 dB; B02/B03/B04 −9 dB |
| Abandoned laundry | Bounds `[1420,120,1580,360]`; west opening `(1420,240.8)`, east opening approximately `(1580,294.6667)`, each width 96; walls 8 | W01 earth contacts; B01 0 dB; B02/B03/B04 −3 dB |

The laundry openings are derived from the actual projected routes. The geometry constant
`LaundryOpeningY=220` is not the current live opening Y: this layout uses the projected
bridge branch, since its bridge approach `(1748.5714,452)` is not cardinal to `(1800,500)`.
The west route connects `(1500,220)` to `(1000,350)` and the east route to `(1800,500)`.
Preserve both branches of the layout implementation; only the current geometry is authorized.

Floor eligibility requires finite projection-local position, matching live prerequisites
and the existing navigation support test (closest point within one projection pixel).
Use the **strict interior** of each shell inset by its eight-pixel wall thickness:
sickhouse `(2488,1318,2792,1494)`, laundry `(1428,128,1572,352)`.
Add these closed-depth, open-width portal strips to bridge the inset to the outer threshold:

- Sickhouse: `2496 < x < 2560`, `1310 <= y <= 1318`.
- Laundry west: `1420 <= x <= 1428`, `westY−48 < y < westY+48`.
- Laundry east: `1572 <= x <= 1580`, `eastY−48 < y < eastY+48`.

These are conservative audio regions, not replacement collision geometry. Elsewhere inside
or on either shell rectangle, including wall collars and portal end points, keep footstep
silence. Outside both shells, delegate to the unchanged exterior selector. Unsupported or
invalid position remains silent for footsteps. New floor sounds start only at an eligible
modeled contact; entering, loading or standing in a room creates no sound event.

The sickhouse timber and laundry earth assignments are deliberately limited sound-design
defaults for these footprints. They do not install boards, debris, soil, art or collision.
The owner may revise either assignment before granting the single implementation scope.

## Enclosure blend and player composition

Introduce a pure `OpeningRegionInteriorAudioProjection` and one transient
`InteriorAudioRamp`. A new `OpeningRegionInteriorAudioSource` owns the shared current
snapshot. It reads layouts and the Seeker; it never writes a Domain, save or provider object.
Use the same floor eligibility query for the listener and for contact-material override.

At an eligible interior point, find the shortest Euclidean distance `d` to any of that
room's **opening segments**, using their live width and centre. Define interior blend
`b = clamp(d / 32, 0, 1)`. At the doorway plane within an opening, `b=0`; at least
32 pixels from every opening, `b=1`. Wall distance creates no leakage or source.
Exterior, unsupported and invalid bindings target neutral `b=0`, with no interior caption.
This is an artistic enclosure curve, not physical distance attenuation of a new emitter.
On an ordinary exit, gains return to unity within at most 0.75 seconds; this short exit
transition is included in the proposed scope. After settling, exterior output is unchanged.

For each bed, let its table value be `a` dB and compute target amplitude
`g = 10^(a*b/20)`. Thus B01 stays exactly 1; sickhouse full-depth B02/B03/B04 reach
approximately 0.354813; laundry full-depth values reach approximately 0.707946.
All gains stay in `[0,1]`. Existing regional selection, environmental variation and
0.75-second regional retargeting are unchanged.

Advance the shared four-gain ramp exactly once per process frame before consumers
(`ProcessPriority=-50` on the new source). Each finite nonnegative delta up to 0.25 seconds
moves each gain toward its target by at most `delta/0.75` amplitude units, clamped at
the target. This bounded slew handles doorway jitter without restarting a fade every frame.
Store target blend/profile separately for captions. Negative/nonfinite delta, a delta
above 0.25, missing/replaced owners or navigation, pause, disabled Seeker, focus loss,
successful load and a world-position jump over 96 pixels reset the gains immediately to
1 and clear the caption. Once valid active observations resume, recompute from position;
there is no catch-up, entry cue, sound replay or persisted indoor state. Teardown unsubscribes.

Ordinary bed output becomes `regional weight × interior gain × existing spatial gain
(B02 only) × existing bus processing`. Apply the new gain once: B01/B03/B04 at their
material players; B02 at its sole positional player. Never scale the forwarded regional
weight as well as the B02 player. Do not add a second B02 player, bus, listener, effect,
ducking lease or category setting. A04/A05, recorded-reply indication, Speech, Music and
local footsteps retain their existing player gain and scheduling. Interior changes do
not become regional transitions or new music opportunities.

The new source defaults to `Automatic=false` outside ordinary Main. Existing stopped
proofs receive neutral gain when no interior source is installed. Diagnostic overrides
are available only in the new stopped interior proof. Missing resources/buses retain the
existing silent-output behavior; captions and gameplay remain available. A bad interior
owner falls back to neutral ambience and the original silent interior footstep behavior.

## Accessibility, assets and rights

Add one independent noninteractive caption line, **“Outdoor sounds softened”**, visible
only for an eligible profile with target `b>=0.5` and captions enabled. It names no room,
roof, NPC, clue or hidden action. The existing regional, contact, action and conversation
lines keep their owners and wording; the new source clears only its own line.
Captions remain independent of Master/category mute and gain. Check the fifth line at
1280 × 720 and 1920 × 1080, including long existing captions, credits/history and keyboard
return. The source adds no cue history entry, perception event, map visit or journal record.

Reuse B01–B04 at `game/assets/audio/material/` and W01-01…06 / W04-01…06 at
`game/assets/audio/footsteps/`, with their installed import settings, existing provenance,
credits and licences. **Zero new WAVs, PCM ranges, downloads, generation or importer edits.**
Keep current material/footstep credits and export inclusion patterns. A short credits-panel
explanation may name reuse for interior footsteps; do not rewrite frozen provenance/credits.
No new contributor, licence claim, new audition or unchanged-byte acceptance is required.

## Exact implementation files after authority

| File | Finite proposed change |
| --- | --- |
| `src/Application/Audio/OpeningRegionInteriorAudioProjection.cs` (new) | Immutable two-room descriptors/snapshots, supported floor/portal query, interior material override, enclosure target and named constants. |
| `src/Application/Audio/InteriorAudioRamp.cs` (new) | Four bounded gains, one-frame slew and neutral reset; presentation time only. |
| `game/OpeningRegionInteriorAudioSource.cs` (new) | Exact owner/navigation/transform checks, ordered shared snapshot, local material query, cancellation and caption ownership; defaults stopped. |
| `game/Main.tscn` | Register one new source with `Automatic=true`, priority −50; existing nodes/art/positions/collision are unchanged. |
| `game/OpeningRegionFootstepMaterialSource.cs` | Use validated interior override for eligible floor points; retain exterior selector and existing support checks. |
| `game/OpeningRegionMaterialAudioController.cs` | Multiply its three bed gains once; optional neutral source for legacy proof scenes. |
| `game/OpeningRegionPositionalAudioController.cs` | Multiply only B02 player gain once in live and new diagnostic output; preserve spatial/reply authority. |
| `game/MaterialSoundCaptionPanel.cs` | Add independent interior line/set/clear and accommodate five lines without changing existing ownership. |
| `game/MaterialAudioCreditsPanel.cs` | Explain existing W01/W04 reuse; existing notice paths stay intact. |
| `tests/Application.Tests/Audio/OpeningRegionInteriorAudioProjectionTests.cs` (new) | Boundaries, live descriptors, unsupported/foreign input, interpolation and material delegation. |
| `tests/Application.Tests/Audio/InteriorAudioRampTests.cs` (new) | Bounds, exact targets, jitter, partitioned time, invalid delta and resets. |
| `game/InteriorAudioRuntimeSmokeCheck.cs` (new) | Ordinary Main structures, contacts, cancellation, captions, save/transcript parity and failure isolation. |
| `game/InteriorAudioRuntimeProof.cs`, `game/InteriorAudioRuntimeProof.tscn` (new) | Stopped, disposable installed-player captures; never commits gameplay. |
| `scripts/check-interior-audio-output.py` (new) | Measure proposed attenuation, output isolation and cancellation from captured Master PCM. |
| `scripts/validate-interior-audio-runtime.py` (new) | Successor-local inputs/resources, output evidence, preserved predecessor hashes and rejection probes. |

Godot-generated C# UIDs accompany the three new game scripts under the existing ignored-UID
policy. No new asset folder, `export_presets.cfg`, bus layout, Seeker, cadence, original
exterior material projection, geometry/layout owner, save, hearing or CI input is proposed.
Retain implementation authority in `audio-direction/interior-runtime-authority.json`,
verification in `audio-direction/interior-runtime-verification.json`, and detailed evidence
under `audio-direction/interior-runtime-review/`. Update the ledger/handover and remaining
roadmap commentary after implementation; leave the wider material item unchecked.

## Finite implementation verification

1. Test both supported inner floors, all three exact doorway segments/strips, shell and
   inset edges, epsilon neighbours, wall collars, foreign/replaced layouts/transforms,
   NaN/infinity, unsupported navigation and exterior delegation. Test `b=0,0.5,1`, gain
   composition, monotone slew, frame partitioning and each reset. No synthesized room
   entry event or standing contact is admitted.
2. Run retained product format/build, all four unit-test projects, scenario validation
   and `scripts/smoke-godot.sh`, plus the new interior smoke. Exercise the existing sickhouse
   approach and both laundry route openings/return paths; preserve five sickhouse wall
   shapes, six laundry boundary sections and eleven anchors/twelve connections. Do not
   alter frozen predecessor validators/allowlists. Reuse unchanged accepted art evidence.
3. Capture the actual installed Master output: each B01–B04 alone at neutral, half-depth
   and full-depth in each profile (24 cases). Add two doorway sweeps with jitter, two
   reset/owner-loss cases, two indoor contact cases (W04/W01), one unsupported wall case,
   one overlap of ambience/contact/A05/reply/M02, four Ambience/Effects/Music/Master mute
   isolation cases, one mono case, and both Default/Reduced-range mixes (15 more cases;
   **39 total**). For steady linear Default-range bed cases, compare relative RMS with
   `a*b` within 0.25 dB; B01 must remain unchanged. For nonlinear preset/full mixes,
   record measurements and clipping/behavior rather than infer the same RMS ratio.
4. Verify ordinary Main save **and transcript** byte parity in default, muted, all-zero
   and mono/reduced modes through the same traversal/action sequence. Test successful
   and failed restore, pause/resume, focus loss, disabling, teleport, removal, missing
   resources/bus and teardown; no late step, leaked caption or change to hearing/world time.
5. Inspect both viewports with all five captions, credits/history, keyboard return and
   muted output. Confirm admitted reply/history, A04/A05 ducking and optional music
   scheduling remain unchanged. Measure equality outside both footprints after the bounded
   exit transition settles.
6. Verify all reused resource/import/provenance bytes, existing notices and archive
   resource packaging. Keep the executable-export/template and device-listening limits
   explicit. Bind evidence and use a successor validator with at least four intentional
   rejection probes: altered proposal/resource, changed protected geometry, duplicate
   B02 ownership and missing measured-output evidence.

## Planning completion and the one decision

This proposal completes the missing planning deliverable. It adds no runtime or PCM.
Planning verification checks proposal/input hashes, protected product bytes, continuation
closures and authored whitespace; existing passing product evidence is reused for prose.
The structured inventory records the inspected commit and local editor exception.

**One new scope decision:** authorize `phase9-interior-audio-runtime-integration` exactly
as above: the two audio floor assignments, listener enclosure gains, doorway transitions,
existing-resource footsteps, shared source, generic caption and finite implementation checks.
Record the owner's response against both proposal hashes before implementing. Authority is
required by [AGENTS.md](../AGENTS.md): “A genuinely new runtime/layout/physical scope still
needs the applicable authority.” Standing commit/push authority covers this completed
proposal; it does not grant the new runtime/material model.

No physical/layout change is needed for this proposed slice. Requests for different room
extents, altered portals, roof/floor artwork, new collision or a physical material simulation
would change this proposal and need their own concrete authority. Full room acoustics,
NPC steps, water/tools/actions, voice, rest gameplay, new listening and release stay outside
this finite scope. The completed shelter, terrain, positional, music and accepted art scopes
stay closed.
