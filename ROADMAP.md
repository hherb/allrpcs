# Allrpcs Roadmap

## Product and architecture guardrails

Allrpcs is a slow-paced, top-down 2D investigation and healing game. The first vertical slice has
one human-controlled, customizable Seeker; autonomous companions and encountered roles; natural
language dialogue; exploration and evidence; a stabilizing remedy; and local save continuity on
macOS, Linux, and Windows.

- Godot 4.6 .NET owns rendering, authoring, input, and authoritative runtime state. Pure C# Domain
  and Application libraries own deterministic rules.
- The engine owns truth, perception, inventory, movement, discovery, disease progression, and
  action outcomes. LLMs decide only what their characters attempt through validated affordances.
- World truths, evidence, claims, beliefs, and private knowledge remain distinct. Critical
  conclusions require redundant evidence routes rather than linear quest flags.
- OpenAI, Anthropic, and Ollama remain interchangeable adapters. Credentials and provider objects
  never enter domain state, saves, scenes, logs, source, or model context.
- Durable state is local and provider-neutral. Hidden chain-of-thought is never requested or stored.
- Greybox controls, markers, status text, and geometry are behavior evidence, not final UX or art.

## Delivered foundation

Phase 0 established the Godot/.NET solution, dependency boundaries, quality spine, credential-free
hosted checks, unsigned development exports, pinned workflows, and `AGPL-3.0-only` licence. The
retained Phase 1 greybox covers keyboard movement, bounded camera and collision, deterministic
navigation, healer commands, interaction focus, and regression smokes.

Phases 2a–2j established typed identifiers and immutable world, character, environmental,
perception, inventory, and capability/tool discovery-condition boundaries. Phases 2k1–2k38 added
focused-search lifecycle, outcome, perception-application, consumption, resolution, and recording
contracts. Phases 2k39–2k48 now connect temporary F/C/E/R controls to controller-owned activity,
outcome, and initialized Seeker perception snapshots: E records an exact `Identified` outcome and R
applies and consumes the first retained Seeker outcome without changing completed activity.

## Active milestone: authoritative world and discovery

- [ ] Finish bounded focused searches with authoritative perception change and discovery
  presentation. The exact next slice is Phase 2k49 in `HANDOVER.md`; automatic lifecycle, progress,
  duration, failure reasons, discovery grants, and final interaction UI remain absent.
- [ ] Add passive noticing without arbitrary pixel clicking.
- [ ] Add knowledge-triggered reinterpretation of earlier observations.
- [ ] Add an append-only authoritative world-event log and qualitative meaningful-event time.
- [ ] Add deterministic, schema-versioned save/load.
- [ ] Add representative visible, concealed, partial, residue, and tool-gated discoveries.
- [ ] Add scenario validation proving filtered views cannot leak hidden or private state.

Phase 2 acceptance remains: save/load preserves world state exactly, and no UI or character context
reveals hidden information before its authored discovery conditions are met.

## Later milestones

- [ ] Phase 3: deterministic-healer natural-language dialogue and validated, interruptible actions.
- [ ] Phase 4: provider-neutral live LLM turns with filtered context and explicit offline behavior.
- [ ] Phase 5: local memory, sourced beliefs, relationships, commitments, and migration continuity.
- [ ] Phase 6: disease progression, evidence-driven remedies, experiments, and stabilization.
- [ ] Phase 7: constrained autonomous roles and consequential but fair sabotage.
- [ ] Phase 8: opening region, Seeker customization, journal/map, and dead-end-resistant playtests.
- [ ] Phase 9: validated art direction, production art, effects, ambience, sound, and music.
- [ ] Phase 10: optional provider-neutral ASR/TTS with complete typed-input parity.
- [ ] Phase 11: cross-platform, accessibility, migration, packaging, signing, and release-candidate
  validation.
- [ ] Phase 12: deferred multiplayer, campaigns, authoring tools, mods, procedural dressing, and
  ports.

## Delivery boundaries

- Keep domain rules deterministic and engine-independent; validate every world-changing intention
  in the engine and give each model only its character's filtered context.
- Preserve valid silence, refusal, uncertainty, disagreement, recoverable critical knowledge, and
  saveability when providers are unavailable.
- Use automated behavioral tests and representative evidence for visual or interaction claims.
- Point-and-click movement remains deferred pending prototype retention. Disease-time pausing and a
  world-time debug overlay remain deferred pending authoritative time and interaction systems.
- Unsigned hosted artifacts are development evidence only. Merge, release, signing, notarization,
  distribution, and repository-visibility changes require separate explicit authorization.
