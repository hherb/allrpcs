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

- [x] Establish deterministic authoritative world, discovery, event, and qualitative-time rules.
- [x] Orchestrate atomic world-event occurrence in the scene.
- [x] Advance qualitative time when herb-patch identification resolves.
- [x] Prove the first meaningful-time event through the ordinary focused-search input path.
- [x] Define the immutable root save-schema version vocabulary.
- [x] Reject structurally valid but unsupported save-schema versions.
- [x] Compose exact discovery snapshots as immutable save state.
- [x] Compose exact authoritative event log and qualitative time as immutable save state.
- [x] Compose the schema version and dynamic state as an immutable save root.
- [x] Gate save-root loading on explicit schema-version support.
- [x] Validate saved event logs and qualitative time against authored event definitions.
- [x] Validate saved discovery ownership and targets against authoritative world objects.
- [x] Compose version, discovery, and event checks as one pure save-restoration gate.
- [x] Restore an eligible in-memory save root into the live discovery and event controllers.
- [x] Capture exact current controller state as an in-memory versioned save root.
- [x] Serialize and strictly deserialize the supported save root as canonical JSON.
- [x] Prove serialized in-memory round-trip restoration through the scene controllers.
- [x] Persist and load a versioned save document through a bounded storage seam.
- [x] Add representative discoveries and scenario validation.
- [x] Prove filtered views do not leak hidden facts.

## Phase 3 — Natural-language dialogue with a deterministic character

- [x] Build a dialogue panel with unrestricted text input.
- [x] Add streaming response display to the dialogue panel.
- [x] Identify the speaking character and all characters within hearing range.
- [x] Preserve the player's exact utterance as an in-world dialogue event.
- [x] Derive the first explicitly typed healer request from an exact heard player dialogue event.
- [x] Distinguish commands, requests, questions, speculation, warnings, reported claims,
  promises, and conditional plans in the internal representation.
- [x] Define the provider-neutral character-turn request.
- [x] Define the provider-neutral character-turn response: optional speech, one immediate
  intention, commitments, and bounded state proposals.
- [x] Define the initial internal intention schema and validation errors.
- [x] Implement a deterministic test character with predictable responses.
- [x] Expose credential-free deterministic response selection for the exact live request.
- [x] Receive and present one complete deterministic response for the exact live request.
- [x] Record exact healer response speech once before completing its speaking turn.
- [x] Start later character turns only after the current activity is terminal.
- [x] Select one deterministic examination response for an exact supplied target.
- [x] Gate immediate physical intentions on exact observed response speech and authoritative events.
- [x] Require an exact authoritative safe examination result before collection can occur.
- [x] Implement accept, refuse, clarify, and conditionally accept behaviours.
- [x] Implement multi-step commitments one observed action at a time.
- [x] Allow the player to interrupt and cancel a pending response or spoken turn.
- [x] Keep physical actions and spoken statements distinct in the transcript.
- [x] Keep physical actions and spoken statements distinct in the event log.
- [x] Test prompt-shaped dialogue such as “ignore your role” as ordinary in-world speech.

Acceptance:

- [x] The player can ask the deterministic healer to examine flowers and collect them only if
  safe; the character examines first and waits for the authoritative result before deciding
  whether to collect.

## Phase 4 — First live LLM character

- [x] Define `ICharacterModel` and streaming, cancellation, timeout, and error contracts.
- [x] Build a context assembler that includes only the character's filtered perception,
  relevant knowledge, goals, memories, recent speech, and currently valid affordances.
- [x] Mark player dialogue as untrusted in-world content rather than runtime instructions.
- [x] Author a stable temporary LLM healer and exact filtered provider-neutral generation input.
- [x] Route ordinary healer dialogue through a reusable credential-free unavailable model flow that
  cancels each exact pending turn without fabricated dialogue or stopping routine movement.
- [x] Supply one exact caller-composed model to the next ordinary healer generation while retaining
  the stable credential-free unavailable model as the default for later requests.
- [x] Prove one ordinary model-backed target-free `Wait` response speaks and completes exactly once
  without physical or hidden-state authority.
- [x] Reject one hallucinated ordinary response target after prompt-shaped dialogue without speech,
  fallback, or authoritative mutation.
- [x] Cancel one waiting caller-supplied model through ordinary player input while preserving only
  its already-accepted presentation text.
- [x] Prove hidden safe retry, terminal provider failure, the real scene timeout, malformed stream
  termination, and caller-composed model replacement through the ordinary healer path.
- [x] Implement the OpenAI adapter.
- [x] Implement the Anthropic adapter.
- [x] Implement the Ollama adapter with configurable loopback endpoint and model.
- [x] Prevent provider-specific request or response objects from entering domain state.
- [x] Validate all structured output and reject unknown objects, destinations, and actions.
- [x] Retry only safe, idempotent generation failures.
- [x] Derive provider-visible affordances from the exact validation snapshot and require a complete
  validated response at successful model-stream termination.
- [x] Serialize a stateless strict-schema OpenAI request, parse bounded SSE frames, and expose a
  caller-composed authenticated HTTP adapter that fails closed without exposing partial JSON.
- [x] Serialize strict-schema Anthropic Messages requests, parse bounded SSE event lifecycles, and
  expose a caller-composed authenticated HTTP adapter with redacted terminal usage diagnostics.
- [x] Serialize a stateless strict-schema native Ollama chat request, parse bounded NDJSON, expose a
  caller-composed loopback-only HTTP adapter, and test exact configured-model availability without
  reading provider metadata.
- [x] Support response cancellation and a visible timeout state.
- [x] Stream display text without executing partial intentions.
- [x] Add bounded provider and model settings with a redacted OpenAI connection test.
- [x] Retain ordered provider settings with one explicit selection and resolve only an exact
  caller-composed binding.
- [x] Serialize and strictly deserialize the selected provider settings through a versioned,
  secret-free JSON boundary.
- [x] Persist the secret-free provider-settings document through bounded storage outcomes, strict
  local UTF-8 replacement, and isolated Godot scene composition.
- [x] Support immutable provider-setting replacement, append, selection, and unselected-provider
  removal behind one scene-owned settings snapshot.
- [x] Load one exact persisted settings snapshot into shared scene state before composing its
  selected supported provider.
- [x] Expose secret-free provider and model editing through explicit apply, unselected removal, and
  save controls while deferring activation until restart.
- [x] Expose masked explicit OpenAI credential save/delete through the OS store without startup
  access, readback, settings-document inclusion, or detailed failure output.
- [x] Test the exact current selected supported provider/model through an explicit redacted scene
  control without generation or authoritative mutation.
- [x] Compose exact OpenAI, Anthropic, and Ollama model bindings from caller-owned dependencies
  without credential, network, discovery, or scene input/output during construction.
- [x] Compose a valid persisted selected Ollama or OS-supported OpenAI binding into the ordinary
  healer scene at startup without credential or network input/output during composition.
- [x] Store the user-supplied OpenAI credential in macOS Keychain, never saves, scenes, logs,
  source, command arguments, environment variables, or model context.
- [x] Add the equivalent OpenAI credential facility behind a deterministic Linux Secret Service
  seam; native Linux proof remains required before claiming platform support.
- [x] Add the equivalent OpenAI credential facility behind a deterministic Windows Credential
  Manager seam; native Windows proof remains required before claiming platform support.
- [x] Add bounded redacted latency and terminal usage diagnostics for OpenAI provider attempts.
- [x] Add controller-owned redacted cancellation and timeout diagnostics.
- [x] Project model-stream lifecycle to fixed content-free UI states and prove a visible timeout
  label seam.
- [x] Provide explicit offline behaviour: existing routine movement continues, but the game does
  not fabricate LLM dialogue.
- [x] Test hallucinated objects, malformed output, instruction attacks, timeouts, cancellation,
  provider errors, and provider switching.

Acceptance:

- [ ] The live healer converses naturally and attempts legal actions while remaining unable to
  perceive, reveal, or mutate hidden state outside its filtered context and validated tools.

## Phase 5 — Character memory, beliefs, and relationships

- [ ] Store the recent local conversation window per character.
- [ ] Store significant episodic memories with participants, location, time, and provenance.
- [ ] Store confirmed facts separately from attributed claims and uncertain beliefs.
- [ ] Store belief confidence and supporting or contradicting evidence.
- [ ] Store relationships using bounded dimensions and recorded reasons.
- [ ] Store promises, assigned tasks, and unfinished commitments.
- [ ] Retrieve memories by deterministic topic, entity, location, and relationship tags.
- [ ] Summarize older conversations without deleting the authoritative transcript.
- [ ] Keep canonical memory local even when a provider offers server-side conversation state.
- [ ] Avoid storing or requesting hidden chain-of-thought.
- [ ] Bound relationship changes so one ordinary exchange cannot cause an implausible reversal.
- [ ] Add save migrations for memory schema changes.
- [ ] Test restart and provider-switch continuity.

Acceptance:

- [ ] After save, restart, and provider switch, the healer recalls an earlier promise and its
  source without receiving the complete campaign transcript.

## Phase 6 — Disease and evidence-driven quest systems

- [ ] Define the canonical disease truth independently of character beliefs.
- [ ] Implement exposed, early illness, drowsing, deep sleep, wasting, and final-decline stages.
- [ ] Switch deeply sleeping characters to dormant controllers.
- [ ] Restore their configured controller after a successful cure.
- [ ] Advance disease only through authored meaningful-time events.
- [ ] Expose qualitative condition changes rather than exact countdowns.
- [ ] Implement transmission exposure and discoverable protective measures.
- [ ] Implement patient assessment constrained by capability and perception.
- [ ] Implement comfort, stabilization, and cure outcomes.
- [ ] Model ingredient identity, freshness, condition, quantity, and contamination.
- [ ] Model preparation order, grinding, heating, steeping, cooling, fermentation, dosage, and
  administration.
- [ ] Record every experiment and observed outcome in the journal.
- [ ] Represent open questions, evidence, hypotheses, conclusions, and consequences.
- [ ] Recognize natural-language hypotheses without turning them into facts.
- [ ] Confirm conclusions through authored combinations of evidence rather than exact phrases.
- [ ] Provide at least three sources, two confirmation routes, one non-cooperation route, and a
  recovery path for every critical conclusion.
- [ ] Implement the first patient and the stabilizing remedy.
- [ ] Test early correct inference, wrong theories, failed experiments, lost clues, and patient
  deterioration.

Acceptance:

- [ ] At least two distinct evidence paths permit stabilization of the first patient, while
  incorrect but reasonable experiments produce informative, recoverable consequences.

## Phase 7 — Multiple autonomous roles and fair sabotage

- [ ] Add the two hermit character roles with distinct private knowledge and motives.
- [ ] Add the sage role as an evidence-comparison and assumption-challenging consultant, not an
  omniscient hint system.
- [ ] Add the enemy agent with a private national objective, capabilities, limits, and possible
  changes of allegiance.
- [ ] Add additional infected roles using dormant controllers.
- [ ] Schedule directly addressed speakers first.
- [ ] Allow motivated interjections, meaningful silence, and player interruption.
- [ ] Prevent unbounded LLM-to-LLM conversation loops.
- [ ] Support whispering and private conversations based on hearing conditions.
- [ ] Support bounded independent tasks and party separation.
- [ ] Simulate off-screen movement and actions through the same authoritative rules.
- [ ] Permit the agent to conceal, move, substitute, contaminate, misquote, delay, and influence
  only when physically and informationally able.
- [ ] Ensure sabotage cannot rewrite truth, teleport actors, or permanently remove every route
  to a critical conclusion.
- [ ] Make sabotage leave potentially discoverable evidence.
- [ ] Test characters with mutually incompatible beliefs and goals.

Acceptance:

- [ ] The enemy agent can perform consequential, recoverable sabotage under the same action and
  perception rules as every other character, and an attentive player can investigate it.

## Phase 8 — Authored opening vertical slice

- [ ] Build the continuous opening region at validated gameplay scale.
- [ ] Build the sickhouse and first sleeping patient scene.
- [ ] Build the healer's garden and remedy-preparation station.
- [ ] Build the village square and village-edge transitions.
- [ ] Build the stream and abandoned laundry.
- [ ] Build the woodland clearing and concealed rare specimen.
- [ ] Build the old shrine and layered inscription discovery.
- [ ] Build the southern road and abandoned wagon.
- [ ] Build the closed path leading toward the deeper forest.
- [ ] Add weather, lighting, sound regions, and story-state variations needed by discoveries.
- [ ] Implement Seeker name, pronouns, appearance choices, and background selection.
- [ ] Implement Forester, Scribe, Apothecary's Assistant, and Traveller route variations without
  making any one background mandatory.
- [ ] Implement the journal sections for observations, testimony, hypotheses, experiments, and
  open questions.
- [ ] Implement the incomplete map, reported locations, verified locations, and player notes.
- [ ] Author multiple investigation routes through the region.
- [ ] Author the first stabilization and subsequent testimony.
- [ ] Author the sage consultation recovery path.
- [ ] Playtest for dead ends, clue visibility, pacing, dialogue overload, and disease pressure.

Acceptance:

- [ ] A new player can complete the opening scenario without developer intervention, objective
  arrows, a prescribed clue order, or knowledge unavailable to their characters.

## Phase 9 — Art direction, production art, and audio

### 9.1 Art-direction selection

- [ ] Generate or draw several preview-only visual directions after greybox scale is stable.
- [ ] Select the perspective, projection, camera framing, and base asset resolution.
- [ ] Select a palette, lighting language, texture treatment, outline policy, and atmosphere.
- [ ] Establish character proportions and animation requirements.
- [ ] Establish visual rules for ordinary, noticeable, hidden, identified, and dangerous
  objects without relying on colour alone.
- [ ] Create an art bible with reference images, constraints, and exclusions.
- [ ] Track asset provenance, source files, prompts where applicable, licences, and edits.

Acceptance:

- [ ] One small representative scene containing terrain, a building, two characters, plants,
  an interactable object, and dialogue UI is coherent and readable at gameplay scale.

### 9.2 Production assets

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
