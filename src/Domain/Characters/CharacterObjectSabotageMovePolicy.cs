using Allrpcs.Domain.Discovery;
using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Domain.Characters;

/// <summary>Checks whether one exact move sabotage can change placement and leave evidence.</summary>
public static class CharacterObjectSabotageMovePolicy
{
    /// <summary>Reports whether the candidate can move its exact placed target.</summary>
    public static bool CanApply(
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

        if (!CharacterObjectSabotageEvidencePolicy.CanLeave(
                character,
                physicalAccess,
                worldObjects,
                existingEvidence,
                candidate.Evidence))
        {
            return false;
        }

        WorldObject target = candidate.Destination.Occurrence.Target;
        WorldObjectPlacement? currentPlacement = placements.Entries.SingleOrDefault(
            placement => placement.WorldObject.Identity.Id == target.Identity.Id);

        return currentPlacement is not null
            && ReferenceEquals(currentPlacement.WorldObject, target)
            && currentPlacement.Location.Identity.Id
                != candidate.Destination.Destination.Identity.Id;
    }
}
