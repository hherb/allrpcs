using Allrpcs.Domain.Characters;
using Godot;

namespace Allrpcs.Game;

/// <summary>Verifies exact acknowledgement of the bound third-cycle Move result.</summary>
internal static class CharacterSabotageActionMoveCycleResultAcknowledgementSmokeVerifier
{
    /// <summary>Checks rejected substitute results and one exact task-resetting acknowledgement.</summary>
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
        CharacterSabotageActionTaskController task =
            scene.GetNode<CharacterSabotageActionTaskController>(
                "../CycledCharacterSabotageActionTaskController");
        CharacterTurnActivityController turns =
            scene.GetNode<CharacterTurnActivityController>("../CharacterTurnActivityController");
        if (target.BoundResult is not { } result
            || target.CandidateBinding is not { } candidateBinding
            || composition.Request is not { } request
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
        return target.AcknowledgedResult is null
            && !target.TryAcknowledgeBoundResult(null)
            && !target.TryAcknowledgeBoundResult(rebuiltResult)
            && target.TryAcknowledgeBoundResult(result)
            && ReferenceEquals(target.AcknowledgedResult, result)
            && !target.TryAcknowledgeBoundResult(result)
            && observation.ObservedResult is null
            && task.Status == CharacterSabotageActionTaskStatus.Idle
            && task.Result is null
            && ReferenceEquals(target.BoundResult, result)
            && ReferenceEquals(target.CandidateBinding, candidateBinding)
            && ReferenceEquals(candidateBinding.Candidate, source.CycleCandidate)
            && model.CallCount == 1
            && priorActionModel.CallCount == 1
            && priorMoveModel.CallCount == 1
            && turns.Activity is null
            && ReferenceEquals(state.Placements, placements)
            && ReferenceEquals(state.Evidence, evidence);
    }
}
