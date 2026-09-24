using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Verifies explicit start of only the exact typed continuation Move request.</summary>
internal static class CharacterSabotageActionMoveCycleContinuationSpecificRequestStartSmokeVerifier
{
    /// <summary>Checks rejected substitutes and one explicit fresh typed start.</summary>
    internal static bool PassesMainScene(
        Node scene,
        CharacterObjectSabotageMoveController state,
        CharacterObjectSabotageMoveConsiderationRequest request,
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
        CharacterSabotageActionMoveCycleContinuationSpecificRequestStartCoordinator target =
            scene.GetNode<
                CharacterSabotageActionMoveCycleContinuationSpecificRequestStartCoordinator>(
                    "../CharacterSabotageActionMoveCycleContinuationSpecificRequestStartCoordinator");
        TemporaryCharacterObjectSabotageMoveModelSource models =
            scene.GetNode<TemporaryCharacterObjectSabotageMoveModelSource>(
                "../ContinuedTemporaryCharacterObjectSabotageMoveModelSource");
        CharacterObjectSabotageMoveTaskController task =
            scene.GetNode<CharacterObjectSabotageMoveTaskController>(
                "../ContinuedCharacterObjectSabotageMoveTaskController");
        CharacterObjectSabotageMoveConsiderationRequest rebuiltRequest = new(
            source.Character,
            source.PhysicalAccess,
            source.WorldObjects,
            recovered,
            evidence,
            [source.ContinuationCandidate]);
        RecordingMoveModel continuedMoveModel = new();

        bool startedSuccessfully = models.TrySupplyModel(continuedMoveModel)
            && target.StartedRequest is null
            && task.Status == CharacterObjectSabotageMoveTaskStatus.Idle
            && !target.TryStart(null)
            && !target.TryStart(rebuiltRequest)
            && continuedMoveModel.CallCount == 0
            && target.TryStart(request)
            && ReferenceEquals(target.StartedRequest, request)
            && task.Status == CharacterObjectSabotageMoveTaskStatus.Running
            && task.Result is null
            && continuedMoveModel.CallCount == 1
            && !target.TryStart(request)
            && ReferenceEquals(state.Placements, recovered)
            && ReferenceEquals(state.Evidence, evidence)
            && continuedModel.CallCount == 1
            && model.CallCount == 1
            && actionModel.CallCount == 1
            && priorActionModel.CallCount == 1
            && priorMoveModel.CallCount == 1;
        return startedSuccessfully
            && CharacterSabotageActionMoveCycleContinuationAppliedResultObservationSmokeVerifier
                .PassesMainScene(
                    scene,
                    source,
                    state,
                    continuedModel,
                    model,
                    actionModel,
                    priorActionModel,
                    priorMoveModel);
    }
}
