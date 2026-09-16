namespace Allrpcs.Domain.Characters;

/// <summary>Checks ordinary-interaction, capability, and informational access together.</summary>
public static class CharacterTargetAccessPolicy
{
    /// <summary>Returns whether one actor satisfies every requirement for one target character.</summary>
    public static bool IsSatisfied(
        Character actor,
        Character target,
        CharacterInteractionAccessSnapshot interactionAccess,
        CharacterTargetAccessRequirement requirement)
    {
        ArgumentNullException.ThrowIfNull(actor);
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(interactionAccess);
        ArgumentNullException.ThrowIfNull(requirement);

        return actor.Identity.Id != target.Identity.Id
            && interactionAccess.ActorId == actor.Identity.Id
            && requirement.TargetCharacterId == target.Identity.Id
            && interactionAccess.ReachableTargetCharacterIds.Contains(target.Identity.Id)
            && actor.Capabilities.Entries.Any(capability =>
                StringComparer.Ordinal.Equals(
                    capability.Key,
                    requirement.RequiredCapabilityKey))
            && actor.Knowledge.KnowledgeReferences.Contains(
                requirement.RequiredKnowledgeId);
    }
}
