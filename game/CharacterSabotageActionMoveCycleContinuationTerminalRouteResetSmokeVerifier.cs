using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Verifies exact reset of one retained continuation Move terminal route.</summary>
internal static class CharacterSabotageActionMoveCycleContinuationTerminalRouteResetSmokeVerifier
{
    /// <summary>Checks substitute tokens and clears only the exact retained route once.</summary>
    internal static bool PassesMainScene(
        Node scene,
        CharacterObjectSabotageMoveController state,
        CharacterSabotageActionConsiderationRequest authorRequest,
        WorldObjectPlacementSnapshot recovered,
        DiscoveryEvidenceSnapshot evidence,
        CharacterSabotageActionMoveCycleContinuationCompositionStartSmokeVerifier.RecordingModel
            continuedActionModel,
        RecordingMoveModel continuedMoveModel,
        CharacterSabotageActionMoveCycleCompositionStartSmokeVerifier.RecordingModel actionModel,
        CharacterSabotageActionMoveCompositionStartSmokeVerifier.RecordingModel priorActionModel,
        RecordingMoveModel priorMoveModel)
    {
        CharacterSabotageActionMoveCycleContinuationTerminalRouteCoordinator target =
            scene.GetNode<CharacterSabotageActionMoveCycleContinuationTerminalRouteCoordinator>(
                "../CharacterSabotageActionMoveCycleContinuationTerminalRouteCoordinator");
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
            && ReferenceEquals(target.ResetAuthorRequest, authorRequest)
            && ReferenceEquals(state.Placements, recovered)
            && ReferenceEquals(state.Evidence, evidence)
            && continuedActionModel.CallCount == 1
            && continuedMoveModel.CallCount == 1
            && actionModel.CallCount == 1
            && priorActionModel.CallCount == 1
            && priorMoveModel.CallCount == 1;
        return reset
            && CharacterSabotageActionMoveCycleContinuationTurnCompletionSmokeVerifier
                .PassesMainScene(
                    scene,
                    state,
                    authorRequest,
                    recovered,
                    evidence,
                    continuedActionModel,
                    continuedMoveModel,
                    actionModel,
                    priorActionModel,
                    priorMoveModel);
    }
}
