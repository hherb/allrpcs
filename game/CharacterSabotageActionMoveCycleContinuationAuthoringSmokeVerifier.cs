using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Verifies one continuation-admission-guarded Move author invocation.</summary>
internal static class CharacterSabotageActionMoveCycleContinuationAuthoringSmokeVerifier
{
    /// <summary>Checks substitute admissions and one exact fresh author output.</summary>
    internal static bool PassesMainScene(
        Node scene,
        CharacterObjectSabotageMoveController state,
        CharacterSabotageActionAuthoringAdmission admission,
        WorldObjectPlacementSnapshot recovered,
        DiscoveryEvidenceSnapshot evidence,
        RecordingMoveModel model,
        CharacterSabotageActionMoveCycleCompositionStartSmokeVerifier.RecordingModel actionModel,
        CharacterSabotageActionMoveCompositionStartSmokeVerifier.RecordingModel priorActionModel,
        RecordingMoveModel priorMoveModel)
    {
        TemporaryCharacterObjectSabotageMoveSource source =
            scene.GetNode<TemporaryCharacterObjectSabotageMoveSource>(
                "../TemporaryCharacterObjectSabotageMoveSource");
        CharacterSabotageActionMoveCycleContinuationAuthoringCoordinator target =
            scene.GetNode<CharacterSabotageActionMoveCycleContinuationAuthoringCoordinator>(
                "../CharacterSabotageActionMoveCycleContinuationAuthoringCoordinator");
        CharacterSabotageActionMoveRequestController author =
            scene.GetNode<CharacterSabotageActionMoveRequestController>(
                "../ContinuedCharacterSabotageActionMoveRequestController");
        CharacterSabotageActionAuthoringAdmission rebuiltAdmission = new(admission.Boundary);

        bool authoredSuccessfully = target.Admission is null
            && target.AuthorRequest is null
            && author.Request is null
            && !target.TryAuthor(null)
            && !target.TryAuthor(rebuiltAdmission)
            && target.TryAuthor(admission)
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
            && ReferenceEquals(binding.Candidate, source.ContinuationCandidate)
            && !target.TryAuthor(admission)
            && target.IsCurrentAuthorRequest(request)
            && ReferenceEquals(state.Placements, recovered)
            && ReferenceEquals(state.Evidence, evidence)
            && model.CallCount == 1
            && actionModel.CallCount == 1
            && priorActionModel.CallCount == 1
            && priorMoveModel.CallCount == 1;
        return authoredSuccessfully
            && CharacterSabotageActionMoveCycleContinuationCompositionSmokeVerifier.PassesMainScene(
                scene,
                state,
                target.AuthorRequest!,
                recovered,
                evidence,
                model,
                actionModel,
                priorActionModel,
                priorMoveModel);
    }
}
