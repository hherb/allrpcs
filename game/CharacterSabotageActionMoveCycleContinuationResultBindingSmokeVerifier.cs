using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Verifies exact binding of the observed continuation Move result.</summary>
internal static class CharacterSabotageActionMoveCycleContinuationResultBindingSmokeVerifier
{
    /// <summary>Checks rejected substitute results and one exact unacknowledged binding.</summary>
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
        TemporaryCharacterObjectSabotageMoveSource source =
            scene.GetNode<TemporaryCharacterObjectSabotageMoveSource>(
                "../TemporaryCharacterObjectSabotageMoveSource");
        CharacterSabotageActionMoveCycleContinuationResultBindingCoordinator target =
            scene.GetNode<CharacterSabotageActionMoveCycleContinuationResultBindingCoordinator>(
                "../CharacterSabotageActionMoveCycleContinuationResultBindingCoordinator");
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
            && ReferenceEquals(binding.Candidate, source.ContinuationCandidate)
            && ReferenceEquals(binding.ActionKind, CharacterSabotageActionKind.Move)
            && !target.TryBindObservedResult(result)
            && target.IsCurrentBoundResult(result, binding)
            && ReferenceEquals(state.Placements, recovered)
            && ReferenceEquals(state.Evidence, evidence)
            && continuedModel.CallCount == 1
            && model.CallCount == 1
            && actionModel.CallCount == 1
            && priorActionModel.CallCount == 1
            && priorMoveModel.CallCount == 1;
        return boundSuccessfully
            && CharacterSabotageActionMoveCycleContinuationResultAcknowledgementSmokeVerifier
                .PassesMainScene(
                    scene,
                    state,
                    result,
                    recovered,
                    evidence,
                    continuedModel,
                    model,
                    actionModel,
                    priorActionModel,
                    priorMoveModel);
    }
}
