using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Verifies one exact fifth-cycle Move recovery candidate is authored.</summary>
internal static class
    CharacterSabotageActionMoveCycleContinuationSuccessorRecoveryCandidateSmokeVerifier
{
    /// <summary>Checks rejected provenance, evidence visibility, and one exact candidate.</summary>
    internal static bool PassesMainScene(
        Node scene,
        CharacterObjectSabotageMoveController state,
        CharacterObjectSabotageMoveConsiderationResult result,
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
        TemporaryCharacterObjectSabotageMoveSource source =
            scene.GetNode<TemporaryCharacterObjectSabotageMoveSource>(
                "../TemporaryCharacterObjectSabotageMoveSource");
        CharacterSabotageActionMoveCycleContinuationSuccessorRecoveryCandidateCoordinator target =
            scene.GetNode<
                CharacterSabotageActionMoveCycleContinuationSuccessorRecoveryCandidateCoordinator>(
                    "../CharacterSabotageActionMoveCycleContinuationSuccessorRecoveryCandidateCoordinator");
        CharacterObjectSabotageMoveConsiderationResult rebuiltResult = new(
            result.Request,
            result.Decision);
        WorldObjectPlacementSnapshot rebuiltPlacements = new(placements.Entries);
        DiscoveryEvidenceSnapshot rebuiltEvidence = new(evidence.Entries);
        CharacterObjectSabotageMoveCandidate rebuiltCandidate = new(
            source.SuccessorCandidate.Destination,
            source.SuccessorCandidate.Evidence);
        CharacterEvidenceViewSnapshot unseenEvidenceView = new(
            source.Character.Identity.Id,
            []);
        CharacterEvidenceViewSnapshot evidenceView = new(
            source.Character.Identity.Id,
            [source.SuccessorCandidate.Evidence.Evidence.EvidenceId]);
        WorldObjectPlacement sourcePlacement = result.Request.Placements.Entries.Single(entry =>
            ReferenceEquals(
                entry.WorldObject,
                source.SuccessorCandidate.Destination.Occurrence.Target));

        bool authored = target.RecoveryCandidate is null
            && target.EvidenceView is null
            && !target.TryAuthor(null, placements, evidence, source.SuccessorCandidate, evidenceView)
            && !target.TryAuthor(result, null, evidence, source.SuccessorCandidate, evidenceView)
            && !target.TryAuthor(result, placements, null, source.SuccessorCandidate, evidenceView)
            && !target.TryAuthor(result, placements, evidence, null, evidenceView)
            && !target.TryAuthor(result, placements, evidence, source.SuccessorCandidate, null)
            && !target.TryAuthor(
                rebuiltResult,
                placements,
                evidence,
                source.SuccessorCandidate,
                evidenceView)
            && !target.TryAuthor(
                result,
                rebuiltPlacements,
                evidence,
                source.SuccessorCandidate,
                evidenceView)
            && !target.TryAuthor(
                result,
                placements,
                rebuiltEvidence,
                source.SuccessorCandidate,
                evidenceView)
            && !target.TryAuthor(result, placements, evidence, rebuiltCandidate, evidenceView)
            && !target.TryAuthor(
                result,
                placements,
                evidence,
                source.SuccessorCandidate,
                unseenEvidenceView)
            && target.TryAuthor(
                result,
                placements,
                evidence,
                source.SuccessorCandidate,
                evidenceView)
            && target.RecoveryCandidate is { } recoveryCandidate
            && ReferenceEquals(recoveryCandidate.Candidate, source.SuccessorCandidate)
            && ReferenceEquals(recoveryCandidate.SourcePlacement, sourcePlacement)
            && ReferenceEquals(target.EvidenceView, evidenceView)
            && !target.TryAuthor(
                result,
                placements,
                evidence,
                source.SuccessorCandidate,
                evidenceView)
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
        return authored
            && CharacterSabotageActionMoveCycleContinuationSuccessorRecoveryEligibilitySmokeVerifier
                .PassesMainScene(
                    scene,
                    state,
                    target.RecoveryCandidate!,
                    target.EvidenceView!,
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
