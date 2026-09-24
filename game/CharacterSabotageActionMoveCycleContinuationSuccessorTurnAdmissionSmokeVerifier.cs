using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Verifies one exact fifth-cycle Move completion admission.</summary>
internal static class CharacterSabotageActionMoveCycleContinuationSuccessorTurnAdmissionSmokeVerifier
{
    /// <summary>Checks substitute tokens and admits only the exact current completion once.</summary>
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
        CharacterSabotageActionMoveCycleContinuationSuccessorTurnAdmissionCoordinator target =
            scene.GetNode<
                CharacterSabotageActionMoveCycleContinuationSuccessorTurnAdmissionCoordinator>(
                    "../CharacterSabotageActionMoveCycleContinuationSuccessorTurnAdmissionCoordinator");
        Character foreignActor = RepresentativeEnemyAgentCharacterSource.Create();
        CharacterSabotageActionConsiderationRequest rebuiltAuthorRequest = new(
            completion.AuthorRequest.Actor,
            completion.AuthorRequest.ActionKinds);
        CharacterSabotageTurnCompletion rebuiltCompletion = new(
            completion.Actor,
            completion.AuthorRequest,
            completion.ActionKind);
        CharacterSabotageTurnCompletion foreignActorCompletion = new(
            foreignActor,
            completion.AuthorRequest,
            completion.ActionKind);
        CharacterSabotageTurnCompletion rebuiltAuthorCompletion = new(
            completion.Actor,
            rebuiltAuthorRequest,
            completion.ActionKind);
        CharacterSabotageTurnCompletion wrongKindCompletion = new(
            completion.Actor,
            completion.AuthorRequest,
            CharacterSabotageActionKind.Conceal);

        bool admitted = target.Admission is null
            && !target.TryAdmit(null)
            && !target.TryAdmit(rebuiltCompletion)
            && !target.TryAdmit(foreignActorCompletion)
            && !target.TryAdmit(rebuiltAuthorCompletion)
            && !target.TryAdmit(wrongKindCompletion)
            && target.TryAdmit(completion)
            && ReferenceEquals(target.Admission, completion)
            && !target.TryAdmit(completion)
            && ReferenceEquals(target.Admission, completion)
            && ReferenceEquals(state.Placements, recovered)
            && ReferenceEquals(state.Evidence, evidence)
            && actionModel.CallCount == 1
            && continuedActionModel.CallCount == 1
            && continuedMoveModel.CallCount == 1
            && cycleActionModel.CallCount == 1
            && priorActionModel.CallCount == 1
            && priorMoveModel.CallCount == 1
            && moveModel.CallCount == 1;
        return admitted
            && CharacterSabotageActionMoveCycleContinuationSuccessorOrdinaryTurnBoundarySmokeVerifier
                .PassesMainScene(
                    scene,
                    state,
                    target.Admission!,
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
