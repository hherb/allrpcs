using Allrpcs.Domain.Characters;
using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Discovery;

/// <summary>
/// Evaluates the authored discovery conditions for one target world object.
/// </summary>
public static class DiscoveryConditionPolicy
{
    /// <summary>
    /// Determines whether every capability or tool requirement authored for the target is
    /// satisfied. A target without either requirement fails closed.
    /// </summary>
    public static bool IsSatisfied(
        ObjectId targetObjectId,
        DiscoveryConditionSet conditions,
        CharacterCapabilitySet capabilities,
        CharacterInventory inventory)
    {
        ArgumentNullException.ThrowIfNull(targetObjectId);
        ArgumentNullException.ThrowIfNull(conditions);
        ArgumentNullException.ThrowIfNull(capabilities);
        ArgumentNullException.ThrowIfNull(inventory);

        DiscoveryCapabilityRequirement? capabilityRequirement =
            conditions.CapabilityRequirements.Entries.FirstOrDefault(
                requirement => requirement.ObjectId == targetObjectId);
        DiscoveryToolRequirement? toolRequirement =
            conditions.ToolRequirements.Entries.FirstOrDefault(
                requirement => requirement.ObjectId == targetObjectId);

        if (capabilityRequirement is null && toolRequirement is null)
        {
            return false;
        }

        if (capabilityRequirement is not null &&
            !DiscoveryCapabilityPolicy.IsSatisfied(capabilityRequirement, capabilities))
        {
            return false;
        }

        return toolRequirement is null || DiscoveryToolPolicy.IsSatisfied(toolRequirement, inventory);
    }
}
