using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.WorldObjects;
using Godot;

namespace Allrpcs.Game;

/// <summary>Verifies one exact follow-up Move recovery candidate is authored without recovery.</summary>
internal static class CharacterSabotageActionMoveRecoveryCandidateSmokeVerifier
{
    /// <summary>Checks rejected substitute provenance and one exact recovery candidate.</summary>
    internal static bool PassesMainScene(
        Node scene,
        TemporaryCharacterObjectSabotageMoveSource source,
        CharacterObjectSabotageMoveController state,
        CharacterObjectSabotageMoveConsiderationResult result,
        WorldObjectPlacementSnapshot placements,
        DiscoveryEvidenceSnapshot evidence,
        RecordingMoveModel model,
        CharacterSabotageActionMoveCompositionStartSmokeVerifier.RecordingModel actionModel)
    {
        CharacterSabotageActionMoveRecoveryCandidateCoordinator target =
            scene.GetNode<CharacterSabotageActionMoveRecoveryCandidateCoordinator>(
                "../CharacterSabotageActionMoveRecoveryCandidateCoordinator");
        CharacterObjectSabotageMoveConsiderationResult rebuiltResult = new(
            result.Request,
            result.Decision);
        WorldObjectPlacementSnapshot rebuiltPlacements = new(placements.Entries);
        DiscoveryEvidenceSnapshot rebuiltEvidence = new(evidence.Entries);
        CharacterObjectSabotageMoveCandidate foreignCandidate = new(
            source.FollowUpCandidate.Destination,
            source.FollowUpCandidate.Evidence);
        WorldObjectPlacement sourcePlacement = result.Request.Placements.Entries.Single(entry =>
            ReferenceEquals(
                entry.WorldObject,
                source.FollowUpCandidate.Destination.Occurrence.Target));

        if (target.RecoveryCandidate is not null
            || target.TryAuthor(null, placements, evidence, source.FollowUpCandidate)
            || target.TryAuthor(result, null, evidence, source.FollowUpCandidate)
            || target.TryAuthor(result, placements, null, source.FollowUpCandidate)
            || target.TryAuthor(result, placements, evidence, null)
            || target.TryAuthor(rebuiltResult, placements, evidence, source.FollowUpCandidate)
            || target.TryAuthor(result, rebuiltPlacements, evidence, source.FollowUpCandidate)
            || target.TryAuthor(result, placements, rebuiltEvidence, source.FollowUpCandidate)
            || target.TryAuthor(result, placements, evidence, foreignCandidate)
            || !target.TryAuthor(result, placements, evidence, source.FollowUpCandidate)
            || target.RecoveryCandidate is not { } recoveryCandidate
            || !ReferenceEquals(recoveryCandidate.Candidate, source.FollowUpCandidate)
            || !ReferenceEquals(recoveryCandidate.SourcePlacement, sourcePlacement)
            || target.TryAuthor(result, placements, evidence, source.FollowUpCandidate)
            || !ReferenceEquals(target.RecoveryCandidate, recoveryCandidate)
            || model.CallCount != 1
            || actionModel.CallCount != 1
            || !ReferenceEquals(state.Placements, placements)
            || !ReferenceEquals(state.Evidence, evidence))
        {
            return false;
        }

        return CharacterSabotageActionMoveRecoveryEligibilitySmokeVerifier.PassesMainScene(
            scene,
            source,
            state,
            recoveryCandidate,
            placements,
            evidence,
            model,
            actionModel);
    }
}
