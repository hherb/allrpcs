using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Verifies one exact follow-up Move admission at the ordinary turn boundary.</summary>
internal static class CharacterSabotageActionMoveOrdinaryTurnBoundarySmokeVerifier
{
    /// <summary>Checks substitute references and binds only the exact empty turn slot.</summary>
    internal static bool PassesMainScene(
        Node scene,
        CharacterObjectSabotageMoveController state,
        CharacterSabotageTurnCompletion completion,
        WorldObjectPlacementSnapshot recovered,
        DiscoveryEvidenceSnapshot evidence,
        RecordingMoveModel model,
        CharacterSabotageActionMoveCompositionStartSmokeVerifier.RecordingModel actionModel)
    {
        CharacterSabotageActionMoveOrdinaryTurnBoundaryCoordinator target =
            scene.GetNode<CharacterSabotageActionMoveOrdinaryTurnBoundaryCoordinator>(
                "../CharacterSabotageActionMoveOrdinaryTurnBoundaryCoordinator");
        CharacterTurnActivityController turns = scene.GetNode<CharacterTurnActivityController>(
            "../CharacterTurnActivityController");
        TemporaryCharacterTurnSource turnSource = scene.GetNode<TemporaryCharacterTurnSource>(
            "../TemporaryCharacterTurnSource");
        CharacterSabotageTurnCompletion rebuiltCompletion = new(
            completion.Actor,
            completion.AuthorRequest,
            completion.ActionKind);

        if (target.Admission is not null
            || target.TurnActivity is not null
            || turns.Activity is not null
            || target.TryBind(null, turns.Activity)
            || target.TryBind(rebuiltCompletion, turns.Activity)
            || target.TryBind(completion, turnSource.Activity)
            || !target.TryBind(completion, turns.Activity)
            || !ReferenceEquals(target.Admission, completion)
            || target.TurnActivity is not null
            || target.TryBind(completion, turns.Activity)
            || !CharacterSabotageOrdinaryTurnBoundaryEligibilityPolicy.CanSchedule(
                target.Admission,
                completion,
                target.TurnActivity,
                turns.Activity)
            || CharacterSabotageOrdinaryTurnBoundaryEligibilityPolicy.CanSchedule(
                target.Admission,
                rebuiltCompletion,
                target.TurnActivity,
                turns.Activity)
            || CharacterSabotageOrdinaryTurnBoundaryEligibilityPolicy.CanSchedule(
                target.Admission,
                completion,
                target.TurnActivity,
                turnSource.Activity)
            || !ReferenceEquals(state.Placements, recovered)
            || !ReferenceEquals(state.Evidence, evidence)
            || model.CallCount != 1
            || actionModel.CallCount != 1)
        {
            return false;
        }

        return CharacterSabotageActionMoveSchedulingAuthorizationSmokeVerifier.PassesMainScene(
            scene,
            state,
            completion,
            recovered,
            evidence,
            model,
            actionModel);
    }
}
