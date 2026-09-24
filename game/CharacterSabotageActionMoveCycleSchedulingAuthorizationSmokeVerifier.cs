using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Verifies one exact third-cycle Move scheduling authorization.</summary>
internal static class CharacterSabotageActionMoveCycleSchedulingAuthorizationSmokeVerifier
{
    /// <summary>Checks substitute references and authorizes only the current empty boundary.</summary>
    internal static bool PassesMainScene(
        Node scene,
        CharacterObjectSabotageMoveController state,
        CharacterSabotageTurnCompletion completion,
        WorldObjectPlacementSnapshot recovered,
        DiscoveryEvidenceSnapshot evidence,
        RecordingMoveModel model,
        CharacterSabotageActionMoveCycleCompositionStartSmokeVerifier.RecordingModel actionModel,
        CharacterSabotageActionMoveCompositionStartSmokeVerifier.RecordingModel priorActionModel,
        RecordingMoveModel priorMoveModel)
    {
        CharacterSabotageActionMoveCycleSchedulingAuthorizationCoordinator target =
            scene.GetNode<CharacterSabotageActionMoveCycleSchedulingAuthorizationCoordinator>(
                "../CharacterSabotageActionMoveCycleSchedulingAuthorizationCoordinator");
        CharacterTurnActivityController turns = scene.GetNode<CharacterTurnActivityController>(
            "../CharacterTurnActivityController");
        TemporaryCharacterTurnSource turnSource = scene.GetNode<TemporaryCharacterTurnSource>(
            "../TemporaryCharacterTurnSource");
        CharacterSabotageTurnCompletion rebuiltCompletion = new(
            completion.Actor,
            completion.AuthorRequest,
            completion.ActionKind);

        bool authorizedSuccessfully = target.Authorization is null
            && !target.TryAuthorize(null, turns.Activity)
            && !target.TryAuthorize(rebuiltCompletion, turns.Activity)
            && !target.TryAuthorize(completion, turnSource.Activity)
            && target.TryAuthorize(completion, turns.Activity)
            && target.Authorization is { } authorization
            && ReferenceEquals(authorization.Admission, completion)
            && authorization.TurnActivity is null
            && !target.TryAuthorize(completion, turns.Activity)
            && ReferenceEquals(target.Authorization, authorization)
            && target.IsCurrentAuthorization(authorization)
            && !target.IsCurrentAuthorization(null)
            && ReferenceEquals(state.Placements, recovered)
            && ReferenceEquals(state.Evidence, evidence)
            && model.CallCount == 1
            && actionModel.CallCount == 1
            && priorActionModel.CallCount == 1
            && priorMoveModel.CallCount == 1;
        return authorizedSuccessfully
            && CharacterSabotageActionMoveCycleContinuationBoundarySmokeVerifier.PassesMainScene(
                scene,
                state,
                target.Authorization!,
                recovered,
                evidence,
                model,
                actionModel,
                priorActionModel,
                priorMoveModel);
    }
}
