using Allrpcs.Domain.Identifiers;

namespace Allrpcs.Application.Map;

/// <summary>Projects exact canonical membership into one safe Seeker-owned map snapshot.</summary>
public static class SeekerMapSnapshotProjection
{
    /// <summary>Returns a fresh map snapshot with membership in canonical location order.</summary>
    public static SeekerMapSnapshot Project(
        OpeningRegionMapLocationCatalogue catalogue,
        CharacterId seekerId,
        IEnumerable<MapLocationDefinition> reportedLocations,
        IEnumerable<MapLocationDefinition> verifiedLocations)
    {
        ArgumentNullException.ThrowIfNull(catalogue);
        ArgumentNullException.ThrowIfNull(seekerId);
        ArgumentNullException.ThrowIfNull(reportedLocations);
        ArgumentNullException.ThrowIfNull(verifiedLocations);

        HashSet<MapLocationDefinition> reported = ValidateMembership(
            catalogue,
            reportedLocations,
            nameof(reportedLocations));
        HashSet<MapLocationDefinition> verified = ValidateMembership(
            catalogue,
            verifiedLocations,
            nameof(verifiedLocations));
        if (reported.Overlaps(verified))
        {
            throw new ArgumentException(
                "Reported and verified map-location membership must be disjoint.",
                nameof(verifiedLocations));
        }

        return new SeekerMapSnapshot(
            catalogue,
            seekerId,
            catalogue.Locations.Where(reported.Contains),
            catalogue.Locations.Where(verified.Contains));
    }

    private static HashSet<MapLocationDefinition> ValidateMembership(
        OpeningRegionMapLocationCatalogue catalogue,
        IEnumerable<MapLocationDefinition> locations,
        string parameterName)
    {
        HashSet<MapLocationDefinition> retained = new(ReferenceEqualityComparer.Instance);
        foreach (MapLocationDefinition? location in locations)
        {
            if (location is null
                || !catalogue.Locations.Any(candidate => ReferenceEquals(candidate, location))
                || !retained.Add(location))
            {
                throw new ArgumentException(
                    "Map membership must contain unique exact catalogue definitions.",
                    parameterName);
            }
        }

        return retained;
    }
}
