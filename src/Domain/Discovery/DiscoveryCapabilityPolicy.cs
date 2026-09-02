using Allrpcs.Domain.Characters;

namespace Allrpcs.Domain.Discovery;

/// <summary>
/// Evaluates one character-capability discovery requirement against an existing capability set.
/// </summary>
public static class DiscoveryCapabilityPolicy
{
    /// <summary>
    /// Determines whether the capability set contains the required canonical capability key.
    /// </summary>
    public static bool IsSatisfied(
        DiscoveryCapabilityRequirement requirement,
        CharacterCapabilitySet capabilities)
    {
        ArgumentNullException.ThrowIfNull(requirement);
        ArgumentNullException.ThrowIfNull(capabilities);

        return capabilities.Entries.Any(
            capability => string.Equals(
                capability.Key,
                requirement.RequiredCapabilityKey,
                StringComparison.Ordinal));
    }
}
