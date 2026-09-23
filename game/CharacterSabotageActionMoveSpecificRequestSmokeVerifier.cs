using Allrpcs.Domain.Characters;
using Godot;

namespace Allrpcs.Game;

/// <summary>Verifies exact typed Move request composition from the acknowledged fresh result.</summary>
internal static class CharacterSabotageActionMoveSpecificRequestSmokeVerifier
{
    /// <summary>Checks rejected substitute provenance and one fresh exact typed request.</summary>
    internal static bool PassesMainScene(
        Node scene,
        TemporaryCharacterObjectSabotageMoveSource source,
        CharacterObjectSabotageMoveController state,
        CharacterSabotageActionMoveCompositionStartSmokeVerifier.RecordingModel model)
    {
        CharacterSabotageActionMoveResultBindingCoordinator binding =
            scene.GetNode<CharacterSabotageActionMoveResultBindingCoordinator>(
                "../CharacterSabotageActionMoveResultBindingCoordinator");
        CharacterSabotageActionMoveSpecificRequestCoordinator target =
            scene.GetNode<CharacterSabotageActionMoveSpecificRequestCoordinator>(
                "../CharacterSabotageActionMoveSpecificRequestCoordinator");
        CharacterSabotageActionSpecificRequestCoordinator firstCycleRequests =
            scene.GetNode<CharacterSabotageActionSpecificRequestCoordinator>(
                "../CharacterSabotageActionSpecificRequestCoordinator");
        if (binding.AcknowledgedResult is not { } result
            || binding.CandidateBinding is not { } candidateBinding
            || state.Placements is not { } placements
            || state.Evidence is not { } evidence
            || firstCycleRequests.MoveRequest is not { } firstCycleRequest)
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
                source.Candidate);

        if (target.Request is not null
            || target.TryCompose(null, candidateBinding)
            || target.TryCompose(result, null)
            || target.TryCompose(rebuiltResult, candidateBinding)
            || target.TryCompose(result, rebuiltBinding)
            || target.TryCompose(result, wrongCandidateBinding)
            || !target.TryCompose(result, candidateBinding)
            || target.Request is not { } request
            || !ReferenceEquals(request.Character, source.Character)
            || !ReferenceEquals(request.PhysicalAccess, source.PhysicalAccess)
            || !ReferenceEquals(request.WorldObjects, source.WorldObjects)
            || !ReferenceEquals(request.Placements, placements)
            || !ReferenceEquals(request.Evidence, evidence)
            || request.Candidates.Count != 1
            || !ReferenceEquals(request.Candidates[0], source.FollowUpCandidate)
            || target.TryCompose(result, candidateBinding)
            || model.CallCount != 1
            || !ReferenceEquals(firstCycleRequests.MoveRequest, firstCycleRequest)
            || !ReferenceEquals(state.Placements, placements)
            || !ReferenceEquals(state.Evidence, evidence))
        {
            return false;
        }

        return CharacterSabotageActionMoveSpecificRequestStartSmokeVerifier.PassesMainScene(
            scene,
            source,
            state,
            model);
    }
}
