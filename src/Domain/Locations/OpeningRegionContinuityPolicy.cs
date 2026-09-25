namespace Allrpcs.Domain.Locations;

/// <summary>Checks whether every authored opening landmark is reachable from the exact entry.</summary>
public static class OpeningRegionContinuityPolicy
{
    /// <summary>Returns true only when one bidirectional graph connects every retained location.</summary>
    public static bool IsContinuous(OpeningRegionSnapshot region)
    {
        ArgumentNullException.ThrowIfNull(region);

        HashSet<Location> visited = new(ReferenceEqualityComparer.Instance)
        {
            region.EntryLocation,
        };
        Queue<Location> pending = new([region.EntryLocation]);
        while (pending.TryDequeue(out Location? current))
        {
            foreach (OpeningRegionConnection connection in region.Connections)
            {
                Location? neighbor = ReferenceEquals(connection.First, current)
                    ? connection.Second
                    : ReferenceEquals(connection.Second, current)
                        ? connection.First
                        : null;
                if (neighbor is not null && visited.Add(neighbor))
                {
                    pending.Enqueue(neighbor);
                }
            }
        }

        return visited.Count == region.Locations.Count;
    }
}
