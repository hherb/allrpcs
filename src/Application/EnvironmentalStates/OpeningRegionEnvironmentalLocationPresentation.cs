using Allrpcs.Domain.EnvironmentalStates;
using Allrpcs.Domain.Locations;

namespace Allrpcs.Application.EnvironmentalStates;

/// <summary>Retains one exact location-scoped qualitative environment presentation.</summary>
public sealed class OpeningRegionEnvironmentalLocationPresentation
{
    internal OpeningRegionEnvironmentalLocationPresentation(
        OpeningRegionEnvironmentalRegionSet regions,
        Location location,
        OpeningRegionEnvironmentalRegion region,
        string text)
    {
        Regions = regions;
        Location = location;
        Region = region;
        Text = text;
    }

    /// <summary>Gets the exact environmental-region snapshot projected for presentation.</summary>
    public OpeningRegionEnvironmentalRegionSet Regions { get; }

    /// <summary>Gets the exact retained location projected for presentation.</summary>
    public Location Location { get; }

    /// <summary>Gets the exact retained environmental region containing the location.</summary>
    public OpeningRegionEnvironmentalRegion Region { get; }

    /// <summary>Gets the exact qualitative weather state from the retained region.</summary>
    public EnvironmentalState WeatherState => Region.WeatherState;

    /// <summary>Gets the exact qualitative lighting state from the retained region.</summary>
    public EnvironmentalState LightingState => Region.LightingState;

    /// <summary>Gets the exact qualitative sound state from the retained region.</summary>
    public EnvironmentalState SoundState => Region.SoundState;

    /// <summary>Gets fixed greybox text containing only canonical environmental-state keys.</summary>
    public string Text { get; }
}
