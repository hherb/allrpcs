# Allrpcs

Allrpcs is a Godot 4.6 .NET game under development. Its bounded-camera movement, navigation,
deterministic healer-command, and interaction-focus greybox keeps testable movement, framing,
navigation-request, follow-target, command-selection, and focus-selection rules plus future
authoritative rules in engine-independent C# projects while Godot owns input mapping, physics
presentation, camera behavior, collision, navigation topology, and scene authoring.

Phase 9.1 completed human approval of the painterly storybook Direction A after a four-way preview
comparison, with Direction D retained only as a feasibility fallback. The comparison, approved
refinement, bounded art bible, exact generation prompts, provenance, and review tradeoffs are
recorded in [`art-direction/representative-scene`](art-direction/representative-scene/README.md).
None of these previews is imported into Godot or treated as production art.

The first bounded Phase 9.2 source-sheet proof decomposes that direction into terrain/path, plants,
object-state cues, and a four-direction Seeker walk sample under
[`art-direction/production-proof`](art-direction/production-proof/README.md). Its documented alpha,
seam, pivot, baseline, and editable-master gaps led to a corrected individual-asset pipeline. The
corrected proof now has 75 manifest-bound exports, twenty-six layered masters, exact seam and
grayscale checks, four normalized six-frame walks, four two-frame idle/breathe loops, and four
four-frame reach-and-inspect gestures. The terrain set includes one exact 96-pixel straight module and one
90-degree bend, T-junction, cross-junction, rounded end-cap, one 512-pixel live-angle opening
junction, one tailored 512-pixel Village Square junction with matching ports, and one exact
124x96 Village Edge–Square connector, one accepted 200x220 Square–Garden continuation, one accepted 380x360 Square–Sickhouse
continuation, plus one
deterministic connected-topology review, one repeatable woodland leaf-litter tile, and exact
quiet-moss-to-leaf straight, two outer-corner, two inset-corner transitions, and one world-aligned
1024x512 full-clearing feather overlay. Three additional feasibility-only modules provide one
repeatable 128x96 Stream-water fill, one mirrorable 128x32 bank edge, and one reusable 32x32
bank-opening cap. Six external views bind them to the exact retained 400x96 water band and 128x96
crossing at 0.8x, 1.0x, and 2.0x without changing `Main.tscn`, runtime art, or physical authority.
The product owner approved the six supplied feasibility views without requesting changes. That
acceptance applied only to the external water, bank, opening-cap, crossing, and route-framing
presentation. The owner then separately approved the exact bounded runtime promotion. Three
runtime-owned copies now cover the retained 400x96 band and banks at their original hashes; the
centered 128x96 crossing remains above the water, and the 200-pixel collision opening stays clear.
The product owner said the supplied six ordinary-scene 0.8x, 1.0x, and 2.0x wide/tall Compatibility
views look right and requested no correction. This is bounded visual acceptance; it does not
establish pacing, usability, or human accessibility acceptance. All 75 exports have an isolated
source-parity Godot import scene with 12 named animations. A separate external terrain review
binds quiet moss, leaf litter, packed earth, and all five transitions to exact original,
grayscale, protanopia, deuteranopia, and tritanopia views plus descriptive luminance and local-texture
measurements. No proof-directory path is referenced by `Main.tscn` or ordinary gameplay, and the
simulation evidence is not human accessibility acceptance.

The separately approved Square–Garden path promotion presents the accepted world-aligned
200x220 continuation at `(2300,1250)`, `z=-71`, replacing only its greybox presentation line.
Its layered master and independent imports preserve exact accepted pixels and all physical
authority. The owner accepted six ordinary-scene views matching the external correction;
see the [runtime result](art-direction/production-proof/corrected/review/square-garden-path-runtime-result.md).

An external-only [Square–Sickhouse feasibility proof](art-direction/production-proof/corrected/review/square-sickhouse-path-feasibility.md)
retains six failed straight-strip views and six world-aligned correction views. The correction
uses only the existing straight-path pixels, matches 1,713 junction-overlap pixels exactly, and
preserves the Sickhouse endpoint and world-marker layering. The owner accepted all six corrected
external views on 2026-10-02. The owner approved its named promotion scope on 2026-10-02. One exact runtime-owned
380x360 sprite now replaces only its presentation line at `(2510,1320)`, `z=-71`. All six runtime
views exactly match the accepted external images; the owner approved the runtime views on 2026-10-02.
See the [runtime result](art-direction/production-proof/corrected/review/square-sickhouse-path-runtime-result.md).

An external-only [Square–Southern Road feasibility proof](art-direction/production-proof/corrected/review/square-southern-road-path-feasibility.md)
retains six failed strip views and six world-aligned correction views. The correction matches
1,662 junction-overlap pixels exactly; its flat endpoint across the untextured road remains a
visible limitation. The owner approved all six external views and the separate named promotion
on 2026-10-02. One exact 520x416 runtime-owned sprite now replaces only its presentation line
at `(1700,1232)`, `z=-71`, with repeat disabled. All six runtime views match the accepted images;
the owner approved the ordinary-scene views after display on 2026-10-02. Physical/gameplay
authority is unchanged.
See the [runtime result](art-direction/production-proof/corrected/review/square-southern-road-path-runtime-result.md).

The following [Southern Road–Wagon feasibility proof](art-direction/production-proof/corrected/review/southern-road-wagon-path-feasibility.md)
retains six external-only failed strip views. The straight physical route differs from the bent
road presentation, leaving exposed shoulders and unresolved junction coverage. No runtime or
production file changed. A [world-aligned correction](art-direction/production-proof/corrected/review/southern-road-wagon-path-correction.md)
now provides six external views with bent-road coverage and documented disposable fill
substitution. The owner visually approved these views except for a left-side overshoot. An
[external tip trim](art-direction/production-proof/corrected/review/southern-road-wagon-path-trim.md)
shortens the junction extension from 48 to 16 pixels. The owner visually accepted the trim
on 2026-10-02 and separately approved the named `terrain.southern_road_wagon_path` promotion.
The exact accepted overlay is now manifest-bound with a layered master and independent
lossless imports. Ordinary startup uses the same bounded presentation substitutions;
[all six runtime views](art-direction/production-proof/corrected/review/southern-road-wagon-path-runtime-result.md)
match the accepted external PNG bytes. The owner accepted the ordinary runtime views
on 2026-10-02.

An external-only [Shrine–Southern Road strip proof](art-direction/production-proof/corrected/review/shrine-southern-road-path-feasibility.md)
now retains six failed feasibility views. Greybox fringes, brown-fill darkening, and a
20.384-pixel shrine-exit centerline offset prevent promotion. No runtime or production file
changed. A subsequent [external correction](art-direction/production-proof/corrected/review/shrine-southern-road-path-correction.md)
now provides 18 midpoint, shrine-exit, and junction views. It removes the route fringe and
brown-fill darkening, aligns the entry with the physical route/opening, and preserves accepted
overlays and all authority. A thin junction seam remains visible at 2.0x; owner review and any
required external seam correction precede a separately approved named promotion.

The first nine bounded runtime terrain promotions copy only the manifest-bound
`terrain.quiet_moss`, `terrain.woodland_leaf_litter`, straight
`terrain.woodland_leaf_litter_transition`, and two complementary outer-corner exports into
`game/art/runtime/terrain`, followed by one `terrain.straight_path` and its matching
`terrain.path_end_cap`, the exact `terrain.opening_junction`, then the tailored
`terrain.woodland_village_edge_transition`. Each has exact SHA-256 parity and
its own lossless, mipmap-free Godot import identity. `Main.tscn` repeats quiet moss across the full
3200-by-1800 world at
`z_index = -100` and leaf litter only across the existing 648-by-250 Woodland Clearing bounds at
`z_index = -90`. At `z_index = -80`, two 392-pixel rotated straight spans and four 256-pixel outer
corners form one rounded visual perimeter derived from those unchanged bounds. Its three-pixel
vertical overhang centers the corner diameter without changing collision, navigation, or leaf-tile
coverage. The 96x256 straight module repeats across only the exact Abandoned Laundry-to-Woodland
Clearing connection and meets its rounded 256x256 end cap at the clearing anchor. It and the end cap
draw at `z_index = -70`, above the retained eleven greybox routes at `-75` and below every opening,
label, and interaction marker. The junction uses its exact `(224, 224)` pivot at the Abandoned
Laundry anchor and a rigid `133.025066°` rotation to join the retained Stream leg to that approved
straight path without scaling, shearing, collision, navigation, or topology changes. The feather
overlay is world-aligned at `(536, 136)`, covers the exact clearing rectangle and 180-pixel Village
route continuation, and uses a deterministic irregular 64-pixel alpha feather to remove hard
moss/leaf intersections on every exposed side. It draws at `z_index = -79`, above the retained
perimeter and below route art, without changing the leaf-tile bounds. Static and credential-free
engine checks bind all nine source hashes, runtime paths, geometry, coverage,
repeat mode, draw order, and continued isolation from the proof-only directory.
The product owner accepted the quiet-moss appearance in supplied 1.0x and 2.0x views. That review
also exposed one pre-existing 2.0x overlap between the identified stone-marker description and a
yellow opening marker; the description now occupies a checked non-overlapping rectangle. The product
owner subsequently accepted the leaf-litter clearing's repetition, seams, scale/readability,
contrast, initial rectangular edge, and corrected label placement in supplied 0.8x, 1.0x, and 2.0x
wide and tall views. During the following north-transition review, the owner found the remaining
hard edges unnatural. The bounded correction reused the straight transition and promoted only the
two already manifest-bound outer corners to complete the rounded perimeter; the owner accepted the
supplied correction as "much better." The following bounded path slice promotes only the straight
module and matching rounded end cap on the Laundry-to-Clearing connection; after two bounded visual
corrections, the owner approved the supplied 0.8x, 1.0x, and 2.0x wide and tall final views. The
isolated Stream–Laundry–Woodland junction review subsequently replaced its hard composited elbow
with an alpha-aware, tangent-continuous 96-pixel curve and clipped preview continuations at the
module boundary. The product owner accepted the corrected proof's curve and texture continuity,
then separately authorized this exact runtime promotion. The owner accepted the resulting
ordinary-scene curve, texture continuity, apparent width, seams, and nearby label/marker readability
in supplied 0.8x, 1.0x, and 2.0x wide and tall runtime views. The next separately approved slice
added the tailored clearing feather after the owner rejected hard straight moss/leaf intersections;
the owner then identified and verified a second correction for the north/northeast edge before
accepting the final unobstructed and ordinary-scene views. That acceptance is limited to the visible
blend and does not establish pacing, usability, or human accessibility acceptance. Inset
corners, bends, junctions, other paths, water, structures, props, characters, UI systems, and
gameplay authority remain unchanged. The Stream–Laundry–Woodland junction follows the exact live
57.599282-degree turn and retains both 96-pixel connector seams and curved physical-route
containment; its editable master and isolated import proof remain outside ordinary runtime paths.

The first bounded runtime plant promotion separately copies only the manifest-bound
`plants.foliage_broadleaf` export into `game/art/runtime/plants`, preserving its exact 72x56 pixels,
SHA-256, lossless mipmap-free import, and authored `(36, 53)` planted-foot pivot. Six unscaled,
unrotated placements form two overlapping rows across the existing 176x104 Healer's Garden plot;
the retained preparation bench draws above them. They are presentation only and add no collision,
navigation, interaction, preparation, treatment, or gameplay authority. The product owner rejected
the first single-clump placement as insufficient coverage, then accepted the full-plot correction
in supplied 0.8x, 1.0x, and 2.0x wide and tall Compatibility views. That review establishes only
this garden-foliage presentation, not pacing, usability, or human accessibility acceptance.

The next bounded runtime promotion regenerates the retained ordinary preparation-table source and
its proof-only cue canvases at 96x96 logical pixels with exact 192x192 editable masters after the
product owner found the proposed 80x80 scale too small. Only the ordinary table is separately copied
to `game/art/runtime/objects`, preserving SHA-256
`d36d2f9d5341edd64f9eadac7053bf3847b63990523400698d78e90d246a3a35`, the `(48, 92)` planted-foot
pivot, and lossless mipmap-free import. It replaces the Healer's Garden greybox bench and mortar at
the same station footprint, above the accepted foliage, without changing preparation, interaction,
collision, navigation, or other gameplay authority. The owner accepted its scale and placement in
supplied 0.8x, 1.0x, and 2.0x wide and tall Compatibility views; that review does not establish
pacing, usability, or human accessibility acceptance.

The following separately approved plant promotion copies only the manifest-bound
`plants.herb_bed_empty` export into `game/art/runtime/plants`, preserving its exact 96x80 pixels,
SHA-256 `8ced13faba2ac9240977df0c62686939ffcd24a09d278006485c866b0a11f2fa`, the `(48, 77)`
planted-foot pivot, and lossless mipmap-free import. Two unscaled, unrotated instances replace only
the Healer's Garden greybox plot and bed-line visuals beneath the six accepted broadleaf plantings
and preparation table. They add no collision, navigation, interaction, remedy-preparation,
treatment, or gameplay authority. Six 0.8x, 1.0x, and 2.0x wide and tall Compatibility views are
available for product-owner review. The first review found the north-left and north-right foliage
crowns outside the bed rails; moving the complete north row 27 pixels south corrected that bounded
presentation without changing any asset or authority. The product owner accepted the corrected
wide and tall views; that acceptance does not establish pacing, usability, or human accessibility.

The next separately approved character promotion copies only the 24 manifest-bound Seeker walk
frames into `game/art/runtime/characters/seeker`. Four six-frame loops retain their exact 108x168
south/north and 144x168 west/east geometry, 120 ms timing, contact-foot pivots, source hashes, and
lossless mipmap-free imports. They replace only the Seeker's greybox body and facing marker; the
existing movement speed, collision, navigation, camera, controls, interaction, and gameplay
authority remain unchanged. Compatibility-rendered 0.8x, 1.0x, and 2.0x wide and tall views cover
all four directions. The product owner approved the presentation and found 2.0x playable and
enjoyable on the reviewed screen. That screen-specific evidence does not change the 1.0x camera
default or establish broader pacing, usability, or human-accessibility acceptance. Idle/breathe and
reach/inspect frames remain isolated proof resources.

The next separately approved character promotion copies only the eight manifest-bound Seeker
idle/breathe frames into the same runtime-owned directional set. Four two-frame 600 ms loops retain
their exact 108x168 south/north and 144x168 west/east geometry, contact-foot pivots, source hashes,
and lossless mipmap-free imports. While stationary, the Seeker now breathes in the last movement
direction; starting presentation remains south-facing. The accepted walk loops and all movement,
collision, navigation, camera, controls, interaction, and gameplay authority remain unchanged. The
product owner reviewed both frames in all four directions at 0.8x, 1.0x, and 2.0x in wide and tall
Compatibility views and found the presentation looked good. That acceptance does not establish
pacing, usability, human accessibility, or acceptance of another asset. Reach/inspect frames remain
isolated proof resources.

The next separately approved character promotion copies only the 16 manifest-bound Seeker
reach/inspect frames into the same runtime-owned directional set. Four non-looping four-frame
gestures retain exact 192x168 geometry, 160 ms timing, contact-foot pivots, source hashes, and
lossless mipmap-free imports. A gesture starts only after an ordinary focused-search resolution
successfully changes authoritative state, uses the retained facing, yields immediately to movement,
and returns to the matching idle after completion. Repeated, stale, rejected, and unchanged
resolutions remain visually inert; the presentation neither gates nor delays resolution. The
product owner reviewed all four frames in all four directions at 0.8x, 1.0x, and 2.0x in wide and
tall Compatibility views and accepted the presentation. That acceptance does not establish pacing,
usability, human accessibility, or acceptance of another asset.

The following approved feasibility-only slice checks the existing manifest-bound 256x256
packed-earth cross-junction against the four retained Village Square routes without importing it
into ordinary gameplay. Its exact 90-degree ports cannot match the live `51.340192`, `75.963757`,
`144.462322`, and `180` degree headings under any rigid placement: the minimax rotation still
leaves a `70.670096` degree worst mismatch. Six deterministic 0.8x, 1.0x, and 2.0x wide and tall
planning views make the mismatch visible while the validator binds the live and approved
coordinates, 96-pixel route width, manifest hash, dimensions, pivot, and exact review bytes.
`Main.tscn`, the greybox junction, collision, navigation, topology, camera behavior, and gameplay
authority remain unchanged. After separate product-owner approval to create one proof, a tailored
512x512 junction now provides exact 96-pixel connectors at all four live headings, a 1024x1024
editable master, deterministic route-band containment, and six external review views. After the
first review exposed visibly misaligned guide-only paths, the corrected views used exact textured
continuations and the product owner approved their alignment. The proof remains isolated and grants
no runtime or ordinary-scene authority. After separate approval, the exact manifest-bound 512x512
junction was copied into runtime ownership with its `(256, 256)` pivot centered at the live Village
Square anchor. It draws at `z_index = -70`; the four exact 96-pixel greybox centrelines now begin at
the module boundary and continue unchanged to Sickhouse, Healer's Garden, Village Edge, and Southern
Road. Collision, navigation, topology, camera, scenario, and gameplay authority remain unchanged.
The product owner accepted the resulting ordinary-scene presentation in supplied 0.8x, 1.0x, and
2.0x wide and tall Compatibility views. That acceptance does not establish pacing, usability, human
accessibility, or acceptance of another asset.

The approved [`world-layout-proposal`](art-direction/world-layout-proposal/README.md) now defines
the live 3200x1800 authored-region greybox reflow. It retains all eleven
opening locations, all twelve topology edges, the 240-pixel-per-second movement speed, 96-pixel path
language, and existing zoom tuning; documents route and screen-span distances, camera limits,
navigation consequences, and five empty-area content budgets; and includes labelled full-region,
1280x720, and 1280x827 diagrams. Its validator now proves full 96-pixel separation between every
independent route band, including focused near-miss regressions. The runtime binds the proposal's
exact SHA-256, world bounds, anchors, straight route network, camera limits, collision-backed
landmark openings, and intentionally closed north-west forest gate. The five content budgets remain
non-authoritative, and no location, topology edge, scenario, dialogue, clue, or gameplay rule changed.
The product-owner graphical checkpoint traversed every open connection, accepted the closed-gate
readability, Shrine-to-Southern-Road pacing, and camera framing across 0.8/1.0/2.0 zoom and varied
window shapes. It also exposed and verified corrections for route-mask-restricted Village Edge and
Sickhouse interiors plus a physically reachable but visually undiscoverable Stream; the authored
scene and runtime now share one visible stream band, crossing, and label.

The provider-neutral dialogue foundation retains exact in-world speech, audiences, declared act
types, requests, responses, intentions, commitments, and bounded state proposals. Its first pure
deterministic character behavior selects fixed accept, refuse, clarify, or conditionally-accept
responses solely from the declared act type. Immutable commitment progress advances only one
value-equal pending intention per observation. Character-turn activity can represent awaiting,
speaking, completed, and cancelled states, with pure cancellation, response-reception, and
completion eligibility and transition rules. A focused Godot controller owns one exact activity,
delegates those transitions, and exposes an explicit player cancellation action for awaiting and
speaking turns. Character-turn transcript entries structurally distinguish exact spoken deliveries
from exact non-wait physical intentions while continuing to project each delivery's exact utterance.
Immutable snapshots and a pure append transition preserve occurrence order
without treating response proposals as executed actions. A character-turn world-event occurrence
now retains an exact authoritative event together with one exact tagged transcript occurrence.
Pure eligibility and transition rules require that exact authored event, then atomically record its
identifier, apply its declared qualitative-time effect, and append the tagged occurrence while
retaining the exact occurrence in the result. Response-proposal execution remains later work.
The live greybox now derives the first in-range healer request from the exact occurred player event
using an explicitly supplied act type and starts one awaiting-response activity. A separate
credential-free temporary source can select the existing deterministic response for that exact
request without installing or presenting it.

The first live-model boundary remains provider-neutral through Application: one exact affordance
snapshot produces the bounded context entries shown to a model and validates the completed response
returned for that turn. The generation controller applies one safe text-free retry and synthesizes
caller cancellation or deadline timeout without allowing any terminal failure to carry a response.
Opt-in redacted diagnostics observe provider attempts plus controller-synthesized cancellation and
timeout through caller-owned sinks, time sources, and independent diagnostic cancellation. Fixed
content-free presentation states cover generating, completed, cancelled, timed-out, and failed
streams. The scene now owns one exact immutable stream lifecycle, projects bounded text deltas
separately from those content-free states, and applies asynchronously observed updates only on the
Godot main thread. Completed validated responses enter only their exact awaiting turn; explicit
player cancellation cancels both generation and turn, while timeout or failure cancels the pending
turn without fabricating dialogue. Ordinary heard healer requests now use the authored LLM
assignment, exact bounded input, and a scene model source that consumes one exact caller-supplied
model for the next generation or falls back to one stable offline model. A completed terminal flow
can reset to a fresh empty stream for a later exact request without retaining prior presentation or
content. Credential-free ordinary-path checks prove one validated target-free `Wait` response
speaks exactly once, a hallucinated target is rejected without speech or mutation, and caller
cancellation preserves only already-accepted stream text while cancelling the exact turn.

Infrastructure contains caller-composed OpenAI Responses, Anthropic Messages, and loopback-only
Ollama HTTP adapters with stateless strict structured output, bounded streaming completion, and
optional terminal token diagnostics. Immutable provider settings retain one explicit selection and
support immutable replacement, append, selection, and unselected-provider removal. They resolve only
a binding that shares the exact configured settings object. Redacted connection tests use that same
selection, and a strict versioned JSON boundary contains only canonical provider IDs and bounded
model names. Infrastructure factories compose each provider's exact model and tester over the same
caller-owned dependencies without credential, network, discovery, or scene input/output. The
ordinary scene loads one persisted selection into shared settings/composition state, while macOS
Keychain, Linux Secret Service, and Windows Credential Manager OpenAI credential stores are present.
Settings and credential UI remain explicit and do not save on edit. Anthropic credential storage
and generated actions beyond the single known herb-patch examination flow remain later work.

## Prerequisites

- A 64-bit .NET 8 SDK.
- The Godot 4.6.3 .NET editor, not the standard editor without C# support.
- `actionlint` 1.7.12 or newer for local GitHub Actions validation.

Both executables must be available to the shell as `dotnet` and `godot`. Verify them before
building:

```sh
dotnet --info
godot --version
```

The verified local versions for this foundation are .NET SDK 8.0.130 and Godot
4.6.3.stable.mono.official. Root `global.json` keeps `dotnet` in the .NET 8.0.1xx feature band
and permits only the latest installed patch in that band. The Godot SDK version is pinned in
`game/Allrpcs.Game.csproj`. To update the toolchain, change these pins and the prerequisite
versions together, then rerun the complete local quality sequence before committing.

## Local quality checks

From the repository root:

```sh
./scripts/quality.sh
```

The sequence restores packages serially, rejects C# formatting drift, builds with current
recommended .NET analysis and compiler warnings treated as errors, runs the domain, application,
infrastructure, and architecture tests, then exercises both headless and 60-frame graphical Godot
launches. The headless launches wait for the navigation map to synchronize; the interaction-focus
check also
requires the authored healer move destination to resolve onto that map before exercising input.
They prove a retained route through the stream crossing, reject representative empty-area targets,
and exercise healer follow, wait, move completion, invalid and excluded command rejection,
separation, and return to follow across the approved route network. A separate interaction-focus
launch proves nearest selection, authored-order
tie-breaking, focus changes, out-of-range clearing, hidden and disabled exclusions, distinct
authored object identifiers, exact focused identifier-instance retention, null and mismatched
world-object request rejection, and eligible focused-search starts through exact or distinct
value-equal world objects for both visible targets. The mapped F path resolves the focused identifier
through the world-object controller and rejects a missing object without invoking the start path or
changing surrounding state. The same launch proves focus-independent cancellation and
world-object-gated outcome-aware completion through the exact retained active requests, exact
outcome recording, explicit world-object-bound first-recorded Seeker outcome resolution,
mismatched-object rejection, target-only perception replacement, outcome consumption, unchanged
inputs and world-object state, missing-object completion and resolution failure-state identity,
activity-indicator presentation, and deterministic changed/unchanged result text for every
lifecycle action. Resolved-perception checks prove that each exact looked-up object's authored
description appears only for the exact identified visibility singleton, including through a
distinct value-equal object with replacement text. Null, absent, unrelated, and non-identified
inputs clear and hide the reused label without changing source objects, controller snapshots,
marker presentation, or lifecycle result text. The same lifecycle evidence reaches both authored
temporary descriptions and hides a missing object again. It also proves that
released, unrelated, movement, and healer input preserve all controller snapshots and the exact
prior text, and that the temporary focused-search source stably authors one
capability-only first-world-object context and one tool-only second-world-object context from the
existing world-object snapshot, retains their exact nested instances, fails explicitly for an
undersized snapshot, and forwards its exact aggregate instances through the input path. The
temporary Seeker perception-source launch proves exact candidate, Seeker, and world-object
identifier alignment, value-equal target lookup with exact source-identifier retention, explicit
missing-target failure, stable order, target-only `Unnoticed` seeding, and unchanged inputs and
surrounding state. The separate perception-controller launch proves the ordinary resolved scene
state, then isolates exact one-time source initialization, wrong-character, repeated, and null
rejection while preserving activity, outcome, and presentation state. The passive candidate-source
launch proves exact ordinary production and atomic resolution consume the one retained candidate,
change only its live perception target to `Noticed`, and leave source perception, focused-search,
and presentation state unchanged. The observation-reinterpretation controller launches prove empty
ownership, authoritative exact/value-equal production, stable first-eligible trigger-aware
resolution, target-only identification, complete-candidate consumption, fail-closed mismatches and
nulls, and unchanged passive-noticing, focused-search, and presentation state. Its source launch
proves exact character, second-world-object, and trigger-knowledge identifier retention plus the
ordinary one-time production/resolution path and source-state preservation. The
outcome-resolution-controller launch
proves value-equal recorded-outcome and matching value-equal world-object resolution, target-only
perception replacement, consumption, stable unrelated-entry order and instances, unchanged inputs,
and fail-closed mismatched and null rejection without changing activity or presentation. The
world-object-controller launch proves exact lookup of both scene-authored objects, including through
a value-equal distinct identifier, absent and uninitialized results, null rejection, stable exact
snapshot retention, and unchanged source, targets, focused-search state, and presentation. The
world-event source, controller, and startup-recording launches prove two independently authored,
stable events in source order, exact `greybox-ready` startup selection and recording, and unchanged
initial qualitative time and surrounding state. The focused-search world-event launch drives the
ordinary F/E/R input path against the authored herb patch. It proves start and completion leave the
event log and qualitative time unchanged, then successful resolution consumes the exact outcome,
identifies and presents the herb, records the exact `herb-patch-identified` event, and advances
qualitative time. Repeated or newly completed but ineligible resolution preserves the exact
focused-search, world-event, world-time, and presentation state. The world-time controller launch
retains its focused boundary checks.
The movement-boundary launch uses the existing Seeker body and authored collision shapes to prove
all four outer borders plus representative stream, village, and clearing collisions, then proves
the exact stream/laundry and village routes remain clear. The sequence first runs
`actionlint` against every hosted workflow. A focused-search presentation launch proves the one
temporary Seeker indicator is hidden by default, follows empty, unrelated, one-match, multi-match,
and hide-again decisions, and leaves supplied inputs intact. A separate controller launch proves
condition-aware capability-only, tool-only, and combined starts; fail-closed absent, unsatisfied,
already-active, and wrong-character starts; unrelated-entry handling; successful cancellation and
completion; presentation changes; and exact input and activity-instance retention. The checks do
not require provider credentials or network access after restore. The save-document storage launch
uses only a process-specific system-temporary file and proves exact persisted restoration, missing
and failed reads, malformed JSON rejection, replacement recovery, cleanup, and preservation of
unrelated scene state. The project selects the
Compatibility renderer for graphical launches.

To verify formatting alone without changing files, run:

```sh
dotnet format Allrpcs.sln --no-restore --verify-no-changes
```

The architecture tests inspect authored project files without loading Godot. They enforce the
project-reference directions listed below.

The quality sequence also runs the pure representative discovery-scenario validator. It exits
nonzero if its five ordered evidence definitions contain broken ownership or object references,
unavailable requirements, or evidence that cannot reach its required visibility through the
authored initial, capability, tool, passive-noticing, or knowledge-triggered routes:

```sh
dotnet run --project tools/ScenarioValidator/Allrpcs.ScenarioValidator.csproj --no-build --no-restore
```

## Credential-free hosted checks

`.github/workflows/ci.yml` configures pull-request and `main` checks on explicit stable runner
images:

- Pure formatting, zero-warning compilation, domain tests, Application movement and camera
  framing tests, and architecture tests run on `ubuntu-24.04` without installing or launching
  Godot.
- The headless main-scene smoke runs separately on `ubuntu-24.04` with Godot 4.6.3 .NET.
- Unsigned debug exports run natively on `ubuntu-24.04`, Apple Silicon `macos-15`, and
  `windows-2025`. Their targets are Linux x64, macOS Universal 2, and Windows x64.

The workflow grants only `contents: read`, disables checkout credential persistence, defines no
provider inputs or secrets, and uses no dependency cache. Uploaded evidence is named
`UNSIGNED-DEVELOPMENT-*`, retained for seven days, and includes a plain-text limitations
manifest. The macOS job rejects a bundle-level signature and the Windows job rejects an
Authenticode signature before upload. These debug outputs are neither signed releases nor
installers and are not approved for distribution.

Every external action is pinned to a full commit SHA with its verified release tag in a comment:
`actions/checkout` v7.0.1, `actions/setup-dotnet` v6.0.0,
`chickensoft-games/setup-godot` v2.4.2, and `actions/upload-artifact` v7.0.1. Before updating
them, recheck the [supported runner labels][github-runners], action release notes, and Godot
compatibility, update the SHA and tag comment together, then run the complete local quality
sequence.

The workflow was first verified on GitHub-hosted runners in private-repository push run
[`33310947250`][push-run] and pull-request run [`33311261187`][pull-request-run]. The exact
resolved images were Ubuntu 24.04 `20260823.283.1`, macOS 15 arm64 `20260727.0256.1`, and
Windows 2025 with Visual Studio 2026 `20260824.214.3`. All quality, headless smoke, native
export, unsigned-signature rejection, and artifact-upload steps passed in both event types. See
`HANDOVER.md` for job, artifact, pull-request, and limitation evidence. Hosted debug exports
remain build evidence only, not releases.

[github-runners]: https://docs.github.com/en/actions/reference/runners/github-hosted-runners
[push-run]: https://github.com/hherb/allrpcs/actions/runs/33310947250
[pull-request-run]: https://github.com/hherb/allrpcs/actions/runs/33311261187

## Unsigned development exports

With Godot 4.6.3 .NET export templates installed, reproduce one CI export locally from the
repository root:

```sh
./scripts/export-development.sh linux-x64
./scripts/export-development.sh macos-universal
./scripts/export-development.sh windows-x64
```

Each command writes only under `artifacts/unsigned-development/`, which Git ignores. The
committed `game/export_presets.cfg` explicitly disables macOS signing and notarization and
Windows code signing. Linux has no signing step. These commands create development-build
evidence only; they do not package, sign, notarize, publish, or distribute the game.

## Launch the keyboard greybox

```sh
godot --path game
```

Use W, A, S, and D to move the gold placeholder Seeker around the 3200-by-1800 test region. Its
ground is the repeated production quiet-moss tile; every route, landmark, stream, character, label,
and boundary above it remains greybox presentation. The Seeker's named default speed is 240 pixels
per second. Diagonal input is normalized so it does not move faster than a cardinal direction. The
camera follows with a named smoothing speed of 6 and world limits matching the region, so ordinary
movement cannot reveal space beyond it. Its named minimum, default, and maximum zoom tuning values
are 0.8, 1.0, and 2.0. Zoom and camera-pan input are intentionally not mapped yet.

The 3200-by-1800 region is authored world space, not a required display resolution. The window uses
1280-by-720 as its minimum supported physical size and 16:9 design viewport. It remains resizable,
uses canvas-item scaling with aspect expansion, and can occupy larger or fullscreen displays without
changing world coordinates or stretching one axis independently. Wider screens reveal more horizontal
camera space and taller screens reveal more vertical camera space within the same bounded world.

Collision-backed route boundaries keep the Seeker inside the approved twelve-corridor network.
The stream banks, village and landmark shells, clearing tree boundary, and closed forest gate block
their exact non-route silhouettes while the authored openings retain every open topology edge.

The credential-free movement-boundary smoke exercises those authored shapes with deterministic,
test-only Godot physics motions from the existing Seeker body. It verifies collision sweeps and
clear paths without synthesizing keyboard input or claiming final input UX coverage:

```sh
ALLRPCS_SEEKER_BOUNDARY_CHECK=1 godot --headless --path game --quit-after 30
```

A deterministically authored `NavigationRegion2D` covers the approved twelve-corridor opening-region
network inside the same 3200-by-1800 region. It retains 24 pixels of companion clearance inside the
96-pixel route language and excludes collision-backed route boundaries and landmark shells. The
obvious cyan placeholder healer starts at the named `HealerStart` position and follows the Seeker
over that navigation surface at a named 180-pixel-per-second speed only while outside a validated
72-pixel stop distance. Its circular collision matches its 36-pixel visible footprint, so the
healer stops without overlapping the Seeker and can traverse every open connection after separation.

The small **TEMP** panel shows the current controls, healer mode, interaction focus, last
explicit focused-search attempt, and current opening-region recovery guidance. The much larger
temporary provider-settings and dialogue panels start hidden so they do not obscure the playfield;
press Tab to show both and Tab again to hide both and release their keyboard focus. Press U to open
the one-shot Seeker customization menu. The menu
suspends Seeker movement while open, requires a name, all five pronoun forms, and exact authored
appearance/background choices, atomically installs the customized live character, then resumes
movement after successful selection. The same one-shot submission retains the selected background's
optional route definition without changing navigation, collision, or route availability. Press F to start
a focused search only when the current valid
interaction focus resolves to an existing world object, C to cancel the Seeker's active search, E
to complete it, or R to resolve the first retained Seeker outcome; these temporary search controls
are detailed below. At the old shrine, press I while inside the shrine and within the fixed
112-pixel inscription radius to reveal the layered-inscription marker once through its exact
discovery coordinator. Press X to
toggle between follow and wait. Press M to move the
healer to the obvious pink ring; the healer uses the existing navigation surface, stops within a
validated 12-pixel distance, then changes to wait. Press X again to resume follow. Off-world and
obstacle-excluded destinations are rejected without changing the active command. The Seeker and
healer mutually ignore body collision so a following healer cannot pin the Seeker against authored
walls; the validated 72-pixel follow stop still keeps their ordinary movement visually separated.
These keys, the
ring, and the status panel are temporary behavior-test controls, not final command or dialogue UX.

Three temporary authored objects exercise interaction focus: the two nearby discovery markers and
the village water cask. A bright yellow **FOCUS** ring and the top-left status identify only the
nearest visible, enabled target inside the inclusive 112-pixel range. Exact distance ties resolve by
authored scene order, so focus is stable at the Seeker's initial midpoint. Moving nearer another
target changes focus; moving outside every range clears it. A hidden marker and an obvious excluded
marker exercise the fail-closed presentation paths and can never receive the indicator. These
markers, labels, range,
ordering, and indicator are temporary greybox behavior evidence. Each marker also authors one
canonical object-identifier text value, which its Godot adapter parses once while ready. The focus
controller exposes the exact identifier instance for only the currently selected visible, enabled,
in-range marker and clears it with focus. Its request factory also receives an existing nullable
world object, rejects null or a target mismatch, accepts a distinct value-equal identifier, and
retains the matching object's exact identifier instance. Its start-attempt boundary supplies the
activity controller's exact Seeker identifier to that factory, then passes an existing
discovery-condition set, capability set, and inventory unchanged to the condition-aware start path
only for a created request. It returns false without invoking that path when focus is clear or the
object is absent or mismatched. This bridge does not make the
markers authoritative Phase 2 objects or add perception state, search input, search execution, or
interaction UX.

After a successful temporary R resolution, the existing small cyan label beneath only the matching
herb patch or stone marker presents that exact looked-up world object's authored description. The
label is cleared and hidden for every state other than exactly identified. It reflects the
controller's retained Seeker perception, not current interaction focus, and is temporary behavior
evidence rather than final discovery UI.

Every shape, color, obstacle, collision outline, navigation polygon, target marker, and status
element is an authored primitive for scale and behavior testing only; none selects final art or
terrain. Close the titled **Allrpcs** window to stop the process. To open the project in the editor
instead, run `godot --editor --path game`.

## Project boundaries

- `src/Domain` contains authoritative rules and has no project dependencies.
- `src/Application` contains pure movement, camera-framing, navigation-request validation,
  healer follow-target selection, healer command selection, bounded interaction-focus and dialogue
  hearing selection, and the storage-independent save-document port; it depends only on `Domain`.
- `src/Infrastructure` owns JSON and local-file save adapters and depends on `Application`.
- `game` is the Godot presentation and composition project and depends on `Application` and
  `Infrastructure`.
- `tests/Domain.Tests` tests the pure `Domain` project without loading Godot.
- `tests/Application.Tests` tests pure movement, camera-framing, navigation-request, healer
  follow-target, healer command, interaction-focus, dialogue-hearing, and save-document port
  behavior without loading Godot.
- `tests/Infrastructure.Tests` tests serialization and isolated local-file behavior without loading
  Godot.
- `tests/Architecture.Tests` enforces these exact references by inspecting project files.

The Godot scene also owns credential-free engine checks for navigation topology, healer-command
movement, interaction-focus presentation, and Seeker collision boundaries. These checks load the
authored scene; they do not move Godot physics or presentation policy into `Application`.

## Save-schema version boundary

`Allrpcs.Domain.Persistence.SaveSchemaVersion` is the engine-independent immutable root version
discriminator for versioned saves. It defines canonical versions `1` through `8`; construction
still accepts any positive integer, while `Parse` accepts only invariant canonical decimal text.
Whitespace, signs, leading zeroes, zero, negative values, overflow, and non-numeric text are rejected
rather than normalized. `Value`, `ToString()`, parsing, and record equality remain stable round-trip
boundaries.

`SaveSchemaVersionSupportPolicy.CanLoad` separately accepts only the eight known versions, including
distinct value-equal instances. Later positive versions remain valid version values but fail closed.
The policy leaves its input unchanged and does not parse, migrate, serialize, or load anything.

These version boundaries do not themselves define a save aggregate, serializer, file or path
policy, storage provider, migration, load orchestration, autosave, replay, Godot integration,
credentials, or UI.

`Allrpcs.Domain.Persistence.DiscoverySaveState` is the first immutable save-state composition. It
retains the exact existing character-perception, focused-search activity, focused-search outcome,
passive-noticing candidate, and observation-reinterpretation candidate snapshots through get-only
properties. It rejects null snapshots and adds no cross-snapshot validation, copying, projection,
transition, serialization, restoration, or controller behavior.

`WorldEventSaveState` separately retains the exact authoritative event-log and qualitative
world-time snapshots. Authored world-event definitions remain outside this dynamic save state so a
later restoration boundary can validate saved identifiers against the running scenario. The value
rejects null snapshots and adds no consistency decision, event replay, time advancement,
serialization, restoration, or controller behavior.

`SaveSnapshot` is the immutable in-memory root. It retains one exact schema version, discovery
save state, world-event save state, canonical all-character memory, the version-3-or-later
first-patient composition, version-5 safe journal state, version-6 visibility-safe map,
version-7 ID-only investigation-route lifecycle, and version-8 closed first-patient recovery stage
through get-only properties and rejects invalid
version/component combinations. Its
legacy version-`1` constructor supplies one shared immutable empty-memory snapshot, and version `1`
rejects populated memory. Record equality continues to use the version value and retained state
instances.

`SaveSnapshotLoadPolicy.CanLoad` is the pure root-level version gate. It delegates to
`SaveSchemaVersionSupportPolicy`, accepts versions 1 through 8, preserves the root and every nested
instance, and does not inspect state contents, migrate, deserialize, or restore.

`WorldEventSaveStateRestorationPolicy.CanRestore` is the pure event-state restoration gate. Every
saved log identifier must resolve against an exact authored world-event definition, and projecting
those resolved events must reproduce the saved qualitative time by identifier value. Empty initial
state is valid; undefined identifiers, missing advances, stale advances, no-advance identifiers,
and unrelated time identifiers fail closed. The policy leaves all inputs unchanged and does not
replay events, advance time, replace state, deserialize, or access storage.

`DiscoverySaveStateRestorationPolicy.CanRestore` is the pure discovery-state restoration gate for
one supplied searcher. The perception owner plus every focused-search activity, outcome,
passive-noticing candidate, and reinterpretation candidate owner must be value-equal to that
searcher. Every perception or lifecycle target must resolve against the authoritative world-object
snapshot. Empty owned collections and distinct value-equal identifiers are accepted; mismatched
owners and missing objects fail closed. The policy does not evaluate discovery conditions,
knowledge triggers, visibility changes, lifecycle eligibility, serialization, or controller state.

`SaveSnapshotRestorationPolicy.CanRestore` is the complete pure root gate. It validates every input
before delegating unchanged to the schema-version, discovery-state, world-event-state, and
first-patient restoration boundaries. Version-3 eligibility additionally requires every saved
disease, infection, preparation recipe, input and result ingredient, ordered step, quantity unit,
administration, and experiment event to resolve compatibly against the exact authored treatment
context and world events. It returns only a Boolean and does not deserialize, migrate, replay,
replace controller state, choose a file, or access storage.

`FocusedSearchActivityController.TryRestoreDiscoveryState` is the first live save-state restoration
boundary. It delegates complete eligibility to `DiscoverySaveStateRestorationPolicy` using the
controller's Seeker and a supplied authoritative world-object snapshot. Success installs the save
state's exact perception, focused-search activity and outcome, passive-noticing candidate, and
observation-reinterpretation candidate snapshots together, then presents only the restored activity
through the existing indicator. Rejected and repeated attempts preserve the installed snapshots.
This operation does not restore world events, coordinate a complete save root, deserialize, migrate,
access storage, or orchestrate startup loading.

`WorldEventController.TryRestoreWorldEventState` is the matching live event-state restoration
boundary. It validates an existing save state against the controller's exact initialized authored
event definitions, then installs the save state's exact event-log and qualitative-time snapshots
together while retaining those definitions. Unsupported logs, inconsistent time, uninitialized
controllers, and null input fail without changing controller state. The operation does not replay
events, coordinate discovery restoration, deserialize, migrate, access storage, or orchestrate
startup loading.

`SaveSnapshotRestorationController.TryRestore` coordinates all live boundaries for one
already-materialized `SaveSnapshot`. It completes root preflight and constructs the rebound
first-patient graph, a fresh complete safe journal and exact optional selection, a fresh canonical
map, the complete reconstructed investigation-route lifecycle, and canonical recovery-dialogue
tokens before changing any controller. Success atomically installs
discovery, world-event, memory, first-patient, preparation, journal, map, route progress, ordered
accepted completions, optional exact consumed completion, and recovery-dialogue progress while
retaining authored definitions and presenting only the restored focused-search activity. Rejected roots do
not partially alter any controller. This remains an in-memory seam: it does not parse JSON, choose a
file or path, autosave, replay events, or run automatically during startup.

`SaveSnapshotCaptureController.Capture` is the matching scene-owned in-memory capture seam. It
returns no snapshot until discovery perception, authored world events, the event log, qualitative
time, all first-patient state, a complete same-Seeker safe journal and map, and internally
consistent initialized route progress are available. Each successful call creates fresh discovery,
world-event, first-patient, journal, map, ID-only route-lifecycle, recovery-stage, and version-8 root
compositions.
Capture accepts only exact current-catalogue progress in authored order, unique exact route-member
observations, exact complete accepted tokens, and optional consumption of one exact accepted token.
It fails closed before producing a document and does not mutate or present controller state,
serialize, choose a file or path, access storage, migrate, replay, autosave, or orchestrate startup
loading.

`Allrpcs.Infrastructure.Persistence.SaveSnapshotJsonSerializer` writes compact canonical JSON for
versions 1 through 8 while preserving the exact legacy documents. Version 2 adds canonical
all-character memory; version 3 adds complete disease truth, completed preparations, remaining
batches, administration observations, and universal experiment records; version 4 adds the exact
delivery mode to each heard event and optional response in recent conversation. Version 5 adds the
safe journal owner, all five ordered content arms, and optional selected-section key. Version 6 adds
only safe map visibility and player-authored notes. Version 7 adds only ordered route keys, recorded
location/evidence identifiers, accepted completion keys, and the optional consumed key; labels,
descriptions, coordinates, discovery text, and runtime object graphs remain absent. Version 8 adds
one closed `not-heard`, `patient-testimony-heard`, or `sage-consulted` marker without making journal
text authoritative or persisting a runtime dialogue graph. Stable
document-local references reconnect shared memories, preparations, and events without serializing
runtime object references. Provider identifiers, credentials, provider state, hidden reasoning, and
an additional authoritative transcript are absent. Serialization preserves every input and does not
write a file, choose a path, validate a scenario, migrate, encrypt, or restore runtime state.

`SaveSnapshotJsonDeserializer` is the strict inverse for all eight known JSON versions. It accepts only
each version's complete closed object shape and exact JSON value kinds, rejecting malformed JSON,
comments, trailing commas, duplicate, missing, or unknown properties, non-canonical versions and
identifiers, unknown closed values, invalid collections, and unresolved memory references.
Successful reads materialize fresh immutable Domain values in serialized order while reconnecting
document-local shared identities. `DeserializeCurrent` migrates versions 1 through 4 to version 5,
using the caller-owned exact initial first-patient composition when required and creating an empty
unselected journal owned by the saved discovery character. Versions 1 through 3 historically lacked
delivery metadata, so their heard dialogue and non-null response speech migrate as explicit ordinary
speech; versions 4 through 8 retain their exact ordinary or whisper modes. Scenario eligibility and
controller mutation remain separate. `DeserializeCurrent` migrates versions 1 through 7 to version
8, adding a canonical empty same-Seeker map where needed, three canonical empty route entries with
no accepted or consumed completion, and inert not-heard recovery progress. Neither entry point
accesses storage.

`SaveSnapshotJsonController` is the scene composition seam for serialized in-memory round trips.
`CaptureSerialized` delegates exact live capture to `SaveSnapshotCaptureController` before canonical
serialization. `TryRestoreSerialized` supplies exact authored initial first-patient state for legacy
migration, rejects invalid or incompatible documents, and delegates the current materialized root to
`SaveSnapshotRestorationController`, whose complete scenario preflight prevents partial restoration.

`Allrpcs.Application.Persistence.ISaveDocumentStore` is the storage-independent single-document
port. It accepts only a non-empty already-serialized document and returns explicit `Saved` or
`Failed` write results. Reads distinguish `Loaded`, `Missing`, and `Failed`; only `Loaded` carries
the exact non-empty document. The port exposes no path, slot, Godot, JSON, provider, or credential
type.

`FileSaveDocumentStore` is the local Infrastructure adapter. It requires one trusted absolute path,
writes strict UTF-8 without a byte-order mark to a unique sibling temporary file, flushes it before
same-directory replacement, and reports expected I/O, access, encoding, and platform failures
without treating empty or invalid UTF-8 data as a loaded save. `SaveDocumentStorageController`
composes that adapter at the fixed Godot `user://save-v1.json` path with exact capture and complete
preflighted restoration. The ordinary `SaveLoadInputController` delegates only to those operations.
Once explicit map and route state exists, an explicit Q or Save-button action may initialize the same
safe journal that J would compose, so saving does not depend on an undocumented journal-open step;
incomplete map or route state remains inert without initializing the journal and now explicitly asks
the player to initialize that state with M instead of reporting only a generic failure. A successful
save also gives the close, reopen, and L instruction needed for the manual checkpoint. Physical Q or
the visible Save button reports `SAVED`, `PRESS M FIRST`, or `SAVE FAILED`, while physical L or the
visible Load button reports `LOADED`, `NO SAVE FOUND`, or `LOAD FAILED`. It never exposes the fixed
path or serialized content. Startup remains inert, and there is still no autosave, multiple slots,
user-selected path, settings or provider-state persistence, encryption, compression, sync, replay,
or credential behavior. Both storage and input smokes use a process-specific system-temporary file
and delete it after the check, so validation cannot overwrite an ordinary user save. The input
smoke covers incomplete capture, missing storage, exact save/change/load restoration, malformed and
future-version rejection without live-state mutation, released and unrelated input, and unrelated
scene-state isolation.

## Discovery-scenario validation vocabulary

`DiscoveryEvidence` maps one exact evidence identifier to its authoritative world object, minimum
visibility threshold, and explicit criticality. `DiscoveryEvidenceSnapshot` retains stable authored
order while rejecting null entries and duplicate evidence identifiers. `DiscoveryScenario` composes
the exact observer, authoritative objects, evidence definitions, initial perception, focused-search
conditions, bounded available capabilities, tool inventory, knowledge, passive-noticing candidates,
and reinterpretation candidates needed by the next pure validator. These immutable values do not
yet author the five representative discoveries, validate cross-references or reachability, filter a
character view, or change scene behavior.

## Stable domain identifiers

`Allrpcs.Domain.Identifiers` defines distinct immutable identifiers for characters, world
objects, locations, evidence, knowledge, and authoritative world events. They share one
canonical-text contract: 1 to 64 characters, beginning with a lowercase ASCII letter and then
containing only lowercase ASCII letters, digits, and single internal hyphens. For example,
`healer`, `village-square-2`, and `patient-stabilized` are canonical. Inputs are validated rather
than normalized, so case, whitespace, underscores, repeated hyphens, leading or trailing hyphens,
and non-ASCII text are rejected.

Each identifier's `Value` and `ToString()` return the same canonical text, and its `Parse` method
round-trips that text. Equality requires both the identifier type and its canonical value, so a
character and an object with the same text remain different identifiers. These types are opaque
keys only: they do not contain mutable entity state, scenario content, Godot types, provider or
persistence details, or serialization-framework behavior.

## Controller types

`Allrpcs.Domain.Controllers.ControllerType` defines exactly four controller types: `human`,
`llm`, `deterministic-test`, and `dormant`. `All` exposes the complete set in that stable order,
while `Value` and `ToString()` expose each type's canonical text. `Parse` accepts only those exact
values and returns the corresponding defined instance; case changes, whitespace, provider names,
and other unknown text are rejected rather than normalized or registered.

These values describe controller categories only. They do not create controller instances,
select providers or models, define prompts or behavior, attach a controller to a character, or
introduce runtime orchestration, serialization-framework, persistence, Godot, or UI behavior.

## Character model boundaries

`Allrpcs.Domain.Characters.CharacterIdentity` pairs a stable `CharacterId` with an authored
display name. Display names must contain a non-whitespace character, are bounded to 80 UTF-16
code units as measured by .NET `string.Length`, and are retained exactly rather than trimmed,
case-folded, or otherwise normalized. The immutable value compares by both identifier and display
name.

`PublicCharacterBackground` and `PrivateCharacterBackground` are separate immutable value types.
Their structural separation lets future filtered views carry a public background without also
carrying private text. Both retain authored text exactly, explicitly permit an empty string when
no background is authored, reject null, and are bounded to 4,000 UTF-16 code units. Private
background is ordinary authored character state: it is not hidden chain-of-thought, a credential
container, arbitrary metadata, or a provider instruction channel.

The Seeker customization foundation retains one exact canonical Seeker identity, five bounded
custom pronoun forms, one asset-neutral appearance choice, and one public background choice.
Representative options author three greybox appearance labels and the Forester, Scribe,
Apothecary's Assistant, and Traveller backgrounds in stable order. Pure composition accepts only
the exact retained appearance and background instances. One immutable selection state retains its
exact predecessor, options, and optional profile so the main-scene source can compare-and-swap one
exact transition result and reject rebuilt, stale, foreign, or repeated installation. The hidden
startup menu opens only on U, presents no default choice, validates complete authored input, and
installs that exact state once while preserving the Seeker's prior movement-processing state. A
current one-shot hint states that each background unlocks one optional route advantage; it no
longer describes the already-wired route effects as inactive. A
pure projection creates one fresh customized `Character` with the selected exact identity and
public background while retaining the current character's exact private background, capabilities,
limitations, controller assignment, conditions, inventory, knowledge, beliefs, relationships, and
goals. Its immutable application token retains the exact selected state, prior character, and
projected character. A representative route catalogue maps the exact Forester choice to the closed
forest path, Scribe to the old shrine, Apothecary's Assistant to the healer's garden, and Traveller
to the southern road. One immutable scene-owned route state retains that exact catalogue and the
one selected definition. The synchronous scene coordinator preflights and compare-and-swaps the
selected customization, live temporary character, route state, and one exact scene-owned additive
access token, rejecting null, empty, rebuilt, foreign, stale, non-destination, repeated, and
already-applied paths without partial mutation. Exact Forester access is consumed by the existing
closed-path owner, which removes its gate collision and greybox gate presentation once while
retaining the exact route, variation, destination, and scene provenance. Exact Scribe access is
separately bound to the exact old-shrine scene and layered-inscription provenance; the shrine owner
retains that additive inspection authorization once without identifying or revealing the
inscription, changing discovery, presentation, evidence, or environmental state, or bypassing the
ordinary discovery transition. Exact Apothecary's Assistant access is separately bound to the
existing healer-garden preparation scene, treatment context, and recipe; that scene retains the
authorization once without starting or advancing preparation, consuming ingredients, or changing
patient or evidence state. Exact Traveller access is separately bound to the existing southern-road
and abandoned-wagon scene; that scene retains the authorization once without changing geometry,
navigation, discovery, or presentation. Ordinary startup remains closed, concealed, and inert,
non-Forester choices leave the forest gate unchanged, and no access closes a baseline route or
claims that a distant destination is navigation-reachable. Pronouns are retained but not yet
projected into dialogue; appearance assets, downstream background-specific interactions,
persistence, and provider behavior remain deferred.

The journal foundation defines one asset-neutral catalogue containing observations, testimony,
hypotheses, experiments, and open questions in stable canonical order. Each immutable definition
retains its exact bounded authored key and display label. The catalogue defensively copies the
complete five-section sequence, preserves the exact supplied definitions, and rejects missing,
null, duplicate, reordered, or unknown keys. The representative source returns a fresh catalogue
without entries, perception or dialogue projection, editable notes, scene ownership, UI, input,
persistence, authoritative gameplay mutation, or provider behavior.

The observations-section projection binds only the catalogue's exact first definition to one exact
Seeker-owned perception snapshot that has already excluded every `Unnoticed` object. It retains the
exact catalogue, definition, Seeker identifier, snapshot, visibility entries, and stable order;
value-equal rebuilt or different character identifiers and still-hidden inputs fail closed. Empty
filtered perception is valid. This boundary creates no observation text or entry, resolves no world
object, and does not own scene state, mutate perception, present UI, handle input, persist, or call a
provider.

The adjacent observation-world projection retains that exact filtered section binding together
with one exact authoritative world-object snapshot only after every visible target resolves by
identifier. Value-equal perception identifiers are valid, unrelated authoritative objects remain
retained, and a missing visible target rejects the complete binding without changing either input.
It performs no visibility interpretation, state ownership, mutation, presentation, input handling,
persistence, or provider work.

The structural observation-entry projection resolves that validated binding into fresh read-only
entries in perception order. Each entry retains the exact filtered visibility and its exact
value-equal matching authoritative world object; unrelated world objects remain outside the result,
and empty perception produces an empty independent collection. A separate authored-copy projection
keeps only the exact `Identified` visibility singleton and copies its filtered object identifier plus
exact authored display name and description. It neither ranks later visibility states nor exposes an
authoritative object through its safe output.

The immutable observations-section snapshot composes those projections into a consumer-safe
boundary containing only the exact canonical section, exact Seeker identifier, and independent
read-only authored copies. It exposes no perception or authoritative world snapshot and still owns
no editable journal state, scene, UI, input, persistence, gameplay mutation, or provider behavior.

The safe journal snapshot retains that exact observations snapshot with its exact catalogue and
Seeker while copying the remaining testimony, hypotheses, experiments, and open-questions
definitions into an independent read-only pending sequence. Composition requires the exact
catalogue observations definition; rebuilt value-equal and foreign definitions fail closed.
Section-specific installation projections return fresh journals that replace only the exact next
pending definition in canonical order. Rebuilt sections, distinct value-equal Seeker identifiers,
stale order, and repeated installation fail closed. Installing the exact final open-questions
snapshot leaves an independent empty pending sequence.

The testimony-section projection binds the exact canonical testimony definition only to dialogue
events whose recorded audience contains the journal Seeker. It copies the selected exact events in
stable source order, accepts value-equal listener identifiers, and leaves unrelated dialogue and
the Seeker's own speech outside the result. A separate authored-copy projection creates fresh
audience-free values containing only the exact speaker identifier, utterance text, and
provider-neutral delivery mode. An immutable testimony-section snapshot composes that binding and
fresh copy projection into one safe value retaining only the exact canonical section, exact Seeker,
and independent read-only authored copies. It exposes no journal, dialogue event, audience, or
listener identity.

The hypotheses-section projection binds only a post-testimony journal whose exact next pending
definition is the canonical hypotheses section to one exact existing immutable investigation-
hypothesis snapshot. Its authored copies contain only each exact hypothesis and question identifier
plus exact authored question and statement text in stable order. The immutable hypotheses snapshot
retains only those fresh copies, the exact section, and exact Seeker before its installation replaces
only that pending definition.

The experiments-section projection binds only the exact next canonical definition to one existing
universal experiment journal. Its fresh read-only authored copies retain only each exact experiment
identifier, procedure text, and observed-outcome text in recorded order; authoritative world events
remain behind the binding. The open-questions projection similarly binds only the final exact
canonical definition to one existing open-question snapshot, then copies only each exact question
identifier and prompt in authored order. Their immutable section snapshots retain only the exact
section, exact Seeker, and independent safe copies. Installing both in order completes the safe
journal without inferring experiment efficacy, question answers, confidence, truth, status, or
completion. One scene-owned controller can retain an already-complete exact safe journal once. It
starts null, accepts only a journal with every section installed and no pending definitions, and
rejects incomplete, repeated, replacement, and null initialization without replacing retained
state. Ordinary startup leaves it uninitialized and inert.

An explicitly invoked scene coordinator can compose that journal from existing exact ordinary
state: current filtered Seeker perception and world objects, the latest heard dialogue event when
present, authored first-patient investigation plans and confirmed conclusions, and the universal
experiment journal. It copies only identified observations and safe authored section values, keeps
confirmed questions out of the open-question section, and initializes the journal controller once.
Missing, unsafe, or repeated inputs fail closed. Ordinary startup never invokes this path, authors
no replacement facts, and leaves journal UI, input, refresh, persistence, gameplay mutation, and
providers absent.

One scene-owned section-selection controller starts null and accepts only the exact retained journal
plus one exact canonical section definition from that journal. It permits explicit replacement by a
different exact canonical section while rejecting uninitialized, stale, foreign, rebuilt,
value-equal, repeated, and null inputs without changing the current selection. Ordinary startup
does not initialize the journal or select a section.

A pure selected-section presentation projection accepts only one complete exact journal and one of
its exact canonical sections. Its immutable result exposes the exact selected definition plus
exactly one of the journal's already-safe observation, testimony, hypothesis, experiment, or
open-question snapshots; it exposes no complete journal, catalogue, authoritative source, dialogue
audience, or world event. A scene controller owns one explicitly requested projection and can
explicitly replace it only after the exact selection changes. Unprojected, unchanged, repeated,
foreign, rebuilt, incomplete, and null paths fail closed without changing prior presentation state.
Ordinary startup, journal composition, and section selection do not project automatically.

A pure plain-text projection renders only that selected safe arm with the exact authored section
label. Observations show copied names and descriptions, testimony shows canonical speaker and
delivery identifiers without audiences, hypotheses show authored question and statement text,
experiments show authored procedure and observed outcome text, and open questions show authored
prompts. Every empty arm uses one explicit neutral empty state. The hidden-by-default journal panel
accepts only the presentation controller's exact current snapshot, preserves rendered state across
rejected stale, rebuilt, foreign, repeated, and null values, and permits replacement only after an
explicit exact presentation refresh.

The mapped J action explicitly composes the representative journal on first open, selects and
presents its first canonical section, captures focus inside the panel, stops Seeker movement, and
restores the exact prior movement-processing state on close. Previous and Next controls traverse the
five existing exact definitions in stable canonical order with closed ends; Close or J hides the
panel while retaining the exact immutable presentation for reopening. Ordinary startup remains
inert. The journal flow still adds no automatic source refresh, gameplay mutation, authoritative
source exposure, or provider work.

The journal now has a closed content-safe persistence boundary. A pure capture accepts only a
complete exact safe journal and an optional exact canonical selection, then creates fresh immutable
ordered copies of all five section arms without retaining the catalogue, authoritative sources,
dialogue audiences, experiment events, controllers, or UI state. Save schema version 5 requires that
journal state, canonical JSON writes and strictly reads it, and current loading migrates versions 1
through 4 to an explicit empty unselected journal owned by the saved discovery character. A pure
restoration projection validates duplicate identities and shared-question prompt consistency before
creating one fresh complete journal whose section snapshots retain the exact definitions of a fresh
canonical catalogue. Scene restoration installs that journal and its optional exact selection only
after complete root preflight. Scene capture requires a complete same-Seeker journal and writes fresh
safe content copies into schema version 5. JSON and local storage round trips preserve the safe
journal and selection, so the Phase 8 journal roadmap item is complete.

The pure opening-region map foundation derives one fresh asset-neutral catalogue from all eleven
already-authored representative landmarks. Each definition retains only its exact canonical
location identifier and bounded player-facing label in stable authored order. One immutable
Seeker-owned map snapshot exposes only reported and verified definitions, keeps hidden catalogue
membership private, requires unique exact definitions with disjoint membership, and returns fresh
canonical-order snapshots for explicit reporting or verification. Verification promotes a reported
location or adds a previously hidden visited location without exposing world objects, positions,
navigation topology, or unrelated landmarks.

An explicit testimony-report projection accepts only one exact audience-free authored copy already
retained by the exact Seeker's safe journal testimony section. The caller explicitly supplies the
exact canonical location to report: text, speaker, and ordinary or whisper delivery are never
classified or treated as authority. A separate pure visit projection accepts one exact retained
opening-region anchor and a finite Seeker position within an explicit positive distance, then emits
only a coordinate-free Seeker/location token. Exact token, projection, owner, catalogue, and
location provenance are required before verification changes the map.

Player map notes remain a separate immutable Seeker-owned safe state. Explicit transitions add or
replace one bounded nonblank note only for an exact reported or verified location, keep notes in
canonical location order, and remove only an existing exact note. Every successful operation
returns fresh state while preserving its map and note inputs. A scene-owned controller now
initializes one exact fresh catalogue, empty map, and empty note state atomically for one Seeker,
then delegates explicit testimony report, visit verification, note set or replace, and note removal
through exact expected-state compare-and-swap. Ordinary startup leaves it uninitialized.

A pure safe map presentation exposes only known definitions in canonical order, their reported or
verified status, and optional bounded note text. Stable plain text has one neutral empty state and
contains no hidden catalogue locations, coordinates, or topology. A scene presentation controller
projects or refreshes only the map controller's exact current map and notes, and a hidden-by-default
panel renders only that controller's exact presentation.

Physical M now opens that panel and the temporary healer-move command uses H. First open initializes
the map once from the exact current scene Seeker, projects empty safe state, captures focus, and
suspends movement. Later opens refresh only after the exact map or note source changes; unchanged
state reuses the retained presentation. Close or M restores the exact prior focus and
movement-processing state. The map and journal reject opening over each other.

The panel's note controls list only already-known safe presentation entries in canonical order.
They add or replace nonblank notes up to the existing 1,000-code-unit bound and remove only an exact
existing note, then synchronously refresh the safe presentation while retaining the selected known
location. Empty, whitespace, stale, hidden, released, unrelated, repeated, rebuilt-owner, and null
paths do not change map, note, presentation, panel, focus, movement, journal, or gameplay state.

One scene coordinator can verify one exact live opening-region visit without retaining or presenting
coordinates. It requires the controller's exact current map, the current scene Seeker's exact
identifier and position, the scene source's exact projection, one exact retained anchor, and the
matching canonical map location. After map initialization, ordinary physics processing scans the
authored anchors in projection order, submits only the first not-yet-verified anchor within the named
24-pixel radius, and delegates only its coordinate-free visit token to the map controller.

A separate explicit scene coordinator can report one canonical map location only from the exact
current complete safe journal, its exact testimony section and retained authored copy, the exact
current Seeker, and the controller's exact current map. The pairing remains caller-authored: it does
not classify testimony text, delivery mode, or speaker, and it does not infer a location. Reporting
and visit verification preserve source snapshots, promote reported locations when visited, add
hidden verified locations, and retain canonical location order. There is still no automatic
testimony reporting, player-visible reporting choice, coordinate or topology exposure, provider
call, or credential access.

The map has a closed visibility-safe persistence boundary. Pure capture accepts only an exact
same-Seeker map and note pair backed by one exact catalogue, then retains only the Seeker identifier,
reported and verified location identifiers, and location/note-text pairs in canonical order. Pure
restoration preflights unknown, duplicate, overlapping, hidden-note, duplicate-note, invalid-text,
and inconsistent-provenance state before creating fresh map and note snapshots bound to exact
definitions from one fresh representative catalogue. Empty and populated state round trip without
exposing hidden catalogue locations, labels for hidden locations, coordinates, topology, testimony,
scene objects, controllers, or UI state.

Schema version 6 introduced the visibility-safe map contract. Its strict JSON adds only `map` with
`seekerId`, `reportedLocationIds`, `verifiedLocationIds`, and ordered `{locationId,text}` notes;
unknown, missing, duplicate-property, wrong-kind, oversized, and malformed structures fail closed.
Versions 1 through 5 retain their exact document shapes and migrate to fresh empty same-Seeker map
state. Scene capture requires complete initialized same-Seeker journal, map, and note state before
emitting version 6. Scene restoration preflights first-patient, journal, and map state before
installing discovery, world events, memory, first-patient state, journal selection, and the exact
fresh map composition. Serialized and isolated-storage checks cover populated report, verification,
and note round trips, legacy empty-map migration, canonical rebinding, and unchanged controller state
after malformed, unknown-location, or hidden-note rejection. Ordinary startup remains inert. The
first explicit M-key map open initializes both the empty safe map and the three investigation routes
from that exact map catalogue and the old-shrine discovery scenario. It also routes the exact
already-noticed stream-residue evidence retained before route progress existed. Later physical
anchor visits verify their safe map locations automatically. There is still no automatic testimony
reporting or coordinate/topology exposure.

The opening region now has three asset-neutral, provider-neutral investigation routes. The
stream/laundry route retains only `stream`, `abandoned-laundry`, and `residue`; the shrine route
retains only `old-shrine` and `partial-inscription`; and the road/wagon route retains only
`southern-road`, `abandoned-wagon`, and `tool-gated-clue`. Route keys and ordered location/evidence
identifier lists are immutable, copied, nonempty, and duplicate-free. One pure policy validates
that every identifier is an exact member of one supplied representative map catalogue and
discovery scenario and that no two routes repeat the same ID shape. No label, description,
coordinate, topology, testimony, background, provider, controller, or UI state enters the route
catalogue.

Separate immutable route-progress state starts with one empty entry per exact route and records
only explicitly supplied exact member locations or evidence identities. Each successful operation
returns a fresh bounded snapshot, preserves every unrelated entry, and does not infer or expose
route completion. An inert scene controller can initialize this validated catalogue once and install
those explicit operations through expected-state compare-and-swap. Ordinary startup does not
initialize it. The first successful explicit map open performs the one-shot initialization from the
map's exact catalogue and the old-shrine scene's exact discovery scenario, then records only the
exact retained stream-residue evidence view; later map opens retain the exact current progress.
Initialization does not start discovery, reveal new evidence, change first-patient state, write the
journal, call a provider, or present route progress.

A separate pure authority transition accepts a route location only when the exact supplied map
definition is in the exact supplied `SeekerMapSnapshot` verified-location view and its identifier is
an exact member of that route. Its evidence operation likewise accepts only an exact route evidence
identifier retained by the supplied filtered `CharacterEvidenceViewSnapshot`. A scene coordinator
requires the exact current scene-owned map for location progress and installs both operations through
the route controller's expected-progress compare-and-swap. Rebuilt, foreign, unverified, unseen,
nonmember, repeated, mismatched, and stale inputs remain inert. The coordinator is explicit and inert
at ordinary startup; it still does not infer route completion, write any authority source, or present
progress. A catalogue-routing coordinator can take one exact already-verified map location or visible
evidence identity, preflight every exact matching route, record it into all matches in authored
catalogue order, and ask the completion owner to observe each exact resulting snapshot. It never
chooses one preferred route or pools partial progress. The live map-visit coordinator submits its
exact successfully verified location through that path when route progress is active. After the
old-shrine discovery atomically commits its exact identified presentation and filtered evidence
exposure, it likewise submits only that exposure's exact `partial-inscription` identity and exact
filtered Seeker view. Missing route progress preserves the successful discovery without initializing
routes; repeated discovery does not route twice.

The same representative discovery scenario is now shared with the live world-object composition.
Its exact stream-residue candidate resolves once through the established passive-noticing sequence,
advances only the exact residue object from unnoticed to noticed, and filters the resulting evidence
view to the exact `residue` identity. It submits immediately when route progress is active; otherwise
the first successful map open routes that same retained exact view after initializing progress. The
exact representative wagon clue is retained by the live world-object controller and one focus target at the
authored abandoned-wagon anchor. The existing F/E/R focused-search lifecycle applies the scenario's
exact field-lens requirement, advances only that object to identified, filters the view to exact
`tool-gated-clue` evidence, and then performs the same optional route submission. Missing progress
preserves successful discoveries without implicitly initializing routes; stale, rebuilt, foreign,
unresolved, repeated, unseen, and nonmember paths remain inert. Testimony is still unwired, and
observed completion tokens remain unconsumed until the explicit acknowledgement control is used.

A separate pure completion policy returns an immutable provenance token only when one exact route
entry contains every exact authored location and evidence member. It accepts any observation order,
never pools partial progress across routes, and exposes no completion Boolean. An inert scene owner
can explicitly observe those tokens from the route controller's exact current snapshot, retain one
token per exact route in observation order, and preserve previously accepted tokens when another
route completes. A second inert scene owner can explicitly consume one exact accepted token once
without removing or rebuilding it. Neither owner changes route progress, any authority source, or
the ordered accepted-token list.

The ordinary scene now presents the first exact accepted completion only while no completion has
been consumed. A pure projection selects one fixed title/body pair through the closed three-key
authored route mapping; the presentation exposes no progress snapshot, route object, evidence text,
provider content, or hidden state. The greybox panel renders only its coordinator's exact safe
snapshot. Its explicit Acknowledge control must consume the same bound accepted token before the
panel clears. The panel gives that sole action keyboard focus while visible and releases focus when
it clears, so Return completes the same exact acknowledgement without a pointer; failed consumption
leaves the presentation visible, focused, and state intact. Rebuilt,
unaccepted, stale, later, repeated, null, and restored-consumed paths remain inert. Successful
acknowledgement retains all accepted tokens, route progress, discovery, map, journal, perception,
world objects, and save schema 7. This closes the representative multiple-route flow without adding
testimony, stabilization, sage recovery, objectives, clue ordering, disease pressure, or another
evidence source.

`CharacterCapability` and `CharacterLimitation` are structurally distinct immutable authored
values. Each has a stable key using the existing 1-to-64-character canonical identifier-text
contract and a nonblank description bounded to 500 UTF-16 code units. Valid descriptions retain
authored whitespace, casing, line endings, and Unicode exactly. `CharacterCapabilitySet` and
`CharacterLimitationSet` explicitly permit an empty sequence, preserve authored entry order,
copy caller input, and reject duplicate keys within their own boundary.

A capability describes authored character state only. It is not permission, an executable
action, an input, an affordance, or proof that an attempt will succeed. A limitation is not a
public/private classification, authorization rule, action validator, or provider prompt rule.

`CharacterControllerAssignment` associates one stable `CharacterId` with exactly one existing
closed `ControllerType`. The immutable value retains both objects exactly and compares by their
values. It records only the character's current authored controller-type association: it does not
create a controller instance, configure a provider or model, define prompts or behavior, route
input, or introduce runtime orchestration or switching policy.

`CharacterCondition` is a qualitative authored value with a canonical key and a nonblank
description bounded to 500 UTF-16 code units. Valid descriptions preserve whitespace, casing,
line endings, and Unicode exactly. `CharacterConditionSet` explicitly permits an empty sequence,
preserves authored order, copies caller input, and rejects duplicate keys using ordinal
comparison. These types do not define disease stages, symptoms, severity, progression, treatment,
timers, death, or other runtime health mechanics.

`CharacterInventory` contains only the character inventory's current ordered references to
existing `ObjectId` values. It explicitly permits an empty sequence, copies caller input, exposes
only a read-only collection, and rejects null or duplicate identifiers. It does not define object
or item state, quantities, stacks, slots, weight, capacity, containers, equipment, ownership,
transfer, visibility, use, crafting, consumption, or executable policy.

`CharacterKnowledge` contains only a character's current ordered references to existing
`KnowledgeId` values. It explicitly permits an empty sequence, copies caller input, exposes only
a read-only collection, and rejects null or duplicate identifiers. Separate `KnowledgeEntry`
values associate an exact existing identifier with nonblank authored text bounded to 4,000 UTF-16
code units. `KnowledgeSnapshot` retains those entries in stable duplicate-free identifier order,
and `KnowledgeLookup` returns the exact value-equal match. These contracts do not classify content
as world truth, evidence, a claim, or a belief, and do not define confidence, provenance,
discovery, reinterpretation, visibility, perception, relevance ranking, or executable policy.

`CharacterBelief` is an authored value with a canonical key and a nonblank statement bounded to
500 UTF-16 code units. Valid statements preserve whitespace, casing, line endings, and Unicode
exactly. `CharacterBeliefSet` explicitly permits an empty sequence, preserves authored order,
copies caller input, exposes no mutable collection, and rejects null entries or duplicate keys.
These types record only what is authored as a character belief. They do not classify a belief as
world truth or knowledge, or define evidence, confidence, provenance, attributed claims,
discovery, reinterpretation, visibility, perception, authorization, or executable policy.

`CharacterRelationship` associates an existing related `CharacterId` with a nonblank authored
summary bounded to 500 UTF-16 code units. Valid summaries preserve whitespace, casing, line
endings, and Unicode exactly. `CharacterRelationshipSet` explicitly permits an empty sequence,
preserves authored order, copies caller input, exposes no mutable collection, and rejects null
entries or duplicate related-character identifiers. These types do not define scores,
dimensions, reasons, history, reciprocity, symmetry, self-relationship policy, graph traversal,
visibility, perception, authorization, or executable policy.

`CharacterGoal` is an authored value with a canonical key and a nonblank description bounded to
500 UTF-16 code units. Valid keys and descriptions are preserved exactly. `CharacterGoalSet`
explicitly permits an empty sequence, preserves authored order, copies caller input, exposes no
mutable collection, and rejects null entries or duplicate keys using ordinal equality. These
types do not define priority, status, progress, deadlines, dependencies, plans, completion,
rewards, visibility, mutation, behavior selection, authorization, or executable policy.

`Character` is the immutable composition root for the complete authored character model. It
retains exactly one existing identity, public background, private background, capability set,
limitation set, controller assignment, condition set, inventory, knowledge, belief set,
relationship set, and goal set. Every component is required, and the controller assignment's
character identifier must equal the identity identifier. The aggregate adds no other cross-field
policy and exposes only its supplied components through get-only properties.

`RepresentativeHermitCharacterSource` creates exactly two fresh LLM-controlled hermits in stable
woodland-then-riverside order. Each composition aligns one exact identity with its controller
assignment and carries a distinct non-empty private background, knowledge reference, belief, and
goal. The returned collection is read-only, and the source adds no scheduling, generation,
provider, autonomous-action, sabotage, scene, persistence, or presentation behavior.

`RepresentativeSageCharacterSource` creates one fresh LLM-controlled village sage whose authored
capabilities are limited to comparing presented evidence and challenging unsupported assumptions.
Explicit limitations prevent the role from using unavailable evidence or converting advice into
authoritative truth, while one private background, knowledge reference, belief, and goal supply
bounded character state. The source does not select hints, evaluate evidence, mutate an
investigation, schedule dialogue, call a provider, or add runtime behavior.

`RepresentativeEnemyAgentCharacterSource` creates one fresh LLM-controlled covert role with a
private national order, ordinary cover, bounded influence capabilities, and explicit physical,
informational, authoritative-action, and investigation-recovery limitations. Competing delay and
civilian-safety goals plus one authored belief make changed allegiance representable without
implementing an allegiance transition. These values grant no action authority and add no
sabotage, scheduling, provider, scene, or persistence mechanics.

`CharacterBeliefGoalIncompatibility` explicitly binds two value-distinct exact characters to one
exact belief and goal retained by each character, together with a non-blank rationale of at most 500
UTF-16 code units. It rejects value-equal character identities and value-equal replacement beliefs
or goals instead of inferring conflict from authored prose or keys. The pure
`RepresentativeSageEnemyAgentIncompatibilitySource` creates one fresh fixture whose sage favors
timely evidence-led recovery while the enemy agent favors a limited politically useful delay. It
extends only the fresh fixture characters' belief and goal sets, retaining the representative
roles' exact knowledge, capabilities, limitations, and public/private backgrounds. The declaration
does not resolve the conflict, change allegiance, schedule behavior, call a provider, or add scene,
persistence, input, UI, event, time, transcript, movement, or sabotage mechanics.

`CharacterBeliefGoalIncompatibilityPerspective` orients that declaration from either exact retained
character to the other, carrying both sides' exact declared beliefs and goals without inferring a
side from value-equal character state, authored prose, or keys. Its pure projection returns exactly
the first-character and second-character perspectives in stable declaration order as a read-only
collection. It does not evaluate either position, choose a response, resolve the conflict, mutate a
character, or add scheduling, dialogue, sabotage, provider, Application, Infrastructure, scene,
persistence, input, UI, event, time, transcript, or movement behavior.

`CharacterBeliefGoalIncompatibilityResponseKind` closes provider-neutral responses to maintaining
the subject position, reconsidering its belief, reconsidering its goal, or seeking reconciliation.
`CharacterBeliefGoalIncompatibilityResponse` binds one exact perspective and canonical kind to a
non-blank explanation of at most 500 UTF-16 code units. These values make an explicit response
representable but do not select one, judge either character, alter a belief or goal, change
allegiance, produce dialogue, call a provider, or apply any runtime state change.

`CharacterBeliefGoalIncompatibilityResponseRound` retains exactly the two declaration-ordered exact
perspectives and one exact response for each perspective. It copies both caller collections, rejects
incomplete, duplicate, reversed, cross-declaration, value-equal replacement, or otherwise
non-exactly aligned entries, and exposes only read-only collections. A complete round does not judge
or select responses, mutate either character, resolve the incompatibility, or authorize any runtime
behavior.

`CharacterBeliefGoalIncompatibilityConsiderationRequest` retains one exact perspective as a
provider-neutral request for an explicit response. It preserves every nested declaration reference
without copying character state and grants no provider, scheduling, selection, application, or
mutation authority.

`CharacterBeliefGoalIncompatibilityResponsePolicy` accepts a response only when it retains the
request's exact perspective instance. Other subjects, independently composed fixtures, and distinct
value-equal replacement perspectives fail closed. The pure policy does not judge the response kind
or explanation, choose a response, or change either character.

`CharacterBeliefGoalIncompatibilityConsiderationResult` binds one exact request to one response only
after that exact-perspective policy accepts it. The immutable result adds no response judgment,
round orchestration, model invocation, state transition, or runtime authority.

`ICharacterBeliefGoalIncompatibilityResponseModel` defines one provider-neutral asynchronous method
for obtaining a response to an exact consideration request while observing caller cancellation. It
defines no implementation, provider binding, retry, timeout, orchestration, scene, or response
application behavior.

`CharacterBeliefGoalIncompatibilityConsiderationController` forwards one exact request and caller
cancellation token to one exact provider-neutral response model exactly once, then constructs the
existing validated result. Null inputs and pre-cancellation fail before model invocation; null or
non-exact output fails through the Domain result contract, while model cancellation and failure
propagate unchanged. The controller adds no retry, deadline, provider, judgment, application, or
character mutation behavior.

`CharacterBeliefGoalIncompatibilityResponseRoundController` projects one exact declaration into its
two exact perspectives, obtains one validated response for each in stable declaration order, and
returns the existing complete immutable response round. Its optional finite shared deadline applies
to the complete sequential round, reports deadline expiry separately from caller cancellation, and
never returns a partial round. It does not implement a model, judge or apply either response, resolve
the incompatibility, or mutate character state.

`CharacterPhysicalAccessSnapshot` explicitly retains one exact character identifier and at most 64
stable copied, value-unique world-object identifiers that authoritative caller state says are
physically accessible. Its pure lookup requires value-matching character and object identifiers.
It does not infer reachability from visibility, perception, dialogue, model output, or coordinates,
and it grants no capability or action authority.

`CharacterActionAccessRequirement` retains one exact target object, one canonical required
capability key, and one exact required knowledge reference. Its pure eligibility policy succeeds
only when the same value-matching character has explicit physical access to that object, an authored
capability with that key, and the referenced knowledge. Missing or mismatched physical,
informational, or capability input fails closed without changing any input. This gate does not
define an action vocabulary, treat eligibility as occurrence, mutate a target, or grant model text
authority.

`CharacterInteractionAccessSnapshot` retains one exact actor and at most 64 stable copied,
value-unique target-character identifiers currently reachable through ordinary interaction. It
rejects self-targets without inferring reachability from dialogue, perception, model output, or
coordinates. `CharacterTargetAccessRequirement` retains one exact target character, one canonical
required capability key, and one exact required knowledge reference. Its pure policy succeeds only
for a distinct value-matching actor and target when the unchanged interaction snapshot, actor
capabilities, and retained knowledge satisfy every requirement. These contracts do not define a
delay or influence proposal, occurrence, elapsed time, relationship effect, or runtime authority.

`CharacterSabotageActionKind` defines only the seven closed canonical roadmap kinds—`conceal`,
`move`, `substitute`, `contaminate`, `misquote`, `delay`, and `influence`—as identity-preserving
singletons in stable order. Parsing accepts only those exact values. A kind alone has no actor,
target, access requirement, proposal, occurrence, effect, recovery consequence, evidence, scene,
persistence, or provider authority.

`CharacterSabotageActionConsiderationRequest` retains one exact actor and a stable defensive copy
of distinct caller-supplied canonical action kinds in caller order. It permits an empty request and
enumerates the supplied sequence once. The caller remains responsible for including only kinds
whose action-specific physical, informational, capability, and current-state gates have already
passed; the request does not infer eligibility, choose a target, or authorize an occurrence.

`CharacterSabotageActionDecision` retains the exact deciding actor identifier and either one exact
request-member action kind or explicit abstention. The pure decision policy requires identifier
reference identity and exact request membership, and an immutable consideration result binds only
an accepted decision to its exact request. These contracts select only a kind: they do not select
or construct an action-specific candidate, apply an effect, or grant scene or provider authority.

`ICharacterSabotageActionDecisionModel` is the provider-neutral Application seam for one exact
action-kind request. Its controller rejects null and pre-cancelled input, forwards the exact request
and caller cancellation once, and constructs the existing validated result. The interface and
controller themselves add no provider binding, retry, deadline, background task, action-specific
dispatch, or authoritative mutation.

`CharacterSabotageActionModelBinding` retains one exact OpenAI setting and one exact caller-composed
decision model without invoking it, accessing a credential, or performing network I/O.

`CharacterSabotageActionDecisionJsonDeserializer` accepts only one bounded complete JSON object
containing exactly `actorId` and nullable `selectedActionKind`. It ordinally rebinds value-matching
text to the request's exact actor identifier and exact member kind, delegates final acceptance to
the Domain policy, and rejects every other shape or value through one content-free failure. It does
not itself define a schema, serialize a request, or bind a provider.

The matching OpenAI stream boundary accepts only one ordered Responses lifecycle, bounds semantic
events and accumulated output, and completes framed SSE only after that strict decoder accepts an
exact selection or abstention. It does not send HTTP, access credentials, retry, or retain partial
or failed output. The request serializer emits one stateless streaming structured-output request
with bounded trusted actor and caller-filtered action-kind context plus a closed exact schema.
Descriptive actor text remains in-world data; no credential, target, candidate, capability set,
knowledge, or world state is added.

The OpenAI adapter loads a valid credential before its single caller-owned HTTP send, requires a
successful event-stream response, preserves caller cancellation, and completes only through that
strict SSE boundary. Its factory creates the exact immutable binding over caller-owned settings,
HTTP, credential, and endpoint dependencies without construction-time credential access, network
I/O, or model invocation; failures remain bounded and omit provider bodies and partial output.

`TemporaryCharacterSabotageActionModelSource` owns an exact configured binding through
initialize/expected-instance replace/clear operations, gives one supplied one-shot model priority,
and otherwise returns one stable fail-closed offline model. Taking or changing a model never invokes
it, and clearing a binding preserves any queued one-shot model.

`TemporaryCharacterSabotageActionModelComposition` creates bindings only through the exact OpenAI
factory and retains its installed binding as the expected-instance token for replacement and clear.
The shared selected-provider composition controller activates or replaces that seam for OpenAI and
clears only its exact binding for a supported Ollama selection, restoring the same stable offline
model while preserving a queued one-shot. Composition itself performs no credential access, HTTP
send, or model call.

`CharacterSabotageActionTaskController` owns one bounded scene task, rejects overlap, supports one
cancellation request and explicit terminal reset, and installs only an accepted exact result on the
main thread. Cancellation, model failure, and Domain rejection expose no result.

`TemporaryCharacterSabotageActionConsiderationFlowController` composes that exact source and task
only when its public `TryStart` is called with an already-authored request. It checks task idleness
before consuming a model, so rejected overlap preserves a queued one-shot model. Ordinary startup
is inert. The main scene dependency-orders the source, task, and explicit flow, but no node invokes
it automatically. The flow does not create requests, schedule work, dispatch an action, or mutate
world state.

`CharacterSabotageActionSelectionCoordinator` exposes one exact completed selection or abstention
only when a caller explicitly observes it. Duplicate, idle, running, cancelled, failed, null, and
stale acknowledgement paths stay inert. Explicit acknowledgement resets only the same exact task
result and clears the observation. The main scene dependency-orders the coordinator after the task
and flow, but neither startup nor the coordinator polls, dispatches, applies, or acknowledges an
action automatically.

`CharacterSabotageActionConcealmentRequestController` authors a one-shot sabotage-action request
only from the existing source's exact enemy actor, physical access, world objects, and concealment
candidate plus the scene controller's exact current concealment and evidence snapshots. The
existing concealment policy must still accept that complete graph. The request retains only the
exact actor and canonical `Conceal` kind; unavailable, stale, value-equal replacement, ineligible,
null, and repeated inputs remain inert. This isolated controller is not in `Main.tscn` and does not
start a model or task, observe a selection, dispatch or apply concealment, or change source or
authoritative state.

`CharacterSabotageActionMoveRequestController` applies the same one-shot exact-state boundary to
the existing move source and scene state. It requires the source's exact enemy actor, physical
access, world objects, destination, and candidate together with the move controller's exact current
placements and evidence, and the existing move policy must still accept that graph. The authored
request retains only the exact actor and canonical `Move` kind. The controller remains isolated
from `Main.tscn` and does not combine kinds, start work, dispatch or apply movement, or mutate any
source or authoritative state.

`CharacterSabotageActionSubstitutionRequestController` authors only the canonical `Substitute`
request after exact-reference checks join the existing substitution source to the scene's current
substitution and evidence snapshots and the existing policy accepts the unchanged graph. Its
one-shot output retains the exact enemy actor. It remains isolated and does not combine kinds,
start or observe work, dispatch or apply substitution, or mutate source or authoritative state.

`CharacterSabotageActionContaminationRequestController` provides the corresponding isolated gate
for the canonical `Contaminate` kind. It accepts only the existing source's exact enemy actor,
physical access, world objects, and evidence candidate with the contamination controller's exact
current contamination and evidence snapshots, then delegates to the existing contamination
policy. Unavailable, stale, replacement, ineligible, null, and repeated inputs remain inert and no
model, task, dispatch, application, or mutation follows from the authored request.

`CharacterSabotageActionMisquotationRequestController` binds the canonical `Misquote` kind to the
temporary source's complete exact actor, heard source event, composed delivery, occurrence,
witness, and evidence-candidate graph. The existing witness-evidence policy must still prove actor
capability and both hearings. The isolated one-shot gate does not deliver dialogue, expose
evidence, combine kinds, start consideration, or mutate any source or authoritative state.

`CharacterSabotageActionDelayRequestController` accepts one already-bounded target-delay
consideration request only when every supplied actor, interaction-access, world-event, event-log,
world-time, and member-candidate reference is exact, the scene delay controller still owns those
current state snapshots, and the existing delay policy accepts the graph. It authors only the
canonical `Delay` kind and does not start consideration, record an event, advance time, or mutate
the action-specific request or state.

`CharacterSabotageActionInfluenceRequestController` similarly binds an existing bounded influence
request's exact actor, interaction access, current relationship, and exact member candidate to the
scene influence controller's still-current relationship. The existing influence policy must accept
the unchanged graph before the one-shot controller authors only the canonical `Influence` kind.
It does not start consideration, alter a relationship, record a reason, or mutate either request or
scene state.

`CharacterSabotageActionRequestComposer` reads the seven action-specific authors' exact current
outputs and accepts only a caller-held exact snapshot of those references for the same enemy actor.
It retains that exact output snapshot internally and creates one fresh request containing each
available singleton once in canonical roadmap order.
Empty, foreign-actor, value-equal replacement, wrong-kind, duplicate-kind, stale, null, and repeated
composition stays inert. The isolated composer does not rerun an action-specific gate, start the
existing consideration flow, select a candidate, dispatch or apply an action, or mutate any author
output or authoritative state.

`CharacterSabotageActionAuthorBindingCoordinator` explicitly reads one already-observed exact
selection only when its result belongs to the composer's exact request and all seven retained author
outputs remain the same current instances in canonical order. A selected kind exposes only its
corresponding exact singleton author request and its immutable strongly typed action-specific
candidate binding; abstention binds once without exposing either. Each author creates its candidate
binding atomically with the generic request, and the binding accepts only that exact request,
canonical kind, and candidate instance. The coordinator acknowledges only a still-current exact
bound result through the existing selection coordinator, retains exact internal acknowledgement
provenance, then retains the selected author request and candidate binding as later dispatch tokens.
Unavailable, unbound, running, cancelled, failed,
reset, stale, rebuilt, replaced, foreign-actor, wrong-kind, duplicate, null, externally
acknowledged, and repeated states remain inert. This isolated binding does not start an
action-specific consideration, dispatch or apply an action, author evidence, or mutate composer,
authors, authoritative state, or a queued model. The main scene dependency-orders the binder after
all of those owners without polling it; the existing switch-gated request smoke explicitly
observes, binds, and acknowledges one live conceal selection while ordinary startup stays inert.

`CharacterSabotageActionSpecificRequestCoordinator` accepts only the binding coordinator's exact
selected author request and strongly typed candidate binding after that generic result was
acknowledged. Seven compile-time-typed branches revalidate the corresponding live scene source,
authoritative snapshots, and existing action policy before creating exactly one existing
provider-neutral consideration request containing the selected exact candidate. Delay and
influence authors retain their exact upstream bounded request internally so the later gate can
revalidate event-log, world-time, interaction-access, and relationship context without rebuilding
it. Abstention, unavailable, pre-acknowledgement, stale, replaced, foreign, wrong-kind, null,
repeated, and ineligible states remain inert. Composition consumes no model and starts or applies
no action.

`CharacterSabotageActionSpecificRequestStartCoordinator` starts only that one exact composed
request. It rebinds the selected generic author token to the matching typed candidate without
reflection or untyped candidate casts, rechecks the exact current authority and existing policy,
and only then consumes the corresponding queued, configured, or offline action-specific model.
Every rejected preflight preserves queued model state, and a successful start is retained once by
its exact generic author request. The coordinator adds no scheduling or automatic startup.

`CharacterSabotageActionSpecificResultCoordinator` observes only the exact terminal result whose
request is the one started above and whose optional selection is the same exact typed candidate.
It retains one of seven strongly typed results and explicitly acknowledges that result only through
the existing concealment, move, substitution, contamination, misquotation, delay, or influence
result owner, preserving each owner's current-state and presentation/resolution checks. Ordinary
startup remains inert. The switch-gated main-scene smoke drives one live conceal selection through
generic acknowledgement, exact request composition, an explicit selecting action-specific start,
typed application, presentation, observation, and acknowledgement.

`CharacterSabotageActionSpecificOutcomeCoordinator` accepts only that acknowledged selected result
while its exact generic author request, strongly typed candidate binding, action-specific request,
and existing branch-owned post-action authority remain current. It retains only one typed outcome:
the exact object-state and evidence snapshots for concealment, movement, substitution, or
contamination; the exact current exposure for misquotation; or the existing exact applied delay or
influence result. Abstention, unavailable, pre-acknowledgement, stale, replaced, foreign,
wrong-kind, null, repeated, and non-current-authority paths remain inert. The coordinator performs
no transition, model access, recovery consideration, evidence removal, scheduling, or persistence.
The switch-gated main-scene smoke proves one exact selected concealment outcome while ordinary
startup remains inert.

`CharacterSabotageOutcomeEligibilityCoordinator` publishes at most one strongly typed retained
outcome for its existing recovery or evidence route. It requires the exact generic author request,
typed result and candidate binding, branch-owned acknowledgement snapshots, and current authority
retained through the outcome seam. Concealment, movement, substitution, and contamination also
delegate to their existing recovery policies with one caller-supplied evidence view; delay and
influence accept only the exact candidate already admitted by their existing one-shot recovery
candidate controller; misquotation retains only its exact current exposure and witness view. Null,
rebuilt, wrong-kind, stale, replaced, repeated, unacknowledged, ineligible, and non-current paths
remain inert. Publication starts no recovery or consideration, consumes no model, creates no
recovery request, and changes no source, evidence, perception, presentation, event-log, world-time,
relationship, or provider state. The switch-gated concealment smoke proves exact eligibility after
outcome acknowledgement while ordinary startup remains inert.

`CharacterSabotageOutcomeRecoveryCoordinator` routes the five non-targeted retained eligibility
branches through their existing owners. Focused Delay and Influence coordinators own the two
targeted continuations. All recheck the generic author token, typed result and candidate,
branch-owned acknowledgement snapshots, eligibility output, and still-current authoritative state
immediately before delegation. Movement additionally retains the source's exact pre-move placement
token. Object recovery retains only the exact recovered concealment, placement, substitution, or
contamination snapshot and preserves authoritative evidence. Misquotation retains the exact current
exposure and witness view, then delegates once through the existing truth-neutral presentation route
without consuming evidence. Delay composes and explicitly starts only its exact recovery request,
then accepts only that request's exact selected recovery after the existing resolution owner has
installed fresh append-only event-log and world-time snapshots. It acknowledges that result once and
retains the exact completed consideration, applied recovery, and fresh snapshots as terminal generic
Delay output. Influence likewise composes and explicitly starts only its exact request after complete
preflight, initializes recovery authority from the exact current relationship, and consumes the
queued model only after every generic and typed provenance token remains current. It then accepts
only that request's exact selected recovery after the existing resolution owner has installed the
fresh restored relationship, acknowledges the result once, and retains the exact completed
consideration, applied recovery, and relationship as terminal generic Influence output. Rejected
calls cannot consume another model. Null, rebuilt, stale, replaced, foreign, wrong-branch,
ineligible, non-current, unresolved, occupied, and repeated calls remain inert. Neither coordinator
defines a new recovery rule. Separate switch-gated main-scene smokes carry every branch while
ordinary startup remains inert.

`CharacterSabotageTerminalRouteCoordinator` accepts only the exact selected generic author request
and canonical kind after exactly one of the seven existing route owners exposes its final current
output. It retains the exact recovered concealment, placement, substitution, contamination,
misquotation presentation, Delay recovery, or Influence recovery without copying it into a common
value-shaped result. Null, rebuilt, wrong-kind, incomplete, stale, multiple-branch, and repeated
inputs remain inert. An explicit exact reset clears only this coordinator's retained references
once and records the reset provenance; it does not clear or change any branch owner, authoritative
state, presentation, task, model source, or prior output, and it does not schedule another sabotage
action. The existing switch-gated route smokes prove all seven terminal bindings while ordinary
startup remains inert.

`CharacterSabotageTurnCompletionCoordinator` exposes one scene-owned completed-turn token only
after that exact terminal route has been reset, its retained output has been cleared, and exactly
one matching branch owner still retains its current authoritative result. The token retains the
reset request's exact actor, generic author request, and canonical action kind without copying the
route output. Null, pre-reset, rebuilt, foreign-actor, wrong-kind, stale-owner, multiple-branch, and
repeated inputs remain inert. `CharacterSabotageTurnSchedulingEligibilityPolicy` is a pure gate
that accepts only the coordinator's exact current token with coherent exact-reference provenance.
`CharacterSabotageTurnAdmissionCoordinator` retains that same exact current token once after the
gate accepts it, without copying its actor, request, kind, or route output. Null, rebuilt,
malformed-provenance, replaced, and repeated tokens leave the admission and every prior owner
unchanged. `CharacterSabotageOrdinaryTurnBoundaryCoordinator` binds the exact admission once only
while the exact current `CharacterTurnActivityController` activity passes the existing ordinary
turn-start policy; it retains that exact activity reference, including an empty slot, without
changing the controller. `CharacterSabotageOrdinaryTurnBoundaryEligibilityPolicy` purely
revalidates the exact admission and bound/current activity references immediately before any future
scheduling. `CharacterSabotageOrdinaryTurnSchedulingAuthorizationCoordinator` retains one fresh
authorization only from that exact still-current boundary graph. The authorization contains only
the exact admission and ordinary-turn references, including an exact empty slot; it does not copy
actor, request, kind, or route output. Null, rebuilt, replaced-admission, changed-slot, active-slot,
malformed-provenance, and repeated inputs remain inert. `CharacterSabotageCycleBoundaryCoordinator`
consumes only that exact current authorization once at an explicit boundary and retains one fresh
token containing the authorization itself, without copying its provenance. Null, rebuilt, stale,
changed-slot, and repeated inputs fail closed. `CharacterSabotageCycleStartEligibilityPolicy`
purely revalidates the exact current boundary, authorization, admission, and ordinary-turn graph
before any future action authoring; rebuilt or active-slot graphs fail closed.
`CharacterSabotageActionAuthoringAdmissionCoordinator` then retains one fresh admission containing
only that exact still-current boundary. Its actor projection follows the boundary's authorization
and completed-turn token rather than copying identity. Null, rebuilt-boundary, malformed-provenance,
and repeated inputs fail closed. None of these boundaries authors, starts, observes, acknowledges,
dispatches, applies, resets, reserves, or schedules another action, and ordinary startup remains
inert.

`CharacterSabotageActionMoveAuthoringCoordinator` accepts only that coordinator's exact current
authoring admission while the complete boundary graph remains current. It invokes a fresh instance
of the existing Move request author once for an exact follow-up candidate whose evidence identifier
is distinct from the retained first-cycle evidence. The guarded author retains its exact request and
typed candidate binding; the coordinator retains the exact admission and author request without
resetting the original author or changing recovered placement or evidence. Null, rebuilt,
malformed, stale, and repeated admission paths remain inert. This seam does not compose or start a
generic consideration request, reserve the ordinary turn, consume a model, clear prior ownership,
or enable automatic sabotage; ordinary startup remains inert.

`CharacterSabotageActionMoveCompositionCoordinator` accepts only that guarded author's exact
current output while its admission, typed Move candidate binding, cycle boundary, authorization,
completed-turn admission, and empty ordinary-turn slot remain current. It retains the exact
admission and guarded author request, then produces one fresh generic request containing the exact
actor and only canonical `Move`. Null, rebuilt-request, foreign-actor, wrong-kind, replaced-binding,
stale-graph, and repeated paths remain inert. It does not start consideration, consume or supply a
model, reset either author, reserve the ordinary turn, clear prior provenance, or enable automatic
sabotage; ordinary startup remains inert.

`CharacterSabotageActionMoveCompositionStartCoordinator` explicitly starts only that composition
coordinator's exact fresh request while its complete retained graph remains current. It uses a
fresh scene-owned task and one-shot model source, preserving the queued model across null, original
author-output, rebuilt-request, stale-graph, and other rejected preflight failures. A successful
start retains the exact request and consumes the queued model once without observing or
acknowledging the result, selecting or dispatching an action, resetting either author, reserving the
ordinary turn, clearing prior provenance, or enabling automatic cadence. Ordinary startup remains
inert.

`CharacterSabotageActionMoveResultObservationCoordinator` observes only the exact completed result
of that fresh generic Move task while the complete admission, authoring, composition, and start
graph remains current. It retains the exact result once without acknowledging it. The paired result
binding coordinator then accepts only that observation and the guarded author's exact typed
follow-up candidate binding, acknowledges the same exact task result, and retains both tokens after
the task resets. Null, running, rebuilt-result, rebuilt-request, foreign-actor, wrong-kind,
stale-graph, replaced-binding, and repeated paths remain inert.

`CharacterSabotageActionMoveSpecificRequestCoordinator` composes only that exact acknowledged
result and follow-up candidate into one fresh provider-neutral typed Move request over the exact
current placement and evidence snapshots. Its explicit start coordinator revalidates the complete
graph and starts only that request on a separate fresh task, preserving its queued model across
rejected preflight. These seams do not reset either author, reserve the ordinary turn, clear prior
provenance, add automatic cadence, or change ordinary startup behavior. The newly started typed
result is observed only after that exact task completes, selects the retained follow-up candidate,
and installs fresh authoritative placement and evidence snapshots. The observer retains that exact
result and those exact live snapshots without requiring the request's pre-application snapshots to
remain current.

`CharacterSabotageActionMoveAppliedResultBindingCoordinator` binds only that exact observation to
the same exact authoritative snapshots. It then acknowledges only the bound result, resets only the
scheduled typed task, clears the observer, and retains the exact acknowledged result, placements,
and evidence. Rebuilt result, request, candidate, placement, evidence, stale, null, running, and
repeated paths remain inert. No terminal route, recovery, turn completion, ordinary-turn
reservation, author reset, or additional cycle is started, and ordinary startup remains inert.

The acknowledged follow-up Move result can explicitly author one exact recovery candidate from its
typed request's exact pre-move source placement while the retained result and fresh authoritative
snapshots remain current. A separate one-shot eligibility seam pairs only that candidate with an
explicitly supplied evidence view accepted by the existing recovery policy. The recovery seam then
delegates only that exact pair to the existing stale-safe Move controller, retains the fresh restored
placement snapshot, and preserves authoritative evidence. Missing or ambiguous source placement,
null, rebuilt, foreign, unseen-evidence, stale, and repeated paths remain inert. These seams do not
reset either author, reserve the ordinary turn, start another cycle, invoke another model, or add
automatic startup behavior.

The recovered follow-up Move can then retain one exact second-cycle terminal route from only the
guarded author request, canonical Move kind, and fresh recovered placement while that complete graph
remains current. An explicit reset clears only those retained route references once and records the
exact author/kind provenance without changing the recovery owner or authoritative state. A separate
completion owner exposes one fresh `CharacterSabotageTurnCompletion` for only that exact reset route.
The existing pure scheduling gate admits only its exact current token, a distinct one-shot admission
owner retains that token, and a distinct ordinary-turn boundary binds it only while the exact current
turn slot remains empty. Null, rebuilt, foreign-actor, wrong-kind, replaced-placement, stale,
active-slot, and repeated paths remain inert. These seams do not reserve the slot, authorize scheduling,
start another cycle, clear prior owners, consume another model, or enable automatic sabotage.

The still-current follow-up boundary can now retain one fresh ordinary-turn scheduling authorization
containing only its exact admission and empty turn reference. A distinct explicit cycle owner consumes
only that exact authorization once, and the existing pure cycle-start gate revalidates the resulting
boundary before a distinct one-shot admission owner retains it for future authoring. Null, rebuilt,
malformed-provenance, changed-slot, stale-route, stale-recovery, foreign-chain, and repeated paths
remain inert. These seams do not reserve the ordinary turn, invoke an action author, compose or start a
request, reset either prior author, clear prior provenance, consume another model, or enable automatic
sabotage.

That exact cycle admission now guards a third scene-owned Move author using one fresh source-owned
candidate whose evidence identifier is distinct from both earlier Move cycles. The recovery owner
retains and revalidates the exact authoritative evidence snapshot as well as the recovered placement,
so replaced authority fails closed. A separate composition seam produces one fresh generic Move
request from only that guarded output, and an explicit start seam consumes one separately supplied
model on a fresh scene-owned task only after the complete admission, authoring, composition, recovery,
and empty-turn graph is revalidated. Null, rebuilt, foreign, wrong-kind, stale-authority, and repeated
paths remain inert and preserve the queued model before successful start. A distinct observer retains
only the exact completed result while that full graph remains current. The adjacent binding seam pairs
only that exact observation with the third author's exact typed candidate, then acknowledges the same
bound result and resets only its task while retaining both exact downstream tokens. Null, running,
rebuilt-result/request, foreign-actor, wrong-kind, replaced-binding, stale-graph, and repeated paths
remain inert. Those tokens can now compose one fresh provider-neutral typed Move request over the
exact current recovered placement and evidence snapshots. An explicit start seam revalidates that
complete graph before consuming one separately supplied model on a distinct typed task. Rejected
preflight preserves the queued model. A separate observer accepts only the exact completed selected
result and the exact fresh placement and evidence snapshots installed by that task; it relies on
retained exact acknowledgement provenance after application rather than requiring the request's
pre-application snapshots to remain current. The adjacent binding seam accepts only that exact
observation and authoritative outcome, then resets only the typed task while retaining the exact
result and snapshot tokens. One recovery-candidate owner derives the unique exact pre-move source
placement from that retained typed request; a separate eligibility owner pairs only that candidate
with one explicitly supplied current evidence view accepted by the existing policy. The recovery
owner delegates only that exact pair to the stale-safe Move controller, retains the fresh restored
placement and unchanged authoritative evidence, and stops before terminal routing. Null, rebuilt,
foreign, unseen-evidence, stale-authority, running, and repeated paths remain inert. These seams do
not reset a prior author, reserve the ordinary turn, add automatic cadence, contact a provider, or
change ordinary-startup behavior.

That exact recovered third-cycle Move can now retain one distinct terminal route over the guarded
author request, canonical Move kind, and exact recovered placement and evidence authority. Its
explicit reset clears only the route references once while preserving recovery authority and
recording the exact author/kind provenance. A separate completion owner exposes one fresh
`CharacterSabotageTurnCompletion` from only that exact reset route, and a distinct admission owner
retains it only after the existing pure scheduling gate accepts the same exact token. Null, rebuilt,
foreign-actor, wrong-kind, replaced-authority, stale, and repeated inputs remain inert. These seams
do not reserve the ordinary turn, authorize another cycle, clear upstream provenance, consume
another model, or enable automatic sabotage. A separate ordinary-turn boundary now binds that exact
admission once only while the scene's exact turn reference is still empty and startable. It retains
the exact completion and empty reference without creating a request, reserving the slot, or changing
any prior owner or authoritative snapshot.

That third-cycle ordinary-turn boundary can now retain one fresh scheduling authorization after
revalidating its exact admitted completion and still-empty turn reference. The authorization keeps
only those exact references; it does not reserve or replace the turn, enter another sabotage cycle,
invoke an author, or consume a model.

That exact authorization can now be consumed once at a distinct explicit continuation boundary.
The boundary retains the authorization itself, revalidates its complete current graph, and does not
copy completion provenance, reserve the ordinary turn, invoke an author, or start another action.

The continuation boundary can now pass the existing pure cycle-start gate into one distinct
action-authoring admission. Only the exact still-current boundary is admitted once; rebuilt,
malformed, stale, and repeated references remain inert, and no action-specific author is invoked.

That exact admission can now guard one fresh invocation of a distinct Move author over a fourth
eligible candidate with its own evidence identity. The author retains the exact continuation
admission, request, typed candidate binding, and current third-cycle recovery authority. It does not
reset any prior author, compose or start consideration, change placement or evidence state, reserve
the ordinary turn, or consume a model.

The exact continuation-guarded author output can now compose one fresh generic Move request after
revalidating the full admission, recovery, candidate-binding, and empty-turn graph. Composition
retains the author tokens and creates no task, model call, decision, state change, or cadence.

An explicit continuation start seam can now consume one separately supplied model on a fresh
scene-owned task only after the exact composed request and its complete retained graph remain
current. Null, rebuilt, author-owned, stale, occupied, and repeated starts fail before model
consumption. Successful start leaves its result unobserved and authoritative state unchanged.

A distinct observer can now retain only the exact completed generic Move result installed by that
fresh task while the complete continuation graph remains current. Running, null, rebuilt-request,
foreign, wrong-kind, stale, and repeated results remain inert; observation does not acknowledge the
result, reset the task, bind its candidate, dispatch it, or alter authoritative state.

The adjacent continuation binding seam can now pair only that exact observation with the fourth
author's exact typed candidate binding. Null, rebuilt, wrong-request, stale-author, and repeated
inputs remain inert. Binding retains both references without clearing the observation, resetting
the task, composing a typed request, dispatching, or applying the candidate.

Only that bound continuation pair can now be acknowledged once. Acknowledgement resets the fresh
generic task and clears its observation while retaining the exact result, candidate binding, author
output, and current recovery graph as downstream provenance. It starts no typed work and changes no
authoritative snapshot or prior owner.

Those exact continuation acknowledgement tokens can now compose one fresh provider-neutral typed
Move request over the current authoritative placement and evidence snapshots and the exact fourth
candidate. Composition revalidates eligibility and retained provenance, but does not start the typed
task, consume a model, apply movement, author recovery, or alter the ordinary turn.

An explicit typed continuation start seam can now consume one separately supplied Move model on a
fresh scene-owned typed task only while that exact request and its complete retained graph remain
current. Rejected preflight preserves the queued model. Successful start leaves completion
unobserved and does not itself reset the task, recover the Move, reserve the ordinary turn, or add
automatic cadence.

Only the exact completed applied typed continuation result can now be observed with the fresh
placement and evidence snapshots installed by its task. The adjacent binding seam accepts and
acknowledges only that exact observation and current authority, resets only the typed task, and
retains the result and snapshots as downstream provenance. A separate recovery-candidate owner
derives the unique exact pre-move source placement from that retained request, and an eligibility
owner pairs only that candidate with an explicitly supplied visible-evidence view accepted by the
existing policy. The recovery owner delegates only that exact pair to the stale-safe Move state
controller, retaining the fresh restored placement and unchanged authoritative evidence. Null,
running, rebuilt, foreign-actor, wrong-candidate, unseen-evidence, replaced-authority, stale, and
repeated inputs remain inert. No terminal route, turn completion, ordinary-turn reservation,
automatic cadence, provider, credential, persistence, UI/input, navigation, perception, dialogue,
timer, or new Domain behavior is added.

That exact recovered continuation Move can now retain one distinct terminal route over the guarded
author request, canonical Move kind, retained generic acknowledgement and candidate binding, and
the exact current recovered placement and evidence authority. Its explicit reset clears only the
route references once while preserving recovery authority and recording the exact author/kind
provenance. A separate completion owner exposes one fresh `CharacterSabotageTurnCompletion` from
only that exact reset route, and a distinct admission owner retains it only after the existing pure
scheduling gate accepts the same exact token. Null, rebuilt, foreign-actor, wrong-kind,
replaced-authority, stale, and repeated inputs remain inert. These seams do not reserve the ordinary
turn, clear upstream provenance, invoke another model, authorize another cycle, or enable automatic
sabotage.

That admitted continuation completion can now bind once to the ordinary-turn boundary only while
the scene's exact current turn reference remains empty and startable. A separate authorization
owner then retains one fresh scheduling token over only that exact completion and empty reference.
An explicit successor boundary consumes only that exact still-current authorization, and the
existing pure cycle-start gate admits only that exact boundary once for future action authoring.
Null, rebuilt, malformed, changed-slot, active-slot, stale, and repeated references remain inert.
These seams do not reserve or replace the turn, invoke an author or model, enter an action-specific
flow, clear prior owners, or enable automatic sabotage; ordinary startup remains inert.

That successor admission now guards one fresh fifth-cycle Move author invocation with a distinct
source-owned evidence identity. The author accepts the candidate only as an exact caller-supplied
reference after revalidating the complete successor-boundary and continuation-recovery graph. A
separate composition seam accepts only that exact guarded output and produces one fresh generic
request containing the exact actor and canonical `Move` kind. Null, rebuilt, malformed, prior-cycle,
duplicate-evidence, stale-authority, and repeated inputs remain inert. Neither seam starts
consideration, supplies or consumes a model, reserves the ordinary turn, changes placement or
evidence authority, clears prior provenance, or enables automatic sabotage; ordinary startup
remains inert.

That exact fifth-cycle generic request can now start once on its own fresh scene-owned task and
fresh one-shot model source only after the complete retained successor graph is revalidated. A
separate observer retains only the exact completed task-installed result for the exact started
request and canonical `Move` selection. Null, guarded-author-output, rebuilt-request,
foreign-actor, wrong-kind, running, stale-graph, replaced-authority, and repeated inputs fail closed
without consuming the queued model during rejected preflight. The result remains unacknowledged,
the task remains completed, and no typed Move request is composed or started. Earlier model-call
counts, ordinary-turn state, placement, evidence, and all retained provenance remain unchanged;
ordinary startup remains inert.

That exact fifth-cycle observation can now bind once to only the fresh author's exact source-owned
Move candidate binding. A separate explicit acknowledgement accepts only that same still-current
bound result, resets only its completed generic task, clears the observation, and retains the exact
result and candidate provenance. Null, rebuilt-result, wrong-candidate, stale-graph,
replaced-authority, and repeated inputs remain inert. Neither seam composes or starts typed Move
work, dispatches or applies an action, reserves the ordinary turn, clears prior provenance, or
changes placement or evidence authority; ordinary startup remains inert.

Those exact fifth-cycle acknowledgement tokens can now compose one fresh provider-neutral typed
Move request over the current authoritative placement and evidence snapshots and only the exact
source-owned fifth candidate. An explicit start seam consumes one separately supplied Move model
on fresh scene-owned owners only while that request and its complete retained graph remain current.
Null, rebuilt-result, rebuilt-binding, wrong-candidate, stale-authority, occupied, and repeated
inputs remain inert, and rejected preflight preserves the queued model. Successful start leaves
completion unobserved and does not acknowledge or reset the task, apply movement, reserve the
ordinary turn, clear provenance, or enable automatic sabotage; ordinary startup remains inert.

Only that exact fifth-cycle typed task's completed applied result can now be observed together with
the fresh placement and evidence snapshots installed by the task. A separate binding seam accepts
only those exact three observed tokens and explicitly acknowledges them, resetting only the
completed typed task while retaining the result and authoritative snapshots for future recovery
composition. Null, running, rebuilt-request, rebuilt-result, foreign-actor, wrong-candidate,
stale-authority, stale-graph, substituted-snapshot, and repeated inputs remain inert. Neither seam
authors recovery, recovers movement, reserves the ordinary turn, clears earlier provenance, or
invokes another model; ordinary startup remains inert.

Only that exact acknowledged fifth-cycle applied result can now author one recovery candidate from
its source-owned Move candidate and unique pre-move source placement. The candidate retains one
exact caller-supplied evidence view only when the existing recovery policy accepts it against the
retained authoritative placement and evidence snapshots. A separate admission seam publishes only
that exact candidate/view pair, and a stale-safe recovery seam delegates it through the existing
state owner, installing one fresh restored placement snapshot while retaining authoritative
evidence. Null, rebuilt-result, rebuilt-candidate, substituted-snapshot, unseen-evidence, stale,
and repeated inputs remain inert.

That exact recovered fifth-cycle Move can now be retained as one terminal route only with the exact
task-owned generic author request, canonical `Move` kind, and recovered placement/evidence outputs.
An explicit reset clears only that exact route once while preserving its reset provenance and the
recovered authoritative state. Null, rebuilt-author, wrong-kind, substituted-snapshot, stale-graph,
and repeated inputs remain inert.

Only that exact reset fifth-cycle terminal route can now expose one fresh
`CharacterSabotageTurnCompletion`. A separate admission owner accepts only that exact current token
through the existing pure scheduling gate. Null, rebuilt-completion, rebuilt-author, foreign-actor,
wrong-kind, substituted-recovery-snapshot, stale-graph, and repeated inputs remain inert. Neither
seam binds or reserves the ordinary turn, starts a sixth sabotage cycle, clears earlier provenance,
changes recovered authority, or invokes another model; ordinary startup remains inert.

That exact admitted fifth-cycle completion can now bind once to the still-empty ordinary-turn
reference after revalidating its complete completion, recovery, and admission graph through the
existing pure boundary gate. A separate owner retains one fresh scheduling authorization only from
that exact still-current boundary. Null, rebuilt-admission, substituted non-empty activity,
stale-graph, and repeated inputs remain inert. Neither seam reserves, replaces, starts, or cancels
an ordinary turn, enters a sixth sabotage cycle, clears earlier provenance, changes recovered
authority, or invokes another model; ordinary startup remains inert.

That exact fifth-cycle authorization can now enter one explicit successor boundary and pass the
existing pure cycle-start gate once after revalidating the complete retained graph. Null, rebuilt
authorization, rebuilt admission, substituted activity, rebuilt or malformed boundary, stale-graph,
and repeated inputs remain inert. The resulting admission retains only the exact boundary; it does
not invoke an author or model, compose or start consideration, reserve an ordinary turn, or begin a
sixth sabotage action. This closes the repeated Move-cycle proof boundary so Phase 7 can pivot to
acceptance-level behavior rather than extend the same chain again; ordinary startup remains inert.

`CharacterSabotageActionRequestConsiderationFlowController` connects only the composer's exact
current request to the existing explicit consideration flow. Stale, null, overlapping, and repeated
starts fail closed before replacing or consuming a queued one-shot model. Exact selection and
abstention remain task results only: this isolated coordinator does not observe or acknowledge a
result, dispatch or apply an action, select an action-specific candidate, or mutate author output or
authoritative state. The main scene dependency-orders all seven authors, the composer, and this
coordinator while ordinary startup remains inert. A switch-gated smoke authors one exact live
conceal-only request and completes its selection without observation, acknowledgement, dispatch,
application, or authoritative-state mutation.

`CharacterSabotageOrdinaryTurnAuthoringCoordinator` now accepts only the exact current awaiting
enemy-agent turn and the exact scene-owned actor, preflights the complete seven-author graph, invokes
the existing authors in canonical order, and composes only their exact eligible outputs. The
temporary scene-owned target-action source provides representative exact Delay and Influence inputs
without starting either action. Null, rebuilt-turn, replacement-actor, stale, already-authored, and repeated
graphs stay inert without partial publication. A separate ordinary-turn start coordinator retains
that exact turn and composed request and delegates one explicit provider-neutral generic decision to
the existing task flow. One observation coordinator accepts only that exact task-installed completed
selection while the complete turn, authoring, selected-kind output, candidate binding, and scene graph
remain current. A separate binding coordinator delegates the existing author-provenance binding and
acknowledges only that exact observed result, leaving its selected Delay request and candidate retained
while resetting only the completed generic task. One adjacent coordinator revalidates that entire
acknowledged graph and delegates the existing action-specific seam to compose one exact typed Delay
request. A separate start coordinator accepts only that exact request, preserves its queued model across
rejected preflight, and delegates the existing typed start once. One result coordinator accepts only its
exact task-installed completed selection, delegates the existing typed observation and Delay resolution
and acknowledgement owners, and retains the exact result, fresh event log and world time, and applied
result. Post-application checks use retained acknowledgement provenance rather than rerunning the stale
pre-action authority predicate. One adjacent outcome coordinator delegates the existing action-specific
outcome binder for only those exact retained tokens. Four adjacent recovery coordinators then publish
the exact Delay recovery candidate and eligibility token, compose and explicitly start its exact
provider-neutral request, and retain and acknowledge only its exact completed applied recovery with
fresh authoritative event-log and world-time snapshots. Four terminal coordinators validate that full
retained graph, delegate the existing terminal-route retention and reset owners, expose one exact
completed-turn token, and admit only that token through the pure scheduling gate. Null, rebuilt,
foreign, replaced-authority, wrong-kind, already-published, and repeated inputs stay inert without
consuming queued one-shot models. Its switch-gated main-scene smoke proves eligible Conceal, Delay, and
Influence outputs beside four absent kinds through one applied and recovered Delay, terminal reset, and
completion admission. A sabotage-specific lifecycle coordinator then retains the exact admitted
completion and recovered authority, replaces only its exact response-free `AwaitingResponse` activity
with a distinct terminal `Resolved` successor that keeps the same request, and binds only that exact
successor through the existing ordinary-turn boundary. It neither fabricates speech nor delegates
player cancellation. Rebuilt turn, completion, or recovery tokens, active-boundary attempts, stale or
already-resolved activities, and repeated calls remain inert while all three model-call counts stay at
one. The proof stops before scheduling another action. Ordinary startup remains authoring-, model-,
mutation-, scheduling-, and presentation-free, and no sixth Move cycle is introduced.

The ordinary scene now owns a separate bounded sabotage cadence measured only in authoritative
meaningful-world-event occurrences, not frames, wall-clock time, or the independent-task travel
clock. One exact due event can produce one opportunity for the existing scene-owned enemy actor.
An explicit continuation can reserve the shared character-turn slot without fabricating dialogue, so
a normal dialogue turn cannot overlap it. Only the exact current reservation invokes the seven existing
authors, composes eligible Conceal, Delay, and Influence outputs in canonical order, starts the existing
provider-neutral generic decision flow, and observes its exact completed result once. The reserved path
now binds and acknowledges that selection through its exact author provenance, composes and explicitly
starts its typed Delay request, applies and acknowledges the exact completed result, and binds the
authoritative event-log and world-time outcome. It then publishes the exact scene-owned recovery
candidate and eligibility token, composes and explicitly starts the provider-neutral recovery request,
applies and acknowledges its exact completed recovery, retains and resets the exact terminal route,
and exposes and admits one exact completed-turn token while retaining the same reservation. The shared
turn owner can then stage only one exact actor-bound response-free activity without exposing that
pending token to ordinary dialogue processing. The existing lifecycle owner resolves only that exact
activity, the ordinary boundary binds only its terminal successor, and the shared turn owner releases
only the same exact reservation. A separate access-gated investigation coordinator accepts only the
released route's exact recovered Delay history and scene-authored Seeker interaction access, then
publishes one filtered `CharacterEvidenceViewSnapshot` containing only the authored
`sage-delay-history` identifier. The matching pure Domain investigation boundaries also cover recovered
Influence history; they reject inaccessible targets, rebuilt candidates or recovery results, stale
relationship authority, and incomplete Delay history before exposing evidence.
Null, rebuilt actor, cadence, event, opportunity, reservation, request, stale authority, early,
duplicate, and repeated inputs remain inert and preserve queued one-shot models. Ordinary startup
without an eligible meaningful event performs no sabotage work. The explicit proof stops after exact
release and evidence exposure without scheduling another opportunity, action, or cadence cycle.

`RepresentativeOpeningRegionSource` begins Phase 8 with one immutable authored topology containing the
sickhouse, healer's garden, village square, village edge, stream, abandoned laundry, woodland clearing,
old shrine, southern road, abandoned wagon, and closed deeper-forest path. Exact bidirectional
connections provide alternate routes between the water, woodland, shrine, and road branches. The
snapshot rejects duplicate landmarks, duplicate undirected connections, foreign endpoints, and a
non-retained entry; the pure continuity policy proves every landmark reachable from the exact sickhouse
entry.

`OpeningRegionProjection` now binds one exact retained anchor to every topology location inside an
explicit 3200-by-1800-pixel gameplay region. Every anchor retains at least 24 pixels of route-endpoint
clearance and 120 pixels of landmark separation. Missing, duplicate, foreign, rebuilt, non-finite,
out-of-bounds, and insufficiently separated inputs fail before a projection is published. Projected
connections retain their exact authored topology edge and exact endpoint anchors. The main-scene proof
asks the synchronized Godot navigation map for all twelve routes and checks the free-space probes around
all eleven endpoints, rather than inferring traversal from topology or straight-line distance.

The first Phase 8 landmark scene is a temporary greybox sickhouse containing the exact representative
deep-sleeping patient. `OpeningRegionFirstPatientSceneBinding` accepts only the projection's exact
sickhouse entry anchor, the structurally valid exact deep-sleep infection, and a patient position within
96 pixels of that anchor. `OpeningRegionSickhouseLayout` retains that exact binding, a single north-wall
entrance at least twice the 24-pixel route clearance, and the exact sickhouse anchor as the patient
approach. Five collision-backed wall sections replace the former rectangle. The synchronized navigation
surface admits the entrance corridor and room interior while excluding wall clearance, and the pure
route policy rejects wall-crossing or out-of-bounds paths. A main-scene proof routes the Seeker into the
room, to the patient approach, and back out. After the exact remedy batch is prepared, the mapped T
action can administer its full remaining quantity while the Seeker is within 64 pixels of the exact
current patient, including the authored patient approach, and the scene retains the exact binding,
treatment context, batch, disease truth, and authored administration event. The command overlay reports
when the Seeker must move closer and confirms administration while asking the player to remain for
testimony. That one provider-neutral operation atomically consumes the batch,
stabilizes the deep-sleep infection, advances the exact world event, and records both administration and
universal experiments. Far, stale, rebuilt, wrong-context, repeated, released, and unrelated inputs are
inert. The exact stabilized administration then authorizes one fixed provider-neutral patient testimony
heard only by the in-range Seeker: it exposes ordinary authored copy, not disease truth or provider
content, remains one-shot, and updates the recovery status with the next sage step. Focus, discovery,
persistence changes, and production art remain deferred. Once that exact testimony has been heard,
the mapped K action can consult
the existing village sage only within 64 pixels of the visibly labelled purple sage at the village
square. One fixed provider-neutral reply
preserves the redundant stream, shrine, and wagon investigation routes instead of prescribing a clue
order; exact stabilized disease state, administration record, retained testimony, sage, Seeker, and
positions are all required. Rejected K input reports the missing testimony or required proximity, and
accepted input reports that the sage was consulted and prompts Q. The consultation is one-shot and does
not invoke a model, mutate truth, choose evidence, or change persistence. Explicit journal composition
retains the exact patient
testimony followed by the exact sage consultation, deduplicates the current last event by reference,
and uses the existing visibility-safe journal persistence shape without a schema change.

Save/reopen continuity now covers the ordinary recovery loop explicitly. A save captured before
preparation resets the transient activity to the authored initial ingredients; one containing the
full batch reconstructs its exact completed preparation and can administer it once. The untouched
initial disease snapshot reuses the exact authored instance only when every saved disease field
still matches, while progressed truth remains freshly rebound. After stabilization, version 8's
closed recovery stage distinguishes pending testimony from already-heard testimony and completed
sage consultation. Restoration rebuilds runtime dialogue tokens from that marker only after exact
canonical treatment, administration event, patient, sage, and Seeker validation; journal text is
never used as authority. Live capture appends missing exact heard recovery tokens only to the
content-safe persisted journal copy, retaining all existing entries, order, and section selection.
Versions 1 through 7 migrate to not-heard. Switch-gated scene coverage captures all five boundaries
from live controller state and proves one-shot continuation plus fail-closed incompatible progress
without substituting for direct play evidence. One uninterrupted fresh graphical checkpoint has now
completed and acknowledged the Stream/laundry route, prepared and administered the full remedy, heard
the patient testimony, consulted the sage, saved schema version 8, fully closed, reopened, and loaded.
The restored UI reported `LOADED` and showed exactly one patient and one sage entry in the Testimony
section. This closes the bounded Phase 8 playtest item without claiming the separate
independent-new-player acceptance criterion.

The second Phase 8 landmark scene is a temporary greybox healer's garden and preparation station.
`OpeningRegionRemedyPreparationSceneBinding` accepts only the projection's exact retained
`healer-garden` anchor, the structurally valid first-patient context and its exact remedy recipe, and a
station position within 72 pixels. The reachable scene shows garden beds, a bench, and mortar without
starting or advancing preparation. The mapped P action now advances one provider-neutral ordinary flow
only while the Seeker is within 48 pixels of the exact current station and the scene retains the exact
current binding, treatment context, recipe, ingredient state, and complete first-patient state. Each
press starts the recipe, advances exactly one next authored grinding, heating, steeping, or cooling step,
completes the exact activity, or installs its exact full prepared batch. Far, stale, rebuilt,
wrong-context, repeated, released, and unrelated inputs are inert. Apothecary access remains optional
and additive. The temporary command overlay reports when the Seeker must move closer to the station,
then reports preparation start, each of the four completed steps, completion, and the installed batch
ready for treatment. This flow does not administer a dose, stabilize the patient, write journals,
persist partial preparation activity, call a provider, or add production presentation.

The village-square and village-edge transition scene binds only the projection's exact retained anchors
to separate bounded areas with 24-pixel endpoint clearance. Thirteen collision-backed boundary sections
leave explicit garden, square, edge, stream-branch, and woodland-branch openings. The synchronized
navigation surface excludes wall clearance while retaining every authored opening-region route, and a
switch-gated proof drives the Seeker garden -> square -> edge and back through both navigation paths and
physical collision openings. Rebuilt, foreign, missing, swapped, overlapping, out-of-bounds, and
insufficient-clearance bindings fail closed. Stream, laundry, woodland, clue, discovery, dialogue,
inhabitant, production-art, weather, sound, journal, and map work was deferred at that boundary.

The stream and abandoned-laundry scene binds only those two exact retained projection anchors to
distinct bounded areas with 24-pixel endpoint clearance. The layout copies and validates the two
authored stream-bank collision polygons and adds an inert laundry shell whose east and west openings
retain the water and later woodland routes. Its pure route policy rejects paths through either bank,
the laundry top/bottom or closed side sections, and the 3200-by-1800 world bound. The switch-gated
main-scene proof checks that the collision polygons still match the named geometry, then drives the
Seeker village edge -> stream -> abandoned laundry and back over synchronized navigation and physical
openings. Missing, rebuilt, foreign, swapped, overlapping, out-of-bounds, insufficient-clearance,
narrow-opening, oversized-wall, invalid-approach, malformed-bank, and bank-crossing inputs fail closed.
Laundry interaction and clues, woodland, rare-specimen, shrine, road, wagon, discovery, dialogue,
inhabitant, production-art, weather, lighting, sound, journal, and map work remain deferred.

The woodland-clearing scene binds only the projection's exact retained clearing anchor to one
bounded area with 24-pixel endpoint clearance. Eight collision-backed tree-boundary sections leave
explicit openings toward the abandoned laundry, village edge, old shrine, and closed deeper-forest
path without overlapping the stream bank. Its pure route policy rejects wall-crossing and
out-of-world paths. The switch-gated proof retains exact projection provenance, a hidden inert
rare-specimen marker, synchronized village/laundry round trips, and physical travel through the
stream crossing, laundry, and clearing openings. The marker is not an interaction candidate, world
object, discovery, collectible, identified specimen, or authoritative evidence.

The old-shrine scene similarly binds only its exact retained projection anchor to one bounded area.
Five collision-backed boundary sections leave explicit openings toward the woodland clearing and
southern road. A hidden scene-authored inscription node contains two inert marker layers without an
interaction script, collision, visible presentation, or authoritative state. The switch-gated proof
drives a synchronized and physically clear woodland-clearing/shrine round trip while rejecting shrine
wall crossings. Ordinary interaction and input, player-facing inscription text, dialogue, road/wagon
content, production art, weather, lighting, sound, journal, and map work remain deferred.

The layered-inscription discovery boundary now binds that exact authored marker position to the
representative scenario's exact `shrine-inscription` object and initial Seeker `Noticed` visibility.
Only the exact live Seeker position inside the shrine and within the fixed 112-pixel access radius can
carry the scenario's retained reinterpretation candidate and trigger knowledge through the existing
pure `Noticed`-to-`Identified` resolution. One scene-owned coordinator preflights access, exact
candidate and trigger retention, reinterpretation, state application, marker presentation, and
filtered evidence exposure before one atomic scene commit. Rebuilt or foreign authority, missing
trigger knowledge, stale or far live position, changed marker position, already-visible state, null
input, and repeated input fail closed without partial application. Ordinary startup remains concealed;
the mapped I action is the only ordinary activation path and reveals the existing two inert marker
layers without adding inscription copy, journal/UI presentation, persistence, or a broader evidence
route.

The southern-road/abandoned-wagon scene retains the exact old-shrine layout and accepts only the
projection's exact `southern-road` and `abandoned-wagon` anchors with both exact topology
connections. The road begins at its retained anchor with 24 pixels of clearance on each side and
ends at the sole north opening of a bounded inert wagon shell. Two collision-backed road edges and
five wagon-shell sections are derived directly from the pure validated layout; malformed, rebuilt,
foreign, swapped, overlapping, out-of-bounds, insufficient-clearance, misaligned-opening, and
boundary-crossing inputs fail closed. The switch-gated proof follows the existing synchronized
shrine, clearing, laundry, and stream crossing before entering the southern road, reaches the exact
wagon marker, returns physically clear, rejects representative road and wagon collisions, and
preserves the shrine discovery graph. `wagon-clue`, `wagon-residue`, passive noticing, focused
search, collection, presentation, evidence, closed-path, dialogue, inhabitants, production art,
weather, lighting, sound, journal, and map behavior remain absent.

The closed deeper-forest-path scene retains the exact existing woodland-clearing layout and the
projection's exact `closed-deeper-forest-path` anchor without making that distant landmark
reachable. Two collision-backed approach sides remain inside the clearing and align with its
existing 48-pixel closed-path opening; a visible collision-backed gate spans that opening and has
no input or opening transition. The nearest reachable point keeps the projection's full 24-pixel
clearance from the gate. The switch-gated proof reaches that point over synchronized navigation
and physical movement, rejects representative side and gate crossings, and preserves the
concealed rare specimen, shrine discovery, southern-road, and wagon snapshots. The deeper forest,
unlock conditions, dialogue, discovery, progression, production assets, and broader presentation
remain absent.

The opening-region environmental baseline now groups every exact retained topology location into
one of four stable, non-overlapping regions: village shelter, waterway, woodland edge, or southern
road. Each region retains exact qualitative weather, lighting, and sound states from one catalog;
rebuilt or foreign locations and states, missing coverage, overlap, and duplicate keys fail closed.
An exact-location lookup returns only the retained region. These definitions do not play audio,
change scene lighting, advance time, mutate discovery state, or select production assets.

One adjacent pure story-state transition accepts only a layered-inscription presentation from that
baseline's exact topology. It retains weather and every non-woodland region while replacing only
the woodland-edge lighting and sound states with caller-authored exact states. The result is a new
immutable region snapshot; the discovery graph and baseline remain unchanged.

The main scene now owns that exact representative baseline as its exact current qualitative
environmental snapshot at ordinary startup. One explicit coordinator accepts only the old-shrine
source's exact current layered-inscription presentation, the environmental source's exact
baseline/current snapshot pair, and its own exact authored replacement light and sound states. It
prepares the pure variation without mutation, preflights its exact old-shrine projection and empty
presenter, and installs the result once through exact-instance compare-and-swap. The mapped I action
invokes that flow only after it successfully creates the exact inscription presentation; a rejected
downstream preflight neither retries nor rolls back discovery. Null, absent, rebuilt, foreign,
stale, already-varied, already-presented, and repeated paths remain inert. No time, persistence,
rendered light, or audio state changes with the qualitative snapshot.

An adjacent Application projection resolves one exact retained location to its exact current region
and emits fixed greybox text containing only the canonical weather, lighting, and sound keys. The
scene's old-shrine presentation controller accepts only the exact current environmental snapshot and
exact retained shrine location once, then renders that projected text in a dedicated label. Its
switch-gated proof keeps the label hidden at ordinary startup, drives one ordinary I-key discovery
through exact variation and label presentation, rejects foreign and repeated presentation, and
retains one exact completed flow token. Rendered lighting, ambient playback, production assets,
other location presentations, and additional story-state variations remain deferred.

`CharacterTargetSabotageProposal` retains one exact actor and complete character-target access
requirement while admitting only the `delay` and `influence` kinds. It rejects self-targets, and its
pure eligibility policy delegates the unchanged interaction, capability, and knowledge gate for the
value-matching actor and target. It does not make a proposal occur, define elapsed time or a
relationship change, select an action, or grant scene, persistence, or provider authority.

`CharacterTargetSabotageOccurrence` binds one exact eligible `delay` or `influence` proposal to one
exact retained target-character candidate. Its pure policy requires that target to value-match the
proposal requirement and delegates the complete unchanged actor and access eligibility. It does not
apply an effect, delay elapsed time, change a relationship, author evidence, or grant scene,
persistence, or provider authority.

`CharacterTargetSabotageDelayCandidate` binds one exact eligible character-target occurrence to one
exact caller-authored world event. Its pure policy accepts only `delay`, requires that exact event to
be an authoritative advancing definition eligible against the current log and qualitative time, and
reuses the complete unchanged occurrence eligibility gate. It does not record the event, advance
time, select a target, author evidence, or grant scene, persistence, or provider authority.

`CharacterTargetSabotageDelayTransition` applies one exact eligible target delay through the existing
authoritative world-event occurrence transition. Its immutable result retains the exact candidate
and returns fresh event-log and qualitative-time snapshots while rejected input leaves every source
unchanged. It does not select, decide, recover, persist, or grant Application, scene, UI, or provider
authority.

`CharacterTargetSabotageDelaySelection` materializes and validates at most 64 supplied delay
candidates once, rejects null or duplicate exact candidate references, and returns the exact first
currently eligible candidate in stable caller order. Distinct structurally equivalent candidates
remain valid. This pure Domain selection does not apply the delay, retain a consideration request,
author evidence, or grant Application, scene, persistence, UI, or provider authority.

`CharacterTargetSabotageDelayConsiderationRequest` retains the exact actor, ordinary-interaction
access, authoritative event definitions, current event log, qualitative time, and a stable defensive
copy of at most 64 exact delay candidates. Construction enumerates candidates once and rejects null
or duplicate exact references while permitting distinct structurally equivalent candidates. The
provider-neutral request does not judge eligibility, select, decide, apply, invoke a model, or grant
Application, scene, persistence, UI, or provider authority.

`CharacterTargetSabotageDelayDecision` retains the exact deciding actor identifier and either one
exact request candidate or an explicit abstention. Its pure acceptance policy requires the exact
actor identifier instance and requires any selection to remain an exact currently eligible request
member against the unchanged access and world state. An immutable consideration result binds only
an accepted decision to its exact request. These contracts do not apply a delay, invoke a model,
author evidence, or grant scene, persistence, UI, or provider authority.

`CharacterTargetSabotageDelayConsiderationTransition` leaves an accepted abstention inert and
delegates an accepted exact selection through the existing immutable target-delay transition,
yielding fresh event-log and qualitative-time state. The provider-neutral
`ICharacterTargetSabotageDelayDecisionModel` and its Application controller obtain one decision for
the exact request and caller cancellation, then construct the validated Domain result. They do not
bind a provider, own background work, apply automatically, recover time, author evidence, or grant
scene, persistence, or UI behavior.

An immutable target-delay binding retains exact caller-composed OpenAI settings and model without
invoking it. The isolated temporary model source supports exact-instance initialized, replaced, and
cleared bindings, one-shot precedence, configured fallback, and one stable fail-closed offline
model without construction-time input/output.

A bounded target-delay task owns one decision at a time, installs only a completed accepted result
on the main thread, rejects overlap, supports one cancellation request and explicit terminal reset,
and exposes failures only as status. An explicit isolated flow composes an already-authored exact
request with the model source and task while preserving a queued one-shot across rejected overlap.
These nodes do not schedule themselves or add input/UI, settings mutation, persistence, evidence,
navigation, movement, perception, dialogue, or other world behavior.

`CharacterTargetSabotageDelayController` owns one exact authoritative event-log and qualitative-time
pair and resolves each exact accepted consideration request at most once. It retains abstention
referentially unchanged while consuming that request, and atomically installs only the existing pure
transition's fresh snapshots plus exact applied result for a selection. Stale, repeated,
uninitialized, null, and distinct value-equivalent state fail closed. It does not schedule
consideration, author evidence, recover elapsed time, persist state, or add other world behavior.

`CharacterTargetSabotageDelayResolutionCoordinator` observes each exact completed target-delay task
result once and delegates it to that authoritative controller. Explicit acknowledgement resets the
task only while the exact resolved event log, world time, and latest-applied-result token all remain
current; repeated observation or acknowledgement and externally advanced authority fail closed.
The coordinator does not start work, construct requests, schedule decisions, recover elapsed time,
author evidence, or persist world state.

The target-delay provider boundary strictly decodes one bounded JSON decision into the exact
request actor and candidate instances, delegates final acceptance to the existing Domain policy,
and redacts malformed values. A bounded accumulator and framed-SSE completion path require the
complete ordered OpenAI Responses lifecycle and expose no partial, failed, refused, or trailing
decision. Stateless request serialization supplies only bounded trusted actor, current qualitative
time, occurred-event identifiers, and exact candidate summaries as in-world data, plus a closed
schema for one exact candidate index or abstention. It stores no provider state, carries no
credential, sends no request, invokes no model, and neither selects nor applies a delay by itself.
The OpenAI adapter loads one credential only when invoked, posts that stateless request through
caller-owned HTTP and endpoint dependencies, requires semantic event-stream content, and exposes
only bounded redacted transport or semantic failure. Its factory creates an exact retained binding
without credential access, network input/output, or model invocation.

An explicit target-delay composition seam initializes, replaces, or clears only its own exact
binding in the existing temporary model source. Selected OpenAI game settings activate that seam;
switching to Ollama restores the stable offline delay model. The dependency-ordered source,
authoritative state controller, task, explicit consideration flow, and resolution coordinator are
owned inertly in `Main.tscn`: ordinary startup neither initializes authority nor schedules work,
while a dedicated credential-free smoke explicitly initializes one exact request state, applies one
exact caller-started selection, acknowledges it only while that authority remains current, and
proves a later offline failure leaves the applied state unchanged. This scene path does not schedule
itself, recover elapsed time, author evidence, persist state, or add input/UI or other world behavior.

`CharacterTargetSabotageDelayRecoveryCandidate` binds one exact applied delay result to one exact
caller-authored recovery world event. Its pure policy requires the applied result's exact event-log
and qualitative-time snapshots to remain current, the exact delay event identifier to remain both
the log tail and current qualitative-time event, and the recovery event to be the exact
authoritative unrecorded advancing definition. Stale, rebuilt, mismatched, already-recorded,
non-authoritative, and non-advancing input fails closed without changing any source.

`CharacterTargetSabotageDelayRecoveryTransition` applies one exact-current eligible recovery through
the existing immutable world-event occurrence transition. Its result retains the exact recovery
candidate, appends recovery after the complete delay history, and returns fresh event-log and
qualitative-time snapshots without deleting or rewriting the delay occurrence. Bounded stable
selection validates at most 64 supplied recovery candidates once before returning the exact first
eligible candidate; it rejects null and duplicate exact references while permitting distinct
equivalent wrappers.

`CharacterTargetSabotageDelayRecoveryConsiderationRequest` retains the exact recovery actor,
authoritative event definitions, current event log, qualitative time, and a defensive read-only
copy of at most 64 exact recovery candidates. It validates the collection once but does not judge
eligibility, select, decide, apply, invoke a model, schedule work, author evidence, persist state, or
grant Application, scene, UI, navigation, movement, perception, dialogue, or other world authority.

`CharacterTargetSabotageDelayRecoveryCandidateRequestController` retains one immutable request only
for the recovery-candidate controller's exact current candidate. It derives the exact delayed target
from the retained occurrence and publishes that target, caller-supplied authoritative world-event
definitions, the delay result's exact event log and qualitative time, and only that candidate after
the unchanged recovery and decision policies accept the graph. Unavailable, stale, rebuilt,
foreign, missing-event, inapplicable, repeated, and null input stays inert. The main scene owns this
request source immediately after its candidate dependency, but ordinary startup does not create a
request or start recovery.

`CharacterTargetSabotageDelayRecoveryCandidateConsiderationFlowController` starts a decision only
for that request source's exact current request while the recovery task is idle, and only then
consumes the queued, configured, or offline recovery model. Rejected stale, null, overlapping, and
repeated starts preserve the queued model and exact request graph. The main scene dependency-orders
this flow with the existing recovery model, task, state, and resolution owners; only the dedicated
switch-gated smoke initializes recovery authority and explicitly starts the exact request, proving
one append-only delay-to-recovery chain without making ordinary startup autonomous.

`CharacterTargetSabotageDelayRecoveryStartCoordinator` preflights one complete exact-current graph
before publishing the candidate or request, initializing recovery authority, or consuming a model.
It requires the exact latest applied delay result and world-state authority, an applicable
caller-authored recovery event in the supplied definitions, unused one-shot sources, uninitialized
recovery authority, and an idle task. The main scene dependency-orders the coordinator after every
exact dependency, and only the existing switch-gated target-delay smoke calls it after one exact
applied delay. Rejected repeated or occupied starts preserve the current candidate and request
graph and consume no additional model. Ordinary startup remains inert, and the coordinator is not invoked
automatically or allowed to author events, evidence, persistence, UI, input, timing, movement,
perception, or dialogue behavior.

`CharacterTargetSabotageDelayRecoveryDecision` retains the exact deciding actor identifier and
either one exact recovery candidate or explicit abstention. Its acceptance policy requires the
request actor's exact identifier instance and, for a selection, the exact request-member candidate,
the candidate delay occurrence's exact target identifier, and unchanged authoritative event, log,
and time eligibility. An immutable consideration result binds only an accepted decision to its
exact request. The consideration transition leaves abstention inert and delegates an accepted
selection through the existing append-only recovery transition without changing request sources.

`ICharacterTargetSabotageDelayRecoveryDecisionModel` is the provider-neutral Application seam for
one exact request. Its controller rejects null and pre-cancelled input, forwards the exact request
and caller cancellation once, and constructs the Domain result so foreign, stale, or invalid
decisions fail closed. A construction-only binding retains one exact OpenAI setting and exact model.
The temporary scene source supplies one queued one-shot model before a stable configured model and
otherwise returns one stable fail-closed offline model; replacement and clearing require the exact
expected binding and preserve any queued one-shot model.

One isolated scene task owns the bounded delay-recovery decision lifecycle, installing only an
accepted result on the main thread while rejecting overlap and exposing explicit cancellation and
terminal reset. An explicit flow consumes a model only while that task is idle, so rejected overlap
cannot consume a queued one-shot model. Infrastructure strictly decodes one complete bounded closed
JSON object containing only the request actor and a nullable candidate index, rebinds value-equal
text and an in-range selection to the request's exact instances, and delegates final acceptance to
the unchanged Domain policy. Malformed, foreign, stale, wrong-owner, out-of-range, over-depth, and
oversized input fails through one content-free error. A stateless OpenAI request serializer emits
only the selected model, disabled storage, streaming mode, a bounded output budget, provider-
independent instructions, bounded trusted actor/world/recovery-candidate context, and one strict
request-bound selection-or-abstention schema. It never reads credentials or invokes a provider.
The bounded semantic accumulator accepts only one ordered OpenAI Responses lifecycle, permits
bounded metadata and output-text deltas, and materializes a decision through the strict
request-bound decoder only after verified output completion and response completion. Framed SSE
completion rejects malformed, incomplete, refused, failed, excessive, trailing, or cancelled
streams without exposing provider content. The OpenAI adapter validates a caller-owned credential,
posts that exact serialized request to the configured Responses endpoint, requires an SSE response,
and returns only the completed exact decision. Provider rejections and transport failures expose no
provider body or transport detail, while caller cancellation is preserved. A construction-only
factory binds the exact OpenAI settings and caller-owned dependencies without credential access,
HTTP, or model invocation. An isolated inert scene composition explicitly initializes, replaces,
or clears only its own exact binding in the existing temporary model source, preserving queued
one-shot precedence and the stable offline fallback. Selected OpenAI game settings now initialize
and replace that exact composition with fresh settings-bound models, while a supported Ollama
selection clears only its installed binding and restores the same offline model. The
dependency-ordered model source is owned by `Main.tscn`; ordinary startup remains inert, and
the credential-free composition smoke proves unsupported Anthropic selection is unchanged, queued
one-shot precedence survives replacement, and no credential, HTTP, or model input occurs. The scene
also dependency-orders the target-delay recovery task, explicit consideration flow, authority, and
resolution coordinator without starting them. Ordinary startup leaves recovery uninitialized and
inert. A dedicated switch-gated credential-free smoke alone initializes the exact request world
pair, supplies one exact one-shot selection, observes its exact applied recovery, acknowledges the
completed task only while the observed result and all resolved authority tokens remain current,
and proves a later offline failure leaves both applied authority and every request input unchanged.
The controller consumes each exact accepted request at most once, installs a selected recovery's
fresh event-log, world-time, and exact result together, and leaves current authority plus any prior
applied result referentially unchanged for abstention. The coordinator observes each fresh
completed result once; stale authority cannot be acknowledged or reset. Stale, rebuilt, repeated,
uninitialized, and null inputs fail closed. An isolated one-shot candidate controller additionally
accepts only the exact current applied delay result plus one caller-authored authoritative advancing
recovery event. Missing, recorded, non-advancing, stale, rebuilt, repeated, and null inputs stay
inert. The main scene owns that candidate source, its exact-request source, and their explicit
start coordinator; only the switch-gated target-delay smoke publishes them and begins recovery.
Nothing automatically schedules recovery, authors the recovery event, persists state, or adds
input/UI or other world behavior.

`CharacterTargetSabotageInfluenceCandidate` binds one exact eligible character-target occurrence to
one exact existing directional relationship change. Its pure policy accepts only `influence`,
requires the change to belong to the occurrence target and relate that target to the proposal actor,
reuses the complete occurrence eligibility gate, and delegates unchanged applicability to
`CharacterRelationshipChangePolicy`. It does not apply the change, infer a relationship, select a
target, author evidence, or grant elapsed-time, scene, persistence, or provider authority.

`CharacterTargetSabotageInfluenceSelection` materializes and validates at most 64 supplied
influence candidates once, rejects null or duplicate exact candidate references, and returns the
exact first currently eligible candidate in stable caller order. Distinct structurally equivalent
candidates remain valid. This pure Domain selection does not apply or recover influence, make a
decision, retain a consideration request, author evidence, or grant elapsed-time, Application,
scene, persistence, UI, or provider authority.

`CharacterTargetSabotageInfluenceConsiderationRequest` retains the exact actor, ordinary-interaction
access snapshot, current directional relationship, and a stable defensive copy of at most 64 exact
influence candidates. Construction enumerates the supplied candidates once and rejects null or
duplicate exact references while permitting distinct structurally equivalent candidates. The
provider-neutral request does not judge eligibility, decide, apply, or recover influence; author
evidence; or grant elapsed-time, Application, scene, persistence, UI, or provider authority.

`CharacterTargetSabotageInfluenceDecision` retains the exact deciding actor identifier and either
one exact candidate or an explicit abstention. Its pure acceptance policy requires the exact actor
identifier instance, requires any selection to be the exact currently eligible request member, and
permits abstention without changing request state. An immutable consideration result binds only an
accepted decision to its exact request and selected candidate. These contracts do not apply or
recover influence, author evidence, invoke a model, or grant elapsed-time, Application, scene,
persistence, UI, or provider authority.

`CharacterTargetSabotageInfluenceConsiderationTransition` leaves an accepted abstention inert and
delegates an accepted exact selection through the existing immutable influence transition, yielding
fresh relationship state with the exact candidate and reason retained. It accepts only the already
validated consideration result and does not select, reconsider, recover, author evidence, or add
elapsed-time, Application, scene, persistence, UI, model, or provider behavior.

`ICharacterTargetSabotageInfluenceDecisionModel` is the provider-neutral Application seam for one
exact influence request and caller cancellation. The pure orchestration controller rejects nulls
and pre-cancellation before invoking that model exactly once, then constructs the existing Domain
consideration result so invalid decisions fail closed without changing request state. An immutable
OpenAI binding retains exact caller-composed settings and model without invoking it.

The isolated temporary influence model source supports exact-instance initialized, replaced, and
cleared bindings, one-shot model precedence, configured fallback, and one stable fail-closed
offline model. A bounded scene task owns one decision at a time, installs only a completed accepted
result on the main thread, rejects overlap, supports one cancellation request and explicit terminal
reset, and exposes failures only as status. An explicit isolated flow composes an already-authored
exact request with that source and task while preserving a queued one-shot model across rejected
overlap. One inert game composition seam creates exact OpenAI influence bindings from caller-owned
settings, HTTP, and credential dependencies and installs, replaces, or clears them only through the
source's expected-instance operations. The selected-provider composition activates that seam for
OpenAI, leaves an unsupported replacement unchanged, and restores the stable offline model for a
supported local-provider replacement without performing construction-time input/output.

The influence model source, authoritative relationship controller, recovery-candidate controller,
task, explicit consideration flow, and resolution coordinator are dependency-ordered in
`Main.tscn`. Ordinary startup neither initializes authority nor starts a decision; a switch-gated
credential-free smoke explicitly initializes one exact request relationship, applies one exact
selected result, creates one exact caller-authored inverse recovery candidate, acknowledges the
result only while that authority remains current, and proves a later offline failure leaves the
applied state and candidate unchanged. This scene path does not schedule itself, create a recovery
request, start recovery work, or add input/UI, settings mutation, persistence, evidence,
elapsed-time, navigation, movement, perception, dialogue, or other world behavior.

`CharacterTargetSabotageInfluenceController` separately owns one exact authoritative directional
relationship and resolves each exact accepted consideration request at most once. It requires the
request relationship to be the exact current instance, retains abstention numerically and
referentially unchanged while consuming that request, and installs only the existing pure
transition's fresh relationship plus exact applied result for a selection. Stale, repeated,
uninitialized, null, and distinct value-equivalent state fail closed. It does not schedule
consideration, author evidence, select or apply recovery, persist relationship state, or add other
world behavior.

`CharacterTargetSabotageInfluenceResolutionCoordinator` observes each exact completed influence
task result once and delegates it to that authoritative controller. An explicit acknowledgement
resets the task only while the exact resolved relationship and latest applied-result token remain
current; repeated observation or acknowledgement and externally advanced authority fail closed.
The coordinator does not start work, construct requests, schedule decisions, recover influence,
author evidence, or persist relationship state.

The strict influence decision decoder accepts only one bounded closed JSON object containing the
exact actor text and a nullable zero-based candidate index. It rebinds value-matching text and the
index to exact request instances, delegates final acceptance to the Domain policy, and collapses
malformed, unknown, duplicate, over-depth, oversized, or policy-invalid input into one redacted
error. The semantic Responses accumulator and SSE completion require an ordered complete lifecycle,
bound every event and output document, reject partial, failed, refused, trailing, or malformed
streams, preserve caller cancellation, and expose a decision only after successful completion.

The OpenAI influence request serializer emits one stateless streaming structured-output request
with bounded actor, relationship, and candidate context and a closed exact schema. The HTTP adapter
loads and validates a credential before sending, requires a successful `text/event-stream`
response, redacts provider and transport failures, and returns only the exact accepted decision.
Its binding factory retains caller-owned settings, HTTP, credential, and endpoint dependencies
without construction-time I/O. Game composition now installs that binding only for selected OpenAI
settings and the main scene retains the explicit task flow, but neither path schedules itself,
applies influence, or mutates any request source.

`CharacterTargetSabotageInfluenceTransition` accepts only that fully eligible candidate, delegates
the immutable change to the existing relationship transition, and returns a fresh relationship
together with the exact applied candidate. Rejected input throws without changing any source. This
Domain-only transition does not select the candidate, recover the prior relationship, author
evidence, or add elapsed-time, Application, scene, persistence, UI, or provider behavior.

`CharacterTargetSabotageInfluenceRecoveryCandidate` retains one exact applied influence result and
one exact proposed inverse relationship change. Its pure policy requires the exact current result
relationship instance, the same target-to-actor direction and relationship dimension, the exact
applied reason at the history tail, the exact inverse delta, and unchanged applicability through
`CharacterRelationshipChangePolicy`. Stale, rebuilt, fabricated, mismatched, or inapplicable input
fails closed. It does not apply recovery, remove history, infer evidence, or authorize Application,
scene, UI, persistence, or provider behavior.

`CharacterTargetSabotageInfluenceRecoveryCandidateController` owns at most one recovery candidate
for the influence controller's exact latest applied-result and relationship authority. Explicit
creation retains the caller-authored inverse change only when the unchanged recovery policy accepts
it. Uninitialized, missing, stale, value-equal rebuilt, repeated, misdirected, wrong-dimension,
wrong-delta, and null input cannot replace its current candidate or change the influence result,
relationship, change, or reason graph. The controller does not create a consideration request,
start recovery work, author evidence, persist state, or add UI, input, timing, movement,
perception, or dialogue behavior.

`CharacterTargetSabotageInfluenceRecoveryCandidateRequestController` retains one immutable
consideration request only for the recovery-candidate controller's exact current candidate. It
derives the exact relationship-owning target from the retained influence occurrence and publishes
that target, the influence result's exact current relationship, and only that candidate after the
unchanged recovery and decision policies accept the graph. Unavailable, stale, value-equal rebuilt,
foreign, repeated, and null input stays inert. The main scene owns this request source immediately
after its candidate dependency, but ordinary startup does not create a request or start recovery.

`CharacterTargetSabotageInfluenceRecoveryCandidateConsiderationFlowController` starts a decision
only for that request source's exact current request while the recovery task is idle, and only then
consumes the queued, configured, or offline recovery model. Rejected stale, null, overlapping, and
repeated starts preserve the queued model and exact request graph. The main scene dependency-orders
this flow with the existing recovery model, task, state, and resolution owners; only the dedicated
switch-gated smoke initializes recovery authority and explicitly starts the exact request, proving
the influence relationship can be restored without making ordinary startup autonomous.

`CharacterTargetSabotageInfluenceRecoveryStartCoordinator` composes those exact scene owners behind
one explicit start. It first requires the influence controller's exact latest result and
relationship, unused candidate and request sources, uninitialized recovery authority, and an idle
recovery task, then validates a prospective inverse candidate and request before publishing any
state or consuming the queued model. Only after that complete preflight does it create the exact
candidate and request, initialize recovery authority from their exact relationship, and start the
exact request once. The main scene dependency-orders this coordinator, and the existing
switch-gated influence smoke uses it for the complete influence-to-recovery chain while ordinary
startup remains inert. Stale, rebuilt, foreign, misdirected, wrong-dimension, wrong-delta, null,
repeated, and occupied input cannot change either authority or consume the queued model.

`CharacterTargetSabotageInfluenceRecoveryTransition` accepts only that exact-current recovery,
delegates the inverse update to the existing immutable relationship transition, and returns the
exact recovery candidate with fresh numerically restored relationship state. Both the influence and
recovery reasons remain recorded; rejected input leaves every source unchanged. This transition
does not discover or consume evidence, select recovery, or add Application, scene, UI, persistence,
elapsed-time, or provider behavior.

`CharacterTargetSabotageInfluenceRecoverySelection` materializes and validates at most 64 supplied
recovery candidates once, rejects null or duplicate exact candidate references, and returns the
exact first currently eligible candidate in stable caller order. Distinct structurally equivalent
candidates remain valid. This pure Domain selection does not author candidates or evidence, apply
recovery, retain a consideration request, invoke a model, or add Application, scene, UI,
persistence, elapsed-time, or provider behavior.

`CharacterTargetSabotageInfluenceRecoveryConsiderationRequest` retains one exact deciding
character, exact current directional relationship, and a stable defensive copy of at most 64 exact
recovery candidates. Construction enumerates the supplied candidates once and rejects null or
duplicate exact references while permitting distinct structurally equivalent candidates. This
provider-neutral request does not judge eligibility, decide, apply recovery, author evidence,
invoke a model, or add Application, scene, UI, persistence, elapsed-time, or provider behavior.

`CharacterTargetSabotageInfluenceRecoveryDecision` retains the exact deciding actor identifier and
either one exact recovery candidate or an explicit abstention. Its pure acceptance policy requires
the request actor to own the current directional relationship, the exact actor identifier instance,
and any selection to be an exact currently eligible request member; it permits abstention without
changing request state. An immutable consideration result binds only an accepted decision to its
exact request and selected candidate. These contracts do not apply recovery, author evidence,
invoke a model, or add Application, scene, UI, persistence, elapsed-time, or provider behavior.

`CharacterTargetSabotageInfluenceRecoveryConsiderationTransition` leaves an accepted abstention
inert and delegates an accepted exact selection through the existing immutable recovery transition,
yielding fresh numerically restored relationship state with the exact candidate and both reasons
retained. It accepts only the already validated consideration result and does not select,
reconsider, author evidence, or add Application, scene, UI, persistence, elapsed-time, model, or
provider behavior.

`ICharacterTargetSabotageInfluenceRecoveryDecisionModel` is the provider-neutral Application seam
for one exact recovery request. The pure consideration controller rejects null and pre-cancelled
input, forwards the exact request and caller cancellation once, and constructs the existing Domain
result so wrong-owner, foreign, stale, or otherwise invalid decisions fail closed. It does not bind
a model, own background work, apply recovery, decode provider output, send HTTP, or add scene, UI,
persistence, elapsed-time, or other world behavior.

`CharacterTargetSabotageInfluenceRecoveryModelBinding` retains one exact OpenAI setting and exact
caller-composed recovery model without construction-time I/O or invocation. The temporary scene
source supplies one queued exact one-shot model before a stable configured model and otherwise
returns one stable fail-closed offline model; exact expected-binding replacement and clearing do
not consume a queued one-shot model. One inert composition seam creates, replaces, and clears only
its exact source binding from caller-owned settings, HTTP, and credential dependencies. The shared
selected-provider composition activates this seam only for OpenAI and restores its stable offline
model for a supported local-provider selection without construction-time input/output.

`CharacterTargetSabotageInfluenceRecoveryTaskController` owns at most one isolated recovery
decision through idle, running, completed, cancelled, and failed states. It installs only an exact
accepted result during main-thread processing, rejects overlap, supports one explicit cancellation
and terminal reset, and leaves the full request graph unchanged on model or validation failure. It
does not choose when to start, consume a model source, apply recovery, integrate scene authority,
decode provider output, send HTTP, or persist state.

`TemporaryCharacterTargetSabotageInfluenceRecoveryConsiderationFlowController` starts that task
only when explicitly called while it is idle, taking the next one-shot, configured, or stable
offline model only after the idle check. Rejected overlap therefore preserves a queued one-shot
model. Its source, task, and explicit flow are dependency-ordered in `Main.tscn`; an environment-
gated credential-free smoke proves one exact selection, explicit reset, and stable offline failure,
while ordinary startup remains inert. This path does not apply or acknowledge recovery, create
provider traffic, persist state, or add UI or world scheduling.

`CharacterTargetSabotageInfluenceRecoveryController` owns one exact initialized authoritative
relationship and consumes each exact accepted consideration request at most once. A selection
delegates through the existing immutable recovery transition and installs the fresh numerically
restored relationship plus its exact result; abstention preserves the exact relationship and prior
result while still consuming that request. Uninitialized, stale, value-equal rebuilt, repeated, or
null input fails closed.

`CharacterTargetSabotageInfluenceRecoveryResolutionCoordinator` observes each exact completed
recovery result at most once and delegates it only while the recovery controller retains the
request's exact relationship authority. Explicit acknowledgement resets the task only while the
exact resolved relationship and exact latest recovery result remain current. Selected recovery
succeeds, referentially inert abstention is consumed without mutation, and duplicate observation,
stale authority, and repeated acknowledgement fail closed without changing any request source.

The recovery model source, authoritative relationship controller, task, explicit consideration
flow, and result-resolution coordinator are dependency-ordered in `Main.tscn`. Ordinary startup
remains uninitialized and inert. The dedicated switch-gated credential-free smoke alone initializes
one exact relationship, applies one explicitly selected recovery, acknowledges its exact completed
result, and proves a later offline failure leaves the restored authority unchanged. This path does
not automatically schedule work, author candidates or evidence, persist relationships, or add UI,
input, elapsed time, navigation, movement, perception, dialogue, or other world behavior.

The strict influence-recovery decision decoder accepts only one bounded closed JSON object
containing exact actor text and a nullable zero-based recovery-candidate index. It rebinds
value-matching actor text and the index to exact request instances, delegates final acceptance to
the existing Domain policy, and collapses malformed, unknown, duplicate, over-depth, oversized,
out-of-range, wrong-owner, stale, foreign, or otherwise policy-invalid input into one redacted
error. The internal schema writer bounds the selection enum to the exact request candidate count
plus abstention. Neither contract changes the request, relationship, candidate, or reason graph.

The OpenAI influence-recovery request serializer emits one stateless streaming structured-output
request with bounded actor-owned character context, current directional relationship values, and
exact indexed recovery-candidate context. Its strict closed schema permits only the exact actor and
one supplied candidate index or abstention. Serialization performs no credential lookup, HTTP,
streaming, model invocation, task work, scene integration, recovery application, persistence, UI,
or other world behavior.

The bounded influence-recovery semantic accumulator accepts only one ordered OpenAI Responses
lifecycle, permits bounded metadata and output-text deltas, and materializes a decision through the
strict request-bound decoder only after verified output completion and response completion. Framed
SSE completion rejects incomplete or trailing event streams while preserving caller cancellation.
The caller-composed HTTP adapter loads one credential only on explicit invocation, posts the
stateless structured-output request, requires a successful event-stream response, and redacts
provider and transport failure details. Its factory retains exact OpenAI settings, HTTP, credential,
and endpoint dependencies without construction-time input/output. Game composition now installs
that binding only for selected OpenAI settings and the main scene retains the explicit task flow,
but neither path schedules itself, applies or acknowledges recovery, persists state, or adds UI or
other world behavior.

`CharacterObjectSabotageProposal` retains one exact actor and action-access requirement while
admitting only the object-targeted `conceal`, `move`, `substitute`, and `contaminate` kinds. Its
pure eligibility policy first requires the proposal actor to value-match the supplied character,
then delegates the complete unchanged physical, capability, and knowledge requirement to
`CharacterActionAccessPolicy`. It does not model `misquote`, `delay`, or `influence` as object
targets, and it grants no occurrence, effect, mutation, scene, persistence, or provider authority.

`CharacterObjectSabotageOccurrence` binds one exact object proposal to one exact authoritative
world-object target candidate. Its pure policy requires the proposal requirement to name that
target by value, requires the exact target instance to belong to the supplied world-object snapshot,
and delegates unchanged proposal eligibility. It does not apply an effect, replace an object,
change truth, position, perception, evidence, events, time, or transcripts, or grant scene,
persistence, or provider authority.

`CharacterObjectSabotageEvidence` binds one exact object-sabotage occurrence candidate to one exact
discovery-evidence definition. Its pure policy delegates complete occurrence eligibility, requires
the evidence to identify the occurrence target by value, and rejects an evidence identifier already
present in the supplied snapshot. It does not append evidence, apply sabotage, prove that an
authored discovery route can reveal the evidence, or alter truth, perception, state, scenes,
persistence, or providers.

`CharacterObjectSabotageEvidenceTransition` accepts only that complete evidence-bound eligibility
and returns a fresh discovery-evidence snapshot with the candidate's exact definition appended at
the stable tail. Its immutable result retains the exact accepted candidate, while the caller's
snapshot remains unchanged. This evidence-only transition does not apply the sabotage effect,
record a world event, advance time, append a transcript, establish discovery reachability, or add
scene, persistence, or provider behavior.

`WorldObjectConcealmentSnapshot` retains at most 64 value-unique exact object identifiers in stable
order; absence means that an object is not concealed. `CharacterObjectSabotageConcealmentPolicy`
accepts only a completely eligible evidence-bound `conceal` candidate whose target is not already
concealed. `CharacterObjectSabotageConcealmentSelection` fully materializes at most 64 caller-ordered
candidates, rejects null entries and value-equal duplicate evidence identifiers, then returns the
exact first candidate accepted by that complete policy or `null`. The immutable transition returns
one fresh concealment snapshot containing the exact target identifier and the existing fresh
evidence-transition result as one all-or-nothing value; the pure sequence composes selection and
that transition, failing before a result when no candidate is eligible. These operations do not
mutate the authoritative object or its position, remove any route, reveal or recover a concealed
object, apply another sabotage kind, or add events, time, transcripts, scenes, persistence, or
provider behavior.

`CharacterObjectSabotageConcealmentConsiderationRequest` defensively retains at most 64 exact
caller-ordered candidates with value-unique evidence identifiers together with the exact character,
physical-access, world-object, concealment, and evidence snapshots that define the current choice.
`CharacterObjectSabotageConcealmentDecision` explicitly selects one exact candidate evidence
identifier or abstains with no selection. Its pure acceptance policy requires the exact considering
character identifier and, for selection, the exact identifier of a candidate accepted by the
complete current concealment policy. The immutable consideration result binds one accepted decision
to its exact request and retains the exact selected request candidate, or `null` for abstention. A
provider-neutral cancellation-aware Application interface obtains one decision, and its coordinator
returns only that validated exact result. The pure consideration transition delegates a selection
through the existing all-or-nothing concealment and evidence transition while abstention returns no
application. The scene controller resolves that result only when its request retains the exact
current concealment and evidence snapshots: selection installs both fresh result snapshots
atomically, while abstention retains both exact scene instances and consumes that exact request.
A bounded scene task obtains one decision with caller cancellation, installs only a non-stale
accepted result on the main thread, and exposes closed idle, running, completed, cancelled, and
failed states plus terminal reset. A temporary model source consumes one exact caller-supplied model
for the next decision only, then resumes one exact configured binding or one stable fail-closed
offline model. Configured bindings retain the exact canonical OpenAI setting and model, support
exact stale-safe replacement and clearing, and are composed by the existing settings controller
without credential or network input/output. OpenAI-to-OpenAI changes replace the future
concealment model, while selecting Ollama clears it back to offline behavior. The explicit temporary
consideration flow composes the authored enemy, access, world objects, current scene snapshots, and
stable candidate collection only when asked, then delegates to that task. `Main.tscn` wires these
nodes after exact sabotage-state initialization but does not start them automatically. One scene
coordinator observes each fresh exact completed result at most once: a selection projects only the
request's exact prior concealment to the current scene-owned concealment through the existing
stale-safe presentation seam, while abstention and every other task state preserve presentation and
task state. The main-scene water-cask body therefore hides only after an explicitly started selected
result; startup remains inert. One explicit acknowledgement resets only that exact already-observed
completed result. A selected result must still match the exact last-presented current concealment;
an abstention must still retain its exact request concealment and evidence as current scene state.
Unobserved, stale, idle, running, cancelled, failed, null, and repeated acknowledgements preserve
task, concealment, evidence, and presentation state. An acknowledged result permits a later explicit
decision without changing the authored source, authoritative world object, or body position.
Infrastructure strictly decodes a complete selection-or-abstention document against that exact
request, serializes only relevant authored character context and its eligible candidates into one
stateless strict OpenAI Responses request, completes only a bounded valid SSE lifecycle, and exposes
one caller-composed authenticated adapter behind the provider-neutral decision interface. Adapter
and binding construction perform no credential or network input/output. These contracts do not
decide timing, schedule work automatically, acknowledge completed tasks automatically, persist
data, invoke a provider during scene startup, or authorize another sabotage effect.

`CharacterObjectSabotageConcealmentRecoveryPolicy` permits recovery only for a `conceal` candidate
whose target is currently concealed, whose exact target-bound evidence definition remains in the
authoritative evidence snapshot, and whose identifier is visible in one supplied character
evidence view. Its transition removes only the value-matching target into a fresh concealment
snapshot while retaining unrelated exact identifiers, the exact evidence view, and the exact
candidate. It does not consume evidence, mutate an object or perception, prove how the character
discovered the evidence, or add routes, scenes, persistence, or providers.

`CharacterObjectSabotageMoveDestination` binds one exact `move` occurrence to one exact authored
destination. `WorldObjectPlacementSnapshot` retains at most 64 exact authoritative object/location
pairs in stable copied order with value-duplicate object identifiers rejected. One move candidate
requires its destination and discovery evidence to retain the same exact occurrence. Its complete
policy requires the existing object-sabotage evidence eligibility, the exact authoritative target
placement, and a destination with a different location identifier. The immutable transition
replaces only that target placement at its stable index and appends the exact fresh evidence as one
all-or-nothing result. It does not infer coordinates, paths, reachability, travel, timing, current
actor location, or scene movement.

`CharacterObjectSabotageMoveRecoveryCandidate` retains that exact move candidate and one exact
pre-move source placement for the occurrence target, rejecting exact and value-equal destination
reuse. Recovery requires the exact target to remain at the exact authored destination, the exact
evidence definition to remain authoritative, and its identifier to appear in one character evidence
view. The immutable transition restores the exact retained source placement at the same stable
index while preserving unrelated placements and all evidence. It does not infer where the object
was, consume evidence, mutate an authoritative object or perception, or move a scene body.

`CharacterObjectSabotageSubstitution` binds one exact `substitute` occurrence to one exact authored
replacement with a distinct object identifier. A candidate requires that substitution and its
discoverable evidence to retain the same exact occurrence. Bounded substitution snapshots retain at
most 64 exact entries in stable order and reject value-duplicate target identifiers. The pure policy
delegates complete occurrence and fresh-evidence eligibility and rejects a target already present in
the substitution overlay. Its immutable transition appends the exact substitution and evidence as
one all-or-nothing result while leaving the authoritative world-object snapshot and both source
objects unchanged. These contracts do not infer inventory, placement, similarity, plausibility,
reachability, critical-route safety, or replacement eligibility, or present a substitution.

Substitution recovery requires the exact candidate substitution to remain active, its exact
target-bound evidence definition to remain authoritative, and that evidence identifier to appear in
one character evidence view. The immutable transition removes only that exact substitution into a
fresh stable snapshot while retaining unrelated substitutions, the exact evidence view, and the
exact candidate without consuming evidence. `CharacterObjectSabotageSubstitutionController` owns
one exact substitution/evidence pair and installs eligible application or recovery results only from
the exact expected current snapshots. Stale, distinct value-equal, null, and ineligible operations
leave both scene-owned snapshots unchanged.

`WorldObjectContaminationSnapshot` retains at most 64 value-unique exact object identifiers in
stable caller order, exposes a value-based contamination lookup, and leaves authoritative world
objects unchanged. The pure contamination policy accepts only a completely eligible evidence-bound
`contaminate` occurrence whose target is not already contaminated. Its immutable transition appends
the exact target identifier and exact fresh evidence as one all-or-nothing result. Recovery requires
that target to remain contaminated, its exact target-bound evidence definition to remain
authoritative, and that evidence identifier to appear in one character evidence view; it removes
only the value-matching target into a fresh contamination snapshot without consuming evidence. One
pure bounded selection fully validates a caller-ordered candidate collection before returning the
exact first eligible candidate, and one sequence can apply that candidate through the existing
atomic transition. A provider-neutral consideration request retains exact snapshots and a copied
read-only candidate view. Its exact selection-or-abstention decision policy accepts only the
request character and exact candidate evidence identifier, rechecking current contamination
eligibility before acceptance. An immutable result binds only an accepted exact decision and its
selected request candidate; its pure transition applies selections atomically and leaves accepted
abstentions inert. A provider-neutral model seam and Application controller obtain one decision
with caller cancellation before constructing that exact result. The scene controller owns one
exact contamination/evidence pair, rejects stale or repeated results, and applies visible-evidence
recovery without consuming evidence. Its bounded task controller installs accepted results only on
the Godot main thread and closes overlap, cancellation, failure, stale-result, and reset behavior.
The strict OpenAI path accepts only one complete request-owned contamination decision, serializes
one stateless streaming Responses request with bounded trusted character and candidate context plus
a closed nullable-selection schema, and rejects incomplete or trailing semantic-event lifecycles.
Its caller-composed adapter loads one credential only on explicit invocation, requires an event
stream, redacts provider and transport failure detail, and exposes one construction-only binding
over exact OpenAI settings, HTTP, credential, and endpoint dependencies.

`TemporaryCharacterObjectSabotageContaminationModelSource` gives one exact caller-supplied model
precedence for the next decision, otherwise returning an exact configured model or one stable
fail-closed offline model. Configured binding initialization, replacement, and clearing use
exact-reference compare-and-swap and preserve a queued one-shot. The selected-settings scene flow
composes, replaces, or clears its OpenAI binding without construction-time credential or network
input/output. The main scene authors one exact eligible water-cask candidate from its authoritative
world-object snapshot, initializes exact empty contamination and evidence state, and composes one
current request only on explicit start. Its existing task resolves selection on the main thread;
overlap preserves the queued model, abstention is inert, and offline failure changes no state. One
temporary triangular marker projects only exact contamination snapshots. A scene coordinator
observes each fresh completed result once and explicitly acknowledges only an already-observed
result whose selected presentation or abstention snapshots remain current. These contracts do not
define contamination contents or effects, mutate ingredient contamination, automatically start a
decision, retry or fall back to another provider, persist data, or change authoritative objects,
movement, events, time, transcripts, input, or final UI. The presentation flow can explicitly
recover only the exact last-presented current contamination through retained evidence projected
from supplied perception. Ordinary exact water-cask focused-search resolution asks that flow to
recover; unrelated outcomes, null input, and stale presentation leave contamination, evidence, and
the marker unchanged. Recovery hides only the marker and does not consume evidence or change the
resolved perception, actors, source snapshots, positions, or authoritative world objects.

`CharacterDialogueMisquotationProposal` retains one exact prior dialogue event, bounded differing
quotation text, one exact actor, and one canonical capability requirement while leaving the source
delivery and audience unchanged. Its pure eligibility policy requires the actor to value-match the
supplied character, to have heard the source as an explicit listener, and to own the required
authored capability. Truth-equivalent, blank, excessive, unheard, wrong-actor, and
missing-capability inputs fail closed. `CharacterDialogueMisquotationOccurrence` then binds one
exact proposal to one exact already-composed dialogue event. Its pure policy requires an eligible
proposal, the exact proposal actor identifier as speaker, exact proposed text, and at least one
listener. It does not create, select, send, append, or persist dialogue, infer hearing from
positions, or alter the source event.

`CharacterDialogueMisquotationEvidence` identifies one exact non-actor witness for one exact
misquotation occurrence. Its pure policy requires that witness to occur in both the unchanged
source audience and the delivered misquotation audience after complete occurrence validation. This
is only a provider-neutral potentially discoverable contradiction seam: it does not declare either
text true, automatically expose or correct a claim, mutate a transcript or character, deliver new
dialogue, select a candidate, or define scene, persistence, model, delay, or influence behavior.

`CharacterDialogueMisquotationOccurrenceCoordinator` accepts one explicit proposal, delivery, and
bounded caller-supplied hearing query. It preflights the exact proposal actor, proposed text, retained
source delivery mode, and non-empty selected audience before delegating once to the ordinary
`DialogueEventController`; the resulting exact event then becomes the occurrence's exact delivered
event. Invalid hearing, actor, text, mode, absent-listener, null, stale, and repeated inputs cannot
replace either owner. The main scene owns this coordinator inertly and exercises it only behind a
dedicated smoke switch. `CharacterDialogueMisquotationEvidenceCandidateController` can then retain
one potential witness-evidence candidate only for that occurrence coordinator's exact current
occurrence and only when the existing pure observability policy accepts the explicit actor and
witness. The main scene owns that candidate controller immediately after the occurrence coordinator,
but only the dedicated switch-gated smoke requests a candidate. The
`CharacterDialogueMisquotationEvidenceCandidateRequestController` can then retain one immutable
consideration request only for the candidate controller's exact current candidate while the existing
observability policy still accepts the supplied actor. Its request preserves that exact actor and
candidate, and the main scene owns the request controller immediately after its candidate dependency.
`CharacterDialogueMisquotationEvidenceCandidateConsiderationFlowController` can explicitly start
one decision only for that request controller's exact current request while the existing shared task
is idle. It validates request identity and task availability before taking the next one-shot,
configured, or offline model, so unavailable, stale, overlapping, cancelled, and completed paths do
not consume a queued model. The main scene owns this flow after its request dependency, but only the
dedicated switch-gated smoke invokes it after creating the exact live occurrence, candidate, and
request. These owners do not automatically deliver dialogue, append a transcript, start a task,
consume a model, create or expose evidence, select, present, acknowledge, correct, consume, or
persist anything.

`CharacterDialogueMisquotationEvidenceSelection` fully preflights at most 64 caller-ordered
evidence candidates, rejects nulls and duplicate exact delivered events, and returns the exact first
candidate observable by one supplied actor. Distinct value-equal deliveries remain separate exact
candidates. A provider-neutral consideration request retains that exact actor and a copied read-only
candidate view under the same bounds. Its explicit decision selects one exact request candidate or
abstains, and the pure acceptance policy rechecks exact actor identity, request membership, and
current observability. The immutable consideration result binds one accepted exact decision to its
exact request and resolves the exact selected request evidence, or `null` for abstention. These
contracts do not expose or apply evidence, change dialogue or truth, or mutate any character,
event, audience, transcript, caller collection, or world state. A provider-neutral cancellation-aware
Application interface obtains one decision, and its controller forwards the exact request and caller
cancellation once before constructing only the existing validated result. It adds no provider,
retry, deadline, fallback, task, scene, persistence, or application behavior. The strict OpenAI
path accepts only `actorId` plus a nullable zero-based candidate index, rebinds both to exact request
instances, requires a complete bounded semantic-event/SSE lifecycle, and serializes one stateless
streaming request with trusted actor/candidate context and a closed schema. Its caller-composed
adapter loads one credential only on explicit invocation, rejects failed response bodies without
reading them, and requires an event-stream success response. The exact settings/model binding and
factory retain caller-owned dependencies without construction-time credential or network I/O.
This transport path does not expose or apply evidence, deliver dialogue, mutate truth or
transcripts, select retries or fallbacks, or compose a task, scene, settings source, or persistence.

`TemporaryCharacterDialogueMisquotationEvidenceSource` authors one stable representative enemy,
one exact heard source event, one exact already-composed delivered occurrence, and one exact
observable non-actor witness candidate. Its source-owned consideration flow creates a fresh request
retaining that exact actor and candidate only on explicit start, then uses the existing one-shot,
configured, or offline model selection and bounded task. The main scene owns those inert nodes in
dependency order; its switch-gated smoke proves exact selection, abstention, overlap preservation,
explicit reset, and fail-closed offline behavior without changing either dialogue event or the
candidate. Ordinary startup does not begin a task or invoke a model. This composition does not
expose or apply evidence, present or acknowledge a result, deliver dialogue, mutate truth or a
transcript, or add credentials, provider traffic, persistence, input, retry, timing, scheduling,
navigation, or movement.

`CharacterDialogueMisquotationEvidenceExposureSnapshot` defensively copies at most 64 exact
evidence entries in stable caller order and rejects nulls or duplicate exact delivered events while
allowing distinct value-equal events. Its pure exposure transition returns the exact current
snapshot for abstention or appends the selected result evidence into a fresh snapshot; exact repeat
delivery and capacity exhaustion fail without changing current state. A scene controller initializes
one exact snapshot once and resolves each exact consideration request at most once only from the
expected current snapshot. Selection installs the fresh transition result, while abstention retains
the exact current instance. Stale, duplicate, overflowing, repeated, uninitialized, and null inputs
leave controller state unchanged. These contracts do not present, acknowledge, discover, correct,
or persist evidence; change dialogue, truth, transcripts, characters, or world state; or compose
task results into the main scene.

`CharacterDialogueMisquotationEvidenceExposureCoordinator` observes each fresh exact completed
misquotation-evidence task result once and delegates it only from the exposure controller's exact
current snapshot. Selection installs the transition's fresh snapshot, while abstention retains the
exact current instance. Explicit acknowledgement resets only the exact observed result while its
post-resolution exposure snapshot remains current; stale acknowledgement and stale, duplicate,
full, idle, running, cancelled, failed, null, or repeatedly observed results are inert. The temporary
exposure flow initializes exact empty scene state without starting a task, and the main scene owns
the exposure controller and coordinator in dependency order. Ordinary startup remains inert, and
the switch-gated smoke proves exact selection, abstention, acknowledgement, offline failure, and
unchanged source dialogue. This composition does not present, discover, correct, or persist
evidence; deliver dialogue; change truth, transcripts, characters, events, audiences, world state,
or perception; or add input, retry, timing, scheduling, navigation, or movement behavior.

`CharacterDialogueMisquotationEvidenceViewSnapshot` retains one exact witness identifier and a
defensive stable copy of at most 64 exact exposed entries for only that witness. It rejects nulls,
entries owned by any other or distinct value-equal witness, and duplicate exact delivered events,
while allowing distinct value-equal deliveries. Its pure projection filters one exact exposure
snapshot by witness reference identity without recreating entries, occurrences, or dialogue
events. `CharacterDialogueMisquotationEvidenceViewController` owns the exact source exposure and
projected view behind one-time initialization and exact-reference compare-and-swap refresh. The
exposure coordinator advances that view with each successful selection; abstention retains both
exact instances, and acknowledgement requires the exposure and view to remain aligned. The inert
main-scene flow initializes the representative witness's empty view without presenting,
discovering, correcting, consuming, or persisting evidence; changing dialogue, truth, transcripts,
characters, events, audiences, world state, or perception; or adding input, provider, retry,
timing, scheduling, navigation, or movement behavior.

`CharacterDialogueMisquotationEvidencePresentationSelection` retains one exact witness view and
either one exact member evidence entry or explicit abstention. Construction rejects non-members,
including distinct value-equal evidence, and its pure policy accepts the selection only for the
exact current view instance so stale or distinct equivalent views fail closed. A scene controller
accepts at most one explicit choice per exact current view, retains all nested evidence and dialogue
identities, and replaces an older choice only after the scene-owned view advances. The main scene
owns that controller after the view owner but never invokes it automatically: ordinary startup,
selection, abstention, acknowledgement, and offline failure remain presentation-free. This
selection layer adds no UI, truth judgment, correction, discovery, consumption, persistence,
dialogue delivery, input, provider activity, scheduling, navigation, or movement behavior.

`CharacterDialogueMisquotationEvidencePresentationSnapshot` derives one exact witness identifier,
source dialogue event, and delivered misquotation event from one non-abstaining exact presentation
selection without copying text or declaring either event true or false. Its pure projection returns
that immutable event pair only when the selection retains the exact current witness view; accepted
abstention and stale or distinct equivalent views return no presentation. A scene controller
projects each exact current selection at most once, records abstention without fabricating a
snapshot, and exposes a selected snapshot only while its exact selection and view remain current.
Explicit acknowledgement accepts only that exact current view, selection, and presentation, then
hides only the presentation while retaining the view, selection, exposure, evidence, and dialogue
events. Stale, distinct equivalent, abstaining, repeated, and null acknowledgement inputs fail
closed.
The main scene owns that controller after its view and selection dependencies but never invokes it
automatically. This projection layer adds no text formatting, UI, truth judgment, correction,
discovery, evidence consumption, persistence, dialogue delivery, input, provider activity,
scheduling, navigation, or movement behavior.

`TemporaryCharacterDialogueMisquotationEvidencePresentationFlowController` explicitly composes
one exact current witness-view choice through the existing selection and presentation owners. It
accepts either one exact view member or abstention, and stale, distinct equivalent, non-member, and
repeated choices fail closed. It also delegates exact acknowledgement without mutating the retained
selection or evidence. The main scene owns this flow after its dependencies but does not call it
automatically. Its switch-gated smoke proves an explicit initial abstention and later exact
selection and acknowledgement after successful exposure while ordinary startup and failures remain
presentation-free. Acknowledgement hides only the exact presentation; the switch-gated smoke
retains the exact main-scene view, selection, exposure, evidence, and source dialogue events. This
orchestration adds no automatic UI behavior, truth judgment, correction, discovery, evidence
consumption, persistence, dialogue delivery, input, provider activity, scheduling, navigation, or
movement behavior.

`TemporaryCharacterDialogueMisquotationEvidencePresentationPanel` is an initially hidden
screen-space temporary UI that accepts only the presentation owner's exact current non-null
snapshot. It renders the source and delivered utterance texts separately under the neutral labels
`Source statement` and `Delivered statement`, without calling either one true, false, corrected, or
authoritative. A stale or distinct equivalent snapshot is rejected, and the panel clears and hides
after the exact presentation is acknowledged or becomes stale. The main scene owns this panel but
never presents it automatically. Its neutral `Dismiss` input delegates only the exact current view,
selection, and non-null presentation through the existing acknowledgement flow; missing, stale,
distinct equivalent, repeated, and already-acknowledged state stays inert. Dismissal clears only the
temporary text and visibility and does not mutate the retained view, selection, exposure, evidence,
or dialogue events.

`TemporaryCharacterDialogueMisquotationEvidenceReviewPanel` keeps its statement list hidden until
the player explicitly chooses `Review evidence`. An empty or unavailable witness view remains inert;
otherwise the list renders each exact current entry in stable exposure order as separate neutral
source and delivered statement text. `Close review` hides and clears that observational list without
selecting anything. Selecting one list item resolves only that exact retained entry and delegates it
through the existing one-shot presentation flow and panel; absent, stale, repeated, and out-of-range
signals fail closed. The main scene owns this input surface, while evidence creation and exposure
remain switch-gated. Review, selection, close, and dismissal do not label either statement as true,
false, corrected, or authoritative and do not discover, consume, correct, persist, or redeliver
evidence or mutate dialogue, transcripts, characters, events, audiences, world state, or perception.

The dedicated switch-gated live-misquotation main-scene smoke carries its exact newly delivered
event through occurrence, witness-candidate creation, request ownership, one completed decision,
exposure, witness-view refresh, and explicit result acknowledgement. It then reviews, selects,
renders, and dismisses that exact live evidence through the existing truth-neutral UI owners. The
distinct preauthored candidate is rejected even though it describes equivalent authored dialogue;
repeated observation, acknowledgement, selection, and dismissal remain inert. The task returns to
idle while the exact live exposure, witness view, request, candidate, occurrence, and dialogue
events remain retained. Ordinary startup still performs none of these actions, and the smoke does
not append a transcript, correct or consume evidence, judge truth, persist state, invoke a provider,
or change characters, world objects, perception, navigation, movement, or general dialogue flow.

`TemporaryCharacterObjectSabotageSubstitutionSource` retains the exact scene world-object snapshot
and its stable water cask, one representative enemy with complete target access, empty substitution
and evidence state, one distinct authored replacement, and one exact eligible evidence-bound
candidate. Its flow initializes the scene controller without effect and applies only on explicit
request. Recovery projects retained authoritative evidence through supplied perception and removes
only the exact active substitution. `CharacterObjectSabotageSubstitutionPresentationController`
stale-safely projects that overlay onto distinct original and replacement cask polygons. The main
scene starts with the original visible and uses no new input; ordinary exact water-cask focused-search
resolution can recover an explicitly applied substitution while unrelated outcomes leave it active.
World-object truth, evidence, source objects, cask position, and actor positions remain unchanged.

`CharacterObjectSabotageSubstitutionConsiderationRequest` retains the exact character, physical
access, world objects, substitutions, evidence, and a stable copied collection of at most 64 exact
candidates with unique evidence identifiers. Its provider-neutral decision selects one exact
evidence identifier or explicitly abstains. The pure acceptance policy requires the exact character
identifier instance and either abstention or an exact currently eligible candidate identifier; an
unknown or distinct value-equal identifier fails closed. An immutable consideration result retains
the exact request, accepted decision, and exact selected request candidate, or null only for explicit
abstention. The provider-neutral cancellation-aware decision seam obtains one decision and returns
only that validated result. Its pure consideration transition delegates selected results to the
existing atomic substitution/evidence transition while preserving abstention without application.
The scene controller resolves a result only when its request retains the exact current scene-owned
substitution and evidence snapshots, installs both fresh selection snapshots atomically, and accepts
each exact request at most once. A closed scene task owns one decision through idle, running,
completed, cancelled, and failed states, resolves only a non-stale successful result on the main
thread, and resets only explicitly from terminal state. One temporary source gives an exact one-shot
model precedence over an exact configured binding, otherwise returning one stable fail-closed
offline model. Binding initialization, replacement, and clearing use exact-reference compare-and-
swap and preserve a queued one-shot. The selected-settings scene flow composes, replaces, or clears
the OpenAI substitution binding without construction-time credential or network input/output. The
main scene composes exact current source and scene snapshots into one request only on explicit start;
overlap, cancellation, provider failure, invalid decisions, and stale success remain mutation-free.
A scene coordinator observes each fresh completed result once, projects only a selected result
through the existing exact substitution presentation seam, and explicitly acknowledges only an
already-observed result whose presentation or abstention state remains current. The strict JSON
boundary canonicalizes accepted identifiers back to exact request instances, rejects malformed or
unsupported documents behind one bounded error, and serializes a stateless streaming OpenAI request
with a closed selection-or-null schema and only bounded trusted character, target, replacement, and
eligibility context.

The request-bound substitution stream accumulator accepts only the supported OpenAI Responses
semantic lifecycle, bounds output deltas, validates the complete output-text document through the
strict decoder, and exposes the exact accepted decision only after one successful completion event.
Its bounded SSE completion seam rejects empty, malformed, incomplete, refused, failed, unframed,
duplicate-terminal, and post-completion input without retaining or exposing provider text. It
preserves caller cancellation and transport I/O failure identity and leaves the caller-owned stream
open. The OpenAI adapter composes that strict path with the existing serializer over caller-owned
HTTP and credential dependencies, requires an event-stream success response, redacts transport
failures, and performs no input/output until explicitly invoked. Its immutable Application binding
retains the exact OpenAI settings and model; the factory creates that binding with a default or
validated caller-supplied endpoint without loading credentials, sending HTTP, or invoking a model.
These contracts add no automatic invocation or acknowledgement, persistence, timing, scheduling,
event, time, transcript, navigation, input, or UI.

`CharacterObjectSabotageMoveController` initializes exact placement and authoritative evidence
snapshots once. Its compare-and-swap operations install both fresh move-result snapshots or only the
fresh recovered placements while retaining the exact evidence snapshot. Stale, value-equal expected
snapshots and ineligible operations fail without partial installation. The isolated controller is
not wired into `Main.tscn`, selects no sabotage behavior or timing, and adds no persistence,
navigation, presentation, provider, event, time, transcript, or input behavior.

`TemporaryCharacterObjectSabotageMoveSource` retains the existing exact world-object snapshot and
its stable water cask, authors exact source and destination locations, and exposes one representative
enemy, complete physical access, the exact initial placement, empty evidence, one currently eligible
move candidate, and its exact aligned recovery candidate. Its isolated smoke proves stable bounded
read-only output and inert startup. It initializes no scene-owned state, applies or recovers nothing,
and adds no coordinate, body, path, reachability, duration, timing, persistence, input, UI, event,
transcript, navigation, provider, model, or presentation behavior.

`TemporaryCharacterObjectSabotageMoveFlowController` initializes the existing move controller from
that exact authored placement and empty evidence without applying the move. One explicit request
delegates the existing all-or-nothing move transition; recovery projects the current authoritative
evidence through one supplied perception snapshot and delegates the exact recovery transition.
Repeat application, unseen evidence, and repeated recovery fail closed; recovery retains evidence,
and source snapshots remain unchanged. The flow adds no automatic timing or choice, physical scene
movement, persistence, UI, provider, or presentation.
The main scene now composes the exact source, move controller, and explicit flow in dependency order.
An opt-in smoke proves that ordinary startup retains the exact water cask, authored source
placement, eligible move, aligned recovery, and empty scene-owned evidence without invoking either
operation. No ordinary scene flow calls the move or recovery seams.

`CharacterObjectSabotageMovePresentationController` binds one exact authoritative world object,
exact source and destination `Location` instances, and explicit finite scene coordinates. It
initially projects and then stale-safely advances only snapshots retaining that exact target at one
of those exact locations. Value-equal replacement targets or locations, unsupported locations,
stale or repeated snapshots, invalid coordinates, and nulls preserve the body position and last
presented snapshot. This isolated presentation contract does not choose or apply sabotage, infer a
path or coordinate, move an actor, or add timing, navigation, persistence, input, UI, or providers.

`TemporaryCharacterObjectSabotageMovePresentationFlowController` composes that projection with the
existing main-scene move state and explicit flow. Startup presents the water cask at its exact
authored source coordinate without applying a move. Each explicit apply or perception-gated
recovery first requires the presentation controller's exact last snapshot to match current
scene-owned placements, then presents only the exact fresh successful transition. Repeat apply,
unseen recovery, repeated recovery, and stale presentation fail closed. Recovery restores the exact
retained source coordinate and evidence remains authoritative; world objects and all authored source
snapshots remain unchanged. No ordinary caller or automatic schedule invokes these operations, and
the coordinator adds no actor movement, derived coordinates or paths, evidence consumption,
persistence, input, UI, events, time, transcripts, navigation, providers, or models.

Ordinary focused-search resolution also forwards only the exact water-cask outcome and current
perception to the move presentation flow. If an explicit move left its exact authoritative evidence,
identifying that cask restores its retained source placement and scene coordinate; resolving another
target leaves the cask at its destination. This forwarding neither applies sabotage nor changes
world-object truth, actors, evidence, authored source snapshots, search policy, or any unrelated
placement.

`CharacterObjectSabotageMoveConsiderationRequest` immutably retains one exact character, physical-
access snapshot, authoritative world-object, placement, and evidence snapshots, plus at most 64
exact move candidates in stable caller order. It defensively copies the supplied candidates and
rejects null entries, duplicate evidence identifiers, excess entries, and null authority inputs.
The provider-neutral request does not select or apply a move, recover a placement, project scene
coordinates, invoke a model, or mutate any retained input.

`CharacterObjectSabotageMoveDecision` explicitly selects one exact move-candidate evidence
identifier or abstains with no selection. Its pure acceptance policy requires the exact considering
character identifier and, for selection, the exact identifier of a candidate accepted by the
complete current move policy. The immutable consideration result binds one accepted decision to its
exact request and retains the exact selected request candidate, or `null` for abstention. A
provider-neutral cancellation-aware Application interface obtains one decision, and its coordinator
returns only that validated exact result. The pure consideration transition delegates selection to
the existing all-or-nothing move transition and returns no application for abstention. The scene
controller resolves a result only when its request retains the exact current placement and evidence
snapshots: selection installs both fresh result snapshots atomically, while abstention consumes the
exact request and retains both scene instances. A bounded scene task obtains one decision with
caller cancellation, installs only a non-stale accepted result on the main thread, and exposes
closed idle, running, completed, cancelled, and failed states plus terminal reset. These seams do
not recover a move, choose timing, construct a provider model, or invoke a provider by themselves.
The temporary scene model source retains at most one exact caller-supplied move model for the next
decision and otherwise returns one stable fail-closed offline model. One explicit flow composes the
authored enemy, access, world, exact current placement and evidence snapshots, and move candidate,
then starts the bounded task with the next model only while that task is idle. Overlap rejection
does not consume a newly queued model. `Main.tscn` wires the source, task, and explicit flow after
the existing move state initialization; ordinary startup remains inert with the offline model,
idle task, source placement, and empty evidence. These scene seams add no configured binding,
request serialization, provider adapter or invocation, automatic timing, recovery or presentation,
persistence, input, UI, event, time, transcript, navigation, or actor-movement behavior.
Infrastructure strictly decodes one complete selection-or-abstention document against the exact
move request, serializes only the relevant trusted character, eligible candidate, and authored
destination context into a stateless strict OpenAI Responses request, and completes only a bounded
valid SSE lifecycle. One caller-composed authenticated adapter exposes that transport behind the
provider-neutral move-decision interface. Construction performs no credential load or network I/O;
explicit invocation validates credentials, response status and media type, redacts transport and
semantic failures, and preserves caller cancellation. Exact immutable bindings retain the canonical
OpenAI setting and model and are composed through default or caller-supplied validated endpoints.
The scene model source supports exact stale-safe configured-binding initialization, replacement,
and clearing while preserving one-shot precedence and stable fail-closed offline behavior. The
existing selected-settings flow activates and replaces that future OpenAI move model without
construction-time input/output, while Ollama clears it back to offline behavior.

One scene coordinator observes each fresh exact completed move result at most once. A selected
result projects only the request's exact prior placement to the current scene-owned placement
through the existing stale-safe presentation seam; abstention and every other terminal state
preserve the body and task state. Explicit acknowledgement resets only an exact already-observed
completed result: a selection must retain the exact current last-presented placement, while an
abstention must retain its exact request placement and evidence snapshots as current scene state.
Unobserved, stale, idle, running, failed, null, and repeated acknowledgements remain inert. These
contracts do not select timing, schedule or acknowledge work automatically, recover a move, persist
state, or add input, UI, events, time, transcripts, navigation, or actor movement.

`CharacterObjectSabotageConcealmentController` initializes exact concealment and authoritative
evidence snapshots once. Its compare-and-swap operations delegate the complete Domain transitions
and install both fresh explicit or first-eligible apply results, or only the fresh recovered
concealment while retaining the exact evidence snapshot. Stale, value-equal expected snapshots and
ineligible operations fail without partial installation. First-eligible application does not choose
when to run or retain a candidate schedule. `TemporaryCharacterObjectSabotageSource` retains the
exact water-cask object and complete snapshot from the existing scene world-object source,
then authors one exact enemy concealment candidate with explicit access and empty initial state. It
also retains one stable read-only bounded collection whose sole entry is that exact candidate. The
temporary flow routes its explicit apply request through first-eligible collection selection while
preserving the exact candidate for recovery and presentation. `Main.tscn` initializes that source,
scene-owned state, and temporary flow without applying an effect.

`CharacterObjectSabotageConcealmentPresentationController` projects exact immutable concealment
state onto one retained scene body. Initialization and later presentation retain exact target and
snapshot instances; only a fresh snapshot from the exact last-presented snapshot can change
visibility. The temporary main-scene presentation flow explicitly composes one eligible apply with
hiding the water-cask body, then uses the existing perception-to-evidence projection so noticed
authoritative evidence can recover and reveal it. Unseen, null, stale, and repeated operations
preserve state and presentation. The ordinary focused-search route now supplies that discovery
path: the exact cask is a value-equal interaction target with one exact capability-gated condition,
and only successful resolution of its own recorded outcome asks the presentation flow to recover
it. Resolving another target leaves the cask concealed. No sabotage behavior or timing is selected
automatically, and these seams do not move an object or actor, infer coordinate access, consume
evidence, persist state, or invoke a provider.

`RepresentativeInfectedCharacterSource` creates exactly two fresh dormant roles in stable
forester-then-scribe order. Each exact identity is shared with its dormant controller assignment
and carries distinct latent background, capability, knowledge, and goal state plus an authored
deep-sleep condition and dormant-response limitation. The source does not define authoritative
disease state, healing, wake-up or controller transitions, dialogue, scenes, or persistence.

The complete character-model boundary still does not define mutable state, lifecycle or event
handling, runtime registration, relationship or goal mechanics, capability authorization,
behavior selection, perception filtering, dialogue context, persistence, serialization, Godot,
or UI behavior.

## Dialogue model boundaries

`DialogueDeliveryMode` defines only ordinary speech and whispering as closed provider-neutral
values. An immutable `DialogueDelivery` retains one exact existing `DialogueUtterance` and exact
mode with record value equality. Delivery is explicit authored or player intent and is never
inferred from dialogue text. The pure Application `DialogueDeliveryHearingSelector` validates one
ordinary and one whisper hearing range, selects only the range named by the exact mode, and delegates
bounded stable candidate filtering to `DialogueHearingSelector`. It does not create dialogue events,
infer direct address or privacy, inspect visibility or occlusion, mutate scene state, or define UI.
`DialogueEvent` now retains the exact delivery and audience while continuing to project that
delivery's exact utterance. Every event construction explicitly names ordinary speech or
whispering. The scene occurrence controller accepts one exact delivery, validates both named
hearing ranges through the mode-aware selector, and retains no event on invalid input. An isolated
scene check proves an exact whisper uses the shorter inclusive hearing boundary without changing
surrounding state.
The temporary dialogue panel exposes an explicit `WHISPER` toggle. The input controller retains
the resulting exact delivery and projects its utterance for existing consumers; `Main` carries that
same delivery into event occurrence. The spoken transcript retains the exact delivery while
continuing to project its utterance for existing consumers. `Main` appends the exact occurred player
delivery. `CharacterTurnResponse` retains one optional exact delivery from the responder and projects
its utterance as speech; deterministic responses explicitly use ordinary speech. Provider responses
must pair non-null speech with exactly one canonical delivery mode and pair null speech with null
mode. The scene response recorder applies that exact delivery to hearing and transcript append, and
an end-to-end check proves an explicit whisper reaches only the nearby listener. Provider-neutral
recent-speech and current untrusted-dialogue context entries now retain their exact delivery mode,
while non-dialogue context rejects one. The generation request requires the current entry's text and
mode to be the exact request values. Shared trusted request JSON serializes the current delivery mode
and each recent-speech mode for OpenAI, Anthropic, Ollama, and compatible local requests while the
raw in-world line remains separate untrusted user content. Version-4 save JSON retains each exact
heard and response delivery mode; versions 1 through 3 migrate historically implicit non-null speech
as ordinary speech. These explicit distance-governed flows complete the bounded whisper/private-
conversation roadmap item without inferring privacy or direct address from text or promising secrecy
against every physically present listener.

`DialogueUtterance` retains one exact typed `CharacterId` speaker and the player's exact nonblank
Unicode text without trimming, normalization, case folding, or line-ending changes. Text is bounded
to 2,000 UTF-16 code units. `DialogueAudienceSnapshot` retains that exact speaker plus at most 32
exact, duplicate-free listener identifiers in stable supplied order; it copies caller input and
rejects the speaker as a listener.

`CharacterTurnSpeakerSchedulingPolicy` orders at most 32 distinct eligible speaker identifiers by
placing value-equal directly addressed candidates first while retaining stable eligible order and
the exact eligible instances inside both priority partitions. Absent addresses are ignored, input
collections are unchanged, and the returned schedule is an independent read-only collection. The
policy does not infer addresses, decide whether anyone should speak, start turns, generate text,
schedule interjections, or control runtime work.

`CharacterTurnParticipationKind` defines exactly respond, interject, and remain-silent choices in
stable order. One immutable `CharacterTurnParticipationDecision` retains an exact scheduled
character identifier and exact kind. Interjection and meaningful silence require one exact existing
authored goal as their motive, while a direct response cannot retain a motive. The pure acceptance
policy additionally requires the character's exact identity and exact retained goal, permits a
response only when directly addressed, and permits an interjection only when not directly
addressed. From at most 32 distinct, already-accepted decisions in scheduled order, the pure
selection policy returns the exact first response or interjection and returns no selection when all
characters remain silent. A complete-round policy accepts at most 32 distinct exact scheduled
characters, one positionally aligned decision per character, and at most 32 distinct direct
addresses. It uses value-based address membership while preserving exact character, decision, and
authored-goal identity, validates every decision before selection, and returns that existing first
speaking decision or no selection. One immutable provider-neutral participation-consideration
request retains an exact scheduled `Character`, exact heard `DialogueEvent`, and explicit direct-
address flag only when that character's identifier is present by value among the event listeners.
Distinct value-equal audience identity is accepted without replacing any supplied or nested
instance. A pure consideration policy delegates one returned decision to the existing exact-
identity, direct-address, and authored-motive gate using only that request. A bounded round
transition preflights up to 32 distinct scheduled characters against the same exact heard event and
up to 32 distinct direct-address identifiers before returning one request per character in stable
order. It uses value-based listener and address membership while retaining every exact character
and the exact event, returns an independent read-only collection, and permits an empty round. These
requests may be paired with a decision only through an immutable result that applies the exact
consideration gate first and retains both exact inputs. One complete immutable result round copies
and retains 0–32 positionally aligned exact requests and accepted results, requires distinct
scheduled character identifiers and one shared exact heard event, and exposes independent read-only
collections. A one-method provider-neutral Application interface obtains a cancellation-aware
decision for one exact request. Its bounded controller delegates complete request preflight to the
Domain transition before any invocation, calls the interface once per request in stable order, and
returns only the validated complete result round. Its deadline overload applies one positive finite
deadline across that complete sequential round, returns no partial result on expiry, and preserves
caller cancellation distinctly from timeout without retrying. A pure budget-gated selection policy
delegates speaker choice over that round's exact accepted decisions and returns the exact aligned
result. The scene budget controller can start a follow-up using that result's exact decision and
heard event without advancing the budget before completion. A strict bounded JSON decoder
reconstructs a participation decision with the exact scheduled-character identifier and exact
matching authored goal, then reapplies the consideration policy. The OpenAI Responses participation
serializer sends the heard utterance as untrusted in-world dialogue, supplies bounded exact
character identity, background, goals, and direct-address context, and requests a closed two-
property decision schema. Its semantic SSE completion accepts one bounded verified output-text
document followed by successful completion, never exposes deltas as presentation, and returns only
the exact decoded decision. A caller-composed OpenAI participation adapter loads one immediate
credential, posts that stateless request to the exact validated Responses endpoint, requires an SSE
success response, preserves caller cancellation, and redacts transport and rejected-response
details. An immutable Application binding retains one exact participation model with exact matching
canonical provider settings. A non-empty snapshot copies up to all three canonical provider
bindings in stable order, rejecting duplicates and mismatches, while pure selectors return only the
exact provider binding authorized by the exact selected settings instance. A focused Infrastructure
factory composes the OpenAI adapter over exact caller-owned settings, HTTP, credential, and endpoint
dependencies without loading credentials, sending HTTP, or invoking the model. These contracts do
not generate or infer motives or addresses, define local Chat Completions, Anthropic, or Ollama
participation dialects, retry participation decisions, initialize
an autonomous chain, integrate the ordinary scene flow, or alter player interruption behavior. A
focused Godot task controller owns at most one complete participation-decision round, applies its
result only from the main-thread process callback, and exposes closed idle, running, completed,
caller-cancelled, timed-out, and failed states without partial decisions or failure details. The
temporary scene source gives one exact caller-supplied model precedence, otherwise selects a stable
exact settings-backed model, and returns one stable fail-closed offline model when unconfigured. It
atomically initializes or replaces valid bindings, rejects unavailable providers and mismatched
settings, and can clear bindings without consuming a queued one-shot model. Existing settings
composition installs both ordinary and participation adapters for OpenAI without input/output and
clears participation back to offline when a supported ordinary provider has no participation
adapter. The controller and source do not invoke participation models during composition, retry,
select or start a follow-up, advance a budget, or mutate dialogue, transcript, world, or
presentation state. One scene-owned ordinary player-dialogue coordinator accepts one complete
bounded stable collection of exact heard LLM listeners and one complete distinct value-based
direct-address set. It materializes and validates both inputs with the available continuing budget,
idle participation task, and free character-turn slot before replacing a budget or consuming one
model. It starts at most one bounded round, retains no copy of the event or result, hands only a
complete result to the existing budget-gated follow-up-start seam, and resets every terminal task
outcome. Meaningful silence, offline or provider failure, timeout, cancellation, interruption,
exhaustion, invalid or ineligible listener collections, overlap, and an active turn start no
follow-up. An isolated credential-free two-listener scene check proves stable consideration order,
value-equal direct addressing, direct-response priority, exact motivated interjection after
meaningful direct-listener silence, all-silent no-turn behavior, overlap rejection, and unchanged
surrounding scene state. A temporary ordinary participant source owns one stable read-only roster
containing the exact existing healer followed by one fresh representative sage with distinct exact
LLM assignments. Ordinary player dialogue hearing includes both roster members from the moving
healer body and a scene-owned sage position while keeping only the healer directly addressed. The
complete heard roster is scheduled after preflight, and a pure `CharacterTurnParticipantLookup`
resolves the exact value-matching participant for an awaiting selected turn only after validating
the whole roster. Absent and non-awaiting turns fail closed without retaining or mutating state.

The ordinary generation-input source now supplies either roster participant with exact turn and
character identity, filtered character-specific perception, only referenced knowledge and goals,
that character's unfinished commitment memory, heard recent speech, and only currently valid
affordances. Healer context and behavior remain unchanged; the sage receives no unobserved world
state or physical affordance. Exact response-speech recording resolves the selected roster member
to the healer body or sage position, selects its heard audience, and appends one occurrence. A
selected direct healer response or motivated sage interjection traverses the same bounded
generation, validated reception, speech, completion, and one-step budget advance. After exact
character speech completes, only heard LLM roster members enter the next participation round under
the existing budget. An isolated close-position smoke proves sage then healer interjections advance
in order and that the exhausted two-turn budget starts no third round. Player cancellation still
cancels an active participation round and monotonically interrupts that exact chain. Consecutive
eligible player roots replace the prior budget with distinct fresh state only after complete
preflight. The scene does not infer addresses or privacy from text, persist an active chain, add a
sage sprite, or broaden provider dialects.

`AutonomousConversationBudget` retains a nonnegative completed-character-turn count, a positive
configured maximum no greater than the existing 16-turn recent-conversation window, and whether the
player has interrupted the chain. Pure continuation and transition rules permit another turn only
below that maximum and before interruption, record exactly one completion at a time, and make
interruption monotonic. A focused scene controller owns one exact immutable budget and delegates
those transitions. Pure composition policies gate both the earlier complete scheduled decision
round and the completed consideration-result round, so an exhausted or interrupted chain cannot
expose a follow-up selection. The latter retains the exact aligned result and lets the controller
start its exact heard-event follow-up. Selection and start do not transition the budget, create a
conversation loop, schedule speakers, infer participation, or wire ordinary player input.

Independent character-task Domain contracts use canonical task identifiers and retain exact
assigned characters plus exact non-wait examine/collect intentions. Bounded immutable assignment
and lifecycle snapshots preserve stable order, reject value-equal task duplicates, and allow at
most one active task per character. Pure lifecycle rules permit only assigned-to-active/cancelled
and active-to-completed/cancelled transitions. Stable scheduling selects the first assigned task
for a character only while that character has no active task. One exact active task can complete
only through a physical-action transcript entry retaining its exact assigned character and
intention plus an existing authoritative eligible world event; the atomic occurrence transition
returns fresh task, event-log, world-time, and transcript snapshots. A separate bounded party
snapshot and departure transition atomically start the selected task and remove the same
value-equal party member without moving any character. A focused Godot controller now owns exact
task and party snapshots, initializes them once, and starts a departure only from the exact expected
current pair. The temporary scene source authors one healer herb-patch examination and the exact
healer-first ordinary party; `Main` installs those snapshots and starts that exact departure without
moving the healer. A separate authored source binds the resulting exact active task to the existing
healer examination event and physical-action transcript entry. Its scene controller completes the
task and installs fresh task, event-log, world-time, and transcript snapshots only when every
expected current snapshot is still exact. Credential-free smokes prove stable selection,
same-character separation, stale and ineligible no-ops, exact authoritative completion, and no
partial installation. Ordinary scene flow does not yet trigger that completion, navigate or hide a
departing character, simulate off-screen time, rejoin a party member, change dialogue/model context,
or persist task or party state.

One immutable Application travel plan retains an exact active independent-task activity, validated
finite world bounds and origin, and 1-32 stable copied waypoints. It delegates all bounds and
coordinate validation to the existing provider-neutral navigation request validator. Immutable
progress retains that exact plan and exposes only its completed waypoint count and final completion
state. A pure transition advances through exactly the next waypoint, one at a time, and rejects
skips, repeats, completed progress, or a distinct plan identity without changing either input. These
contracts do not choose a route, add Godot navigation, define elapsed time, speed, interpolation, or
collision behavior, move or hide a body, trigger a task occurrence, or invoke a provider. A focused
Godot controller owns one exact plan and its current immutable progress, initializes once, and
installs a one-waypoint advance only from the exact expected current plan and progress. Its isolated
credential-free smoke proves stale, skipped, repeated, and completed inputs leave both scene values
unchanged. The controller does not author or start a route, integrate `Main`, or broaden any motion
or timing policy. A temporary scene source binds only the exact authored active healer task to one
fresh route from its captured departure position to the exact existing herb-patch scene position.
`Main` creates that route only after the atomic task departure and initializes the scene travel
controller without advancing progress or changing the healer body. An isolated source smoke proves
the task, origin, destination, and zero progress are retained exactly. This composition still does
not simulate travel, hide a separated character, trigger the task occurrence, or rejoin the party.

Independent-task occurrence now also requires the exact scene-owned travel plan and its exact
completed immutable progress. A pure Application eligibility policy rejects incomplete progress,
a distinct plan, and distinct or value-equal activity instances. The occurrence controller
compare-and-swaps that travel state before applying the existing atomic task, event-log,
qualitative-time, and transcript transition. Its credential-free smoke proves pre-arrival,
mismatched, stale, and repeated attempts preserve every surrounding snapshot, while the exact
authored one-waypoint final arrival permits one occurrence. `Main` still does not advance travel or
trigger that occurrence, and no elapsed-time, speed, interpolation, collision, body presentation,
return travel, party rejoin, persistence, or provider policy is implied.

A separate pure Application transition composes the exact final waypoint with the existing
authoritative independent-task occurrence. It returns one immutable result containing fresh
completed travel progress plus fresh task, event-log, qualitative-time, and transcript state only
after every travel and occurrence precondition succeeds. Non-final, skipped, repeated,
distinct-plan, distinct-activity, stale-task, ineligible-event, and null inputs fail without changing
any supplied immutable value. This combined result is not yet installed by Godot or ordinary flow.

The scene occurrence controller can now delegate that combined transition and install its exact
completed travel, task, event-log, qualitative-time, and transcript state through one
compare-and-swap operation. Its smoke proves stale snapshots, mismatched travel, an unrecognized
event, pre-arrival occurrence, repetition, and nulls cannot partially change scene state. This
method also retains the exact accepted combined result and initializes one authored return route
from the herb patch to the outbound origin. The return route is created only for the exact authored
completed healer task; invalid, stale, mismatched, repeated, or already-initialized state cannot
partially install it.

Provider-neutral return-travel contracts retain that exact successful arrival result, its exact
completed task activity, validated finite bounds and origin, and 1-32 stable copied waypoints. The
return must begin at the outbound final waypoint and end at the outbound origin. Immutable progress
advances only through the exact next waypoint. A scene controller owns one exact return plan and
progress, while a temporary source authors only the exact healer route. Domain rejoin rules append
one absent exact character to a party with capacity without changing existing order. A pure
Application transition composes only the exact final return waypoint with that bounded rejoin, and
the scene installs both fresh states through compare-and-swap guards. `Main` still does not advance
outbound or return travel, trigger the task occurrence, or rejoin the party; no elapsed-time, speed,
interpolation, collision, navigation-agent, off-screen-location, body-presentation,
dialogue/model-context, persistence, or provider policy was added.

A separate scene-owned round-trip coordinator delegates exactly one caller-supplied next waypoint.
Non-final outbound and return steps use the existing immutable travel controllers; final outbound
arrival delegates the existing atomic task occurrence and return-route initialization, while final
return arrival delegates the existing atomic party rejoin. Its isolated credential-free smoke
drives the authored one-waypoint outbound and return routes in order and proves stale, mismatched,
repeated, and null input cannot change any scene-owned snapshot. `Main` does not call this
coordinator, and it defines no timing, movement, navigation, body-presentation, persistence,
dialogue/model-context, or provider policy.

The coordinator also accepts one complete exact scene snapshot and selects the only active leg: an
outbound step requires no initialized return plus the exact current active-task occurrence, while a
return step requires the exact initialized return pair and no occurrence candidate. It rejects an
incomplete, stale, mismatched, or caller-selected phase before delegating either existing operation.

Provider-neutral independent-task step timing now consists of one finite positive cadence bounded
to one hour and one immutable clock retaining unconsumed elapsed time for that exact cadence. A pure
transition accepts no more than one interval per call, produces a fresh clock, and exposes at most
one due step while carrying sub-interval remainder forward. It does not select or advance a route,
read a scene clock, process a frame, move or hide a body, or define navigation, persistence,
dialogue/model-context, or provider behavior.

A focused scene controller initializes one exact task-step cadence and its zero-elapsed clock once.
It installs each pure clock advance only from the exact expected cadence and clock, retaining the
exact accepted result so a later coordinator can observe whether one step became due. Stale,
repeated, invalid-duration, and null input cannot change its clock. The controller does not read
Godot frame time, invoke the round-trip coordinator, or alter task, party, world, transcript, or
body state.

The round-trip coordinator can now compose one exact clock advance with the complete caller-supplied
task, party, travel, world-event, time, and transcript snapshot. Sub-interval elapsed time changes
only the immutable clock. When a step becomes due, the clock result installs only after the active
outbound or return operation succeeds, so stale, invalid, failed, or repeated route input leaves the
due interval unconsumed and every scene snapshot unchanged. This remains an explicit call boundary;
`Main` does not initialize the cadence or process elapsed time.

`Main` now initializes a fixed three-second authored cadence and advances its immutable scene clock
with finite non-negative frame elapsed time capped to one interval per process call. Each due
interval supplies the exact current task, party, outbound or return progress, world-event, time, and
transcript state to the timed coordinator. The first authored interval completes the herb-patch
arrival and authoritative task occurrence; the second completes return travel and stable party
rejoin; completed return state stops further clock consumption.

A dedicated body-presentation controller retains the exact authored healer identifier and last
presented party snapshot. Initial separated party state hides that body, and only a fresh exact
party snapshot can update visibility; the stable rejoined party therefore restores it once. The
live round-trip keeps the healer's scene position unchanged throughout. This presentation adds no
interpolation, speed, collision/navigation agent, generalized concealment action, off-screen
position model, dialogue/model-context, persistence, or provider behavior.

The pure Application `DialogueHearingSelector` accepts one exact speaker, a finite world position,
a finite positive inclusive range, and at most 32 uniquely identified finite-position candidates.
It excludes the speaker and returns every in-range listener in stable candidate order, retaining
exact identifiers. It does not inspect visibility, perception, obstacles, line of sight, acoustic
properties, scene nodes, or Godot state.

`DialogueEvent` immutably associates an exact utterance with a value-matching audience.
`DialogueActType` defines only the eight canonical roadmap categories, and `DialogueAct` pairs one
exact utterance with one explicitly declared type. These values perform no text classification;
prompt-shaped text remains ordinary in-world speech and gains no instruction or action authority.

The temporary bottom dialogue panel submits either Return or its button through the focused
`DialogueInputController`. The controller initializes once with the exact scene Seeker identifier,
constructs `DialogueUtterance` directly so the Domain remains the sole text validator, and exposes
only the exact last accepted utterance. Rejected null, blank, or oversized text preserves the last
accepted value; successful repeated submissions create distinct occurrences and clear the field.

`Main` observes each fresh accepted utterance exactly once. `DialogueEventController` delegates the
scene's Seeker and healer positions and the named 192-pixel inclusive range to
`DialogueHearingSelector`, then retains the exact resulting in-world event. The scene's empty
`CharacterTurnTranscriptController` appends that same utterance as structurally tagged spoken
speech through `CharacterTurnTranscriptAppendTransition`. When no turn is active, the temporary
request source gates the exact healer against that event's audience, pairs the event's exact
utterance with the explicitly supplied `request` act type, and starts one exact awaiting-response
activity. Awaiting and speaking activity cannot be replaced; after completion or cancellation, a
later heard utterance can start a fresh exact activity. No text classification occurs.

`CharacterTurnRequest` is provider-neutral and retains one exact responding character, dialogue
event, and act. It rejects a responder outside the event audience and rejects an act for a
different utterance. The initial `CharacterIntentionType` vocabulary is limited to `wait`,
`examine-object`, and `collect-object`. `CharacterIntention` retains that type and an optional exact
target `ObjectId`, including invalid shapes so `CharacterIntentionValidationPolicy` can return the
closed missing-target or unexpected-target error without mutating input. The policy does not look
up world objects or authorize or execute an action.

`CharacterTurnDisposition` is the closed exact-singleton vocabulary `accept`, `refuse`, `clarify`,
and `conditionally-accept`. `CharacterCommitment` defensively retains one non-empty ordered sequence
of exact proposed intentions without selecting, advancing, observing, or executing a step.
`CharacterStateProposal` retains only a canonical key and exact nonblank proposed value bounded to
500 UTF-16 code units. It is proposed character-owned text, not world truth or mutation authority.

`CharacterTurnResponse` retains its exact request and disposition, optional exact bounded delivery
from the value-matching responder while projecting its utterance as speech, exactly one immediate
intention, a copied nullable-free list of commitments, and a copied stable list of at most eight
state proposals. It does not validate or execute intentions, advance commitments, apply proposals,
or inspect world state.

The pure deterministic response selector maps only the declared dialogue-act type to fixed
accept, refuse, clarify, or conditionally-accept behavior. Commitment progress advances one
value-equal pending intention per observed action. Character-turn activity contracts represent
awaiting, speaking, completed, and cancelled states. Pure reception accepts only a complete
response retaining the exact awaited request and produces speaking activity; pure completion
accepts only speaking activity. Both transitions retain exact request and response instances.
Pure start eligibility permits a fresh awaiting activity only when current activity is absent,
completed, or cancelled, and its transition retains the exact new request without changing the
prior immutable activity.

The separate deterministic examination selector accepts an exact request and explicitly supplied
target identifier. It produces a fresh accepted response with fixed healer speech, an exact-target
`examine-object` immediate intention, and no commitments or state proposals. The temporary live
response source uses that selector with the exact authored herb-patch identifier; it does not infer
the target from dialogue text.

Pure immediate-intention occurrence policy requires the exact speaking response, responder,
non-wait action, latest observed response speech, and authoritative eligible world event before
delegating atomic event/time/transcript recording. `CharacterExaminationResult` can retain only a
targeted observed examination occurrence and one closed `safe-to-collect` or
`unsafe-to-collect` result. The deterministic post-examination selector proposes collection of the
exact examined target only for the safe result and otherwise waits. A separate pure collection
occurrence gate additionally requires the exact examination occurrence to remain latest in the
transcript before atomic authoritative collection recording. These contracts do not infer an
examination result from speech, descriptions, or hidden world state.

The pure deterministic examination-result selector returns a fresh `safe-to-collect` result only
when one observed occurrence is a targeted `examine-object` action retaining the exact explicitly
supplied herb-patch identifier. Distinct value-equal identifiers and malformed occurrences fail
closed. The temporary result source delegates only that exact boundary. Separate authored advancing
events identify the healer's herb-patch examination and collection without changing the object.

The focused Godot `CharacterTurnActivityController` initializes once from one exact activity and
delegates fresh starts, cancellation, response reception, and speaking completion to those Domain
contracts. The live scene begins without an activity. On a later process pass, a separate temporary
response source delegates the exact awaiting request to the pure deterministic selector and the
controller receives that fresh complete response as speaking activity. The dialogue panel reveals
only that already complete exact speech, one Unicode text element per process pass. A displayed
prefix has no response or action authority.

While the response is speaking, a temporary recorder emits its exact healer utterance once through
the existing hearing boundary and appends that same utterance once to the transcript. It does
nothing for awaiting, completed, or cancelled activity. A separate completion controller first
observes that exact event and transcript entry, preserves a one-frame speaking cancellation window,
and only then delegates completion. The temporary examination flow separately occurs the exact
response intention through the authoritative event/time/transcript coordinator. On the following
pass it selects the exact safe result, then selects and occurs collection only while the exact
examination remains the latest transcript occurrence. Intervening speech therefore prevents
collection. Consecutive heard player utterances still produce distinct completed activities after
the prior one is terminal.

Transcript entries structurally distinguish exact spoken utterances from exact non-wait physical
intentions. Immutable transcript snapshots and append transitions retain exact entries in stable
occurrence order. `CharacterTurnWorldEventOccurrence` links one exact authoritative `WorldEvent`
definition to one exact tagged transcript entry without inferring execution from a response
proposal. Its pure policy accepts only the exact event instance retained in the authored snapshot
and delegates existing world-event eligibility. The pure transition records that exact event
identifier, applies its authored qualitative-time effect, appends the exact transcript occurrence,
and returns all resulting immutable state together with the retained occurrence. Failed
eligibility changes nothing.

The completed deterministic Phase 3 loop does not classify dialogue text, stream provider content,
apply state proposals, infer an examination target or general safety, advance live commitments, add
provider contracts, or mutate inventory, world objects, discovery, or movement from a response.
Examination and collection are authoritative recorded occurrences only; the herb patch remains in
the live world and no item is added to an inventory.

## Live character-model boundaries

The Application layer now owns the provider-neutral `ICharacterModel` seam. It accepts one exact
`CharacterModelGenerationRequest`, observes caller cancellation, and exposes an asynchronous stream
of closed Application update types. Exact non-empty text deltas permit whitespace-only fragments,
retain text without normalization, and are individually and cumulatively bounded to
`DialogueUtterance.MaximumTextLength`. An immutable stream prefix preserves exact updates in stable
order and rejects unknown updates, excessive combined text, multiple terminal outcomes, or any
update after termination.

One terminal update distinguishes completed, caller-cancelled, timed-out, and failed generation.
Failures carry only a bounded provider-neutral code for unavailable, authentication, rate-limit,
transport, invalid-response, or provider-rejection outcomes; provider error bodies and DTOs do not
enter Domain state. The generation controller enforces one caller-supplied deadline across the
whole operation, translates caller cancellation and deadline expiry into distinct terminal states,
and rejects a stream that ends without a terminal update as an invalid response. It permits one
retry only for unavailable, rate-limited, or transport failures that emitted no text; it never
retries exposed partial text, authentication, invalid responses, provider rejection, cancellation,
or timeout. An opt-in progress observer receives accepted text deltas and only the final validated
terminal update; retryable text-free failures stay hidden and observer failure or independent
cancellation cannot alter generation. The scene queues those observations across the async boundary,
applies them on the Godot main thread, and presents text without parsing or executing it. The
authored healer's canonical LLM assignment routes ordinary heard player requests through the exact
bounded generation input and a scene-owned model source. The source consumes one exact
caller-composed model for the next generation only, then resumes its stable credential-free
unavailable default. One scripted ordinary response proves exact target-free `Wait` speech can be
validated, recorded once, and completed without physical authority; the next request resets and
fails offline without retaining the prior result. Prompt-shaped dialogue remains untrusted when a
scripted response targets a hallucinated object: validation replaces that completion with a bounded
invalid-response failure and no speech or mutation. Explicit ordinary cancellation preserves an
already-accepted text delta only as presentation and cancels the exact pending turn. Additional
credential-free ordinary checks prove a hidden text-free unavailable attempt safely retries the
same exact request, a non-retryable provider rejection runs once, and the scene's 30-second deadline
produces a distinct timeout. An unterminated stream with exposed text cannot retry and becomes a
bounded invalid response. Two consecutive caller-composed models receive distinct exact turns,
with stream presentation reset and both speeches recorded once in order. Every unsuccessful path
cancels only its pending turn, exposes only canonical status plus accepted presentation text, and
does not fabricate dialogue, fallback, action, or authoritative mutation. The deterministic
response source remains explicit focused-test scaffolding only. No real provider call occurs in
standard validation. Ordinary gameplay exposes only one targeted model affordance and executes it
only after the completed response passes the existing provider-neutral validation boundary.

Provider-neutral model context is an immutable stable sequence of typed text entries, bounded to
128 entries, 4,000 UTF-16 code units per entry, and 32,000 code units in total. Its closed kinds
separate filtered perception, relevant knowledge, goals, memories, recent speech, valid affordances,
and untrusted player dialogue. The assembler derives identified perception, the exact responding
character's current authored knowledge, and goals, then retains only separately supplied memory
entries, derives recent local speech, retains only separately supplied valid-affordance entries,
and appends exactly one untrusted player-dialogue entry. Perception projection exposes only
non-empty authored descriptions for exact
`VisibilityState.Identified` entries backed by an authoritative world object; all other visibility
states, identifiers, display names, and absent objects remain hidden. Knowledge projection follows
the character's ordered references, excludes unreferenced authored content, and fails closed when
a referenced entry is missing. Both preserve exact text instances. Goal projection preserves exact
authored description text and order without exposing goal keys or other character state.
Recent-speech projection preserves only exact utterance text from supplied dialogue events the
character spoke or was recorded as hearing; unrelated dialogue is omitted. The assembler rejects
mismatched character/perception inputs, null supplied entries, and every kind other than exact
`Memory` or exact `ValidAffordance` in its corresponding supplied input. Generation requests reject
missing, duplicate, changed, or merely value-equal replacement player text. The assembler does not
derive memories or valid affordances, claim current affordance eligibility, define a retained
conversation store or window, or call a provider.

Complete provider-neutral responses pass one pure Application validation gate before they can be
accepted. The gate requires the exact caller-expected turn request, delegates existing structural
validation for the immediate intention and every future commitment intention, and rejects targeted
intentions whose value-equal identifier is absent from the authoritative world-object snapshot.
The immediate intention also must be `Wait` or match an exact type and value-equal target pair in an
immutable current-affordance snapshot. Current eligibility is deliberately not projected onto
future commitments. Errors retain the exact offending intention in deterministic response order;
the response, requests, intentions, world objects, and affordances remain unchanged. The
affordance snapshot is still supplied by a trusted caller rather than derived from gameplay state or
serialized into model context.

Phase 5 begins with provider-neutral local memory contracts rather than provider-owned state. A
`CharacterRecentConversationWindow` retains up to 16 exact completed character turns and 16,000
UTF-16 code units of their heard and response speech in stable oldest-to-newest order. Each entry
has a canonical `CharacterTurnId`; identifiers are unique per window, and pure append evicts only
the oldest turns needed to restore both bounds. Construction and transition copy their collections,
retain exact supplied character, activity, and turn instances, and reject incomplete activities,
mismatched characters, and duplicate turn identities.

Significant episodic memories remain separate from both the recent window and authoritative truth.
Each `CharacterEpisodicMemory` has a canonical memory identity, one exact remembering character, a
bounded unique participant list that includes that character, an exact location, an exact
qualitative-world-time snapshot, completed-turn provenance, and a non-blank summary of at most
1,000 UTF-16 code units. A copied read-only per-character snapshot preserves up to 128 exact memory
instances in supplied order with unique identities. These initial contracts do not yet derive,
retrieve, persist, summarize, migrate, or project memory into model context, and do not define UI
or scene ownership.

Epistemic memory is also local and provider-neutral. Each `CharacterEpistemicMemory` has a canonical
identity, one exact remembering character, one exact completed source turn, bounded statement text,
and exactly one closed category: confirmed fact, attributed claim, or uncertain belief. Only an
attributed claim accepts—and requires—an exact speaker. A copied per-character snapshot retains at
most 256 exact entries in stable unique-identity order. This structural separation records the
character's classification; it does not establish authoritative world truth or derive memory from
dialogue.

An uncertain belief alone can have a `CharacterBeliefAssessment`. It retains the exact belief,
stored confidence from 0 to 10,000 basis points, and independently ordered supporting and
contradicting `EvidenceId` references. Each disposition is copied, duplicate-free, capped at 32,
and cannot contain an identity from the other disposition. The assessment does not calculate
confidence, reinterpret evidence, or promote or demote epistemic categories.

`CharacterRelationshipMemory` stores one directional character-to-character view over the closed
trust, affection, respect, and fear dimensions, each bounded from -100 to 100. Recorded reasons
have canonical identities, exact completed-turn provenance, one retained dimension, and bounded
text. Dimensions and reasons are copied in stable order; dimensions and reason identities are
unique, reasons can reference only retained dimensions, each relationship retains at most 64
reasons, and each per-character snapshot retains at most 64 distinct related characters. These
contracts do not update values, imply reciprocity, or yet bound change across an exchange.

One `CharacterRelationshipChange` can alter one retained dimension by only 1 to 10 points in either
direction and carries one exact locally authored reason whose source turn identifies the exchange.
The pure eligibility policy requires value-equal directional ownership, an existing dimension,
available reason capacity, a unique reason and source turn, and an in-range result; it rejects
overflow instead of clamping. The immutable transition replaces only that dimension, appends the
exact reason, and retains unrelated dimension and reason instances. A source turn can therefore
affect a relationship only once, and no ordinary exchange can reverse a strongly held value.

Unfinished character commitments are stored separately as promises, assigned tasks, or an explicit
other kind. Each `CharacterCommitmentMemory` has a canonical identity, exact responsible character,
exact counterparty, exact completed source turn, and bounded description. A copied per-character
snapshot preserves at most 128 exact commitments in stable unique-identity order. There is no
completion transition, scheduling, execution policy, or provider-side state in these contracts.

Deterministic memory indexing uses four closed local categories—episodic, epistemic, relationship,
and commitment—and four closed canonical tag kinds: topic, entity, location, and relationship. A
category-safe reference retains exactly one existing memory and its exact owner; each index entry
copies 1 to 16 unique tags in supplied order, and a per-character index copies at most 576 exact
entries with unique category-specific memory identities. The index does not copy memory content or
derive, summarize, persist, or project any memory. A per-character query copies 1 to 8 unique exact
tags and requests 1 to 32 results. Pure retrieval requires all query tags, distinguishes the same
value under different tag kinds, returns exact entries in index order, and stops at the requested
bound without scoring, ranking, fuzzy matching, or input mutation.

Older-conversation summaries remain separate from both the bounded recent window and the
authoritative transcript. Each local `CharacterConversationSummary` has a canonical identity, one
exact remembering character, 1 to 32 copied unique exact source-turn identities in supplied order,
and non-blank text of at most 2,000 UTF-16 code units. A copied per-character snapshot retains at
most 128 exact summaries with unique identities. These contracts do not derive or generate text,
persist, migrate, index, retrieve, or project summaries. A pure recording transition accepts only
source-turn identities present in the recent window in the same relative order, appends the exact
caller-supplied summary, and returns the exact recent window and authoritative transcript unchanged;
it neither edits nor evicts either conversation source.

`CharacterMemoryAggregate` is the canonical local provider-neutral memory state for one character.
It retains the exact recent window, older summaries, episodic and epistemic memories, belief
assessments, relationships, unfinished commitments, and deterministic index while requiring
value-equal ownership throughout. Every indexed reference must retain the exact memory instance in
its corresponding category snapshot; a missing, merely value-equal replacement, or same-valued
memory from another category fails closed. The aggregate has no provider conversation identifier,
provider response state, reasoning, or chain-of-thought field. Provider requests remain stateless
and reasoning-free, so no provider-owned state can replace this local canonical aggregate.

`CharacterMemorySnapshot` copies zero to 32 exact per-character aggregates into stable supplied
order, rejects null entries and value-equal duplicate character identities, and exposes only a
get-only read-only collection. The in-memory save root can retain this exact snapshot under the
version-`2` character-memory vocabulary. A pure one-way migration accepts only a version-`1` root
and produces a fresh version-`2` root retaining the exact discovery, world-event, and shared empty
memory instances. Strict current-schema loading executes that migration before complete restoration;
already-version-`2` roots retain their fully materialized memory. The scene owns one exact immutable
memory snapshot, starts explicitly empty, captures canonical version-`2` JSON, and installs memory
with discovery and world-event state only after one complete preflight. Rejected roots preserve all
three state areas, and legacy documents cannot silently discard populated memory. This boundary does
not synthesize memory, complete commitments, change relationships, add save UI, or alter the
authoritative transcript.

One pure Application projection now selects only the value-equal responding character's exact
unfinished commitments from canonical memory. It emits stable `Memory` entries containing only the
closed commitment kind, counterparty identity, completed source-turn identity, and exact description;
missing character memory yields no entries, while null or over-budget input fails closed through the
canonical memory and existing model-context bounds. The temporary scene supplies those entries from
the scene-owned memory controller to the existing context assembler. A credential-free ordinary-flow
smoke serializes a healer promise, resets and restores canonical memory, and proves the same bounded
promise and source reach distinct consecutive OpenAI- and Anthropic-bound requests after an exact
future-turn binding replacement, without supplying the complete campaign transcript. Providers still
cannot own, rewrite, derive, summarize, complete, or otherwise mutate canonical memory.

Phase 6 begins with hidden engine-owned disease truth rather than character belief, memory, dialogue,
or provider state. Canonical `DiseaseId` values identify up to 32 exact immutable authored disease
definitions. `DiseaseTruthSnapshot` copies up to 256 exact character-disease infections in stable
order, permits multiple distinct diseases per character, rejects duplicate character/disease pairs,
and requires every infection to retain the exact definition instance in the authoritative snapshot.
The closed progression vocabulary is exposed, early illness, drowsing, deep sleep, wasting, and
final decline.

Pure disease progression replaces only one exact retained infection and advances exactly one stage.
It requires the exact authored `Advance` world event to be retained, recorded, and current, and each
infection retains the exact last progression-event identity so the same meaningful-time event cannot
be consumed twice. Final decline cannot progress. A separate Application selector yields a dormant
effective controller from deep sleep onward without replacing the configured assignment; before deep
sleep or without matching infection it returns that assignment by exact instance. Qualitative
condition projection keeps exposure invisible and emits only fixed nonnumeric appearance text for
later stages, without disease identity or model-context wiring. Disease persistence, scene ownership,
automatic progression, and authored-scene ownership remain unimplemented.

Successful cure is an explicit immutable outcome retaining the exact authoritative infection and
exact authored world event; it is never inferred from provider text or a qualitative condition. A
pure eligibility policy requires that infection, event definition, and most recently recorded event
identity. The transition removes only the cured infection while retaining exact definitions and every
unrelated infection in stable order. The existing effective-controller selector consequently returns
the exact configured assignment after the last deep infection is removed, while any other deep
infection keeps the character dormant.

Authored disease exposures likewise retain the exact character, disease definition, and world event.
Eligible exposures require that event to be the most recently recorded occurrence and append one exact
`Exposed` infection only when the disease and event are canonical, the character is not already
infected, capacity remains, and no matching active protection exists. Protective-measure definitions
link an exact disease to a discovery-evidence identity; applying one requires that exact definition,
the evidence in the exact character view, and the most recently recorded authored application event.
Protection and infection state stay bounded and immutable. There is still no automatic spread,
duration or removal of protection, scene ownership, persistence, recipe, ingredient, experiment,
journal, UI, or provider wiring.

Patient assessment is a pure, fail-closed Application projection over engine-owned truth. Immutable
requests and character-to-character perception inputs retain exact assessor and patient identities.
Eligibility requires a distinct patient, matching perception, the exact assessor character, an
authored `assess-patients` capability, and no more than 32 inspected capabilities. Eligible results
contain only the existing fixed qualitative condition vocabulary for the perceived patient; exposure
and disease identities remain hidden, and equal symptoms from multiple diseases are deduplicated.
The result is an observation, not a diagnosis, fact, belief, or treatment authority.

Each infection now retains one closed supportive-care state: unassisted, comforted, or stabilized.
Explicit comfort and stabilization outcomes retain the exact infection, stronger care state, and
authored world event. Their pure policy requires that infection and event by exact instance and that
event as the most recently recorded occurrence; the transition replaces only the cared-for
infection. Comfort leaves meaningful-event progression active, while stabilization blocks further
progression. Successful cure remains the separate explicit outcome that removes the infection.
No provider text or qualitative observation can apply care or cure.

Canonical `IngredientId` values identify authored ingredients independently of physical world
objects. One immutable ingredient definition retains that exact identity. Ingredient quantities use
only positive bounded integers in the closed count, milligram, or millilitre vocabulary; units never
convert implicitly. One immutable ingredient state retains the exact definition and quantity plus
closed qualitative freshness, pre-preparation physical condition, and contamination values. There
is no decay clock, state transition, inventory, scene, persistence, provider, or discovery wiring.

Ingredient preparation has a closed grinding, heating, steeping, cooling, and fermentation
vocabulary. A plan copies one to 16 exact steps in authored order and permits repeated process kinds.
Immutable progress exposes only the exact next step; a pure policy and transition advance it exactly
once, and distinct value-equal or out-of-order steps fail closed. Canonical preparation-recipe
identities distinguish authored recipes. A recipe retains one exact plan, one to 16 ordered exact
input requirements with unique ingredient identities, and one exact result definition and quantity.
Requirements and ingredient-state snapshots are copied, read-only, bounded, and duplicate-free;
amounts must use the same exact unit and never convert implicitly.

A pure availability policy requires every recipe input by exact definition instance, exact unit, and
sufficient integer amount. It deliberately does not invent freshness, physical-condition, or
contamination requirements. Pure consumption removes depleted states, replaces only partially
consumed states in their original positions, and preserves unrelated exact instances. Starting a
recipe atomically consumes eligible inputs and creates progress at step zero. An activity can advance
only through its exact next authored step, and only complete progress produces the exact
recipe-authored result outcome. The result is not automatically added to ingredient storage.

A dosage retains one exact completed preparation outcome and a positive quantity no larger than that
outcome in the same exact unit. An authored administration outcome retains exact administrator and
patient identities, that exact dosage, and one exact meaningful world event; it does not itself
validate event occurrence or change disease truth. Self-administration is not prohibited at this
structural boundary. A reusable prepared batch begins with the exact completed preparation and its
full exact quantity. Pure consumption accepts only a dosage retaining that exact preparation and
unit with no more than the remaining amount; partial use returns a new batch with the exact
preparation and reduced positive quantity, while complete use returns no remaining batch. Rejected
dosage leaves the input batch unchanged. A bounded immutable prepared-batch snapshot retains up to
256 distinct exact preparation instances. Its pure consumption transition finds only the dosage's
exact preparation, replaces a partially consumed batch in place, removes a fully consumed batch, and
preserves every unrelated exact batch in stable order. One combined administration transition
preflights that batch, the exact latest authored event, and both journal boundaries before returning
new batch state, the rich administration-specific record, and the universal experiment record as one
externally atomic result. Rejection exposes no partial state. The transition still does not occur the
event, apply disease truth or consequences, own scene state, or persist the snapshots.

Ingredient-administration experiment records retain a canonical experiment identity, one exact
administration outcome, and one exact qualitative condition observation. Empty observations remain
valid and do not mean "no effect." The bounded immutable journal preserves exact records in authored
order, rejects duplicate experiment identities, and provides a pure append transition. Recording is
now atomically gated on the administration outcome's exact authored world event being the most recent
authoritative occurrence. The transition constructs the exact record only after that check and then
delegates duplicate and capacity enforcement to the existing journal append transition. It does not
infer efficacy, consume dosage, mutate disease truth, or provide scene ownership or persistence, so
the richer administration-specific record remains available for treatment logic.

A separate universal experiment journal can record every kind of authored experiment rather than
only ingredient administration. Each immutable record retains an exact experiment identity, the
exact authoritative world event, and bounded nonblank procedure and observed-outcome text without
interpreting efficacy. The journal copies up to 256 records in stable order and rejects duplicate
experiment or event identities. Its pure atomic recorder accepts any exact authored world event only
when that event identity is the newest authoritative log entry; stale or value-equal replacement
events, duplicates, and capacity exhaustion leave the input journal unchanged. Scene ownership and
persistence remain separate later boundaries.

Investigation contracts keep open questions, hypotheses, evidence, conclusions, and consequences
separate. Questions, hypotheses, conclusions, and non-executable consequences retain canonical
identities plus bounded exact authored text; hypotheses remain possible answers rather than facts.
Evidence links use only the closed `supports` or `contradicts` relationship and do not bypass the
existing character-filtered evidence view. An authored conclusion plan contains one to eight
distinct alternate routes, each requiring one to 16 unique evidence identities. A pure confirmation
policy succeeds only when one whole route is visible and never pools partial alternate routes.
Confirmation atomically removes the exact open question and appends the exact authored conclusion;
it does not parse natural language, execute consequences, reveal hidden evidence, or change gameplay
state.

A hypothesis-recognition foundation canonicalizes a proposed question and hypothesis identity
against exact open-question and authored-hypothesis snapshots while retaining the exact source
natural-language utterance. Its output remains only a possible answer: it does not create a
conclusion, evidence, belief, fact, or gameplay mutation. A provider-neutral Application request
retains that exact utterance plus the exact bounded open-question and authored-hypothesis snapshots.
One cancellable interpreter may propose only a question/hypothesis identity pair or no match; the
controller delegates every proposal to the Domain canonicalization policy.

The first caller-composed Infrastructure interpreter serializes only that bounded state through a
narrow supplied transport seam. Its deterministic JSON marks the exact source text as untrusted
in-world text and carries only the exact open-question and hypothesis identities and authored
statements. Its strict response boundary accepts either an explicit no-match or one canonical
question/hypothesis identity pair; malformed, extra, duplicate, oversized, unknown-shape, and partial
documents fail without retaining provider text. Unknown but well-formed identities reach the existing
canonicalization policy and fail closed. Construction and the standard suite perform no network or
credential access. No provider binding, retry, scene ownership, or persistence exists yet.

Critical-conclusion route validation associates each evidence identity with a distinct authored
source and whether that source requires character cooperation. Every critical plan must have at
least two complete routes using at least three distinct sources, include one route independent of
cooperation, and retain a complete route after any single source is removed. The representative
first-patient investigation has four sources and two disjoint routes: abandoned-laundry residue plus
an old-shrine inscription, or an assistant preparation note plus a cooled-dose observation. This
recoverability is reused by the representative first-patient scenario.

The representative first-patient scenario retains an exact sleeping-patient identity, hidden
grey-sleep truth at deep sleep and without care, available exact moonwort leaf and clean-water state, one
grind/heat/steep/cool recipe, its stabilizing-remedy link, and the recoverable critical investigation.
A pure structural policy requires the exact patient infection and disease, exact recipe ingredient
definitions and units, and an acceptable critical-route scenario. Value-equal replacement patient,
disease, remedy, ingredient, and conclusion objects fail closed.

Stabilization additionally requires a distinct exact remedy-to-conclusion evidence requirement and
the exact authored conclusion in current confirmed state. Only an administration using the exact
patient, exact completed recipe, latest exact authored event, and an exact retained prepared batch
with enough quantity can delegate to the existing care and experiment-execution transitions. One
atomic first-patient result returns newly stabilized disease truth, the dose-consumed prepared-batch
snapshot, and newly appended administration-specific and universal experiment journals. Authored
procedure and qualitative outcome text is bounded by the universal journal and never interpreted as
efficacy. Depleted or replacement batches, duplicate or full journals, stale events, wrong
conclusions, and treatment-ineligible state expose no partial result and leave every supplied
snapshot unchanged. Focused representative tests prove either independent evidence route can
authorize early stabilization, any one source can be lost, an unauthored reasonable theory cannot
authorize care, and a universally recorded informative failed experiment can precede deterioration
and later successful recovery. These pure contracts do not occur events, execute consequences, or
own scene or persistence state.

The first Phase 6 scene controller owns one exact disease-truth snapshot, prepared-batch snapshot,
administration-specific journal, and universal experiment journal. It initializes all four once and
accepts stabilization only when the treatment context retains its exact current disease truth. A
successful call delegates to the pure first-patient transition before installing all four exact
result snapshots; rejected, stale, duplicate, full, or uninitialized calls preserve every installed
snapshot. The controller still does not occur the administration event, interpret observed text,
execute a consequence, save or load state, expose UI, or call a provider.

The ordinary scene now retains one exact authored representative first-patient treatment context and
initializes the scene controller once from that context's exact disease truth plus explicit empty
prepared-batch, administration-experiment, and universal-experiment snapshots. Startup creates no
completed preparation or administered dose. A focused credential-free smoke proves stable context
access, exact disease identity, empty journals and batches, and fail-closed repeated initialization
without changing world events, memory, movement, dialogue, model, or presentation state.

One separate scene controller now initializes from the context's exact authored remedy recipe and
ingredient snapshot and owns a single bounded preparation lifecycle. Starting the exact recipe
atomically installs the existing Domain transition's consumed ingredient snapshot and new activity;
only each exact next authored step can advance it, and only complete progress produces the exact
recipe-authored outcome.
Repeated, unavailable, value-equal-step, out-of-order, incomplete, or post-completion operations
preserve every installed value. Completion does not yet create a prepared batch, occur an event,
administer a dose, alter disease truth or journals, persist state, expose UI, or call a provider.

One pure prepared-batch append transition now turns an exact completed preparation into a full batch
at the end of stable existing order. It rejects duplicate exact preparations and full snapshots
without changing the input. The first-patient scene controller accepts that outcome only for the
exact current treatment context and its exact authored remedy recipe, then atomically installs the
new prepared-batch snapshot while retaining disease truth and both journals. It still does not occur
an administration event, select a dosage, stabilize the patient automatically, or add input/UI.

The temporary authoritative world-event source now also retains one exact advancing event for the
healer administering the first-patient remedy, without occurring it at startup. One pure composite
transition requires that exact authored event and one exact retained prepared batch, prospectively
occurs the event, constructs the dosage and administration from the retained preparation, and
delegates stabilization against the prospective latest event log. Its single immutable result
returns the exact new event log, world time, disease truth, prepared batches, administration-specific
journal, and universal journal together. Replacement events or batches, unavailable quantities,
stale or repeated occurrences, wrong contexts or preparations, and duplicate or full journals expose
no partial state.

One scene coordinator gathers both initialized controllers, delegates that whole preflight once,
then installs the already-accepted result into authoritative world-event and first-patient state
without a second fallible boundary. The credential-free ordinary-flow smoke prepares the exact
authored recipe in order, installs its full batch, occurs exactly one authored administration event,
consumes the exact bounded dose, stabilizes the exact patient infection, and appends both exact
experiment records atomically. Rejected replacement and repeated calls preserve both controllers and
unrelated scene state. There is still no player input or final UI, observed-text efficacy inference,
investigation-consequence execution, Phase 6 persistence, provider call, release, or distribution.

`Allrpcs.Domain.Persistence.FirstPatientSaveState` now composes the exact disease-truth,
prepared-batch, administration-specific experiment, and universal experiment snapshots as one
immutable value. `FirstPatientStateController.Capture` returns a fresh composition only when all
four live snapshots are initialized. It does not mutate or reinterpret first-patient state, and its
credential-free ordinary-scene smoke proves world-event, preparation, discovery, memory, dialogue,
movement, model, and presentation state remain unchanged.

`SaveSchemaVersion.FirstPatient` defines version `3`, whose in-memory `SaveSnapshot` retains one
exact first-patient composition in addition to version-2 character memory and the earlier dynamic
state. Earlier roots cannot retain this composition, and version `3` cannot omit it.
`SaveSnapshotFirstPatientMigration` is the pure version-2 to version-3 prerequisite: callers must
supply the exact authored initial first-patient state rather than having the migration fabricate
disease truth. Canonical version-3 JSON now writes and strictly reads complete disease truth,
completed preparations, remaining prepared batches, administration observations, and universal
experiment records. Stable document-local preparation indices preserve exact shared identity across
batches and administrations without serializing runtime object references. The reader rejects
unknown structure, invalid references, incomplete preparations, conflicting shared events, and
non-canonical values. `FirstPatientSaveStateRestorationTransition` then fails closed unless that
materialized graph is compatible with the exact authored disease, ingredients, recipe, steps, units,
patient infection, and authoritative events. It returns a fresh immutable graph rebound to those
exact authored instances while preserving saved order and dynamic values. The scene controller and
complete root coordinator install the rebound state only after full preflight; live capture,
legacy migration, canonical JSON, and local storage now carry complete version-3 state end to end.

The temporary scene now authors one complete stable healer whose exact identifier is shared with the
live turn source and whose controller assignment is the canonical LLM type. Its generation-input
source accepts only an exact awaiting healer activity and delegates context construction to the
Application assembler. One identified herb-patch description, one referenced knowledge entry, one
goal, only canonical unfinished commitment memory for the healer, the exact heard in-world request,
and the same text explicitly marked as untrusted player dialogue enter the bounded request. The
unnoticed stone marker, unreferenced knowledge, other characters' memory, display names, and other
scene state stay absent. The returned bundle retains the exact authoritative world-object and
targeted-affordance snapshots used for response validation.

`Allrpcs.Infrastructure.Dialogue.CharacterTurnResponseJsonDeserializer` is the first strict
provider-response boundary. It accepts one exact provider-neutral turn request and a complete JSON
document, then materializes only existing Domain response values. Speech and delivery mode must be
jointly null or a bounded string paired with exactly `ordinary-speech` or `whispering`. Exact
property sets reject unknown, missing, or duplicate members at every level; canonical closed
disposition and intention
vocabularies reject unknown actions; existing structural intention validation rejects missing or
unexpected targets. The initial closed intention vocabulary has no destination-bearing action, so
a proposed destination has no accepted document shape. The transport document, commitment count,
and intentions per commitment are bounded independently of the Domain response. This decoder does
not retain provider DTOs or error
bodies, execute intentions, or call a provider. Object existence and current immediate-action
eligibility are enforced separately by the Application validation gate.

Shared Infrastructure-only prompt and schema writers keep trusted bounded context separate from the
exact untrusted player utterance while preserving one closed local response decoder. The OpenAI
boundary serializes a stateless Responses request with `store:false`, streaming enabled, and the
fully bounded strict schema. A strict UTF-8 server-sent-event reader accepts LF or CRLF terminated
frames, complete comment heartbeats, optional event names, and repeated data lines while bounding
line length, line count, frame count, document count, and joined document length. Malformed,
truncated, excessive, or NUL-containing streams return no partial provider content.

`OpenAiCharacterModel` implements the provider-neutral model seam over a caller-owned `HttpClient`,
immutable provider/model settings, `IOpenAiCredentialSource`, and validated `OpenAiEndpoint`. The
official version-one API base remains the default. Caller-composed alternatives must use HTTPS, or
HTTP on a numeric loopback address with an explicit port; paths other than `/v1`, user information,
queries, and fragments are rejected. HTTPS bases use the derived Responses resource and bounded
semantic SSE completion. Numeric-loopback bases instead use deterministic non-streaming Chat
Completions so local servers can fully disable reasoning without their Responses compatibility
layer reopening a hidden reasoning budget. That path accepts only one bounded assistant choice with
an exact `stop` finish. Both yield one provider-neutral terminal update. Credentials exist only on
the transient request authorization header; unsuccessful response bodies are never read. HTTP
authentication, rate-limit, transient service, request-rejection, and transport outcomes map to
bounded codes without provider details.

The Anthropic boundary serializes one stateless Messages request with two trusted system text
blocks, one exact untrusted user message, streaming enabled, and `output_config.format` structured
JSON. Its provider schema omits unsupported string and array maximum constraints while the strict
local decoder still enforces every bound. The Messages accumulator accepts only one indexed text
block, buffers its JSON deltas, permits content-free pings, requires an `end_turn` message delta and
final stop, and rejects thinking, tools, refusal, unknown events, invalid ordering, malformed usage,
or incomplete output without retaining provider content. `AnthropicCharacterModel` uses a
caller-owned `HttpClient`, Anthropic settings, and read-only credential source; it posts only to the
official Messages endpoint with `x-api-key` and `anthropic-version: 2023-06-01`, never reads failure
bodies, and yields one provider-neutral terminal update with optional redacted usage diagnostics.

The Ollama request boundary disables thinking, requests streaming structured output with the same
closed schema, uses trusted system messages plus one exact untrusted user message, and requests
deterministic temperature zero. Provider-unsupported length and maximum-count schema keywords are
omitted while the strict local decoder retains every bound. A strict UTF-8 NDJSON reader bounds
every line, document, and stream count, rejects blank or truncated input, and leaves caller-owned
streams open. The native accumulator accepts only ordered assistant content and one successful
terminal stop, discards provider metadata, derives wholly present bounded usage, and maps malformed,
thinking, tool, error, or post-terminal shapes to redacted provider-neutral failures without
exposing partial structured JSON.

The Application diagnostic boundary retains only a canonical provider, terminal status or bounded
failure code, elapsed duration, and wholly present or absent input/output/total token counts. Counts
must be non-negative, additive, and no greater than one million; elapsed time is bounded to one
hour. A caller-owned cancellation-aware sink is isolated by a best-effort recorder so its failure
cannot replace the generation result. Responses usage is read only from the completed event, while
Chat Completions usage is mapped only from its bounded terminal envelope; all response metadata is
discarded. Optional adapter instrumentation uses caller-owned monotonic time and the sink for
completed or failed provider attempts. Absent usage remains unknown and malformed usage fails closed
without exposing partial provider content.

The provider/model settings boundary retains one exact canonical OpenAI, Anthropic, or Ollama
provider instance and one bounded model name. An immutable selection snapshot copies one to three
exact settings in caller order, requires exactly one per provider, and retains one explicitly
selected configured provider. Selection transitions preserve the exact settings instances and
order. Settings updates replace one provider at its stable position or append one newly configured
canonical provider, retaining the exact supplied setting and every unrelated instance. Removal is
limited to a configured unselected provider and likewise preserves stable order and exact unrelated
instances. An immutable copied binding snapshot retains at most one exact caller-composed model,
settings object, and connection tester per canonical provider. Resolution and redacted connection
testing require the selected binding to share the exact configured settings object. There is no
default, substitution, credential fallback, or model-name rewriting; missing or mismatched bindings
return only provider-neutral `Unavailable`.

The strict version-1 provider-settings JSON boundary writes only `schemaVersion`, canonical provider
identifiers, and bounded model names in stable settings order. Deserialization rejects malformed,
future, duplicate, missing, unknown, non-canonical, excessive, or selection-incomplete documents;
credentials and API bases have no field in the closed schema. The ordinary scene source retains the
exact settings selection and binding snapshots and replaces them only after validating the exact
selected binding. Replacement affects future `TakeModel` calls, preserves an already-taken model and
one-shot supplied-model precedence, and leaves rejected state unchanged. Its stable credential-free
offline default remains intact. It does not construct a provider adapter or read a settings document.

`ICharacterModelSettingsDocumentStore` is the provider-settings-specific Application port for one
already-serialized non-empty document. Writes return only `Saved` or `Failed`; reads return
`Loaded`, `Missing`, or `Failed`, and only a loaded result retains the exact document. The port does
not reuse the gameplay-save contract or expose paths, JSON, provider details, credentials, or
storage exceptions. `FileCharacterModelSettingsDocumentStore` implements strict UTF-8 reads and
same-directory atomic replacement at one trusted absolute path. Empty or invalid UTF-8 data fails
closed, and the document remains secret-free.

`CharacterModelSettingsDocumentController` composes the strict JSON boundary with that port. Loads
distinguish a missing document, storage failure, and invalid supported-schema input without leaking
parser details. The Godot `CharacterModelSettingsStorageController` owns the fixed
`user://character-model-settings-v1.json` path but neither saves nor loads in response to input on
its own. Its smoke uses a process-specific temporary file, so validation cannot replace ordinary
user settings.

The scene-owned `CharacterModelSettingsController` retains one exact loaded selection and delegates
replacement, append, selection, and removal to the immutable Application transitions. Rejected
changes preserve its current snapshot. Its atomic apply operation computes both the setting update
and selection before replacing current state. Saving is a separate explicit operation over only the
exact current snapshot; uninitialized and storage-failed saves return a bounded failure without
changing memory, and initialization, apply, update, selection, and removal never save implicitly.

The temporary settings panel exposes the three canonical providers and one bounded exact model name
without a default selection. Apply changes only scene-owned memory, remove accepts only a configured
unselected provider, save explicitly replaces the secret-free document, and activate explicitly
initializes or replaces the exact current supported binding for future turns. Activation performs no
save, credential read, connection test, or generation. Rejected input or activation retains the
exact prior live model, and activation during an ordinary turn cannot replace the model already taken
for that turn.

The same panel exposes a masked OpenAI credential field backed by a scene-owned controller. The
controller composes only the current OS credential store and performs no startup operation. Save
validates the bounded key, clears the field immediately, and delegates it without retaining or
presenting the key; delete never loads it first. Both controls expose only fixed saved, deleted,
missing, busy, or failed text. Scene exit cancels an active operation, and deterministic smokes use
only an injected store rather than a user's credential facility.

At scene startup, `TemporaryCharacterModelCompositionController` consumes that same retained valid
selection and composes only its exact selected setting. Ollama uses the fixed validated loopback
endpoint; OpenAI uses the existing OS-specific credential source on macOS, Linux, or Windows.
Construction reads no credential and makes no HTTP request. Missing, failed, invalid,
unsupported-platform, and Anthropic selections preserve the stable offline behavior. The scene does
not create or discover a local server, own a model process, or claim native Linux/Windows credential
proof. Explicit activation composes the current supported selection without input/output and
atomically replaces only future model takes. An explicit connection button composes only the exact
current selected supported binding and delegates its existing redacted tester. It displays only
connected, busy, or failed, performs no generation or scene mutation, and does nothing at startup.
The deterministic smoke harness disables automatic persisted composition globally and uses isolated
settings, credential stores, and bindings, so local validation cannot inherit ordinary user settings
or credentials or call a provider.

The OpenAI and Anthropic binding factories retain one exact caller-supplied settings object,
`HttpClient`, and credential source across the corresponding model and connection tester. OpenAI's
endpoint-specific overloads also share the exact validated API base. Numeric-loopback OpenAI-
compatible endpoints use deterministic non-streaming Chat Completions with `reasoning_effort` set
to `none`, a zero thinking budget, disabled thinking template flags, temperature zero, and a fixed
seed. HTTPS endpoints retain the Responses API path. Both paths use strict structured output and
the same bounded local decoder. The Ollama factory likewise shares one exact caller-supplied
`HttpClient` and validated loopback endpoint. Explicit diagnostics
overloads retain the exact caller-owned `TimeProvider` and redacted diagnostic sink on the model.
Construction rejects null or provider-mismatched inputs before creating a binding and performs no
credential load, HTTP request, provider discovery, or scene mutation.

OpenAI-compatible live conformance is distinct from the permissive fail-closed transport safety
probe. `ALLRPCS_LIVE_OPENAI_CONFORMANCE=1` runs three fixed target-free wait requests, while
`ALLRPCS_LIVE_OPENAI_ACTION_CONFORMANCE=1` runs three fixed known-target examination requests through
the existing response validation policy. Both require one completed response per request, exact
request identity, bounded healer speech, no commitments or state proposals, and the expected safe
intention. Their failure output contains only bounded aggregate counts; it retains no model text,
prompt, credential, or raw provider detail. Both remain network-free unless explicitly enabled and
also require the existing explicit API-base and model environment values.

Ollama has separately enabled parity checks over its existing fixed loopback endpoint.
`ALLRPCS_LIVE_OLLAMA_CONFORMANCE=1` runs the same three target-free wait requests, while
`ALLRPCS_LIVE_OLLAMA_ACTION_CONFORMANCE=1` runs the same three known-target examination requests.
Both require `ALLRPCS_LIVE_OLLAMA_MODEL` to name an already-installed model and reuse the same
provider-neutral evaluator and count-only failure summary. The original permissive Ollama safety
probe remains independent, and the standard suite performs no Ollama request when the live switches
and model are absent.

`ALLRPCS_LIVE_OLLAMA_ORDINARY_WAIT=1` and `ALLRPCS_LIVE_OLLAMA_ORDINARY_ACTION=1` separately supply
that explicitly named Ollama model to one ordinary healer turn in the Godot scene. The equivalent
OpenAI-compatible paths use `ALLRPCS_LIVE_OPENAI_ORDINARY_WAIT=1` or
`ALLRPCS_LIVE_OPENAI_ORDINARY_ACTION=1` plus the existing explicit API-base and model values. Exactly
one provider and mode may be enabled. The wait check requires completed bounded speech without
physical, event, time, world-object, or perception mutation. The action check submits the known
herb-patch request, requires a completed locally validated response, and proves examination and
deterministic safe collection retain the exact action chain while world objects and perception
remain unchanged. Console output is fixed and content-free. The checks do not discover or start a
server, select, pull, or modify a model, or run during standard validation.

The temporary ordinary healer input now exposes only one targeted action in addition to always-safe
wait: examining the identified herb patch. A completed model response still has to pass the existing
known-object and current-affordance validation before scene reception. The temporary examination
flow compares the validated canonical object-id value so an Infrastructure-deserialized target can
be distinct from the authoritative identifier instance, while the resulting occurrence, safe result,
collection, transcript, and world event retain the exact proposed action chain. Unknown or
unafforded targets remain failures and do not mutate authoritative state.

All three HTTP adapters remain caller-composed. `OllamaEndpoint` permits only an explicit HTTP
IP-loopback origin and derives the native `/api/chat` and `/api/show` resources; hostnames,
LAN/public addresses, credentials, paths, queries, fragments, HTTPS, and implicit ports are
rejected. `OpenAiEndpoint` derives only `/v1/responses`, `/v1/chat/completions`, and `/v1/models/`
resources from its bounded API base. The OpenAI and Anthropic connection testers retrieve only the
exact configured model
through their official caller-owned authenticated HTTP boundary; the Ollama tester posts only the
exact configured model to `/api/show` at its validated loopback endpoint. None generates content or
reads success or failure bodies, and all expose only provider-neutral outcomes.

`MacOsOpenAiCredentialStore` implements save/replace, load, and delete for one exact generic
password identity in macOS Keychain. The `/usr/bin/security` process is isolated behind an injected
boundary, never uses a shell, receives save bytes only through redirected standard input, bounds
and clears native output buffers, discards standard error, and maps OS details to loaded, missing,
saved, deleted, or failed outcomes.

`LinuxSecretServiceOpenAiCredentialStore` uses the fixed `Allrpcs.OpenAI` application and `api-key`
account attributes through `/usr/bin/secret-tool`. Availability is explicit and fails closed. The
native runner permits only exact store, lookup, and clear commands, passes save bytes through
redirected standard input without a shell or added newline, bounds lookup output, clears retained
secret buffers, and discards error details. Because `secret-tool clear` does not distinguish a
missing item, delete first performs a bounded lookup probe and clears its output before clearing the
exact item. Standard tests use deterministic fake boundaries and never touch a user's real
keychain or Secret Service. Linux support is not claimed until the same bytes are proved on Linux.

`WindowsOpenAiCredentialStore` uses one fixed generic `Allrpcs.OpenAI` target and `api-key` user
identity behind an injected Credential Manager seam. The native implementation uses the current
user's local-machine-persistent credential set, replaces only that exact target, maps native errors
to redacted outcomes, and clears managed input, unmanaged write memory, copied output, and the
Credential Manager read buffer before releasing it. Standard tests are fake-boundary-only and do
not touch a user's credential set. Windows support is not claimed until the same bytes are proved
on Windows. No Anthropic credential store, automatic credential read, diagnostic export, automatic
live recomposition, or generated action beyond the bounded known herb-patch examination exists. The opt-in
`OllamaCharacterModelLiveTests` and `OpenAiCharacterModelLiveTests` check explicitly named
already-installed or local models without pulling or modifying them; the standard suite does not
access a local server. Earlier oMLX Responses probes safely rejected non-conforming or incomplete
output and exposed an integration defect: reasoning-capable local models could spend the entire
output budget in reasoning despite a simple bounded game turn, while oMLX's streamed delta text
could differ from its final structured text. The loopback Chat Completions path disables reasoning
through every locally supported control and treats only one bounded `stop` completion as a response.
Local oMLX `LFM2.5-8B-A1B-MLX-8bit` and `Qwen3.6-35B-A3B-8bit`, plus native Ollama
`qwen3.5:4b-q8_0`, each passed the fixed three-request wait and known-target action series and both
ordinary-scene flows. No provider output, reasoning, target metadata, or raw failure was retained.

## Location model boundaries

`Allrpcs.Domain.Locations.LocationIdentity` pairs a stable `LocationId` with an authored display
name. Display names must contain a non-whitespace character, are bounded to 80 UTF-16 code units
as measured by .NET `string.Length`, and are retained exactly rather than trimmed, case-folded,
or otherwise normalized. The immutable value compares by both identifier and display name.

`LocationDescription` is a structurally separate immutable authored value. It retains text
exactly, explicitly permits an empty string when no description is authored, rejects null, and
is bounded to 4,000 UTF-16 code units. It records authoritative authored location state; it is
not proof that any character can perceive, identify, understand, or otherwise access that text.

`Location` is the immutable composition root for the authored location model. It requires and
retains exactly one existing `LocationIdentity` and one existing `LocationDescription`, exposing
only those supplied components through get-only properties. The aggregate adds no cross-field
policy, methods, or events.

The complete location boundary still does not define hierarchy, containment, connections,
coordinates, regions, navigation topology, terrain, access rules, environmental state, mutable
occupancy, lifecycle or event handling, filtered views, visibility, perception, discovery,
persistence, serialization, Godot, or UI behavior.

## World-object model boundaries

`Allrpcs.Domain.WorldObjects.WorldObjectIdentity` pairs a stable `ObjectId` with an authored
display name. Display names must contain a non-whitespace character, are bounded to 80 UTF-16 code
units as measured by .NET `string.Length`, and are retained exactly rather than trimmed,
case-folded, or otherwise normalized. The immutable value compares by both identifier and display
name and remains structurally separate from character and location identities.

`WorldObjectDescription` is a structurally separate immutable authored value. It retains text
exactly, explicitly permits an empty string when no description is authored, rejects null, and is
bounded to 4,000 UTF-16 code units. It records authoritative authored world-object state; it is not
proof that any character can perceive, identify, understand, or otherwise access that text.

`WorldObject` is the immutable composition root for the authored world-object model. It requires
and retains exactly one existing `WorldObjectIdentity` and one existing
`WorldObjectDescription`, exposing only those supplied components through get-only properties.
The aggregate adds no cross-field policy, methods, or events.

`WorldObjectSnapshot` contains existing world objects in stable supplied order. It explicitly
permits an empty sequence, copies caller input exactly once, exposes a read-only list, retains
each supplied world-object instance, and rejects null input, null entries, and value-equal
duplicate `WorldObject.Identity.Id` values. It does not compare display names or descriptions for
identity.

`WorldObjectLookup.FindById` selects only by value-equal `WorldObject.Identity.Id`. It returns the
exact retained world-object instance or `null` without changing either input; display names and
descriptions do not participate in selection.

The complete world-object boundary still does not define item properties, tags, portability,
quantities, stacks, containers, inventory mechanics, ownership, use, location assignment,
containment, projection, filtering, replacement, mutable state, lifecycle or event handling,
visibility, perception, discovery, persistence, serialization, Godot, or UI behavior.

## Item model boundaries

`Allrpcs.Domain.Items.Item` classifies exactly one existing immutable `WorldObject` as an item.
It retains and exposes that supplied instance through one get-only property without duplicating
the world object's identifier, name, or description.

This classification boundary does not define item properties, tags, portability, quantities,
stacks, containers, inventory mechanics, ownership, use, mutation, lifecycle, events,
interactions, location assignment, persistence, serialization, Godot, or UI behavior.

## Environmental-state model boundaries

`Allrpcs.Domain.EnvironmentalStates.EnvironmentalState` is a qualitative authored value with a
canonical key and a nonblank description bounded to 500 UTF-16 code units. Valid descriptions
preserve authored whitespace, casing, line endings, and Unicode exactly. It remains structurally
separate from `CharacterCondition`.

`EnvironmentalStateSet` contains existing environmental states in stable authored order. It
explicitly permits an empty sequence, copies caller input, exposes a read-only collection, and
rejects null input, null entries, and duplicate canonical keys using ordinal equality.

The generic state and set boundaries do not assign environmental states by themselves. The
opening-region environmental boundary separately assigns every exact retained opening location to
one complete, non-overlapping region with exact weather, lighting, and sound states. Its lookup
uses exact location identity, and its representative source provides only qualitative baseline
authorship. One Application transition derives a new woodland light/sound snapshot from the exact
layered-inscription presentation without mutating either input. A separate exact-instance policy
authorizes only the baseline-to-derived installation, while a location-scoped projection retains the
exact current snapshot, location, region, and channel states behind canonical-key greybox text.
One scene flow preflights the exact derived snapshot and location projection before installing the
variation and presenting it after the ordinary layered-inscription discovery.

These boundaries still do not define terrain, hazards, ambient playback, rendered lighting,
transitions over time, mutation, lifecycle, events, simulation, persistence, serialization, or
general production UI behavior.

## Perception-snapshot boundaries

`Allrpcs.Domain.Perception.VisibilityState` defines exactly `unnoticed`, `noticed`,
`identified`, `understood`, `altered`, and `collected`. `All` exposes those singleton values in
that stable roadmap order. `Value` and `ToString()` expose canonical text, and `Parse` accepts
only that exact text and returns the corresponding defined instance.

`WorldObjectVisibility` associates exactly one existing `ObjectId` with one existing
`VisibilityState`. It requires and retains both supplied instances and compares associations by
their values. It adds no default state or cross-field policy.

`CharacterPerceptionSnapshot` associates exactly one existing `CharacterId` with world-object
visibility entries in stable supplied order. It retains the supplied character identifier and
entry instances, explicitly permits an empty sequence, copies caller input, exposes a read-only
collection, and rejects null input, null entries, and duplicate object identifiers. It adds no
implicit `unnoticed` entries, default state, or cross-entry policy.

These boundaries do not define filtered character views, character-aggregate integration,
progression, transitions, discovery, reinterpretation, permissions, actions, events, inventory
ownership, mutation, persistence, serialization, Godot, or UI behavior.

## Inventory boundaries

`Allrpcs.Domain.Inventory.InventoryOwnership` associates exactly one existing `ObjectId` with
exactly one existing owner `CharacterId`. It requires and retains both supplied instances and
compares associations by their values.

`InventoryOwnershipSnapshot` contains existing ownership associations in stable supplied order.
It explicitly permits an empty sequence, copies caller input, exposes a read-only collection,
retains each supplied entry instance, and rejects null input, null entries, and duplicate object
identifiers. It adds no implicit ownership or default owner.

`InventoryCapacity` associates exactly one existing `CharacterId` with a non-negative maximum
count of distinct owned world objects. It retains the supplied character identifier, explicitly
permits zero capacity, and compares capacities by both values. It does not enforce that capacity
against any ownership snapshot or define a default or unlimited policy.

`InventoryCapacitySnapshot` contains existing capacities in stable supplied order. It explicitly
permits an empty sequence, copies caller input, exposes a read-only collection, retains each
supplied entry instance, and rejects null input, null entries, and duplicate character
identifiers. It adds no implicit capacity, lookup behavior, or cross-snapshot policy.

`InventoryCapacityPolicy.CanOwnAdditionalObject` is a pure, fail-closed decision for one new
distinct ownership entry. It finds capacity by value-equal character identifier, counts only
ownership entries for that character, and permits the addition exactly while that count is below
the recorded maximum. It does not accept an object identifier or mutate either snapshot.

`InventoryTakePolicy.CanTakeObject` is a pure eligibility decision for one existing character and
one existing world object. It rejects an object already owned by any character and otherwise
delegates to `InventoryCapacityPolicy.CanOwnAdditionalObject` for the intended owner. It returns
only eligibility and does not mutate either snapshot or execute a transfer.

`InventoryDropPolicy.CanDropObject` is a pure eligibility decision for one existing character and
one existing world object. It permits the drop exactly when the ownership snapshot records the
value-equal object identifier as owned by the value-equal intended character. It returns only
eligibility and does not mutate the ownership snapshot or execute the drop.

`InventoryGivePolicy.CanGiveObject` is a pure eligibility decision for one existing giver, one
distinct existing recipient, and one existing world object. It requires the giver to own the
object and delegates recipient capacity eligibility to
`InventoryCapacityPolicy.CanOwnAdditionalObject`. It returns only eligibility and does not mutate
either snapshot or execute the transfer.

`InventoryUsePolicy.CanUseObject` is a pure eligibility decision for one existing character and
one existing world object. It permits use exactly when the ownership snapshot records the
value-equal object identifier as owned by the value-equal intended character. It returns only
eligibility and does not mutate the ownership snapshot or execute, consume, equip, or alter the
object.

These boundaries do not define character-inventory integration, capacity reservation, quantities,
stacks, slots, weight, containers, equipment, use execution, crafting, consumption, transfer
execution, location, possession history, visibility, discovery, permissions, actions, events,
mutation, persistence, serialization, Godot, or UI behavior.

## Discovery-requirement boundaries

`Allrpcs.Domain.Discovery.DiscoveryCapabilityRequirement` associates exactly one existing target
`ObjectId` with one required canonical character-capability key. It retains the supplied object
identifier instance, validates the capability key with the shared canonical-identifier contract,
preserves that key exactly, and compares requirements by their values.

`Allrpcs.Domain.Discovery.DiscoveryToolRequirement` associates exactly one existing target
`ObjectId` with one existing required-tool `ObjectId`. It retains both supplied identifier
instances and compares requirements by their values. The value deliberately adds no policy that
the target and required tool must be different objects.

`DiscoveryCapabilityRequirementSet` contains existing capability requirements in stable authored
order. It copies caller input, retains the exact supplied requirement instances, permits an empty
sequence, exposes only a read-only collection, and rejects null input, null entries, and
value-equal duplicate target-object identifiers.

`DiscoveryToolRequirementSet` provides the equivalent boundary for existing tool requirements.
It preserves stable authored order, copies caller input, retains the exact supplied requirement
instances, permits an empty sequence, exposes only a read-only collection, and rejects null input,
null entries, and value-equal duplicate target-object identifiers.

`DiscoveryConditionSet` composes one existing `DiscoveryCapabilityRequirementSet` and one existing
`DiscoveryToolRequirementSet`. It retains the exact supplied set instances through get-only
properties, accepts their existing empty-set behavior, and permits the same target-object
identifier to occur once in each set.

`Allrpcs.Domain.Discovery.DiscoveryCapabilityPolicy.IsSatisfied` is a pure, fail-closed evaluation
of one capability requirement against one existing `CharacterCapabilitySet`. It returns true
exactly when a capability key equals the required key by ordinal value, independent of authored
descriptions, order, and unrelated capabilities, and it does not mutate either input.

`Allrpcs.Domain.Discovery.DiscoveryToolPolicy.IsSatisfied` is the equivalent pure, fail-closed
evaluation of one tool requirement against one existing `CharacterInventory`. It returns true
exactly when the inventory contains the required tool-object identifier by value, independent of
the target object, authored order, and unrelated entries, and it does not mutate either input.

`Allrpcs.Domain.Discovery.DiscoveryConditionPolicy.IsSatisfied` is the pure one-target combined
decision. It finds capability and tool requirements by value-equal target-object identifier,
ignores unrelated targets, fails closed when neither kind is authored, delegates a sole authored
requirement to its existing policy, and requires both existing policies to pass when both kinds
are authored for the target. It rejects null inputs and does not mutate them.

`Allrpcs.Domain.Discovery.FocusedSearchRequest` identifies exactly one existing searcher
`CharacterId` and one existing target `ObjectId`. It retains both supplied identifier instances
through get-only properties and compares requests by their identifier values. The request does
not start or evaluate a search.

`Allrpcs.Domain.Discovery.FocusedSearchActivity` denotes that one existing
`FocusedSearchRequest` is active. It retains the exact supplied request instance through a
get-only property and compares activities by request value. The activity does not start,
progress, complete, cancel, schedule, or evaluate a search.

`Allrpcs.Domain.Discovery.FocusedSearchActivitySnapshot` contains existing focused-search
activities in stable supplied order. It permits an empty sequence, copies caller input, retains
the exact supplied activity instances, exposes a read-only list, and rejects nulls plus duplicate
requests by value. It permits one character to have activities for distinct targets because the
snapshot records state rather than enforcing start eligibility.

`Allrpcs.Domain.Discovery.FocusedSearchStartPolicy.CanStartSearch` is the pure start-eligibility
decision. It permits a request exactly when the activity snapshot contains no existing activity
for a value-equal searcher character identifier. Different characters may search the same target.
It rejects null inputs, ignores unrelated activities, and neither starts a search nor mutates its
inputs.

`Allrpcs.Domain.Discovery.FocusedSearchEligibilityPolicy.CanStartSearch` is the pure combined
eligibility decision. It permits a request only when the existing start policy permits it and the
existing discovery-condition policy is satisfied for the request's target object. It rejects null
inputs, fails closed for missing or unsatisfied target conditions and an already-active searcher,
inherits the delegates' unrelated-entry behavior, and does not mutate any input.

`Allrpcs.Domain.Discovery.FocusedSearchStartTransition.StartSearch` is the pure start transition.
It delegates eligibility to the start policy, rejects an ineligible request, and otherwise returns
a new activity snapshot that retains existing activity instances in stable order before one new
activity retaining the exact supplied request instance. It does not mutate either input.

`Allrpcs.Domain.Discovery.FocusedSearchCancellationPolicy.CanCancelSearch` is the pure
cancellation-eligibility decision. It permits cancellation exactly when the activity snapshot
contains an activity whose complete request is value-equal to the supplied request. It rejects
null inputs, ignores unrelated activities, and neither cancels a search nor mutates its inputs.

`Allrpcs.Domain.Discovery.FocusedSearchCancellationTransition.CancelSearch` is the pure
cancellation transition. It delegates eligibility to the cancellation policy, rejects an
ineligible request, and otherwise returns a new activity snapshot that removes the activity for
the complete value-equal request while retaining every other activity instance in stable order.
It does not mutate either input.

`Allrpcs.Domain.Discovery.FocusedSearchCompletionPolicy.CanCompleteSearch` is the pure
completion-eligibility decision. It permits completion exactly when the activity snapshot
contains an activity whose complete request is value-equal to the supplied request. It rejects
null inputs, ignores unrelated activities, and neither completes a search nor mutates its inputs.

`Allrpcs.Domain.Discovery.FocusedSearchCompletionTransition.CompleteSearch` is the pure
completion transition. It delegates eligibility to the completion policy, rejects an ineligible
request, and otherwise returns a new activity snapshot that removes the activity for the complete
value-equal request while retaining every other activity instance in stable order. It does not
mutate either input.

`Allrpcs.Domain.Discovery.FocusedSearchOutcome` records one existing focused-search request and
one existing resulting `VisibilityState`. It retains both supplied instances through get-only
properties and compares outcomes by their values. It does not produce an outcome, apply it to
perception, validate or advance visibility, or connect outcome data to completion.

`Allrpcs.Domain.Discovery.FocusedSearchOutcomeSnapshot` copies one supplied sequence of outcomes
exactly once and exposes the exact outcome instances in stable order through a read-only list.
Empty snapshots are valid; null values and outcomes with value-equal duplicate requests are
rejected. The snapshot does not compare or advance resulting visibility states, produce outcomes,
apply them to perception, or connect them to focused-search completion.

`Allrpcs.Domain.Discovery.FocusedSearchOutcomeProjection.ForCharacter` is the pure
per-character outcome projection. It returns a new snapshot containing every outcome whose
request has a value-equal searcher character identifier, retaining the existing outcome instances
and their request and resulting-visibility-state instances in stable order. It permits no matches
and multiple distinct requests, ignores unrelated outcomes, rejects null inputs, and does not
mutate either input or apply any outcome.

`Allrpcs.Domain.Discovery.FocusedSearchOutcomeLookup.FindByRequest` is the pure exact-request
lookup. It matches both the searcher character and target object by request value, returning the
exact retained outcome or `null` when no outcome matches. It rejects null inputs, relies on the
snapshot's duplicate-request invariant, leaves every input unchanged, and does not produce or
apply an outcome.

`Allrpcs.Domain.Perception.CharacterPerceptionLookup.FindByObject` is the pure exact-object
lookup for one character's perception snapshot. It matches the object identifier by value,
returning the exact retained visibility entry or `null` when no entry matches. It rejects null
inputs, relies on the snapshot's duplicate-object invariant, leaves every input unchanged, and
does not inspect the complete character, compare or advance visibility, or apply an outcome.

`Allrpcs.Domain.Discovery.FocusedSearchOutcomeApplicabilityPolicy.CanApply` is the pure
perception-applicability decision for one focused-search outcome. It returns true only when the
perception snapshot represents the outcome request's searcher and the existing perception lookup
finds that request's target object. It rejects null inputs, ignores current and resulting
visibility states, leaves every input unchanged, and does not decide a visibility transition or
apply an outcome.

`Allrpcs.Domain.Discovery.FocusedSearchOutcomeChangePolicy.WouldChange` is the pure
visibility-state change decision for one focused-search outcome and perception snapshot. It first
delegates applicability to the existing policy, then delegates exact target selection to the
existing perception lookup and returns true only when the retained target entry's current state
differs from the outcome's resulting state. It rejects null inputs, treats every distinct state
pair alike without ranking it, leaves every input unchanged, and does not apply an outcome.

`Allrpcs.Domain.Discovery.FocusedSearchOutcomeApplicationTransition.Apply` is the pure
perception transition for one existing focused-search outcome. It delegates the complete decision
to the change policy, rejects inapplicable and unchanged outcomes, and otherwise returns a new
snapshot that retains the exact character identifier, stable entry order, and every unrelated
entry instance. Only the target entry is replaced, retaining its exact object identifier and the
outcome's exact resulting visibility-state instance. It leaves both inputs unchanged and does not
rank states, produce or consume outcomes, or connect application to focused-search completion.

`Allrpcs.Domain.Discovery.FocusedSearchOutcomeConsumptionPolicy.CanConsume` is the pure
consumption-eligibility decision for one existing focused-search outcome. It delegates exact
request selection to the existing outcome lookup and returns true only when the retained complete
outcome is value-equal to the supplied outcome, including its resulting visibility state. It
rejects null inputs, leaves every input and retained instance unchanged, and does not remove or
otherwise consume an outcome, change perception, or connect consumption to completion.

`Allrpcs.Domain.Discovery.FocusedSearchOutcomeConsumptionTransition.Consume` is the pure
consumption transition for one eligible focused-search outcome. It delegates the complete
decision to the consumption policy, rejects outcomes that are not exactly retained, and otherwise
returns a new snapshot without the outcome for that exact request. It preserves stable order and
every unrelated outcome instance, leaves both inputs unchanged, and does not change perception or
connect consumption to focused-search completion.

`Allrpcs.Domain.Discovery.FocusedSearchOutcomeResolutionPolicy.CanResolve` is the pure combined
resolution-eligibility decision for one existing focused-search outcome. It delegates perception
applicability and visibility-state change detection completely to the existing change policy, then
delegates exact recorded-outcome eligibility completely to the existing consumption policy. It
returns true only when both decisions succeed, rejects null inputs through those delegated
policies, leaves every input and retained instance unchanged, and does not apply a visibility
change, remove an outcome, or connect resolution to focused-search completion.

`Allrpcs.Domain.Discovery.WorldObjectBoundFocusedSearchOutcomeResolutionPolicy.CanResolve` is the
pure world-object-bound resolution-eligibility decision for one existing focused-search outcome.
It rejects null inputs, requires the supplied world object's stable identifier to be value-equal to
the outcome request's target, then delegates the unchanged complete perception-and-outcome decision
to the existing resolution policy. It leaves the outcome, world object, perception, outcome
snapshot, and every retained nested instance unchanged; it does not inspect authored object text,
apply or consume an outcome, mutate a world object, or make any source or controller authoritative.

`Allrpcs.Domain.Discovery.FocusedSearchOutcomeResolutionResult` is one immutable resolution-result
value retaining exact existing character-perception and focused-search-outcome snapshot instances
through get-only properties. Its record equality therefore uses those retained snapshot instances.
It rejects null inputs and does not perform resolution, apply a visibility change, remove an
outcome, validate either snapshot, or connect resolution to focused-search completion.

`Allrpcs.Domain.Discovery.FocusedSearchOutcomeResolutionTransition.Resolve` is the pure combined
resolution transition for one eligible focused-search outcome. It delegates complete eligibility
to the existing resolution policy, rejects ineligible resolution, then delegates perception
replacement and outcome removal to the existing application and consumption transitions. It
returns one resolution result retaining those exact returned snapshots, leaves every input and
retained unrelated instance unchanged, and does not produce outcomes, grant discovery, or connect
resolution to focused-search completion.

`Allrpcs.Domain.Discovery.WorldObjectBoundFocusedSearchOutcomeResolutionTransition.Resolve` is the
pure world-object-bound resolution transition for one eligible focused-search outcome. It delegates
complete eligibility to the world-object-bound resolution policy, rejects an ineligible resolution,
then delegates unchanged state creation to the existing resolution transition and returns its exact
result. It accepts value-equal target identifiers, leaves the supplied world object and every other
input unchanged, and does not inspect authored object text, mutate a world object, make any source
or controller authoritative, or grant discovery.

`Allrpcs.Domain.Discovery.FocusedSearchOutcomeRecordingPolicy.CanRecord` is the pure
recording-eligibility decision for one existing focused-search outcome. It delegates exact-request
selection to the existing outcome lookup and returns true only when no outcome for that request is
already retained. It allows both partial request matches, rejects exact-instance and value-equal
duplicate requests, rejects null inputs through the delegated lookup, leaves every input and
retained instance unchanged, and does not record or produce an outcome or connect recording to
focused-search completion.

`Allrpcs.Domain.Discovery.FocusedSearchOutcomeRecordingTransition.Record` is the pure recording
transition for one eligible focused-search outcome. It delegates complete eligibility to the
existing recording policy, rejects outcomes whose request is already retained, and otherwise
returns a new snapshot preserving every retained outcome instance in stable order before the exact
supplied outcome instance. It leaves both inputs unchanged and does not produce an outcome, connect
recording to focused-search completion, or apply a visibility change.

`Allrpcs.Domain.Discovery.FocusedSearchOutcomeCompletionPolicy.CanCompleteWithOutcome` is the pure
combined completion-eligibility decision for one existing focused-search outcome. It delegates
active-request eligibility completely to the existing completion policy and recording eligibility
completely to the existing recording policy, returning true only when the request is active and no
outcome for that request is already retained. It rejects null inputs before delegation, leaves every
input and retained instance unchanged, and does not complete an activity, record or produce an
outcome, change perception, or connect the decision to Godot.

`Allrpcs.Domain.Discovery.FocusedSearchOutcomeCompletionResult` is one immutable
completion-with-outcome result retaining exact existing focused-search activity and outcome
snapshot instances through get-only properties. Its record equality therefore uses those retained
snapshot instances. It rejects null inputs and does not complete an activity, record or produce an
outcome, inspect or validate either snapshot, change perception, or connect completion to Godot.

`Allrpcs.Domain.Discovery.FocusedSearchOutcomeCompletionTransition.CompleteWithOutcome` is the
pure combined completion transition for one eligible existing focused-search outcome. It delegates
complete eligibility to the existing combined policy, rejects inactive or already-recorded
requests, then delegates exact-request activity removal and exact-outcome append to the existing
completion and recording transitions. It returns one completion result retaining those exact
returned snapshots, preserves stable order and every unrelated retained instance, leaves every
input unchanged, and does not produce or resolve outcomes, change perception, or connect
completion to Godot.

`Allrpcs.Domain.Discovery.FocusedSearchActivityProjection.ForCharacter` is the pure
per-character activity projection. It returns a new snapshot containing every activity whose
request has a value-equal searcher character identifier, retaining the existing activity instances
in stable order. It permits no matches and multiple distinct requests, ignores unrelated
activities, rejects null inputs, and does not mutate either input.

`Allrpcs.Domain.Discovery.ObservationReinterpretationCandidate` identifies exactly one earlier
observation by an existing observer `CharacterId` and target `ObjectId`, together with the existing
`KnowledgeId` that could trigger its reinterpretation. It rejects null inputs, retains all three
supplied identifier instances through get-only properties, and uses only their existing value
semantics. It does not inspect character knowledge or perception, decide whether the trigger is
satisfied, define a resulting visibility state, reinterpret an observation, or change state.

`Allrpcs.Domain.Discovery.ObservationReinterpretationCandidateSnapshot` contains existing
reinterpretation candidates in stable supplied order. It explicitly permits an empty sequence,
copies caller input, exposes a read-only list, retains each exact candidate instance, and rejects
null input, null entries, and value-equal complete duplicates. Candidates may share any proper
subset of observer, target, and trigger identifiers. The snapshot does not project, look up,
select, rank, order, evaluate, apply, or consume candidates.

`Allrpcs.Domain.Discovery.ObservationReinterpretationCandidateProjection.ForCharacter` is the pure
per-character candidate projection. It returns a new snapshot containing every candidate whose
observer identifier is value-equal to the supplied character identifier, retaining exact candidate
and nested identifier instances in stable source order. It permits no matches and multiple matches
with any target or trigger knowledge, omits unrelated observers, rejects null inputs, and does not
mutate, evaluate, rank, reorder, or select among candidates.

`Allrpcs.Domain.Discovery.ObservationReinterpretationCandidateLookup.Find` is the pure exact
candidate lookup. It matches the complete observer, target, and trigger-knowledge value and returns
the exact retained candidate or `null`. It rejects partial matches and null inputs, leaves every
input unchanged, and does not project, produce, rank, reorder, evaluate, apply, or consume
candidates.

`Allrpcs.Domain.Discovery.ObservationReinterpretationCandidateTriggerPolicy.CanTrigger` is the
pure knowledge-trigger decision. It returns true only when the complete character has a
value-equal observer identity and references the candidate's value-equal trigger knowledge. It
rejects null inputs and leaves the complete character, its ordered knowledge, and the candidate
unchanged without inspecting perception or the target object.

`Allrpcs.Domain.Discovery.ObservationReinterpretationCandidateApplicabilityPolicy.CanApply` adds
only the existing-perception boundary. It delegates trigger satisfaction, requires a perception
snapshot for the value-equal observer, and uses exact object lookup to require the target without
inspecting its current visibility state. It rejects null inputs and changes nothing.

`Allrpcs.Domain.Discovery.ObservationReinterpretationCandidateChangePolicy.WouldChange` delegates
complete applicability and returns true only when the retained target is exactly `Noticed`.
Every missing, mismatched, untriggered, `Unnoticed`, `Identified`, `Understood`, or
`AlteredOrCollected` combination is false without mutation.

`Allrpcs.Domain.Discovery.ObservationReinterpretationCandidateApplicationTransition.Apply`
rejects every non-changing combination, then returns a new perception snapshot with only the
value-equal target changed from `Noticed` to `Identified`. It preserves the perception owner's
exact identifier, stable entry order, every unrelated entry instance, the target's exact retained
object identifier, the complete character, and all inputs.

`ObservationReinterpretationCandidateConsumptionPolicy.CanConsume` and
`ObservationReinterpretationCandidateConsumptionTransition.Consume` provide complete-candidate
retention checking and pure removal. They accept a distinct value-equal candidate; eligibility is
false and removal is rejected for empty, absent, or partial-match snapshots, while null inputs are
rejected. Both preserve stable order, every unrelated exact candidate, every nested identifier,
and all inputs.

`ObservationReinterpretationCandidateResolutionPolicy.CanResolve`,
`ObservationReinterpretationCandidateResolutionResult`, and
`ObservationReinterpretationCandidateResolutionTransition.Resolve` combine trigger-aware
`Noticed`-to-`Identified` application with complete candidate consumption. Resolution succeeds only
when both delegated operations are eligible, returns only the two new immutable snapshots, and
leaves the character and every input snapshot unchanged.

`ObservationReinterpretationCandidateRecordingPolicy.CanRecord` and
`ObservationReinterpretationCandidateRecordingTransition.Record` provide duplicate-free pure
recording. `ObservationReinterpretationCandidateProductionPolicy.CanProduce` and
`ObservationReinterpretationCandidateProductionTransition.Produce` additionally require the
candidate's observer and target to match an existing authoritative character and world object.
They deliberately do not require that the trigger knowledge has already been gained; later
resolution performs that check. Eligible production appends the exact candidate in stable order.

`ObservationReinterpretationCandidateResolutionSelection.SelectNext` and
`ObservationReinterpretationCandidateResolutionSequence.ResolveNext` provide deterministic
per-character resolution. Selection returns the exact first retained candidate that the supplied
complete character can resolve against its perception, skipping other observers, unmet triggers,
missing targets, and non-`Noticed` targets. Sequencing delegates that selection and the atomic
transition, changing and consuming only the selected candidate.

`Allrpcs.Domain.WorldEvents.WorldEventLogSnapshot` is the immutable ordered authoritative
world-event identifier log. It explicitly permits an empty sequence, copies caller input, exposes
only a read-only list, retains every exact supplied `WorldEventId`, and rejects null input, null
entries, and value-equal duplicate identifiers.

`WorldEventLogLookup.Find`, `WorldEventLogAppendPolicy.CanAppend`, and
`WorldEventLogAppendTransition.Append` provide exact retained-identifier lookup and duplicate-free
pure append. Lookup returns the exact retained value-equal identifier or `null`; eligible append
returns a new snapshot preserving stable order and every retained instance before the exact supplied
identifier. Duplicate and null rejection leave all inputs unchanged. These contracts do not define
event payloads, time, replay, persistence, or production.

`WorldEventRecordingPolicy.CanRecord` and `WorldEventRecordingTransition.Record` add the
authoritative-definition boundary to identifier-log append. A requested identifier is recordable
only when it resolves to a defined event and is absent from the log; recording appends the exact
identifier retained by that authoritative event, even for a distinct value-equal request.

`WorldEventTimeEffect` is the closed world-event classification vocabulary: exact singleton values
`no-advance` and `advance` in stable order with strict canonical parsing. Immutable `WorldEvent`
values retain one exact existing identifier and time effect. `WorldEventSnapshot` copies ordered
events into a read-only collection and rejects nulls and duplicate event identifiers even when
their time effects differ; `WorldEventLookup.FindById` returns the exact retained value-equal event.

`WorldEventLogProjection.Resolve` deterministically maps the identifier log to exact authoritative
events in log order, ignores unrecorded definitions, and fails closed when an identifier has no
definition. `WorldTimeSnapshot` represents qualitative time only as the exact latest event that
advanced it, with null as the explicit initial state. `WorldTimeAdvancementPolicy.CanAdvance` and
`WorldTimeAdvancementTransition.Advance` reject `no-advance` events and the current value-equal
event. `WorldTimeProjection.FromEvents` reconstructs the same state from ordered events by selecting
the exact latest `advance` identifier. These contracts define no elapsed time, phase names,
durations, scheduling, disease progression, persistence, or serialization.

`WorldEventOccurrencePolicy.CanOccur`, `WorldEventOccurrenceResult`, and
`WorldEventOccurrenceTransition.Occur` combine authoritative recording with qualitative time as one
immutable Domain operation. Every eligible event is recorded; `no-advance` preserves the exact
supplied time snapshot, while `advance` installs a new snapshot retaining the authoritative event's
exact identifier. Undefined, already-recorded, or repeated-current advancing events fail closed.

`Allrpcs.Game.WorldEventLogController` and `WorldTimeController` remain focused fixtures for their
individual Domain boundaries. The scene's `WorldEventController` is the ordinary runtime authority:
it initializes once from exact authoritative-event, identifier-log, and qualitative-time snapshots,
then delegates occurrence eligibility and transition to the Domain contracts. A successful
occurrence installs the returned log and time snapshots together. Uninitialized, undefined,
duplicate, repeated-current, and null cases preserve every controller snapshot or reject null.

`Allrpcs.Game.TemporaryWorldEventSource` independently authors one retained ordered snapshot. Its
exact `greybox-ready`/`no-advance` event is the explicit startup selection; its exact
`herb-patch-identified`/`advance` event is the first meaningful-time definition. `Main._Ready`
initializes the world-event controller and records only the startup event, preserving the exact
initial time snapshot.

Ordinary herb-patch focused-search resolution preflights both the existing world-object-bound
resolution policy and world-event occurrence policy. Success consumes the exact outcome, changes
only the target perception, records the exact authored event identifier, and advances qualitative
time. If either side is ineligible, both controller states remain unchanged. This temporary
cross-controller boundary does not add generic event payloads, clocks, disease progression,
persistence, serialization, or new presentation.

`Allrpcs.Domain.Discovery.PassiveNoticingCandidate` identifies exactly one existing observer
`CharacterId` and one existing target `ObjectId` without selecting or evaluating the candidate. It
rejects null inputs, retains both supplied identifier instances through get-only properties, and
uses only their existing value semantics. It does not inspect world or character state, grant
discovery, or change perception.

`Allrpcs.Domain.Discovery.PassiveNoticingCandidateSnapshot` contains existing passive-noticing
candidates in stable supplied order. It explicitly permits an empty sequence, copies caller input,
exposes a read-only list, retains each supplied candidate instance, and rejects null input, null
entries, and value-equal duplicate candidates. The same observer may have distinct targets and the
same target may have distinct observers. The snapshot does not produce, select, filter, rank,
reorder, or evaluate candidates.

`Allrpcs.Domain.Discovery.PassiveNoticingCandidateProjection.ForCharacter` is the pure
per-character candidate projection. It returns a new snapshot containing every candidate whose
observer identifier is value-equal to the supplied character identifier, retaining the exact
candidate and nested identifier instances in stable source order. It permits no matches and
multiple distinct matches, omits unrelated candidates, rejects null inputs, leaves both inputs
unchanged, and does not produce, select, rank, reorder, or evaluate candidates.

`Allrpcs.Domain.Discovery.PassiveNoticingCandidateLookup.Find` is the pure exact-candidate lookup.
It matches the complete candidate value, including both observer and target identifiers, and
returns the exact retained candidate or `null` when no candidate matches. It rejects either null
input, leaves every input unchanged, and does not produce, project, select among, rank, reorder, or
evaluate candidates.

`Allrpcs.Domain.Discovery.PassiveNoticingCandidateApplicabilityPolicy.CanApply` is the pure
candidate-to-perception applicability decision. It returns true only when the perception snapshot
belongs to the candidate's value-equal observer and the existing exact-object lookup finds the
candidate target. It rejects null inputs, retains every input instance unchanged, and deliberately
does not inspect the current visibility state, decide a change, or mutate perception.

`Allrpcs.Domain.Discovery.PassiveNoticingCandidateChangePolicy.WouldChange` is the pure
passive-noticing change decision. It delegates complete applicability, selects the exact retained
target through the existing perception lookup, and returns true only when that target is exactly
`Unnoticed`. It rejects null inputs and leaves every input unchanged; later visibility states are
never regressed or redundantly rewritten, and the policy does not itself change perception.

`Allrpcs.Domain.Discovery.PassiveNoticingCandidateApplicationTransition.Apply` is the pure
passive-noticing perception transition. It delegates complete change eligibility and returns a new
snapshot whose candidate target alone is replaced at its stable position with `Noticed`, retaining
the perception-owned character and target-object identifier instances and every unrelated entry.
It rejects inapplicable or non-changing candidates, leaves all inputs unchanged, and does not
produce, retain, consume, or select a candidate.

`Allrpcs.Domain.Discovery.PassiveNoticingCandidateConsumptionPolicy.CanConsume` is the pure
candidate-retention decision. It delegates complete value matching to the existing exact-candidate
lookup and returns true only when a value-equal candidate is retained in the supplied snapshot. It
rejects null inputs, preserves every exact instance and stable order, and does not remove, apply,
produce, or select among candidates.

`Allrpcs.Domain.Discovery.PassiveNoticingCandidateConsumptionTransition.Consume` is the pure
candidate-removal transition. It delegates complete consumption eligibility and returns a new
snapshot without the value-equal complete retained candidate, preserving every unrelated exact
candidate and nested identifier instance in stable order. It accepts a distinct value-equal
candidate, rejects empty, absent, partial-match, and null inputs, and leaves every input unchanged.

`Allrpcs.Domain.Discovery.PassiveNoticingCandidateResolutionPolicy.CanResolve` is the pure combined
resolution decision. It returns true only when the existing change policy permits an
`Unnoticed`-to-`Noticed` perception change and the complete candidate is retained for consumption.
It leaves both snapshots and every nested instance unchanged and does not create resolution state.

`Allrpcs.Domain.Discovery.PassiveNoticingCandidateResolutionResult` is the immutable result shape
for one passive-noticing resolution. It retains the exact supplied perception and candidate
snapshot instances behind get-only properties, rejects null snapshots, and performs no resolution.

`Allrpcs.Domain.Discovery.PassiveNoticingCandidateResolutionTransition.Resolve` is the pure atomic
resolution transition. It delegates complete combined eligibility, then delegates the existing
application and consumption transitions, returning only the resulting perception and candidate
snapshots. The target alone changes from `Unnoticed` to `Noticed`, the complete candidate alone is
removed, and every unrelated exact instance and stable position is retained without mutating input.

`Allrpcs.Domain.Discovery.PassiveNoticingCandidateRecordingPolicy.CanRecord` and
`PassiveNoticingCandidateRecordingTransition.Record` provide duplicate-free pure candidate
recording. Eligibility rejects only an existing value-equal complete candidate while allowing both
partial matches; recording appends the exact supplied eligible candidate after all existing entries
in stable order. Both preserve all inputs and do not prove that either identifier names an existing
authoritative character or world object.

`Allrpcs.Domain.Discovery.PassiveNoticingCandidateProductionPolicy.CanProduce` and
`PassiveNoticingCandidateProductionTransition.Produce` bind recording to an existing authoritative
character and world object. Eligibility requires both authoritative identities to be value-equal to
the supplied candidate and delegates duplicate rejection to the recording policy. Production then
delegates exact append behavior to the recording transition, returning a new snapshot that retains
the exact supplied candidate. Both accept distinct value-equal identifier instances, reject every
null, mismatch, and complete duplicate, and leave all character, object, candidate, and snapshot
inputs unchanged.

`Allrpcs.Domain.Discovery.PassiveNoticingCandidateResolutionSelection.SelectNext` and
`PassiveNoticingCandidateResolutionSequence.ResolveNext` provide deterministic per-observer
resolution. Selection walks the retained snapshot in stable order and returns the exact first
candidate for a value-equal observer that delegates successfully to complete resolution
eligibility, skipping other observers, absent targets, and targets no longer at `Unnoticed`.
Resolution delegates that exact selection to the existing atomic transition, changing only its
target to `Noticed` and removing only the selected complete candidate in the returned snapshots.
When no candidate is eligible, selection returns `null` and resolution rejects without changing
inputs; neither operation ranks candidates or inspects coordinates, distance, line of sight, or
world state.

`Allrpcs.Application.Discovery.FocusedSearchActivityIndicatorPolicy.ShouldShow` is the pure
per-character activity-indicator decision. It delegates selection to the existing activity
projection and returns true exactly when the new projected snapshot contains at least one entry,
including multiple distinct matching requests. It returns false for empty or unrelated snapshots,
rejects null inputs, and does not mutate either input.

`Allrpcs.Game.FocusedSearchActivityIndicator.Present` is the temporary Godot presentation adapter
under the existing Seeker. It accepts an existing character identifier and activity snapshot,
delegates the complete decision to the Application policy, and changes only the visibility of one
authored cyan in-world ring. The ring is hidden by default, and ordinary play does not construct or
inject activity outside the explicit temporary lifecycle inputs described below.

`Allrpcs.Game.FocusedSearchActivityController` is the sole live owner of the existing Seeker's
immutable focused-search activity, outcome, passive-noticing candidate,
observation-reinterpretation candidate, and optional perception snapshots. It initializes all
candidate and lifecycle snapshots empty from the Seeker's one authored stable character identifier.
Its separate one-time perception initialization accepts only an existing snapshot for that
value-equal Seeker identifier and retains the exact supplied snapshot instance. Null,
wrong-character, and repeated initialization are rejected without changing any owned snapshot or
presentation. Its start path accepts existing discovery conditions, capabilities, and inventory,
delegates the complete start decision to the combined Domain eligibility policy, and only then
delegates state creation to the existing start transition. Cancellation and request-only
completion retain their existing policy and transition delegation. Its separate outcome-aware
completion path accepts an existing outcome, delegates the combined completion decision and
transition, installs both exact result snapshots, and only then presents the resulting activity
snapshot. Rejected lifecycle operations preserve every owned snapshot and presentation. Ordinary play
reaches the outcome-aware path only through the explicit temporary completion input; the
credential-free focused-search, outcome-controller, and interaction-focus smokes cover these
boundaries. Its separate outcome-resolution path accepts an existing outcome and world object,
rejects null inputs and target-mismatched objects before applying or consuming anything, delegates
complete eligibility to the world-object-bound policy and state creation to the world-object-bound
transition, then installs the exact returned perception and outcome snapshots without changing
activity, the supplied world object, or presentation. Ordinary play reaches that boundary only
through the explicit temporary resolution input described below.

Its passive-noticing production boundary accepts an existing candidate, complete character, and
world object, requires the candidate's observer to identify this controller's Seeker, and delegates
the remaining authoritative identity and duplicate checks plus exact append behavior to the
existing Domain production policy and transition. Its passive-noticing resolve-next boundary fails
closed while perception is uninitialized or no retained candidate is eligible, otherwise delegates
stable selection and atomic state construction to the existing Domain selection and sequence and
installs both returned snapshots. These paths retain value-equal candidate identifiers, keep the
controller as the only live perception owner, and do not change focused-search or presentation
state. During `Main._Ready`, ordinary play resolves the source candidate's exact target through
the initialized world-object controller, produces it from the exact retained candidate, character,
and world object, then invokes resolve-next once. Either impossible step fails explicitly; success
consumes the candidate and replaces only its `Unnoticed` live perception entry with `Noticed`.
This one-time sequence does not poll, inspect coordinates, distance, line of sight, focus, or input,
and it does not add passive-noticing presentation.

Its observation-reinterpretation production boundary likewise requires the candidate observer to
identify this controller's Seeker, then delegates authoritative character/world-object matching and
duplicate-free exact append behavior to the Domain production boundary. Resolve-next requires a
matching complete Seeker character and initialized perception, then delegates stable selection and
atomic `Noticed`-to-`Identified` application plus complete-candidate consumption. Rejections preserve
every owned snapshot and presentation. During `Main._Ready`, the scene produces the exact temporary
candidate through live character and world-object authority, resolves it once against the
character's exact trigger knowledge, and refreshes the existing resolved-perception indicators.
There is no polling, proximity, timing, focus, input, knowledge grant, or new UI authority.

Each existing `Allrpcs.Game.GreyboxInteractionTarget` parses its one exported canonical object-ID
text through the Domain `ObjectId` boundary when the node becomes ready and retains that exact
identifier instance. On each refresh, the existing `InteractionFocusController` resolves every
marker identifier through `WorldObjectController` and makes only matching existing objects eligible
for the otherwise unchanged pure visible, enabled, in-range selection. A missing nearest object
therefore falls back to the next eligible marker under the existing distance and authored-order
rules; no eligible object clears the focus indicator, status, index, and identifier. A selection
exposes the exact looked-up object's identifier instance, including a distinct value-equal instance,
and presents the focus status from that lookup's authored `WorldObject.Identity.DisplayName` rather
than the greybox marker label. The status refreshes when a value-equal replacement changes that name
without changing the selected authored index, returns to `Interaction focus: NONE` when focus
clears, and never inspects or exposes the object description. Each existing labelled marker is
likewise refreshed from its exact matching object's display name, including for a matching disabled
marker, while a missing object clears that marker label and an unlabelled hidden marker remains
unlabelled. Markers no longer author a separate display-name input: their `Label` nodes are
presentation-only and receive text through this lookup. The refresh does not alter focus eligibility
or retain a world object beyond the refresh. Its request-construction boundary
accepts an existing `CharacterId` and nullable `WorldObject`, returns no request when focus is clear
or the object is absent or target-mismatched, and otherwise returns a new `FocusedSearchRequest`
retaining the exact character identifier and the matching object's exact identifier. Exact and
distinct value-equal object identifiers are accepted. Its separate start-attempt boundary supplies
the activity controller's exact Seeker identifier to that factory and, for a valid request only,
forwards the request plus existing
`DiscoveryConditionSet`, `CharacterCapabilitySet`, and `CharacterInventory` instances to the
controller's condition-aware start path. It returns the existing start result and adds no Domain
eligibility, transition, input, discovery-context authoring, world-object filtering, or ranking
behavior.

`Allrpcs.Game.TemporaryFocusedSearchContextSource` is the narrow scene source for those three
aggregate start inputs. It derives its target identifiers only from the existing temporary
world-object snapshot: the first object's exact identifier authors the capability-only requirement,
the second object's exact identifier authors the tool-only requirement, and the third water-cask
identifier appends an `inspect-water-sources` capability requirement plus matching capability. It
fails explicitly when fewer than three objects are supplied and never falls back to greybox
interaction targets. The source retains the exact created capabilities, tool identifier,
requirements, sets, and aggregates and
exposes only the existing discovery-condition set, capability set, and inventory needed by the start
bridge. Independent-snapshot smoke evidence proves exact nested retention without selecting by
authored identity text or changing the supplied snapshot and objects. The source does not start a
search, poll focus, own lifecycle state, grant discovery, or make temporary scene state
authoritative.

`Allrpcs.Game.TemporaryWorldObjectSource` independently authors the herb patch, stone marker, and
village water cask, then appends the exact representative scenario's wagon clue into one retained
`WorldObjectSnapshot` in that stable order when the scene becomes ready. It retains one distinct
world object per identity, exact identifier values, display names, and non-empty descriptions. The
source identifiers remain distinct instances that are value-equal to their greybox targets'
independently parsed `ObjectIdText` references. Their descriptions are not focused-search inputs or
copied into focus presentation; the first two existing resolved-perception indicators receive one
only when their matching perception entry is `Identified`. The exact water-cask instance continues
to supply the temporary sabotage seam, while the exact shared wagon instance supplies its
field-lens-gated discovery seam. The source does not mutate or replace targets, become authoritative,
filter or rank objects, call controllers, grant discovery, or select ordinary-play behavior.

`Allrpcs.Game.WorldObjectController` is the separate scene owner of one optional immutable
world-object snapshot. Its one-time initialization boundary rejects null, retains the exact first
supplied snapshot instance, and fails closed on repeated initialization without replacing that
snapshot. Its lookup boundary rejects a null identifier, returns no object while uninitialized or
when the identifier is absent, and otherwise delegates to the pure Domain lookup to return the
exact retained object for a value-equal identifier. During `Main._Ready`, the scene passes the
temporary source's exact retained snapshot once and fails explicitly if initialization cannot be
established. The temporary F, E, and R paths call lookup only for the focused identifier, active
request target, and selected recorded-outcome target respectively. The controller does not filter,
rank, or mutate world objects; inspect focused-search or perception state; grant discovery; or
change presentation itself. The focus controller's explicit-position refresh seam exercises the
same lookup and presentation without moving the Seeker; ordinary per-frame refresh still supplies
the Seeker's current position.

`Allrpcs.Game.TemporaryFocusedSearchOutcomeSource` is a separate narrow scene source for one
explicitly temporary deterministic focused-search result. Each call accepts an existing request
plus an existing nullable world object. It rejects a null request and returns no outcome for a null
or target-mismatched object. A matching object, including one with a distinct value-equal stable
identifier, produces a distinct `FocusedSearchOutcome` retaining the exact request and the exact
existing `VisibilityState.Identified` singleton. The source leaves the world object, its identity,
and its description unchanged and does not inspect authored object text, focus, activity, outcomes,
perception, or broader world state; decide lifecycle eligibility; call a controller; own state;
apply or consume an outcome; or change visibility. Ordinary play invokes it only from the pressed
temporary E completion path when an active Seeker request exists and its target resolves through
the world-object controller.

`Allrpcs.Game.TemporarySeekerPerceptionSource` authors one temporary immutable perception snapshot
when the scene becomes ready. It receives the exact retained temporary passive-noticing candidate and
derives its ordered visibility entries only from the existing temporary world-object snapshot. It
resolves the candidate target through value-equal world-object lookup and fails explicitly when that
target is absent. Every entry retains its world object's exact stable identifier in source order; the
resolved candidate target alone begins at the exact `VisibilityState.Unnoticed` singleton and every
other object begins at `Noticed`. The snapshot retains the candidate's exact observer identifier, and
repeated scene access returns the same snapshot instance. Derivation leaves the candidate, snapshot,
objects, identities, and descriptions unchanged. The source does not read greybox targets, authored
identity text, focus, or lifecycle state; call a controller; or make either source authoritative.
During `Main._Ready`, the scene passes this exact snapshot to the controller's one-time initialization
boundary before the one-time passive-noticing sequence replaces only the controller-owned snapshot.

`Allrpcs.Game.TemporarySeekerCharacterSource` authors and retains one complete immutable temporary
Seeker `Character` after the existing Seeker controller is ready. Its `CharacterIdentity` and human
`CharacterControllerAssignment` both retain the controller's exact parsed `CharacterId` instance.
The remaining required aggregate components are explicitly temporary empty authored backgrounds,
capabilities, limitations, conditions, inventory, beliefs, relationships, and goals. Its knowledge
contains exactly one authored `stone-marker-meaning` reference used only by the bounded
reinterpretation path. The source exposes only its one retained character and does not grant
knowledge or change focused-search, perception, movement, input, or presentation state.

`Allrpcs.Game.TemporaryPassiveNoticingCandidateSource` separately authors and retains one
identifier-only candidate after the character and world-object sources are ready. It uses the
temporary character's exact identity identifier and the first authoritative world object's exact
identifier without reading authored text, greybox coordinates, distance, line of sight, focus,
input, or perception. It does not call the controller, produce or resolve a candidate, change
visibility, or alter any presentation. During `Main._Ready`, the separate scene coordinator supplies
that exact candidate to the controller's one-time ordinary production and resolution sequence; the
source retains its unchanged candidate after the controller consumes the live snapshot entry.

`Allrpcs.Game.TemporaryObservationReinterpretationCandidateSource` separately retains one candidate
using the temporary character's exact identifier, the second authoritative world object's exact
identifier, and the character's exact first knowledge reference. It does not parse copied identity
text, inspect perception, or call the controller. During `Main._Ready`, the coordinator resolves the
target through the live world-object controller, produces the exact candidate from the exact source
character and object, then resolves it once. The atomic result changes only the stone marker from
`Noticed` to `Identified`, consumes the live candidate, leaves every source unchanged, and exposes
the already-authored stone-marker description through the existing indicator.

The temporary `focused_search_start` action maps physical F to one `Main._UnhandledInput` branch.
On that pressed action only, `Main` resolves the current focused identifier through
`WorldObjectController` and passes only the exact returned object plus the context source's exact
retained condition, capability, and inventory aggregates to the interaction-focus start boundary.
A matching herb-patch or stone-marker object may therefore start the existing immutable activity
and show its indicator. Clear, hidden, disabled, out-of-range, missing-object, or already-active
attempts preserve the existing focused-search snapshots and surrounding world-object, target,
perception, context, and presentation state; the missing-object path reports `UNCHANGED` without
invoking the start boundary.

The separate temporary `focused_search_cancel` action maps physical C to one narrow
`Main._UnhandledInput` branch. On that pressed action only, `Main` finds the Seeker's active
activity in the controller's current immutable snapshot and passes that activity's exact retained
request to `FocusedSearchActivityController.TryCancelSearch`. Cancellation does not read or depend
on the current interaction focus. A successful cancellation reaches the existing policy and
transition, replaces the snapshot with its empty successor, and hides the activity indicator; no
active search and repeated cancellation preserve the existing snapshot instance and presentation.

The separate temporary `focused_search_complete` action maps physical E to the equivalent narrow
`Main._UnhandledInput` path. It selects the same exact retained active request, resolves only that
request's target through `WorldObjectController`, and passes the exact returned object to the
temporary outcome source. Only a created `Identified` outcome is forwarded to
`FocusedSearchActivityController.TryCompleteSearchWithOutcome`, without reading or depending on
interaction focus. A successful completion replaces the activity snapshot with its empty
successor, appends the exact outcome to a new outcome snapshot, and hides the indicator. No active
search, an absent target object, and repeated completion preserve the focused-search snapshots,
world-object source and controller state, targets, perception, and presentation; the attempted E
path reports `UNCHANGED`.

The separate temporary `focused_search_resolve` action maps physical R to one narrow
`Main._UnhandledInput` path. It selects the first retained outcome in stable recorded order whose
request belongs to the controller's value-equal Seeker identifier, asks `WorldObjectController` for
that outcome's target identifier, and passes both the exact outcome and exact looked-up world-object
instance to `FocusedSearchActivityController.TryResolveOutcome` only when the object exists. The
controller delegates complete bound eligibility to the pure Domain policy before applying or
consuming anything. That decision accepts distinct value-equal target identifiers and rejects a
mismatch. The path does not inspect interaction focus, mutate the world-object snapshot or object,
or make either world-object component authoritative. A successful resolution for any of the three
scene-authored targets replaces only the matching perception entry with its `Identified` successor,
consumes the exact outcome, and preserves completed activity absence plus every unrelated entry
instance and order. Exact water-cask resolution additionally asks the existing sabotage
presentation flow to recover through the now-visible authoritative evidence; other target outcomes
do not ask for recovery. With no matching outcome, a target absent from the current world-object
snapshot, a target-mismatched controller input, or a repeated R after consumption, all three
focused-search snapshots plus world-object, source, target, and presentation state retain their
exact instances; R reports `UNCHANGED` where input was attempted.

Each visible greybox target owns one instance of
`Allrpcs.Game.TemporaryResolvedPerceptionIndicator`. Its presentation boundary receives only that
target's existing nullable `WorldObject` and the controller's current Seeker perception, uses only
the object's exact stable identifier with `CharacterPerceptionLookup`, and shows its authored
`IDENTIFIED` label only for the exact `VisibilityState.Identified` singleton. Null objects or
perception, absent or unrelated entries, `Noticed`, and every other state remain hidden without
ranking or exposing object identity or description. `Main` initializes both indicators after the
existing one-time perception initialization and refreshes them only after a successful R
resolution. Each refresh resolves the visible target through `WorldObjectController` and passes
only the exact returned object; a missing object hides that target's marker without changing state.
Failed and repeated resolution paths preserve the existing presentation.

These inputs add no automatic polling or outcome processing, discovery grant, failure-reason
presentation, or final interaction UI.

One separate temporary label initializes to `Focused search: NO ATTEMPT`. A pressed F, C, E, or R
updates it with the attempted action and `CHANGED` or `UNCHANGED`, using only the existing Boolean
return from that lifecycle call path. It does not infer or present a failure reason. Released
focused-search actions, unrelated input, and healer actions preserve the exact prior text, while
the existing handled-input behavior remains unchanged.

The discovery-condition boundaries do not inspect a complete character, ownership or capacity
snapshots, perception or world state, grant discovery, or change visibility. They do not define
configurable Boolean expressions, alternative requirement groups, further requirement types, tool
classifications or properties, passive-noticing collection, selection, ordering, eligibility,
outcomes, or transitions, reinterpretation, actions, results, errors, events, effects, mutable
state, persistence, serialization, Godot, or UI behavior. Beyond the pure combined eligibility
decision, the focused-search boundaries add no discovery grant, visibility change,
complete-character or world inspection, replacement, range, duration, location, progress, outcome
production or application, reason value, further lifecycle policy, in-place mutation, scheduling,
persistence, production activity source, or final UI behavior. The temporary Godot adapters change
only the authored indicator visibility and the Boolean-derived result text, and the temporary
controller owns only the immutable snapshots and bounded lifecycle delegations described above.
The interaction-focus controller adds only
world-object-gated focus availability plus valid-focus and matching-world-object request/start
composition, while the temporary context source only supplies retained greybox inputs. Ordinary
greybox play invokes the start bridge only through physical F,
cancellation only through physical C, and completion only through physical E.

Point-and-click movement, zoom or camera-pan input, controller mappings beyond the documented
temporary keys, final interaction actions or authoritative outcomes, Phase 2 objects and
controller-owned perception models,
natural-language commands, final command or interaction UI, LLM-controlled companions, providers,
persistence, credentials, signing, installers, and distribution configuration are not part of
this greybox slice.

## Licence

Allrpcs is licensed under the GNU Affero General Public License version 3 only
(`AGPL-3.0-only`). See `LICENSE` for the complete terms.
