using Allrpcs.Domain.Characters;
using Godot;

namespace Allrpcs.Game;

/// <summary>Verifies exact binding and acknowledgement of the fresh generic Move result.</summary>
internal static class CharacterSabotageActionMoveResultBindingSmokeVerifier
{
    /// <summary>Checks rejected substitute results, exact binding, and exact acknowledgement.</summary>
    internal static bool PassesMainScene(
        Node scene,
        TemporaryCharacterObjectSabotageMoveSource source,
        CharacterObjectSabotageMoveController state,
        CharacterSabotageActionMoveCompositionStartSmokeVerifier.RecordingModel model)
    {
        CharacterSabotageActionMoveCompositionCoordinator composition =
            scene.GetNode<CharacterSabotageActionMoveCompositionCoordinator>(
                "../CharacterSabotageActionMoveCompositionCoordinator");
        CharacterSabotageActionMoveResultObservationCoordinator observation =
            scene.GetNode<CharacterSabotageActionMoveResultObservationCoordinator>(
                "../CharacterSabotageActionMoveResultObservationCoordinator");
        CharacterSabotageActionMoveResultBindingCoordinator target =
            scene.GetNode<CharacterSabotageActionMoveResultBindingCoordinator>(
                "../CharacterSabotageActionMoveResultBindingCoordinator");
        CharacterSabotageActionMoveRequestController author =
            scene.GetNode<CharacterSabotageActionMoveRequestController>(
                "../ScheduledCharacterSabotageActionMoveRequestController");
        CharacterSabotageActionTaskController task =
            scene.GetNode<CharacterSabotageActionTaskController>(
                "../ScheduledCharacterSabotageActionTaskController");
        CharacterTurnActivityController turns =
            scene.GetNode<CharacterTurnActivityController>("../CharacterTurnActivityController");
        if (observation.ObservedResult is not { } result
            || composition.Request is not { } request
            || author.CandidateBinding
                is not CharacterSabotageActionCandidateBinding<CharacterObjectSabotageMoveCandidate>
                    candidateBinding
            || state.Placements is not { } placements
            || state.Evidence is not { } evidence)
        {
            return false;
        }

        CharacterSabotageActionConsiderationResult rebuiltResult = new(
            request,
            new CharacterSabotageActionDecision(
                request.Actor.Identity.Id,
                CharacterSabotageActionKind.Move));
        if (target.CandidateBinding is not null
            || target.AcknowledgedResult is not null
            || target.TryBindObservedResult(null)
            || target.TryBindObservedResult(rebuiltResult)
            || !target.TryBindObservedResult(result)
            || !ReferenceEquals(target.CandidateBinding, candidateBinding)
            || !ReferenceEquals(candidateBinding.Request, composition.AuthorRequest)
            || !ReferenceEquals(candidateBinding.Candidate, source.FollowUpCandidate)
            || target.TryBindObservedResult(result)
            || target.TryAcknowledgeBoundResult(null)
            || target.TryAcknowledgeBoundResult(rebuiltResult)
            || !target.TryAcknowledgeBoundResult(result)
            || !ReferenceEquals(target.AcknowledgedResult, result)
            || target.TryAcknowledgeBoundResult(result)
            || observation.ObservedResult is not null
            || task.Status != CharacterSabotageActionTaskStatus.Idle
            || task.Result is not null
            || model.CallCount != 1
            || turns.Activity is not null
            || !ReferenceEquals(state.Placements, placements)
            || !ReferenceEquals(state.Evidence, evidence))
        {
            return false;
        }

        return CharacterSabotageActionMoveSpecificRequestSmokeVerifier.PassesMainScene(
            scene,
            source,
            state,
            model);
    }
}
