using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Verifies one exact eligible follow-up Move recovery.</summary>
internal static class CharacterSabotageActionMoveRecoverySmokeVerifier
{
    /// <summary>Checks substitute inputs and one stale-safe recovery without another model call.</summary>
    internal static bool PassesMainScene(
        Node scene,
        TemporaryCharacterObjectSabotageMoveSource source,
        CharacterObjectSabotageMoveController state,
        CharacterObjectSabotageMoveRecoveryCandidate recoveryCandidate,
        CharacterEvidenceViewSnapshot evidenceView,
        WorldObjectPlacementSnapshot placements,
        DiscoveryEvidenceSnapshot evidence,
        RecordingMoveModel model,
        CharacterSabotageActionMoveCompositionStartSmokeVerifier.RecordingModel actionModel)
    {
        CharacterSabotageActionMoveRecoveryCoordinator target =
            scene.GetNode<CharacterSabotageActionMoveRecoveryCoordinator>(
                "../CharacterSabotageActionMoveRecoveryCoordinator");
        CharacterObjectSabotageMoveRecoveryCandidate rebuiltCandidate = new(
            recoveryCandidate.Candidate,
            recoveryCandidate.SourcePlacement);
        CharacterEvidenceViewSnapshot rebuiltView = new(
            evidenceView.CharacterId,
            evidenceView.EvidenceIds);

        if (target.RecoveryCandidate is not null
            || target.EvidenceView is not null
            || target.RecoveredPlacements is not null
            || target.RecoveredEvidence is not null
            || target.TryRecover(null, evidenceView)
            || target.TryRecover(recoveryCandidate, null)
            || target.TryRecover(rebuiltCandidate, evidenceView)
            || target.TryRecover(recoveryCandidate, rebuiltView)
            || !ReferenceEquals(state.Placements, placements)
            || !ReferenceEquals(state.Evidence, evidence)
            || !target.TryRecover(recoveryCandidate, evidenceView)
            || target.RecoveredPlacements is not { } recovered
            || !ReferenceEquals(target.RecoveryCandidate, recoveryCandidate)
            || !ReferenceEquals(target.EvidenceView, evidenceView)
            || !ReferenceEquals(target.RecoveredEvidence, evidence)
            || ReferenceEquals(recovered, placements)
            || !ReferenceEquals(state.Placements, recovered)
            || !ReferenceEquals(state.Evidence, evidence)
            || recovered.Entries.Count != placements.Entries.Count
            || recovered.Entries.Count(entry =>
                ReferenceEquals(entry, recoveryCandidate.SourcePlacement)) != 1
            || target.TryRecover(recoveryCandidate, evidenceView)
            || !ReferenceEquals(target.RecoveredPlacements, recovered)
            || !target.IsCurrentRecovery(recovered, evidence)
            || model.CallCount != 1
            || actionModel.CallCount != 1)
        {
            return false;
        }

        return CharacterSabotageActionMoveTerminalRouteSmokeVerifier.PassesMainScene(
            scene,
            state,
            recovered,
            evidence,
            model,
            actionModel);
    }
}
