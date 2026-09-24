using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Dialogue;
using Godot;

namespace Allrpcs.Game;

/// <summary>Verifies exact binding of the observed third-cycle Move result and candidate.</summary>
internal static class CharacterSabotageActionMoveCycleResultBindingSmokeVerifier
{
    /// <summary>Checks rejected substitute results and one exact unacknowledged binding.</summary>
    internal static bool PassesMainScene(
        Node scene,
        TemporaryCharacterObjectSabotageMoveSource source,
        CharacterObjectSabotageMoveController state,
        CharacterSabotageActionMoveCycleCompositionStartSmokeVerifier.RecordingModel model,
        CharacterSabotageActionMoveCompositionStartSmokeVerifier.RecordingModel priorActionModel,
        RecordingMoveModel priorMoveModel)
    {
        CharacterSabotageActionMoveCycleCompositionCoordinator composition =
            scene.GetNode<CharacterSabotageActionMoveCycleCompositionCoordinator>(
                "../CharacterSabotageActionMoveCycleCompositionCoordinator");
        CharacterSabotageActionMoveCycleResultObservationCoordinator observation =
            scene.GetNode<CharacterSabotageActionMoveCycleResultObservationCoordinator>(
                "../CharacterSabotageActionMoveCycleResultObservationCoordinator");
        CharacterSabotageActionMoveCycleResultBindingCoordinator target =
            scene.GetNode<CharacterSabotageActionMoveCycleResultBindingCoordinator>(
                "../CharacterSabotageActionMoveCycleResultBindingCoordinator");
        CharacterSabotageActionMoveRequestController author =
            scene.GetNode<CharacterSabotageActionMoveRequestController>(
                "../CycledCharacterSabotageActionMoveRequestController");
        CharacterSabotageActionTaskController task =
            scene.GetNode<CharacterSabotageActionTaskController>(
                "../CycledCharacterSabotageActionTaskController");
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
        if (target.BoundResult is not null
            || target.CandidateBinding is not null
            || target.TryBindObservedResult(null)
            || target.TryBindObservedResult(rebuiltResult)
            || !target.TryBindObservedResult(result)
            || !ReferenceEquals(target.BoundResult, result)
            || !ReferenceEquals(target.CandidateBinding, candidateBinding)
            || !ReferenceEquals(candidateBinding.Request, composition.AuthorRequest)
            || !ReferenceEquals(candidateBinding.Candidate, source.CycleCandidate)
            || target.TryBindObservedResult(result)
            || !ReferenceEquals(observation.ObservedResult, result)
            || task.Status != CharacterSabotageActionTaskStatus.Completed
            || !ReferenceEquals(task.Result, result)
            || model.CallCount != 1
            || priorActionModel.CallCount != 1
            || priorMoveModel.CallCount != 1
            || turns.Activity is not null
            || !ReferenceEquals(state.Placements, placements)
            || !ReferenceEquals(state.Evidence, evidence))
        {
            return false;
        }

        return CharacterSabotageActionMoveCycleResultAcknowledgementSmokeVerifier.PassesMainScene(
            scene,
            source,
            state,
            model,
            priorActionModel,
            priorMoveModel);
    }
}
