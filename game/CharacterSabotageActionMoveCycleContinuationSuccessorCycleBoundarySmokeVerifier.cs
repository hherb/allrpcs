using Allrpcs.Domain.Dialogue;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Verifies the explicit successor boundary after the fifth Move cycle.</summary>
internal static class
    CharacterSabotageActionMoveCycleContinuationSuccessorCycleBoundarySmokeVerifier
{
    /// <summary>Checks substitute graphs and enters only the exact current authorization once.</summary>
    internal static bool PassesMainScene(
        Node scene,
        CharacterObjectSabotageMoveController state,
        CharacterSabotageOrdinaryTurnSchedulingAuthorization authorization,
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
        CharacterSabotageActionMoveCycleContinuationSuccessorCycleBoundaryCoordinator target =
            scene.GetNode<
                CharacterSabotageActionMoveCycleContinuationSuccessorCycleBoundaryCoordinator>(
                    "../CharacterSabotageActionMoveCycleContinuationSuccessorCycleBoundaryCoordinator");
        TemporaryCharacterTurnSource turnSource = scene.GetNode<TemporaryCharacterTurnSource>(
            "../TemporaryCharacterTurnSource");
        CharacterSabotageTurnCompletion completion = authorization.Admission;
        CharacterSabotageTurnCompletion rebuiltAdmission = new(
            completion.Actor,
            completion.AuthorRequest,
            completion.ActionKind);
        CharacterSabotageOrdinaryTurnSchedulingAuthorization rebuiltAuthorization = new(
            authorization.Admission,
            authorization.TurnActivity);
        CharacterSabotageOrdinaryTurnSchedulingAuthorization rebuiltAdmissionAuthorization = new(
            rebuiltAdmission,
            authorization.TurnActivity);
        CharacterSabotageOrdinaryTurnSchedulingAuthorization substitutedActivityAuthorization = new(
            authorization.Admission,
            turnSource.Activity);

        bool enteredSuccessfully = target.Boundary is null
            && !target.TryEnter(null)
            && !target.TryEnter(rebuiltAuthorization)
            && !target.TryEnter(rebuiltAdmissionAuthorization)
            && !target.TryEnter(substitutedActivityAuthorization)
            && target.TryEnter(authorization)
            && target.Boundary is { } boundary
            && ReferenceEquals(boundary.Authorization, authorization)
            && !target.TryEnter(authorization)
            && ReferenceEquals(target.Boundary, boundary)
            && target.IsCurrentBoundary(boundary)
            && !target.IsCurrentBoundary(new CharacterSabotageCycleBoundary(authorization))
            && ReferenceEquals(state.Placements, recovered)
            && ReferenceEquals(state.Evidence, evidence)
            && actionModel.CallCount == 1
            && continuedActionModel.CallCount == 1
            && continuedMoveModel.CallCount == 1
            && cycleActionModel.CallCount == 1
            && priorActionModel.CallCount == 1
            && priorMoveModel.CallCount == 1
            && moveModel.CallCount == 1;
        return enteredSuccessfully
            && CharacterSabotageActionMoveCycleContinuationSuccessorCycleStartAdmissionSmokeVerifier
                .PassesMainScene(
                    scene,
                    state,
                    target.Boundary!,
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
