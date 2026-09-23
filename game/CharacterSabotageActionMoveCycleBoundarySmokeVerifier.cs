using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Verifies one explicit follow-up Move cycle boundary.</summary>
internal static class CharacterSabotageActionMoveCycleBoundarySmokeVerifier
{
    /// <summary>Checks substitute authorizations and enters only the exact current boundary.</summary>
    internal static bool PassesMainScene(
        Node scene,
        CharacterObjectSabotageMoveController state,
        CharacterSabotageOrdinaryTurnSchedulingAuthorization authorization,
        WorldObjectPlacementSnapshot recovered,
        DiscoveryEvidenceSnapshot evidence,
        RecordingMoveModel model,
        CharacterSabotageActionMoveCompositionStartSmokeVerifier.RecordingModel actionModel)
    {
        CharacterSabotageActionMoveCycleBoundaryCoordinator target =
            scene.GetNode<CharacterSabotageActionMoveCycleBoundaryCoordinator>(
                "../CharacterSabotageActionMoveCycleBoundaryCoordinator");
        TemporaryCharacterTurnSource turnSource = scene.GetNode<TemporaryCharacterTurnSource>(
            "../TemporaryCharacterTurnSource");
        CharacterSabotageOrdinaryTurnSchedulingAuthorization rebuiltAuthorization = new(
            authorization.Admission,
            authorization.TurnActivity);
        CharacterSabotageCycleBoundary rebuiltBoundary = new(authorization);
        CharacterSabotageOrdinaryTurnSchedulingAuthorization activeAuthorization = new(
            authorization.Admission,
            turnSource.Activity);
        CharacterSabotageCycleBoundary activeBoundary = new(activeAuthorization);

        if (target.Boundary is not null
            || target.TryEnter(null)
            || target.TryEnter(rebuiltAuthorization)
            || !target.TryEnter(authorization)
            || target.Boundary is not { } boundary
            || !ReferenceEquals(boundary.Authorization, authorization)
            || target.TryEnter(authorization)
            || !ReferenceEquals(target.Boundary, boundary)
            || CharacterSabotageCycleStartEligibilityPolicy.CanStart(
                null,
                boundary,
                authorization,
                authorization.Admission,
                authorization.TurnActivity)
            || CharacterSabotageCycleStartEligibilityPolicy.CanStart(
                boundary,
                rebuiltBoundary,
                authorization,
                authorization.Admission,
                authorization.TurnActivity)
            || CharacterSabotageCycleStartEligibilityPolicy.CanStart(
                activeBoundary,
                activeBoundary,
                activeAuthorization,
                activeAuthorization.Admission,
                turnSource.Activity)
            || !CharacterSabotageCycleStartEligibilityPolicy.CanStart(
                boundary,
                boundary,
                authorization,
                authorization.Admission,
                authorization.TurnActivity)
            || !ReferenceEquals(state.Placements, recovered)
            || !ReferenceEquals(state.Evidence, evidence)
            || model.CallCount != 1
            || actionModel.CallCount != 1)
        {
            return false;
        }

        return CharacterSabotageActionMoveCycleAuthoringAdmissionSmokeVerifier.PassesMainScene(
            scene,
            state,
            boundary,
            recovered,
            evidence,
            model,
            actionModel);
    }
}
