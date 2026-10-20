# Phase 3 thin Main and exit gate

Owner authority: the 18 October Phase 3 decision recorded for
`owner-code-health-phase3-2026-10-18` in `CONTINUATION.json`. This is the last Phase 3 item in
[the program plan](2026-10-17-code-health-and-playable-slice.md): `game/Main.cs` under 400 lines
with gameplay unchanged, then one full root `./quality.sh` for the phase exit.

## Scope and completion

Out of scope: the closed structured-decision, coordinator and SmokeCheckHost scopes, the deferred
Phase 2 live checks, new gameplay or sabotage kinds, Phase 4 and release.

Done when Main.cs is under 400 lines, `dotnet format`/`build`/all four test projects and the full
smoke suite pass, and one full `./quality.sh` passes and is recorded in
`docs/superpowers/evidence/2026-10-18-phase3-exit-quality.json`.

## What Main still does inline (measured at `ce20efd5`, 732 lines)

| Lines | Work | Kind |
|---|---|---|
| 42-183 | 46 node fields and their `GetNode` lookups | composition |
| 184-260, 322-503 | stream/flow/dialogue/transcript/first-patient/world-event/sabotage/world-object/independent-task/perception initialization | one-shot authored state |
| 513-631 | independent-task tick: outbound vs return selection, reached waypoint, candidate occurrence, bounded elapsed, round-trip advance, party body presentation | per-frame state machine |
| 657-673 | layered-inscription discover then environmental completion | input forward |
| 674-688, 709-731 | healer follow/wait toggle, move command, mode status text | input + presentation |

Reflection contracts kept on Main: `resolvedPerceptionIndicators` and
`RefreshResolvedPerceptionPresentation` (`SaveSnapshotRestorationOwnershipSmokeCheck`,
`InteractionFocusSmokeCheck`); `_UnhandledInput`, `_Process` and `ConversationTurns` stay public
surface for checks.

## Moves

1. **Independent-task tick** → `Allrpcs.Application.Navigation.CharacterIndependentTaskTickCoordinator`
   over `ICharacterIndependentTaskTickPorts`, with xUnit tests. Same read order, same exception
   texts and order, same early return once return travel is complete (frame time is validated
   only after that), same party-presentation rule (only when the party snapshot instance changed).
   `game/CharacterIndependentTaskTickNodePorts.cs` forwards to the existing controllers.
2. **Healer mode status text** → `Allrpcs.Application.Movement.HealerModeStatusText`, pure, with
   tests. Main keeps the change-detection field and the label write.
3. **Layered inscription** → the environmental-flow node gains `TryCompleteRevealed()`, which
   assembles the exact arguments Main assembled; Main keeps the discover-then-complete dispatch.
4. **Authored-state initialization** → `game/MainSceneInitializer.cs`, a static composition helper
   in two calls that keep the exact order around the `HerbExaminationActionController` insertion
   (`InitializeBeforeExamination`, `InitializeAfterExamination`). Every fail-fast exception text is
   unchanged. Fields used only during initialization become initializer lookups.

No `MainProcessOrder` change: Main's priority and the examination controller's sibling index are
unchanged, so `MainProcessOrderCheck` stays valid.

## Verification

D2 scoped checks per move (`dotnet format`, `build`, the four test projects, the affected smoke
launches); then `./scripts/smoke-godot.sh` in full and one root `./quality.sh` for the phase exit.
