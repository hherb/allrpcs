using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Verifies one exact eligible continuation Move recovery.</summary>
internal static class CharacterSabotageActionMoveCycleContinuationRecoverySmokeVerifier
{
    /// <summary>Checks substitutes and one stale-safe recovery without another model call.</summary>
    internal static bool PassesMainScene(
        Node scene,
        CharacterObjectSabotageMoveController state,
        CharacterObjectSabotageMoveRecoveryCandidate recoveryCandidate,
        CharacterEvidenceViewSnapshot evidenceView,
        WorldObjectPlacementSnapshot placements,
        DiscoveryEvidenceSnapshot evidence,
        CharacterSabotageActionMoveCycleContinuationCompositionStartSmokeVerifier.RecordingModel
            continuedModel,
        RecordingMoveModel model,
        CharacterSabotageActionMoveCycleCompositionStartSmokeVerifier.RecordingModel actionModel,
        CharacterSabotageActionMoveCompositionStartSmokeVerifier.RecordingModel priorActionModel,
        RecordingMoveModel priorMoveModel)
    {
        CharacterSabotageActionMoveCycleContinuationRecoveryCoordinator target =
            scene.GetNode<CharacterSabotageActionMoveCycleContinuationRecoveryCoordinator>(
                "../CharacterSabotageActionMoveCycleContinuationRecoveryCoordinator");
        CharacterObjectSabotageMoveRecoveryCandidate rebuiltCandidate = new(
            recoveryCandidate.Candidate,
            recoveryCandidate.SourcePlacement);
        CharacterEvidenceViewSnapshot rebuiltView = new(
            evidenceView.CharacterId,
            evidenceView.EvidenceIds);

        return target.RecoveryCandidate is null
            && target.EvidenceView is null
            && target.RecoveredPlacements is null
            && target.RecoveredEvidence is null
            && !target.TryRecover(null, evidenceView)
            && !target.TryRecover(recoveryCandidate, null)
            && !target.TryRecover(rebuiltCandidate, evidenceView)
            && !target.TryRecover(recoveryCandidate, rebuiltView)
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
            && continuedModel.CallCount == 1
            && model.CallCount == 1
            && actionModel.CallCount == 1
            && priorActionModel.CallCount == 1
            && priorMoveModel.CallCount == 1;
    }
}
