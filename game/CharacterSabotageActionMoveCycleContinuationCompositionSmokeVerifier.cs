using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Verifies exact one-shot composition of the continuation Move output.</summary>
internal static class CharacterSabotageActionMoveCycleContinuationCompositionSmokeVerifier
{
    /// <summary>Checks substitute author requests and one fresh exact generic request.</summary>
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
        CharacterSabotageActionMoveCycleContinuationCompositionCoordinator target =
            scene.GetNode<CharacterSabotageActionMoveCycleContinuationCompositionCoordinator>(
                "../CharacterSabotageActionMoveCycleContinuationCompositionCoordinator");
        CharacterSabotageActionConsiderationRequest rebuiltRequest = new(
            authorRequest.Actor,
            authorRequest.ActionKinds);
        CharacterSabotageActionConsiderationRequest wrongKindRequest = new(
            authorRequest.Actor,
            [CharacterSabotageActionKind.Conceal]);

        bool composedSuccessfully = target.Request is null
            && !target.TryCompose(null)
            && !target.TryCompose(rebuiltRequest)
            && !target.TryCompose(wrongKindRequest)
            && target.TryCompose(authorRequest)
            && target.Request is { } request
            && !ReferenceEquals(request, authorRequest)
            && ReferenceEquals(target.AuthorRequest, authorRequest)
            && ReferenceEquals(request.Actor, authorRequest.Actor)
            && request.ActionKinds.Count == 1
            && ReferenceEquals(request.ActionKinds[0], CharacterSabotageActionKind.Move)
            && !target.TryCompose(authorRequest)
            && target.IsCurrentRequest(request)
            && ReferenceEquals(state.Placements, recovered)
            && ReferenceEquals(state.Evidence, evidence)
            && model.CallCount == 1
            && actionModel.CallCount == 1
            && priorActionModel.CallCount == 1
            && priorMoveModel.CallCount == 1;
        return composedSuccessfully
            && CharacterSabotageActionMoveCycleContinuationCompositionStartSmokeVerifier
                .PassesMainScene(
                    scene,
                    state,
                    target.Request!,
                    recovered,
                    evidence,
                    model,
                    actionModel,
                    priorActionModel,
                    priorMoveModel);
    }
}
