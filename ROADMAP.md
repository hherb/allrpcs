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
- [x] Serialize stateless strict-schema OpenAI Responses requests plus deterministic reasoning-free
  loopback Chat Completions requests, parse bounded provider envelopes, and expose a caller-composed
  authenticated HTTP adapter that fails closed without exposing partial JSON or reasoning.
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
  save controls.
- [x] Activate the exact current supported binding explicitly for future turns without saving,
  interrupting an active turn, or replacing its already-taken model.
- [x] Expose masked explicit OpenAI credential save/delete through the OS store without startup
  access, readback, settings-document inclusion, or detailed failure output.
- [x] Test the exact current selected supported provider/model through an explicit redacted scene
  control without generation or authoritative mutation.
- [x] Compose exact OpenAI, Anthropic, and Ollama model bindings from caller-owned dependencies
  without credential, network, discovery, or scene input/output during construction.
- [x] Allow caller-composed OpenAI-compatible HTTPS or numeric-loopback version-one API bases while
  keeping the official OpenAI endpoint as the default.
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
- [x] Separate permissive fail-closed live transport safety from fixed three-attempt OpenAI-compatible
  conformance checks that require completed safe waits and report only bounded outcome counts.
- [x] Require a separate fixed live action series to complete with a known currently afforded target
  and pass the existing provider-neutral response validation policy.
- [x] Apply the same fixed wait and known-target live-conformance series to an explicitly selected
  already-installed Ollama model without changing the permissive safety probe or standard suite.
- [x] Add an explicitly enabled, content-free Ollama or OpenAI-compatible harness for carrying one
  live known-target response through the ordinary healer scene and its existing validated action
  boundary; live execution remains separate evidence.
- [x] Add the equivalent ordinary-scene harness for target-free live speech while proving no
  physical, event, time, world-object, or perception mutation; live execution remains separate.
- [x] Expose the known herb-patch examination affordance to ordinary healer generation and carry a
  validated distinct value-equal provider target through authoritative examination and safe
  collection without changing world objects or perception.

Acceptance:

- [x] The live healer converses naturally and attempts legal actions while remaining unable to
  perceive, reveal, or mutate hidden state outside its filtered context and validated tools.

## Phase 5 — Character memory, beliefs, and relationships

- [x] Store the recent local conversation window per character.
- [x] Store significant episodic memories with participants, location, time, and provenance.
- [x] Store confirmed facts separately from attributed claims and uncertain beliefs.
- [x] Store belief confidence and supporting or contradicting evidence.
- [x] Store relationships using bounded dimensions and recorded reasons.
- [x] Store promises, assigned tasks, and unfinished commitments.
- [x] Retrieve memories by deterministic topic, entity, location, and relationship tags.
- [x] Summarize older conversations without deleting the authoritative transcript.
- [x] Keep canonical memory local even when a provider offers server-side conversation state.
- [x] Avoid storing or requesting hidden chain-of-thought.
- [x] Bound relationship changes so one ordinary exchange cannot cause an implausible reversal.
- [x] Compose bounded all-character memory into the version-2 save-root prerequisite.
- [x] Define the pure version-1 to version-2 empty-memory migration without enabling loading.
- [x] Serialize and strictly deserialize complete canonical version-2 character memory.
- [x] Execute memory-schema migration during load and restore character memory atomically.
- [x] Test restart and provider-switch continuity.

Acceptance:

- [x] After save, restart, and provider switch, the healer recalls an earlier promise and its
  source without receiving the complete campaign transcript.

## Phase 6 — Disease and evidence-driven quest systems

- [x] Define the canonical disease truth independently of character beliefs.
- [x] Implement exposed, early illness, drowsing, deep sleep, wasting, and final-decline stages.
- [x] Switch deeply sleeping characters to dormant controllers.
- [x] Restore their configured controller after a successful cure.
- [x] Advance disease only through authored meaningful-time events.
- [x] Expose qualitative condition changes rather than exact countdowns.
- [x] Implement transmission exposure and discoverable protective measures.
- [x] Implement patient assessment constrained by capability and perception.
- [x] Implement comfort, stabilization, and cure outcomes.
- [x] Model ingredient identity, freshness, condition, quantity, and contamination.
- [x] Model preparation order, grinding, heating, steeping, cooling, fermentation, dosage, and
  administration.
- [x] Record every experiment and observed outcome in the journal.
- [x] Represent open questions, evidence, hypotheses, conclusions, and consequences.
- [x] Recognize natural-language hypotheses without turning them into facts.
- [x] Confirm conclusions through authored combinations of evidence rather than exact phrases.
- [x] Provide at least three sources, two confirmation routes, one non-cooperation route, and a
  recovery path for every critical conclusion.
- [x] Implement the first patient and the stabilizing remedy.
- [x] Test early correct inference, wrong theories, failed experiments, lost clues, and patient
  deterioration.
- [x] Compose and capture immutable first-patient state without changing unrelated scene state.
- [x] Define the version-3 in-memory save root and explicit version-2 migration prerequisite.
- [x] Serialize and strictly deserialize complete canonical version-3 first-patient saves.
- [x] Execute migration and atomically restore complete version-3 saves.

Acceptance:

- [x] At least two distinct evidence paths permit stabilization of the first patient, while
  incorrect but reasonable experiments produce informative, recoverable consequences.

## Phase 7 — Multiple autonomous roles and fair sabotage

- [x] Add the two hermit character roles with distinct private knowledge and motives.
- [x] Add the sage role as an evidence-comparison and assumption-challenging consultant, not an
  omniscient hint system.
- [x] Add the enemy agent with a private national objective, capabilities, limits, and possible
  changes of allegiance.
- [x] Add additional infected roles using dormant controllers.
- [x] Schedule directly addressed speakers first.
- [x] Allow motivated interjections, meaningful silence, and player interruption.
  - [x] Define, validate, and deterministically select provider-neutral participation decisions.
  - [x] Validate one complete bounded scheduled participation round before selecting its speaker.
  - [x] Derive and scene-start an exact heard-event follow-up from a complete selected round.
  - [x] Interrupt an initialized autonomous chain through ordinary player cancellation input.
  - [x] Define an immutable provider-neutral participation-consideration request for each scheduled
    listener.
  - [x] Validate a participation decision against the exact consideration request and authored
    character state.
  - [x] Preflight and create a complete bounded round of stable consideration requests.
  - [x] Bind each accepted participation decision to its exact consideration request.
  - [x] Retain one complete bounded result round with exact request and event alignment.
  - [x] Define one provider-neutral cancellation-aware participation decision interface.
  - [x] Obtain and validate one complete participation decision round through that interface.
  - [x] Budget-gate and scene-start the exact selected result from a complete consideration round.
  - [x] Strictly decode participation decision JSON against exact character state and addressing.
  - [x] Serialize one bounded strict OpenAI Responses participation decision request.
  - [x] Complete one strict bounded OpenAI Responses participation decision SSE stream.
  - [x] Expose one caller-composed authenticated OpenAI participation decision adapter.
  - [x] Bound one complete sequential participation-decision round with a shared deadline.
  - [x] Own one bounded participation-round task through closed scene terminal states.
  - [x] Supply one exact one-shot participation model with fail-closed offline behavior.
  - [x] Bind and select exact participation models through canonical provider settings.
  - [x] Compose the selected OpenAI participation model without construction-time input/output.
  - [x] Scene-own configured participation bindings with safe replacement and offline clearing.
  - [x] Coordinate one eligible directly addressed ordinary player event through a bounded
    participation round and the existing budget-gated follow-up-start seam.
  - [x] Obtain participation decisions and drive the selected follow-up in the ordinary dialogue
    flow.
  - [x] Preflight and coordinate a complete stable ordinary listener schedule and direct-address set.
  - [x] Resolve the exact bounded LLM participant for an awaiting selected character turn.
  - [x] Scene-own an exact healer-first ordinary roster and include its participants in hearing.
  - [x] Generate and record one exact selected direct response or motivated sage interjection.
- [x] Prevent unbounded LLM-to-LLM conversation loops.
  - [x] Define and scene-own bounded completed-turn and monotonic-interruption state.
  - [x] Gate complete-round and complete-result autonomous selection on the exact conversation
    budget.
  - [x] Gate exact selected follow-up starts and atomically complete turns with one budget advance.
  - [x] Close an initialized ordinary completion/cancellation path at its bound or interruption.
  - [x] Install a fresh finite budget for each eligible ordinary player-rooted autonomous chain.
  - [x] Initialize and drive every ordinary autonomous chain through these bounded seams.
  - [x] Start only heard LLM listeners after exact character speech and stop at the finite bound.
- [x] Support whispering and private conversations based on hearing conditions.
  - [x] Define explicit provider-neutral ordinary-speech and whispering delivery values.
  - [x] Select ordinary or whisper listeners through explicit bounded hearing ranges.
  - [x] Retain exact delivery on dialogue events and apply it at scene occurrence.
  - [x] Carry explicit player-authored whisper intent through ordinary scene occurrence.
  - [x] Retain exact delivery on spoken transcript entries and scene transcript append.
  - [x] Retain exact delivery mode on recent and current dialogue model context.
  - [x] Serialize exact dialogue delivery modes into shared trusted model-request context.
  - [x] Retain an optional exact response delivery and strictly decode provider-selected modes.
  - [x] Apply exact response delivery to scene hearing and transcript recording.
  - [x] Persist exact heard and response delivery modes through the version-4 save boundary.
- [x] Support bounded independent tasks and party separation.
  - [x] Define canonical bounded independent-task assignments and immutable lifecycle snapshots.
  - [x] Enforce closed forward task lifecycle transitions and stable per-character scheduling.
  - [x] Complete only an exact active task through an authoritative physical-action occurrence.
  - [x] Represent bounded party membership and atomically start a task while separating its member.
  - [x] Scene-own task and party state and drive one authored departure through existing rules.
  - [x] Complete one exact active authored task through authoritative event, time, and transcript
    state only.
  - [x] Rejoin one exact completed-task character through stable bounded party membership only after
    its exact return completes.
- [x] Simulate off-screen movement and actions through the same authoritative rules.
  - [x] Define one bounded provider-neutral travel plan and immutable progress transition for an
    exact active independent task without introducing Godot navigation or elapsed-time policy.
  - [x] Scene-own one exact independent-task travel plan and advance its immutable progress only
    from the exact expected current plan, progress, and next waypoint.
  - [x] Author and scene-initialize one exact active healer route from its departure position to
    the existing herb-patch task target without advancing or moving it.
  - [x] Require exact completed scene-owned travel before the independent-task physical occurrence
    can atomically change task, event, qualitative-time, or transcript state.
  - [x] Compose one pure final-waypoint arrival and authoritative task occurrence as an immutable
    all-or-nothing Application result without scene installation.
  - [x] Install one exact final-waypoint arrival and authoritative task occurrence atomically from
    compare-and-swapped scene-owned travel, task, event, time, and transcript state.
  - [x] Define bounded exact completed-task return travel and immutable one-waypoint progress.
  - [x] Scene-own and author the healer return route from its exact successful arrival back to its
    outbound origin without selecting timing or movement policy.
  - [x] Initialize that exact return route as part of the final arrival-occurrence scene operation.
  - [x] Compose and install exact final return progress plus stable bounded party rejoin atomically.
  - [x] Coordinate one explicit next outbound or return waypoint through those existing atomic scene
    operations without selecting when a step occurs.
  - [x] Select the active outbound or return leg only from one exact caller-supplied scene snapshot.
  - [x] Define a bounded provider-neutral cadence and immutable elapsed clock that exposes at most
    one due independent-task travel step per advance.
  - [x] Scene-own one exact task-step cadence and compare-and-swap immutable elapsed clock progress.
  - [x] Consume one due cadence interval only when the exact active outbound or return step succeeds.
  - [x] Drive the authored one-waypoint task occurrence and return rejoin from a bounded live-scene
    cadence without moving or hiding its character body.
  - [x] Hide the exact separated task character body and restore it only from the exact rejoined
    party snapshot without changing its retained scene position.
- [ ] Permit the agent to conceal, move, substitute, contaminate, misquote, delay, and influence
  only when physically and informationally able.
  - [x] Define bounded explicit per-character physical access to stable world-object identifiers.
  - [x] Require one targeted action's physical access, authored capability, and referenced knowledge
    together through a pure fail-closed eligibility gate.
  - [x] Define the closed conceal, move, substitute, contaminate, misquote, delay, and influence
    action-kind vocabulary without granting occurrence authority.
  - [x] Bind object-targeted sabotage proposals to exact actors and complete unchanged access
    requirements while rejecting non-object action kinds.
  - [x] Bind one eligible object sabotage proposal to its exact authoritative world-object target
    without applying an effect.
  - [x] Author one exact temporary enemy concealment candidate with explicit physical access,
    authoritative object state, empty concealment state, and fresh noticed evidence.
  - [x] Bind that authored candidate to the exact tail water cask in the existing complete scene
    world-object snapshot and initialize its empty state without applying it automatically.
- [ ] Ensure sabotage cannot rewrite truth, teleport actors, or permanently remove every route
  to a critical conclusion.
  - [x] Apply one eligible concealment as fresh bounded object state together with its exact fresh
    evidence, without mutating the authoritative object, its position, or any critical route.
  - [x] Recover only the evidence-bound concealed target after a character can see its exact
    authoritative sabotage evidence, preserving unrelated concealment and all source snapshots.
  - [x] Scene-own exact concealment and evidence snapshots and compare-and-swap eligible apply and
    recovery results without mutating authoritative inputs.
  - [x] Run one explicitly requested temporary concealment exactly once and recover it only through
    the existing perception-to-evidence projection without consuming evidence.
  - [x] Project exact concealment snapshots onto one scene body through stale-safe visibility state.
  - [x] Explicitly conceal and hide the main-scene water cask, then recover and restore it only from
    noticed exact evidence without changing its position or authoritative world object.
- [ ] Make sabotage leave potentially discoverable evidence.
  - [x] Require each eligible object-sabotage occurrence candidate to define one fresh exact
    target-bound discovery-evidence value without applying it.
  - [x] Append that exact evidence to a fresh stable snapshot only after complete occurrence and
    evidence eligibility succeeds.
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
