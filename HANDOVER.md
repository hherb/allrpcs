# Allrpcs Handover

## Current state

`main` contains merged PR #203 at `1578c75`. The current branch is
`codex/phase-7-participation`.

This branch completes three bounded Phase 7 participation foundations:

- `CharacterTurnParticipationKind` defines exactly `respond`, `interject`, and
  `remain-silent` canonical singleton values in stable order;
- `CharacterTurnParticipationDecision` retains an exact scheduled character, exact kind, and the
  exact existing authored-goal motive required for interjection or meaningful silence;
- `CharacterTurnParticipationDecisionPolicy` accepts only exact character and goal instances,
  direct-address responses, non-addressed interjections, and either addressed or non-addressed
  meaningful silence;
- `CharacterTurnParticipationSelectionPolicy` returns the exact first response or interjection
  from at most 32 distinct already-accepted decisions in scheduled order, or no selection when all
  characters remain silent.

The combined roadmap item remains open because this branch does not generate or infer decisions,
inspect dialogue text, invoke models, start or cancel turns, integrate the ordinary scene, change
player interruption, add interjection frequency or conversation-loop bounds, perform actions,
mutate goals, add UI or persistence, start Phase 8, release, sign, or distribute.

## Validation and review

- final host-local `./scripts/quality.sh` passed cleanly:
  - workflow validation, restore, formatting, and zero-warning solution build passed
  - 1,978 Domain, 402 Application, 447 Infrastructure, and 8 architecture tests passed
  - the representative discovery scenario validated 5 evidence definitions
  - all credential-free Godot smokes and the bounded Compatibility-renderer launch passed
- 27 focused participation tests passed
- test-first missing-type failures were observed before each implementation slice
- `git diff --check`, practical 500-line/120-character limits, closed-vocabulary, exact-identity,
  authored-goal, direct-address, stable-order, bounded-input, network-free, and scope review passed

The restricted full quality run completed its zero-warning build and reproduced the known VSTest
localhost-bind restriction. The identical host-local command passed. Hosted jobs for recent PRs
still had `steps: []` under the Actions account-budget condition, so do not retry or dispatch them
unless that condition changes.

## Exact next bounded slice

Take **Phase 7i: explicit bounded autonomous-conversation budget**.

Acceptance boundary:

1. Add one immutable Domain budget retaining a nonnegative completed character-turn count, a
   positive configured maximum no larger than the existing 16-turn recent-conversation window, and
   whether the player has interrupted the chain.
2. Add pure policy and transition seams that allow one further character turn only below the
   configured maximum and before interruption, increment exactly once after a completed turn, and
   make interruption monotonic.
3. Add focused Domain tests for zero/start/exhausted/interrupted states, the exact upper bound,
   invalid counts, immutable transition results, repeated interruption, null rejection, and the
   smallest public API surface.
4. Do not loop, schedule speakers, create participation decisions, invoke a model, start/cancel a
   character-turn activity, wire player input or scenes, alter recent-conversation retention, add
   timeouts/retries, perform actions, or add UI/persistence/provider behavior.

Leave both the combined participation/player-interruption roadmap item and the unbounded-loop item
unticked until their later ordinary-flow integrations are complete.
