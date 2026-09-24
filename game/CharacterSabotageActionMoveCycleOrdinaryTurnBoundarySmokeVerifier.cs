using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Verifies one exact third-cycle Move admission at the ordinary turn boundary.</summary>
internal static class CharacterSabotageActionMoveCycleOrdinaryTurnBoundarySmokeVerifier
{
    /// <summary>Checks substitute references and binds only the exact empty turn slot.</summary>
    internal static bool PassesMainScene(
        Node scene,
        CharacterObjectSabotageMoveController state,
        CharacterSabotageTurnCompletion completion,
        WorldObjectPlacementSnapshot recovered,
        DiscoveryEvidenceSnapshot evidence,
        RecordingMoveModel model,
        CharacterSabotageActionMoveCycleCompositionStartSmokeVerifier.RecordingModel actionModel,
        CharacterSabotageActionMoveCompositionStartSmokeVerifier.RecordingModel priorActionModel,
        RecordingMoveModel priorMoveModel)
    {
        CharacterSabotageActionMoveCycleOrdinaryTurnBoundaryCoordinator target =
            scene.GetNode<CharacterSabotageActionMoveCycleOrdinaryTurnBoundaryCoordinator>(
                "../CharacterSabotageActionMoveCycleOrdinaryTurnBoundaryCoordinator");
        CharacterTurnActivityController turns = scene.GetNode<CharacterTurnActivityController>(
            "../CharacterTurnActivityController");
        TemporaryCharacterTurnSource turnSource = scene.GetNode<TemporaryCharacterTurnSource>(
            "../TemporaryCharacterTurnSource");
        CharacterSabotageTurnCompletion rebuiltCompletion = new(
            completion.Actor,
            completion.AuthorRequest,
            completion.ActionKind);

        bool boundSuccessfully = target.Admission is null
            && target.TurnActivity is null
            && turns.Activity is null
            && !target.TryBind(null, turns.Activity)
            && !target.TryBind(rebuiltCompletion, turns.Activity)
            && !target.TryBind(completion, turnSource.Activity)
            && target.TryBind(completion, turns.Activity)
            && ReferenceEquals(target.Admission, completion)
            && target.TurnActivity is null
            && !target.TryBind(completion, turns.Activity)
            && target.IsCurrentBoundary(completion, turns.Activity)
            && !target.IsCurrentBoundary(rebuiltCompletion, turns.Activity)
            && !target.IsCurrentBoundary(completion, turnSource.Activity)
            && ReferenceEquals(state.Placements, recovered)
            && ReferenceEquals(state.Evidence, evidence)
            && model.CallCount == 1
            && actionModel.CallCount == 1
            && priorActionModel.CallCount == 1
            && priorMoveModel.CallCount == 1;
        return boundSuccessfully
            && CharacterSabotageActionMoveCycleSchedulingAuthorizationSmokeVerifier.PassesMainScene(
                scene,
                state,
                completion,
                recovered,
                evidence,
                model,
                actionModel,
                priorActionModel,
                priorMoveModel);
    }
}
