using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Verifies one exact follow-up Move scheduling authorization.</summary>
internal static class CharacterSabotageActionMoveSchedulingAuthorizationSmokeVerifier
{
    /// <summary>Checks substitute references and authorizes only the current empty boundary.</summary>
    internal static bool PassesMainScene(
        Node scene,
        CharacterObjectSabotageMoveController state,
        CharacterSabotageTurnCompletion completion,
        WorldObjectPlacementSnapshot recovered,
        DiscoveryEvidenceSnapshot evidence,
        RecordingMoveModel model,
        CharacterSabotageActionMoveCompositionStartSmokeVerifier.RecordingModel actionModel)
    {
        CharacterSabotageActionMoveSchedulingAuthorizationCoordinator target =
            scene.GetNode<CharacterSabotageActionMoveSchedulingAuthorizationCoordinator>(
                "../CharacterSabotageActionMoveSchedulingAuthorizationCoordinator");
        CharacterTurnActivityController turns = scene.GetNode<CharacterTurnActivityController>(
            "../CharacterTurnActivityController");
        TemporaryCharacterTurnSource turnSource = scene.GetNode<TemporaryCharacterTurnSource>(
            "../TemporaryCharacterTurnSource");
        CharacterSabotageTurnCompletion rebuiltCompletion = new(
            completion.Actor,
            completion.AuthorRequest,
            completion.ActionKind);

        if (target.Authorization is not null
            || target.TryAuthorize(null, turns.Activity)
            || target.TryAuthorize(rebuiltCompletion, turns.Activity)
            || target.TryAuthorize(completion, turnSource.Activity)
            || !target.TryAuthorize(completion, turns.Activity)
            || target.Authorization is not { } authorization
            || !ReferenceEquals(authorization.Admission, completion)
            || authorization.TurnActivity is not null
            || target.TryAuthorize(completion, turns.Activity)
            || !ReferenceEquals(target.Authorization, authorization)
            || !ReferenceEquals(state.Placements, recovered)
            || !ReferenceEquals(state.Evidence, evidence)
            || model.CallCount != 1
            || actionModel.CallCount != 1)
        {
            return false;
        }

        return CharacterSabotageActionMoveCycleBoundarySmokeVerifier.PassesMainScene(
            scene,
            state,
            authorization,
            recovered,
            evidence,
            model,
            actionModel);
    }
}
