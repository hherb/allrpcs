using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Verifies one exact third-cycle Move completion admission.</summary>
internal static class CharacterSabotageActionMoveCycleTurnAdmissionSmokeVerifier
{
    /// <summary>Checks substitute tokens and admits only the exact current completion once.</summary>
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
        CharacterSabotageActionMoveCycleTurnAdmissionCoordinator target =
            scene.GetNode<CharacterSabotageActionMoveCycleTurnAdmissionCoordinator>(
                "../CharacterSabotageActionMoveCycleTurnAdmissionCoordinator");
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

        return target.Admission is null
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
            && model.CallCount == 1
            && actionModel.CallCount == 1
            && priorActionModel.CallCount == 1
            && priorMoveModel.CallCount == 1;
    }
}
