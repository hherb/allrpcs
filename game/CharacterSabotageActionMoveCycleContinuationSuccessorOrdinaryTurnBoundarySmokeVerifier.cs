using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Verifies the fifth-cycle Move admission at the ordinary turn boundary.</summary>
internal static class
    CharacterSabotageActionMoveCycleContinuationSuccessorOrdinaryTurnBoundarySmokeVerifier
{
    /// <summary>Checks substitute references and binds only the exact empty turn slot.</summary>
    internal static bool PassesMainScene(
        Node scene,
        CharacterObjectSabotageMoveController state,
        CharacterSabotageTurnCompletion completion,
        WorldObjectPlacementSnapshot recovered,
        DiscoveryEvidenceSnapshot evidence,
        CharacterSabotageActionMoveCycleContinuationSuccessorCompositionStartSmokeVerifier
            .RecordingModel actionModel,
        CharacterSabotageActionMoveCycleContinuationCompositionStartSmokeVerifier.RecordingModel
            continuedActionModel,
        RecordingMoveModel continuedMoveModel,
        CharacterSabotageActionMoveCycleCompositionStartSmokeVerifier.RecordingModel cycleActionModel,
        CharacterSabotageActionMoveCompositionStartSmokeVerifier.RecordingModel priorActionModel,
        RecordingMoveModel priorMoveModel,
        RecordingMoveModel moveModel)
    {
        CharacterSabotageActionMoveCycleContinuationSuccessorOrdinaryTurnBoundaryCoordinator target =
            scene.GetNode<
                CharacterSabotageActionMoveCycleContinuationSuccessorOrdinaryTurnBoundaryCoordinator>(
                    "../CharacterSabotageActionMoveCycleContinuationSuccessorOrdinaryTurnBoundaryCoordinator");
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
            && actionModel.CallCount == 1
            && continuedActionModel.CallCount == 1
            && continuedMoveModel.CallCount == 1
            && cycleActionModel.CallCount == 1
            && priorActionModel.CallCount == 1
            && priorMoveModel.CallCount == 1
            && moveModel.CallCount == 1;
        return boundSuccessfully
            && CharacterSabotageActionMoveCycleContinuationSuccessorSchedulingAuthorizationSmokeVerifier
                .PassesMainScene(
                    scene,
                    state,
                    target.Admission!,
                    recovered,
                    evidence,
                    actionModel,
                    continuedActionModel,
                    continuedMoveModel,
                    cycleActionModel,
                    priorActionModel,
                    priorMoveModel,
                    moveModel);
    }
}
