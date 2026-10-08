# Allrpcs Roadmap

## Purpose

Keep the remaining work, milestone acceptance criteria, and settled design constraints here.
Use [CONTINUATION.json](CONTINUATION.json) for the persistent closed scopes and one active slice;
use [HANDOVER.md](HANDOVER.md) for its handoff and current verification;
use [README.md](README.md) for development commands. Detailed completed-work evidence stays
in the linked audits and art packages, with earlier checklists retained in Git history.

When work is completed:

- Change its checkbox from `[ ]` to `[x]` only after the listed acceptance condition has been
  verified.
- Keep closure evidence in `CONTINUATION.json`; choose one missing deliverable from a
  remaining item below, then sync `HANDOVER.md` with `python3 scripts/check-continuation.py --sync`.
- Closed scopes survive handover pruning. Do not create another audit/comparison of the
  same accepted area as a successor; reopen it only at the owner's explicit request.
- Keep partially completed items unticked and describe their current state in `HANDOVER.md`.
- Do not mark platform, provider, packaging, or manual behaviour as verified without current
  evidence.

## Product vision

Allrpcs is a slow-paced, top-down 2D game of exploration, conversation, investigation, and
healing. A contagious disease causes infected people to fall into a deep sleep and waste away.
The player and a party of autonomous characters must find healing plants, recover lost
knowledge, test remedies, and contend with an enemy agent attempting to prolong the outbreak.

There are no conventional NPCs. Every embodied character is a role with a controller that can
be human, LLM, or dormant. The first release is single-player: one human controls the Seeker,
while LLMs control companions and encountered roles. Infected characters use the dormant
controller until successfully healed.

## Settled design decisions

- First release: single-player with LLM companions.
- Initial player role: a customizable Seeker with a selectable background.
- Initial LLM roles: travelling healer, two hermits, sage, enemy agent, and infected characters
  that become responsive only after healing.
- Presentation: continuous top-down 2D movement with smooth camera scrolling in any direction.
- Time model: real-time movement and animation, with disease progression tied to meaningful
  journey and investigation events rather than wall-clock time.
- Dialogue: unrestricted natural human language. Structured intentions are an invisible engine
  protocol, never a player-facing command vocabulary.
- World authority: the game engine owns truth, perception, inventory, movement, discovery,
  disease progression, and action outcomes. LLMs decide only what their characters attempt.
- Knowledge model: world truths, evidence, claims, beliefs, and private knowledge remain
  distinct.
- Quest model: a redundant evidence network with multiple valid routes, not a linear chain of
  quest flags.
- Persistence: durable world and character state is local and provider-neutral.
- LLM support: OpenAI, Anthropic, and Ollama through interchangeable adapters.
- Implementation: Godot 4.6 .NET with C#; Godot handles rendering and authoring while pure C#
  libraries hold domain and application rules.
- Initial renderer: Godot Compatibility renderer, unless prototype evidence requires otherwise.
- Voice: future ASR and TTS use the same dialogue pipeline through replaceable interfaces.
- Art production: validate greybox scale and camera first; select an art direction before
  producing final assets.

## First vertical-slice definition

The first complete milestone lets a player:

1. Create a Seeker and choose a background.
2. Meet and converse naturally with the healer.
3. Explore the afflicted village, stream, laundry, woodland, shrine, and abandoned wagon.
4. Discover hidden and partially understood evidence.
5. Ask the healer to perform a conditional task in natural language.
6. Form and test hypotheses about the disease.
7. Identify ingredients and prepare a stabilizing remedy.
8. Stabilize the first sleeping patient.
9. Save, close, reopen, and continue with character memory intact.
10. Run on macOS, Linux, and Windows.

## Completed foundation

Phases 0 and 2–8 are complete: project/tooling foundations; authoritative world, discovery and
saves; natural-language dialogue; provider adapters; character memory and relationships;
disease and evidence-driven quests; autonomous roles and fair sabotage; and the authored
opening scenario with its human acceptance checkpoint. Phase 1 greybox movement, camera,
collision, navigation, healer tasks, and interaction focus are established.

Do not reopen these milestones as new work. Detailed autonomous-role traceability remains in
[PHASE_7_ACCEPTANCE_AUDIT.md](PHASE_7_ACCEPTANCE_AUDIT.md). Provider, platform, credential,
packaging, and accessibility claims remain limited to their recorded evidence.

## Phase 1 — Remaining exploration work

- [ ] Implement point-and-click movement if retained after prototype testing.
- [ ] Pause disease time and world decisions during dialogue, reading, and inventory use.
- [ ] Add a debug overlay for position, region, navigation target, and current world-time event.

## Phase 9 — Production art and audio

### Selected direction and current production state

Phase 9.1 is complete. Use Direction A (painterly storybook gouache) and the
[art bible](art-direction/representative-scene/ART_BIBLE.md): 1280x720 base viewport,
aspect expansion, smooth filtering, and editable masters at exact 2x export dimensions.
Preserve the approved 3200x1800 opening-region layout and independent physical authority.
Direction D remains only the art bible's gated feasibility fallback.

The [production manifest](art-direction/production-proof/corrected/asset-manifest.json)
tracks exports, masters, provenance and isolated imports. Accepted runtime presentation now
includes ground and clearing transitions, garden assets, Seeker walk/idle/inspect animations,
the Village Square junction, retained Stream modules, and promoted opening-region paths.
Individual proof/correction/runtime records remain under
[the production review directory](art-direction/production-proof/corrected/review/).
Preserve failed candidates and historical inventories; reproduce old proofs on their pinned
source tree rather than changing evidence to accommodate successors.

The [rounded closed-forest path scope](art-direction/forest-path-ending-correction/runtime-promotion-scope.md)
is separately approved and implemented, including asset/master/imports and 18 runtime
captures matching accepted external pixels. Its
[runtime package](art-direction/forest-path-runtime-promotion/README.md) records the confirmed
checkpoint and verification. Preserve closed-side-only coverage and the existing gate and
navigation; the near side inside the Clearing remains untextured.

- [x] Obtain owner runtime visual confirmation before publishing this forest-path promotion
  on 2026-10-03 ("confirmed").

The [whole-channel composition](art-direction/approved-external-compositions/README.md)
and western watercourse corrections retain their original external acceptance records.
Whole-channel runtime promotion received its own named scope. The [prepared proposal](art-direction/whole-channel-runtime-scope/README.md)
received Stage 1 approval; the [completed disposable preview](art-direction/whole-channel-runtime-preview/README.md)
binds exact transparent assets and actual GPU views. The [Stage 2 packet](art-direction/whole-channel-runtime-preview/stage-2-scope.md)
received named production approval and is implemented in the
[runtime promotion](art-direction/whole-channel-runtime-promotion/README.md). The owner
confirmed the exact runtime views on 2026-10-03 ("confirmed and approved"), clearing the
publication checkpoint. Preserve accepted sources and the accepted
Shrine intersection triangle. Routine bounded external proof approvals remain waived;
named runtime/layout/physical promotion and runtime visual checkpoints remain separate.
Visual acceptance does not establish gameplay, pacing, usability, or accessibility acceptance.

### 9.2 Remaining production assets

Approved artwork batch (2026-10-04): **Reusable trees, shrubs, fallen wood, rocks, and
environmental details** — 42 modules; see `art-direction/woodland-environment-batch/README.md`.
Approved structures batch (2026-10-04): **Village, sickhouse, garden, shrine, laundry, wagon,
and woodland structures** — 42 modules; see `art-direction/structures-batch/README.md`.
Approved characters batch (2026-10-04): 456 active new assets and 48 inherited frames;
see `art-direction/characters-batch/README.md`. It includes eight designs, eight portraits,
432 active directional frames (144 idle/inspection plus 288 polished walks), eight
sleeping/recovery poses, three Seeker appearance choices,
and references to the 48 approved Seeker cloak frames. Facial shapes, noses and hairstyles
were revised following owner feedback. Animation registration, timing and runtime use remain
provisional. Original walk previews repeat the same leading foot and remain retained as
incomplete lineage. The owner authorized mechanical rigging; `characters-batch/walk-rig/`
contains 288 replacement eight-phase frames with alternating anatomical leg ownership.
The separate `characters-batch/walk-rig-polish/` adds 288 revised candidate frames,
shaft-aligned connectors, restrained knee flex and removal of a moving staff fragment.
Original artwork and the first rig snapshot remain as lineage. The owner approved
the complete polished walking series; exact hashes and scoped validation are frozen.
The owner also confirmed full-batch approval, including the 168 retained non-walking
assets. Complete active character acceptance is hash-bound.
The terrain category remains open for residual work.

Approved complete objects artwork batch (2026-10-04): 42 assets across all seven
families, editable 2x masters, seven labelled detail boards and two actual-export-scale
compositions; see `art-direction/objects-batch/README.md`. Lightweight authoring checks
and session review are recorded with the package. Exact owner acceptance and scoped
approval checks are recorded. No runtime or scenario content changed.

Approved complete UI artwork batch (2026-10-04): 36 components across journal, map,
inventory, dialogue, settings and provider-state; editable 2x masters, six labelled boards
and two export-scale compositions. See `art-direction/ui-batch/README.md` for exact-hash
owner acceptance and scoped validation; production remains external and no functional UI changed.
Review fixes, source crop accounting and byte reproduction are recorded with the package.
Approved effects source series (2026-10-04): 30 static candidates across weather,
lighting, discovery, treatment and disease-state presentation. The owner approved
the five displayed source sheets; editable 2x masters, five labelled family boards,
two full-series export-scale boards and representative groups were packaged afterwards.
See `art-direction/effects-batch/README.md` for exact source acceptance, scoped checks
and derivative reproduction evidence. Animation, anchors, runtime integration and
effects semantics remain separate.

Approved complete residual-ground artwork series (2026-10-04): 18 material, variation
and transition studies, editable 2x masters, three labelled family boards, both 1:1 scale
boards and representative groups. Exact acceptance binds all 45 artwork files; scoped
checks and byte reproduction pass. See `art-direction/residual-ground-batch/README.md`.
The original read-only terrain inventory verifies all 35 predecessor terrain exports.
The subsequent `art-direction/residual-ground-readiness/README.md` repeats all twelve
material/transition exports and exposes transparent perimeter gaps and unverified
connector geometry. The separate
[twelve-module external tiling feasibility series](art-direction/residual-ground-tiling-proof/README.md)
now contains six opaque repeatable 128x128 surfaces, six matching transitions, editable
2x masters, labelled detail boards and 1:1 repeat/connected compositions. Lightweight
authoring checks verify filled perimeters, exact opposite painted edges and 160 connected
joins. The owner approved the complete series ("Looks good - all approved"); acceptance
binds all 57 artwork files and the technical review manifest. Consolidated scoped checks
also pass all sixteen oriented curved ports. A read-only readiness audit measures twelve
new/legacy direct edge comparisons; these different materials have no shared exact paint
ports. The [read-only compatibility audit](art-direction/residual-ground-compatibility/README.md)
now shows all 36 overlay/field combinations and twelve legacy pairings in both join
directions. Six further 512px plain/dressed comparisons retain visible finite repeats.
The [authored interface audit](art-direction/residual-ground-compatibility/interfaces.md)
binds 39 terrain-texture assignments and 828 source files; no explicit new-field use
is selected. Select a field and intended interface/composition, or request a placement
proposal, before a separate source-matched external feasibility slice. No new legacy
connector family or material geography follows from generic edge colour differences.
The owner selected the [Garden meadow-apron composition](art-direction/garden-ground-feasibility/README.md)
after the external soil/apron comparison. Its tight soil mask follows the retained bed
alpha silhouettes; rejected dark crop-frame mats remain as evidence. Native source
composition, editable masters and six bounded disposable Godot previews pass scoped
checks. The [named Garden runtime scope proposal](art-direction/garden-ground-runtime-scope/README.md)
was explicitly approved. Both exact underlays are installed; the
[runtime checkpoint](art-direction/garden-ground-runtime-promotion/README.md)
has separate owner approval of all six runtime views and is cleared for reviewed
direct-main publication. No broader terrain placement is inferred from this promotion.
The owner requested a [next placement proposal](art-direction/ground-placement-proposal/README.md):
unchanged packed earth around the Village Square junction, using a bounded
alpha-derived apron. The owner selected its exact use and recipe; the
[external feasibility series](art-direction/square-ground-feasibility/README.md)
now contains native baseline/hard/feathered compositions, editable 2x masters,
edge/port comparisons and eighteen disposable wide/tall GPU views. The owner
approved the feathered composition; consolidated scoped checks and four mutation
probes pass. The separate [runtime scope proposal](art-direction/square-ground-runtime-scope/README.md)
has separate named approval in its owner record. The exact one-asset runtime
implementation is accepted in the [ordinary-scene checkpoint](art-direction/square-ground-runtime-promotion/README.md);
all six runtime views and paired baselines match the approved external pixels.
The owner separately confirmed all six runtime views on 2026-10-05; full quality and reviewed direct-main publication are cleared.
The owner authorized choosing the next placement and selected the **16px-feather
Sickhouse entrance pale-gravel composition** from the
[external four-variant series](art-direction/sickhouse-ground-feasibility/README.md).
Three editable underlay masters, native edge boards and all 24 disposable GPU
views are retained. Consolidated checks prove source/mask/master parity, protected
predecessor bytes and bounded GPU changes; four disposable mutations are rejected.
The [separate named runtime scope](art-direction/sickhouse-ground-runtime-scope/README.md)
was approved and its one unchanged 208x86 underlay is installed in the
[runtime checkpoint](art-direction/sickhouse-ground-runtime-promotion/README.md).
Six actual runtime views and paired baselines equal approved external GPU pixels.
The owner separately confirmed these exact views on 2026-10-05; full quality and reviewed direct-main publication are cleared.
No further placement or terrain-completeness acceptance follows from this scope.
The continuing request produced one [external Wagon entrance packed-earth proposal](art-direction/wagon-ground-feasibility/README.md):
complete four-variant comparisons, three layered masters and 24 disposable GPU views.
The owner selected feather-24; its [named runtime scope](art-direction/wagon-ground-runtime-scope/README.md)
received named approval. One unchanged 285x92 underlay is installed in its
[ordinary-scene checkpoint](art-direction/wagon-ground-runtime-promotion/README.md).
Six actual views and baselines match the selected external pixels. The owner
confirmed these exact actual views on 2026-10-05; reviewed direct-main publication
is cleared. Accepted physical/presentation owners are preserved.
Orientation samples turn coverage while retaining world-aligned source paint. Preserve
the approved studies and existing terrain. Overall terrain production suitability and
runtime integration remain open.

The continuing request now produced the [Shrine east-entrance pale-gravel external comparison](art-direction/shrine-ground-feasibility/README.md):
three editable 2x masters, 24 actual-scene views and complete GPU edge crops.
The proposed 92x143 apron at `(596,250)` uses z=-78 above unchanged woodland
transitions and below the route. Initial occluded z=-90 evidence is retained.
The owner selected feather-16; consolidated validation/preservation checks pass.
The [named runtime scope](art-direction/shrine-ground-runtime-scope/README.md) is
approved. The [ordinary-scene checkpoint](art-direction/shrine-ground-runtime-promotion/README.md)
installs the unchanged selected underlay; all six actual views and six baselines
equal selected external pixels exactly. The owner confirmed these exact actual views on 2026-10-05; the Shrine-ground slice is complete.

The [Laundry east-entrance packed-earth comparison](art-direction/laundry-ground-feasibility/README.md)
is produced as one complete external series: baseline, hard support and two
inward feathers, three editable 2x masters, 24 actual-scene views and edge crops.
The owner agreed to continue with recommended feather-24. Exact selection
hashes, independent masks/master/capture checks and successor preservation are
recorded. The [named runtime scope](art-direction/laundry-ground-runtime-scope/README.md)
has named approval and is implemented in the
[runtime checkpoint](art-direction/laundry-ground-runtime-promotion/README.md).
All six actual views and baselines exactly match selected external pixels;
the owner confirmed these exact actual views on 2026-10-05. The
[terminal acceptance](art-direction/laundry-ground-runtime-promotion/owner-runtime-acceptance.json)
closes the Laundry slice. Existing ground/channel acceptance is carried forward.

Completeness/accessibility/pacing remain open.

The [Village Edge external coverage audit](art-direction/village-edge-ground-coverage/README.md)
finds continuous opaque moss coverage at all three entrances. Its bounded apron recipe
has a [complete external comparison](art-direction/village-edge-ground-feasibility/README.md):
baseline/hard-support/feather-16/feather-24, three editable 2x masters, 24 full views
and twelve dedicated detail views. The owner approved feather-16 at all three
entrances on 2026-10-06. Fresh consolidated scoped checks pass; the separate
[named runtime scope proposal](art-direction/village-edge-ground-runtime-scope/README.md)
has owner approval. The [runtime checkpoint](art-direction/village-edge-ground-runtime-promotion/README.md)
installs the unchanged feather-16 underlay; all nine actual views and paired baselines
match selected external RGBA. The owner confirmed these exact actual views on 2026-10-06. Fresh product quality passes with 5,033 tests and all Godot smokes; terrain completeness remains open.

The [closed-forest approach coverage audit](art-direction/closed-forest-ground-coverage/README.md)
retains six baseline views and exact GPU gate crops. Opaque moss and the accepted
leaf-litter route already cover this approach; no additional apron is proposed.
Product files and accepted owners remain unchanged.

The [Clearing-opening external comparison](art-direction/clearing-village-edge-ground-feasibility/README.md)
retains baseline/hard-support/feather-16/feather-24 in 24 actual-scene views and
three editable masters, with exact native/2x crops. The agent recommends retaining
the baseline; no new runtime selection is recorded. Forty source-bound static
colour-vision/luminance images begin the remaining visual-check work for this
local shoulder. Human accessibility and broader contrast checks remain open.
All predecessor product/CI inputs and accepted owners are unchanged.

The [Woodland Clearing external dressing series](art-direction/woodland-clearing-dressing/README.md)
compares retained baseline, eight sparse placements and sixteen grouped placements
using unchanged approved modules and proposed pivots. Twenty-four actual-scene
captures, complete native/2x crops and thirty static readability images are retained.
Independent route/entrance/label clearance, frozen predecessor bytes and eight review
probes pass. The owner selected grouped on 2026-10-06, replacing the earlier sparse
choice; the hash-bound selection is closed. The owner approved the
[named sixteen-placement scope](art-direction/woodland-clearing-dressing-runtime-scope/owner-approval.json).
The [runtime installation](art-direction/woodland-clearing-dressing-runtime-promotion/README.md)
keeps exact native modules, pivots and draw order, copies unchanged editable masters
and adds independent proof/runtime imports (113 assets / 64 masters). Existing scene
blocks, physical, discovery and gameplay state are retained. The owner
[confirmed all eight exact actual runtime views](art-direction/woodland-clearing-dressing-runtime-promotion/owner-runtime-acceptance.json)
on 2026-10-06. All eight paired baselines and grouped runtime captures match selected
external RGBA; fresh product quality passes with 5,033 tests, all Godot smokes and
cold import. The entire grouped runtime slice is terminally accepted; do not reopen
historical pending records. On 2026-10-06 the owner closed the redundant proposed
forest-approach dressing successor. Further automatic ground/Clearing/forest studies
are parked in the [persistent ledger](CONTINUATION.json); this is not a claim of global
terrain, accessibility or pacing completion. The complete
[character import/registration proof](art-direction/character-animation-import-proof/README.md)
now plays 480 approved directional frames across 120 isolated animations. Measured
fixed pivots, exact decoded RGBA, independent lossless imports, ordered playback and
native/2× collective comparisons pass. This closes the isolated character deliverable;
ordinary character integration and the broader scale/collision/accessibility items
remain unfinished. The owner-accepted [reference gait model](art-direction/reference-gait-model.md)
closes the inherited cloak correction on 2026-10-07: 48 exact accepted frames/masters,
with sixteen natural side poses, brief double support, backward-only knee hinges,
knee-before-foot exchange and actual native/2× Godot import/playback evidence.
The [terminal acceptance](art-direction/seeker-cloak-reference-gait-revision/owner-acceptance.json)
binds the selected candidate and isolated proof. All sixteen accepted front/back
poses and every predecessor remain unchanged. Use this as the reference for future
walking animation work. Historical pending fields are superseded; the whole gait
correction and the approved audio-policy slice are closed. The audio bus/control
implementation is verified, and the complete material audition is accepted. The approved
six-sound runtime subset and approved Seeker terrain footsteps are implemented; the other
remaining audio scopes stay unfinished.

Artwork workflow (owner instruction, 2026-10-03, reaffirmed 2026-10-04 for every remaining
9.2 art slice): produce the entire selected category's
artwork series first, keeping editable 2x masters and separate review exports. Present all
asset families together as a labelled series of images, including scale references and
representative grouped compositions. Discuss and revise the artwork with the owner before
running full tests or CI. Do not repeat full code CI, Godot smokes or historical validation
suites for individual artwork candidates. During authoring, use only lightweight checks
needed to create and visually inspect the images. Once artwork revisions are agreed, run
scoped asset validation and required full tests/CI for the consolidated batch. Repeat checks
only as justified by subsequent changes or failures. Named runtime promotion approval and
runtime visual confirmation remain separate checkpoints. Carry existing owner approval
forward: reconcile direct instructions and terminal acceptance records before asking
anything. Matching acceptance supersedes historical pending metadata; never reopen
unchanged artwork, scope or displayed views after a new turn, documentation update
or verification retry. A genuinely new scope or materially changed asset is a new
decision; finish already authorized work without another approval request.
When product code, runtime assets and CI inputs are verified unchanged, retain the preceding
passing full-suite evidence and run fresh scoped validation for the newly approved artwork.

- [ ] Produce terrain tiles, transitions, paths, banks, water, and ground variation.
- [x] Produce reusable trees, shrubs, fallen wood, rocks, and environmental details.
- [x] Produce village, sickhouse, garden, shrine, laundry, wagon, and woodland structures.
- [x] Produce owner-approved character designs, portraits, directional sprites and animation-frame artwork.
- [x] Import and register the complete approved directional character animation set in an isolated proof.
- [x] Correct the inherited Seeker cloak walking gait with alternating contacts and passing poses in an isolated candidate.
- [x] Produce herbs, remedies, tools, scrolls, inscriptions, containers, and clue objects.
- [x] Produce journal, map, inventory, dialogue, settings, and provider-state UI assets.
- [x] Produce weather, lighting, discovery, treatment, and disease-state effects.
- [ ] Validate alpha, seams, scale, pivots, collision silhouettes, and texture import settings.
- [ ] Test important visuals under colour-vision and contrast simulations.
- [ ] Keep editable source assets separate from exported runtime assets.

### 9.3 Sound and music

The [audio direction and dynamic range policy](audio-direction/README.md) is owner-approved
on 2026-10-07: quiet tactile naturalism, the existing four authored rain-based regions,
sparse optional music, perception-bound cues/captions, independent category controls and
explicitly provisional mix targets. [Terminal acceptance](audio-direction/owner-acceptance.json)
binds the exact reviewed policy files and supersedes their historical proposal fields.
The [structured policy](audio-direction/dynamic-range-policy.json) remains a design record;
the external material audition is owner-accepted, and six unchanged excerpts now play in
ordinary Main. The
[bus/control implementation](audio-direction/buses-controls.md) is complete under the
[approved runtime scope](audio-direction/buses-controls-authority.json): five sibling buses
plus Master, persistent independent gains/mutes, accessible settings, functional range/mono
controls and disposable foreground/routing diagnostics. [Verification](audio-direction/buses-controls-verification.json)
records 5,054 tests, the complete Godot smoke suite and 38 measured final-output probes.
The [finite source/licensing plan](audio-direction/material-audition-source-plan.md) now
accounts for all sixteen audition candidates and acquisition/credit/measurement records.
The owner approved acquisition and the isolated complete-series audition on 2026-10-07.
The [external audition package](audio-direction/material-audition/README.md) retains all
sixteen excerpt classes, three measured five-minute mixes, rights/credits, edit lineage
and nineteen passing real-material output probes. The owner accepted fifteen excerpts
and requested only a quiet flute revision for M02 rest/healing. Those fifteen remain
frozen. The owner then accepted revised M02 and the complete series, selecting Default
range. [Terminal acceptance](audio-direction/material-audition/owner-acceptance.json)
closes the whole audition slice; no further listening gate remains. Seventeen source containers,
including the rejected rest source, are retained. The [approved material runtime scope](audio-direction/material-runtime-scope.md)
maps all sixteen accepted candidates to actual owners and missing prerequisites.
Under [owner authority](audio-direction/material-runtime-authority.json), B01–B04 regional
beds and A04 grinding/A05 treatment feedback are implemented with bounded crossfades,
independent captions, source credits and success/load/lease isolation.
[Runtime verification](audio-direction/material-runtime-verification.json) records 5,072
unit tests, the retained Godot suite, ordinary action parity and 36 measured output cases.
The six shipping WAVs remain byte-identical to the accepted audition. Full macOS
application export is unavailable on this host; scoped audio/resource packaging passes.
The approved Seeker terrain-footstep scope is now implemented with four explicit
materials, post-collision supported displacement cadence, 24 exact PCM contact slices,
independent captions and packaged provenance. [Footstep verification](audio-direction/footstep-runtime-verification.json)
and [review evidence](audio-direction/footstep-runtime-review/README.md) retain 5,095 unit
tests, 293 retained Godot launches, four Main modes with identical full save roots, 67
Master-output cases, native PCM parity and resource packaging. The named slice is closed;
other interiors, Healer/NPC steps, water/tool actions, positional cues, music and voice
remain unfinished. No anatomical animation synchronization, device listening or runnable
release is claimed. The broader material-sound roadmap item remains unchecked.

- [x] Establish the audio direction and dynamic range policy.
- [ ] Add footsteps by terrain, water, wind, foliage, interiors, tools, and remedy preparation.
- [ ] Add positional ambience and meaningful off-screen sound cues.
- [ ] Add restrained music that supports slow investigation without manufacturing urgency.
- [x] Add volume buses and independent controls for music, ambience, effects, dialogue, and
  future speech.

Acceptance:

- [ ] The complete opening region has a coherent, accessible visual and audio identity, and all
  gameplay-relevant objects remain legible without hidden-object sparkle or pixel hunting.

## Phase 10 — Voice-ready interaction and optional ASR/TTS

- [ ] Define `ISpeechRecognizer` independently of any provider.
- [ ] Define `ISpeechSynthesizer` independently of any provider.
- [ ] Implement microphone permission and capture behind a platform-neutral boundary.
- [ ] Implement push-to-talk first.
- [ ] Add partial and final transcript events.
- [ ] Allow correction or confirmation before consequential actions when recognition is
  uncertain.
- [ ] Implement streamed or buffered character speech playback.
- [ ] Implement per-character voice profiles without putting voice IDs in domain rules.
- [ ] Implement player interruption and speech cancellation.
- [ ] Keep subtitles and full text interaction available at all times.
- [ ] Add one ASR implementation and one TTS implementation.
- [ ] Add provider alternatives only after the common path is verified.
- [ ] Test device loss, permission denial, network failure, feedback loops, accents, and noisy
  input.

Acceptance:

- [ ] Typed and spoken input produce the same dialogue events and intention flow, and the game
  remains fully playable without microphone or speech services.

## Phase 11 — Cross-platform hardening and release candidate

- [ ] Validate fresh installation on supported Windows systems.
- [ ] Validate fresh installation on Apple Silicon macOS.
- [ ] Validate Intel macOS if retained in the support matrix.
- [ ] Validate representative supported Linux distributions and display servers.
- [ ] Test high-DPI displays, window resizing, full-screen modes, and multiple monitors.
- [ ] Complete remappable keyboard, mouse, and controller controls.
- [ ] Complete keyboard-only navigation and screen-reader evaluation for text-heavy UI.
- [ ] Test long sessions, repeated saves, corrupt-save recovery, and schema migrations.
- [ ] Test local-model performance and constrained hardware behaviour.
- [ ] Test provider rate limits, outages, cancellation, and credential removal.
- [ ] Ensure diagnostics exclude credentials and dialogue content by default.
- [ ] Establish crash reporting and consent policy.
- [ ] Establish package identity, icons, versioning, and update policy.
- [ ] Build platform-native packages on native runners.
- [ ] Sign Windows packages when distribution credentials are authorized.
- [ ] Sign and notarize macOS packages when distribution credentials are authorized.
- [ ] Choose and validate Linux package formats.
- [ ] Run complete opening-scenario smoke tests on packaged builds.
- [ ] Prepare release notes, privacy information, model-provider disclosures, and known
  limitations.

Acceptance:

- [ ] Signed or explicitly documented unsigned release candidates install, launch, save, load,
  configure supported providers, and complete the opening scenario on every claimed platform.

## Phase 12 — Deferred expansions

These are intentionally outside the first vertical slice and should not be pulled forward
without an explicit roadmap decision:

- [ ] Online cooperative multiplayer with server-authoritative world state.
- [ ] Asynchronous human turns.
- [ ] Shared-computer multi-character play.
- [ ] Human takeover or reassignment of any LLM role during a campaign.
- [ ] Additional realms, diseases, remedies, and evidence networks.
- [ ] Scenario-authoring tools suitable for non-programmers.
- [ ] Mod and community-scenario packaging.
- [ ] Procedurally assisted landscape dressing that preserves authored discovery logic.
- [ ] Additional accessibility modalities.
- [ ] Mobile or web ports.

## Cross-cutting engineering requirements

These standing requirements apply to every phase:

- Keep domain rules deterministic and independently testable wherever possible.
- Keep Godot types at the presentation and engine-integration boundary.
- Keep provider types inside provider adapters.
- Give each LLM only its character's filtered context.
- Validate all world-changing intentions in the engine.
- Never embed cloud-provider credentials in source or distributable assets.
- Preserve full natural-language transcripts locally while sending only relevant bounded
  context to a provider.
- Do not store hidden chain-of-thought.
- Make critical quest knowledge redundant and recoverable.
- Treat silence, refusal, uncertainty, and disagreement as valid character behaviour.
- Ensure the game remains saveable when a provider is unavailable.
- Add automated tests before or with testable behaviour, not only after regressions.
- Use representative manual play evidence for interaction and visual claims.
- Update this file and `HANDOVER.md` at the end of each completed slice.
