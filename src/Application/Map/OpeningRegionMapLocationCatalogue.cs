namespace Allrpcs.Application.Map;

/// <summary>Retains every canonical opening-region map location in stable authored order.</summary>
public sealed class OpeningRegionMapLocationCatalogue
{
    private static readonly string[] CanonicalIds =
    [
        "sickhouse",
        "healer-garden",
        "village-square",
        "village-edge",
        "stream",
        "abandoned-laundry",
        "woodland-clearing",
        "old-shrine",
        "southern-road",
        "abandoned-wagon",
        "closed-deeper-forest-path",
    ];

    /// <summary>Creates a complete catalogue from exact canonical definitions.</summary>
    public OpeningRegionMapLocationCatalogue(IEnumerable<MapLocationDefinition> locations)
    {
        ArgumentNullException.ThrowIfNull(locations);

        List<MapLocationDefinition> retainedLocations = locations.ToList();
        if (retainedLocations.Count != CanonicalIds.Length)
        {
            throw new ArgumentException(
                "An opening-region map catalogue must contain every canonical location exactly once.",
                nameof(locations));
        }

        for (int index = 0; index < CanonicalIds.Length; index++)
        {
            MapLocationDefinition? location = retainedLocations[index];
            if (location is null
                || !string.Equals(location.Id.Value, CanonicalIds[index], StringComparison.Ordinal))
            {
                throw new ArgumentException(
                    "Map locations must use the complete canonical opening-region order.",
                    nameof(locations));
            }
        }

        Locations = retainedLocations.AsReadOnly();
    }

    /// <summary>Gets the exact map-location definitions in stable canonical order.</summary>
    public IReadOnlyList<MapLocationDefinition> Locations { get; }
}
