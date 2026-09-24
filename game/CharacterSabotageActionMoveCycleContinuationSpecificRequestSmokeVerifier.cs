using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Verifies typed Move composition from the acknowledged continuation result.</summary>
internal static class CharacterSabotageActionMoveCycleContinuationSpecificRequestSmokeVerifier
{
    /// <summary>Checks substitute provenance and one fresh exact typed request.</summary>
    internal static bool PassesMainScene(
        Node scene,
        CharacterObjectSabotageMoveController state,
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
        CharacterSabotageActionMoveCycleContinuationResultBindingCoordinator binding =
            scene.GetNode<CharacterSabotageActionMoveCycleContinuationResultBindingCoordinator>(
                "../CharacterSabotageActionMoveCycleContinuationResultBindingCoordinator");
        CharacterSabotageActionMoveCycleContinuationSpecificRequestCoordinator target =
            scene.GetNode<CharacterSabotageActionMoveCycleContinuationSpecificRequestCoordinator>(
                "../CharacterSabotageActionMoveCycleContinuationSpecificRequestCoordinator");
        if (binding.AcknowledgedResult is not { } result
            || binding.CandidateBinding is not { } candidateBinding)
        {
            return false;
        }

        CharacterSabotageActionConsiderationResult rebuiltResult = new(
            result.Request,
            new CharacterSabotageActionDecision(
                result.Request.Actor.Identity.Id,
                CharacterSabotageActionKind.Move));
        CharacterSabotageActionCandidateBinding<CharacterObjectSabotageMoveCandidate>
            rebuiltBinding = new(
                candidateBinding.Request,
                CharacterSabotageActionKind.Move,
                candidateBinding.Candidate);
        CharacterSabotageActionCandidateBinding<CharacterObjectSabotageMoveCandidate>
            wrongCandidateBinding = new(
                candidateBinding.Request,
                CharacterSabotageActionKind.Move,
                source.CycleCandidate);

        bool composedSuccessfully = target.Request is null
            && !target.TryCompose(null, candidateBinding)
            && !target.TryCompose(result, null)
            && !target.TryCompose(rebuiltResult, candidateBinding)
            && !target.TryCompose(result, rebuiltBinding)
            && !target.TryCompose(result, wrongCandidateBinding)
            && target.TryCompose(result, candidateBinding)
            && target.Request is { } request
            && ReferenceEquals(request.Character, source.Character)
            && ReferenceEquals(request.Placements, recovered)
            && ReferenceEquals(request.Evidence, evidence)
            && request.Candidates.Count == 1
            && ReferenceEquals(request.Candidates[0], source.ContinuationCandidate)
            && !target.TryCompose(result, candidateBinding)
            && target.IsCurrentRequest(request)
            && continuedModel.CallCount == 1
            && model.CallCount == 1
            && actionModel.CallCount == 1
            && priorActionModel.CallCount == 1
            && priorMoveModel.CallCount == 1;
        return composedSuccessfully
            && CharacterSabotageActionMoveCycleContinuationSpecificRequestStartSmokeVerifier
                .PassesMainScene(
                    scene,
                    state,
                    target.Request!,
                    recovered,
                    evidence,
                    continuedModel,
                    model,
                    actionModel,
                    priorActionModel,
                    priorMoveModel);
    }
}
