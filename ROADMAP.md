# Allrpcs Roadmap

## Product and architecture guardrails

Allrpcs is a slow-paced, top-down 2D investigation and healing game. Its first vertical slice has
one customizable human Seeker, autonomous companions and encountered roles, natural-language
dialogue, exploration and evidence, a stabilizing remedy, and local save continuity on macOS,
Linux, and Windows.

- Godot 4.6 .NET owns rendering, authoring, input, and authoritative runtime state; pure C# Domain
  and Application libraries own deterministic rules.
- The engine owns truth, perception, inventory, movement, discovery, disease progression, and
  outcomes. LLMs choose only validated character attempts from filtered context.
- World truths, evidence, claims, beliefs, and private knowledge remain distinct. Critical
  conclusions require redundant evidence routes.
- OpenAI, Anthropic, and Ollama remain interchangeable adapters. Credentials, provider objects, and
  hidden chain-of-thought never enter domain state, saves, scenes, logs, source, or model context.
- Greybox controls, markers, status text, and geometry are behavior evidence, not final UX or art.

## Delivered foundation

Phase 0 established the Godot/.NET solution, dependency boundaries, quality spine, credential-free
hosted checks, unsigned development exports, pinned workflows, and `AGPL-3.0-only` licence. Phase 1
retains keyboard movement, bounded camera and collision, deterministic navigation, healer commands,
interaction focus, and regression smokes.

Phases 2a–2j established typed identifiers and immutable world, character, environmental,
perception, inventory, capability, tool, and discovery-condition boundaries. Phases 2k1–2k49 add
the focused-search lifecycle, deterministic outcome recording/resolution, controller-owned
immutable Seeker perception, temporary F/C/E/R wiring, and exact target-only `IDENTIFIED`
presentation. Phase 2l1 adds the immutable authoritative world-object snapshot; it is not wired to
the temporary greybox targets.

## Active milestone: authoritative world and discovery

- [ ] Finish bounded focused searches with authoritative perception change and discovery
  presentation. The exact next prerequisite is Phase 2l2 in `HANDOVER.md`: pure exact world-object
  lookup. Automatic lifecycle, progress, duration, failure reasons, discovery grants, and final
  interaction UI remain absent.
- [ ] Add passive noticing without arbitrary pixel clicking.
- [ ] Add knowledge-triggered reinterpretation of earlier observations.
- [ ] Add an append-only authoritative world-event log and qualitative meaningful-event time.
- [ ] Add deterministic, schema-versioned save/load.
- [ ] Add representative visible, concealed, partial, residue, and tool-gated discoveries.
- [ ] Add scenario validation proving filtered views cannot leak hidden or private state.

Phase 2 acceptance remains: save/load preserves world state exactly, and no UI or character context
reveals hidden information before its authored discovery conditions are met.

## Later milestones

- [ ] Phase 3: deterministic-healer natural-language dialogue and validated interruptible actions.
- [ ] Phase 4: provider-neutral live LLM turns with filtered context and explicit offline behavior.
- [ ] Phase 5: local memory, sourced beliefs, relationships, commitments, and migration continuity.
- [ ] Phase 6: disease progression, evidence-driven remedies, experiments, and stabilization.
- [ ] Phase 7: constrained autonomous roles and consequential but fair sabotage.
- [ ] Phase 8: opening region, Seeker customization, journal/map, and dead-end-resistant playtests.
- [ ] Phases 9–12: production art and audio, optional provider-neutral voice, cross-platform release
  validation, then deferred multiplayer, campaigns, authoring tools, mods, and ports.

## Delivery boundaries

Keep rules deterministic and engine-independent; validate every world-changing intention in the
engine; preserve valid silence, refusal, uncertainty, disagreement, recoverable critical knowledge,
and saveability without providers. Point-and-click movement remains deferred pending a retained
prototype. Disease-time pausing and a world-time debug overlay remain deferred pending authoritative
systems. Unsigned hosted artifacts are development evidence only; merge, release, signing,
notarization, distribution, and repository-visibility changes require separate authorization.
