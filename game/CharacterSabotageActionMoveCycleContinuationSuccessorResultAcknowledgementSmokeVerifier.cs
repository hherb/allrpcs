using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Verifies exact acknowledgement of the bound fifth-cycle Move result.</summary>
internal static class
    CharacterSabotageActionMoveCycleContinuationSuccessorResultAcknowledgementSmokeVerifier
{
    /// <summary>Checks substitute results and resets only the exact bound task once.</summary>
    internal static bool PassesMainScene(
        Node scene,
        CharacterObjectSabotageMoveController state,
        CharacterSabotageActionConsiderationResult result,
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
        CharacterSabotageActionMoveCycleContinuationSuccessorResultBindingCoordinator target =
            scene.GetNode<
                CharacterSabotageActionMoveCycleContinuationSuccessorResultBindingCoordinator>(
                    "../CharacterSabotageActionMoveCycleContinuationSuccessorResultBindingCoordinator");
        CharacterSabotageActionMoveCycleContinuationSuccessorResultObservationCoordinator
            observation = scene.GetNode<
                CharacterSabotageActionMoveCycleContinuationSuccessorResultObservationCoordinator>(
                    "../CharacterSabotageActionMoveCycleContinuationSuccessorResultObservationCoordinator");
        CharacterSabotageActionTaskController task =
            scene.GetNode<CharacterSabotageActionTaskController>(
                "../SuccessorCharacterSabotageActionTaskController");
        CharacterSabotageActionConsiderationResult rebuiltResult = new(
            result.Request,
            new CharacterSabotageActionDecision(
                result.Request.Actor.Identity.Id,
                CharacterSabotageActionKind.Move));

        return target.CandidateBinding is { } binding
            && target.AcknowledgedResult is null
            && !target.TryAcknowledgeBoundResult(null)
            && !target.TryAcknowledgeBoundResult(rebuiltResult)
            && target.TryAcknowledgeBoundResult(result)
            && ReferenceEquals(target.AcknowledgedResult, result)
            && observation.ObservedResult is null
            && task.Status == CharacterSabotageActionTaskStatus.Idle
            && task.Result is null
            && !target.TryAcknowledgeBoundResult(result)
            && target.IsRetainedAcknowledgedBinding(result, binding)
            && ReferenceEquals(state.Placements, recovered)
            && ReferenceEquals(state.Evidence, evidence)
            && model.CallCount == 1
            && continuedActionModel.CallCount == 1
            && continuedMoveModel.CallCount == 1
            && actionModel.CallCount == 1
            && priorActionModel.CallCount == 1
            && priorMoveModel.CallCount == 1;
    }
}
