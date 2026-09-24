using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Verifies exact binding and acknowledgement of the applied third-cycle Move result.</summary>
internal static class CharacterSabotageActionMoveCycleAppliedResultBindingSmokeVerifier
{
    /// <summary>Checks substitute inputs and acknowledges only the exact bound observation.</summary>
    internal static bool PassesMainScene(
        Node scene,
        TemporaryCharacterObjectSabotageMoveSource source,
        CharacterObjectSabotageMoveController state,
        CharacterObjectSabotageMoveConsiderationResult result,
        WorldObjectPlacementSnapshot placements,
        DiscoveryEvidenceSnapshot evidence,
        RecordingMoveModel model,
        CharacterSabotageActionMoveCycleCompositionStartSmokeVerifier.RecordingModel actionModel,
        CharacterSabotageActionMoveCompositionStartSmokeVerifier.RecordingModel priorActionModel,
        RecordingMoveModel priorMoveModel)
    {
        CharacterSabotageActionMoveCycleAppliedResultObservationCoordinator observation =
            scene.GetNode<CharacterSabotageActionMoveCycleAppliedResultObservationCoordinator>(
                "../CharacterSabotageActionMoveCycleAppliedResultObservationCoordinator");
        CharacterSabotageActionMoveCycleAppliedResultBindingCoordinator target =
            scene.GetNode<CharacterSabotageActionMoveCycleAppliedResultBindingCoordinator>(
                "../CharacterSabotageActionMoveCycleAppliedResultBindingCoordinator");
        CharacterObjectSabotageMoveTaskController task =
            scene.GetNode<CharacterObjectSabotageMoveTaskController>(
                "../CycledCharacterObjectSabotageMoveTaskController");
        WorldObjectPlacementSnapshot rebuiltPlacements = new(placements.Entries);
        DiscoveryEvidenceSnapshot rebuiltEvidence = new(evidence.Entries);
        CharacterObjectSabotageMoveConsiderationResult rebuiltResult = new(
            result.Request,
            new CharacterObjectSabotageMoveDecision(
                result.Request.Character.Identity.Id,
                source.CycleCandidate.Evidence.Evidence.EvidenceId));

        bool acknowledged = target.BoundResult is null
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
            && model.CallCount == 1
            && actionModel.CallCount == 1
            && priorActionModel.CallCount == 1
            && priorMoveModel.CallCount == 1
            && ReferenceEquals(state.Placements, placements)
            && ReferenceEquals(state.Evidence, evidence);
        return acknowledged
            && CharacterSabotageActionMoveCycleRecoveryCandidateSmokeVerifier.PassesMainScene(
                scene,
                source,
                state,
                result,
                placements,
                evidence,
                model,
                actionModel,
                priorActionModel,
                priorMoveModel);
    }
}
