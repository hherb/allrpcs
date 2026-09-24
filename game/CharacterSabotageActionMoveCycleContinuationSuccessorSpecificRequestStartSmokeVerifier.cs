using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Verifies explicit start of only the exact typed fifth-cycle Move request.</summary>
internal static class
    CharacterSabotageActionMoveCycleContinuationSuccessorSpecificRequestStartSmokeVerifier
{
    /// <summary>Checks rejected substitutes and one explicit fresh typed start.</summary>
    internal static bool PassesMainScene(
        Node scene,
        CharacterObjectSabotageMoveController state,
        CharacterObjectSabotageMoveConsiderationRequest request,
        WorldObjectPlacementSnapshot recovered,
        DiscoveryEvidenceSnapshot evidence,
        CharacterSabotageActionMoveCycleContinuationSuccessorCompositionStartSmokeVerifier
            .RecordingModel actionModel,
        CharacterSabotageActionMoveCycleContinuationCompositionStartSmokeVerifier.RecordingModel
            continuedActionModel,
        RecordingMoveModel continuedMoveModel,
        CharacterSabotageActionMoveCycleCompositionStartSmokeVerifier.RecordingModel cycleActionModel,
        CharacterSabotageActionMoveCompositionStartSmokeVerifier.RecordingModel priorActionModel,
        RecordingMoveModel priorMoveModel)
    {
        TemporaryCharacterObjectSabotageMoveSource source =
            scene.GetNode<TemporaryCharacterObjectSabotageMoveSource>(
                "../TemporaryCharacterObjectSabotageMoveSource");
        CharacterSabotageActionMoveCycleContinuationSuccessorSpecificRequestStartCoordinator target =
            scene.GetNode<
                CharacterSabotageActionMoveCycleContinuationSuccessorSpecificRequestStartCoordinator>(
                    "../CharacterSabotageActionMoveCycleContinuationSuccessorSpecificRequestStartCoordinator");
        TemporaryCharacterObjectSabotageMoveModelSource models =
            scene.GetNode<TemporaryCharacterObjectSabotageMoveModelSource>(
                "../SuccessorTemporaryCharacterObjectSabotageMoveModelSource");
        CharacterObjectSabotageMoveTaskController task =
            scene.GetNode<CharacterObjectSabotageMoveTaskController>(
                "../SuccessorCharacterObjectSabotageMoveTaskController");
        CharacterObjectSabotageMoveConsiderationRequest rebuiltRequest = new(
            source.Character,
            source.PhysicalAccess,
            source.WorldObjects,
            recovered,
            evidence,
            [source.SuccessorCandidate]);
        RecordingMoveModel moveModel = new();

        bool started = models.TrySupplyModel(moveModel)
            && target.StartedRequest is null
            && task.Status == CharacterObjectSabotageMoveTaskStatus.Idle
            && !target.TryStart(null)
            && !target.TryStart(rebuiltRequest)
            && moveModel.CallCount == 0
            && target.TryStart(request)
            && ReferenceEquals(target.StartedRequest, request)
            && task.Status == CharacterObjectSabotageMoveTaskStatus.Running
            && task.Result is null
            && moveModel.CallCount == 1
            && !target.TryStart(request)
            && ReferenceEquals(state.Placements, recovered)
            && ReferenceEquals(state.Evidence, evidence)
            && actionModel.CallCount == 1
            && continuedActionModel.CallCount == 1
            && continuedMoveModel.CallCount == 1
            && cycleActionModel.CallCount == 1
            && priorActionModel.CallCount == 1
            && priorMoveModel.CallCount == 1;
        return started
            && CharacterSabotageActionMoveCycleContinuationSuccessorAppliedResultObservationSmokeVerifier
                .PassesMainScene(
                    scene,
                    source,
                    state,
                    actionModel,
                    continuedActionModel,
                    continuedMoveModel,
                    cycleActionModel,
                    priorActionModel,
                    priorMoveModel,
                    moveModel);
    }
}
