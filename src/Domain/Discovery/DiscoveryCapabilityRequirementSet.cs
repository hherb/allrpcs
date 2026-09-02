using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Discovery;

/// <summary>
/// Contains ordered capability requirements with one requirement per target world object.
/// </summary>
public sealed class DiscoveryCapabilityRequirementSet
{
    /// <summary>
    /// Creates a capability-requirement set. An empty sequence explicitly means no capability
    /// requirements are authored.
    /// </summary>
    public DiscoveryCapabilityRequirementSet(
        IEnumerable<DiscoveryCapabilityRequirement> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);

        List<DiscoveryCapabilityRequirement> orderedEntries = new();
        HashSet<ObjectId> objectIds = new();

        foreach (DiscoveryCapabilityRequirement entry in entries)
        {
            if (entry is null)
            {
                throw new ArgumentException(
                    "Capability-requirement entries cannot contain null values.",
                    nameof(entries));
            }

            if (!objectIds.Add(entry.ObjectId))
            {
                throw new ArgumentException(
                    "Capability-requirement entries cannot contain duplicate target-object identifiers.",
                    nameof(entries));
            }

            orderedEntries.Add(entry);
        }

        Entries = orderedEntries.AsReadOnly();
    }

    /// <summary>
    /// Gets capability requirements in their stable authored order.
    /// </summary>
    public IReadOnlyList<DiscoveryCapabilityRequirement> Entries { get; }
}
