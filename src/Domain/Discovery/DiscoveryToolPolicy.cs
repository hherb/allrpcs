using Allrpcs.Domain.Characters;

namespace Allrpcs.Domain.Discovery;

/// <summary>
/// Evaluates one tool-dependent discovery requirement against an existing character inventory.
/// </summary>
public static class DiscoveryToolPolicy
{
    /// <summary>
    /// Determines whether the inventory contains the required tool-object identifier.
    /// </summary>
    public static bool IsSatisfied(
        DiscoveryToolRequirement requirement,
        CharacterInventory inventory)
    {
        ArgumentNullException.ThrowIfNull(requirement);
        ArgumentNullException.ThrowIfNull(inventory);

        return inventory.ObjectReferences.Any(
            objectReference => objectReference == requirement.RequiredToolObjectId);
    }
}
