using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Verifies exact third-cycle Move recovery eligibility without invoking recovery.</summary>
internal static class CharacterSabotageActionMoveCycleRecoveryEligibilitySmokeVerifier
{
    /// <summary>Checks rejected substitutes and one exact candidate and view pairing.</summary>
    internal static bool PassesMainScene(
        Node scene,
        TemporaryCharacterObjectSabotageMoveSource source,
        CharacterObjectSabotageMoveController state,
        CharacterObjectSabotageMoveRecoveryCandidate recoveryCandidate,
        WorldObjectPlacementSnapshot placements,
        DiscoveryEvidenceSnapshot evidence,
        RecordingMoveModel model,
        CharacterSabotageActionMoveCycleCompositionStartSmokeVerifier.RecordingModel actionModel,
        CharacterSabotageActionMoveCompositionStartSmokeVerifier.RecordingModel priorActionModel,
        RecordingMoveModel priorMoveModel)
    {
        CharacterSabotageActionMoveCycleRecoveryEligibilityCoordinator target =
            scene.GetNode<CharacterSabotageActionMoveCycleRecoveryEligibilityCoordinator>(
                "../CharacterSabotageActionMoveCycleRecoveryEligibilityCoordinator");
        CharacterObjectSabotageMoveRecoveryCandidate rebuiltCandidate = new(
            recoveryCandidate.Candidate,
            recoveryCandidate.SourcePlacement);
        CharacterEvidenceViewSnapshot unseenView = new(source.Character.Identity.Id, []);
        CharacterEvidenceViewSnapshot evidenceView = new(
            source.Character.Identity.Id,
            [source.CycleCandidate.Evidence.Evidence.EvidenceId]);

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
            || priorActionModel.CallCount != 1
            || priorMoveModel.CallCount != 1
            || !ReferenceEquals(state.Placements, placements)
            || !ReferenceEquals(state.Evidence, evidence))
        {
            return false;
        }

        return CharacterSabotageActionMoveCycleRecoverySmokeVerifier.PassesMainScene(
            scene,
            state,
            recoveryCandidate,
            evidenceView,
            placements,
            evidence,
            model,
            actionModel,
            priorActionModel,
            priorMoveModel);
    }
}
