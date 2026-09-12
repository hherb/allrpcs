# Allrpcs Handover

## Current state

`main` contains merged PR #206 at `4b5573f`. The current branch is
`codex/phase-7-autonomous-followups`.

This branch advances the two still-open Phase 7 conversation parents through bounded Domain and
scene foundations:

- `AutonomousConversationTurnStartPolicy` permits an already-selected response or interjection
  only while the exact budget can continue, the decision and candidate request retain the same
  responder identifier instance, and the existing activity-start policy accepts the slot;
- `AutonomousConversationTurnStartTransition` starts that exact request as one awaiting activity
  without constructing a request or advancing the budget;
- `AutonomousConversationTurnCompletionTransition` preflights a speaking activity and open budget,
  then returns both an exact completed activity and one advanced budget in
  `AutonomousConversationTurnCompletionResult` without partial mutable state;
- `AutonomousConversationFollowUpRequestTransition` creates one request for an already-selected
  speaking listener, retaining its exact identifier and exact heard dialogue event while rejecting
  meaningful silence or an absent audience member;
- `AutonomousConversationBudgetController` now validates one complete caller-supplied participation
  round, selects its exact speaker under the scene-owned budget, derives the heard-event request,
  and starts it through the new gates;
- the same controller atomically installs exact autonomous completion state and advances its budget
  once; its lower-level composition helpers and the turn-controller installation seam remain
  internal;
- the ordinary observed-speech completion controller uses that atomic path whenever a budget is
  initialized, and ordinary player cancellation monotonically interrupts an initialized budget
  after cancelling its exact awaiting or speaking activity.

The roadmap parents remain open. Normal `Main` does not initialize an autonomous budget, schedule
participants, obtain participation decisions, or start a multi-character chain. The temporary
model input and character sources still accept only the authored healer. This branch does not
generate or infer participation decisions, create provider participation prompts, add model
adapters, add an embodied sage or hermit, change hearing candidates, loop, automatically schedule
speakers, execute intentions, alter transcript retention, add timeouts/retries, add whispering or
private conversations, add independent tasks or sabotage, start Phase 8, release, sign, or
distribute.

Draft PR #207 contains this work at implementation commit `95c31b6`.

## Validation and review

- focused autonomous follow-up Domain tests pass: 38 tests covering response/interjection starts,
  silence, exhausted/interrupted budgets, exact versus distinct value-equal responder identity,
  every activity state, immutable inputs, request derivation, unheard speakers, atomic completion,
  null rejection, and smallest public surfaces;
- test-first compile failures were observed before each missing Domain policy, transition, and
  result implementation;
- the focused scene smoke passes complete-round selection, exact heard-event request creation,
  gated start, atomic completion, one budget advance, monotonic interruption, invalid attempts,
  unchanged inputs, and the closed public controller surface;
- the ordinary completion-flow smoke passes with one initialized budget advance;
- both ordinary awaiting-turn and speaking-turn cancellation-input smokes pass with monotonic budget
  interruption and unchanged unrelated world, perception, movement, and presentation state;
- final host-local `./scripts/quality.sh` passed workflow validation, restore, formatting, and the
  zero-warning solution build; 2,049 Domain, 402 Application, 447 Infrastructure, and 8
  architecture tests; all 5 representative scenario definitions; the complete headless smoke
  matrix; and the bounded Compatibility-renderer launch;
- `git diff --check`, practical 500-line/120-character limits for new and materially changed files,
  immutable/exact-identity behavior, complete-round-before-selection, atomic completion, bounded
  loop closure, smallest public surface, network-free behavior, and scope review pass.

Restricted VSTest reproduced the known localhost-bind denial and restricted Godot reproduced the
known `user://logs` failure; the identical focused commands passed host-local. Recent hosted jobs
still had `steps: []` under the Actions account-budget condition. Draft PR #207 triggered run
`34680076640`; all five jobs completed with `steps: []` without executing checkout or test steps.
Do not retry or dispatch them unless that condition changes.

## Exact next bounded slice

Take **Phase 7s: provider-neutral participation-consideration request**.

Acceptance boundary:

1. Add one immutable Domain request retaining one exact scheduled `Character`, one exact heard
   `DialogueEvent`, and whether that character was directly addressed.
2. Require the character identifier to be present in the event audience by value while retaining
   the exact supplied character, event, and nested instances.
3. Add focused Domain tests for addressed and unaddressed listeners, distinct value-equal audience
   identity, absent listener rejection, null rejection, unchanged inputs, immutability, and the
   smallest public API surface.
4. Do not assemble model context, define or invoke a provider/model interface, generate a decision,
   validate or select a round, construct a character-turn request, start or complete activity,
   mutate a budget, loop, wire the scene, add roles/hearing/UI/persistence, or begin sabotage.

Leave both Phase 7 parent items unticked until later slices obtain one decision per scheduled
listener and drive every ordinary autonomous chain through the existing bounded scene seams.
