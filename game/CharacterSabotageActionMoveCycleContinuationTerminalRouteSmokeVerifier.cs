using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Verifies exact retention of one recovered continuation Move terminal route.</summary>
internal static class CharacterSabotageActionMoveCycleContinuationTerminalRouteSmokeVerifier
{
    /// <summary>Checks rejected substitute tokens and one exact terminal retention.</summary>
    internal static bool PassesMainScene(
        Node scene,
        TemporaryCharacterObjectSabotageMoveSource source,
        CharacterObjectSabotageMoveController state,
        WorldObjectPlacementSnapshot recovered,
        DiscoveryEvidenceSnapshot evidence,
        CharacterSabotageActionMoveCycleContinuationCompositionStartSmokeVerifier.RecordingModel
            continuedActionModel,
        RecordingMoveModel continuedMoveModel,
        CharacterSabotageActionMoveCycleCompositionStartSmokeVerifier.RecordingModel actionModel,
        CharacterSabotageActionMoveCompositionStartSmokeVerifier.RecordingModel priorActionModel,
        RecordingMoveModel priorMoveModel)
    {
        CharacterSabotageActionMoveCycleContinuationAuthoringCoordinator authoring =
            scene.GetNode<CharacterSabotageActionMoveCycleContinuationAuthoringCoordinator>(
                "../CharacterSabotageActionMoveCycleContinuationAuthoringCoordinator");
        CharacterSabotageActionMoveCycleContinuationTerminalRouteCoordinator target =
            scene.GetNode<CharacterSabotageActionMoveCycleContinuationTerminalRouteCoordinator>(
                "../CharacterSabotageActionMoveCycleContinuationTerminalRouteCoordinator");
        if (authoring.AuthorRequest is not { } authorRequest)
        {
            return false;
        }

        CharacterSabotageActionConsiderationRequest rebuiltAuthorRequest = new(
            authorRequest.Actor,
            authorRequest.ActionKinds);
        CharacterSabotageActionConsiderationRequest foreignActorRequest = new(
            RepresentativeEnemyAgentCharacterSource.Create(),
            authorRequest.ActionKinds);
        WorldObjectPlacementSnapshot rebuiltPlacements = new(recovered.Entries);
        DiscoveryEvidenceSnapshot rebuiltEvidence = new(evidence.Entries);

        bool retained = target.AuthorRequest is null
            && target.ActionKind is null
            && target.RecoveredPlacements is null
            && target.RecoveredEvidence is null
            && !target.TryRetain(null, CharacterSabotageActionKind.Move, recovered, evidence)
            && !target.TryRetain(authorRequest, null, recovered, evidence)
            && !target.TryRetain(authorRequest, CharacterSabotageActionKind.Move, null, evidence)
            && !target.TryRetain(authorRequest, CharacterSabotageActionKind.Move, recovered, null)
            && !target.TryRetain(
                rebuiltAuthorRequest,
                CharacterSabotageActionKind.Move,
                recovered,
                evidence)
            && !target.TryRetain(
                foreignActorRequest,
                CharacterSabotageActionKind.Move,
                recovered,
                evidence)
            && !target.TryRetain(
                authorRequest,
                CharacterSabotageActionKind.Conceal,
                recovered,
                evidence)
            && !target.TryRetain(
                authorRequest,
                CharacterSabotageActionKind.Move,
                rebuiltPlacements,
                evidence)
            && !target.TryRetain(
                authorRequest,
                CharacterSabotageActionKind.Move,
                recovered,
                rebuiltEvidence)
            && target.TryRetain(
                authorRequest,
                CharacterSabotageActionKind.Move,
                recovered,
                evidence)
            && ReferenceEquals(target.AuthorRequest, authorRequest)
            && ReferenceEquals(target.ActionKind, CharacterSabotageActionKind.Move)
            && ReferenceEquals(target.RecoveredPlacements, recovered)
            && ReferenceEquals(target.RecoveredEvidence, evidence)
            && !target.TryRetain(
                authorRequest,
                CharacterSabotageActionKind.Move,
                recovered,
                evidence)
            && ReferenceEquals(target.AuthorRequest, authorRequest)
            && ReferenceEquals(target.RecoveredPlacements, recovered)
            && ReferenceEquals(target.RecoveredEvidence, evidence)
            && ReferenceEquals(state.Placements, recovered)
            && ReferenceEquals(state.Evidence, evidence)
            && continuedActionModel.CallCount == 1
            && continuedMoveModel.CallCount == 1
            && actionModel.CallCount == 1
            && priorActionModel.CallCount == 1
            && priorMoveModel.CallCount == 1;
        return retained
            && CharacterSabotageActionMoveCycleContinuationTerminalRouteResetSmokeVerifier
                .PassesMainScene(
                    scene,
                    state,
                    authorRequest,
                    recovered,
                    evidence,
                    continuedActionModel,
                    continuedMoveModel,
                    actionModel,
                    priorActionModel,
                    priorMoveModel);
    }
}
