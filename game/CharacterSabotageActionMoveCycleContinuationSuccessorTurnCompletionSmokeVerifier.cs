using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Verifies one exact completed fifth-cycle Move turn after route reset.</summary>
internal static class CharacterSabotageActionMoveCycleContinuationSuccessorTurnCompletionSmokeVerifier
{
    /// <summary>Checks substitute provenance and exposes only the exact completion once.</summary>
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
        CharacterSabotageActionMoveCycleContinuationSuccessorTurnCompletionCoordinator target =
            scene.GetNode<
                CharacterSabotageActionMoveCycleContinuationSuccessorTurnCompletionCoordinator>(
                    "../CharacterSabotageActionMoveCycleContinuationSuccessorTurnCompletionCoordinator");
        Character foreignActor = RepresentativeEnemyAgentCharacterSource.Create();
        CharacterSabotageActionConsiderationRequest rebuiltAuthorRequest = new(
            authorRequest.Actor,
            authorRequest.ActionKinds);
        WorldObjectPlacementSnapshot rebuiltPlacements = new(recovered.Entries);
        DiscoveryEvidenceSnapshot rebuiltEvidence = new(evidence.Entries);

        bool completed = target.Completion is null
            && !target.TryComplete(
                null,
                authorRequest,
                CharacterSabotageActionKind.Move,
                recovered,
                evidence)
            && !target.TryComplete(
                authorRequest.Actor,
                null,
                CharacterSabotageActionKind.Move,
                recovered,
                evidence)
            && !target.TryComplete(
                authorRequest.Actor,
                authorRequest,
                null,
                recovered,
                evidence)
            && !target.TryComplete(
                foreignActor,
                authorRequest,
                CharacterSabotageActionKind.Move,
                recovered,
                evidence)
            && !target.TryComplete(
                authorRequest.Actor,
                rebuiltAuthorRequest,
                CharacterSabotageActionKind.Move,
                recovered,
                evidence)
            && !target.TryComplete(
                authorRequest.Actor,
                authorRequest,
                CharacterSabotageActionKind.Conceal,
                recovered,
                evidence)
            && !target.TryComplete(
                authorRequest.Actor,
                authorRequest,
                CharacterSabotageActionKind.Move,
                rebuiltPlacements,
                evidence)
            && !target.TryComplete(
                authorRequest.Actor,
                authorRequest,
                CharacterSabotageActionKind.Move,
                recovered,
                rebuiltEvidence)
            && target.TryComplete(
                authorRequest.Actor,
                authorRequest,
                CharacterSabotageActionKind.Move,
                recovered,
                evidence)
            && target.Completion is { } completion
            && ReferenceEquals(completion.Actor, authorRequest.Actor)
            && ReferenceEquals(completion.AuthorRequest, authorRequest)
            && ReferenceEquals(completion.ActionKind, CharacterSabotageActionKind.Move)
            && !target.TryComplete(
                authorRequest.Actor,
                authorRequest,
                CharacterSabotageActionKind.Move,
                recovered,
                evidence)
            && ReferenceEquals(target.Completion, completion)
            && ReferenceEquals(state.Placements, recovered)
            && ReferenceEquals(state.Evidence, evidence)
            && actionModel.CallCount == 1
            && continuedActionModel.CallCount == 1
            && continuedMoveModel.CallCount == 1
            && cycleActionModel.CallCount == 1
            && priorActionModel.CallCount == 1
            && priorMoveModel.CallCount == 1
            && moveModel.CallCount == 1;
        return completed
            && CharacterSabotageActionMoveCycleContinuationSuccessorTurnAdmissionSmokeVerifier
                .PassesMainScene(
                    scene,
                    state,
                    target.Completion!,
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
