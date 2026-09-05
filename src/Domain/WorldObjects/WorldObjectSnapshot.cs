using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.WorldObjects;

/// <summary>
/// Contains an ordered snapshot of world objects with duplicate-free identifiers.
/// </summary>
public sealed class WorldObjectSnapshot
{
    /// <summary>
    /// Creates a world-object snapshot. An empty sequence explicitly means no world objects are
    /// recorded.
    /// </summary>
    public WorldObjectSnapshot(IEnumerable<WorldObject> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);

        List<WorldObject> orderedEntries = new();
        HashSet<ObjectId> objectIds = new();

        foreach (WorldObject entry in entries)
        {
            if (entry is null)
            {
                throw new ArgumentException(
                    "World-object entries cannot contain null values.",
                    nameof(entries));
            }

            if (!objectIds.Add(entry.Identity.Id))
            {
                throw new ArgumentException(
                    "World-object entries cannot contain duplicate object identifiers.",
                    nameof(entries));
            }

            orderedEntries.Add(entry);
        }

        Entries = orderedEntries.AsReadOnly();
    }

    /// <summary>
    /// Gets world objects in their stable supplied order.
    /// </summary>
    public IReadOnlyList<WorldObject> Entries { get; }
}
