using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Verifies one exact continuation Move recovery candidate is authored.</summary>
internal static class CharacterSabotageActionMoveCycleContinuationRecoveryCandidateSmokeVerifier
{
    /// <summary>Checks rejected provenance and one exact recovery candidate.</summary>
    internal static bool PassesMainScene(
        Node scene,
        TemporaryCharacterObjectSabotageMoveSource source,
        CharacterObjectSabotageMoveController state,
        CharacterObjectSabotageMoveConsiderationResult result,
        WorldObjectPlacementSnapshot placements,
        DiscoveryEvidenceSnapshot evidence,
        CharacterSabotageActionMoveCycleContinuationCompositionStartSmokeVerifier.RecordingModel
            continuedModel,
        RecordingMoveModel model,
        CharacterSabotageActionMoveCycleCompositionStartSmokeVerifier.RecordingModel actionModel,
        CharacterSabotageActionMoveCompositionStartSmokeVerifier.RecordingModel priorActionModel,
        RecordingMoveModel priorMoveModel)
    {
        CharacterSabotageActionMoveCycleContinuationRecoveryCandidateCoordinator target =
            scene.GetNode<CharacterSabotageActionMoveCycleContinuationRecoveryCandidateCoordinator>(
                "../CharacterSabotageActionMoveCycleContinuationRecoveryCandidateCoordinator");
        CharacterObjectSabotageMoveConsiderationResult rebuiltResult = new(
            result.Request,
            result.Decision);
        WorldObjectPlacementSnapshot rebuiltPlacements = new(placements.Entries);
        DiscoveryEvidenceSnapshot rebuiltEvidence = new(evidence.Entries);
        CharacterObjectSabotageMoveCandidate foreignCandidate = new(
            source.ContinuationCandidate.Destination,
            source.ContinuationCandidate.Evidence);
        WorldObjectPlacement sourcePlacement = result.Request.Placements.Entries.Single(entry =>
            ReferenceEquals(
                entry.WorldObject,
                source.ContinuationCandidate.Destination.Occurrence.Target));

        bool authored = target.RecoveryCandidate is null
            && !target.TryAuthor(null, placements, evidence, source.ContinuationCandidate)
            && !target.TryAuthor(result, null, evidence, source.ContinuationCandidate)
            && !target.TryAuthor(result, placements, null, source.ContinuationCandidate)
            && !target.TryAuthor(result, placements, evidence, null)
            && !target.TryAuthor(rebuiltResult, placements, evidence, source.ContinuationCandidate)
            && !target.TryAuthor(result, rebuiltPlacements, evidence, source.ContinuationCandidate)
            && !target.TryAuthor(result, placements, rebuiltEvidence, source.ContinuationCandidate)
            && !target.TryAuthor(result, placements, evidence, foreignCandidate)
            && target.TryAuthor(result, placements, evidence, source.ContinuationCandidate)
            && target.RecoveryCandidate is { } recoveryCandidate
            && ReferenceEquals(recoveryCandidate.Candidate, source.ContinuationCandidate)
            && ReferenceEquals(recoveryCandidate.SourcePlacement, sourcePlacement)
            && !target.TryAuthor(result, placements, evidence, source.ContinuationCandidate)
            && ReferenceEquals(target.RecoveryCandidate, recoveryCandidate)
            && continuedModel.CallCount == 1
            && model.CallCount == 1
            && actionModel.CallCount == 1
            && priorActionModel.CallCount == 1
            && priorMoveModel.CallCount == 1
            && ReferenceEquals(state.Placements, placements)
            && ReferenceEquals(state.Evidence, evidence);
        return authored
            && CharacterSabotageActionMoveCycleContinuationRecoveryEligibilitySmokeVerifier
                .PassesMainScene(
                    scene,
                    source,
                    state,
                    target.RecoveryCandidate!,
                    placements,
                    evidence,
                    continuedModel,
                    model,
                    actionModel,
                    priorActionModel,
                    priorMoveModel);
    }
}
