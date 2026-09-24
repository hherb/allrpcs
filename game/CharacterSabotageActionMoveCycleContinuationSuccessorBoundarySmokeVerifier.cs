using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Verifies one explicit successor boundary after the continuation Move.</summary>
internal static class CharacterSabotageActionMoveCycleContinuationSuccessorBoundarySmokeVerifier
{
    /// <summary>Checks substitute authorizations and enters only the exact current boundary.</summary>
    internal static bool PassesMainScene(
        Node scene,
        CharacterObjectSabotageMoveController state,
        CharacterSabotageOrdinaryTurnSchedulingAuthorization authorization,
        WorldObjectPlacementSnapshot recovered,
        DiscoveryEvidenceSnapshot evidence,
        CharacterSabotageActionMoveCycleContinuationCompositionStartSmokeVerifier.RecordingModel
            continuedActionModel,
        RecordingMoveModel continuedMoveModel,
        CharacterSabotageActionMoveCycleCompositionStartSmokeVerifier.RecordingModel actionModel,
        CharacterSabotageActionMoveCompositionStartSmokeVerifier.RecordingModel priorActionModel,
        RecordingMoveModel priorMoveModel)
    {
        CharacterSabotageActionMoveCycleContinuationSuccessorBoundaryCoordinator target =
            scene.GetNode<
                CharacterSabotageActionMoveCycleContinuationSuccessorBoundaryCoordinator>(
                "../CharacterSabotageActionMoveCycleContinuationSuccessorBoundaryCoordinator");
        CharacterSabotageOrdinaryTurnSchedulingAuthorization rebuiltAuthorization = new(
            authorization.Admission,
            authorization.TurnActivity);

        bool enteredSuccessfully = target.Boundary is null
            && !target.TryEnter(null)
            && !target.TryEnter(rebuiltAuthorization)
            && target.TryEnter(authorization)
            && target.Boundary is { } boundary
            && ReferenceEquals(boundary.Authorization, authorization)
            && !target.TryEnter(authorization)
            && ReferenceEquals(target.Boundary, boundary)
            && target.IsCurrentBoundary(boundary)
            && !target.IsCurrentBoundary(new CharacterSabotageCycleBoundary(authorization))
            && ReferenceEquals(state.Placements, recovered)
            && ReferenceEquals(state.Evidence, evidence)
            && continuedActionModel.CallCount == 1
            && continuedMoveModel.CallCount == 1
            && actionModel.CallCount == 1
            && priorActionModel.CallCount == 1
            && priorMoveModel.CallCount == 1;
        return enteredSuccessfully
            && CharacterSabotageActionMoveCycleContinuationSuccessorAuthoringAdmissionSmokeVerifier
                .PassesMainScene(
                    scene,
                    state,
                    target.Boundary!,
                    recovered,
                    evidence,
                    continuedActionModel,
                    continuedMoveModel,
                    actionModel,
                    priorActionModel,
                    priorMoveModel);
    }
}
