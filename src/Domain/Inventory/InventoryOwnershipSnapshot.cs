using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Inventory;

/// <summary>
/// Contains an ordered snapshot of duplicate-free world-object ownership associations.
/// </summary>
public sealed class InventoryOwnershipSnapshot
{
    /// <summary>
    /// Creates an inventory-ownership snapshot. An empty sequence explicitly means no ownership
    /// associations are recorded.
    /// </summary>
    public InventoryOwnershipSnapshot(IEnumerable<InventoryOwnership> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);

        List<InventoryOwnership> orderedEntries = new();
        HashSet<ObjectId> objectIds = new();

        foreach (InventoryOwnership entry in entries)
        {
            if (entry is null)
            {
                throw new ArgumentException(
                    "Inventory-ownership entries cannot contain null values.",
                    nameof(entries));
            }

            if (!objectIds.Add(entry.ObjectId))
            {
                throw new ArgumentException(
                    "Inventory-ownership entries cannot contain duplicate object identifiers.",
                    nameof(entries));
            }

            orderedEntries.Add(entry);
        }

        Entries = orderedEntries.AsReadOnly();
    }

    /// <summary>
    /// Gets inventory-ownership associations in their stable supplied order.
    /// </summary>
    public IReadOnlyList<InventoryOwnership> Entries { get; }
}
