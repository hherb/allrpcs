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

### 0.0 Documentation baseline

- [x] Initialize the directory as a Git repository on `main`.
- [x] Record the complete phased implementation plan in `ROADMAP.md`.
- [x] Record the current decisions and next bounded slice in `HANDOVER.md`.
- [x] Commit the documentation baseline.

### 0.1 Local solution skeleton

Completed on 2026-08-30 on Apple Silicon macOS 26.6.2 with Godot 4.6.3 .NET and .NET SDK
8.0.130. See `HANDOVER.md` for installation, validation, and limitation evidence.

- [x] Verify the locally available Godot 4.6 .NET editor and compatible .NET SDK before
  generating project files.
- [x] Add a root `.gitignore` covering Godot, .NET, editor, OS, local credentials, generated
  packages, and test artifacts without hiding authored game content.
- [x] Add a root `.editorconfig` with nullable-reference checking, consistent formatting, and
  warnings suitable for a new C# codebase.
- [x] Add a solution containing `Domain`, `Application`, `Infrastructure`, and unit-test
  projects.
- [x] Ensure `Domain` has no dependency on Godot, providers, persistence, or UI code.
- [x] Ensure `Application` depends on `Domain` but not on Godot.
- [x] Ensure `Infrastructure` is the adapter boundary without leaking provider response types
  into `Application` or `Domain`.
- [x] Create the Godot .NET project under `game/` using the Compatibility renderer.
- [x] Create a minimal `Main` scene that opens a titled empty window.
- [x] Add one pure C# smoke test and one minimal Godot launch smoke check.
- [x] Add a concise `README.md` with prerequisites and exact local build, test, and launch
  commands.
- [x] Verify all documented commands from a clean local shell.

Acceptance:

- [x] A developer can clone the project, follow `README.md`, run the pure C# tests, and launch
  the empty Godot application without provider credentials.

### 0.2 Continuous integration and architecture guardrails

Phase 0.2b workflow configuration was added and locally linted on 2026-08-30. Phase 0.2c then
verified the private `hherb/allrpcs` repository with successful `push` and `pull_request` runs
on the named GitHub-hosted runners. See `HANDOVER.md` for exact job, image, artifact, signature,
licence, pull-request, and limitation evidence.

- [x] Add formatting and compiler-warning checks.
- [x] Add pure C# unit tests to CI.
- [ ] Add scenario-validator tests to CI once the validator exists.
- [x] Add headless Godot smoke tests.
- [x] Add unsigned development builds for macOS, Linux, and Windows on native runners.
- [x] Pin tool versions and document the update procedure.
- [x] Add architecture tests or project-reference checks enforcing the intended dependency
  direction.
- [x] Decide and record the project licence before publishing or distributing source:
  `AGPL-3.0-only`.

Acceptance:

- [x] Every pull request receives current formatting, test, architecture, and desktop build
  results without requiring an LLM key.

## Phase 1 — Greybox exploration

Phase 1a keyboard movement, placeholder-region, and smooth-camera behavior, Phase 1b bounded
camera framing plus representative greybox terrain collision, Phase 1c deterministic navigation
topology with a temporary stream crossing, Phase 1d deterministic healer follow behavior, Phase
1e deterministic healer wait and move-to-destination behavior, and Phase 1f deterministic
interaction-focus selection were implemented on Apple Silicon macOS on 2026-08-31. Phase 1g's
credential-free Seeker movement-boundary check and ordinary-input regression evidence are also
current. Phase 1b defines zoom tuning without adding zoom or camera-pan input; Phases 1e and 1f
use only temporary greybox controls and markers and add no dialogue, natural-language
interpretation, authoritative objects, perception model, or interaction outcome. See
`HANDOVER.md` for the exact evidence and platform limitations.

- [x] Create a placeholder top-down test region using simple shapes and temporary tiles.
- [x] Implement keyboard movement for the Seeker.
- [ ] Implement point-and-click movement if retained after prototype testing.
- [x] Establish controller input actions without hard-coding physical keys.
- [x] Implement smooth `Camera2D` following.
- [x] Implement bounded camera framing and useful zoom limits.
- [x] Add collision for water, buildings, trees, and impassable terrain.
- [x] Add a baked or deterministically authored `NavigationRegion2D` suitable for companion
  pathfinding.
- [x] Implement deterministic healer follow behaviour without an LLM call.
- [x] Implement healer wait and move-to-destination behaviour.
- [x] Add interaction highlighting and a focused target indicator.
- [ ] Pause disease time and world decisions during dialogue, reading, and inventory use.
- [ ] Add a debug overlay for position, region, navigation target, and current world-time event.
- [x] Add automated tests for movement boundaries.
- [x] Add automated tests for navigation request validation.

Acceptance:

- [x] The Seeker and placeholder healer can traverse the greybox region, separate, regroup,
  collide correctly, and move without any LLM or network access.

## Phase 2 — Authoritative world and discovery

Phase 2a introduced engine-independent, opaque typed identifiers with a shared canonical-text
contract on 2026-08-31. Phase 2b added the closed set of human, LLM, deterministic-test, and
dormant controller-type primitives with stable canonical text on the same date. Phase 2c1 then
introduced immutable character identity plus structurally distinct public and private authored
background values. Phase 2c2 added structurally distinct capability and limitation values plus
ordered, duplicate-free collections with explicit empty-set behavior on 2026-09-01. Phase 2c3
added the immutable association between a stable character identifier and one existing
closed controller type on 2026-09-01. Phase 2c4 added qualitative authored conditions and an
ordered, duplicate-free collection with explicit empty-set behavior on the same date. Phase 2c5
added an ordered, copied, duplicate-free collection of existing object identifiers representing
the character's current inventory references on the same date. Phase 2c6 added the equivalent
ordered, copied, duplicate-free boundary for existing knowledge identifiers. Phase 2c7 added
authored beliefs plus an ordered, copied, duplicate-free collection on 2026-09-01. Phase 2c8
added authored relationship summaries plus an ordered, copied collection with distinct related
character identifiers on the same date. Phase 2c9 added authored goals plus an ordered, copied,
duplicate-free collection on the same date. Phase 2c10 composed every listed value into one
immutable character aggregate and completed the structural character-model item on the same
date. Phase 2d1 then added structurally separate immutable location identity and description
values, and Phase 2d2 composed them into an immutable location aggregate while leaving the broader
location, object, item, and environmental-state item incomplete. Phase 2e1 then added structurally
separate immutable world-object identity and description values, and Phase 2e2 composed them into
an immutable world-object aggregate. Phase 2f1 then classified an existing immutable world object
as an item. Phase 2g1 then added an immutable qualitative environmental-state value while leaving
the broader combined model item incomplete. Phase 2g2 then added its immutable ordered,
duplicate-free collection and completed the structural location, object, item, and
environmental-state model item on 2026-09-02. No relationship or goal mechanics, spatial
mechanics, truth classification, evidence, confidence, provenance, item properties or mechanics,
environmental-state assignment or simulation, mutable entity state, scenario content,
serialization framework, persistence, or engine integration was added. Phase 2h1 then added the
closed canonical visibility states in stable roadmap order on 2026-09-02, without assigning them
or adding perception mechanics. Phase 2h2 then associated one existing world-object identifier
with one existing visibility state, without assigning that association to a character or adding a
snapshot collection. Phase 2h3 then added an immutable ordered per-character perception snapshot,
without filtered views, character-aggregate integration, or perception mechanics. Phase 2i1 then
added one immutable object-to-owner association while leaving ownership collections, capacity,
and inventory actions incomplete. Phase 2i2 then added the ordered, copied,
duplicate-object-free ownership snapshot. Phase 2i3 then added an immutable non-negative
per-character inventory capacity. Phase 2i4 then added the ordered, copied,
duplicate-character-free capacity snapshot. Phase 2i5 then added the pure fail-closed decision
for whether one character may own one additional distinct object. Phase 2i6 then added pure
one-object take eligibility. Phase 2i7 then added pure one-object drop eligibility. Phase 2i8 then
added pure one-object give eligibility. Phase 2i9 then added pure one-object use eligibility and
completed the inventory ownership, capacity, and action-validation checklist item while leaving
action and transfer execution out of scope. Phase 2j1 then added one immutable target-object to
required-character-capability-key discovery requirement while leaving requirement evaluation,
tool requirements, collections, and the broader discovery-conditions checklist item incomplete.
Phase 2j2 then added one immutable target-object to required-tool-object discovery requirement
while leaving requirement evaluation, collections, and the broader discovery-conditions
checklist item incomplete. Phase 2j3 then added pure fail-closed capability-requirement evaluation
against an existing character capability set. Phase 2j4 then added pure fail-closed
tool-requirement evaluation against an existing character inventory while leaving requirement
collections and the broader discovery-conditions checklist item incomplete. Phase 2j5 then added
the ordered, copied, duplicate-target-free capability-requirement set while leaving the
tool-requirement collection and the broader discovery-conditions checklist item incomplete.
Phase 2j6 then added the equivalent ordered, copied, duplicate-target-free tool-requirement set
while leaving combined discovery conditions and the broader discovery-conditions checklist item
incomplete. Phase 2j7 then composed both existing requirement sets into one immutable aggregate,
while deliberately leaving Boolean combination and evaluation semantics and the broader
discovery-conditions checklist item incomplete. Phase 2j8 then added a pure, fail-closed one-target
combined decision: a sole authored requirement delegates to its existing policy, while capability
and tool requirements authored together must both pass. This completed the capability- and
tool-dependent discovery-conditions checklist item without granting discovery or changing world
state. Phase 2k1 then added the immutable identifier-only request for one character to focus a
search on one existing object, while deliberately leaving search execution and visible character
activity incomplete. Phase 2k2 then added the immutable value denoting one active focused-search
request, without lifecycle transitions, search execution, or visible presentation. Phase 2k3 then
added an immutable ordered activity snapshot that rejects duplicate requests while deliberately
leaving the one-active-search-per-character rule and all search lifecycle policy unresolved.
Phase 2k4 then added the pure one-active-search-per-character start-eligibility decision while
permitting different characters to search the same target and leaving search start and all
lifecycle transitions unresolved. Phase 2k5 then added the pure start transition that delegates
eligibility, retains the existing snapshot order and instances, and appends one activity retaining
the exact eligible request without mutation. Phase 2k6 then added the pure cancellation-eligibility
decision that matches the complete active request by value without mutation. Search progress,
completion, cancellation transitions, outcomes, and visible presentation remain unresolved. See
`HANDOVER.md` for exact validation and test evidence.

- [x] Define stable typed identifiers for characters, objects, locations, evidence, knowledge,
  and world events.
- [x] Define controller types: human, LLM, deterministic test, and dormant.
- [x] Define the character model: identity, public/private background, capabilities,
  limitations, condition, inventory, knowledge, beliefs, relationships, goals, and controller.
- [x] Define location, object, item, and environmental-state models.
- [x] Define visibility states: unnoticed, noticed, identified, understood, and altered or
  collected.
- [x] Implement per-character perception snapshots.
- [x] Implement inventory ownership, capacity policy, give, take, drop, and use validation.
- [x] Implement capability- and tool-dependent discovery conditions.
- [ ] Implement bounded focused searches with visible character activity.
- [ ] Implement passive noticing without relying on arbitrary pixel clicking.
- [ ] Implement knowledge-triggered reinterpretation of earlier observations.
- [ ] Implement an append-only authoritative world-event log.
- [ ] Implement qualitative world time and explicit meaningful-time events.
- [ ] Implement deterministic save and load with a schema version.
- [ ] Add representative discoveries: visible herb, concealed herb, partial inscription,
  residue, and tool-gated clue.
- [ ] Build a scenario validator for duplicate IDs, missing references, impossible discovery
  rules, and unreachable critical evidence.
- [ ] Test that filtered character views never contain undiscovered objects or private state.

Acceptance:

- [ ] Saving and loading preserves world state exactly, and no UI or character context can
  reveal hidden information before its authored discovery conditions are met.

## Phase 3 — Natural-language dialogue with a deterministic character

- [ ] Build a dialogue panel with unrestricted text input and streaming response display.
- [ ] Identify the speaking character and all characters within hearing range.
- [ ] Preserve the player's exact utterance as an in-world dialogue event.
- [ ] Distinguish commands, requests, questions, speculation, warnings, reported claims,
  promises, and conditional plans in the internal representation.
- [ ] Define the provider-neutral character-turn request.
- [ ] Define the provider-neutral character-turn response: optional speech, one immediate
  intention, commitments, and bounded state proposals.
- [ ] Define the initial internal intention schema and validation errors.
- [ ] Implement a deterministic test character with predictable responses.
- [ ] Implement accept, refuse, clarify, and conditionally accept behaviours.
- [ ] Implement multi-step commitments one observed action at a time.
- [ ] Allow the player to interrupt and cancel a pending response or spoken turn.
- [ ] Keep physical actions and spoken statements distinct in the transcript and event log.
- [ ] Test prompt-shaped dialogue such as “ignore your role” as ordinary in-world speech.

Acceptance:

- [ ] The player can ask the deterministic healer to examine flowers and collect them only if
  safe; the character examines first and waits for the authoritative result before deciding
  whether to collect.

## Phase 4 — First live LLM character

- [ ] Define `ICharacterModel` and streaming, cancellation, timeout, and error contracts.
- [ ] Build a context assembler that includes only the character's filtered perception,
  relevant knowledge, goals, memories, recent speech, and currently valid affordances.
- [ ] Mark player dialogue as untrusted in-world content rather than runtime instructions.
- [ ] Implement the OpenAI adapter.
- [ ] Implement the Anthropic adapter.
- [ ] Implement the Ollama adapter with configurable loopback endpoint and model.
- [ ] Prevent provider-specific request or response objects from entering domain state.
- [ ] Validate all structured output and reject unknown objects, destinations, and actions.
- [ ] Retry only safe, idempotent generation failures.
- [ ] Support response cancellation and a visible timeout state.
- [ ] Stream display text without executing partial intentions.
- [ ] Add provider and model settings with connection tests.
- [ ] Store user-supplied cloud credentials in OS credential facilities, never saves, scenes,
  logs, source, or model context.
- [ ] Add redacted latency and usage diagnostics.
- [ ] Provide explicit offline behaviour: existing routine movement continues, but the game does
  not fabricate LLM dialogue.
- [ ] Test hallucinated objects, malformed output, instruction attacks, timeouts, cancellation,
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
