using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Verifies exact retention of one recovered follow-up Move terminal route.</summary>
internal static class CharacterSabotageActionMoveTerminalRouteSmokeVerifier
{
    /// <summary>Checks rejected substitute tokens and one exact terminal retention.</summary>
    internal static bool PassesMainScene(
        Node scene,
        CharacterObjectSabotageMoveController state,
        WorldObjectPlacementSnapshot recovered,
        DiscoveryEvidenceSnapshot evidence,
        RecordingMoveModel model,
        CharacterSabotageActionMoveCompositionStartSmokeVerifier.RecordingModel actionModel)
    {
        CharacterSabotageActionMoveCompositionCoordinator composition =
            scene.GetNode<CharacterSabotageActionMoveCompositionCoordinator>(
                "../CharacterSabotageActionMoveCompositionCoordinator");
        CharacterSabotageActionMoveTerminalRouteCoordinator target =
            scene.GetNode<CharacterSabotageActionMoveTerminalRouteCoordinator>(
                "../CharacterSabotageActionMoveTerminalRouteCoordinator");
        if (composition.AuthorRequest is not { } authorRequest)
        {
            return false;
        }

        CharacterSabotageActionConsiderationRequest rebuiltAuthorRequest = new(
            authorRequest.Actor,
            authorRequest.ActionKinds);
        WorldObjectPlacementSnapshot rebuiltPlacements = new(recovered.Entries);

        if (target.AuthorRequest is not null
            || target.ActionKind is not null
            || target.RecoveredPlacements is not null
            || target.TryRetain(null, CharacterSabotageActionKind.Move, recovered)
            || target.TryRetain(authorRequest, null, recovered)
            || target.TryRetain(authorRequest, CharacterSabotageActionKind.Move, null)
            || target.TryRetain(
                rebuiltAuthorRequest,
                CharacterSabotageActionKind.Move,
                recovered)
            || target.TryRetain(authorRequest, CharacterSabotageActionKind.Conceal, recovered)
            || target.TryRetain(
                authorRequest,
                CharacterSabotageActionKind.Move,
                rebuiltPlacements)
            || !target.TryRetain(
                authorRequest,
                CharacterSabotageActionKind.Move,
                recovered)
            || !ReferenceEquals(target.AuthorRequest, authorRequest)
            || !ReferenceEquals(target.ActionKind, CharacterSabotageActionKind.Move)
            || !ReferenceEquals(target.RecoveredPlacements, recovered)
            || target.TryRetain(authorRequest, CharacterSabotageActionKind.Move, recovered)
            || !ReferenceEquals(target.AuthorRequest, authorRequest)
            || !ReferenceEquals(target.RecoveredPlacements, recovered)
            || !ReferenceEquals(state.Placements, recovered)
            || !ReferenceEquals(state.Evidence, evidence)
            || model.CallCount != 1
            || actionModel.CallCount != 1)
        {
            return false;
        }

        return CharacterSabotageActionMoveTerminalRouteResetSmokeVerifier.PassesMainScene(
            scene,
            state,
            authorRequest,
            recovered,
            evidence,
            model,
            actionModel);
    }
}
