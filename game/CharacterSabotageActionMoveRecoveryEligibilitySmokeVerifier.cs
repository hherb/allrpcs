using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Verifies exact follow-up Move recovery eligibility without invoking recovery.</summary>
internal static class CharacterSabotageActionMoveRecoveryEligibilitySmokeVerifier
{
    /// <summary>Checks rejected substitute inputs and one exact candidate/view pairing.</summary>
    internal static bool PassesMainScene(
        Node scene,
        TemporaryCharacterObjectSabotageMoveSource source,
        CharacterObjectSabotageMoveController state,
        CharacterObjectSabotageMoveRecoveryCandidate recoveryCandidate,
        WorldObjectPlacementSnapshot placements,
        DiscoveryEvidenceSnapshot evidence,
        RecordingMoveModel model,
        CharacterSabotageActionMoveCompositionStartSmokeVerifier.RecordingModel actionModel)
    {
        CharacterSabotageActionMoveRecoveryEligibilityCoordinator target =
            scene.GetNode<CharacterSabotageActionMoveRecoveryEligibilityCoordinator>(
                "../CharacterSabotageActionMoveRecoveryEligibilityCoordinator");
        CharacterObjectSabotageMoveRecoveryCandidate rebuiltCandidate = new(
            recoveryCandidate.Candidate,
            recoveryCandidate.SourcePlacement);
        CharacterEvidenceViewSnapshot unseenView = new(
            source.Character.Identity.Id,
            []);
        CharacterEvidenceViewSnapshot evidenceView = new(
            source.Character.Identity.Id,
            [source.FollowUpCandidate.Evidence.Evidence.EvidenceId]);

        if (target.RecoveryCandidate is not null
            || target.EvidenceView is not null
            || target.TryPublish(null, evidenceView)
            || target.TryPublish(recoveryCandidate, null)
            || target.TryPublish(rebuiltCandidate, evidenceView)
            || target.TryPublish(recoveryCandidate, unseenView)
            || !target.TryPublish(recoveryCandidate, evidenceView)
            || !ReferenceEquals(target.RecoveryCandidate, recoveryCandidate)
            || !ReferenceEquals(target.EvidenceView, evidenceView)
            || target.TryPublish(recoveryCandidate, evidenceView)
            || model.CallCount != 1
            || actionModel.CallCount != 1
            || !ReferenceEquals(state.Placements, placements)
            || !ReferenceEquals(state.Evidence, evidence))
        {
            return false;
        }

        return CharacterSabotageActionMoveRecoverySmokeVerifier.PassesMainScene(
            scene,
            source,
            state,
            recoveryCandidate,
            evidenceView,
            placements,
            evidence,
            model,
            actionModel);
    }
}
