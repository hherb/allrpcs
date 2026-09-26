using Allrpcs.Domain.Locations;

namespace Allrpcs.Domain.EnvironmentalStates;

/// <summary>Assigns every exact opening location to one non-overlapping environmental region.</summary>
public sealed class OpeningRegionEnvironmentalRegionSet
{
    /// <summary>Creates a complete exact region assignment from one retained state catalog.</summary>
    public OpeningRegionEnvironmentalRegionSet(
        OpeningRegionSnapshot topology,
        EnvironmentalStateSet environmentalStates,
        IEnumerable<OpeningRegionEnvironmentalRegion> regions)
    {
        ArgumentNullException.ThrowIfNull(topology);
        ArgumentNullException.ThrowIfNull(environmentalStates);
        ArgumentNullException.ThrowIfNull(regions);
        List<OpeningRegionEnvironmentalRegion> retained = [];
        HashSet<string> regionKeys = new(StringComparer.Ordinal);
        HashSet<Location> assignedLocations = new(ReferenceEqualityComparer.Instance);
        foreach (OpeningRegionEnvironmentalRegion region in regions)
        {
            if (region is null || !regionKeys.Add(region.Key))
            {
                throw new ArgumentException(
                    "Environmental regions must be non-null with unique canonical keys.",
                    nameof(regions));
            }

            ValidateStates(environmentalStates, region);
            foreach (Location location in region.Locations)
            {
                if (!topology.Locations.Any(candidate => ReferenceEquals(candidate, location))
                    || !assignedLocations.Add(location))
                {
                    throw new ArgumentException(
                        "Region locations must be exact retained topology members assigned once.",
                        nameof(regions));
                }
            }

            retained.Add(region);
        }

        if (retained.Count == 0
            || topology.Locations.Any(location => !assignedLocations.Contains(location)))
        {
            throw new ArgumentException(
                "Environmental regions must cover every exact opening location once.",
                nameof(regions));
        }

        Topology = topology;
        EnvironmentalStates = environmentalStates;
        Regions = retained.AsReadOnly();
    }

    /// <summary>Gets the exact authored opening topology.</summary>
    public OpeningRegionSnapshot Topology { get; }

    /// <summary>Gets the exact qualitative environmental-state catalog.</summary>
    public EnvironmentalStateSet EnvironmentalStates { get; }

    /// <summary>Gets complete non-overlapping regions in stable authored order.</summary>
    public IReadOnlyList<OpeningRegionEnvironmentalRegion> Regions { get; }

    private static void ValidateStates(
        EnvironmentalStateSet states,
        OpeningRegionEnvironmentalRegion region)
    {
        EnvironmentalState[] required =
        [
            region.WeatherState,
            region.LightingState,
            region.SoundState,
        ];
        if (required.Any(state => !states.Entries.Any(candidate => ReferenceEquals(
            candidate,
            state))))
        {
            throw new ArgumentException(
                "Every regional channel must use an exact retained environmental state.",
                nameof(region));
        }
    }
}
