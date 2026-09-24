using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Verifies exact reset of one retained third-cycle Move terminal route.</summary>
internal static class CharacterSabotageActionMoveCycleTerminalRouteResetSmokeVerifier
{
    /// <summary>Checks substitute tokens and clears only the exact retained route once.</summary>
    internal static bool PassesMainScene(
        Node scene,
        CharacterObjectSabotageMoveController state,
        CharacterSabotageActionConsiderationRequest authorRequest,
        WorldObjectPlacementSnapshot recovered,
        DiscoveryEvidenceSnapshot evidence,
        RecordingMoveModel model,
        CharacterSabotageActionMoveCycleCompositionStartSmokeVerifier.RecordingModel actionModel,
        CharacterSabotageActionMoveCompositionStartSmokeVerifier.RecordingModel priorActionModel,
        RecordingMoveModel priorMoveModel)
    {
        CharacterSabotageActionMoveCycleTerminalRouteCoordinator target =
            scene.GetNode<CharacterSabotageActionMoveCycleTerminalRouteCoordinator>(
                "../CharacterSabotageActionMoveCycleTerminalRouteCoordinator");
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
            && model.CallCount == 1
            && actionModel.CallCount == 1
            && priorActionModel.CallCount == 1
            && priorMoveModel.CallCount == 1;
        return reset
            && CharacterSabotageActionMoveCycleTurnCompletionSmokeVerifier.PassesMainScene(
                scene,
                state,
                authorRequest,
                recovered,
                evidence,
                model,
                actionModel,
                priorActionModel,
                priorMoveModel);
    }
}
