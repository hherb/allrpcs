using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.WorldObjects;

/// <summary>Retains bounded authoritative world-object placements in stable order.</summary>
public sealed class WorldObjectPlacementSnapshot
{
    /// <summary>Gets the maximum number of world-object placements in one snapshot.</summary>
    public const int MaximumPlacementCount = 64;

    /// <summary>Creates an immutable stable copy of exact world-object placements.</summary>
    public WorldObjectPlacementSnapshot(IEnumerable<WorldObjectPlacement> entries)
    {
        ArgumentNullException.ThrowIfNull(entries);

        List<WorldObjectPlacement> retainedEntries = [];
        HashSet<ObjectId> distinctObjectIds = [];
        foreach (WorldObjectPlacement? entry in entries)
        {
            if (entry is null)
            {
                throw new ArgumentException(
                    "World-object placements cannot contain null entries.",
                    nameof(entries));
            }

            if (!distinctObjectIds.Add(entry.WorldObject.Identity.Id))
            {
                throw new ArgumentException(
                    "World-object placements cannot contain duplicate object identifiers.",
                    nameof(entries));
            }

            retainedEntries.Add(entry);
            if (retainedEntries.Count > MaximumPlacementCount)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(entries),
                    retainedEntries.Count,
                    $"World-object placements accept at most {MaximumPlacementCount} entries.");
            }
        }

        Entries = retainedEntries.AsReadOnly();
    }

    /// <summary>Gets exact placements in stable supplied order.</summary>
    public IReadOnlyList<WorldObjectPlacement> Entries { get; }
}
