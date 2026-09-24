using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Verifies exact acknowledgement of the bound continuation Move result.</summary>
internal static class CharacterSabotageActionMoveCycleContinuationResultAcknowledgementSmokeVerifier
{
    /// <summary>Checks substitute results and resets only the exact bound task once.</summary>
    internal static bool PassesMainScene(
        Node scene,
        CharacterObjectSabotageMoveController state,
        CharacterSabotageActionConsiderationResult result,
        WorldObjectPlacementSnapshot recovered,
        DiscoveryEvidenceSnapshot evidence,
        CharacterSabotageActionMoveCycleContinuationCompositionStartSmokeVerifier.RecordingModel
            continuedModel,
        RecordingMoveModel model,
        CharacterSabotageActionMoveCycleCompositionStartSmokeVerifier.RecordingModel actionModel,
        CharacterSabotageActionMoveCompositionStartSmokeVerifier.RecordingModel priorActionModel,
        RecordingMoveModel priorMoveModel)
    {
        CharacterSabotageActionMoveCycleContinuationResultBindingCoordinator target =
            scene.GetNode<CharacterSabotageActionMoveCycleContinuationResultBindingCoordinator>(
                "../CharacterSabotageActionMoveCycleContinuationResultBindingCoordinator");
        CharacterSabotageActionMoveCycleContinuationResultObservationCoordinator observation =
            scene.GetNode<CharacterSabotageActionMoveCycleContinuationResultObservationCoordinator>(
                "../CharacterSabotageActionMoveCycleContinuationResultObservationCoordinator");
        CharacterSabotageActionTaskController task =
            scene.GetNode<CharacterSabotageActionTaskController>(
                "../ContinuedCharacterSabotageActionTaskController");
        CharacterSabotageActionConsiderationResult rebuiltResult = new(
            result.Request,
            new CharacterSabotageActionDecision(
                result.Request.Actor.Identity.Id,
                CharacterSabotageActionKind.Move));

        bool acknowledgedSuccessfully = target.CandidateBinding is { } binding
            && target.AcknowledgedResult is null
            && !target.TryAcknowledgeBoundResult(null)
            && !target.TryAcknowledgeBoundResult(rebuiltResult)
            && target.TryAcknowledgeBoundResult(result)
            && ReferenceEquals(target.AcknowledgedResult, result)
            && observation.ObservedResult is null
            && task.Status == CharacterSabotageActionTaskStatus.Idle
            && task.Result is null
            && !target.TryAcknowledgeBoundResult(result)
            && target.IsCurrentAcknowledgedBinding(result, binding)
            && ReferenceEquals(state.Placements, recovered)
            && ReferenceEquals(state.Evidence, evidence)
            && continuedModel.CallCount == 1
            && model.CallCount == 1
            && actionModel.CallCount == 1
            && priorActionModel.CallCount == 1
            && priorMoveModel.CallCount == 1;
        return acknowledgedSuccessfully
            && CharacterSabotageActionMoveCycleContinuationSpecificRequestSmokeVerifier
                .PassesMainScene(
                    scene,
                    state,
                    recovered,
                    evidence,
                    continuedModel,
                    model,
                    actionModel,
                    priorActionModel,
                    priorMoveModel);
    }
}
