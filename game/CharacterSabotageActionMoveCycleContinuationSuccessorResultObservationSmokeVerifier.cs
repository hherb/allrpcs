using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Verifies observation of the exact completed fifth-cycle generic Move result.</summary>
internal static class
    CharacterSabotageActionMoveCycleContinuationSuccessorResultObservationSmokeVerifier
{
    /// <summary>Checks rejected non-current results and one exact completed result.</summary>
    internal static bool PassesMainScene(
        Node scene,
        CharacterObjectSabotageMoveController state,
        CharacterSabotageActionConsiderationRequest request,
        WorldObjectPlacementSnapshot recovered,
        DiscoveryEvidenceSnapshot evidence,
        CharacterSabotageActionMoveCycleContinuationSuccessorCompositionStartSmokeVerifier
            .RecordingModel model,
        CharacterSabotageActionMoveCycleContinuationCompositionStartSmokeVerifier.RecordingModel
            continuedActionModel,
        RecordingMoveModel continuedMoveModel,
        CharacterSabotageActionMoveCycleCompositionStartSmokeVerifier.RecordingModel actionModel,
        CharacterSabotageActionMoveCompositionStartSmokeVerifier.RecordingModel priorActionModel,
        RecordingMoveModel priorMoveModel)
    {
        CharacterSabotageActionMoveCycleContinuationSuccessorResultObservationCoordinator target =
            scene.GetNode<
                CharacterSabotageActionMoveCycleContinuationSuccessorResultObservationCoordinator>(
                    "../CharacterSabotageActionMoveCycleContinuationSuccessorResultObservationCoordinator");
        CharacterSabotageActionTaskController task =
            scene.GetNode<CharacterSabotageActionTaskController>(
                "../SuccessorCharacterSabotageActionTaskController");
        CharacterSabotageActionConsiderationResult rebuiltResult = CreateResult(
            new CharacterSabotageActionConsiderationRequest(request.Actor, request.ActionKinds),
            CharacterSabotageActionKind.Move);
        CharacterSabotageActionConsiderationResult foreignActorResult = CreateResult(
            new CharacterSabotageActionConsiderationRequest(
                RepresentativeEnemyAgentCharacterSource.Create(),
                [CharacterSabotageActionKind.Move]),
            CharacterSabotageActionKind.Move);
        CharacterSabotageActionConsiderationResult wrongKindResult = CreateResult(
            new CharacterSabotageActionConsiderationRequest(
                request.Actor,
                [CharacterSabotageActionKind.Conceal]),
            CharacterSabotageActionKind.Conceal);

        if (target.ObservedResult is not null
            || target.TryObserveCompletedResult(null)
            || target.TryObserveCompletedResult(rebuiltResult)
            || target.TryObserveCompletedResult(foreignActorResult)
            || target.TryObserveCompletedResult(wrongKindResult)
            || task.Status != CharacterSabotageActionTaskStatus.Running
            || task.Result is not null
            || model.CallCount != 1)
        {
            return false;
        }

        task._Process(0);
        CharacterSabotageActionConsiderationResult? result = task.Result;
        return task.Status == CharacterSabotageActionTaskStatus.Completed
            && result is not null
            && ReferenceEquals(result.Request, request)
            && ReferenceEquals(result.SelectedActionKind, CharacterSabotageActionKind.Move)
            && target.TryObserveCompletedResult(result)
            && ReferenceEquals(target.ObservedResult, result)
            && !target.TryObserveCompletedResult(result)
            && ReferenceEquals(state.Placements, recovered)
            && ReferenceEquals(state.Evidence, evidence)
            && model.CallCount == 1
            && continuedActionModel.CallCount == 1
            && continuedMoveModel.CallCount == 1
            && actionModel.CallCount == 1
            && priorActionModel.CallCount == 1
            && priorMoveModel.CallCount == 1;
    }

    private static CharacterSabotageActionConsiderationResult CreateResult(
        CharacterSabotageActionConsiderationRequest request,
        CharacterSabotageActionKind selectedKind) =>
        new(request, new CharacterSabotageActionDecision(request.Actor.Identity.Id, selectedKind));
}
