using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Verifies continuation Move recovery eligibility without invoking recovery.</summary>
internal static class CharacterSabotageActionMoveCycleContinuationRecoveryEligibilitySmokeVerifier
{
    /// <summary>Checks rejected substitutes and one exact candidate and view pairing.</summary>
    internal static bool PassesMainScene(
        Node scene,
        TemporaryCharacterObjectSabotageMoveSource source,
        CharacterObjectSabotageMoveController state,
        CharacterObjectSabotageMoveRecoveryCandidate recoveryCandidate,
        WorldObjectPlacementSnapshot placements,
        DiscoveryEvidenceSnapshot evidence,
        CharacterSabotageActionMoveCycleContinuationCompositionStartSmokeVerifier.RecordingModel
            continuedModel,
        RecordingMoveModel model,
        CharacterSabotageActionMoveCycleCompositionStartSmokeVerifier.RecordingModel actionModel,
        CharacterSabotageActionMoveCompositionStartSmokeVerifier.RecordingModel priorActionModel,
        RecordingMoveModel priorMoveModel)
    {
        CharacterSabotageActionMoveCycleContinuationRecoveryEligibilityCoordinator target =
            scene.GetNode<
                CharacterSabotageActionMoveCycleContinuationRecoveryEligibilityCoordinator>(
                    "../CharacterSabotageActionMoveCycleContinuationRecoveryEligibilityCoordinator");
        CharacterObjectSabotageMoveRecoveryCandidate rebuiltCandidate = new(
            recoveryCandidate.Candidate,
            recoveryCandidate.SourcePlacement);
        CharacterEvidenceViewSnapshot unseenView = new(source.Character.Identity.Id, []);
        CharacterEvidenceViewSnapshot evidenceView = new(
            source.Character.Identity.Id,
            [source.ContinuationCandidate.Evidence.Evidence.EvidenceId]);

        bool published = target.RecoveryCandidate is null
            && target.EvidenceView is null
            && !target.TryPublish(null, evidenceView)
            && !target.TryPublish(recoveryCandidate, null)
            && !target.TryPublish(rebuiltCandidate, evidenceView)
            && !target.TryPublish(recoveryCandidate, unseenView)
            && target.TryPublish(recoveryCandidate, evidenceView)
            && ReferenceEquals(target.RecoveryCandidate, recoveryCandidate)
            && ReferenceEquals(target.EvidenceView, evidenceView)
            && !target.TryPublish(recoveryCandidate, evidenceView)
            && continuedModel.CallCount == 1
            && model.CallCount == 1
            && actionModel.CallCount == 1
            && priorActionModel.CallCount == 1
            && priorMoveModel.CallCount == 1
            && ReferenceEquals(state.Placements, placements)
            && ReferenceEquals(state.Evidence, evidence);
        return published
            && CharacterSabotageActionMoveCycleContinuationRecoverySmokeVerifier.PassesMainScene(
                scene,
                state,
                recoveryCandidate,
                evidenceView,
                placements,
                evidence,
                continuedModel,
                model,
                actionModel,
                priorActionModel,
                priorMoveModel);
    }
}
