using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Domain.Locations;

/// <summary>Contains the bounded authored landmarks and traversal graph for the opening region.</summary>
public sealed class OpeningRegionSnapshot
{
    /// <summary>Gets the largest accepted opening-region landmark count.</summary>
    public const int MaximumLocationCount = 32;

    /// <summary>Gets the largest accepted opening-region connection count.</summary>
    public const int MaximumConnectionCount = 64;

    /// <summary>Creates an immutable region only from exact retained endpoints.</summary>
    public OpeningRegionSnapshot(
        Location entryLocation,
        IEnumerable<Location> locations,
        IEnumerable<OpeningRegionConnection> connections)
    {
        ArgumentNullException.ThrowIfNull(entryLocation);
        ArgumentNullException.ThrowIfNull(locations);
        ArgumentNullException.ThrowIfNull(connections);

        List<Location> retainedLocations = [];
        HashSet<LocationId> locationIds = [];
        foreach (Location location in locations)
        {
            if (location is null || !locationIds.Add(location.Identity.Id))
            {
                throw new ArgumentException(
                    "Opening-region locations must be non-null with unique identifiers.",
                    nameof(locations));
            }

            retainedLocations.Add(location);
            if (retainedLocations.Count > MaximumLocationCount)
            {
                throw new ArgumentOutOfRangeException(nameof(locations));
            }
        }

        if (!retainedLocations.Any(location => ReferenceEquals(location, entryLocation)))
        {
            throw new ArgumentException(
                "The entry location must be one exact retained location.",
                nameof(entryLocation));
        }

        List<OpeningRegionConnection> retainedConnections = [];
        HashSet<string> connectionKeys = new(StringComparer.Ordinal);
        foreach (OpeningRegionConnection connection in connections)
        {
            if (connection is null
                || !retainedLocations.Any(location => ReferenceEquals(location, connection.First))
                || !retainedLocations.Any(location => ReferenceEquals(location, connection.Second))
                || !connectionKeys.Add(ConnectionKey(connection)))
            {
                throw new ArgumentException(
                    "Connections must be non-null, unique, and use exact retained endpoints.",
                    nameof(connections));
            }

            retainedConnections.Add(connection);
            if (retainedConnections.Count > MaximumConnectionCount)
            {
                throw new ArgumentOutOfRangeException(nameof(connections));
            }
        }

        EntryLocation = entryLocation;
        Locations = retainedLocations.AsReadOnly();
        Connections = retainedConnections.AsReadOnly();
    }

    /// <summary>Gets the exact authored player-entry landmark.</summary>
    public Location EntryLocation { get; }

    /// <summary>Gets exact landmarks in stable authored order.</summary>
    public IReadOnlyList<Location> Locations { get; }

    /// <summary>Gets exact bidirectional connections in stable authored order.</summary>
    public IReadOnlyList<OpeningRegionConnection> Connections { get; }

    private static string ConnectionKey(OpeningRegionConnection connection)
    {
        string first = connection.First.Identity.Id.Value;
        string second = connection.Second.Identity.Id.Value;
        return StringComparer.Ordinal.Compare(first, second) < 0
            ? $"{first}\n{second}"
            : $"{second}\n{first}";
    }
}
