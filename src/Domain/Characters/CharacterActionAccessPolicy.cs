namespace Allrpcs.Domain.Characters;

/// <summary>Checks explicit physical, capability, and informational action access together.</summary>
public static class CharacterActionAccessPolicy
{
    /// <summary>Returns whether one character satisfies every declared action-access requirement.</summary>
    public static bool IsSatisfied(
        Character character,
        CharacterPhysicalAccessSnapshot physicalAccess,
        CharacterActionAccessRequirement requirement)
    {
        ArgumentNullException.ThrowIfNull(character);
        ArgumentNullException.ThrowIfNull(physicalAccess);
        ArgumentNullException.ThrowIfNull(requirement);

        return CharacterPhysicalAccessPolicy.CanAccess(
                physicalAccess,
                character.Identity.Id,
                requirement.TargetObjectId)
            && character.Capabilities.Entries.Any(capability =>
                StringComparer.Ordinal.Equals(
                    capability.Key,
                    requirement.RequiredCapabilityKey))
            && character.Knowledge.KnowledgeReferences.Contains(
                requirement.RequiredKnowledgeId);
    }
}
