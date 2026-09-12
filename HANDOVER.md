# Allrpcs Handover

## Current state

`main` contains merged PR #204 at `b70feb0`. The current branch is
`codex/phase-7-conversation-bounds`.

This branch completes two bounded Phase 7 autonomous-conversation foundations:

- `AutonomousConversationBudget` retains a nonnegative completed-turn count, a positive configured
  maximum no greater than the existing 16-turn recent-conversation window, and whether the player
  has interrupted the chain;
- pure continuation and transition seams allow one further turn only below that maximum and before
  interruption, increment exactly once, reject completion after exhaustion or interruption, and
  make interruption monotonic;
- `AutonomousConversationBudgetController` owns one exact immutable budget in the scene, delegates
  completion and interruption, and fails closed before initialization and on repeated operations;
- one credential-free focused Godot smoke proves exact one-time ownership, immutable replacement,
  monotonic interruption, closed public surface, and unchanged state after rejected operations.

The unbounded-loop roadmap item remains open because no ordinary autonomous follow-up is gated or
started yet. This branch does not resolve a complete participation round, generate or infer
decisions, loop, schedule or start speakers, invoke a model, wire player input, cancel an activity,
alter recent-conversation retention, add timeouts/retries, perform actions, add hearing/privacy
rules, add UI/persistence/provider behavior, start Phase 8, release, sign, or distribute.

Draft PR #205 contains this work.

## Validation and review

- final host-local `./scripts/quality.sh` passed cleanly:
  - workflow validation, restore, formatting, and zero-warning solution build passed
  - 1,996 Domain, 402 Application, 447 Infrastructure, and 8 architecture tests passed
  - the representative discovery scenario validated 5 evidence definitions
  - all credential-free Godot smokes and the bounded Compatibility-renderer launch passed
- 18 focused autonomous-conversation Domain tests passed
- the focused budget-controller Godot smoke passed
- test-first missing-type failures were observed before the Domain and scene-controller implementations
- `git diff --check`, practical 500-line/120-character limits, immutable-state, upper-bound,
  monotonic-interruption, fail-closed, smallest-surface, network-free, and scope review passed

The restricted focused VSTest run reproduced the known localhost-bind restriction; the identical
host-local command passed. The restricted focused Godot run reproduced the known `user://logs`
failure; the identical host-local command passed. Hosted jobs for recent PRs still had `steps: []`
under the Actions account-budget condition, so do not retry or dispatch them unless that condition
changes. Draft PR #205 triggered run `34662311735` at implementation commit `91c89d5`; all five
jobs completed with `steps: []` under that same infrastructure/account condition.

## Exact next bounded slice

Take **Phase 7k: complete scheduled participation-round acceptance**.

Acceptance boundary:

1. Add one pure Domain seam that accepts a stable scheduled list of exact `Character` instances,
   one corresponding participation decision per character, and the directly addressed character
   identifiers supplied by the caller.
2. Bound every input to 32 distinct characters, require exact scheduled-character identity and
   stable positional alignment, and delegate every decision to the existing participation-decision
   policy using value-based direct-address membership and exact authored-goal motives.
3. Validate the complete round before delegating to the existing selection policy, then return the
   exact first speaking decision or no selection when all accepted decisions are meaningful silence.
4. Add focused Domain tests for empty, response, interjection, all-silent, mismatched order/count,
   duplicate/oversized/null input, invalid late decisions after an earlier speaker, exact-instance
   retention, unchanged inputs, and the smallest public API surface.
5. Do not generate or infer decisions or addresses, inspect dialogue text, invoke a model, start or
   cancel activities, loop, wire the scene or player input, alter the budget or recent-conversation
   window, perform actions, or add hearing/UI/persistence/provider behavior.

Leave both ordinary participation/player-interruption integration and the unbounded-loop parent
item unticked until a later ordinary-flow slice consumes these pure seams.
