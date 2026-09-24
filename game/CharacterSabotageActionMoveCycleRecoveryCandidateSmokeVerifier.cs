using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Verifies one exact third-cycle Move recovery candidate is authored without recovery.</summary>
internal static class CharacterSabotageActionMoveCycleRecoveryCandidateSmokeVerifier
{
    /// <summary>Checks rejected provenance and one exact recovery candidate.</summary>
    internal static bool PassesMainScene(
        Node scene,
        TemporaryCharacterObjectSabotageMoveSource source,
        CharacterObjectSabotageMoveController state,
        CharacterObjectSabotageMoveConsiderationResult result,
        WorldObjectPlacementSnapshot placements,
        DiscoveryEvidenceSnapshot evidence,
        RecordingMoveModel model,
        CharacterSabotageActionMoveCycleCompositionStartSmokeVerifier.RecordingModel actionModel,
        CharacterSabotageActionMoveCompositionStartSmokeVerifier.RecordingModel priorActionModel,
        RecordingMoveModel priorMoveModel)
    {
        CharacterSabotageActionMoveCycleRecoveryCandidateCoordinator target =
            scene.GetNode<CharacterSabotageActionMoveCycleRecoveryCandidateCoordinator>(
                "../CharacterSabotageActionMoveCycleRecoveryCandidateCoordinator");
        CharacterObjectSabotageMoveConsiderationResult rebuiltResult = new(
            result.Request,
            result.Decision);
        WorldObjectPlacementSnapshot rebuiltPlacements = new(placements.Entries);
        DiscoveryEvidenceSnapshot rebuiltEvidence = new(evidence.Entries);
        CharacterObjectSabotageMoveCandidate foreignCandidate = new(
            source.CycleCandidate.Destination,
            source.CycleCandidate.Evidence);
        WorldObjectPlacement sourcePlacement = result.Request.Placements.Entries.Single(entry =>
            ReferenceEquals(entry.WorldObject, source.CycleCandidate.Destination.Occurrence.Target));

        if (target.RecoveryCandidate is not null
            || target.TryAuthor(null, placements, evidence, source.CycleCandidate)
            || target.TryAuthor(result, null, evidence, source.CycleCandidate)
            || target.TryAuthor(result, placements, null, source.CycleCandidate)
            || target.TryAuthor(result, placements, evidence, null)
            || target.TryAuthor(rebuiltResult, placements, evidence, source.CycleCandidate)
            || target.TryAuthor(result, rebuiltPlacements, evidence, source.CycleCandidate)
            || target.TryAuthor(result, placements, rebuiltEvidence, source.CycleCandidate)
            || target.TryAuthor(result, placements, evidence, foreignCandidate)
            || !target.TryAuthor(result, placements, evidence, source.CycleCandidate)
            || target.RecoveryCandidate is not { } recoveryCandidate
            || !ReferenceEquals(recoveryCandidate.Candidate, source.CycleCandidate)
            || !ReferenceEquals(recoveryCandidate.SourcePlacement, sourcePlacement)
            || target.TryAuthor(result, placements, evidence, source.CycleCandidate)
            || !ReferenceEquals(target.RecoveryCandidate, recoveryCandidate)
            || model.CallCount != 1
            || actionModel.CallCount != 1
            || priorActionModel.CallCount != 1
            || priorMoveModel.CallCount != 1
            || !ReferenceEquals(state.Placements, placements)
            || !ReferenceEquals(state.Evidence, evidence))
        {
            return false;
        }

        return CharacterSabotageActionMoveCycleRecoveryEligibilitySmokeVerifier.PassesMainScene(
                scene,
                source,
                state,
                recoveryCandidate,
                placements,
                evidence,
                model,
                actionModel,
                priorActionModel,
                priorMoveModel);
    }
}
