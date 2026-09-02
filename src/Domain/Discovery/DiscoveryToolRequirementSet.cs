using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Discovery;

/// <summary>
/// Contains ordered tool requirements with one requirement per target world object.
/// </summary>
public sealed class DiscoveryToolRequirementSet
{
    /// <summary>
    /// Creates a tool-requirement set. An empty sequence explicitly means no tool requirements
    /// are authored.
    /// </summary>
    public DiscoveryToolRequirementSet(IEnumerable<DiscoveryToolRequirement> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);

        List<DiscoveryToolRequirement> orderedEntries = new();
        HashSet<ObjectId> objectIds = new();

        foreach (DiscoveryToolRequirement entry in entries)
        {
            if (entry is null)
            {
                throw new ArgumentException(
                    "Tool-requirement entries cannot contain null values.",
                    nameof(entries));
            }

            if (!objectIds.Add(entry.ObjectId))
            {
                throw new ArgumentException(
                    "Tool-requirement entries cannot contain duplicate target-object identifiers.",
                    nameof(entries));
            }

            orderedEntries.Add(entry);
        }

        Entries = orderedEntries.AsReadOnly();
    }

    /// <summary>
    /// Gets tool requirements in their stable authored order.
    /// </summary>
    public IReadOnlyList<DiscoveryToolRequirement> Entries { get; }
}
