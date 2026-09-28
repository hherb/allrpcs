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
  layered masters, seam and grayscale checks, and a clear 960x540 review composition.
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
