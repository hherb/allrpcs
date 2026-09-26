using Allrpcs.Domain.EnvironmentalStates;
using Allrpcs.Domain.Locations;

namespace Allrpcs.Application.EnvironmentalStates;

/// <summary>Projects exact location-scoped qualitative state for greybox presentation.</summary>
public static class OpeningRegionEnvironmentalLocationPresentationProjection
{
    /// <summary>Projects the exact retained region and its canonical channel keys.</summary>
    public static OpeningRegionEnvironmentalLocationPresentation Project(
        OpeningRegionEnvironmentalRegionSet regions,
        Location location)
    {
        ArgumentNullException.ThrowIfNull(regions);
        ArgumentNullException.ThrowIfNull(location);
        OpeningRegionEnvironmentalRegion? region =
            OpeningRegionEnvironmentalRegionLookup.FindByLocation(regions, location);
        if (region is null)
        {
            throw new ArgumentException(
                "Environmental presentation requires an exact retained location.",
                nameof(location));
        }

        string text = $"Environment: {region.WeatherState.Key} / "
            + $"{region.LightingState.Key} / {region.SoundState.Key}";
        return new OpeningRegionEnvironmentalLocationPresentation(
            regions,
            location,
            region,
            text);
    }
}
