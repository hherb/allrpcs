using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Inventory;

/// <summary>
/// Contains an ordered snapshot of duplicate-free per-character inventory capacities.
/// </summary>
public sealed class InventoryCapacitySnapshot
{
    /// <summary>
    /// Creates an inventory-capacity snapshot. An empty sequence explicitly means no capacities
    /// are recorded.
    /// </summary>
    public InventoryCapacitySnapshot(IEnumerable<InventoryCapacity> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);

        List<InventoryCapacity> orderedEntries = new();
        HashSet<CharacterId> characterIds = new();

        foreach (InventoryCapacity entry in entries)
        {
            if (entry is null)
            {
                throw new ArgumentException(
                    "Inventory-capacity entries cannot contain null values.",
                    nameof(entries));
            }

            if (!characterIds.Add(entry.CharacterId))
            {
                throw new ArgumentException(
                    "Inventory-capacity entries cannot contain duplicate character identifiers.",
                    nameof(entries));
            }

            orderedEntries.Add(entry);
        }

        Entries = orderedEntries.AsReadOnly();
    }

    /// <summary>
    /// Gets per-character inventory capacities in their stable supplied order.
    /// </summary>
    public IReadOnlyList<InventoryCapacity> Entries { get; }
}
