using Allrpcs.Domain.WorldObjects;

namespace Allrpcs.Domain.Characters;

/// <summary>Decides whether an eligible proposal names one exact authoritative world object.</summary>
public static class CharacterObjectSabotageOccurrencePolicy
{
    /// <summary>Returns whether one exact authoritative target may receive the proposed action.</summary>
    public static bool CanOccur(
        Character character,
        CharacterPhysicalAccessSnapshot physicalAccess,
        WorldObjectSnapshot worldObjects,
        CharacterObjectSabotageOccurrence candidate)
    {
        ArgumentNullException.ThrowIfNull(character);
        ArgumentNullException.ThrowIfNull(physicalAccess);
        ArgumentNullException.ThrowIfNull(worldObjects);
        ArgumentNullException.ThrowIfNull(candidate);

        return candidate.Proposal.AccessRequirement.TargetObjectId == candidate.Target.Identity.Id
            && worldObjects.Entries.Any(worldObject => ReferenceEquals(worldObject, candidate.Target))
            && CharacterObjectSabotageEligibilityPolicy.CanPropose(
                character,
                physicalAccess,
                candidate.Proposal);
    }
}
