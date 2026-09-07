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

Phases 2a–2j established the checked engine-independent world, character, perception, inventory,
and discovery-condition foundations below. Phases 2k1–2k49 added the bounded focused-search
lifecycle, deterministic outcome recording and resolution, controller-owned immutable Seeker
perception, temporary F/C/E/R wiring, and exact target-only `IDENTIFIED` presentation. Phases
2l1–2l20 added an immutable world-object snapshot, exact identifier lookup, a temporary scene source
that independently authors the two visible world-object identities and distinct non-empty
descriptions into one stable snapshot, and separate one-time controller ownership with exact
value-equal lookup.
Temporary R-resolution is gated on the selected outcome target's existence. The R path passes the exact
looked-up object into the focused-search controller, which delegates complete world-object-bound
eligibility and state creation to a pure Domain policy and transition. The transition accepts
value-equal target identifiers, rejects every bound or existing resolution ineligibility, delegates
unchanged state creation to the existing resolution transition, and returns its exact result while
preserving every input and retained unrelated instance. Resolved-perception presentation also
receives only exact controller-looked-up world objects, uses their stable identifiers, and hides a
target marker when its object is missing. These paths preserve immutable state but do not make the
source or controller authoritative or grant discovery. Temporary outcome creation now also requires
the active request target to resolve to a matching existing world object, including through a
distinct value-equal identifier; a missing object leaves the active search and all surrounding state
unchanged. Initial temporary Seeker perception is now derived only from that world-object snapshot,
retaining its exact identifier instances and order without reading greybox targets. Temporary
focused-search condition targets are likewise derived only from the first two world objects in
snapshot order, retain those exact identifiers, and fail explicitly when both are unavailable
without changing existing requirement or inventory semantics. Focused-search request creation and
the F start path now also require the focused target to resolve to a matching existing world object,
accept distinct value-equal identifiers while retaining the supplied object's exact identifier, and
leave all surrounding state unchanged when the object is missing. Temporary interaction-focus
refresh now also resolves each otherwise eligible greybox marker through the world-object
controller, retains the selected object's exact identifier instance, falls back under the unchanged
distance and authored-order rules when the nearest object is missing, and clears focus presentation
when no object-backed marker remains. Its status now presents the exact looked-up object's authored
display name, including when a value-equal replacement changes the name without changing the
selected marker, and returns to `Interaction focus: NONE` when focus clears without inspecting the
object description. Every existing labelled marker now also presents the matching world-object
display name, including for a matching disabled marker; absent objects clear their labels, hidden
unlabelled markers remain unlabelled, and focus behavior remains unchanged. Marker labels are now
presentation-only: markers retain independently parsed value-equal object-reference IDs but no
longer author a separate display-name input. The source-owned descriptions retain exact stable
instances and are not focused-search inputs or part of marker labels, focus status, or focused-search
result presentation. Each resolved-perception indicator now reuses its existing label to present the
exact looked-up object's authored description only while the controller's retained perception has
that object at exactly `Identified`. Null or absent objects, null or unrelated perception, and every
other visibility singleton clear and hide the label. A distinct value-equal object supplies its own
replacement description without changing any supplied object, snapshot, lifecycle state, marker
presentation, or world-object ordering.
Phases 2m1–2m3 add one immutable identifier-only passive-noticing candidate for an existing observer
and target, an immutable snapshot of those candidates, and a pure observer-specific projection. The
snapshot copies caller input in stable supplied order, retains exact candidate instances, rejects
nulls and value-equal duplicates, and permits candidates that share only an observer or target. The
projection returns a new snapshot containing exactly the candidates for a value-equal observer,
preserving stable order and exact retained instances while omitting unrelated entries. These
boundaries do not produce, select, rank, reorder, or evaluate candidates, so passive noticing remains
incomplete. The exact next bounded slice is defined in `HANDOVER.md`.

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
- [x] Implement bounded focused searches with visible character activity.
- [x] Connect focused-search outcome resolution to authoritative world objects, perception
  change, and discovery presentation.
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
