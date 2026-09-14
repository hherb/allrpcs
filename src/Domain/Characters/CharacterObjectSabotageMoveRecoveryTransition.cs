using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Domain.Characters;

/// <summary>Returns fresh placement state after evidence-authorized move recovery.</summary>
public static class CharacterObjectSabotageMoveRecoveryTransition
{
    /// <summary>Restores the exact retained source placement without consuming evidence.</summary>
    public static CharacterObjectSabotageMoveRecoveryResult Recover(
        WorldObjectPlacementSnapshot placements,
        DiscoveryEvidenceSnapshot evidence,
        CharacterEvidenceViewSnapshot evidenceView,
        CharacterObjectSabotageMoveRecoveryCandidate candidate)
    {
        ArgumentNullException.ThrowIfNull(placements);
        ArgumentNullException.ThrowIfNull(evidence);
        ArgumentNullException.ThrowIfNull(evidenceView);
        ArgumentNullException.ThrowIfNull(candidate);

        if (!CharacterObjectSabotageMoveRecoveryPolicy.CanRecover(
                placements,
                evidence,
                evidenceView,
                candidate))
        {
            throw new InvalidOperationException(
                "Only visible authoritative sabotage evidence can recover an exact moved target.");
        }

        WorldObject target = candidate.Candidate.Destination.Occurrence.Target;
        WorldObjectPlacementSnapshot resultingPlacements = new(
            placements.Entries.Select(placement =>
                ReferenceEquals(placement.WorldObject, target)
                    ? candidate.SourcePlacement
                    : placement));

        return new CharacterObjectSabotageMoveRecoveryResult(
            resultingPlacements,
            evidenceView,
            candidate);
    }
}
