using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Verifies one exact completed follow-up Move turn after route reset.</summary>
internal static class CharacterSabotageActionMoveTurnCompletionSmokeVerifier
{
    /// <summary>Checks substitute provenance and exposes only the exact completion once.</summary>
    internal static bool PassesMainScene(
        Node scene,
        CharacterObjectSabotageMoveController state,
        CharacterSabotageActionConsiderationRequest authorRequest,
        WorldObjectPlacementSnapshot recovered,
        DiscoveryEvidenceSnapshot evidence,
        RecordingMoveModel model,
        CharacterSabotageActionMoveCompositionStartSmokeVerifier.RecordingModel actionModel)
    {
        CharacterSabotageActionMoveTurnCompletionCoordinator target =
            scene.GetNode<CharacterSabotageActionMoveTurnCompletionCoordinator>(
                "../CharacterSabotageActionMoveTurnCompletionCoordinator");
        Character foreignActor = RepresentativeEnemyAgentCharacterSource.Create();
        CharacterSabotageActionConsiderationRequest rebuiltAuthorRequest = new(
            authorRequest.Actor,
            authorRequest.ActionKinds);
        WorldObjectPlacementSnapshot rebuiltPlacements = new(recovered.Entries);

        if (target.Completion is not null
            || target.TryComplete(
                null,
                authorRequest,
                CharacterSabotageActionKind.Move,
                recovered)
            || target.TryComplete(
                authorRequest.Actor,
                null,
                CharacterSabotageActionKind.Move,
                recovered)
            || target.TryComplete(authorRequest.Actor, authorRequest, null, recovered)
            || target.TryComplete(
                authorRequest.Actor,
                authorRequest,
                CharacterSabotageActionKind.Move,
                null)
            || target.TryComplete(
                foreignActor,
                authorRequest,
                CharacterSabotageActionKind.Move,
                recovered)
            || target.TryComplete(
                authorRequest.Actor,
                rebuiltAuthorRequest,
                CharacterSabotageActionKind.Move,
                recovered)
            || target.TryComplete(
                authorRequest.Actor,
                authorRequest,
                CharacterSabotageActionKind.Conceal,
                recovered)
            || target.TryComplete(
                authorRequest.Actor,
                authorRequest,
                CharacterSabotageActionKind.Move,
                rebuiltPlacements)
            || !target.TryComplete(
                authorRequest.Actor,
                authorRequest,
                CharacterSabotageActionKind.Move,
                recovered)
            || target.Completion is not { } completion
            || !ReferenceEquals(completion.Actor, authorRequest.Actor)
            || !ReferenceEquals(completion.AuthorRequest, authorRequest)
            || !ReferenceEquals(completion.ActionKind, CharacterSabotageActionKind.Move)
            || target.TryComplete(
                authorRequest.Actor,
                authorRequest,
                CharacterSabotageActionKind.Move,
                recovered)
            || !ReferenceEquals(target.Completion, completion)
            || !ReferenceEquals(state.Placements, recovered)
            || !ReferenceEquals(state.Evidence, evidence)
            || model.CallCount != 1
            || actionModel.CallCount != 1)
        {
            return false;
        }

        CharacterSabotageTurnCompletion rebuiltCompletion = new(
            authorRequest.Actor,
            authorRequest,
            CharacterSabotageActionKind.Move);
        CharacterSabotageTurnCompletion foreignActorCompletion = new(
            foreignActor,
            authorRequest,
            CharacterSabotageActionKind.Move);
        CharacterSabotageTurnCompletion rebuiltAuthorCompletion = new(
            authorRequest.Actor,
            rebuiltAuthorRequest,
            CharacterSabotageActionKind.Move);
        CharacterSabotageTurnCompletion wrongKindCompletion = new(
            authorRequest.Actor,
            authorRequest,
            CharacterSabotageActionKind.Conceal);
        if (CharacterSabotageTurnSchedulingEligibilityPolicy.CanSchedule(null, completion)
            || CharacterSabotageTurnSchedulingEligibilityPolicy.CanSchedule(completion, null)
            || CharacterSabotageTurnSchedulingEligibilityPolicy.CanSchedule(
                completion,
                rebuiltCompletion)
            || CharacterSabotageTurnSchedulingEligibilityPolicy.CanSchedule(
                completion,
                foreignActorCompletion)
            || CharacterSabotageTurnSchedulingEligibilityPolicy.CanSchedule(
                completion,
                rebuiltAuthorCompletion)
            || CharacterSabotageTurnSchedulingEligibilityPolicy.CanSchedule(
                completion,
                wrongKindCompletion)
            || !CharacterSabotageTurnSchedulingEligibilityPolicy.CanSchedule(
                completion,
                completion))
        {
            return false;
        }

        return CharacterSabotageActionMoveTurnAdmissionSmokeVerifier.PassesMainScene(
            scene,
            state,
            completion,
            recovered,
            evidence,
            model,
            actionModel);
    }
}
