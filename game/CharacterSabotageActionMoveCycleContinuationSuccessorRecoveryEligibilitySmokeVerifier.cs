using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Verifies exact fifth-cycle Move recovery eligibility without recovery.</summary>
internal static class
    CharacterSabotageActionMoveCycleContinuationSuccessorRecoveryEligibilitySmokeVerifier
{
    /// <summary>Checks rejected substitutes and one exact candidate and view admission.</summary>
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
        CharacterSabotageActionMoveCycleContinuationSuccessorRecoveryEligibilityCoordinator target =
            scene.GetNode<
                CharacterSabotageActionMoveCycleContinuationSuccessorRecoveryEligibilityCoordinator>(
                    "../CharacterSabotageActionMoveCycleContinuationSuccessorRecoveryEligibilityCoordinator");
        CharacterObjectSabotageMoveRecoveryCandidate rebuiltCandidate = new(
            recoveryCandidate.Candidate,
            recoveryCandidate.SourcePlacement);
        CharacterEvidenceViewSnapshot rebuiltEvidenceView = new(
            evidenceView.CharacterId,
            evidenceView.EvidenceIds);

        bool published = target.RecoveryCandidate is null
            && target.EvidenceView is null
            && !target.TryPublish(null, evidenceView)
            && !target.TryPublish(recoveryCandidate, null)
            && !target.TryPublish(rebuiltCandidate, evidenceView)
            && !target.TryPublish(recoveryCandidate, rebuiltEvidenceView)
            && target.TryPublish(recoveryCandidate, evidenceView)
            && ReferenceEquals(target.RecoveryCandidate, recoveryCandidate)
            && ReferenceEquals(target.EvidenceView, evidenceView)
            && !target.TryPublish(recoveryCandidate, evidenceView)
            && ReferenceEquals(target.RecoveryCandidate, recoveryCandidate)
            && ReferenceEquals(target.EvidenceView, evidenceView)
            && actionModel.CallCount == 1
            && continuedActionModel.CallCount == 1
            && continuedMoveModel.CallCount == 1
            && cycleActionModel.CallCount == 1
            && priorActionModel.CallCount == 1
            && priorMoveModel.CallCount == 1
            && moveModel.CallCount == 1
            && ReferenceEquals(state.Placements, placements)
            && ReferenceEquals(state.Evidence, evidence);
        return published
            && CharacterSabotageActionMoveCycleContinuationSuccessorRecoverySmokeVerifier
                .PassesMainScene(
                    scene,
                    state,
                    recoveryCandidate,
                    evidenceView,
                    placements,
                    evidence,
                    actionModel,
                    continuedActionModel,
                    continuedMoveModel,
                    cycleActionModel,
                    priorActionModel,
                    priorMoveModel,
                    moveModel);
    }
}
