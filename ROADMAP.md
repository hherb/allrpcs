# Allrpcs Roadmap

## Purpose

This roadmap is the durable implementation checklist for Allrpcs. It records the agreed
product direction, implementation order, and completion criteria so that future sessions can
continue without reopening settled design decisions.

When work is completed:

- Change its checkbox from `[ ]` to `[x]` only after the listed acceptance condition has been
  verified.
- Update `HANDOVER.md` with the evidence, limitations, and exact next bounded slice.
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

## Phase 0 — Project foundation

Complete. The repository, Godot/.NET solution, dependency boundaries, local quality spine,
credential-free hosted checks, unsigned development exports, tool pins, and `AGPL-3.0-only`
licence are established. Scenario-validator CI remains attached to the validator work in Phase 2.
Exact commands and current limitations live in `README.md` and `HANDOVER.md`.

## Phase 1 — Greybox exploration

The credential-free greybox establishes keyboard movement, smooth bounded camera framing,
representative collision, deterministic navigation, healer follow/wait/move behavior, interaction
focus, and movement-boundary regression coverage. Its controls and markers remain temporary.

- [ ] Implement point-and-click movement if retained after prototype testing.
- [ ] Pause disease time and world decisions during dialogue, reading, and inventory use.
- [ ] Add a debug overlay for position, region, navigation target, and current world-time event.

## Phase 2 — Authoritative world and discovery

Complete. Deterministic world objects, perception, focused search, passive noticing,
reinterpretation, evidence filtering, inventory, events, qualitative time, scenario validation,
versioned canonical save JSON, bounded storage, and scene capture/restoration are implemented and
tested. The engine remains authoritative and filtered views do not leak hidden facts.

## Phase 3 — Natural-language dialogue with a deterministic character

Complete. Unrestricted player dialogue, typed internal acts and intentions, deterministic healer
responses, streaming presentation, interruption, one-at-a-time turn scheduling, conditional
multi-step commitments, authoritative physical outcomes, transcripts, and event-log separation are
implemented and tested, including prompt-shaped in-world speech.

## Phase 4 — First live LLM character

Complete. Provider-neutral model contracts and bounded context assembly support OpenAI, Anthropic,
Ollama, and OpenAI-compatible endpoints with strict structured output, cancellation, timeout,
safe retry, filtered affordances, redacted diagnostics, secret-free settings, OS credential seams,
explicit offline behavior, and credential-free/live conformance boundaries. Native credential and
packaging claims remain limited to their separately recorded evidence.

## Phase 5 — Character memory, beliefs, and relationships

Complete. Filtered character memory, claims, beliefs, contradictions, confidence changes,
relationships, goals, bounded context relevance, memory save/restore, and dialogue-driven updates
are deterministic and preserve private-knowledge boundaries.

## Phase 6 — Disease and evidence-driven quest systems

Complete. Disease stages, meaningful-event progression, evidence networks, hypotheses,
experiments, ingredients, preparation, administration, first-patient stabilization, save state,
recovery routes, and dead-end validation are implemented as authoritative provider-neutral rules.

## Phase 7 — Multiple autonomous roles and fair sabotage

Complete. Provider-neutral autonomous participation, scheduling, dialogue follow-up, independent
tasks, actor-bound action authoring, exact acknowledgement/outcome/recovery routes, and all seven
sabotage kinds are implemented with shared turn ownership. Sabotage obeys ordinary access and
authority rules, cannot rewrite truth or permanently remove every critical route, and leaves
recoverable evidence. `PHASE_7_ACCEPTANCE_AUDIT.md` retains the detailed traceability record.

## Phase 8 — Authored opening vertical slice

Complete. The authored opening region, player/background setup, investigation routes, recovery,
journal/map state, persistence checkpoint, and human acceptance checkpoint are complete.

- [x] Establish the continuous validated opening region through the sickhouse, healer's garden,
  village transitions, stream, abandoned laundry, woodland clearing, and old-shrine greyboxes,
  including the inert rare-specimen and layered-inscription markers.
- [x] Complete player-triggered layered-inscription discovery at the old shrine through the mapped
  ordinary I action, exact access and reinterpretation, atomic one-shot presentation, and filtered
  evidence exposure.
- [x] Build the southern road and abandoned wagon.
- [x] Build the closed path leading toward the deeper forest.
- [x] Add weather, lighting, sound regions, and story-state variations needed by discoveries.
- [x] Implement Seeker name, pronouns, appearance choices, and background selection.
- [x] Implement Forester, Scribe, Apothecary's Assistant, and Traveller route variations without
  making any one background mandatory.
- [x] Implement the journal sections for observations, testimony, hypotheses, experiments, and
  open questions.
- [x] Implement the incomplete map, reported locations, verified locations, and player notes.
- [x] Author multiple investigation routes through the region.
- [x] Author the first stabilization and subsequent testimony.
- [x] Author the sage consultation recovery path.
- [x] Playtest for dead ends, clue visibility, pacing, dialogue overload, and disease pressure.

Acceptance:

- [x] A new player can complete the opening scenario without developer intervention, objective
  arrows, a prescribed clue order, or knowledge unavailable to their characters.

## Phase 9 — Art direction, production art, and audio

### 9.1 Art-direction selection

- [x] Generate or draw several preview-only visual directions after greybox scale is stable.
- [x] Select the perspective, projection, camera framing, and base asset resolution.
- [x] Select a palette, lighting language, texture treatment, outline policy, and atmosphere.
- [x] Establish character proportions and animation requirements.
- [x] Establish visual rules for ordinary, noticeable, hidden, identified, and dangerous
  objects without relying on colour alone.
- [x] Create an art bible with reference images, constraints, and exclusions.
- [x] Track asset provenance, source files, prompts where applicable, licences, and edits.

Acceptance:

- [x] One small representative scene containing terrain, a building, two characters, plants,
  an interactable object, and dialogue UI is coherent and readable at gameplay scale.

### 9.2 Production assets

- [x] Complete one bounded source-sheet pipeline proof for modular terrain/path, herb-bed/foliage,
  preparation-table state cues, and a Seeker four-direction walk sample; keep its recorded alpha,
  seam, pivot, baseline, and editable-master failures outside the runtime.
- [x] Correct that proof into individual clean-alpha exports with one manifest, exact 2x geometry,
  layered masters, seam and grayscale checks, and a clear review composition.
- [x] Normalize all four six-frame Seeker walk directions after the first direction passed, then
  prove exact source parity and lossless smooth-filtered import in an isolated Godot scene without
  changing ordinary presentation or gameplay authority.
- [x] Establish an external two-frame idle/breathe proof for all four Seeker directions with clean
  alpha, fixed pivots and contact baselines, explicit timing, layered source, and no runtime import.
- [x] Import the four idle/breathe loops into the isolated proof with exact byte parity, fixed
  origins, 600 ms timing, and no ordinary-scene connection.
- [x] Establish and import a generic four-frame reach-and-inspect proof for all four Seeker
  directions with clean alpha, fixed origins, 160 ms non-looping timing, retained prompts and
  provenance, and no gameplay authority.
- [x] Establish one modular 90-degree packed-earth path bend with a 256x256 clean-alpha export,
  exact 512x512 editable master, 96-pixel straight-path-compatible ports, external review
  composition, and isolated lossless source-parity import without gameplay authority.
- [x] Establish one modular packed-earth T-junction with a 256x256 clean-alpha export, exact
  512x512 editable master, three 96-pixel straight-path-compatible ports, and an external review
  composition without runtime authority.
- [x] Import only that exact manifest-bound T-junction into the isolated proof with lossless source
  parity and no ordinary-scene connection.
- [x] Establish one modular packed-earth cross-junction with a 256x256 clean-alpha export, exact
  512x512 editable master, four 96-pixel straight-path-compatible ports, and an external review
  composition without runtime authority.
- [x] Import only that exact manifest-bound cross-junction into the isolated proof with lossless
  source parity and no ordinary-scene connection.
- [x] Establish one modular packed-earth path end-cap with a 256x256 clean-alpha export, exact
  512x512 editable master, one 96-pixel top port, rounded terminus above a transparent lower half,
  and an external review composition.
- [x] Import only that exact manifest-bound end-cap into the isolated proof, then prove the
  straight, bend, T-junction, cross-junction, and end-cap compose as one deterministic connected
  topology without ordinary-scene authority.
- [x] Establish and import one repeatable woodland leaf-litter ground tile with a 256x256 opaque
  export, exact 512x512 editable master, opposing-edge parity, 2x2 seam proof, useful texture and
  contrast gates, and no path-like corridor or ordinary-scene authority.
- [x] Establish and import one deterministic quiet-moss-to-leaf-litter edge transition with exact
  source connector regions, repeatable top/bottom edges, a two-row external review, source parity,
  and no ordinary-scene authority.
- [x] Establish and import two complementary quiet-moss/leaf-litter outer corners with exact
  straight-transition seams, exact unused moss edges, editable masters, connected reviews, source
  parity, and no ordinary-scene authority.
- [x] Establish and import two complementary quiet-moss inset corners in leaf litter with exact
  transition seams, exact unused leaf-litter edges, editable masters, connected reviews, source
  parity, and no ordinary-scene authority.
- [x] Establish one manifest-bound external colour-vision and contrast review for quiet moss,
  woodland leaf litter, packed earth, and all five existing ground transitions using documented
  full-severity simulations and descriptive luminance/texture measurements without claiming human
  accessibility acceptance.
- [x] Rebaseline the base viewport to 1280x720 with aspect expansion and regenerate all Seeker walk,
  idle/breathe, and reach-and-inspect proofs from retained high-resolution sources at a minimum
  144-pixel visible height, exact 2x masters, and a fixed 156-pixel planted-foot baseline without
  changing terrain pixels or ordinary gameplay art references.
- [x] Create one external 3200x1800 opening-region layout proposal with all eleven retained
  locations, exact route topology, quantified travel and camera framing, explicit empty-area
  content budgets, and no runtime authority; keep implementation gated on explicit review.
- [x] After explicit product-owner approval, implement that exact proposal as one coordinated
  greybox reflow with proposal-hash provenance, 3200x1800 world and camera bounds, eleven anchors,
  twelve physically clear route corridors, collision-aligned landmark openings, and the retained
  closed north-west forest gate without changing scenario or gameplay authority.
- [x] Complete the product-owner graphical greybox checkpoint across all anchors and open routes,
  the closed gate, Shrine-to-Southern-Road pacing, and 0.8/1.0/2.0 framing in representative window
  shapes; correct the exposed Village Edge, Sickhouse, and Stream readability defects without
  changing topology or gameplay authority.
- [x] Promote only the manifest-bound quiet-moss tile into runtime ownership with exact source/hash
  parity, a repeat-enabled lossless import, full 3200x1800 coverage behind the retained greybox, and
  credential-free static and engine checks without importing another production asset.
- [x] Complete the quiet-moss product-owner review in supplied 1.0x and 2.0x views and move only the
  identified stone-marker description out of its exposed 2.0x yellow-opening overlap, with a focused
  non-overlap regression and no route, camera, collision, or gameplay change.
- [x] Promote only the manifest-bound woodland leaf-litter tile into separate runtime ownership with
  exact source/hash parity and repeat it across the existing 648x250 Woodland Clearing bounds behind
  all retained greybox presentation, without adding transitions or changing physical authority.
- [x] Complete the leaf-litter product-owner review at 0.8x, 1.0x, and 2.0x in supplied 1280x720 and
  1280x827 views, accepting its repetition, seams, scale/readability, contrast, intentionally hard
  edge, and corrected stone-marker description placement.
- [x] Promote only the manifest-bound straight moss-to-leaf transition into separate runtime
  ownership and repeat one exact quarter-turn across the Woodland Clearing's north edge behind the
  retained greybox presentation, leaving the other three edges and every corner unchanged.
- [x] Correct the product-owner-rejected hard clearing edges by promoting only the two manifest-bound
  outer corners and reusing the straight transition across a derived rounded visual perimeter,
  without changing the exact leaf-tile bounds, collision, navigation, routes, or gameplay authority.
- [x] Complete the rounded-perimeter product-owner review across the supplied 0.8x, 1.0x, and 2.0x
  wide and tall views, recording the accepted correction as "much better" without claiming human
  accessibility acceptance.
- [x] Promote only the manifest-bound straight path and its matching rounded end cap across the
  exact Abandoned Laundry-to-Woodland Clearing connection, preserving the other eleven greybox
  routes and all collision, navigation, topology, landmark, and gameplay authority.
- [x] Complete the straight-path product-owner review across the supplied 0.8x, 1.0x, and 2.0x wide
  and tall views, recording approval only for the final Laundry-to-Clearing presentation without
  claiming pacing, usability, or human accessibility acceptance.
- [x] Check the existing straight and 90-degree bend modules against the approved
  Stream–Laundry–Woodland junction geometry. The exact ports cannot follow its 57.599282-degree
  turn; retain the greybox junction and gate one junction-specific proof module on approval.
- [x] After explicit product-owner approval, establish and import one isolated 512x512
  Stream–Laundry–Woodland junction proof with a 1024x1024 editable master, exact live-angle
  96-pixel ports, a bound 335x458 visible footprint, exact seam reconstruction, and curved
  physical-route containment without changing `Main.tscn` or runtime authority.
- [x] Complete the isolated opening-junction product-owner review, replacing the rejected hard
  texture overlap with an alpha-aware tangent-continuous curve and removing the preview-only elbow
  triangle before recording acceptance; do not treat that acceptance as runtime-promotion approval.
- [x] After separate explicit product-owner approval, promote only that exact manifest-bound
  opening junction at the live Stream–Laundry–Woodland turn with exact source/hash/import parity,
  `(224, 224)` pivot placement, rigid `133.025066°` rotation, retained greybox route presentation,
  and unchanged collision, navigation, topology, camera, and gameplay authority.
- [x] Complete the ordinary-scene opening-junction review across the supplied 0.8x, 1.0x, and 2.0x
  1280x720 and 1280x827 views before recording runtime visual acceptance.
- [x] After explicit product-owner approval, establish, import, and promote one deterministic
  1024x512 world-aligned Woodland Clearing/Village Edge overlay with a 2048x1024 editable master,
  exact clearing/opening geometry, and a 64-pixel alpha feather without changing collision,
  navigation, routes, layout, camera, scenario, or gameplay authority.
- [x] Complete the overlay review across supplied 0.8x, 1.0x, and 2.0x wide and tall views; replace
  the rejected hard straight edges, extend the correction across the north/northeast intersection,
  and record final visual acceptance without claiming pacing, usability, or human accessibility
  acceptance.
- [x] After explicit product-owner approval, promote only the manifest-bound broadleaf foliage into
  separate runtime ownership and repeat six exact planted-foot-pivot instances across the existing
  Healer's Garden plot without adding collision, navigation, interaction, or gameplay authority.
- [x] Complete the garden-foliage review across supplied 0.8x, 1.0x, and 2.0x wide and tall views;
  replace the rejected single clump with full-plot coverage beneath the retained preparation bench,
  and record visual acceptance without claiming pacing, usability, or accessibility acceptance.
- [x] After explicit product-owner approval and scale feedback, regenerate the ordinary preparation
  table and its proof-only cue canvases at exact 96x96 logical and 192x192 master geometry, then
  promote only the ordinary table into separate runtime ownership at the existing Healer's Garden
  station without changing preparation, interaction, collision, navigation, or gameplay authority.
- [x] Complete the preparation-table review across supplied 0.8x, 1.0x, and 2.0x wide and tall
  Compatibility views and record visual acceptance without treating the proof-only cues as runtime
  assets or claiming pacing, usability, or accessibility acceptance.
- [x] After explicit product-owner approval, promote only the manifest-bound empty herb bed into
  separate runtime ownership and place two exact planted-foot-pivot instances beneath the accepted
  Healer's Garden foliage and table, replacing only the greybox plot and bed-line visuals without
  changing collision, navigation, interaction, remedy preparation, treatment, or gameplay authority.
- [x] Correct the first herb-bed review's out-of-bed north-left and north-right foliage by moving
  only the complete north row south into the retained bed interiors, without changing assets,
  pivots, table placement, physical authority, or gameplay authority.
- [x] Complete the herb-bed product-owner review across the supplied 0.8x, 1.0x, and 2.0x wide and
  tall Compatibility views before recording visual acceptance or authorizing another asset.
- [x] After explicit product-owner approval, promote only the 24 manifest-bound Seeker walk frames
  into separate runtime ownership and replace only the greybox body/facing presentation with exact
  four-direction six-frame 120 ms loops, retaining movement, collision, navigation, camera,
  interaction, and gameplay authority unchanged.
- [x] Complete the Seeker-walk product-owner review across supplied 0.8x, 1.0x, and 2.0x wide and
  tall Compatibility views; record the product owner's presentation approval and screen-specific
  finding that 2.0x felt playable and enjoyable without changing the camera default or claiming
  broader pacing, usability, or accessibility acceptance.
- [x] After explicit product-owner approval, promote only the eight manifest-bound Seeker
  idle/breathe frames into separate runtime ownership and play exact four-direction two-frame 600 ms
  loops while stationary, retaining the last movement facing and leaving walk, movement, collision,
  navigation, camera, interaction, and gameplay authority unchanged.
- [x] Complete the Seeker idle/breathe product-owner review across both frames in all four directions
  at 0.8x, 1.0x, and 2.0x in supplied wide and tall Compatibility views; record only that the
  presentation looked good without claiming pacing, usability, or human accessibility acceptance.
- [x] After explicit product-owner approval, promote only the 16 manifest-bound Seeker
  reach/inspect frames into separate runtime ownership and play exact four-direction non-looping
  four-frame 160 ms gestures only after changed focused-search resolution, with movement
  interruption and no input, timing-gate, physical, or gameplay-authority change.
- [x] Complete the Seeker reach/inspect product-owner review across all four frames and directions at
  0.8x, 1.0x, and 2.0x in supplied wide and tall Compatibility views; record visual acceptance
  without claiming pacing, usability, or human accessibility acceptance.
- [x] Check the existing manifest-bound cross-junction against the four live Village Square route
  headings in deterministic 0.8x, 1.0x, and 2.0x wide and tall external views; retain the greybox
  junction after proving that exact rigid placement leaves a minimum 70.670096-degree port mismatch.
- [x] After explicit product-owner approval, establish one isolated tailored 512x512 Village Square
  junction with a 1024x1024 editable master, four exact 96-pixel live-heading ports, route-band
  containment, six deterministic external review views, and isolated Godot import parity.
- [x] Complete product-owner visual review of the tailored Village Square junction; record approval
  of the corrected four-path alignment only, retain the greybox junction and all physical/gameplay
  authority, and require separate approval before any runtime promotion.
- [x] After separate explicit product-owner approval, promote only the exact manifest-bound
  tailored Village Square junction into runtime ownership at its `(256, 256)` pivot, replace only
  the four greybox route portions beneath its 512x512 footprint, and retain all four exact
  96-pixel centrelines and continuations without changing physical or gameplay authority.
- [x] Complete the ordinary-scene Village Square junction review across supplied 0.8x, 1.0x, and
  2.0x wide and tall Compatibility views and record only the bounded runtime visual acceptance.
- [x] After explicit product-owner approval, establish one external feasibility-only Stream proof
  with repeatable water, a mirrored bank edge, one reusable opening cap, exact authored band,
  crossing, collision and route geometry, six deterministic 0.8x/1.0x/2.0 wide/tall views, and
  isolated Godot import parity without changing `Main.tscn` or runtime authority.
- [x] Complete product-owner review of the six Stream water-and-bank feasibility views and record
  only the approved external presentation without inferring runtime-promotion, pacing, usability,
  or human-accessibility acceptance.
- [x] After separate explicit product-owner approval, promote only the three manifest-bound Stream
  modules into runtime ownership at the retained 400x96 band, leave the centered 128x96 crossing
  above the water, and mirror clipped banks and caps only over the two retained collision polygons.
- [x] Complete product-owner visual review of the six ordinary-scene Stream views at 0.8x, 1.0x,
  and 2.0x in wide and tall Compatibility frames; the owner said they look right without
  requesting a correction. This records only bounded visual acceptance.
- [x] After separate owner approval, test the unchanged straight-path module as an external-only
  124x96 Village Edge–Village Square connector in six deterministic wide/tall views. Record its
  failed greybox/port feasibility without changing runtime.
- [x] Complete the prescribed external endpoint-blend correction and record its exact seams but
  conspicuous opaque west fade. Retain the original failed proof and all runtime assets unchanged.
- [x] Establish the external source-alpha/eight-column west RGB comparison in six deterministic
  0.8x/1.0x/2.0x wide/tall views, retaining original columns 8..91 and the exact corrected east
  blend. Recommend it for owner review without inferring visual acceptance or promotion authority.
- [x] Complete the alpha connector's owner visual checkpoint and authorized promotion: one
  manifest-bound 124x96 connector, one 248x192 editable master, and one presentation-line
  shortening. Preserve exact accepted RGBA pixels and all physical/navigation authority.
- [ ] Complete the ordinary-scene owner checkpoint for the connector's six runtime views,
  which match the accepted external previews pixel-for-pixel.

- [ ] Produce terrain tiles, transitions, paths, banks, water, and ground variation.
- [ ] Produce reusable trees, shrubs, fallen wood, rocks, and environmental details.
- [ ] Produce village, sickhouse, garden, shrine, laundry, wagon, and woodland structures.
- [ ] Produce character designs, portraits, directional sprites, and required animation frames.
- [ ] Produce herbs, remedies, tools, scrolls, inscriptions, containers, and clue objects.
- [ ] Produce journal, map, inventory, dialogue, settings, and provider-state UI assets.
- [ ] Produce weather, lighting, discovery, treatment, and disease-state effects.
- [ ] Validate alpha, seams, scale, pivots, collision silhouettes, and texture import settings.
- [ ] Test important visuals under colour-vision and contrast simulations.
- [ ] Keep editable source assets separate from exported runtime assets.

### 9.3 Sound and music

- [ ] Establish the audio direction and dynamic range policy.
- [ ] Add footsteps by terrain, water, wind, foliage, interiors, tools, and remedy preparation.
- [ ] Add positional ambience and meaningful off-screen sound cues.
- [ ] Add restrained music that supports slow investigation without manufacturing urgency.
- [ ] Add volume buses and independent controls for music, ambience, effects, dialogue, and
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

These requirements apply to every phase:

- [ ] Keep domain rules deterministic and independently testable wherever possible.
- [ ] Keep Godot types at the presentation and engine-integration boundary.
- [ ] Keep provider types inside provider adapters.
- [ ] Give each LLM only its character's filtered context.
- [ ] Validate all world-changing intentions in the engine.
- [ ] Never embed cloud-provider credentials in source or distributable assets.
- [ ] Preserve full natural-language transcripts locally while sending only relevant bounded
  context to a provider.
- [ ] Do not store hidden chain-of-thought.
- [ ] Make critical quest knowledge redundant and recoverable.
- [ ] Treat silence, refusal, uncertainty, and disagreement as valid character behaviour.
- [ ] Ensure the game remains saveable when a provider is unavailable.
- [ ] Add automated tests before or with testable behaviour, not only after regressions.
- [ ] Use representative manual play evidence for interaction and visual claims.
- [ ] Update this file and `HANDOVER.md` at the end of each completed slice.
