# Phase 3 Main coordinator extraction

Owner authority: the 18 October Phase 3 decision recorded for
`owner-code-health-phase3-2026-10-18` in `CONTINUATION.json`. This is program Phase 3 item 2 in
[the program plan](2026-10-17-code-health-and-playable-slice.md) (review §1.4 and §1.5).

## Scope and completion

Move the two state machines that `game/Main.cs` runs inline into `src/Application` under xUnit
tests, leave Main delegating through thin native adapters, make the order-dependent processing
explicit and check it. Behavior is unchanged: same gameplay, input precedence, status text,
exceptions, request/restore/cancellation ownership and once-only fencing.

Out of scope: SmokeCheckHost (item 3), the independent-task tick, healer commands, the layered
inscription input, first-patient and world initialization, any new gameplay, the deferred Phase 2
checks, the full root gate (due at Phase 3 exit) and any phase-exit claim.

## What moves (measured at `b0e390f5`)

**Conversation turn** (`Main.cs:138-144`, `650-877`, cancel block `883-890`, BUSY→READY status
`525-526`, restore owner `main-result-tracking`):

- submission acceptance (`CanAccept`);
- generation start for an awaiting LLM turn, including the context-budget refusal, exact
  participant/assignment/request checks and the fixed 30 s deadline;
- consumption of each terminal model result once: stale-input cancel, response reception and
  examination offer (or the conditional-collection scaffold), failure status and turn cancel;
- participation continuation after a completed turn whose speech occurred;
- player-dialogue occurrence once per delivery with hearing candidates (Seeker, visible healer,
  sage position), transcript append, and new-chain start with healer direct address;
- player cancel (examination, generation, round, turn, budget, in that order);
- session reset of the three once-only fences.

**Focused-search input** (`Main.cs:891-938`, `980-1096`): the cancel, complete, resolve and start
actions, the herb-patch world-event commit, wagon-clue routing, sabotage-evidence recovery,
success presentation and the `Focused search <ACTION>: CHANGED|UNCHANGED` status.

## Design

- `Allrpcs.Application.Dialogue.ConversationTurnCoordinator<TInput>` sequences the steps over
  `IConversationTurnPorts<TInput>`. Each port member forwards to exactly one existing node
  member. `TInput : ICharacterModelGenerationInput` keeps the input source's exact prepared
  input (and its currency fence) opaque to Application; `TemporaryCharacterModelGenerationInput`
  implements the one-property interface.
- `Allrpcs.Application.Input.FocusedSearchInputCoordinator` handles a
  `FocusedSearchInputAction` over `IFocusedSearchInputPorts`; sabotage recovery routing is a list
  of `FocusedSearchEvidenceRecovery` (evidence object, recover delegate) read at call time.
- `game/ConversationTurnNodePorts.cs` and `game/FocusedSearchInputNodePorts.cs` are plain C#
  adapters (not nodes), built at the end of `Main._Ready`. Main still calls the coordinators from
  `_Process` and `_UnhandledInput`, so the many checks that pause Main with `SetProcess(false)`
  keep pausing the same work, and input precedence is unchanged.
- Reflection-based checks: `DialogueOwnershipMainSmokeCheck` calls the coordinator's public steps
  through `Main.ConversationTurns` and injects its transcript fault on the adapter.
  `RefreshResolvedPerceptionPresentation` and `resolvedPerceptionIndicators` stay on Main.

## Process order (§1.5)

- Main gets an explicit `ProcessPriority` of -40 (`MainProcessOrder.MainPriority`): still after
  the four early coordinators/sources (-50…-43) and the -100 check fixtures, still before every
  default-priority child. The only relative change is against the `CheckDeadline` autoload
  watchdog, which shares no state with Main.
- The order-dependent children stay at default priority. Giving them non-zero priorities would
  reorder them against hundreds of interleaved default-priority siblings, which is a behavior
  change this slice does not authorize. Their contract is equal priority plus tree order, now
  written down in `MainProcessOrder.Dependencies` and asserted.
- `MoveChild(examinationAction, GetChildCount() - 2)` becomes an explicit placement immediately
  before `WholeChannelPresentation`. That is what `-2` meant when it was added in `fd0910b2`, and
  it still yields the same index.
- The standalone `MainProcessOrderCheck.tscn` (launched by `scripts/smoke-godot.sh`, so no
  node is added to `Main.tscn`) instantiates `Main.tscn` and asserts Main's priority, every
  declared (earlier, later) pair using Godot's rule (priority first, then tree order) and the
  examination action's adjacency.

## Tasks

- [x] Write this plan.
- [x] Add the conversation-turn ports and coordinator, with xUnit tests pinning each branch,
  ordering and once-only fence, the cancel order and session reset.
- [x] Add the focused-search ports and coordinator, with xUnit tests pinning each action,
  the herb-patch commit and prevalidation, wagon routing, recovery routing and status text.
- [x] Add both native adapters, delegate from Main, and update the reflection-based check.
- [x] Add `MainProcessOrder`, set Main's priority, anchor the examination action, add and
  register the order check.
- [x] Run format, build, all four test projects and the affected native launches. Review, sync
  the ledger and handover, then commit and push the review branch.

## Evidence

Under D2: `dotnet format --verify-no-changes`, the zero-warning build, all four xUnit projects,
and these native launches from `scripts/smoke-godot.sh`: dialogue ownership (all modes),
character-turn cancellation input, character-model generation flow, ordinary cancellation, live
ordinary flow, ordinary participation cancellation, character examination flow, focused-search
world event, interaction focus, wagon-clue route, the four water-cask sabotage investigation flows,
save-snapshot restoration ownership and the new process-order check.

## Outcome

Complete; see [the evidence record](../evidence/2026-10-18-phase3-main-coordinators.json).
`game/Main.cs` went from 1,123 to 730 lines. The 37 new xUnit cases and all 6,044 solution
tests pass. The full smoke suite passed 363 launches. Its one failure,
`ALLRPCS_ORDINARY_MODEL_FAILURE_CHECK=retry`, is intermittent on unmodified `main` too (3 of 10
runs) and is not caused by this change.
