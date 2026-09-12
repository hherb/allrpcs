# Allrpcs Handover

## Current state

`main` contains merged PR #214 at `e6f6836`. The current branch is
`codex/phase-7-complete-listener-scheduling`.

Draft PR #215 is open and mergeable. Its five hosted jobs failed immediately with `steps: []`,
confirming the unchanged Actions/account infrastructure condition.

This branch completes two bounded Phase 7 slices:

- Phase 7aq generalizes the ordinary player-dialogue participation coordinator from one healer to
  one complete bounded stable collection of exact heard LLM listeners and one complete distinct
  value-based direct-address set. It materializes and preflights both collections, task/turn
  availability, and fresh-budget replacement before consuming a model or changing budget state.
- Phase 7ar adds a pure bounded participant lookup that preflights a complete distinct roster of
  exact LLM assignments before returning the exact value-matching character for an awaiting turn.

Ordinary player input still supplies only its directly addressed healer. The isolated two-listener
smoke proves direct-response priority, meaningful-silence-to-motivated-interjection selection,
all-silent no-turn behavior, stable order, exact event/character/motive identity, value-equal
addressing, overlap rejection without model consumption, and unchanged surrounding scene state.

The first Phase 7 parent remains open because the ordinary scene does not yet schedule or generate
another listener's interjection. The loop parent remains open because character speech does not
start another participation round.

This branch does not add another embodied or heard scene character; infer addresses from text;
generalize generation context beyond the healer; start a round from character speech; add local
Chat Completions, Anthropic, or Ollama participation dialects; add diagnostics or retries; persist
an active chain; add whispering/private conversations, independent tasks, sabotage, or Phase 8
work; release; sign; or distribute.

## Validation and review

- Phase 7aq's new two-listener smoke was introduced against the intended collection API and failed
  compilation until the coordinator accepted complete schedule/address collections.
- Phase 7ar's focused Domain tests were introduced before the lookup and failed compilation; the
  corrected five-test suite then passed host-local.
- focused host-local ordinary participation smokes pass both legacy one-listener behavior and the
  new direct-response, motivated-interjection, all-silent, stable-order, exact-identity, overlap,
  no-consumption, and surrounding-state cases;
- host-local restore, formatting, and the zero-warning solution build pass, followed by 2,098
  Domain, 426 Application, 522 Infrastructure, and 8 architecture tests;
- the representative validator passes all 5 evidence definitions through `dotnet exec` on its
  built DLL after the known macOS `dotnet run` apphost stall;
- the complete host-local headless Godot smoke matrix and bounded Compatibility-renderer launch
  pass;
- `git diff --check`, the practical 500-line/120-character limits for new files, public-surface,
  exact-identity, full-preflight, no-consumption, fail-closed, documentation, and scope review pass.

Restricted execution reproduced the known VSTest loopback-socket and Godot `user://logs` failures;
the identical checks passed host-local. Hosted Actions remained account-budget blocked with
`steps: []` on PR #215; do not retry or dispatch those unchanged workflows.

## Exact next bounded slice

Take **Phase 7as: scene-owned ordinary participant roster and exact turn resolution**.

Acceptance boundary:

1. Add one scene-owned temporary ordinary participant source that retains the exact existing healer
   plus one freshly authored representative sage in stable order as a bounded read-only roster with
   distinct identifiers and exact LLM assignments.
2. Resolve an ordinary awaiting turn's exact character from that roster through
   `CharacterTurnParticipantLookup` before the existing generation start, while keeping the current
   healer-only generation input contract and ordinary healer behavior unchanged.
3. Add focused credential-free scene coverage for source ordering, exact healer retention, distinct
   sage identity, value-equal awaiting-turn resolution, absent/non-awaiting no-result behavior,
   closed source surface, and unchanged dialogue, world, perception, model, and presentation state.
4. Do not yet add a sage sprite or hearing position, place the sage in an ordinary event audience,
   schedule or generate a sage turn, generalize per-character perception/knowledge/affordances,
   infer addresses, start a participation round from character speech, drive a loop, or tick either
   Phase 7 parent item.
