using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Verifies one exact eligible fifth-cycle Move recovery.</summary>
internal static class CharacterSabotageActionMoveCycleContinuationSuccessorRecoverySmokeVerifier
{
    /// <summary>Checks substitutes and one stale-safe recovery without another model call.</summary>
    internal static bool PassesMainScene(
        Node scene,
        CharacterObjectSabotageMoveController state,
        CharacterObjectSabotageMoveRecoveryCandidate recoveryCandidate,
        CharacterEvidenceViewSnapshot evidenceView,
        WorldObjectPlacementSnapshot placements,
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
        CharacterSabotageActionMoveCycleContinuationSuccessorRecoveryCoordinator target =
            scene.GetNode<
                CharacterSabotageActionMoveCycleContinuationSuccessorRecoveryCoordinator>(
                    "../CharacterSabotageActionMoveCycleContinuationSuccessorRecoveryCoordinator");
        CharacterObjectSabotageMoveRecoveryCandidate rebuiltCandidate = new(
            recoveryCandidate.Candidate,
            recoveryCandidate.SourcePlacement);
        CharacterEvidenceViewSnapshot rebuiltEvidenceView = new(
            evidenceView.CharacterId,
            evidenceView.EvidenceIds);

        return target.RecoveryCandidate is null
            && target.EvidenceView is null
            && target.RecoveredPlacements is null
            && target.RecoveredEvidence is null
            && !target.TryRecover(null, evidenceView)
            && !target.TryRecover(recoveryCandidate, null)
            && !target.TryRecover(rebuiltCandidate, evidenceView)
            && !target.TryRecover(recoveryCandidate, rebuiltEvidenceView)
            && ReferenceEquals(state.Placements, placements)
            && ReferenceEquals(state.Evidence, evidence)
            && target.TryRecover(recoveryCandidate, evidenceView)
            && target.RecoveredPlacements is { } recovered
            && ReferenceEquals(target.RecoveryCandidate, recoveryCandidate)
            && ReferenceEquals(target.EvidenceView, evidenceView)
            && ReferenceEquals(target.RecoveredEvidence, evidence)
            && !ReferenceEquals(recovered, placements)
            && ReferenceEquals(state.Placements, recovered)
            && ReferenceEquals(state.Evidence, evidence)
            && recovered.Entries.Count == placements.Entries.Count
            && recovered.Entries.Count(entry =>
                ReferenceEquals(entry, recoveryCandidate.SourcePlacement)) == 1
            && !target.TryRecover(recoveryCandidate, evidenceView)
            && ReferenceEquals(target.RecoveredPlacements, recovered)
            && target.IsCurrentRecovery(recovered, evidence)
            && actionModel.CallCount == 1
            && continuedActionModel.CallCount == 1
            && continuedMoveModel.CallCount == 1
            && cycleActionModel.CallCount == 1
            && priorActionModel.CallCount == 1
            && priorMoveModel.CallCount == 1
            && moveModel.CallCount == 1;
    }
}
