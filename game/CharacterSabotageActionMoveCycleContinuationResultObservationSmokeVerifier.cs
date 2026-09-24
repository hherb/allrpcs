using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Verifies observation of the exact completed continuation Move result.</summary>
internal static class CharacterSabotageActionMoveCycleContinuationResultObservationSmokeVerifier
{
    /// <summary>Checks rejected non-current results and one exact completed result.</summary>
    internal static bool PassesMainScene(
        Node scene,
        CharacterObjectSabotageMoveController state,
        CharacterSabotageActionConsiderationRequest request,
        WorldObjectPlacementSnapshot recovered,
        DiscoveryEvidenceSnapshot evidence,
        CharacterSabotageActionMoveCycleContinuationCompositionStartSmokeVerifier.RecordingModel
            continuedModel,
        RecordingMoveModel model,
        CharacterSabotageActionMoveCycleCompositionStartSmokeVerifier.RecordingModel actionModel,
        CharacterSabotageActionMoveCompositionStartSmokeVerifier.RecordingModel priorActionModel,
        RecordingMoveModel priorMoveModel)
    {
        TemporaryCharacterObjectSabotageMoveSource source =
            scene.GetNode<TemporaryCharacterObjectSabotageMoveSource>(
                "../TemporaryCharacterObjectSabotageMoveSource");
        CharacterSabotageActionMoveCycleContinuationResultObservationCoordinator target =
            scene.GetNode<CharacterSabotageActionMoveCycleContinuationResultObservationCoordinator>(
                "../CharacterSabotageActionMoveCycleContinuationResultObservationCoordinator");
        CharacterSabotageActionTaskController task =
            scene.GetNode<CharacterSabotageActionTaskController>(
                "../ContinuedCharacterSabotageActionTaskController");
        CharacterSabotageActionConsiderationResult rebuiltResult = CreateResult(
            new CharacterSabotageActionConsiderationRequest(request.Actor, request.ActionKinds),
            CharacterSabotageActionKind.Move);
        CharacterSabotageActionConsiderationResult wrongKindResult = CreateResult(
            new CharacterSabotageActionConsiderationRequest(
                request.Actor,
                [CharacterSabotageActionKind.Conceal]),
            CharacterSabotageActionKind.Conceal);
        CharacterSabotageActionConsiderationResult foreignActorResult = CreateResult(
            new CharacterSabotageActionConsiderationRequest(
                RepresentativeEnemyAgentCharacterSource.Create(),
                [CharacterSabotageActionKind.Move]),
            CharacterSabotageActionKind.Move);

        if (target.ObservedResult is not null
            || target.TryObserveCompletedResult(null)
            || target.TryObserveCompletedResult(rebuiltResult)
            || target.TryObserveCompletedResult(foreignActorResult)
            || target.TryObserveCompletedResult(wrongKindResult)
            || task.Status != CharacterSabotageActionTaskStatus.Running
            || task.Result is not null
            || continuedModel.CallCount != 1)
        {
            return false;
        }

        task._Process(0);
        bool observedSuccessfully = task.Status == CharacterSabotageActionTaskStatus.Completed
            && task.Result is { } result
            && ReferenceEquals(result.Request, request)
            && ReferenceEquals(result.Request.Actor, source.Character)
            && ReferenceEquals(result.SelectedActionKind, CharacterSabotageActionKind.Move)
            && target.TryObserveCompletedResult(result)
            && ReferenceEquals(target.ObservedResult, result)
            && !target.TryObserveCompletedResult(result)
            && ReferenceEquals(state.Placements, recovered)
            && ReferenceEquals(state.Evidence, evidence)
            && continuedModel.CallCount == 1
            && model.CallCount == 1
            && actionModel.CallCount == 1
            && priorActionModel.CallCount == 1
            && priorMoveModel.CallCount == 1;
        return observedSuccessfully
            && CharacterSabotageActionMoveCycleContinuationResultBindingSmokeVerifier.PassesMainScene(
                scene,
                state,
                target.ObservedResult!,
                recovered,
                evidence,
                continuedModel,
                model,
                actionModel,
                priorActionModel,
                priorMoveModel);
    }

    private static CharacterSabotageActionConsiderationResult CreateResult(
        CharacterSabotageActionConsiderationRequest request,
        CharacterSabotageActionKind selectedKind) =>
        new(request, new CharacterSabotageActionDecision(request.Actor.Identity.Id, selectedKind));
}
