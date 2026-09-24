using Allrpcs.Domain.Characters;
using Godot;

namespace Allrpcs.Game;

/// <summary>Verifies one-shot observation of the exact completed third-cycle Move result.</summary>
internal static class CharacterSabotageActionMoveCycleResultObservationSmokeVerifier
{
    /// <summary>Checks rejected non-current results and one exact completed result.</summary>
    internal static bool PassesMainScene(
        Node scene,
        TemporaryCharacterObjectSabotageMoveSource source,
        CharacterObjectSabotageMoveController state,
        CharacterSabotageActionMoveCycleCompositionStartSmokeVerifier.RecordingModel model,
        CharacterSabotageActionMoveCompositionStartSmokeVerifier.RecordingModel priorActionModel,
        RecordingMoveModel priorMoveModel)
    {
        CharacterSabotageActionMoveCycleCompositionCoordinator composition =
            scene.GetNode<CharacterSabotageActionMoveCycleCompositionCoordinator>(
                "../CharacterSabotageActionMoveCycleCompositionCoordinator");
        CharacterSabotageActionMoveCycleCompositionStartCoordinator start =
            scene.GetNode<CharacterSabotageActionMoveCycleCompositionStartCoordinator>(
                "../CharacterSabotageActionMoveCycleCompositionStartCoordinator");
        CharacterSabotageActionMoveCycleResultObservationCoordinator target =
            scene.GetNode<CharacterSabotageActionMoveCycleResultObservationCoordinator>(
                "../CharacterSabotageActionMoveCycleResultObservationCoordinator");
        CharacterSabotageActionTaskController task =
            scene.GetNode<CharacterSabotageActionTaskController>(
                "../CycledCharacterSabotageActionTaskController");
        if (composition.Request is not { } request
            || state.Placements is not { } placements
            || state.Evidence is not { } evidence)
        {
            return false;
        }

        CharacterSabotageActionConsiderationResult rebuiltResult = CreateResult(
            new CharacterSabotageActionConsiderationRequest(
                source.Character,
                [CharacterSabotageActionKind.Move]),
            CharacterSabotageActionKind.Move);
        CharacterSabotageActionConsiderationResult foreignActorResult = CreateResult(
            new CharacterSabotageActionConsiderationRequest(
                RepresentativeEnemyAgentCharacterSource.Create(),
                [CharacterSabotageActionKind.Move]),
            CharacterSabotageActionKind.Move);
        CharacterSabotageActionConsiderationResult wrongKindResult = CreateResult(
            new CharacterSabotageActionConsiderationRequest(
                source.Character,
                [CharacterSabotageActionKind.Conceal]),
            CharacterSabotageActionKind.Conceal);

        if (target.ObservedResult is not null
            || target.TryObserveCompletedResult(null)
            || target.TryObserveCompletedResult(rebuiltResult)
            || target.TryObserveCompletedResult(foreignActorResult)
            || target.TryObserveCompletedResult(wrongKindResult)
            || task.Status != CharacterSabotageActionTaskStatus.Running
            || task.Result is not null
            || model.CallCount != 1
            || priorActionModel.CallCount != 1
            || priorMoveModel.CallCount != 1
            || !ReferenceEquals(state.Placements, placements)
            || !ReferenceEquals(state.Evidence, evidence))
        {
            return false;
        }

        task._Process(0);
        if (task.Status != CharacterSabotageActionTaskStatus.Completed
            || task.Result is not { } result
            || !ReferenceEquals(result.Request, request)
            || !ReferenceEquals(result.Request.Actor, source.Character)
            || !ReferenceEquals(result.SelectedActionKind, CharacterSabotageActionKind.Move)
            || !target.TryObserveCompletedResult(result)
            || !ReferenceEquals(target.ObservedResult, result)
            || target.TryObserveCompletedResult(result)
            || task.Status != CharacterSabotageActionTaskStatus.Completed
            || !ReferenceEquals(task.Result, result)
            || model.CallCount != 1
            || priorActionModel.CallCount != 1
            || priorMoveModel.CallCount != 1
            || !ReferenceEquals(start.StartedRequest, request)
            || !ReferenceEquals(composition.Request, request)
            || !ReferenceEquals(state.Placements, placements)
            || !ReferenceEquals(state.Evidence, evidence))
        {
            return false;
        }

        return CharacterSabotageActionMoveCycleResultBindingSmokeVerifier.PassesMainScene(
            scene,
            source,
            state,
            model,
            priorActionModel,
            priorMoveModel);
    }

    private static CharacterSabotageActionConsiderationResult CreateResult(
        CharacterSabotageActionConsiderationRequest request,
        CharacterSabotageActionKind selectedKind) =>
        new(request, new CharacterSabotageActionDecision(request.Actor.Identity.Id, selectedKind));
}
