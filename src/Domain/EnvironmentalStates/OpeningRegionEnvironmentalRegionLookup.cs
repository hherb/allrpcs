using Allrpcs.Domain.Locations;

namespace Allrpcs.Domain.EnvironmentalStates;

/// <summary>Finds the unique environmental region retaining one exact opening location.</summary>
public static class OpeningRegionEnvironmentalRegionLookup
{
    /// <summary>Returns the exact retained region, or null for a rebuilt or foreign location.</summary>
    public static OpeningRegionEnvironmentalRegion? FindByLocation(
        OpeningRegionEnvironmentalRegionSet regions,
        Location location)
    {
        ArgumentNullException.ThrowIfNull(regions);
        ArgumentNullException.ThrowIfNull(location);
        return regions.Regions.SingleOrDefault(region =>
            region.Locations.Any(candidate => ReferenceEquals(candidate, location)));
    }
}
