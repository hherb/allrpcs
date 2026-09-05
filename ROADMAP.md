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
  conclusions use redundant evidence routes rather than linear quest flags.
- OpenAI, Anthropic, and Ollama remain interchangeable adapters. Provider objects and credentials
  never enter domain state, saves, scenes, logs, source, or model context.
- Durable state is local and provider-neutral. Hidden chain-of-thought is never requested or
  stored.
- Greybox controls, markers, status text, and geometry are behavior evidence, not final UX or art.

## Completed foundation and greybox

- [x] Phase 0 established the repository, Godot/.NET solution and dependency boundaries, local
  quality spine, credential-free hosted checks, unsigned development exports, pinned workflows,
  and `AGPL-3.0-only` licence.
- [x] The retained Phase 1 greybox establishes keyboard movement, bounded camera framing,
  representative collision, deterministic navigation, healer follow/wait/move behavior,
  interaction focus, and regression smokes.
- [ ] Point-and-click movement remains deferred pending prototype retention.
- [ ] Disease-time pausing and a world-time debug overlay remain deferred pending authoritative
  time and interaction systems.

## Active milestone: authoritative world and discovery

Phases 2a–2j established typed identifiers; immutable world, character, environmental,
perception, and inventory models; inventory policies; and capability/tool discovery conditions.
They preserve authored order and fail closed without granting discovery or mutating world state.

Phases 2k1–2k23 established immutable focused-search requests and activities; pure lifecycle
policies and transitions; condition-aware Seeker start/cancel/complete control; visible activity;
stable focus, request, and condition bridges; and temporary F/C/E inputs with Boolean result text.

Phases 2k24–2k38 established immutable outcomes and ordered snapshots plus pure projection,
lookup, applicability, change, application, consumption, resolution, and recording boundaries.

Phases 2k39–2k47 connect an existing outcome to completion and explicit resolution through
combined contracts, controller-owned activity/outcome state, a temporary deterministic outcome
source, the E input, and temporary Seeker perception initialization. Completion records an exact
`Identified` outcome and removes the activity. The controller retains the temporary source's exact
ordered `Noticed` perception snapshot once, and its controller-only boundary can apply and consume
an eligible recorded outcome without changing activity or presentation. Ordinary input does not
resolve it yet.

- [ ] Finish bounded focused searches with authoritative perception change and discovery effect.
  There is still no resolution input, automatic lifecycle, progress, duration, failure reason,
  discovery effect, or final interaction UI. The exact next slice is in `HANDOVER.md`.
- [ ] Add passive noticing without arbitrary pixel clicking.
- [ ] Add knowledge-triggered reinterpretation of earlier observations.
- [ ] Add an append-only authoritative world-event log and qualitative meaningful-event time.
- [ ] Add deterministic, schema-versioned save/load.
- [ ] Add representative visible, concealed, partial, residue, and tool-gated discoveries.
- [ ] Add scenario validation and tests proving filtered views cannot leak hidden/private state.

Phase 2 acceptance remains: save/load preserves world state exactly, and no UI or character context
reveals hidden information before its authored discovery conditions are met.

## Later milestones

- [ ] Phase 3: unrestricted natural-language dialogue with a deterministic healer and validated,
  interruptible, one-action-at-a-time commitments.
- [ ] Phase 4: provider-neutral live LLM turns with filtered context, validated output, streaming,
  cancellation, timeouts, credential safety, and explicit offline behavior.
- [ ] Phase 5: local character memory, sourced beliefs, bounded relationships, commitments,
  deterministic retrieval, summaries, migrations, and restart/provider-switch continuity.
- [ ] Phase 6: authoritative disease stages, meaningful-time progression, evidence-driven remedies,
  experiments, recoverable failures, and the first patient stabilization.
- [ ] Phase 7: multiple autonomous roles, bounded off-screen action, and consequential but fair,
  physically and informationally constrained sabotage.
- [ ] Phase 8: the authored opening region, Seeker customization, journal/map, multiple
  investigation routes, stabilization, and dead-end-resistant playtesting.
- [ ] Phase 9: validated art direction followed by accessible production art, effects, ambience,
  sound, and music with recorded asset provenance.
- [ ] Phase 10: provider-neutral, optional ASR/TTS with push-to-talk, correction, subtitles,
  interruption, cancellation, and full typed-input parity.
- [ ] Phase 11: cross-platform, accessibility, long-session, migration, provider-failure,
  diagnostics, packaging, signing, and opening-scenario release-candidate validation.
- [ ] Phase 12: explicitly deferred expansions including multiplayer, role reassignment, new
  campaigns, authoring tools, mods, procedural dressing, extra accessibility modes, and ports.

## Cross-cutting completion rules

- Keep domain rules deterministic and independently testable; keep Godot and provider types at
  their boundaries; validate every world-changing intention in the engine.
- Give each model only its character's filtered context. Treat player text as untrusted in-world
  content and retain natural-language transcripts locally.
- Preserve recoverable critical knowledge and valid silence, refusal, uncertainty, and
  disagreement. The game must remain saveable when providers are unavailable.
- Add automated tests with behavior, use representative manual evidence for visual/interaction
  claims, and update this roadmap and `HANDOVER.md` at each completed slice.
- Unsigned hosted artifacts are development evidence only. Merge, release, signing, notarization,
  distribution, and repository-visibility changes require separate explicit authorization.
