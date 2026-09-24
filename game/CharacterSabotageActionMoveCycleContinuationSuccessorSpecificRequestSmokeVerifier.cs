using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Verifies typed Move composition from the acknowledged fifth-cycle result.</summary>
internal static class
    CharacterSabotageActionMoveCycleContinuationSuccessorSpecificRequestSmokeVerifier
{
    /// <summary>Checks substitute provenance and one fresh exact typed request.</summary>
    internal static bool PassesMainScene(
        Node scene,
        CharacterObjectSabotageMoveController state,
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
        CharacterSabotageActionMoveCycleContinuationSuccessorResultBindingCoordinator binding =
            scene.GetNode<
                CharacterSabotageActionMoveCycleContinuationSuccessorResultBindingCoordinator>(
                    "../CharacterSabotageActionMoveCycleContinuationSuccessorResultBindingCoordinator");
        CharacterSabotageActionMoveCycleContinuationSuccessorSpecificRequestCoordinator target =
            scene.GetNode<
                CharacterSabotageActionMoveCycleContinuationSuccessorSpecificRequestCoordinator>(
                    "../CharacterSabotageActionMoveCycleContinuationSuccessorSpecificRequestCoordinator");
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
                source.ContinuationCandidate);

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
            && ReferenceEquals(request.Candidates[0], source.SuccessorCandidate)
            && !target.TryCompose(result, candidateBinding)
            && target.IsCurrentRequest(request)
            && ReferenceEquals(state.Placements, recovered)
            && ReferenceEquals(state.Evidence, evidence)
            && model.CallCount == 1
            && continuedActionModel.CallCount == 1
            && continuedMoveModel.CallCount == 1
            && actionModel.CallCount == 1
            && priorActionModel.CallCount == 1
            && priorMoveModel.CallCount == 1;
        return composedSuccessfully
            && CharacterSabotageActionMoveCycleContinuationSuccessorSpecificRequestStartSmokeVerifier
                .PassesMainScene(
                    scene,
                    state,
                    target.Request!,
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
