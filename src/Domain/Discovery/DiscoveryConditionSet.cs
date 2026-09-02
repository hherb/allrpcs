namespace Allrpcs.Domain.Discovery;

/// <summary>
/// Composes the authored capability and tool requirement sets for discovery without defining
/// evaluation semantics between them.
/// </summary>
public sealed class DiscoveryConditionSet
{
    /// <summary>
    /// Creates a discovery-condition set from existing capability and tool requirement sets.
    /// </summary>
    public DiscoveryConditionSet(
        DiscoveryCapabilityRequirementSet capabilityRequirements,
        DiscoveryToolRequirementSet toolRequirements)
    {
        ArgumentNullException.ThrowIfNull(capabilityRequirements);
        ArgumentNullException.ThrowIfNull(toolRequirements);

        CapabilityRequirements = capabilityRequirements;
        ToolRequirements = toolRequirements;
    }

    /// <summary>
    /// Gets the authored capability requirements.
    /// </summary>
    public DiscoveryCapabilityRequirementSet CapabilityRequirements { get; }

    /// <summary>
    /// Gets the authored tool requirements.
    /// </summary>
    public DiscoveryToolRequirementSet ToolRequirements { get; }
}
