using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Domain.Characters;

/// <summary>Atomically returns fresh placement and evidence state for eligible move sabotage.</summary>
public static class CharacterObjectSabotageMoveTransition
{
    /// <summary>Returns fresh snapshots with the exact target moved and exact evidence appended.</summary>
    public static CharacterObjectSabotageMoveResult Apply(
        Character character,
        CharacterPhysicalAccessSnapshot physicalAccess,
        WorldObjectSnapshot worldObjects,
        WorldObjectPlacementSnapshot placements,
        DiscoveryEvidenceSnapshot existingEvidence,
        CharacterObjectSabotageMoveCandidate candidate)
    {
        ArgumentNullException.ThrowIfNull(character);
        ArgumentNullException.ThrowIfNull(physicalAccess);
        ArgumentNullException.ThrowIfNull(worldObjects);
        ArgumentNullException.ThrowIfNull(placements);
        ArgumentNullException.ThrowIfNull(existingEvidence);
        ArgumentNullException.ThrowIfNull(candidate);

        if (!CharacterObjectSabotageMovePolicy.CanApply(
                character,
                physicalAccess,
                worldObjects,
                placements,
                existingEvidence,
                candidate))
        {
            throw new InvalidOperationException(
                "Only eligible move sabotage can atomically change placement and leave evidence.");
        }

        CharacterObjectSabotageEvidenceResult evidenceResult =
            CharacterObjectSabotageEvidenceTransition.Leave(
                character,
                physicalAccess,
                worldObjects,
                existingEvidence,
                candidate.Evidence);
        WorldObject target = candidate.Destination.Occurrence.Target;
        WorldObjectPlacement replacement = new(
            target,
            candidate.Destination.Destination);
        WorldObjectPlacementSnapshot resultingPlacements = new(
            placements.Entries.Select(
                placement => ReferenceEquals(placement.WorldObject, target)
                    ? replacement
                    : placement));

        return new CharacterObjectSabotageMoveResult(
            resultingPlacements,
            evidenceResult);
    }
}
