using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Domain.Characters;

/// <summary>Requires an exact moved placement and visible authoritative evidence for recovery.</summary>
public static class CharacterObjectSabotageMoveRecoveryPolicy
{
    /// <summary>Reports whether retained evidence permits restoring the exact source placement.</summary>
    public static bool CanRecover(
        WorldObjectPlacementSnapshot placements,
        DiscoveryEvidenceSnapshot evidence,
        CharacterEvidenceViewSnapshot evidenceView,
        CharacterObjectSabotageMoveRecoveryCandidate candidate)
    {
        ArgumentNullException.ThrowIfNull(placements);
        ArgumentNullException.ThrowIfNull(evidence);
        ArgumentNullException.ThrowIfNull(evidenceView);
        ArgumentNullException.ThrowIfNull(candidate);

        WorldObject target = candidate.Candidate.Destination.Occurrence.Target;
        WorldObjectPlacement? currentPlacement = placements.Entries.SingleOrDefault(
            placement => placement.WorldObject.Identity.Id == target.Identity.Id);

        return currentPlacement is not null
            && ReferenceEquals(currentPlacement.WorldObject, target)
            && ReferenceEquals(
                currentPlacement.Location,
                candidate.Candidate.Destination.Destination)
            && candidate.Candidate.Evidence.Evidence.ObjectId == target.Identity.Id
            && evidence.Entries.Any(definition =>
                ReferenceEquals(definition, candidate.Candidate.Evidence.Evidence))
            && evidenceView.EvidenceIds.Any(evidenceId =>
                evidenceId == candidate.Candidate.Evidence.Evidence.EvidenceId);
    }
}
