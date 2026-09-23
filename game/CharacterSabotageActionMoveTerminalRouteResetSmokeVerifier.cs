using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Verifies exact reset of one retained follow-up Move terminal route.</summary>
internal static class CharacterSabotageActionMoveTerminalRouteResetSmokeVerifier
{
    /// <summary>Checks substitute tokens and clears only the exact retained route once.</summary>
    internal static bool PassesMainScene(
        Node scene,
        CharacterObjectSabotageMoveController state,
        CharacterSabotageActionConsiderationRequest authorRequest,
        WorldObjectPlacementSnapshot recovered,
        DiscoveryEvidenceSnapshot evidence,
        RecordingMoveModel model,
        CharacterSabotageActionMoveCompositionStartSmokeVerifier.RecordingModel actionModel)
    {
        CharacterSabotageActionMoveTerminalRouteCoordinator target =
            scene.GetNode<CharacterSabotageActionMoveTerminalRouteCoordinator>(
                "../CharacterSabotageActionMoveTerminalRouteCoordinator");
        CharacterSabotageActionConsiderationRequest rebuiltAuthorRequest = new(
            authorRequest.Actor,
            authorRequest.ActionKinds);
        WorldObjectPlacementSnapshot rebuiltPlacements = new(recovered.Entries);

        if (target.ResetAuthorRequest is not null
            || target.ResetActionKind is not null
            || target.TryReset(null, CharacterSabotageActionKind.Move, recovered)
            || target.TryReset(authorRequest, null, recovered)
            || target.TryReset(authorRequest, CharacterSabotageActionKind.Move, null)
            || target.TryReset(
                rebuiltAuthorRequest,
                CharacterSabotageActionKind.Move,
                recovered)
            || target.TryReset(authorRequest, CharacterSabotageActionKind.Conceal, recovered)
            || target.TryReset(
                authorRequest,
                CharacterSabotageActionKind.Move,
                rebuiltPlacements)
            || !target.TryReset(
                authorRequest,
                CharacterSabotageActionKind.Move,
                recovered)
            || target.AuthorRequest is not null
            || target.ActionKind is not null
            || target.RecoveredPlacements is not null
            || !ReferenceEquals(target.ResetAuthorRequest, authorRequest)
            || !ReferenceEquals(target.ResetActionKind, CharacterSabotageActionKind.Move)
            || target.TryReset(authorRequest, CharacterSabotageActionKind.Move, recovered)
            || !ReferenceEquals(target.ResetAuthorRequest, authorRequest)
            || !ReferenceEquals(state.Placements, recovered)
            || !ReferenceEquals(state.Evidence, evidence)
            || model.CallCount != 1
            || actionModel.CallCount != 1)
        {
            return false;
        }

        return CharacterSabotageActionMoveTurnCompletionSmokeVerifier.PassesMainScene(
            scene,
            state,
            authorRequest,
            recovered,
            evidence,
            model,
            actionModel);
    }
}
