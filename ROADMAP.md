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

Completed foundations:

- [x] Author the hermits, sage, enemy agent, and additional dormant infected roles with distinct
  bounded identity, knowledge, motives, goals, limitations, and controller assignments.
- [x] Schedule directly addressed speakers first while allowing motivated interjections, meaningful
  silence, explicit whisper delivery, player interruption, and finite LLM-to-LLM conversation.
- [x] Support bounded independent tasks, party separation, authoritative off-screen occurrence,
  finite cadence-based outbound and return travel, party rejoin, and stale-safe body presentation.
- [x] Define provider-neutral sabotage vocabulary, exact physical/informational eligibility,
  authoritative occurrence, fresh discoverable evidence, and immutable fail-closed scene ownership.
- [x] Complete the recoverable concealment chain through exact candidate selection, consideration,
  strict OpenAI decoding/SSE/adapter/binding, explicit main-scene presentation, and acknowledgement.
- [x] Complete the recoverable move chain through exact placement authority, candidate selection,
  consideration, strict OpenAI decoding/SSE/adapter/binding, explicit main-scene presentation,
  focused-search recovery, and acknowledgement.
- [x] Define, apply, present, and recover one exact evidence-bound water-cask substitution without
  replacing authoritative world-object truth, consuming evidence, or changing source positions.
- [x] Carry substitution consideration through exact request, selection-or-abstention decision,
  accepted result, atomic transition, scene resolution, bounded task ownership, one-shot/offline
  model supply, and explicit main-scene request composition.
- [x] Strictly decode one exact substitution decision and serialize one bounded stateless OpenAI
  Responses request with a closed schema and bounded trusted context.
- [x] Complete one strict bounded OpenAI Responses substitution-decision SSE lifecycle and expose
  one caller-composed authenticated adapter plus construction-only exact OpenAI model binding.
- [x] Own, replace, and clear the exact configured substitution binding through selected settings,
  then present and explicitly acknowledge each exact completed main-scene result at most once.
- [x] Define bounded immutable world-object contamination, exact evidence-bound application, and
  visible-evidence recovery without changing authoritative objects or consuming evidence.
- [x] Select and atomically apply the first exact eligible contamination candidate, then retain one
  bounded provider-neutral consideration request and strictly validate an exact selection or
  abstention without applying it.
- [x] Bind and apply one accepted exact contamination decision, then own its provider-neutral
  cancellation-aware decision task and stale-safe recoverable scene state without a model binding.
- [x] Strictly decode one exact contamination decision, serialize one bounded stateless OpenAI
  Responses request with a closed schema, and complete its bounded semantic-event/SSE lifecycle.
- [x] Expose one caller-composed authenticated contamination adapter and construction-only exact
  OpenAI model binding without construction-time input/output or model invocation.
- [x] Test representative sage/enemy incompatible beliefs and goals through complete bounded
  provider-neutral response rounds without judging or applying either response.

Current continuation:

- [x] Own and compose the exact contamination model binding, then run, present, and explicitly
  acknowledge one exact main-scene contamination consideration without automatic selection.
- [x] Recover exact presented water-cask contamination only through its ordinary resolved focused
  search and retained noticed evidence, without consuming evidence or changing world truth.
- [x] Define one exact heard-source misquotation proposal, strict delivered-dialogue occurrence,
  and non-actor witness seam without rewriting dialogue truth or automatically delivering text.
- [x] Select one exact observable misquotation-evidence candidate, retain a bounded provider-neutral
  consideration, bind only its exact actor's exact current selection or abstention, and obtain one
  validated result through a cancellation-aware provider-neutral model seam.
- [x] Strictly decode and stream one exact misquotation-evidence decision, serialize its bounded
  stateless OpenAI request and closed schema, and compose an exact authenticated model binding.
- [x] Supply configured, one-shot, or offline misquotation-evidence models, own one bounded
  cancellation-aware decision task, and start it only through one explicit isolated scene flow.
- [x] Author one stable exact observable misquotation-evidence candidate and compose it into an
  inert source-owned main-scene consideration flow without exposing or applying the evidence.
- [x] Retain bounded exact exposed misquotation evidence, append only one accepted exact selection,
  and own the exact exposure snapshot through a stale-safe scene controller without presentation.
- [x] Observe and explicitly acknowledge each exact completed misquotation-evidence result once,
  then compose exact empty exposure ownership into the inert source-owned main-scene flow.
- [x] Project bounded exposed misquotation evidence into one immutable exact-witness view without
  leaking entries to a distinct value-equal character identifier.
- [x] Own and refresh the exact witness view with successful main-scene evidence exposure while
  keeping ordinary startup, abstention, failure, and acknowledgement presentation-free.
- [x] Define one truth-neutral exact-member presentation selection and accept it only for the exact
  current witness view, with explicit abstention and stale-view rejection.
- [x] Own at most one explicit presentation selection per exact current view in the main scene while
  keeping ordinary startup and evidence consideration presentation-free.
- [x] Project one accepted exact selected misquotation-evidence item into an immutable truth-neutral
  source/delivered event pair while returning no presentation for abstention or a stale view.
- [x] Own each exact current presentation projection at most once while hiding stale projections
  and retaining abstention without UI or authoritative mutation.
- [x] Compose the inert presentation owner into the main scene while keeping ordinary startup,
  selection, abstention, acknowledgement, and offline failure presentation-free.
- [x] Orchestrate one explicit exact witness-view selection or abstention through the existing
  truth-neutral presentation owners without automatic choice or authoritative mutation.
- [x] Own that presentation flow inertly in the main scene and prove only switch-gated explicit
  abstention and selection after exact evidence exposure.
- [x] Explicitly acknowledge one exact current witness presentation through its isolated flow,
  hiding only that snapshot while retaining its view, selection, exposure, and source evidence.
- [x] Exercise that acknowledgement only in the switch-gated main-scene smoke after explicit
  presentation selection, leaving ordinary startup and every retained exact source inert.
- [x] Render one exact current witness presentation in isolated screen-space temporary UI under
  separate truth-neutral source and delivered statement labels, hiding stale or acknowledged data.
- [x] Compose that temporary UI inertly into the main scene and exercise it only through the
  switch-gated exact selection, presentation, and acknowledgement smoke path.
- [x] Dismiss one exact current temporary witness presentation through neutral explicit UI input,
  delegating only through the existing acknowledgement flow while preserving every source object.
- [x] Review one exact current witness evidence view explicitly as stable truth-neutral source and
  delivered statement pairs, with empty and stale views hidden and no automatic selection.
- [x] Select one exact reviewed evidence entry through the existing one-shot presentation owners,
  rejecting absent, stale, repeated, and out-of-range UI signals without authoritative mutation.
- [x] Close the observational evidence list explicitly without selecting, presenting,
  acknowledging, consuming, correcting, or persisting evidence.
- [x] Derive one explicit one-shot misquotation occurrence through the ordinary dialogue-event
  hearing seam, rejecting invalid actor, text, delivery mode, audience, query, and repeated input.
- [x] Own that occurrence coordinator inertly in the main scene and exercise it only through one
  switch-gated delivery without changing transcripts or any evidence flow.
- [x] Create one potential witness-evidence candidate only for the exact current occurrence,
  rejecting unavailable, stale, unheard, null, and repeated input without exposure or mutation.
- [x] Own that exact-current candidate controller inertly in the main scene and request one
  candidate only after the dedicated switch-gated occurrence without changing existing state.
- [x] Create one provider-free consideration request only for the exact current observable
  candidate, rejecting unavailable, stale, null, and repeated input without starting work.
- [x] Own that exact-current request controller inertly after its main-scene candidate dependency,
  retaining the exact actor and candidate without replacing the preauthored consideration flow.
- [x] Start one explicit decision only for the exact current live-candidate request while the shared
  task is idle, preserving a queued one-shot model across every rejected start.
- [x] Own that exact-request decision flow inertly in the main scene and exercise it only after the
  dedicated switch-gated occurrence, candidate, and request chain without exposing evidence.
- [x] Expose and explicitly acknowledge one exact completed live-candidate decision through the
  existing exposure and witness-view owners, with repeated and mismatched state failing closed.
- [x] Review, select, render, and dismiss only that exact live witness evidence through the existing
  truth-neutral UI owners while retaining every source, occurrence, request, and exposed entry.
- [x] Define bounded character-targeted ordinary-interaction, capability, and knowledge access with
  explicit self-target rejection, without yet defining a delay or influence proposal or effect.
- [x] Retain only access-bound, non-self `delay` and `influence` proposals and validate them through
  the unchanged character-target access policy without defining occurrence or effect semantics.
- [x] Bind one exact eligible `delay` or `influence` proposal to its exact retained target-character
  occurrence candidate without applying elapsed-time or relationship effects.
- [x] Bind one exact eligible `delay` occurrence to an exact authoritative advancing world event
  without recording that event or advancing qualitative world time.
- [x] Apply one exact eligible `delay` candidate through the immutable authoritative world-event
  occurrence transition while retaining the exact candidate and every rejected source.
- [x] Select the exact first eligible delay candidate in stable bounded caller order only after
  complete one-pass collection validation, without applying it.
- [x] Retain exact current actor, access, authoritative event, log, time, and bounded candidate state
  for one provider-neutral delay consideration without selecting, deciding, or applying it.
- [x] Accept only an exact-actor abstention or exact currently eligible request-member delay
  selection, then bind that decision immutably to its exact consideration request.
- [x] Apply an accepted exact delay selection through the existing immutable world-event transition
  while leaving an accepted abstention inert and every request source unchanged.
- [x] Obtain and validate one provider-neutral delay decision with exact request and caller
  cancellation forwarding, without binding a model or owning background work.
- [x] Retain exact OpenAI target-delay settings and model, then supply configured, one-shot, or
  stable offline models without construction-time I/O or automatic invocation.
- [x] Own one isolated bounded target-delay decision task and explicit consideration flow with
  exact result installation, overlap rejection, cancellation, reset, and fail-closed offline behavior.
- [x] Strictly decode one bounded provider target-delay decision into an accepted exact request
  selection or abstention without exposing malformed content or changing request world state.
- [x] Accumulate only one complete ordered OpenAI Responses event lifecycle and framed SSE stream
  into that exact decision without exposing partial, failed, refused, or trailing output.
- [x] Serialize one stateless streaming target-delay request with bounded trusted actor, event-log,
  world-time, and candidate context plus a closed exact decision schema.
- [x] Execute one target-delay request through caller-owned OpenAI credential, HTTP, settings, and
  endpoint dependencies, then create its exact binding without construction-time input/output.
- [x] Compose, replace, and clear that exact OpenAI binding explicitly through the existing
  temporary target-delay model source while preserving its stable offline fallback.
- [x] Activate the target-delay binding from selected OpenAI game settings and restore the exact
  offline model for Ollama without automatic decision work or authoritative mutation.
- [x] Own the target-delay model source, decision task, and explicit consideration flow inertly in
  the main scene, with one switch-gated exact-selection smoke and unchanged world state.
- [x] Own one exact authoritative target-delay event-log and world-time pair in an isolated scene
  controller, consuming each exact accepted request at most once while keeping abstention inert.
- [x] Observe and explicitly acknowledge each exact completed target-delay result once, resetting
  its task only while the exact resolved world-state authority remains current.
- [x] Dependency-order target-delay authority and result resolution in the main scene while keeping
  ordinary startup inert and proving exact application only through a switch-gated smoke.
- [x] Define one exact-current authoritative advancing world-event candidate for target-delay
  recovery, with stale, rebuilt, mismatched, recorded, non-authoritative, and non-advancing input
  failing closed.
- [x] Apply one exact-current target-delay recovery as a fresh immutable world-event occurrence,
  retaining complete delay history and the exact recovery candidate.
- [x] Select one exact eligible target-delay recovery in stable bounded caller order, then retain
  exact actor and current authoritative world state for provider-neutral consideration without
  selecting, deciding, or applying it.
- [x] Accept only an exact-actor abstention or exact currently eligible request-member target-delay
  recovery selection, then bind and apply that decision while leaving abstention inert.
- [x] Obtain and validate one provider-neutral target-delay recovery decision with exact request and
  caller cancellation forwarding, without binding a model or owning background work.
- [x] Retain exact OpenAI target-delay recovery settings and model, then supply configured, one-shot,
  or stable offline models without construction-time I/O or automatic invocation.
- [x] Own one isolated bounded target-delay recovery task and explicit consideration flow with
  exact result installation, overlap preservation, cancellation, reset, and fail-closed behavior.
- [x] Strictly decode one exact target-delay recovery decision from a bounded closed JSON object
  while rebinding actor text and candidate index to exact request instances.
- [x] Serialize one bounded stateless OpenAI structured-output request over trusted exact
  target-delay recovery world and candidate context without credential or network access.
- [x] Accumulate one complete ordered target-delay recovery OpenAI Responses lifecycle and framed
  SSE stream into an exact accepted decision without exposing partial or provider-failed output.
- [x] Execute one exact target-delay recovery request through caller-owned OpenAI credential, HTTP,
  settings, and endpoint dependencies with redacted failures and caller cancellation.
- [x] Create one exact target-delay recovery OpenAI binding from caller-owned dependencies without
  construction-time credential access, HTTP, or model invocation.
- [x] Compose, replace, and clear that exact binding explicitly through the existing temporary
  target-delay recovery model source while preserving its stable offline fallback.
- [x] Activate that exact target-delay recovery binding from selected OpenAI game settings while
  restoring the same stable offline model for Ollama without automatic decision work.
- [x] Dependency-order the target-delay recovery source, task, and explicit consideration flow in
  the main scene with one switch-gated exact-selection smoke and unchanged request world state.
- [x] Own one exact target-delay recovery event-log and world-time pair in an isolated scene
  controller, consuming each exact accepted request once while preserving abstention and prior result.
- [x] Resolve each fresh exact completed target-delay recovery once through scene authority, then
  acknowledge only while the observed result and all resolved authority tokens remain current.
- [x] Dependency-order target-delay recovery authority and result resolution in the main scene while
  keeping ordinary startup inert and proving exact application only through switch-gated smoke.
- [x] Own one exact-current target-delay recovery candidate from the applied delay result and one
  caller-authored advancing recovery event, with stale or inapplicable input failing closed.
- [x] Retain one request only from the delay-recovery candidate source's exact current candidate,
  preserving the exact delayed target and current authoritative event, log, and time graph.
- [x] Dependency-order the delay-recovery candidate and request sources in the main scene while
  ordinary startup stays inert and only the switch-gated delay smoke creates their exact state.
- [x] Start recovery consideration only for the request source's exact current request while the
  task is idle, preserving the queued model across every rejected start.
- [x] Dependency-order that exact-request flow and prove one switch-gated delay-to-recovery chain
  while ordinary startup remains inert and no autonomous trigger is added.
- [x] Preflight one complete exact-current delay-recovery graph before publishing candidate,
  request, or authority state and before consuming the queued model, then start it exactly once.
- [x] Dependency-order the delay-recovery start coordinator in the main scene and route the
  switch-gated delay-to-recovery smoke through it while ordinary startup remains inert.
- [x] Bind one exact eligible `influence` occurrence to an applicable directional target-to-actor
  relationship change without applying that change.
- [x] Apply one exact eligible `influence` candidate as an immutable relationship transition while
  retaining the exact candidate and leaving every rejected source unchanged.
- [x] Define one exact-current inverse relationship-change candidate for influence recovery, with
  stale, rebuilt, misdirected, wrong-dimension, wrong-delta, and inapplicable input failing closed.
- [x] Apply one exact-current eligible inverse influence change as fresh numerically restored
  relationship state while retaining both immutable influence and recovery reasons.
- [x] Select the exact first eligible influence candidate in stable bounded caller order only after
  complete one-pass collection validation, without applying or recovering it.
- [x] Retain exact current actor, interaction-access, relationship, and bounded candidate state for
  one provider-neutral influence consideration without deciding or applying it.
- [x] Accept only an exact-actor abstention or exact currently eligible request-member influence
  selection, then bind that decision immutably to its exact consideration request.
- [x] Apply an accepted exact influence selection through the existing immutable transition while
  leaving an accepted abstention inert and every request source unchanged.
- [x] Obtain and validate one provider-neutral influence decision with exact request and caller
  cancellation forwarding, without binding a model or owning background work.
- [x] Retain exact OpenAI influence settings and model, then supply configured, one-shot, or stable
  offline models without construction-time I/O or automatic invocation.
- [x] Own one isolated bounded influence-decision task and explicit consideration flow with exact
  result installation, overlap rejection, cancellation, reset, and fail-closed offline behavior.
- [x] Strictly decode and semantically stream one exact influence decision, then serialize one
  bounded stateless OpenAI structured-output request over trusted relationship context.
- [x] Execute that request through caller-owned credential and HTTP dependencies, requiring a
  successful event stream and exposing an inert exact OpenAI model binding without construction I/O.
- [x] Compose, replace, and clear one exact OpenAI influence binding from selected game settings,
  then dependency-order its source and explicit task flow in the main scene without automatic work.
- [x] Own one exact authoritative influence relationship in an isolated scene controller, consuming
  each exact accepted request at most once while keeping abstention referentially and numerically inert.
- [x] Observe and explicitly acknowledge each exact completed influence result once, resetting its
  task only while the exact resolved relationship authority remains current.
- [x] Dependency-order influence authority and result resolution in the main scene while keeping
  ordinary startup inert and proving exact application only through a switch-gated smoke.
- [x] Select one exact eligible influence-recovery candidate in stable bounded caller order only
  after complete one-pass collection validation, without applying recovery.
- [x] Retain the exact deciding character, current relationship, and bounded candidate state for
  one provider-neutral influence-recovery consideration without deciding or applying it.
- [x] Accept only an exact relationship-owner abstention or exact currently eligible request-member
  recovery selection, then bind that decision immutably to its exact consideration request.
- [x] Apply one accepted exact recovery selection through the existing immutable transition while
  leaving an accepted abstention inert and every request source unchanged.
- [x] Obtain and validate one provider-neutral influence-recovery decision with exact request and
  caller cancellation forwarding, without binding a model or owning background work.
- [x] Retain exact OpenAI influence-recovery settings and model, then supply configured, one-shot,
  or stable offline models without construction-time I/O or automatic invocation.
- [x] Own one isolated bounded influence-recovery decision task with exact result installation,
  overlap rejection, cancellation, reset, and fail-closed model or validation failure.
- [x] Start one recovery decision only through an explicit isolated flow while its task is idle,
  preserving a queued one-shot model across every rejected overlapping start.
- [x] Strictly decode one exact influence-recovery decision from a bounded closed JSON object while
  rebinding actor text and candidate index to exact request instances.
- [x] Serialize one bounded stateless OpenAI structured-output request over trusted exact
  influence-recovery relationship and candidate context without credential or network access.
- [x] Complete one bounded influence-recovery semantic-event and framed SSE lifecycle, exposing an
  exact accepted decision only after successful response completion.
- [x] Execute that request through caller-owned credential and HTTP dependencies, requiring a
  successful event stream and exposing an inert exact OpenAI model binding without construction I/O.
- [x] Compose, replace, and clear one exact OpenAI influence-recovery binding from explicit game
  settings without credential, HTTP, model, task, scene, or authoritative input/output.
- [x] Activate that recovery binding through selected game settings and dependency-order its source
  and explicit task flow in the main scene while keeping ordinary startup inert.
- [x] Own one exact authoritative recovery relationship in an isolated scene controller, consuming
  each exact accepted request at most once while keeping abstention referentially inert.
- [x] Observe and explicitly acknowledge each exact completed influence-recovery result once,
  resetting its task only while the exact resolved relationship authority remains current.
- [x] Dependency-order recovery authority and result resolution in the main scene while keeping
  ordinary startup inert and proving exact recovery only through a switch-gated smoke.
- [x] Produce at most one exact-current inverse recovery candidate from the influence controller's
  latest applied-result authority while every invalid or repeated input remains inert.
- [x] Dependency-order that candidate source in the main scene and prove explicit production after
  one applied influence while ordinary startup remains inert and recovery work stays unstarted.
- [x] Retain one request from only the candidate source's exact current recovery candidate, deriving
  the exact relationship-owning target and preserving the exact current relationship graph.
- [x] Dependency-order that request source in the main scene while ordinary startup stays inert and
  the switch-gated influence smoke alone creates the exact candidate and request.
- [x] Start recovery consideration only for the request source's exact current request while the
  task is idle, preserving the queued model across every rejected start.
- [x] Dependency-order the exact-request flow and prove one switch-gated influence-to-recovery chain
  while ordinary startup remains inert and no autonomous trigger is added.
- [x] Preflight one complete exact-current recovery graph before publishing its candidate, request,
  or authority and before consuming the queued model, then start that exact request once.
- [x] Dependency-order the recovery-start coordinator in the main scene and route the switch-gated
  influence-to-recovery smoke through it while ordinary startup remains inert.
- [x] Retain one exact actor and stable distinct caller-filtered canonical sabotage kinds for one
  provider-neutral action consideration without inferring eligibility or selecting a target.
- [x] Accept only that actor's exact request-member action kind or explicit abstention, then bind
  the accepted decision immutably to its exact request without dispatching an action.
- [x] Obtain and validate one provider-neutral sabotage-action decision with exact request and
  caller cancellation forwarding, without binding a model or owning background work.
- [x] Strictly decode one bounded sabotage-action decision document and rebind only value-matching
  actor and request-member kind text to their exact request instances.
- [x] Accumulate one bounded ordered OpenAI Responses stream and complete framed SSE only after an
  exact accepted sabotage-action selection or abstention.
- [x] Serialize one stateless structured-output sabotage-action request with only bounded trusted
  actor and caller-filtered action-kind context plus a closed exact decision schema.
- [x] Retain one exact OpenAI setting and caller-composed sabotage-action decision model without
  construction-time model invocation or input/output.
- [x] Execute one sabotage-action request through caller-owned OpenAI dependencies and create its
  exact immutable binding without construction-time credential access, network I/O, or invocation.
- [x] Supply one configured, one-shot, or stable fail-closed offline sabotage-action model through
  exact-instance binding replacement and clearing without construction-time input/output.
- [x] Own one isolated sabotage-action decision task with overlap rejection, bounded cancellation,
  explicit terminal reset, and main-thread-only accepted-result installation.
- [x] Explicitly compose one already-authored sabotage-action request with the isolated model source
  and task while preserving a queued one-shot model across rejected overlap.
- [x] Compose one explicit OpenAI sabotage-action binding into the exact existing model source with
  stale-safe initialize, replacement, and clear behavior and no construction-time input/output.
- [x] Activate, replace, or clear that exact binding from selected game settings while preserving
  one-shot precedence and restoring the stable offline model for supported local settings.
- [x] Dependency-order the sabotage-action source, task, and explicit flow in the main scene while
  ordinary startup remains inert and only a switch-gated smoke starts consideration.
- [x] Explicitly observe one exact completed sabotage-action selection or abstention at most once
  and acknowledge only that same current task result without dispatching an action.
- [x] Dependency-order that explicit selection coordinator in the main scene while ordinary startup
  remains inert and only the switch-gated smoke observes and acknowledges a result.
- [x] Author one exact sabotage-action request containing only the canonical conceal kind when the
  exact current authored concealment graph remains eligible, without starting or applying work.
- [x] Author one exact sabotage-action request containing only the canonical move kind when the
  exact current authored move graph remains eligible, without combining, starting, or applying it.
- [x] Author one exact sabotage-action request containing only the canonical substitute kind when
  the exact current authored substitution graph remains eligible, without starting or applying it.
- [x] Author one exact sabotage-action request containing only the canonical contaminate kind when
  the exact current authored contamination graph remains eligible, without starting or applying it.
- [x] Author one exact sabotage-action request containing only the canonical misquote kind when the
  exact authored hearing and witness graph remains eligible, without delivering dialogue.
- [x] Author one exact sabotage-action request containing only the canonical delay kind when the
  exact bounded delay graph remains eligible against current event-log and world-time state.
- [x] Author one exact sabotage-action request containing only the canonical influence kind when
  the exact bounded influence graph remains eligible against current relationship state.
- [x] Compose the seven action-specific authors' exact current outputs into at most one fresh
  same-actor request in canonical order without rerunning a gate or starting consideration.
- [x] Start the existing explicit sabotage-action consideration flow only for the composer-owned
  exact request without observing, dispatching, or applying its eventual selection.
- [x] Dependency-order the seven request authors, composer, and exact-start coordinator in the main
  scene while ordinary startup stays inert and only a switch-gated smoke starts consideration.
- [x] Bind one exact observed sabotage-action selection or abstention to only its still-current
  action-specific author output without acknowledging, dispatching, or applying the result.
- [x] Dependency-order the exact author binding in the main scene while ordinary startup remains
  inert and only the switch-gated smoke observes and binds one live selection.
- [x] Explicitly acknowledge only the still-current exact bound generic result while retaining its
  selected author request as a later dispatch token and leaving abstention without an output.
- [x] Bind every authored singleton request to its exact strongly typed action-specific candidate,
  and retain only the selected exact binding without starting, dispatching, or applying an action.
- [x] Compose one acknowledged exact candidate binding into only its existing action-specific
  provider-neutral request after revalidating the corresponding live source and authority.
- [x] Start only that exact composed action-specific request once after revalidating its typed
  candidate and current authority, preserving queued models across every rejected preflight.
- [x] Observe and explicitly acknowledge only the exact typed terminal result for that started
  request through its existing action-specific result owner.
- [x] Bind one acknowledged selected typed result to its exact current branch-owned authoritative
  outcome without starting recovery, consuming a model, or rebuilding any value.
- [x] Gate one retained exact typed sabotage outcome for its existing recovery or evidence route
  while preserving branch acknowledgement and current-authority reference identity.
- [x] Route one exact eligible concealment outcome through its existing evidence-gated recovery
  transition without adding a new recovery rule or consuming another model.
- [x] Route one exact eligible move outcome through its existing evidence-gated recovery transition
  while retaining its exact source placement and consuming no additional model.
- [x] Route one exact eligible substitution outcome through its existing evidence-gated recovery
  transition without rebuilding its candidate or consuming another model.
- [x] Route one exact eligible contamination outcome through its existing evidence-gated recovery
  transition while preserving authoritative evidence and consuming no additional model.
- [x] Route one exact eligible misquotation exposure through its current witness view and existing
  truth-neutral presentation route without consuming evidence or another model.
- [x] Compose one exact eligible delay outcome into its existing recovery request while retaining
  the authoritative world-event, event-log, world-time, and recovery-candidate instances.
- [x] Explicitly start only that exact composed delay-recovery request once without observing or
  applying its result, and preserve the queued recovery model across rejected preflight.
- [x] Bind one exact completed generic Delay recovery back to its author route only after the
  existing resolution owner installs current fresh append-only world snapshots, then acknowledge it
  once and retain its exact terminal output.
- [x] Compose one exact eligible generic Influence outcome into its existing recovery request while
  retaining the exact current relationship and inverse recovery candidate without starting work.
- [x] Explicitly start only that exact composed Influence-recovery request after complete preflight,
  preserving its queued recovery model across rejected paths and leaving completion unapplied.
- [x] Bind one exact completed generic Influence recovery back to its author route only after the
  existing resolution owner installs the fresh restored relationship, then acknowledge it once and
  retain its exact terminal output.
- [x] Retain one exact completed generic sabotage route across the seven canonical branches without
  copying its terminal output or scheduling another action.
- [x] Reset only that exact retained terminal route once while preserving every prior route owner,
  authoritative snapshot, presentation, task, and model source.
- [x] Expose one exact completed sabotage-turn token after terminal-route reset while retaining its
  actor, generic author request, and canonical action kind without copying route output.
- [x] Define a pure ordinary-scheduling eligibility gate for only that exact current completed-turn
  token without scheduling or authoring another action.
- [x] Retain one exact ordinary-scheduling admission for the eligible completed-turn token without
  copying its provenance or starting another sabotage action.
- [x] Bind that exact admission once to the existing ordinary character-turn start boundary without
  creating a turn request, reserving the slot, or starting the sabotage flow.
- [x] Purely revalidate the exact bound admission against the still-current ordinary turn reference
  before any future scheduling without mutating either owner.
- [x] Retain one exact ordinary-turn scheduling authorization from the still-current revalidated
  sabotage admission without reserving the slot or starting another sabotage action.
- [x] Consume only that exact still-current authorization once at an explicit sabotage-cycle
  boundary without copying provenance, reserving the turn, or starting any action.
- [x] Purely revalidate the exact current sabotage-cycle boundary graph immediately before future
  action authoring without mutating its authorization, admission, or ordinary turn.
- [x] Admit that exact still-current cycle boundary once at the first action-authoring seam without
  copying provenance or invoking any action-specific author.
- [x] Let only that exact admission guard one fresh invocation of the existing Move author without
  resetting prior ownership, composing a request, or consuming another model.
- [x] Compose only that exact admission-guarded Move author output into one fresh generic Move
  request without starting consideration, resetting either author, or consuming a model.
- [x] Explicitly start only that exact fresh generic Move request on a fresh scene-owned task while
  preserving the queued model across rejected preflight and leaving completion unobserved.
- [x] Observe only that exact completed fresh generic Move result after revalidating its complete
  retained graph, without acknowledging, dispatching, or applying it.
- [x] Bind the exact observed fresh result to the guarded follow-up Move candidate, then acknowledge
  only that same bound result while retaining both exact downstream tokens.
- [x] Compose those exact acknowledged tokens into one fresh provider-neutral typed Move request
  over the current authoritative placement and evidence snapshots without starting it.
- [x] Explicitly start only that exact fresh typed Move request on a separate fresh scene-owned task
  while preserving its queued model across rejected preflight and leaving completion unobserved.
- [x] Observe only that exact completed typed follow-up Move result together with the controller's
  exact fresh placement and evidence snapshots, without acknowledging or routing it.
- [x] Bind and acknowledge only that exact observed typed result and current outcome, resetting only
  its task while retaining the exact result and authoritative snapshot tokens.
- [ ] Permit the enemy agent to conceal, move, substitute, contaminate, misquote, delay, and
  influence only when physically and informationally able.
- [ ] Ensure sabotage cannot rewrite truth, teleport actors, or permanently remove every route to a
  critical conclusion.
- [ ] Make sabotage leave potentially discoverable evidence.

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
