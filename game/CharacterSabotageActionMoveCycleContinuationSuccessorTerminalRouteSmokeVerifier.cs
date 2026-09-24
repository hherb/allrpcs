using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Verifies exact retention and reset of the fifth-cycle Move terminal route.</summary>
internal static class CharacterSabotageActionMoveCycleContinuationSuccessorTerminalRouteSmokeVerifier
{
    /// <summary>Checks rejected substitutes, one exact retention, and one exact reset.</summary>
    internal static bool PassesMainScene(
        Node scene,
        CharacterObjectSabotageMoveController state,
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
        CharacterSabotageActionMoveCycleContinuationSuccessorAuthoringCoordinator authoring =
            scene.GetNode<CharacterSabotageActionMoveCycleContinuationSuccessorAuthoringCoordinator>(
                "../CharacterSabotageActionMoveCycleContinuationSuccessorAuthoringCoordinator");
        CharacterSabotageActionMoveCycleContinuationSuccessorTerminalRouteCoordinator target =
            scene.GetNode<
                CharacterSabotageActionMoveCycleContinuationSuccessorTerminalRouteCoordinator>(
                    "../CharacterSabotageActionMoveCycleContinuationSuccessorTerminalRouteCoordinator");
        if (authoring.AuthorRequest is not { } authorRequest)
        {
            return false;
        }

        CharacterSabotageActionConsiderationRequest rebuiltAuthorRequest = new(
            authorRequest.Actor,
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
                evidence);
        bool reset = retained
            && target.ResetAuthorRequest is null
            && target.ResetActionKind is null
            && !target.TryReset(null, CharacterSabotageActionKind.Move, recovered, evidence)
            && !target.TryReset(authorRequest, null, recovered, evidence)
            && !target.TryReset(
                rebuiltAuthorRequest,
                CharacterSabotageActionKind.Move,
                recovered,
                evidence)
            && !target.TryReset(
                authorRequest,
                CharacterSabotageActionKind.Conceal,
                recovered,
                evidence)
            && !target.TryReset(
                authorRequest,
                CharacterSabotageActionKind.Move,
                rebuiltPlacements,
                evidence)
            && !target.TryReset(
                authorRequest,
                CharacterSabotageActionKind.Move,
                recovered,
                rebuiltEvidence)
            && target.TryReset(
                authorRequest,
                CharacterSabotageActionKind.Move,
                recovered,
                evidence)
            && target.AuthorRequest is null
            && target.ActionKind is null
            && target.RecoveredPlacements is null
            && target.RecoveredEvidence is null
            && ReferenceEquals(target.ResetAuthorRequest, authorRequest)
            && ReferenceEquals(target.ResetActionKind, CharacterSabotageActionKind.Move)
            && !target.TryReset(
                authorRequest,
                CharacterSabotageActionKind.Move,
                recovered,
                evidence);
        return reset
            && ReferenceEquals(state.Placements, recovered)
            && ReferenceEquals(state.Evidence, evidence)
            && actionModel.CallCount == 1
            && continuedActionModel.CallCount == 1
            && continuedMoveModel.CallCount == 1
            && cycleActionModel.CallCount == 1
            && priorActionModel.CallCount == 1
            && priorMoveModel.CallCount == 1
            && moveModel.CallCount == 1;
    }
}
