using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Verifies exact binding of the observed fifth-cycle generic Move result.</summary>
internal static class
    CharacterSabotageActionMoveCycleContinuationSuccessorResultBindingSmokeVerifier
{
    /// <summary>Checks rejected substitute results and one exact unacknowledged binding.</summary>
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
        TemporaryCharacterObjectSabotageMoveSource source =
            scene.GetNode<TemporaryCharacterObjectSabotageMoveSource>(
                "../TemporaryCharacterObjectSabotageMoveSource");
        CharacterSabotageActionMoveCycleContinuationSuccessorResultObservationCoordinator
            observation = scene.GetNode<
                CharacterSabotageActionMoveCycleContinuationSuccessorResultObservationCoordinator>(
                    "../CharacterSabotageActionMoveCycleContinuationSuccessorResultObservationCoordinator");
        CharacterSabotageActionMoveCycleContinuationSuccessorResultBindingCoordinator target =
            scene.GetNode<
                CharacterSabotageActionMoveCycleContinuationSuccessorResultBindingCoordinator>(
                    "../CharacterSabotageActionMoveCycleContinuationSuccessorResultBindingCoordinator");
        CharacterSabotageActionTaskController task =
            scene.GetNode<CharacterSabotageActionTaskController>(
                "../SuccessorCharacterSabotageActionTaskController");
        CharacterSabotageActionConsiderationResult rebuiltResult = new(
            result.Request,
            new CharacterSabotageActionDecision(
                result.Request.Actor.Identity.Id,
                CharacterSabotageActionKind.Move));

        bool boundSuccessfully = target.BoundResult is null
            && target.CandidateBinding is null
            && !target.TryBindObservedResult(null)
            && !target.TryBindObservedResult(rebuiltResult)
            && target.TryBindObservedResult(result)
            && ReferenceEquals(target.BoundResult, result)
            && target.CandidateBinding is { } binding
            && ReferenceEquals(binding.Request,
                scene.GetNode<CharacterSabotageActionMoveRequestController>(
                    "../SuccessorCharacterSabotageActionMoveRequestController").Request)
            && ReferenceEquals(binding.ActionKind, CharacterSabotageActionKind.Move)
            && ReferenceEquals(binding.Candidate, source.SuccessorCandidate)
            && !target.TryBindObservedResult(result)
            && target.IsCurrentBoundResult(result, binding)
            && ReferenceEquals(observation.ObservedResult, result)
            && task.Status == CharacterSabotageActionTaskStatus.Completed
            && ReferenceEquals(task.Result, result)
            && ReferenceEquals(state.Placements, recovered)
            && ReferenceEquals(state.Evidence, evidence)
            && model.CallCount == 1
            && continuedActionModel.CallCount == 1
            && continuedMoveModel.CallCount == 1
            && actionModel.CallCount == 1
            && priorActionModel.CallCount == 1
            && priorMoveModel.CallCount == 1;
        return boundSuccessfully
            && CharacterSabotageActionMoveCycleContinuationSuccessorResultAcknowledgementSmokeVerifier
                .PassesMainScene(
                    scene,
                    state,
                    result,
                    recovered,
                    evidence,
                    model,
                    continuedActionModel,
                    continuedMoveModel,
                    actionModel,
                    priorActionModel,
                    priorMoveModel);
    }
}
