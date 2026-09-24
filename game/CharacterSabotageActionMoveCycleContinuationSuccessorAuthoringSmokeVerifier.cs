using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Verifies one successor-admission-guarded Move author invocation.</summary>
internal static class
    CharacterSabotageActionMoveCycleContinuationSuccessorAuthoringSmokeVerifier
{
    /// <summary>Checks substitute inputs and one exact fresh fifth-cycle author output.</summary>
    internal static bool PassesMainScene(
        Node scene,
        CharacterObjectSabotageMoveController state,
        CharacterSabotageActionAuthoringAdmission admission,
        WorldObjectPlacementSnapshot recovered,
        DiscoveryEvidenceSnapshot evidence,
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
        CharacterSabotageActionMoveCycleContinuationSuccessorAuthoringCoordinator target =
            scene.GetNode<
                CharacterSabotageActionMoveCycleContinuationSuccessorAuthoringCoordinator>(
                "../CharacterSabotageActionMoveCycleContinuationSuccessorAuthoringCoordinator");
        CharacterSabotageActionMoveRequestController author =
            scene.GetNode<CharacterSabotageActionMoveRequestController>(
                "../SuccessorCharacterSabotageActionMoveRequestController");
        CharacterSabotageActionAuthoringAdmission rebuiltAdmission = new(admission.Boundary);
        CharacterObjectSabotageMoveCandidate rebuiltCandidate = new(
            source.SuccessorCandidate.Destination,
            source.SuccessorCandidate.Evidence);

        bool authoredSuccessfully = target.Admission is null
            && target.AuthorRequest is null
            && author.Request is null
            && !target.TryAuthor(null, source.SuccessorCandidate)
            && !target.TryAuthor(rebuiltAdmission, source.SuccessorCandidate)
            && !target.TryAuthor(admission, null)
            && !target.TryAuthor(admission, source.Candidate)
            && !target.TryAuthor(admission, source.FollowUpCandidate)
            && !target.TryAuthor(admission, source.CycleCandidate)
            && !target.TryAuthor(admission, source.ContinuationCandidate)
            && !target.TryAuthor(admission, rebuiltCandidate)
            && target.TryAuthor(admission, source.SuccessorCandidate)
            && target.AuthorRequest is { } request
            && ReferenceEquals(target.Admission, admission)
            && ReferenceEquals(author.Request, request)
            && ReferenceEquals(request.Actor, source.Character)
            && request.ActionKinds.Count == 1
            && ReferenceEquals(request.ActionKinds[0], CharacterSabotageActionKind.Move)
            && author.CandidateBinding
                is CharacterSabotageActionCandidateBinding<CharacterObjectSabotageMoveCandidate>
                    binding
            && ReferenceEquals(binding.Request, request)
            && ReferenceEquals(binding.ActionKind, CharacterSabotageActionKind.Move)
            && ReferenceEquals(binding.Candidate, source.SuccessorCandidate)
            && !target.TryAuthor(admission, source.SuccessorCandidate)
            && target.IsCurrentAuthorRequest(request)
            && ReferenceEquals(state.Placements, recovered)
            && ReferenceEquals(state.Evidence, evidence)
            && continuedActionModel.CallCount == 1
            && continuedMoveModel.CallCount == 1
            && actionModel.CallCount == 1
            && priorActionModel.CallCount == 1
            && priorMoveModel.CallCount == 1;
        return authoredSuccessfully
            && CharacterSabotageActionMoveCycleContinuationSuccessorCompositionSmokeVerifier
                .PassesMainScene(
                    scene,
                    state,
                    target.AuthorRequest!,
                    recovered,
                    evidence,
                    continuedActionModel,
                    continuedMoveModel,
                    actionModel,
                    priorActionModel,
                    priorMoveModel);
    }
}
