using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Verifies exact binding and acknowledgement of the applied follow-up Move result.</summary>
internal static class CharacterSabotageActionMoveAppliedResultBindingSmokeVerifier
{
    /// <summary>Checks substitute outcomes and acknowledges only the exact bound observation.</summary>
    internal static bool PassesMainScene(
        Node scene,
        TemporaryCharacterObjectSabotageMoveSource source,
        CharacterObjectSabotageMoveController state,
        RecordingMoveModel model,
        CharacterSabotageActionMoveCompositionStartSmokeVerifier.RecordingModel actionModel)
    {
        CharacterSabotageActionMoveAppliedResultObservationCoordinator observation =
            scene.GetNode<CharacterSabotageActionMoveAppliedResultObservationCoordinator>(
                "../CharacterSabotageActionMoveAppliedResultObservationCoordinator");
        CharacterSabotageActionMoveAppliedResultBindingCoordinator target =
            scene.GetNode<CharacterSabotageActionMoveAppliedResultBindingCoordinator>(
                "../CharacterSabotageActionMoveAppliedResultBindingCoordinator");
        CharacterObjectSabotageMoveTaskController task =
            scene.GetNode<CharacterObjectSabotageMoveTaskController>(
                "../ScheduledCharacterObjectSabotageMoveTaskController");
        if (observation.ObservedResult is not { } result
            || observation.ObservedPlacements is not { } placements
            || observation.ObservedEvidence is not { } evidence)
        {
            return false;
        }

        WorldObjectPlacementSnapshot rebuiltPlacements = new(placements.Entries);
        DiscoveryEvidenceSnapshot rebuiltEvidence = new(evidence.Entries);
        CharacterObjectSabotageMoveConsiderationResult rebuiltResult = new(
            result.Request,
            new CharacterObjectSabotageMoveDecision(
                result.Request.Character.Identity.Id,
                source.FollowUpCandidate.Evidence.Evidence.EvidenceId));
        if (target.BoundResult is not null
            || target.AcknowledgedResult is not null
            || target.Placements is not null
            || target.Evidence is not null
            || target.TryBindObservedResult(null, placements, evidence)
            || target.TryBindObservedResult(result, null, evidence)
            || target.TryBindObservedResult(result, placements, null)
            || target.TryBindObservedResult(rebuiltResult, placements, evidence)
            || target.TryBindObservedResult(result, rebuiltPlacements, evidence)
            || target.TryBindObservedResult(result, placements, rebuiltEvidence)
            || !target.TryBindObservedResult(result, placements, evidence)
            || !ReferenceEquals(target.BoundResult, result)
            || !ReferenceEquals(target.Placements, placements)
            || !ReferenceEquals(target.Evidence, evidence)
            || target.TryBindObservedResult(result, placements, evidence)
            || target.TryAcknowledgeBoundResult(null)
            || target.TryAcknowledgeBoundResult(rebuiltResult)
            || task.Status != CharacterObjectSabotageMoveTaskStatus.Completed
            || !ReferenceEquals(task.Result, result)
            || !ReferenceEquals(state.Placements, placements)
            || !ReferenceEquals(state.Evidence, evidence)
            || !target.TryAcknowledgeBoundResult(result)
            || !ReferenceEquals(target.AcknowledgedResult, result)
            || !ReferenceEquals(target.BoundResult, result)
            || !ReferenceEquals(target.Placements, placements)
            || !ReferenceEquals(target.Evidence, evidence)
            || observation.ObservedResult is not null
            || observation.ObservedPlacements is not null
            || observation.ObservedEvidence is not null
            || task.Status != CharacterObjectSabotageMoveTaskStatus.Idle
            || task.Result is not null
            || task.ResolvedPlacements is not null
            || task.ResolvedEvidence is not null
            || target.TryAcknowledgeBoundResult(result)
            || model.CallCount != 1
            || actionModel.CallCount != 1
            || !ReferenceEquals(state.Placements, placements)
            || !ReferenceEquals(state.Evidence, evidence))
        {
            return false;
        }

        return true;
    }
}
