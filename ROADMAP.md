# Allrpcs Roadmap

## Purpose

Keep the remaining work, milestone acceptance criteria, and settled design constraints here.
Use [HANDOVER.md](HANDOVER.md) for the exact next bounded slice and current verification;
use [README.md](README.md) for development commands. Detailed completed-work evidence stays
in the linked audits and art packages, with earlier checklists retained in Git history.

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
is separately approved and implemented locally, including asset/master/imports and 18 runtime
captures matching accepted external pixels. Its
[runtime package](art-direction/forest-path-runtime-promotion/README.md) records the pending
checkpoint and verification. Preserve closed-side-only coverage and the existing gate and
navigation; the near side inside the Clearing remains untextured.

- [ ] Obtain owner runtime visual confirmation before publishing this forest-path promotion.

The [whole-channel composition](art-direction/approved-external-compositions/README.md)
and western watercourse corrections have external visual acceptance only. Whole-channel
runtime promotion requires its own named scope. Preserve accepted sources and the accepted
Shrine intersection triangle. Routine bounded external proof approvals remain waived;
named runtime/layout/physical promotion and runtime visual checkpoints remain separate.
Visual acceptance does not establish gameplay, pacing, usability, or accessibility acceptance.

### 9.2 Remaining production assets

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
