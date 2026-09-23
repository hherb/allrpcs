using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;
using Godot;

namespace Allrpcs.Game;

/// <summary>Verifies one exact follow-up-cycle admission-guarded Move author invocation.</summary>
internal static class CharacterSabotageActionMoveCycleAuthoringSmokeVerifier
{
    /// <summary>Checks rejected substitute admissions and one exact fresh author output.</summary>
    internal static bool PassesMainScene(
        Node scene,
        CharacterSabotageCycleBoundary boundary,
        TemporaryCharacterObjectSabotageMoveSource source,
        CharacterObjectSabotageMoveController state,
        CharacterSabotageActionMoveCompositionStartSmokeVerifier.RecordingModel actionModel,
        RecordingMoveModel moveModel)
    {
        CharacterSabotageActionMoveCycleAuthoringAdmissionCoordinator admissions =
            scene.GetNode<CharacterSabotageActionMoveCycleAuthoringAdmissionCoordinator>(
                "../CharacterSabotageActionMoveCycleAuthoringAdmissionCoordinator");
        CharacterSabotageActionMoveCycleAuthoringCoordinator target =
            scene.GetNode<CharacterSabotageActionMoveCycleAuthoringCoordinator>(
                "../CharacterSabotageActionMoveCycleAuthoringCoordinator");
        CharacterSabotageActionMoveRequestController author =
            scene.GetNode<CharacterSabotageActionMoveRequestController>(
                "../CycledCharacterSabotageActionMoveRequestController");
        if (admissions.Admission is not { } currentAdmission
            || state.Placements is not { } retainedPlacements
            || state.Evidence is not { } retainedEvidence)
        {
            return false;
        }

        CharacterSabotageActionAuthoringAdmission rebuiltAdmission = new(boundary);
        CharacterSabotageActionAuthoringAdmission malformedAdmission = new(
            new CharacterSabotageCycleBoundary(boundary.Authorization));

        if (target.Admission is not null
            || target.AuthorRequest is not null
            || author.Request is not null
            || target.TryAuthor(null)
            || target.TryAuthor(rebuiltAdmission)
            || target.TryAuthor(malformedAdmission)
            || !target.TryAuthor(currentAdmission)
            || target.Admission is not { } retainedAdmission
            || target.AuthorRequest is not { } request
            || !ReferenceEquals(retainedAdmission, currentAdmission)
            || !ReferenceEquals(author.Request, request)
            || !ReferenceEquals(request.Actor, source.Character)
            || request.ActionKinds.Count != 1
            || !ReferenceEquals(request.ActionKinds.Single(), CharacterSabotageActionKind.Move)
            || author.CandidateBinding
                is not CharacterSabotageActionCandidateBinding<CharacterObjectSabotageMoveCandidate>
                    binding
            || !ReferenceEquals(binding.Request, request)
            || !ReferenceEquals(binding.ActionKind, CharacterSabotageActionKind.Move)
            || !ReferenceEquals(binding.Candidate, source.CycleCandidate)
            || !ReferenceEquals(state.Placements, retainedPlacements)
            || !ReferenceEquals(state.Evidence, retainedEvidence)
            || actionModel.CallCount != 1
            || moveModel.CallCount != 1)
        {
            return false;
        }

        return !target.TryAuthor(currentAdmission)
            && ReferenceEquals(target.Admission, retainedAdmission)
            && ReferenceEquals(target.AuthorRequest, request)
            && ReferenceEquals(author.Request, request)
            && ReferenceEquals(author.CandidateBinding, binding)
            && ReferenceEquals(state.Placements, retainedPlacements)
            && ReferenceEquals(state.Evidence, retainedEvidence)
            && actionModel.CallCount == 1
            && moveModel.CallCount == 1
            && CharacterSabotageActionMoveCycleCompositionSmokeVerifier.PassesMainScene(
                scene,
                source,
                state,
                actionModel,
                moveModel);
    }
}
