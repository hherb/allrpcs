using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Verifies exact reset of one retained fifth-cycle Move terminal route.</summary>
internal static class CharacterSabotageActionMoveCycleContinuationSuccessorTerminalRouteResetSmokeVerifier
{
    /// <summary>Checks substitute tokens and clears only the exact retained route once.</summary>
    internal static bool PassesMainScene(
        Node scene,
        CharacterObjectSabotageMoveController state,
        CharacterSabotageActionConsiderationRequest authorRequest,
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
        CharacterSabotageActionMoveCycleContinuationSuccessorTerminalRouteCoordinator target =
            scene.GetNode<
                CharacterSabotageActionMoveCycleContinuationSuccessorTerminalRouteCoordinator>(
                    "../CharacterSabotageActionMoveCycleContinuationSuccessorTerminalRouteCoordinator");
        CharacterSabotageActionConsiderationRequest rebuiltAuthorRequest = new(
            authorRequest.Actor,
            authorRequest.ActionKinds);
        WorldObjectPlacementSnapshot rebuiltPlacements = new(recovered.Entries);
        DiscoveryEvidenceSnapshot rebuiltEvidence = new(evidence.Entries);

        bool reset = target.ResetAuthorRequest is null
            && target.ResetActionKind is null
            && !target.TryReset(null, CharacterSabotageActionKind.Move, recovered, evidence)
            && !target.TryReset(authorRequest, null, recovered, evidence)
            && !target.TryReset(authorRequest, CharacterSabotageActionKind.Move, null, evidence)
            && !target.TryReset(authorRequest, CharacterSabotageActionKind.Move, recovered, null)
            && !target.TryReset(
                rebuiltAuthorRequest,
                CharacterSabotageActionKind.Move,
                recovered,
                evidence)
            && !target.TryReset(
                authorRequest,
                CharacterSabotageActionKind.Conceal,
                recovered,
                evidence)
            && !target.TryReset(
                authorRequest,
                CharacterSabotageActionKind.Move,
                rebuiltPlacements,
                evidence)
            && !target.TryReset(
                authorRequest,
                CharacterSabotageActionKind.Move,
                recovered,
                rebuiltEvidence)
            && target.TryReset(
                authorRequest,
                CharacterSabotageActionKind.Move,
                recovered,
                evidence)
            && target.AuthorRequest is null
            && target.ActionKind is null
            && target.RecoveredPlacements is null
            && target.RecoveredEvidence is null
            && ReferenceEquals(target.ResetAuthorRequest, authorRequest)
            && ReferenceEquals(target.ResetActionKind, CharacterSabotageActionKind.Move)
            && !target.TryReset(
                authorRequest,
                CharacterSabotageActionKind.Move,
                recovered,
                evidence)
            && ReferenceEquals(state.Placements, recovered)
            && ReferenceEquals(state.Evidence, evidence)
            && actionModel.CallCount == 1
            && continuedActionModel.CallCount == 1
            && continuedMoveModel.CallCount == 1
            && cycleActionModel.CallCount == 1
            && priorActionModel.CallCount == 1
            && priorMoveModel.CallCount == 1
            && moveModel.CallCount == 1;
        return reset
            && CharacterSabotageActionMoveCycleContinuationSuccessorTurnCompletionSmokeVerifier
                .PassesMainScene(
                    scene,
                    state,
                    authorRequest,
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
