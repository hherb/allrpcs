using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Verifies binding and acknowledgement of the applied fifth-cycle Move result.</summary>
internal static class
    CharacterSabotageActionMoveCycleContinuationSuccessorAppliedResultBindingSmokeVerifier
{
    /// <summary>Checks substitutes and acknowledges only the exact bound observation.</summary>
    internal static bool PassesMainScene(
        Node scene,
        CharacterObjectSabotageMoveController state,
        CharacterObjectSabotageMoveConsiderationResult result,
        WorldObjectPlacementSnapshot placements,
        DiscoveryEvidenceSnapshot evidence,
        CharacterSabotageActionMoveCycleContinuationSuccessorCompositionStartSmokeVerifier
            .RecordingModel actionModel,
        CharacterSabotageActionMoveCycleContinuationCompositionStartSmokeVerifier.RecordingModel
            continuedActionModel,
        RecordingMoveModel continuedMoveModel,
        CharacterSabotageActionMoveCycleCompositionStartSmokeVerifier.RecordingModel cycleActionModel,
        CharacterSabotageActionMoveCompositionStartSmokeVerifier.RecordingModel priorActionModel,
        RecordingMoveModel priorMoveModel,
        RecordingMoveModel moveModel)
    {
        CharacterSabotageActionMoveCycleContinuationSuccessorAppliedResultObservationCoordinator
            observation = scene.GetNode<
                CharacterSabotageActionMoveCycleContinuationSuccessorAppliedResultObservationCoordinator>(
                    "../CharacterSabotageActionMoveCycleContinuationSuccessorAppliedResultObservationCoordinator");
        CharacterSabotageActionMoveCycleContinuationSuccessorAppliedResultBindingCoordinator target =
            scene.GetNode<
                CharacterSabotageActionMoveCycleContinuationSuccessorAppliedResultBindingCoordinator>(
                    "../CharacterSabotageActionMoveCycleContinuationSuccessorAppliedResultBindingCoordinator");
        CharacterObjectSabotageMoveTaskController task =
            scene.GetNode<CharacterObjectSabotageMoveTaskController>(
                "../SuccessorCharacterObjectSabotageMoveTaskController");
        WorldObjectPlacementSnapshot rebuiltPlacements = new(placements.Entries);
        DiscoveryEvidenceSnapshot rebuiltEvidence = new(evidence.Entries);
        CharacterObjectSabotageMoveConsiderationResult rebuiltResult = new(
            result.Request,
            new CharacterObjectSabotageMoveDecision(
                result.Request.Character.Identity.Id,
                result.SelectedCandidate!.Evidence.Evidence.EvidenceId));

        return target.BoundResult is null
            && target.AcknowledgedResult is null
            && target.Placements is null
            && target.Evidence is null
            && !target.TryBindObservedResult(null, placements, evidence)
            && !target.TryBindObservedResult(result, null, evidence)
            && !target.TryBindObservedResult(result, placements, null)
            && !target.TryBindObservedResult(rebuiltResult, placements, evidence)
            && !target.TryBindObservedResult(result, rebuiltPlacements, evidence)
            && !target.TryBindObservedResult(result, placements, rebuiltEvidence)
            && target.TryBindObservedResult(result, placements, evidence)
            && ReferenceEquals(target.BoundResult, result)
            && ReferenceEquals(target.Placements, placements)
            && ReferenceEquals(target.Evidence, evidence)
            && !target.TryBindObservedResult(result, placements, evidence)
            && !target.TryAcknowledgeBoundResult(null)
            && !target.TryAcknowledgeBoundResult(rebuiltResult)
            && task.Status == CharacterObjectSabotageMoveTaskStatus.Completed
            && ReferenceEquals(task.Result, result)
            && target.TryAcknowledgeBoundResult(result)
            && ReferenceEquals(target.AcknowledgedResult, result)
            && ReferenceEquals(target.BoundResult, result)
            && ReferenceEquals(target.Placements, placements)
            && ReferenceEquals(target.Evidence, evidence)
            && observation.ObservedResult is null
            && observation.ObservedPlacements is null
            && observation.ObservedEvidence is null
            && task.Status == CharacterObjectSabotageMoveTaskStatus.Idle
            && task.Result is null
            && task.ResolvedPlacements is null
            && task.ResolvedEvidence is null
            && !target.TryAcknowledgeBoundResult(result)
            && target.IsCurrentAcknowledgedOutcome(result, placements, evidence)
            && actionModel.CallCount == 1
            && continuedActionModel.CallCount == 1
            && continuedMoveModel.CallCount == 1
            && cycleActionModel.CallCount == 1
            && priorActionModel.CallCount == 1
            && priorMoveModel.CallCount == 1
            && moveModel.CallCount == 1
            && ReferenceEquals(state.Placements, placements)
            && ReferenceEquals(state.Evidence, evidence);
    }
}
