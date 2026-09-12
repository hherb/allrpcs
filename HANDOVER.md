# Allrpcs Handover

## Current state

`main` contains merged PR #205 at `1e76568`. The current branch is
`codex/phase-7-participation-rounds`.

This branch completes two bounded Phase 7 participation and loop-safety foundations:

- `CharacterTurnParticipationRoundPolicy` accepts at most 32 distinct exact scheduled
  `Character` instances, one distinct positionally aligned decision per character, and at most 32
  distinct directly addressed identifiers;
- it uses value-based direct-address membership while the existing decision policy enforces exact
  scheduled-character identity and exact authored-goal motives, validates the complete round, then
  delegates to the existing selection policy and returns its exact first speaking decision or no
  selection for meaningful silence;
- `AutonomousConversationParticipationPolicy` validates that complete round first, then exposes its
  exact selected speaker only while the exact autonomous-conversation budget permits another turn;
- exhausted and interrupted budgets suppress follow-up selection without changing any supplied
  budget, character, decision, address, or authored-goal instance.

The ordinary participation/player-interruption and unbounded-loop roadmap parents remain open
because no character-turn request or activity is gated or started by these seams. This branch does
not generate or infer decisions or addresses, inspect dialogue text, transition a budget, create a
loop, schedule or start speakers, invoke a model, wire the scene or player input, cancel an activity,
alter recent-conversation retention, add timeouts/retries, perform actions, add hearing/privacy
rules, add UI/persistence/provider behavior, start Phase 8, release, sign, or distribute.

Draft PR #206 contains this work.

## Validation and review

- final host-local quality sequence passed:
  - workflow validation, restore, formatting, and zero-warning solution build passed through
    `./scripts/quality.sh`
  - 2,011 Domain, 402 Application, 447 Infrastructure, and 8 architecture tests passed through that
    same invocation
  - direct `dotnet exec` of the already-built validator confirmed all 5 representative discovery
    evidence definitions
  - `./scripts/smoke-godot.sh` and the bounded Compatibility-renderer launch passed
- 15 focused new participation-round and budgeted-selection Domain tests passed
- test-first missing-type failures were observed before both Domain implementations
- `git diff --check`, practical 500-line/120-character limits, complete-round-before-selection,
  exact-identity, value-membership, immutable-input, upper-bound, fail-closed, smallest-surface,
  network-free, and scope review passed

The restricted focused Phase 7k VSTest run and the first complete quality run reproduced the known
localhost-bind restriction; the identical host-local commands passed. An earlier host-local
`./scripts/quality.sh` passed cleanly after Phase 7k. On the final tree the combined script passed
all builds and tests, then its unchanged scenario-validator apphost stalled in macOS `_dyld_start`
before managed code; the exact built validator DLL passed through `dotnet exec`, followed by the
complete smoke script and graphical launch. Hosted jobs for recent PRs still had `steps: []` under
the Actions account-budget condition. Draft PR #206 triggered run `34663656361` at handover commit
`9d3a78f`; all five jobs completed with `steps: []` under that same infrastructure/account
condition. Do not retry or dispatch those jobs unless the condition changes.

## Exact next bounded slice

Take **Phase 7m: autonomous follow-up turn-start eligibility**.

Acceptance boundary:

1. Add one pure Domain policy accepting an exact `AutonomousConversationBudget`, an exact selected
   `CharacterTurnParticipationDecision`, one candidate `CharacterTurnRequest`, and the optional
   current `CharacterTurnActivity`.
2. Permit a start only when the budget can continue, the decision is a speaking response or
   interjection, the decision retains the exact same responding-character identifier instance as
   the request, and the existing character-turn start policy accepts the current activity.
3. Add focused Domain tests for response, interjection, silence, exhausted and interrupted budgets,
   exact versus distinct value-equal responder identity, absent/awaiting/speaking/completed/cancelled
   current activity, null rejection, unchanged inputs, and the smallest public API surface.
4. Do not validate or resolve a participation round again, construct a request, transition a budget
   or activity, generate or infer decisions or addresses, inspect dialogue text, invoke a model,
   loop, wire the scene or player input, perform actions, or add hearing/UI/persistence/provider
   behavior.

Leave both ordinary participation/player-interruption integration and the unbounded-loop parent
item unticked until later ordinary-flow slices consume these pure eligibility seams.
