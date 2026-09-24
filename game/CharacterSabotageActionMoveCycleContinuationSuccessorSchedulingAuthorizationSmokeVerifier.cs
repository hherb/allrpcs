using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Verifies one exact fifth-cycle Move scheduling authorization.</summary>
internal static class
    CharacterSabotageActionMoveCycleContinuationSuccessorSchedulingAuthorizationSmokeVerifier
{
    /// <summary>Checks substitute references and authorizes only the current empty boundary.</summary>
    internal static bool PassesMainScene(
        Node scene,
        CharacterObjectSabotageMoveController state,
        CharacterSabotageTurnCompletion completion,
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
        CharacterSabotageActionMoveCycleContinuationSuccessorSchedulingAuthorizationCoordinator
            target = scene.GetNode<
                CharacterSabotageActionMoveCycleContinuationSuccessorSchedulingAuthorizationCoordinator>(
                    "../CharacterSabotageActionMoveCycleContinuationSuccessorSchedulingAuthorizationCoordinator");
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
            && actionModel.CallCount == 1
            && continuedActionModel.CallCount == 1
            && continuedMoveModel.CallCount == 1
            && cycleActionModel.CallCount == 1
            && priorActionModel.CallCount == 1
            && priorMoveModel.CallCount == 1
            && moveModel.CallCount == 1;
        return authorizedSuccessfully
            && CharacterSabotageActionMoveCycleContinuationSuccessorCycleBoundarySmokeVerifier
                .PassesMainScene(
                    scene,
                    state,
                    target.Authorization!,
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
